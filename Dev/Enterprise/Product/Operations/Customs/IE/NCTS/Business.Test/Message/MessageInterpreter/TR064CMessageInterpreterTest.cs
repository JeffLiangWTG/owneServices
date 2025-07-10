using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR064C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR064CMessageInterpreter))]
	class TR064CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR064CMessageInterpreter, TR064CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR054C;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var tr064Text = InterchangeProcessorTestHelper.GetStandardTR064CText("21IEDU4EX144268149");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", tr064Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Request Declaration Invalidation (TR064) message has been received for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDU4EX144268149</td></tr><tr><td>Case Id</td><td>ID123456</td></tr><tr><td>Remarks</td><td>Remarks</td></tr></table>";

		protected override TR064CProvider GetProvider(TextReader reader) => new TR064CProvider(new MailBoxItemProvider<Tr064C>(reader).Message);
	}
}
