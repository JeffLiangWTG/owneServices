using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	sealed class FilterRequirementsTest : TestCase
	{
		public void TestAddCountries()
		{
			Requirements.Add(FilterConstants.Country, new string[] { "GB", "SG", "HK", "DE" });
			AssertCountries("Added Set", "DE", "GB", "HK", "SG");
		}

		public void TestAddCountriesAndDepartments()
		{
			var expectedAddedBranches = new string[] { "BRN" };
			Requirements.Add(FilterConstants.Country, ["GB", "SG", "HK", "DE"]);
			Requirements.Add(FilterConstants.Department, expectedAddedBranches);
			AssertCountries("Added Set", "DE", "GB", "HK", "SG");
			AssertContainsExactElementsInAnyOrder("Added Set - Department", expectedAddedBranches, Requirements[FilterConstants.Department]);
		}

		public void TestAddRange()
		{
			Requirements.Add(FilterConstants.Country, new string[] { "GB", "SG", "HK", "DE" });
			var filterRequirement = new FilterRequirement(FilterConstants.Country);
			filterRequirement.AddRange(new string[] { "NZ", "AU", "CH", "SG", "AU", "DE" });
			Requirements.Add(filterRequirement);
			AssertCountries("added set3", "AU", "CH", "DE", "GB", "HK", "NZ", "SG");
		}

		void AssertCountries(string message, params string[] expectedValues)
		{
			AssertContainsExactElementsInAnyOrder(message + " - Country", expectedValues, Requirements[FilterConstants.Country]);
		}

		FilterRequirementList requirements;
		FilterRequirementList Requirements => requirements ?? (requirements = new FilterRequirementList());
	}
}
