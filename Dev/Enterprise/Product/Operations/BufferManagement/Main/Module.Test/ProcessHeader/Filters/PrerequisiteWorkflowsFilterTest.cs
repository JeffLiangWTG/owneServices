using System.Collections.Generic;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module
{
	[TestedType(typeof(PrerequisiteWorkflowsFilter))]
	public class PrerequisiteWorkflowsFilterTest : ModuleFilterTestCase<PrerequisiteWorkflowsFilter>
	{
		public void TestFilterReturnsCorrectOutput()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job-Level Workflow");
			var workflowOne = BMSTestHelper.CreateWorkflow(jobHeader, "workflowOne");
			var workflowTwo = BMSTestHelper.CreateWorkflow(jobHeader, "workflowTwo");
			var workflowThree = BMSTestHelper.CreateWorkflow(jobHeader, "workflowThree");
			var workflowFour = BMSTestHelper.CreateWorkflow(jobHeader, "workflowFour");
			var workflowFive = BMSTestHelper.CreateWorkflow(jobHeader, "workflowFive");
			var workflowSix = BMSTestHelper.CreateWorkflow(jobHeader, "workflowSix");

			workflowTwo.GetOrCreateDependencyLink(workflowThree);
			workflowTwo.GetOrCreateDependencyLink(workflowFour);
			workflowThree.GetOrCreateDependencyLink(workflowFive);
			workflowTwo.GetOrCreateDependencyLink(workflowSix);
			workflowThree.GetOrCreateDependencyLink(workflowSix);

			Factory.Save();

			var filter0 = GetNewModuleFilter();
			var result0 = Factory.Load<ProcessHeader>(filter0.Query);

			AssertContainsExactElementsInAnyOrder("should result in all prerequisites", new ProcessHeader[] { workflowThree, workflowFour, workflowFive, workflowSix }, result0);

			var filter1 = GetNewModuleFilter();
			filter1.SelectedFilters.AddTextFilterStrip("Completion Statement", "workflowOne");
			var result1 = Factory.Load<ProcessHeader>(filter1.Query);

			AssertContainsExactElementsInAnyOrder("should result in nothing", new List<ProcessHeader> { }, result1);

			var filter2 = GetNewModuleFilter();
			filter2.SelectedFilters.AddTextFilterStrip("Completion Statement", "workflowTwo");
			var result2 = Factory.Load<ProcessHeader>(filter2.Query);

			AssertContainsExactElementsInAnyOrder("workflowTwo is a prerequisite to workflowThree, workflowFour and workflowSix", new List<ProcessHeader> { workflowThree, workflowFour, workflowSix }, result2);

			var filter3 = GetNewModuleFilter();
			filter3.SelectedFilters.AddTextFilterStrip("Completion Statement", "workflowThree");
			var result3 = Factory.Load<ProcessHeader>(filter3.Query);

			AssertContainsExactElementsInAnyOrder("workflowTwos prerequisites are workflowFive and workflowSix ", new List<ProcessHeader> { workflowFive, workflowSix }, result3);
		}

		#region Implementation

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override PrerequisiteWorkflowsFilter GetNewModuleFilter()
		{
			return new PrerequisiteWorkflowsFilter("moo", new ProcessHeaderCollection(Factory));
		}

		#endregion
	}
}
