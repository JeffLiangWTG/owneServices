using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(TaskAssignedFilter))]
	public class TaskAssignedFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTaskAssignedFilter_Performance()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var taskAssignedFilter = (TaskAssignedFilter)filterBizo[TaskAssignedFilter.Schema.Identifier];
			taskAssignedFilter.IsActive = true;
			taskAssignedFilter.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilter.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Either;

			var sql = filterBizo.Filter.LiteralTextSqlFormatted;

			CombineAssertions("WHEN executing TaskAssignFilter, QUERY should not replace'NOT IN' and 'UNION ALL' to 'NOT EXISTS' because more efficient", () =>
			{
				AssertContains("still contains 'NOT IN' for P9_TYPE NOT IN MIL/TRG/EXC", "not in", sql, ignoreCase: true);
				AssertNotContains("not contains 'UNION ALL", "union all", sql, ignoreCase: true);
				AssertContains("contain 'NOT EXISTS'", "not exists", sql, ignoreCase: true);
			});
		}

		#region Query Implementations

		Dictionary<ZString, ProcessHeader> TestEnvSetup(bool setupClosedOrCancelledTasks = false)
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "TST";

			var staffCode = GlbStaff.CurrentUser.GS_Code;

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader3");
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader4");
			var jobHeader5 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader5");
			var jobHeader6 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader6");
			var jobHeader7 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader7");
			var jobHeader8 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader8");
			var jobHeader9 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader9");

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "AssignedToBoth");
			BMSTestHelper.CreateTask(workflow1, staffCode, capability: capability);
			BMSTestHelper.CreateTask(workflow1, staffCode, capability: capability);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "AssignedToResource");
			BMSTestHelper.CreateTask(workflow2, staffCode);
			BMSTestHelper.CreateTask(workflow2, staffCode);

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "AssignedToCapability");
			BMSTestHelper.CreateTask(workflow3, capability: capability);
			BMSTestHelper.CreateTask(workflow3, capability: capability);

			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader4, "AssignedToResourceOrCapability");
			BMSTestHelper.CreateTask(workflow4, staffCode);
			BMSTestHelper.CreateTask(workflow4, capability: capability);

			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader5, "AssignedToBothOrNone");
			BMSTestHelper.CreateTask(workflow5, staffCode, capability: capability);
			BMSTestHelper.CreateTask(workflow5);

			var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader6, "AssignedToResourceOrNone");
			BMSTestHelper.CreateTask(workflow6, staffCode);
			BMSTestHelper.CreateTask(workflow6);

			var workflow7 = BMSTestHelper.CreateWorkflow(jobHeader7, "AssignedToCapabilityOrNone");
			BMSTestHelper.CreateTask(workflow7, capability: capability);
			BMSTestHelper.CreateTask(workflow7);

			var workflow8 = BMSTestHelper.CreateWorkflow(jobHeader8, "AssignedToNone");
			BMSTestHelper.CreateTask(workflow8);
			BMSTestHelper.CreateTask(workflow8);

			var workflow9 = BMSTestHelper.CreateWorkflow(jobHeader9, "EmptyWorkflow");

			var jobHeaders = new[] { jobHeader1, jobHeader2, jobHeader3, jobHeader4, jobHeader5, jobHeader6, jobHeader7, jobHeader8, jobHeader9 };

			if (setupClosedOrCancelledTasks)
			{
				int alternate = 0;
				jobHeaders
					.SelectMany(j => j.Tasks)
					.ForEach(t => t.P9_Status = alternate++ % 2 == 1
						? ProcessTaskStatusCodeList.Codes.Cancelled
						: ProcessTaskStatusCodeList.Codes.Closed);
			}

			Factory.Save();

			return jobHeaders.SelectMany(j => new[] { j }.Concat(j.ProcessHeaders))
				.ToDictionary(t => t.FH_CompletionStatement);
		}

		void AssertHasWorkflows(string taskOrdinality, string taskAssignmentType, params string[] workflowNames)
		{
			AssertHasWorkflows(taskOrdinality, taskAssignmentType, setupClosedAndCancelledTasks: false, workflowNames: workflowNames);
		}

		void AssertHasWorkflows(string taskOrdinality, string taskAssignmentType, bool setupClosedAndCancelledTasks, params string[] workflowNames)
		{
			var jobsAndWorkflows = TestEnvSetup(setupClosedAndCancelledTasks);
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var taskAssignedFilter = (TaskAssignedFilter)filterBizo[TaskAssignedFilter.Schema.Identifier];
			taskAssignedFilter.IsActive = true;

			taskAssignedFilter.TaskOrdinality = taskOrdinality;
			taskAssignedFilter.TaskAssignmentType = taskAssignmentType;

			var result = Factory.Load<ProcessHeader>(filterBizo.Filter);

			AssertContainsExactElementsInAnyOrder(workflowNames.SelectMany(n => new[] { jobsAndWorkflows[n], jobsAndWorkflows[n].JobHeader }), result);
		}

		#region ALL

		public void TestQuery_ALL_EIT_ClosedAndCancelled()
		{
			// GIVEN all tasks are closed/cancelled WHEN finding records with ALL & EIT THEN it should find all
			//	BECAUSE the exclusion filters out closed/cancelled tasks - regardless of staff/capability is empty.
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.All, TaskAssignmentTypeFilterList.Codes.Either, true, "AssignedToBoth", "AssignedToBothOrNone",
				"AssignedToResourceOrNone", "AssignedToResource", "AssignedToCapabilityOrNone", "AssignedToCapability", "AssignedToResourceOrCapability", "EmptyWorkflow",
				"AssignedToNone");
		}

		public void TestQuery_ALL_EIT()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.All, TaskAssignmentTypeFilterList.Codes.Either, "AssignedToBoth", "AssignedToResource", "AssignedToCapability", "AssignedToResourceOrCapability", "EmptyWorkflow");
		}

		public void TestQuery_ALL_BOT()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.All, TaskAssignmentTypeFilterList.Codes.Both, "AssignedToBoth", "EmptyWorkflow");
		}

		public void TestQuery_ALL_RES()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.All, TaskAssignmentTypeFilterList.Codes.Resources, "AssignedToBoth", "AssignedToResource", "EmptyWorkflow");
		}

		public void TestQuery_ALL_CAP()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.All, TaskAssignmentTypeFilterList.Codes.Capabilities, "AssignedToBoth", "AssignedToCapability", "EmptyWorkflow");
		}

		public void TestQuery_ALL_UNA()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.All, TaskAssignmentTypeFilterList.Codes.Unassigned, "EmptyWorkflow", "AssignedToNone");
		}

		#endregion

		#region ANY

		public void TestQuery_ANY_EIT()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.Any, TaskAssignmentTypeFilterList.Codes.Either, "AssignedToBoth", "AssignedToResource", "AssignedToCapability", "AssignedToResourceOrCapability", "AssignedToBothOrNone", "AssignedToResourceOrNone", "AssignedToCapabilityOrNone");
		}

		public void TestQuery_ANY_BOT()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.Any, TaskAssignmentTypeFilterList.Codes.Both, "AssignedToBoth", "AssignedToBothOrNone");
		}

		public void TestQuery_ANY_RES()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.Any, TaskAssignmentTypeFilterList.Codes.Resources, "AssignedToBoth", "AssignedToResource", "AssignedToResourceOrCapability", "AssignedToBothOrNone", "AssignedToResourceOrNone");
		}

		public void TestQuery_ANY_CAP()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.Any, TaskAssignmentTypeFilterList.Codes.Capabilities, "AssignedToBoth", "AssignedToCapability", "AssignedToResourceOrCapability", "AssignedToBothOrNone", "AssignedToCapabilityOrNone");
		}

		public void TestQuery_ANY_UNA()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.Any, TaskAssignmentTypeFilterList.Codes.Unassigned, "AssignedToBothOrNone", "AssignedToResourceOrNone", "AssignedToCapabilityOrNone", "AssignedToNone");
		}

		#endregion

		#region NON

		public void TestQuery_NON_EIT()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.None, TaskAssignmentTypeFilterList.Codes.Either, "AssignedToNone", "EmptyWorkflow");
		}

		public void TestQuery_NON_BOT()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.None, TaskAssignmentTypeFilterList.Codes.Both, "AssignedToResource", "AssignedToCapability", "AssignedToResourceOrCapability", "AssignedToResourceOrNone", "AssignedToCapabilityOrNone", "AssignedToNone", "EmptyWorkflow");
		}

		public void TestQuery_NON_RES()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.None, TaskAssignmentTypeFilterList.Codes.Resources, "AssignedToCapability", "AssignedToCapabilityOrNone", "AssignedToNone", "EmptyWorkflow");
		}

		public void TestQuery_NON_CAP()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.None, TaskAssignmentTypeFilterList.Codes.Capabilities, "AssignedToResource", "AssignedToResourceOrNone", "AssignedToNone", "EmptyWorkflow");
		}

		public void TestQuery_NON_UNA()
		{
			AssertHasWorkflows(TaskAssignedOrdinalityFilterList.Codes.None, TaskAssignmentTypeFilterList.Codes.Unassigned, "AssignedToBoth", "AssignedToResource", "AssignedToCapability", "AssignedToResourceOrCapability", "EmptyWorkflow");
		}

		#endregion

		#region Empty

		public void TestEmpty()
		{
			AssertHasWorkflows(string.Empty, string.Empty, "AssignedToBoth", "AssignedToResource", "AssignedToCapability", "AssignedToResourceOrCapability", "AssignedToBothOrNone", "AssignedToResourceOrNone", "AssignedToCapabilityOrNone", "AssignedToNone", "EmptyWorkflow");
		}
		#endregion

		#endregion

		#region Serialisation

		public void TestSerialisation()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var serialisedFilter = (TaskAssignedFilter)filterBizo[TaskAssignedFilter.Schema.Identifier];
			serialisedFilter.IsActive = true;

			serialisedFilter.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			serialisedFilter.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;

			var deserialisedFilter = new TaskAssignedFilter(Factory);

			BMSTestHelper.SerialiseAndDeSerialise(serialisedFilter, deserialisedFilter);

			AssertEquals(TaskAssignedOrdinalityFilterList.Codes.All, deserialisedFilter.TaskOrdinality);
			AssertEquals(TaskAssignmentTypeFilterList.Codes.Both, deserialisedFilter.TaskAssignmentType);
		}

		#endregion

		#region Validation

		public void TestValidation_Ordinality()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var taskAssignedFilter = (TaskAssignedFilter)filterBizo[TaskAssignedFilter.Schema.Identifier];
			taskAssignedFilter.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilter.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilter.IsActive = true;

			taskAssignedFilter.Validation.ValidateAll();

			AssertNoErrors(taskAssignedFilter);

			taskAssignedFilter.TaskOrdinality = "ANG";
			AssertHasError(taskAssignedFilter.TaskOrdinalityInfo, "Enter a valid selection.");

			taskAssignedFilter.TaskOrdinality = "";
			AssertHasError(taskAssignedFilter.TaskOrdinalityInfo, "Please enter a value.");
		}

		public void TestValidation_AssignmentType()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var taskAssignedFilter = (TaskAssignedFilter)filterBizo[TaskAssignedFilter.Schema.Identifier];
			taskAssignedFilter.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilter.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilter.IsActive = true;

			taskAssignedFilter.Validation.ValidateAll();

			AssertNoErrors(taskAssignedFilter);

			taskAssignedFilter.TaskAssignmentType = "ANG";
			AssertHasError(taskAssignedFilter.TaskAssignmentTypeInfo, "Enter a valid selection.");

			taskAssignedFilter.TaskAssignmentType = "";
			AssertHasError(taskAssignedFilter.TaskAssignmentTypeInfo, "Please enter a value.");
		}

		#endregion

		#region Filtration

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_Group_JobFirst()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripTSA = filterBizo.FilterStrips.AddNew();
			stripTSA.FilterDescription = TaskAssignedFilter.Schema.Identifier;
			var taskAssignedFilterA = (TaskAssignedFilter)stripTSA.CurrentModuleFilter;
			taskAssignedFilterA.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilterA.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilterA.IsActive = true;
			taskAssignedFilterA.GroupName = "A";
			taskAssignedFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetJobOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripTSB = filterBizo.FilterStrips.AddNew();
			stripTSB.FilterDescription = TaskAssignedFilter.Schema.Identifier;
			var taskAssignedFilterB = (TaskAssignedFilter)stripTSB.CurrentModuleFilter;
			taskAssignedFilterB.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilterB.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilterB.IsActive = true;
			taskAssignedFilterB.GroupName = "B";
			taskAssignedFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;
			jobOrWorkflowB.SetWorkflowOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 4, filterBizo.FilterStrips.Count);
			AssertEquals("active", 4, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(1, filterBizo.Filter.LiteralTextSqlFormatted.AllIndexesOf("UNION ALL").Count());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_Group_WorkflowFirst()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripTSA = filterBizo.FilterStrips.AddNew();
			stripTSA.FilterDescription = TaskAssignedFilter.Schema.Identifier;
			var taskAssignedFilterA = (TaskAssignedFilter)stripTSA.CurrentModuleFilter;
			taskAssignedFilterA.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilterA.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilterA.IsActive = true;
			taskAssignedFilterA.GroupName = "A";
			taskAssignedFilterA.GroupOrCategory = FilterOrCategory.Red;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.IsActive = true;

			var stripTSB = filterBizo.FilterStrips.AddNew();
			stripTSB.FilterDescription = TaskAssignedFilter.Schema.Identifier;
			var taskAssignedFilterB = (TaskAssignedFilter)stripTSB.CurrentModuleFilter;
			taskAssignedFilterB.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilterB.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilterB.IsActive = true;
			taskAssignedFilterB.GroupName = "B";
			taskAssignedFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;
			jobOrWorkflowB.SetJobOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 4, filterBizo.FilterStrips.Count);
			AssertEquals("active", 4, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(1, filterBizo.Filter.LiteralTextSqlFormatted.AllIndexesOf("UNION ALL").Count());
		}

		public void TestFilterIsUnionedCorrectly_WhenJobOrWorkflowFilterIsApplied_OrCategory()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();

			var stripTSA = filterBizo.FilterStrips.AddNew();
			stripTSA.FilterDescription = TaskAssignedFilter.Schema.Identifier;
			var taskAssignedFilterA = (TaskAssignedFilter)stripTSA.CurrentModuleFilter;
			taskAssignedFilterA.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilterA.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilterA.IsActive = true;
			taskAssignedFilterA.GroupName = "A";
			taskAssignedFilterA.GroupOrCategory = FilterOrCategory.Red;
			taskAssignedFilterA.OrCategory = FilterOrCategory.Yellow;

			var stripJWA = filterBizo.FilterStrips.AddNew();
			stripJWA.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowA = (JobOrWorkflowFilter)stripJWA.CurrentModuleFilter;
			jobOrWorkflowA.SetWorkflowOnly();
			jobOrWorkflowA.GroupName = "A";
			jobOrWorkflowA.GroupOrCategory = FilterOrCategory.Red;
			jobOrWorkflowA.OrCategory = FilterOrCategory.Yellow;
			jobOrWorkflowA.IsActive = true;

			var stripTSAB = filterBizo.FilterStrips.AddNew();
			stripTSAB.FilterDescription = TaskAssignedFilter.Schema.Identifier;
			var taskAssignedFilterAB = (TaskAssignedFilter)stripTSAB.CurrentModuleFilter;
			taskAssignedFilterAB.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilterAB.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilterAB.IsActive = true;
			taskAssignedFilterAB.GroupName = "A";
			taskAssignedFilterAB.GroupOrCategory = FilterOrCategory.Red;
			taskAssignedFilterAB.OrCategory = FilterOrCategory.Blue;

			var stripTSB = filterBizo.FilterStrips.AddNew();
			stripTSB.FilterDescription = TaskAssignedFilter.Schema.Identifier;
			var taskAssignedFilterB = (TaskAssignedFilter)stripTSB.CurrentModuleFilter;
			taskAssignedFilterB.TaskOrdinality = TaskAssignedOrdinalityFilterList.Codes.All;
			taskAssignedFilterB.TaskAssignmentType = TaskAssignmentTypeFilterList.Codes.Both;
			taskAssignedFilterB.IsActive = true;
			taskAssignedFilterB.GroupName = "B";
			taskAssignedFilterB.GroupOrCategory = FilterOrCategory.Green;

			var stripJWB = filterBizo.FilterStrips.AddNew();
			stripJWB.FilterDescription = ProcessHeader.ModuleFilterConstants.JobOrWorkflow;
			var jobOrWorkflowB = (JobOrWorkflowFilter)stripJWB.CurrentModuleFilter;
			jobOrWorkflowB.IsActive = true;
			jobOrWorkflowB.SetJobOnly();
			jobOrWorkflowB.GroupName = "B";
			jobOrWorkflowB.GroupOrCategory = FilterOrCategory.Green;

			AssertEquals("strips", 5, filterBizo.FilterStrips.Count);
			AssertEquals("active", 5, filterBizo.ActiveModuleFilters.Count);
			AssertEquals(3, filterBizo.Filter.LiteralTextSqlFormatted.AllIndexesOf("UNION ALL").Count());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TaskAssignedFilter(Factory);
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
