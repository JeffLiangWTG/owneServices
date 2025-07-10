using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	public class BMNCNShapeFloatTest : NetworkTestCase
	{
		[ExpectNoExceptions]
		public void TestInvalidNetworksDontStackOverflow()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader, name: "New Friends Can Not?", isScaled: true);

			var link = CreateShape(diagram, "Link");
			var shape1 = CreateShape(diagram, "Wisdom");
			var shape2 = CreateShape(diagram, "Power");
			var shape3 = CreateShape(diagram, "Courage");

			link.MakeVisiblePrerequisiteOf(shape1, diagram);
			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			shape3.MakeVisiblePrerequisiteOf(shape1, diagram);

			var network = CreateNetwork(diagram);
			network.Refresh(RefreshType.Saving);

			AssertNotNull(network.DiagramEntity.Schedule);

			network.SwitchToScaled();
			network.Refresh(RefreshType.Saving);

			AssertNotNull("Also, testing we didn't stack overflow.", shape1.AsEntity(network).Schedule);
		}

		public void TestNetworkIncludingBuffers_ShouldIgnoreBufferBranch()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);

			network.CreateRelationship(shape1, shape3);
			network.CreateRelationship(shape2, shape3);

			shape1.X = shape2.X = 0;
			shape3.X = 200;

			shape2.Y = 100;

			shape1.Width = shape3.Width = 200;
			shape2.Width = 100; // Should have float of 1 resolution increment

			network.Refresh(RefreshType.Saving, shape2);

			AssertMatch(PlannedDuration("2 days", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 day", "8 hours"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("2 days", "none"), shape3.AdditionalDetail);

			AssertEquals(3, network.Entities.Count);

			new SuggestBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(5, network.Entities.Count);

			var buffer = network.Shapes.Single(s => s.IsBufferShape && s.Name.EndsWith("Feeding Buffer"));
			network.Refresh(RefreshType.Saving, shape2);

			AssertMatch(PlannedDuration("2 days", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 day", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("2 days", "none"), shape3.AdditionalDetail);
		}

		public void TestStraightLine()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow5 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);
			var shape5 = CreateShape(workflow5, jobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);
			shape4.MakeVisiblePrerequisiteOf(shape5);

			root.IsScaled = true;
			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape5.AdditionalDetail);

			AssertEquals(true, shape1.Schedule.IsCriticalPath);
			AssertEquals(true, shape2.Schedule.IsCriticalPath);
			AssertEquals(true, shape3.Schedule.IsCriticalPath);
			AssertEquals(true, shape4.Schedule.IsCriticalPath);
			AssertEquals(true, shape5.Schedule.IsCriticalPath);
		}

		public void TestTwoCriticalPaths()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow5 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var workflow1to3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var diagram = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(diagram);
			var jobShape = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);
			var shape5 = CreateShape(workflow5, jobShape, explicitDurationMinutes: 60);

			var shape1to3 = CreateShape(workflow1to3, jobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);
			shape4.MakeVisiblePrerequisiteOf(shape5);

			shape1.MakeVisiblePrerequisiteOf(shape1to3);
			shape1to3.MakeVisiblePrerequisiteOf(shape3);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape5.AdditionalDetail);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1to3.AdditionalDetail);
		}

		public void TestOneLooseNode()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow5 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var workflow1to4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);
			var shape5 = CreateShape(workflow5, jobShape, explicitDurationMinutes: 60);

			var shape1to4 = CreateShape(workflow1to4, jobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);
			shape4.MakeVisiblePrerequisiteOf(shape5);

			shape1.MakeVisiblePrerequisiteOf(shape1to4);
			shape1to4.MakeVisiblePrerequisiteOf(shape4);

			root.IsScaled = true;
			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape5.AdditionalDetail);

			AssertMatch(PlannedDuration("1 hour", "1 hour"), shape1to4.AdditionalDetail);
		}

		public void TestMultipleCPEndPoints()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow5 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var extraStartWorkflow = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var extraEndWorkflow = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);
			var shape5 = CreateShape(workflow5, jobShape, explicitDurationMinutes: 60);

			var extraStartShape = CreateShape(extraStartWorkflow, jobShape, explicitDurationMinutes: 60);
			var extraEndShape = CreateShape(extraEndWorkflow, jobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);
			shape4.MakeVisiblePrerequisiteOf(shape5);

			extraStartShape.MakeVisiblePrerequisiteOf(shape2);
			shape4.MakeVisiblePrerequisiteOf(extraEndShape);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape5.AdditionalDetail);

			AssertMatch(PlannedDuration("1 hour", "none"), extraStartShape.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), extraEndShape.AdditionalDetail);
		}

		public void TestMultipleLooseEndPoints()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow5 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var extraStartWorkflow = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var extraEndWorkflow = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);
			var shape5 = CreateShape(workflow5, jobShape, explicitDurationMinutes: 60);

			var extraStartShape = CreateShape(extraStartWorkflow, jobShape, explicitDurationMinutes: 60);
			var extraEndShape = CreateShape(extraEndWorkflow, jobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);
			shape4.MakeVisiblePrerequisiteOf(shape5);

			extraStartShape.MakeVisiblePrerequisiteOf(shape2);
			shape3.MakeVisiblePrerequisiteOf(extraEndShape);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape5.AdditionalDetail);

			AssertMatch(PlannedDuration("1 hour", "none"), extraStartShape.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "1 hour"), extraEndShape.AdditionalDetail);
		}

		public void TestChildShapeWithLeaves()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var childJobHeader = CreateJobHeader<OrgHeader>();
			var cj_workflow1 = childJobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var cj_workflow2 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);

			var jobDiagramShape = CreateDiagram(jobHeader);
			var jobNetwork = CreateNetwork(jobDiagramShape);
			var jobDiagram = jobNetwork.DiagramEntity;
			var childJobShape = CreateShape(childJobHeader, jobDiagram);

			var shape1 = CreateShape(workflow1, jobDiagram, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobDiagram, explicitDurationMinutes: 60);

			var cj_shape1 = CreateShape(cj_workflow1, childJobShape, explicitDurationMinutes: 60);
			var cj_shape2 = CreateShape(cj_workflow2, childJobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(childJobShape);
			childJobShape.MakeVisiblePrerequisiteOf(shape2);
			cj_shape1.MakeVisiblePrerequisiteOf(cj_shape2);

			jobDiagram.Shape.IsScaled = true;
			jobNetwork.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape2.AdditionalDetail);
		}

		public void TestChildShapeWithLeaves_EndPointsDetermined()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var childJobHeader = CreateJobHeader<OrgHeader>();
			var cj_workflow1 = childJobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var cj_workflow2 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);
			var cj_workflow3 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);
			var cj_workflow4 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var childJobShape = CreateShape(childJobHeader, jobShape);

			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);

			var cj_shape1 = CreateShape(cj_workflow1, childJobShape, explicitDurationMinutes: 60);
			var cj_shape2 = CreateShape(cj_workflow2, childJobShape, explicitDurationMinutes: 60);
			var cj_shape3 = CreateShape(cj_workflow3, childJobShape, explicitDurationMinutes: 60);
			var cj_shape4 = CreateShape(cj_workflow4, childJobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(cj_shape2);
			cj_shape3.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);

			cj_shape1.MakeVisiblePrerequisiteOf(cj_shape2);
			cj_shape2.MakeVisiblePrerequisiteOf(cj_shape3);
			cj_shape3.MakeVisiblePrerequisiteOf(cj_shape4);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "1 hour"), cj_shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "1 hour"), cj_shape4.AdditionalDetail);

			AssertEquals(false, cj_shape1.Schedule.IsCriticalPath);
			AssertEquals(true, cj_shape2.Schedule.IsCriticalPath);
			AssertEquals(true, cj_shape3.Schedule.IsCriticalPath);
			AssertEquals("Even though it has no float, this isn't the longest path", false, cj_shape4.Schedule.IsCriticalPath);
		}

		public void TestChildShapeWithLeaves_JumpsOverChildShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var childJobHeader = CreateJobHeader<OrgHeader>();
			var cj_workflow1 = childJobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var cj_workflow2 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);
			var cj_workflow3 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);
			var cj_workflow4 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var childJobShape = CreateShape(childJobHeader, jobShape);

			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);

			var cj_shape1 = CreateShape(cj_workflow1, childJobShape, explicitDurationMinutes: 60);
			var cj_shape2 = CreateShape(cj_workflow2, childJobShape, explicitDurationMinutes: 60);
			var cj_shape3 = CreateShape(cj_workflow3, childJobShape, explicitDurationMinutes: 60);
			var cj_shape4 = CreateShape(cj_workflow4, childJobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);

			shape1.MakeVisiblePrerequisiteOf(cj_shape2);
			cj_shape3.MakeVisiblePrerequisiteOf(shape3);

			cj_shape1.MakeVisiblePrerequisiteOf(cj_shape2);
			cj_shape2.MakeVisiblePrerequisiteOf(cj_shape3);
			cj_shape3.MakeVisiblePrerequisiteOf(cj_shape4);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "1 hour"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "1 hour"), cj_shape4.AdditionalDetail);
		}

		public void TestChildShapesLinkWithOverride()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var childJobHeader = CreateJobHeader<OrgHeader>();
			var cj_workflow1 = childJobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var cj_workflow2 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);
			var cj_workflow3 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);
			var cj_workflow4 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var childJobShape = CreateShape(childJobHeader, jobShape);

			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);

			var cj_shape1 = CreateShape(cj_workflow1, childJobShape, explicitDurationMinutes: 60);
			var cj_shape2 = CreateShape(cj_workflow2, childJobShape, explicitDurationMinutes: 60);
			var cj_shape3 = CreateShape(cj_workflow3, childJobShape, explicitDurationMinutes: 60);
			var cj_shape4 = CreateShape(cj_workflow4, childJobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);

			shape2.MakeVisiblePrerequisiteOf(childJobShape);
			shape2.MakeVisiblePrerequisiteOf(cj_shape2);
			childJobShape.MakeVisiblePrerequisiteOf(shape4);

			cj_shape1.MakeVisiblePrerequisiteOf(cj_shape2);
			cj_shape2.MakeVisiblePrerequisiteOf(cj_shape3);
			cj_shape3.MakeVisiblePrerequisiteOf(cj_shape4);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "3 hours"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape4.AdditionalDetail);
		}

		public void TestUpdatesOnMinutesChange()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow5 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var workflow1to4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);
			var shape5 = CreateShape(workflow5, jobShape, explicitDurationMinutes: 60);

			var shape1to4 = CreateShape(workflow1to4, jobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);
			shape4.MakeVisiblePrerequisiteOf(shape5);

			shape1.MakeVisiblePrerequisiteOf(shape1to4);
			shape1to4.MakeVisiblePrerequisiteOf(shape4);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape5.AdditionalDetail);

			AssertMatch(PlannedDuration("1 hour", "1 hour"), shape1to4.AdditionalDetail);

			shape1to4.ExplicitDurationMinutes = 120;
			network.Refresh(RefreshType.Saving, shape1to4);

			AssertMatch(PlannedDuration("2 hours", "none"), shape1to4.AdditionalDetail);
		}

		public void TestChildShapeLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);

			var childJobHeader = CreateJobHeader<OrgHeader>();
			var cj_workflow1 = childJobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var cj_workflow2 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);
			var cj_workflow3 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);
			var cj_workflow4 = Create1HourWorkflow(childJobHeader, GlbStaff.CurrentUser.GS_Code);

			var root = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var childJobShape = CreateShape(childJobHeader, jobShape);

			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);

			var cj_shape1 = CreateShape(cj_workflow1, childJobShape, explicitDurationMinutes: 60);
			var cj_shape2 = CreateShape(cj_workflow2, childJobShape, explicitDurationMinutes: 60);
			var cj_shape3 = CreateShape(cj_workflow3, childJobShape, explicitDurationMinutes: 60);
			var cj_shape4 = CreateShape(cj_workflow4, childJobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);

			shape1.MakeVisiblePrerequisiteOf(childJobShape);
			childJobShape.MakeVisiblePrerequisiteOf(shape3);

			cj_shape1.MakeVisiblePrerequisiteOf(cj_shape2);
			cj_shape2.MakeVisiblePrerequisiteOf(cj_shape3);
			cj_shape3.MakeVisiblePrerequisiteOf(cj_shape4);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "3 hours"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), cj_shape4.AdditionalDetail);

			AssertEquals(false, shape2.Schedule.IsCriticalPath);
			AssertEquals(true, cj_shape1.Schedule.IsCriticalPath);
		}

		public void TestMultiLevelChildShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.Name = "job";
			var workflow1 = jobHeader.ProcessHeaders[0];
			CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			workflow1.Name = "W1";
			var workflow2 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			workflow2.Name = "W2";
			var workflow3 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			workflow3.Name = "W3";
			var workflow4 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			workflow4.Name = "W4";
			var workflow5 = Create1HourWorkflow(jobHeader, GlbStaff.CurrentUser.GS_Code);
			workflow5.Name = "W5";

			var ch_jobHeader = CreateJobHeader<OrgHeader>();
			ch_jobHeader.Name = "ch_job";
			var ch_ch_jobHeader = CreateJobHeader<OrgHeader>();
			ch_ch_jobHeader.Name = "ch_ch_job";
			var ch_ch_workflow = ch_ch_jobHeader.ProcessHeaders[0];
			CreateTask(ch_ch_workflow, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			ch_ch_workflow.Name = "ch_ch_workflow";

			var root = CreateDiagram(jobHeader, isScaled: true);
			var network = CreateNetwork(root);
			var jobShape = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, jobShape, explicitDurationMinutes: 60);
			var shape2 = CreateShape(workflow2, jobShape, explicitDurationMinutes: 60);
			var shape3 = CreateShape(workflow3, jobShape, explicitDurationMinutes: 60);
			var shape4 = CreateShape(workflow4, jobShape, explicitDurationMinutes: 60);
			var shape5 = CreateShape(workflow5, jobShape, explicitDurationMinutes: 60);

			var ch_jobShape = CreateShape(ch_jobHeader, jobShape);
			var ch_ch_jobShape = CreateShape(ch_ch_jobHeader, ch_jobShape);
			var ch_ch_shape = CreateShape(ch_ch_workflow, ch_ch_jobShape, explicitDurationMinutes: 60);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);
			shape3.MakeVisiblePrerequisiteOf(shape4);
			shape4.MakeVisiblePrerequisiteOf(shape5);

			shape1.MakeVisiblePrerequisiteOf(ch_jobShape);
			ch_jobShape.MakeVisiblePrerequisiteOf(shape4);

			network.Refresh(RefreshType.Saving);

			AssertMatch(PlannedDuration("1 hour", "none"), shape1.AdditionalDetail);
			AssertEquals(true, shape1.Schedule.IsCriticalPath);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape3.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape4.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape5.AdditionalDetail);
			AssertEquals(true, shape5.Schedule.IsCriticalPath);

			AssertMatch(PlannedDuration("1 hour", "1 hour"), ch_ch_shape.AdditionalDetail);
			AssertEquals(false, ch_ch_shape.Schedule.IsCriticalPath);
		}

		public void TestCrossHierarchicalLinksBetweenJobs_ShouldNotNegateEachOther()
		{
			var superJob = CreateJobHeader<OrgHeader>();

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			CreateTask(workflow1_1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow1_2 = Create1HourWorkflow(jobHeader1, GlbStaff.CurrentUser.GS_Code);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader1.ProcessHeaders[0];
			CreateTask(workflow2_1, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);
			var workflow2_2 = Create1HourWorkflow(jobHeader2, GlbStaff.CurrentUser.GS_Code);

			var superJobDiagramShape = CreateDiagram(superJob, isScaled: true);
			var network = CreateNetwork(superJobDiagramShape);
			var superJobShape = network.DiagramEntity;

			var jobShape1 = CreateShape(jobHeader1, superJobShape);
			var shape1_1 = CreateShape(workflow1_1, jobShape1, explicitDurationMinutes: 60);
			var shape1_2 = CreateShape(workflow1_2, jobShape1, explicitDurationMinutes: 60);

			var jobShape2 = CreateShape(jobHeader2, superJobShape);
			var shape2_1 = CreateShape(workflow2_1, jobShape2, explicitDurationMinutes: 60);
			var shape2_2 = CreateShape(workflow2_2, jobShape2, explicitDurationMinutes: 60);

			shape1_1.MakeVisiblePrerequisiteOf(shape1_2);
			shape2_1.MakeVisiblePrerequisiteOf(shape2_2);

			var link1 = (ILink)shape1_1.MakeVisiblePrerequisiteOf(jobShape2);
			var link2 = (ILink)jobShape1.MakeVisiblePrerequisiteOf(shape2_2);

			var targetEntity = shape2_2;
			var targetShape = targetEntity.AsShape();

			network.Refresh(RefreshType.Saving);

			CombineAssertions("Since neither of the links are more specific than the other (one goes from workflow to job, and the other goes from job to workflow) all links are relevant", () =>
			{
				AssertEquals("link1", true, link1.IsApplicableToEntity(targetShape, new BMNCNShapeDescendantsStrategy()));
				AssertEquals("link2", true, link2.IsApplicableToEntity(targetShape, new BMNCNShapeDescendantsStrategy()));

				AssertEquals("link1 (NetworkAttachment version)", true, network.Entities.GetInstance(link1).IsApplicableToEntity(targetEntity, new ShapeNetworkEntityDescendantsStrategy()));
				AssertEquals("link2 (NetworkAttachment version)", true, network.Entities.GetInstance(link2).IsApplicableToEntity(targetEntity, new ShapeNetworkEntityDescendantsStrategy()));
			});

			AssertMatch(PlannedDuration("1 hour", "none"), shape1_1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape1_2.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2_1.AdditionalDetail);
			AssertMatch(PlannedDuration("1 hour", "none"), shape2_2.AdditionalDetail);
		}

		#region Implementation

		static Regex PlannedDuration(string duration, string cpfloat)
		{
			return new Regex(string.Format("Planned Dur.: {0}{1}.*Float: {2}", duration, System.Environment.NewLine, cpfloat));
		}

		#endregion
	}
}
