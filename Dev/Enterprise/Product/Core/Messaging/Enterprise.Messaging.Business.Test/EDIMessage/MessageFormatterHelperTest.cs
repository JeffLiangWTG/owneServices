using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class MessageFormatterHelperTest : TestCase
	{
		public void TestAddHeader()
		{
			const string testHeader = "Header \r\n ";
			const string testString = "This is a test string";

			using (Stream stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				var encoder = new UTF8Encoding();
				byte[] inputByteArray = encoder.GetBytes(testString);
				stream.Write(inputByteArray, 0, inputByteArray.Length);
				stream.Flush();
				using (var headerStream = stream.AddHeader(testHeader))
				{
					StreamReader reader = new StreamReader(headerStream);
					AssertEquals(testHeader + testString, reader.ReadToEnd());
				}
			}
		}

		public void TestAddFooter()
		{
			const string testFooter = " \r\n Footer";
			const string testString = "This is a test string";

			using (Stream stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				var encoder = new UTF8Encoding();
				byte[] inputByteArray = encoder.GetBytes(testString);
				stream.Write(inputByteArray, 0, inputByteArray.Length);
				stream.Flush();
				stream.AddFooter(testFooter);
				StreamReader reader = new StreamReader(stream);
				AssertEquals(testString + testFooter, reader.ReadToEnd());
			}
		}

		public void TestReplaceTextBlock()
		{
			const string replaceString = "*****";
			const string testString = "This is a test StartTextBlock yes this is EndTextBlock and also StartTextBlock no they are EndTextBlock string";
			const string expectedString = "This is a test StartTextBlock" + replaceString + "EndTextBlock and also StartTextBlock no they are EndTextBlock string";

			using (Stream stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				var encoder = new UTF8Encoding();
				byte[] inputByteArray = encoder.GetBytes(testString);
				stream.Write(inputByteArray, 0, inputByteArray.Length);
				stream.Flush();
				using (var formattedStream = stream.ReplaceTextBlock("StartTextBlock", "EndTextBlock", replaceString, 30))
				{
					StreamReader reader = new StreamReader(formattedStream);
					AssertEquals(expectedString, reader.ReadToEnd());
				}
			}
		}

		public void TestReplaceChars()
		{
			KeyValuePair<char, string>[] replacePairs =
				new KeyValuePair<char, string>[] {
					new KeyValuePair<char, string>('\r', ""),
					new KeyValuePair<char, string>('\n', ""),
					new KeyValuePair<char, string>('\t', ""),
					new KeyValuePair<char, string>('\'', " "),
					new KeyValuePair<char, string>('\x1c', "\r\n"),
				};

			string testString = "This \r is \n is \t a test ?' message'. This" + '\x1f' + "format" + '\x1d' + "I don't" + '\x1c' + " understand";

			using (Stream stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				var encoder = new UTF8Encoding();
				byte[] inputByteArray = encoder.GetBytes(testString);
				stream.Write(inputByteArray, 0, inputByteArray.Length);
				stream.Flush();
				using (var formattedStream = stream.ReplaceChars(replacePairs))
				{
					StreamReader reader = new StreamReader(formattedStream);

					string expectedString = testString;
					for (int i = 0; i < replacePairs.Length; i++)
					{
						expectedString = expectedString.Replace(replacePairs[i].Key.ToString(), replacePairs[i].Value);
					}

					AssertEquals(expectedString, reader.ReadToEnd());
				}
			}
		}

		public void TestAddStringAfterEachNumberOfChars()
		{
			const string testString = "This is a test StartTextBlock";
			const string expectedString = "This \r\nis a \r\ntest \r\nStart\r\nTextB\r\nlock";

			using (Stream stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				var encoder = new UTF8Encoding();
				byte[] inputByteArray = encoder.GetBytes(testString);
				stream.Write(inputByteArray, 0, inputByteArray.Length);
				stream.Flush();
				using (var formattedStream = stream.AddStringAfterEachNumberOfChars(5, "\r\n"))
				{
					StreamReader reader = new StreamReader(formattedStream);
					AssertEquals(expectedString, reader.ReadToEnd());
				}
			}
		}

		public void TestReplaceStrings()
		{
			KeyValuePair<string, string>[] replacePairs =
				new KeyValuePair<string, string>[] {
				new KeyValuePair<string, string>("dog", "t2"),
				new KeyValuePair<string, string>("dogcat", "t1"),
				new KeyValuePair<string, string>("cat", "t3"),
				new KeyValuePair<string, string>("catdog", "t5"),
				new KeyValuePair<string, string>("t2", "t4")
				};

			string[] otherStrings = new string[] { "let", "dogcat", "catdogcat", "dogcatdog", "be", "t6", "  ", "matador", "    ", "  f ", "matador" };

			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < 2000; i++)
			{
				int oldStringsIndex = i % replacePairs.Length;
				int otherStringsIndex = i % otherStrings.Length;

				builder.Append(replacePairs[oldStringsIndex].Key);
				builder.Append(otherStrings[otherStringsIndex]);
				builder.Append(replacePairs[oldStringsIndex].Value);
				builder.Append(otherStrings[otherStringsIndex]);
			}

			string testString = builder.ToString();

			using (Stream stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				var encoder = new UTF8Encoding();
				byte[] inputByteArray = encoder.GetBytes(testString);
				stream.Write(inputByteArray, 0, inputByteArray.Length);
				stream.Flush();
				using (var formattedStream = stream.ReplaceStrings(replacePairs, 31))
				{
					StreamReader reader = new StreamReader(formattedStream);

					string expectedString = testString;
					for (int i = 0; i < replacePairs.Length; i++)
					{
						expectedString = expectedString.Replace(replacePairs[i].Key, replacePairs[i].Value);
					}

					AssertEquals(testString, expectedString, reader.ReadToEnd());
				}
			}
		}
	}
}
