using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleGuidForeignCollectionFilter))]
	sealed class ModuleGuidForeignCollectionFilterTest : ModuleFilterTestCase<ModuleGuidForeignCollectionFilter>
	{
		public void TestHasComparisonOperator()
		{
			AssertEquals(true, Filter.HasComparisonOperator);
		}

		public void TestAllowedComparisonOperators()
		{
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, ModuleTextFilter.ComparisonConstants.AllMatch, ModuleTextFilter.ComparisonConstants.AnyMatch, ModuleTextFilter.ComparisonConstants.NoneMatch }, Filter.AllowedComparisonOperators);
		}

		public void TestQuery()
		{
			var dummyWithAllOnes = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithAllOnes.Z0_Description = "dummyWithAllOnes";
			var dummyWithNoOnes = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithNoOnes.Z0_Description = "dummyWithNoOnes";
			var dummyWithSomeOnes = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithSomeOnes.Z0_Description = "dummyWithSomeOnes";
			var dummyWithNoDependencies = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithNoDependencies.Z0_Description = "dummyWithNoDependencies";

			var dependentWithOne1 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithOne1.ZD1_Number = 1;
			dependentWithOne1.ZD1_Z0 = dummyWithAllOnes.PK;
			var dependentWithOne2 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithOne2.ZD1_Number = 1;
			dependentWithOne2.ZD1_Z0 = dummyWithAllOnes.PK;

			var dependentWithZero1 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithZero1.ZD1_Number = 0;
			dependentWithZero1.ZD1_Z0 = dummyWithNoOnes.PK;
			var dependentWithZero2 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithZero2.ZD1_Number = 0;
			dependentWithZero2.ZD1_Z0 = dummyWithNoOnes.PK;

			var dependentWithOne3 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithOne3.ZD1_Number = 1;
			dependentWithOne3.ZD1_Z0 = dummyWithSomeOnes.PK;
			var dependentWithZero3 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithZero3.ZD1_Number = 0;
			dependentWithZero3.ZD1_Z0 = dummyWithSomeOnes.PK;

			Factory.Save();
			AddSubModuleLayoutForTest(Filter, numberColumnValue: 1, addCodeFilter: false);

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<DummyBusinessObject>(Filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match: " + Filter.Query.LiteralTextSqlFormatted, new[] { dummyWithAllOnes.Z0_Description, dummyWithSomeOnes.Z0_Description }, result.Select(x => x.Z0_Description));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertEquals(false, Filter.Query.IsEmpty);
			result = Factory.Load<DummyBusinessObject>(Filter.Query);
			AssertContainsExactElementsInAnyOrder("None match: " + Filter.Query.LiteralTextSqlFormatted, new[] { dummyWithNoOnes.Z0_Description, dummyWithNoDependencies.Z0_Description }, result.Select(x => x.Z0_Description));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<DummyBusinessObject>(Filter.Query);
			AssertContainsExactElementsInAnyOrder("All match: " + Filter.Query.LiteralTextSqlFormatted, new[] { dummyWithAllOnes.Z0_Description, dummyWithNoDependencies.Z0_Description }, result.Select(x => x.Z0_Description));
		}

		public void TestAllMatchesQueryWithModuleFilterIsEmpty()
		{
			var filter = new ModuleGuidForeignCollectionFilter("Test", DummyModuleIDs.Dummy, DummyBizoSchema.PK, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory), typeof(DummyBusinessObject));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			var subFilter = filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Z0_Code");
			subFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;

			AssertNoExceptionThrown(() => Factory.Load<DummyBusinessObject>(filter.Query));
		}

		public void TestAllMatchesQueryWithDefaultFilterOverridden_ShouldMatchCorrectly()
		{
			var dummyWithAllAbc = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithAllAbc.Z0_Description = "dummyWithAllAbc";
			var dummyWithSomeAbc = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithSomeAbc.Z0_Description = "dummyWithSomeAbc";

			var dependentWithAbc1 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithAbc1.ZD1_Code = "ABC";
			dependentWithAbc1.ZD1_Z0 = dummyWithAllAbc.PK;
			var dependentWithAbc2 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithAbc2.ZD1_Code = "ABC";
			dependentWithAbc2.ZD1_Z0 = dummyWithAllAbc.PK;

			var dependentWithAbc3 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithAbc3.ZD1_Code = "ZZZ";
			dependentWithAbc3.ZD1_Z0 = dummyWithSomeAbc.PK;
			var dependentWithoutAbc = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithoutAbc.ZD1_Code = "XYZ";
			dependentWithoutAbc.ZD1_Number = 1;
			dependentWithoutAbc.ZD1_Z0 = dummyWithSomeAbc.PK;

			Factory.Save();
			AddSubModuleLayoutForTest(Filter, numberColumnValue: 0, addCodeFilter: true);

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			var result = Factory.Load<DummyBusinessObject>(Filter.Query);
			AssertContainsExactElementsInAnyOrder("dummyWithSomeAbc should not have been selected because an AlwaysApplied filter was overridden. The selected filters, and an empty filter, will return the same number of rows, but they're different rows, so it shouldn't be considered matching. And yet..." + Filter.Query.LiteralTextSqlFormatted,
				new[] { dummyWithAllAbc.Z0_Description }, result.Select(x => x.Z0_Description));
		}

		public void TestAllMatches_ShouldPreserveSubQueryParameters()
		{
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			Filter.SelectedFilters.AddGuidFilterStrip("Dummy", ZGuid.BrettsGuid);

			var sql = Filter.Query.ParameterisedText.ParameterisedQueryText;

			AssertContains("The query text should have a customised parameter in it. SAD!", "@", sql);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised. SAD!", ZGuid.BrettsGuid.ToString(), sql, ignoreCase: true);
		}

		public void TestAllMatches_WhenSubQueryHasSameNamedParametersAsMainQuery_ShouldProduceQueryWithUniquelyNamedParameters()
		{
			var dummyFilterBizo = new DummyFilterBusinessObjectWithForeignCollectionFilter(Filter);
			dummyFilterBizo.AddGuidFilterStrip("Z0_Guid", ZGuid.BrettsGuid);

			var otherGuid = ZGuid.NewZGuid();
			var subFilter = dummyFilterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("moo");
			subFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			subFilter.SelectedFilters.AddGuidFilterStrip("Dummy", otherGuid);

			var sql = dummyFilterBizo.Filter.ParameterisedText.ParameterisedQueryText;

			AssertContains("The query text should have a customised parameter in it. SAD!", "@FOREIGN", sql);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised (this one contained Brett's guid). SAD!", ZGuid.BrettsGuid.ToString(), sql, ignoreCase: true);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised (this one contained otherGuid). SAD!", otherGuid.ToString(), sql, ignoreCase: true);

			var parameterNames = dummyFilterBizo.Filter.Params.Select(x => x.ParameterName).ToArray();
			AssertEquals("There should be four distinct parameter names. SAD!" + System.Environment.NewLine + sql, 4, parameterNames.Length);

			var distinctNames = parameterNames.Distinct();
			AssertContainsExactElementsInAnyOrder("All the parameters should have different names so they don't collide. SAD!" + System.Environment.NewLine + sql, parameterNames, distinctNames);

			AssertNoExceptionThrown("Running the query shouldn't throw exceptions. SAD!" + System.Environment.NewLine + sql, () => Factory.Load<DummyBusinessObject>(dummyFilterBizo.Filter));
		}

		public void TestAllMatches_WhenNestedFilters_ShouldDependOnDepth()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var guid3 = ZGuid.NewZGuid();

			var dummyFilterBizo = new DummyFilterBusinessObjectWithForeignCollectionFilter(Filter);
			dummyFilterBizo.AddGuidFilterStrip("Z0_Guid", guid1);

			var rootFilter = dummyFilterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("moo");
			rootFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			rootFilter.SelectedFilters.AddGuidFilterStrip("Dummy", guid2);
			rootFilter.SelectedFilters.ModuleFilters.AddFilter(GetNewModuleFilter());

			var subFilter = rootFilter.SelectedFilters.AddFilterStrip<ModuleGuidForeignCollectionFilter>("moo");
			subFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			subFilter.SelectedFilters.AddGuidFilterStrip("Dummy", guid3);

			var sql = dummyFilterBizo.Filter.ParameterisedText.ParameterisedQueryText;

			AssertContains("The query text should have customised parameters in it. SAD!", "@FOREIGN", sql);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised (this one contained guid1). SAD!", guid1.ToString(), sql, ignoreCase: true);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised (this one contained guid2). SAD!", guid2.ToString(), sql, ignoreCase: true);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised (this one contained guid3). SAD!", guid3.ToString(), sql, ignoreCase: true);

			var parameterNames = dummyFilterBizo.Filter.Params.Select(x => x.ParameterName).ToArray();
			AssertEquals("We should have 7 parameters in total. SAD!" + System.Environment.NewLine + sql, 7, parameterNames.Length);

			var topQueryParameterNames = new string[] { "@FOREIGN__0__1_", "@FOREIGN__0__2_", "@FOREIGN__0__3_" };
			AssertContainsExactElementsInAnyOrder($"The parameter names in the top level query should be in the form @FOREIGN_<digit>_<name> but was: {string.Join(",", parameterNames)}", topQueryParameterNames, parameterNames.Where(name => name.StartsWith("@FOREIGN__0__")));

			var subQueryParameterNames = new string[] { "@FOREIGN__0_0__1_", "@FOREIGN__0_0__2_", "@FOREIGN__0_0__3_" };
			AssertContainsExactElementsInAnyOrder($"The parameter names in the query at the first nested level should be in the form @FOREIGN_<digit>_<digit>_<name> but was: {string.Join(", ", parameterNames)}", subQueryParameterNames, parameterNames.Where(name => name.StartsWith("@FOREIGN__0_0__")));
		}

		public void TestAllMatches_WhenMultipleFiltersPresentInLayout_WithSameValues_ShouldHaveDistinctParameterNames()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var guid3 = ZGuid.NewZGuid();

			var dummyFilterBizo = new DummyFilterBusinessObjectWithForeignCollectionFilter(Filter);
			dummyFilterBizo.AddGuidFilterStrip("Z0_Guid", guid1);

			var subFilter1 = dummyFilterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("moo");
			subFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			subFilter1.SelectedFilters.AddGuidFilterStrip("Dummy", guid2);

			var subFilter2 = dummyFilterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("moo");
			subFilter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			subFilter2.SelectedFilters.AddGuidFilterStrip("Dummy", guid3);

			var sql = dummyFilterBizo.Filter.ParameterisedText.ParameterisedQueryText;

			AssertContains("The query text should have customised parameters in it. SAD!", "@FOREIGN", sql);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised (this one contained guid1). SAD!", guid1.ToString(), sql, ignoreCase: true);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised (this one contained guid1). SAD!", guid2.ToString(), sql, ignoreCase: true);
			AssertNotContains("The query text should NOT contain the literal text for sub query clauses that should be parameterised (this one contained guid1). SAD!", guid3.ToString(), sql, ignoreCase: true);

			var parameterNames = dummyFilterBizo.Filter.Params.Select(x => x.ParameterName).ToArray();
			AssertEquals("There should be seven distinct parameter names. The names from the first sub-filter should not be repeated on the second. SAD!" + System.Environment.NewLine + sql, 7, parameterNames.Length);

			var distinctNames = parameterNames.Distinct();
			AssertContainsExactElementsInAnyOrder("All the parameters should have different names so they don't collide. SAD!" + System.Environment.NewLine + sql, parameterNames, distinctNames);

			AssertNoExceptionThrown("Running the query shouldn't throw exceptions. SAD!" + System.Environment.NewLine + sql, () => Factory.Load<DummyBusinessObject>(dummyFilterBizo.Filter));
		}

		public void TestAllMatches_ForSameFilterStripsInDifferentLayouts_ShouldUseSameParameterNames()
		{
			var dummyFilterBizo1 = new DummyFilterBusinessObjectWithForeignCollectionFilter(Filter);
			dummyFilterBizo1.AddGuidFilterStrip("Z0_Guid", ZGuid.BrettsGuid);

			var otherGuid = ZGuid.NewZGuid();
			var subFilter = dummyFilterBizo1.AddFilterStrip<ModuleGuidForeignCollectionFilter>("moo");
			subFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			subFilter.SelectedFilters.AddGuidFilterStrip("Dummy", otherGuid);

			var sql = dummyFilterBizo1.Filter.ParameterisedText.ParameterisedQueryText;
			var parameterNames = dummyFilterBizo1.Filter.Params.Select(x => x.ParameterName).Where(x => x.StartsWith("@FOREIGN")).ToArray();
			AssertEquals("There should be three distinct parameter names. SAD!" + System.Environment.NewLine + sql, 3, parameterNames.Length);

			var dummyFilterBizo2 = new DummyFilterBusinessObjectWithForeignCollectionFilter(Filter);
			dummyFilterBizo2.AddTextFilterStrip("Z0_Code", "AAA");

			var subFilter2 = dummyFilterBizo2.AddFilterStrip<ModuleGuidForeignCollectionFilter>("moo");
			subFilter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			subFilter2.SelectedFilters.AddGuidFilterStrip("Dummy", otherGuid);

			var sql2 = dummyFilterBizo2.Filter.ParameterisedText.ParameterisedQueryText;
			var parameterNames2 = dummyFilterBizo2.Filter.Params.Select(x => x.ParameterName).Where(x => x.StartsWith("@FOREIGN")).ToArray();
			AssertEquals("There should be three distinct parameter names. SAD!" + System.Environment.NewLine + sql2, 3, parameterNames.Length);

			AssertContainsExactElementsInAnyOrder("The filter strip layouts of the sub query filters were exactly the same in both layouts, so the parameter names should be the same. This will allow multiple clients who set up the exact same layouts to use the same query plan." + System.Environment.NewLine + sql2,
				parameterNames, parameterNames2);
		}

		public void TestAllMatches_WhenLayoutReloadedFromDatabase_ShouldStillUseSameParameterNames()
		{
			var dummyFilterBizo1 = new DummyFilterBusinessObjectWithForeignCollectionFilter(Filter);
			dummyFilterBizo1.AddGuidFilterStrip("Z0_Guid", ZGuid.BrettsGuid);

			var otherGuid = ZGuid.NewZGuid();
			var subFilter = dummyFilterBizo1.AddFilterStrip<ModuleGuidForeignCollectionFilter>("moo");
			subFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			subFilter.SelectedFilters.AddGuidFilterStrip("Dummy", otherGuid);

			var sql = dummyFilterBizo1.Filter.ParameterisedText.ParameterisedQueryText;
			var parameterNames = dummyFilterBizo1.Filter.Params.Select(x => x.ParameterName).Where(x => x.StartsWith("@FOREIGN")).ToArray();
			AssertEquals("There should be three distinct parameter names. SAD!" + System.Environment.NewLine + sql, 3, parameterNames.Length);

			var layout = dummyFilterBizo1.SaveLayout("THE BEST IS YET TO COME!!!");

			var newFactory = new BusinessObjectFactory();
			var loadedLayout = newFactory.Load<StmModuleFilter>(layout.PK);

			var newFilterBizo = new DummyFilterBusinessObjectWithForeignCollectionFilter(Filter);
			newFilterBizo.LoadLayout(loadedLayout);

			var sql2 = newFilterBizo.Filter.ParameterisedText.ParameterisedQueryText;
			var parameterNames2 = newFilterBizo.Filter.Params.Select(x => x.ParameterName).Where(x => x.StartsWith("@FOREIGN")).ToArray();
			AssertEquals("There should be three distinct parameter names. SAD!" + System.Environment.NewLine + sql2, 3, parameterNames.Length);

			AssertContainsExactElementsInAnyOrder("The filter strip layout is the same when reloaded from the database, so the parameter names should be the same. This will allow the original query plan to be reused each time the query is run." + System.Environment.NewLine + sql2,
				parameterNames, parameterNames2);
		}

		public void TestNewFilterStrip_WithoutSpecifyingSubModuleFilters_ShouldNotProduceEmptyQuery()
		{
			var dummyWithDependent = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummyWithoutDependent = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithDependent.Z0_Description = "I'm so popular!";
			dummyWithoutDependent.Z0_Description = "Nobody likes me";

			var dependent = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependent.ZD1_Z0 = dummyWithDependent.PK;

			Factory.Save();

			var query = Filter.Query;
			var results = Factory.Load<DummyBusinessObject>(query);

			AssertContainsExactElementsInAnyOrder("Only the object with a dependent should be matched by default because the query is ANY (so it must have at least one), and yet..." + query.LiteralTextADOFormatted, new[] { "I'm so popular!" }, results.Select(x => x.Z0_Description));
		}

		public void TestIsEmpty()
		{
			AssertEquals("ModuleGuidForeignCollection filters are never empty because the presence of the filter indicates that a relationship is expected/not expected and always requires a query, and yet...", false, Filter.IsEmpty);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#region Implementation

		public static void AddSubModuleLayoutForTest(IModuleFilterWithSelectedFilters filter, int numberColumnValue, bool addCodeFilter)
		{
			var subFilter = filter.SelectedFilters.AddFilterStrip<ModuleNumberRangeFilter>("ZD1_Number");
			subFilter.Property1 = numberColumnValue;
			subFilter.Property2 = numberColumnValue;

			if (addCodeFilter)
			{
				var codeFilter = filter.SelectedFilters.AddTextFilterStrip("Always Applied Code", "XYZ");
				codeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			}
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ModuleGuidForeignCollectionFilter GetNewModuleFilter()
		{
			return new ModuleGuidForeignCollectionFilter("moo", DummyModuleIDs.DummyDependent, DummyBizoSchema.PK, DummyDependentBizoSchema.ZD1_Z0, new DummyDependentBusinessObjectCollection(Factory), typeof(DummyBusinessObject));
		}

		#endregion
	}
}
