using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR864C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR864CMessageInterpreter))]
	class TR864CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR864CMessageInterpreter, TR864CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR864C;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardTR864CText("19MRNCC060C0123456");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Declaration Invalidation Request Cancellation (TR864) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>19MRNCC060C0123456</td></tr><tr><td>Case Id</td><td>ID123456</td></tr><tr><td>Invalidation Request Cancellation Reason</td><td>Reason for cancellation</td></tr></table>";

		protected override TR864CProvider GetProvider(TextReader reader) => new TR864CProvider(new MailBoxItemProvider<Tr864C>(reader).Message);
	}
}
