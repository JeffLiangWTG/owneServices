using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC009C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC009CMessageInterpreter))]
	class CC009CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC009CMessageInterpreter, CC009CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE009;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";
			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var messageText = InterchangeProcessorTestHelper.GetStandardCC009CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", messageText, includeResponseWrap: false);
			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"
			An Invalidation Decision Message (IE009) has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>19AA12345678901230</td></tr>
				<tr><td>Request Date &amp; Time</td><td>30-Jan-23 15:30</td></tr>
				<tr><td>Decision Date &amp; Time</td><td>30-Jan-23 16:00</td></tr>
				<tr><td>Decision</td><td>Invalidated</td></tr>
				<tr><td>Initiated by Customs</td><td>Y</td></tr>
				<tr><td>Justification</td><td>Reason for decision</td></tr>
			</table>";

		protected override CC009CProvider GetProvider(TextReader reader) => new CC009CProvider(new MailBoxItemProvider<Cc009CType>(reader).Message);
	}
}
