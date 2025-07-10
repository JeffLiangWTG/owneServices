using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC028C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC028CMessageInterpreter))]
	class CC028CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC028CMessageInterpreter, CC028CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE028;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A MRN Allocated (IE028) message has been received for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Accepted Date</td><td>18-Sep-71</td></tr></table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC028CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override CC028CProvider GetProvider(TextReader reader) => new CC028CProvider(new MailBoxItemProvider<Cc028CType>(reader).Message);
	}
}
