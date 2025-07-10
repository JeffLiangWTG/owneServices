using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5ULDataProvidersTest : XMLMessageTestHelper<GOVCBR5ULDataProvidersTest>
	{
		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";

		public void TestCreateEntryLinesOnRefundType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;

			var entryNumberTypeIs929 = entry.EntryNumbers.AddNew();
			entryNumberTypeIs929.CE_EntryNum = "416372000190U";
			entryNumberTypeIs929.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumberTypeIs929.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var entryNumberTypeIs5UL = entry.EntryNumbers.AddNew();
			entryNumberTypeIs5UL.CE_EntryNum = "12345030211800000025";
			entryNumberTypeIs5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 001;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 002;

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 003;

			var entryLine4 = entry.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 004;

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumberTypeIs5UL);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);

			var import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals(1, import5UL.EntryLines.Length);

			var entry5UL = declaration.CustomsEntryHeaders.AddNew();
			entry5UL.CH_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			entry5UL.CH_CH_PrimeEntry = entry.PK;

			var entryNumberTypeIs5UL1 = entry5UL.EntryNumbers.AddNew();
			entryNumberTypeIs5UL1.CE_EntryNum = "12345030211800000025";
			entryNumberTypeIs5UL1.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var entryLine5UL1 = entry5UL.MergedLines.AddNew();
			entryLine5UL1.CL_LineNumber = 001;

			var entryLine5UL2 = entry5UL.MergedLines.AddNew();
			entryLine5UL2.CL_LineNumber = 002;

			var entryLine5UL3 = entry5UL.MergedLines.AddNew();
			entryLine5UL3.CL_LineNumber = 003;

			import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals(1, import5UL.EntryLines.Length);
		}

		public void TestCreateInvoiceLineMappingRule()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "11234", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "HS_Desc");

			#region Entry Header
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			#endregion

			#region 5UL Entry
			var entry5UL = declaration.CustomsEntryHeaders.AddNew();
			entry5UL.CH_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			entry5UL.CH_CH_PrimeEntry = entry.PK;

			var entryNumberTypeIs5UL = entry.EntryNumbers.AddNew();
			entryNumberTypeIs5UL.CE_EntryNum = "12345030211800000025";
			entryNumberTypeIs5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var entryLine5UL1 = entry5UL.MergedLines.AddNew();
			entryLine5UL1.CL_LineNumber = 001;
			entryLine5UL1.CL_AdValoremTariff = "11234";

			var entryLine5UL2 = entry5UL.MergedLines.AddNew();
			entryLine5UL2.CL_LineNumber = 002;
			entryLine5UL2.CL_AdValoremTariff = "11234";
			#endregion

			#region Invoice
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			#endregion

			#region Link
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine5UL1);
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine5UL1);
			invoiceLine3.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine5UL2);
			#endregion

			//entry.MergedLines[0].AdditionalInvoiceLineLinks example

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "ABCDE", entryNumberTypeIs5UL);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var import5UL = new Import5ULCreator().Create(entry, refundDetails);
			var import5ULLine = import5UL.EntryLines;

			AssertEquals("ABCDE", import5ULLine[0].SoABillNumber);
		}

		public void Test5ULHeaderData()
		{
			#region Payer
			var payer = Factory.NewWithValidTestData<OrgHeader>();
			payer.OH_Category = OrgConstants.Category.Business;
			payer.OH_IsBroker = false;
			payer.OH_FullName = "이케아코리아유한회사";
			payer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "BusinessRegNo", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "KoreanRegNoForResident", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "UnipassIDForOrganization", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.UnipassIDForIndividual, "UnipassIDForIndividual", Core.Constants.CountryCodes.KoreaSouth);
			var payerAddress = payer.MainAddress;
			payerAddress.OA_Address1 = "경기도 광명시 일직로 17(일직동)";
			payerAddress.OA_Address2 = ZString.Empty;
			payerAddress.OA_PostCode = ZString.Empty;

			var payerWrapper = OrgHeaderWrapper.New(payer);
			payerWrapper.ZO_BankCode = "020";
			payerWrapper.ZO_BankAccNo = "1005002390381";

			var payerContact = payer.Contacts.AddNew();
			payerContact.OC_ContactName = "프레드릭클래";
			var payerAllocation = payerContact.Allocations.AddNew();
			payerAllocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;

			#endregion

			#region Header
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "030";
			declaration.JE_CustomsDivision = "75";
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.JE_TaxOffice = "613";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;

			var entryNumberTypeIs5UL = entry.EntryNumbers.AddNew();
			entryNumberTypeIs5UL.CE_EntryNum = "416372000190U";
			entryNumberTypeIs5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 001;
			#endregion

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumberTypeIs5UL);
			sendingObject.RefundCause = "04";
			sendingObject.RefundReason = "02";
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals("416372000190U", import5UL.RefundDeclarationNumber);
			AssertEquals("A", import5UL.RefundType);
			AssertEquals("04", import5UL.RefundCauseCode);
			AssertEquals("02", import5UL.RefundReasonCode);
			AssertEquals("030", import5UL.DeclarationCustomsOffice);
			AssertEquals("75", import5UL.DeclarationCustomsDivision);
			AssertEquals("1005002390381", import5UL.BankAccountNumber);
			AssertEquals("020", import5UL.BankCode);
			AssertEquals("613", import5UL.TaxOfficeCode);
			AssertEquals(0m, import5UL.TotalRefundAmount);

			AssertNotNull(import5UL.Payer);
			var payerData = import5UL.Payer;
			AssertEquals("이케아코리아유한회사", payerData.CompanyName);
			AssertEquals("프레드릭클래", payerData.RepresentativeName);
			AssertEquals("경기도 광명시 일직로 17(일직동)", payerData.AddressLine1);
			AssertEquals(ZString.Empty, payerData.AddressLine2);
			AssertEquals(ZString.Empty, payerData.Postcode);
			AssertEquals(ZString.Empty, payerData.BuildingNumber);
			AssertEquals(ZString.Empty, payerData.RoadNameCode);
			AssertEquals(false, payerData.IsIndividual);

			AssertEquals("BusinessRegNo", payerData.BusinessRegNo);
			AssertEquals("KoreanRegNoForResident", payerData.KoreanRegNoForResident);
			AssertEquals("UnipassIDForOrganization", payerData.UnipassIDForOrganization);

			payer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			declaration.JE_OH_DutyPayer = payer.PK;
			import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertNotNull(import5UL.Payer);
			payerData = import5UL.Payer;
			AssertEquals("KoreanRegNoForResident", payerData.KoreanRegNoForResident);
			AssertEquals("UnipassIDForIndividual", payerData.UnipassIDForIndividual);
		}

		public void Test5ULLineData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;

			var entryNumberTypeIsIMP = entry.EntryNumbers.AddNew();
			entryNumberTypeIsIMP.CE_EntryNum = "4207216092149M";
			entryNumberTypeIsIMP.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			entryNumberTypeIsIMP.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var entry5UL = declaration.CustomsEntryHeaders.AddNew();
			entry5UL.CH_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			entry5UL.CH_CH_PrimeEntry = entry.PK;

			var entryNumberTypeIs5UL = entry5UL.EntryNumbers.AddNew();
			entryNumberTypeIs5UL.CE_EntryNum = "416372000190U";
			entryNumberTypeIs5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumberTypeIs5UL.CE_EntryLineReference = "2";

			var entryLine = entry5UL.MergedLines.AddNew();
			entryLine.CL_LineNumber = 002;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumberTypeIs5UL);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var import5UL = new Import5ULCreator().Create(entry, refundDetails);
			var import5ULLine = import5UL.EntryLines;
			AssertNotNull(import5ULLine);
			AssertEquals(1, import5ULLine.Length);
			AssertEquals(1, import5ULLine[0].RefundLineNo);
			AssertEquals("4207216092149M", import5ULLine[0].ImportDeclarationNumber);
			AssertEquals(0, import5ULLine[0].VersionNumber5WN);

			var entryLine2 = entry5UL.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 001;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);

			import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals(1, import5UL.EntryLines.Length);
		}

		public void Test5ULTaxItemDataA()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;

			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();

			var entryNumberTypeIsIMP = entry.EntryNumbers.AddNew();
			entryNumberTypeIsIMP.CE_EntryNum = "4207216092149M";
			entryNumberTypeIsIMP.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			entryNumberTypeIsIMP.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			#region Import Entry Charge
			entryLine1.CL_ValueForVAT = 165283400;      // 5CQ <->
			entryLine1.CL_ValueExemptForVAT = 10;     // 5CR <->
			entryLine2.CL_ValueForVAT = 100;        // 5CQ <->
			entryLine2.CL_ValueExemptForVAT = 20;     // 5CR <->
			#endregion

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_Tariff = "11234";
			invoiceLine1.JI_Model = "Model";
			invoiceLine1.JI_InvoiceQuantity = 5;
			invoiceLine1.UnitPrice = 10;
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			sendingObject.DutyPenaltyToRefund = 100m;
			sendingObject.EDTPenaltyToRefund = 200m;
			sendingObject.AGTPenaltyToRefund = 300m;
			sendingObject.VATPenaltyToRefund = 400m;
			sendingObject.LQTPenaltyToRefund = 500m;
			sendingObject.SCTPenaltyToRefund = 600m;
			sendingObject.TRTPenaltyToRefund = 700m;

			sendingObject.DutyRefundAmount = 8297490;             // CUD <-> DTY
			sendingObject.EDTRefundAmount = 100;                // 5AB <-> EDT
			sendingObject.AGTRefundAmount = 200;                // CAP <-> AGT
			sendingObject.VATRefundAmount = 16528;                // VAT <-> VAT
			sendingObject.LQTRefundAmount = 300;                // ACT <-> LQT
			sendingObject.SCTRefundAmount = 400;                // IND <-> SCT
			sendingObject.TRTRefundAmount = 500;                // ENV <-> TRT
			sendingObject.PenaltyForLateDeclarationRefundAmount = 100;      // 5AC <-> PML
			sendingObject.PenaltyForMissedDeclarationRefundAmount = 200;       // 5AY <-> PMM
			sendingObject.LatePaymentRefundAmount = 800m;                       // 5CT <->
			sendingObject.NonDutyTaxRefundAmount = 900m;                        // 5CS <->

			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertNotNull(import5UL.EntryLines);
			AssertEquals(1, import5UL.EntryLines.Length);

			var import5ULLine = import5UL.EntryLines[0];
			AssertNotNull(import5ULLine.TaxItems);

			var taxItems = import5ULLine.TaxItems.ToList();
			AssertEquals(7, taxItems.Count);
			AssertEquals(EntryTaxTypeList.Codes.CUD, taxItems[0].TaxItem);
			AssertEquals(8297490m, taxItems[0].Tax);
			AssertEquals(100m, taxItems[0].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes._5AB, taxItems[1].TaxItem);
			AssertEquals(100m, taxItems[1].Tax);
			AssertEquals(200m, taxItems[1].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes.CAP, taxItems[2].TaxItem);
			AssertEquals(200m, taxItems[2].Tax);
			AssertEquals(300m, taxItems[2].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes.VAT, taxItems[3].TaxItem);
			AssertEquals(16528m, taxItems[3].Tax);
			AssertEquals(400m, taxItems[3].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes.ACT, taxItems[4].TaxItem);
			AssertEquals(300m, taxItems[4].Tax);
			AssertEquals(500m, taxItems[4].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes.IND, taxItems[5].TaxItem);
			AssertEquals(400m, taxItems[5].Tax);
			AssertEquals(600m, taxItems[5].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes.ENV, taxItems[6].TaxItem);
			AssertEquals(500m, taxItems[6].Tax);
			AssertEquals(700m, taxItems[6].PenaltyAmount);

			AssertNotNull(import5ULLine.OtherTaxItems);

			var otherTaxItems = import5ULLine.OtherTaxItems.ToList();
			AssertEquals(6, otherTaxItems.Count);
			AssertEquals(EntryTaxTypeList.Codes._5CQ, otherTaxItems[0].TaxItem);
			AssertEquals(165283500m, otherTaxItems[0].Tax);
			AssertEquals(0m, otherTaxItems[0].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes._5CR, otherTaxItems[1].TaxItem);
			AssertEquals(30m, otherTaxItems[1].Tax);
			AssertEquals(0m, otherTaxItems[1].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes._5AC, otherTaxItems[2].TaxItem);
			AssertEquals(100m, otherTaxItems[2].Tax);
			AssertEquals(0m, otherTaxItems[2].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes._5AY, otherTaxItems[3].TaxItem);
			AssertEquals(200m, otherTaxItems[3].Tax);
			AssertEquals(0m, otherTaxItems[3].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes._5CT, otherTaxItems[4].TaxItem);
			AssertEquals(800m, otherTaxItems[4].Tax);
			AssertEquals(0m, otherTaxItems[4].PenaltyAmount);

			AssertEquals(EntryTaxTypeList.Codes._5CS, otherTaxItems[5].TaxItem);
			AssertEquals(900m, otherTaxItems[5].Tax);
			AssertEquals(0m, otherTaxItems[5].PenaltyAmount);
		}

		public void Test5ULDeclarationCustoms()
		{
			#region Tariff
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var office1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(office1.PK, Constants.ZZ.CodeListAttributeNames.RefundDepartment, "75");
			var office2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "011", "성남세관 의정부세관비즈니스센터", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(office2.PK, Constants.ZZ.CodeListAttributeNames.RefundDepartment, "10");
			var office3 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "013", "인천공항국제우편세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
			#endregion

			#region Import5ULHeader
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "030";
			declaration.JE_CustomsDivision = "75";

			var entry = declaration.CustomsEntryHeaders.AddNew();

			var entryNumberTypeIs5UL = entry.EntryNumbers.AddNew();
			entryNumberTypeIs5UL.CE_EntryNum = "416372000190U";
			entryNumberTypeIs5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumberTypeIs5UL.CE_EntryLineReference = "2";

			var statementHeader = entry.Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementNumber = "12345030211800000025";
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryNum = "4207216092149M";
			#endregion

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumberTypeIs5UL);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals("030", import5UL.DeclarationCustomsOffice);
			AssertEquals("75", import5UL.DeclarationCustomsDivision);

			declaration.JE_CustomsOffice = "011";
			declaration.JE_CustomsDivision = "75";
			import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals("011", import5UL.DeclarationCustomsOffice);
			AssertEquals("10", import5UL.DeclarationCustomsDivision);

			declaration.JE_CustomsOffice = "013";
			declaration.JE_CustomsDivision = "50";
			import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals("013", import5UL.DeclarationCustomsOffice);
			AssertEquals("50", import5UL.DeclarationCustomsDivision);
		}

		public void Test5ULRefundNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals(EDIMessage.RefundEntryNumberPlaceHolder, import5UL.RefundDeclarationNumber);

			var entryNumberTypeIs5UL = entry.EntryNumbers.AddNew();
			entryNumberTypeIs5UL.CE_EntryNum = "123452200001U";
			entryNumberTypeIs5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumberTypeIs5UL.CE_EntryLineReference = "2";

			sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumberTypeIs5UL);
			refundDetails = new GOVCBR5ULDetails(sendingObject);
			import5UL = new Import5ULCreator().Create(entry, refundDetails);
			AssertEquals("123452200001U", import5UL.RefundDeclarationNumber);
		}

		public void TestStandAlone5ULData()
		{
			#region Payer
			var payer = Factory.NewWithValidTestData<OrgHeader>();
			payer.OH_Category = OrgConstants.Category.Business;
			payer.OH_IsBroker = false;
			payer.OH_FullName = "이케아코리아유한회사";
			payer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "BusinessRegNo", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "KoreanRegNoForResident", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "UnipassIDForOrganization", Core.Constants.CountryCodes.KoreaSouth);

			var payerAddress = payer.MainAddress;
			payerAddress.OA_OH = payer.PK;
			payerAddress.OA_Address1 = "경기도 광명시 일직로 17(일직동)";
			payerAddress.OA_Address2 = ZString.Empty;
			payerAddress.OA_PostCode = ZString.Empty;

			var payerWrapper = OrgHeaderWrapper.New(payer);
			payerWrapper.ZO_BankCode = "020";
			payerWrapper.ZO_BankAccNo = "1005002390381";

			var payerContact = payer.Contacts.AddNew();
			payerContact.OC_ContactName = "프레드릭클래";
			var payerAllocation = payerContact.Allocations.AddNew();
			payerAllocation.PC_Type = EDIMessage.ApplicationCodes.KRCustoms;
			#endregion

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconDeclaration.CRD_CustomsOffice = "010";
			reconDeclaration.CRD_DeclarantType = RefundTypeList.Codes.A;
			reconDeclaration.CRD_RefundCauseCode = RefundCauseCodeList.Codes._01;
			reconDeclaration.CRD_RefundReasonCode = RefundReasonCodeList.Codes._02;
			reconDeclaration.CRD_CustomsDivision = "20";
			reconDeclaration.CRD_TaxOffice = "613";
			reconDeclaration.CRD_OA_DeclarantAddress = payerAddress.PK;

			var importStandAlone5UL = new Import5ULCreator().Create(reconDeclaration);
			AssertNotNull(importStandAlone5UL);
			AssertNotNull(importStandAlone5UL.EntryLines);
			var builder = new GOVCBR5ULMessageBuilder(importStandAlone5UL);
			AssertNoExceptionThrown(() => builder.GenerateMessage());

			var reconEntryLine1 = reconDeclaration.CusReconEntryLines.AddNew();
			reconEntryLine1.CRL_LineNumber = 1;
			reconEntryLine1.CRL_OriginalEntryLineNumber = 1;
			var reconEntryLine2 = reconDeclaration.CusReconEntryLines.AddNew();
			reconEntryLine2.CRL_LineNumber = 2;
			reconEntryLine2.CRL_OriginalEntryLineNumber = 2;
			importStandAlone5UL = new Import5ULCreator().Create(reconDeclaration);
			AssertNotNull(importStandAlone5UL);
			AssertNotNull(importStandAlone5UL.EntryLines[0].OtherTaxItems);
			AssertNotNull(importStandAlone5UL.EntryLines[1].OtherTaxItems);
			builder = new GOVCBR5ULMessageBuilder(importStandAlone5UL);
			AssertNoExceptionThrown(() => builder.GenerateMessage());

			var chargeTypes = new[] { "DTY", "EDT", "AGT", "VAT", "LQT", "SCT", "TRT", "PML", "PMM", "PMP", "PMN" };
			var penaltyTypes = new[] { "5AD", "5AE", "5AF", "5AG", "5AH", "5AI", "5AJ" };
			CreateCharges(reconEntryLine1, chargeTypes, 10m);
			CreateCharges(reconEntryLine2, chargeTypes, 20m);
			CreateCharges(reconEntryLine2, penaltyTypes, 10m);

			var reconEntry1 = reconEntryLine1.Header;
			reconEntry1.CRE_OriginalEntryNumber = "4207216092149M";
			reconEntry1.CRE_Amendment5WNVersionNumber = 0;
			reconEntry1.CRE_CustomsBillNumber = "030112100576919";
			var reconEntry2 = reconEntryLine2.Header;
			reconEntry2.CRE_OriginalEntryNumber = "4207216092150M";
			reconEntry2.CRE_Amendment5WNVersionNumber = 1;
			reconEntry2.CRE_CustomsBillNumber = "030112100576920";

			var entryNum = reconDeclaration.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum.CE_EntryNum = "6N00221000001X";
			entryNum.CE_IssueDate = ZDateTime.Today;

			importStandAlone5UL = new Import5ULCreator().Create(reconDeclaration);
			AssertNotNull(importStandAlone5UL);
			AssertEquals("6N00221000001X", importStandAlone5UL.RefundDeclarationNumber);
			AssertEquals("A", importStandAlone5UL.RefundType);
			AssertEquals("01", importStandAlone5UL.RefundCauseCode);
			AssertEquals("02", importStandAlone5UL.RefundReasonCode);
			AssertEquals(false, importStandAlone5UL.Are5FE_5ULToBeSentTogether);
			AssertEquals("010", importStandAlone5UL.DeclarationCustomsOffice);
			AssertEquals("20", importStandAlone5UL.DeclarationCustomsDivision);
			AssertEquals("1005002390381", importStandAlone5UL.BankAccountNumber);
			AssertEquals("020", importStandAlone5UL.BankCode);
			AssertEquals(1710m, importStandAlone5UL.TotalRefundAmount);
			AssertEquals("613", importStandAlone5UL.TaxOfficeCode);

			AssertNotNull(importStandAlone5UL.Payer);
			var payerData = importStandAlone5UL.Payer;
			AssertEquals("이케아코리아유한회사", payerData.CompanyName);
			AssertEquals("프레드릭클래", payerData.RepresentativeName);
			AssertEquals("경기도 광명시 일직로 17(일직동)", payerData.AddressLine1);
			AssertEquals(ZString.Empty, payerData.AddressLine2);
			AssertEquals(ZString.Empty, payerData.Postcode);
			AssertEquals(ZString.Empty, payerData.BuildingNumber);
			AssertEquals(ZString.Empty, payerData.RoadNameCode);
			AssertEquals(false, payerData.IsIndividual);

			AssertEquals("BusinessRegNo", payerData.BusinessRegNo);
			AssertEquals("KoreanRegNoForResident", payerData.KoreanRegNoForResident);
			AssertEquals("UnipassIDForOrganization", payerData.UnipassIDForOrganization);

			var importStandAlone5ULLines = importStandAlone5UL.EntryLines;
			AssertNotNull(importStandAlone5ULLines);
			AssertEquals(2, importStandAlone5ULLines.Length);
			AssertEquals("4207216092149M", importStandAlone5ULLines[0].ImportDeclarationNumber);
			AssertEquals("4207216092150M", importStandAlone5ULLines[1].ImportDeclarationNumber);
			AssertEquals(1, importStandAlone5ULLines[0].RefundLineNo);
			AssertEquals(2, importStandAlone5ULLines[1].RefundLineNo);
			AssertEquals(1, importStandAlone5ULLines[0].ImportEntryLineNo);
			AssertEquals(2, importStandAlone5ULLines[1].ImportEntryLineNo);
			AssertEquals(0, importStandAlone5ULLines[0].VersionNumber5WN);
			AssertEquals(1, importStandAlone5ULLines[1].VersionNumber5WN);
			AssertEquals("030112100576919", importStandAlone5ULLines[0].SoABillNumber);
			AssertEquals("030112100576920", importStandAlone5ULLines[1].SoABillNumber);
			AssertEquals(660m, importStandAlone5ULLines[0].TotalOtherTaxItemAmount);
			AssertEquals(1050m, importStandAlone5ULLines[1].TotalOtherTaxItemAmount);

			AssertNotNull(importStandAlone5ULLines[0].TaxItems);
			AssertNotNull(importStandAlone5ULLines[1].TaxItems);
			AssertNotNull(importStandAlone5ULLines[0].OtherTaxItems);
			AssertNotNull(importStandAlone5ULLines[1].OtherTaxItems);

			var taxItems1 = importStandAlone5ULLines[0].TaxItems.ToList();
			AssertEquals(7, taxItems1.Count);
			AssertTaxItem(taxItems1, EntryTaxTypeList.Codes.CUD, 10m, 0m);
			AssertTaxItem(taxItems1, EntryTaxTypeList.Codes._5AB, 20m, 0m);
			AssertTaxItem(taxItems1, EntryTaxTypeList.Codes.CAP, 30m, 0m);
			AssertTaxItem(taxItems1, EntryTaxTypeList.Codes.VAT, 40m, 0m);
			AssertTaxItem(taxItems1, EntryTaxTypeList.Codes.ACT, 50m, 0m);
			AssertTaxItem(taxItems1, EntryTaxTypeList.Codes.IND, 60m, 0m);
			AssertTaxItem(taxItems1, EntryTaxTypeList.Codes.ENV, 70m, 0m);

			var otherTaxItems1 = importStandAlone5ULLines[0].OtherTaxItems.ToList();
			AssertEquals(2, otherTaxItems1.Count);
			AssertTaxItem(otherTaxItems1, EntryTaxTypeList.Codes._5AC, 80m, 0m);
			AssertTaxItem(otherTaxItems1, EntryTaxTypeList.Codes._5AY, 90m, 0m);

			var taxItems2 = importStandAlone5ULLines[1].TaxItems.ToList();
			AssertEquals(7, taxItems2.Count);
			AssertTaxItem(taxItems2, EntryTaxTypeList.Codes.CUD, 20m, 10m);
			AssertTaxItem(taxItems2, EntryTaxTypeList.Codes._5AB, 30m, 60m);
			AssertTaxItem(taxItems2, EntryTaxTypeList.Codes.CAP, 40m, 70m);
			AssertTaxItem(taxItems2, EntryTaxTypeList.Codes.VAT, 50m, 50m);
			AssertTaxItem(taxItems2, EntryTaxTypeList.Codes.ACT, 60m, 30m);
			AssertTaxItem(taxItems2, EntryTaxTypeList.Codes.IND, 70m, 20m);
			AssertTaxItem(taxItems2, EntryTaxTypeList.Codes.ENV, 80m, 40m);

			var otherTaxItems2 = importStandAlone5ULLines[1].OtherTaxItems.ToList();
			AssertEquals(2, otherTaxItems2.Count);
			AssertTaxItem(otherTaxItems2, EntryTaxTypeList.Codes._5AC, 90m, 0m);
			AssertTaxItem(otherTaxItems2, EntryTaxTypeList.Codes._5AY, 100m, 0m);

			void CreateCharges(CusReconEntryLine reconEntryLine, string[] chargeTypes, decimal amount)
			{
				var distinctChargeTypes = chargeTypes.Distinct().ToArray();
				foreach (var chargeType in distinctChargeTypes)
				{
					var reconCustomsCharge = reconEntryLine.CusReconCharges.AddNew();
					reconCustomsCharge.CRC_Amount = amount;
					reconCustomsCharge.CRC_ChargeType = chargeType;
					amount += 10m;
				}
			}

			void AssertTaxItem(List<Import5ULTaxItem> items, string taxType, decimal expectedTax, decimal expectedPenaltyAmount)
			{
				var item = items.Cast<Import5ULTaxItem>().FirstOrDefault(x => x.TaxItem == taxType);
				AssertNotNull(item);
				AssertEquals(expectedTax, item.Tax);
				AssertEquals(expectedPenaltyAmount, item.PenaltyAmount);
			}
		}

		public void Test5ULLineDataForRefundLine()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.B;
			SetCusReconEntryLineData(reconDeclaration, 1, "A", "123452500001U", 1, new ZDateTime("2025-01-01"), "폐기멸실변질손상 환급장치장소1", "기타, A타입, 100", "기타 참고사항1");
			SetCusReconEntryLineData(reconDeclaration, 2, "B", "00000000000000000001", 2, new ZDateTime("2025-01-02"), "폐기멸실변질손상 환급장치장소2", "기타, B타입, 200", "기타 참고사항2");
			SetCusReconEntryLineData(reconDeclaration, 3, "C", "00000000000000000002", 3, new ZDateTime("2025-01-03"), "폐기멸실변질손상 환급장치장소3", "기타, C타입, 300", "기타 참고사항3");
			SetCusReconEntryLineData(reconDeclaration, 4, "D", "00000000000000000003", 4, new ZDateTime("2025-01-04"), "폐기멸실변질손상 환급장치장소4", "기타, D타입, 400", "기타 참고사항4");

			var importStandAlone5UL = new Import5ULCreator().Create(reconDeclaration);
			AssertEquals(4, importStandAlone5UL.EntryLines.Length);

			Assert5ULLineData(importStandAlone5UL.EntryLines[0], "A", "123452500001U", 1, null, ZDateTime.Empty, null, null, null);
			Assert5ULLineData(importStandAlone5UL.EntryLines[1], "B", null, 0, "00000000000000000001", new ZDateTime("2025-01-02"), "폐기멸실변질손상 환급장치장소2", "기타, B타입, 200", "기타 참고사항2");
			Assert5ULLineData(importStandAlone5UL.EntryLines[2], "C", null, 0, "00000000000000000002", new ZDateTime("2025-01-03"), "폐기멸실변질손상 환급장치장소3", "기타, C타입, 300", "기타 참고사항3");
			Assert5ULLineData(importStandAlone5UL.EntryLines[3], "D", null, 0, "00000000000000000003", new ZDateTime("2025-01-04"), "폐기멸실변질손상 환급장치장소4", "기타, D타입, 400", "기타 참고사항4");

			var builder = new GOVCBR5ULMessageBuilder(importStandAlone5UL);
			AssertNoExceptionThrown(() => builder.GenerateMessage());

			void Assert5ULLineData(Import5ULEntryLine import5ULEntryLine, ZString cancelReasonCode, ZString? expDecNumber, ZInt expEntryLineNo, ZString? disposalNumber, ZDateTime disposalDate, ZString? goodsLocation, ZString? residualSubstance, ZString? damageSituation)
			{
				AssertEquals(cancelReasonCode, import5ULEntryLine.CancelReasonCode);
				AssertEquals(expDecNumber, import5ULEntryLine.ExportDeclarationNumber);
				AssertEquals(expEntryLineNo, import5ULEntryLine.ExportEntryLineNo);
				AssertEquals(disposalNumber, import5ULEntryLine.DisposalNumber);
				AssertEquals(disposalDate, import5ULEntryLine.DisposalDate);
				AssertEquals(goodsLocation, import5ULEntryLine.GoodsLocationDescription);
				AssertEquals(residualSubstance, import5ULEntryLine.ResidualSubstanceDescription);
				AssertEquals(damageSituation, import5ULEntryLine.DamageSituation);
			}
		}

		public void Test5ULInvoiceLineDataForRefundInvoiceLine()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			reconDeclaration.CRD_DeclarationType = RefundTypeList.Codes.B;
			var reconEntryLine1 = SetCusReconEntryLineData(reconDeclaration, 1, "A", "123452500001U", 1, new ZDateTime("2025-01-01"), "폐기멸실변질손상 환급장치장소1", "기타, A타입, 100", "기타 참고사항1");
			SetCusReconInvoiceLineData(reconEntryLine1, 1, "HS Desc 1-1", "model name 1-1", 5m, 10m, 5000m);
			SetCusReconInvoiceLineData(reconEntryLine1, 2, "HS Desc 1-2", "model name 1-2", 3m, 5m, 7000m);
			var reconEntryLine2 = SetCusReconEntryLineData(reconDeclaration, 2, "B", "00000000000000000001", 2, new ZDateTime("2025-01-02"), "폐기멸실변질손상 환급장치장소2", "기타, B타입, 200", "기타 참고사항2");
			SetCusReconInvoiceLineData(reconEntryLine2, 1, "HS Desc 2-1", "model name 2-1", 10m, 10m, 9000m);

			var importStandAlone5UL = new Import5ULCreator().Create(reconDeclaration);
			AssertEquals(2, importStandAlone5UL.EntryLines.Length);
			AssertEquals(2, importStandAlone5UL.EntryLines[0].InvoiceLines.Length);
			AssertEquals(1, importStandAlone5UL.EntryLines[1].InvoiceLines.Length);

			Assert5ULInvoiceLineData(importStandAlone5UL.EntryLines[0].InvoiceLines[0], 1, "HS Desc 1-1", "model name 1-1", 5m, 10m, 5000m);
			Assert5ULInvoiceLineData(importStandAlone5UL.EntryLines[0].InvoiceLines[1], 2, "HS Desc 1-2", "model name 1-2", 3m, 5m, 7000m);
			Assert5ULInvoiceLineData(importStandAlone5UL.EntryLines[1].InvoiceLines[0], 1, "HS Desc 2-1", "model name 2-1", 10m, 10m, 9000m);

			var builder = new GOVCBR5ULMessageBuilder(importStandAlone5UL);
			AssertNoExceptionThrown(() => builder.GenerateMessage());

			void Assert5ULInvoiceLineData(Import5ULInvoiceLine import5ULInvoiceLine, ZShort invoiceLineNumber, ZString description, ZString goodsDescription, ZDecimal refundQuantity, ZDecimal invoiceQuantity, ZDecimal unitPrice)
			{
				AssertEquals(invoiceLineNumber, import5ULInvoiceLine.InvoiceLineNo);
				AssertEquals(description, import5ULInvoiceLine.HSDescription);
				AssertEquals(goodsDescription, import5ULInvoiceLine.ItemDescription);
				AssertEquals(refundQuantity, import5ULInvoiceLine.RefundQuantity);
				AssertEquals(invoiceQuantity, import5ULInvoiceLine.InvoiceQuantity);
				AssertEquals(unitPrice, import5ULInvoiceLine.UnitPrice);
			}
		}

		CusReconEntryLine SetCusReconEntryLineData(CusReconDeclaration reconDeclaration, ZShort lineNumber, ZString code, ZString referenceNumber, ZInt lineNo, ZDateTime dateOfExpiry, ZString additionalDescription, ZString referenceNumber2, ZString description)
		{
			var reconEntryLine = reconDeclaration.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_LineNumber = lineNumber;
			reconEntryLine.CRL_OriginalEntryLineNumber = lineNumber;

			var contractRevocation = reconEntryLine.ContractRevocation;
			contractRevocation.CSI_Code = code;
			contractRevocation.CSI_ReferenceNumber = referenceNumber;
			contractRevocation.CSI_LineNo = lineNo;
			contractRevocation.CSI_DateOfExpiry = dateOfExpiry;
			contractRevocation.CSI_AdditionalDescription = additionalDescription;
			contractRevocation.CSI_ReferenceNumber2 = referenceNumber2;
			contractRevocation.CSI_Description = description;

			return reconEntryLine;
		}

		RefundInvoiceLine SetCusReconInvoiceLineData(CusReconEntryLine reconEntryLine, ZShort invoiceLineNumber, ZString description, ZString goodsDescription, ZDecimal refundQuantity, ZDecimal invoiceQuantity, ZDecimal unitPrice)
		{
			var reconInvoiceLine = reconEntryLine.RefundInvoiceLines.AddNew();
			reconInvoiceLine.CSI_LineNo = invoiceLineNumber;
			reconInvoiceLine.CSI_AdditionalDescription = description;
			reconInvoiceLine.CSI_Description = goodsDescription;
			reconInvoiceLine.CSI_Quantity = refundQuantity;
			reconInvoiceLine.CSI_Quantity2 = invoiceQuantity;
			reconInvoiceLine.CSI_Value = unitPrice;
			return reconInvoiceLine;
		}

		public void TestGOVCBR5ULDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "6N0022500001M";
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_StatementType = "I";
			statementHeader.B2_StatementNumber = "0127030112200237053";
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_AssociatedEntry = "0127030112200237053";
			statementLine.B3_EntryNum = "6N0022500001M";
			Factory.Save();

			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObject.RefundAmountOfDutyAmount = 10m;
			sendingObject.RefundAmountOfEducationTax = 20m;
			sendingObject.RefundAmountOfAgricultureTax = 30m;
			sendingObject.RefundAmountOfVAT = 40m;
			sendingObject.RefundAmountOfLiquorTax = 50m;
			sendingObject.RefundAmountOfSpecialConsumptionTax = 60m;
			sendingObject.RefundAmountOfTransportationTax = 70m;
			AssertEquals("RefundAmountOfVAT * 10 =>", 400m, sendingObject.RefundAmountOfValueForVAT);
			AssertEquals(ZDecimal.Zero, sendingObject.RefundAmountOfVATExemptionValue);
			sendingObject.RefundAmountOfPenaltyForLateDeclaration = 80m;
			sendingObject.RefundAmountOfPenaltyForMissedDeclaration = 90m;
			sendingObject.RefundAmountOfPenaltyForLatePayment = 100m;
			sendingObject.RefundAmountOfNonDutyTaxRevenue = 110m;

			var detail = new GOVCBR5ULDetails(sendingObject);
			AssertEquals("0127030112200237053", detail.DisbursementBillNumber);
			AssertEquals(entry, detail.Entry);
			AssertEquals(EDIMessage.RefundEntryNumberPlaceHolder, detail.RefundRequestNumber);

			var entryNumber = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals(entryNumber, detail.RefundEntryNumber);

			var penaltiesToRefundList = detail.PenaltiesToRefund.ToList();
			AssertEquals(ZDecimal.Zero, penaltiesToRefundList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.CUD).Value);
			AssertEquals(ZDecimal.Zero, penaltiesToRefundList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes._5AB).Value);
			AssertEquals(ZDecimal.Zero, penaltiesToRefundList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.CAP).Value);
			AssertEquals(ZDecimal.Zero, penaltiesToRefundList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.VAT).Value);
			AssertEquals(ZDecimal.Zero, penaltiesToRefundList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.ACT).Value);
			AssertEquals(ZDecimal.Zero, penaltiesToRefundList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.IND).Value);
			AssertEquals(ZDecimal.Zero, penaltiesToRefundList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.ENV).Value);

			var refundAmountsList = detail.RefundAmounts.ToList();
			AssertEquals(10m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.CUD).Value);
			AssertEquals(20m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes._5AB).Value);
			AssertEquals(30m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.CAP).Value);
			AssertEquals(40m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.VAT).Value);
			AssertEquals(50m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.ACT).Value);
			AssertEquals(60m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.IND).Value);
			AssertEquals(70m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes.ENV).Value);
			AssertEquals(400m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes._5CQ).Value);
			AssertEquals(0m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes._5CR).Value);
			AssertEquals(80m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes._5AC).Value);
			AssertEquals(90m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes._5AY).Value);
			AssertEquals(100m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes._5CT).Value);
			AssertEquals(110m, refundAmountsList.SingleOrDefault(x => x.Key == EntryTaxTypeList.Codes._5CS).Value);
		}

		public void TestGOVCBR5ULDetails_MessageSubType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = entry.CH_VersionID + 1;
			amendmentSessionalData.CSI_ItemNumber = 1;

			CombineAssertions("When use JobDeclarationAmendmentMessageSendingObject", () =>
			{
				var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);

				var detail = new GOVCBR5ULDetails(sendingObject);
				AssertEquals(ZString.Empty, sendingObject.RefundRequestSubmissionYN);
				AssertEquals(Constants.RefundRequestType.StandAlone, detail.MessageSubType);
				AssertEquals(false, detail.Are5FE_5ULToBeSentTogether);

				var refundSessionalData = amendmentSessionalData.RefundSessionalDataCollection.AddNew();
				refundSessionalData.CSI_Code = YesNo.Yes;
				sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
				detail = new GOVCBR5ULDetails(sendingObject);
				AssertEquals(Constants.RefundRequestType.Simultaneous, detail.MessageSubType);
				AssertEquals(true, detail.Are5FE_5ULToBeSentTogether);

				refundSessionalData.CSI_Code = YesNo.No;
				sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
				detail = new GOVCBR5ULDetails(sendingObject);
				AssertEquals(Constants.RefundRequestType.StandAlone, detail.MessageSubType);
				AssertEquals(false, detail.Are5FE_5ULToBeSentTogether);

				amendmentSessionalData.RefundSessionalDataCollection.RemoveAndDelete(refundSessionalData);
			});

			CombineAssertions("When use PenaltyRefundRequestMessageSendingObject", () =>
			{
				var entryNum5UL = entry.EntryNumbers.AddNew();
				entryNum5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
				entryNum5UL.CE_EntryNum = "1234500001";

				var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, ZString.Empty, entryNum5UL);
				var detail = new GOVCBR5ULDetails(sendingObject);
				AssertNull(sendingObject.AmendmentSessionalData);
				AssertEquals(Constants.RefundRequestType.StandAlone, detail.MessageSubType);
				AssertEquals(false, detail.Are5FE_5ULToBeSentTogether);

				sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, ZString.Empty, entryNum5UL);
				sendingObject.Amendment5WNNumber = (ZShort)amendmentSessionalData.CSI_ItemNumber;
				detail = new GOVCBR5ULDetails(sendingObject);
				AssertNotNull(sendingObject.AmendmentSessionalData);
				AssertNull(sendingObject.AmendmentSessionalData.RefundSessionalData);
				AssertEquals(Constants.RefundRequestType.StandAlone, detail.MessageSubType);
				AssertEquals(false, detail.Are5FE_5ULToBeSentTogether);

				var refundSessionalData = instruction.RefundSessionalDataCollection.AddNew();
				refundSessionalData.CSI_CSI_SupportingInfo = amendmentSessionalData.PK;
				refundSessionalData.CSI_Code = ZString.Empty;
				amendmentSessionalData.RefundSessionalDataCollection.Reload(false);

				sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, ZString.Empty, entryNum5UL);
				sendingObject.Amendment5WNNumber = (ZShort)amendmentSessionalData.CSI_ItemNumber;
				detail = new GOVCBR5ULDetails(sendingObject);
				AssertNotNull(sendingObject.AmendmentSessionalData.RefundSessionalData);
				AssertEquals(ZString.Empty, sendingObject.AmendmentSessionalData.RefundSessionalData.CSI_Code);
				AssertEquals(Constants.RefundRequestType.StandAlone, detail.MessageSubType);
				AssertEquals(false, detail.Are5FE_5ULToBeSentTogether);

				refundSessionalData.CSI_Code = YesNo.Yes;
				sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, ZString.Empty, entryNum5UL);
				sendingObject.Amendment5WNNumber = (ZShort)amendmentSessionalData.CSI_ItemNumber;
				detail = new GOVCBR5ULDetails(sendingObject);
				AssertEquals(Constants.RefundRequestType.Simultaneous, detail.MessageSubType);
				AssertEquals(true, detail.Are5FE_5ULToBeSentTogether);
			});
		}

		public void TestGOVCBR5ULDetails_Amendment5WNVersionNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var sendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			var detail = new GOVCBR5ULDetails(sendingObject);
			AssertEquals("Default value: 0", 0u, detail.Amendment5WNVersionNumber);

			CombineAssertions("When use PenaltyRefundRequestMessageSendingObject", () =>
			{
				var entryNum5UL = entry.EntryNumbers.AddNew();
				entryNum5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
				entryNum5UL.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal;
				entryNum5UL.CE_EntryNum = "1234500001";

				var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, ZString.Empty, entryNum5UL);
				sendingObject.Amendment5WNNumber = 2;
				var detail = new GOVCBR5ULDetails(sendingObject);
				AssertEquals("User entered value is 2, but return 2", 2u, detail.Amendment5WNVersionNumber);
			});
		}
	}
}
