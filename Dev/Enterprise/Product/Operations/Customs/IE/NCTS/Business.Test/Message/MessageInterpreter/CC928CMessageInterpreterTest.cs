using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC928C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC928CMessageInterpreter))]
	class CC928CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC928CMessageInterpreter, CC928CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE928;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Positive Acknowledgement message has been received from Customs for Job B00000012 through the IE928 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>Reference Number</td><td>RNALPHN8</td></tr></table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var cc928Text = InterchangeProcessorTestHelper.GetStandardCC928CText("LRN123456789", "RNALPHN8");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc928Text, includeResponseWrap: false);

			return message;
		}

		protected override CC928CProvider GetProvider(TextReader reader) => new CC928CProvider(new MailBoxItemProvider<Cc928CType>(reader).Message);
	}
}
