using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CusAuthorisationRuleLookups))]
sealed class CusAuthorisationRuleLookupsTest : Customs.Business.Testing.CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
{
	public void TestRuleCodeListDefault()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		AssertEquals("LOC", lookups.RuleCodeList.CodesAsString);
	}

	public void TestRuleValueListForRuleCode_UnknownType()
	{
		var lookups = CusAuthorisationRuleLookupsForTesting();
		lookups.Parent.CPR_RuleCode = "AAA";
		AssertEquals(ZString.Empty, ((CodeDescriptionPairList)lookups.ValueList).CodesAsString);
	}

	public void TestValueList_LOC()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "BEFAC", "BE Facility", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		CombineAssertions(() =>
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			var cusAuthorisationRule = lookups.Parent;
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			AssertType<ZZRefCusCodeListCombinedCollection>("LOC'-Lookup-Type", lookups.ValueList);

			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			var valueListLOC = lookups.ValueList as ZZRefCusCodeListCombinedCollection;
			valueListLOC.Load();
			AssertContainsExactElementsInAnyOrder("Belgian Codes", new[] { "BEFAC" }, valueListLOC.Select(x => x.ZZD_Code));

			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			valueListLOC = lookups.ValueList as ZZRefCusCodeListCombinedCollection;
			valueListLOC.Load();
			AssertEquals("No German Codes", false, valueListLOC.Any());
		});
	}

	protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting()
	{
		var cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
		cusAuthorisationRule.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
		cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		return new CusAuthorisationRuleLookups(cusAuthorisationRule);
	}
}
