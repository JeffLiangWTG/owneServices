using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.PDI400V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
namespace Enterprise.Customs.ES.Business.Testing;

sealed class IncompleteImportH1ResponseMessageProcessorTest : ImportH1CommonResponseMessageProcessorTest<IncompleteImportH1ResponseMessageProcessor, Pdi400V1Sal>
{
	public void TestProcessAcceptanceTestFileOperationRegistered10()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileOperationRegistered10(), InterchangeID);
		ProcessMessageForTest(message);
		AssertIncompleteImportH1Declaration(message, entryStatusCode: EntryStatusCodes.IncompletePreDeclaration, operationText: "10 - Accepted PDI");
	}

	public void TestProcessAcceptanceTestFileOperationRegistered11()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileOperationRegistered11(), InterchangeID);
		ProcessMessageForTest(message);
		AssertIncompleteImportH1Declaration(message, entryStatusCode: OriginalEntryStatus, operationText: "11 - Amended PDI");
	}

	ZString ExpectedMessageDetailsAccepted(ZString operationText) => "<H3>Accepted Declaration</H3>" +
		$"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>{operationText}</td></tr></table>" +
		"<table border=\"0\"><tr><td>Domain:</td><td>&nbsp;&nbsp;</td><td>10 - No CCI, no national centralized</td></tr></table>" +
		"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>13-08-2024, 17:41:57</td></tr></table>" +
		"<br><table border=\"0\"><tr><td>Customs Registration Number:</td><td>&nbsp;&nbsp;</td><td>24ES009998I0004SR7</td></tr></table>" +
		"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>ABCDEFGHIJKLMNOP</td></tr></table>" +
		"<br><H3>Required Certificates</H3>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Item</strong></td><td><strong>Measure</strong></td><td><strong>Agency</strong></td><td><strong>Documents</strong></td></tr>" +
			"<tr><td>1</td><td>410</td><td>SIF02 Sanidad Animal - Mº Agricultura</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>N851 - CERTIFICADO FITOSANITARIO</td></tr>" +
					"<tr><td>C085 - DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]</td></tr>" +
				"</table></td></tr>" +
			"<tr><td>1</td><td>405</td><td>SIF02 Sanidad Animal - Mº Agricultura</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS(B)</td></tr>" +
					"<tr><td>C657 - CERTIFICADO DE SANIDAD</td></tr>" +
				"</table></td></tr>" +
			"<tr><td>1</td><td>710</td><td>SIF01 SOIVRE Mº Comercio</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>C678 - DOC.SANIT.COMUN PIENSOS+ALIMENTOS</td></tr>" +
					"<tr><td>1405 - INSPECCION SANIDAD EXTERIOR. NO PROCEDE</td></tr>" +
				"</table></td></tr>" +
			"<tr><td>2</td><td>410</td><td>SIF03 Sanidad Animal - Mº Agricultura</td>" +
				"<td><table width=\"100%\">" +
					"<tr><td>1413 - INSPECCION SANIDAD EXTERIOR-NO AFECTADOS</td></tr>" +
				"</table></td></tr>" +
			"</table>" +
			"<br><H3>Notifications</H3>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Code</strong></td><td><strong>Text</strong></td></tr>" +
				"<tr><td>1</td><td>Text1</td></tr>" +
				"<tr><td>2</td><td>Text2</td></tr>" +
				"</table>";

	void AssertIncompleteImportH1Declaration(TestEdiMessage message, string entryStatusCode, string operationText)
	{
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: ExpectedMessageDetailsAccepted(operationText), messageSubType: "ACC", entryStatusCode: entryStatusCode, movementReferenceNumber: MRNCode, exportMRN: ExportMRNCode, messageNum: MessageNum);
	}

	protected override void SetUp()
	{
		base.SetUp();

		Factory.AddDocumentsToRefDataForTest();
	}

	string GetAcceptanceTestFileOperationRegistered10() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.IncompleteImportH1TestFilePath, "AcceptedMessageOperationRegistered10.txt");
	string GetAcceptanceTestFileOperationRegistered11() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.IncompleteImportH1TestFilePath, "AcceptedMessageOperationRegistered11.txt");
	protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.IncompleteImportH1TestFilePath, "RejectedMessage.txt");
	protected override string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.IncompleteImportH1TestFilePath, "ErrorMessage.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.IncompleteImportH1TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	protected override ZString GetExpectedProcessorFriendlyName() => "Import Incomplete Pre-Declaration (H1) Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => [DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1];

	protected override IncompleteImportH1ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new IncompleteImportH1ResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
}
