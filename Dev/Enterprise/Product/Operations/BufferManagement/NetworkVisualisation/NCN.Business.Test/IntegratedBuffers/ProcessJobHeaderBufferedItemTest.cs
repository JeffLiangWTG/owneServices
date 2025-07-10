using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ProcessJobHeader))]
	class ProcessJobHeaderBufferedItemTest : BufferedItemTestCase
	{
		public void TestRelatedBuffers_WhenApprovedShapeExists_WithNoBuffers()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals("Should find no buffer when approved shape is not buffered", 0, ((IBufferedItem)jobHeader2).GetRelatedBuffers().Count);
		}

		public void TestRelatedBuffers_WhenApprovedShapeExists_WithBuffers()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals("Should use project buffer for approved shape", projectBuffer, ((IBufferedItem)jobHeader2).GetRelatedBuffers().Single());
		}

		[TestDate(2014, 7, 15, 15, 16, 0)]
		public void TestStartableTime_WhenScheduledToStart()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			AssertEquals(default(DateTime), ((IBufferedItem)jobHeader1).StartableTime);

			var rtrTag = TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(Factory));
			rtrTag.Definition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			jobHeader2.AddTag(rtrTag);

			Factory.Save();

			AssertEquals(default(DateTime), ((IBufferedItem)jobHeader2).StartableTime);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			AssertEquals(new ZDateTime(2014, 7, 15, 15, 16, 0), ((IBufferedItem)jobHeader2).StartableTime);
		}

		[TestDate(2025, 01, 29, 1, 2, 3)]
		public void TestApprovedShapeBufferPenetrationServiceDontCrashInLooping()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = VisualBoardsTestHelper.CreateBuffer(system);
			var jobHeaderInDiagram = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowInDiagram = BMSTestHelper.CreateWorkflow(jobHeaderInDiagram, "In diagram", buffer);

			var jobHeaderInLoop = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowInLoop = BMSTestHelper.CreateWorkflow(jobHeaderInLoop, "In Loop", buffer);
			BMSTestHelper.CreateParentChildLink(workflowInLoop, jobHeaderInLoop);

			var diagramShape = CreateDiagram(jobHeaderInDiagram);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape = CreateShape(workflowInDiagram, diagram);
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			{
				var service = new ApprovedShapeBufferPenetrationService(workflowInDiagram.WrapWithEnumerable().ToArray());

				var approvedShape = service.GetApprovedShapeDetails(workflowInDiagram);
				AssertNotNull(approvedShape);

				var noShape = service.GetApprovedShapeDetails(workflowInLoop);
				AssertNull(noShape);
			}
		}

		protected override IBufferedItem GetBufferedItem()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_ReleaseDateTime = GetExpectedStartableTime();

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			return jobHeader;
		}

		protected override IEnumerable<IBuffer> GetExpectedRelatedBuffers()
		{
			yield break;
		}

		protected override ZDateTime GetExpectedStartableTime()
		{
			return default(DateTime);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedPlannedDurationInMinutes()
		{
			return 180;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedRemainingEstimateInMinutes()
		{
			return 90;
		}

		protected override void SetUp()
		{
			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			base.SetUp();
		}

		SchematicTestConfig config;
	}
}
