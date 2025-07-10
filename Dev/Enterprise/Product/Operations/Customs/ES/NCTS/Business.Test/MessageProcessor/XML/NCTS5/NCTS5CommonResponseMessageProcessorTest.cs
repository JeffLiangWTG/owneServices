using System;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using CusGuaranteeHeader = Enterprise.Customs.ES.Business.CusGuaranteeHeader;
using CusTempStorageRegHeader = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLine;
using CusTempStorageRegLineTransaction = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

public abstract class NCTS5CommonResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider> : ESNCTSResponseMessageProcessorTest<TResponse, TResponseProvider>
	where TResponse : NCTS5CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage>
	where TResponseProvider : class, ICommonServiceSegment, IResponseCode
	where TPrettyMessage : IMessagePrettyFormatter
{
	public void TestProcessMessageWrongXML()
	{
		SetSentInterchange(nctsHeader, InterchangeID);
		var message = CreateNewEDIMessage(ApplicationReference, WrongXMLTestFile, InterchangeID);

		ProcessMessageForTest(message);

		AssertNCTSDeclaration(message, emStatus: EDIMessage.Status.Failed, messageSubType: "AAA", messageStatus: EDIMessage.Status.Failed, mrnEntrynum: MRNCodeWhenRejectedOrError, messageNum: ZString.Empty, commonCustomsStatus: OriginalEntryStatus, phaseStatus: PhaseStatusWhenWrongXML);
		AssertLoggerMessagesWhenProcessMessageWrongXML();
	}

	public void TestMessageProcessingError()
	{
		var message = CreateNewEDIMessage("AAAAAAAA", ZString.Empty, InterchangeID, false);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertLoggerMessagesWhenMessageProcessingError();
		});
	}

	public void TestMessageProcessingErrorNoReference()
	{
		var message = CreateNewEDIMessage(ZString.Empty, ZString.Empty, InterchangeID, false);

		Factory.Save();

		ProcessMessageForTest(message);
		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertContains("Log has error", "No Application Reference found for message", logger.UserLogStrings[0]);
		});
	}

	public void TestProcessRejectedMessage()
	{
		AddMessageProcessAndAssertResult_RejectedMessageBase(RejectedEntryStatus);
	}

	public void TestProcessErrorMessage()
	{
		var responseMessage = CreateNewEDIMessage(ApplicationReference, GetErrorTestFile(), InterchangeID);

		ProcessMessageForTest(responseMessage);
		var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
			"<tr><td>9 / 34</td><td>642</td><td>Item15</td><td>1207 - Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}TransitOperation y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}Invalidation</td><td>&nbsp;</td></tr>" +
			"</table>";

		AssertNCTSDeclaration(responseMessage, messageSubType: "REJ", messageStatus: MessageStatusWhenRejectedOrError, emStatus: EMStatusWhenRejectedOrError, mrnEntrynum: MRNCodeWhenRejectedOrError, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, commonCustomsStatus: OriginalEntryStatus, phaseStatus: PhaseStatusWhenErrorOrRejected, arrivalMovementHeader: TypeDeclaration != NctsMovementType.Codes.Departure ? nctsHeader.ArrivalMovementHeader : null);
	}

	public void TestProcessMessageAcceptedWithSegmentIdTooLong()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFileWithLongSegmentId(), InterchangeID);

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown("No exception expected since SementId is cut to 35 chars", () => ProcessMessageForTest(message));

			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
		});
	}

	protected void AddMessageProcessAndAssertResult_RejectedMessage(bool isEntryStatusEmpty = false)
	{
		var entryStatus = isEntryStatusEmpty ? ZString.Empty : RejectedEntryStatus;

		AddMessageProcessAndAssertResult_RejectedMessageBase(entryStatus);
	}

	void AddMessageProcessAndAssertResult_RejectedMessageBase(ZString entryStatus)
	{
		var responseMessage = CreateNewEDIMessage(ApplicationReference, GetRejectedTestFile(), InterchangeID);

		ProcessMessageForTest(responseMessage);
		var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
		"<H4>List of Errors:</H4>" +
		"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
		"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
		"<tr><td>14</td><td>/CC014C/TransitOperation/MRN</td><td>(1403) No existe declaración para el valor del MRN indicado.</td><td>22ES000101500651J4</td></tr></table>";

		AssertNCTSDeclaration(responseMessage, messageSubType: "REJ", messageStatus: MessageStatusWhenRejectedOrError, emStatus: EMStatusWhenRejectedOrError, mrnEntrynum: MRNCodeWhenRejectedOrError, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, commonCustomsStatus: entryStatus, phaseStatus: PhaseStatusWhenErrorOrRejected, arrivalMovementHeader: TypeDeclaration != NctsMovementType.Codes.Departure ? nctsHeader.ArrivalMovementHeader : null);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		if (TypeDeclaration == NctsMovementType.Codes.Departure)
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			nctsHeader.MovementHeader.BM_Phase = InitialPhaseStatus;
			nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = ApplicationReference;
		}
		else
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			nctsHeader.ArrivalMovementHeader.BM_Phase = InitialPhaseStatus;
			nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		}

		sentInterchange = SetSentInterchange(nctsHeader, InterchangeID);
	}
	protected EDIInterchange sentInterchange;

	protected const string InitialPhaseStatus = "AAA";

	protected virtual ZString MessageStatusWhenRejectedOrError => ZString.Empty;

	protected virtual ZString EMStatusWhenRejectedOrError => EDIMessage.Status.Received;

	protected virtual ZString MRNCodeWhenRejectedOrError => ZString.Empty;

	protected virtual ZString PhaseStatusWhenErrorOrRejected => InitialPhaseStatus;

	protected virtual ZString PhaseStatusWhenWrongXML => InitialPhaseStatus;

	protected virtual ZString RejectedEntryStatus => OriginalEntryStatus;

	protected virtual void AssertLoggerMessagesWhenProcessMessageWrongXML()
	{
		AssertContains("logger", "Unable to read message text from message", GetAllConcatenatedUserLogStrings());
	}

	protected virtual void AssertLoggerMessagesWhenMessageProcessingError()
	{
		AssertContains("Log has error", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
	}

	protected void AssertNCTSDeclaration(TestEdiMessage message, string messageSubType, string expectedMessageInterpretation = "", string commonCustomsStatus = "", string emStatus = EDIMessage.Status.Received, string messageStatus = "", string messageNum = MessageNum, CusEntryNumber cusEntryNumberMRN = null, CusEntryNumber cusEntryNumberClearance = null, CusEntryNumber cusEntryNumberSummary = null, string mrnEntrynum = "", string mrnEntryStatus = "", string clearanceReferenceNumber = "", ZDateTime? clearanceIssueDate = null, ZDateTime? mrnIssueDate = null, ZDateTime? clearanceExpiryDate = null, string summaryEntryType = "", string summaryEntryNum = "", NctsArrivalMovementHeader arrivalMovementHeader = null, string phaseStatus = InitialPhaseStatus, string placeOfUnloadingCode = "")
	{
		AssertEquals("BM_Phase", phaseStatus, nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader.BM_Phase : nctsHeader.ArrivalMovementHeader.BM_Phase);
		GenericCommonAssertProcessEntryDataNCTS(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, messageNum: messageNum, commonCustomsStatus: commonCustomsStatus, emStatus: emStatus, messageStatus: messageStatus, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberClearance: cusEntryNumberClearance, cusEntryNumberSummary: cusEntryNumberSummary, mrnEntrynum: mrnEntrynum, mrnEntryStatus: mrnEntryStatus, clearanceReferenceNumber: clearanceReferenceNumber, clearanceIssueDate: clearanceIssueDate, mrnIssueDate: mrnIssueDate, clearanceExpiryDate: clearanceExpiryDate, movementReferenceNumber: mrnEntrynum, clearanceDate: clearanceIssueDate, arrivalLimit: clearanceExpiryDate, summaryEntryType: summaryEntryType, summaryEntryNum: summaryEntryNum, arrivalMovementHeader: arrivalMovementHeader, placeOfUnloadingCode: placeOfUnloadingCode);
	}

	protected void AssertLoggerWriteOffTransactionError()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(nctsHeader);

			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFileWithLongSegmentId(), InterchangeID);

			ProcessMessageForTest(message);
		}

		var expectedError = "Reference reference has a positive balance of 2.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
		AssertContains("logger exception", expectedError, GetAllConcatenatedUserLogStrings());
	}

	protected void SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(NctsHeader nctsHeader)
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		SetUpHeaderForLoggerWriteOffTransactionError(nctsHeader, orgAddress);

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";

		var cusGuarantee = Factory.New<CusGuaranteeHeader>();
		cusGuarantee.CPH_Number = "Test1";
		cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
		cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		cusGuarantee.CPH_SubType = "1";
		cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var commonGuarantee = Factory.New<EU.Business.Declaration.CommonGuarantee>();
		commonGuarantee.PW_BondNumber = "Test1";
		commonGuarantee.Parent = regHeader;
		commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

		var guarantee = regHeader.Guarantee.CusGuarantee;
		var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
		guaranteeLineTransaction.CPL_Reference = "reference";
		guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		guaranteeLineTransaction.CPL_TranValue = 2.0m;

		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;

		var openingRegLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		openingRegLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		openingRegLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		openingRegLineTransaction.SRT_BondAmount = 1.0m;
		openingRegLineTransaction.SRT_GrossWeight = 6;

		var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction.SRT_InternalReferenceNumber = nctsHeader.MovementHeader.BM_PaperlessInbondNum;
		regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		regLineTransaction.SRT_GrossWeight = 6;
	}

	void SetUpHeaderForLoggerWriteOffTransactionError(NctsHeader nctsHeader, OrgAddress orgAddress)
	{
		nctsHeader.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "9999000002";

		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "9999000002";
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
	}

	protected (CusTempStorageRegLineTransaction transaction1, CusTempStorageRegLineTransaction transaction2, CusTempStorageRegLineTransaction transaction3) SetUpTransactionsForRejectedTestForDPTAndDPN()
	{
		var entryReference = nctsHeader.MovementHeader.BM_PaperlessInbondNum;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction1.SRT_InternalReferenceNumber = entryReference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		var regLineTransaction2 = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = entryReference;
		regLineTransaction2.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.TransitDepartureDeclaration;
		var regLineTransaction3 = regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction3.SRT_InternalReferenceNumber = entryReference;
		regLineTransaction3.SRT_InternalReferenceType = ZString.Empty;

		return (regLineTransaction1, regLineTransaction2, regLineTransaction3);
	}

	protected abstract string GetRejectedTestFile();
	protected abstract string GetErrorTestFile();
	protected abstract string GetAcceptanceTestFileWithLongSegmentId();

	protected const string MessageNum = "20221212085429562003";
}
