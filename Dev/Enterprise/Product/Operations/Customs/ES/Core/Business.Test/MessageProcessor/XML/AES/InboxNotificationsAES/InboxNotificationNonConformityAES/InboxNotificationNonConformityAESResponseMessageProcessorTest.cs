using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaDisconformeExporV1Sal;
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

public class InboxNotificationNonConformityAESResponseMessageProcessorTest : XMLResponseMessageProcessorTest<InboxNotificationNonConformityAESResponseMessageProcessor, IMessagePrettyFormatter, ComunicaDisconformeExporV1Sal>
{
	public void TestProcessAcceptedMessage()
	{
		AddMessageProcessAndAssertResult();
	}

	public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
	{
		AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearedNotPending, AddMessageProcessAndAssertResult);
	}

	void AddMessageProcessAndAssertResult()
	{
		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

		ProcessMessageForTest(message);

		var expectedMessageInterpretationText = "<H3>Inbox Communication</H3>" +
			"<br><table border=\"0\"><tr><td>Message Type:</td><td>&nbsp;&nbsp;</td><td>CDISEX - Non-Conformity Communication</td></tr></table>" +
			"<table border=\"0\"><tr><td>Received:</td><td>&nbsp;&nbsp;</td><td>27-10-2022, 10:12:18</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101100871B9</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Remarks:</td><td>&nbsp;&nbsp;</td><td>Despacho disconforme</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>NL - Not cleared</td></tr></table>";

		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: "ACC", entryStatusCode: EntryStatusCodes.Invalidated, movementReferenceNumber: MRNCodeClearedNotPending, messageNum: MessageNum);
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSNotEnabled()
	{
		using (SetTemporaryStorageEnabled(false))
		{
			SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, entryHeader.CH_BGMReference);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
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

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
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

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
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

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
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

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
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
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, regLine2, guarantee) = TransactionsTestHelper.SetupTransactions(Factory, regLine, regHeader, entryHeader, cancelPreparationDate, MRNCodeClearedNotPending);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction_PreRequisites(regLine, regLine2, regHeader, guarantee, regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
			ProcessMessageForTest(message);

			TransactionsTestHelper.AssertProcessCancelationResponse_WithCONAndPNDTransaction(regLine, regLine2, regHeader, entryHeader, guarantee, regLineTransaction1, regLineTransaction2, regLineTransaction3, regLineTransaction4, regLineTransaction5, cancelPreparationDate, MRNCodeClearedNotPending, TransactionsAESCommentPrefix);

			message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);
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
		Factory.Save();
	}
	CusEntryInstruction instruction;
	EU.TemporaryStorage.Business.CusTempStorageRegHeader regHeader;
	EU.TemporaryStorage.Business.CusTempStorageRegLine regLine;

	string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationNonConformityAESTestFilePath, "AcceptedMessage.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationNonConformityAESTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	const string MRNCodeClearedNotPending = "22ES000101100871B9";
	const string MessageNum = "20221027101216082089";

	protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;

	protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
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
		message.EM_MessageText = GetAcceptanceTestFile();

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
		message.EM_MessageText = GetAcceptanceTestFile();

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

	protected override ZString GetExpectedProcessorFriendlyName() => "Inbox Notification Export Non-Conformity Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportNonConformityCommunication };

	protected override InboxNotificationNonConformityAESResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationNonConformityAESResponseMessageProcessor(logger);

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

	readonly ZDateTime cancelPreparationDate = new ZDateTime(2022, 10, 27, 10, 12, 18);
}
