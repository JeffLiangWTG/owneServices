using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRuleWrapper))]
	public class ComplianceRuleWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRulesAndCountries()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "AU");
			var newRule = Factory.New<ComplianceRule>();
			newRule.CRU_Origin = "AU";
			newRule.CRU_Destination = "CN";
			newRule.CRU_HarmonizedCode = "123456";

			var rules = new ComplianceRuleCandidateCollection();
			rules.Add(newRule);

			var countries = new ComplianceRuleTargetCountryCollection();
			countries.Add(country);

			var wrapper = new ComplianceRuleWrapper(rules, countries);
			AssertEquals(rules, wrapper.Rules);
			AssertEquals(countries, wrapper.Countries);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, "AU");
			var newRule = Factory.New<ComplianceRule>();
			newRule.CRU_Origin = "AU";
			newRule.CRU_Destination = "CN";
			newRule.CRU_HarmonizedCode = "123456";

			var rules = new ComplianceRuleCandidateCollection();
			rules.Add(newRule);

			var countries = new ComplianceRuleTargetCountryCollection();
			countries.Add(country);

			return new ComplianceRuleWrapper(rules, countries);
		}
	}
}
