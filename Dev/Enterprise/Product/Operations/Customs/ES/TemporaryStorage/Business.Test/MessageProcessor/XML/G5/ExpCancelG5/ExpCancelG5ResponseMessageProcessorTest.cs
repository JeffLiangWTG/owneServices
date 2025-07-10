using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpCancelV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;
using CusTempStorageRegLine = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine;
using CusTempStorageRegLineTransaction = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

sealed class ExpCancelG5ResponseMessageProcessorTest : G5CommonResponseMessageProcessorTest<ExpCancelG5ResponseMessageProcessor, ExpCancelG5MessagePrettyFormatter, G5ExpCancelV1Sal>
{
	public void TestProcessAcceptedMessage()
	{
		AddMessageProcessAndAssertResult_AcceptedMessage();
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValue0()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode, conTransactionTranValue: 0m);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
			"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 14:00:23</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>6FBUNZ2DEFR5Z2QL</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled, expectedMessageInterpretation: expectedMessageInterpretationText);
		AssertEquals("When there are CON transactions with the correct reference but the tranvalue is 0 no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValueNot0()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
			"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 14:00:23</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>6FBUNZ2DEFR5Z2QL</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled, expectedMessageInterpretation: expectedMessageInterpretationText);
		AssertGuaranteeTransactionForExpedition(FormattedDSDTCode, cancelationDate, 20.0m, MRNCode, TransactionCommentSuffix);
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactions_WithoutOpeningBalance()
	{
		SetUpGuaranteeData(shouldAddOBLTransaction: false, conTransactionReference: FormattedDSDTCode);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
			"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 14:00:23</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>6FBUNZ2DEFR5Z2QL</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled, expectedMessageInterpretation: expectedMessageInterpretationText);
		AssertEquals("When there is no OBL transaction no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithoutCONTransactions()
	{
		SetUpGuaranteeData(shouldAddCONTransaction: false);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
			"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 14:00:23</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>6FBUNZ2DEFR5Z2QL</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled, expectedMessageInterpretation: expectedMessageInterpretationText);
		AssertEquals("When there are no CON transactions with the correct reference no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValueNot0_LocationNotInPremises()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode, shouldHaveLocationInPremises: false);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
			"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 14:00:23</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>6FBUNZ2DEFR5Z2QL</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled, expectedMessageInterpretation: expectedMessageInterpretationText);
		AssertEquals("When there the location is not in premises no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingAcceptedMessage_WithCONTransactionsTranValueNot0_DeclarantNotConsignee()
	{
		SetUpGuaranteeData(conTransactionReference: FormattedDSDTCode, shouldHaveSameDeclarantAndConsignee: false);

		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
			"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 14:00:23</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>6FBUNZ2DEFR5Z2QL</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled, expectedMessageInterpretation: expectedMessageInterpretationText);
		AssertEquals("When Declarant and Consignee are not the same no new transaction is created", 0, temporaryStorageHeader.Guarantee.CusGuarantee.GetTransactions().Count(x => x.CPL_TransactionType == Customs.Business.PermitTransactionTypeList.Codes.TRA));
	}

	public void TestGuaranteeTransactionsTranValueWhenProcessingRejectedMessage_WithCONTransactionsTranValueNot0()
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
	
	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndLocationManagedInPremises()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			temporaryStorageHeader.LRN = "JPB";
			temporaryStorageHeader.MRN = "TEST";
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			regLineTransaction1.SRT_PackageQty = 1;
			regLineTransaction1.SRT_GrossWeight = 2.63m;
			regLineTransaction1.SRT_BondAmount = 1.0m;
			SetUpGuarantees();
			var regLineTransaction2 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, temporaryStorageHeader.LRN, ZString.Empty);

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions before process cancelation response", 2, numTransactions);
				AssertEquals("SRH_Status before process cancelation response", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus before process cancelation response", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions before process cancelation response", 1, numGuaranteeTransactions);
			});

			AddMessageProcessAndAssertResult_AcceptedMessage();
			numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response", 3, numTransactions);
				AssertEquals("SRH_Status after process cancelation response", "OPN", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response", "OPN", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 2, numGuaranteeTransactions);
				AssertTransaction((CusTempStorageRegLine)regLine, bondAmound: 1.0m);
				AssertGuarantee((CusGuaranteeHeader)guarantee, tranValue: -1.0m);
			});

			var regLineTransaction3 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			regLineTransaction3.SRT_PackageQty = 1;
			regLineTransaction3.SRT_GrossWeight = 2.63m;
			regLineTransaction3.SRT_BondAmount = -1.0m;

			AddMessageProcessAndAssertResult_AcceptedMessage();
			numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response and create a new transaction", 5, numTransactions);
				AssertEquals("SRH_Status after process cancelation response", "OPN", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response", "OPN", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 3, numGuaranteeTransactions);
				AssertTransaction((CusTempStorageRegLine)regLine, bondAmound: 1.0m);
				AssertGuarantee((CusGuaranteeHeader)guarantee, tranValue: -1.0m);
			});
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndLocationManagedInPremises_WithMoreThanOneTransaction()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			temporaryStorageHeader.LRN = "JPB";
			temporaryStorageHeader.MRN = "TEST";
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			regLineTransaction1.SRT_PackageQty = 1;
			regLineTransaction1.SRT_GrossWeight = 2.63m;
			regLineTransaction1.SRT_BondAmount = -1.0m;
			SetUpGuarantees();
			var regLineTransaction2 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			regLineTransaction2.SRT_PackageQty = 2;
			regLineTransaction2.SRT_GrossWeight = 2.63m;
			regLineTransaction2.SRT_BondAmount = -1.0m;
			var regLine2 = Factory.New<CusTempStorageRegLine>();
			regLine2.SRL_LineNumber = 2;
			regLine2.SRL_SRH = this.regHeader.PK;
			regLine2.SRL_CustomsStatus = "CLS";
			var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			regLineTransaction3.SRT_PackageQty = 2;
			regLineTransaction3.SRT_GrossWeight = 2.63m;
			regLineTransaction3.SRT_BondAmount = -1.0m;

			var regLine1 = this.regLine;
			var regHeader = regLine.RegHeader;

			var numTransactions = regLine1.CusTempStorageRegLineTransactions.Count + regLine2.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions before process cancelation response", 3, numTransactions);
				AssertEquals("SRH_Status before process cancelation response", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus line1 before process cancelation response", "CLS", regLine1.SRL_CustomsStatus);
				AssertEquals("SRL_CustomsStatus line2 before process cancelation response", "CLS", regLine2.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions before process cancelation response", 1, numGuaranteeTransactions);
			});

			AddMessageProcessAndAssertResult_AcceptedMessage();
			numTransactions = regLine1.CusTempStorageRegLineTransactions.Count + regLine2.CusTempStorageRegLineTransactions.Count;
			numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response", 5, numTransactions);
				AssertEquals("SRH_Status after process cancelation response", "OPN", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus line1 after process cancelation response", "OPN", regLine1.SRL_CustomsStatus);
				AssertEquals("SRL_CustomsStatus line2 after process cancelation response", "OPN", regLine2.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 3, numGuaranteeTransactions);
				AssertTransaction(regLine1, bondAmound: 2.0m, packageQty: 3, grossWeight: 5.26m);
				AssertTransaction(regLine2, bondAmound: 1.0m, packageQty: 2);
				AssertGuarantee((CusGuaranteeHeader)guarantee, tranValue: -2.0m);
				AssertGuarantee((CusGuaranteeHeader)guarantee, tranValue: -1.0m);
			});
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndLocationManagedInPremises_WithNegativeBondAmount()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			temporaryStorageHeader.MRN = "TEST";
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			regLineTransaction1.SRT_PackageQty = 1;
			regLineTransaction1.SRT_GrossWeight = 2.63m;
			regLineTransaction1.SRT_BondAmount = -1.0m;
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions before process cancelation response", 1, numTransactions);
				AssertEquals("SRH_Status before process cancelation response", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus before process cancelation response", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions before process cancelation response", 1, numGuaranteeTransactions);
			});

			AddMessageProcessAndAssertResult_AcceptedMessage();
			numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response", 2, numTransactions);
				AssertEquals("SRH_Status after process cancelation response", "OPN", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response", "OPN", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 2, numGuaranteeTransactions);
				AssertTransaction((CusTempStorageRegLine)regLine, bondAmound: 1.0m);
			});
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndLocationManagedInPremises_WithZeroBondAmount()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			temporaryStorageHeader.MRN = "TEST";
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			regLineTransaction1.SRT_PackageQty = 1;
			regLineTransaction1.SRT_GrossWeight = 2.63m;
			regLineTransaction1.SRT_BondAmount = ZDecimal.Zero;
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions before process cancelation response", 1, numTransactions);
				AssertEquals("SRH_Status before process cancelation response", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus before process cancelation response", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions before process cancelation response", 1, numGuaranteeTransactions);
			});

			AddMessageProcessAndAssertResult_AcceptedMessage();
			numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response", 2, numTransactions);
				AssertEquals("SRH_Status after process cancelation response", "OPN", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response", "OPN", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response is equal", 1, numGuaranteeTransactions);
				AssertTransaction((CusTempStorageRegLine)regLine, bondAmound: 0m);
			});
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndLocationManagedInPremises_WithDifferentReference()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			temporaryStorageHeader.MRN = "123";
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			AddMessageProcessAndAssertResult_AcceptedMessage();

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response is equal", 1, numTransactions);
				AssertEquals("SRH_Status after process cancelation response is equal", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is equal", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 1, numGuaranteeTransactions);
			});
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndLocationManagedInPremises_WithDifferentInternalReference()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, "JPB");
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			AddMessageProcessAndAssertResult_AcceptedMessage();

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response is equal", 1, numTransactions);
				AssertEquals("SRH_Status after process cancelation response is equal", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is equal", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 1, numGuaranteeTransactions);
			});
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndLocationManagedInPremises_WithDifferentInternalReferenceType()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN, CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.LameEntry);
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			AddMessageProcessAndAssertResult_AcceptedMessage();

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response is equal", 1, numTransactions);
				AssertEquals("SRH_Status after process cancelation response is equal", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is equal", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 1, numGuaranteeTransactions);
			});
		}
	}

	void AssertTransaction(CusTempStorageRegLine regLine, ZDecimal bondAmound, int packageQty = 1, decimal grossWeight = 2.63m)
	{
		var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_Comments.Contains("G5 JOB: "));

		AssertEquals("SRT_TransactionType", CusTempStorageRegLineTransactionTypeList.Codes.Transaction, transaction.SRT_TransactionType);
		AssertEquals("SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
		AssertEquals("SRT_InternalReferenceType", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements, transaction.SRT_InternalReferenceType);
		AssertEquals("SRT_InternalReferenceNumber", temporaryStorageHeader.LRN, transaction.SRT_InternalReferenceNumber);
		AssertEquals("SRT_ReferenceType", "MRN", transaction.SRT_ReferenceType);
		AssertEquals("SRT_Reference", temporaryStorageHeader.MRN, transaction.SRT_Reference);
		AssertEquals("SRT_Comments", "G5 JOB: " + temporaryStorageHeader.JobNumber + TransactionCommentSuffix, transaction.SRT_Comments);
		AssertEquals("SRT_TransactionDate", cancelationDate.ToDateTimeOffset(null), transaction.SRT_TransactionDate);
		AssertEquals("SRT_PhysicalInOutDate", cancelationDate.ToDateTimeOffset(null), transaction.SRT_PhysicalInOutDate);
		AssertEquals("SRT_PackageQty", packageQty, transaction.SRT_PackageQty);
		AssertEquals("SRT_GrossWeight", grossWeight, transaction.SRT_GrossWeight);
		AssertEquals("SRT_BondAmount", bondAmound, transaction.SRT_BondAmount);
	}

	void AssertGuarantee(CusGuaranteeHeader cusGuarantee, ZDecimal tranValue)
	{
		var guarantee = cusGuarantee.CusGuaranteeLineTransactions.First(x => x.CPL_Comment.Contains("G5X: ") && x.CPL_TranValue == tranValue);

		AssertEquals("CPL_TransactionType", "TRA", guarantee.CPL_TransactionType);
		AssertEquals("CPL_TransactionDate", cancelationDate, guarantee.CPL_TransactionDate);
		AssertEquals("CPL_Reference", "SUM", guarantee.CPL_Reference);
		AssertEquals("CPL_TranValue", tranValue, guarantee.CPL_TranValue);
		AssertEquals("CPL_Comment", "G5X: " + temporaryStorageHeader.LRN + ". MRN: " + temporaryStorageHeader.MRN + TransactionCommentSuffix, guarantee.CPL_Comment);
		AssertEquals("CPL_TransactionStatus", "CON", guarantee.CPL_TransactionStatus);
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSNotEnabledAndLocationManagedInPremises()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			AddMessageProcessAndAssertResult_AcceptedMessage();

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response is equal", 1, numTransactions);
				AssertEquals("SRH_Status after process cancelation response is equal", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is equal", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 1, numGuaranteeTransactions);
			});
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndLocationNotManagedInPremises()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			AddMessageProcessAndAssertResult_AcceptedMessage();

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response is equal", 1, numTransactions);
				AssertEquals("SRH_Status after process cancelation response is equal", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is equal", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 1, numGuaranteeTransactions);
			});
		}
	}

	public void TestProcessCancelationResponse_WithCONTransactionAndTSEnabledAndPremiseTypeLAMNotManagedInPremises()
	{
		SetUpPremises(temporaryStorageHeader, CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, temporaryStorageHeader.LRN);
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			AddMessageProcessAndAssertResult_AcceptedMessage();

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response is equal", 1, numTransactions);
				AssertEquals("SRH_Status after process cancelation response is equal", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is equal", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 1, numGuaranteeTransactions);
			});
		}
	}

	public void TestProcessCancelationResponse_WithPNDTransaction()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, temporaryStorageHeader.LRN);
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			AddMessageProcessAndAssertResult_AcceptedMessage();

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response is equal", 1, numTransactions);
				AssertEquals("SRH_Status after process cancelation response is equal", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is equal", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 1, numGuaranteeTransactions);
			});
		}
	}

	public void TestProcessCancelationResponse_WithDELTransaction()
	{
		SetUpPremises(temporaryStorageHeader);
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var regLineTransaction1 = SetUpTransaction(this.regLine, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, temporaryStorageHeader.LRN);
			SetUpGuarantees();

			var regLine = regLineTransaction1.RegLine;
			var regHeader = regLine.RegHeader;

			AddMessageProcessAndAssertResult_AcceptedMessage();

			var numTransactions = regLine.CusTempStorageRegLineTransactions.Count;
			var guarantee = ((CusTempStorageRegHeader)regHeader).Guarantee.CusGuarantee;
			var numGuaranteeTransactions = guarantee.CusGuaranteeLineTransactions.Count;
			CombineAssertions(() =>
			{
				AssertEquals("Number of transactions after process cancelation response is equal", 1, numTransactions);
				AssertEquals("SRH_Status after process cancelation response is equal", "CLS", regHeader.SRH_Status);
				AssertEquals("SRL_CustomsStatus after process cancelation response is equal", "CLS", regLine.SRL_CustomsStatus);
				AssertEquals("Number of guarantee transactions after process cancelation response", 1, numGuaranteeTransactions);
			});
		}
	}

	CusTempStorageRegLineTransaction SetUpTransaction(CusTempStorageRegLine regLine, ZString status, ZString referenceNum, string referenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.G5Movements)
	{
		var regLineTransaction1 = (CusTempStorageRegLineTransaction)regLine.CusTempStorageRegLineTransactions.AddNew();
		regLineTransaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		regLineTransaction1.SRT_TransactionStatus = status;
		regLineTransaction1.SRT_InternalReferenceNumber = referenceNum;
		regLineTransaction1.SRT_InternalReferenceType = referenceType;
		regLineTransaction1.SRT_PhysicalInOutDate = physicalInOutDate.ToDateTimeOffset(null);
		regLineTransaction1.SRT_Reference = "TEST";

		return regLineTransaction1;
	}

	void SetUpGuarantees()
	{
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
	}

	void SetUpPremises(TemporaryStorageHeader header, string premiseType = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse)
	{
		header.GoodsLocation.Address.AuthorisationNumber = "9999000002";

		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Type = premiseType;
		premises.SRP_CustomsLocation = "9999000002";
		premises.SRP_Code = "X";
		premises.SRP_Description = "DESC";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;
		premises.AuthorizationNumber = "AAA";
	}

	void AddMessageProcessAndAssertResult_AcceptedMessage()
	{
		var message = CreateNewEDIMessage(temporaryStorageHeader.AMA_JobReference, GetAcceptanceTestFile(), InterchangeID);
		ProcessMessageForTest(message, temporaryStorageHeader.Messages);

		var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
			"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>28-02-2024, 14:00:23</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>CSV Electronic Declaration:</td><td>&nbsp;&nbsp;</td><td>6FBUNZ2DEFR5Z2QL</td></tr></table>";
		AssertG5Declaration(message, "ACC", messageNum: MessageNum, expectedCustomsStatus: EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled, expectedMessageInterpretation: expectedMessageInterpretationText);
	}

	protected override void SetUp()
	{
		base.SetUp();

		temporaryStorageHeader.DsdtMrnNumber = DSDTCode;
		temporaryStorageHeader.AcceptanceDate = new ZDateTime(2023, 01, 01, 02, 01, 00);
		temporaryStorageHeader.MRN = MRNCode;

		regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_AppCode = "AAA";
		regHeader.SRH_Reference = "reference";
		regHeader.SRH_Status = "CLS";
		regHeader.SRH_Reference = "SUM";
		regLine = Factory.New<CusTempStorageRegLine>();
		regLine.SRL_LineNumber = 1;
		regLine.SRL_SRH = regHeader.PK;
		regLine.SRL_CustomsStatus = "CLS";

		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "JPB";
		orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "TestAddress";
	}
	OrgHeader orgHeader;
	OrgAddress orgAddress;
	CusTempStorageRegHeader regHeader;
	CusTempStorageRegLine regLine;

	const string MRNCode = "24ES009998987654321";
	const string DSDTCode = "24ES00999912345678";
	const string FormattedDSDTCode = "99994234567";
	const string TransactionCommentSuffix = " (Canceled)";
	readonly ZDateTime cancelationDate = new ZDateTime(2024, 02, 28, 14, 00, 23);
	readonly ZDateTime physicalInOutDate = new ZDateTime(2024, 03, 28, 14, 00, 23);

	string GetAcceptanceTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpCancelG5TestFilePath, "AcceptedMessage.txt");

	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpCancelG5TestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	protected override ZString GetExpectedProcessorFriendlyName() => "G5 Expedition Cancellation Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation };

	protected override ExpCancelG5ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ExpCancelG5ResponseMessageProcessor(logger);

	protected override string GetRejectedTestFile() => ESG5TestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpCancelG5TestFilePath, "RejectedMessage.txt");
}
