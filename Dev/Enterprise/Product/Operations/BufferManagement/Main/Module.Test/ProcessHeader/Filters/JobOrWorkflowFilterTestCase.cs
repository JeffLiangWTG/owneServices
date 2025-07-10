using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(JobOrWorkflowFilter))]
	class JobOrWorkflowFilterTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestHelperMethods()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);

			filter.SetJobOnly();
			AssertEquals(true, filter.Property0);
			AssertEquals(false, filter.Property1);
			AssertEquals(false, filter.Property2);
			AssertEquals(true, filter.IsJobOnly);
			AssertEquals(false, filter.IsWorkflowOnly);
			AssertEquals(false, filter.IsJobAndWorkflow);

			var query = filter.Query;
			var results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder(new[] { "Job" }, results.Select(x => x.FH_CompletionStatement));

			filter.SetWorkflowOnly();
			AssertEquals(false, filter.Property0);
			AssertEquals(true, filter.Property1);
			AssertEquals(false, filter.Property2);
			AssertEquals(false, filter.IsJobOnly);
			AssertEquals(true, filter.IsWorkflowOnly);
			AssertEquals(false, filter.IsJobAndWorkflow);

			query = filter.Query;
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder(new[] { "Workflow" }, results.Select(x => x.FH_CompletionStatement));

			filter.SetJobAndWorkflow();
			AssertEquals(false, filter.Property0);
			AssertEquals(false, filter.Property1);
			AssertEquals(true, filter.Property2);
			AssertEquals(false, filter.IsJobOnly);
			AssertEquals(false, filter.IsWorkflowOnly);
			AssertEquals(true, filter.IsJobAndWorkflow);

			query = filter.Query;
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder(new[] { "Job", "Workflow" }, results.Select(x => x.FH_CompletionStatement));

			filter.SetJobOnly(); // just to make sure it unsets job and workflow
			AssertEquals(true, filter.Property0);
			AssertEquals(false, filter.Property1);
			AssertEquals(false, filter.Property2);
			query = filter.Query;
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder(new[] { "Job" }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestChangingFilterPresenceOrSettings_ShouldInvalidateAllCachedFiltersForAllFilterStrips()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var completionStatementFilter = filterBizo.AddTextFilterStrip(ProcessHeader.ModuleFilterConstants.CompletionStatement, "Mrs Sullivan");
			var query = filterBizo.Filter;

			void AssertIsQueryCached(string message, bool shouldBeCached)
			{
				AssertEquals(message, !shouldBeCached, filterBizo.ModuleFilters.IsQueryStale(filterBizo.ActiveModuleFilters));
				AssertEquals(message, !shouldBeCached, completionStatementFilter.IsQueryStale);
			}

			AssertIsQueryCached("The query has just been generated so it should be cached. SAD!", true);

			var jobOrWorkflowfilter = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			AssertIsQueryCached("Adding a job or workflow filter strip should invalidation the filter collection and other filters. SAD!", false);

			void AssertChangingJobOrWorkflowInvalidatesQuery(Action changeFilterAction)
			{
				query = filterBizo.Filter;
				AssertEquals("The query has just been generated so it should be cached. SAD!", false, completionStatementFilter.IsQueryStale);

				changeFilterAction.Invoke();
				AssertEquals("The job or workflow's configuration is changed so all cached queries should be invalidated. SAD!", true, completionStatementFilter.IsQueryStale);
			}

			AssertChangingJobOrWorkflowInvalidatesQuery(() => jobOrWorkflowfilter.SetWorkflowOnly());
			AssertChangingJobOrWorkflowInvalidatesQuery(() => jobOrWorkflowfilter.SetJobOnly());
			AssertChangingJobOrWorkflowInvalidatesQuery(() => jobOrWorkflowfilter.SetJobAndWorkflow());
			AssertChangingJobOrWorkflowInvalidatesQuery(() => jobOrWorkflowfilter.IsActive = false);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			return filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
		}
	}
}
