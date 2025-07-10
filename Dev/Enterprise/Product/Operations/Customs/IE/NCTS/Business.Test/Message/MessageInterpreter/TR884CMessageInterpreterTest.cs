using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR884C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR884CMessageInterpreter))]
	class TR884CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR884CMessageInterpreter, TR884CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR884C;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var tr054Text = InterchangeProcessorTestHelper.GetStandardTR884CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", tr054Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"
			A Document Presentation Request Cancellation (TR884) message has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>19MRNCC060C0123456</td></tr>
				<tr><td>Case Id</td><td>Test ID</td></tr>
				<tr><td>Document Presentation Request Cancellation Reason</td><td>Test Presentation Request Cancellation Reason</td></tr>
			</table>";

		protected override TR884CProvider GetProvider(TextReader reader) => new TR884CProvider(new MailBoxItemProvider<Tr884C>(reader).Message);
	}
}
