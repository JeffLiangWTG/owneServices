using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CusAuthorisationRuleLookups))]
	public class CusAuthorisationRuleLookupsTest : Customs.Business.Testing.CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
	{
		public void TestValueFromList()
		{
			cusAuthorisationRule.CPR_RuleCode = GBCusAuthorisationRuleTypeList.Codes.ORG;
			AssertType<OrgHeaderCollection>(cusAuthorisationRule.Lookups.ValueList);

			cusAuthorisationRule.CPR_RuleCode = GBCusAuthorisationRuleTypeList.Codes.CTY;
			AssertType<GBCusAuthorisationCountryCodePrefixList>(cusAuthorisationRule.Lookups.ValueList);
		}

		public void TestRuleCodeListType()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			AssertType<GBCusAuthorisationRuleTypeBaseList>(lookups.RuleCodeList);

			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			AssertType<GBCusAuthorisationRuleTypeList>(cusAuthorisationRule.Lookups.RuleCodeList);

			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			AssertType<GBCusAuthorisationRuleTypeList>(cusAuthorisationRule.Lookups.RuleCodeList);
		}

		protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting()
		{
			var cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			return new CusAuthorisationRuleLookups(cusAuthorisationRule);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationHeader = cusAuthorisationRule.AuthorisationHeader;
			cusAuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
		}
		CusAuthorisationRule cusAuthorisationRule;
		CusAuthorisationHeader cusAuthorisationHeader;
	}
}
