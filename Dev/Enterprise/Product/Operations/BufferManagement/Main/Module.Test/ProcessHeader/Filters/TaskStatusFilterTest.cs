using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(TaskStatusFilter))]
	public class TaskStatusFilterTest : NonPersistentBusinessObjectTestCase
	{
		#region Serialisation

		public void TestSerialisation()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var serialisedFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			serialisedFilter.IsActive = true;

			serialisedFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			serialisedFilter.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Assigned).Value = true;
			serialisedFilter.TaskTypeCheckList.First(l => l.Description.StartsWith("BUN")).Value = true;

			var deserialisedFilter = new TaskStatusFilter(Factory);

			BMSTestHelper.SerialiseAndDeSerialise(serialisedFilter, deserialisedFilter);

			AssertEquals(TaskStatusAggregatorList.Codes.All, deserialisedFilter.TaskAggregator);
			foreach (var pair in deserialisedFilter.TaskTypeCheckList)
			{
				AssertEquals(pair.Description.StartsWith("BUN"), pair.Value);
			}
			foreach (var pair in deserialisedFilter.TaskStatusCheckList)
			{
				AssertEquals(pair.Description == ProcessTaskStatusCodeList.Codes.Assigned, pair.Value);
			}
		}

		#endregion

		#region Filtration

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilter.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();

			AssertEquals(2, filterBizo.ActiveModuleFilters.Count);
			AssertNotContains("UNION ALL", taskStatusFilter.Query.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetWorkflowOnly();
			AssertNotContains("UNION ALL", taskStatusFilter.Query.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetJobAndWorkflow();
			AssertEquals(1, taskStatusFilter.Query.LiteralTextSqlFormatted.ToUpper().AllIndexesOf("UNION ALL").Count());
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single_OrCategoryApplied()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilter.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();
			jobOrWorkflow.OrCategory = FilterOrCategory.Blue;

			AssertEquals(2, filterBizo.ActiveModuleFilters.Count);
			AssertNotContains("UNION ALL", taskStatusFilter.Query.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetWorkflowOnly();
			AssertNotContains("UNION ALL", taskStatusFilter.Query.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetJobAndWorkflow();
			AssertEquals(1, taskStatusFilter.Query.LiteralTextSqlFormatted.ToUpper().AllIndexesOf("UNION ALL").Count());
		}

		public void TestFilterContainsNoUnion_WhenJobOrWorkflowFilterIsApplied_Single_OrCategoryApplied_ExtraFilterApplied()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilter.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilter.IsActive = true;

			var assignedResourceFilter = (AssignedResourceFilter)filterBizo["Resource Assigned To Any Task"];
			assignedResourceFilter.Property = "AAA";
			assignedResourceFilter.IsActive = true;

			var jobOrWorkflow = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflow.IsActive = true;
			jobOrWorkflow.SetJobOnly();
			jobOrWorkflow.OrCategory = FilterOrCategory.Blue;

			AssertEquals(3, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(1, filterBizo.Filter.LiteralTextSqlFormatted.ToUpper().AllIndexesOf("UNION ALL").Count());

			jobOrWorkflow.SetWorkflowOnly();
			AssertNotContains("UNION ALL", taskStatusFilter.Query.LiteralTextSqlFormatted.ToUpper());

			jobOrWorkflow.SetJobAndWorkflow();
			AssertEquals(1, taskStatusFilter.Query.LiteralTextSqlFormatted.ToUpper().AllIndexesOf("UNION ALL").Count());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_Group_JobFirst()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripTSA = filterBizo.FilterStrips.AddNew();
			stripTSA.FilterDescription = TaskStatusFilter.Schema.Identifier;
			var taskStatusFilterA = (TaskStatusFilter)stripTSA.CurrentModuleFilter;
			taskStatusFilterA.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilterA.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilterA.IsActive = true;
			taskStatusFilterA.GroupName = "A";
			taskStatusFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetJobOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripTSB = filterBizo.FilterStrips.AddNew();
			stripTSB.FilterDescription = TaskStatusFilter.Schema.Identifier;
			var taskStatusFilterB = (TaskStatusFilter)stripTSB.CurrentModuleFilter;
			taskStatusFilterB.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilterB.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilterB.IsActive = true;
			taskStatusFilterB.GroupName = "B";
			taskStatusFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;
			jobOrWorkflowB.SetWorkflowOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 4, filterBizo.FilterStrips.Count);
			AssertEquals("active", 4, filterBizo.ActiveModuleFilters.Count);
			AssertNotContains("UNION ALL", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_Group_WorkflowFirst()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilterA = (TaskStatusFilter)filterBizo.FilterStrips.AddNew(TaskStatusFilter.Schema.Identifier).CurrentModuleFilter;
			taskStatusFilterA.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilterA.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilterA.GroupName = "A";
			taskStatusFilterA.GroupOrCategory = FilterOrCategory.Red;

			var jobOrWorkflowA = (JobOrWorkflowFilter)filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.JobOrWorkflow).CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;

			var taskStatusFilterB = (TaskStatusFilter)filterBizo.FilterStrips.AddNew(TaskStatusFilter.Schema.Identifier).CurrentModuleFilter;
			taskStatusFilterB.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilterB.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilterB.GroupName = "B";
			taskStatusFilterB.GroupOrCategory = FilterOrCategory.Green;

			var jobOrWorkflowB = (JobOrWorkflowFilter)filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.JobOrWorkflow).CurrentModuleFilter;
			jobOrWorkflowB.SetJobOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 4, filterBizo.FilterStrips.Count);
			AssertEquals("active", 4, filterBizo.ActiveModuleFilters.Count);
			AssertNotContains("UNION ALL", filterBizo.Filter.LiteralTextSqlFormatted.ToUpper());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_OrCategory()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripTSA = filterBizo.FilterStrips.AddNew();
			stripTSA.FilterDescription = TaskStatusFilter.Schema.Identifier;
			var taskStatusFilterA = (TaskStatusFilter)stripTSA.CurrentModuleFilter;
			taskStatusFilterA.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilterA.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilterA.IsActive = true;
			taskStatusFilterA.GroupName = "A";
			taskStatusFilterA.GroupOrCategory = FilterOrCategory.Red;
			taskStatusFilterA.OrCategory = FilterOrCategory.Yellow;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.OrCategory = FilterOrCategory.Yellow;
			jobOrWorkflowA.IsActive = true;

			var stripTSAB = filterBizo.FilterStrips.AddNew();
			stripTSAB.FilterDescription = TaskStatusFilter.Schema.Identifier;
			var taskStatusFilterAB = (TaskStatusFilter)stripTSAB.CurrentModuleFilter;
			taskStatusFilterAB.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilterAB.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilterAB.IsActive = true;
			taskStatusFilterAB.GroupName = "A";
			taskStatusFilterAB.GroupOrCategory = FilterOrCategory.Red;
			taskStatusFilterAB.OrCategory = FilterOrCategory.Blue;

			var stripTSB = filterBizo.FilterStrips.AddNew();
			stripTSB.FilterDescription = TaskStatusFilter.Schema.Identifier;
			var taskStatusFilterB = (TaskStatusFilter)stripTSB.CurrentModuleFilter;
			taskStatusFilterB.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilterB.TaskStatusCheckList.First(l => l.Description == ProcessTaskStatusCodeList.Codes.Open).Value = true;
			taskStatusFilterB.IsActive = true;
			taskStatusFilterB.GroupName = "B";
			taskStatusFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;
			jobOrWorkflowB.SetJobOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 5, filterBizo.FilterStrips.Count);
			AssertEquals("active", 5, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(2, filterBizo.Filter.LiteralTextSqlFormatted.ToUpper().AllIndexesOf("UNION ALL").Count());
		}

		public void TestGeneratedFilterQuery_WithAllAggregator_WithJobFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetJobOnly();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Job Only query should use ProcessHeader ParentID", "FH_ParentID NOT IN", filterQuery);
					AssertNotContains("Job Only query should not use ProcessHeader PK", "FH_PK NOT IN", filterQuery);

					AssertEquals("Job Only subquery should SELECT ParentID", 2, filterQuery.AllIndexesOf("SELECT P9_ParentID FROM dbo.ProcessTasks").Count());
					AssertNotContains("Job Only subquery should not SELECT ProcessHeader", "SELECT FH_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertEquals("Job query with ALL aggregator should contain two SELECT statement", 2, filterQuery.AllIndexesOf("SELECT").Count());

					AssertNotContains("First subquery should not be used with Job Only filter", "SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertEquals("Only one UNION requred from ALL aggregator (first subquery should not be included with Job Only filter)", 1, filterQuery.AllIndexesOf("UNION ALL").Count());
				});

				AssertModuleResults(module, "JobHeader2");
			}
		}

		public void TestGeneratedFilterQuery_WithAnyAggregator_WithJobFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetJobOnly();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Job Only query should use ProcessHeader ParentID", "FH_ParentID IN", filterQuery);
					AssertNotContains("Job Only query should not use ProcessHeader PK", "FH_PK IN", filterQuery);

					AssertContains("Job Only subquery should SELECT ParentID", "SELECT P9_ParentID FROM dbo.ProcessTasks", filterQuery);
					AssertNotContains("Job Only subquery should not SELECT ProcessHeader", "SELECT FH_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertEquals("Job query with ANY aggregator should contain a single SELECT statement", 1, filterQuery.AllIndexesOf("SELECT").Count());

					AssertNotContains("First subquery should not be used with Job Only filter", "SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertNotContains("No UNION required as first subquery should not be used with Job Only filter", "UNION ALL", filterQuery);
				});

				AssertModuleResults(module, "JobHeader1", "JobHeader2", "JobHeader3");
			}
		}

		public void TestGeneratedFilterQuery_WithNoneAggregator_WithJobFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetJobOnly();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Job Only query should use ProcessHeader ParentID", "FH_ParentID NOT IN", filterQuery);
					AssertNotContains("Job Only query should not use ProcessHeader PK", "FH_PK NOT IN", filterQuery);

					AssertContains("Job Only subquery should SELECT ParentID", "SELECT P9_ParentID FROM dbo.ProcessTasks", filterQuery);
					AssertNotContains("Job Only subquery should not SELECT ProcessHeader", "SELECT FH_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertEquals("Job query with ANY aggregator should contain a single SELECT statement", 1, filterQuery.AllIndexesOf("SELECT").Count());

					AssertNotContains("First subquery should not be used with Job Only filter", "SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertNotContains("No UNION required as first subquery should not be used with Job Only filter", "UNION ALL", filterQuery);
				});

				AssertModuleResults(module, "JobHeader4");
			}
		}

		public void TestGeneratedFilterQuery_WithAllAggregator_WithWorkflowFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetWorkflowOnly();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Workflow Only query should use ProcessHeader PK", "FH_PK NOT IN", filterQuery);
					AssertNotContains("Workflow Only should not use ProcessHeader ParentID", "FH_ParentID NOT IN", filterQuery);

					AssertEquals("Workflow Only subquery should SELECT ProcessHeader", 2, filterQuery.AllIndexesOf("SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks").Count());
					AssertNotContains("Workflow Only subquery shoud not SELECT ParentID", "SELECT P9_ParentID FROM dbo.ProcessTasks", filterQuery);
					AssertEquals("Workflow query with ALL aggregator should contain two SELECT statements", 2, filterQuery.AllIndexesOf("SELECT").Count());

					AssertNotContains("Second subquery should not be used with Workflow Only filter", "SELECT FH_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertEquals("Only one UNION requred from ALL aggregator (second subquery should not be included with Workflow Only filter)", 1, filterQuery.AllIndexesOf("UNION ALL").Count());
				});

				AssertModuleResults(module, "Workflow1", "Workflow3", "Workflow4");
			}
		}

		public void TestGeneratedFilterQuery_WithAnyAggregator_WithWorkflowFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetWorkflowOnly();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Workflow Only query should use ProcessHeader PK", "FH_PK IN", filterQuery);
					AssertNotContains("Workflow Only should not use ProcessHeader ParentID", "FH_ParentID IN", filterQuery);

					AssertContains("Workflow Only subquery should SELECT ProcessHeader", "SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertNotContains("Workflow Only subquery shoud not SELECT ParentID", "SELECT P9_ParentID FROM dbo.ProcessTasks", filterQuery);
					AssertEquals("Workflow query with ANY aggregator should contain a single SELECT statement", 1, filterQuery.AllIndexesOf("SELECT").Count());

					AssertNotContains("Second subquery should not be used with Workflow Only filter", "SELECT FH_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertNotContains("No UNION required as second subquery should not be used with Workflow Only filter", "UNION ALL", filterQuery);
				});

				AssertModuleResults(module, "Workflow1", "Workflow2", "Workflow3", "Workflow4", "Workflow6");
			}
		}

		public void TestGeneratedFilterQuery_WithNoneAggregator_WithWorkflowFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetWorkflowOnly();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Workflow Only query should use ProcessHeader PK", "FH_PK NOT IN", filterQuery);
					AssertNotContains("Workflow Only should not use ProcessHeader ParentID", "FH_ParentID NOT IN", filterQuery);

					AssertContains("Workflow Only subquery should SELECT ProcessHeader", "SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertNotContains("Workflow Only subquery shoud not SELECT ParentID", "SELECT P9_ParentID FROM dbo.ProcessTasks", filterQuery);
					AssertEquals("Workflow query with ANY aggregator should contain a single SELECT statement", 1, filterQuery.AllIndexesOf("SELECT").Count());

					AssertNotContains("Second subquery should not be used with Workflow Only filter", "SELECT FH_FH_ProcessHeader FROM dbo.ProcessTasks", filterQuery);
					AssertNotContains("No UNION required as second subquery should not be used with Workflow Only filter", "UNION ALL", filterQuery);
				});

				AssertModuleResults(module, "Workflow5", "Workflow7", "Workflow8");
			}
		}

		public void TestGeneratedFilterQuery_WithAllAggregator_WithJobAndWOrkflowFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetJobAndWorkflow();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Job and Workflow query should use ProcessHeader PK", "FH_PK IN", filterQuery);
					AssertNotContains("Job and Workflow query should not use ProcessHeader ParentID", "FH_ParentID IN", filterQuery);

					AssertEquals("Job and Workflow subquery with ALL aggregator should SELECT ProcessHeader four times", 4, filterQuery.AllIndexesOf("SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks").Count());
					AssertContains("Job and Workflow subquery should also SELECT ParentHeader", "SELECT FH_FH_ParentHeader FROM dbo.ProcessHeader", filterQuery);
					AssertEquals("Job and Workflow query with ALL aggregator should contain five SELECT statement", 5, filterQuery.AllIndexesOf("SELECT").Count());

					AssertEquals("Job and Workflow query with ALL aggregator should contain three UNION statement", 3, filterQuery.AllIndexesOf("UNION ALL").Count());
				});

				AssertModuleResults(module, "JobHeader2", "Workflow1", "Workflow3", "Workflow4");
			}
		}

		public void TestGeneratedFilterQuery_WithAnyAggregator_WithJobAndWorkflowFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetJobAndWorkflow();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Job and Workflow query should use ProcessHeader PK", "FH_PK IN", filterQuery);
					AssertNotContains("Job and Workflow query should not use ProcessHeader ParentID", "FH_ParentID IN", filterQuery);

					AssertEquals("Job and Workflow subquery with ANY aggregator should SELECT ProcessHeader twice", 2, filterQuery.AllIndexesOf("SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks").Count());
					AssertContains("Job and Workflow subquery should also SELECT ParentHeader", "SELECT FH_FH_ParentHeader FROM dbo.ProcessHeader", filterQuery);
					AssertEquals("Job and Workflow query with ANY aggregator should contain three SELECT statement", 3, filterQuery.AllIndexesOf("SELECT").Count());

					AssertEquals("Job and Workflow query with ANY aggregator should contain one UNION statement", 1, filterQuery.AllIndexesOf("UNION ALL").Count());
				});

				AssertModuleResults(module, "JobHeader1", "JobHeader2", "JobHeader3", "Workflow1", "Workflow2", "Workflow3", "Workflow4", "Workflow6");
			}
		}

		public void TestGeneratedFilterQuery_WithNoneAggregator_WithJobAndWorkflowFilterSet_ShouldUnionQueriesCorrectly_AndRetrieveCorrectRows()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject;

				var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;
				TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilter, "BUN");
				taskStatusFilter.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetJobAndWorkflow();
				jobOrWorkflowA.GroupName = "A";

				var filterQuery = taskStatusFilter.Query.LiteralTextADO;
				CombineAssertions(() =>
				{
					AssertContains("Job and Workflow query should use ProcessHeader PK", "FH_PK NOT IN", filterQuery);
					AssertNotContains("Job and Workflow query should not use ProcessHeader ParentID", "FH_ParentID NOT IN", filterQuery);

					AssertEquals("Job and Workflow subquery with NONE aggregator should SELECT ProcessHeader twice", 2, filterQuery.AllIndexesOf("SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks").Count());
					AssertContains("Job and Workflow subquery should also SELECT ParentHeader", "SELECT FH_FH_ParentHeader FROM dbo.ProcessHeader", filterQuery);
					AssertEquals("Job and Workflow query with NONE aggregator should contain three SELECT statement", 3, filterQuery.AllIndexesOf("SELECT").Count());

					AssertEquals("Job and Workflow query with NONE aggregator should contain one UNION statement", 1, filterQuery.AllIndexesOf("UNION ALL").Count());
				});

				AssertModuleResults(module, "JobHeader4", "Workflow5", "Workflow7", "Workflow8");
			}
		}

		#endregion

		#region Validation

		public void TestValidation()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilter.IsActive = true;

			taskStatusFilter.Validation.ValidateAll();

			AssertNoErrors(taskStatusFilter);

			taskStatusFilter.TaskAggregator = "ANG";
			AssertHasError(taskStatusFilter.TaskAggregatorInfo, "Enter a valid selection.");

			taskStatusFilter.TaskAggregator = "";
			AssertHasError(taskStatusFilter.TaskAggregatorInfo, "Please enter a value.");
		}

		#endregion

		#region Query Optimisation

		public void TestGeneratedFilterQuery_ShouldExcludeCorrectTaskTypes()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;
			TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);

			var filterQuery = taskStatusFilter.Query.ParameterisedText.ParameterisedQueryText.ToUpper();

			CombineAssertions(() =>
			{
				AssertContains("P9_TYPE <> 'MIL'", filterQuery);
				AssertContains("P9_TYPE <> 'TRG'", filterQuery);
				AssertContains("P9_TYPE <> 'EXC'", filterQuery);
			});
		}

		public void TestGeneratedFilterQuery_IsValid_WhenNoTypeSpecified()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;
			TickFilterStatusCode(taskStatusFilter, ProcessTaskStatusCodeList.Codes.Open);

			var queryText = taskStatusFilter.Query.ParameterisedText.ParameterisedQueryText;

			AssertNotContains("Task Type Filter should not use literal filter clause", "P9_Type IN", queryText);
		}

		public void TestGeneratedFilterQuery_ShouldUseTVP_ForTaskTypes()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;

			TickFilterStatusCodes(taskStatusFilter, new string[] { ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Assigned });
			TickFilterTaskTypeCodes(taskStatusFilter, new string[] { "BUN", "COM", "UDF" });

			var queryText = taskStatusFilter.Query.ParameterisedText.ParameterisedQueryText;

			CombineAssertions(() =>
			{
				AssertNotContains("Task Type Filter should not use literal filter clause", "P9_Type IN ('BUN','COM','UDF)", queryText);
				AssertContains("Task Type Filter should use table valued parameters", "P9_Type IN (SELECT Value FROM @", queryText);
			});
		}

		public void TestGeneratedFilterQuery_ShouldNotUseTVP_ForTaskStatus()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var taskStatusFilter = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;

			TickFilterStatusCodes(taskStatusFilter, new string[] { ProcessTaskStatusCodeList.Codes.Open, ProcessTaskStatusCodeList.Codes.Assigned });
			TickFilterTaskTypeCodes(taskStatusFilter, new string[] { "BUN", "COM", "UDF" });

			taskStatusFilter.IsActive = true;
			var queryText = taskStatusFilter.Query.ParameterisedText.ParameterisedQueryText;

			CombineAssertions(() =>
			{
				AssertContains("Task Status Filter should use table valued parameters", "P9_Status IN ('OPN' /* Parameterised value literalised by ZNonPersistentDataQuery */,'ASN' /* Parameterised value literalised by ZNonPersistentDataQuery */)", queryText);
				AssertNotContains("Task Status Filter should not use table valued parameters", "P9_Status IN (SELECT Value FROM @", queryText);
			});
		}

		public void TestGeneratedFilterQuery_WithMultipleTypeFilters_ShouldEvaluateQueryWithNoExceptions_AndRetrieveRowsCorrectly()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject as ProcessHeaderFilterBusinessObject;

				var taskStatusFilterA = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilterA.TaskAggregator = TaskStatusAggregatorList.Codes.All;
				TickFilterStatusCode(taskStatusFilterA, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilterA, "BUN");
				taskStatusFilterA.GroupName = "A";

				var taskStatusFilterB = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilterB.TaskAggregator = TaskStatusAggregatorList.Codes.All;
				TickFilterStatusCode(taskStatusFilterB, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilterB, "BUN");
				taskStatusFilterB.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetJobAndWorkflow();
				jobOrWorkflowA.GroupName = "A";
				jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
				jobOrWorkflowA.OrCategory = FilterOrCategory.Yellow;

				AssertNoExceptionThrown("Module with multiple filters should load successfully", () =>
				{
					//If this test fails, ensure the Parameter used for P9_Type has a different identifier for each filterstrip
					AssertModuleResults(module, "JobHeader2", "Workflow1", "Workflow3", "Workflow4");
				});
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizo = module.FilterBusinessObject as ProcessHeaderFilterBusinessObject;

				var taskStatusFilterA = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilterA.TaskAggregator = TaskStatusAggregatorList.Codes.Any;
				TickFilterStatusCode(taskStatusFilterA, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilterA, "BUN");
				taskStatusFilterA.GroupName = "A";

				var taskStatusFilterB = filterBizo.AddFilterStrip<TaskStatusFilter>(TaskStatusFilter.Schema.Identifier);
				taskStatusFilterB.TaskAggregator = TaskStatusAggregatorList.Codes.Any;
				TickFilterStatusCode(taskStatusFilterB, ProcessTaskStatusCodeList.Codes.Open);
				TickFilterTaskTypeCode(taskStatusFilterB, "BUN");
				taskStatusFilterB.GroupName = "A";

				var jobOrWorkflowA = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
				jobOrWorkflowA.SetJobAndWorkflow();
				jobOrWorkflowA.GroupName = "A";
				jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
				jobOrWorkflowA.OrCategory = FilterOrCategory.Yellow;

				AssertNoExceptionThrown("Module with multiple filters should load successfully", () =>
				{
					//If this test fails, ensure the Parameter used for P9_Type has a different identifier for each filterstrip
					AssertModuleResults(module, "JobHeader1", "JobHeader2", "JobHeader3", "Workflow1", "Workflow2", "Workflow3", "Workflow4", "Workflow6");
				});
			}
		}

		#endregion

		#region Implementation

		static void AssertModuleResults(string failureMessage, ZFilterModule module, params string[] expectedCompletionStatements)
		{
			module.PerformSearch_ForTest();
			AssertContainsExactElementsInAnyOrder(expectedCompletionStatements, module.GridCollection.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));
		}

		static void AssertModuleResults(ZFilterModule module, params string[] expectedCompletionStatements)
		{
			AssertModuleResults("", module, expectedCompletionStatements);
		}

		static void TickFilterStatusCode(TaskStatusFilter filter, string code)
		{
			filter.TaskStatusCheckList.First(l => l.Description == code).Value = true;
		}

		static void TickFilterStatusCodes(TaskStatusFilter filter, params string[] codes)
		{
			foreach (var code in codes)
			{
				TickFilterStatusCode(filter, code);
			}
		}

		static void TickFilterTaskTypeCode(TaskStatusFilter filter, string code)
		{
			filter.TaskTypeCheckList.First(l => l.Description.StartsWith(code)).Value = true;
		}

		static void TickFilterTaskTypeCodes(TaskStatusFilter filter, params string[] codes)
		{
			foreach (var code in codes)
			{
				TickFilterTaskTypeCode(filter, code);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			return (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
		}

		ProcessJobHeader jobHeader1;
		ProcessJobHeader jobHeader2;
		ProcessJobHeader jobHeader3;
		ProcessJobHeader jobHeader4;

		ProcessHeader workflow1;
		ProcessHeader workflow2;
		ProcessHeader workflow3;
		ProcessHeader workflow4;
		ProcessHeader workflow5;
		ProcessHeader workflow6;
		ProcessHeader workflow7;
		ProcessHeader workflow8;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "COM", "BUN");

			jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader1");
			workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow1");
			workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow2");
			BMSTestHelper.CreateTask(workflow1, description: "Task1", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			BMSTestHelper.CreateTask(workflow2, description: "Task2", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			BMSTestHelper.CreateTask(workflow2, description: "Task3", taskType: "COM", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader2");
			workflow3 = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow3");
			workflow4 = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow4");
			BMSTestHelper.CreateTask(workflow3, description: "Task4", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			BMSTestHelper.CreateTask(workflow4, description: "Task5", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			BMSTestHelper.CreateTask(workflow4, description: "Task6", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader3");
			workflow5 = BMSTestHelper.CreateWorkflow(jobHeader3, "Workflow5");
			workflow6 = BMSTestHelper.CreateWorkflow(jobHeader3, "Workflow6");
			BMSTestHelper.CreateTask(workflow5, description: "Task7", taskType: "COM", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			BMSTestHelper.CreateTask(workflow6, description: "Task8", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			BMSTestHelper.CreateTask(workflow6, description: "Task9", taskType: "COM", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader4");
			workflow7 = BMSTestHelper.CreateWorkflow(jobHeader4, "Workflow7");
			workflow8 = BMSTestHelper.CreateWorkflow(jobHeader4, "Workflow8");
			BMSTestHelper.CreateTask(workflow7, description: "Task10", taskType: "COM", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			BMSTestHelper.CreateTask(workflow8, description: "Task11", taskType: "COM", taskStatus: ProcessTaskStatusCodeList.Codes.Open);
			BMSTestHelper.CreateTask(workflow8, description: "Task12", taskType: "COM", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();
		}

		#endregion
	}
}
