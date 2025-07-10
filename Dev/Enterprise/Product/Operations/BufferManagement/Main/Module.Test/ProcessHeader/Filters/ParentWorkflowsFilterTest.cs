using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ParentWorkflowsFilter))]
	public class ParentWorkflowsFilterTest : ModuleFilterTestCase<ParentWorkflowsFilter>
	{
		public void TestFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, false, "Job-Level Workflow");

			var workflowWithNoChildren = BMSTestHelper.CreateWorkflow(jobHeader, "No parent no children");
			var singleParentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Single Parent");
			var childOfSingleParent1 = BMSTestHelper.CreateWorkflow(jobHeader, "Child of single parent 1");
			var childOfSingleParent2 = BMSTestHelper.CreateWorkflow(jobHeader, "Child of single parent 2");
			childOfSingleParent1.GetOrCreateLinkToParent(singleParentWorkflow);
			childOfSingleParent2.GetOrCreateLinkToParent(singleParentWorkflow);
			var daddyWorkflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Daddy 1");
			var daddyWorkflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Daddy 2");
			var childOfDaddies1 = BMSTestHelper.CreateWorkflow(jobHeader, "Child of daddies 1");
			var childOfDaddies2 = BMSTestHelper.CreateWorkflow(jobHeader, "Child of daddies 2");
			childOfDaddies1.GetOrCreateLinkToParent(daddyWorkflow1);
			childOfDaddies1.GetOrCreateLinkToParent(daddyWorkflow2);
			childOfDaddies2.GetOrCreateLinkToParent(daddyWorkflow1);
			childOfDaddies2.GetOrCreateLinkToParent(daddyWorkflow2);

			Factory.Save();

			var filter = GetNewModuleFilter();
			var subFilterResult = Factory.Load<ProcessHeader>(filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Job-Level Workflow", "No parent no children", "Single Parent", "Child of single parent 1", "Child of single parent 2", "Daddy 1", "Daddy 2", "Child of daddies 1", "Child of daddies 2" }, subFilterResult.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match, no subfilters: " + filter.Query.LiteralTextADOFormatted, new[] { "Child of single parent 1", "Child of single parent 2", "Child of daddies 1", "Child of daddies 2" }, result.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match, no subfilters: " + filter.Query.LiteralTextADOFormatted, new[] { "Job-Level Workflow", "No parent no children", "Single Parent", "Daddy 1", "Daddy 2" }, result.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match, no subfilters: " + filter.Query.LiteralTextADOFormatted, new[] { "Job-Level Workflow", "No parent no children", "Single Parent", "Child of single parent 1", "Child of single parent 2", "Daddy 1", "Daddy 2", "Child of daddies 1", "Child of daddies 2" }, result.Select(x => x.FH_CompletionStatement));

			var subFilter = filter.SelectedFilters.AddTextFilterStrip("Completion Statement", "Single Parent");

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match, single parent subfilter: " + filter.Query.LiteralTextADOFormatted, new[] { "Child of single parent 1", "Child of single parent 2" }, result.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match, single parent subfilter: " + filter.Query.LiteralTextADOFormatted, new[] { "Job-Level Workflow", "No parent no children", "Single Parent", "Daddy 1", "Daddy 2", "Child of daddies 1", "Child of daddies 2" }, result.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match, single parent subfilter: " + filter.Query.LiteralTextADOFormatted, new[] { "Job-Level Workflow", "No parent no children", "Single Parent", "Child of single parent 1", "Child of single parent 2", "Daddy 1", "Daddy 2" }, result.Select(x => x.FH_CompletionStatement));

			subFilter.Property = "Daddy 1";

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match, Daddy1 subfilter: " + filter.Query.LiteralTextADOFormatted, new[] { "Child of daddies 1", "Child of daddies 2" }, result.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match, Daddy1 subfilter: " + filter.Query.LiteralTextADOFormatted, new[] { "Job-Level Workflow", "No parent no children", "Single Parent", "Child of single parent 1", "Child of single parent 2", "Daddy 1", "Daddy 2" }, result.Select(x => x.FH_CompletionStatement));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<ProcessHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("All match, Daddy1 subfilter: " + filter.Query.LiteralTextADOFormatted, new[] { "Job-Level Workflow", "No parent no children", "Single Parent", "Daddy 1", "Daddy 2" }, result.Select(x => x.FH_CompletionStatement));
		}

		public void TestFilterAppliedEvenIfNoSubFilters()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, false, "Parent Job-Level Workflow");
			var workflow1_jobHeader1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Parent workflow_1 jobHeader1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, false, "Child Job-Level Workflow");
			var workflow1_jobHeader2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Child workflow_1 jobHeader2");

			jobHeader2.GetOrCreateLinkToParent(jobHeader1);
			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			var jobOrWorkflowFilter = (JobOrWorkflowFilter)strip.CurrentModuleFilter;
			jobOrWorkflowFilter.IsActive = true;
			jobOrWorkflowFilter.SetJobOnly();

			var strip2 = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ParentWorkflows);
			var workflowParentsFilter = (ParentWorkflowsFilter)strip2.CurrentModuleFilter;
			workflowParentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("None match, no subfilters: " + workflowParentsFilter.Query.LiteralTextADOFormatted, new[] { "Parent Job-Level Workflow" }, result.Select(x => x.FH_CompletionStatement));

			workflowParentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Any match, no subfilters: " + workflowParentsFilter.Query.LiteralTextADOFormatted, new[] { "Child Job-Level Workflow" }, result.Select(x => x.FH_CompletionStatement));

			workflowParentsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All match, no subfilters: " + workflowParentsFilter.Query.LiteralTextADOFormatted, new[] { "Parent Job-Level Workflow", "Child Job-Level Workflow" }, result.Select(x => x.FH_CompletionStatement));
		}

		public override void TestQueryIsEmptyByDefault()
		{
			AssertEquals(false, Filter.Query.IsEmpty);
		}

		#region Implementation

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ParentWorkflowsFilter GetNewModuleFilter()
		{
			return new ParentWorkflowsFilter("moo", new ProcessHeaderCollection(Factory));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		#endregion
	}
}
