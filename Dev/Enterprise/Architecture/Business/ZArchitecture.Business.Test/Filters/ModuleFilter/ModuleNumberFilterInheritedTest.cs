using System;
using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleNumberFilterInheritedTest : ModuleTextFilterBaseTest
	{
		public void TestMaxLength()
		{
			{
				var test = new ModuleFiltersAndExpectedSQLForTest()
				{
					ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = new String('a', 101), ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red },
				},
					ExpectedSQL = "Z0_Description = '" + new string('a', 100) + "'"
				};

				var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);
				AssertEquals("Correct SQL", test.ExpectedSQL, resultQuery.LiteralTextADO);
				var filter = (ModuleNumberFilter)test.ModuleFilters.First();
				filter.Validation.ValidateAll();
				AssertNoWarnings(filter.PropertyInfo);
			}

			{
				var test = new ModuleFiltersAndExpectedSQLForTest()
				{
					ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = new String('a', 101) + "," + new String('b', 101), ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red },
				},
					ExpectedSQL = "(Z0_Description in ('" + new string('a', 100) + "', '" + new string('b', 100) + "'))"
				};

				var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);
				AssertEquals("Correct SQL", test.ExpectedSQL, resultQuery.LiteralTextADO);
				var filter = (ModuleNumberFilter)test.ModuleFilters.First();
				filter.Validation.ValidateAll();
				AssertNoWarnings(filter.PropertyInfo);
			}
		}

		public void TestMultipleValues()
		{
			{
				var test = new ModuleFiltersAndExpectedSQLForTest()
				{
					ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "1,2,3", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red },
				},
					ExpectedSQL = "(Z0_Description in ('1', '2', '3'))"
				};

				var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);
				AssertEquals("Correct SQL", test.ExpectedSQL, resultQuery.LiteralTextADO);
				var filter = (ModuleNumberFilter)test.ModuleFilters.First();
				filter.Validation.ValidateAll();
				AssertNoWarnings(filter.PropertyInfo);
			}

			{
				var test = new ModuleFiltersAndExpectedSQLForTest()
				{
					ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "1,2,3", ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual, OrCategory = FilterOrCategory.Red },
				},
					ExpectedSQL = "(Z0_Description not in ('1', '2', '3'))"
				};

				var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);
				AssertEquals("Correct SQL", test.ExpectedSQL, resultQuery.LiteralTextADO);
				var filter = (ModuleNumberFilter)test.ModuleFilters.First();
				filter.Validation.ValidateAll();
				AssertNoWarnings(filter.PropertyInfo);
			}

			{
				var test = new ModuleFiltersAndExpectedSQLForTest()
				{
					ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "1,2,3", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red },
				},
					ExpectedSQL = "Z0_Description like '1%' or Z0_Description like '2%' or Z0_Description like '3%'"
				};

				var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);
				AssertEquals("Correct SQL", test.ExpectedSQL, resultQuery.LiteralTextADO);
				var filter = (ModuleNumberFilter)test.ModuleFilters.First();
				filter.Validation.ValidateAll();
				AssertNoWarnings(filter.PropertyInfo);
			}

			{
				var test = new ModuleFiltersAndExpectedSQLForTest()
				{
					ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "1,2,3", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains, OrCategory = FilterOrCategory.Red },
				},
					ExpectedSQL = "Z0_Description like '%1%' or Z0_Description like '%2%' or Z0_Description like '%3%'"
				};

				var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);
				AssertEquals("Correct SQL", test.ExpectedSQL, resultQuery.LiteralTextADO);
				var filter = (ModuleNumberFilter)test.ModuleFilters.First();
				filter.Validation.ValidateAll();
				AssertNoWarnings(filter.PropertyInfo);
			}

			//The following two aren't supported and have a validation warning.

			{
				var test = new ModuleFiltersAndExpectedSQLForTest()
				{
					ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "1,2,3", ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith, OrCategory = FilterOrCategory.Red },
				},
					ExpectedSQL = "Z0_Description not like '1,2,3%'"
				};

				var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);
				AssertEquals("Correct SQL", test.ExpectedSQL, resultQuery.LiteralTextADO);
				var filter = (ModuleNumberFilter)test.ModuleFilters.First();
				filter.Validation.ValidateAll();
				AssertHasWarning(filter.PropertyInfo, "Multiple values cannot be used in conjunction with 'not start' or 'not contains'.");
			}

			{
				var test = new ModuleFiltersAndExpectedSQLForTest()
				{
					ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "1,2,3", ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain, OrCategory = FilterOrCategory.Red },
				},
					ExpectedSQL = "Z0_Description not like '%1,2,3%'"
				};

				var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);
				AssertEquals("Correct SQL", test.ExpectedSQL, resultQuery.LiteralTextADO);
				var filter = (ModuleNumberFilter)test.ModuleFilters.First();
				filter.Validation.ValidateAll();
				AssertHasWarning(filter.PropertyInfo, "Multiple values cannot be used in conjunction with 'not start' or 'not contains'.");
			}
		}

		protected override ModuleFiltersAndExpectedSQLForTest GetExactComparisonTest()
		{
			return new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "1", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red },
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "2", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red }
				},
				ExpectedSQL = "(Z0_Description in ('1', '2'))"
			};
		}

		protected override ModuleFiltersAndExpectedSQLForTest GetExactComparisonTest_InGroup()
		{
			return new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "1", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 1" },
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "2", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 1" },
					new ModuleNumberFilter("Filter", DummyBizoSchema.Z0_Description) { Property = "3", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 2" }
				},
				ExpectedSQL = "(Z0_Description in ('1', '2')) and Z0_Description = '3'"
			};
		}
	}
}
