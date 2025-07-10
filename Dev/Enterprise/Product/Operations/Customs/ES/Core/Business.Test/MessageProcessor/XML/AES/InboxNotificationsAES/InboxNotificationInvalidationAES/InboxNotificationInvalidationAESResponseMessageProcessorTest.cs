using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaInvalidacionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing;

public class InboxNotificationInvalidationAESResponseMessageProcessorTest : XMLResponseMessageProcessorTest<InboxNotificationInvalidationAESResponseMessageProcessor, IMessagePrettyFormatter, ComunicaInvalidacionV1Sal>
{
	public void TestProcessAcceptedMessage_InvalidatedByCustoms()
	{
		AddMessageProcessAndAssertResult_InvalidatedByCustoms();
	}

	public void TestProcessAcceptedMessage_NotInvalidatedByCustoms()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileNotInvalidatedByCustoms(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CINVAL - Invalidated Declaration</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES000101100342B6</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>01-02-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Requested Date:</td><td>&nbsp;&nbsp;</td><td>12-01-2022, 17:45:58</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Invalidación de PreDUA por caducidad de 30 días</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";

		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Cancelled, movementReferenceNumber: MRNCodeClearedNotPending, messageNum: MessageNum);
	}

	public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
	{
		AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearedNotPending, AddMessageProcessAndAssertResult_InvalidatedByCustoms);
	}

	void AddMessageProcessAndAssertResult_InvalidatedByCustoms()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustoms(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CINVAL - Invalidated Declaration</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>21ES000101100342B6</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Invalidation by Customs</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Date:</td><td>&nbsp;&nbsp;</td><td>01-02-2022</td></tr></table>" +
			"<table border=\"0\"><tr><td>Invalidation Reason:</td><td>&nbsp;&nbsp;</td><td>Invalidación de PreDUA por caducidad de 30 días</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PI - Pre-Declaration Invalidated</td></tr></table>";

		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Invalidated, movementReferenceNumber: MRNCodeClearedNotPending, messageNum: MessageNum);
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSNotEnabled()
	{
		using (SetTemporaryStorageEnabled(false))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileNotInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndLocationNotManagedInPremises()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			entryHeader.Declaration.CustomsEntryInstructions[0].GoodsLocation.Address.AuthorisationNumber = "9999000000";

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileNotInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndPremiseTypeADTNotManagedInPremises()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);
			regHeader.Premises.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileNotInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTransactionNotDUE()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileNotInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTransactionReferenceNotEntryHeader()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, "TestReference");

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileNotInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = regHeader.Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_PreRequisites(numTransactions, regHeader.SRH_Status, regLine.SRL_CustomsStatus, numGuaranteeTransactions);
		}
	}

	public void TestProcessCancelationResponse_WithCONAndPNDTransaction()
	{
		using (SetTemporaryStorageEnabled(true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, regLine2, guarantee) = TransactionsTestHelper.SetupTransactions(Factory, regLine, regHeader, entryHeader, cancelPreparationDate, MRNCodeClearedNotPending);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction_PreRequisites(regLine, regLine2, regHeader, guarantee, regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction(regLine, regLine2, regHeader, entryHeader, guarantee, regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, cancelPreparationDate, MRNCodeClearedNotPending, TransactionsAESCommentPrefix);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileInvalidatedByCustoms(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction_AfterProcess(regLine, regLine2, guarantee);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader.CH_EntryStatus = OriginalEntryStatus;
		entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending);

		(var orgHeader, var orgAddress) = SetUpOrganization();

		instruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;

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

		SetSentInterchange(entryHeader, InterchangeID);

		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "INV", "Invalidated", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Cancelled", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();
	}
	CusEntryInstruction instruction;
	EU.TemporaryStorage.Business.CusTempStorageRegHeader regHeader;
	EU.TemporaryStorage.Business.CusTempStorageRegLine regLine;

	string GetAcceptanceTestFileInvalidatedByCustoms() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationAESTestFilePath, "AcceptedMessageInvalidatedByCustoms.txt");
	string GetAcceptanceTestFileNotInvalidatedByCustoms() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationAESTestFilePath, "AcceptedMessageNotInvalidatedByCustoms.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	const string MRNCodeClearedNotPending = "21ES000101100342B6";
	const string MessageNum = "20220201220312939475";

	protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty; protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending);

		var interchangeID = ZGuid.NewZGuid();

		SetSentInterchange(entryHeader, interchangeID);

		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		responseInterchange.EI_SessionGUID = interchangeID;
		responseInterchange.EI_TransportType = interchangeTransportType;

		var message = Factory.New<TestEdiMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = MessageType;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageSubType = "AAA";
		message.EM_ApplicationReference = "TEST";
		message.EM_MessageText = GetAcceptanceTestFileInvalidatedByCustoms();

		responseInterchange.ContainedMessages.Add(message);

		return message;
	}

	protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

		var interchangeID = ZGuid.NewZGuid();

		SetSentInterchange(entryHeader, interchangeID);

		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		responseInterchange.EI_SessionGUID = interchangeID;
		responseInterchange.EI_TransportType = interchangeTransportType;

		var message = Factory.New<TestEdiMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = MessageType;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageSubType = "AAA";
		message.EM_ApplicationReference = "TEST";
		message.EM_MessageText = GetAcceptanceTestFileInvalidatedByCustoms();

		responseInterchange.ContainedMessages.Add(message);
		return message;
	}

	protected override void AssertLoggerMessagesWhenProcessMessageWrongXML()
	{
		AssertContains("logger", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
	}

	protected override void AssertLoggerMessagesWhenMessageProcessingError()
	{
		base.AssertLoggerMessagesWhenMessageProcessingError();

		AssertContains("logger exception", "Message Text is empty so can't continue with processing", GetAllConcatenatedUserLogStrings());
	}

	protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification Export Invalidation Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication };

	protected override InboxNotificationInvalidationAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationInvalidationAESResponseMessageProcessor(logger);

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString status, ZString referenceNum, string referenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ExportDeclaration)
	{
		var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = status;
		regLineTransaction1.SRT_InternalReferenceNumber = referenceNum;
		regLineTransaction1.SRT_InternalReferenceType = referenceType;
		regLineTransaction1.SRT_PhysicalInOutDate = cancelPreparationDate.ToDateTimeOffset(null);
		regLineTransaction1.SRT_Reference = MRNCodeClearedNotPending;

		return regLineTransaction1;
	}

	readonly ZDateTime cancelPreparationDate = new ZDateTime(2022, 02, 01, 22, 03, 17);
}
