using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC060C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC060CMessageInterpreter))]
	class CC060CMessageInterpreterTest : InboundMessageInterpreterAbstractTest<NCTSInboundEDIMessage, CC060CMessageInterpreter, CC060CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE060;

		protected override ZString GetExpectedInterpretation(NCTSInboundEDIMessage message) => GetExpectedInterpretationText();

		protected override NCTSInboundEDIMessage CreateIncomingMessageToTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "B00001000";
			var message = Factory.New<NCTSInboundEDIMessage>();
			message.EM_LinkedObject = nctsHeader;
			var text = InterchangeProcessorTestHelper.GetStandardCC060CText();
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", text, includeResponseWrap: false);
			return message;
		}

		protected override CC060CProvider GetProvider(TextReader reader) => new CC060CProvider(new MailBoxItemProvider<Cc060CType>(reader).Message);

		public static ZString GetExpectedInterpretationText(string notificationType = "0")
		{
			var notificationTypeCodeAndDescription = notificationType;

			switch (notificationType)
			{
				case "0":
					notificationTypeCodeAndDescription = $"0 - Decision to Control (and requested documents if needed)";
					break;
				case "1":
					notificationTypeCodeAndDescription = $"1 - Additional documents request";
					break;
				case "2":
					notificationTypeCodeAndDescription = $"2 - Intention to Control";
					break;
			}
			return $@"A Control Decision Notification (IE060) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>19MRNCC060C0123456</td></tr><tr><td>Control Notification Date &amp; Time</td><td>31-Jan-23 10:22</td></tr><tr><td>Notification Type</td><td>{notificationTypeCodeAndDescription}</td></tr></table><br /><br />Type of Control:<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Sequence Number</td><td>1</td></tr><tr><td>Control Type</td><td>10 - Documentary controls</td></tr><tr><td>Control Text</td><td>Documentary controls</td></tr></table><br /><br />Type of Control:<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Sequence Number</td><td>2</td></tr><tr><td>Control Type</td><td>50 - Other</td></tr><tr><td>Control Text</td><td>Another text related to the type of control</td></tr></table><br /><br />Requested Document:<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Sequence Number</td><td>1</td></tr><tr><td>Requested Document Type</td><td>Y022 - Consignor / exporter (AEO certificate number)</td></tr><tr><td>Requested Document Description</td><td>Consignor / exporter (AEO certificate number)</td></tr></table><br /><br />Requested Document:<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Sequence Number</td><td>2</td></tr><tr><td>Requested Document Type</td><td>Y029 - Other authorised economic operator (AEO certificate number)</td></tr><tr><td>Requested Document Description</td><td>Other text</td></tr></table>";
		}

		protected override void SetUp()
		{
			MessageTestHelper.SetupCL384Types(Factory);
			MessageTestHelper.SetupCL716Types(Factory);
			MessageTestHelper.SetupCL215Types(Factory);
			base.SetUp();
		}
	}
}
