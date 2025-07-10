using System;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpNotifV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;
using CusTempStorageRegLineTransaction = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

sealed class ExpeditionG5ResponseMessageProcessorTest : G5CommonResponseMessageProcessorTest<ExpeditionG5ResponseMessageProcessor, ExpeditionG5MessagePrettyFormatter, G5ExpNotifV1Sal>
{
	public void TestProcessAcceptedMessage_GreenCircuit()
	{
		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceGreenCircuitTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>27-02-2024, 14:41:50</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000373</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000037</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>F0B1C60760FC9CDB</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>GYK3XXKVPCQV7F4D</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertGuaranteeTransactionForExpedition(FormattedDSDTCode, acceptanceDate, -5.0m, MRNCode);
	}

	public void TestProcessAcceptedMessage_OrangeCircuit()
	{
		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceOrangeCircuitTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>27-02-2024, 14:41:50</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000373</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000037</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>F0B1C60760FC9CDB</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>GYK3XXKVPCQV7F4D</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.ORANGE, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl);
		AssertGuaranteeTransactionForExpedition(FormattedDSDTCode, acceptanceDate, -5.0m, MRNCode);
	}

	public void TestProcessAcceptedMessage_RedCircuit()
	{
		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceRedCircuitTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>27-02-2024, 14:41:50</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>GYK3XXKVPCQV7F4D</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.RED, expectedMrnNumber: MRNCode, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl);
		AssertGuaranteeTransactionForExpedition(ZString.Empty, acceptanceDate, -5.0m, MRNCode);
	}

	public void TestProcessRejectedMessage_CustomsStatusEmpty_TemporaryStorageRegisterNotEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption();

			temporaryStorageHeader.CustomsStatus = ZString.Empty;

			AddMessageProcessAndAssertResult_RejectedMessage();

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestProcessRejectedMessage_CustomsStatusEmpty_TemporaryStorageEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption();

			temporaryStorageHeader.CustomsStatus = ZString.Empty;

			AddMessageProcessAndAssertResult_RejectedMessage();

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefType)", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestProcessRejectedMessage_CustomsStatusNotEmpty_TemporaryStorageEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption();

			AddMessageProcessAndAssertResult_RejectedMessage();

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
			SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(temporaryStorageHeader);

			var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceTestFileWithLongSegmentId(), InterchangeID);

			ProcessMessageForTest(message);
		}

		var expectedError = "Reference reference has a positive balance of 2.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
		AssertContains("logger exception", expectedError, GetAllConcatenatedUserLogStrings());
	}

	protected override void SetUp()
	{
		base.SetUp();

		TemporaryStorageTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		temporaryStorageHeader.LRN = "LRNTest";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		var guarantee = Factory.New<CusGuaranteeHeader>();
		guarantee.CPH_Number = "Test1";
		guarantee.CPH_OH_PermitHolder = orgHeader.PK;
		guarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
		guarantee.CPH_SubType = "1";
		guarantee.CPH_StartDate = ZDate.BrettsBirthday;

		var tsGuarantee = temporaryStorageHeader.Guarantee;
		tsGuarantee.PW_Override = true;
		tsGuarantee.PW_BondNumber = "Test1";
		tsGuarantee.PW_BondAmount = 5.0m;
		guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);
	}

	(CusTempStorageRegLineTransaction transaction1, CusTempStorageRegLineTransaction transaction2, CusTempStorageRegLineTransaction transaction3) SetUpTransactionsForRejectedTestForTemporaryStorageGoodsConsumption()
	{
		var reference = temporaryStorageHeader.LRN;

		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";
		var regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		var regLineTransaction1 = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction1.SRT_InternalReferenceNumber = reference;
		regLineTransaction1.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		var regLineTransaction2 = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		regLineTransaction2.SRT_InternalReferenceNumber = reference;
		regLineTransaction2.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		var regLineTransaction3 = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		regLineTransaction3.SRT_InternalReferenceNumber = reference;
		regLineTransaction3.SRT_InternalReferenceType = ZString.Empty;

		return (regLineTransaction1, regLineTransaction2, regLineTransaction3);
	}

	void SetUpDataForConfirmTemporaryStorageGoodsConsumptionLoggerWriteOffTransactionError(TemporaryStorageHeader header)
	{
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AAA";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address";

		SetUpHeaderForLoggerWriteOffTransactionError(header, orgAddress);

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
		commonGuarantee.Parent = regHeader;
		commonGuarantee.PW_BondNumber = "Test1";
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
		regLineTransaction.SRT_InternalReferenceNumber = header.LRN;
		regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements;
		regLineTransaction.SRT_GrossWeight = 6;
	}

	void SetUpHeaderForLoggerWriteOffTransactionError(TemporaryStorageHeader header, OrgAddress orgAddress)
	{
		header.GoodsLocation.Address.AuthorisationNumber = "9999000002";

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = "ADT";
		premises.SRP_CustomsLocation = "9999000002";
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.AuthorizationNumber = "AAA";
	}

	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionG5TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	protected override ZString GetExpectedProcessorFriendlyName() => "G5 Expedition Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.G5v1Expedition };

	protected override ExpeditionG5ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ExpeditionG5ResponseMessageProcessor(logger);

	string GetAcceptanceGreenCircuitTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionG5TestFilePath, "AcceptedMessageGreenCircuit.txt");

	string GetAcceptanceOrangeCircuitTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionG5TestFilePath, "AcceptedMessageOrangeCircuit.txt");

	string GetAcceptanceRedCircuitTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionG5TestFilePath, "AcceptedMessageRedCircuit.txt");

	protected override string GetRejectedTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionG5TestFilePath, "RejectedMessage.txt");

	const string MRNCode = "24ES009999Y00079Y7";
	const string DSDTCode = "24ES00999880000373";
	const string FormattedDSDTCode = "99984000037";
	const string CSVClearance = "F0B1C60760FC9CDB";
	readonly ZDateTime acceptanceDate = new ZDateTime(2024, 02, 27, 14, 41, 50);
}
