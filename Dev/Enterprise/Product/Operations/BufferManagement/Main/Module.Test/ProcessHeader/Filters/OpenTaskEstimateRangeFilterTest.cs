using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.GUI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(OpenTaskEstimateRangeFilter))]
	public class OpenTaskEstimateRangeFilterTest : NonPersistentBusinessObjectTestCase
	{
		#region Query Implementations

		ProcessJobHeader orgJobHeader;
		ProcessJobHeader inqJobHeader;
		ProcessHeader workflow1;
		ProcessHeader workflow1Child;
		ProcessHeader workflow2;
		ProcessHeader workflow3;

		void SetUpWorkflowsForQueryTests()
		{
			orgJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "ORG JobHeader");
			inqJobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, false, "INQ JobHeader");

			workflow1 = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with task 1.5h");
			workflow1Child = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with parent workflow and task 3h");
			workflow2 = BMSTestHelper.CreateWorkflow(inqJobHeader, "INQ workflow with tasks 3h and 6h");
			workflow3 = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with tasks 6h and 9h");

			workflow1Child.GetOrCreateLinkToParent(workflow1);

			// min est = 1; variation factor = 2; std est = 1.5
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			task1.P9_EstDuration = new ZDateTime(2015, 1, 1, 1, 0, 0);
			task1.P9_EstimateVariationFactor = 2;

			// min est = 2; variation factor = 2; std est = 3
			var task1_child = BMSTestHelper.CreateTask(workflow1Child, GlbStaff.CurrentUser.GS_Code);
			task1_child.P9_EstDuration = new ZDateTime(2015, 1, 1, 2, 0, 0);
			task1_child.P9_EstimateVariationFactor = 2;

			// min est = 2; variation factor = 2; std est = 3
			var task2_1 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);
			task2_1.P9_EstDuration = new ZDateTime(2015, 1, 1, 2, 0, 0);
			task2_1.P9_EstimateVariationFactor = 2;

			// min est = 4; variation factor = 2; std est = 6
			var task2_2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);
			task2_2.P9_EstDuration = new ZDateTime(2015, 1, 1, 4, 0, 0);
			task2_2.P9_EstimateVariationFactor = 2;

			// min est = 4; variation factor = 2; std est = 6
			var task3_1 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code);
			task3_1.P9_EstDuration = new ZDateTime(2015, 1, 1, 4, 0, 0);
			task3_1.P9_EstimateVariationFactor = 2;

			// min est = 6; variation factor = 2; std est = 9
			var task3_2 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code);
			task3_2.P9_EstDuration = new ZDateTime(2015, 1, 1, 6, 0, 0);
			task3_2.P9_EstimateVariationFactor = 2;

			Factory.Save();
		}

		void SetUpRegistryForQueryTests()
		{
			AddRegistryItem("ORG", "TRA", isExcludedFromTransferRules: true, isCompletionStatementTaskType: false);
			AddRegistryItem("INQ", "COM", isExcludedFromTransferRules: false, isCompletionStatementTaskType: true);
		}

		void AddRegistryItem(string workflowType, string taskType, bool isExcludedFromTransferRules, bool isCompletionStatementTaskType)
		{
			var registryItem = WorkflowDataRegistry.Instance.TaskTypes.Value;

			var type = registryItem.GetTaskTypesFromWorkflowCode(workflowType).AddNew();
			type.Code = taskType;
			type.IsExcludedFromTransferRules = isExcludedFromTransferRules;
			type.IsCompletionStatementTaskType = isCompletionStatementTaskType;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryItem);
		}

		#region Inside

		public void TestInsideRange_ParentChildShouldBeConsideredSeparately()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			filter.SetTimeValuesFromHoursForTest(3, 3);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Only the child workflow has tasks inside the range, so it should match and not its parent workflow, and yet...", new[] { workflow1Child }, result);

			filter.SetTimeValuesFromHoursForTest(1, 2);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Only the parent workflow has tasks inside the range, so it should match and not its child workflow, and yet...", new[] { workflow1 }, result);

			filter.SetTimeValuesFromHoursForTest(1, 3);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Both the parent and child workflows have tasks inside the range, both should be matched, and yet...", new[] { workflow1, workflow1Child }, result);
		}

		public void TestInsideRange_AllTasksMustBeInsideRange()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			filter.SetTimeValuesFromHoursForTest(6, 9);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All tasks were within the specified range, so workflow3 should have been matched, and yet...", new[] { workflow3 }, result);

			filter.SetTimeValuesFromHoursForTest(5, 7);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All workflows have tasks outside of the specified range (even though one task does fall within the range), so the result shou be empty and yet...", Array.Empty<ProcessHeader>(), result);

			filter.SetTimeValuesFromHoursForTest(8, 10);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All workflows have tasks outside of the specified range (even though one task does fall within the range), so the result shou be empty and yet...", Array.Empty<ProcessHeader>(), result);
		}

		public void TestInsideRange_TasksExceeding24HoursOfStandardEstimateMustBeConsidered()
		{
			SetUpWorkflowsForQueryTests();

			var workflow23h = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with task 23h");
			var workflow24h = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with task 24h");
			var workflow25h = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with task 25h");

			var task23h = BMSTestHelper.CreateTask(workflow23h, GlbStaff.CurrentUser.GS_Code);
			task23h.P9_EstDuration = new ZDateTime(2015, 1, 1, 23, 0, 0);
			task23h.P9_EstimateVariationFactor = 1;

			var task24h = BMSTestHelper.CreateTask(workflow24h, GlbStaff.CurrentUser.GS_Code);
			task24h.P9_EstDuration = new ZDateTime(2015, 1, 2, 0, 0, 0);
			task24h.P9_EstimateVariationFactor = 1;

			var task25h = BMSTestHelper.CreateTask(workflow25h, GlbStaff.CurrentUser.GS_Code);
			task25h.P9_EstDuration = new ZDateTime(2015, 1, 2, 1, 0, 0);
			task25h.P9_EstimateVariationFactor = 1;

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(20, 23);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workflow23h }, result);

			filter.SetTimeValuesFromHoursForTest(20, 24);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workflow23h, workflow24h }, result);

			filter.SetTimeValuesFromHoursForTest(24, 25);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workflow24h, workflow25h }, result);
		}

		public void TestInsideRange_EmptyWorkflow_ShouldMatch()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x => x.P9_Status = "CLS");
			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(100, 200);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow3 has no open tasks, so it should be included in the result regardless of the task estimates specified, and yet...", new[] { workflow3 }, result);

			workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x => x.P9_Status = "CAN");
			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(100, 200);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow3 has no open tasks, so it should be included in the result regardless of the task estimates specified, and yet...", new[] { workflow3 }, result);

			var tasks = workflow3.TaskCollection.Cast<ProcessTask>().ToArray();
			tasks.ForEach(x => x.Delete());
			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(100, 200);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow3 has no open tasks, so it should be included in the result regardless of the task estimates specified, and yet...", new[] { workflow3 }, result);
		}

		public void TestInsideRange_NullEstimate_ShouldBeConsideredZero()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x => x.P9_Status = "CLS");

			var task = Factory.New<ProcessTask>();
			task.P9_FH_ProcessHeader = workflow3.PK;
			task.P9_ParentID = workflow3.FH_ParentId;
			task.P9_ParentTableCode = workflow3.FH_ParentTableCode;
			task.P9_EstimateVariationFactor = 2;
			Factory.Save();

			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM dbo.ProcessTasks WHERE P9_PK = '{0}' AND P9_EstDuration IS NULL", task.PK)))
			{
				AssertEquals("P9_EstDuration should be null for this test, which will be the case when no estimate has been entered yet, and yet...", 1, command.ExecuteScalar());
			}

			filter.SetTimeValuesFromHoursForTest(6, 9);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder("A task with a null estimate was added to workflow3, which should be considered 0, thus outside the specified range, and should exclude the workflow from selection, and yet...", Array.Empty<ProcessHeader>(), result);

			filter.SetTimeValuesFromHoursForTest(0, 0);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder("A task with a null estimate was added to workflow3, which should be considered 0, thus inside the specified range, so that workflow should be included, and yet...", new[] { workflow3 }, result);
		}

		public void TestInsideRange_ShouldOnlySelectJobIfAllWorkflowsMatch()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			filter.SetTimeValuesFromHoursForTest(1.5m, 9);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All workflows have only tasks within the specified estimates (inclusive), so all workflows with tasks and job headers should have been selected, and yet...",
				new[] { orgJobHeader, inqJobHeader, workflow1, workflow1Child, workflow2, workflow3 }, result);

			filter.SetTimeValuesFromHoursForTest(3, 9);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow1 doesn't match, so orgJobHeader shouldn't match either, and yet...", new[] { inqJobHeader, workflow1Child, workflow2, workflow3 }, result);

			filter.SetTimeValuesFromHoursForTest(1, 8);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow3 doesn't match, so orgJobHeader shouldn't match either, and yet...", new[] { inqJobHeader, workflow1, workflow1Child, workflow2 }, result);
		}

		public void TestInsideRange_WithExcludedTaskTypes()
		{
			SetUpWorkflowsForQueryTests();
			SetUpRegistryForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			var transferTask = BMSTestHelper.CreateTask(workflow1Child, GlbStaff.CurrentUser.GS_Code, taskType: "TRA");
			transferTask.P9_EstDuration = new ZDateTime(2015, 1, 1, 10, 0, 0); // Std Estimate 15
			transferTask.P9_EstimateVariationFactor = 2;
			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(3, 6);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("A task outside the specified range was added, but it is an excluded type (excluded from transfer rules), so its workflow should still have been selected. And yet...",
				new[] { inqJobHeader, workflow1Child, workflow2 }, result);

			var comTask = BMSTestHelper.CreateTask(workflow1Child, GlbStaff.CurrentUser.GS_Code, taskType: "COM");
			comTask.P9_EstDuration = new ZDateTime(2015, 1, 1, 6, 0, 0); // Std Estimate 9
			comTask.P9_EstimateVariationFactor = 2;
			Factory.Save();

			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("A task outside the specified range was added, and though it was an excluded type (COM) it wasn't an excluded type for the workflow type (OrgHeader), so workflow1Child shouldn't have been selected. And yet...",
				new[] { inqJobHeader, workflow2 }, result);

			comTask.P9_FH_ProcessHeader = workflow2.PK;
			comTask.P9_ParentTableCode = "O1";
			ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(Factory); // this test involves a crazy thing like moving a task between jobs, so let's suppress updating statuses on save to make it work
			Factory.Save();

			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The task outside the specified range was moved to a workflow which should consider it an excluded type, so the workflow should still have been selected. And yet...",
				new[] { inqJobHeader, workflow1Child, workflow2 }, result);

			transferTask.P9_FH_ProcessHeader = workflow2.PK;
			transferTask.P9_ParentTableCode = "O1";
			ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(Factory); // this test involves a crazy thing like moving a task between jobs, so let's suppress updating statuses on save to make it work
			Factory.Save();

			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The task outside the specified range was moved to a workflow which shouldn't consider it an excluded type, and the task is out of range, so the workflow should be excluded. And yet...",
				new[] { workflow1Child }, result);
		}

		#endregion

		#region Outside

		public void TestOutsideRange_ParentChildShouldBeConsideredSeparately()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.OutsideRange;

			filter.SetTimeValuesFromHoursForTest(3, 9);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Only workflow1 has tasks outside the range, so it should match and not its child workflow, and yet...", new[] { orgJobHeader, workflow1 }, result);

			workflow1.TaskCollection[0].P9_FH_ProcessHeader = workflow1Child.PK;
			Factory.Save();

			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Only the child workflow has tasks outside the range, so it should match and not its parent workflow, and yet...", new[] { orgJobHeader, workflow1Child }, result);
		}

		public void TestOutsideRange_ShouldMatchIfAnyTasksOutsideOfRange()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.OutsideRange;

			filter.SetTimeValuesFromHoursForTest(1.5m, 9);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All tasks are inside the specified range (inclusively) so no workflows should have matched, and yet...", Array.Empty<ProcessHeader>(), result);

			filter.SetTimeValuesFromHoursForTest(1.5m, 8);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("One task is outisde the range specified so it's workflow and job header should have been selected, and yet...", new[] { orgJobHeader, workflow3 }, result);
		}

		public void TestOutsideRange_TasksExceeding24HoursOfStandardEstimateMustBeConsidered()
		{
			SetUpWorkflowsForQueryTests();

			var workflow23h = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with task 23h");
			var workflow24h = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with task 24h");
			var workflow25h = BMSTestHelper.CreateWorkflow(orgJobHeader, "OH workflow with task 25h");

			var task23h = BMSTestHelper.CreateTask(workflow23h, GlbStaff.CurrentUser.GS_Code);
			task23h.P9_EstDuration = new ZDateTime(2015, 1, 1, 23, 0, 0);
			task23h.P9_EstimateVariationFactor = 1;

			var task24h = BMSTestHelper.CreateTask(workflow24h, GlbStaff.CurrentUser.GS_Code);
			task24h.P9_EstDuration = new ZDateTime(2015, 1, 2, 0, 0, 0);
			task24h.P9_EstimateVariationFactor = 1;

			var task25h = BMSTestHelper.CreateTask(workflow25h, GlbStaff.CurrentUser.GS_Code);
			task25h.P9_EstDuration = new ZDateTime(2015, 1, 2, 1, 0, 0);
			task25h.P9_EstimateVariationFactor = 1;

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.OutsideRange;

			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(0, 24);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { orgJobHeader, workflow25h }, result);

			filter.SetTimeValuesFromHoursForTest(0, 23);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { orgJobHeader, workflow24h, workflow25h }, result);

			filter.SetTimeValuesFromHoursForTest(0, 22);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { orgJobHeader, workflow23h, workflow24h, workflow25h }, result);
		}

		public void TestOutsideRange_EmptyWorkflow_ShouldNotMatch()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.OutsideRange;

			workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x => x.P9_Status = "CLS");
			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(1, 8);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow3 has no open tasks, so it shouldn't be included in the result, and yet...", Array.Empty<ProcessHeader>(), result);

			workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x => x.P9_Status = "CAN");
			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(1, 8);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow3 has no open tasks, so it shouldn't be included in the result, and yet...", Array.Empty<ProcessHeader>(), result);

			workflow3.TaskCollection.Cast<ProcessTask>().ToArray().ForEach(x => x.Delete());
			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(1, 8);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("workflow3 has no open tasks, so it shouldn't be included in the result, and yet...", Array.Empty<ProcessHeader>(), result);
		}

		public void TestOutsideRange_NullEstimate_ShouldBeConsideredZero()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.OutsideRange;

			workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x => x.P9_Status = "CLS");

			var task = Factory.New<ProcessTask>();
			task.P9_FH_ProcessHeader = workflow3.PK;
			task.P9_ParentID = workflow3.FH_ParentId;
			task.P9_ParentTableCode = workflow3.FH_ParentTableCode;
			task.P9_EstimateVariationFactor = 2;
			Factory.Save();

			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM dbo.ProcessTasks WHERE P9_PK = '{0}' AND P9_EstDuration IS NULL", task.PK)))
			{
				AssertEquals("P9_EstDuration should be null for this test, which will be the case when no estimate has been entered yet, and yet...", 1, command.ExecuteScalar());
			}

			filter.SetTimeValuesFromHoursForTest(1, 9);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder("A task with a null estimate was added to workflow3, which should be considered 0, thus outside the specified range, so that workflow should be included, and yet...", new[] { orgJobHeader, workflow3 }, result);

			filter.SetTimeValuesFromHoursForTest(0, 9);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder("A task with a null estimate was added to workflow3, which should be considered 0, thus inside the specified range, and should exclude the workflow from selection, and yet...", Array.Empty<ProcessHeader>(), result);
		}

		public void TestOutsideRange_ShouldSelectJobIfAnyWorkflowsMatch()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.OutsideRange;

			filter.SetTimeValuesFromHoursForTest(1, 1);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All workflows with tasks have tasks outside the specified estimates (exclusive), so all workflows with tasks and job headers should have been selected, and yet...",
				new[] { orgJobHeader, inqJobHeader, workflow1, workflow1Child, workflow2, workflow3 }, result);

			filter.SetTimeValuesFromHoursForTest(3, 9);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("orgJobHeader should match because one of its workflows matches, even though not all of its workflow match, and yet...", new[] { orgJobHeader, workflow1 }, result);

			filter.SetTimeValuesFromHoursForTest(1, 8);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("orgJobHeader should match because one of its workflows matches, even though not all of its workflow match, and yet...", new[] { orgJobHeader, workflow3 }, result);
		}

		public void TestOutsideRange_WithExludedTaskTypes()
		{
			SetUpWorkflowsForQueryTests();
			SetUpRegistryForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.OutsideRange;

			var transferTask = BMSTestHelper.CreateTask(workflow1Child, GlbStaff.CurrentUser.GS_Code, taskType: "TRA");
			transferTask.P9_EstDuration = new ZDateTime(2015, 1, 1, 10, 0, 0); // Std Estimate 15
			transferTask.P9_EstimateVariationFactor = 2;
			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(1, 9);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("A task outside the specified range was added, but it is an excluded type (excluded from transfer rules), so its workflow should still not have been selected. And yet...", Array.Empty<ProcessHeader>(), result);

			var comTask = BMSTestHelper.CreateTask(workflow1Child, GlbStaff.CurrentUser.GS_Code, taskType: "COM");
			comTask.P9_EstDuration = new ZDateTime(2015, 1, 1, 10, 0, 0); // Std Estimate 15
			comTask.P9_EstimateVariationFactor = 2;
			Factory.Save();

			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("A task outside the specified range was added, and though it was an excluded type (COM) it wasn't an excluded type for the workflow type (OrgHeader), so that workflow should be selected. And yet...",
				new[] { orgJobHeader, workflow1Child }, result);

			comTask.P9_FH_ProcessHeader = workflow2.PK;
			comTask.P9_ParentTableCode = "O1";
			ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(Factory); // this test involves a crazy thing like moving a task between jobs, so let's suppress updating statuses on save to make it work
			Factory.Save();

			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The task outside the specified range was moved to a workflow which should consider it an excluded type, so the workflow should still not have been selected. And yet...", Array.Empty<ProcessHeader>(), result);

			transferTask.P9_FH_ProcessHeader = workflow2.PK;
			transferTask.P9_ParentTableCode = "O1";
			ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(Factory); // this test involves a crazy thing like moving a task between jobs, so let's suppress updating statuses on save to make it work
			Factory.Save();

			result = Factory.Load<ProcessHeader>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("The task outside the specified range was moved to a workflow which shouldn't consider it an excluded type, and the task is out of range, so the workflow should be selected. And yet...",
				new[] { inqJobHeader, workflow2 }, result);
		}

		public void TestGetTaskTypesToExludeXml_InvalidXmlCharacters_ShouldEscapeValues()
		{
			SetUpWorkflowsForQueryTests();

			AddRegistryItem("OH", "T<>", isExcludedFromTransferRules: true, isCompletionStatementTaskType: false);
			AddRegistryItem("IN'", "C'&", isExcludedFromTransferRules: false, isCompletionStatementTaskType: true);
			AddRegistryItem("OH", "TRA", isExcludedFromTransferRules: false, isCompletionStatementTaskType: false);

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.Scope = ScopeRangeList.Codes.InsideRange;
			filter.SetTimeValuesFromHoursForTest(1, 9);
			var expected = @"'<row><WorkflowType>OH</WorkflowType><TaskType>T&lt;&gt;</TaskType></row><row><WorkflowType>IN''</WorkflowType><TaskType>C''&amp;</TaskType></row>'";
			AssertEquals("The query should include xml for the two tasks that qualify for exclusion, and values should be properly xml-escaped, and yet...", true, filter.Query.LiteralTextSqlFormatted.Contains(expected));

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder("The filter should have included everything, and the invalid xml characters in the task types should have not caused the query to crash, and yet...",
				new[] { orgJobHeader, inqJobHeader, workflow1, workflow1Child, workflow2, workflow3 }, result);
		}

		public void TestGetTaskTypesToExludeXml_NoExludedTasks_ShouldStillWork()
		{
			SetUpWorkflowsForQueryTests();

			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.Scope = ScopeRangeList.Codes.InsideRange;
			filter.SetTimeValuesFromHoursForTest(1, 9);

			AssertEquals("The query should not include any xml because there are no relevant task types, and yet...", false, filter.Query.LiteralTextSqlFormatted.Contains("<"));

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder("The filter should have included everything, and not having any task types to exclude should have not caused the query to crash, and yet...",
				new[] { orgJobHeader, inqJobHeader, workflow1, workflow1Child, workflow2, workflow3 }, result);
		}

		#endregion

		public void TestDecimalLimits_ShouldNotRoundEstimates()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x =>
			{
				x.P9_EstDuration = new ZDateTime(2015, 1, 1, 0, 1, 0);
				x.P9_EstimateVariationFactor = 1;
			});

			Factory.Save();

			filter.SetTimeValuesFromHoursForTest(0, 0);
			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder("The filter should not round task estimates down to zero when their value is one minute, and yet...", Array.Empty<ProcessHeader>(), result);

			filter.MinStdEstimate = filter.MinStdEstimate.AddMinutes(1);
			filter.MaxStdEstimate = filter.MaxStdEstimate.AddMinutes(1);
			result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder("The filter should not round its arguments down to 0.01, and yet... Query: " + filter.Query.LiteralTextSqlFormatted, new[] { workflow3 }, result);
		}

		public void TestAllMinuteValues_ShouldNotBeRoundedIncorrectly()
		{
			SetUpWorkflowsForQueryTests();
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var filter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			filter.IsActive = true;
			filter.Scope = ScopeRangeList.Codes.InsideRange;

			workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x => x.P9_EstimateVariationFactor = 1);
			var expected = new[] { workflow3 };

			for (var i = 0; i < 60; i++)
			{
				var minute = i;
				workflow3.TaskCollection.Cast<ProcessTask>().ForEach(x => x.P9_EstDuration = new ZDateTime(2015, 1, 1, 0, minute, 0));
				Factory.Save();

				if (i > 0)
				{
					filter.MinStdEstimate = filter.MinStdEstimate.AddMinutes(1);
					filter.MaxStdEstimate = filter.MaxStdEstimate.AddMinutes(1);
				}

				var result = Factory.Load<ProcessHeader>(filterBizo.Filter);

				AssertContainsExactElementsInAnyOrder("Any rounding done by the query should still match the correct tasks, and yet...", expected, result);
			}
		}

		public void TestConversionFromTimeToDecimalAndBack_ShouldKeepOriginalPrecision()
		{
			var filter = new OpenTaskEstimateRangeFilter(Factory);
			var baseTime = filter.MinStdEstimate;

			var testTime = baseTime.AddHours(999).AddMinutes(59);
			filter.MinStdEstimate = testTime.AddHours(1);
			filter.MaxStdEstimate = testTime.AddMonths(1);

			AssertEquals("A value was set above the limit of 999 hours and 59 minutes, so it should have been reduced to that value, and yet...", testTime, filter.MinStdEstimate);
			AssertEquals("A value was set above the limit of 999 hours and 59 minutes, so it should have been reduced to that value, and yet...", testTime, filter.MaxStdEstimate);

			for (var i = 0; i < 60; i++)
			{
				testTime = baseTime.AddMinutes(i);
				filter.MinStdEstimate = testTime;
				filter.MaxStdEstimate = testTime;

				AssertEquals(testTime, filter.MinStdEstimate);
				AssertEquals(testTime, filter.MaxStdEstimate);
			}
		}

		public void TestTaskEstimateRangeFilter_FiresHasChanges()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.System.FS_IsLive = false; // otherwise the controls will be read only.

			using (var form = new BMSystemManagementForm(config.System))
			{
				form.Show();
				Application.DoEvents();

				OpenTaskEstimateRangeFilter GetFilterBoundToCurrentControls()
				{
					var control = form.FindAll<BMFilterStripWrapperControl>().Single();
					control.FilterControlIdentifier = "Open Task Estimate Range";

					var filterStripControl = control.FindSingle<FilterRuleFilterStripControl>();
					var filterStrip = filterStripControl.FindSingle<ProcessHeaderFilterStrip>();
					filterStrip.CurrentDataItem.FilterDescription = OpenTaskEstimateRangeFilter.Schema.Identifier;

					var filterControl = filterStrip.FindSingle<OpenTaskEstimateRangeFilterControl>();
					return filterControl.BindingSource.Cast<OpenTaskEstimateRangeFilter>().Single();
				}

				var filter = GetFilterBoundToCurrentControls();
				filter.Scope = ScopeRangeList.Codes.InsideRange;
				Factory.Save();
				Application.DoEvents();

				var bottomSaveButton = form.FindAll<ZPostingButtonsUserControl>().Single().SaveButton;
				AssertEquals("Since the form contains no changes, the save button should say 'New'", "&New", bottomSaveButton.Text);

				filter = GetFilterBoundToCurrentControls();
				filter.SetTimeValuesFromHoursForTest(3, 4);
				Application.DoEvents();
				AssertEquals("Since the form contains changes, the save button should say 'Save'", "&Save", bottomSaveButton.Text);
			}
		}

		#endregion

		#region Serialisation

		[TestDate(2020, 1, 15)]
		public void TestSerialisation()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var serialisedFilter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			serialisedFilter.IsActive = true;

			serialisedFilter.Scope = ScopeRangeList.Codes.InsideRange;
			serialisedFilter.SetTimeValuesFromHoursForTest(0.1m, 3.4m);

			var deserialisedFilter = new OpenTaskEstimateRangeFilter(Factory);

			BMSTestHelper.SerialiseAndDeSerialise(serialisedFilter, deserialisedFilter);

			AssertEquals(ScopeRangeList.Codes.InsideRange, deserialisedFilter.Scope);
			AssertEquals(ZDateTime.DefaultDurationEpoch.AddMinutes(6), deserialisedFilter.MinStdEstimate);
			AssertEquals(ZDateTime.DefaultDurationEpoch.AddHours(3).AddMinutes(24), deserialisedFilter.MaxStdEstimate);
		}

		#endregion

		#region Validation

		public void TestScopeValidation()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var incompleteOpenTaskEstimateRangeFilter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			incompleteOpenTaskEstimateRangeFilter.Scope = ScopeRangeList.Codes.InsideRange;
			incompleteOpenTaskEstimateRangeFilter.IsActive = true;

			incompleteOpenTaskEstimateRangeFilter.Validation.ValidateAll();

			AssertNoErrors(incompleteOpenTaskEstimateRangeFilter);

			incompleteOpenTaskEstimateRangeFilter.Scope = "ANG";
			AssertHasError(incompleteOpenTaskEstimateRangeFilter.ScopeInfo, "Enter a valid selection.");

			incompleteOpenTaskEstimateRangeFilter.Scope = "";
			AssertHasError(incompleteOpenTaskEstimateRangeFilter.ScopeInfo, "Please enter a value.");
		}

		public void TestMinMaxValidation()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var incompleteOpenTaskEstimateRangeFilter = (OpenTaskEstimateRangeFilter)filterBizo[OpenTaskEstimateRangeFilter.Schema.Identifier];
			incompleteOpenTaskEstimateRangeFilter.Scope = ScopeRangeList.Codes.InsideRange;
			incompleteOpenTaskEstimateRangeFilter.SetTimeValuesFromHoursForTest(0, 0);
			incompleteOpenTaskEstimateRangeFilter.IsActive = true;

			incompleteOpenTaskEstimateRangeFilter.Validation.ValidateAll();

			AssertNoErrors(incompleteOpenTaskEstimateRangeFilter);

			incompleteOpenTaskEstimateRangeFilter.SetTimeValuesFromHoursForTest(10, 0);

			AssertHasError(incompleteOpenTaskEstimateRangeFilter.MinStdEstimateInfo, "The Min Std Est Hour must be less than or equal to the Max Std Est Hour.");
			AssertHasError(incompleteOpenTaskEstimateRangeFilter.MaxStdEstimateInfo, "The Max Std Est Hour must be greater than or equal to the Min Std Est Hour.");
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OpenTaskEstimateRangeFilter(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "COM", "BUN");
			Factory.Save();
		}

		#endregion
	}
}
