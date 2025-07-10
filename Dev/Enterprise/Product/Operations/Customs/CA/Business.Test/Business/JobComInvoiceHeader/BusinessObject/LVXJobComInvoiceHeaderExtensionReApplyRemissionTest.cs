using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LVXJobComInvoiceHeaderExtensionReApplyRemissionTest : TestCaseWithFactory
	{
		public void TestReApplyCLVSRemissionThresholdAllAfterNoRemission()
		{
			PrepareData();
			(JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine) = CreateDeclaration(Core.Constants.CountryCodes.Chile);
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(invoiceLine, 180, 0, 259.6m, 59m, 1180m, true);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			declaration.RunPreSaveValidation();
			var gst = invoiceLine.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertEquals(69m, gst.C1_Amount);

			invoiceHeader.ApplyCLVSRemissionThresholdAll();
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals(0m, gst.C1_Amount);
			AssertEquals(true, invoiceHeader.IsRemissionAll());

			invoiceHeader.CA_RN_NKExport = "US";
			AssertEquals(CalculationMethods.Codes.NoRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals(ZString.Empty, invoiceLine.CA_AuthorityNumber);
			AssertEquals(ZString.Empty, invoiceLine.CA_99TariffCode);
		}

		public void TestApplyRemissionActionSettingAmount0()
		{
			PrepareData();
			(JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine) = CreateDeclaration(Core.Constants.CountryCodes.Chile);

			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
				AssertRemissionAmounts(invoiceLine, 180, 0, 259.6m, 59m, 1180m, true);

			invoiceHeader.ApplyCLVSRemissionThresholdAll();
			AssertEquals(true, invoiceLine.DutiesAndTaxes[0].C1_Override);
			AssertEquals(180m, invoiceLine.DutiesAndTaxes[0].C1_Amount);
			AssertEquals(1000m, invoiceLine.CA_CustomsValue);
			AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber1, invoiceLine.CA_AuthorityNumber);

			AssertEquals(ZDecimal.Zero, invoiceLine.DutiesAndTaxes[1].C1_Amount);
			AssertEquals(ZDecimal.Zero, invoiceLine.DutiesAndTaxes[2].C1_Amount);

			invoiceLine.CA_AuthorityNumber = ZString.Empty;
			invoiceLine.CA_CustomsValue = 7.62m;
			invoiceLine.DutiesAndTaxes[0].C1_Override = false;
			invoiceHeader.ApplyCLVSRemissionThresholdAll();
			AssertEquals(false, invoiceLine.DutiesAndTaxes[0].C1_Override);
			AssertEquals(ZDecimal.Zero, invoiceLine.DutiesAndTaxes[0].C1_Amount);
			AssertEquals(ZDecimal.Zero, invoiceLine.DutiesAndTaxes[1].C1_Amount);
			AssertEquals(ZDecimal.Zero, invoiceLine.DutiesAndTaxes[2].C1_Amount);
			AssertEquals(DutyAndTaxManager.TaxRemittedOICNumber1, invoiceLine.CA_AuthorityNumber);
		}

		public void TestReApplyCLVSRemissionMexicoAndUSDutyAndTaxAfterNoRemission()
		{
			PrepareData();
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
			var taxRate1 = universalHelper.CreateRate(countervailingRelTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.1*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate2 = universalHelper.CreateRate(countervailingRelTariff, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.2*[MIL]", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate3 = universalHelper.CreateRate(countervailingRelTariff, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.3*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate4 = universalHelper.CreateRate(countervailingRelTariff, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.4*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate5 = universalHelper.CreateRate(countervailingRelTariff, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.5*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate6 = universalHelper.CreateRate(countervailingRelTariff, rateCode4.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-10), rateFormula: "0.6*[KGM]", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate7 = universalHelper.CreateRate(countervailingRelTariff, rateCode5.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);
			var taxRate8 = universalHelper.CreateRate(countervailingRelTariff, rateCode6.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "22*VFD", dataGrouping: Core.Constants.CountryCodes.Canada);

			(JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine) = CreateDeclaration(Core.Constants.CountryCodes.UnitedStates);
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			ActiveBusinessObjectCollection.RefreshAll(Factory);
			AssertEquals("DutiesAndTaxes count", 4, invoiceLine.DutiesAndTaxes.Count);
			invoiceLine.DutiesAndTaxes.ApplySort(DutyAndTax.Schema.C1_TaxType, ListSortDirection.Ascending);
			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.CVD, ZString.Empty, true, RateTypes.Codes.Specific, 200, "KGM", 180);
			AssertTax(invoiceLine.DutiesAndTaxes[1], false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, 0);
			AssertTax(invoiceLine.DutiesAndTaxes[2], false, DutyAndTaxTypes.Codes.ExciseTax, ZString.Empty, false, ZString.Empty, 0, ZString.Empty, 0);
			AssertTax(invoiceLine.DutiesAndTaxes[3], false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5, ZString.Empty, 59m);

			AssertEquals("NormalValueForTax", 1180m, invoiceLine.DutyAndTaxManager.NormalValueForTax);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			declaration.RunPreSaveValidation();
			var gst = invoiceLine.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertEquals(69m, gst.C1_Amount);

			invoiceHeader.ApplyCLVSRemissionMexicoAndUSDutyAndTax();
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals(0m, gst.C1_Amount);
			AssertEquals(true, invoiceHeader.IsRemissionMexicoAndUSDutyAndTax());

			invoiceHeader.CA_RN_NKExport = "AU";
			AssertEquals(CalculationMethods.Codes.NoRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals(ZString.Empty, invoiceLine.CA_AuthorityNumber);
			AssertEquals(ZString.Empty, invoiceLine.CA_99TariffCode);
		}

		public void TestReApplyCLVSRemissionMexicoAndUSDutyOnlyAfterNoRemission()
		{
			PrepareData();
			(JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine) = CreateDeclaration(Core.Constants.CountryCodes.UnitedStates);
			invoiceLine.DutyAndTaxManager.PopulateDutiesAndTaxes();
			AssertRemissionAmounts(invoiceLine, 180, 0, 259.6m, 59m, 1180m, true);

			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			declaration.RunPreSaveValidation();
			var gst = invoiceLine.DutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST);
			AssertEquals(69m, gst.C1_Amount);

			invoiceHeader.ApplyCLVSRemissionMexicoAndUSDutyOnly();
			AssertEquals(CalculationMethods.Codes.RegularRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals(59m, gst.C1_Amount);
			AssertEquals(true, invoiceHeader.IsRemissionMexicoAndUSDutyOnly());

			invoiceHeader.CA_RN_NKExport = "AU";
			AssertEquals(CalculationMethods.Codes.NoRemission, invoiceLine.CA_CalculationMethod);
			AssertEquals(ZString.Empty, invoiceLine.CA_AuthorityNumber);
			AssertEquals(ZString.Empty, invoiceLine.CA_99TariffCode);
		}

		void AssertRemissionAmounts(JobComInvoiceLine invoiceLine, decimal simaAmount, decimal dutyAmount, decimal exciseTaxAmount, decimal gstAmount, decimal valueForTax, bool simaDutyOverride)
		{
			ActiveBusinessObjectCollection.RefreshAll(Factory);
			AssertEquals("DutiesAndTaxes count", 3, invoiceLine.DutiesAndTaxes.Count);
			invoiceLine.DutiesAndTaxes.ApplySort(DutyAndTax.Schema.C1_TaxType, ListSortDirection.Ascending);
			AssertTax(invoiceLine.DutiesAndTaxes[0], false, DutyAndTaxTypes.Codes.CVD, ZString.Empty, simaDutyOverride, RateTypes.Codes.Specific, 200, "KGM", simaAmount);
			AssertTax(invoiceLine.DutiesAndTaxes[1], false, DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, false, RateTypes.Codes.AdValorem, 20, ZString.Empty, dutyAmount);
			AssertTax(invoiceLine.DutiesAndTaxes[2], false, DutyAndTaxTypes.Codes.GST, "001", false, RateTypes.Codes.AdValorem, 5, ZString.Empty, gstAmount);

			AssertEquals("NormalValueForTax", valueForTax, invoiceLine.DutyAndTaxManager.NormalValueForTax);
		}

		void AssertTax(DutyAndTax tax, bool isInRefFiles, ZString taxType, ZString code, bool ovr, ZString rateType, ZDecimal rate, ZString unitOfMeasure, ZDecimal amount)
		{
			CombineAssertions(() =>
			{
				AssertEquals("IsInRefFiles", isInRefFiles, tax.IsInRefFiles);
				AssertEquals("C1_TaxType", taxType, tax.C1_TaxType);
				AssertEquals("C1_Code", code, tax.C1_Code);
				AssertEquals("C1_Override", ovr, tax.C1_Override);
				AssertEquals("C1_RateType", rateType, tax.C1_RateType);
				AssertEquals("C1_Rate", rate, tax.C1_Rate);
				AssertEquals("C1_UnitOfMeasure", unitOfMeasure, tax.C1_UnitOfMeasure);
				AssertEquals("C1_Amount", amount, tax.C1_Amount);
			});
		}

		#region PrepareRefFiles

		void FillRateLine(CACRateLine rateLine, ZString rateType, ZDecimal min, ZDecimal regular, ZDecimal max)
		{
			rateLine.ZR_DutyRateMax = max;
			rateLine.ZR_DutyRateMin = min;
			rateLine.ZR_DutyRateRegular = regular;
			rateLine.ZR_DutyRateType = rateType;
		}

		void FillRefNumber(CACTaxRefNumber refNumber, ZString gstRefNum, ZString exciseRefNum)
		{
			refNumber.ZE_GSTRefNumber = gstRefNum;
			refNumber.ZE_ExciseTaxRefNumber = exciseRefNum;
		}

		void FillTaxRate(CACTaxRate taxRate, ZString refNumber, ZString unitOfMeasure, ZString title, ZString rateType, ZDecimal rate)
		{
			taxRate.ZH_TaxRefNumber = refNumber;
			taxRate.ZH_UnitOfMeasure = unitOfMeasure;
			taxRate.ZH_Title = title;
			taxRate.ZH_RateType = rateType;
			taxRate.ZH_Rate = rate;
			taxRate.ZH_EffectiveDate = effectiveDate;
			taxRate.ZH_ExpiryDate = expiryDate;
		}

		CACClassHeader GetClassHeader(string classificationNumber)
		{
			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = classificationNumber;
			classHeader.ZA_EffectiveDate = effectiveDate;
			classHeader.ZA_ExpiryDate = expiryDate;
			classHeader.ZA_AreaCode = "900";
			return classHeader;
		}

		void PrepareRefFiles()
		{
			#region Populate Class Header

			#region Populate Class Rates

			#region Class Rate 1

			var classHeader1 = GetClassHeader(classificationNumber1);

			var classRate = classHeader1.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_UnitOfMeasure = customsUnits1;

			var rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0.7, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 19, 0);

			rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0.0234, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 5, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0, 0.0948);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0.04740, 0, 0);

			rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.Chile;

			#endregion

			#region Class Rate 2

			var classHeader = GetClassHeader(classificationNumber4);

			classRate = classHeader.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_UnitOfMeasure = customsUnits2;
			classRate.ZB_FreeInd = true;

			#endregion

			#region Class Rate 3

			classHeader = GetClassHeader(classificationNumber5);

			classRate = classHeader.ClassRates.AddNew();
			classRate.ZB_EffectiveDate = effectiveDate;
			classRate.ZB_ExpiryDate = expiryDate;
			classRate.ZB_UnitOfMeasure = customsUnits1;

			rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.General;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 123, 0);

			rate = classRate.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.MostFavouredNation;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 8, 0);

			#endregion

			#endregion

			#region Populate Excise Duty Rates

			#region Excise Duty Rate 1

			classHeader = GetClassHeader(classificationNumber2);

			var exciseRate = classHeader.ExciseDutyRates.AddNew();
			exciseRate.ZB_EffectiveDate = effectiveDate;
			exciseRate.ZB_ExpiryDate = expiryDate;
			exciseRate.ZB_UnitOfMeasure = customsUnits1;

			rate = exciseRate.Rates.AddNew();
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Free, 0, 0, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AcceptX, 0, 0, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AcceptT, 0, 0, 0);

			#endregion

			#region Excise Duty Rate 3

			exciseRate = classHeader.ExciseDutyRates.AddNew();
			exciseRate.ZB_EffectiveDate = effectiveDate;
			exciseRate.ZB_ExpiryDate = effectiveDate;
			exciseRate.ZB_UnitOfMeasure = customsUnits3;

			rate = exciseRate.Rates.AddNew();
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 6.03, 0, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 165, 0);

			#endregion

			#endregion

			#region Populate Ref Numbers

			classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = classificationNumber3;
			classHeader.ZA_EffectiveDate = effectiveDate;
			classHeader.ZA_ExpiryDate = expiryDate;
			classHeader.ZA_AreaCode = "900";

			var refNumHeader = classHeader.RefNumbers.AddNew();
			refNumHeader.ZD_EffectiveDate = effectiveDate;
			refNumHeader.ZD_ExpiryDate = expiryDate;
			FillRefNumber(refNumHeader.RefNumbers.AddNew(), "001", "BB2");
			FillRefNumber(refNumHeader.RefNumbers.AddNew(), "AA1", "BB1");
			FillRefNumber(refNumHeader.RefNumbers.AddNew(), "AA3", "BB3");

			var exciseRates = new CACTaxRateCollection(Factory, CACTaxRate.TaxType.Excise);
			FillTaxRate(exciseRates.AddNew(), refNumHeader.RefNumbers[1].ZE_ExciseTaxRefNumber, customsUnits1, "Excise TAX 1", RateTypes.Codes.Exempt, 0);
			FillTaxRate(exciseRates.AddNew(), refNumHeader.RefNumbers[0].ZE_ExciseTaxRefNumber, customsUnits1, "Excise TAX 2", RateTypes.Codes.AdValorem, 22);
			FillTaxRate(exciseRates.AddNew(), refNumHeader.RefNumbers[2].ZE_ExciseTaxRefNumber, customsUnits3, "Excise TAX 3", RateTypes.Codes.AcceptT, 12);
			exciseRates[2].ZH_Inactive = true;

			#endregion

			#endregion

			#region  Populate Tariff Header

			#region Tariff 1

			var tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode1;
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = currentDate;
			tariffHeader.ZF_RateEffectiveDate = effectiveDate;
			tariffHeader.ZF_RateExpiryDate = currentDate;

			rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Free, 0, 0, 0);

			rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0.352, 0);

			#endregion

			#region Tariff 2

			tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode2;
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = expiryDate;
			tariffHeader.ZF_RateEffectiveDate = effectiveDate;
			tariffHeader.ZF_RateExpiryDate = effectiveDate;

			rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Specific, 0, 0.66, 0);
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 20, 0);

			#endregion

			#region Tariff 3

			tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode3;
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = expiryDate;
			tariffHeader.ZF_FreeInd = true;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.Free, 0, 0, 0);

			#endregion

			#region Tariff 4

			tariffHeader = Factory.New<CACTariffHeader>();
			tariffHeader.ZF_TariffCode = tariffCode4;
			tariffHeader.ZF_AuthEffectiveDate = effectiveDate;
			tariffHeader.ZF_AuthExpiryDate = expiryDate;
			tariffHeader.ZF_RateEffectiveDate = effectiveDate;
			tariffHeader.ZF_RateExpiryDate = expiryDate;

			rate = tariffHeader.Rates.AddNew();
			rate.ZC_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;
			FillRateLine(rate.RateLines.AddNew(), RateTypes.Codes.AdValorem, 0, 22, 0);

			#endregion

			#endregion
		}

		#endregion

		#region Implementation

		void PrepareData()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var caDataGrouping = universalHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			var taxOrFee1 = universalHelper.CreateTaxOrFee("RT1", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 10000m);
			var taxOrFee2 = universalHelper.CreateTaxOrFee("RT2", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 10000m);
			var taxOrFee3 = universalHelper.CreateTaxOrFee("RT3", 0m, Core.Constants.CountryCodes.Canada, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), threshold: 10000m);
			Factory.Save();

			var chinaTradeGroup = universalHelper.CreateTradeGroup(Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.China, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.AddCountry(chinaTradeGroup, Core.Constants.CountryCodes.China, ZDate.Today.AddYears(-1), ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var antiDumpingRateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.AntiDumping);
			var antiDumpingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.ADD, antiDumpingRateType.PK);
			var surTaxRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.SUR, antiDumpingRateType.PK);
			var countervailingRateCode = universalHelper.LoadOrCreateNewCusRateCode(Factory, DutyAndTaxTypes.Codes.CVD, antiDumpingRateType.PK);
			Factory.Save();
			var surtaxTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "1234567890", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(surtaxTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM");
			var surTaxRate = universalHelper.CreateRate(surtaxTariff, surTaxRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "0.1 * VFD");
			var surTaxApplicability = universalHelper.CreateCusApplicability(surTaxRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0123456789", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "0123456789");
			var antiDumpingRate = universalHelper.CreateRate(antiDumpingTariff, antiDumpingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "100.5 * [NMB]");
			var antiDumpingApplicability = universalHelper.CreateCusApplicability(antiDumpingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			countervailingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0789456123", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(countervailingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TNE");
			var countervailingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1408", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, relatedTariffCode: "0789456123");
			var countervailingRelationShip = universalHelper.CreateTariffRelationship(countervailingTariff.PK, harmonizedTariffType.PK, "0789456123");
			var countervailingRate = universalHelper.CreateRate(countervailingTariff, countervailingRateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime, rateFormula: "200 * [KGM]");
			var countervailingApplicability = universalHelper.CreateCusApplicability(countervailingRate, chinaTradeGroup, ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);

			universalHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "CAGSTRateCodes", Core.Constants.CountryCodes.Canada);
			universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "DESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Core.Constants.CountryCodes.Canada);
			universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, "DESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, Core.Constants.CountryCodes.Canada);
			var gst1 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "001", "001 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "5.00");
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, RateTypes.Codes.AdValorem);
			var gst2 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "AA1", "AA1 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "12.00");
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, RateTypes.Codes.Specific);
			var gst3 = universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CAGSTRateCodes, "AA3", "AA3 DESC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate, "18.00");
			universalHelper.CreateNewOrGetExistingCusCodeListAttribute(gst3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType, RateTypes.Codes.AcceptX);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("CA");
			CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.DefaultGeneralRateOfDuty.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DecimalEffectiveDate { NewValue = 20, PreviousValue = 20, EffectiveDate = effectiveDate });
			PrepareRefFiles();
		}

		(JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine) CreateDeclaration(ZString exportCountry)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Postal;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			invoiceHeader.CA_TimeLimit = 3;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "CAD";
			invoiceHeader.JZ_InvoiceCurrExRate = 10;
			invoiceHeader.CA_RN_NKExport = exportCountry;
			invoiceHeader.CA_TreatmentCode = TariffTreatmentCodes.Codes.NewZealand;

			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.CA_ADJValue = 10;
			invoiceLine.CA_CVforCurrConv = 20;
			invoiceLine.CA_CVforCurrConvOvr = true;
			invoiceLine.JI_NetWeight = 1;
			invoiceLine.JI_CustomsSecondQuantity = 7;
			invoiceLine.JI_CustomsSecondUnitQty = customsUnits2;
			invoiceLine.JI_CustomsThirdQuantity = 60;
			invoiceLine.JI_CustomsThirdUnitQty = customsUnits3;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.CA_CustomsValueOvr = true;
			invoiceLine.CA_CustomsValue = 1000;
			invoiceLine.CA_99TariffCode = ZString.Empty;
			invoiceLine.JI_Tariff = classificationNumber3;
			invoiceLine.JI_CustomsQuantity = 10;
			invoiceLine.JI_CustomsUnitQty = customsUnits1;
			invoiceLine.DutiesAndTaxes.DeleteAll();

			var tax = invoiceLine.DutiesAndTaxes.AddNew();
			tax.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			tax.C1_Override = true;
			tax.C1_ExemptCode = SIMACodes.Codes.C31;
			tax.C1_Rate = 200m;
			tax.C1_RateType = RateTypes.Codes.Specific;
			tax.C1_UnitOfMeasure = "KGM";
			tax.C1_Amount = 180;
			invoiceLine.CA_CalculationMethod = CalculationMethods.Codes.RegularRemission;

			return (declaration, invoiceHeader, invoiceLine);
		}

		TariffView countervailingRelTariff;
		readonly ZDateTime currentDate = ZDateTime.Today;
		readonly ZDateTime effectiveDate = ZDateTime.Today.AddDays(-1);
		readonly ZDateTime expiryDate = ZDateTime.Today.AddDays(1);
		const string customsUnits1 = CustomsUnitOfMeasureList.Codes.Kilogram;
		const string customsUnits2 = CustomsUnitOfMeasureList.Codes.Gram;
		const string customsUnits3 = CustomsUnitOfMeasureList.Codes.Litre;
		const string classificationNumber1 = "1234567890";
		const string classificationNumber2 = "0987654321";
		const string classificationNumber3 = "0789456123";
		const string classificationNumber4 = "4567891230";
		const string classificationNumber5 = "1223123400";
		const string tariffCode1 = "4901";
		const string tariffCode2 = "4902";
		const string tariffCode3 = "4903";
		const string tariffCode4 = "9905";

		#endregion
	}
}
