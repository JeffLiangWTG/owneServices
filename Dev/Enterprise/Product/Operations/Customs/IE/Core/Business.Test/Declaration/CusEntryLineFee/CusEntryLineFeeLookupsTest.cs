using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFeeLookups))]
sealed class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMethodOfPaymentList()
	{
		AssertEquals("A, E, J, M", CusEntryLineFeeTest.NewTestObjects(Factory).fee.Lookups.MethodOfPaymentList.CodesAsString);
	}

	public void TestMethodOfPaymentListIsCached()
	{
		var methodOfPaymentList = CusEntryLineFeeTest.NewTestObjects(Factory).fee.Lookups.MethodOfPaymentList;
		AssertSame(methodOfPaymentList, CusEntryLineFeeTest.NewTestObjects(Factory).fee.Lookups.MethodOfPaymentList);
		Factory.ClearCachedValue<PaymentMethodList>();
		AssertEquals(false, ReferenceEquals(methodOfPaymentList, CusEntryLineFeeTest.NewTestObjects(Factory).fee.Lookups.MethodOfPaymentList));
	}

	public void TestNationalFeeTypeCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");

		var rateType_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.Duty);
		var rateType_ADD = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty);
		var rateType_CVD = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty);
		var rateType_EXC = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.Excise);
		var rateType_EXP = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.ExportDuty);
		var rateType_INT = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.Interest);
		var rateType_LEV = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.Levies);
		var rateType_MSC = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.Miscellaneous);
		var rateType_SEC = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.SecurityDeposit);
		var rateType_VAT = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.VAT);

		// DTY
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.CustomsDutiesOnIndustrialProducts, rateType_DTY.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.CustomsAdditionalDuties, rateType_DTY.PK, countryCode: Core.Constants.CountryCodes.Ireland);

		// ADD
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.DefinitiveAntidumpingDuties, rateType_ADD.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.ProvisionalAntidumpingDuties, rateType_ADD.PK, countryCode: Core.Constants.CountryCodes.Ireland);

		// CVD
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.DefinitiveCountervailingDuties, rateType_CVD.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.ProvisionalCountervailingDuties, rateType_CVD.PK, countryCode: Core.Constants.CountryCodes.Ireland);

		// EXC
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.ExciseCarbonTax, rateType_EXC.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.ExciseMineralOil, rateType_EXC.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.Excises, rateType_EXC.PK, countryCode: Core.Constants.CountryCodes.Ireland);

		// SEC
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.SecuritiesOnDuties, rateType_SEC.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.OtherDutiesValuationDeposits, rateType_SEC.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.QuotaDeposit, rateType_SEC.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.SugarAndPoultryDeposits, rateType_SEC.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.SecuritiesForEndUse, rateType_SEC.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.Securities_Other, rateType_SEC.PK, countryCode: Core.Constants.CountryCodes.Ireland);

		// VAT
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.VatOnSecurities, rateType_VAT.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.DeferredVAT, rateType_VAT.PK, countryCode: Core.Constants.CountryCodes.Ireland);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.SpecialArrangementsVAT, rateType_VAT.PK, countryCode: Core.Constants.CountryCodes.Ireland);

		Factory.Save();

		var nationalFeeTypeCodeList = CusEntryLineFeeTest.NewTestObjects(Factory).fee.Lookups.NationalFeeTypeCodeList;
		AssertContainsExactElementsInAnyOrder("NationalFeeTypeCodeList should get correct RateCode",
			new[] {
				UniversalReferenceConstants.IERefCusRateCodes.VatOnSecurities,
				UniversalReferenceConstants.IERefCusRateCodes.DeferredVAT,
				UniversalReferenceConstants.IERefCusRateCodes.SpecialArrangementsVAT,
				UniversalReferenceConstants.IERefCusRateCodes.SecuritiesOnDuties,
				UniversalReferenceConstants.IERefCusRateCodes.ExciseCarbonTax,
				UniversalReferenceConstants.IERefCusRateCodes.OtherDutiesValuationDeposits,
				UniversalReferenceConstants.IERefCusRateCodes.QuotaDeposit,
				UniversalReferenceConstants.IERefCusRateCodes.SugarAndPoultryDeposits,
				UniversalReferenceConstants.IERefCusRateCodes.SecuritiesForEndUse,
				UniversalReferenceConstants.IERefCusRateCodes.Excises,
				UniversalReferenceConstants.IERefCusRateCodes.Securities_Other,
				UniversalReferenceConstants.IERefCusRateCodes.ExciseMineralOil,
				UniversalReferenceConstants.IERefCusRateCodes.CustomsDutiesOnIndustrialProducts,
				UniversalReferenceConstants.IERefCusRateCodes.CustomsAdditionalDuties,
				UniversalReferenceConstants.IERefCusRateCodes.DefinitiveAntidumpingDuties,
				UniversalReferenceConstants.IERefCusRateCodes.ProvisionalAntidumpingDuties,
				UniversalReferenceConstants.IERefCusRateCodes.DefinitiveCountervailingDuties,
				UniversalReferenceConstants.IERefCusRateCodes.ProvisionalCountervailingDuties,
				EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
	},
			nationalFeeTypeCodeList.GetAllCodes()
		);

		AssertSame("NationalFeeTypeCodeList should have been cached in Factory.", nationalFeeTypeCodeList, CusEntryLineFeeTest.NewTestObjects(Factory).fee.Lookups.NationalFeeTypeCodeList);
	}
}
