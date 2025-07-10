using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class MessageFileFormatCheckerTest : TestCase
	{
		public void TestCheckIsZipCompressedData()
		{
			var log = new StringBuilder();

			var notificationsMock = new Mock<INotifications>();
			notificationsMock.Setup(n => n.AddMessage(It.IsAny<string>())).Callback<string>(m => log.AppendLine(m));

			var checker = new MessageFileFormatChecker(notificationsMock.Object);

			byte[] zipData = { 0x50, 0x4B, 0x03, 0x04 };

			Assert(checker.CheckIsZipCompressedData(zipData));
			AssertEquals("Should not log any information.", 0, log.Length);

			byte[] invalidData = { 0x00, 0x00, 0x00, 0x00 };

			Assert(!checker.CheckIsZipCompressedData(invalidData));
			AssertContains(@"Unable to parse the message content into a ZIP file.", log.ToString());
		}

		public void TestCheckIsXMLDocument()
		{
			var log = new StringBuilder();

			var notificationsMock = new Mock<INotifications>();
			notificationsMock.Setup(n => n.AddMessage(It.IsAny<string>())).Callback<string>(m => log.AppendLine(m));

			var checker = new MessageFileFormatChecker(notificationsMock.Object);

			var xmlContent = "<root></root>";
			Assert(checker.CheckIsXMLDocument(xmlContent));
			AssertEquals("Should not log any information.", 0, log.Length);

			using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));
			Assert(checker.CheckIsXMLDocument(xmlStream));
			AssertEquals("Should not log any information.", 0, log.Length);

			var invalidXmlContent = "<root>";
			Assert(!checker.CheckIsXMLDocument(invalidXmlContent));
			AssertContains(@"Unable to parse the message content into an XML document. Error:", log.ToString());

			log.Clear();

			using var invalidXmlStream = new MemoryStream(Encoding.UTF8.GetBytes(invalidXmlContent));
			Assert(!checker.CheckIsXMLDocument(invalidXmlStream));
			AssertContains(@"Unable to parse the message content into an XML document. Error:", log.ToString());
		}
	}
}
