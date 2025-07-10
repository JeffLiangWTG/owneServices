using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(JobLevelWorkflowFilter))]
	class JobLevelWorkflowFilterTest : ModuleFilterTestCase<JobLevelWorkflowFilter>
	{
		public void TestFilter_HasCorrectProperties()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo.AddFilterStrip<JobLevelWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobLevelWorkflow);

			AssertEquals("Multilingual Description", "Job-level Workflow", filter.MultilingualDescription);
			AssertContainsExactElementsInAnyOrder("The only allowed comparison operator should be filters match.", new[] { ModuleTextFilter.ComparisonConstants.FiltersMatch }, filter.AllowedComparisonOperators);
		}

		public void TestFilter_ReturnsCorrectResults()
		{
			var completionStatementJobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "RYAN!");
			var completionStatementWorkflow1 = BMSTestHelper.CreateWorkflowAndTask(completionStatementJobHeader1, "completionStatementWorkflow1");
			var completionStatementWorkflow2 = BMSTestHelper.CreateWorkflowAndTask(completionStatementJobHeader1, "completionStatementWorkflow2");

			var completionStatementJobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "RYAN!");
			var completionStatementWorkflow3 = BMSTestHelper.CreateWorkflowAndTask(completionStatementJobHeader2, "completionStatementWorkflow3");
			var completionStatementWorkflow4 = BMSTestHelper.CreateWorkflowAndTask(completionStatementJobHeader2, "completionStatementWorkflow4");

			var blankWorkflowJobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var blankWorkflow1 = BMSTestHelper.CreateWorkflowAndTask(blankWorkflowJobHeader, "blankWorkflow1");
			var blankWorkflow2 = BMSTestHelper.CreateWorkflowAndTask(blankWorkflowJobHeader, "blankWorkflow2");

			Factory.Save();

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			var result = Factory.Load<ProcessHeader>(Filter.Query);

			AssertContainsExactElementsInAnyOrder("When we have no sub-filter strips selected then it should return all of the workflows.\n\nQuery: "
				+ Filter.Query.LiteralTextSqlFormatted, new[] { completionStatementWorkflow1, completionStatementWorkflow2, completionStatementWorkflow3, completionStatementWorkflow4, blankWorkflow1, blankWorkflow2 }, result);

			Filter.SelectedFilters.AddTextFilterStrip("Completion Statement", "RYAN!");
			result = Factory.Load<ProcessHeader>(Filter.Query);
			AssertContainsExactElementsInAnyOrder("WHEN we have selected Completion Statement sub-filter strip with search query 'RYAN!' THEN it should return only the workflows which have that completion statement as the job header description.\n\nQuery:\n"
				+ Filter.Query.LiteralTextSqlFormatted, new[] { completionStatementWorkflow1, completionStatementWorkflow2, completionStatementWorkflow3, completionStatementWorkflow4 }, result);

			Filter.SelectedFilters.AddTextFilterStrip("Workflow Type", "Garbage");
			result = Factory.Load<ProcessHeader>(Filter.Query);
			AssertEquals("WHEN we have ADDITIONALLY selected workflow type sub-filter strip with 'Garbage' THEN it should not match with any job headers and hence return no workflows.\n\nQuery:\n" + Filter.Query.LiteralTextSqlFormatted, 0, result.Length);
		}

		#region Implementation

		public override void TestQueryIsEmptyByDefault()
		{
			Assert("If FH_FH_ProcessHeader is null in subquery for the job headers then clearly query won't be empty.", true);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals("This filter should not be an expensive query.", false, Filter.IsExpensiveQuery);
		}

		protected override JobLevelWorkflowFilter GetNewModuleFilter()
		{
			return new JobLevelWorkflowFilter("Job-level Workflow", new ProcessJobHeaderCollection(Factory));
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(JobLevelWorkflowFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { nameof(filter.ComparisonOperator) }).ToArray();
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
		protected override ZString ExpectedDescription => "Job-level Workflow";

		#endregion
	}
}
