using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	[TestedType(typeof(IncidentClosePromptServiceTask))]
	class IncidentClosePromptServiceTaskTest : ServiceTaskTestCase<IncidentClosePromptServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);
			CloseTasksKeepingIncidentOpen(task1, task2);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(process.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => process.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestShouldCreateTaskForIncidentWithAllTasksClosed()
		{
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);
			CloseTasksKeepingIncidentOpen(task1, task2);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
		}

		public void TestShouldNotCreateTaskForGroupControlledIncidentWithAllTasksClosed()
		{
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);
			CloseTasksKeepingIncidentOpen(task1, task2);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);

			var controlStage = "ZZZ";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(controlStage, "desc", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_IncidentGroupNumber = "ING000003";
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = controlStage;
			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = Incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals("Incident should be GroupControlled", true, Incident.IsGroupControlled);
			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed).ToArray();
			AssertEquals("No Task should have been created when Incident is GroupControlled", 0, openTasks.Length);
		}

		public void TestShouldCreateTaskForIncidentWithAllTasksClosedAndRelatedWorkItemClosed()
		{
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			var relatedWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			relatedWorkItem.Cancel();
			Incident.RelatedWorkItems.Add(relatedWorkItem);
			Factory.Save();

			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);
			CloseTasksKeepingIncidentOpen(task1, task2);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed));
			AssertEquals("Precondition: Should have related WorkItem", 1, Incident.RelatedWorkItems.Count);
			AssertEquals("Precondition: WorkItem should be cancelled", true, relatedWorkItem.IsClosedOrCancelled);
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
		}

		[TestDate(2021, 10, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestShouldCreateTaskForIncidentWithEditTimeSinceLastRun()
		{
			var lastRunTime = new ZDateTime(2021, 10, 22, 9, 0, 0).ToDateTime();
			EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lastRunTime);

			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			CloseTasksKeepingIncidentOpen(task1, task2);

			TestDateAttribute.AddHours(1);
			AssertEquals("Precondition: Should be set", lastRunTime, EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertGreaterThan("Precondition: Last edit time should be more recent than the last run time", Incident.IM_SystemLastEditTimeUtc, lastRunTime);
			AssertGreaterThan("Precondition: Current time should be more recent than the last edit time", ZDateTime.UtcNow, Incident.IM_SystemLastEditTimeUtc);
			AssertEquals("Precondition: Should have no tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
		}

		[TestDate(2021, 10, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestCreatedTaskForIncidentShouldFallbackToCancelledTaskWIthHighestSequence()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task2.P9_Sequence = 2;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_G4_RequiredCapability = capability.PK;
			Factory.Save();

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var task2Reloaded = secondFactory.Load<ProcessTask>(task2.PK);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2Reloaded.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			secondFactory.Save();
			task2.Reload();

			TestDateAttribute.AddHours(1);
			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertGreaterThan("Precondition: Current time should be more recent than the last edit time", ZDateTime.UtcNow, Incident.IM_SystemLastEditTimeUtc);
			AssertEquals("Precondition: Should have no tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
			AssertEquals("Should be highest sequence + 5", 7, promptTask.P9_Sequence);
			AssertEquals("Should be populated by cancelled task with highest sequence", staff.GS_Code, promptTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should be populated by cancelled task with highest sequence", capability.PK, promptTask.P9_G4_RequiredCapability);
		}

		public void TestShouldNotCreateTaskForClosedIncident()
		{
			Incident.CloseIncident(string.Empty, string.Empty);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no tasks", 0, Incident.WorkflowItems.Count);
			AssertEquals("Precondition: Should be closed", IncidentMainLookups.Status.Closed, Incident.IM_Status);

			process.RunTask();

			AssertEquals("No task should be created since the incident is already closed", 0, Incident.WorkflowItems.Count);
		}

		public void TestShouldNotCreateTaskForCancelledIncident()
		{
			Incident.IM_Status = IncidentConstants.IncidentStatus.Cancelled;

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no tasks", 0, Incident.WorkflowItems.Count);
			AssertEquals("Precondition: Should be cancelled", IncidentConstants.IncidentStatus.Cancelled, Incident.IM_Status);

			process.RunTask();

			AssertEquals("No task should be created since the incident is already canceled", 0, Incident.WorkflowItems.Count);
		}

		public void TestShouldNotCreateTaskForIncidentWithOpenTask()
		{
			Incident.WorkflowItems.AddNew();
			Factory.Save();

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have an open task", 1, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			process.RunTask();

			AssertEquals("No task should be created since the incident has an open task", 1, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
		}

		public void TestShouldNotCreateTaskForIncidentWithNoTasks()
		{
			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no tasks", 0, Incident.WorkflowItems.Count);
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			process.RunTask();

			AssertEquals("No task should be created since the incident has no tasks", 0, Incident.WorkflowItems.Count);
		}

		public void TestShouldNotCreateTaskForIncidentWithRelatedWorkItemOpen()
		{
			var relatedWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			relatedWorkItem.WorkflowItems.AddNew();
			Incident.RelatedWorkItems.Add(relatedWorkItem);
			Incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			var task1 = Incident.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have all tasks closed", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should have related WorkItem", 1, Incident.RelatedWorkItems.Count);
			AssertEquals("Precondition: WorkItem should not be closed or cancelled", false, relatedWorkItem.IsClosedOrCancelled);
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			process.RunTask();

			AssertEquals("No task should be created since the incident has an open related work item", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
		}

		[TestDate(2021, 10, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestShouldNotCreateTaskForIncidentWithEditTimeBeforeLastRun()
		{
			var lastRunTime = new ZDateTime(2021, 10, 22, 13, 0, 0).ToDateTime();
			EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lastRunTime);
			AssertEquals("Precondition: Should be set", lastRunTime, EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertGreaterThan("Precondition: Last edit time should be less recent than the last run time", lastRunTime, Incident.IM_SystemLastEditTimeUtc);
			AssertEquals("Precondition: Should have no tasks", 0, Incident.WorkflowItems.Count);
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			TestDateAttribute.AddDays(1);
			AssertGreaterThan("Precondition: current time should be greater than the last edit time", ZDateTime.UtcNow, Incident.IM_SystemLastEditTimeUtc);

			process.RunTask();

			AssertEquals("Task should not have been created", 0, Incident.WorkflowItems.Count);
		}

		[TestDate(2021, 10, 22, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestRunTaskShouldUpdateRegistry()
		{
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();
			CloseTasksKeepingIncidentOpen(task1, task2);
			TestDateAttribute.AddHours(1);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertGreaterThan("Precondition: Current time should be greater than last edit time", ZDateTime.UtcNow, Incident.IM_SystemLastEditTimeUtc);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);

			process.RunTask();

			AssertEquals("Task should have been created", 1, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Registry should be updated", new ZDateTime(2021, 10, 22, 13, 0, 0), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
		}

		[TestDate(2021, 10, 01, 1, 1, 1)]
		public void TestCreatedTaskShouldPopulateStaffAndCapabilityFromLatestClosedTask()
		{
			var staffA = Factory.NewWithValidTestData<GlbStaff>();
			var staffB = Factory.NewWithValidTestData<GlbStaff>();
			var staffC = Factory.NewWithValidTestData<GlbStaff>();
			var capabilityA = Factory.NewWithValidTestData<GlbCapability>();
			var capabilityB = Factory.NewWithValidTestData<GlbCapability>();
			var capabilityC = Factory.NewWithValidTestData<GlbCapability>();
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			var task3 = Incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_GS_NKAssignedStaffMember = staffA.GS_Code;
			task1.P9_G4_RequiredCapability = capabilityA.PK;
			task2.P9_Sequence = 20;
			task2.P9_GS_NKAssignedStaffMember = staffB.GS_Code;
			task2.P9_G4_RequiredCapability = capabilityB.PK;
			task3.P9_Sequence = 15;
			task3.P9_GS_NKAssignedStaffMember = staffC.GS_Code;
			task3.P9_G4_RequiredCapability = capabilityC.PK;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			TestDateAttribute.AddHours(1);
			CloseTasksKeepingIncidentOpen(task1, task3, true);
			TestDateAttribute.AddHours(1);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertGreaterThan("Precondition: Last edit time should be earlier than current time", ZDateTime.UtcNow, Incident.IM_SystemLastEditTimeUtc);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);
			AssertGreaterThan("Precondition: Task 3 should be the most recently closed task", task3.P9_CompletedTimeUtc, task2.P9_CompletedTimeUtc);
			AssertGreaterThan("Precondition: Task 3 should be the most recently closed task", task3.P9_CompletedTimeUtc, task1.P9_CompletedTimeUtc);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
			AssertEquals("Should populate from task 3 as it was the last one to be closed", staffC.GS_Code, promptTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should populate from task 3 as it was the last one to be closed", capabilityC.PK, promptTask.P9_G4_RequiredCapability);
			AssertEquals("Should create a new workflow", "Review Incident status and close", promptTask.ProcessHeader.FH_CompletionStatement);
			AssertEquals("Should have the sequence number of latest completed task + 5", 20, promptTask.P9_Sequence);
			AssertEquals("Type should investigation", "INV", promptTask.P9_Type);
			AssertEquals("Low estimated should be 10 min", 10, (int)Math.Round(promptTask.LowEstimatedDurationHours * 60));
		}

		[TestDate(2021, 10, 01, 1, 1, 1)]
		public void TestCreatedTaskShouldPrioritiseClosedTasksOverCancelled()
		{
			var staffA = Factory.NewWithValidTestData<GlbStaff>();
			var staffB = Factory.NewWithValidTestData<GlbStaff>();
			var capabilityA = Factory.NewWithValidTestData<GlbCapability>();
			var capabilityB = Factory.NewWithValidTestData<GlbCapability>();
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_GS_NKAssignedStaffMember = staffA.GS_Code;
			task1.P9_G4_RequiredCapability = capabilityA.PK;
			task2.P9_Sequence = 20;
			task2.P9_GS_NKAssignedStaffMember = staffB.GS_Code;
			task2.P9_G4_RequiredCapability = capabilityB.PK;
			Factory.Save();

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var task2Reloaded = secondFactory.Load<ProcessTask>(task2.PK);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			TestDateAttribute.AddHours(1);
			task2Reloaded.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			secondFactory.Save();
			task2.Reload();
			TestDateAttribute.AddHours(1);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertGreaterThan("Precondition: Last edit time should be earlier than current time", ZDateTime.UtcNow, Incident.IM_SystemLastEditTimeUtc);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
			AssertEquals("Should populate from task 1 as it was closed rather than cancelled", staffA.GS_Code, promptTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should populate from task 1 as it was closed rather than cancelled", capabilityA.PK, promptTask.P9_G4_RequiredCapability);
			AssertEquals("Should create a new workflow", "Review Incident status and close", promptTask.ProcessHeader.FH_CompletionStatement);
			AssertEquals("Should have the sequence number of latest completed task + 5", 15, promptTask.P9_Sequence);
			AssertEquals("Type should investigation", "INV", promptTask.P9_Type);
			AssertEquals("Low estimated should be 10 min", 10, (int)Math.Round(promptTask.LowEstimatedDurationHours * 60));
		}

		[TestDate(2021, 10, 01, 1, 1, 1)]
		public void TestCreatedTaskShouldPopulateStaffAndCapabilityFromLatestClosedTaskOnRelatedWorkItem()
		{
			var staffA = Factory.NewWithValidTestData<GlbStaff>();
			var staffB = Factory.NewWithValidTestData<GlbStaff>();
			var staffC = Factory.NewWithValidTestData<GlbStaff>();
			var staffD = Factory.NewWithValidTestData<GlbStaff>();
			var capabilityA = Factory.NewWithValidTestData<GlbCapability>();
			var capabilityB = Factory.NewWithValidTestData<GlbCapability>();
			var capabilityC = Factory.NewWithValidTestData<GlbCapability>();
			var capabilityD = Factory.NewWithValidTestData<GlbCapability>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(Incident, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_ParentId = Incident.PK;
			workflow3.FH_ParentId = Incident.PK;
			workflow1.FH_ParentId = Incident.PK;
			workflow1.FH_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			workflow2.FH_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			workflow3.FH_ParentTableCode = IncidentMainSchema.Constants.Prefix;
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			var task3 = Incident.WorkflowItems.AddNew();

			task1.P9_Sequence = 10;
			task1.P9_GS_NKAssignedStaffMember = staffA.GS_Code;
			task1.P9_G4_RequiredCapability = capabilityA.PK;
			task1.P9_FH_ProcessHeader = workflow1.PK;

			task2.P9_Sequence = 20;
			task2.P9_GS_NKAssignedStaffMember = staffB.GS_Code;
			task2.P9_G4_RequiredCapability = capabilityB.PK;
			task2.P9_FH_ProcessHeader = workflow2.PK;

			task3.P9_Sequence = 15;
			task3.P9_GS_NKAssignedStaffMember = staffC.GS_Code;
			task3.P9_G4_RequiredCapability = capabilityC.PK;
			task3.P9_FH_ProcessHeader = workflow3.PK;

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			TestDateAttribute.AddHours(1);
			var relatedWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			relatedWorkItem.WorkflowItems.AddNew();
			Incident.RelatedWorkItems.Add(relatedWorkItem);
			Incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			var latestClosedTask = relatedWorkItem.WorkflowItems.AddNew();
			latestClosedTask.P9_Sequence = 1;
			latestClosedTask.P9_GS_NKAssignedStaffMember = staffD.GS_Code;
			latestClosedTask.P9_G4_RequiredCapability = capabilityD.PK;
			latestClosedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			TestDateAttribute.AddHours(1);
			var cancelledTask = relatedWorkItem.WorkflowItems.AddNew();
			cancelledTask.P9_Sequence = 2;
			cancelledTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			relatedWorkItem.Cancel();

			TestDateAttribute.AddHours(1);

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var task3Reloaded = secondFactory.Load<ProcessTask>(task3.PK);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3Reloaded.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();
			secondFactory.Save();
			task3.Reload();
			TestDateAttribute.AddHours(1);

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Working, Incident.IM_Status);
			AssertEquals("Precondition: Should be closed or cancelled", true, relatedWorkItem.IsClosedOrCancelled);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
			AssertEquals("Should populate from task on work item as it was the last one to be closed/cancelled", staffD.GS_Code, promptTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should populate from task on work item as it was the last one to be closed/cancelled", capabilityD.PK, promptTask.P9_G4_RequiredCapability);
			AssertEquals("Should create a new workflow", "Review Incident status and close", promptTask.ProcessHeader.FH_CompletionStatement);
			AssertEquals("Should have the latest sequence number", 25, promptTask.P9_Sequence);
			AssertEquals("Type should investigation", "INV", promptTask.P9_Type);
			AssertEquals("Low estimated should be 10 min", 10, (int)Math.Round(promptTask.LowEstimatedDurationHours * 60));
		}

		public void TestCreatedTaskShouldPopulateStaffFromTheLatestTaskWhichHasAnAssignedStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Sequence = 2;
			Factory.Save();

			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var task2Reloaded = secondFactory.Load<ProcessTask>(task2.PK);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			TestDateAttribute.AddHours(1);

			task2Reloaded.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			secondFactory.Save();
			task2.Reload();

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
			AssertEquals("Should populate from latest task with a staff member assigned", staff.GS_Code, promptTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should populate from latest task with a staff member assigned", ZGuid.Empty, promptTask.P9_G4_RequiredCapability);
			AssertEquals("Should populate from latest task with a staff member assigned", 6, promptTask.P9_Sequence);
			AssertEquals("Type should investigation", "INV", promptTask.P9_Type);
			AssertEquals("Low estimated should be 10 min", 10, (int)Math.Round(promptTask.LowEstimatedDurationHours * 60));
		}

		public void TestCreatedTaskShouldPopulateFromTheLatestTaskWhichHasAnAssignedCapability()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var task1 = Incident.WorkflowItems.AddNew();
			var task2 = Incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_G4_RequiredCapability = capability.PK;
			task2.P9_Sequence = 2;
			Factory.Save();

			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var task2Reloaded = secondFactory.Load<ProcessTask>(task2.PK);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			TestDateAttribute.AddHours(1);

			task2Reloaded.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			secondFactory.Save();
			task2.Reload();

			AssertEquals("Precondition: Should have minimum default value", new ZDateTime(2021, 09, 13), EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc.Value);
			AssertEquals("Precondition: Should have no open tasks", 0, Incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
			AssertEquals("Precondition: Should be open", IncidentMainLookups.Status.Open, Incident.IM_Status);

			process.RunTask();

			var openTasks = Incident.WorkflowItems.Where(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled).ToArray();
			AssertEquals("Task should have been created", 1, openTasks.Length);
			var promptTask = openTasks[0];
			AssertEquals("Review Incident status and close", promptTask.P9_Description);
			AssertEquals(@"This Incident has an open status and all workflow tasks were closed or cancelled.

To transition the Incident to a closed state, close this task and when prompted decide if its appropriate to send the Incident closure email notification to the customer.Cancel the email notification if its not appropriate.", ORtfTextUtil.RtfToText(promptTask.P9_Notes.ToAscii()));
			AssertEquals("Should populate from latest task with a capability assigned", capability.PK, promptTask.P9_G4_RequiredCapability);
			AssertEquals("Should populate from latest task with a capability assigned", string.Empty, promptTask.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should populate from latest task with a capability assigned", 6, promptTask.P9_Sequence);
			AssertEquals("Type should investigation", "INV", promptTask.P9_Type);
			AssertEquals("Low estimated should be 10 min", 10, (int)Math.Round(promptTask.LowEstimatedDurationHours * 60));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Implementation

		void CloseTasksKeepingIncidentOpen(ProcessTask task1, ProcessTask task2, bool shouldAddTimeBetweenSaves = false)
		{
			Incident.SetConcurrencyPolicy("IM_Status", ConcurrencyPolicy.Ignore);
			Incident.SetConcurrencyPolicy("IM_ResolutionCode", ConcurrencyPolicy.Ignore);

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var task2Reloaded = secondFactory.Load<ProcessTask>(task2.PK);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			if (shouldAddTimeBetweenSaves)
			{
				TestDateAttribute.AddHours(1);
			}

			task2Reloaded.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			var incidentReloaded = task2Reloaded.Parent as SupportIncident;
			incidentReloaded.SetConcurrencyPolicy("IM_Status", ConcurrencyPolicy.Ignore);
			incidentReloaded.SetConcurrencyPolicy("IM_ResolutionCode", ConcurrencyPolicy.Ignore);

			secondFactory.Save();
			task2.Reload();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			if (UseSnapshotProtectionAttribute.IsProtected)
			{
				return;
			}
			logger = new TestServiceLogger();
			SetupIncident();
			process = new IncidentClosePromptServiceTaskForTest(Incident) { ServiceLogger = logger };

			bmTestHelper = ObjectFactory.Get<IBMTestHelper>();

			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, "INC");
		}

		IncidentClosePromptServiceTask process;
		TestServiceLogger logger;
		SupportIncident Incident;
		IBMTestHelper bmTestHelper;

		class IncidentClosePromptServiceTaskForTest : IncidentClosePromptServiceTask
		{
			public IncidentClosePromptServiceTaskForTest(SupportIncident incident)
			{
				Incident = incident;
			}

			SupportIncident Incident { get; }

			BusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new BusinessObjectFactory()); }
			}
			BusinessObjectFactory factory;

			public override void RunTask(CancellationToken token)
			{
				var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					base.RunTask(token);
					Incident.WorkflowItems.Reload(true);
				}
			}
		}

		void SetupIncident()
		{
			var client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test.contact@abc.org";
			client.Contacts.Add(contact);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Tester";

			Incident = Factory.NewWithValidTestData<SupportIncident>();
			Incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			Incident.IM_IncidentNumber = "CS00088801";
			Incident.IM_Description = "Incident 1";
			Incident.IM_OH_Client = client.PK;
			Incident.IM_OC_Contact = contact.PK;

			Factory.Save();
		}

		#endregion
	}
}
