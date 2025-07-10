using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC055C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC055CMessageInterpreter))]
	class CC055CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC055CMessageInterpreter, CC055CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE055;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";
			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var messageText = InterchangeProcessorTestHelper.GetStandardCC055CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", messageText, includeResponseWrap: false);
			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => @"
			A Guarantee Not Valid Message (IE055) has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>19MRNCC055C0123456</td></tr>
				<tr><td>Declaration Acceptance Date</td><td>31-Jan-23</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Sequence Number</td><td>1</td></tr>
				<tr><td>Guarantee Reference Number</td><td>12GRNCC055C012345A678901</td></tr>
				<tr><td>Invalid Guarantee Reason Sequence Number</td><td>1</td></tr>
				<tr><td>Invalid Guarantee Reason Code</td><td>G02</td></tr>
				<tr><td>Invalid Guarantee Reason Text</td><td>Guarantee exists, but not valid</td></tr>
				<tr><td>Invalid Guarantee Reason Sequence Number</td><td>2</td></tr>
				<tr><td>Invalid Guarantee Reason Code</td><td>G05</td></tr>
				<tr><td>Invalid Guarantee Reason Text</td><td>Guarantee error</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Sequence Number</td><td>2</td></tr>
				<tr><td>Guarantee Reference Number</td><td>12GRNCC055C012345A678901</td></tr>
				<tr><td>Invalid Guarantee Reason Sequence Number</td><td>1</td></tr>
				<tr><td>Invalid Guarantee Reason Code</td><td>G04</td></tr>
				<tr><td>Invalid Guarantee Reason Text</td><td>Holder of Guarantee is not equal to Holder of Transit procedure in declaration</td></tr>
			</table>";

		protected override CC055CProvider GetProvider(TextReader reader) => new CC055CProvider(new MailBoxItemProvider<Cc055CType>(reader).Message);
	}
}
