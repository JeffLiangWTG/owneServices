using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR084C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR084CMessageInterpreter))]
	class TR084CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR084CMessageInterpreter, TR084CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR084C;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var tr054Text = InterchangeProcessorTestHelper.GetStandardTR084CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", tr054Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Request Document Presentation (TR084) message has been received for Job B00000012.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDU4EX144268149</td></tr><tr><td>LRN</td><td>LRNTR084123456789</td></tr><tr><td>Request Date</td><td>24-Feb-23 15:33</td></tr><tr><td>Date Limit</td><td>26-Mar-23 16:00</td></tr></table><br /><br />Additional Information<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>Y022</td></tr><tr><td>Document Complementary Information</td><td>TR084 add info completementary information</td></tr></table><br /><br />Additional Information<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>Y024</td></tr><tr><td>Document Complementary Information</td><td>TR084 add info completementary information 2</td></tr></table>";

		protected override TR084CProvider GetProvider(TextReader reader) => new TR084CProvider(new MailBoxItemProvider<Tr084C>(reader).Message);
	}
}
