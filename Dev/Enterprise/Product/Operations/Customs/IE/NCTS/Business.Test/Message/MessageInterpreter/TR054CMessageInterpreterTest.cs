using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR054C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR054CMessageInterpreter))]
	class TR054CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR054CMessageInterpreter, TR054CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR054C;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var tr054Text = InterchangeProcessorTestHelper.GetStandardTR054CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", tr054Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Request for Advice (TR054) message has been received for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDU4EX144268149</td></tr><tr><td>Advice Requested</td><td>Y</td></tr><tr><td>Advice Request Date &amp; Time</td><td>01-Jan-23 10:15</td></tr></table>";

		protected override TR054CProvider GetProvider(TextReader reader) => new TR054CProvider(new MailBoxItemProvider<Tr054C>(reader).Message);
	}
}
