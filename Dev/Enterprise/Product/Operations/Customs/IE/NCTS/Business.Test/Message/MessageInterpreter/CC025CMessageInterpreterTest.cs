using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC025C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC025CMessageInterpreter))]
	class CC025CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC025CMessageInterpreter, CC025CProvider>
	{
		internal static ZString GetExpectedInterpretationText(string releaseIndicator)
		{
			var releaseIndicatorCodeAndDescription = releaseIndicator;

			switch (releaseIndicator)
			{
				case "1":
					releaseIndicatorCodeAndDescription = $"1 - {MessageTestHelper.ReleaseNotification1}";
					break;
				case "2":
					releaseIndicatorCodeAndDescription = $"2 - {MessageTestHelper.ReleaseNotification2}";
					break;
				case "3":
					releaseIndicatorCodeAndDescription = $"3 - {MessageTestHelper.ReleaseNotification3}";
					break;
				case "4":
					releaseIndicatorCodeAndDescription = $"4 - {MessageTestHelper.ReleaseNotification4}";
					break;
			}

			return $@"
			A Goods Release Notification (IE025) message has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr>
				<tr><td>Release Date</td><td>18-Sep-71</td></tr>
				<tr><td>Release Indicator</td><td>{releaseIndicatorCodeAndDescription}</td></tr>
			</table><br />
			<br />
			House Consignment:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Sequence Number</td><td>1</td></tr>
				<tr><td>Release Type</td><td>1 - Partial release</td></tr>
			</table><br />
			<br />
			Consignment Item:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Goods Item Number</td><td>1</td></tr>
				<tr><td>Declaration Goods Item Number</td><td>1</td></tr>
				<tr><td>Release Type</td><td>1 - Partial release</td></tr>
			</table><br />
			<br />
			Consignment Item:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Goods Item Number</td><td>2</td></tr>
				<tr><td>Declaration Goods Item Number</td><td>2</td></tr>
				<tr><td>Release Type</td><td>2 - Full release</td></tr>
			</table><br />
			<br />
			House Consignment:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Sequence Number</td><td>2</td></tr>
				<tr><td>Release Type</td><td>2 - Full release</td></tr>
			</table><br />
			<br />
			Consignment Item:<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Goods Item Number</td><td>3</td></tr>
				<tr><td>Declaration Goods Item Number</td><td>3</td></tr>
				<tr><td>Release Type</td><td>2 - Full release</td></tr>
			</table>";
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE025;

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";

			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC025CText("21IEDUB11A782454R2", "2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => GetExpectedInterpretationText("2");

		protected override CC025CProvider GetProvider(TextReader reader) => new CC025CProvider(new MailBoxItemProvider<Cc025CType>(reader).Message);

		protected override void SetUp()
		{
			MessageTestHelper.SetupReleaseCodes(Factory);
			base.SetUp();
		}
	}
}
