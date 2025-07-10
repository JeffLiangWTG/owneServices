using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	static class ModuleTestHelper
	{
		public static void AssertModuleTextFilterResult<T>(FilterStripBusinessObject moduleFilters, SQLComparisonOperator comparisonOperator, ZString value, BusinessObject[] expected, ZString filterConstants) where T : BusinessObject
		{
			var filter = (ModuleTextFilter)moduleFilters[filterConstants];
			filter.IsActive = true;
			filter.SqlComparisonOperator = comparisonOperator;
			filter.Property = value;

			var filteredResult = moduleFilters.Factory.Load<T>(moduleFilters.Filter);
			Assertion.AssertContainsExactElementsInAnyOrder($"{filterConstants} {comparisonOperator} {value}", expected.Select(x => x.PK), filteredResult.Select(x => x.PK));
		}

		public static void AssertModuleDateFilterResult<T>(FilterStripBusinessObject moduleFilters, ZString propertySearch, ZDateTime property1, ZDateTime property2, BusinessObject[] expected, ZString filterConstants) where T : BusinessObject
		{
			var filter = (ModuleDateFilter)moduleFilters[filterConstants];
			filter.IsActive = true;
			filter.PropertySearch = propertySearch;
			filter.Property1 = property1;
			filter.Property2 = property2;

			var filteredResult = moduleFilters.Factory.Load<T>(moduleFilters.Filter);
			Assertion.AssertContainsExactElementsInAnyOrder($"{filterConstants} {propertySearch} {property1} {property2}", expected.Select(x => x.PK), filteredResult.Select(x => x.PK));
		}

		public static void AssertModuleGuidFilterResult<T>(FilterStripBusinessObject moduleFilters, ZGuid value, BusinessObject[] expected, ZString filterConstants) where T : BusinessObject
		{
			var filter = (ModuleGuidFilter)moduleFilters[filterConstants];
			filter.IsActive = true;
			filter.Property = value;

			var filteredResult = moduleFilters.Factory.Load<T>(moduleFilters.Filter);
			Assertion.AssertContainsExactElementsInAnyOrder($"{filterConstants} {value}", expected.Select(x => x.PK), filteredResult.Select(x => x.PK));
		}

		public static void AssertModuleNkFilterResult<T>(FilterStripBusinessObject moduleFilters, SQLComparisonOperator comparisonOperator, string value, BusinessObject[] expected, ZString filterConstants) where T : BusinessObject
		{
			var filter = (ModuleNkFilter)moduleFilters[filterConstants];
			filter.IsActive = true;
			filter.SqlComparisonOperator = comparisonOperator;
			filter.Property = value;

			var filteredResult = moduleFilters.Factory.Load<T>(moduleFilters.Filter);
			Assertion.AssertContainsExactElementsInAnyOrder($"{filterConstants} {comparisonOperator} {value}", expected.Select(x => x.PK), filteredResult.Select(x => x.PK));
		}
	}
}
