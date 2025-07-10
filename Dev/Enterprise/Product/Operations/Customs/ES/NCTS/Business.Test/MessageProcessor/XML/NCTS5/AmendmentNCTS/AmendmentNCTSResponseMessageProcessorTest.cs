using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC013C_v515.CC013CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class AmendmentNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<AmendmentNCTSResponseMessageProcessor, AmendmentNCTSMessagePrettyFormatter, Cc013Cv1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceFile(), InterchangeID);
			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Amendment</H3>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>CAUU4NQB4W488BHT</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pre-Declaration</td></tr></table>";

			AssertEquals("ReleaseStatus should be 0 when process Amendment Declaration", "0", nctsHeader.BH_ReleaseStatus);
			AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: OriginalEntryStatus, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
		}

		protected override ZString PhaseStatusWhenErrorOrRejected => ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Amendment Declaration Message Processor";
		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment };
		string GetAcceptanceFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentNCTSTestFilePath, "AcceptedMessage.txt");
		protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentNCTSTestFilePath, "RejectedMessage.txt");
		protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentNCTSTestFilePath, "ErrorMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override AmendmentNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new AmendmentNCTSResponseMessageProcessor(logger);
	}
}
