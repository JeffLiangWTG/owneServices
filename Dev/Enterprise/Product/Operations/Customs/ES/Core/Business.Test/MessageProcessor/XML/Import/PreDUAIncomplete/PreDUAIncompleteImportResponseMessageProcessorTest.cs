using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PreDeclaIncompletaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class PreDUAIncompleteImportResponseMessageProcessorTest : XMLResponseMessageProcessorTest<PreDUAIncompleteImportResponseMessageProcessor, IMessagePrettyFormatter, PreDeclaIncompletaV1Sal>
	{
		[TestDate(2021, 01, 15, 00, 00, 00)]
		public void TestProcessAcceptedMessage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var supportingDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			helper.CreateNewOrGetExistingCusCodeType(supportingDocumentType, "Supporting Documents for Import");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "N851", "CERTIFICADO FITOSANITARIO", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "C085", "DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "N853", "DOC.SANIT.COMUN ENTRADA PRODUCTOS(B)", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "C657", "CERTIFICADO DE SANIDAD", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "C678", "DOC.SANIT.COMUN PIENSOS+ALIMENTOS", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "1405", "INSPECCION SANIDAD EXTERIOR. NO PROCEDE", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "1413", "INSPECCION SANIDAD EXTERIOR-NO AFECTADOS", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, supportingDocumentType, "N003", "CERTIFICADO DE CALIDAD-R/UE", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration</H3>" +
				"<table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(0000)Operación Correcta</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>SZL6GY3FP7WLB5NP</td></tr></table>" +
				"<br><br><H3>Required Certificates</H3>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Item</strong></td><td><strong>Measure</strong></td><td><strong>Agency</strong></td><td><strong>Documents</strong></td></tr>" +
				"<tr><td>1</td><td>FTN</td><td>SIF03 Sanidad Vegetal-M Agricultura.</td><td><table width=\"100%\"><tr><td>N851 - CERTIFICADO FITOSANITARIO</td></tr><tr><td>C085 - DOCUMENTO SANITARIO COMUN PARA VEGETALES[A]</td></tr></table></td></tr>" +
				"<tr><td>1</td><td>SNM</td><td>SIF05 Sanidad Exterior - Mº Sanidad</td><td><table width=\"100%\"><tr><td>N853 - DOC.SANIT.COMUN ENTRADA PRODUCTOS(B)</td></tr><tr><td>C657 - CERTIFICADO DE SANIDAD</td></tr><tr><td>C678 - DOC.SANIT.COMUN PIENSOS+ALIMENTOS</td></tr><tr><td>1405 - INSPECCION SANIDAD EXTERIOR. NO PROCEDE</td></tr><tr><td>1413 - INSPECCION SANIDAD EXTERIOR-NO AFECTADOS</td></tr><tr><td>C640 - </td></tr></table></td></tr>" +
				"<tr><td>1</td><td>SVI</td><td>SIF01  Mº Comercio</td><td><table width=\"100%\"><tr><td>N003 - CERTIFICADO DE CALIDAD-R/UE</td></tr></table></td></tr>" +
				"</table>";
			AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, movementReferenceNumber: MRNCode);
		}

		public void TestProcessAcceptedMessageWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCode, MovementReferenceNumberIssueDate);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			AssertAcceptedEntryInstruction(message, entryHeader, movementReferenceNumber: MRNCode, movementReferenceNumberIssueDate: MovementReferenceNumberIssueDate);
		}

		public void TestProcessMessageRejected()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			var expectedMessageInterpretation = "<H4>Error = 2010 - Código de mercancia no valido</H4><H4>Item = 1</H4>";
			AssertAcceptedEntryInstruction(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "REJ", entryStatusCode: "INI");
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			var sentInterchange = SetSentInterchange(entryHeader, InterchangeID);
			sentInterchange.EI_To = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PDI", "Incomplete Pre-Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
			RefCusCodeListTypes.GetCachedList(Factory, EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
		}

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.PreDUAIncompleteImportTestFilePath, "AcceptedMessage.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.PreDUAIncompleteImportTestFilePath, "RejectedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => string.Empty;

		protected override PreDUAIncompleteImportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new PreDUAIncompleteImportResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Incomplete Pre SAD Import Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration };

		const string MRNCode = "20ES00999930006184";
		const string MessageNum = "20200409124949167000";
		protected readonly ZDateTime MovementReferenceNumberIssueDate = new ZDateTime(2021, 01, 15, 00, 00, 00);

		void AssertAcceptedEntryInstruction(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", ZDateTime? movementReferenceNumberIssueDate = null, string messageSubType = "ACC", string movementReferenceNumber = "", string entryStatusCode = "PDI")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, chStatus: "RCV", emStatus: "RCV", messageSubType: messageSubType, entryStatusCode: entryStatusCode, acceptanceDate: movementReferenceNumberIssueDate, movementReferenceNumber: movementReferenceNumber);
		}
	}
}
