using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class UniversalReferenceTestDataHelper : Universal.Testing.UniversalReferenceTestDataHelper
	{
		public UniversalReferenceTestDataHelper(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void CreateNctsDeclarationTypeList(string countryOrDataGrouping = Core.Constants.CountryCodes.Latvia)
		{
			var eurpoeanUnionCode = RefDataGroupingCodes.EuropeanUnionEUN;
			var nctsDeclarationTypeCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType;
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);

			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(countryOrDataGrouping, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(nctsDeclarationTypeCode, "NCTS Declaration Type (Box 1)");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, nctsDeclarationTypeCode, "T1", "Goods moving under external Community transit procedure", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, nctsDeclarationTypeCode, "T2", "Goods moving under internal Community transit procedure", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, nctsDeclarationTypeCode, "T-", "Mixed consignment of T1 and T2 goods", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, nctsDeclarationTypeCode, "T2F", "Goods moving under internal Community transit procedure between different fiscal territories", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, nctsDeclarationTypeCode, "TIR", "TIR declaration", startDate, endDate);
		}

		public void CreateCountryListOfAU_DE_FR(string codeType, string countryOrDataGrouping = Core.Constants.CountryCodes.Latvia)
		{
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var eurpoeanUnionCode = RefDataGroupingCodes.EuropeanUnionEUN;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(countryOrDataGrouping, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CountryList – NCTS");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);
		}

		public void CreateCountryListOfAU_DE_FR_ForCountryAndEU(string codeType, string countryOrDataGrouping = Core.Constants.CountryCodes.Latvia)
		{
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var eurpoeanUnionCode = RefDataGroupingCodes.EuropeanUnionEUN;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(countryOrDataGrouping, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CountryList – NCTS");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, codeType, Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(countryOrDataGrouping, codeType, Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(countryOrDataGrouping, codeType, Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(countryOrDataGrouping, codeType, Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);
		}

		public static void RunAssertionsInPhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertionWithHtml.CombineAssertions(assertions);
			}
		}

		public static void RunAssertionsOutsidePhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertionWithHtml.CombineAssertions(assertions);
			}
		}

		public static void EnsureCountriesAreInRefCusCodeList(BusinessObjectFactory factory, string refCusCodeListTypeCode, params (string countryCode, string countryName)[] countries)
		{
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var eurpoeanUnionCode = RefDataGroupingCodes.EuropeanUnionEUN;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var cachekey = RefCusCodeListTypes.GetKey(
				country: eurpoeanUnionCode,
				codeType: refCusCodeListTypeCode,
				date: ZDateTime.Today.Date,
				transportMode: "",
				includeParentDataGrouping: true,
				attributes: null,
				attributeNameValuePairs: null,
				matchIfAttributeNotExists: false,
				attributeName: null,
				attributeValues: null,
				notExistAttributeNameValuePairs: null,
				languageCode: "",
				onlyThisLanguage: false);
			factory.ClearCachedValue<ZArchitecture.Core.CodeDescriptionPairList>("ZZRefCusCodeList_" + cachekey);

			helper.CreateNewOrGetExistingCusCodeType(refCusCodeListTypeCode, "CountryList – NCTS");
			foreach ((string countryCode, string countryName) in countries)
			{
				helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, refCusCodeListTypeCode, countryCode, countryName, startDate, endDate);
			}
		}
	}
}
