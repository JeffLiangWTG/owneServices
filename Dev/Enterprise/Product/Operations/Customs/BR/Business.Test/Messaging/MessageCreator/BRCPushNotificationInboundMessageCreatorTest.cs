using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCPushNotificationInboundMessageCreatorTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.PUS;

		public void TestGenerateMessageFromInterchange()
		{
			AssertProcessEDIInterchange(BRCExportPushNotificationMessageProcessorTest.JsonMessage, "EXP");

			AssertProcessEDIInterchange(BRCImportPushNotificationMessageProcessorTest.JsonMessageDiagnostic, "IMP");
			AssertProcessEDIInterchange(BRCImportPushNotificationMessageProcessorTest.JsonMessageRegister, "IMP");
			AssertProcessEDIInterchange(BRCImportPushNotificationMessageProcessorTest.JsonMessageSituation, "IMP");

			AssertProcessEDIInterchange(BRCLPCOPushNotificationMessageProcessorTest.JsonMessageChangeSituation, "LPC");
			AssertProcessEDIInterchange(BRCLPCOPushNotificationMessageProcessorTest.JsonMessageInclusion, "LPC");
			AssertProcessEDIInterchange(BRCLPCOPushNotificationMessageProcessorTest.JsonMessageCancel, "LPC");
			AssertProcessEDIInterchange(BRCLPCOPushNotificationMessageProcessorTest.JsonMessageAnalysis, "LPC");
			AssertProcessEDIInterchange(BRCLPCOPushNotificationMessageProcessorTest.JsonMessageAuto, "LPC");

			AssertProcessEDIInterchange(BRCCatalogPushNotificationMessageProcessorTest.GetMessageText(), "CAT");

			void AssertProcessEDIInterchange(string messageBody, string expectedMessageSubType)
			{
				AssertCreateMessageFromInterchange(messageBody, messageBody, expectedMessageSubType: expectedMessageSubType);
				AssertCreateMessageFromInterchange(CreateUniversalInterchangeXml(responseMessage: messageBody), messageBody, expectedMessageSubType: expectedMessageSubType);
			}
		}

		public void TestGenerateMessageFromInterchangeEmptyMessageBody()
		{
			var messageBody = ZString.Empty;
			AssertCreateMessageFromInterchange(messageBody, messageBody);
		}

		public void TestGenerateMessageFromInterchangeIncorrectMessage()
		{
			var messageBody = incorrectJsonMessage;
			AssertCreateMessageFromInterchange(messageBody, messageBody);
		}

		const string incorrectJsonMessage = @"
		{
			""genericBRFieldTest1"":2921,
			""genericBRFieldTest2"":""duex-historico""
		}";
	}
}
