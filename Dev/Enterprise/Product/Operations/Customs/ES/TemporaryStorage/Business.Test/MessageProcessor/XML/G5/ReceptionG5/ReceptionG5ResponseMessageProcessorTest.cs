using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5RecNotifV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing
{
	sealed class ReceptionG5ResponseMessageProcessorTest : G5CommonResponseMessageProcessorTest<ReceptionG5ResponseMessageProcessor, ReceptionG5MessagePrettyFormatter, G5RecNotifV1Sal>
	{
		public void TestProcessAcceptedMessage_GreenCircuit()
		{
			var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceGreenCircuitTestFile(), InterchangeID);
			ProcessMessageForTest(message, temporaryStorageHeader.Messages);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 15:18:09</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00097Y8</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000464</td></tr></table>" +
				"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000046</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>VQVE4H9SDGK2JERP</td></tr></table>";
			AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		}

		public void TestProcessAcceptedMessage_OrangeCircuit()
		{
			var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceOrangeCircuitTestFile(), InterchangeID);
			ProcessMessageForTest(message, temporaryStorageHeader.Messages);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 15:18:09</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00097Y8</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000464</td></tr></table>" +
				"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000046</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>VQVE4H9SDGK2JERP</td></tr></table>";
			AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.ORANGE, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl);
		}

		public void TestProcessAcceptedMessage_RedCircuit()
		{
			var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceRedCircuitTestFile(), InterchangeID);
			ProcessMessageForTest(message, temporaryStorageHeader.Messages);

			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 15:18:09</td></tr></table>" +
				"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00097Y8</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>VQVE4H9SDGK2JERP</td></tr></table>";
			AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.RED, expectedMrnNumber: MRNCode, expectedDsdtMRN: "", expectedCsvClearance: "", expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl);
		}

		string GetAcceptanceGreenCircuitTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionG5TestFilePath, "AcceptedMessageGreenCircuit.txt");

		string GetAcceptanceOrangeCircuitTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionG5TestFilePath, "AcceptedMessageOrangeCircuit.txt");

		string GetAcceptanceRedCircuitTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionG5TestFilePath, "AcceptedMessageRedCircuit.txt");

		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionG5TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		protected override ZString GetExpectedProcessorFriendlyName() => "G5 Reception Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.G5v1Reception };

		protected override ReceptionG5ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ReceptionG5ResponseMessageProcessor(logger);

		protected override string GetRejectedTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionG5TestFilePath, "RejectedMessage.txt");

		const string MRNCode = "24ES009999Y00097Y8";
		const string DSDTCode = "24ES00999880000464";
		const string CSVClearance = "5EA9589535D0AF79";
		readonly ZDateTime acceptanceDate = new ZDateTime(2024, 02, 28, 15, 18, 09);
	}
}
