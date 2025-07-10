using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class ModuleTextFilterInheritedTest : ModuleTextFilterBaseTest
	{
		public void TestOptimisedStartsWithComparison_NotMaxLength()
		{
			var test = new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "ABC", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "CDE", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red }
				},
				ExpectedSQL = "Z0_Code like 'ABC%' or Z0_Code like 'CDE%'"
			};

			Assert($"GIVEN same filter-strips with '{ModuleTextFilter.ComparisonConstants.StartsWith}'", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.StartsWith) > 1);
			AssertEquals("GIVEN filters' property values are not MAX-LENGTH", test.ModuleFilters.Count(), test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => filter.Property.Length != filter.FilterColumn.MaxLength));

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);

			AssertEquals("WHEN calling GetCombinedFilter THEN should have 'OR', not 'IN'", test.ExpectedSQL, resultQuery.LiteralTextADO);
		}

		public void TestOptimisedStartsWithComparison_MaxLength()
		{
			var test = new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "ABCDE", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "FGHIJ", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "KLM", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red }
				},
				ExpectedSQL = "(Z0_Code in ('ABCDE', 'FGHIJ')) or Z0_Code like 'KLM%'"
			};

			Assert($"GIVEN same filter-strips with '{ModuleTextFilter.ComparisonConstants.StartsWith}'", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.StartsWith) > 1);
			Assert("GIVEN filters' property values are MAX-LENGTH", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => filter.Property.Length == filter.FilterColumn.MaxLength) >= 2);

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);

			AssertEquals("WHEN calling GetCombinedFilter THEN should have 'IN', not 'OR' for the MAX-LENGTH property values", test.ExpectedSQL, resultQuery.LiteralTextADO);
		}

		public void TestOptimisedStartsWithComparison_MaxLength_InGroup()
		{
			var test = new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "ABCDE", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red, GroupName = "Group1" },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "FGHIJ", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red, GroupName = "Group1" },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "KLMNO", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red, GroupName = "Group2" }
				},
				ExpectedSQL = "(Z0_Code in ('ABCDE', 'FGHIJ')) and Z0_Code = 'KLMNO'"
			};

			Assert($"GIVEN same filter-strips with '{ModuleTextFilter.ComparisonConstants.StartsWith}'", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.StartsWith) > 1);
			Assert("GIVEN filters' property values are MAX-LENGTH", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => filter.Property.Length == filter.FilterColumn.MaxLength) >= 2);
			Assert("GIVEN multiple same filter-strips in group", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => !filter.GroupName.IsEmpty) > 1);

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);

			AssertEquals("WHEN calling GetCombinedFilter THEN should have 'IN', not 'OR' for the MAX-LENGTH property values of same group", test.ExpectedSQL, resultQuery.LiteralTextADO);
		}

		public void TestOptimisedMixedComparison()
		{
			var test = new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "ABCDE", ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith, OrCategory = FilterOrCategory.Red, GroupName = "Group1" },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "FGHIJ", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group1" },
				},
				ExpectedSQL = "(Z0_Code in ('ABCDE', 'FGHIJ'))"
			};

			Assert($"GIVEN same filter-strips with '{ModuleTextFilter.ComparisonConstants.Exact}'/'{ModuleTextFilter.ComparisonConstants.StartsWith}'",
				test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.StartsWith || filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact) > 1);
			Assert("GIVEN filters' property values are MAX-LENGTH", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Any(filter => filter.Property.Length == filter.FilterColumn.MaxLength));

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);

			AssertEquals("WHEN calling GetCombinedFilter THEN should have 'IN', not 'OR' for the MAX-LENGTH property values of same group", test.ExpectedSQL, resultQuery.LiteralTextADO);
		}

		public void TestOptimisedCurrentUserComparison()
		{
			var test = new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleNkFilterTest.GlbStaffFilterForTest("Staff Filter", DummyBizoSchema.Z0_Code, ModuleIDs.GlbStaff, list: new StmNoteNonDependentCollection(Factory)) { OrCategory = FilterOrCategory.Red, IsActive = true, Property = "USR1" },
					new ModuleNkFilterTest.GlbStaffFilterForTest("Staff Filter", DummyBizoSchema.Z0_Code, ModuleIDs.GlbStaff, list: new StmNoteNonDependentCollection(Factory)) { OrCategory = FilterOrCategory.Red, IsActive = true, ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser },
				},
				ExpectedSQL = "(Z0_Code in ('E', 'USR1'))"
			};

			Assert($"GIVEN same filter-strips with '{ModuleTextFilter.ComparisonConstants.CurrentUser}'", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Any(filter => filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.CurrentUser));
			AssertEquals("GIVEN current user is 'E'", "E", Env.CurrentUser.Initials);

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);

			AssertEquals("WHEN calling GetCombinedFilter THEN should have 'IN', not 'OR' for currentUser and user filter", test.ExpectedSQL, resultQuery.LiteralTextADO);
		}

		public void TestOptimisedContainsComparison()
		{
			var test = new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "ABCDE", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains, OrCategory = FilterOrCategory.Red, GroupName = "Group1" },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "FGHIJ", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group1" },
				},
				ExpectedSQL = "(Z0_Code in ('ABCDE', 'FGHIJ'))"
			};

			Assert($"GIVEN same filter-strips with '{ModuleTextFilter.ComparisonConstants.Exact}'/'{ModuleTextFilter.ComparisonConstants.Contains}'",
				test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Count(filter => filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.Contains || filter.ComparisonOperator == ModuleTextFilter.ComparisonConstants.Exact) > 1);
			Assert("GIVEN filters' property values are MAX-LENGTH", test.ModuleFilters.Cast<ModuleFilterWithListAndComparisonOperators<ZString>>().Any(filter => filter.Property.Length == filter.FilterColumn.MaxLength));

			var resultQuery = ModuleFilterCombiner.GetCombinedFilter(test.ModuleFilters);

			AssertEquals("WHEN calling GetCombinedFilter THEN should have 'IN', not 'OR' for the MAX-LENGTH property values of same group", test.ExpectedSQL, resultQuery.LiteralTextADO);
		}

		#region Virtual Methods

		protected override ModuleFiltersAndExpectedSQLForTest GetExactComparisonTest()
		{
			return new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "ABCDE", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "FGHIJ", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red },
				},
				ExpectedSQL = "(Z0_Code in ('ABCDE', 'FGHIJ'))"
			};
		}

		protected override ModuleFiltersAndExpectedSQLForTest GetExactComparisonTest_InGroup()
		{
			return new ModuleFiltersAndExpectedSQLForTest()
			{
				ModuleFilters = new[] {
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "ABCDE", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 1" },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "FGHIJ", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 1" },
					new ModuleTextFilter("Filter", DummyBizoSchema.Z0_Code) { Property = "KLMNO", ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact, OrCategory = FilterOrCategory.Red, GroupName = "Group 2" },
				},
				ExpectedSQL = "(Z0_Code in ('ABCDE', 'FGHIJ')) and Z0_Code = 'KLMNO'"
			};
		}

		#endregion
	}
}
