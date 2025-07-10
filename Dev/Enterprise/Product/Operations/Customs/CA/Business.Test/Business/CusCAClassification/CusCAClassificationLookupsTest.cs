using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAClassificationLookupsTest : TestCaseWithFactory
	{
		public void TestTypeOfSimpeLookups()
		{
			CombineAssertions(() =>
			{
				var caClass = Factory.New<CusCAClassification>();
				caClass.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
				AssertEquals(typeof(CanadianProvinceList), caClass.Lookups.StatesOfOrigin.GetType());
				AssertEquals(typeof(USStatesList), caClass.Lookups.CFIAStatesOfOrigin.GetType());
				AssertEquals(typeof(CanadianProvinceList), caClass.Lookups.CanadianProvinces.GetType());
				AssertEquals(typeof(CACFIAEndUseCodesCollection), caClass.Lookups.CFIAEndUseCodes.GetType());
				AssertEquals(typeof(CACFIAMiscCodesCollection), caClass.Lookups.CFIAMiscIDCodes.GetType());
				AssertEquals(typeof(ValueForDutyCodes), caClass.Lookups.ValueForDutyCodes.GetType());
				AssertEquals(typeof(CodeDescriptionPairList), caClass.Lookups.TreatmentCodes.GetType());
				AssertEquals(typeof(ImportReasonCodes), caClass.Lookups.ImportReasonCodes.GetType());
				AssertEquals(typeof(GSTStatusCodes), caClass.Lookups.GSTStatusCodes.GetType());
				AssertEquals(typeof(ExciseTaxExemptionCodes), caClass.Lookups.ETExemptionCodes.GetType());
				AssertEquals(typeof(CodeDescriptionPairList), caClass.Lookups.ExciseTaxRateCodes.GetType());
				AssertEquals(typeof(Customs.Business.YesNoList), caClass.Lookups.CCA_PGAIndicatorList.GetType());
				AssertEquals(typeof(RefCurrencyCollection), caClass.Lookups.Currencies.GetType());
			});
		}

		public void TestExciseTaxRateCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, DutyAndTaxTypes.Codes.ExciseTax);
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "C01", refCusRateType.PK);
			var refCusRateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "C02", refCusRateType.PK);
			var refCusRateCode3 = helper.LoadOrCreateNewCusRateCode(Factory, "C03", refCusRateType.PK);

			var classHeader = Factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = "1234567890";
			classHeader.ZA_EffectiveDate = ZDateTime.Today.AddDays(-1);
			classHeader.ZA_ExpiryDate = ZDateTime.Today.AddDays(1);
			var refNumHeader = Factory.New<CACTaxRefNumHeader>();
			refNumHeader.ZD_ZA_ClassNumber = classHeader.PK;
			refNumHeader.ZD_EffectiveDate = ZDateTime.Today.AddDays(-1);
			refNumHeader.ZD_ExpiryDate = ZDateTime.Today.AddDays(1);

			var refNum1 = refNumHeader.RefNumbers.AddNew();
			refNum1.ZE_ExciseTaxRefNumber = "C04";
			var refNum2 = refNumHeader.RefNumbers.AddNew();
			refNum2.ZE_ExciseTaxRefNumber = "C05";

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			pivot.CI_TariffNum = classHeader.ZA_ClassificationNumber;
			var exciseTaxRateCodes = pivot.CAClassificationLookups.ExciseTaxRateCodes;
			AssertEquals(4, exciseTaxRateCodes.Count);
			Assert(exciseTaxRateCodes.Cast<ICodeDescription>().Any(x => x.Code == "C01"));
			Assert(exciseTaxRateCodes.Cast<ICodeDescription>().Any(x => x.Code == "C02"));
			Assert(exciseTaxRateCodes.Cast<ICodeDescription>().Any(x => x.Code == "NO"));
			Assert(exciseTaxRateCodes.Cast<ICodeDescription>().Any(x => x.Code == "C03"));
			Assert(!exciseTaxRateCodes.Cast<ICodeDescription>().Any(x => x.Code == "C04"));
			Assert(!exciseTaxRateCodes.Cast<ICodeDescription>().Any(x => x.Code == "C05"));
		}

		public void TestStatesOfOrigin()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals("CanadianProvinceList", typeof(CanadianProvinceList), pivot.CAClassificationLookups.StatesOfOrigin.GetType());
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("USStatesList", typeof(USStatesList), pivot.CAClassificationLookups.StatesOfOrigin.GetType());
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals("Generic list", typeof(CodeDescriptionPairList), pivot.CAClassificationLookups.StatesOfOrigin.GetType());

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals("CanadianProvinceList", typeof(CanadianProvinceList), pivot.CAClassificationLookups.StatesOfOrigin.GetType());
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("Still CanadianProvinceList", typeof(CanadianProvinceList), pivot.CAClassificationLookups.StatesOfOrigin.GetType());
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals("Still CanadianProvinceList", typeof(CanadianProvinceList), pivot.CAClassificationLookups.StatesOfOrigin.GetType());
		}

		public void TestTreatmentCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var constants = typeof(TariffTreatmentCodes.Codes).GetFields();
			foreach (var item in constants)
			{
				var code = item.GetValue(null).ToString();
				helper.CreatePreferenceForCountry(code, code, Core.Constants.CountryCodes.Canada);
			}
			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "02");
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedStates);
			tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Canada, "10");
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.UnitedStates);

			var pivot = Factory.New<CusClassPartPivot>();
			AssertEquals("All TTs should be valid for blank origin", 26, pivot.CAClassificationLookups.TreatmentCodes.Count);
			pivot.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("Should be 2 TTs valid for US", 2, pivot.CAClassificationLookups.TreatmentCodes.Count);

			var classification = Factory.New<CusClassification>();
			AssertEquals("All TTs should be valid for CusClassification", 26, classification.CAClassificationLookups.TreatmentCodes.Count);
			classification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("Should be 2 TTs valid for US", 2, classification.CAClassificationLookups.TreatmentCodes.Count);

			var caClassification = Factory.New<CusCAClassification>();
			AssertEquals("All TTs should be valid for CusClassification", 26, caClassification.Lookups.TreatmentCodes.Count);
			caClassification.CCA_RN_NKOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals("All TTs should be valid for CusClassification", 26, caClassification.Lookups.TreatmentCodes.Count);
		}

		public void TestThereIsNoExceptionWhenParentIsCusClassification()
		{
			var classification = Factory.New<CusClassification>();
			var caClassification = classification.Details;
			AssertNoExceptionThrown(() =>
			{
				_ = caClassification.Lookups.StatesOfOrigin;
				_ = caClassification.Lookups.ExciseTaxRateCodes;
			});
		}
	}
}
