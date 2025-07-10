using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineBusinessObjectTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestCustomsValueInUSD()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			currency.ExchangeRates.DeleteAll();
			Factory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var entryLine = testItems.EntryLine;
			entryLine.CL_CustomsValue = 6000m;
			AssertNotNull(entryLine.CurrencyConverter);
			AssertEquals(0m, entryLine.CustomsValueInUSD);
			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_SellRate = 6.75m;
			Factory.Save();
			AssertEquals(888.89m, entryLine.CustomsValueInUSD);
		}

		public void TestCustomsSupervisionConditions()
		{
			var refFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(refFactory);
			var tariff = helper.CreateCustomsTariff("27132000");
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "bM", tariff);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCUSRequirement, "BQ", tariff);
			refFactory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			testItems.EntryHeader.CH_MessageType = "CUS";
			testItems.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			testItems.InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			Factory.Save();
			AssertEquals("M, b", testItems.EntryLine.CustomsSupervisionConditions);
		}

		public void TestInspectionSupervisionConditions()
		{
			var refFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(refFactory);
			var tariff = helper.CreateCustomsTariff("27132000");
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "bM", tariff);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCIQRequirement, "BQ", tariff);
			refFactory.Save();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			testItems.InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			Factory.Save();
			CombineAssertions(() =>
			{
				testItems.EntryHeader.CH_MessageType = EntryTypeList.Codes.CustomsEntry;
				testItems.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("B, Q", testItems.EntryLine.InspectionSupervisionConditions);
				testItems.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				testItems.InvoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
				AssertEquals("M, b", testItems.EntryLine.InspectionSupervisionConditions);
			});
		}

		[TestDate(2018, 12, 11)]
		public void TestEffectiveDeclareCurrency()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CURR", "Currencies");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CURR", "USD", "USD currency", new ZDateTime(2018, 1, 1), new ZDateTime(2018, 12, 31));
			Factory.Save();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = testDeclaration.CustomsEntryHeaders.AddNew();
			var instruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testEntry.CH_CEI_Instruction = instruction.PK;
			var entryLine = testEntry.MergedLines.AddNew();
			AssertEquals("CNY", entryLine.CurrencyCode);
			var invoiceHeader = testDeclaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_JZ = invoiceHeader.PK;
			AssertEquals("USD", entryLine.CurrencyCode);
			invoiceHeader.JZ_RX_NKInvoice_Currency = "INR";
			AssertEquals("CNY", entryLine.CurrencyCode);
		}

		public void TestTradeUnitQty()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory);
			testItems.InvoiceLine.JI_TradeUnitQty = "035";
			Factory.Save();
			AssertEquals("035", testItems.EntryLine.TradeUnitQty);
		}

		public void TestDutyAndTaxes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.Duty);
			var expRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.ExportDuty);
			var excRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.Excise);
			var addRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.AntiDumping);
			var cvdRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.China, Universal.Constants.RateTypes.Countervailing);
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, "DT1", dtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "DT2", dtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EX1", expRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EX2", expRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EC1", excRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EC2", excRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "ADD", addRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "CVD", cvdRateType.PK);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_ZZF_NKTaxType = "VRD";
			entryLine.Fees.AddOrUpdate("DT1", 12m);
			entryLine.Fees.AddOrUpdate("DT2", 23m);
			entryLine.Fees.AddOrUpdate("EX1", 34m);
			entryLine.Fees.AddOrUpdate("EX2", 45m);
			entryLine.Fees.AddOrUpdate("EC1", 56m);
			entryLine.Fees.AddOrUpdate("EC2", 67m);
			entryLine.Fees.AddOrUpdate("VRD", 78m);
			entryLine.Fees.AddOrUpdate("DTY", 89m);
			entryLine.Fees.AddOrUpdate("VAT", 91m);
			entryLine.Fees.AddOrUpdate("ADD", 21.1m);
			entryLine.Fees.AddOrUpdate("CVD", 43.3m);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals("DutyAmount", 89m, entryLine.DutyAmount);
			AssertEquals("GSTVATAmount", 91m, entryLine.GSTVATAmount);
			AssertEquals("ExciseAmount", 0m, entryLine.ExciseAmount);
			AssertEquals("AntiDumpingAmount", 0m, entryLine.AntiDumpingAmount);
			AssertEquals("CountervailingAmount", 0m, entryLine.CountervailingAmount);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("DutyAmount", 35m, entryLine.DutyAmount);
			AssertEquals("GSTVATAmount", 78m, entryLine.GSTVATAmount);
			AssertEquals("ExciseAmount", 123m, entryLine.ExciseAmount);
			AssertEquals("AntiDumpingAmount", 21.1m, entryLine.AntiDumpingAmount);
			AssertEquals("CountervailingAmount", 43.3m, entryLine.CountervailingAmount);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertEquals("DutyAmount", 79m, entryLine.DutyAmount);
		}

		[TestDate(2018, 12, 11)]
		public void TestEntryLRNAndLineNo()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CURR", "Currencies");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CURR", "USD", "USD currency", new ZDateTime(2018, 1, 1), new ZDateTime(2018, 12, 31));
			Factory.Save();
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = testDeclaration.CustomsEntryHeaders.AddNew();
			var instruction = testDeclaration.CustomsEntryInstructions.AddNew();
			testEntry.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = testEntry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			AssertEquals(ZString.Empty, entryLine1.EntryLRNAndEntryLineNo);
			var entryLine2 = testEntry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = testEntry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			AssertEquals(3, testEntry.CountOfLines);
			testEntry.CH_BGMReference = "ABCD123";
			var entryLine4 = testEntry.MergedLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			var entryLine5 = testEntry.MergedLines.AddNew();
			entryLine5.CL_LineNumber = 5;
			var entryLine6 = testEntry.MergedLines.AddNew();
			entryLine6.CL_LineNumber = 6;
			var entryLine7 = testEntry.MergedLines.AddNew();
			entryLine7.CL_LineNumber = 7;
			var entryLine8 = testEntry.MergedLines.AddNew();
			entryLine8.CL_LineNumber = 8;
			AssertEquals(8, testEntry.CountOfLines);
			AssertEquals("ABCD123/1", entryLine1.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/2", entryLine2.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/3", entryLine3.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/4", entryLine4.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/5", entryLine5.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/6", entryLine6.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/7", entryLine7.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/8", entryLine8.EntryLRNAndEntryLineNo);
			var entryLine9 = testEntry.MergedLines.AddNew();
			entryLine9.CL_LineNumber = 9;
			var entryLine10 = testEntry.MergedLines.AddNew();
			entryLine10.CL_LineNumber = 10;
			var entryLine11 = testEntry.MergedLines.AddNew();
			entryLine11.CL_LineNumber = 11;
			var entryLine12 = testEntry.MergedLines.AddNew();
			entryLine12.CL_LineNumber = 12;
			var entryLine13 = testEntry.MergedLines.AddNew();
			entryLine13.CL_LineNumber = 13;
			AssertEquals(13, testEntry.CountOfLines);
			AssertEquals("ABCD123/01", entryLine1.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/02", entryLine2.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/03", entryLine3.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/04", entryLine4.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/05", entryLine5.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/06", entryLine6.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/07", entryLine7.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/08", entryLine8.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/09", entryLine9.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/10", entryLine10.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/11", entryLine11.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/12", entryLine12.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/13", entryLine13.EntryLRNAndEntryLineNo);
			var entryLine = testEntry.MergedLines.AddNew();
			for (var i = 14; i <= 100; i++)
			{
				entryLine = testEntry.MergedLines.AddNew();
				entryLine.CL_LineNumber = (ZShort)i;
			}

			AssertEquals("ABCD123/100", entryLine.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/001", entryLine1.EntryLRNAndEntryLineNo);
			AssertEquals("ABCD123/013", entryLine13.EntryLRNAndEntryLineNo);
		}

		public void TestRequiresLegalInspection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "HSN");
			Factory.Save();
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200001", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCUSRequirement, "A", tariff1);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200002", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportCUSRequirement, "B", tariff2);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffType.PK, "2713200003", minDate, maxDate);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportCIQRequirement, "L", tariff3);
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			var invoiceLine = testItem.InvoiceLine;
			testItem.JobDeclaration.JE_MessageType = "IMP";
			var entryLine = testItem.EntryLine;
			Assert("Should be false when Tariff not set.", !entryLine.RequiresLegalInspection);
			invoiceLine.JI_Tariff = "2713200001";
			Assert("ImportCUSRequirement.", entryLine.RequiresLegalInspection);
			invoiceLine.JI_Tariff = "2713200002";
			Assert("ExportCUSRequirement.", !entryLine.RequiresLegalInspection);
			invoiceLine.JI_Tariff = "2713200003";
			Assert("ImportCIQRequirement.", entryLine.RequiresLegalInspection);
			testItem.JobDeclaration.JE_MessageType = "EXP";
			invoiceLine.JI_Tariff = "2713200001";
			Assert("ImportCUSRequirement.", !entryLine.RequiresLegalInspection);
			invoiceLine.JI_Tariff = "2713200002";
			Assert("ExportCUSRequirement.", entryLine.RequiresLegalInspection);
			invoiceLine.JI_Tariff = "2713200003";
			Assert("ImportCIQRequirement.", !entryLine.RequiresLegalInspection);
		}

		protected override bool RatesAreReciprocal => true;

		protected override ZString ExpectedFallbackEntrylineDescription => InvoiceLinePartClassificationTariffDescriptionSyncroniserTest.TariffDescriptionCore;

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			new LineMerger((JobDeclaration)declaration).DoMerge();
		}

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructions.AddNew();
			foreach (JobComInvoiceLine line in declaration.InvoiceLines)
			{
				line.JI_CEI = testInstruction.PK;
			}
		}
	}
}
