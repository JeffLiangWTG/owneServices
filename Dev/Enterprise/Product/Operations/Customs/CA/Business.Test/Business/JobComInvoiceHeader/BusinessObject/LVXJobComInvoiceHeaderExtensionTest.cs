using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LVXJobComInvoiceHeaderExtensionTest : TestCaseWithFactory
	{
		public void TestCheckCurrentRemissionConditions()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly));

			invoiceLine1.CA_CalculationMethod = ZString.Empty;
			invoiceLine1.CA_RemissionType = ZString.Empty;
			invoiceLine1.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber1;
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly));

			invoiceLine1.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			invoiceLine1.CA_RemissionType = RemissionTypeList.Codes.OrderInCouncil;
			invoiceLine1.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber1;
			AssertEquals(true, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly));

			invoiceLine1.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber2;
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll));
			AssertEquals(true, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly));

			invoiceLine1.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber3;
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax));
			AssertEquals(true, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly));

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly));

			invoiceLine2.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber3;
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly));

			invoiceLine2.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;
			invoiceLine2.CA_RemissionType = RemissionTypeList.Codes.OrderInCouncil;
			invoiceLine2.CA_AuthorityNumber = DutyAndTaxManager.TaxRemittedOICNumber3;
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionAll));
			AssertEquals(false, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyAndTax));
			AssertEquals(true, invoice.CheckCurrentRemissionConditions(LVXJobComInvoiceHeaderExtension.RemissionMexicoAndUSDutyOnly));
		}

		public void TestIsRemissionAll()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.LVXInvoiceHeader;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.China;
			Assert(invoice.IsRemissionAll());
			invoice.JZ_ValuationDateOverride = ZDate.Today.AddDays(-2);
			Assert(!invoice.IsRemissionAll());
			invoice.JZ_ValuationDateOverride = ZDate.Today.AddDays(2);
			Assert(!invoice.IsRemissionAll());
			invoice.JZ_ValuationDateOverride = ZDate.Today;
			Assert(invoice.IsRemissionAll());
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			Assert(!invoice.IsRemissionAll());
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Mexico;
			Assert(!invoice.IsRemissionAll());
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Canada;
			Assert(invoice.IsRemissionAll());
			Assert(!invoice.IsRemissionAll("US"));
			Assert(!invoice.IsRemissionAll("MX"));
			invoiceLine.CA_CustomsValue = 20m;
			Assert(invoice.IsRemissionAll());
			invoiceLine.CA_CustomsValue = 21m;
			Assert(!invoice.IsRemissionAll());
		}

		public void TestIsRemissionMexicoAndUSDutyAndTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.LVXInvoiceHeader;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.China;
			Assert(!invoice.IsRemissionMexicoAndUSDutyAndTax());
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			Assert(invoice.IsRemissionMexicoAndUSDutyAndTax());
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Mexico;
			Assert(invoice.IsRemissionMexicoAndUSDutyAndTax());
			Assert(!invoice.IsRemissionMexicoAndUSDutyAndTax("AU"));
			invoice.JZ_ValuationDateOverride = ZDate.Today.AddDays(-2);
			Assert(!invoice.IsRemissionMexicoAndUSDutyAndTax());
			invoice.JZ_ValuationDateOverride = ZDate.Today.AddDays(2);
			Assert(!invoice.IsRemissionMexicoAndUSDutyAndTax());
			invoice.JZ_ValuationDateOverride = ZDate.Today;
			Assert(invoice.IsRemissionMexicoAndUSDutyAndTax());
			invoiceLine.CA_CustomsValue = 40m;
			Assert(invoice.IsRemissionMexicoAndUSDutyAndTax());
			invoiceLine.CA_CustomsValue = 41m;
			Assert(!invoice.IsRemissionMexicoAndUSDutyAndTax());
		}

		public void TestIsRemissionMexicoAndUSDutyOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.LVXInvoiceHeader;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.China;
			Assert(!invoice.IsRemissionMexicoAndUSDutyOnly());
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			Assert(invoice.IsRemissionMexicoAndUSDutyOnly());
			invoice.CA_RN_NKExport = Core.Constants.CountryCodes.Mexico;
			Assert(invoice.IsRemissionMexicoAndUSDutyOnly());
			Assert(!invoice.IsRemissionMexicoAndUSDutyOnly("AU"));
			invoice.JZ_ValuationDateOverride = ZDate.Today.AddDays(-2);
			Assert(!invoice.IsRemissionMexicoAndUSDutyOnly());
			invoice.JZ_ValuationDateOverride = ZDate.Today.AddDays(2);
			Assert(!invoice.IsRemissionMexicoAndUSDutyOnly());
			invoice.JZ_ValuationDateOverride = ZDate.Today;
			Assert(invoice.IsRemissionMexicoAndUSDutyOnly());
			invoiceLine.CA_CustomsValue = 150m;
			Assert(invoice.IsRemissionMexicoAndUSDutyOnly());
			invoiceLine.CA_CustomsValue = 151m;
			Assert(!invoice.IsRemissionMexicoAndUSDutyOnly());
		}

		public void TestClearCachedValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var dty1 = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CustomsDuty);
			dty1.C1_Override = true;
			dty1.C1_Amount = 15m;
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertEquals(15m, invoiceLine.DutyAndTaxManager.CalculatedValueForTax);

			dty1.C1_Amount = 0m;
			invoiceLine.DutyAndTaxManager.ClearCachedValues();
			AssertEquals(0m, invoiceLine.DutyAndTaxManager.CalculatedValueForTax);
		}

		public void TestApplyRemissionAction()
		{
			JobComInvoiceLineTestHelper.CreateCAGSTRateCode(Factory, 5m);
			var tariffNumber = "3824600001";
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CFIA", tariffNumber);
			var startDate = ZDateTime.Today.AddMonths(-1);
			var endDate = ZDateTime.Today.AddMonths(1);

			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = tariffNumber;
			classHeader.ZA_EffectiveDate = startDate;
			classHeader.ZA_ExpiryDate = endDate;
			classHeader.ZA_AreaCode = "XXX";

			var rate1 = Factory.New<CACTaxRate>();
			rate1.ZH_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			rate1.ZH_EffectiveDate = ZDateTime.Today.AddDays(-1);
			rate1.ZH_ExpiryDate = ZDateTime.Today.AddDays(1);
			rate1.ZH_TaxRefNumber = "XXX";
			rate1.ZH_RateType = RateTypes.Codes.AdValorem;
			rate1.ZH_Rate = 10;

			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ExciseTax, rateType1.PK);
			var rateType2 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Duty);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CustomsDuty, rateType2.PK);
			var rateType3 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode3 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "E90", rateType3.PK);
			var rateCode4 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "E91", rateType3.PK);
			var rateCode5 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "BB1", rateType3.PK);
			var rateCode6 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "BB2", rateType3.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, tariffNumber, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var taxRate1 = universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.1*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate2 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.2*[MIL]", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate3 = universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.3*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate4 = universalHelper.CreateRate(tariff1, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate5 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate6 = universalHelper.CreateRate(tariff1, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate7 = universalHelper.CreateRate(tariff1, rateCode5.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate8 = universalHelper.CreateRate(tariff1, rateCode6.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration.LVXInvoiceHeader;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariffNumber;
			invoiceLine1.JI_LinePrice = 100m;
			var tax1 = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			tax1.C1_Code = "001";
			tax1.C1_Amount = 5.15m;
			tax1.C1_Override = false;
			var sim1 = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SIMADuty);
			sim1.C1_Amount = 14m;
			sim1.C1_Override = true;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariffNumber;
			invoiceLine2.JI_LinePrice = 200m;
			var tax2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
			tax2.C1_Code = "001";
			tax2.C1_Amount = 10m;
			tax2.C1_Override = false;
			var sim2 = invoiceLine2.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SIMADuty);
			sim2.C1_Amount = 14m;
			sim2.C1_Override = false;

			invoice.ApplyCLVSRemissionThresholdAll();
			AssertEquals(0m, tax1.C1_Amount);
			AssertEquals(0m, tax2.C1_Amount);
			AssertEquals(14m, sim1.C1_Amount);
			AssertEquals(0m, sim2.C1_Amount);
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine1.CA_CalculationMethod);
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine2.CA_CalculationMethod);
			AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber1, invoiceLine1.CA_AuthorityNumber);
			AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber1, invoiceLine2.CA_AuthorityNumber);
			AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine1.CA_RemissionType);
			AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine2.CA_RemissionType);

			var sur1 = invoiceLine1.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
			sur1.C1_Amount = 3m;
			sur1.C1_Override = false;
			sur1.C1_ExemptCode = SIMACodes.Codes.C51;
			sim2.C1_Amount = 14m;

			invoice.ApplyCLVSRemissionMexicoAndUSDutyAndTax();
			AssertEquals(0m, tax1.C1_Amount);
			AssertEquals(0m, tax2.C1_Amount);
			AssertEquals(14m, sim1.C1_Amount);
			AssertEquals(0m, sim2.C1_Amount);
			AssertEquals(3m, sur1.C1_Amount);
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine1.CA_CalculationMethod);
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine2.CA_CalculationMethod);
			AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber2, invoiceLine1.CA_AuthorityNumber);
			AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber2, invoiceLine2.CA_AuthorityNumber);
			AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine1.CA_RemissionType);
			AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine2.CA_RemissionType);

			sim2.C1_Amount = 14m;
			invoice.ApplyCLVSRemissionMexicoAndUSDutyOnly();
			AssertEquals(5.15m, tax1.C1_Amount);
			AssertEquals(10m, tax2.C1_Amount);
			AssertEquals(14m, sim1.C1_Amount);
			AssertEquals(0m, sim2.C1_Amount);
			AssertEquals(3m, sur1.C1_Amount);
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine1.CA_CalculationMethod);
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine2.CA_CalculationMethod);
			AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber3, invoiceLine1.CA_AuthorityNumber);
			AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber3, invoiceLine2.CA_AuthorityNumber);
			AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine1.CA_RemissionType);
			AssertEquals(RemissionTypeList.Codes.OrderInCouncil, invoiceLine2.CA_RemissionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var caDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			var taxOrFee1 = helper.CreateTaxOrFee("RT1", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 20m);
			var taxOrFee2 = helper.CreateTaxOrFee("RT2", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 40m);
			var taxOrFee3 = helper.CreateTaxOrFee("RT3", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 150m);
			Factory.Save();
		}
	}
}
