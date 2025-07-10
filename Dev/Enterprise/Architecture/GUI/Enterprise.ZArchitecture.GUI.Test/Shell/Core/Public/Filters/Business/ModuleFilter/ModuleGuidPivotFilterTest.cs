using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ModuleGuidPivotFilter))]
	public class ModuleGuidPivotFilterTest : ModuleFilterTestCase<ModuleGuidPivotFilter>
	{
		public void TestQuery_NoPivotTableFilter()
		{
			SetUpBusinessObjects();
			Factory.Save();

			AddSubModuleLayoutForTest(Filter, numberColumnValue: 1, addCodeFilter: false);

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var query = Filter.Query;
			var result = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("Any match: " + query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes", "dummyWithSomeOnes" }, result.Select(x => x.Z0_Description));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			query = Filter.Query;
			AssertEquals(false, query.IsEmpty);
			result = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("None match: " + query.LiteralTextADOFormatted, new[] { "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			query = Filter.Query;
			result = Factory.Load<DummyBusinessObject>(query);
			AssertContainsExactElementsInAnyOrder("All match: " + query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			Filter.Clear();
			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertEquals(false, Filter.Query.IsEmpty);
			result = Factory.Load<DummyBusinessObject>(Filter.Query);
			AssertContainsExactElementsInAnyOrder("None match with no submodule filters: " + Filter.Query.LiteralTextADOFormatted, new[] { "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));
		}

		public void TestQuery_PivotTableFilter()
		{
			SetUpBusinessObjects();
			Factory.Save();

			var pivotFilter = new ZQuery(DummyPivotSchema.ZDP_AddInfo, "Only this");
			var filter = new ModuleGuidPivotFilter("moo", DummyModuleIDs.DummyDependent, DummyPivotSchema.ZDP_ZD1, DummyPivotSchema.ZDP_Z0, new DummyDependentBusinessObjectCollection(Factory), typeof(DummyBusinessObject), typeof(DummyPivot), pivotFilter);

			AddSubModuleLayoutForTest(filter, numberColumnValue: 1, addCodeFilter: false);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes" }, result.Select(x => x.Z0_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertEquals(false, filter.Query.IsEmpty);
			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes", "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			var pivot = Factory.New<DummyPivot>();
			pivot.ZDP_Z0 = dummyWithAllOnes.PK;
			pivot.ZDP_ZD1 = dependentWithZero1.PK;
			Factory.Save();

			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes", "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			pivot.ZDP_AddInfo = "Only this";
			Factory.Save();

			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			filter.Clear();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertEquals(false, filter.Query.IsEmpty);
			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match with no submodule filters: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));
		}

		public void TestQuery_PivotTableFilter_NoSubModuleLayout()
		{
			SetUpBusinessObjects();
			Factory.Save();

			var pivotFilter = new ZQuery(DummyPivotSchema.ZDP_AddInfo, "Only this");
			var filter = new ModuleGuidPivotFilter("moo", DummyModuleIDs.DummyDependent, DummyPivotSchema.ZDP_ZD1, DummyPivotSchema.ZDP_Z0, new DummyDependentBusinessObjectCollection(Factory), typeof(DummyBusinessObject), typeof(DummyPivot), pivotFilter);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes" }, result.Select(x => x.Z0_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertEquals(false, filter.Query.IsEmpty);
			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes", "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			var pivot = Factory.New<DummyPivot>();
			pivot.ZDP_Z0 = dummyWithAllOnes.PK;
			pivot.ZDP_ZD1 = dependentWithZero1.PK;
			Factory.Save();

			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes", "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			pivot.ZDP_AddInfo = "Only this";
			Factory.Save();

			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithAllOnes", "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));

			filter.Clear();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertEquals(false, filter.Query.IsEmpty);
			result = Factory.Load<DummyBusinessObject>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match with no submodule filters: " + filter.Query.LiteralTextADOFormatted, new[] { "dummyWithSomeOnes", "dummyWithNoOnes", "dummyWithNoDependencies" }, result.Select(x => x.Z0_Description));
		}

		public void TestHasComparisonOperator()
		{
			AssertEquals(true, Filter.HasComparisonOperator);
		}

		public void TestAllowedComparisonOperators()
		{
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, ModuleTextFilter.ComparisonConstants.AllMatch, ModuleTextFilter.ComparisonConstants.AnyMatch, ModuleTextFilter.ComparisonConstants.NoneMatch }, Filter.AllowedComparisonOperators);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals(false, Filter.Query.IsEmpty);
		}

		static void AddSubModuleLayoutForTest(IModuleFilterWithSelectedFilters filter, int numberColumnValue, bool addCodeFilter)
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

		#region Implementation

		DummyBusinessObject dummyWithAllOnes;
		DummyDependantBusinessObject dependentWithZero1;

		void SetUpBusinessObjects()
		{
			var dummies = Factory.Load<DummyBusinessObject>(new ZQuery());
			dummies.DeleteAll();

			dummyWithAllOnes = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithAllOnes.Z0_Description = "dummyWithAllOnes";
			var dummyWithNoOnes = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithNoOnes.Z0_Description = "dummyWithNoOnes";
			var dummyWithSomeOnes = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithSomeOnes.Z0_Description = "dummyWithSomeOnes";
			var dummyWithNoDependencies = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyWithNoDependencies.Z0_Description = "dummyWithNoDependencies";

			var dependentWithOne1 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithOne1.ZD1_Number = 1;
			var dependentWithOne2 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithOne2.ZD1_Number = 1;

			dependentWithZero1 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithZero1.ZD1_Number = 0;
			var dependentWithZero2 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithZero2.ZD1_Number = 0;

			var dependentWithOne3 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithOne3.ZD1_Number = 1;
			var dependentWithZero3 = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dependentWithZero3.ZD1_Number = 0;

			var pivot1 = Factory.New<DummyPivot>();
			pivot1.ZDP_Z0 = dummyWithAllOnes.PK;
			pivot1.ZDP_ZD1 = dependentWithOne1.PK;
			pivot1.ZDP_AddInfo = "Only this";
			var pivot2 = Factory.New<DummyPivot>();
			pivot2.ZDP_Z0 = dummyWithAllOnes.PK;
			pivot2.ZDP_ZD1 = dependentWithOne2.PK;
			var pivot3 = Factory.New<DummyPivot>();
			pivot3.ZDP_Z0 = dummyWithNoOnes.PK;
			pivot3.ZDP_ZD1 = dependentWithZero1.PK;
			var pivot4 = Factory.New<DummyPivot>();
			pivot4.ZDP_Z0 = dummyWithNoOnes.PK;
			pivot4.ZDP_ZD1 = dependentWithZero2.PK;
			var pivot5 = Factory.New<DummyPivot>();
			pivot5.ZDP_Z0 = dummyWithSomeOnes.PK;
			pivot5.ZDP_ZD1 = dependentWithOne3.PK;
			var pivot6 = Factory.New<DummyPivot>();
			pivot6.ZDP_Z0 = dummyWithSomeOnes.PK;
			pivot6.ZDP_ZD1 = dependentWithZero3.PK;
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ModuleGuidPivotFilter GetNewModuleFilter()
		{
			return new ModuleGuidPivotFilter("moo", DummyModuleIDs.DummyDependent, DummyPivotSchema.ZDP_ZD1, DummyPivotSchema.ZDP_Z0, new DummyDependentBusinessObjectCollection(Factory), typeof(DummyBusinessObject), typeof(DummyPivot));
		}

		#endregion
	}
}
