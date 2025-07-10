using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessHeaderFilterBusinessObject))]
	class ProcessHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Current Task Only Filter

		public void TestJobWorkflowsModule_WhenHasCurrentTaskOnlyFilter_ShouldBeValid()
		{
			AssertJobWorkflowsModule_WhenHasCurrentTaskOnlyFilter_ShouldBeValid();
		}

		public void TestJobWorkflowsModule_WhenHasCurrentTaskOnlyFilter_QIDisabled_ShouldBeValid()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertJobWorkflowsModule_WhenHasCurrentTaskOnlyFilter_ShouldBeValid();
		}

		void AssertJobWorkflowsModule_WhenHasCurrentTaskOnlyFilter_ShouldBeValid()
		{
			AssertWhenHasCurrentTaskOnlyFilter_ShouldBeValid(
				new ProcessHeaderFilterBusinessObject(),
				addCurrentTaskFilter: (filterBizO) =>
				{
					var tasksFilter = ((ProcessHeaderFilterBusinessObject)filterBizO).AddFilterStrip<ModuleGuidForeignCollectionFilter>("Tasks");
					tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
					var currentTaskOnlyFilter = tasksFilter.SelectedFilters.AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
					currentTaskOnlyFilter.Property0 = true;
				}
			);
		}

		public void TestJobWorkflowsModule_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldBeValid()
		{
			AssertJobWorkflowsModule_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldBeValid();
		}

		public void TestJobWorkflowsModule_WhenHasUserDefinedCurrentTaskOnlyFilter_QIDisabled_ShouldBeValid()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertJobWorkflowsModule_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldBeValid();
		}

		void AssertJobWorkflowsModule_WhenHasUserDefinedCurrentTaskOnlyFilter_ShouldBeValid()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var filterBizO = module.FilterBusinessObject;

				var tasksFilter = filterBizO.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Tasks");
				tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
				var currentTaskOnlyFilter = tasksFilter.SelectedFilters.AddFilterStrip<ModuleFlagsFilter>("Current Task Only");
				currentTaskOnlyFilter.Property0 = true;

				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "User Defined Filter X", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
			}

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				var currentFilterBizO = module.FilterBusinessObject;
				AssertWhenHasCurrentTaskOnlyFilter_ShouldBeValid(
					currentFilterBizO,
					addCurrentTaskFilter: (filterBizO) =>
					{
						((ProcessHeaderFilterBusinessObject)filterBizO).AddFilterStrip<ModuleUserDefinedFilter>("[USR]User Defined Filter X");
					}
				);
			}
		}

		void AssertWhenHasCurrentTaskOnlyFilter_ShouldBeValid(FilterBusinessObject filterBizO, Action<FilterBusinessObject> addCurrentTaskFilter)
		{
			AssertNoErrors("Precondition: filterBizO.HasErrors", filterBizO);

			addCurrentTaskFilter(filterBizO);

			var getCurrentTasks = ObjectFactory.Get<IBMSQLFunctionHelper>().GetCurrentTasks;
			Assert($"FilterBusinessObject should contain {getCurrentTasks}()", filterBizO.Filter.LiteralTextADO.Contains($"{getCurrentTasks}()"));

			filterBizO.RunPreSaveValidation();

			AssertNoErrors("WHEN Module has CurrentTaskOnly filter THEN should valid", filterBizO);
		}

		#endregion

		#region Allow Company Filters In Tag Rules Registry

		public void TestAllowCompanyFiltersInTagRulesRegistry()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow N");
			var task = BMSTestHelper.CreateTask(workflow, description: "Task N", taskType: "BUN", taskStatus: ProcessTaskStatusCodeList.Codes.Open);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];

			foreach (var moduleName in filter.ModuleOptions.GetAllCodes())
			{
				SetFilterStrips(filterBizo, moduleName);
				Factory.Save();

				var notAllowCompanyFiltersSQLCommands = AssertLoadFilterBizOAndGetSQLCommands(filterBizo.Filter, allowCompanyFiltersInTagRules: false);
				var allowCompanyFiltersSQLCommands = AssertLoadFilterBizOAndGetSQLCommands(filterBizo.Filter, allowCompanyFiltersInTagRules: true);
				AssertContainsExactElementsInAnyOrder(
					$"Module = '{moduleName}': AllowCompanyFilters and NotAllowCompanyFilters should have same SQL commands, ",
					notAllowCompanyFiltersSQLCommands,
					allowCompanyFiltersSQLCommands);
			}
		}

		IEnumerable<string> AssertLoadFilterBizOAndGetSQLCommands(ZQuery query, bool allowCompanyFiltersInTagRules)
		{
			BMSRegistry.Instance.AllowCompanyFiltersInTagRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowCompanyFiltersInTagRules);

			using (TestConnection.TrackExecutedCommands())
			{
				var factory = new BusinessObjectFactory();
				var results = factory.Load<ProcessHeader>(query);
				Assert("should find Workflow", results.Any(result => result.FH_CompletionStatement == "Workflow N"));

				return TestConnection.ExecutedCommands.Where(sqlCommand => sqlCommand.Contains("Workflow N"));
			}
		}

		void SetFilterStrips(ProcessHeaderFilterBusinessObject filterBizo, string moduleName)
		{
			var parentJobFilter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];
			parentJobFilter.SelectedModule = moduleName;
			parentJobFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			parentJobFilter.IsActive = true;
			parentJobFilter.OrCategory = FilterOrCategory.Red;

			var customSqlFilter = (ModuleTextFilter)filterBizo[ProcessHeader.ModuleFilterConstants.CompletionStatement];
			customSqlFilter.Property = "Workflow N";
			customSqlFilter.IsActive = true;
			customSqlFilter.OrCategory = FilterOrCategory.Red;
		}

		#endregion

		#region Buffer Zone

		[RequiresLargeLogFile]
		[TestDate(2015, 7, 14)]
		public void TestBufferZoneFilter()
		{
			var sql = @"
INSERT INTO RefDatabase_RefUNLOCOUtcOffset ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc])
VALUES (newid(),'AUSYD','2015-01-01 00:00:00','2015-10-04 00:00:00',600);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc])
VALUES (newid(),'AUBNE','2015-01-01 00:00:00','2015-10-04 00:00:00',600);";
			Db.Connection.ExecuteNonQuery(sql);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);
			var branch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.GB_Code = "SDS";

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 9);
			BMSTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow0 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow in buffer zone 0", buffer, releaseDateTime: ZDateTime.UtcNow.AddMinutes(-10));
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow in buffer zone 1", buffer, releaseDateTime: ZDateTime.UtcNow.AddMinutes(-7));
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow in buffer zone 2", buffer, releaseDateTime: ZDateTime.UtcNow.AddMinutes(-4));
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow in buffer zone 3", buffer, releaseDateTime: ZDateTime.UtcNow.AddMinutes(-1));
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow in bucket", bucket);

			BMSTestHelper.CreateTask(workflow0);
			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2);
			BMSTestHelper.CreateTask(workflow3);
			BMSTestHelper.CreateTask(workflow4);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals(0, workflow0.BufferZone);
				AssertEquals(1, workflow1.BufferZone);
				AssertEquals(2, workflow2.BufferZone);
				AssertEquals(3, workflow3.BufferZone);
				AssertNull(workflow4.BufferZone);

				var bizo = new ProcessHeaderFilterBusinessObject();
				var filter = (ModuleNumberRangeFilter)bizo[ProcessHeader.ModuleFilterConstants.BufferZone];

				filter.IsActive = true;

				filter.Property1 = 0;
				AssertContainsExactElementsInAnyOrder("Workflows in zone 0 should be returned", new[] { workflow0 }, Factory.Load<ProcessHeader>(filter.Query));

				filter.Property2 = 1;
				AssertContainsExactElementsInAnyOrder("Workflows in zone 0 and 1 should be returned", new[] { workflow0, workflow1 }, Factory.Load<ProcessHeader>(filter.Query));

				filter.Property1 = 1;
				filter.Property2 = 2;
				AssertContainsExactElementsInAnyOrder("Workflows in zone 1 and zone 2 should be returned", new[] { workflow1, workflow2 }, Factory.Load<ProcessHeader>(filter.Query));

				filter.Property1 = 2;
				filter.Property2 = 2;
				AssertContainsExactElementsInAnyOrder("Workflows in zone 2 should be returned", new[] { workflow2 }, Factory.Load<ProcessHeader>(filter.Query));

				filter.Property1 = 3;
				filter.Property2 = 3;
				AssertContainsExactElementsInAnyOrder("Workflows in zone 3 should be returned", new[] { workflow3 }, Factory.Load<ProcessHeader>(filter.Query));
			}
		}

		public void TestBufferZoneQueryFallbackLogic_CurrentBranchAndCurrentDepartmentNotUsed()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleNumberRangeFilter)bizo[ProcessHeader.ModuleFilterConstants.BufferZone];

			Assert(!filter.Query.FilterString.Contains("@CurrentBranch"));
			Assert(!filter.Query.FilterString.Contains("@CurrentDepartment"));
		}

		public void TestBufferZoneFilterValidation()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleNumberRangeFilter)bizo[ProcessHeader.ModuleFilterConstants.BufferZone];

			filter.Property1 = -1;
			filter.Property2 = -1;

			AssertHasError(filter.Property1Info, "Please enter a value greater than or equal to 0.");
			AssertHasError(filter.Property2Info, "Please enter a value greater than or equal to 0.");

			filter.Property1 = 0;
			filter.Property2 = 0;

			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			filter.Property1 = 3;
			filter.Property2 = 3;

			AssertNoErrors(filter.Property1Info);
			AssertNoErrors(filter.Property2Info);

			filter.Property1 = 4;
			filter.Property2 = 4;

			AssertHasError(filter.Property1Info, "Please enter a value less than or equal to 3.");
			AssertHasError(filter.Property2Info, "Please enter a value less than or equal to 3.");
		}

		#endregion

		#region Task Filters

		public void TestPrerequisiteStatus_ClosedWithOpenPrerequisites()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "I saw a cow");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "It was pretty");

			var link = BMSTestHelper.CreateDependencyLink(workflow1, workflow2);

			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2).P9_Status = "CLS";

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow2.FH_Status);

			var bizo = new ProcessHeaderFilterBusinessObject();

			var workflowPrerequisitesFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.PrerequisiteStatus];
			workflowPrerequisitesFilter.Property = WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites;
			workflowPrerequisitesFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			workflowPrerequisitesFilter.IsActive = true;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionContains(workflow2, results);
		}

		public void TestPrerequisiteStatus_Blocked()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Help");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "There are aliens");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Attacking my brain");

			BMSTestHelper.CreateDependencyLink(workflow1, workflow2);
			BMSTestHelper.CreateDependencyLink(workflow2, workflow3);

			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2).P9_Status = "CLS";
			BMSTestHelper.CreateTask(workflow3);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow3.FH_Status);

			var bizo = new ProcessHeaderFilterBusinessObject();

			var workflowPrerequisitesFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.PrerequisiteStatus];
			workflowPrerequisitesFilter.Property = WorkflowPrerequisiteStatusList.Codes.Blocked;
			workflowPrerequisitesFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			workflowPrerequisitesFilter.IsActive = true;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, results.Length);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow3, results);
		}

		public void TestPrerequisiteStatus_ClearedToRelease()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Help");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "There are aliens");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Attacking my brain");

			BMSTestHelper.CreateDependencyLink(workflow1, workflow2);
			BMSTestHelper.CreateDependencyLink(workflow1, workflow3);

			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2).P9_Status = "CLS";
			BMSTestHelper.CreateTask(workflow3);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow3.FH_Status);

			var bizo = new ProcessHeaderFilterBusinessObject();

			var workflowPrerequisitesFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.PrerequisiteStatus];
			workflowPrerequisitesFilter.Property = WorkflowPrerequisiteStatusList.Codes.ClearedToRelease;
			workflowPrerequisitesFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			workflowPrerequisitesFilter.IsActive = true;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, results.Length);
			AssertCollectionContains(workflow1, results);
			AssertCollectionNotContains(workflow2, results);
			AssertCollectionNotContains(workflow3, results);
		}

		public void TestPrerequisiteStatus()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60);

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow1.PrerequisiteStatus);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();

			var workflowPrerequisitesFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.PrerequisiteStatus];
			workflowPrerequisitesFilter.Property = WorkflowPrerequisiteStatusList.Codes.ClearedToStart;
			workflowPrerequisitesFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			workflowPrerequisitesFilter.IsActive = true;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertCollectionContains(workflow1, results);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Neep");
			var task2 = BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 60);
			workflow2.GetOrCreateDependencyLink(workflow1);

			Factory.Save();

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow1, workflow2.PostrequisiteWorkflows);

			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow1.FH_Status);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.Blocked, workflow1.PrerequisiteStatus);
			AssertCollectionNotContains(workflow1, results);

			workflowPrerequisitesFilter.Property = WorkflowPrerequisiteStatusList.Codes.Blocked;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertCollectionContains(workflow1, results);
			AssertCollectionNotContains(workflow2, results);

			workflowPrerequisitesFilter.Property = WorkflowPrerequisiteStatusList.Codes.ClearedToStart;
			workflow1.FH_StaggeredReleaseDelayExpiry = ZDateTime.UtcNow.AddHours(-1);

			Factory.Save();

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionContains(workflow2, results);

			workflowPrerequisitesFilter.Property = WorkflowPrerequisiteStatusList.Codes.ClearedToRelease;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertCollectionContains(workflow1, results);
			AssertCollectionContains(workflow2, results);
		}

		public void TestAggregateTaskStatusFilter()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypeCategory1 = categorisedTaskTypes.AddNew();
			taskTypeCategory1.Code = "ORG";
			var taskType1 = taskTypeCategory1.TaskTypes.AddNew();
			taskType1.Code = "COM";
			var taskType2 = taskTypeCategory1.TaskTypes.AddNew();
			taskType2.Code = "BUN";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeaderWithDifferentTasks = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeaderWithSameTasks = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeaderWithNoTasks = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflowWithDifferentTasks = jobHeaderWithDifferentTasks.ProcessHeaders[0];
			var workflowWithSameTasks = jobHeaderWithSameTasks.ProcessHeaders[0];
			var workflowWithNoTasks = jobHeaderWithNoTasks.ProcessHeaders[0];

			var task1_1 = BMSTestHelper.CreateTask(workflowWithDifferentTasks, staffCode: staff.GS_Code, taskType: taskType1.Code);
			var task1_2 = BMSTestHelper.CreateTask(workflowWithDifferentTasks, staffCode: staff.GS_Code, taskType: taskType2.Code);

			var task2_1 = BMSTestHelper.CreateTask(workflowWithSameTasks, staffCode: staff.GS_Code, taskType: taskType1.Code);
			var task2_2 = BMSTestHelper.CreateTask(workflowWithSameTasks, taskType: taskType1.Code, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			// Test All

			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilter.IsActive = true;

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionNotContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionNotContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionNotContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionNotContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);

			// Test All with mulitple checked

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code, taskType2.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Open);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);

			// Test Any

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Closed);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(workflowWithNoTasks, results);
			AssertCollectionNotContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionNotContains(jobHeaderWithSameTasks, results);
			AssertCollectionNotContains(jobHeaderWithNoTasks, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Cancelled);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(workflowWithNoTasks, results);
			AssertCollectionNotContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionContains(jobHeaderWithSameTasks, results);
			AssertCollectionNotContains(jobHeaderWithNoTasks, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Cancelled);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(workflowWithNoTasks, results);
			AssertCollectionNotContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionContains(jobHeaderWithSameTasks, results);
			AssertCollectionNotContains(jobHeaderWithNoTasks, results);

			// Test Any with multiple checked

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code, taskType2.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Open);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(workflowWithNoTasks, results);
			AssertCollectionContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionContains(jobHeaderWithSameTasks, results);
			AssertCollectionNotContains(jobHeaderWithNoTasks, results);

			// Test None

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Closed);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Cancelled);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionNotContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Cancelled);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionNotContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);

			// Test None with multiple checked

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code, taskType2.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Open);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionContains(workflowWithNoTasks, results);
			AssertCollectionNotContains(jobHeaderWithDifferentTasks, results);
			AssertCollectionNotContains(jobHeaderWithSameTasks, results);
			AssertCollectionContains(jobHeaderWithNoTasks, results);
		}

		public void TestAggregateTaskStatusFilter_NON_OneJobMultipleWorkflows()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypeCategory1 = categorisedTaskTypes.AddNew();
			taskTypeCategory1.Code = "ORG";
			var taskType1 = taskTypeCategory1.TaskTypes.AddNew();
			taskType1.Code = "COM";
			var taskType2 = taskTypeCategory1.TaskTypes.AddNew();
			taskType2.Code = "BUN";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflowWithDifferentTasks = jobHeader.ProcessHeaders[0];
			var workflowWithSameTasks = jobHeader.ProcessHeaders.AddNew();

			var task1_1 = BMSTestHelper.CreateTask(workflowWithDifferentTasks, staffCode: staff.GS_Code, taskType: taskType1.Code);
			var task1_2 = BMSTestHelper.CreateTask(workflowWithDifferentTasks, staffCode: staff.GS_Code, taskType: taskType2.Code);

			var task2_1 = BMSTestHelper.CreateTask(workflowWithSameTasks, staffCode: staff.GS_Code, taskType: taskType1.Code);
			var task2_2 = BMSTestHelper.CreateTask(workflowWithSameTasks, taskType: taskType1.Code, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Closed);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;
			taskStatusFilter.IsActive = true;

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionContains(jobHeader, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Working);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionContains(jobHeader, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Cancelled);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(jobHeader, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Cancelled);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(jobHeader, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code, taskType2.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Open);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(jobHeader, results);
		}

		public void TestAggregateTaskStatusFilter_ALL_OneJobMultipleWorkflows()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypeCategory1 = categorisedTaskTypes.AddNew();
			taskTypeCategory1.Code = "ORG";
			var taskType1 = taskTypeCategory1.TaskTypes.AddNew();
			taskType1.Code = "COM";
			var taskType2 = taskTypeCategory1.TaskTypes.AddNew();
			taskType2.Code = "BUN";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflowWithDifferentTasks = jobHeader.ProcessHeaders[0];
			var workflowWithSameTasks = jobHeader.ProcessHeaders.AddNew();

			var task1_1 = BMSTestHelper.CreateTask(workflowWithDifferentTasks, staffCode: staff.GS_Code, taskType: taskType1.Code);
			var task1_2 = BMSTestHelper.CreateTask(workflowWithDifferentTasks, staffCode: staff.GS_Code, taskType: taskType2.Code);

			var task2_1 = BMSTestHelper.CreateTask(workflowWithSameTasks, staffCode: staff.GS_Code, taskType: taskType1.Code);
			var task2_2 = BMSTestHelper.CreateTask(workflowWithSameTasks, taskType: taskType1.Code, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Closed);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			taskStatusFilter.IsActive = true;

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(jobHeader, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionNotContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(jobHeader, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionNotContains(jobHeader, results);

			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskTypeCheckList, taskType1.Code, taskType2.Code);
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Cancelled, ProcessTaskStatusCodeList.Codes.Open);
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflowWithDifferentTasks, results);
			AssertCollectionContains(workflowWithSameTasks, results);
			AssertCollectionContains(jobHeader, results);
		}

		public void TestTaskStatusFilter_IgnoresMilestonesAndTriggers()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Workflow 1";

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var milestone = job.WorkflowItems.AddNew();
			milestone.P9_FH_ProcessHeader = workflow.PK;
			milestone.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			milestone.IsMilestone = true;

			var trigger = job.WorkflowItems.AddNew();
			trigger.P9_FH_ProcessHeader = workflow.PK;
			trigger.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			trigger.IsWorkflowTrigger = true;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.Any;
			taskStatusFilter.IsActive = true;

			// Workflows with Closed tasks
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Closed);

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflow, results);
			AssertEquals(2, results.Length);

			// Workflows with Cancelled tasks
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Cancelled);

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains("Only the trigger is cancelled, so workflow should not be detected.", workflow, results);
			AssertEquals(0, results.Length);

			// Workflows with Open tasks
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Open);

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionNotContains("Only the milestone is open, so workflow should not be detected.", workflow, results);
			AssertEquals(0, results.Length);

			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.All;
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Closed);

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains("Only the milestones and triggers don't match, so workflow should be detected.", workflow, results);

			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Open);

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains("Only the milestones match, so workflow should be detected.", workflow, results);
		}

		public void TestTaskStatusFilter_NullTasksDontRuin()
		{
			var taskWithoutHeader = Factory.NewWithValidTestData<ProcessTask>();
			taskWithoutHeader.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			taskWithoutHeader.P9_FH_ProcessHeader = ZGuid.Empty;

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Workflow 1";

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var taskStatusFilter = (TaskStatusFilter)filterBizo[TaskStatusFilter.Schema.Identifier];
			taskStatusFilter.IsActive = true;
			taskStatusFilter.TaskAggregator = TaskStatusAggregatorList.Codes.None;
			BMSTestHelper.SetValuesExclusive(taskStatusFilter.TaskStatusCheckList, ProcessTaskStatusCodeList.Codes.Open);

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertCollectionContains(workflow, results);
			AssertEquals(2, results.Length);
		}

		#endregion

		#region Workflow Filters

		public void TestFH_Status()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buff");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "bulk");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "neep");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "neep");
			BMSTestHelper.CreateDependencyLink(workflow1, workflow2);
			var task1 = BMSTestHelper.CreateTask(workflow1, taskStatus: "OPN");
			BMSTestHelper.CreateTask(workflow2, taskStatus: "CAN");

			Factory.Save();

			AssertEquals("Precondition: Worfklow1 is OPN", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: Worfklow2 is COP", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow2.FH_Status);

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.WorkflowStatus];
			filter.IsActive = true;
			filter.Property = WorkflowStatusList.Codes.Closed;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertCollectionNotContains("OPN is not closed", workflow1, results);
			AssertCollectionContains("COP counts as closed", workflow2, results);

			task1.P9_Status = "CAN";
			Factory.Save();

			AssertEquals("Precondition: Worfklow1 is cls", WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals("Precondition: Worfklow2 is cls", WorkflowStatusList.Codes.Closed, workflow2.FH_Status);

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertCollectionContains("CLS is closed", workflow1, results);
			AssertCollectionContains("CLS is closed", workflow2, results);
		}

		public void TestFH_Status_Close()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowOpen = BMSTestHelper.CreateWorkflow(jobHeader, "workflowOpen");
			var workflowCloseWithPrereq = BMSTestHelper.CreateWorkflow(jobHeader, "workflowCloseWithPrereq");
			var workflowClosed = BMSTestHelper.CreateWorkflow(jobHeader, "workflowClosed");
			BMSTestHelper.CreateDependencyLink(workflowOpen, workflowCloseWithPrereq);
			var workflowBlocked = BMSTestHelper.CreateWorkflow(jobHeader, "workflowBlocked");
			BMSTestHelper.CreateDependencyLink(workflowOpen, workflowBlocked);

			BMSTestHelper.CreateTask(workflowOpen, taskStatus: "OPN");
			BMSTestHelper.CreateTask(workflowCloseWithPrereq, taskStatus: "CAN");
			BMSTestHelper.CreateTask(workflowClosed, taskStatus: "CLS");
			BMSTestHelper.CreateTask(workflowBlocked, taskStatus: "OPN");

			Factory.Save();

			AssertEquals("Precondition: Worfklow1 is OPN", WorkflowStatusList.Codes.Open, workflowOpen.FH_Status);
			AssertEquals("Precondition: Worfklow2 is COP", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflowCloseWithPrereq.FH_Status);
			AssertEquals("Precondition: Worfklow2 is CLS", WorkflowStatusList.Codes.Closed, workflowClosed.FH_Status);
			AssertEquals("Precondition: Worfklow2 is BLK", WorkflowStatusList.Codes.Blocked, workflowBlocked.FH_Status);

			var processHeaderFilter = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)processHeaderFilter[ProcessHeader.ModuleFilterConstants.WorkflowStatus];
			filter.IsActive = true;
			filter.Property = WorkflowStatusList.Codes.Closed;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			var results = Factory.Load<ProcessHeader>(processHeaderFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workflowCloseWithPrereq, workflowClosed }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			results = Factory.Load<ProcessHeader>(processHeaderFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workflowCloseWithPrereq, workflowClosed }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			results = Factory.Load<ProcessHeader>(processHeaderFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workflowCloseWithPrereq, workflowClosed }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			results = Factory.Load<ProcessHeader>(processHeaderFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflowOpen, workflowBlocked }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			results = Factory.Load<ProcessHeader>(processHeaderFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflowOpen, workflowBlocked }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			results = Factory.Load<ProcessHeader>(processHeaderFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflowOpen, workflowBlocked }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.Property = null;
			results = Factory.Load<ProcessHeader>(processHeaderFilter.Filter);
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<ProcessHeader>(), results);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			results = Factory.Load<ProcessHeader>(processHeaderFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflowOpen, workflowClosed, workflowCloseWithPrereq, workflowBlocked }, results);
		}

		public void TestCurrentComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buff");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "bulk");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "neep", buffer1);
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "neep", buffer1);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "neep", buffer2);
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "neep", buffer2);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow3_1 = BMSTestHelper.CreateWorkflow(jobHeader3, "neep", buffer2);
			var workflow3_2 = BMSTestHelper.CreateWorkflow(jobHeader3, "neep", buffer1);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();

			var filter = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.CurrentComponent];
			filter.IsActive = true;
			filter.Property = buffer1.PK;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(5, results.Length);

			AssertCollectionContains(jobHeader1, results);
			AssertCollectionContains(workflow1_1, results);
			AssertCollectionContains(workflow1_2, results);
			AssertCollectionContains(jobHeader3, results);
			AssertCollectionContains(workflow3_2, results);

			filter = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.CurrentComponent];
			filter.IsActive = true;
			filter.Property = buffer2.PK;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(5, results.Length);

			AssertCollectionContains(jobHeader2, results);
			AssertCollectionContains(workflow2_1, results);
			AssertCollectionContains(workflow2_2, results);
			AssertCollectionContains(jobHeader3, results);
			AssertCollectionContains(workflow3_1, results);
		}

		public void TestPlannedDuration()
		{
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, string.Empty, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, string.Empty, 60);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleDurationFilter)bizo[ProcessHeader.ModuleFilterConstants.PlannedDuration];
			filter.IsActive = true;
			filter.MinDurationMinutes = 120;
			filter.MaxDurationMinutes = 180;

			var jobOrWorkflowFilter = (JobOrWorkflowFilter)bizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflowFilter.IsActive = true;
			jobOrWorkflowFilter.SetWorkflowOnly();

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertEquals(workflow, results[0]);

			filter.MaxDurationMinutes = 150;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(0, results.Length);

			filter.MinDurationMinutes = 0;
			filter.MaxDurationMinutes = 180;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertEquals(workflow, results[0]);
		}

		public void TestWorkflowTypeFilter_ComparisonOperatorShouldBeVisible()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader1.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders[0];
			var workflow3 = jobHeader3.ProcessHeaders[0];

			Factory.Save();

			Assert("Prerequisite: FH_WorkflowType should be set in order to make 'Workflow Type' filter work.",
				new ProcessHeader[] { jobHeader1, workflow1 }.All(x => x.FH_WorkflowType == "ORG"));

			Assert("Prerequisite: FH_WorkflowType should be set in order to make 'Workflow Type' filter work.",
				new ProcessHeader[] { jobHeader2, jobHeader3, workflow2, workflow3 }.All(x => x.FH_WorkflowType == "DUM"));

			var bizo = new ProcessHeaderFilterBusinessObject();

			var workflowTypeFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.WorkflowType];
			workflowTypeFilter.Property = "ORG";
			workflowTypeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			workflowTypeFilter.IsActive = true;

			AssertEquals(true, workflowTypeFilter.HasComparisonOperator);

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, workflow1 }, results);

			workflowTypeFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader2, workflow2, jobHeader3, workflow3 }, results);
		}

		public void TestWorkflowTypeFilter_AdditionalParentFiltersDoesNotChangeFilterBehaviour()
		{
			DummyProcessTaskLoadStrategy.AdditionalParentFiltersAdder.Value = (descriptor, subQuery) => subQuery.AddToFilter(DummyBizoSchema.Z0_Code, "USA");

			var dummy1 = Factory.NewWithValidTestData<DiscriminatingDummy>();
			dummy1.Z0_Code = "USA";
			var dummy2 = Factory.NewWithValidTestData<DiscriminatingDummy>();
			dummy2.Z0_Code = "AUS";

			var jobHeader1 = BMSTestHelper.CreateJobHeader(dummy1, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "USA! USA! USA!");

			var jobHeader2 = BMSTestHelper.CreateJobHeader(dummy2, addDefaultProcessHeaderIfNone: false);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Aussie Aussie Aussie!");

			Factory.Save();

			Assert("Prerequisite: FH_WorkflowType should be set in order to make 'Workflow Type' filter work.",
				new ProcessHeader[] { jobHeader1, workflow1, jobHeader2, workflow2 }.All(x => x.FH_WorkflowType == "DUM"));

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var workflowTypeFilter = (ModuleTextFilter)filterBizo[ProcessHeader.ModuleFilterConstants.WorkflowType];

			workflowTypeFilter.IsActive = true;
			workflowTypeFilter.Property = "DUM";

			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, workflow1, jobHeader2, workflow2 }, Factory.Load<ProcessHeader>(filterBizo.Filter));
		}

		public void TestJobOrWorkflowFilter()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var jobOrWorkflowFilter = (JobOrWorkflowFilter)bizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflowFilter.IsActive = true;

			jobOrWorkflowFilter.Property0 = true; // job
			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
			Assert(results.Any(h => h.PK == jobHeader.PK));

			jobOrWorkflowFilter.Property1 = true; // workflow
			AssertEquals("Properties should be mutually exclusive", false, jobOrWorkflowFilter.Property0);
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, results.Length);
			Assert(results.Any(h => h.PK == workflow1.PK));
			Assert(results.Any(h => h.PK == workflow2.PK));

			jobOrWorkflowFilter.Property2 = true; // job AND workflow
			AssertEquals(false, jobOrWorkflowFilter.Property1);
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(3, results.Length);
			Assert(results.Any(h => h.PK == jobHeader.PK));
			Assert(results.Any(h => h.PK == workflow1.PK));
			Assert(results.Any(h => h.PK == workflow2.PK));
		}

		public void TestTemplateFilter()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var templateJobHeader = ProcessJobHeader.GetForParent(template, Factory, false);
			templateJobHeader.FH_P0_Template = template.PK;
			var templateWorkflow1 = templateJobHeader.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "workflow1";
			templateWorkflow1.FH_P0_Template = template.PK;

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var templateFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.Template];
			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("By default templates should be excluded ", 3, results.Length);

			templateFilter.IsActive = true;
			templateFilter.Property = TemplateFilterOptions.Codes.All;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should show all workflows including template", 5, results.Length);

			templateFilter.Property = TemplateFilterOptions.Codes.NonTemplate;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should show non templates only", 3, results.Length);

			var jobOrWorkflowFilter = (JobOrWorkflowFilter)bizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflowFilter.IsActive = true;
			jobOrWorkflowFilter.SetJobOnly();
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should show  non templates and jobs", 1, results.Length);

			templateFilter.Property = TemplateFilterOptions.Codes.Template;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should show templates only", 1, results.Length);

			jobOrWorkflowFilter.SetWorkflowOnly();
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should show  template workflows", 1, results.Length);

			templateFilter.Property = TemplateFilterOptions.Codes.NonTemplate;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should show non template workflows only", 2, results.Length);
		}

		public void TestLoadLayout_WhenJobCodeFilterDefaultsSet_ShouldNotLoadLastUsedLayout()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = bizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];

			var strip = bizo.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(bizo, "Mai Layout", false, false, SaveColumnLayout.Ignore);

			bizo.LoadLayout(savedLayout, true);
			AssertEquals("Mai Layout", bizo.LastUsedLayout.S9_FilterName);
			AssertEquals(true, filter.IsActive);
			filter.IsActive = false;

			bizo.SetExternalDefaults(new FilterBusinessObjectDefaults { new FilterBusinessObjectDefault(ProcessHeader.ModuleFilterConstants.JobCode, "Property", new ZString("WI00000001"), true) });
			bizo.LoadLayout(savedLayout, true);
			filter = bizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			AssertEquals(false, filter.IsActive);
		}

		public void TestStaffFilters()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var frodo = Factory.NewWithValidTestData<GlbStaff>();
			frodo.GS_Code = "FRO";
			var sam = Factory.NewWithValidTestData<GlbStaff>();
			sam.GS_Code = "SAM";

			var header1 = jobHeader.ProcessHeaders.AddNew();
			var task1_1 = org.WorkflowItems.AddNew();
			task1_1.P9_FH_ProcessHeader = header1.PK;
			task1_1.P9_Sequence = 1;
			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1_1.P9_GS_NKAssignedStaffMember = frodo.GS_Code;
			var task1_2 = org.WorkflowItems.AddNew();
			task1_2.P9_FH_ProcessHeader = header1.PK;
			task1_2.P9_Sequence = 2;
			task1_2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1_2.P9_GS_NKAssignedStaffMember = sam.GS_Code;

			var header2 = jobHeader.ProcessHeaders.AddNew();
			var task2_1 = org.WorkflowItems.AddNew();
			task2_1.P9_FH_ProcessHeader = header2.PK;
			task2_1.P9_Sequence = 1;
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2_1.P9_GS_NKAssignedStaffMember = sam.GS_Code;
			var task2_2 = org.WorkflowItems.AddNew();
			task2_2.P9_FH_ProcessHeader = header2.PK;
			task2_2.P9_Sequence = 2;
			task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2_2.P9_GS_NKAssignedStaffMember = frodo.GS_Code;

			AssertEquals(task1_1, org.WorkflowItems.GetCurrentTask(header1));
			AssertEquals(task2_1, org.WorkflowItems.GetCurrentTask(header2));

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var resourceAnyTaskFilter = (ModuleNkFilter)filterBizo["Resource Assigned To Any Task"];
			var resourceCurrentTaskFilter = (ModuleNkFilter)filterBizo["Resource Assigned To Current Task"];

			resourceAnyTaskFilter.IsActive = true;
			resourceAnyTaskFilter.Property = frodo.GS_Code;

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertEquals(3, results.Length);
			AssertCollectionContains(jobHeader, results);
			AssertCollectionContains(header1, results);
			AssertCollectionContains(header2, results);

			resourceAnyTaskFilter.IsActive = false;
			resourceCurrentTaskFilter.IsActive = true;
			resourceCurrentTaskFilter.Property = frodo.GS_Code;

			results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertEquals(2, results.Length);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, header1 }, results);
		}

		public void TestParentWorkflowsFilter_ShouldExist()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = filterBizo["Parent Workflows"];
			AssertEquals(typeof(ParentWorkflowsFilter), filter.GetType());
			AssertEquals(filter.MultilingualDescription, "Parent Workflows");

			// Actual functionality of this filter is tested in ParentWorkflowsFilterTest.cs
		}

		public void TestWorkflowsForJobFilter_AnyMatch()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "A");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "B");

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var parentJobFilter = filterBizo.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>("Parent Job");
			parentJobFilter.SelectedModule = ModuleIDs.Organisation.Name;
			parentJobFilter.Property = jobHeader.FH_ParentId;

			var workflowsFilter = filterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Workflows for Job");
			AssertEquals(ModuleTextFilter.ComparisonConstants.AnyMatch, workflowsFilter.ComparisonOperator);

			workflowsFilter.SelectedFilters.AddTextFilterStrip("Completion Statement", "A");

			var query = filterBizo.Filter;
			var results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The job and workflows should have been returned because one of the workflows in the job meets the ANY MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, new[] { "Job", "A", "B" }, results.Select(x => x.FH_CompletionStatement));

			workflow1.FH_CompletionStatement = "C";
			Factory.Save();

			query = filterBizo.Filter;
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("There should be an empty result because none of the workflows (including job-level) match the ANY MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			jobHeader.FH_CompletionStatement = "A Job";
			Factory.Save();

			query = filterBizo.Filter;
			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The job and workflows should have been returned because the job header meets the ANY MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, new[] { "A Job", "C", "B" }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestWorkflowsForJobFilter_NoneMatch()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "A");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "B");

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var parentJobFilter = filterBizo.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>("Parent Job");
			parentJobFilter.SelectedModule = ModuleIDs.Organisation.Name;
			parentJobFilter.Property = jobHeader.FH_ParentId;

			var workflowsFilter = filterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Workflows for Job");
			workflowsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			workflowsFilter.SelectedFilters.AddTextFilterStrip("Completion Statement", "A");

			var query = filterBizo.Filter;
			var results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("There should be an empty result because one of the workflows matches the NONE MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			workflow1.FH_CompletionStatement = "C";
			Factory.Save();

			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("The job and workflows should have been returned because none of the workflows in the job (including job-level) meet the NONE MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, new[] { "Job", "C", "B" }, results.Select(x => x.FH_CompletionStatement));

			jobHeader.FH_CompletionStatement = "A Job";
			Factory.Save();

			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("There should be an empty result because the job-level workflow matches the NONE MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));
		}

		public void TestWorkflowsForJobFilter_AllMatch()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Spaceship Job");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Spaceship");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Scorpion");

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var parentJobFilter = filterBizo.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>("Parent Job");
			parentJobFilter.SelectedModule = ModuleIDs.Organisation.Name;
			parentJobFilter.Property = jobHeader.FH_ParentId;

			var workflowsFilter = filterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Workflows for Job");
			workflowsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			workflowsFilter.SelectedFilters.AddTextFilterStrip("Completion Statement", "Spaceship");

			var query = filterBizo.Filter;
			var results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("No results should be returned because not all of the workflows in the job meet the ALL MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));

			workflow2.FH_CompletionStatement = "Spaceship 2";
			Factory.Save();

			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("Now all of the workflows in the job meet the ALL MATCH criteria, so the job's workflows should have been returned, and yet... " + query.LiteralTextSqlFormatted, new[] { "Spaceship Job", "Spaceship", "Spaceship 2" }, results.Select(x => x.FH_CompletionStatement));

			jobHeader.FH_CompletionStatement = "Job";
			Factory.Save();

			results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("No results should be returned because the job-level workflow doesn't meet the ALL MATCH criteria, and yet... " + query.LiteralTextSqlFormatted, Array.Empty<string>(), results.Select(x => x.FH_CompletionStatement));
		}

		public void TestLastTransferTypeFilter()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);

			foreach (CodeDescriptionPair pair in new TransferTypeList())
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow transferred by " + pair.Description);
				workflow.FH_LastTransferType = pair.Code;
			}

			var notTransferredWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Not yet transferred");
			AssertEquals("NA", notTransferredWorkflow.FH_LastTransferType);

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip(ProcessHeader.ModuleFilterConstants.LastTransferType);

			foreach (CodeDescriptionPair pair in filter.List)
			{
				filter.Property = pair.Code;
				var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { "Workflow transferred by " + pair.Description }, results.Select(x => x.FH_CompletionStatement));
			}
		}

		public void TestLastTransferTypeFilter_ShouldHaveLimitedComparisonOperators()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = filterBizo.AddTextFilterStrip(ProcessHeader.ModuleFilterConstants.LastTransferType);

			AssertContainsExactElementsInAnyOrder("The default Text filter comparison operators except 'exact' and 'not equal' make no sense for this filter and shouldn't be included. SAD!",
				new[] { "exact", "not equal" }, filter.ComparisonOperator_List.GetAllCodes());
		}

		#endregion

		#region Job Filters

		#region Job Code

		public void TestJobCodeFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MYORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var processHeader = jobHeader.ProcessHeaders.First();
			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var jobCodeFilter = (JobCodeFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobCode];

			jobCodeFilter.IsActive = true;
			jobCodeFilter.WorkflowTypeCode = "ORG";
			jobCodeFilter.Property = "MYORGSYD";

			var jobOrWorkflowFilter = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflowFilter.IsActive = true;
			jobOrWorkflowFilter.SetJobAndWorkflow();

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertEquals(2, results.Length);
			Assert(results.Any(h => h.PK == jobHeader.PK));
			Assert(results.Any(h => h.PK == processHeader.PK));

			jobCodeFilter.Property = "URORGSYD";

			var loadedProcessHeader = Factory.LoadTop1<ProcessHeader>(filterBizo.Filter);
			AssertNull(loadedProcessHeader);
		}

		public void TestJobCodeFilter_Validation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MYORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var jobCodeFilter = (JobCodeFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobCode];

			jobCodeFilter.IsActive = true;
			jobCodeFilter.Property = "MYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYD";
			jobCodeFilter.WorkflowTypeCode = "ORG";

			jobCodeFilter.Validation.ValidateAll();
			AssertHasWarning(jobCodeFilter.PropertyInfo, "Field input is too long, searching for: \"MYORGSYDMYOR\" instead.");

			jobCodeFilter.Property = "MYORGSYDMYOR";
			jobCodeFilter.Validation.ValidateAll();
			AssertNoWarning(jobCodeFilter.PropertyInfo, "Field input is too long, searching for: \"MYORGSYDMYOR\" instead.");
		}

		public void TestJobCodeFilterOverflow()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MYORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var jobCodeFilter = (JobCodeFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobCode];

			jobCodeFilter.IsActive = true;
			jobCodeFilter.Property = "MYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYDMYORGSYD";
			jobCodeFilter.WorkflowTypeCode = "ORG";

			var jobOrWorkflowFilter = (JobOrWorkflowFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflowFilter.IsActive = true;
			jobOrWorkflowFilter.SetJobAndWorkflow();

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorReporter.LastMessageReported, string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals(ErrorReporter.LastMessageReported, 0, ErrorReporter.TotalErrorCount);
			});
		}

		#endregion

		#region Job Description

		public void TestJobDescriptionFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "MYORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var processHeader = jobHeader.ProcessHeaders.First();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "MYORG";
			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var processHeader1 = jobHeader1.ProcessHeaders.First();

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var jobDescriptionFilter = (JobDescriptionFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobDescription];

			jobDescriptionFilter.IsActive = true;
			jobDescriptionFilter.Property = "MYORGSYD";
			jobDescriptionFilter.WorkflowTypeCode = "ORG";

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, processHeader }, results);

			jobDescriptionFilter.Property = "URORGSYD";

			var loadedProcessHeader = Factory.LoadTop1<ProcessHeader>(filterBizo.Filter);
			AssertNull(loadedProcessHeader);
		}

		public void TestCompletionStatementFilterDescription()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var completionStatementFilter = (ModuleTextFilter)filterBizo["Completion Statement"];

			AssertEquals("Description", completionStatementFilter.MultilingualDescription);
		}

		public void TestJobDescriptionFilter_Validation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "MYORGSYD";
			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var jobDescriptionFilter = (JobDescriptionFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobDescription];

			jobDescriptionFilter.IsActive = true;
			jobDescriptionFilter.Property = "TESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTEST";
			jobDescriptionFilter.WorkflowTypeCode = "ORG";

			jobDescriptionFilter.Validation.ValidateAll();
			AssertHasWarning(jobDescriptionFilter.PropertyInfo, "Field input is too long, searching for: \"TESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTEST\" instead.");
		}

		public void TestJobDescriptionFilterOverflow()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "MYORGSYD";
			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var jobDescriptionFilter = (ModuleTextFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobDescription];
			var workflowTypeFilter = (ModuleTextFilter)filterBizo[ProcessHeader.ModuleFilterConstants.WorkflowType];

			jobDescriptionFilter.IsActive = true;
			workflowTypeFilter.IsActive = true;
			jobDescriptionFilter.Property = "TESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTEST";
			workflowTypeFilter.Property = "ORG";

			var results = Factory.Load<ProcessHeader>(filterBizo.Filter);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorReporter.LastMessageReported, string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals(ErrorReporter.LastMessageReported, 0, ErrorReporter.TotalErrorCount);
			});
		}

		public void TestWorkflowTypeFilter_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var workflowTypeFilter = (ModuleTextFilter)filterBizo[ProcessHeader.ModuleFilterConstants.WorkflowType];
			workflowTypeFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				WorkflowDescriptors.AccPayableOrderHeaderCode,
				WorkflowDescriptors.APInvoiceCode,
				WorkflowDescriptors.ARInvoiceCode,
				WorkflowDescriptors.CampaignWorkflowDescriptorCode,
				WorkflowDescriptors.CollectionBatchCode,
				WorkflowDescriptors.CollectionOrderCode,
				WorkflowDescriptors.CommunicationWorkflowDescriptorCode,
				WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode,
				WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode,
				WorkflowDescriptors.GlbGroupWorkflowDescriptorCode,
				WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode,
				WorkflowDescriptors.GlbStaffDescriptorCode,
				WorkflowDescriptors.GlbStaffHolidayDescriptorCode,
				WorkflowDescriptors.HRCampaignWorkflowDescriptorCode,
				WorkflowDescriptors.HRHiringRequestDescriptorCode,
				WorkflowDescriptors.HRJobApplicationWorkflowDescriptorCode,
				WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode,
				WorkflowDescriptors.HRRecruitmentJobCampaignWorkflowDescriptorCode,
				WorkflowDescriptors.OpportunityWorkflowDescriptorCode,
				WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode,
				WorkflowDescriptors.ProjectWorkflowDescriptorCode,
				WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode,
				WorkflowDescriptors.WorkItemWorkflowDescriptorCode,
			}, ((CodeDescriptionPairList)(workflowTypeFilter.List)).GetAllCodes());
		}

		#endregion

		#region Job Property

		public void TestJobPropertyFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "MYORGSYD";
			org1.OH_FullName = "Hitech Software";
			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var processHeader1 = jobHeader1.ProcessHeaders.First();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "USORGSYD";
			org2.OH_FullName = "Lowtech Software";
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);
			var processHeader2 = jobHeader2.ProcessHeaders.First();

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (JobTextPropertyFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobProperty];

			filter.IsActive = true;
			filter.WorkflowTypeCode = "ORG";
			filter.JobPropertyName = OrgHeaderSchema.Constants.OH_FullName;
			filter.Property = "Hitec";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			AssertContainsExactElementsInAnyOrder(Factory.Load<ProcessHeader>(filterBizo.Filter), new ProcessHeader[] { jobHeader1, processHeader1 });

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			AssertContainsExactElementsInAnyOrder(Factory.Load<ProcessHeader>(filterBizo.Filter), new ProcessHeader[] { jobHeader2, processHeader2 });

			filter.Property = "Software";
			AssertContainsExactElementsInAnyOrder(Factory.Load<ProcessHeader>(filterBizo.Filter), new ProcessHeader[] { jobHeader1, processHeader1, jobHeader2, processHeader2 });

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertEquals(0, Factory.Load<ProcessHeader>(filterBizo.Filter).Length);
		}

		public void TestJobPropertyFilter_Validation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MYORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var processHeader = jobHeader.ProcessHeaders.AddNew();
			processHeader.FillWithValidTestData();
			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (JobTextPropertyFilter)filterBizo[ProcessHeader.ModuleFilterConstants.JobProperty];
			filter.IsActive = true;
			filter.Validation.ValidateAll();
			AssertHasErrors(filter.WorkflowTypeCodeInfo);

			filter.WorkflowTypeCode = "Z98";
			filter.Validation.ValidateAll();
			AssertHasErrors(filter.WorkflowTypeCodeInfo);

			filter.WorkflowTypeCode = "ORG";
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.WorkflowTypeCodeInfo);
			AssertHasErrors(filter.JobPropertyNameInfo);

			filter.JobPropertyName = "Z98";
			filter.Validation.ValidateAll();
			AssertHasErrors(filter.JobPropertyNameInfo);

			filter.JobPropertyName = OrgHeaderSchema.Constants.OH_FullName;
			filter.Validation.ValidateAll();
			AssertNoErrors(filter.JobPropertyNameInfo);
			AssertNoErrors(filter.PropertyInfo);

			foreach (CodeDescriptionPair code in filter.List)
			{
				filter.WorkflowTypeCode = code.Code;
				foreach (CodeDescriptionPair option in filter.JobPropertyNameList)
				{
					filter.JobPropertyName = option.Code;
					var column = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(option.Code.Substring(0, option.Code.IndexOf('_'))).GetSchemaColumn(option.Code);
					if (column == null || !column.HasMaxLength || column.MaxLength == int.MaxValue)
					{
						continue; // QBK - VB_QuoteNumber returns a null column on lookup however has a max length = int.MaxValue
					}
					filter.Property = new string('t', column.MaxLength + 1);
					var truncated = new string('t', column.MaxLength);
					filter.Validation.ValidateAll();
					var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
					CombineAssertions(() =>
					{
						AssertHasWarning(filter.PropertyInfo, string.Format(CultureInfo.InvariantCulture, "Job Property is too long, searching for: \"{0}\" instead.", truncated));
						AssertEquals(code.Code + " " + ErrorReporter.LastMessageReported, 0, ErrorReporter.TotalErrorCount);
						AssertEquals(code.Code + " " + ErrorReporter.LastMessageReported, "", ErrorReporter.LastMessageReported);
					});

					filter.Property = truncated;
					filter.Validation.ValidateAll();
					AssertNoWarning(filter.PropertyInfo, string.Format(CultureInfo.InvariantCulture, "Job Property is too long, searching for: \"{0}\" instead.", truncated));

					filter.PropertyInfo.ClearValue();
				}
			}
		}

		public void TestJobPropertyFilter_ShouldIncludeClientWorkflowTypes()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");

			var dummy1 = Factory.New<USADummy>();
			var dummy2 = Factory.New<AussieDummy>();
			var dummy3 = Factory.New<AussieDummy>();

			dummy1.Z0_Description = "OI OI OI!";
			dummy2.Z0_Description = "OI OI OI!";
			dummy3.Z0_Description = "Oui!";

			var jobHeader1 = BMSTestHelper.CreateJobHeader(dummy1, addDefaultProcessHeaderIfNone: false, description: "USADummy");
			var jobHeader2 = BMSTestHelper.CreateJobHeader(dummy2, addDefaultProcessHeaderIfNone: false, description: "First AussieDummy");
			var jobHeader3 = BMSTestHelper.CreateJobHeader(dummy3, addDefaultProcessHeaderIfNone: false, description: "Second AussieDummy");

			Factory.Save();

			using (ClientHookLoader.Instance.OverrideClientHookForTest(new DummyClientHook()))
			{
				var bizo = new ProcessHeaderFilterBusinessObject();
				var filter = (JobTextPropertyFilter)bizo[ProcessHeader.ModuleFilterConstants.JobProperty];

				filter.IsActive = true;
				filter.WorkflowTypeCode = "DUM";
				filter.JobPropertyName = DummyBizoSchema.Z0_Description.Name;
				filter.Property = "OI OI OI!";

				AssertContainsExactElementsInAnyOrder("Should include only the AussieDummy because the client-specific type decider has added the filter by Z0_Code.", new[] { jobHeader2 }, Factory.Load<ProcessHeader>(bizo.Filter));
			}
		}

		#region Dummy Client-Specific Boilerplate

		class DummyClientHook : ClientHook
		{
			public override Clients Client => Clients.EDI;

			public override string ClientDisplayName => "It's gonna be YUGE";

			public override ITypeDeciderDictionary ClientTypeDeciders => new TypeDeciderDictionary(new Dictionary<Type, ITypeDecider> { { typeof(ProcessTask), new DummyProcessTaskTypeDecider() } });

			protected override void InitialiseCore()
			{
				base.InitialiseCore();

				DummyWorkflowDescriptors.RegisterThisSubTypeOverride();
			}
		}

		class DummyWorkflowDescriptors : WorkflowDescriptors
		{
			DummyWorkflowDescriptors()
			{
				AddDescriptor(new USAUSAUSAWorkflowDescriptor());
				AddDescriptor(new AussieAussieAussieWorkflowDescriptor());
			}

			internal static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(() => new DummyWorkflowDescriptors());
			}
		}

		class DummyProcessTaskTypeDecider : ProcessTaskTypeDecider
		{
			protected override Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
			{
				var dummy = factory.Load<DiscriminatingDummy>(parentID);

				if (dummy != null)
				{
					switch (dummy.Z0_Code)
					{
						case "USA":
							return typeof(USADummyProcessTask);
						case "AUS":
							return typeof(AUSDummyProcessTask);
					}
				}

				return base.GetTypeForLoad(parentTablePrefix, parentID, factory);
			}

			protected override void AddAdditionalParentFilters(WorkflowDescriptor descriptor, ZDBOnlySubQuery subQuery)
			{
				if (descriptor.WorkflowProviderType == typeof(USADummy))
				{
					subQuery.AddToFilter(DummyBizoSchema.Z0_Code, "USA");
				}
				if (descriptor.WorkflowProviderType == typeof(AussieDummy))
				{
					subQuery.AddToFilter(DummyBizoSchema.Z0_Code, "AUS");
				}

				base.AddAdditionalParentFilters(descriptor, subQuery);
			}
		}

		class USAUSAUSAWorkflowDescriptor : WorkflowDescriptor
		{
			public override string Code => WorkflowDescriptors.DummyWorkflowDescriptorCode;

			public override IMultilingualString Description => (NoResString)"USA! USA! USA!";

			public override ControllerID ControllerID => DummyControllerIDs.Dummy;

			public override Type WorkflowProviderType => typeof(USADummy);
		}

		class AussieAussieAussieWorkflowDescriptor : WorkflowDescriptor
		{
			public override string Code => WorkflowDescriptors.DummyWorkflowDescriptorCode;

			public override IMultilingualString Description => (NoResString)"Aussie Aussie Aussie!";

			public override ControllerID ControllerID => DummyControllerIDs.Dummy;

			public override Type WorkflowProviderType => typeof(AussieDummy);
		}

		class USADummy : DiscriminatingDummy
		{
			public USADummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();

				Z0_Code = "USA";
			}
		}

		class AussieDummy : DiscriminatingDummy
		{
			public AussieDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();

				Z0_Code = "AUS";
			}
		}

		class USADummyProcessTask : DummyProcessTask
		{
			public USADummyProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class AUSDummyProcessTask : DummyProcessTask
		{
			public AUSDummyProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion

		#endregion

		#region Parent Job

		public void TestParentJobFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "MYORGSYD";
			org1.OH_FullName = "Hitech Software";
			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var processHeader1 = jobHeader1.ProcessHeaders.First();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "USORGSYD";
			org2.OH_FullName = "Lowtech Software";
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);
			var processHeader2 = jobHeader2.ProcessHeaders.First();

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader3 = ProcessJobHeader.GetForParent(enquiry, Factory);
			var processHeader3 = jobHeader3.ProcessHeaders.First();

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];
			filter.IsActive = true;
			filter.SelectedModule = "OrgHeader";
			filter.Property = org1.PK;

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertEquals(2, result.Length);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, processHeader1 }, result);

			filter.SelectedModule = ModuleIDs.SalesEnquiry.Name;
			filter.Property = enquiry.PK;

			result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertEquals(2, result.Length);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader3, processHeader3 }, result);
		}

		public void TestParentJobFilter_ShouldHaveWorkflowProviderModuleOptions()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidModuleSpecifiedFilter)filterBizo["Parent Job"];

			AssertEquals(true, filter.ModuleOptions.Count > 30);

			var allowedModules = ParentJobModuleFilter.GetWorkflowProviderModules(Factory).ToArray();

			foreach (var code in filter.ModuleOptions.GetAllCodes())
			{
				var id = ModuleIDs.AllExcludingClientModules.First(x => x.Name == code);

				AssertCollectionContains($"Each module option must be workflow enabled, and yet {code} is not.", id, allowedModules);
			}
		}

		public void TestMultipleParentJobFilters_WhenUsedInOrCategory()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "ORG", "INQ");

			var enquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var enquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();

			enquiry1.O1_EnquiryType = "CCR";
			enquiry2.O1_EnquiryType = "WEB";

			var jobHeader_enquiry1 = BMSTestHelper.CreateJobHeader(enquiry1, addDefaultProcessHeaderIfNone: false);
			var jobHeader_enquiry2 = BMSTestHelper.CreateJobHeader(enquiry2, addDefaultProcessHeaderIfNone: false);
			var jobHeader_org = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow_enquiry1 = BMSTestHelper.CreateWorkflow(jobHeader_enquiry1, "Workflow SalesEnquiry1");
			var workflow_enquiry2 = BMSTestHelper.CreateWorkflow(jobHeader_enquiry2, "Workflow SalesEnquiry2");
			var workflow_org = BMSTestHelper.CreateWorkflow(jobHeader_org, "Workflow OrgHeader");

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var parentJobFilter1 = (ParentJobModuleFilter)bizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ParentJob).CurrentModuleFilter;
			var parentJobFilter2 = (ParentJobModuleFilter)bizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.ParentJob).CurrentModuleFilter;

			parentJobFilter1.SelectedModule = ModuleIDs.SalesEnquiry.Name;
			parentJobFilter2.SelectedModule = ModuleIDs.Organisation.Name;

			parentJobFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			parentJobFilter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			parentJobFilter1.Property = jobHeader_enquiry1.FH_ParentId;
			parentJobFilter2.Property = jobHeader_org.FH_ParentId;

			parentJobFilter1.OrCategory = FilterOrCategory.BurlyWood;
			parentJobFilter2.OrCategory = FilterOrCategory.BurlyWood;

			AssertContainsExactElementsInAnyOrder("Both filters having the same OR category should cause workflows in the two selected jobs to be found. Module filter query: " + bizo.Filter.LiteralTextSqlFormatted,
				new[] { jobHeader_enquiry1, workflow_enquiry1, jobHeader_org, workflow_org }, Factory.Load<ProcessHeader>(bizo.Filter));

			parentJobFilter1.OrCategory = FilterOrCategory.None;
			parentJobFilter2.OrCategory = FilterOrCategory.None;

			AssertContainsExactElementsInAnyOrder("With no OR category, nothing should be found, since the queries are mutually exclusive. Module filter query: " + bizo.Filter.LiteralTextSqlFormatted, Array.Empty<ProcessHeader>(), Factory.Load<ProcessHeader>(bizo.Filter));

			parentJobFilter1.Property = ZGuid.Empty;
			parentJobFilter2.Property = ZGuid.Empty;
			parentJobFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			parentJobFilter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

			parentJobFilter1.SelectedFilters.AddTextFilterStrip("Type", "WEB");

			AssertContainsExactElementsInAnyOrder("With no OR category, nothing should be found, since the queries are mutually exclusive. Module filter query: " + bizo.Filter.LiteralTextSqlFormatted, Array.Empty<ProcessHeader>(), Factory.Load<ProcessHeader>(bizo.Filter));

			parentJobFilter1.OrCategory = FilterOrCategory.BurlyWood;
			parentJobFilter2.OrCategory = FilterOrCategory.BurlyWood;

			AssertContainsExactElementsInAnyOrder("Both filters having the same OR category should cause workflows in the two matching job types to be found. Module filter query: " + bizo.Filter.LiteralTextSqlFormatted,
				new[] { jobHeader_enquiry2, workflow_enquiry2, jobHeader_org, workflow_org }, Factory.Load<ProcessHeader>(bizo.Filter));
		}

		#endregion

		#endregion

		#region Flag Filters

		public void TestCriticalHandoverFlag()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);

			jobHeader1.FH_IsCriticalHandover = true;
			jobHeader2.FH_IsCriticalHandover = true;
			jobHeader3.FH_IsCriticalHandover = false;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var criticalHandoverFilter = (ModuleFlagsFilter)filterBizo[ProcessHeader.ModuleFilterConstants.CriticalHandover];
			criticalHandoverFilter.IsActive = true;
			criticalHandoverFilter.Property0 = true;
			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(filterBizo.Filter));

			AssertWorkflowsFound(resultsGetter(), jobHeader1, jobHeader2);

			criticalHandoverFilter.Property0 = false;

			AssertWorkflowsFound(resultsGetter(), jobHeader3);
		}

		public void TestAllowTaskAutoAssignmentFlag()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);

			jobHeader1.FH_AllowTaskAutoAssignment = true;
			jobHeader2.FH_AllowTaskAutoAssignment = true;
			jobHeader3.FH_AllowTaskAutoAssignment = false;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var autoAssignTasksFilter = (ModuleFlagsFilter)filterBizo[ProcessHeader.ModuleFilterConstants.AutoAssignTasks];
			autoAssignTasksFilter.IsActive = true;
			autoAssignTasksFilter.Property0 = true;
			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(filterBizo.Filter));

			AssertWorkflowsFound(resultsGetter(), jobHeader1, jobHeader2);

			autoAssignTasksFilter.Property0 = false;

			AssertWorkflowsFound(resultsGetter(), jobHeader3);
		}

		public void TestIsStandbyFlag()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);

			jobHeader1.FH_IsStandby = true;
			jobHeader2.FH_IsStandby = true;
			jobHeader3.FH_IsStandby = false;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var standbyTasksFilter = (ModuleFlagsFilter)filterBizo[ProcessHeader.ModuleFilterConstants.StandbyTask];
			standbyTasksFilter.IsActive = true;
			standbyTasksFilter.Property0 = true;
			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(filterBizo.Filter));

			AssertWorkflowsFound(resultsGetter(), jobHeader1, jobHeader2);

			standbyTasksFilter.Property0 = false;

			AssertWorkflowsFound(resultsGetter(), jobHeader3);
		}

		public void TestIsApprovedFlag()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);

			jobHeader1.FH_IsApproved = true;
			jobHeader2.FH_IsApproved = true;
			jobHeader3.FH_IsApproved = false;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var approvedFilter = (ModuleFlagsFilter)filterBizo[ProcessHeader.ModuleFilterConstants.Approved];
			approvedFilter.IsActive = true;
			approvedFilter.Property0 = true;
			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(filterBizo.Filter));

			AssertWorkflowsFound(resultsGetter(), jobHeader1, jobHeader2);

			approvedFilter.Property0 = false;

			AssertWorkflowsFound(resultsGetter(), jobHeader3);
		}

		#endregion

		#region Date Filters

		[TestDate(2015, 1, 16)]
		public void TestDateFilters_ShouldFallbackToJob()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader1", addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader2", addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");

			jobHeader1.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(-1);
			jobHeader2.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(1);

			workflow1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-1);
			workflow2.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(3);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var earliestStartDateFilter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.EarliestStartDate];
			var agreedDeliveryDateFilter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate];

			earliestStartDateFilter.IsActive = true;
			earliestStartDateFilter.PropertySearch = ModuleDateFilter.Past;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Only workflow1 is due to start in the past", new[] { workflow1 }, results);

			earliestStartDateFilter.PropertySearch = ModuleDateFilter.Future;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("jobHeader2 and therefore workflow2 are due to start in the future", new[] { jobHeader2, workflow2 }, results);

			earliestStartDateFilter.IsActive = false;
			agreedDeliveryDateFilter.IsActive = true;
			agreedDeliveryDateFilter.PropertySearch = ModuleDateFilter.Past;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("jobHeader1 and therefore workflow1 are due in the past", new[] { jobHeader1, workflow1 }, results);

			agreedDeliveryDateFilter.PropertySearch = ModuleDateFilter.Future;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Only workflow2 is due in the future", new[] { workflow2 }, results);

			agreedDeliveryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			agreedDeliveryDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
			agreedDeliveryDateFilter.Property2 = ZDateTime.Today.AddDays(1);

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("jobHeader1 and therefore workflow1 are due within the specified period", new[] { jobHeader1, workflow1 }, results);

			agreedDeliveryDateFilter.Property2 = ZDateTime.Today.AddDays(3);

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("jobHeader1 and therefore workflow1, and now workflow2 are due within the specified period", new[] { jobHeader1, workflow1, workflow2 }, results);
		}

		public void TestDateFilters_ForNoDateSearchType_ShouldEnsureJobAndWorkflowHaveNoValue()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader1", addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader2", addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader3", addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3");

			jobHeader1.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(-1);
			jobHeader2.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(1);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var earliestStartDateFilter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.EarliestStartDate];
			var agreedDeliveryDateFilter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate];

			earliestStartDateFilter.IsActive = true;
			earliestStartDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Workflows and jobs except in jobHeader2 have no value for DoNotStartBeforeDate", new[] { jobHeader1, jobHeader3, workflow1, workflow3 }, results);

			earliestStartDateFilter.IsActive = false;
			agreedDeliveryDateFilter.IsActive = true;
			agreedDeliveryDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Workflows and jobs except in jobHeader1 have no value for AgreedDeliveryDate", new[] { jobHeader2, jobHeader3, workflow2, workflow3 }, results);
		}

		[TestDate(2015, 3, 6)]
		public void TestDateFilters_WhenJobAndWorkflowHaveValue_ShouldUseWorkflowValueOnly()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader", addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-1);
			workflow.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(1);

			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(-1);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(1);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var earliestStartDateFilter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.EarliestStartDate];
			var agreedDeliveryDateFilter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate];

			earliestStartDateFilter.IsActive = true;
			earliestStartDateFilter.PropertySearch = ModuleDateFilter.Past;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("jobHeader is due to start in the past", new[] { jobHeader }, results);

			earliestStartDateFilter.PropertySearch = ModuleDateFilter.Future;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow is due to start in the future", new[] { workflow }, results);

			earliestStartDateFilter.IsActive = false;
			agreedDeliveryDateFilter.IsActive = true;
			agreedDeliveryDateFilter.PropertySearch = ModuleDateFilter.Past;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("jobHeader is due in the past", new[] { jobHeader }, results);

			agreedDeliveryDateFilter.PropertySearch = ModuleDateFilter.Future;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow is due in the future", new[] { workflow }, results);
		}

		public void TestDateFiltersQuery_WhenJobOrWorkflowFilterIsJobOnly_ShouldNotContainExcessSQL()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();

			var agreedDeliveryDateFilter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate];
			agreedDeliveryDateFilter.IsActive = true;
			agreedDeliveryDateFilter.PropertySearch = ModuleDateFilter.Past;

			var jobOrWorkflowFilter = (JobOrWorkflowFilter)bizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];
			jobOrWorkflowFilter.IsActive = true;
			jobOrWorkflowFilter.SetJobOnly();

			AssertEquals("Ensuring that Query does not contain redundant SQL", 0, bizo.Filter.FilterString.CountMatches("OR"));

			jobOrWorkflowFilter.SetWorkflowOnly();

			AssertEquals("Ensuring that Query does not contain redundant SQL", 1, bizo.Filter.FilterString.CountMatches("FH_FH_ParentHeader is not NULL"));
		}

		[TestDate(2014, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestUtcFilters_ShouldWorkWithPastAndPresentFilterOptions()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			workflow1.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddMinutes(10);
			workflow2.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddMinutes(-10);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate];

			filter.IsActive = true;

			filter.PropertySearch = ModuleDateFilter.Future;
			AssertContainsExactElementsInAnyOrder(new[] { workflow1 }, Factory.Load<ProcessHeader>(bizo.Filter));

			filter.PropertySearch = ModuleDateFilter.Past;
			AssertContainsExactElementsInAnyOrder(new[] { workflow2 }, Factory.Load<ProcessHeader>(bizo.Filter));
		}

		[TestDate(2014, 7, 14)]
		[TestUtcOffset(10, 0, 0)]
		public void TestUtcFilters_ShouldWorkWithOffsetFilterOptions()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			workflow1.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddMinutes(10);
			workflow2.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddMinutes(-10);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleDateFilter)bizo[ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate];

			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;

			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			filter.Property2 = new ZInt(10).GetDateTimeFromMinutes();
			AssertContainsExactElementsInAnyOrder(new[] { workflow1 }, Factory.Load<ProcessHeader>(bizo.Filter));

			filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Past;
			filter.Property2 = new ZInt(10).GetDateTimeFromMinutes();
			AssertContainsExactElementsInAnyOrder(new[] { workflow2 }, Factory.Load<ProcessHeader>(bizo.Filter));
		}

		#endregion

		#region Date Acceptability

		public void TestDateAcceptability_ShouldFallBackToJob()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Jobby the Hutt");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Jabbo the Hutt");
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "werqit 1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "werqit 2");
			var workflow1_3 = BMSTestHelper.CreateWorkflow(jobHeader1, "werqit 3");
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "you better 1");
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "you better 2");

			jobHeader1.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			workflow1_1.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartSharpFinish;
			workflow1_2.FH_DateAcceptability = DateAcceptabilityList.Codes.GraduatedStartSharpFinish;
			workflow2_1.FH_DateAcceptability = DateAcceptabilityList.Codes.SharpStartExtendedFinish;

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.DateAcceptability];

			filter.IsActive = true;
			filter.Property = "|_";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			AssertContainsExactElementsInAnyOrder(new[] { workflow1_1, workflow1_2 }, Factory.Load<ProcessHeader>(bizo.Filter));

			filter.Property = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, workflow1_3 }, Factory.Load<ProcessHeader>(bizo.Filter));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertContainsExactElementsInAnyOrder(new[] { workflow1_3, jobHeader2, workflow2_2 }, Factory.Load<ProcessHeader>(bizo.Filter));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, workflow1_1, workflow1_2, workflow1_3, workflow2_1 }, Factory.Load<ProcessHeader>(bizo.Filter));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "|";
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, workflow1_1, workflow1_2, workflow1_3, workflow2_1 }, Factory.Load<ProcessHeader>(bizo.Filter));
		}

		#endregion

		#region Capability

		public void TestCapabilityFilter()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			header1.FH_CompletionStatement = "Workflow 1";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Workflow 2";
			var header3 = jobHeader.ProcessHeaders.AddNew();
			header3.FH_CompletionStatement = "Workflow 3";
			var header4 = jobHeader.ProcessHeaders.AddNew();
			header4.FH_CompletionStatement = "Workflow 4";

			var task1 = job.WorkflowItems.AddNew(); // With staff AND capability
			task1.P9_FH_ProcessHeader = header1.PK;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_G4_RequiredCapability = capability.PK;

			var task2 = job.WorkflowItems.AddNew(); // With staff only
			task2.P9_FH_ProcessHeader = header2.PK;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var task3 = job.WorkflowItems.AddNew(); // With capability only
			task3.P9_FH_ProcessHeader = header3.PK;
			task3.P9_GS_NKAssignedStaffMember = string.Empty;
			task3.P9_G4_RequiredCapability = capability.PK;

			var task4 = job.WorkflowItems.AddNew(); // With neither staff nor capability
			task4.P9_FH_ProcessHeader = header4.PK;
			task4.P9_GS_NKAssignedStaffMember = string.Empty;

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var resourceFilter = (AssignedResourceFilter)bizo[ProcessHeader.ModuleFilterConstants.ResourceAssignedToAnyTask];
			var capabilityFilter = (CapabilityFilter)bizo["Capability Required on Any Task"];

			resourceFilter.IsActive = true;
			resourceFilter.Property = staff.GS_Code;
			capabilityFilter.IsActive = true;
			capabilityFilter.Property = capability.PK;

			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(bizo.Filter));

			AssertWorkflowsFound(resultsGetter(), jobHeader, header1); // For staff AND capability

			capabilityFilter.Property = ZGuid.Empty;
			AssertWorkflowsFound(resultsGetter(), jobHeader, header1, header2); // For staff

			resourceFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			capabilityFilter.Property = capability.PK;
			AssertWorkflowsFound(resultsGetter(), jobHeader, header3); // For capability and NO staff

			capabilityFilter.Property = ZGuid.Empty;
			AssertWorkflowsFound(resultsGetter(), jobHeader, header3, header4); // For NO staff

			resourceFilter.IsActive = false;
			capabilityFilter.Property = capability.PK;
			AssertWorkflowsFound(resultsGetter(), jobHeader, header1, header3); // For capability
		}

		public void TestJobOrWorkFlow_TasksFilters()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			header1.FH_CompletionStatement = "Workflow 1";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Workflow 2";
			var header3 = jobHeader.ProcessHeaders.AddNew();
			header3.FH_CompletionStatement = "Workflow 3";
			var header4 = jobHeader.ProcessHeaders.AddNew();
			header4.FH_CompletionStatement = "Workflow 4";

			var task1 = job.WorkflowItems.AddNew(); // With staff AND capability
			task1.P9_FH_ProcessHeader = header1.PK;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_G4_RequiredCapability = capability.PK;

			var task2 = job.WorkflowItems.AddNew(); // With staff only
			task2.P9_FH_ProcessHeader = header2.PK;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var task3 = job.WorkflowItems.AddNew(); // With capability only
			task3.P9_FH_ProcessHeader = header3.PK;
			task3.P9_GS_NKAssignedStaffMember = string.Empty;
			task3.P9_G4_RequiredCapability = capability.PK;

			var task4 = job.WorkflowItems.AddNew(); // With neither staff nor capability
			task4.P9_FH_ProcessHeader = header4.PK;
			task4.P9_GS_NKAssignedStaffMember = string.Empty;

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var resourceAnyTaskFilter = (ModuleNkFilter)bizo[ProcessHeader.ModuleFilterConstants.ResourceAssignedToAnyTask];
			var capabilityFilter = (ModuleGuidFilter)bizo["Capability Required on Any Task"];
			var resourceCurrentTask = (ModuleNkFilter)bizo["Resource Assigned To Current Task"];
			var jobOrWorkflowFilter = (JobOrWorkflowFilter)bizo[ProcessHeader.ModuleFilterConstants.JobOrWorkflow];

			jobOrWorkflowFilter.Property0 = true; // job
			jobOrWorkflowFilter.IsActive = true;

			capabilityFilter.IsActive = true;
			capabilityFilter.Property = capability.PK;

			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(bizo.Filter));
			AssertWorkflowsFound(resultsGetter(), jobHeader);

			capabilityFilter.IsActive = false;
			resourceAnyTaskFilter.IsActive = true;
			resourceAnyTaskFilter.Property = staff.GS_Code;
			resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(bizo.Filter));
			AssertWorkflowsFound(resultsGetter(), jobHeader);

			resourceAnyTaskFilter.IsActive = false;
			resourceCurrentTask.IsActive = true;
			resourceCurrentTask.Property = staff.GS_Code;

			resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(bizo.Filter));
			AssertWorkflowsFound(resultsGetter(), jobHeader);
		}

		static void AssertWorkflowsFound(ProcessHeader[] headers, params ProcessHeader[] expectedProcessHeaders)
		{
			AssertEquals(expectedProcessHeaders.Length, headers.Length);
			foreach (var processHeader in expectedProcessHeaders)
			{
				if (headers.FirstOrDefault(h => h.FH_CompletionStatement == processHeader.FH_CompletionStatement) == null)
				{
					Fail("No workflow in results with completion statement " + processHeader);
				}
			}
		}

		#endregion

		#region ReleaseGroup

		public void TestReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_GG_ReleaseGroup = group.PK;
			workflow3.FH_GG_ReleaseGroup = group.PK;

			var bizo = new ProcessHeaderFilterBusinessObject();
			var releaseGroupFilterStrip = (ModuleGuidFilter)bizo["Release Group"];
			releaseGroupFilterStrip.IsActive = true;
			releaseGroupFilterStrip.Property = group.PK;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionContains(workflow1, workflows);
			AssertCollectionContains(workflow3, workflows);
		}

		#endregion

		#region Branch and Department

		public void TestBranchFilterStrip()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			jobHeader1.FH_GB_Branch = branch.PK;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizo[ProcessHeader.ModuleFilterConstants.Branch];
			filter.IsActive = true;
			filter.Property = branch.PK;

			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(filterBizo.Filter));

			AssertWorkflowsFound(resultsGetter(), jobHeader1);
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;

			AssertWorkflowsFound(resultsGetter(), jobHeader2);
		}

		public void TestDepartmentFilterStrip()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			jobHeader1.FH_GE_Department = department.PK;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizo[ProcessHeader.ModuleFilterConstants.Department];
			filter.IsActive = true;
			filter.Property = department.PK;

			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(filterBizo.Filter));

			AssertWorkflowsFound(resultsGetter(), jobHeader1);
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;

			AssertWorkflowsFound(resultsGetter(), jobHeader2);
		}

		public void TestEffectiveBranchFilterStrip()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "WF1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow2a = BMSTestHelper.CreateWorkflow(jobHeader2, "WF2a");
			var workflow2b = BMSTestHelper.CreateWorkflow(jobHeader2, "WF2b");

			var task = BMSTestHelper.CreateTask(workflow1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task1 = BMSTestHelper.CreateTask(workflow2a);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task2 = BMSTestHelper.CreateTask(workflow2b);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			jobHeader1.FH_GB_Branch = branch.PK;
			workflow2a.FH_GB_Branch = branch.PK;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizo[ProcessHeader.ModuleFilterConstants.EffectiveBranch];
			filter.IsActive = true;
			filter.Property = branch.PK;

			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(filterBizo.Filter));

			AssertWorkflowsFound(resultsGetter(), workflow1, workflow2a);
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;

			AssertWorkflowsFound(resultsGetter(), jobHeader1, jobHeader2, workflow2b);
		}

		public void TestEffectiveDepartmentFilterStrip()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "WF1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow2a = BMSTestHelper.CreateWorkflow(jobHeader2, "WF2a");
			var workflow2b = BMSTestHelper.CreateWorkflow(jobHeader2, "WF2b");

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			jobHeader1.FH_GE_Department = department.PK;
			workflow2a.FH_GE_Department = department.PK;

			var task = BMSTestHelper.CreateTask(workflow1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task1 = BMSTestHelper.CreateTask(workflow2a);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var task2 = BMSTestHelper.CreateTask(workflow2b);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizo[ProcessHeader.ModuleFilterConstants.EffectiveDepartment];
			filter.IsActive = true;
			filter.Property = department.PK;

			var resultsGetter = new Func<ProcessHeader[]>(() => Factory.Load<ProcessHeader>(filterBizo.Filter));

			AssertWorkflowsFound(resultsGetter(), workflow1, workflow2a);
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;

			AssertWorkflowsFound(resultsGetter(), jobHeader1, jobHeader2, workflow2b);
		}

		#endregion

		#region Effective Buffer Duration Filter

		public void TestEffectiveBufferDurationFilter_ValidationBehavior()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleDurationFilter)bizo[ProcessHeader.ModuleFilterConstants.EffectiveBufferDuration];
			filter.IsActive = true;
			filter.Scope = "Between";
			filter.MinDurationMinutes = -600;
			filter.MaxDurationMinutes = 6000;
			filter.Validation.ValidateAll();

			AssertHasError(filter.MinDurationMinutesInfo, "Please enter a value greater than or equal to 0.");
			AssertNoErrors(filter.MaxDurationMinutesInfo);

			filter.MinDurationMinutes = 0;
			filter.MaxDurationMinutes = 0;
			filter.Validation.ValidateAll();

			AssertNoErrors(filter.MinDurationMinutesInfo);
			AssertNoErrors(filter.MaxDurationMinutesInfo);
		}

		public void TestEffectiveBufferDurationFilter_ShouldFilterCorrectly()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "standard", timespanMinutes: 1440);
			var bufferTimespan1 = Factory.NewWithValidTestData<BMBufferTimespan>();
			bufferTimespan1.BMT_BufferTimespanInMinutes = 480;

			var bufferTimespan2 = Factory.NewWithValidTestData<BMBufferTimespan>();
			bufferTimespan2.BMT_BufferTimespanInMinutes = 960;

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_BMT_BufferTimespan = bufferTimespan2.PK;

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Own Buffer WF", null);
			workflow1.FH_BMT_BufferTimespan = bufferTimespan1.PK;

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Job Buffer WF", null);

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader1, "Dedicated Buffer WF", buffer);
			workflow3.FH_FC_DedicatedBuffer = buffer.PK;

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleDurationFilter)bizo[ProcessHeader.ModuleFilterConstants.EffectiveBufferDuration];
			filter.IsActive = true;
			filter.Scope = "Between";
			filter.MinDurationMinutes = 420;
			filter.MaxDurationMinutes = 540;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);

			AssertEquals(1, results.Length);
			AssertCollectionContains(workflow1, results);
			AssertCollectionNotContains(workflow2, results);
			AssertCollectionNotContains(workflow3, results);

			filter.MinDurationMinutes = 900;
			filter.MaxDurationMinutes = 1020;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(workflow2, results);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionNotContains(workflow3, results);

			filter.MinDurationMinutes = 1380;
			filter.MaxDurationMinutes = 1500;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(workflow3, results);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionNotContains(workflow2, results);

			filter.MinDurationMinutes = 420;
			filter.MaxDurationMinutes = 1500;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(3, results.Length);
			AssertCollectionContains(workflow1, results);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow3, results);
		}

		#endregion

		#region Tag Filters

		public void TestTagQuery_Performance()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEF", isExclusive: true);
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "MGD");

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var tagMagnitudeFilterStrip = (ModuleGuidAppliedToSubCollectionFilter)filterBizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			tagMagnitudeFilterStrip.IsActive = true;
			tagMagnitudeFilterStrip.Property = magnitude.PK;
			tagMagnitudeFilterStrip.ComparisonOperator = TagWithJobOrWorkflowFilter.IsAppliedComparisonOperator;

			var sql = filterBizo.Filter.LiteralTextSqlFormatted;

			CombineAssertions("WHEN executing TaskAssignFilter, QUERY should not contain 'NOT IN' because 'NOT EXISTS' is more efficient", () =>
			{
				AssertNotContains("not contain 'NOT IN'", "not in", sql, ignoreCase: true);
				AssertContains("contain 'NOT EXISTS'", "not exists", sql, ignoreCase: true);
			});
		}

		class FilterStripTestCase
		{
			public string Code { get; set; }
			public string Description { get; set; }
			public bool IsExclusive { get; set; }
			public FilterOrCategory OrCategory { get; set; }
			public string ComparisonOperator { get; set; }
			public string GroupName { get; set; }
			public FilterOrCategory GroupOrCategory { get; set; }

			public FilterStripTestCase(string code, string description, bool isExclusive, FilterOrCategory orCategory, string comparisonOperator, string groupName, FilterOrCategory groupOrCategory)
			{
				Code = code;
				Description = description;
				IsExclusive = isExclusive;
				OrCategory = orCategory;
				ComparisonOperator = comparisonOperator;
				GroupName = groupName;
				GroupOrCategory = groupOrCategory;
			}
		}

		List<string> SetupFilterStrips(List<FilterStripTestCase> testcases, FilterStripCollection collection)
		{
			var filters = new List<string>();
			foreach (var testcase in testcases)
			{
				var strip = collection.AddNew(testcase.Description);
				strip.OrCategory = testcase.OrCategory;

				switch (testcase.Description)
				{
					case ProcessHeader.ModuleFilterConstants.TagMagnitude:
						{
							var definition = BMSTestHelper.CreateTagDefinition(Factory, testcase.Code, isExclusive: testcase.IsExclusive);
							var tag = BMSTestHelper.CreateTagMagnitude(definition, testcase.Code);
							Factory.Save();
							var filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
							filter.ComparisonOperator = testcase.ComparisonOperator;
							filter.Property = tag.PK;
							filter.OrCategory = testcase.OrCategory;
							filter.IsActive = true;
							filter.GroupName = testcase.GroupName;
							filter.GroupOrCategory = testcase.GroupOrCategory;
							filters.Add(tag.PK.ToString());
							break;
						}
					case ProcessHeader.ModuleFilterConstants.TagDefinitionCode:
						{
							var definition = BMSTestHelper.CreateTagDefinition(Factory, testcase.Code, isExclusive: testcase.IsExclusive);
							Factory.Save();
							var filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
							filter.ComparisonOperator = testcase.ComparisonOperator;
							filter.Property = definition.PK;
							filter.OrCategory = testcase.OrCategory;
							filter.IsActive = true;
							filter.GroupName = testcase.GroupName;
							filter.GroupOrCategory = testcase.GroupOrCategory;
							filters.Add(definition.PK.ToString());
							break;
						}
					default:
						{
							var filter = (ModuleTextFilter)strip.CurrentModuleFilter;
							filter.ComparisonOperator = testcase.ComparisonOperator;
							filter.Property = testcase.Code;
							filter.OrCategory = testcase.OrCategory;
							filter.IsActive = true;
							filter.GroupName = testcase.GroupName;
							filter.GroupOrCategory = testcase.GroupOrCategory;
							filters.Add(filter.Property);
							break;
						}
				}
			}
			return filters;
		}

		public void TestTagFilter_OneAggregation_ShouldAggregateQuery()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("OR1", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR2", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR3", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("EXC", "Tag Magnitude", true, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("AND", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("INC", "Workflow Type", false, FilterOrCategory.Blue, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("ACI", "Workflow Type", false, FilterOrCategory.Blue, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, @"FH_WorkflowType in \( \'ACI\', \'INC\' \)"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude = \'" + filterstrips[3] + @"\' JOIN"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude = \'" + filterstrips[4] + @"\' WHERE"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[0] + "\', \'" + filterstrips[1] + "\', \'" + filterstrips[2] + @"\' \) WHERE"));
		}

		public void TestTagFilter_TwoAggregations_ShouldAggregateQueriesCorrectly()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("OR1", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR2", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR3", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR4", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR5", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR6", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[0] + "\', \'" + filterstrips[1] + "\', \'" + filterstrips[2] + @"\' \) WHERE"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[3] + "\', \'" + filterstrips[4] + "\', \'" + filterstrips[5] + @"\' \) WHERE"));
		}

		public void TestTagFilter_TwoAggregations_ShouldAggregateNotInAndInherited()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("IN1", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("IN2", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("IH1", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("IH2", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NO1", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NO2", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator, "", FilterOrCategory.None)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[0] + "\', \'" + filterstrips[1] + @"\' \) WHERE"));
			Assert(Regex.IsMatch(sql, @"FH_PK not in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[4] + "\', \'" + filterstrips[5] + @"\' \) WHERE"));
			Assert(Regex.IsMatch(sql, @"SELECT FH_PK FROM dbo.GetWorkflowsAndDescendentsFromTagMagnitude \( \'" + filterstrips[2] + @"\' \)"));
			Assert(Regex.IsMatch(sql, @"SELECT FH_PK FROM dbo.GetWorkflowsAndDescendentsFromTagMagnitude \( \'" + filterstrips[3] + @"\' \)"));
		}

		public void TestTagFilter_NoAggregations_ShouldAggregateNothing()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("NC1", "Tag Magnitude", false, FilterOrCategory.None, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NC2", "Tag Magnitude", false, FilterOrCategory.None, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NC3", "Tag Magnitude", false, FilterOrCategory.None, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NC4", "Tag Magnitude", false, FilterOrCategory.None, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude = \'" + filterstrips[0] + @"\' WHERE"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude = \'" + filterstrips[1] + @"\' WHERE"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude = \'" + filterstrips[2] + @"\' WHERE"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude = \'" + filterstrips[3] + @"\' WHERE"));
		}

		public void TestTagFilter_TwoSubGroupsSameColor_TwoAggregations_ShouldNotAggregateOverSubGroups()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("FA1", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Fabulous", FilterOrCategory.Green),
				new FilterStripTestCase("FA2", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Fabulous", FilterOrCategory.Green),
				new FilterStripTestCase("NF1", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Not Fabolous", FilterOrCategory.Green),
				new FilterStripTestCase("NF2", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Not Fabolous", FilterOrCategory.Green),
				new FilterStripTestCase("NF3", "Tag Magnitude", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Not Fabolous", FilterOrCategory.Green)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[0] + "\', \'" + filterstrips[1] + @"\' \) WHERE"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[2] + "\', \'" + filterstrips[3] + "\', \'" + filterstrips[4] + @"\' \) WHERE"));
		}

		public void TestTagFilter_TwoSubGroupsDifferentColor_TwoAggregations_ShouldNotAggregateOverSubGroups()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("SAR", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Red", FilterOrCategory.Red),
				new FilterStripTestCase("SIM", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Red", FilterOrCategory.Red),
				new FilterStripTestCase("GRI", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Red", FilterOrCategory.Red),
				new FilterStripTestCase("CHU", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Blue", FilterOrCategory.Blue),
				new FilterStripTestCase("CAB", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Blue", FilterOrCategory.Blue),
				new FilterStripTestCase("TUC", "Tag Magnitude", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Blue", FilterOrCategory.Blue)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[0] + "\', \'" + filterstrips[1] + "\', \'" + filterstrips[2] + @"\' \) WHERE"));
			Assert(Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude in \( \'" + filterstrips[3] + "\', \'" + filterstrips[4] + "\', \'" + filterstrips[5] + @"\' \) WHERE"));
		}

		public void TestTagFilters_NoAggregation_EmptyBizo_ShouldReturnOriginalQuery()
		{
			var query = new ZQuery();
			TagQueryProvider.CreateTagMagnitudeFilterSqlAndAddToQuery(new List<ZGuid>() { BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(Factory, "TAG", isExclusive: false), "TAG").PK }, new ZSqlParameterCollection(), false, false, false, JoinCondition.And, query);
			var subGroup = new TagMagnitudeSubGroup(new ProcessHeaderFilterBusinessObject(), ProcessHeader.ModuleFilterConstants.TagMagnitude);
			var newQuery = subGroup.GetSubQuery(query);
			Assert("The query should not be changed", newQuery == query);
		}

		public void TestFilterStripLayout_WithTagAndTagDefinitionFilters_ShouldProduceWorkingQuery()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Worklfow");
			workflow.AddTag(config.PrincessLunaTag);
			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var magFilter = filterBizo.AddFilterStrip<ModuleGuidFilter>(ProcessHeader.ModuleFilterConstants.TagMagnitude);
			magFilter.Property = config.PrincessLunaTag.PK;

			var defFilter = filterBizo.AddFilterStrip<ModuleGuidFilter>(ProcessHeader.ModuleFilterConstants.TagDefinitionCode);
			defFilter.Property = config.PrincessLunaTag.Definition.PK;

			var query = filterBizo.Filter;
			var result = Factory.Load<ProcessHeader>(query).SingleOrDefault();
			AssertNotNull("The query should not have mixed up TagMagnitude and TagDefinition PK parameters because of TagSubGroup, thus returning the matching workflow. SAD! " + query.LiteralTextSqlFormatted, result);
		}

		public void TestTagFilters_NoAggregation_TestGridColourStripBusinessObject()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var gridColorBizo = new GridColourStripBusinessObject(processHeaderBizo, null, typeof(DummyLogged));
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("TG1", "Tag Magnitude", false, FilterOrCategory.RosyBrown, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("TG2", "Tag Magnitude", false, FilterOrCategory.RosyBrown, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
			};

			var filterstrips = SetupFilterStrips(testcases, gridColorBizo.FilterStrips);
			var gridColourFilter = gridColorBizo.Filter;
			var sql = gridColourFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert("The query should contain the first tag filter without changing it.", Regex.IsMatch(sql, @"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude = \'" + filterstrips[0] + "\' WHERE"));
			Assert("The query should contain the second tag filter without changing it.", Regex.IsMatch(sql, @"OR \( FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink jobLink ON jobLink.TGL_ParentId = FH_FH_ParentHeader AND jobLink.TGL_TGM_Magnitude = \'" + filterstrips[1] + "\' WHERE "));
		}

		public void TestFindColourByTagFilter()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "TAG", isExclusive: false);
			var tag = BMSTestHelper.CreateTagMagnitude(definition, "TAG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.AddTag(tag);

			using (var form = new ZForm(jobHeader))
			{
				var grid = new ZGrid { BindTo = "ProcessHeaders" };
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("FH_FC_CurrentComponent", 80));
				form.Controls.Add(grid);
				grid.SetDataBinding(jobHeader, "ProcessHeaders");

				var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
				var gridColorBizo = new GridColourStripBusinessObject(processHeaderBizo, null, typeof(DummyLogged));
				gridColorBizo.BGColor = Color.Red;

				var strip = gridColorBizo.FilterStrips.AddNew("Tag Magnitude");

				var filter = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
				filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator;
				filter.Property = tag.PK;
				filter.IsActive = true;

				Factory.Save();

				var colorScheme = jobHeader.Factory.New<GridColourScheme>();
				colorScheme.ColourStrips.Add(gridColorBizo);

				GridColourSchemeManagerForTest.RecreateGridColourSchemeManager(grid, colorScheme, false);

				CombineAssertions(() =>
				{
					AssertEquals("The colour of the first Workflow should be red", gridColorBizo.BGColor, GridColourSchemeManagerForTest.GetCustomRowBackgroundColour(grid, 0));
					AssertEquals("The colour of the second Workflow should be empty", new Color(), GridColourSchemeManagerForTest.GetCustomRowBackgroundColour(grid, 1));
				});
			}
		}

		#region Task Tag Definition Code

		public void TestTaskTagDefinitionCode_NotIn()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 30);
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, 30);

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			task1.AddTag(magnitude);
			task3.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var tagDefinitionCodeFilterStrip = (ModuleGuidInSubCollectionFilter)bizo[ProcessHeader.ModuleFilterConstants.TagTaskDefinitionCode];
			tagDefinitionCodeFilterStrip.IsActive = true;
			tagDefinitionCodeFilterStrip.Property = definition.PK;
			tagDefinitionCodeFilterStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionNotContains(workflow1, workflows);
			AssertCollectionNotContains(workflow3, workflows);
			AssertCollectionContains(workflow2, workflows);
			AssertCollectionContains(jobHeader, workflows);
		}

		public void TestTaskTagDefinitionCode()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 30);
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, 30);

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			task1.AddTag(magnitude);
			task3.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var taskTagDefinitionFilterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagTaskDefinitionCode];
			taskTagDefinitionFilterStrip.IsActive = true;
			taskTagDefinitionFilterStrip.Property = definition.PK;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionContains(workflow1, workflows);
			AssertCollectionContains(workflow3, workflows);
		}

		#endregion

		#region Task Tag Magnitude

		public void TestTaskTagMagnitude_NotIn()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 30);
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, 30);

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			task1.AddTag(magnitude);
			task3.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var taskTagMagnitudeFilterStrip = (ModuleGuidInSubCollectionFilter)bizo[ProcessHeader.ModuleFilterConstants.TagTaskMagnitude];
			taskTagMagnitudeFilterStrip.IsActive = true;
			taskTagMagnitudeFilterStrip.Property = magnitude.PK;
			taskTagMagnitudeFilterStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionNotContains(workflow1, workflows);
			AssertCollectionNotContains(workflow3, workflows);
			AssertCollectionContains(jobHeader, workflows);
			AssertCollectionContains(workflow2, workflows);
		}

		public void TestTaskTagMagnitude()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 30);
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, 30);

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			task1.AddTag(magnitude);
			task3.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var taskTagMagnitudeFilterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagTaskMagnitude];
			taskTagMagnitudeFilterStrip.IsActive = true;
			taskTagMagnitudeFilterStrip.Property = magnitude.PK;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionContains(workflow1, workflows);
			AssertCollectionContains(workflow3, workflows);
		}

		#endregion

		#region Tag Definition Code

		public void TestTagDefinitionCode_NotIn()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			workflow1.AddTag(magnitude);
			workflow3.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var tagDefinitionFilterStrip = (ModuleGuidInSubCollectionFilter)bizo[ProcessHeader.ModuleFilterConstants.TagDefinitionCode];
			tagDefinitionFilterStrip.IsActive = true;
			tagDefinitionFilterStrip.Property = definition.PK;
			tagDefinitionFilterStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionNotContains(workflow1, workflows);
			AssertCollectionNotContains(workflow3, workflows);
			AssertCollectionContains(jobHeader, workflows);
			AssertCollectionContains(workflow2, workflows);
		}

		public void TestTagDefinitionCode()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			workflow1.AddTag(magnitude);
			workflow3.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var taskTagDefinitionCodeFilterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagDefinitionCode];
			taskTagDefinitionCodeFilterStrip.IsActive = true;
			taskTagDefinitionCodeFilterStrip.Property = definition.PK;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionContains(workflow1, workflows);
			AssertCollectionContains(workflow3, workflows);
		}

		#endregion

		#region Tag Definition Code (Job Fallback)

		public void TestTagDefinitionCode_TagOnJob()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Twilight Sparkle");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Friendship is magic");

			jobHeader.AddTag(config.PrincessCelestiaTag);
			workflow.AddTag(config.RedTag);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagDefinitionCode];

			filter.IsActive = true;
			filter.Property = config.PriorityTags.PK;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Tag is applied to workflow directly, so only the workflow should be loaded", new[] { workflow }, results);

			filter.Property = config.PonyTags.PK;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Tag is applied to job, so both it and the workflow should be loaded", new[] { workflow, jobHeader }, results);
		}

		public void TestTagDefinitionCode_TagOnJob_NotIn()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Twilight Sparkle");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Friendship is magic");

			jobHeader.AddTag(config.PrincessCelestiaTag);
			workflow.AddTag(config.RedTag);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagDefinitionCode];

			filter.IsActive = true;
			filter.Property = config.PriorityTags.PK;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Tag is applied to workflow directly, so only the job should be loaded", new[] { jobHeader }, results);

			filter.Property = config.PonyTags.PK;

			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Tag is applied to job, so neither should be loaded", Array.Empty<ProcessHeader>(), results);
		}

		public void TestTagDefinitionCode_TagOnWorkflow()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Twilight Sparkle");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1");
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_1");
			var workflow1_1_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_2");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "WF2");
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF2_1");

			workflow1_1.GetOrCreateLinkToParent(workflow1);
			workflow1_1_1.GetOrCreateLinkToParent(workflow1_1);
			workflow1_2.GetOrCreateLinkToParent(workflow1);

			workflow1.AddTag(config.RainbowDashTag);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagDefinitionCode];
			filter.IsActive = true;
			filter.Property = config.PonyTags.PK;

			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator;
			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should return tagged workflows only", 1, results.Length);
			AssertCollectionContains(workflow1, results);

			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should return tagged workflows.", 4, results.Length);
			AssertCollectionContains(workflow1, results);
			AssertCollectionContains(workflow1_1, results);
			AssertCollectionContains(workflow1_1_1, results);
			AssertCollectionContains(workflow1_2, results);
			AssertCollectionNotContains(workflow2, results);
			AssertCollectionNotContains(workflow2_1, results);
			AssertCollectionNotContains(jobHeader, results);

			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should return non-tagged workflows.", 6, results.Length);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow2_1, results);
			AssertCollectionContains(jobHeader, results);
			AssertCollectionContains(workflow1_1, results);
			AssertCollectionContains(workflow1_1_1, results);
			AssertCollectionContains(workflow1_2, results);
			AssertCollectionNotContains(workflow1, results);

			filter.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedNorInheritedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should return non-tagged workflows.", 3, results.Length);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow2_1, results);
			AssertCollectionContains(jobHeader, results);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionNotContains(workflow1_1, results);
			AssertCollectionNotContains(workflow1_1_1, results);
			AssertCollectionNotContains(workflow1_2, results);
		}
		#endregion

		#region Tag Definition Code (Aggregation)

		public void TestTagDefinitionCodeFilter_OneAggregation_In_ShouldFindResults()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var definition1 = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude1 = definition1.Magnitudes.AddNew();
			magnitude1.TGM_Code = "WO1";

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude2 = definition2.Magnitudes.AddNew();
			magnitude2.TGM_Code = "WO2";

			workflow1.AddTag(magnitude1);
			workflow3.AddTag(magnitude2);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();

			var strip = bizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.TagDefinitionCode);
			strip.OrCategory = FilterOrCategory.CornflowerBlue;

			var taskTagDefinitionCodeFilterStrip1 = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
			taskTagDefinitionCodeFilterStrip1.OrCategory = FilterOrCategory.CornflowerBlue;
			taskTagDefinitionCodeFilterStrip1.IsActive = true;
			taskTagDefinitionCodeFilterStrip1.Property = definition1.PK;

			strip = bizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.TagDefinitionCode);
			strip.OrCategory = FilterOrCategory.CornflowerBlue;

			var taskTagDefinitionCodeFilterStrip2 = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
			taskTagDefinitionCodeFilterStrip2.OrCategory = FilterOrCategory.CornflowerBlue;
			taskTagDefinitionCodeFilterStrip2.IsActive = true;
			taskTagDefinitionCodeFilterStrip2.Property = definition2.PK;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionContains(workflow1, workflows);
			AssertCollectionContains(workflow3, workflows);
			AssertCollectionNotContains(workflow2, workflows);
		}

		public void TestTagDefinitionCodeFilter_OneAggregation_NotIn_ShouldFindResults()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var definition1 = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude1 = definition1.Magnitudes.AddNew();
			magnitude1.TGM_Code = "WO1";

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude2 = definition2.Magnitudes.AddNew();
			magnitude2.TGM_Code = "WO2";

			workflow1.AddTag(magnitude1);
			workflow3.AddTag(magnitude2);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();

			var strip = bizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.TagDefinitionCode);
			strip.OrCategory = FilterOrCategory.CornflowerBlue;

			var taskTagDefinitionCodeFilterStrip1 = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
			taskTagDefinitionCodeFilterStrip1.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator;
			taskTagDefinitionCodeFilterStrip1.OrCategory = FilterOrCategory.CornflowerBlue;
			taskTagDefinitionCodeFilterStrip1.IsActive = true;
			taskTagDefinitionCodeFilterStrip1.Property = definition1.PK;

			strip = bizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.TagDefinitionCode);
			strip.OrCategory = FilterOrCategory.CornflowerBlue;

			var taskTagDefinitionCodeFilterStrip2 = (ModuleGuidAppliedToSubCollectionFilter)strip.CurrentModuleFilter;
			taskTagDefinitionCodeFilterStrip1.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator;
			taskTagDefinitionCodeFilterStrip2.OrCategory = FilterOrCategory.CornflowerBlue;
			taskTagDefinitionCodeFilterStrip2.IsActive = true;
			taskTagDefinitionCodeFilterStrip2.Property = definition2.PK;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionNotContains(workflow1, workflows);
			AssertCollectionNotContains(workflow3, workflows);
			AssertCollectionContains(workflow2, workflows);
		}

		public void TestTagDefinitionCodeFilter_OneAggregation_ShouldAggregateQuery()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("OR1", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR2", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR3", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("AND", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("INC", "Workflow Type", false, FilterOrCategory.Blue, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("ACI", "Workflow Type", false, FilterOrCategory.Blue, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);

			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, @"FH_WorkflowType in \( \'ACI\', \'INC\' \)"));
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[0]}', '{filterstrips[1]}', '{filterstrips[2]}' \)"));
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag = '{filterstrips[3]}'"));
		}

		public void TestTagDefinitionCodeFilter_TwoAggregations_ShouldAggregateQueriesCorrectly()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("OR1", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR2", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR3", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR4", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR5", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("OR6", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[0]}', '{filterstrips[1]}', '{filterstrips[2]}' \)"));
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[3]}', '{filterstrips[4]}', '{filterstrips[5]}' \)"));
		}

		public void TestTagDefinitionCodeFilter_TwoAggregations_ShouldAggregateNotInAndInherited() //
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("IN1", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("IN2", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("IH1", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("IH2", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NO1", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NO2", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator, "", FilterOrCategory.None)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[0]}', '{filterstrips[1]}' \)"));
			Assert(Regex.IsMatch(sql, $@"FH_PK not in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[4]}', '{filterstrips[5]}' \)"));
			Assert(Regex.IsMatch(sql, $@"SELECT Child.FH_PK FROM dbo.TagLink JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK CROSS APPLY dbo.GetWorkflowDescendentHierarchy \( TGL_ParentId, 30 \) AS Child WHERE TGM_TGD_Tag = '{filterstrips[2]}'"));
			Assert(Regex.IsMatch(sql, $@"SELECT Child.FH_PK FROM dbo.TagLink JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK CROSS APPLY dbo.GetWorkflowDescendentHierarchy \( TGL_ParentId, 30 \) AS Child WHERE TGM_TGD_Tag = '{filterstrips[3]}'"));
		}

		public void TestTagDefinitionCodeFilter_NoAggregations_ShouldAggregateNothing()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("NC1", "Tag Definition Code", false, FilterOrCategory.None, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NC2", "Tag Definition Code", false, FilterOrCategory.None, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NC3", "Tag Definition Code", false, FilterOrCategory.None, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("NC4", "Tag Definition Code", false, FilterOrCategory.None, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag = '{filterstrips[0]}'"));
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag = '{filterstrips[1]}'"));
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag = '{filterstrips[2]}'"));
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag = '{filterstrips[3]}'"));
		}

		public void TestTagDefinitionCodeFilter_TwoSubGroupsSameColor_TwoAggregations_ShouldNotAggregateOverSubGroups()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("FA1", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Fabulous", FilterOrCategory.Green),
				new FilterStripTestCase("FA2", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Fabulous", FilterOrCategory.Green),
				new FilterStripTestCase("NF1", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Not Fabolous", FilterOrCategory.Green),
				new FilterStripTestCase("NF2", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Not Fabolous", FilterOrCategory.Green),
				new FilterStripTestCase("NF3", "Tag Definition Code", false, FilterOrCategory.Red, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Not Fabolous", FilterOrCategory.Green)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[0]}', '{filterstrips[1]}' \)"));
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[2]}', '{filterstrips[3]}', '{filterstrips[4]}' \)"));
		}

		public void TestTagDefinitionCodeFilter_TwoSubGroupsDifferentColor_TwoAggregations_ShouldNotAggregateOverSubGroups()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("SAR", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Red", FilterOrCategory.Red),
				new FilterStripTestCase("SIM", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Red", FilterOrCategory.Red),
				new FilterStripTestCase("GRI", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Red", FilterOrCategory.Red),
				new FilterStripTestCase("CHU", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Blue", FilterOrCategory.Blue),
				new FilterStripTestCase("CAB", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Blue", FilterOrCategory.Blue),
				new FilterStripTestCase("TUC", "Tag Definition Code", false, FilterOrCategory.Green, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "Blue", FilterOrCategory.Blue)
			};

			var filterstrips = SetupFilterStrips(testcases, processHeaderBizo.FilterStrips);
			var layout = Factory.New<StmModuleFilter>();
			processHeaderBizo.FillLayoutValues(layout, ModuleIDs.ProcessHeader);
			var processHeaderFilter = processHeaderBizo.Filter;
			var sql = processHeaderFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[0]}', '{filterstrips[1]}', '{filterstrips[2]}' \)"));
			Assert(Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag in \( '{filterstrips[3]}', '{filterstrips[4]}', '{filterstrips[5]}' \)"));
		}

		public void TestTagDefinitionCodeFilters_NoAggregation_EmptyBizo_ShouldReturnOriginalQuery()
		{
			var query = new ZQuery();
			TagQueryProvider.CreateTagDefinitionCodeFilterSqlAndAddToQuery(new List<ZGuid>() { BMSTestHelper.CreateTagDefinition(Factory, "TAG", isExclusive: false).PK }, new ZSqlParameterCollection(), false, false, JoinCondition.And, query);
			var subGroup = new TagDefinitionCodeSubGroup(new ProcessHeaderFilterBusinessObject(), ProcessHeader.ModuleFilterConstants.TagDefinitionCode);
			var newQuery = subGroup.GetSubQuery(query);
			Assert("The query should not be changed", newQuery == query);
		}

		public void TestTagDefinitionCodeFilters_NoAggregation_TestGridColourStripBusinessObject()
		{
			var processHeaderBizo = new ProcessHeaderFilterBusinessObject();
			var gridColorBizo = new GridColourStripBusinessObject(processHeaderBizo, null, typeof(DummyLogged));
			var testcases = new List<FilterStripTestCase>
			{
				new FilterStripTestCase("TG1", "Tag Definition Code", false, FilterOrCategory.RosyBrown, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
				new FilterStripTestCase("TG2", "Tag Definition Code", false, FilterOrCategory.RosyBrown, ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator, "", FilterOrCategory.None),
			};

			var filterstrips = SetupFilterStrips(testcases, gridColorBizo.FilterStrips);
			var gridColourFilter = gridColorBizo.Filter;
			var sql = gridColourFilter.LiteralTextSqlFormatted;
			sql = Regex.Replace(sql, @"\s+", " ");
			Assert("The query should contain the first tag filter without changing it.", Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag = '{filterstrips[0]}'"));
			Assert("The query should contain the second tag filter without changing it.", Regex.IsMatch(sql, $@"FH_PK in \( SELECT FH_PK FROM dbo.ProcessHeader JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK WHERE TGM_TGD_Tag = '{filterstrips[1]}'"));
		}

		#endregion

		#region Tag Magnitude (Exclusive)

		public void TestTagMagnitude_Exclusive()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ACK", isExclusive: true);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "EX1");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "EX2");

			jobHeader.AddTag(magnitude1);
			workflow1.AddTag(magnitude2);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var tagMagnitudeFilterStrip = (ModuleGuidInSubCollectionFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			tagMagnitudeFilterStrip.IsActive = true;
			tagMagnitudeFilterStrip.Property = magnitude1.PK;
			tagMagnitudeFilterStrip.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);

			AssertCollectionContains(jobHeader, workflows);
			AssertCollectionContains(workflow2, workflows);
			AssertCollectionNotContains(workflow1, workflows);

			tagMagnitudeFilterStrip.Property = magnitude2.PK;

			workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, workflows.Length);

			AssertCollectionContains(workflow1, workflows);
			AssertCollectionNotContains(jobHeader, workflows);
			AssertCollectionNotContains(workflow2, workflows);
		}

		public void TestTagMagnitude_Exclusive_NotIn()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ACK", isExclusive: true);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "EX1");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "EX2");

			jobHeader.AddTag(magnitude1);
			workflow1.AddTag(magnitude2);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var tagMagnitudeFilterStrip = (ModuleGuidInSubCollectionFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			tagMagnitudeFilterStrip.IsActive = true;
			tagMagnitudeFilterStrip.Property = magnitude1.PK;
			tagMagnitudeFilterStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, workflows.Length);

			AssertCollectionContains(workflow1, workflows);
			AssertCollectionNotContains(jobHeader, workflows);
			AssertCollectionNotContains(workflow2, workflows);

			tagMagnitudeFilterStrip.Property = magnitude2.PK;

			workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);

			AssertCollectionContains(jobHeader, workflows);
			AssertCollectionContains(workflow2, workflows);
			AssertCollectionNotContains(workflow1, workflows);
		}

		public void TestTagMagnitude_Exclusive_ParentAndWorkflowHaveTheSameTag()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ACK", isExclusive: true);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "EX1");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "EX2");

			jobHeader.AddTag(magnitude1);
			workflow1.AddTag(magnitude1);
			workflow2.AddTag(magnitude2);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var tagMagnitudeFilterStrip = (ModuleGuidInSubCollectionFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			tagMagnitudeFilterStrip.IsActive = true;
			tagMagnitudeFilterStrip.Property = magnitude1.PK;
			tagMagnitudeFilterStrip.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);

			AssertCollectionContains(workflow1, workflows);
			AssertCollectionContains(jobHeader, workflows);
			AssertCollectionNotContains(workflow2, workflows);

			tagMagnitudeFilterStrip.Property = magnitude2.PK;

			workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, workflows.Length);

			AssertCollectionContains(workflow2, workflows);
			AssertCollectionNotContains(workflow1, workflows);
			AssertCollectionNotContains(jobHeader, workflows);
		}

		public void TestTagMagnitude_Exclusive_WhenTagsFromDifferentExclusiveGroupsApplied()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var exclusiveTagGroup = BMSTestHelper.CreateTagDefinition(Factory, "EXC", isExclusive: true);
			var exclusiveTag = BMSTestHelper.CreateTagMagnitude(exclusiveTagGroup, "ONE");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Nope. Nope. Nope.");

			jobHeader.AddTag(config.RedTag);
			workflow.AddTag(exclusiveTag);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];

			filter.IsActive = true;
			filter.Property = config.RedTag.PK;

			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflow }, Factory.Load<ProcessHeader>(bizo.Filter));

			filter.Property = exclusiveTag.PK;
			AssertContainsExactElementsInAnyOrder(new[] { workflow }, Factory.Load<ProcessHeader>(bizo.Filter));

			jobHeader.RemoveTag(config.RedTag);
			workflow.RemoveTag(exclusiveTag);

			jobHeader.AddTag(exclusiveTag);
			workflow.AddTag(config.RedTag);
			Factory.Save();

			filter.Property = config.RedTag.PK;

			AssertContainsExactElementsInAnyOrder(new[] { workflow }, Factory.Load<ProcessHeader>(bizo.Filter));

			filter.Property = exclusiveTag.PK;
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflow }, Factory.Load<ProcessHeader>(bizo.Filter));
		}

		#endregion

		#region Tag Magnitude

		public void TestTagMagnitude_WhenTagGroupIsNotExclusive_ShouldNotIncludeExclusiveSQL()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];

			filter.IsActive = true;
			filter.Property = config.RedTag.PK;

			var exclusiveTagSql = bizo.Filter.LiteralTextSqlFormatted;

			filter.Property = config.PrincessCelestiaTag.PK;
			AssertNotEquals("Query should NOT include exclusive tag logic since we know the user is looking for a non-exclusive tag", exclusiveTagSql, bizo.Filter.ParameterisedText.ParameterisedQueryText);
			AssertNotContains("We want to ensure the query is different, not the parameters", config.PrincessCelestiaTag.PK.ToString(), bizo.Filter.ParameterisedText.ParameterisedQueryText);
		}

		public void TestTagMagnitude_NotIn()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			workflow1.AddTag(magnitude);
			workflow3.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var tagMagnitudeFilterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			tagMagnitudeFilterStrip.IsActive = true;
			tagMagnitudeFilterStrip.Property = magnitude.PK;
			tagMagnitudeFilterStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionNotContains(workflow1, workflows);
			AssertCollectionNotContains(workflow3, workflows);
			AssertCollectionContains(jobHeader, workflows);
			AssertCollectionContains(workflow2, workflows);
		}

		public void TestTagMagnitude()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			workflow1.AddTag(magnitude);
			workflow3.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var tagMagnitudeFilterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			tagMagnitudeFilterStrip.IsActive = true;
			tagMagnitudeFilterStrip.Property = magnitude.PK;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(2, workflows.Length);
			AssertCollectionContains(workflow1, workflows);
			AssertCollectionContains(workflow3, workflows);
		}

		#endregion

		#region Tag Magnitude (Job Fallback)

		public void TestTagMagnitude_TagOnJob_NotIn()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow2_1 = jobHeader2.ProcessHeaders[0];

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			jobHeader1.AddTag(magnitude);
			workflow2_1.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			filterStrip.IsActive = true;
			filterStrip.Property = magnitude.PK;
			filterStrip.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(1, workflows.Length);
			AssertCollectionNotContains(jobHeader1, workflows);
			AssertCollectionNotContains(workflow1_1, workflows);
			AssertCollectionNotContains(workflow1_2, workflows);

			AssertCollectionContains(jobHeader2, workflows);
			AssertCollectionNotContains(workflow2_1, workflows);
		}

		public void TestTagMagnitude_TagOnJob()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			jobHeader.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			filterStrip.IsActive = true;
			filterStrip.Property = magnitude.PK;

			var workflows = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(3, workflows.Length);
			AssertCollectionContains(jobHeader, workflows);
			AssertCollectionContains(workflow1, workflows);
			AssertCollectionContains(workflow2, workflows);
		}

		public void TestTagMagnitude_TagOnWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobWorkflow = jobHeader.ProcessHeaders[0];
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1");
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_1");
			var workflow1_1_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_2");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "WF2");
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF2_1");

			workflow1_1.GetOrCreateLinkToParent(workflow1);
			workflow1_1_1.GetOrCreateLinkToParent(workflow1_1);
			workflow1_2.GetOrCreateLinkToParent(workflow1);

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_IsExclusive = false;
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			workflow1.AddTag(magnitude);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			filterStrip.IsActive = true;
			filterStrip.Property = magnitude.PK;

			filterStrip.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedComparisonOperator;
			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should only return tagged workflows", 1, results.Length);
			AssertCollectionContains(workflow1, results);

			filterStrip.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should return tagged workflows and its children", 4, results.Length);
			AssertCollectionContains(workflow1, results);
			AssertCollectionContains(workflow1_1, results);
			AssertCollectionContains(workflow1_1_1, results);
			AssertCollectionContains(workflow1_2, results);
			AssertCollectionNotContains(jobHeader, results);
			AssertCollectionNotContains(jobWorkflow, results);
			AssertCollectionNotContains(workflow2, results);
			AssertCollectionNotContains(workflow2_1, results);

			filterStrip.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should only return non-tagged workflows", 7, results.Length);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionContains(workflow1_1, results);
			AssertCollectionContains(workflow1_1_1, results);
			AssertCollectionContains(workflow1_2, results);
			AssertCollectionContains(jobHeader, results);
			AssertCollectionContains(jobWorkflow, results);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow2_1, results);

			filterStrip.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedNorInheritedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should only return non-tagged workflows nor its children", 4, results.Length);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionNotContains(workflow1_1, results);
			AssertCollectionNotContains(workflow1_1_1, results);
			AssertCollectionNotContains(workflow1_2, results);
			AssertCollectionContains(jobHeader, results);
			AssertCollectionContains(jobWorkflow, results);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow2_1, results);
		}

		public void TestTagMagnitude_TagExclusiveOnWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobWorkflow = jobHeader.ProcessHeaders[0];
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1");
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_1");
			var workflow1_1_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_2");
			var workflow1_2_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_2_1");
			var workflow1_2_2 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1_2_2");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "WF2");
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF2_1");

			workflow1_1.GetOrCreateLinkToParent(workflow1);
			workflow1_1_1.GetOrCreateLinkToParent(workflow1_1);
			workflow1_2.GetOrCreateLinkToParent(workflow1);
			workflow1_2_1.GetOrCreateLinkToParent(workflow1_2);
			workflow1_2_2.GetOrCreateLinkToParent(workflow1_2);

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude1 = definition.Magnitudes.AddNew();
			magnitude1.TGM_Code = "WOW";
			var magnitude2 = definition.Magnitudes.AddNew();
			magnitude2.TGM_Code = "MOM";

			workflow1.AddTag(magnitude1);
			workflow1_2.AddTag(magnitude2);
			workflow1_2_1.AddTag(magnitude1);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filterStrip = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];
			filterStrip.IsActive = true;
			filterStrip.Property = magnitude1.PK;

			//Non-exclusive tag group
			definition.TGD_IsExclusive = false;
			Factory.Save();

			filterStrip.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator;
			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should return tagged workflows and its children", 6, results.Length);
			AssertCollectionContains(workflow1, results);
			AssertCollectionContains(workflow1_1, results);
			AssertCollectionContains(workflow1_1_1, results);
			AssertCollectionContains(workflow1_2_1, results);
			AssertCollectionContains(workflow1_2, results);
			AssertCollectionContains(workflow1_2_2, results);
			AssertCollectionNotContains(jobHeader, results);
			AssertCollectionNotContains(jobWorkflow, results);
			AssertCollectionNotContains(workflow2, results);
			AssertCollectionNotContains(workflow2_1, results);

			filterStrip.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedNorInheritedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should only return non-tagged workflows nor its children", 4, results.Length);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionNotContains(workflow1_1, results);
			AssertCollectionNotContains(workflow1_1_1, results);
			AssertCollectionNotContains(workflow1_2_1, results);
			AssertCollectionNotContains(workflow1_2, results);
			AssertCollectionNotContains(workflow1_2_2, results);
			AssertCollectionContains(jobHeader, results);
			AssertCollectionContains(jobWorkflow, results);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow2_1, results);

			//Exclusive Tag Group
			definition.TGD_IsExclusive = true;
			Factory.Save();

			filterStrip.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.IsAppliedOrInheritedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should return tagged workflows and its children", 4, results.Length);
			AssertCollectionContains(workflow1, results);
			AssertCollectionContains(workflow1_1, results);
			AssertCollectionContains(workflow1_1_1, results);
			AssertCollectionContains(workflow1_2_1, results);
			AssertCollectionNotContains(workflow1_2, results);
			AssertCollectionNotContains(workflow1_2_2, results);
			AssertCollectionNotContains(jobHeader, results);
			AssertCollectionNotContains(jobWorkflow, results);
			AssertCollectionNotContains(workflow2, results);
			AssertCollectionNotContains(workflow2_1, results);

			filterStrip.ComparisonOperator = ModuleGuidAppliedToSubCollectionFilter.NotAppliedNorInheritedComparisonOperator;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals("Should only return non-tagged workflows nor its children", 6, results.Length);
			AssertCollectionNotContains(workflow1, results);
			AssertCollectionNotContains(workflow1_1, results);
			AssertCollectionNotContains(workflow1_1_1, results);
			AssertCollectionNotContains(workflow1_2_1, results);
			AssertCollectionContains(workflow1_2, results);
			AssertCollectionContains(workflow1_2_2, results);
			AssertCollectionContains(jobHeader, results);
			AssertCollectionContains(jobWorkflow, results);
			AssertCollectionContains(workflow2, results);
			AssertCollectionContains(workflow2_1, results);
		}

		public void TestTagFilter_WhenExclusiveTagsAppliedAndOverriddenAtWorkflowLevel_AndUnrelatedTagsApplied()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Spoonflow");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Sporkflow");

			jobHeader.AddTag(config.PrincessCelestiaTag);
			workflow.AddTag(config.PrincessCelestiaTag);

			jobHeader.AddTag(config.PlatinumTag);
			workflow.AddTag(config.RedTag);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo[ProcessHeader.ModuleFilterConstants.TagMagnitude];

			filter.IsActive = true;
			filter.Property = config.RedTag.PK;

			AssertContainsExactElementsInAnyOrder(new[] { workflow }, Factory.Load<ProcessHeader>(filter.Query));

			filter.Property = config.PlatinumTag.PK;
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader }, Factory.Load<ProcessHeader>(filter.Query));

			filter.Property = config.PrincessCelestiaTag.PK;
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflow }, Factory.Load<ProcessHeader>(filter.Query));

			workflow.RemoveTag(config.RedTag);
			workflow.AddTag(config.PlatinumTag);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflow }, Factory.Load<ProcessHeader>(filter.Query));

			filter.Property = config.PrincessCelestiaTag.PK;
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader, workflow }, Factory.Load<ProcessHeader>(filter.Query));
		}

		#endregion

		#endregion

		#region Task Filters

		public void TestTasksFilter()
		{
			var headerWithAllSpire = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var headerWithNoSpire = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var headerWithSomeSpire = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var headerWithNoTasks = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);

			var spireTask1 = BMSTestHelper.CreateTask(headerWithAllSpire, description: "SPIRE!");
			var spireTask2 = BMSTestHelper.CreateTask(headerWithAllSpire, description: "SPIRE!");

			var blankTask1 = BMSTestHelper.CreateTask(headerWithNoSpire);
			var blankTask2 = BMSTestHelper.CreateTask(headerWithNoSpire);

			var spireTask3 = BMSTestHelper.CreateTask(headerWithSomeSpire, description: "SPIRE!");
			var blankTask3 = BMSTestHelper.CreateTask(headerWithSomeSpire);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var tasksFilter = filterBizo.AddFilterStrip<TasksModuleFilter>("Tasks");
			tasksFilter.SelectedFilters.AddTextFilterStrip("Description", "SPIRE!");

			var subFilterResult = Factory.Load<ProcessTask>(tasksFilter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { spireTask1.PK, spireTask2.PK, spireTask3.PK }, subFilterResult.Select(x => x.PK));

			tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<ProcessHeader>(tasksFilter.Query);
			AssertContainsExactElementsInAnyOrder("Any match: " + tasksFilter.Query.LiteralTextSqlFormatted, new[] { headerWithAllSpire.PK, headerWithSomeSpire.PK }, result.Select(x => x.PK));

			tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<ProcessHeader>(tasksFilter.Query);
			AssertContainsExactElementsInAnyOrder("None match: " + tasksFilter.Query.LiteralTextSqlFormatted, new[] { headerWithNoSpire.PK, headerWithNoTasks.PK }, result.Select(x => x.PK));

			tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<ProcessHeader>(tasksFilter.Query);
			AssertContainsExactElementsInAnyOrder("All match: " + tasksFilter.Query.LiteralTextSqlFormatted, new[] { headerWithAllSpire.PK, headerWithNoTasks.PK }, result.Select(x => x.PK));
		}

		[TestDate(2018, 10, 5)]
		public void TestTasksFilter_AllMatch()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Spaceship Job");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Spaceship");
			var task1 = BMSTestHelper.CreateTask(workflow1, description: "In SPAAAACE");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Scorpion Job");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Scorpion");
			var task2 = BMSTestHelper.CreateTask(workflow2, description: "Beware of SCORPIO");

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var jobOrWorkflowFilter = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			jobOrWorkflowFilter.Property0 = false;
			jobOrWorkflowFilter.Property1 = true; // workflow
			jobOrWorkflowFilter.Property2 = false;

			var tasksFilter = filterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Tasks");
			tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			tasksFilter.SelectedFilters.AddTextFilterStrip("Description", "In SPAAAACE");

			var query = filterBizo.Filter;
			var results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder("Now all of the workflows in the job meet the ALL MATCH criteria, so the job's workflows should have been returned, and yet... " + query.LiteralTextSqlFormatted, new[] { "Spaceship" }, results.Select(x => x.FH_CompletionStatement));
		}

		[TestDate(2018, 10, 5)]
		public void TestTasksFilter_AllMatch_OneJob()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "SOC", "Soccering");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Adiaga");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Adiaga II");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Badiaga");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Aruglia");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "Pizzoza");

			var task1 = BMSTestHelper.CreateTask(workflow1, description: "Fast kicking");
			var task2 = BMSTestHelper.CreateTask(workflow2, description: "Low scoring");
			var task3 = BMSTestHelper.CreateTask(workflow3, description: "And ties?");
			var task4 = BMSTestHelper.CreateTask(workflow3, description: "You bet!", capability: capability);
			var task5 = BMSTestHelper.CreateTask(workflow4, description: "And they'll all be signing autographs", capability: capability);

			Factory.Save();

			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var parentJobFilter = filterBizo.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>("Parent Job");
			parentJobFilter.SelectedModule = ModuleIDs.Organisation.Name;
			parentJobFilter.Property = jobHeader.FH_ParentId;

			var jobOrWorkflowFilter = filterBizo.AddFilterStrip<JobOrWorkflowFilter>(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			jobOrWorkflowFilter.Property0 = false;
			jobOrWorkflowFilter.Property1 = true; // workflow
			jobOrWorkflowFilter.Property2 = false;

			var tasksFilter = filterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Tasks");
			tasksFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			tasksFilter.SelectedFilters.AddGuidFilterStrip("Capability", capability.PK);

			var query = filterBizo.Filter;
			var results = Factory.Load<ProcessHeader>(query);
			AssertContainsExactElementsInAnyOrder(query.LiteralTextSqlFormatted, new[] { "Aruglia", "Pizzoza" }, results.Select(x => x.FH_CompletionStatement));
		}

		public void TestTasksFilterCategory_ShouldBeTasks()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var tasksFilter = (TasksModuleFilter)filterBizo["Tasks"];

			AssertEquals(ProcessHeaderFilterBusinessObject.TasksFilterCategory, tasksFilter.Category);
		}

		#endregion

		#region Constraint Status Filters

		public void TestConstraintStatus()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var bizo = new ProcessHeaderFilterBusinessObject();
			var constraintStatusFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.ConstraintStatus];

			foreach (var testCase in BMSTestHelper.CreateConstraintStatusTestCases(Factory, config))
			{
				constraintStatusFilter.Property = testCase.ConstraintStatus;
				constraintStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				constraintStatusFilter.IsActive = true;

				var results = Factory.Load<ProcessHeader>(bizo.Filter);
				AssertContainsExactElementsInAnyOrder(testCase.ExpectedProcessHeader, results);
			}
		}

		public void TestConstraintStatus_CurrentTasksForCCRAndNonCCRHasSameSequence()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var bizo = new ProcessHeaderFilterBusinessObject();
			var constraintStatusFilter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.ConstraintStatus];

			var currentComponent = config.Buffer;
			var readyCCRWorkflow = BMSTestHelper.CreateWorkflow(Factory, "ready-CCR workflow", currentComponent);
			var nonCCRTask = BMSTestHelper.CreateTask(readyCCRWorkflow, config.NonCCR1.GS_Code, description: "ready-CCR workflow: Non-CCR task", sequence: 10);
			var ccrTask = BMSTestHelper.CreateTask(readyCCRWorkflow, config.CCR.GS_Code, description: "ready-CCR workflow: CCR task", sequence: 10);
			AssertEquals("GIVEN CCR current active-task sequence = Non-CCR current active-task sequence", ccrTask.P9_Sequence, nonCCRTask.P9_Sequence);

			Factory.Save();

			constraintStatusFilter.Property = ConstraintStatusList.Codes.ReadyforConstraint;
			constraintStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			constraintStatusFilter.IsActive = true;
			var filter = (ModuleTextFilter)bizo.ActiveModuleFilters.Single();
			CombineAssertions("WHEN filter by ConstraintStatus = ReadyForConstraint", () =>
			{
				AssertEquals("filter description", ProcessHeader.ModuleFilterConstants.ConstraintStatus, filter.Description);
				AssertEquals("filter value", ConstraintStatusList.Codes.ReadyforConstraint, filter.Property);
			});

			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("THEN it should return readyCCRWorkflow", new[] { readyCCRWorkflow }, results);
		}

		#endregion

		#region Index Filters

		public void TestTagFilters_IndexSearch()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				var field1 = new SearchField("TAG", "Tag", typeof(Guid), uiHidden: true);
				var field2 = new SearchField("TAGGROUP", "Tag Group", typeof(Guid), uiHidden: true);
				var field3 = new SearchField("JOBTAG", "Job Tag", typeof(Guid), uiHidden: true);
				var field4 = new SearchField("JOBTAGGROUP", "Job Tag Group", typeof(Guid), uiHidden: true);
				filterStrip.IndexSearchFields = new SearchFieldCollection(null, new SearchField[] { field1, field2, field3, field4 });

				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var tagPK = Guid.NewGuid();
				var tagGroupPK = Guid.NewGuid();

				var filter1 = filterStrip["TAG"] as IndexSearchTagWithJobOrWorkflowFilter;
				filter1.IsActive = true;
				filter1.Property = tagPK;
				filter1.ComparisonOperator = "applied";

				var filter2 = filterStrip["TAGGROUP"] as IndexSearchTagWithJobOrWorkflowFilter;
				filter2.IsActive = true;
				filter2.Property = tagGroupPK;
				filter2.ComparisonOperator = "not applied";

				var actual = filterStrip.GetActiveFiltersQueries().Select(m => m.ToUrlComponent());
				AssertContainsExactElementsInAnyOrder(new string[] {
					$"(((TAG eq {tagPK}) or ((JOBTAG eq {tagPK}) and (TAGGROUP ne {Guid.Empty})))" +
					$" and " +
					$"(not((TAGGROUP eq {tagGroupPK}) or (JOBTAGGROUP eq {tagGroupPK}))))" }, actual);
			}
		}

		#endregion

		#region Implementation

		class DiscriminatingDummy : DummyWithWorkflow
		{
			public DiscriminatingDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProcessHeaderFilterBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
