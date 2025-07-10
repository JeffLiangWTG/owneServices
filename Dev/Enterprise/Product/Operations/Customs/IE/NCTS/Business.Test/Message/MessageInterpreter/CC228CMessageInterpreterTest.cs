using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC228C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC228CMessageInterpreter))]
	class CC228CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC228CMessageInterpreter, CC228CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE228;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC228CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => GetExpectedInterpretationText();

		protected override CC228CProvider GetProvider(TextReader reader) => new CC228CProvider(new MailBoxItemProvider<Cc228CType>(reader).Message);

		public static ZString GetExpectedInterpretationText() => @"A Comprehensive Guarantee Cancellation Liability Liberation Message (IE228) message has been received.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""></table><br />
<br />Guarantee Information:<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>Guarantor’s. identification number</td>
		<td>ID1</td>
	</tr>
	<tr>
		<td>Guarantor’s. Name</td>
		<td>BOB THE BUILDER</td>
	</tr>
	<tr>
		<td>Guarantor’s. Address </td>
		<td>2020 CITY IE</td>
	</tr>
	<tr>
		<td>GRN</td>
		<td>12GRNCC055C012345A678901</td>
	</tr>
	<tr>
		<td>Currency</td>
		<td>GBP</td>
	</tr>
	<tr>
		<td>Guarantee amount</td>
		<td>234</td>
	</tr>
	<tr>
		<td>Invalidity date</td>
		<td>26-Jul-23</td>
	</tr>
	<tr>
		<td>Liability liberation date </td>
		<td>26-Jul-23</td>
	</tr>
	<tr>
		<td>Customs Office Of Guarantee</td>
		<td>RNCC037C</td>
	</tr>
</table>";
	}
}
