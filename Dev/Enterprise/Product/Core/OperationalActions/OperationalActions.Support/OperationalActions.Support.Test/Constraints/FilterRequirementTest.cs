using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	internal sealed class FilterRequirementTest : TestCase
	{
		public void TestAdd()
		{
			AssertValues("nothing added");
			Requirement.Add("AU");
			AssertValues("added AU", "AU");
			Requirement.Add("NZ");
			AssertValues("added NZ", "AU", "NZ");
			Requirement.Add("AU");
			AssertValues("re-added AU", "AU", "NZ");
		}

		public void TestAddRange_Array()
		{
			Requirement.AddRange(new string[] { "GB", "SG", "HK", "DE" });
			AssertValues("added set1", "DE", "GB", "HK", "SG");
			Requirement.AddRange(new string[] { "NZ", "AU", "CH", "SG", "AU" });
			AssertValues("added set2", "AU", "CH", "DE", "GB", "HK", "NZ", "SG");
		}

		public void TestAddRange_Requirement()
		{
			FilterRequirement r1 = new FilterRequirement(FilterConstants.Country);
			r1.AddRange(new string[] { "GB", "SG", "HK", "DE" });
			FilterRequirement r2 = new FilterRequirement(FilterConstants.Country);
			r2.AddRange(new string[] { "NZ", "AU", "CH", "SG", "AU" });
			Requirement.AddRange(r1);
			AssertValues("added set1", "DE", "GB", "HK", "SG");
			Requirement.AddRange(r2);
			AssertValues("added set2", "AU", "CH", "DE", "GB", "HK", "NZ", "SG");
		}

		public void TestRevome()
		{
			Requirement.AddRange(new string[] { "GB", "SG", "HK", "DE" });
			AssertValues("pre-remove", "DE", "GB", "HK", "SG");
			Requirement.Remove("HK");
			AssertValues("post-remove", "DE", "GB", "SG");
		}

		public void TestContains()
		{
			Requirement.AddRange(new string[] { "GB", "SG", "HK", "DE" });
			AssertEquals("GB", true, Requirement.Contains("GB"));
			AssertEquals("SG", true, Requirement.Contains("SG"));
			AssertEquals("HK", true, Requirement.Contains("HK"));
			AssertEquals("DE", true, Requirement.Contains("DE"));
			AssertEquals("AU", false, Requirement.Contains("AU"));
		}

		public void TestOverlaps()
		{
			FilterRequirement r1 = new FilterRequirement(FilterConstants.Country);
			r1.AddRange(new string[] { "DE", "GB", "HK", "SG" });
			FilterRequirement r2 = new FilterRequirement(FilterConstants.Country);
			r2.AddRange(new string[] { "AU", "CH", "GB", "NZ" });
			FilterRequirement r3 = new FilterRequirement(FilterConstants.Country);
			r3.AddRange(new string[] { "AU", "CH", "NZ" });
			AssertEquals("r1 & r2", true, r1.Overlaps(r2));
			AssertEquals("r1 & r3", false, r1.Overlaps(r3));
		}

		public void TestIsSuperSetOf()
		{
			FilterRequirement r1 = new FilterRequirement(FilterConstants.Country);
			r1.AddRange(new string[] { "AU", "CH", "NZ" });
			FilterRequirement r2 = new FilterRequirement(FilterConstants.Country);
			r2.AddRange(new string[] { "AU", "CH", "GB", "NZ" });
			AssertEquals("r1 >= r2", false, r1.IsSuperSetOf(r2));
			AssertEquals("r2 >= r1", true, r2.IsSuperSetOf(r1));
			AssertEquals("r1 >= r1", true, r1.IsSuperSetOf(r1));
		}

		#region Implementation
		void AssertValues(string message, params string[] expectedValues)
		{
			AssertContainsExactElementsInAnyOrder(message, expectedValues, Requirement);
		}

		FilterRequirement Requirement
		{
			get
			{
				return requirement ?? (requirement = new FilterRequirement("Name"));
			}
		}

		FilterRequirement requirement;
		#endregion
	}
}
