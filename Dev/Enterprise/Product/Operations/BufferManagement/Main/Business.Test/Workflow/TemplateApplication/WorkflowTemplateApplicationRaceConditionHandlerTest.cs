using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkflowTemplateApplicationRaceConditionHandlerTest : TestCaseWithFactory
	{
		#region Process

		public void TestProcess_ShouldDeleteDuplicates()
		{
			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "DUPLICATE");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);

			workflow1.FH_ParentTemplateId = templateWorkflow.PK;

			Factory.Save();

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "DuPlIcAtE");
			workflow2.FH_ParentTemplateId = templateWorkflow.PK;

			AssertEquals("Workflow are in the same header", workflow1.JobHeader, workflow2.JobHeader);
			AssertEquals("Before merge 2 workflow with same description are in the header", 2, jobHeader.ChildHeaders.Count());

			workflowTemplateApplicationRaceConditionHandler.Process(new[] { workflow1, workflow2 });

			AssertEquals("Should only have 1 workflow as result of the merge", 1, jobHeader.ChildHeaders.Count());
			Assert("Workflow 2 should be deleted as result of the merge", workflow2.IsDeleted);
		}

		public void TestProcess_ShouldDeleteDuplicates_OnDifferentFactories()
		{
			var factory1 = Factory.CreateNewFactory();
			var factory2 = Factory.CreateNewFactory();

			Factory.RefreshEnabled = false;
			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "DUPLICATE");
			BMSTestHelper.CreateTask(template, templateWorkflow);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			var jobHeaderFromFactory1 = factory1.Load<ProcessJobHeader>(jobHeader.PK);
			var jobHeaderFromFactory2 = factory2.Load<ProcessJobHeader>(jobHeader.PK);

			jobHeaderFromFactory1.ApplyTemplate(template);
			jobHeaderFromFactory2.ApplyTemplate(template);

			AssertEquals("A workflow was create as result of the template application", 1, jobHeaderFromFactory1.ProcessHeaders.Count);
			AssertEquals("A workflow was create as result of the template application", 1, jobHeaderFromFactory2.ProcessHeaders.Count);

			var workflow1 = jobHeaderFromFactory1.ProcessHeaders.First();
			var workflow2 = jobHeaderFromFactory2.ProcessHeaders.First();

			AssertNotEquals("They are different workflows", workflow1.PK, workflow2.PK);
			AssertEquals("But they have same description", workflow1.FH_CompletionStatement, workflow2.FH_CompletionStatement);
			AssertEquals("In the same JobHeader, oh no!", workflow1.FH_FH_ParentHeader, workflow2.FH_FH_ParentHeader);

			factory1.Save();

			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);
			workflowTemplateApplicationRaceConditionHandler.Process(jobHeaderFromFactory2.ProcessHeaders);

			factory2.Save();

			jobHeader = Factory.CreateNewFactory().Load<ProcessJobHeader>(jobHeader.PK); //reload from DB

			AssertEquals("JobHeader should have only one workflow", 1, jobHeader.ProcessHeaders.Count);
			Assert("Duplicated workflow should be deleted", workflow2.IsDeleted);
		}

		public void TestProcess_ShouldNotDelete_WhenDifferentDescription()
		{
			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "A");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);

			workflow1.FH_ParentTemplateId = templateWorkflow.PK;

			Factory.Save();

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "B");
			workflow2.FH_ParentTemplateId = templateWorkflow.PK;

			AssertEquals("Workflow are in the same header", workflow1.JobHeader, workflow2.JobHeader);
			AssertEquals("Before merge 2 workflow with same description are in the header", 2, jobHeader.ChildHeaders.Count());

			workflowTemplateApplicationRaceConditionHandler.Process(new[] { workflow1, workflow2 });

			AssertEquals("No workflow should be deleted as result of the merge", 2, jobHeader.ChildHeaders.Count());
		}

		public void TestProcess_ShouldNotDelete_WhenHasDifferentParentJob()
		{
			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "DUPLICATE");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, description: "DUPLICATE");
			workflow1.FH_ParentTemplateId = templateWorkflow.PK;

			Factory.Save();

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "DUPLICATE");
			workflow2.FH_ParentTemplateId = templateWorkflow.PK;
			workflow2.FH_ParentId = Factory.NewWithValidTestData<DummyWithWorkflow>().PK;

			AssertEquals("Workflow are in the same header", workflow1.JobHeader, workflow2.JobHeader);
			AssertNotEquals("Workflow are in different Job", workflow1.FH_ParentId, workflow2.FH_ParentId);

			AssertEquals("Before merge 2 workflow with same description are in the header", workflow1.FH_FH_ParentHeader, workflow2.FH_FH_ParentHeader);

			workflowTemplateApplicationRaceConditionHandler.Process(new[] { workflow1, workflow2 });

			Assert("Workflow 2 should not be deleted as result of the merge because workflow2 is in a different parent", !workflow2.IsDeleted);
		}

		public void TestProcess_ShouldNotDelete_WhenHasDifferentParentHeader()
		{
			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "DUPLICATE");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, description: "DUPLICATE");
			workflow1.FH_ParentTemplateId = templateWorkflow.PK;

			Factory.Save();

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "DUPLICATE");
			workflow2.FH_ParentTemplateId = templateWorkflow.PK;
			workflow2.FH_FH_ParentHeader = jobHeader2.PK;
			workflow2.FH_ParentId = jobHeader2.FH_ParentId;

			AssertNotEquals("Workflows are in the different header", workflow1.JobHeader, workflow2.JobHeader);
			AssertNotEquals("Workflows are in a different Job", workflow1.FH_ParentId, workflow2.FH_ParentId);

			workflowTemplateApplicationRaceConditionHandler.Process(new[] { workflow1, workflow2 });

			Assert("Workflow 2 should not be deleted as result of the merge because workflow2 is in a different parent", !workflow2.IsDeleted);
		}

		public void TestProcess_ShouldNotDelete_WhenRegistryBufferManagementEnabledIsFalse()
		{
			BMSTestHelper.DisableBMSInRegistry();

			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);

			var onBeforeProcessHit = false;
			workflowTemplateApplicationRaceConditionHandler.OnBeforeProcess += (_, __) => onBeforeProcessHit = true;

			workflowTemplateApplicationRaceConditionHandler.Process(Enumerable.Empty<BusinessObject>());

			AssertEquals("Should not process workflows if BufferManagementEnabled is false", false, onBeforeProcessHit);
		}

		public void TestProcess_ShouldNotDelete_WhenRegistryEnableWorkflowTemplateApplicationConcurrencyProtectionIsFalse()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);

			var onBeforeProcessHit = false;
			workflowTemplateApplicationRaceConditionHandler.OnBeforeProcess += (_, __) => onBeforeProcessHit = true;

			workflowTemplateApplicationRaceConditionHandler.Process(Enumerable.Empty<BusinessObject>());

			AssertEquals("Should not process workflows if EnableWorkflowTemplateApplicationConcurrencyProtection is false", false, onBeforeProcessHit);
		}

		public void TestProcessWorkflow_ShouldExecuteInBatchOf50s()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);

			const int numerOfWorkflowToProcess = 150;
			var jobHeaders = new List<ProcessJobHeader>(numerOfWorkflowToProcess);

			for (int i = 0; i < numerOfWorkflowToProcess; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
				jobHeaders.Add(jobHeader);
			}

			Factory.Save();

			var workflowsToProcess = new List<ProcessHeader>(numerOfWorkflowToProcess);

			foreach (var jobHeader in jobHeaders)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
				workflow.FH_ParentTemplateId = templateWorkflow.PK;
				workflowsToProcess.Add(workflow);
			}
			using (Db.Connection.TrackExecutedCommands())
			{
				workflowTemplateApplicationRaceConditionHandler.Process(workflowsToProcess);

				var processHeaderQueryCount = Db.Connection.ExecutedCommands
				.Count(c => c.Contains("FROM dbo.ProcessHeader"));

				AssertEquals("Should query ProcessHeader 3 times 150 / 50", 3, processHeaderQueryCount);
			}
		}

		public void TestProcessWorkflow_ShouldNotThrowException_WhenWorkflowWasDeleted()
		{
			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "DUPLICATE");
			workflow.FH_ParentTemplateId = templateWorkflow.PK;

			workflow.Delete();

			AssertNoExceptionThrown("Should not process deleted workflows", () => workflowTemplateApplicationRaceConditionHandler.Process(new[] { workflow }));
		}

		public void TestProcessWorkflow_ShouldThrowConcurrencyException_WhenAnyTasksInDB()
		{
			var workflowTemplateApplicationRaceConditionHandler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: true);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "DUPLICATE");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType, name: "TEMPLATE NAME");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			BMSTestHelper.CreateTask(template, templateWorkflow);

			workflow.FH_ParentTemplateId = templateWorkflow.PK;

			BMSTestHelper.DisableBMSInRegistry();
			var previousTask = jobHeader.Parent.WorkflowItems.AddNew();

			AssertNull("Tasks without processHeader", previousTask.ProcessHeader);
			AssertEquals("but in the job", jobHeader.Parent, previousTask.Parent);

			Factory.Save();

			BMSTestHelper.EnableBMSInRegistry();

			var workflowDuplicated = BMSTestHelper.CreateWorkflow(jobHeader, "DUPLICATE");
			workflowDuplicated.FH_ParentTemplateId = templateWorkflow.PK;
			workflowDuplicated.TaskCollection.Add(previousTask);

			AssertEquals("Task now have Workflow", workflowDuplicated, previousTask.ProcessHeader);
			Assert("but is in DB", previousTask.IsInDatabase);
			var exceptionThrown = AssertExceptionThrown<ZSaveConcurrencyException>("Should throw ZSaveConcurrencyException, we don't want to delete tasks in DB.",
				() => workflowTemplateApplicationRaceConditionHandler.Process(new[] { workflowDuplicated }));
			AssertContains(@"Inner Message = Unreconcilable concurrency error due to Workflow Template application in DUPLICATE from Unknown Template.
This error has occurred because the same Workflow Template has been applied twice concurrently and Workflows have been duplicated.
This operation has been cancelled. The duplicate workflow has the Create dates ", exceptionThrown.Message);
		}

		#endregion

		#region EndToEnd

		public void TestShouldDeleteDuplicates_OnSave()
		{
			var factory1 = Factory.CreateNewFactory();
			var factory2 = Factory.CreateNewFactory();

			Factory.RefreshEnabled = false;
			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true));

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "DUPLICATE");
			BMSTestHelper.CreateTask(template, templateWorkflow);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			var jobHeaderFromFactory1 = factory1.Load<ProcessJobHeader>(jobHeader.PK);
			var jobHeaderFromFactory2 = factory2.Load<ProcessJobHeader>(jobHeader.PK);

			jobHeaderFromFactory1.ApplyTemplate(template);
			jobHeaderFromFactory2.ApplyTemplate(template);

			AssertEquals("A workflow was create as result of the template application", 1, jobHeaderFromFactory1.ProcessHeaders.Count);
			AssertEquals("A workflow was create as result of the template application", 1, jobHeaderFromFactory2.ProcessHeaders.Count);

			var workflow1 = jobHeaderFromFactory1.ProcessHeaders.First();
			var workflow2 = jobHeaderFromFactory2.ProcessHeaders.First();

			AssertNotEquals("They are different workflows", workflow1.PK, workflow2.PK);
			AssertEquals("But they have same description", workflow1.FH_CompletionStatement, workflow2.FH_CompletionStatement);
			AssertEquals("In the same JobHeader, oh no!", workflow1.FH_FH_ParentHeader, workflow2.FH_FH_ParentHeader);

			factory1.Save();
			factory2.Save();

			jobHeader = Factory.CreateNewFactory().Load<ProcessJobHeader>(jobHeader.PK); //reload from DB

			AssertEquals("JobHeader should have only one workflow", 1, jobHeader.ProcessHeaders.Count);
			Assert("Duplicated workflow should be deleted", workflow2.IsDeleted);
		}

		public void TestShouldTriggerValidation_OnSave_WhenTryHandleConflictsAutomaticallyIsFalse()
		{
			var factory1 = Factory.CreateNewFactory();
			factory1.NameForDebugging = "F1";
			var factory2 = Factory.CreateNewFactory();
			factory2.NameForDebugging = "F2";

			Factory.RefreshEnabled = false;
			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: false));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: false));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: false));

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "DUPLICATE");
			BMSTestHelper.CreateTask(template, templateWorkflow);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			var jobHeaderFromFactory1 = factory1.Load<ProcessJobHeader>(jobHeader.PK);
			var jobHeaderFromFactory2 = factory2.Load<ProcessJobHeader>(jobHeader.PK);

			jobHeaderFromFactory1.ApplyTemplate(template);
			jobHeaderFromFactory2.ApplyTemplate(template);

			AssertEquals("A workflow was create as result of the template application", 1, jobHeaderFromFactory1.ProcessHeaders.Count);
			AssertEquals("A workflow was create as result of the template application", 1, jobHeaderFromFactory2.ProcessHeaders.Count);

			var workflow1 = jobHeaderFromFactory1.ProcessHeaders.First();
			var workflow2 = jobHeaderFromFactory2.ProcessHeaders.First();

			AssertNotEquals("They are different workflows", workflow1.PK, workflow2.PK);
			AssertEquals("But they have same description", workflow1.FH_CompletionStatement, workflow2.FH_CompletionStatement);
			AssertEquals("In the same JobHeader, oh no!", workflow1.FH_FH_ParentHeader, workflow2.FH_FH_ParentHeader);

			factory1.Save();
			AssertNoExceptionThrown(factory2.Save);
		}

		public void TestShouldNotThrowException_OnSave_WhenWorkflowWasDeleted()
		{
			var factory1 = Factory.CreateNewFactory();
			var factory2 = Factory.CreateNewFactory();

			Factory.RefreshEnabled = false;
			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true));

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "DUPLICATE");
			BMSTestHelper.CreateTask(template, templateWorkflow);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			var jobHeaderFromFactory1 = factory1.Load<ProcessJobHeader>(jobHeader.PK);
			var jobHeaderFromFactory2 = factory2.Load<ProcessJobHeader>(jobHeader.PK);

			jobHeaderFromFactory1.ApplyTemplate(template);
			jobHeaderFromFactory2.ApplyTemplate(template);

			factory1.Save();

			jobHeaderFromFactory2.Reload();

			jobHeaderFromFactory2.ProcessHeaders.ToArray().ForEach(w => w.Delete());

			AssertNoExceptionThrown(() => factory2.Save());
		}

		public void TestShouldDeleteDefaultWorkflow_OnSave_WhenDuplicate()
		{
			var factory1 = Factory.CreateNewFactory();
			var factory2 = Factory.CreateNewFactory();

			Factory.RefreshEnabled = false;
			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true));

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			Factory.Save();
			var jobInFactory1 = factory1.Load<DummyWithWorkflow>(job.PK);
			var jobHeaderInFactory1 = factory1.Load<ProcessJobHeader>(jobHeader.PK);

			AssertEquals("No default was created yet", 0, jobHeaderInFactory1.ProcessHeaders.Count);

			ProcessJobHeader.GetForParent(jobInFactory1, factory1, addDefaultProcessHeaderIfNone: true);

			var defaultWorkflowInFactory1 = jobHeaderInFactory1.ProcessHeaders.Single();
			AssertEquals("After get JobHeader default workflow was created in factory 1", BMGlobalConstants.DefaultWorkflowCompletionStatement, defaultWorkflowInFactory1.FH_CompletionStatement);

			var jobInFactory2 = factory2.Load<DummyWithWorkflow>(job.PK);
			var jobHeaderInFactory2 = factory2.Load<ProcessJobHeader>(jobHeader.PK);

			AssertEquals("No default was created yet", 0, jobHeaderInFactory2.ProcessHeaders.Count);

			ProcessJobHeader.GetForParent(jobInFactory2, factory2, addDefaultProcessHeaderIfNone: true);

			var defaultWorkflowInFactory2 = jobHeaderInFactory2.ProcessHeaders.Single();
			AssertEquals("After get JobHeader default workflow was created in factory 2", BMGlobalConstants.DefaultWorkflowCompletionStatement, defaultWorkflowInFactory1.FH_CompletionStatement);

			AssertNotEquals("Oh no we have two default workflow", defaultWorkflowInFactory1.PK, defaultWorkflowInFactory2.PK);

			factory1.Save();
			factory2.Save();

			var reloadedJobHeader = Factory.CreateNewFactory().Load<ProcessJobHeader>(jobHeader.PK);

			AssertEquals("TemplateApplicationRaceConditionHandler should had deleted default duplicated workflow", 1, reloadedJobHeader.ProcessHeaders.Count);
			AssertEquals(BMGlobalConstants.DefaultWorkflowCompletionStatement, reloadedJobHeader.ProcessHeaders.Single().FH_CompletionStatement);
		}

		[StressTest]
		public void TestHighLoad_ShouldRunSmoothly()
		{
			const int numberOfWorkflows = 300;

			var factory1 = Factory.CreateNewFactory();
			var factory2 = Factory.CreateNewFactory();

			Factory.RefreshEnabled = false;
			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true));

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);

			for (int i = 0; i < numberOfWorkflows; i++)
			{
				var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
				BMSTestHelper.CreateTask(template, templateWorkflow);
			}

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			var jobHeaderFromFactory1 = factory1.Load<ProcessJobHeader>(jobHeader.PK);
			var jobHeaderFromFactory2 = factory2.Load<ProcessJobHeader>(jobHeader.PK);

			jobHeaderFromFactory1.ApplyTemplate(template);
			jobHeaderFromFactory2.ApplyTemplate(template);

			AssertEquals("Workflows were created as result of the template application", numberOfWorkflows, jobHeaderFromFactory1.ProcessHeaders.Count);
			AssertEquals("Workflows were created as result of the template application", numberOfWorkflows, jobHeaderFromFactory2.ProcessHeaders.Count);

			factory1.Save();
			factory2.Save();

			jobHeader = Factory.CreateNewFactory().Load<ProcessJobHeader>(jobHeader.PK); //reload from DB

			AssertEquals($"JobHeader should have {numberOfWorkflows} workflows", numberOfWorkflows, jobHeader.ProcessHeaders.Count);
		}

		#endregion

		#region Implementation

		const string WorkflowType = WorkflowDescriptors.DummyWorkflowDescriptorCode;

		protected override void SetUp()
		{
			BMSTestHelper.EnableBMSInRegistry();
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.RefreshEnabled = false;

			BMSTestHelper.CreateSystem(Factory, WorkflowType);

			base.SetUp();
		}

		#endregion
	}

	class WorkflowTemplateApplicationRaceConditionHandlerLockTests : NonTransactionedTestCase
	{
		public void TestProcess_ShouldNotLockProcessHeader_WhenRegistryEnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLockIsFalse()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var handler = new WorkflowTemplateApplicationRaceConditionHandler(Factory, tryHandleConflictsAutomatically: false);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "DUPLICATE");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			workflow.FH_ParentTemplateId = templateWorkflow.PK;

			var onBeforeProcessHeaderLockHit = false;
			var onAfterProcessHeaderLockHit = false;

			handler.OnBeforeLock += (_, __) => onBeforeProcessHeaderLockHit = true;
			handler.OnAfterLock += (_, __) => onAfterProcessHeaderLockHit = true;

			handler.Process(new[] { workflow });

			AssertEquals("Should not lock ProcessHeader if EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock is false", false, onBeforeProcessHeaderLockHit);
			AssertEquals("Should not lock ProcessHeader if EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock is false", false, onAfterProcessHeaderLockHit);
		}

		public void TestShouldLockProcessHeader_WhenRegistryEnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLockIsTrue()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var factory1 = Factory.CreateNewFactory();
			factory1.RefreshEnabled = false;
			factory1.RelinquishThreadOwnership();

			var factory2 = Factory.CreateNewFactory();
			factory2.RefreshEnabled = false;
			factory2.RelinquishThreadOwnership();

			var mrseOne = new ManualResetEvent(false);
			var mrseTwo = new ManualResetEvent(false);
			var mrseThree = new ManualResetEvent(false);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);

			const int expectedNumberOfProcessHeaderInJob = 5;

			for (int i = 0; i < expectedNumberOfProcessHeaderInJob; i++)
			{
				var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "DUPLICATE" + i);
				BMSTestHelper.CreateTask(template, templateWorkflow);
			}

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			var templatePK = template.PK;
			var jobHeaderPK = jobHeader.PK;

			var threadOneHit = false;
			var threadTwoHit = false;

			var threadOne = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory1.TakeThreadOwnership();
					TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true);
					var workflowRaceConditionHandlerInFactory1 = WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(factory1).WorkflowRaceConditionHandler;

					var query = new ZQuery();
					query.AddToFilter(ProcessHeaderSchema.PK, jobHeaderPK);
					query.IsNoLock = true;

					var jobHeader1 = factory1.Load<ProcessJobHeader>(query).First();
					jobHeader1.ApplyTemplate(factory1.Load<ProcessTaskTemplate>(templatePK));

					workflowRaceConditionHandlerInFactory1.OnBeforeLock += (_, __) =>
					{
						mrseTwo.Set();
						threadOneHit = true;
						mrseOne.WaitOne();
					};

					factory1.Save();
				}
			});

			var threadTwo = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory2.TakeThreadOwnership();
					TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true);
					var workflowRaceConditionHandlerInFactory2 = WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(factory2).WorkflowRaceConditionHandler;

					var query = new ZQuery();
					query.AddToFilter(ProcessHeaderSchema.PK, jobHeaderPK);
					query.IsNoLock = true;

					var jobHeader2 = factory2.Load<ProcessJobHeader>(query).First();
					jobHeader2.ApplyTemplate(factory2.Load<ProcessTaskTemplate>(templatePK));

					workflowRaceConditionHandlerInFactory2.OnBeforeLock += (_, __) =>
					{
						mrseThree.Set();
					};

					workflowRaceConditionHandlerInFactory2.OnAfterLock += (_, __) =>
					{
						threadTwoHit = true;
					};

					factory2.Save();
				}
			});

			threadOne.Start();
			mrseTwo.WaitOne();
			threadTwo.Start();
			mrseThree.WaitOne();

			AssertEquals("Thread one should be hit", true, threadOneHit);
			AssertEquals("Thread two should not be hit yet", false, threadTwoHit);

			mrseOne.Set();
			mrseTwo.Set();

			threadOne.Join();
			threadTwo.Join();

			jobHeader = Factory.CreateNewFactory().Load<ProcessJobHeader>(jobHeader.PK); //reload from DB

			AssertEquals("Thread two should be hit", true, threadTwoHit);
			AssertEquals($"jobHeader should have only {expectedNumberOfProcessHeaderInJob} ProcessHeaders", expectedNumberOfProcessHeaderInJob, jobHeader.ProcessHeaders.Count);
		}

		#region Implementation

		const string WorkflowType = WorkflowDescriptors.DummyWorkflowDescriptorCode;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.RefreshEnabled = false;

			BMSTestHelper.CreateSystem(Factory, WorkflowType);
		}

		#endregion
	}
}
