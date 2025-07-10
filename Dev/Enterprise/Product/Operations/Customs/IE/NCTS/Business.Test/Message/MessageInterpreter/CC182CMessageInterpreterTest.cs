using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC182C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC182CMessageInterpreter))]
	class CC182CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC182CMessageInterpreter, CC182CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE182;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var cc182Text = InterchangeProcessorTestHelper.GetStandardCC182CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc182Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"
			A Forwarded Incident Notification Message (IE182) has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>19AA12345678901230</td></tr>
				<tr><td>Incident Notification Date &amp; Time</td><td>30-Jan-23 15:00</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Incident Code</td><td>1</td></tr>
				<tr><td>Incident Text</td><td>Incident</td></tr>
				<tr><td>Endorsement Date</td><td>20-Jan-23</td></tr>
				<tr><td>Endorsement Authority</td><td>A</td></tr>
				<tr><td>Endorsement Place</td><td>Endorsement Place</td></tr>
				<tr><td>Endorsement Country</td><td>IE</td></tr>
				<tr><td>Container Number</td><td>1</td></tr>
				<tr><td>-Number of Seals</td><td>1</td></tr>
				<tr><td>-Seals Identifier</td><td>1</td></tr>
			</table>";

		protected override CC182CProvider GetProvider(TextReader reader) => new CC182CProvider(new MailBoxItemProvider<Cc182CType>(reader).Message);
	}
}
