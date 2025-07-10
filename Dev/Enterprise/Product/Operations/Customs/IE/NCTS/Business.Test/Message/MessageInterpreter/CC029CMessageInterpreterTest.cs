using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC029C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC029CMessageInterpreter))]
	class CC029CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC029CMessageInterpreter, CC029CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE029;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) =>
@"A Release of Transit (IE029) message has been received for Job B00000012.<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
	<tr>
		<td>MRN</td>
		<td>21IEDUB11A782454R2</td>
	</tr>
	<tr>
		<td>LRN</td>
		<td>LRN123456789</td>
	</tr>
	<tr>
		<td>Release Date</td>
		<td>21-Jul-23</td>
	</tr>
	<tr>
		<td>Additional Declaration Type</td>
		<td>A</td>
	</tr>
	<tr>
		<td>Control Result Code</td>
		<td>R1</td>
	</tr>
	<tr>
		<td>Control Result Date</td>
		<td>20-Jul-23</td>
	</tr>
	<tr>
		<td>Control Result Controller</td>
		<td>IEDUB499</td>
	</tr>
	<tr>
		<td>Control Result Text</td>
		<td>Control Result Text.</td>
	</tr>
</table>";

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00000012";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC029CText("21IEDUB11A782454R2", "LRN123456789");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override CC029CProvider GetProvider(TextReader reader) => new CC029CProvider(new MailBoxItemProvider<Cc029CType>(reader).Message);
	}
}
