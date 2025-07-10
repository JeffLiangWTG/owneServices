using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(SuggestBufferAction))]
	class SuggestBufferActionTest : JobNetworkActionTestCase<SuggestBufferAction>
	{
		protected override void TestExecuteCore()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			AssertEquals(0, network.Entities.Count);

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);
			var shape4 = networkViewModel.CreateNewShape(diagram);

			AssertEquals(false, ((INetworkEntity)shape1).EntityState.HasFlag(EntityState.Inactive));
			AssertEquals(false, ((INetworkEntity)shape2).EntityState.HasFlag(EntityState.Inactive));
			AssertEquals(false, ((INetworkEntity)shape3).EntityState.HasFlag(EntityState.Inactive));
			AssertEquals(false, ((INetworkEntity)shape4).EntityState.HasFlag(EntityState.Inactive));

			network.CreateRelationship(shape1, shape2);
			network.CreateRelationship(shape1, shape3);
			network.CreateRelationship(shape2, shape4);
			network.CreateRelationship(shape3, shape4);

			network.SwitchToScaled();

			SetShapeOffset(shape1, diagram, 0, 50);
			SetShapeOffset(shape2, diagram, 300, 50);
			SetShapeOffset(shape3, diagram, 300, 100);
			SetShapeOffset(shape4, diagram, 700, 50);

			SetShapeSize(shape1, diagram, 300, 100);
			SetShapeSize(shape2, diagram, 300, 100);
			SetShapeSize(shape3, diagram, 300, 100);
			SetShapeSize(shape4, diagram, 300, 100);

			shape2.Width = 400;
			network.Refresh(RefreshType.RedrawDiagram);

			AssertEquals(3 * 60 * BMConstants.WorkingHoursPerDay, shape1.ExplicitDurationMinutes);
			AssertEquals(4 * 60 * BMConstants.WorkingHoursPerDay, shape2.ExplicitDurationMinutes);
			AssertEquals(3 * 60 * BMConstants.WorkingHoursPerDay, shape3.ExplicitDurationMinutes);
			AssertEquals(3 * 60 * BMConstants.WorkingHoursPerDay, shape4.ExplicitDurationMinutes);

			AssertEquals(true, shape1.IsOnCriticalPath);
			AssertEquals(true, shape2.IsOnCriticalPath);
			AssertEquals("shape3 should not be on critical path since path through shape2 is longer in duration", false, shape3.IsOnCriticalPath);
			AssertEquals(true, shape4.IsOnCriticalPath);

			AssertEquals(4, network.Entities.Count);

			var refreshed = false;
			network.Refreshed += (_, x_) => refreshed = true;

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(true, refreshed);
			AssertEquals(6, network.Entities.Count);

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single(s => s.IsBufferShape && s.Name == "Project Buffer");
			AssertEquals(BufferType.Project, ((IBuffer)buffer).Type);

			AssertShapeOffset("Should place buffer directly after shape4 (x-axis), and vertically aligned to its centre (y-axis)", buffer.AsEntity(network), diagram, 1000, 65);
			AssertEquals(500d, buffer.AsEntity(network).Width);
			AssertEquals(70d, buffer.AsEntity(network).Height);
			AssertEquals(5 * 60 * BMConstants.WorkingHoursPerDay, buffer.ExplicitDurationMinutes);
			AssertEquals(true, buffer.AsEntity(network).EntityState.HasFlag(EntityState.Inactive));

			AssertEquals(1, buffer.DependencyAttachments.Count);
			var arrow = buffer.DependencyAttachments[0];
			AssertEquals(shape4.Shape, arrow.FromShape);
			AssertEquals(buffer, arrow.ToShape);

			AssertEquals("Planned Dur.: 1 week", buffer.AsEntity(network).AdditionalDetail);
			AssertEquals("This buffer has not been accepted yet", ((IProposedNetworkEntity)buffer.AsEntity(network)).Description);
		}

		public void TestExecute_NoCriticalChain_ShouldWarnUserAndNotCreateBuffer()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			AssertEquals(0, network.Entities.Count);

			network.SwitchToScaled();

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(0, network.Entities.Count);
			AssertEquals("There is no critical chain in this diagram, so buffers cannot be added.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_BufferShouldSnapToResolutionIncrement()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader);
			var workflowShape = CreateShape(workflow, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			AssertEquals(1, network.Entities.Count);

			var shape = network.Shapes.Single();
			AssertEquals(300d, shape.AsEntity(network).Width);
			AssertEquals(3 * 60 * BMConstants.WorkingHoursPerDay, shape.ExplicitDurationMinutes);
			network.Refresh(RefreshType.RedrawDiagram);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(2, network.Entities.Count);

			var buffer = network.Shapes.Single(s => s.IsBufferShape);
			AssertEquals(200d, buffer.AsEntity(network).Width);
			AssertEquals(2 * 60 * BMConstants.WorkingHoursPerDay, buffer.ExplicitDurationMinutes);
		}

		public void TestExecute_WhenBuffersExist_ShouldClearBuffers()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagram = CreateDiagram(jobHeader, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var childShape1 = CreateShape(workflow1, diagram);
			var childShape2 = CreateShape(workflow2, diagram);
			var childShape3 = CreateShape(workflow3, diagram);

			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, childShape1, childShape2);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, childShape2, childShape3);

			var buffer1_2 = network.Entities.GetInstance(arrow1_2).CreateBuffer();
			var buffer2_3 = network.Entities.GetInstance(arrow2_3).CreateBuffer();

			var refreshed = false;
			network.Refreshed += (_, x_) => refreshed = true;

			AssertEquals(5, network.Entities.Count);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(true, refreshed);
			AssertEquals(3, network.Entities.Count);
			AssertEquals(true, buffer1_2.IsDeleted);
			AssertEquals(true, buffer2_3.IsDeleted);
		}

		public void TestExecute_ShouldAddFeedingBuffersWhereRelevant()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			network.SwitchToScaled();

			var child1_CC = networkViewModel.CreateNewShape(diagram);
			var child2_CC = networkViewModel.CreateNewShape(diagram);
			var child3_CC = networkViewModel.CreateNewShape(diagram);
			var child4_nonCC = networkViewModel.CreateNewShape(diagram);
			var child5_nonCC = networkViewModel.CreateNewShape(diagram);

			child1_CC.Name = "child1_CC";
			child2_CC.Name = "child2_CC";
			child3_CC.Name = "child3_CC";
			child4_nonCC.Name = "child4_nonCC";
			child5_nonCC.Name = "child5_nonCC";

			network.CreateRelationship(child1_CC, child2_CC);
			network.CreateRelationship(child2_CC, child3_CC);
			network.CreateRelationship(child4_nonCC, child2_CC);
			network.CreateRelationship(child5_nonCC, child3_CC);

			child1_CC.Width = child2_CC.Width = child3_CC.Width = 600;
			child4_nonCC.Width = child5_nonCC.Width = 300;

			child1_CC.Height = child2_CC.Height = child3_CC.Height = child4_nonCC.Height = child5_nonCC.Height = 100;
			child1_CC.Y = child5_nonCC.Y = child3_CC.Y = 0;
			child4_nonCC.Y = child2_CC.Y = 200;

			networkViewModel.PushAsLateAsPossible();

			CombineAssertions("starting locations", () =>
			{
				AssertEquals("child1_CC", 0.0, child1_CC.X);
				AssertEquals("child2_CC", 600.0, child2_CC.X);
				AssertEquals("child3_CC", 1200.0, child3_CC.X);
				AssertEquals("child4_nonCC", 300.0, child4_nonCC.X);
				AssertEquals("child5_nonCC", 900.0, child5_nonCC.X);
			});

			AssertEquals(5, network.Entities.Count);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals("Should create two feeding buffers and one project buffer", 8, network.Entities.Count);

			var buffers = network.Entities.ShapeEntities.Where(l => l.IsBufferShape).ToArray();
			AssertEquals(3, buffers.Length);

			foreach (var buffer in buffers)
			{
				AssertEquals("Buffers should all be created inactive", false, buffer.Shape.Active);
				AssertNull("Buffers should not get a ProcessHeader record", buffer.ProcessHeader);
			}

			var child4FeedingBuffer = buffers.Single(b => b.Name == "child4_nonCC Feeding Buffer");
			var child5FeedingBuffer = buffers.Single(b => b.Name == "child5_nonCC Feeding Buffer");
			var projectBuffer = buffers.Single(b => b.Name == "Project Buffer");

			AssertEquals(BufferType.Feeding, ((IBuffer)child4FeedingBuffer.Shape).Type);
			AssertEquals(BufferType.Feeding, ((IBuffer)child5FeedingBuffer.Shape).Type);
			AssertEquals(BufferType.Project, ((IBuffer)projectBuffer.Shape).Type);

			AssertEquals("Feeding buffer should be 50% of the size of the chain preceeding the CC (in this case there is only the size of child4, rounded to next resolution increment)", 200.0, child4FeedingBuffer.Width);
			AssertShapeOffset("Feeding buffer should be placed up against the CC item it feeds", child4FeedingBuffer, diagram, 400, 215);
			AssertShapeOffset("child4 should be pushed forward by the size of the buffer", child4_nonCC, diagram, 100, 200);

			AssertEquals("Feeding buffer should be 50% of the size of the chain preceeding the CC (in this case there is only the size of child5, rounded to next resolution increment)", 200.0, child5FeedingBuffer.Width);
			AssertShapeOffset("Feeding buffer should be placed up against the CC item it feeds", child5FeedingBuffer, diagram, 1000, 15);
			AssertShapeOffset("child5 should be pushed forward by the size of the buffer", child5_nonCC, diagram, 700, 0);

			AssertEquals("Project buffer should be 50% of the size of the CC", 900.0, projectBuffer.Width);
			AssertShapeOffset("Project buffer should be placed after the last CC item", projectBuffer, diagram, 1800, 15);
		}

		public void TestExecute_ShouldNotPushPinnedShapes()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			network.SwitchToScaled();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);

			network.CreateRelationship(shape1, shape3);
			network.CreateRelationship(shape2, shape3);

			shape1.X = 0;
			shape2.X = 100;
			shape3.X = 200;

			shape1.Width = shape3.Width = 200;
			shape2.Width = 100; // Non-CC

			AssertEquals(3, network.Entities.Count);

			shape2.Shape.PinShape(networkViewModel);
			network.Refresh(RefreshType.RedrawDiagram);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(5, network.Entities.Count);

			var feedingBuffer = network.Entities.Single(s => s.Name.EndsWith("Feeding Buffer"));
			AssertEquals(100.0, feedingBuffer.X);
			AssertEquals(100.0, feedingBuffer.Width);

			AssertEquals("Should not move pinned shape - 'back in time' dependency is acceptable", 100.0, shape2.X);
		}

		public void TestExecute_ShouldAddFeedingBuffersWhereRelevant_IgnoresResourceDependencies()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			network.SwitchToScaled();

			var child1_CC = networkViewModel.CreateNewShape(diagram);
			var child2_CC = networkViewModel.CreateNewShape(diagram);
			var child3_CC = networkViewModel.CreateNewShape(diagram);
			var child4_nonCC = networkViewModel.CreateNewShape(diagram);
			var child5_nonCC = networkViewModel.CreateNewShape(diagram);

			child1_CC.Name = "child1_CC";
			child2_CC.Name = "child2_CC";
			child3_CC.Name = "child3_CC";
			child4_nonCC.Name = "child4_nonCC";
			child5_nonCC.Name = "child5_nonCC";

			network.CreateRelationship(child1_CC, child2_CC);
			network.CreateRelationship(child2_CC, child3_CC);
			network.CreateRelationship(child4_nonCC, child2_CC);
			network.CreateRelationship(child5_nonCC, child3_CC);

			var link4_2 = child4_nonCC.PostRequisiteLinks.Single().Attachment;
			link4_2.BNA_Type = AttachmentTypeList.Codes.ResourceDependency;

			child1_CC.Width = child2_CC.Width = child3_CC.Width = 600;
			child4_nonCC.Width = child5_nonCC.Width = 300;

			child1_CC.Height = child2_CC.Height = child3_CC.Height = child4_nonCC.Height = child5_nonCC.Height = 100;
			child1_CC.Y = child5_nonCC.Y = child3_CC.Y = 0;
			child4_nonCC.Y = child2_CC.Y = 200;

			networkViewModel.PushAsLateAsPossible();

			CombineAssertions("starting locations", () =>
			{
				AssertEquals("child1_CC", 0.0, child1_CC.X);
				AssertEquals("child2_CC", 600.0, child2_CC.X);
				AssertEquals("child3_CC", 1200.0, child3_CC.X);
				AssertEquals("child4_nonCC", 300.0, child4_nonCC.X);
				AssertEquals("child5_nonCC", 900.0, child5_nonCC.X);
			});

			AssertEquals(5, network.Entities.Count);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals("Should create one feeding buffer and one project buffer (ignoring resource dependency link)", 7, network.Entities.Count);

			var buffers = network.Entities.ShapeEntities.Where(l => l.Shape.ShapeType == ShapeTypeList.Codes.Buffer).ToArray();
			AssertEquals(2, buffers.Length);

			var child5FeedingBuffer = buffers.Single(b => b.Name == "child5_nonCC Feeding Buffer");
			var projectBuffer = buffers.Single(b => b.Name == "Project Buffer");

			AssertEquals("Feeding buffer should be 50% of the size of the chain preceeding the CC (in this case there is only the size of child5, rounded to next resolution increment)", 200.0, child5FeedingBuffer.Width);
			AssertShapeOffset("Feeding buffer should be placed up against the CC item it feeds", child5FeedingBuffer, diagram, 1000, 15);
			AssertShapeOffset("child5 should be pushed forward by the size of the buffer", child5_nonCC, diagram, 700, 0);

			AssertEquals("Project buffer should be 50% of the size of the CC", 900.0, projectBuffer.Width);
			AssertShapeOffset("Project buffer should be placed after the last CC item", projectBuffer, diagram, 1800, 15);
		}

		public void TestExecute_FeedingBuffers_ChainedNonCCItems()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			network.SwitchToScaled();

			var child1_CC = networkViewModel.CreateNewShape(diagram);
			var child2_CC = networkViewModel.CreateNewShape(diagram);
			var child3_nonCC = networkViewModel.CreateNewShape(diagram);
			var child4_nonCC = networkViewModel.CreateNewShape(diagram);
			var child5_nonCC = networkViewModel.CreateNewShape(diagram);

			child1_CC.Name = "child1_CC";
			child2_CC.Name = "child2_CC";
			child3_nonCC.Name = "child3_nonCC";
			child4_nonCC.Name = "child4_nonCC";
			child5_nonCC.Name = "child5_nonCC";

			network.CreateRelationship(child1_CC, child2_CC);
			network.CreateRelationship(child3_nonCC, child4_nonCC);
			network.CreateRelationship(child4_nonCC, child5_nonCC);
			network.CreateRelationship(child5_nonCC, child2_CC);

			child1_CC.Width = child2_CC.Width = 900;
			child3_nonCC.Width = child4_nonCC.Width = child5_nonCC.Width = 200;

			child1_CC.Height = child2_CC.Height = child3_nonCC.Height = child4_nonCC.Height = child5_nonCC.Height = 100;
			child1_CC.Y = 0;
			child3_nonCC.Y = child4_nonCC.Y = child5_nonCC.Y = child2_CC.Y = 200;

			networkViewModel.PushAsLateAsPossible();

			CombineAssertions("starting locations", () =>
			{
				AssertEquals("child1_CC", 0.0, child1_CC.X);
				AssertEquals("child2_CC", 900.0, child2_CC.X);
				AssertEquals("child3_nonCC", 300.0, child3_nonCC.X);
				AssertEquals("child4_nonCC", 500.0, child4_nonCC.X);
				AssertEquals("child5_nonCC", 700.0, child5_nonCC.X);
			});

			AssertEquals(5, network.Entities.Count);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals("Should create one feeding buffer and one project buffer", 7, network.Entities.Count);

			var buffers = network.Entities.ShapeEntities.Where(l => l.Shape.ShapeType == ShapeTypeList.Codes.Buffer).ToArray();
			AssertEquals(2, buffers.Length);

			var feedingBuffer = buffers.Single(b => b.Name == "child5_nonCC Feeding Buffer");
			var projectBuffer = buffers.Single(b => b.Name == "Project Buffer");

			AssertEquals("Feeding buffer should be 50% of the size of the chain preceeding the CC (in this case the size of child3-5)", 300.0, feedingBuffer.Width);
			AssertShapeOffset("Feeding buffer should be placed up against the CC item it feeds", feedingBuffer, diagram, 600, 215);

			CombineAssertions("Buffered chain should be shifted to accommodate buffer", () =>
			{
				AssertEquals("child3_nonCC", 0.0, child3_nonCC.X);
				AssertEquals("child4_nonCC", 200.0, child4_nonCC.X);
				AssertEquals("child5_nonCC", 400.0, child5_nonCC.X);
			});

			AssertEquals("Project buffer should be 50% of the size of the CC", 900.0, projectBuffer.Width);
			AssertShapeOffset("Project buffer should be placed after the last CC item", projectBuffer, diagram, 1800, 215);
		}

		public void TestExecute_FeedingBuffers_ChainedNonCCItemsBranchFromCC_ShouldOnlyIncludeNonCCSegment()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			network.SwitchToScaled();

			var child1_CC = networkViewModel.CreateNewShape(diagram);
			var child2_CC = networkViewModel.CreateNewShape(diagram);
			var child3_CC = networkViewModel.CreateNewShape(diagram);
			var child4_nonCC = networkViewModel.CreateNewShape(diagram);
			var child5_nonCC = networkViewModel.CreateNewShape(diagram);
			var child6_nonCC = networkViewModel.CreateNewShape(diagram);

			child1_CC.Name = "child1_CC";
			child2_CC.Name = "child2_CC";
			child3_CC.Name = "child3_CC";
			child4_nonCC.Name = "child4_nonCC";
			child5_nonCC.Name = "child5_nonCC";
			child6_nonCC.Name = "child6_nonCC";

			network.CreateRelationship(child1_CC, child2_CC);
			network.CreateRelationship(child1_CC, child4_nonCC); // CC branches off to non-CC items
			network.CreateRelationship(child2_CC, child3_CC);
			network.CreateRelationship(child4_nonCC, child5_nonCC);
			network.CreateRelationship(child5_nonCC, child6_nonCC);
			network.CreateRelationship(child6_nonCC, child3_CC);

			child1_CC.Width = child2_CC.Width = child3_CC.Width = 900;
			child4_nonCC.Width = child5_nonCC.Width = child6_nonCC.Width = 200;

			child1_CC.Height = child2_CC.Height = child3_CC.Height = child4_nonCC.Height = child5_nonCC.Height = child6_nonCC.Height = 100;
			child1_CC.Y = child2_CC.Y = 0;
			child4_nonCC.Y = child5_nonCC.Y = child6_nonCC.Y = child3_CC.Y = 200;

			networkViewModel.PushAsLateAsPossible();

			CombineAssertions("starting locations", () =>
			{
				AssertEquals("child1_CC", 0.0, child1_CC.X);
				AssertEquals("child2_CC", 900.0, child2_CC.X);
				AssertEquals("child3_CC", 1800.0, child3_CC.X);
				AssertEquals("child4_nonCC", 1200.0, child4_nonCC.X);
				AssertEquals("child5_nonCC", 1400.0, child5_nonCC.X);
				AssertEquals("child6_nonCC", 1600.0, child6_nonCC.X);
			});

			AssertEquals(6, network.Entities.Count);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals("Should create one feeding buffer and one project buffer", 8, network.Entities.Count);

			var buffers = network.Entities.ShapeEntities.Where(l => l.Shape.ShapeType == ShapeTypeList.Codes.Buffer).ToArray();
			AssertEquals(2, buffers.Length);

			var feedingBuffer = buffers.Single(b => b.Name == "child6_nonCC Feeding Buffer");
			var projectBuffer = buffers.Single(b => b.Name == "Project Buffer");

			AssertEquals("Feeding buffer should be 50% of the size of the chain preceeding the CC (in this case the size of child4-6 excluding preceeding CC items)", 300.0, feedingBuffer.Width);
			AssertShapeOffset("Feeding buffer should be placed up against the CC item it feeds", feedingBuffer, diagram, 1500, 215);

			CombineAssertions("Buffered chain should be shifted to accommodate buffer", () =>
			{
				AssertEquals("child3_nonCC", 900.0, child4_nonCC.X);
				AssertEquals("child4_nonCC", 1100.0, child5_nonCC.X);
				AssertEquals("child5_nonCC", 1300.0, child6_nonCC.X);
			});

			AssertEquals("Project buffer should be 50% of the size of the CC (1530 rounded up to nearest resolution increment)", 1400.0, projectBuffer.Width);
			AssertShapeOffset("Project buffer should be placed after the last CC item", projectBuffer, diagram, 2700, 215);
		}

		public void TestExecute_ShouldPushFeedingChainsForward_ButNotIntoNegativeOffsetBecauseThatsSillyAndTheDiagramWontAllowIt()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1_cc = networkViewModel.CreateNewShape(diagram);
			var shape2_cc = networkViewModel.CreateNewShape(diagram);
			var shape3_cc = networkViewModel.CreateNewShape(diagram);

			var shape4_nonCC = networkViewModel.CreateNewShape(diagram);

			shape1_cc.Name = "shape1_cc";
			shape2_cc.Name = "shape2_cc";
			shape3_cc.Name = "shape3_cc";
			shape4_nonCC.Name = "shape4_nonCC";

			network.CreateRelationship(shape1_cc, shape2_cc);
			network.CreateRelationship(shape2_cc, shape3_cc);
			network.CreateRelationship(shape4_nonCC, shape3_cc);

			network.SwitchToScaled();

			shape4_nonCC.Width = 500;
			networkViewModel.PushAsLateAsPossible();

			AssertEquals(true, shape1_cc.IsOnCriticalPath);
			AssertEquals(true, shape2_cc.IsOnCriticalPath);
			AssertEquals(true, shape3_cc.IsOnCriticalPath);
			AssertEquals(false, shape4_nonCC.IsOnCriticalPath);

			AssertEquals(0.0, shape1_cc.X);
			AssertEquals(300.0, shape2_cc.X);
			AssertEquals(600.0, shape3_cc.X);

			AssertEquals(100.0, shape4_nonCC.X);

			AssertEquals(4, network.Entities.Count);

			new SuggestBufferAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(6, network.Entities.Count);

			var feedingBuffer = network.Shapes.Single(s => s.Name.EndsWith("Feeding Buffer")).AsEntity(network);
			AssertEquals(300.0, feedingBuffer.Width);
			AssertEquals(300.0, feedingBuffer.X);
			AssertEquals("Should move pre-req shape to the edge of diagram surface to accommodate the buffer", 0.0, shape4_nonCC.X);
		}

		public void TestExecute_WhenFeedingChainIsInSubDiagram_ShouldUseRootDiagramScale()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false, description: "jobHeader1");
			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false, description: "jobHeader2");

			var workflow1_1 = CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = CreateWorkflow(jobHeader1, "workflow1_2");
			var workflow1_3 = CreateWorkflow(jobHeader1, "workflow1_3");
			var workflow2_1 = CreateWorkflow(jobHeader2, "workflow2_1");

			var diagram = CreateDiagram(jobHeader1);
			var shape1_1 = CreateShape(workflow1_1, diagram);
			var shape1_2 = CreateShape(workflow1_2, diagram);
			var shape1_3 = CreateShape(workflow1_3, diagram);

			var subDiagram = CreateShape(jobHeader2, diagram);
			var shape2_1 = CreateShape(workflow2_1, subDiagram);

			shape1_1.MakeVisiblePrerequisiteOf(shape1_2, diagram);
			shape1_2.MakeVisiblePrerequisiteOf(shape1_3, diagram);
			shape2_1.MakeVisiblePrerequisiteOf(shape1_3, diagram);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();

			var buffers = network.Shapes.OfType<BMNCNBufferShape>().ToArray();
			AssertEquals(2, buffers.Length);

			var projectBuffer = buffers.Single(b => b.Type == BufferType.Project);
			var feedingBuffer = buffers.Single(b => b.Type == BufferType.Feeding);

			AssertEquals(500.0, projectBuffer.AsEntity(network).Width);
			AssertEquals(200.0, feedingBuffer.AsEntity(network).Width);
		}

		public void TestExecute_ForNonSavedDiagram()
		{
			IJobNetwork network = null;

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.TriggerSaveAction()).Returns(new Func<ContinueWithSave>(() =>
			{
				network.Refresh(RefreshType.Saving);
				Factory.Save();

				return ContinueWithSave.Yes;
			}));
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			Factory.Save();

			var shape = networkViewModel.CreateNewShape(diagram);
			shape.Name = "Jimminy Jillikers!";

			AssertEquals(true, diagram.HasChanges);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			var newFactory = Factory.CreateNewFactory();
			var loadedShape = newFactory.Load<BMNCNShape>(shape.PK);

			AssertNotNull(loadedShape);
			AssertEquals("The form will attempt to save before performing this operation.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
					"This action is accessible to the root diagram only.",
					"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Cannot execute when the diagram is not in scaled mode.", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetNameCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var shape = Factory.New<BMNCNShape>();
			shape.MakeChildOf(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			Assert("Precondition", !action.IsApplicableAfterActivatingEntity_ForTest(diagram).IsAllowed);
			AssertEquals("Suggest/Remove Buffers", action.GetNameAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();
			Assert("Precondition", action.IsApplicableAfterActivatingEntity_ForTest(diagram).IsAllowed);

			AssertEquals("Suggest Buffers", action.GetNameAfterActivatingEntity_ForTest(diagram));

			shape.BNS_ShapeType = ShapeTypeList.Codes.Buffer;
			AssertEquals("Remove All Buffers", action.GetNameAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var shape = Factory.New<BMNCNShape>();
			shape.MakeChildOf(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			Assert("Precondition", !action.IsApplicableAfterActivatingEntity_ForTest(diagram).IsAllowed);
			AssertEquals("Suggests relevant buffers or removes all buffer shapes from this diagram", action.GetDescriptionAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();
			Assert("Precondition", action.IsApplicableAfterActivatingEntity_ForTest(diagram).IsAllowed);

			AssertEquals("Suggests relevant buffers to be inserted into the diagram", action.GetDescriptionAfterActivatingEntity_ForTest(diagram));

			shape.BNS_ShapeType = ShapeTypeList.Codes.Buffer;
			AssertEquals("Removes all buffer shapes from this diagram", action.GetDescriptionAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Buffer");
		}

		protected override SuggestBufferAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new SuggestBufferAction(networkViewModel);
		}
	}
}
