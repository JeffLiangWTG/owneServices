using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BMNCNShapePushEntitiesTest : NetworkTestCase
	{
		public void TestPushEarlyLateDoesNotRelyOnOrder()
		{
			var diagramShape = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var s1 = networkViewModel.CreateNewShape(diagram);
			var s2 = networkViewModel.CreateNewShape(diagram);
			var s1_1 = networkViewModel.CreateNewShape(s1);
			var s2_1 = networkViewModel.CreateNewShape(s2);

			network.CreateRelationship(s1, s2);

			s1.Width = s2.Width = 200;
			s1_1.Width = s2_1.Width = 100;

			network.FullRefresh();
			network.Shapes.ApplySort(new DepthOrder(network));

			networkViewModel.PushAsEarlyAsPossible();

			AssertCoordinates(s1, x: 0);
			AssertCoordinates(s2, x: 200);
			AssertCoordinates(s1_1, x: 0);
			AssertCoordinates(s2_1, x: 200);

			networkViewModel.PushAsLateAsPossible();

			AssertCoordinates(s1, x: 0);
			AssertCoordinates(s2, x: 200);
			AssertCoordinates(s1_1, x: 100);
			AssertCoordinates(s2_1, x: 300);
		}

		class DepthOrder : IComparer<BMNCNShape>
		{
			internal DepthOrder(IJobNetwork network)
			{
				this.network = network;
			}

			readonly IJobNetwork network;

			public int Compare(BMNCNShape x, BMNCNShape y)
			{
				return x.AsEntity(network).GetOwnersUpHierarchy().Count().CompareTo(y.AsEntity(network).GetOwnersUpHierarchy().Count()) * -1;
			}
		}

		public void TestPushEntityAndChildrenEarly_WorkflowShapeInDiagramPostReqOfSubDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var link_workflow11_workflow12 = workflow1_1.GetOrCreateDependencyLink(workflow1_2);
			var link_workflow21_workflow22 = workflow2_1.GetOrCreateDependencyLink(workflow2_2);
			var link_job2_workflow11 = jobHeader2.GetOrCreateDependencyLink(workflow1_2); // This link pushes workflow1_1 back by the duration of workflow2_2

			var diagramShape = CreateDiagram(jobHeader1);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var subDiagram = CreateShapeAtLocation(jobHeader2, diagram, 300, 200, 600, 200, "subDiagram");

			var diagram_child1 = CreateShapeAtLocation(workflow1_1, diagram, 800, 10, 300, 200, "diagram_child1");
			var diagram_child2 = CreateShapeAtLocation(workflow1_2, diagram, 1600, 100, 300, 200, "diagram_child2");

			var subDiagram_child1 = CreateShapeAtLocation(workflow2_1, subDiagram, 0, 10, 300, 200, "subDiagram_child1");
			var subDiagram_child2 = CreateShapeAtLocation(workflow2_2, subDiagram, 300, 20, 0, 200, "subDiagram_child2");

			CreateDependencyAttachment(diagram, link_workflow11_workflow12, diagram_child1, diagram_child2);
			CreateDependencyAttachment(diagram, link_job2_workflow11, subDiagram, diagram_child2);
			CreateDependencyAttachment(subDiagram, link_workflow21_workflow22, subDiagram_child1, subDiagram_child2);

			networkViewModel.PushAsEarlyAsPossible();

			AssertShapeOffset(diagram_child1, diagram, 0, 10);
			AssertShapeOffset(diagram_child2, diagram, 600, 100);

			AssertShapeOffset(subDiagram, diagram, 0, 200);
			AssertShapeOffset(subDiagram_child1, subDiagram, 0, 10);
			AssertShapeOffset(subDiagram_child2, subDiagram, 300, 20);
		}

		public void TestPushEntityAndChildrenEarly_SubDiagramPostReqOfWorkflowShapeInDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var link_workflow11_workflow12 = workflow1_1.GetOrCreateDependencyLink(workflow1_2);
			var link_workflow21_workflow22 = workflow2_1.GetOrCreateDependencyLink(workflow2_2);
			var link_workflow12_job2 = workflow1_2.GetOrCreateDependencyLink(jobHeader2); // This link pushes jobHeader2 back by the duration of workflow1_1 + workflow1_2

			var diagramShape = CreateDiagram(jobHeader1);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram = CreateShapeAtLocation(jobHeader2, diagram, 400, 200, 600, 200, "subDiagram");

			var diagram_child1 = CreateShapeAtLocation(workflow1_1, diagram, 800, 10, 300, 200, "diagram_child1");
			var diagram_child2 = CreateShapeAtLocation(workflow1_2, diagram, 1600, 100, 300, 200, "diagram_child2");

			var subDiagram_child1 = CreateShapeAtLocation(workflow2_1, subDiagram, 0, 10, 300, 200, "subDiagram_child1");
			var subDiagram_child2 = CreateShapeAtLocation(workflow2_2, subDiagram, 0, 20, 300, 200, "subDiagram_child2");

			CreateDependencyAttachment(diagram, link_workflow11_workflow12, diagram_child1, diagram_child2);
			CreateDependencyAttachment(diagram, link_workflow12_job2, diagram_child2, subDiagram);
			CreateDependencyAttachment(subDiagram, link_workflow21_workflow22, subDiagram_child1, subDiagram_child2);

			networkViewModel.PushAsEarlyAsPossible();

			AssertShapeOffset(diagram_child1, diagram, 0, 10);
			AssertShapeOffset(diagram_child2, diagram, 300, 100);

			AssertShapeOffset(subDiagram, diagram, 600, 200);
			AssertShapeOffset(subDiagram_child1, subDiagram, 0, 10);
			AssertShapeOffset(subDiagram_child2, subDiagram, 300, 20);
		}

		public void TestPushEntityAndChildrenEarly_WhenPinned()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var childShape1 = CreateShapeAtLocation(workflow1, diagram, 0, 0, 100, 50, "childShape1");
			var childShape2 = CreateShapeAtLocation(workflow2, diagram, 0, 0, 100, 50, "childShape2");
			var childShape3 = CreateShapeAtLocation(workflow3, diagram, 0, 0, 100, 50, "childShape3");

			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, childShape1, childShape2);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, childShape2, childShape3);

			childShape2.Shape.PinShape(networkViewModel);

			networkViewModel.PushAsEarlyAsPossible();

			AssertShapeOffset(childShape1, diagram, 0, 0);
			AssertShapeOffset("Should not push childShape2 since it is pinned", childShape2, diagram, 0, 0);
			AssertShapeOffset("childShape3 is pushed but considers the fact that childShape2 is not pushed", childShape3, diagram, 100, 0);
		}

		public void TestPushEntityAndChildrenLate()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var child1 = CreateShapeAtLocation(workflow1, diagram, 0, 10, 100, 50, "child1");
			var child2 = CreateShapeAtLocation(workflow2, diagram, 0, 110, 300, 50, "child2");
			var child3 = CreateShapeAtLocation(workflow3, diagram, 0, 10, 100, 50, "child3");

			var arrow1_3 = CreateDependencyAttachment(diagram, link1_3, child1, child3);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, child2, child3);

			networkViewModel.PushAsLateAsPossible();

			AssertShapeOffset("Should move child1 right up to its postrequisite", child1, diagram, 200, 10);
			AssertShapeOffset("child2 should still be at the start", child2, diagram, 0, 110);
			AssertShapeOffset("child3 should be moved after its prerequisites", child3, diagram, 300, 10);
		}

		public void TestPushEntityAndChildrenLate_MultipleConvergingPostrequisites()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();
			var workflow5 = jobHeader.ProcessHeaders.AddNew();

			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link1_4 = workflow1.GetOrCreateDependencyLink(workflow4);
			var link4_5 = workflow4.GetOrCreateDependencyLink(workflow5);
			var link3_5 = workflow3.GetOrCreateDependencyLink(workflow5);

			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var child1 = CreateShapeAtLocation(workflow1, diagram, 0, 10, 100, 50, "child1");
			var child2 = CreateShapeAtLocation(workflow2, diagram, 0, 110, 300, 50, "child2");
			var child3 = CreateShapeAtLocation(workflow3, diagram, 0, 10, 100, 50, "child3");
			var child4 = CreateShapeAtLocation(workflow4, diagram, 0, 10, 100, 50, "child4");
			var child5 = CreateShapeAtLocation(workflow5, diagram, 0, 10, 100, 50, "child5");

			var arrow1_3 = CreateDependencyAttachment(diagram, link1_3, child1, child3);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, child2, child3);
			var arrow1_4 = CreateDependencyAttachment(diagram, link1_4, child1, child4);
			var arrow4_5 = CreateDependencyAttachment(diagram, link4_5, child4, child5);
			var arrow3_5 = CreateDependencyAttachment(diagram, link3_5, child3, child5);

			networkViewModel.PushAsLateAsPossible();

			AssertShapeOffset("Should move child1 right up to its postrequisite", child1, diagram, 200, 10);
			AssertShapeOffset("child2 should still be at the start", child2, diagram, 0, 110);
			AssertShapeOffset("child3 should be moved after its prerequisites", child3, diagram, 300, 10);
			AssertShapeOffset("child4 should be moved after its prerequisites", child4, diagram, 300, 10);
			AssertShapeOffset("child5 should be moved after its prerequisites", child5, diagram, 400, 10);
		}

		public void TestPushEntityAndChildrenLate_WhenPinned()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var child1 = CreateShapeAtLocation(workflow1, diagram, 0, 10, 100, 50, "child1");
			var child2 = CreateShapeAtLocation(workflow2, diagram, 0, 110, 300, 50, "child2");
			var child3 = CreateShapeAtLocation(workflow3, diagram, 0, 10, 100, 50, "child3");

			var arrow1_3 = CreateDependencyAttachment(diagram, link1_3, child1, child3);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, child2, child3);

			child1.Shape.PinShape(networkViewModel);

			AssertShapeOffset("Initial position of child1", child1, diagram, 0, 10);
			AssertShapeOffset("Initial position of child2", child2, diagram, 0, 110);
			AssertShapeOffset("Initial position of child3", child3, diagram, 0, 10);

			networkViewModel.PushAsLateAsPossible();

			AssertShapeOffset("child1 should not be moved because it is pinned", child1, diagram, 0, 10);
			AssertShapeOffset("child2 should still be at the start", child2, diagram, 0, 110);
			AssertShapeOffset("child3 should be moved after its prerequisites", child3, diagram, 300, 10);
		}

		public void TestPushEntityAndChildrenLate_MultiplePostrequisites()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var child1 = CreateShapeAtLocation(workflow1, diagram, 0, 10, 100, 50, "child1");
			var child2 = CreateShapeAtLocation(workflow2, diagram, 0, 110, 300, 50, "child2");
			var child3 = CreateShapeAtLocation(workflow3, diagram, 0, 10, 100, 50, "child3");

			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, child1, child2);
			var arrow1_3 = CreateDependencyAttachment(diagram, link1_3, child1, child3);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, child2, child3);

			networkViewModel.PushAsLateAsPossible();

			AssertShapeOffset("child1 can't move since there is no gap between post-requisites", child1, diagram, 0, 10);
			AssertShapeOffset("child2 should be moved after its prerequisite", child2, diagram, 100, 110);
			AssertShapeOffset("child3 should be moved after its prerequisites", child3, diagram, 400, 10);
		}

		public void TestPushEntityAndChildrenLate_ExtraSpaceInsideParent_ShouldNotMoveOutsideParentBounds()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];

			var link21_12 = workflow2_1.GetOrCreateDependencyLink(workflow1_2);
			var link2_11 = jobHeader2.GetOrCreateDependencyLink(workflow1_1);
			var link11_12 = workflow1_1.GetOrCreateDependencyLink(workflow1_2);

			var diagramShape = CreateDiagram(jobHeader1);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var subDiagram = CreateShapeAtLocation(jobHeader2, diagram, 0, 10, 200, 50, "subDiagram");

			var child1_1 = CreateShapeAtLocation(workflow1_1, diagram, 0, 10, 100, 50, "child1_1");
			var child1_2 = CreateShapeAtLocation(workflow1_2, diagram, 0, 110, 300, 50, "child1_2");
			var child2_1 = CreateShapeAtLocation(workflow2_1, subDiagram, 0, 10, 100, 50, "child2_1");

			var arrow21_12 = CreateDependencyAttachment(diagram, link21_12, child2_1, child1_2);
			var arrow2_11 = CreateDependencyAttachment(diagram, link2_11, subDiagram, child1_1);
			var arrow11_12 = CreateDependencyAttachment(diagram, link11_12, child1_1, child1_2);

			networkViewModel.PushAsLateAsPossible();

			AssertShapeOffset("child1_1 should be moved after its prerequisite", child1_1, diagram, 200, 10);
			AssertShapeOffset("child1_2 should be moved after its prerequisite", child1_2, diagram, 300, 110);
			AssertShapeOffset("child2_1 should be moved up until its parent boundary, not up to its postrequisite outside its parent", child2_1, subDiagram, 100, 10);
		}

		public void TestPushEntityAndChildrenEarly_1_PostrequisitesInRow()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var child1 = CreateShapeAtLocation(workflow1, diagram, 0, 10, 100, 50, "child1");
			var child2 = CreateShapeAtLocation(workflow2, diagram, 0, 10, 300, 50, "child2");
			var child3 = CreateShapeAtLocation(workflow3, diagram, 0, 10, 100, 50, "child3");

			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, child1, child2);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, child2, child3);

			networkViewModel.PushAsEarlyAsPossible();

			AssertShapeOffset("child1 can't move since there is no pre-requisites", child1, diagram, 0, 10);
			AssertShapeOffset("child2 should be moved after child1", child2, diagram, 100, 10);
			AssertShapeOffset("child3 should be moved after child2", child3, diagram, 400, 10);
		}

		public void TestPushEntityAndChildrenEarly_2_MultiplePostrequisites()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagramShape = CreateDiagram(jobHeader);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var child1 = CreateShapeAtLocation(workflow1, diagram, 0, 10, 100, 50, "child1");
			var child2 = CreateShapeAtLocation(workflow2, diagram, 0, 10, 300, 50, "child2");
			var child3 = CreateShapeAtLocation(workflow3, diagram, 0, 10, 100, 50, "child3");

			var arrow1_3 = CreateDependencyAttachment(diagram, link1_3, child1, child3);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, child2, child3);

			networkViewModel.PushAsEarlyAsPossible();

			AssertShapeOffset("child1 can't move since there is no pre-requisites", child1, diagram, 0, 10);
			AssertShapeOffset("child2 can't move since there is no pre-requisites", child2, diagram, 0, 10);
			AssertShapeOffset("child3 should be moved after pre-requisites", child3, diagram, 300, 10);

			networkViewModel.PushAsLateAsPossible();

			AssertShapeOffset("child1 should move right up to its postrequisite", child1, diagram, 200, 10);
			AssertShapeOffset("child2 can't move since there is no pre-requisites", child2, diagram, 0, 10);
			AssertShapeOffset("child3 should be moved after pre-requisites", child3, diagram, 300, 10);
		}

		public void TestPushEntityAndChildrenEarly_3_ParentShouldNotResize()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var link21_11 = workflow2_1.GetOrCreateDependencyLink(workflow1_1);
			var link11_22 = workflow1_1.GetOrCreateDependencyLink(workflow2_2);

			var diagramShape = CreateDiagram(jobHeader1);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram = CreateShapeAtLocation(jobHeader2, diagram, 0, 0, 300, 50, "subDiagram");

			var child1_1 = CreateShapeAtLocation(workflow1_1, diagram, 0, 0, 100, 50, "child1_1");
			var child2_1 = CreateShapeAtLocation(workflow2_1, subDiagram, 0, 0, 300, 50, "child2_1");
			var child2_2 = CreateShapeAtLocation(workflow2_2, subDiagram, 0, 0, 100, 50, "child2_2");

			var arrow21_11 = CreateDependencyAttachment(diagram, link21_11, child2_1, child1_1);
			var arrow11_22 = CreateDependencyAttachment(diagram, link11_22, child1_1, child2_2);

			networkViewModel.PushAsEarlyAsPossible();

			AssertShapeOffset("child1_1 should be moved after pre-requisites", child1_1, diagram, 300, 0);
			AssertShapeOffset("child2_1 can't move since there is no pre-requisites", child2_1, subDiagram, 0, 0);
			AssertShapeOffset("child2_2 should be moved after pre-requisites", child2_2, subDiagram, 400, 0);

			AssertEquals(0d, subDiagram.X);
			AssertEquals(0d, subDiagram.Y);
			AssertEquals("Parent should not resize.", 300d, subDiagram.Width);
		}

		public void TestPushEntityAndChildrenEarly_4_ParentShouldNotResize()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var link11_21 = workflow1_1.GetOrCreateDependencyLink(workflow2_1);
			var link21_22 = workflow2_1.GetOrCreateDependencyLink(workflow2_2);

			var diagramShape = CreateDiagram(jobHeader1);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram = CreateShapeAtLocation(jobHeader2, diagram, 0, 0, 400, 50, "subDiagram");

			var child1_1 = CreateShapeAtLocation(workflow1_1, diagram, 0, 0, 100, 50, "child1_1");
			var child2_1 = CreateShapeAtLocation(workflow2_1, subDiagram, 0, 0, 300, 50, "child2_1");
			var child2_2 = CreateShapeAtLocation(workflow2_2, subDiagram, 0, 0, 100, 50, "child2_2");

			var arrow11_21 = CreateDependencyAttachment(diagram, link11_21, child1_1, child2_1);
			var arrow21_22 = CreateDependencyAttachment(diagram, link21_22, child2_1, child2_2);

			networkViewModel.PushAsEarlyAsPossible();

			AssertShapeOffset("child1_1 can't move since there is no pre-requisites", child1_1, diagram, 0, 0);
			AssertShapeOffset("child2_1 can't move since the parent is move base on pre-requisites", child2_1, subDiagram, 0, 0);
			AssertShapeOffset("child2_2 should be moved after pre-requisites", child2_2, subDiagram, 300, 0);

			AssertEquals(100d, subDiagram.X);
			AssertEquals(0d, subDiagram.Y);
			AssertEquals("Parent should not resize.", 400d, subDiagram.Width);
		}

		public void TestPushEntityAndChildrenEarly_5_ParentShouldMove()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			var workflow2_1 = jobHeader2.ProcessHeaders[0];

			var link11_P = workflow1_1.GetOrCreateDependencyLink(jobHeader2);

			var diagramShape = CreateDiagram(jobHeader1);
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var subDiagram = CreateShapeAtLocation(jobHeader2, diagram, 0, 0, 300, 50, "subDiagram");

			var child1_1 = CreateShapeAtLocation(workflow1_1, diagram, 0, 0, 100, 50, "child1_1");
			var child2_1 = CreateShapeAtLocation(workflow2_1, subDiagram, 0, 0, 300, 50, "child2_1");

			var arrow11_P = CreateDependencyAttachment(diagram, link11_P, child1_1, subDiagram);

			networkViewModel.PushAsEarlyAsPossible();

			AssertShapeOffset("child1_1 can't move since there is no pre-requisites", child1_1, diagram, 0, 0);

			AssertEquals(100d, subDiagram.X);
			AssertEquals(0d, subDiagram.Y);
		}
	}
}
