using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.SAL;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

sealed class ExpAmendmentG5ResponseMessageProcessorTest : G5CommonResponseMessageProcessorTest<ExpAmendmentG5ResponseMessageProcessor, ExpAmendmentG5MessagePrettyFormatter, G5ExpAmendV1Sal>
{
	public void TestProcessAcceptedMessageWithCSV()
	{
		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
	}

	public void TestProcessAcceptedMessageWithoutCSV()
	{
		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithoutCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: "", expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValue0_GuaranteeAmountSame()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode, conTransactionTranValue: 0m, bondAmount: 0);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertEquals("When there are CON transactions with the correct reference and tranvalue is 0 like bondamount no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValue0_GuaranteeAmountDifferent()
	{
		SetUpGuaranteeData(conTransactionReference: DSDTCode, conTransactionTranValue: 0m);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertGuaranteeTransactionForExpedition(FormattedDSDTCode, acceptanceDate, -30.0m, MRNCode);
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValueNot0_GuaranteeAmountSame()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode, bondAmount: 20m);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertEquals("When there are CON transactions with the correct reference but the tranvalue is the same ad bond amount no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValueNot0_GuaranteeAmountDifferent()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertGuaranteeTransactionForExpedition(FormattedDSDTCode, acceptanceDate, -10.0m, MRNCode, " (Customs Adj)");
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValueNot0_GuaranteeAmountDifferent_WithoutOpeningBalance()
	{
		SetUpGuaranteeData(shouldAddOBLTransaction: false, conTransactionReference: FormattedDSDTCode);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertEquals("When there is no OBL transaction no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithoutCONTransactions_GuaranteeAmount0()
	{
		SetUpGuaranteeData(shouldAddCONTransaction: false, bondAmount: 0m);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertEquals("When there are no CON transactions with the correct reference and bondamount is 0 no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithoutCONTransactions_GuaranteeAmountNot0()
	{
		SetUpGuaranteeData(shouldAddCONTransaction: false);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertGuaranteeTransactionForExpedition(FormattedDSDTCode, acceptanceDate, -30.0m, MRNCode);
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValueNot0_GuaranteeAmountDifferent_LocationNotInPremises()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode, shouldHaveLocationInPremises: false);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertEquals("When there the location is not in premises no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValueNot0_GuaranteeAmountDifferent_DeclarantNotConsignee()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode, shouldHaveSameDeclarantAndConsignee: false);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceWithCSVTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 11:28:15</td></tr></table>" +
			"<table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>24ES009999Y00090Y4</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>DSDT MRN:</td><td>&nbsp;&nbsp;</td><td>24ES00999880000405</td></tr></table>" +
			"<table border=\"0\"><tr><td>DSDT (Summary Declaration format):</td><td>&nbsp;&nbsp;</td><td>99984000040</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>5EA9589535D0AF79</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>YBNVCKPU8ZEFETDB</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedMrnIssueDate: acceptanceDate, expectedCircuit: CircuitCodeList.Codes.GREEN, expectedMrnNumber: MRNCode, expectedDsdtMRN: DSDTCode, expectedCsvClearance: CSVClearance, expectedMessageInterpretation: expectedMessageInterpretationText, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);
		AssertEquals("When Declarant and Consignee are not the same no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingRejectedMessage_WithCONTransactionsTranValueNot0_GuaranteeAmountDifferent()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode);

		var responseMessage = CreateNewEDIMessage(ApplicationReference, GetRejectedTestFile(), InterchangeID);

		ProcessMessageForTest(responseMessage);
		var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Error Type</strong></td><td><strong>Place</strong></td><td><strong>Goods Item Number</strong></td></tr>" +
			"<tr><td>600</td><td>El mensaje es erroneo.</td><td>F</td><td>GoodsItem</td><td>1</td></tr>" +
			"<tr><td>900</td><td>El mensaje enviado no cumple el esquema.</td><td>N</td><td>&nbsp;</td><td>&nbsp;</td></tr></table>";

		AssertG5Declaration(responseMessage, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, messageNum: MessageNum, messageSubType: "REJ");
		AssertEquals("When response is a rejection no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	string GetAcceptanceWithCSVTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpAmendmentG5TestFilePath, "AcceptedMessageWithCSV.txt");

	string GetAcceptanceWithoutCSVTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpAmendmentG5TestFilePath, "AcceptedMessageWithoutCSV.txt");

	protected override ZString GetExpectedProcessorFriendlyName() => "G5 Expedition Amendment Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment };

	protected override ExpAmendmentG5ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ExpAmendmentG5ResponseMessageProcessor(logger);

	protected override string GetRejectedTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpAmendmentG5TestFilePath, "RejectedMessage.txt");

	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpAmendmentG5TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	const string MRNCode = "24ES009999Y00090Y4";
	const string DSDTCode = "24ES00999880000405";
	const string FormattedDSDTCode = "99984000040";
	const string CSVClearance = "5EA9589535D0AF79";
	readonly ZDateTime acceptanceDate = new ZDateTime(2024, 02, 28, 11, 28, 15);
}
