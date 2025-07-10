using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Encryption
{
#if NETFRAMEWORK
	[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
#endif
	public class TwoWayEncoderTest : TestCase
	{
		const string knownPlaintext = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam sem magna, egestas sed massa non, rhoncus eleifend arcu. Aenean eu porta augue, at eleifend turpis. Donec accumsan magna erat, eget finibus velit sodales id. Sed dignissim nisl eget tortor interdum, ut viverra metus egestas. Etiam at ante eget magna facilisis dignissim. Cras risus orci, condimentum vel risus in, tempus rutrum arcu. Nulla vel accumsan ex. Cras in lobortis sapien, eget varius odio. Cras feugiat, magna ut cursus lobortis, ex diam tempus arcu, at molestie risus orci vel nunc. Pellentesque ante metus, iaculis nec mi vestibulum, tincidunt placerat mi. Proin sollicitudin sapien ut sagittis tristique. Ut sodales sed enim eget sodales. Quisque tempus nisl tellus, at mollis enim imperdiet et.

Phasellus tortor sem, auctor eu pharetra quis, convallis quis urna. Fusce lacinia pharetra diam, vitae porta massa commodo quis. Suspendisse nulla mi, vulputate eget dolor at, consectetur viverra nibh. Nunc feugiat, magna id pulvinar biam.";

		const string knownCiphertext = @"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";
		const string knownCipertextUtf8ByteArrayBase64 = @"4mewXIDMpsByn7nJIt1gRrCTnpHOpDrId2qmgKLKNg0kG1rXR2cEQyzjgFGGRoLHRq+S5JveKi74bKe3Fl+bJZdZeK2lbBwYLhveGXUKoqD4NN722ayujvugSkDaokNd6+pPcTto4NBwuDhdwN1/RvDNu3+Fg4s5kGCUEWq14JnyDom8QK1fSImASyezlj5w75I2GBuN2fnABClnHnwRt9TsbpFkfv3MQh7p7NtlgBhY1UdLThHfOvOOZnKwmFGgc0DSvLaMUXlhoLhhAhuTIfOw9c37w0SHNiu3LAssWIQAeVFvJuJv7x3biXCYdsb6VxtMgi9drIjk5eH3D/we0v2tG1gl3l91ADnZuad8sSdv/Tj5EV9OY0rqjw01HDxgIOmF/1dTWBH7KYJfbCPGSHT2Ba4QBEDscnlTv4rYrNt+9a6b+FvQPOozlfh5XJudTnHDS86kX3CHsqar6qis4nIq7usR3YXGSyb3N9QpDPRFObGTBCyw5Fz367gfVPu7Tdg27pDvvtcrdJUMKq1j+DJwRd5kaSl8KSMqNknGxBFeFBzUqCrm0h+w6EsInWSTqH9kJpAQYwONVxdgSRhKYeJC68xgCtvKfvItxVeKsc8rllRLbRCnzSqJGg6yBm3wnT+teZU8j8ULbJhHJybsDhgjck7X/NFe2xlHseaZm4fa7/Gxpre+suXobjYblOW+AjTWPV9qHLcHyE6LnEmmXO/rok1YCg2Hhtw4AzG/jSkOY5cBHBOE9GUhhjTRsc+M9Jez/Mof4890hvGjgljeTbJYa8CjYS4p//35+jknJj3xhh2VscVZ/VtTQ8g3NeWH+DWRBeNHU3XWIuV/uyRv6fQOmSwk5odgwppSAcA2aEJN8mnIcx5iHLegBytvXnn4NNhgxs9Z2HeWqLne15HAFkKzNryNfv47FY30VfdtsIGOIjerhvsaxprBDj3bHFlX2UxsiP4KHHKL6u98oU5vGbU5yma26oQXx9Hpx6GZa2eUejN+O4kuWdd5vGx+A4AFAreT7fHGJBpgCCPGZRUI0P2wuI2YcjswPzkxjs2fpi5OC6dfOVsQAnoSQSWg/ZaR2TUAMdDUqS3GJFHOY5GgRsfuVUkpmMDXNQqMPCSDrOQrSBXKshglD6DJ64gkXtc81TzWQ/Hfmdd33ZbdFOxIg1A5CsjbFM5BVnKKiU/3uBN4aYuEdQ3hApgLCm+VKP6Rq9dIuOa7b7MQS9oYVqwYW5cpqX8kM6SLJqdYMp0FY4s9+XRHGtD+evyf9YClvmLf5HC1aft42fCNnQyatFqp7TtZMS1BhmUKnV0ifJUfH1GszOT+EKi8h5554epz8OXWXWz/AAdqtbKgZ+zADJYRTA==";

		public void TestEncryptKnownCiphertextOnStandardInitialisationVector()
		{
			AssertEquals("Encryption (string) has produced different ciphertext. This could be a breaking change if older clients cannot decrypt it.", knownCiphertext, TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(knownPlaintext));
			AssertEquals("Encryption (byte[]) has produced different ciphertext. This could be a breaking change if older clients cannot decrypt it.", knownCipertextUtf8ByteArrayBase64, Convert.ToBase64String(TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(Encoding.UTF8.GetBytes(knownPlaintext))));
		}

		public void TestDecryptKnownCiphertextOnStandardInitialisationVector()
		{
			AssertEquals("Decyption cannot decrypt known ciphertext. This is a breaking change.", knownPlaintext, TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(knownCiphertext));
		}

		public void TestDecryptKnownBase64ByteArrayOnStandardInitialisationVector()
		{
			AssertEquals("Decyption cannot decrypt known ciphertext. This is a breaking change.", knownPlaintext, Encoding.UTF8.GetString(TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(Convert.FromBase64String(knownCipertextUtf8ByteArrayBase64))));
		}

		public void TestEncoding()
		{
			AssertEncoding("");
			AssertEncoding((char)255 + "odysseytest");
			AssertEncoding("123456789012345");
			AssertEncoding("12345678901234567890123456789012345678901234567890");
		}

		public void TestEncryptArray()
		{
			AssertEncoding(new byte[] { 1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15 });
			AssertEncoding(Encoding.Unicode.GetBytes("123456789012345"));
			AssertEncoding(Encoding.Unicode.GetBytes("12345678901234567890123456789012345678901234567890"));
		}

		public void TestDecrypt_Empty()
		{
			TwoWayEncoder testEncoder = new TwoWayEncoder(Guid.NewGuid());
			AssertEquals("", testEncoder.Decrypt(""));
			AssertEquals("", testEncoder.Decrypt((string)null));
		}

		void AssertEncoding(string password)
		{
			TwoWayEncoder testEncoder = new TwoWayEncoder(Guid.NewGuid());
			string encryptedValue = testEncoder.Encrypt(password);
			string result = testEncoder.Decrypt(encryptedValue);
			AssertEquals("Password", password, result);
		}

		void AssertEncoding(byte[] data)
		{
			TwoWayEncoder testEncoder = new TwoWayEncoder(Guid.NewGuid());
			byte[] encryptedValue = testEncoder.Encrypt(data);
			byte[] result = testEncoder.Decrypt(encryptedValue);
			AssertEquals("Password", data, result);
		}
	}
}
