using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC044C_v515.CC044CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NotificationUnloadingNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<NotificationUnloadingNCTSResponseMessageProcessor, NotificationUnloadingNCTSMessagePrettyFormatter, Cc044Cv1Sal>
	{
		public void TestProcessAcceptedMessageStatusRD()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceResponseStatusRD(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES005001500004J1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>RD - Pending Resolution Of Discrepancy</td></tr></table>";

			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		public void TestProcessAcceptedMessageStatusUL()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceResponseStatusUL(), InterchangeID);
			ProcessMessageForTest(message);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>23ES005001500004J1</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>UL - Completed</td></tr></table>";

			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		}

		public void TestSetReleaseDate()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceResponseStatusUL(), InterchangeID);
			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				AssertEquals("BM_CustomsStatus is CL1", ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("ReleaseDate is set", new ZDateTime(2020, 11, 20, 0, 0, 0), nctsHeader.ReleaseDate);
			});
		}

		protected override ZString PhaseStatusWhenErrorOrRejected => ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;

		protected override ZString PhaseStatusWhenWrongXML => ESNctsMovementHeaderTransactionStatusList.Codes.Arrival;

		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Notification Unloading Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods };

		string GetAcceptanceResponseStatusRD() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotificationUnloadingNCTSTestFilePath, "AcceptedMessageRD.txt");

		string GetAcceptanceResponseStatusUL() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotificationUnloadingNCTSTestFilePath, "AcceptedMessageUL.txt");

		protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotificationUnloadingNCTSTestFilePath, "RejectedMessage.txt");

		protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotificationUnloadingNCTSTestFilePath, "ErrorMessage.txt");

		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotificationUnloadingNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString TypeDeclaration => NctsMovementType.Codes.Arrival;

		protected override NotificationUnloadingNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new NotificationUnloadingNCTSResponseMessageProcessor(logger);
	}
}
