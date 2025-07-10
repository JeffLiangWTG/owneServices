using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ComunicaControlesParV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class InboxNotificationNctsControlCommunicationMessageProcessorTest : NCTS5CommonInboxNotificationResponseMessageProcessorTest<InboxNotificationNctsControlCommunicationMessageProcessor, InboxNotificationNctsControlCommunicationMessagePrettyFormatter, ComunicaControlesParV1Sal>
	{
		public void TestProcessAcceptedMessage()
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
				"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CCOTPA - Controls Communication</td></tr></table>" +
				"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>02-12-2022, 11:20:38</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCodeClearance + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>";

			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, mrnEntrynum: MRNCodeClearance, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl);
		}

		protected override ZString GetMrncode() => MRNCodeClearance;

		const string MRNCodeClearance = "22ES000101500647K2";

		protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification NCTS Controls";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsControls };

		protected override ZString GetAcceptanceTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureControlNCTSTestFilePath, "AcceptedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureControlNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override InboxNotificationNctsControlCommunicationMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationNctsControlCommunicationMessageProcessor(logger);
	}
}
