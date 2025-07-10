using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ProcessHeader))]
	class ProcessHeaderBufferedItemTest : BufferedItemTestCase
	{
		#region Buffer Penetration

		[TestDate(2015, 7, 14)]
		public void TestBufferPenetration_ForQualityIterationOfWorkflowOnCCPMPlan()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CBR", "ORG");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: config.ReleaseGroup.PK);

			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 10, "UDF");
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 10, "CBR");

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			Factory.Save();
			NetworkTestCase.RunBufferPenetrationUpdater();
			BMSTestCaseWithFactory.RunCCPMAndNCNTagRules(Factory);

			var context = WorkingTimeContext.Create(config.Buffer);
			var parentWorkflowPenetration = BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory);

			CombineAssertions("Buffer penetration for workflow should use CCPM schedule", () =>
			{
				AssertEquals(2.0625m, parentWorkflowPenetration.Penetration);
				AssertEquals(projectBuffer, parentWorkflowPenetration.PenetratingBuffer);
			});

			var qualityIterationWorkflow = BMSTestCaseWithFactory.CreateQualityIteration(task1, task2);
			var penetrationForQI = BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qualityIterationWorkflow, context, Factory);
			parentWorkflowPenetration = BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory);

			CombineAssertions("Buffer penetration for Quality Iteration workflow should use penetration of its parent workflow (and therefore CCPM schedule)", () =>
			{
				AssertEquals(parentWorkflowPenetration.Penetration, penetrationForQI.Penetration);
				AssertEquals(projectBuffer, penetrationForQI.PenetratingBuffer);
			});

			var iterateFromTask = qualityIterationWorkflow.Tasks.Single(t => t.P9_Type == "UDF");
			var qcbTask = qualityIterationWorkflow.Tasks.Single(t => t.P9_Type == "CBR");

			var nestedQualityIterationWorkflow = BMSTestCaseWithFactory.CreateQualityIteration(iterateFromTask, qcbTask);
			var penetrationForNestedQI = BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(nestedQualityIterationWorkflow, context, Factory);
			parentWorkflowPenetration = BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory);

			CombineAssertions("Buffer penetration for nested Quality Iteration workflow should use penetration of its root parent workflow (and therefore CCPM schedule)", () =>
			{
				AssertEquals(parentWorkflowPenetration.Penetration, penetrationForNestedQI.Penetration);
				AssertEquals(projectBuffer, penetrationForNestedQI.PenetratingBuffer);
			});
		}

		#endregion

		#region Related Buffers

		public void TestRelatedBuffers_ShouldWalkNetworkButNotPassPostreqBuffersOfOtherBuffers()
		{
			// See eDocs of WI00060108 for diagram

			// Workflow network

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var ccWorkflow1 = CreateWorkflow(jobHeader, "ccWorkflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var ccWorkflow2 = CreateWorkflow(jobHeader, "ccWorkflow2", releaseGroupPK: config.ReleaseGroup.PK);
			var ccWorkflow3 = CreateWorkflow(jobHeader, "ccWorkflow3", releaseGroupPK: config.ReleaseGroup.PK);
			var ccWorkflow4 = CreateWorkflow(jobHeader, "ccWorkflow4", releaseGroupPK: config.ReleaseGroup.PK);

			var nonCCWorkflow1 = jobHeader.ProcessHeaders.AddNew();
			var nonCCWorkflow2 = jobHeader.ProcessHeaders.AddNew();
			var nonCCWorkflow3 = jobHeader.ProcessHeaders.AddNew();

			CreateTask(ccWorkflow1, GlbStaff.CurrentUser.GS_Code, 10 * BMConstants.WorkingHoursPerDay * 60); // 10 working days
			CreateTask(ccWorkflow2, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60); // 3 working days
			CreateTask(ccWorkflow3, GlbStaff.CurrentUser.GS_Code, 6 * BMConstants.WorkingHoursPerDay * 60); // 6 working days
			CreateTask(ccWorkflow4, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60); // 3 working days

			CreateTask(nonCCWorkflow1, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60); // 3 working days
			CreateTask(nonCCWorkflow2, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60); // 3 working days
			CreateTask(nonCCWorkflow3, GlbStaff.CurrentUser.GS_Code, 3 * BMConstants.WorkingHoursPerDay * 60); // 3 working days

			ccWorkflow1.GetOrCreateDependencyLink(ccWorkflow2);
			ccWorkflow2.GetOrCreateDependencyLink(ccWorkflow3);
			ccWorkflow3.GetOrCreateDependencyLink(ccWorkflow4);

			nonCCWorkflow1.GetOrCreateDependencyLink(nonCCWorkflow2);

			// Critical chain interception points:
			nonCCWorkflow2.GetOrCreateDependencyLink(ccWorkflow2);
			nonCCWorkflow3.GetOrCreateDependencyLink(ccWorkflow4);

			// Shape network

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var ccShape1 = CreateShape(ccWorkflow1, diagram, "CC Shape 1");
			var ccShape2 = CreateShape(ccWorkflow2, diagram, "CC Shape 2");
			var ccShape3 = CreateShape(ccWorkflow3, diagram, "CC Shape 3");
			var ccShape4 = CreateShape(ccWorkflow4, diagram, "CC Shape 4");

			var nonCCShape1 = CreateShape(nonCCWorkflow1, diagram, "Non-CC Shape 1");
			var nonCCShape2 = CreateShape(nonCCWorkflow2, diagram, "Non-CC Shape 2");
			var nonCCShape3 = CreateShape(nonCCWorkflow3, diagram, "Non-CC Shape 3");

			ccShape1.MakeVisiblePrerequisiteOf(ccShape2);
			ccShape2.MakeVisiblePrerequisiteOf(ccShape3);
			ccShape3.MakeVisiblePrerequisiteOf(ccShape4);

			nonCCShape1.MakeVisiblePrerequisiteOf(nonCCShape2);

			// Critical chain interception points:
			nonCCShape2.MakeVisiblePrerequisiteOf(ccShape2);
			nonCCShape3.MakeVisiblePrerequisiteOf(ccShape4);

			network.SwitchToScaled();
			ccShape1.Width = 1000;
			ccShape3.Width = 600;

			networkViewModel.PushAsLateAsPossible();

			Factory.Save();

			AssertEquals(true, ccShape1.IsCriticalPath);
			AssertEquals(true, ccShape2.IsCriticalPath);
			AssertEquals(true, ccShape3.IsCriticalPath);
			AssertEquals(true, ccShape4.IsCriticalPath);

			AssertEquals(false, nonCCShape1.IsCriticalPath);
			AssertEquals(false, nonCCShape2.IsCriticalPath);
			AssertEquals(false, nonCCShape3.IsCriticalPath);

			networkViewModel.SuggestAndAcceptAllBuffers();

			var buffers = network.Shapes.OfType<BMNCNBufferShape>().ToArray();
			AssertEquals(3, buffers.Length);

			var feedingBuffer2 = buffers.Single(s => s.Name == "Non-CC Shape 2 Feeding Buffer");
			var feedingBuffer3 = buffers.Single(s => s.Name == "Non-CC Shape 3 Feeding Buffer");
			var projectBuffer = buffers.Single(s => s.BufferType == BufferTypeList.Codes.Project);

			UnitTestUserNotification.Instance.AddOKAnswer();
			networkViewModel.ToggleApproval(ensureIsNowApproved: true);
			Factory.Save();

			AssertRelatedBuffers("CC items should only consider project buffer", ccWorkflow1, projectBuffer);
			AssertRelatedBuffers("CC items should only consider project buffer", ccWorkflow2, projectBuffer);
			AssertRelatedBuffers("CC items should only consider project buffer", ccWorkflow3, projectBuffer);
			AssertRelatedBuffers("CC items should only consider project buffer", ccWorkflow4, projectBuffer);

			AssertRelatedBuffers("Items preceeding feedingBuffer2 should not consider feeding buffer on another path", nonCCWorkflow1, projectBuffer, feedingBuffer2);
			AssertRelatedBuffers("Items preceeding feedingBuffer2 should not consider feeding buffer on another path", nonCCWorkflow2, projectBuffer, feedingBuffer2);
			AssertRelatedBuffers(nonCCWorkflow3, projectBuffer, feedingBuffer3);
		}

		public void TestRelatedBuffers_WhenApprovedShapeExists_WithNoBuffers()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			UnitTestUserNotification.Instance.AddOKAnswer();
			networkViewModel.ToggleApproval();

			AssertEquals("Should use operational buffer when approved shape is not buffered", config.Buffer, ((IBufferedItem)workflow).GetRelatedBuffers().Single());
		}

		public void TestRelatedBuffers_WhenApprovedShapeExists_WithBuffers()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals("Should use project buffer for approved shape", projectBuffer, ((IBufferedItem)workflow).GetRelatedBuffers().Single());
		}

		[TestDate(2015, 02, 02)]
		public void TestRelatedBuffers_WhenApprovedShapeExists_WithBuffers_ClosingWorkflowShouldRecordProjectBufferPenetrationNotOperational()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Prometheus", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Daedalus", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 1);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 1);

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-20);

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			BMSTestCaseWithFactory.RunCCPMAndNCNTagRules(Factory);

			AssertCcpmRtrTagApplied(workflow1);

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals("Should use project buffer for approved shape", projectBuffer, ((IBufferedItem)workflow1).GetRelatedBuffers().Single());

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertEquals(41.002m, workflow1.FH_BufferPenetrationPercentWhenCompleted);
		}

		public void TestRelatedBuffers_WhenApprovedShapeForJobExists_WithBuffers()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader2.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals("Should use project buffer for job's approved shape", projectBuffer, ((IBufferedItem)workflow).GetRelatedBuffers().Single());
			AssertEquals("Should use project buffer for approved", projectBuffer, ((IBufferedItem)jobHeader2).GetRelatedBuffers().Single());
		}

		public void TestRelatedBuffers_OfChildShapes()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(false);
			var jobHeader2 = CreateJobHeader<OrgHeader>(false);
			var childJobHeader = CreateJobHeader<OrgHeader>(false);
			var grandchildJobHeader = CreateJobHeader<OrgHeader>(false);

			jobHeader1.MakePrerequisiteOf(jobHeader2);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(childJobHeader, jobHeader1);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(grandchildJobHeader, childJobHeader);

			var workflow1 = CreateWorkflow(jobHeader1, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2", releaseGroupPK: config.ReleaseGroup.PK);
			var childWorkflow = CreateWorkflow(childJobHeader, "childWorkflow", releaseGroupPK: config.ReleaseGroup.PK);
			var grandchildWorkflow = CreateWorkflow(grandchildJobHeader, "grandchildWorkflow", releaseGroupPK: config.ReleaseGroup.PK);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var task3 = CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task4 = CreateTask(grandchildWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			var diagramShape = CreateDiagram(CreateJobHeader<OrgHeader>());
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(jobHeader1, diagram);
			var subDiagram2 = CreateShape(jobHeader2, diagram);
			var childSubDiagram = CreateShape(childJobHeader, subDiagram1);
			var grandchildSubDiagram = CreateShape(grandchildJobHeader, childSubDiagram);

			subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2);

			UnitTestUserNotification.Instance.AddOKAnswer(); // We need to save the diagram first.
			UnitTestUserNotification.Instance.AddOKAnswer(); // Back-in-time arrows are OK to exist in this diagram.

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval(ensureIsNowApproved: true);

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Type == BufferType.Project);
			var feedingBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Type == BufferType.Feeding);

			AssertRelatedBuffers(jobHeader1, projectBuffer, feedingBuffer);
			AssertRelatedBuffers(jobHeader2, projectBuffer);
			AssertRelatedBuffers(childJobHeader, projectBuffer, feedingBuffer);
			AssertRelatedBuffers("Children at n-levels should affect buffers of their parents", grandchildJobHeader, projectBuffer, feedingBuffer);
		}

		#endregion

		#region IBufferedItem Properties

		[TestDate(2014, 7, 15, 15, 16, 0)]
		public void TestStartableTime_WhenScheduledToStart()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow;
			var workflow = jobHeader.ProcessHeaders[0];

			workflow.FH_ReleaseDateTime = new ZDateTime(2014, 1, 1);
			AssertEquals(new ZDateTime(2014, 1, 1), ((IBufferedItem)workflow).StartableTime);

			var rtrTag = TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(Factory));
			rtrTag.Definition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			workflow.AddTag(rtrTag);

			Factory.Save();

			AssertEquals(new ZDateTime(2014, 1, 1), ((IBufferedItem)workflow).StartableTime);

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			AssertEquals(new ZDateTime(2014, 7, 15, 15, 16, 0), ((IBufferedItem)workflow).StartableTime);
		}

		public void TestRemainingEstimateMinutes_ShouldIncludeEstimatesOfChildren()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(false);
			var jobHeader2 = CreateJobHeader<OrgHeader>(false);
			var jobHeader3 = CreateJobHeader<OrgHeader>(false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "David", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Andrew", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "East", releaseGroupPK: config.ReleaseGroup.PK);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var task3 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(jobHeader2, jobHeader1);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(jobHeader3, jobHeader2);

			AssertEquals(90, ((IBufferedItem)workflow1).RemainingEstimateInMinutes);
			AssertEquals(90, ((IBufferedItem)workflow2).RemainingEstimateInMinutes);
			AssertEquals(90, ((IBufferedItem)workflow3).RemainingEstimateInMinutes);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(jobHeader1, diagram);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			network.SwitchToScaled();
			viewModel.SuggestAndAcceptAllBuffers();
			viewModel.ToggleApproval();

			AssertEquals(270, ((IBufferedItem)workflow1).RemainingEstimateInMinutes);
			AssertEquals(270, ((IBufferedItem)workflow2).RemainingEstimateInMinutes);
			AssertEquals(270, ((IBufferedItem)workflow3).RemainingEstimateInMinutes);
		}

		public void TestPlannedDuration_ShouldUseShapeDuration()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var workflow1 = jobHeader1.ProcessHeaders[0];
			var workflow2 = jobHeader1.ProcessHeaders[0];
			var workflow3 = jobHeader1.ProcessHeaders[0];

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(jobHeader2, jobHeader1);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(jobHeader3, jobHeader2);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(jobHeader1, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			network.SwitchToScaled();
			viewModel.SuggestAndAcceptAllBuffers();
			viewModel.ToggleApproval();
			shape.ExplicitDurationMinutes = 1000;

			AssertEquals(1000, ((IBufferedItem)workflow1).PlannedDurationInMinutes);
			AssertEquals(1000, ((IBufferedItem)workflow2).PlannedDurationInMinutes);
			AssertEquals(1000, ((IBufferedItem)workflow3).PlannedDurationInMinutes);
		}

		#endregion

		#region BufferedItemTestCase Overrides

		protected override IBufferedItem GetBufferedItem()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_ReleaseDateTime = GetExpectedStartableTime();
			workflow.FH_PlannedDurationInMinutes = 222;

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			return workflow;
		}

		protected override IEnumerable<IBuffer> GetExpectedRelatedBuffers()
		{
			yield return config.Buffer;
		}

		protected override ZDateTime GetExpectedStartableTime()
		{
			return new ZDateTime(2014, 7, 15);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedPlannedDurationInMinutes()
		{
			return 222;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedRemainingEstimateInMinutes()
		{
			return 90;
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
			base.SetUp();
		}

		SchematicTestConfig config;

		#endregion
	}
}
