using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRuleCollection))]
	public class ComplianceRuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadComplianceRules()
		{
			var complianceRule1 = CreateComplianceRule(Constants.CountryCodes.Australia, Constants.CountryCodes.UnitedStates);
			var complianceRule2 = CreateComplianceRule(Constants.CountryCodes.Australia, string.Empty);
			CreateComplianceRule(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.NewZealand);

			var collection = new ComplianceRuleCollection(Factory);
			AssertEquals(0, collection.Count);

			collection.LoadComplianceRules(Constants.CountryCodes.Australia);
			AssertContainsExactElementsInAnyOrder(new[] { complianceRule1, complianceRule2 }, collection);

			var complianceRule3 = CreateComplianceRule(string.Empty, Constants.CountryCodes.Australia);
			collection.LoadComplianceRules(Constants.CountryCodes.Australia);
			AssertContainsExactElementsInAnyOrder(new[] { complianceRule1, complianceRule2, complianceRule3 }, collection);
			AssertEquals(Constants.CountryCodes.Australia, complianceRule1.CurrentCountryCode);
			AssertEquals(Constants.CountryCodes.Australia, complianceRule2.CurrentCountryCode);
			AssertEquals(Constants.CountryCodes.Australia, complianceRule3.CurrentCountryCode);

			collection = new ComplianceRuleCollection(Factory);
			collection.LoadComplianceRules(string.Empty);
			AssertEquals("Only load the data when country code is not empty", 0, collection.Count);
		}

		public void TestCountryCode()
		{
			var collection = new ComplianceRuleCollection(Factory);
			AssertEquals("", collection.CountryCode);

			var country = Factory.New<RefCountry>();
			country.Code = "UA";
			collection.LoadComplianceRules(country.RN_Code);
			AssertEquals("UA", collection.CountryCode);

			var newRule = collection.AddNew();
			AssertEquals("UA", newRule.CurrentCountryCode);
		}

		ComplianceRule CreateComplianceRule(string origin, string destination)
		{
			var rule = Factory.New<ComplianceRule>();
			rule.CRU_Origin = origin;
			rule.CRU_Destination = destination;
			return rule;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ComplianceRuleCollection(Factory);
		}
	}
}
