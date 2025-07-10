using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNShape))]
	[TestDate(2014, 10, 31)]
	class BMNCNShapeBufferedItemTest : BufferedItemTestCase
	{
		public void TestRelatedBuffers_PrereqsOfFeedingChainEndpoint()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1_cc = CreateWorkflow(jobHeader, "workflow1_cc", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2_cc = CreateWorkflow(jobHeader, "workflow2_cc");
			var workflow3_cc = CreateWorkflow(jobHeader, "workflow3_cc");
			var workflow4_cc = CreateWorkflow(jobHeader, "workflow4_cc");
			var workflow5_nonCC = CreateWorkflow(jobHeader, "workflow5_nonCC");
			var workflow6_nonCC = CreateWorkflow(jobHeader, "workflow6_nonCC");
			var workflow7_nonCC = CreateWorkflow(jobHeader, "workflow7_nonCC");
			var workflow8_nonCC = CreateWorkflow(jobHeader, "workflow8_nonCC");

			CreateTask(workflow1_cc, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow2_cc, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow3_cc, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow4_cc, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow5_nonCC, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow6_nonCC, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow7_nonCC, GlbStaff.CurrentUser.GS_Code, 60);
			CreateTask(workflow8_nonCC, GlbStaff.CurrentUser.GS_Code, 60);

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1_cc = CreateShape(workflow1_cc, diagram, "shape1_cc");
			var shape2_cc = CreateShape(workflow2_cc, diagram, "shape2_cc");
			var shape3_cc = CreateShape(workflow3_cc, diagram, "shape3_cc");
			var shape4_cc = CreateShape(workflow4_cc, diagram, "shape4_cc");

			var shape5_nonCC = CreateShape(workflow5_nonCC, diagram, "shape5_nonCC");
			var shape6_nonCC = CreateShape(workflow6_nonCC, diagram, "shape6_nonCC");
			var shape7_nonCC = CreateShape(workflow7_nonCC, diagram, "shape7_nonCC");
			var shape8_nonCC = CreateShape(workflow8_nonCC, diagram, "shape8_nonCC");

			// 1 --------> 2 -> 3 --------> 4 ||
			// 5 -> 6 -||> 2
			//                  7 -> 8 -||> 4

			// CC chain
			shape1_cc.MakeVisiblePrerequisiteOf(shape2_cc);
			shape2_cc.MakeVisiblePrerequisiteOf(shape3_cc);
			shape3_cc.MakeVisiblePrerequisiteOf(shape4_cc);

			// Feeding chain 1 hits CC at #2
			shape5_nonCC.MakeVisiblePrerequisiteOf(shape6_nonCC);
			shape6_nonCC.MakeVisiblePrerequisiteOf(shape2_cc);

			// Feeding chain 2 hits CC at #4
			shape7_nonCC.MakeVisiblePrerequisiteOf(shape8_nonCC);
			shape8_nonCC.MakeVisiblePrerequisiteOf(shape4_cc);

			network.SwitchToScaled();

			shape1_cc.Width = 1000;
			shape2_cc.Width = 1000;
			shape3_cc.Width = 1000;
			shape4_cc.Width = 1000;
			shape5_nonCC.Width = 100;
			shape6_nonCC.Width = 100;
			shape7_nonCC.Width = 100;
			shape8_nonCC.Width = 100;

			networkViewModel.PushAsLateAsPossible();
			networkViewModel.SuggestAndAcceptAllBuffers();

			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Type == BufferType.Project);
			var feedingBuffer1 = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Name == "shape6_nonCC Feeding Buffer");
			var feedingBuffer2 = network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Name == "shape8_nonCC Feeding Buffer");

			var dodgyRelatedBufferLink = Factory.New<BMNCNAttachment>();
			dodgyRelatedBufferLink.BNA_Type = AttachmentTypeList.Codes.RelatedBuffer;
			dodgyRelatedBufferLink.BNA_BNS_Owner = feedingBuffer2.PK;
			dodgyRelatedBufferLink.BNA_BNS_ToShape = shape6_nonCC.PK;

			networkViewModel.ToggleApproval();
			networkViewModel.ToggleApproval();
			networkViewModel.ToggleApproval();

			AssertRelatedBuffers(shape1_cc, projectBuffer);
			AssertRelatedBuffers(shape2_cc, projectBuffer);
			AssertRelatedBuffers(shape3_cc, projectBuffer);
			AssertRelatedBuffers(shape4_cc, projectBuffer);

			AssertRelatedBuffers(shape5_nonCC, projectBuffer, feedingBuffer1);
			AssertRelatedBuffers("Should remove dodgy link that is no longer relevant", shape6_nonCC, projectBuffer, feedingBuffer1);

			AssertRelatedBuffers(shape7_nonCC, projectBuffer, feedingBuffer2);
			AssertRelatedBuffers(shape8_nonCC, projectBuffer, feedingBuffer2);
		}

		public void TestRelatedBuffers_WhenParallelPathsExistToProjectBuffer()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddHours(-12);

			var workflow1 = CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = CreateWorkflow(jobHeader, "workflow5");

			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);
			CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);
			CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);
			CreateTask(workflow5, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3);

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);
			var shape3 = CreateShape(workflow3, diagram);
			var shape4 = CreateShape(workflow4, diagram);
			var shape5 = CreateShape(workflow5, diagram);

			// 1 -> 2 -> 4 -> 5
			//   -> 3 -----|> 5

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape1.MakeVisiblePrerequisiteOf(shape3);
			shape2.MakeVisiblePrerequisiteOf(shape4);
			shape4.MakeVisiblePrerequisiteOf(shape5);
			shape3.MakeVisiblePrerequisiteOf(shape5);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var buffers = network.Shapes.OfType<BMNCNBufferShape>().ToArray();
			AssertEquals(2, buffers.Length);
			var projectBuffer = buffers.Single(b => b.Type == BufferType.Project);
			var feedingBuffer = buffers.Single(b => b.Type == BufferType.Feeding);

			AssertRelatedBuffers(shape1, projectBuffer, feedingBuffer);
			AssertRelatedBuffers(shape2, projectBuffer);
			AssertRelatedBuffers(shape3, projectBuffer, feedingBuffer);
			AssertRelatedBuffers(shape4, projectBuffer);
			AssertRelatedBuffers(shape5, projectBuffer);

			AssertEquals(0.25m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(shape1.Shape, projectBuffer, WorkingTimeContext.Create(diagram.Shape), Factory).Penetration);
			AssertBufferPenetration("Shape on the CC should penetrate both downstream buffers since there are parallel paths which means both buffers are considered 'starting points' for buffer penetration.", shape1.Shape, Tuple.Create(projectBuffer, 0.25m), Tuple.Create(feedingBuffer, 1.0m));
		}

		protected override IBufferedItem GetBufferedItem()
		{
			return shape;
		}

		protected override IEnumerable<IBuffer> GetExpectedRelatedBuffers()
		{
			yield return projectBuffer;
		}

		protected override ZDateTime GetExpectedStartableTime()
		{
			return new ZDateTime(2014, 7, 14);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedPlannedDurationInMinutes()
		{
			return 480; // 8 hours, or STD est rounded up to diagram resolution increment
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected override int GetExpectedRemainingEstimateInMinutes()
		{
			return 90;
		}

		SchematicTestConfig config;
		BMNCNShape shape;
		BMNCNBufferShape projectBuffer;

		protected override void SetUp()
		{
			BMSTestHelper.EnableBMSInRegistry();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Throatwobbler Mangrove", releaseDateTime: new ZDateTime(2014, 7, 16), releaseGroupPK: config.ReleaseGroup.PK);

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var diagram = CreateDiagram(jobHeader);
			shape = CreateShape(workflow, diagram);
			diagram.ProcessHeader.FH_DoNotStartBeforeDate = GetExpectedStartableTime();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			base.SetUp();
		}
	}
}
