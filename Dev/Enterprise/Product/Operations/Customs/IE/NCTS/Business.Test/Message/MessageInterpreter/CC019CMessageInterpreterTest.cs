using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC019C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC019CMessageInterpreter))]
	class CC019CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC019CMessageInterpreter, CC019CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE019;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"A Discrepancy message has been received from Customs for Job B00000012 through the IE019 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Discrepancy Date</td><td>18-Sep-71</td></tr><tr><td>Discrepancy Notification Text</td><td>Test Discrepancies Notification</td></tr></table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var cc019Text = InterchangeProcessorTestHelper.GetStandardCC019CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc019Text, includeResponseWrap: false);

			return message;
		}

		protected override CC019CProvider GetProvider(TextReader reader) => new CC019CProvider(new MailBoxItemProvider<Cc019CType>(reader).Message);
	}
}
