using System;
using System.Linq;
using System.Windows;
using CargoWise.Application;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Application = System.Windows.Forms.Application;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class NetworkUserControlViewModelTest : NetworkTestCase
	{
		public void TestJobName_ForUnLinkedDiagram()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var orgHeader = VisualBoardsTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory, false);
			var workflow = CreateWorkflow(jobHeader, "A workflow");
			var diagram = CreateDiagram(jobHeader);
			diagram.BNS_JobType = "ORG";

			var shape = CreateShape(workflow, diagram, "Steeple");

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				diagram.BNS_Name = "Current Jonny";
				AssertEquals(true, diagram.HasChanges);

				form.Network.Controller.TriggerSaveAction();

				AssertEquals(false, diagram.HasChanges);
			}
		}

		public void TestShowEntity()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			Factory.Save();

			var diagramShape = CreateDiagram(jobHeader);
			var refresher = CreateRefresher();
			var controller = CreateController(Factory, refresher);
			var network = CreateNetwork(diagramShape, controller: controller, refresher: refresher);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(0, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(1, networkControlViewModel.HiddenEntities.Count);

			viewModel.ShowEntity(workflow, diagramShape, new Point());

			AssertEquals(1, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(0, networkControlViewModel.HiddenEntities.Count);
		}

		public void TestShowEntity_HiddenDependenciesShouldBeUpdated()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			var diagramShape = CreateDiagram(jobHeader);
			var refresher = CreateRefresher();
			var controller = CreateController(Factory, refresher);
			var network = CreateNetwork(diagramShape, controller: controller, refresher: refresher);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(0, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(2, networkControlViewModel.HiddenEntities.Count);
			AssertEquals(0, networkControlViewModel.HiddenDependencies.Count);

			viewModel.ShowEntity(workflow1, diagramShape, new Point());

			AssertEquals(1, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(1, networkControlViewModel.HiddenEntities.Count);
			AssertEquals(0, networkControlViewModel.HiddenDependencies.Count);

			viewModel.ShowEntity(workflow2, diagramShape, new Point());

			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(0, networkControlViewModel.HiddenEntities.Count);
			AssertEquals(0, networkControlViewModel.HiddenDependencies.Count);
		}

		public void TestShowEntity_ShouldPositionSubDiagramEntitiesWithinSubDiagram()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader2.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			Factory.Save();

			jobHeader2.GetOrCreateLinkToParent(jobHeader1);

			var diagram = CreateDiagram(jobHeader1);
			var refresher = CreateRefresher();
			var controller = CreateController(Factory, refresher);
			var network = CreateNetwork(diagram, controller: controller, refresher: refresher);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);

			AssertEquals(2, networkControlViewModel.HiddenEntities.Count);
			AssertEquals(0, network.Entities.Count);
			AssertEquals(0, viewModel.NetworkViewModel.Nodes.Count());

			viewModel.ShowEntity(jobHeader2, diagram, new Point(100, 120));

			AssertEquals(1, network.Entities.Count);
			AssertEquals(1, viewModel.NetworkViewModel.Nodes.Count());

			var jobHeader2Entity = network.Entities[0];
			Assert(jobHeader2Entity.IsSameEntity(jobHeader2));

			var jobHeader2Node = viewModel.NetworkViewModel.Nodes.First(n => n.Entity.IsSameEntity(jobHeader2));

			// Should persist node locations on entities
			AssertEquals(jobHeader2Node.X, jobHeader2Entity.X);
			AssertEquals(jobHeader2Node.Y, jobHeader2Entity.Y);
		}

		public void TestImportDiagram_DisallowImportToPinnedIfDestinationIsBiggerThanSource()
		{
			var controller = CreateMockableController(Mocks);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var diagram1 = CreateNetwork(CreateDiagram(Factory, name: "diagram1", isScaled: true)).DiagramEntity;

				var diagram2 = CreateNetwork(CreateDiagram(Factory, name: "diagram2")).DiagramEntity;
				var diagram2Child = CreateShape(diagram2, "If shapes had local width this would be unnecessary.");
				diagram2Child.Width = 9000;

				var child2_1 = CreateShape(diagram2, "child2_1");
				var child2_2 = CreateShape(diagram2, "child2_2");
				SetShapeOffset(child2_1, diagram2, 100, 100);
				SetShapeOffset(child2_2, diagram2, 500, 100);
				SetShapeSize(child2_1, diagram2, 100, 100);
				SetShapeSize(child2_2, diagram2, 100, 100);

				var arrow21_22 = child2_1.MakeVisiblePrerequisiteOf(child2_2);

				controller.Setup(m => m.PickEntity(ModuleIDs.NetworkDiagram, false)).Returns(diagram2.Shape);

				var networkViewModel = CreateNetworkViewModel(diagram1.Shape, controller: controller.Object);
				var network = networkViewModel.GetJobNetwork();

				var subDiagram = networkViewModel.CreateNewShape(diagram1);
				subDiagram.Width = 300;
				subDiagram.Height = 150;
				subDiagram.Name = "DEA's dirty underwear";
				subDiagram.Shape.PinShape(networkViewModel);

				var networkControlViewModel = new NetworkUserControlViewModel(network);
				var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
				AssertEquals(1, network.Entities.Count);

				viewModel.ImportEntity(new Point(100, 120), subDiagram);

				AssertEquals("Could not perform action because the imported shape wouldn't fit in the parent's bounds.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Make sure we don't leave the universe in an infected state.", diagram2.Owner);
			}
		}

		public void TestImportDiagram_NotBigEnough_GrandChildPinnedShape()
		{
			var controller = CreateMockableController(Mocks);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var viewModel1 = CreateNetworkViewModel(CreateDiagram(Factory, name: "diagram1", isScaled: true));
				var diagram1 = viewModel1.GetJobNetwork().DiagramEntity;

				var viewModel2 = CreateNetworkViewModel(CreateDiagram(Factory, name: "your mum"));
				var diagram2 = viewModel2.GetJobNetwork().DiagramEntity;
				var diagram2Child = CreateShape(diagram2, "If shapes had local width this would be unnecessary.");
				diagram2Child.Width = 9000;

				var child2_1 = CreateShape(diagram2, "child2_1");
				var child2_2 = CreateShape(diagram2, "child2_2");
				SetShapeOffset(child2_1, diagram2, 100, 100);
				SetShapeOffset(child2_2, diagram2, 500, 100);
				SetShapeSize(child2_1, diagram2, 100, 100);
				SetShapeSize(child2_2, diagram2, 100, 100);

				var arrow21_22 = child2_1.MakeVisiblePrerequisiteOf(child2_2);

				controller.Setup(m => m.PickEntity(ModuleIDs.NetworkDiagram, false)).Returns(diagram2.Shape);
				var networkViewModel = CreateNetworkViewModel(diagram1.Shape, controller: controller.Object);
				var network = networkViewModel.GetJobNetwork();

				var subDiagram = networkViewModel.CreateNewShape(diagram1);
				subDiagram.Width = 400;
				subDiagram.Height = 300;
				subDiagram.Name = "My snow mobile";
				subDiagram.Shape.PinShape(viewModel1);

				var subSubDiagram = networkViewModel.CreateNewShape(subDiagram);
				subSubDiagram.Width = 300;
				subSubDiagram.Height = 150;
				subSubDiagram.Name = "A fridge";

				network.FullRefresh();
				var networkControlViewModel = new NetworkUserControlViewModel(network);
				var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);

				viewModel.ImportEntity(new Point(100, 120), subSubDiagram);

				AssertEquals("Could not perform action because the imported shape wouldn't fit in the parent's bounds.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestImportDiagram_ShouldLayChildItemsExactlyAsLaidOutOnSource()
		{
			var controller = NetworkTestCase.CreateMockableController(Mocks);
			using (ObjectFactory.Substitute(controller.Object))
			{
				var diagram1 = CreateNetwork(CreateDiagram(Factory, name: "diagram1")).DiagramEntity;
				var child1_1 = CreateShape(diagram1, "child1_1");
				var child1_2 = CreateShape(diagram1, "child1_2");
				SetShapeOffset(child1_1, diagram1, 100, 100);
				SetShapeOffset(child1_2, diagram1, 500, 100);
				SetShapeSize(child1_1, diagram1, 100, 100);
				SetShapeSize(child1_2, diagram1, 100, 100);

				var arrow11_12 = child1_1.MakeVisiblePrerequisiteOf(child1_2);

				var diagram2 = CreateNetwork(CreateDiagram(Factory, name: "diagram2")).DiagramEntity;
				var child2_1 = CreateShape(diagram2, "child2_1");
				var child2_2 = CreateShape(diagram2, "child2_2");
				SetShapeOffset(child2_1, diagram2, 100, 100);
				SetShapeOffset(child2_2, diagram2, 500, 100);
				SetShapeSize(child2_1, diagram2, 100, 100);
				SetShapeSize(child2_2, diagram2, 100, 100);

				var arrow21_22 = child2_1.MakeVisiblePrerequisiteOf(child2_2);

				JobNetwork network;

				controller.Setup(m => m.PickEntity(ModuleIDs.NetworkDiagram, false)).Returns(diagram2.Shape);
				network = CreateNetwork(diagram1.Shape);
				var networkControlViewModel = new NetworkUserControlViewModel(network);
				var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
				AssertEquals(2, network.Entities.Count);

				viewModel.ImportEntity(new Point(100, 120), diagram1);

				AssertEquals(5, network.Entities.Count);

				var subDiagramShape = network.Entities.ShapeEntities.Single(s => s.Name == "diagram2");

				AssertEquals(2, subDiagramShape.Children.Count());
				var subDiagramChild1 = subDiagramShape.Children.Single(s => s.Name == "child2_1");
				var subDiagramChild2 = subDiagramShape.Children.Single(s => s.Name == "child2_2");

				AssertShapeOffset(subDiagramShape, diagram1, 100, 120);
				AssertShapeSize("Should size parent large enough to fit children, with a bit of padding to look nice", subDiagramShape, 600, 200);
				AssertShapeOffset(subDiagramChild1, subDiagramShape, 100, 95);
				AssertShapeOffset(subDiagramChild2, subDiagramShape, 500, 95);

				AssertEquals(0, ((INetworkEntity)subDiagramShape).ZIndex);
				AssertEquals(1, ((INetworkEntity)subDiagramChild1).ZIndex);
				AssertEquals(1, ((INetworkEntity)subDiagramChild2).ZIndex);

				AssertEquals(1, subDiagramChild1.PostrequisiteLinks(new ShapeNetworkEntityDescendantsStrategy()).Count());
				var link = subDiagramChild1.PostrequisiteLinks(new ShapeNetworkEntityDescendantsStrategy()).Cast<NetworkAttachment>().Single();
				AssertEquals(subDiagramChild2.PK, link.Attachment.BNA_BNS_ToShape);
			}
		}

		public void TestHideEntity_HiddenDependenciesShouldBeUpdated()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			var diagramShape = CreateDiagram(jobHeader);
			var refresher = CreateRefresher();
			var controller = CreateController(Factory, refresher);
			var network = CreateNetwork(diagramShape, controller: controller, refresher: refresher);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(0, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(2, networkControlViewModel.HiddenEntities.Count);
			AssertEquals(0, networkControlViewModel.HiddenDependencies.Count);

			viewModel.ShowEntity(workflow1, diagramShape, new Point());
			viewModel.ShowEntity(workflow2, diagramShape, new Point());

			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(0, networkControlViewModel.HiddenEntities.Count);
			AssertEquals(0, networkControlViewModel.HiddenDependencies.Count);

			viewModel.HideNode(viewModel.NetworkViewModel.Nodes.First());

			CombineAssertions("Should remove hidden dependency for node no longer visible", () =>
			{
				AssertEquals("Nodes", 1, viewModel.NetworkViewModel.Nodes.Count());
				AssertEquals("HiddenEntities", 1, networkControlViewModel.HiddenEntities.Count);
				AssertEquals("HiddenDependencies", 0, networkControlViewModel.HiddenDependencies.Count);
			});
		}

		public void TestHideEntity_ShouldHideAllDescendants()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			Factory.Save();

			var diagramShape = CreateDiagram(jobHeader);
			var rootDiagram = CreateShape(jobHeader, diagramShape);
			var otherDiagram = rootDiagram.ChildShapes.AddNew();
			var otherOtherDiagram = otherDiagram.ChildShapes.AddNew();

			var refresher = CreateRefresher();
			var controller = CreateController(Factory, refresher);
			var network = CreateNetwork(diagramShape, controller: controller, refresher: refresher);
			AssertEquals(3, network.Entities.Count);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(3, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(1, networkControlViewModel.HiddenEntities.Count);
			AssertEquals(0, networkControlViewModel.HiddenDependencies.Count);

			viewModel.ShowEntity(workflow1, diagramShape, new Point());

			AssertEquals(4, viewModel.NetworkViewModel.Nodes.Count());

			var nodeToRemove = viewModel.NetworkViewModel.Nodes.FirstOrDefault(n => n.Entity.IsSameEntity(rootDiagram));
			AssertNotNull("nodeToRemove", nodeToRemove);

			viewModel.HideNode(nodeToRemove);

			AssertEquals(1, viewModel.NetworkViewModel.Nodes.Count());
		}

		public void TestHideEntity_ShouldUpdateStatusMessage()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			var diagramShape = CreateDiagram(jobHeader);
			var refresher = CreateRefresher();
			var controller = CreateController(Factory, refresher);
			var network = CreateNetwork(diagramShape, controller: controller, refresher: refresher);

			var viewModel = new NetworkUserControlViewModel(network);
			var diagramViewModel = new DiagramAreaUserControlViewModel(viewModel, false);

			AssertEquals(2, viewModel.HiddenEntities.Count);
			AssertEquals(0, viewModel.HiddenDependencies.Count);
			AssertEquals("There are hidden items within this diagram.", viewModel.StatusMessage);
			AssertEquals("Right-click the designer surface to show hidden items.", viewModel.StatusTooltip);

			diagramViewModel.ShowEntity(workflow1, diagramShape, new Point());
			diagramViewModel.ShowEntity(workflow2, diagramShape, new Point());

			AssertEquals(0, viewModel.HiddenEntities.Count);
			AssertEquals(0, viewModel.HiddenDependencies.Count);

			diagramViewModel.ShowConnection(link);

			AssertEquals(0, viewModel.HiddenEntities.Count);
			AssertEquals(0, viewModel.HiddenDependencies.Count);
			AssertNull(viewModel.StatusMessage);
			AssertNull(viewModel.StatusTooltip);

			diagramViewModel.HideNode(viewModel.NetworkViewModel.Nodes.First());

			AssertEquals(1, viewModel.HiddenEntities.Count);
			AssertEquals("There are hidden items within this diagram.", viewModel.StatusMessage);
			AssertEquals("Right-click the designer surface to show hidden items.", viewModel.StatusTooltip);
		}

		public void TestShowDependency()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagramShape = CreateDiagram(jobHeader);
			var workflow1Shape = CreateShape(workflow1, diagramShape);
			var workflow2Shape = CreateShape(workflow2, diagramShape);

			var refresher = CreateRefresher();
			var controller = CreateController(Factory, refresher);
			var network = CreateNetwork(diagramShape, controller: controller, refresher: refresher);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);
			AssertEquals(2, networkControlViewModel.NetworkModel.Entities.Count);
			AssertEquals(1, networkControlViewModel.HiddenDependencies.Count);
			AssertEquals(0, workflow1Shape.DependencyAttachments.Count);
			AssertEquals(0, workflow2Shape.DependencyAttachments.Count);

			var sourceNode = viewModel.NetworkViewModel.Nodes.First(n => n.Entity.AsShape() == workflow1Shape);
			var destNode = viewModel.NetworkViewModel.Nodes.First(n => n.Entity.AsShape() == workflow2Shape);

			viewModel.ShowConnection(link);

			AssertEquals(0, networkControlViewModel.HiddenDependencies.Count);
			AssertEquals(1, workflow1Shape.DependencyAttachments.Count);
			AssertEquals(1, workflow2Shape.DependencyAttachments.Count);

			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());
			var relationship = workflow1Shape.DependencyAttachments.First();
			var connection = viewModel.NetworkViewModel.Connections.First();
			AssertEquals(network.Entities.GetInstance(relationship), connection.Relationship);
			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes.Any(s => s.OutputConnectors[0] == connection.SourceConnector));
			AssertEquals(true, viewModel.NetworkViewModel.ScheduledNodes.Any(s => s.InputConnectors[0] == connection.DestConnector));
		}

		public void TestConnections_ShouldBeCreatedBasedOnProcessHeaderLinks()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			workflow1.MakePrerequisiteOf(workflow2);

			var diagram = jobHeader.GetDefaultDiagram();
			var refresher = CreateRefresher();
			var controller = CreateController(Factory, refresher);
			var network = CreateNetwork(diagram, controller: controller, refresher: refresher);

			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);

			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());

			var connection = viewModel.NetworkViewModel.Connections.First();
			AssertEquals(workflow1.GetDefaultShape(diagram), connection.SourceConnector.ParentNode.Entity.AsShape());
			AssertEquals(workflow2.GetDefaultShape(diagram), connection.DestConnector.ParentNode.Entity.AsShape());
		}

		public void TestDeleteConnection()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.MakePrerequisiteOf(workflow2);

			var link = workflow1.Links.First();

			var network = CreateNetwork(jobHeader.GetDefaultDiagram());
			var networkControlViewModel = new NetworkUserControlViewModel(network);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);

			AssertEquals(2, viewModel.NetworkViewModel.Nodes.Count());
			AssertEquals(1, viewModel.NetworkViewModel.Connections.Count());

			var connection = viewModel.NetworkViewModel.Connections.First();
			viewModel.DeleteConnection(connection);

			Assert(link.IsDeleted);
		}

		public void TestHideRelationship_ShouldDeleteShapeButNotProcessHeaderLink()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagramShape = CreateDiagram(jobHeader);
			var network = CreateNetwork(diagramShape);

			var diagram = network.DiagramEntity;
			var workflow1Shape = CreateShape(workflow1, diagram);
			var workflow2Shape = CreateShape(workflow2, diagram);

			SetShapeOffset(workflow1Shape, diagram, 10, 50);
			SetShapeOffset(workflow2Shape, diagram, 10, 250);

			var dependencyAttachment = CreateDependencyAttachment(diagram, link, workflow1Shape, workflow2Shape);

			Factory.Save();

			var userControlViewModel = new NetworkUserControlViewModel(network);
			var diagramAreaViewModel = new DiagramAreaUserControlViewModel(userControlViewModel, false);
			var connection = userControlViewModel.NetworkViewModel.Connections.First();

			AssertEquals(0, userControlViewModel.HiddenDependencies.Count);

			diagramAreaViewModel.HideConnection(connection);

			AssertEquals(1, userControlViewModel.HiddenDependencies.Count);
			AssertEquals(true, dependencyAttachment.IsDeleted);
			AssertEquals(false, link.IsDeleted);
			AssertEquals(false, workflow1Shape.IsDeleted);
			AssertEquals(false, workflow2Shape.IsDeleted);
		}

		public void TestAutoPlacement_DefaultDiagram()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Fewble";
			var workflow2 = CreateWorkflow(jobHeader, "Give a man a horse, he can ride.");
			var workflow3 = CreateWorkflow(jobHeader, "Give a man a boat, he can sail.");
			var workflow4 = CreateWorkflow(jobHeader, "Give a man a BMX, he can do flips.");
			var link1 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2 = workflow1.GetOrCreateDependencyLink(workflow3);
			var link3 = workflow3.GetOrCreateDependencyLink(workflow4);

			var diagramShape = jobHeader.GetDefaultDiagram();
			var workflow1Shape = workflow1.GetDefaultShape(diagramShape);
			var workflow2Shape = workflow2.GetDefaultShape(diagramShape);
			var workflow3Shape = workflow3.GetDefaultShape(diagramShape);
			var workflow4Shape = workflow4.GetDefaultShape(diagramShape);

			Factory.Save();

			var network = CreateNetwork(diagramShape);
			var userControlViewModel = new NetworkUserControlViewModel(network);
			var diagramAreaViewModel = new DiagramAreaUserControlViewModel(userControlViewModel, false);

			var workflow1Entity = workflow1Shape.AsEntity(network);
			var workflow2Entity = workflow2Shape.AsEntity(network);
			var workflow3Entity = workflow3Shape.AsEntity(network);
			var workflow4Entity = workflow4Shape.AsEntity(network);

			diagramAreaViewModel.CreateNewNodes(new[] { workflow1Entity, workflow2Entity, workflow3Entity, workflow4Entity }, new Location(0, 0));

			AssertCoordinates(workflow1Entity, x: 0, y: 0);
			AssertCoordinates(workflow2Entity, x: 345, y: 0);
			AssertCoordinates(workflow3Entity, x: 345, y: 165); // Parallel to shape 2
			AssertCoordinates(workflow4Entity, x: 690, y: 165); // Extends off of shape 3
		}

		public void TestAutoPlacement_OffsetFromPoint()
		{
			var diagramShape = CreateDiagram(Factory, name: "Birds");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(diagram, name: "Goose");
			var shape2 = CreateShape(diagram, name: "Duck");
			var shape3 = CreateShape(diagram, name: "Swan");
			var shape4 = CreateShape(diagram, name: "Albatross");

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape3.MakeVisiblePrerequisiteOf(shape4);

			var userControlViewModel = new NetworkUserControlViewModel(network, new AlternativeNodeDataTemplateSelector(), new JobNetworkNodeViewModelProvider());
			var diagramAreaViewModel = new DiagramAreaUserControlViewModel(userControlViewModel, false);
			diagramAreaViewModel.CreateNewNodes(new[] { shape1, shape2, shape3, shape4 }, new Location(400, 400));

			AssertCoordinates(shape1, x: 400, y: 400);
			AssertCoordinates(shape2, x: 745, y: 400);
			AssertCoordinates(shape3, x: 400, y: 565);
			AssertCoordinates(shape4, x: 745, y: 565);
		}

		public void TestAutoPlacement_Heirarchy()
		{
			var diagramShape = CreateDiagram(Factory, name: "Birds");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, name: "Flightless");
			var subDiagram2 = CreateShape(diagram, name: "Flightfull");
			var shape1 = CreateShape(subDiagram1, name: "Emu");
			var shape2 = CreateShape(subDiagram1, name: "Kiwi");
			var shape3 = CreateShape(subDiagram2, name: "Pidgeon");
			var shape4 = CreateShape(subDiagram2, name: "Crow");

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape3.MakeVisiblePrerequisiteOf(shape4);

			subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2);

			var userControlViewModel = new NetworkUserControlViewModel(network, new AlternativeNodeDataTemplateSelector(), new JobNetworkNodeViewModelProvider());
			var diagramAreaViewModel = new DiagramAreaUserControlViewModel(userControlViewModel, false);
			diagramAreaViewModel.CreateNewNodes(new[] { subDiagram1, subDiagram2, shape1, shape2, shape3, shape4 }, new Location(0, 0));

			AssertCoordinates(subDiagram1, x: 0, y: 0, width: 615, height: 195);
			AssertCoordinates(shape1, x: 0, y: 40);
			AssertCoordinates(shape2, x: 345, y: 40);

			AssertCoordinates(subDiagram2, x: 690, y: 0, width: 615, height: 195);
			AssertCoordinates(shape3, x: 690, y: 40);
			AssertCoordinates(shape4, x: 1035, y: 40);
		}

		public void TestAutoPlacement_Heirarchy_WithPointOffset()
		{
			var diagramShape = CreateDiagram(Factory, name: "Birds");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, name: "Flightless");
			var subDiagram2 = CreateShape(diagram, name: "Flightfull");
			var shape1 = CreateShape(subDiagram1, name: "Emu");
			var shape2 = CreateShape(subDiagram1, name: "Kiwi");
			var shape3 = CreateShape(subDiagram2, name: "Pidgeon");
			var shape4 = CreateShape(subDiagram2, name: "Crow");

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape3.MakeVisiblePrerequisiteOf(shape4);

			subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2);

			var userControlViewModel = new NetworkUserControlViewModel(network, new AlternativeNodeDataTemplateSelector(), new JobNetworkNodeViewModelProvider());
			var diagramAreaViewModel = new DiagramAreaUserControlViewModel(userControlViewModel, false);
			diagramAreaViewModel.CreateNewNodes(new[] { subDiagram1, subDiagram2, shape1, shape2, shape3, shape4 }, new Location(100, 100));

			AssertCoordinates(subDiagram1, x: 100, y: 100, width: 615, height: 195);
			AssertCoordinates(shape1, x: 100, y: 140);
			AssertCoordinates(shape2, x: 445, y: 140);

			AssertCoordinates(subDiagram2, x: 790, y: 100, width: 615, height: 195);
			AssertCoordinates(shape3, x: 790, y: 140);
			AssertCoordinates(shape4, x: 1135, y: 140);
		}

		public void TestAutoPlacement_DontRuinPins_DontTryWhatYouCantDo()
		{
			var diagram = CreateDiagram(Factory, name: "Ganga");
			var subDiagram1 = CreateShape(diagram, name: "Moon");

			var viewModel = CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();
			subDiagram1.PinShape(viewModel);

			subDiagram1.AsEntity(network).X = 200;

			var userControlViewModel = new NetworkUserControlViewModel(network, new AlternativeNodeDataTemplateSelector(), new JobNetworkNodeViewModelProvider());
			var diagramAreaViewModel = new DiagramAreaUserControlViewModel(userControlViewModel, false);

			AssertExceptionThrown<InvalidOperationException>(() => diagramAreaViewModel.CreateNewNodes(network.Entities.GetInstances(new[] { subDiagram1 }.Cast<INetworkEntity>()), new Location(100, 100)));
		}

		public void TestAutoPlacement_DontRuinPins()
		{
			var diagramShape = CreateDiagram(Factory, name: "Ganga");
			var viewModel = CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, name: "Moon");
			var shape1 = CreateShape(subDiagram1, name: "Janana");
			var shape2 = CreateShape(subDiagram1, name: "Hellvion");
			var shape3 = CreateShape(subDiagram1, name: "Erkplund");
			var shape4 = CreateShape(subDiagram1, name: "Casket");

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape3.MakeVisiblePrerequisiteOf(shape4);

			subDiagram1.X = 400;
			subDiagram1.Y = 400;
			subDiagram1.Width = 200;
			subDiagram1.Height = 200;

			subDiagram1.Shape.PinShape(viewModel);

			var userControlViewModel = new NetworkUserControlViewModel(network, new AlternativeNodeDataTemplateSelector(), new JobNetworkNodeViewModelProvider());
			var diagramAreaViewModel = new DiagramAreaUserControlViewModel(userControlViewModel, false);

			diagramAreaViewModel.CreateNewNodes(new[] { shape1, shape2, shape3, shape4 }, new Location(100, 100));

			AssertCoordinates(subDiagram1, x: 400, y: 400, width: 200, height: 200);
			AssertCoordinates(shape1, x: 400, y: 440, width: 200, height: 150);
			AssertCoordinates(shape2, x: 400, y: 440, width: 200, height: 150);
			AssertCoordinates(shape3, x: 400, y: 445, width: 200, height: 150);
			AssertCoordinates(shape4, x: 400, y: 445, width: 200, height: 150);
		}

		public void TestAutoPlacement_DontRuinPins_Heirarchy()
		{
			var diagramShape = CreateDiagram(Factory, name: "Errant");
			var viewModel = CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram1 = CreateShape(diagram, name: "Gallant");
			var subSubDiagram1 = CreateShape(subDiagram1, name: "Valiant");
			var suSubSubDiagram1 = CreateShape(subSubDiagram1, name: "Plonky");

			subDiagram1.X = 400;
			subDiagram1.Y = 400;
			subDiagram1.Width = 200;
			subDiagram1.Height = 200;

			subDiagram1.Shape.PinShape(viewModel);

			var userControlViewModel = new NetworkUserControlViewModel(network, new AlternativeNodeDataTemplateSelector(), new JobNetworkNodeViewModelProvider());
			var diagramAreaViewModel = new DiagramAreaUserControlViewModel(userControlViewModel, false);

			diagramAreaViewModel.CreateNewNodes(new[] { subSubDiagram1, suSubSubDiagram1 }, new Location(100, 100));

			AssertCoordinates(subDiagram1, x: 400, y: 400, width: 200, height: 200);
			AssertCoordinates(subSubDiagram1, x: 400, y: 440, width: 200, height: 155);
			AssertCoordinates(suSubSubDiagram1, x: 400, y: 480, width: 200, height: 110);
		}
	}
}
