using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module
{
	[TestedType(typeof(DependentWorkflowsFilter))]
	public class DependentWorkflowsFilterTest : ModuleFilterTestCase<DependentWorkflowsFilter>
	{
		public void TestFilterReturnsCorrectOutput()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, false, "Job-Level Workflow");
			var workflowOne = BMSTestHelper.CreateWorkflow(jobHeader, "workflowOne");
			var workflowTwo = BMSTestHelper.CreateWorkflow(jobHeader, "workflowTwo");
			var workflowThree = BMSTestHelper.CreateWorkflow(jobHeader, "workflowThree");
			var workflowFour = BMSTestHelper.CreateWorkflow(jobHeader, "workflowFour");
			var workflowFive = BMSTestHelper.CreateWorkflow(jobHeader, "workflowFive");
			var workflowSix = BMSTestHelper.CreateWorkflow(jobHeader, "workflowSix");

			workflowThree.GetOrCreateDependencyLink(workflowTwo);
			workflowFour.GetOrCreateDependencyLink(workflowTwo);
			workflowFive.GetOrCreateDependencyLink(workflowThree);
			workflowSix.GetOrCreateDependencyLink(workflowTwo);
			workflowSix.GetOrCreateDependencyLink(workflowThree);

			Factory.Save();

			var filter0 = GetNewModuleFilter();
			var result0 = Factory.Load<ProcessHeader>(filter0.Query);

			AssertContainsExactElementsInAnyOrder("should result in all dependents", new ProcessHeader[] { workflowThree, workflowFour, workflowFive, workflowSix }, result0);

			var filter1 = GetNewModuleFilter();
			filter1.SelectedFilters.AddTextFilterStrip("Completion Statement", "workflowOne");
			var result1 = Factory.Load<ProcessHeader>(filter1.Query);

			AssertContainsExactElementsInAnyOrder("Should result in nothing", System.Array.Empty<ProcessHeader>(), result1);

			var filter2 = GetNewModuleFilter();
			filter2.SelectedFilters.AddTextFilterStrip("Completion Statement", "workflowTwo");
			var result2 = Factory.Load<ProcessHeader>(filter2.Query);

			AssertContainsExactElementsInAnyOrder("workflowTwos dependents are workflowThree, Four, and Six", new ProcessHeader[] { workflowThree, workflowFour, workflowSix }, result2);

			var filter3 = GetNewModuleFilter();
			filter3.SelectedFilters.AddTextFilterStrip("Completion Statement", "workflowThree");
			var result3 = Factory.Load<ProcessHeader>(filter3.Query);

			AssertContainsExactElementsInAnyOrder("workflowThrees dependents are workflowFive and Six", new List<ProcessHeader> { workflowFive, workflowSix }, result3);
		}

		#region Implementation

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override DependentWorkflowsFilter GetNewModuleFilter()
		{
			return new DependentWorkflowsFilter("moo", new ProcessHeaderCollection(Factory));
		}

		#endregion
	}
}
