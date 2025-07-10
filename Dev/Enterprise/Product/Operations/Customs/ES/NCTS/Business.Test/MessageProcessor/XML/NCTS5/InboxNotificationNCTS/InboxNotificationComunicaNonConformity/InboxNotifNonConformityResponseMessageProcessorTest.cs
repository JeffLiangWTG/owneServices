using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaDisconformeParV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class InboxNotifNonConformityResponseMessageProcessorTest : NCTS5CommonInboxNotificationResponseMessageProcessorTest<InboxNotifNonConformityResponseMessageProcessor, InboxNotifNonConformityResponseMessagePrettyFormatter, ComunicaDisconformeParV1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			AddMessageProcessAndAssertResult_AcceptedMessage();
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCode, AddMessageProcessAndAssertResult_AcceptedMessage);
		}

		void AddMessageProcessAndAssertResult_AcceptedMessage()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CDITPA - Non-conformity Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>02-12-2022, 11:20:38</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Remarks</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>NL - Not Cleared</td></tr></table>";

			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, mrnEntrynum: MRNCode, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit);
		}

		protected override ZString GetMrncode() => MRNCode;

		const string MRNCode = "22ES000101500647K2";

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification NCTS Non-conformity Communication";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture };

		protected override ZString GetAcceptanceTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureDissatisfiedItemNCTSTestFilePath, "AcceptedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureDissatisfiedItemNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override InboxNotifNonConformityResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotifNonConformityResponseMessageProcessor(logger);
	}
}
