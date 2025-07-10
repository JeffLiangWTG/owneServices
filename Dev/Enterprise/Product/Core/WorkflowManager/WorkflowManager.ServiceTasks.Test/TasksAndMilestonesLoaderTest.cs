using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	[TestedType(typeof(TasksAndMilestonesLoader))]
	class TasksAndMilestonesLoaderTest : LogSubscriberTest<TasksAndMilestonesLoader>
	{
		public void TestUseTheCorrectUserContext()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";

			var task1 = template.WorkflowItems.Triggers.AddNew();
			task1.P9_Description = "The adventures of Sherp-Nerder";
			task1.TriggerConditions.TriggerEventCode = "ADD";
			var triggerAction = task1.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = "IFC";
			triggerAction.PQ_FieldName = "<OH_FullName>";
			triggerAction.PQ_FieldValue = "BUG";

			var someGuy = Factory.NewWithValidTestData<GlbStaff>();
			someGuy.GS_IsController = true;
			Factory.Save();
			OrgHeader org;
			using (Env.SetTemporaryUserContext(someGuy.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				org = Factory.NewWithValidTestData<OrgHeader>();
				org.Logs.AddNew(new EventValue(Events.CustomisableEvent00, deferFiringWorkflow: true));

				using (SuspendApplyingWorkflowTemplatesOnSave())
				{
					Factory.Save();
				}
			}

			RunLogWalkerCycleForTest();

			var orgLogs = org.Logs.GetAllLogs();
			orgLogs.Reload(true);
			org.Reload();

			AssertEquals("BUG", org.OH_FullName);
			AssertEquals("No edits should have happened to the org in the wrong user context", false, orgLogs.Cast<StmALog>().Any(l => l.SL_GS_NKUser != someGuy.GS_Code));
		}

		public void TestTableNames_ContainsAdditionalWorkflowSupportableTables()
		{
			var loader = new TasksAndMilestonesLoader();
			var tableNames = loader.TableNames;

			CombineAssertions(delegate
			{
				AssertCollectionContains(PkgPackageSchema.Constants.TableName, tableNames);
				AssertCollectionContains(DtbConsignmentRunSheetInstructionSchema.Constants.TableName, tableNames);
				AssertCollectionContains(HVLVItemSchema.Constants.TableName, tableNames);
			});
		}

		public void TestLoadTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";

			var task1 = template.WorkflowItems.Tasks.AddNew();
			var task2 = template.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1 - That's a paddlin.";
			task2.P9_Description = "Task 2 - You better believe that's a paddlin.";

			Factory.Save();

			var workflowParent = Factory.NewWithValidTestData<OrgHeader>();

			using (SuspendApplyingWorkflowTemplatesOnSave())
			{
				Factory.Save();
			}

			AssertTasks("We've disabled the application of workflow templates on save, so shouldn't apply workflow templates", workflowParent, Array.Empty<string>());

			RunLogWalkerCycleForTest();
			AssertTasks("There were no events with FireWorkflow = 1, so LogWalker shouldn't apply workflow templates", workflowParent, Array.Empty<string>());

			var log1 = workflowParent.Logs.AddNew(Events.JobClose, "Job closed for the first time");

			AssertEquals(false, log1.SL_FireWorkflow);

			using (SuspendApplyingWorkflowTemplatesOnSave())
			{
				Factory.Save();
			}

			RunLogWalkerCycleForTest();
			AssertTasks("There were no events with FireWorkflow = 1, so LogWalker shouldn't apply workflow templates", workflowParent, Array.Empty<string>());

			var log2 = workflowParent.Logs.AddNew(Events.JobClose, "Job closed for the second time");
			log2.SL_FireWorkflow = true;

			using (SuspendApplyingWorkflowTemplatesOnSave())
			{
				Factory.Save();
			}

			RunLogWalkerCycleForTest();
			AssertTasks("There was an event with FireWorkflow = 1, so LogWalker should go right ahead and apply workflow templates", workflowParent, "Task 1 - That's a paddlin.", "Task 2 - You better believe that's a paddlin.");
		}

		public void TestProcess_LogWithDeferredWorkflowFiring_FireWorkflow()
		{
			var workflowParent = Factory.NewWithValidTestData<OrgHeader>();

			var milestone = workflowParent.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			workflowParent.Logs.AddNew(new EventValue(Events.Departure,
				eventTime: new ZDateTimeOffset(1991, 8, 24),
				deferFiringWorkflow: true));

			Factory.Save();

			AssertEquals("The milestone before the log walker run", ZDateTime.Empty, milestone.P9_ActualDate.ToZDateTime());

			RunLogWalkerCycleForTest();
			milestone.Reload();
			AssertEquals("The milestone after the log walker run", new ZDateTime(1991, 8, 24), milestone.P9_ActualDate.ToZDateTime());
		}

		public void TestProcess_LogWithDeferredWorkflowFiringAndTemplateIsNotLoaded_LoadTemplateAndTriggerProcessTasksOnce()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "McLaren";
			trigger.TriggerConditions.TriggerEventCode = Events.DepartureCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_TriggerParty = "ORP";

			Factory.Save();

			var workflowParent = Factory.NewWithValidTestData<OrgHeader>();
			workflowParent.Logs.AddNew(new EventValue(Events.Departure,
				eventTime: new ZDateTimeOffset(1991, 8, 24),
				deferFiringWorkflow: true));

			using (SuspendApplyingWorkflowTemplatesOnSave())
			{
				Factory.Save();
			}

			AssertEquals("PRECONDITION: Number of process tasks on the parent before test", 0, workflowParent.WorkflowItems.Count);

			RunLogWalkerCycleForTest();
			workflowParent.WorkflowItems.Reload(true);

			AssertEquals("Number of process tasks on the parent (should load from template)", 1, workflowParent.WorkflowItems.Count);

			var wteEvents = workflowParent.WorkflowItems[0].Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			AssertEquals("Number of times the trigger has been triggered (1 time is expected, durring applying the template)", 1, wteEvents.Count());
		}

		public void TestImmediateFieldChangeIsApplied()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var fieldName = OrgHeaderSchema.OH_FullName.Name;
			var defaultValue = "Some default value to check against";
			orgHeader.OH_FullName = defaultValue;

			var targetFieldValue = "Set from Trigger Action";

			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = targetFieldValue;

			// Set deferFiringWorkflow to allow the service task to pick up the work
			orgHeader.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01, deferFiringWorkflow: true));
			Factory.Save();

			var before = Factory.Load<OrgHeader>(orgHeader.PK);
			before.Reload();
			AssertEquals(defaultValue, before.OH_FullName);

			RunLogWalkerCycleForTest();

			var updated = Factory.Load<OrgHeader>(orgHeader.PK);
			updated.Reload();

			AssertEquals(targetFieldValue, orgHeader.OH_FullName);
			AssertEquals(false, trigger.HasRowWarnings);
		}

		public void TestCancelling()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var trigger = orgHeader.WorkflowItems.Milestones.AddNew();
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<WorkflowItems.P9_ActualDateForBinding>";
			action.PQ_FieldValue = "";

			// Set deferFiringWorkflow to allow the service task to pick up the work
			orgHeader.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01, deferFiringWorkflow: true));
			Factory.Save();
			RunLogWalkerCycleForTest();

			var reloadedLog = new BusinessObjectFactory().LoadTop1<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent01Code).AddToFilter(StmALogSchema.SL_Parent, orgHeader.PK));
			Assert(reloadedLog.SL_IsCancelled);
		}

		public void TestNoExceptionsWhenNoStmALogForStmJobQueue()
		{
			var log = (BusinessObject)Factory.New<IQueuedLog>();
			log["SJ_FilterName"] = "TasksAndMilestonesLoader";
			log["SJ_SE_NKEvent"] = Events.CustomisableEvent01Code;
			log["SJ_Status"] = JobQueueStatus.StatusQueued;
			log["SJ_IsDelayFired"] = false;
			log["SJ_EventTime"] = ZDateTime.Now;
			log["SJ_EventTimeUtc"] = ZDateTime.UtcNow;
			log["SJ_PostedTimeUtc"] = ZDateTime.UtcNow;
			log["SJ_ParentTableCode"] = "Z0";

			Factory.Save();

			AssertNoExceptionThrown(() => RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks());

			log.Reload();
			AssertEquals("Log processed successfully", JobQueueStatus.StatusProcessed, log["SJ_Status"]);
		}

		public void TestNoStmALogDbHit()
		{
			var log = (BusinessObject)Factory.New<IQueuedLog>();
			log["SJ_FilterName"] = "TasksAndMilestonesLoader";
			log["SJ_SE_NKEvent"] = Events.CustomisableEvent01Code;
			log["SJ_Status"] = JobQueueStatus.StatusQueued;
			log["SJ_IsDelayFired"] = false;
			log["SJ_EventTime"] = ZDateTime.Now;
			log["SJ_EventTimeUtc"] = ZDateTime.UtcNow;
			log["SJ_PostedTimeUtc"] = ZDateTime.Now;
			log["SJ_GS_NKUser"] = GlbStaff.CurrentUser.GS_Code;
			log["SJ_GE_NKDepartment"] = GlbDepartment.CurrentDepartment.GE_Code;
			log["SJ_GB_NKBranch"] = GlbBranch.CurrentBranch.GB_Code;
			log["SJ_ParentTableCode"] = "Z0";
			log[StmJobQueueSchema.Constants.SJ_ALogReference] = ZGuid.NewZGuid();
			Factory.Save();

			using (AssertDbHitsForAllFactories(new Dictionary<string, int> { { StmALogSchema.Constants.TableName, 0 } }, ignoreUnspecified: true))
			{
				AssertNoExceptionThrown(() => RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks());
			}
		}

		public void TestProcess_StillProcessesLogsWithStmALog_WhenJobWithoutStmALogExists()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var fieldName = OrgHeaderSchema.OH_FullName.Name;
			var defaultValue = "Some default value to check against";
			orgHeader.OH_FullName = defaultValue;

			var targetFieldValue = "Set from Trigger Action";

			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = targetFieldValue;

			// Add StmJobQueue WITHOUT StmALog
			var log = (BusinessObject)Factory.New<IQueuedLog>();
			log["SJ_FilterName"] = "TasksAndMilestonesLoader";
			log["SJ_SE_NKEvent"] = Events.CustomisableEvent01Code;
			log["SJ_Status"] = JobQueueStatus.StatusQueued;
			log["SJ_IsDelayFired"] = false;
			log["SJ_EventTime"] = ZDateTime.Now;
			log["SJ_EventTimeUtc"] = ZDateTime.UtcNow;
			log["SJ_PostedTimeUtc"] = ZDateTime.UtcNow;
			log["SJ_ParentTableCode"] = "Z0";

			// Add StmJobQueue without StmALog
			// Set deferFiringWorkflow to allow the service task to pick up the work
			orgHeader.Logs.AddNew(new EventValue(Events.CustomisableEvent01, deferFiringWorkflow: true));
			Factory.Save();

			var before = Factory.Load<OrgHeader>(orgHeader.PK);
			before.Reload();
			AssertEquals(defaultValue, before.OH_FullName);

			AssertNoExceptionThrown(() => RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks());

			var updated = Factory.Load<OrgHeader>(orgHeader.PK);
			updated.Reload();

			AssertEquals(targetFieldValue, orgHeader.OH_FullName);
			AssertEquals(false, trigger.HasRowWarnings);

			var wteEvents = orgHeader.WorkflowItems[0].Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			AssertEquals("Number of times the trigger has been triggered (1 time is expected, durring applying the template)", 1, wteEvents.Count());

			log.Reload();
			AssertEquals("Log processed successfully", JobQueueStatus.StatusProcessed, log["SJ_Status"]);
		}

		static void AssertTasks<T>(string message, T parent, params string[] taskDescriptions)
			where T : BusinessObject, IWorkflowProvider
		{
			var newFactory = parent.Factory.CreateNewFactory();
			var reloadedParent = (IWorkflowProvider)newFactory.Load(parent.GetType(), parent.PK);

			AssertEquals(message + System.Environment.NewLine + "WorkflowItems.Tasks.Count", taskDescriptions.Length, reloadedParent.WorkflowItems.Tasks.Count);

			CombineAssertions(message, () =>
			{
				foreach (var description in taskDescriptions)
				{
					AssertNotNull("Should be a task with description " + description, reloadedParent.WorkflowItems.Tasks.Cast<ProcessTask>().SingleOrDefault(t => t.P9_Description == description));
				}
			});
		}

		static IDisposable SuspendApplyingWorkflowTemplatesOnSave()
		{
			return ProcessTask.Loader.SuppressTemplateApplication();
		}
	}
}
