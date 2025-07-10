using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DeclaComplemVinculV2Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXDVDResponseMessageProcessorTest : XMLResponseMessageProcessorTest<ComplXDVDResponseMessageProcessor, ComplXDVDMessagePrettyFormatter, DeclaComplemVinculV2Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
				"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>(6) Admisión de Doc. Vinculacion en plazo por Complementación de DVD Provisional</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>DVD22ES009999D04345R3</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>RTANG49KWEYYRWWF</td></tr></table>";

			AssertComplXDVDResponse(message, entryStatusCode: EntryStatusCodes.Cleared, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, movementReferenceNumber: MovementReferenceNumber);
		}

		public void TestProcessRejectedMessage()
		{
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(responseMessage);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Location</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>381</td><td>No se ha declarado un documento precedente compatible con el regimen aduanero  :7100 Debe declarar algún Doc.Precedente del tipo:337, no permitido tipo ZZZ</td><td>DocPrecedente_Partida00001_ED_2_1_DocumentoPrecedente[0](00001)</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>1234</td><td>Error Description</td><td>ErrorTag</td><td>1</td><td>Wrong Value</td></tr>" +
				"</table>";

			AssertComplXDVDResponse(responseMessage, entryStatusCode: OriginalEntryStatus, messageSubType: "REJ", expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		public void TestProcessAcceptedMessageCreatesEntryLineSupportingDocuments()
		{
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000001", subType: "LIQ", status: "ACC");
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000002", subType: "LIQ", status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000003", subType: "LIQ", status: "REJ");
			entryLine.AddEntryLineDocument<SupportingDocument>("5018", "ES3600000004", subType: ZString.Empty, status: ZString.Empty);
			entryLine.AddEntryLineDocument<SupportingDocument>("AAAA", "ES3600000005", subType: "LIQ", status: "ACC");

			Factory.Save();

			CombineAssertions("Before", () =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 5 CL SupportingDocuments before calling ProcessDVDEntryLineSupportingDocuments", 5, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 5 CL SupportingDocuments before calling ProcessDVDEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000001", "ES3600000002", "ES3600000003", "ES3600000004", "ES3600000005" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000001"), "5018", "ES3600000001", "LIQ", "ACC");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000002"), "5018", "ES3600000002", "LIQ", ZString.Empty);
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000003"), "5018", "ES3600000003", "LIQ", "REJ");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000004"), "5018", "ES3600000004", ZString.Empty, ZString.Empty);
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000005"), "AAAA", "ES3600000005", "LIQ", "ACC");
			});

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			CombineAssertions(() =>
			{
				var clSupDocs = GetCLSupportingDocuments();
				AssertEquals("There are 4 CL SupportingDocuments after calling ProcessDVDEntryLineSupportingDocuments", 4, clSupDocs.Length);
				AssertContainsExactElementsInAnyOrder("The 4 CL SupportingDocuments after calling ProcessDVDEntryLineSupportingDocuments have the correct CSI_ReferenceNumber", new ZString[] { "ES3600000002", "ES3600000003", "ES3600000004", "ES3600000005" }, clSupDocs.Select(x => x.CSI_ReferenceNumber).ToArray());
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000002"), "5018", "ES3600000002", "LIQ", "ACC");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000003"), "5018", "ES3600000003", "LIQ", "ACC");
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000004"), "5018", "ES3600000004", ZString.Empty, ZString.Empty);
				AssertDocument(clSupDocs.FirstOrDefault(x => x.CSI_ReferenceNumber == "ES3600000005"), "AAAA", "ES3600000005", "LIQ", "ACC");
			});
		}

		public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageRegisterNotEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

				entryHeader.CH_EntryStatus = ZString.Empty;

				AddMessageProcessAndAssertResult_RejectedMessage(true);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

				entryHeader.CH_EntryStatus = ZString.Empty;

				AddMessageProcessAndAssertResult_RejectedMessage(true);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestProcessRejectedMessage_EntryStatusNotEmpty_TemporaryStorageEnabled()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption(CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

				AddMessageProcessAndAssertResult_RejectedMessage(false);

				CombineAssertions(() =>
				{
					AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
					AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
					AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
				});
			}
		}

		public void TestLoggerWriteOffTransactionError()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(entryHeader, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.WarehouseDeclaration);

				var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

				ProcessMessageForTest(message);
			}

			var expectedError = "Reference reference has a positive balance of 2.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
			AssertContains("logger exception", expectedError, GetAllConcatenatedUserLogStrings());
		}

		protected void AddMessageProcessAndAssertResult_RejectedMessage(bool isEntryStatusEmpty = false)
		{
			var entryStatus = isEntryStatusEmpty ? ZString.Empty : RejectedEntryStatus;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);
			var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Location</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>381</td><td>No se ha declarado un documento precedente compatible con el regimen aduanero  :7100 Debe declarar algún Doc.Precedente del tipo:337, no permitido tipo ZZZ</td><td>DocPrecedente_Partida00001_ED_2_1_DocumentoPrecedente[0](00001)</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
				"<tr><td>1234</td><td>Error Description</td><td>ErrorTag</td><td>1</td><td>Wrong Value</td></tr>" +
				"</table>";

			ProcessMessageForTest(message);
			AssertComplXDVDResponse(message, entryStatusCode: isEntryStatusEmpty ? ZString.Empty : OriginalEntryStatus, messageSubType: "REJ", expectedMessageInterpretation: expectedMessageInterpretationTextRejected);
		}

		void AssertComplXDVDResponse(TestEdiMessage message, string entryStatusCode, string messageSubType, string expectedMessageInterpretation = "", string movementReferenceNumber = "")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, entryStatusCode: entryStatusCode, messageNum: MessageNum, movementReferenceNumber: movementReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = DeclarantName;
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
			declaration.Declarant.OA_OH = declarant.PK;

			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.AllEntryLines.AddNew();

			SetSentInterchange(entryHeader, InterchangeID);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Cleared Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
		CusEntryInstruction entryInstruction;
		CusEntryLine entryLine;

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXDVDTestFilePath, "AcceptedMessage.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXDVDTestFilePath, "RejectedMessage.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXDVDTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

		const string MessageNum = "20221129141207913937";
		const string MovementReferenceNumber = "DVD22ES009999D04345R3";

		protected override ZString GetExpectedProcessorFriendlyName() => "DVD (H2) Compl. X Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.TypeXDvdH2 };

		protected virtual ZString RejectedEntryStatus => OriginalEntryStatus;

		protected override ComplXDVDResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ComplXDVDResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());
	}
}
