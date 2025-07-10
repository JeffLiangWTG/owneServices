using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class DragDropHandlerTest : TestCase
	{
		public void TestHandle_ShouldValidateFileHash()
		{
			DragDropSamples samples = new DragDropSamples();

			AssertNoExceptionThrown(() => ExecuteMessageHandlingScenario(samples.ValidXmlMessage));

			AssertExceptionThrown<IOException>(() => ExecuteMessageHandlingScenario(samples.CorruptedXmlMessage));
			AssertExceptionThrown<InvalidOperationException>(() => ExecuteMessageHandlingScenario(samples.CorruptedXmlMessage.Substring(0, samples.CorruptedXmlMessage.Length - 4)));
		}

		public void TestNotReportErrorWhenParsingXmlFails()
		{
			// Arrange.
			var reporterMock = new Mock<IErrorReporter>();
			var enterpriseChannelMock = new Mock<EnterpriseChannel>() { CallBase = true };
			var invalidXml = @"A2MDMgNjA<?xml version=""1.0""?>
<DragDropMessage xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <fileDrop>
    <FileDropData>
      <fileHash>vv6EHyf6eJB2+SYucAJjvA==</fileHash>
      <fileName>C:\Users\test\AppData\Local\9096.PDF</fileName>
      <fileData>A2MDMgNjAzIDYwMyA2MDMgNjAzIDYwMyA2MDMg</fileData>
    </FileDropData>
  </fileDrop>
</DragDropMessage>";

			// Act & Assert.
			using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
			{
				AssertInnermostException(typeof(XmlException), () => ExecuteMessageHandlingScenario(invalidXml, enterpriseChannelMock.Object));
				reporterMock.VerifyNoOtherCalls();
			}
		}

		void ExecuteMessageHandlingScenario(string xml, EnterpriseChannel enterpriseChannel = null)
		{
			DragDropHandler handler = new DragDropHandler();

			using (Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(xml)))
			{
				handler.Handle(enterpriseChannel, stream);
			}
		}
	}
}
