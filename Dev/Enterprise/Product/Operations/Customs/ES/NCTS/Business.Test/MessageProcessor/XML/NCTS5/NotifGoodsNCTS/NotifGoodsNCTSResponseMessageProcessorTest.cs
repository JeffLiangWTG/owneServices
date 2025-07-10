using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC170C_v515.CC170CV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

class NotifGoodsNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<NotifGoodsNCTSResponseMessageProcessor, NotifGoodsNCTSMessagePrettyFormatter, Cc170Cv1Sal>
{
	public void TestProcessAcceptedMessage_DRL_GreenCircuit()
	{
		ProcessAndAssertAcceptedGreenCircuitDeclaration();
	}

	public void TestProcessAcceptedMessage_DCC_OrangeCircuit()
	{
		ProcessAndAssertAcceptedOrangeCircuitDeclaration();
	}

	public void TestProcessAcceptedMessage_DGP_RedCircuit()
	{
		ProcessAndAssertAcceptedRedCircuitDeclaration();
	}

	public void TestModifyExistingMRN()
	{
		CreateCusEntryNumber(nctsHeader, "TestMRN", "O", new DateTime(2023, 12, 11, 00, 00, 00));
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceGreenCircuitResponseCodeLFile(), InterchangeID);

		var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, "TestMRN");
		queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

		CombineAssertions(() =>
		{
			AssertEquals("PreReq: MRN", "TestMRN", cusEntryNumberMRN.CE_EntryNum);
			AssertEquals("PreReq: Circuit", "O", cusEntryNumberMRN.CE_EntryStatus);
			AssertEquals("PreReq: Date", new DateTime(2023, 12, 11, 00, 00, 00), cusEntryNumberMRN.CE_IssueDate);

			ProcessMessageForTest(message);

			queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			AssertEquals("MRN is update", mrnEntryNumber, cusEntryNumberMRN.CE_EntryNum);
			AssertEquals("Circuit is update", "4", cusEntryNumberMRN.CE_EntryStatus);
			AssertEquals("Date is update", new DateTime(2020, 11, 20, 00, 00, 00), cusEntryNumberMRN.CE_IssueDate);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
	{
		ProcessAndAssertAcceptedGreenCircuitDeclaration();

		CombineAssertions(() =>
		{
			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 1, docMessages.Length);
			AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (mrnEntryNumber + "_NCTS_AEAT_DAT.pdf", ClearanceReferenceNumber) });
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
	{
		var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnEntryNumber + "_NCTS_AEAT_DAT.pdf", "CAU");
		docManagerInfo.Save();

		ProcessAndAssertAcceptedGreenCircuitDeclaration();

		CombineAssertions(() =>
		{
			var eDocs = docManagerInfo.GetRelatedEDocs();
			AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

			var eDocNames = new List<ZString>() { mrnEntryNumber + "_NCTS_AEAT_DAT.pdf" };
			AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenDepartureAndNoCSVClearance_AllDocs()
	{
		ProcessAndAssertAcceptedRedCircuitDeclaration();

		CombineAssertions(() =>
		{
			var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
			var eDocs = docManagerInfo.GetRelatedEDocs();
			AssertEquals("Number of eDocs is correct", 0, eDocs.Count());

			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestProcessMessageAcceptedGreenCircuit_PendingTransactionsAsConfirmed()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertAcceptedGreenCircuitDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has been changed since it was pending", mrnEntryNumber, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has been changed since it was pending", mrnEntryNumber, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageAcceptedOrangeCircuit_PendingTransactionsAsConfirmed()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertAcceptedOrangeCircuitDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has been changed since it was pending", mrnEntryNumber, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has been changed since it was pending", mrnEntryNumber, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageAcceptedRedCircuit_PendingTransactionsAsConfirmed()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertAcceptedRedCircuitDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has been changed since it was pending", mrnEntryNumber, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has been changed since it was pending", mrnEntryNumber, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageRejectedDeclaration_PendingTransactionsAsDeleted()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		var responseMessage = CreateNewEDIMessage(ApplicationReference, GetRejectedTestFile(), InterchangeID);

		ProcessMessageForTest(responseMessage);
		var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
		"<H4>List of Errors:</H4>" +
		"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
		"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
		"<tr><td>14</td><td>/CC014C/TransitOperation/MRN</td><td>(1403) No existe declaración para el valor del MRN indicado.</td><td>22ES000101500651J4</td></tr></table>";

		AssertNCTSDeclaration(responseMessage, messageSubType: "REJ", messageStatus: MessageStatusWhenRejectedOrError, emStatus: EMStatusWhenRejectedOrError, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, commonCustomsStatus: OriginalEntryStatus, phaseStatus: PhaseStatusWhenErrorOrRejected, arrivalMovementHeader: TypeDeclaration != NctsMovementType.Codes.Departure ? nctsHeader.ArrivalMovementHeader : null);

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has not been changed since it was set to DEL", ApplicationReference, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has not been changed since it was set to DEL", ApplicationReference, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageErrorDeclaration_PendingTransactionsAsDeleted()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		var responseMessage = CreateNewEDIMessage(ApplicationReference, GetErrorTestFile(), InterchangeID);

		ProcessMessageForTest(responseMessage);
		var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
			"<tr><td>9 / 34</td><td>642</td><td>Item15</td><td>1207 - Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}TransitOperation y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}Invalidation</td><td>&nbsp;</td></tr>" +
			"</table>";

		AssertNCTSDeclaration(responseMessage, messageSubType: "REJ", messageStatus: MessageStatusWhenRejectedOrError, emStatus: EMStatusWhenRejectedOrError, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, commonCustomsStatus: OriginalEntryStatus, phaseStatus: PhaseStatusWhenErrorOrRejected, arrivalMovementHeader: TypeDeclaration != NctsMovementType.Codes.Departure ? nctsHeader.ArrivalMovementHeader : null);

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has not been changed since it was set to DEL", ApplicationReference, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has not been changed since it was set to DEL", ApplicationReference, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageRegisterNotEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForDPTAndDPN();

			nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;

			AddMessageProcessAndAssertResult_RejectedMessage(true);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefNum)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForDPTAndDPN();

			nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;

			AddMessageProcessAndAssertResult_RejectedMessage(true);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefNum)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestProcessRejectedMessage_EntryStatusNotEmpty_TemporaryStorageEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForDPTAndDPN();

			AddMessageProcessAndAssertResult_RejectedMessage(false);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefNum)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestLoggerWriteOffTransactionError()
	{
		AssertLoggerWriteOffTransactionError();
	}

	void ProcessAndAssertAcceptedGreenCircuitDeclaration()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceGreenCircuitResponseCodeLFile(), InterchangeID);
		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>23-03-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>";

		var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

		var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryNum, ClearanceReferenceNumber);
		queryCLR.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
		queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberClearance = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

		AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberClearance: cusEntryNumberClearance, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.GREEN, mrnIssueDate: AdmissionDate, clearanceExpiryDate: limitDate, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceIssueDate: ExpiryDate, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
	}

	void ProcessAndAssertAcceptedOrangeCircuitDeclaration()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceOrangeCircuitResponseCodeBTestFile(), InterchangeID);
		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>23-03-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>";

		var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

		AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl, cusEntryNumberMRN: cusEntryNumberMRN, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.ORANGE, mrnIssueDate: AdmissionDate, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
	}

	void ProcessAndAssertAcceptedRedCircuitDeclaration()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceRedCircuitResponseCodeGTestFile(), InterchangeID);
		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>23-03-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PG - Pending Guarantee</td></tr></table>";

		var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

		AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance, cusEntryNumberMRN: cusEntryNumberMRN, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.RED, mrnIssueDate: AdmissionDate, phaseStatus: ESNctsMovementHeaderTransactionStatusList.Codes.Declaration);
	}

	void CreateCusEntryNumber(NctsHeader header, ZString entryNum, ZString entryStatus, ZDateTime issueDate)
	{
		var newEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = entryNum;
		newEntryNumber.CE_EntryStatus = entryStatus;
		newEntryNumber.CE_IssueDate = issueDate;
		newEntryNumber.CE_ExpiryDate = ZDateTime.Empty;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		Factory.Save();
	}

	protected override ZString PhaseStatusWhenErrorOrRejected => ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;

	protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Notification Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5DepartureNotification };
	string GetAcceptanceGreenCircuitResponseCodeLFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotifGoodsNCTSTestFilePath, "AcceptedMessageGreenCircuitL.txt");
	string GetAcceptanceOrangeCircuitResponseCodeBTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotifGoodsNCTSTestFilePath, "AcceptedMessageOrangeCircuitB.txt");
	string GetAcceptanceRedCircuitResponseCodeGTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotifGoodsNCTSTestFilePath, "AcceptedMessageRedCircuitG.txt");
	protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotifGoodsNCTSTestFilePath, "RejectedMessage.txt");
	protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotifGoodsNCTSTestFilePath, "ErrorMessage.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotifGoodsNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	protected override NotifGoodsNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new NotifGoodsNCTSResponseMessageProcessor(logger);
	protected new readonly ZDateTime AdmissionDate = new ZDateTime(2020, 11, 20);
	readonly ZDateTime limitDate = new ZDateTime(2022, 3, 23);
	readonly ZString mrnEntryNumber = "22ES000101500659J7";
}
