using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM415V;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM415VMessageInterpreter))]
	class IM415VMessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM415VMessageInterpreter, IM415VProvider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM415V;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", AISInterchangeProcessorTestHelper.GetAISVersion2_0IM415VText("A", "LRN123456789", "21IEDUB11A782454R2", new ZDateTime(2023, 08, 11)));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Customs Declaration Acknowledgment (IM415V) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>2023-08-11</td></tr></table>";

		protected override IM415VProvider GetProvider(TextReader reader) => new IM415VProvider(new MailBoxItemProvider<Im415V>(reader).Message);
	}
}
