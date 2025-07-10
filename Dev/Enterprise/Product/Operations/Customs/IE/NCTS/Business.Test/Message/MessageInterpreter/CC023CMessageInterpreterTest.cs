using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC023C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC023CMessageInterpreter))]
	class CC023CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC023CMessageInterpreter, CC023CProvider>
	{
		public static ZString GetExpectedInterpretationText() => @"A Guarantor Notification (IE023) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Accepted Date</td><td>18-Sep-71 00:00</td></tr><tr><td>Customs Office Of Recovery At Departure</td><td>REF12345</td></tr><tr><td>Guarantor&#39;s Name</td><td>BOB THE BUILDER</td></tr><tr><td>Guarantor&#39;s Address</td><td>123 WHERE ST, CITY, IE</td></tr><tr><td>Guarantor Notification Date</td><td>18-Sep-71 00:00</td></tr><tr><td>Guarantor Notification Text</td><td>Guarantor Notification Text</td></tr></table>";

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE022;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => GetExpectedInterpretationText();

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC023CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override CC023CProvider GetProvider(TextReader reader) => new CC023CProvider(new MailBoxItemProvider<Cc023CType>(reader).Message);
	}
}
