using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing.CusAuthorisations
{
	[TestedType(typeof(CusAuthorisationRuleLookups))]
	sealed class CusAuthorisationRuleLookupsTest : Customs.Business.Testing.CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
	{
		public void TestValueList()
		{
			CombineAssertions(() =>
			{
				var lookups = CusAuthorisationRuleLookupsForTesting();
				var cusAuthorisationRule = lookups.Parent;
				cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertType<RefUNLOCOCollection>("Lookup List", lookups.ValueList);
			});
		}

		protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting()
		{
			var cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			return new CusAuthorisationRuleLookups(cusAuthorisationRule);
		}
	}
}
