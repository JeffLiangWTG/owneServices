using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC514C_v514.CC514CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing;

public class CancelAESResponseMessageProcessorTest : AESCommonResponseMessageProcessorTest<CancelAESResponseMessageProcessor, Cc514Cv1Sal>
{
	public void TestProcessAcceptedMessage_CAN()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancellationOK(), InterchangeID);

		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
			"<br><table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>07-06-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>TU4XCK2LBWJGHRRV</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>CA - Declaration Canceled</td></tr></table>";

		AssertExportCancellation(message, entryStatusCode: EntryStatusCodes.Cancelled, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	public void TestProcessAcceptedMessage_NotCAN_AndTriggerInboxRequest_EHub()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileConsult(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>TU4XCK2LBWJGHRRV</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";

			AssertExportCancellation(message, entryStatusCode: OriginalEntryStatus, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewInboxMessages(entryHeader.Messages, new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication }, DeclarantId, DeclarantName, MRNCode);
		}
	}

	public void TestProcessAcceptedMessage_NotCAN_AndTriggerInboxRequest_xT()
	{
		using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileConsult(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
				"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>TU4XCK2LBWJGHRRV</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>AW - Waiting PCO Decision</td></tr></table>";

			AssertExportCancellation(message, entryStatusCode: OriginalEntryStatus, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText);

			AssertNewCusPollingTransaction(entryHeader.PK, entryHeader.TablePrefix, new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication }, MRNCode);
		}
	}

	void AssertExportCancellation(TestEdiMessage message, string entryStatusCode, string messageSubType, string expectedMessageInterpretation = "")
	{
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, movementReferenceNumber: MRNCode, acceptanceDate: MovementReferenceNumberIssueDate, messageSubType: messageSubType, entryStatusCode: entryStatusCode, messageNum: MessageNum);
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSNotEnabled()
	{
		using (SetTemporaryStorageEnabled(false))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancellationOK(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndLocationNotManagedInPremises()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			entryHeader.Declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = "9999000000";

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancellationOK(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndPremiseTypeADTNotManagedInPremises()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			regHeader.Premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancellationOK(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndMessageNotCAN()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileConsult(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTransactionNotDUE()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancellationOK(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTransactionReferenceNotEntryHeader()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, "TestReference");

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancellationOK(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONAndPNDTransaction()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, regLine2, guarantee) = TransactionsTestHelper.SetupTransactions(Factory, regLine, regHeader, entryHeader, cancelPreparationDate, MRNCode);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction_PreRequisites(regLine, regLine2, regHeader, guarantee, regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancellationOK(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction(regLine, regLine2, regHeader, entryHeader, guarantee, regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, cancelPreparationDate, MRNCode, TransactionsAESCommentPrefix);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileCancellationOK(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction_AfterProcess(regLine, regLine2, guarantee);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader.CH_EntryStatus = OriginalEntryStatus;
		entryHeader.MovementReferenceNumberSetter(MRNCode, MovementReferenceNumberIssueDate);

		(var orgHeader, var orgAddress) = SetUpOrganization();

		regHeader = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";
		regHeader.SRH_Status = "CLS";

		SetUpPremises(entryHeader, orgAddress, regHeader);

		SetUpGuaranteeForRegHeader(orgHeader, regHeader);

		regLine = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		regLine.SRL_CustomsStatus = "CLS";
	}
	EU.TemporaryStorage.Business.CusTempStorageRegHeader regHeader;
	EU.TemporaryStorage.Business.CusTempStorageRegLine regLine;

	string GetAcceptanceTestFileCancellationOK() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelAESTestFilePath, "AcceptedMessageCancellationOK.txt");
	string GetAcceptanceTestFileConsult() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelAESTestFilePath, "AcceptedMessageConsult.txt");
	protected override string GetErrorTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelAESTestFilePath, "ErrorMessage.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	protected override ZString RejectedMessageStatus => EDIMessage.Status.Rejected;

	protected override ZString RejectedMRN => MRNCode;
	protected override ZDateTime RejectedAcceptanceDate => MovementReferenceNumberIssueDate;

	protected override ZString ErrorMRN => MRNCode;

	protected override ZDateTime ErrorAcceptanceDate => MovementReferenceNumberIssueDate;

	protected override ZString GetExpectedProcessorFriendlyName() => "Export Cancel Declaration Message Processor";
	protected override ZString ErrorMessageInterpretation => "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
			"<tr><td>14 / 34</td><td>782</td><td>18</td><td>Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC514CV1Ent.xsd}security y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adex/jdit/ws/aes/CC514CV1Ent.xsd}totalAmountInvoiced</td><td>Wrong value</td></tr>" +
			"</table>";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportCancellation };

	protected override CancelAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new CancelAESResponseMessageProcessor(logger);

	protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelAESTestFilePath, "RejectedMessage.txt");

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString status, ZString referenceNum, string referenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration)
	{
		var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = status;
		regLineTransaction1.SRT_InternalReferenceNumber = referenceNum;
		regLineTransaction1.SRT_InternalReferenceType = referenceType;
		regLineTransaction1.SRT_PhysicalInOutDate = cancelPreparationDate.ToDateTimeOffset(null);
		regLineTransaction1.SRT_Reference = MRNCode;

		return regLineTransaction1;
	}

	readonly ZDateTime cancelPreparationDate = new ZDateTime(2022, 06, 07, 17, 30, 46);
}
