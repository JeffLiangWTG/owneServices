using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC043C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC043CMessageInterpreter))]
	class CC043CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC043CMessageInterpreter, CC043CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE043;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"An Unloading Permission (IE043) message has been received for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Customs Office of Destination (Actual)</td><td>RNALPHN9</td></tr><tr><td>Trader at Destination </td><td>IN043</td></tr><tr><td>Address</td><td>123 WHERE ST, CITY, IE</td></tr></table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC043CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override CC043CProvider GetProvider(TextReader reader) => new CC043CProvider(new MailBoxItemProvider<Cc043CType>(reader).Message);
	}
}
