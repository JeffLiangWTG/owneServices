using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
//using Modules;
//using static ModuleFilterWithListAndComparisonOperators<ZString>;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class ModuleFilterWithListAndComparisonOperatorBaseTest<T> : TestCaseWithFactory where T : IZType
	{
		public void TestOptimisedExactComparison()
		{
			var test = GetExactComparisonTest();

			Assert($"GIVEN multiple same filter-strips with '{ModuleTextFilter.ComparisonConstants.Exact}'", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<T>>().Count(filter => filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact) > 1);

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);

			AssertEquals("WHEN calling GetCombinedFilter THEN should have 'IN', not 'OR'", test.ExpectedSQL, resultQuery.LiteralTextADO);
		}

		public void TestOptimisedExactComparison_InGroup()
		{
			var test = GetExactComparisonTest_InGroup();

			Assert($"GIVEN multiple same filter-strips with '{ModuleTextFilter.ComparisonConstants.Exact}'", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<T>>().Count(filter => filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact) > 1);
			Assert("GIVEN multiple same filter-strips in group", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<T>>().Count(filter => !filter.GroupName.IsEmpty) > 1);

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);

			AssertEquals("WHEN calling GetCombinedFilter THEN should have 'IN', not 'OR' for same group", test.ExpectedSQL, resultQuery.LiteralTextADO);
		}

		#region Implementation

		protected virtual ModuleFiltersAndExpectedSQLForTest GetExactComparisonTest()
		{
			throw new NotImplementedException();
		}

		protected virtual ModuleFiltersAndExpectedSQLForTest GetExactComparisonTest_InGroup()
		{
			throw new NotImplementedException();
		}

		#endregion

		protected ModuleFilterCombiner ModuleFilterCombiner => new ModuleFilterCombiner();
	}
}
