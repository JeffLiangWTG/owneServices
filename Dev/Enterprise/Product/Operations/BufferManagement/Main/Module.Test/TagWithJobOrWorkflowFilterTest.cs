using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(TagWithJobOrWorkflowFilter))]
	class TagWithJobOrWorkflowFilterTest : ModuleFilterTestCase<TagWithJobOrWorkflowFilter>
	{
		public void TestTreatsContainsAndIsAppliedAsAlike()
		{
			Filter.ComparisonOperator = TagWithJobOrWorkflowFilter.ComparisonConstants.Contains;
			AssertEquals(SQLComparisonOperator.Contains, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = TagWithJobOrWorkflowFilter.IsAppliedComparisonOperator;
			AssertEquals(SQLComparisonOperator.Contains, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = TagWithJobOrWorkflowFilter.ComparisonConstants.NotContain;
			AssertEquals(SQLComparisonOperator.NotContains, Filter.SqlComparisonOperator);

			Filter.ComparisonOperator = TagWithJobOrWorkflowFilter.NotAppliedComparisonOperator;
			AssertEquals(SQLComparisonOperator.NotContains, Filter.SqlComparisonOperator);
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

		protected override TagWithJobOrWorkflowFilter GetNewModuleFilter()
		{
			var list = new StmNoteNonDependentCollection(Factory);
			return new TagWithJobOrWorkflowFilter("moo", FilterCategories.Other, ModuleIDs.JobShipment, (_, x_, y_) => new ZQuery(), list);
		}

		#endregion
	}
}
