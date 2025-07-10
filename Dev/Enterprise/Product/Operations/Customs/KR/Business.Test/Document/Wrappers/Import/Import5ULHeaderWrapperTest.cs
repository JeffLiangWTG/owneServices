using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Import5ULHeaderWrapper))]
	sealed class Import5ULHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var entryHeader = new Import5ULCreator().Create(entry, refundDetails);
			var wrapper = new Import5ULHeaderWrapper(entry.PK, entryHeader, Factory);
			AssertSame(entryHeader, wrapper.Header);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var entryHeader = new Import5ULCreator().Create(entry, refundDetails);
			return new Import5ULHeaderWrapper(entry.PK, entryHeader, Factory);
		}

		public void TestRefundDeclarationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNumberTypeIs5UL = entry.EntryNumbers.AddNew();
			entryNumberTypeIs5UL.CE_EntryNum = "229262000004U";
			entryNumberTypeIs5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumberTypeIs5UL);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var entryHeader = new Import5ULCreator().Create(entry, refundDetails);
			var wrapper = new Import5ULHeaderWrapper(entry.PK, entryHeader, Factory);
			AssertEquals("22926-20-00004U", wrapper.FormattedRefundDeclarationNumber);
		}

		public void TestCustomsOfficeName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "020", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "020";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var entryHeader = new Import5ULCreator().Create(entry, refundDetails);
			var wrapper = new Import5ULHeaderWrapper(entry.PK, entryHeader, Factory);
			AssertEquals("인천세관", wrapper.CustomsOfficeName);
		}

		public void TestPayerFormattedData()
		{
			var payer = Factory.NewWithValidTestData<OrgHeader>();
			payer.OH_Category = OrgConstants.Category.Business;
			payer.OH_IsBroker = false;
			payer.OH_FullName = "Test";
			payer.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1248105504", Core.Constants.CountryCodes.KoreaSouth);
			payer.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "4208092046311", Core.Constants.CountryCodes.KoreaSouth);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = payer.PK;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var entryHeader = new Import5ULCreator().Create(entry, refundDetails);
			var wrapper = new Import5ULHeaderWrapper(entry.PK, entryHeader, Factory);
			AssertEquals("124-81-05504", wrapper.Payer.FormattedBusinessRegNo);
			AssertEquals("420809-2046311", wrapper.Payer.FormattedKoreanRegNoForResident);
		}

		public void TestRefundCauseCodeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_EntryNum = "AAA111";
			var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
			sendingObject.RefundCause = RefundCauseCodeList.Codes._04;
			var refundDetails = new GOVCBR5ULDetails(sendingObject);
			var entryHeader = new Import5ULCreator().Create(entry, refundDetails);
			var wrapper = new Import5ULHeaderWrapper(entry.PK, entryHeader, Factory);

			AssertEquals("수정신고", wrapper.RefundCauseCodeDescription);
		}

		public void TestEntryLinesData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "11234", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "HS_Desc1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "11235", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "HS_Desc2");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_RefundType = RefundTypeList.Codes.B;
			instruction.CEI_RefundCauseCode = "04";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumberTypeIsIMP = entry.EntryNumbers.AddNew();
			entryNumberTypeIsIMP.CE_EntryNum = "4207216092149M";
			entryNumberTypeIsIMP.CE_EntryType = JobMessageTypeList.Codes.Import;
			entryNumberTypeIsIMP.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 001;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_SequenceNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_SequenceNumber = 2;
			invoiceLine2.JI_CL = entryLine.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 002;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_SequenceNumber = 1;
			invoiceLine3.JI_CL = entryLine2.PK;

			var entry5UL = declaration.CustomsEntryHeaders.AddNew();
			entry5UL.CH_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			entry5UL.CH_CH_PrimeEntry = entry.PK;

			var entryNumberTypeIs5UL = entry5UL.EntryNumbers.AddNew();
			entryNumberTypeIs5UL.CE_EntryNum = "416372000190U";
			entryNumberTypeIs5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumberTypeIs5UL.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var entryLine5UL1 = entry5UL.MergedLines.AddNew();
			entryLine5UL1.CL_LineNumber = 001;
			entryLine5UL1.CL_AdValoremTariff = "11234";
			var invoiceLine5UL1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5UL1.JI_SequenceNumber = 1;
			invoiceLine5UL1.JI_CL = entryLine5UL1.PK;
			invoiceLine5UL1.JI_Tariff = "11234";

			var invoiceLine5UL2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5UL2.JI_SequenceNumber = 2;
			invoiceLine5UL2.JI_CL = entryLine5UL1.PK;
			invoiceLine5UL2.JI_Tariff = "11234";

			var entryLine5UL2 = entry5UL.MergedLines.AddNew();
			entryLine5UL2.CL_LineNumber = 002;
			entryLine5UL2.CL_AdValoremTariff = "11235";
			var invoiceLine5UL3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5UL3.JI_SequenceNumber = 2;
			invoiceLine5UL3.JI_CL = entryLine5UL2.PK;
			invoiceLine5UL3.JI_Tariff = "11235";

			new TestDataSetupHelper(Factory).Create5ULSnapshot(entry);
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5UL, EntrySnapshotStatus.Lodged);
			var wrapper = new EntrySnapshotWrapper(snapshot, Factory).DataProvider5UL;
			AssertEquals(1, wrapper.EntryLineItems.Count);
			AssertEquals("42072-16-092149M", wrapper.EntryLineItems[0].FormattedImportDeclarationNumber);
			AssertEquals(0m, wrapper.EntryLineItems[0].EntryLine.TotalOtherTaxItemAmount);
		}
	}
}
