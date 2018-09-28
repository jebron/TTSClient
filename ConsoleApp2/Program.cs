/*This application will convert text to speech using the Google Cloud Platform Text-to-Speech API.
  The intended use is to synthesize text for the purpose of announcing emergency messages over 
  the plant paging system.

  For more information on how this application works or how the automated paging system works, please refer to the documentation.
*/

using System;
using System.IO;
using Google.Cloud.TextToSpeech.V1;
using System.Net.NetworkInformation;

namespace GCtextToSpeech
{
    class TTSClient
    {
        // A method used to ping a specific IP address or hostanme, used to confirm internet connectivity
        public bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            // Initialize a new instance of the pinger class, and if the ping was successful, set pingable to true
            // Otherwise return a false upon exception
            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                // Cleans up and releases all memory/resources used by the pinger class if the value is null
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            // Returns a boolean value for whether the host was reachable via ping
            return pingable;
        }

        public void convertTextToSpeech()
        {
            // Declare variables for the message to be converted and the filename to be saved
            string messageText;
            string fileName;
            Console.Clear();
            
            // Gather information from the user
            Console.Write("Enter the message to convert to audio: ");
            messageText = Console.ReadLine();
            Console.Write("Enter the name of the file: ");
            fileName = Console.ReadLine();

            // Create a new TextToSpeechClient called client
            TextToSpeechClient client = TextToSpeechClient.Create();

            // Assign the user input message to Text (Text is used in the API as the message)
            SynthesisInput input = new SynthesisInput
            {
                Text = messageText
            };

            // Build the voice request and set the parameters that differ from default settings
            VoiceSelectionParams voice = new VoiceSelectionParams
            {
                LanguageCode = "en-US",
                Name = "en-US-Wavenet-C",
                SsmlGender = SsmlVoiceGender.Female
            };

            // Select the speaking rate and type of audio file you want returned
            AudioConfig config = new AudioConfig
            {
                SpeakingRate = 1.0,
                AudioEncoding = AudioEncoding.Linear16
            };

            // Perform the Text-to-Speech request, passing the text input (SynthesisInput)
            // with the selected voice parameters (VoiceSelectionParams) and audio file type (AudioConfig)
            var response = client.SynthesizeSpeech(new SynthesizeSpeechRequest
            {
                Input = input,
                Voice = voice,
                AudioConfig = config
            });

            // Write the binary AudioContent of the response to an WAV file.
            using (Stream output = File.Create(fileName + ".wav"))
            {
                response.AudioContent.WriteTo(output);
                Console.WriteLine($"\r\nAudio content written to " + fileName + ".wav");
            }   

        }

        static void Main(string[] args)
        {
            string loopClient;
            TTSClient TTSInstance = new TTSClient();
            bool pingResult= TTSInstance.PingHost("8.8.8.8");
            if (pingResult)
            {
                do
                {
                    TTSInstance.convertTextToSpeech();
                    Console.WriteLine("\r\nWould you like to convert another message? (y/n)");
                    loopClient = Console.ReadLine();
                } while (loopClient != "n");
            }
            else
            {
                Console.WriteLine("Cannot connect to the internet, check your connection and try again.");
                Console.ReadKey();
            }
        }
    }
}