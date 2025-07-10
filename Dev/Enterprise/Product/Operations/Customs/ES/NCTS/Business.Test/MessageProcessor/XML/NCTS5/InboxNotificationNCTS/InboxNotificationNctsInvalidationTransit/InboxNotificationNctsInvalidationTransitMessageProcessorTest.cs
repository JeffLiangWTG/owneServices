using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaInvaliTranV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class InboxNotificationNctsInvalidationTransitMessageProcessorTest : NCTS5CommonInboxNotificationResponseMessageProcessorTest<InboxNotificationNctsInvalidationTransitMessageProcessor, InboxNotificationNctsInvalidationTransitMessagePrettyFormatter, ComunicaInvaliTranV1Sal>
	{
		public void TestProcessAcceptedMessage_StatusPI()
		{
			AddMessageProcessAndAssertResult_AcceptedMessage();
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearance, AddMessageProcessAndAssertResult_AcceptedMessage);
		}

		void AddMessageProcessAndAssertResult_AcceptedMessage()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CINVAT - Invalidation Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>11-11-2022, 08:44:06</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500540K1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Invalidation Started by Customs:</td><td>&nbsp;&nbsp;</td><td>NO</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>11-11-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Breve razon</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";

			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, mrnEntrynum: MRNCodeClearance, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.Invalidated);
		}

		public void TestProcessAcceptedMessage_StatusIV()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFileStatusIV(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CINVAT - Invalidation Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>11-11-2022, 08:44:06</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500540K1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Invalidation Started by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>11-11-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Breve razon</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IV - Invalidated</td></tr></table>";

			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, mrnEntrynum: MRNCodeClearance, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.Invalidated);
		}

		public void TestProcessAcceptedMessage_StatusIG()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFileStatusIG(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CINVAT - Invalidation Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>11-11-2022, 08:44:06</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500540K1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Invalidation Started by Customs:</td><td>&nbsp;&nbsp;</td><td>YES</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>11-11-2022</td></tr></table>" +
				"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Breve razon</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>IG - Invalidated by Guarantee</td></tr></table>";
			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, mrnEntrynum: MRNCodeClearance, commonCustomsStatus: NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid);
		}

		protected override ZString GetMrncode() => MRNCodeClearance;

		const string MRNCodeClearance = "22ES000101500540K1";

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification NCTS Invalidation Comunication";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation };

		protected override ZString GetAcceptanceTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureInvalidateTransitNCTSTestFilePath, "AcceptedMessageInvalidatedStatusPI.txt");

		protected ZString GetAcceptanceTestFileStatusIV() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureInvalidateTransitNCTSTestFilePath, "AcceptedMessageInvalidatedStatusIV.txt");

		protected ZString GetAcceptanceTestFileStatusIG() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureInvalidateTransitNCTSTestFilePath, "AcceptedMessageInvalidatedStatusIG.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureInvalidateTransitNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override InboxNotificationNctsInvalidationTransitMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationNctsInvalidationTransitMessageProcessor(logger);
	}
}
