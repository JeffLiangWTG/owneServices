using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ModuleGuidAppliedToSubCollectionFilter))]
	class ModuleGuidAppliedToSubCollectionFilterTest : ModuleFilterTestCase<ModuleGuidAppliedToSubCollectionFilter>
	{
		public void TestTreatsContainsAndIsAppliedAsAlike()
		{
			Filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.ComparisonConstants.Contains;
			AssertEquals(SQLComparisonOperator.Contains, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator;
			AssertEquals(SQLComparisonOperator.Contains, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.ComparisonConstants.AnyMatch;
			AssertEquals(SQLComparisonOperator.Equal, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator;
			AssertEquals(SQLComparisonOperator.Equal, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.ComparisonConstants.NotContain;
			AssertEquals(SQLComparisonOperator.NotContains, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator;
			AssertEquals(SQLComparisonOperator.NotContains, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.ComparisonConstants.NoneMatch;
			AssertEquals(SQLComparisonOperator.NotEqual, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedNorInheritedComparisonOperator;
			AssertEquals(SQLComparisonOperator.NotEqual, Filter.SqlComparisonOperator);
		}

		#region Implementation

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ModuleGuidAppliedToSubCollectionFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new ModuleGuidAppliedToSubCollectionFilter("moo", FilterCategories.Other, ModuleIDs.JobShipment, (_, x_) => new ZQuery(), list);
		}

		#endregion
	}
}
