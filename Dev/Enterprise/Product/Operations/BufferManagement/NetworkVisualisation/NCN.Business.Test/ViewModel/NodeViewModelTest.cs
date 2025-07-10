using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class NodeViewModelTest : NetworkTestCase
	{
		#region Buffer

		public void TestAdjustLocationToScale_ForBuffer()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100, ShapeType = ShapeTypeList.Codes.Buffer };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModelProvider().Create(entity, networkViewModel);
			viewModel.X = 200;
			viewModel.Y = 100;

			viewModel.AdjustLocationToScale();
			AssertEquals(200d, viewModel.X);

			viewModel.X = 201;
			AssertEquals(201d, viewModel.X);
			viewModel.AdjustLocationToScale();
			AssertEquals(200d, viewModel.X);

			viewModel.X = 276;
			viewModel.AdjustLocationToScale();
			AssertEquals(300d, viewModel.X);

			diagramEntity.ResolutionIncrement = 200;
			viewModel.X = 301;
			viewModel.AdjustLocationToScale();
			AssertEquals(400d, viewModel.X);
		}

		public void TestAdjustSizeToScale_Buffer()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100, ShapeType = ShapeTypeList.Codes.Buffer };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModelProvider().Create(entity, networkViewModel);
			viewModel.X = 200;
			viewModel.Y = 100;
			viewModel.AdjustSizeToScale();
			AssertEquals(200d, viewModel.Width);

			viewModel.Width = 201;
			AssertEquals(201d, viewModel.Width);
			viewModel.AdjustSizeToScale();
			AssertEquals(200d, viewModel.Width);

			viewModel.Width = 251;
			viewModel.AdjustSizeToScale();
			AssertEquals(300d, viewModel.Width);

			diagramEntity.ResolutionIncrement = 200;
			viewModel.Width = 201;
			viewModel.AdjustSizeToScale();
			AssertEquals(400d, viewModel.Width);
		}

		public void TestInvalidShapeDimensions()
		{
			var diagramEntity = new Entity();
			var entity = new Entity { Width = -100, Height = -100 };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModelProvider().Create(entity, networkViewModel);
			Assert("Width and Height must be positive", viewModel.Width > 0 && viewModel.Height > 0);
		}

		#endregion

		#region Channel Colors

		public void TestStatusBrush_ForEntityInChannelWithColour()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel1 = NetworkTestCase.CreateChannel(diagram, "Luminous", color: "Papaya Whip", height: 100);
			var channel2 = NetworkTestCase.CreateChannel(diagram, "Beings", color: "Goldenrod", height: 100);

			var parentShape = NetworkTestCase.CreateShape(diagram, "Parent");
			var childShape = NetworkTestCase.CreateShape(diagram, "Child");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			NetworkTestCase.SetShapeRectangle(network, parentShape, diagram, 100, 100, 100, 100);
			NetworkTestCase.SetShapeRectangle(network, childShape, parentShape, 50, 50, 10, 10);

			var parentViewModel = NetworkTestCase.CreateNodeViewModel(parentShape, network, networkViewModel);
			var childViewModel = NetworkTestCase.CreateNodeViewModel(childShape, network, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.Goldenrod);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.Goldenrod);

			NetworkTestCase.SetShapeOffset(parentShape, diagram, network, 100, 0);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.PapayaWhip);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.PapayaWhip);
		}

		public void TestStatusBrush_ForEntityInChannelWithNoColour()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel1 = NetworkTestCase.CreateChannel(diagram, "Luminous", color: "Papaya Whip", height: 100);
			var channel2 = NetworkTestCase.CreateChannel(diagram, "Beings", color: "", height: 100);

			var parentShape = NetworkTestCase.CreateShape(diagram, "Parent");
			var childShape = NetworkTestCase.CreateShape(diagram, "Child");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			NetworkTestCase.SetShapeRectangle(network, parentShape, diagram, 100, 100, 100, 100);
			NetworkTestCase.SetShapeRectangle(network, childShape, parentShape, 50, 50, 10, 10);

			var parentViewModel = NetworkTestCase.CreateNodeViewModel(parentShape, network, networkViewModel);
			var childViewModel = NetworkTestCase.CreateNodeViewModel(childShape, network, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.White, Color.Beige);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.White, Color.Beige);

			NetworkTestCase.SetShapeOffset(parentShape, diagram, network, 100, 0);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.PapayaWhip);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.PapayaWhip);
		}

		public void TestStatusBrush_ForEntityInChannelWithNoColour_AndAffinityWithColour()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel1 = NetworkTestCase.CreateChannel(diagram, "Luminous", color: "Papaya Whip", height: 100);
			var channel2 = NetworkTestCase.CreateChannel(diagram, "Beings", color: "", height: 100);

			var affinity = NetworkTestCase.CreateAffinity(diagram, "COSM", colour: "Silver");

			var parentShape = NetworkTestCase.CreateShape(diagram, "Parent");
			var childShape = NetworkTestCase.CreateShape(diagram, "Child");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			NetworkTestCase.LinkAffinity(network, childShape, affinity);
			NetworkTestCase.SetShapeRectangle(network, parentShape, diagram, 100, 100, 100, 100);
			NetworkTestCase.SetShapeRectangle(network, childShape, parentShape, 50, 50, 10, 10);

			var parentViewModel = NetworkTestCase.CreateNodeViewModel(parentShape, network, networkViewModel);
			var childViewModel = NetworkTestCase.CreateNodeViewModel(childShape, network, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.White, Color.Beige);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.Silver);

			NetworkTestCase.SetShapeOffset(parentShape, diagram, network, 100, 0);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.PapayaWhip);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.PapayaWhip, Color.Silver);
		}

		public void TestStatusBrush_ForEntityInChannelWithColour_AndAffinityWithNoColour()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel1 = NetworkTestCase.CreateChannel(diagram, "Luminous", color: "Papaya Whip", height: 100);
			var channel2 = NetworkTestCase.CreateChannel(diagram, "Beings", color: "", height: 100);

			var affinity = NetworkTestCase.CreateAffinity(diagram, "COSM", colour: "");

			var parentShape = NetworkTestCase.CreateShape(diagram, "Parent");
			var childShape = NetworkTestCase.CreateShape(diagram, "Child");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			NetworkTestCase.LinkAffinity(network, childShape, affinity);
			NetworkTestCase.SetShapeRectangle(network, parentShape, diagram, 100, 100, 100, 100);
			NetworkTestCase.SetShapeRectangle(network, childShape, parentShape, 50, 50, 10, 10);

			var parentViewModel = NetworkTestCase.CreateNodeViewModel(parentShape, network, networkViewModel);
			var childViewModel = NetworkTestCase.CreateNodeViewModel(childShape, network, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.White, Color.Beige);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.White, Color.Beige);

			NetworkTestCase.SetShapeOffset(parentShape, diagram, network, 100, 0);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.PapayaWhip);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.PapayaWhip);
		}

		#endregion

		#region Properties

		public void TestJobNameVisibilityProperty()
		{
			var diagramShape = CreateDiagram(Factory, name: "Diagram");
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();

			var entityShape = CreateShape(diagramShape);
			var entityViewModel = CreateNodeViewModel(entityShape, network, networkViewModel);
			AssertEquals("HasLinkedEntity is false for a new shape", false, entityViewModel.HasLinkedEntity);

			var diagramShape2 = CreateDiagram(Factory, name: "Diagram 2");
			network.LinkEntity(entityShape, diagramShape2);
			AssertEquals("HasLinkedEntity is true if shape has a link", true, entityViewModel.HasLinkedEntity);

			network.UnlinkEntity(entityShape);
			AssertEquals("HasLinkedEntity is false if a shape link is removed", false, entityViewModel.HasLinkedEntity);
		}

		public void TestJobBarVisibilityPropertyForScaledDiagram()
		{
			var diagramShape = CreateDiagram(Factory, name: "Diagram");
			diagramShape.IsScaled = true;
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();

			var entityShape = CreateShape(diagramShape);
			var entityViewModel = CreateNodeViewModel(entityShape, network, networkViewModel);
			AssertEquals("ShowJobBar is always true in a scaled diagram", true, entityViewModel.ShowJobBar);

			var diagramShape2 = CreateDiagram(Factory, name: "Diagram 2");
			network.LinkEntity(entityShape, diagramShape2);
			AssertEquals("ShowJobBar is always true in a scaled diagram", true, entityViewModel.ShowJobBar);

			network.UnlinkEntity(entityShape);
			AssertEquals("ShowJobBar is always true in a scaled diagram", true, entityViewModel.ShowJobBar);
		}

		public void TestJobBarVisibilityPropertyForNonScaledDiagram()
		{
			var diagramShape = CreateDiagram(Factory, name: "Diagram");
			diagramShape.IsScaled = false;
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();

			var entityShape = CreateShape(diagramShape);
			var entityViewModel = CreateNodeViewModel(entityShape, network, networkViewModel);
			AssertEquals("ShowJobBar is false for a new shape in a non-scaled diagram", false, entityViewModel.ShowJobBar);

			var diagramShape2 = CreateDiagram(Factory, name: "Diagram 2");
			network.LinkEntity(entityShape, diagramShape2);
			AssertEquals("ShowJobBar is false for a shape linked to a diagram in a non-scaled diagram", false, entityViewModel.ShowJobBar);

			network.UnlinkEntity(entityShape);
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflowShape = CreateWorkflow(jobHeader, string.Empty);
			network.LinkEntity(entityShape, workflowShape);
			AssertEquals("ShowJobBar is true for a shape linked to a workflow in a non-scaled diagram", true, entityViewModel.ShowJobBar);

			network.UnlinkEntity(entityShape);
			AssertEquals("ShowJobBar is false if a shape link is removed in a non-scaled diagram", false, entityViewModel.ShowJobBar);
		}

		public void TestJobBarVisibilityPropertyForNonScaledDiagramWithAndWithoutAffinities()
		{
			var diagramShape = CreateDiagram(Factory, name: "Diagram");
			diagramShape.IsScaled = false;
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();

			var entityShape = CreateShape(diagramShape);
			var entityViewModel = CreateNodeViewModel(entityShape, network, networkViewModel);
			AssertEquals("ShowJobBarAffinity is false for a new shape in a non-scaled diagram", false, entityViewModel.ShowJobBarAffinities);

			entityViewModel.AppliedAffinities.Add(new DummyAffinity(new Guid(), "DummyAffinity", Color.Empty));
			AssertEquals("ShowJobBarAffinity is true for a shape with at least affinity in a non-scaled diagram", true, entityViewModel.ShowJobBarAffinities);

			entityViewModel.AppliedAffinities.Clear();
			AssertEquals("ShowJobBarAffinity is false for a shape with no affinities in a non-scaled diagram", false, entityViewModel.ShowJobBarAffinities);
		}

		public void TestIsDiagramWithRibbonProperty()
		{
			BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var jobHeader = CreateJobHeader<OrgHeader>();
			var jobHeaderShape = jobHeader.GetDefaultDiagram();
			AssertEquals("Default diagram never has a ribbon (even when it is enabled)", false, (jobHeaderShape as INetworkEntity).IsDiagramWithRibbon);
			var diagramShape = CreateDiagram(Factory, name: "Diagram");
			AssertEquals("Normal diagram always has a ribbon (if it is enabled)", true, (diagramShape as INetworkEntity).IsDiagramWithRibbon);

			BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Default diagram never has a ribbon", false, (jobHeaderShape as INetworkEntity).IsDiagramWithRibbon);
			AssertEquals("Normal diagram does not have a ribbon when it is disabled", false, (diagramShape as INetworkEntity).IsDiagramWithRibbon);
		}

		#endregion

		public void TestNodeLocation_ShouldBePersisted()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			var diagramShape = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var shape = network.Entities.ShapeEntities.First(e => e.RelatedEntityPK == workflow2.PK);
			var entity = (INetworkEntity)shape;

			Factory.Save();

			var node = new NodeViewModel(shape, networkViewModel);
			node.X = 100;
			node.Y = 50;

			AssertEquals(100.0, entity.X);
			AssertEquals(50.0, entity.Y);

			AssertEquals(true, diagramShape.HasChanges);

			Factory.Save();

			var loadedDiagram = new BusinessObjectFactory().Load<BMNCNShape>(diagramShape.PK);
			network = CreateNetwork(loadedDiagram);
			var loadedEntity = network.Entities.ShapeEntities.First(e => e.RelatedEntityPK == workflow2.PK);

			AssertEquals(100.0, loadedEntity.X);
			AssertEquals(50.0, loadedEntity.Y);
		}

		public void TestNodeSize_ShouldBePersisted()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			var diagramShape = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var shape = network.Entities.ShapeEntities.First(e => e.RelatedEntityPK == workflow2.PK);
			var entity = (INetworkEntity)shape;

			Factory.Save();

			var node = new NodeViewModel(shape, networkViewModel);
			node.Width = 120.0;
			node.Height = 100.0;
			node.ZIndex = 5;

			AssertEquals(120.0, entity.Width);
			AssertEquals(100.0, entity.Height);
			AssertEquals(5, entity.ZIndex);

			AssertEquals(true, diagramShape.HasChanges);

			Factory.Save();

			var loadedDiagram = new BusinessObjectFactory().Load<BMNCNShape>(diagramShape.PK);
			network = CreateNetwork(loadedDiagram);
			var loadedEntity = network.Entities.ShapeEntities.First(e => e.RelatedEntityPK == workflow2.PK);

			AssertEquals(120.0, loadedEntity.Width);
			AssertEquals(100.0, loadedEntity.Height);
			AssertEquals(5, loadedEntity.ZIndex);
		}

		public void TestNodeSize_ShouldComeFromEntity()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			var diagramShape = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var shape = network.Entities.ShapeEntities.First(e => e.RelatedEntityPK == workflow2.PK);
			var entity = (INetworkEntity)shape;

			entity.Width = 120;
			entity.Height = 100;

			Factory.Save();

			var node = new NodeViewModel(shape, networkViewModel);
			AssertEquals(120.0, node.Width);
			AssertEquals(100.0, node.Height);
		}

		public void TestCanDelete_ShouldBeAbleToDeleteLinkedShapesOnly()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var diagram = jobHeader.GetDefaultDiagram();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = Factory.New<BMNCNShape>();
			shape1.BNS_ShapeType = ShapeTypeList.Codes.Shape;
			var shape2 = workflow.GetDefaultShape(diagram);

			var shape1ViewModel = new NodeViewModel(shape1.AsEntity(network), networkViewModel);
			var shape2ViewModel = new NodeViewModel(shape2.AsEntity(network), networkViewModel);

			AssertEquals(false, shape1ViewModel.CanDelete);
			AssertEquals(true, shape2ViewModel.CanDelete);
		}

		public void TestCanDelete_WorkflowWithNoParent()
		{
			var diagramShape = CreateDiagram(Factory, name: "Mai Frist Diagram");
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();

			var shape = networkViewModel.CreateNewShape(diagramShape);
			var shapeViewModel = new NodeViewModel(shape, networkViewModel);

			AssertEquals(false, shapeViewModel.CanDelete);
		}

		public void TestHideEntity_WorkflowWithNoParent()
		{
			var diagramShape = CreateDiagram(Factory, name: "Mai Frist Diagram");

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var childShape = networkViewModel.CreateNewShape(diagramShape);
			var workflow = childShape.ProcessHeader;
			AssertNull(workflow);

			AssertEquals(false, childShape.IsDeleted);

			network.HideEntity(childShape);

			AssertEquals(true, childShape.IsDeleted);
		}
	}
}
