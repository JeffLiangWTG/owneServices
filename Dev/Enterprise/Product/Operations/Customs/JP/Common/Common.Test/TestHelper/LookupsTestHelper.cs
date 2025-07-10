using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	public static class LookupsTestHelper
	{
		public static void AssertFilterBusinessObjectDefault(this BusinessObjectCollection collection, string filterDescription, string expectedComparisonOperator, string expectedValue = "", bool expectedIsRemovable = true)
		{
			var filter = collection.FilterBusinessObjectDefaults[filterDescription];
			AssertFilter(filter, filterDescription, expectedComparisonOperator, expectedValue, expectedIsRemovable);
		}

		public static void AssertFilterBusinessObjectDefault<ElementType>(this ActiveBusinessObjectCollection<ElementType> collection, string filterDescription, string expectedComparisonOperator, string expectedValue = "", bool expectedIsRemovable = true) where ElementType : BusinessObject
		{
			var filter = collection.FilterBusinessObjectDefaults[filterDescription];
			AssertFilter(filter, filterDescription, expectedComparisonOperator, expectedValue, expectedIsRemovable);
		}

		public static void AssertFilter(FilterBusinessObjectDefault filter, string filterDescription, string expectedComparisonOperator, string expectedValue = "", bool expectedIsRemovable = true)
		{
			Assertion.AssertNotNull($"Filter for {filterDescription} should not be null", filter);

			var filterName = filter.FilterName;
			Assertion.Assert($"FilterName:{filterName}, ComparisonOperator should be {expectedComparisonOperator}, but is {filter.ComparisonOperator}", expectedComparisonOperator.Equals(filter.ComparisonOperator));
			Assertion.AssertEquals($"FilterName:{filterName}, Value should be {expectedValue}, but is {filter.Value}", expectedValue, filter.Value);
			Assertion.AssertEquals($"FilterName:{filterName}, IsRemovable should be {expectedIsRemovable}, but is {filter.IsRemovable}", expectedIsRemovable, filter.IsRemovable);
		}

		public static void AssertFilterBusinessObjectDefaultNotContain<ElementType>(this ActiveBusinessObjectCollection<ElementType> collection, string filterDescription) where ElementType : BusinessObject
		{
			Assertion.Assert($"Should not contain filter:{filterDescription}", !collection.FilterBusinessObjectDefaults.ContainsDefaultFor(filterDescription));
		}
	}
}
