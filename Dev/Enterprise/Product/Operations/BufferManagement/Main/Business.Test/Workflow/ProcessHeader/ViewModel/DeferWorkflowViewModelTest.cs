using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(DeferWorkflowViewModel))]
	class DeferWorkflowViewModelTest : NonPersistentBusinessObjectTestCase
	{
		#region Defer

		public void TestDefer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = system.Components.AddNew();
			var bucket2 = system.Components.AddNew();

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = bucket2.PK;

			var viewModel = new DeferWorkflowViewModel(workflow, new[] { bucket2.PK });
			viewModel.Defer();
			AssertEquals(bucket1.PK, workflow.FH_FC_CurrentComponent);
		}

		[TestDate(2013, 12, 13)]
		public void TestDefer_ShouldAlsoDeferPrerequisites()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system);
			var bucket2 = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.LinkComponents(bucket1, bucket2);
			BMSTestHelper.LinkComponents(bucket2, buffer);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader1.FH_CompletionStatement = "jobHeader1";
			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_1.FH_CompletionStatement = "workflow1_1";
			workflow1_1.FH_FC_CurrentComponent = buffer.PK;
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_2.FH_CompletionStatement = "workflow1_2";
			workflow1_2.FH_FC_CurrentComponent = buffer.PK;

			workflow1_1.MakePrerequisiteOf(workflow1_2);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader2.FH_CompletionStatement = "jobHeader2";
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			workflow2_1.FH_CompletionStatement = "workflow2_1";
			workflow2_1.FH_FC_CurrentComponent = bucket2.PK;
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2_2.FH_CompletionStatement = "workflow2_2";
			workflow2_2.FH_FC_CurrentComponent = buffer.PK;

			workflow2_1.MakePrerequisiteOf(workflow2_2);
			jobHeader1.MakePrerequisiteOf(jobHeader2);

			var viewModel = new DeferWorkflowViewModel(workflow1_1, new[] { buffer.PK });

			AssertEquals(2, viewModel.WorkflowsToDefer.Count);

			AssertEquals(workflow1_2, viewModel.WorkflowsToDefer[0].ProcessHeader);
			AssertEquals(jobHeader2, viewModel.WorkflowsToDefer[1].ProcessHeader);
			AssertEquals(WorkflowDeferalActionList.Codes.None, viewModel.WorkflowsToDefer[0].ActionToBeTaken);
			AssertEquals(WorkflowDeferalActionList.Codes.None, viewModel.WorkflowsToDefer[1].ActionToBeTaken);

			viewModel.WorkflowsToDefer[0].ActionToBeTaken = WorkflowDeferalActionList.Codes.Defer;
			viewModel.WorkflowsToDefer[1].ActionToBeTaken = WorkflowDeferalActionList.Codes.Defer;

			var deferTime = new ZDateTime(2014, 1, 1);
			viewModel.DoNotStartBeforeDate = deferTime;

			viewModel.Defer();

			AssertEquals("Should defer target workflow", bucket1, workflow1_1.CurrentComponent);
			AssertEquals(deferTime, workflow1_1.DoNotStartBeforeDateLocal);

			AssertDependencyRelationshipExists(workflow1_1, workflow1_2, true);
			AssertDependencyRelationshipExists(workflow2_1, workflow2_2, true);
			AssertDependencyRelationshipExists(jobHeader1, jobHeader2, true);

			AssertEquals("Should defer postrequisite workflow", bucket1, workflow1_2.CurrentComponent);
			AssertEquals(deferTime, workflow1_2.DoNotStartBeforeDateLocal);

			AssertEquals("Should defer postreq by job link", bucket1, workflow2_2.CurrentComponent);
			AssertEquals(deferTime, workflow2_2.DoNotStartBeforeDateLocal);
			AssertEquals("Should not move postreq which was not already released", bucket2, workflow2_1.CurrentComponent);
		}

		[TestDate(2013, 12, 13)]
		public void TestDefer_ShouldDeferSelectedPrerequisites()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";
			workflow4.FH_CompletionStatement = "workflow4";

			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			workflow3.FH_FC_CurrentComponent = buffer.PK;
			workflow4.FH_FC_CurrentComponent = buffer.PK;

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow3);
			workflow1.MakePrerequisiteOf(workflow4);

			var viewModel = new DeferWorkflowViewModel(workflow1, new[] { buffer.PK });

			AssertEquals(3, viewModel.WorkflowsToDefer.Count);
			AssertEquals(workflow2, viewModel.WorkflowsToDefer[0].ProcessHeader);
			AssertEquals(workflow3, viewModel.WorkflowsToDefer[1].ProcessHeader);
			AssertEquals(workflow4, viewModel.WorkflowsToDefer[2].ProcessHeader);

			viewModel.WorkflowsToDefer[0].ActionToBeTaken = WorkflowDeferalActionList.Codes.RemovePrerequisite;
			viewModel.WorkflowsToDefer[1].ActionToBeTaken = WorkflowDeferalActionList.Codes.Defer;
			viewModel.WorkflowsToDefer[2].ActionToBeTaken = WorkflowDeferalActionList.Codes.None;

			AssertDependencyRelationshipExists(workflow1, workflow2, true);
			AssertDependencyRelationshipExists(workflow1, workflow3, true);
			AssertDependencyRelationshipExists(workflow1, workflow4, true);

			var deferTime = new ZDateTime(2014, 1, 1);
			viewModel.DoNotStartBeforeDate = deferTime;
			viewModel.Defer();

			AssertEquals(bucket, workflow1.CurrentComponent);
			AssertEquals(buffer, workflow2.CurrentComponent);
			AssertEquals(bucket, workflow3.CurrentComponent);
			AssertEquals(buffer, workflow4.CurrentComponent);

			AssertEquals(deferTime, workflow1.DoNotStartBeforeDateLocal);
			AssertNotEquals(deferTime, workflow2.DoNotStartBeforeDateLocal);
			AssertEquals(deferTime, workflow3.DoNotStartBeforeDateLocal);
			AssertNotEquals(deferTime, workflow4.DoNotStartBeforeDateLocal);

			AssertDependencyRelationshipExists(workflow1, workflow2, false);
			AssertDependencyRelationshipExists(workflow1, workflow3, true);
			AssertDependencyRelationshipExists(workflow1, workflow4, true);
		}

		public void TestDefer_ShouldNotDeferChildWorkflows_WhenChildWorkflowIsARegularWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeaderParent = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowParent = BMSTestHelper.CreateWorkflow(jobHeaderParent, "werkflow", config.Buffer);
			var jobHeaderChild = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowChild = BMSTestHelper.CreateWorkflow(jobHeaderChild, "werkflow", config.Buffer);
			workflowChild.GetOrCreateLinkToParent(workflowParent);

			Factory.Save();

			var viewModel = new DeferWorkflowViewModel(workflowParent, new[] { config.Buffer.PK });
			viewModel.Defer();

			AssertNoExceptionThrown("Should not have a constraint violation on the constraint that prevents job level workflows from having current components, since we're not attempting to defer JLWs linked to parent workflows",
				() => Factory.Save());

			CombineAssertions(() =>
			{
				AssertEquals(config.Bucket, workflowParent.CurrentComponent);
				AssertEquals(config.Buffer, workflowChild.CurrentComponent);
				AssertNoErrors(viewModel.WorkflowsHintInfo);
			});
		}

		public void TestDefer_ShouldNotDeferChildWorkflows_WhenChildWorkflowIsAJobLevelWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeaderParent = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowParent = BMSTestHelper.CreateWorkflow(jobHeaderParent, "werkflow", config.Buffer);
			var jobHeaderChild = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowChild = BMSTestHelper.CreateWorkflow(jobHeaderChild, "werkflow", config.Buffer);
			jobHeaderChild.GetOrCreateLinkToParent(workflowParent);

			Factory.Save();

			var viewModel = new DeferWorkflowViewModel(workflowParent, new[] { config.Buffer.PK });
			viewModel.Defer();

			AssertNoExceptionThrown("Should not have a constraint violation on the constraint that prevents job level workflows from having current components, since we're not attempting to defer JLWs linked to parent workflows",
				() => Factory.Save());

			CombineAssertions(() =>
			{
				AssertEquals(config.Bucket, workflowParent.CurrentComponent);
				AssertEquals(config.Buffer, workflowChild.CurrentComponent);
				AssertNoErrors(viewModel.WorkflowsHintInfo);
			});
		}

		public void TestDeferOnJobLevelWorkflow_ShouldDeferOnlyRegularNotJobLevelWorkflows_ShouldNotDeferChildWorkflows()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeaderParent = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowParent1 = BMSTestHelper.CreateWorkflowAndTask(jobHeaderParent, "werkflow", config.Buffer);
			var workflowParent2 = BMSTestHelper.CreateWorkflowAndTask(jobHeaderParent, "werkflow2", config.Buffer);

			var jobHeaderChild = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowChild = BMSTestHelper.CreateWorkflow(jobHeaderChild, "werkflow", config.Buffer);
			workflowChild.GetOrCreateLinkToParent(workflowParent1);

			Factory.Save();

			var viewModel = new DeferWorkflowViewModel(jobHeaderParent, new[] { config.Buffer.PK });
			viewModel.Defer();

			AssertNoExceptionThrown("Should not have a constraint violation on the constraint that prevents job level workflows from having current components, since we're not attempting to defer JLWs linked to parent workflows",
				() => Factory.Save());

			CombineAssertions(() =>
			{
				AssertEquals(config.Bucket, workflowParent1.CurrentComponent);
				AssertEquals(config.Bucket, workflowParent2.CurrentComponent);
				AssertEquals(config.Buffer, workflowChild.CurrentComponent);
				AssertNoErrors(viewModel.WorkflowsHintInfo);
			});
		}

		#endregion

		#region Component Transfer Event

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestDefer_ShouldLogXFREventOnWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow", config.Buffer);

			var viewModel = new DeferWorkflowViewModel(workflow, new[] { config.Buffer.PK });
			viewModel.Defer();

			AssertEquals(config.Bucket, workflow.CurrentComponent);
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.Defer, config.Buffer.PK, config.Bucket.PK, bufferPenetration: 0, bufferZone: 3);
			AssertEquals("Moved from component [buffer] (zone 3) to [bucket]. It was deferred.", log.DisplayEventReference);
			AssertNotContains("RES=", log.SL_Reference);
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestDeferWorkflow_DeferReasons()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow", config.Buffer);

			var viewModel = new DeferWorkflowViewModel(workflow, new[] { config.Buffer.PK });
			viewModel.Defer(WorkflowDeferralReasonsList.Codes.PrioritiesChanged);

			AssertEquals(config.Bucket, workflow.CurrentComponent);
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.Defer, config.Buffer.PK, config.Bucket.PK, bufferPenetration: 0, bufferZone: 3, deferReason: "PRI");
			AssertEquals("Moved from component [buffer] (zone 3) to [bucket]. It was deferred with reason code PRI.", log.DisplayEventReference);
			AssertContains("RES=PRI", log.SL_Reference);
		}

		public void TestDeferJob_WithNoWorkflowsSelected_ShouldCauseValidationError()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");

			var link1 = bucket1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = bucket2.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 1", bucket2);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 2", bucket2);

			var viewModel = new DeferWorkflowViewModel(jobHeader, new[] { bucket2.PK });
			var date = ZDateTime.Now.AddDays(1);
			viewModel.DoNotStartBeforeDate = date;
			viewModel.Validation.ValidateAll();
			AssertNoErrors(viewModel.WorkflowsHintInfo);

			var workflowsToDefer = viewModel.WorkflowsToDefer.Cast<WorkflowToDeferBusinessObject>();

			workflowsToDefer.Single(x => x.ProcessHeader.PK == workflow1.PK).ActionToBeTaken = WorkflowDeferalActionList.Codes.None;
			workflowsToDefer.Single(x => x.ProcessHeader.PK == workflow2.PK).ActionToBeTaken = WorkflowDeferalActionList.Codes.None;
			viewModel.Validation.ValidateAll();

			AssertHasError(viewModel.WorkflowsHintInfo, "Please select at least one workflow to defer.");
		}

		[TestDate(2016, 09, 20)]
		public void TestDeferJob_ShouldDeferOpenWorkflowsFromSameSourceComponentByDefault()
		{
			AssertJobDeferred(ZString.Empty, "Bucket 1", ZDateTime.Now.AddDays(1));
		}

		[TestDate(2016, 09, 20)]
		public void TestDeferJob_ShouldDeferOpenWorkflowsSelectedForDeferral()
		{
			AssertJobDeferred(WorkflowDeferalActionList.Codes.None, "Bucket 2", ZDateTime.Empty);
		}

		void AssertJobDeferred(ZString actionForWorkflow1, ZString expectedComponentNameForWorkflow1, ZDateTime expectedStartDateForWorkflow1)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3");

			var link1 = bucket1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = bucket2.PK;
			var link2 = bucket2.FromMeToOthersLinks.AddNew();
			link2.FL_FC_ComponentTo = bucket3.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 1", bucket2);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 2", bucket2);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 3", bucket3);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Closed workflow", bucket2);

			Factory.Save();

			var viewModel = new DeferWorkflowViewModel(jobHeader, new[] { bucket2.PK });
			var date = ZDateTime.Now.AddDays(1);
			viewModel.DoNotStartBeforeDate = date;

			if (!string.IsNullOrEmpty(actionForWorkflow1))
			{
				viewModel.WorkflowsToDefer.Cast<WorkflowToDeferBusinessObject>().Single(x => x.ProcessHeader.PK == workflow1.PK).ActionToBeTaken = actionForWorkflow1;
			}

			viewModel.Defer();

			AssertEquals(expectedComponentNameForWorkflow1, workflow1.CurrentComponent.FC_Name);
			AssertEquals(bucket1.FC_Name, workflow2.CurrentComponent.FC_Name);
			AssertEquals(bucket3.FC_Name, workflow3.CurrentComponent.FC_Name);
			AssertEquals(bucket2.FC_Name, workflow4.CurrentComponent.FC_Name);

			AssertEquals(expectedStartDateForWorkflow1, workflow1.DoNotStartBeforeDateLocal);
			AssertEquals(date, workflow2.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow3.DoNotStartBeforeDateLocal);
			AssertEquals(ZDateTime.Empty, workflow4.DoNotStartBeforeDateLocal);
		}

		#endregion

		#region Implementation

		void AssertDependencyRelationshipExists(ProcessHeader prereq, ProcessHeader postreq, bool relationshipExists)
		{
			var link = prereq.LinksFromMeToOthers.FirstOrDefault(l => l.FP_FH_HeaderTo == postreq.PK);

			if (relationshipExists)
			{
				AssertNotNull(string.Format("There should be a link between {0} and {1}", prereq.Code, postreq.Code), link);
			}
			else
			{
				AssertNull(string.Format("There should NOT be a link between {0} and {1}", prereq.Code, postreq.Code), link);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeferWorkflowViewModel(BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory).ProcessHeaders.AddNew(), null);
		}

		#endregion
	}
}
