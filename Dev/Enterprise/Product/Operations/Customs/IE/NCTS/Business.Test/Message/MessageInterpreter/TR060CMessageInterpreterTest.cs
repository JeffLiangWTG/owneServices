using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR060C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing;

[TestedType(typeof(TR060CMessageInterpreter))]
sealed class TR060CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, TR060CMessageInterpreter, TR060CProvider>
{
	protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR060C;

	protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_JobReference = "B00001000";

		var message = Factory.New<NCTSInboundEDIMessage>();
		message.EM_LinkedObject = nctsHeader;
		var tr060Text = InterchangeProcessorTestHelper.GetStandardTR060CText("19MRNCC060C0123456");
		message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", tr060Text, includeResponseWrap: false);

		return message;
	}

	protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => ExpectedInterpretation;

	protected override TR060CProvider GetProvider(TextReader reader) => new TR060CProvider(new MailBoxItemProvider<Tr060C>(reader).Message);

	protected override void SetUp()
	{
		base.SetUp();
		MessageTestHelper.SetupCL384Types(Factory);
	}

	internal static ZString ExpectedInterpretation => @"A control message (TR060) from Office of Destination has been received for Job B00001000.<br />
		<br />
		<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
			<tr><td>Customs Office of Destination</td><td>RNCC060C</td></tr>
			<tr><td>MRN</td><td>19MRNCC060C0123456</td></tr>
			<tr><td>Control Notification Date &amp; Time</td><td>30-Apr-25</td></tr>
			<tr><td>Notification Type</td><td>0 - Decision to Control (and requested documents if needed)</td></tr>
		</table><br />
		<br />
		Control Types:<br />
		<br />
		<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
			<tr><td>Control Type</td><td>10</td></tr>
			<tr><td>Control Text</td><td>Documentary controls</td></tr>
		</table><br />
		<br />
		Control Types:<br />
		<br />
		<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
			<tr><td>Control Type</td><td>50</td></tr>
			<tr><td>Control Text</td><td>Another text</td></tr>
		</table><br />
		<br />
		Requested Documents:<br />
		<br />
		<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
			<tr><td>Requested Document Type</td><td>Y022</td></tr>
			<tr><td>Requested Document Description</td><td>Consignor / exporter (AEO certificate number)</td></tr>
		</table><br />
		<br />
		Requested Documents:<br />
		<br />
		<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
			<tr><td>Requested Document Type</td><td>Y029</td></tr>
			<tr><td>Requested Document Description</td><td>Other text</td></tr>
		</table>";
}
