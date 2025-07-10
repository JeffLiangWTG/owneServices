using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.BaseCusClassification;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CADutyAndTaxAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRatesForGST()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "CAGSTRateCodes", Core.Constants.CountryCodes.Canada);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "001", "001 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "002", "002 DESC", new ZDate(2024, 10, 01), ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "003", "003 DESC", ZDateTime.BrettsBirthday, new ZDate(2024, 09, 30));
			Factory.Save();

			invoiceLine.Declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 10, 02);
			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			var list = tax.AddInfoLookups.Rates;
			AssertEquals(2, list.Count);
			var rates = list.Cast<ICodeDescription>();
			Assert(rates.Any(x => x.Code == "001"));
			Assert(rates.Any(x => x.Code == "002"));
			Assert(!rates.Any(x => x.Code == "003"));

			invoiceLine.Declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 09, 29);
			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			list = tax.AddInfoLookups.Rates;
			AssertEquals(2, list.Count);
			rates = list.Cast<ICodeDescription>();
			Assert(rates.Any(x => x.Code == "001"));
			Assert(!rates.Any(x => x.Code == "002"));
			Assert(rates.Any(x => x.Code == "003"));
		}

		public void TestRatesIsCorrectlyCached()
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "1234567890";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);

			invoiceLine.JI_Tariff = classHeader.ZA_ClassificationNumber;
			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			var exciseTaxRates = tax.AddInfoLookups.Rates;
			AssertSame("Should be cached", exciseTaxRates, tax.AddInfoLookups.Rates);
			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			var gstRates = tax.AddInfoLookups.Rates;
			AssertEquals("Should not be the same", false, object.ReferenceEquals(exciseTaxRates, gstRates));
			AssertSame("Should be cached", gstRates, tax.AddInfoLookups.Rates);

			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(-1);
			var gstRates2 = tax.AddInfoLookups.Rates;
			AssertEquals("Should not be the same", false, object.ReferenceEquals(gstRates2, gstRates));
			AssertSame("Should be cached", gstRates2, tax.AddInfoLookups.Rates);
		}

		public void TestRatesForSUR()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var refCusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.ADD);
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, refCusRateType.PK);
			var harmonizedTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate1 = helper.CreateRate(tariff1, refCusRateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var rate2 = helper.CreateRate(tariff1, refCusRateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var rate3 = helper.CreateRate(tariff1, refCusRateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var tradeGroupView1 = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.UnitedStates);
			var tradeGroupView2 = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Australia);
			_ = helper.AddNewOrExistingCountry(tradeGroupView1, Core.Constants.CountryCodes.UnitedStates);
			_ = helper.AddNewOrExistingCountry(tradeGroupView2, Core.Constants.CountryCodes.Australia);
			_ = helper.CreateCusApplicability(rate1.PK, tradeGroupView1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C001");
			_ = helper.CreateCusApplicability(rate2.PK, tradeGroupView1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C002");
			_ = helper.CreateCusApplicability(rate3.PK, tradeGroupView1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C003");
			_ = helper.CreateCusApplicability(rate3.PK, tradeGroupView2, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "C004");
			Factory.Save();

			invoiceLine.JI_Tariff = "00000001";
			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			var rates = tax.AddInfoLookups.Rates;
			AssertEquals(3, rates.Count);
			AssertEquals(true, rates.ContainsCode("C001"));
			AssertEquals(true, rates.ContainsCode("C002"));
			AssertEquals(true, rates.ContainsCode("C003"));
		}

		public void TestRatesLookupSameForProductAndClassificationLookup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var refCusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.ExciseTax);
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "C01", refCusRateType.PK);
			var refCusRateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "C02", refCusRateType.PK);
			var refCusRateCode3 = helper.LoadOrCreateNewCusRateCode(Factory, "C03", refCusRateType.PK);
			var refCusRateCode4 = helper.LoadOrCreateNewCusRateCode(Factory, "C04", refCusRateType.PK);

			var harmonizedTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateRate(tariff1, refCusRateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			helper.CreateRate(tariff1, refCusRateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			helper.CreateRate(tariff1, refCusRateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.JI_Tariff = "00000001";
			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			var list = tax.AddInfoLookups.Rates;
			AssertEquals(3, list.Count);
			var rates = list.Cast<ICodeDescription>();
			Assert(rates.Any(x => x.Code == "C01"));
			Assert(rates.Any(x => x.Code == "C04"));
			Assert(rates.Any(x => x.Code == "NO"));
			Assert(!rates.Any(x => x.Code == "C02"));
			Assert(!rates.Any(x => x.Code == "C03"));

			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();

			var part1 = Factory.New<OrgSupplierPart>();
			part1.FillWithValidTestData();
			part1.OP_PartNum = "HYD";
			part1.RelatedOrganisations.AddOwner(importer1);
			part1.RelatedOrganisations.AddSupplier(importer2);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			pivot.CI_OP = part1.PK;
			pivot.CI_TariffNum = "00000001";
			pivot.CCA_ETRateCode = "E91";

			pivot.CI_TariffNum = "00000001";
			pivot.CCA_ETRateCode = "E89";

			Factory.Save();
			AssertEquals(3, pivot.CAClassificationLookups.ExciseTaxRateCodes.Count);
			rates = pivot.CAClassificationLookups.ExciseTaxRateCodes.Cast<ICodeDescription>();
			Assert(rates.Any(x => x.Code == "C01"));
			Assert(rates.Any(x => x.Code == "C04"));
			Assert(rates.Any(x => x.Code == "NO"));
			Assert(!rates.Any(x => x.Code == "C02"));
			Assert(!rates.Any(x => x.Code == "C03"));

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = ClassificationType.IMP;
			classification.CC_TariffNum = "00000001";
			AssertEquals(3, classification.CAClassificationLookups.ExciseTaxRateCodes.Count);
			rates = classification.CAClassificationLookups.ExciseTaxRateCodes.Cast<ICodeDescription>();
			Assert(rates.Any(x => x.Code == "C01"));
			Assert(rates.Any(x => x.Code == "C04"));
			Assert(rates.Any(x => x.Code == "NO"));
			Assert(!rates.Any(x => x.Code == "C02"));
			Assert(!rates.Any(x => x.Code == "C03"));
		}

		public void TestRatesIsDifferent_TariffIsDifferent()
		{
			var classHeader1 = Factory.New<CACClassHeader>();
			classHeader1.ZA_ClassificationNumber = "1234567890";
			classHeader1.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			classHeader1.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var refNumHeader1 = Factory.New<CACTaxRefNumHeader>();
			refNumHeader1.ZD_ZA_ClassNumber = classHeader1.PK;
			refNumHeader1.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			refNumHeader1.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);
			var classHeader2 = Factory.New<CACClassHeader>();
			classHeader2.ZA_ClassificationNumber = "0987654321";
			classHeader2.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			classHeader2.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var refNumHeader2 = Factory.New<CACTaxRefNumHeader>();
			refNumHeader2.ZD_ZA_ClassNumber = classHeader2.PK;
			refNumHeader2.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			refNumHeader2.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);

			invoiceLine.JI_Tariff = classHeader1.ZA_ClassificationNumber;
			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			var rates1 = tax.AddInfoLookups.Rates;
			invoiceLine.JI_Tariff = classHeader2.ZA_ClassificationNumber;
			var rates2 = tax.AddInfoLookups.Rates;
			AssertNotSame("Should not be the same", rates1, rates2);
		}

		public void TestExciseRates()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var refCusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.ExciseTax);
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "C01", refCusRateType.PK);
			var refCusRateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "C02", refCusRateType.PK);
			var refCusRateCode3 = helper.LoadOrCreateNewCusRateCode(Factory, "C03", refCusRateType.PK);
			var refCusRateCode4 = helper.LoadOrCreateNewCusRateCode(Factory, "C04", refCusRateType.PK);

			var harmonizedTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateRate(tariff1, refCusRateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			helper.CreateRate(tariff1, refCusRateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			helper.CreateRate(tariff1, refCusRateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			invoiceLine.JI_Tariff = "00000001";
			invoiceLine.Declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			var list = tax.AddInfoLookups.Rates;
			AssertEquals(3, list.Count);
			var rates = list.Cast<ICodeDescription>();
			Assert(rates.Any(x => x.Code == "C01"));
			Assert(rates.Any(x => x.Code == "C04"));
			Assert(rates.Any(x => x.Code == "NO"));
			Assert(!rates.Any(x => x.Code == "C02"));
			Assert(!rates.Any(x => x.Code == "C03"));

			invoiceLine.JI_Tariff = "00000002";
			list = tax.AddInfoLookups.Rates;
			AssertEquals(5, list.Count);
			rates = list.Cast<ICodeDescription>();
			Assert(rates.Any(x => x.Code == "C01"));
			Assert(rates.Any(x => x.Code == "C02"));
			Assert(rates.Any(x => x.Code == "C03"));
			Assert(rates.Any(x => x.Code == "C04"));
			Assert(rates.Any(x => x.Code == "NO"));
		}

		public void TestProperties()
		{
			AssertEquals("Types", typeof(DutyAndTaxTypes), tax.AddInfoLookups.Types.GetType());
			Assert("'SIM' is excluded from Types list", !tax.AddInfoLookups.Types.ContainsCode(DutyAndTaxTypes.Codes.SIMADuty));
			AssertEquals("RateTypes", typeof(RateTypes), tax.AddInfoLookups.RateTypes.GetType());
			AssertEquals("CustomsUQList", typeof(CustomsUnitOfMeasureList), tax.AddInfoLookups.CustomsUQList.GetType());
			AssertEquals("CurrencyList", typeof(RefCurrencyCollection), tax.AddInfoLookups.CurrencyList.GetType());

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			AssertEquals("ExemptCodes", typeof(CodeDescriptionPairList), tax.AddInfoLookups.ExemptCodes.GetType());

			tax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			AssertEquals("ExemptCodes", typeof(ExciseTaxExemptionCodes), tax.AddInfoLookups.ExemptCodes.GetType());

			tax.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			AssertEquals("ExemptCodes", typeof(GSTStatusCodes), tax.AddInfoLookups.ExemptCodes.GetType());

			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			AssertEquals("Rates", string.Empty, tax.AddInfoLookups.Rates.CodesAsString);
			AssertEquals("ExemptCodes", typeof(SIMACodes), tax.AddInfoLookups.ExemptCodes.GetType());

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			AssertEquals("Rates", string.Empty, tax.AddInfoLookups.Rates.CodesAsString);
			AssertEquals("ExemptCodes", typeof(SIMACodes), tax.AddInfoLookups.ExemptCodes.GetType());

			tax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			AssertEquals("Rates", string.Empty, tax.AddInfoLookups.Rates.CodesAsString);
			AssertEquals("ExemptCodes", typeof(SIMACodes), tax.AddInfoLookups.ExemptCodes.GetType());

			tax.C1_TaxType = DutyAndTaxTypes.Codes.CPT;
			AssertEquals("ExemptCodes", typeof(CPTExcemptionCodes), tax.AddInfoLookups.ExemptCodes.GetType());

			tax.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
			AssertEquals("Rates", string.Empty, tax.AddInfoLookups.Rates.CodesAsString);
			AssertEquals("ExemptCodes", typeof(SIMACodes), tax.AddInfoLookups.ExemptCodes.GetType());
		}

		public void TestPropertiesForPivot()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			var tax = pivot.DutiesAndTaxes.AddNew();
			AssertEquals("Types", 3, tax.AddInfoLookups.Types.Count);
			Assert("'SIM' is excluded from Types list", !tax.AddInfoLookups.Types.ContainsCode(DutyAndTaxTypes.Codes.SIMADuty));
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			AssertEquals("ExemptCodes", typeof(SIMACodes), tax.AddInfoLookups.ExemptCodes.GetType());
			AssertEquals("CurrencyList", typeof(RefCurrencyCollection), tax.AddInfoLookups.CurrencyList.GetType());
		}

		public void TestPropertiesForCusClassification()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = ClassificationType.IMP;
			var tax = classification.DutiesAndTaxes.AddNew();
			AssertEquals("Types", 3, tax.AddInfoLookups.Types.Count);
			Assert("'SIM' is excluded from Types list", !tax.AddInfoLookups.Types.ContainsCode(DutyAndTaxTypes.Codes.SIMADuty));
			tax.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			AssertEquals("ExemptCodes", typeof(SIMACodes), tax.AddInfoLookups.ExemptCodes.GetType());
			AssertEquals("CurrencyList", typeof(RefCurrencyCollection), tax.AddInfoLookups.CurrencyList.GetType());
		}

		public void TestWI00420381()
		{
			AssertNoExceptionThrown(() =>
			{
				var dutyAndTax = Factory.New<DutyAndTax>();
				dutyAndTax.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
				var rates = dutyAndTax.AddInfoLookups.Rates;
			});
		}

		#region Implementation

		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			tax = invoiceLine.DutiesAndTaxes.AddNew();
			tax.Parent = invoiceLine;
		}

		DutyAndTax tax;

		#endregion
	}
}
