using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC225C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC225CMessageInterpreter))]
	class CC225CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC225CMessageInterpreter, CC225CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE225;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC225CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => GetExpectedInterpretationText();

		protected override CC225CProvider GetProvider(TextReader reader) => new CC225CProvider(new MailBoxItemProvider<Cc225CType>(reader).Message);

		public static ZString GetExpectedInterpretationText() => @"A Guarantee Update Notification (IE225) message has been received for GRN, Job: B00001000<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""></table><br />
<br />Guarantee Information:<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>GRN</td>
		<td>12GRNCC055C012345A678901</td>
	</tr>
	<tr>
		<td>Currency</td>
		<td>GBP</td>
	</tr>
	<tr>
		<td>Reference amount</td>
		<td>123</td>
	</tr>
	<tr>
		<td>Percentage of reference amount</td>
		<td>20</td>
	</tr>
	<tr>
		<td>Guarantee amount</td>
		<td>234</td>
	</tr>
	<tr>
		<td>Number of certificates</td>
		<td>45</td>
	</tr>
	<tr>
		<td>Validity date</td>
		<td>25-Jul-23</td>
	</tr>
	<tr>
		<td>Invalidity date</td>
		<td>26-Jul-23</td>
	</tr>
	<tr>
		<td>Invalidity reason code</td>
		<td>ABC</td>
	</tr>
	<tr>
		<td>Restricted use (suspended goods)</td>
		<td>Y</td>
	</tr>
	<tr>
		<td>Custom office of Guarantee Reference number</td>
		<td>RNCC037C</td>
	</tr>
</table>";
	}
}
