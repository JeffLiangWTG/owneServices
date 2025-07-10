using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR862C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR862CMessageInterpreter))]
	class TR862CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR862CMessageInterpreter, TR862CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR862C;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardTR862CText("19MRNCC060C0123456");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Declaration Amendment Request Cancellation (TR862) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>19MRNCC060C0123456</td></tr><tr><td>Case Id</td><td>Test ID</td></tr><tr><td>Amendment request cancellation Reason</td><td>Test Amendment request cancellation reason</td></tr></table>";

		protected override TR862CProvider GetProvider(TextReader reader) => new TR862CProvider(new MailBoxItemProvider<Tr862C>(reader).Message);
	}
}
