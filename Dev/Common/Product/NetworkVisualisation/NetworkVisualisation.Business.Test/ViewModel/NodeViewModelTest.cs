using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	class NodeViewModelTest : TestCase
	{
		#region Context Menu

		public void TestContextMenuShouldBeObservable()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var node = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel);
			Assert("Context menu should be implemented as an observable collection otherwise menu won't be updated on repetitive right clicks", node.MenuItems is IObservableReloadableCollection<NetworkActionMenuItem>);
		}

		#endregion

		#region Supported Actions

		public void TestRefreshActionsWhenInNetwork()
		{
			var network = new DummyNetwork();
			network.AddCustomNetworkAction_ForTest(new StaticNetworkAction(ResString.GetMultilingualString("TestRefreshActionsWhenInNetwork: SillyAction", "SillyAction")));

			var entity = new Entity { CanHaveChildren = false, SupportedActions = NetworkActions.None };

			var networkViewModel = new NetworkViewModel(network);
			var node = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);

			AssertEquals(false, node.MenuItems.First(m => m != null && m.Name == "SillyAction").Items.Any());

			network.ClearCustomNetworkActions_ForTest();
			network.AddCustomNetworkAction_ForTest(new StaticNetworkAction(ResString.GetMultilingualString("TestRefreshActionsWhenInNetwork: SillyAction in boots", "SillyAction"), childActions: new INetworkAction[]
			{
				new StaticNetworkAction(ResString.GetMultilingualString("TestRefreshActionsWhenInNetwork: Boots", "Boots"))
			}));
			node.ReloadMenuItems();
			AssertEquals(true, node.MenuItems.First(m => m != null && m.Name == "SillyAction").Items.Any());
		}

		public void TestSupportsEditEntity()
		{
			var network = new DummyNetwork();
			var entity = new Entity { SupportedActions = NetworkActions.EditEntity };
			var networkViewModel = new NetworkViewModel(network);
			var viewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity);
			AssertEquals(true, viewModel.SupportsEditEntity);

			entity.SupportedActions = NetworkActions.None;
			AssertEquals(false, viewModel.SupportsEditEntity);
		}

		#endregion

		#region Location

		public void TestUpdateEntityPosition_DiagramEntityIsParent()
		{
			var diagramEntity = new Entity();
			var entity = new Entity { Width = 200, Height = 100 };

			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 100, Y = 200 };

			AssertEquals(100.0, entity.X);
			AssertEquals(200.0, entity.Y);
		}

		public void TestUpdateEntityPosition_SubDiagramIsParent_ShouldConstrainWithinParentBounds()
		{
			var diagramEntity = new Entity();
			var subDiagramEntity = new Entity { Width = 400, Height = 200, X = 10, Y = 10 };
			var subDiagramChild = new Entity { Parent = subDiagramEntity, Width = 200, Height = 100, X = 10, Y = 10 };

			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(subDiagramEntity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(subDiagramChild, networkViewModel) { X = 200, Y = 100 };

			AssertEquals(200.0, subDiagramChild.X);
			AssertEquals(100.0, subDiagramChild.Y);

			viewModel.X = 300;
			viewModel.Y = 200;

			AssertEquals(210.0, subDiagramChild.X);
			AssertEquals(105.0, subDiagramChild.Y);
		}

		public void TestAdjustLocationToScale()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100 };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 200, Y = 100 };
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

		public void TestAdjustLocationToScale_ForNonScaledDiagram()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = false,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100 };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 200, Y = 100 };
			viewModel.AdjustLocationToScale();
			AssertEquals(200d, viewModel.X);

			viewModel.X = 201;
			AssertEquals(201d, viewModel.X);
			viewModel.AdjustLocationToScale();
			AssertEquals(201d, viewModel.X);
		}

		public void TestAdjustLocationToScale_ForAnnotationShape()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100, ShapeType = ShapeTypes.Annotation };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 200, Y = 100 };
			viewModel.AdjustLocationToScale();
			AssertEquals(200d, viewModel.X);

			viewModel.X = 201;
			AssertEquals(201d, viewModel.X);
			viewModel.AdjustLocationToScale();
			AssertEquals(201d, viewModel.X);
		}

		public void TestAdjustLocationToScale_ForNonScheduledEntity_ShouldDoNothing()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100, IsNonScheduled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 201, Y = 100 };
			AssertEquals(201d, viewModel.X);

			viewModel.AdjustLocationToScale();
			AssertEquals("Nonscheduled shapes shouldn't snap to scale. SAD!", 201d, viewModel.X);
		}

		public void TestAdjustEntityLocationToDiagramBounds()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				IsDiagramSurfaceFixed = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
				Width = 1600
			};

			var subDiagramEntity = new Entity
			{
				IsDiagramScaled = true,
				IsDiagramSurfaceFixed = true,
				ShapeType = ShapeTypes.Shape,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
				Width = 600,
				X = 1200d,
				CornerRadius = 10,
				Name = "sub-diagram"
			};
			subDiagramEntity.Parent = diagramEntity;

			var entity = new Entity { Width = 200, Height = 100, X = 320d };
			entity.Parent = subDiagramEntity;

			subDiagramEntity.AdjustEntityLocationToDiagramBounds(diagramEntity);
			AssertEquals(1000d, subDiagramEntity.X);

			entity.AdjustEntityLocationToDiagramBounds(subDiagramEntity);
			AssertEquals(1400d, entity.X);
			AssertEquals(200d, entity.Width);

			//make sub-diagram bigger than main diagram
			subDiagramEntity = new Entity
			{
				IsDiagramScaled = true,
				IsDiagramSurfaceFixed = true,
				CanHaveChildren = true,
				ShapeType = ShapeTypes.Shape,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
				Width = 1800,
				X = 1200d,
				CornerRadius = 10,
				Name = "sub-diagram"
			};
			subDiagramEntity.Parent = diagramEntity;

			subDiagramEntity.AdjustEntityLocationToDiagramBounds(diagramEntity);
			AssertEquals(1200d, subDiagramEntity.X);
			AssertEquals(1800d, subDiagramEntity.Width);
		}

		public void TestAdjustEntityLocationToChannelBounds_TwoChannels()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 100, X = 0, Y = 0 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 200);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			AssertEquals(0d, shapeViewModel.Y);
			AssertEquals(100d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Y = 100;
			shapeViewModel.AdjustLocationToScale();

			AssertEquals("Should remain within the Bend channel -- no need to snap", 100d, shapeViewModel.Y);
			AssertEquals(100d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Y = 120; // overlap between Bend and Snap, predominantly in Bend
			shapeViewModel.AdjustLocationToScale();

			AssertEquals("Should return to Bend -- aw snap!", 100d, shapeViewModel.Y);
			AssertEquals(100d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Y = 180; // overlap between Bend and Snap, predominantly in Snap
			shapeViewModel.AdjustLocationToScale();

			AssertEquals("Should snap to Snap -- aw snap!", 200d, shapeViewModel.Y);
			AssertEquals(100d, shapeViewModel.Height);
			AssertEquals("Snap", shapeViewModel.AppliedAttributesReadableText);
		}

		public void TestAdjustEntityLocationToChannelBounds_TwoChannels_FirstChannelSmallerThanSecond_ShapeShouldNotResize()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 380, X = 0, Y = 200 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 400);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			AssertEquals(200d, shapeViewModel.Y);
			AssertEquals(380d, shapeViewModel.Height);
			AssertEquals("Snap", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Y = 180;
			shapeViewModel.AdjustLocationToScale();

			AssertEquals("Should remain within the Snap channel", 200d, shapeViewModel.Y);
			AssertEquals("Shape should not resize, and yet...", 380d, shapeViewModel.Height);
			AssertEquals("Snap", shapeViewModel.AppliedAttributesReadableText);
		}

		public void TestAdjustEntityLocationToChannelBounds_ThreeChannels_ThirdChannelLargest_ShapeShouldSnapToCorrectChannel()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 350, X = 0, Y = 410 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 200);
			var channel3 = new DummyChannel("Works Every Time", 600);

			diagramEntity.DiagramChannels = new[] { channel1, channel2, channel3 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			AssertEquals(410d, shapeViewModel.Y);
			AssertEquals(350d, shapeViewModel.Height);
			AssertEquals("Works Every Time", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Y = 210;
			shapeViewModel.AdjustLocationToScale();

			CombineAssertions("What does the shape say?", () =>
			{
				AssertEquals("Should snap to the Snap channel...", 200d, shapeViewModel.Y);
				AssertEquals("Shape should conform to Snap...", 200d, shapeViewModel.Height);
				AssertEquals("Snap", shapeViewModel.AppliedAttributesReadableText);
			});
		}

		public void TestAdjustEntityLocationAndHeightToChannelBounds_MultipleChannels()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 600, X = 0, Y = 100 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bender", 200);
			var channel2 = new DummyChannel("Bending", 200);
			var channel3 = new DummyChannel("Rodriguez", 200);
			var channel4 = new DummyChannel("Snap", 200);

			diagramEntity.DiagramChannels = new[] { channel1, channel2, channel3, channel4 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			shapeViewModel.AdjustLocationToScale();

			AssertEquals(0d, shapeViewModel.Y);
			AssertEquals(200d, shapeViewModel.Height);
			AssertEquals("Bender", shapeViewModel.AppliedAttributesReadableText);
		}

		public void TestAdjustEntityLocationToChannelBounds_NonChannelledAndBack_ShouldSizeCorrectly()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 100, X = 0, Y = 0 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 200);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			AssertEquals(0d, shapeViewModel.Y);
			AssertEquals(100d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Y = 1000;
			shapeEntity.Height = 1200;

			shapeEntity.Y = 200;
			shapeViewModel.AdjustLocationToScale();

			CombineAssertions(() =>
			{
				AssertEquals(200d, shapeViewModel.Y);
				AssertEquals(200d, shapeViewModel.Height);
				AssertEquals("Snap", shapeViewModel.AppliedAttributesReadableText);
			});
		}

		public void TestAdjustEntityLocationToChannelBounds_NonChannelled_ShouldNotResizeNorThrowException()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 100, X = 0, Y = 0 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 200);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			AssertEquals(0d, shapeViewModel.Y);
			AssertEquals(100d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Y = 1000;
			shapeEntity.Height = 1200;
			shapeViewModel.AdjustLocationToScale();

			CombineAssertions(() =>
			{
				AssertEquals(1000d, shapeViewModel.Y);
				AssertEquals(1200d, shapeViewModel.Height);
			});
		}

		#endregion

		#region Color

		public void TestColorChangeWithAffinities()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(diagramEntity, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(viewModel, Color.White, Color.Beige);

			var affinity1 = new DummyAffinity(Guid.NewGuid(), "thisisalmostwhite", Color.BlanchedAlmond);
			var affinity2 = new DummyAffinity(Guid.NewGuid(), "youwouldalsoguessthisiswhite", Color.PapayaWhip);
			var affinity3 = new DummyAffinity(Guid.NewGuid(), "whiteish", Color.MintCream);

			((INetworkEntity)diagramEntity).AppliedAffinities.Add(affinity1);
			((INetworkEntity)diagramEntity).AppliedAffinities.Add(affinity2);
			((INetworkEntity)diagramEntity).AppliedAffinities.Add(affinity3);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(viewModel, Color.BlanchedAlmond, Color.PapayaWhip, Color.MintCream);

			diagramEntity.Status = WorkStatus.Complete;

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(viewModel, Color.White, Color.LightGray);
		}

		public void TestStatusBrush_WhenAffinityHasNoColour()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(diagramEntity, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(viewModel, Color.White, Color.Beige);

			var affinity1 = new DummyAffinity(Guid.NewGuid(), "Real Colour", Color.PapayaWhip);
			var affinity2 = new DummyAffinity(Guid.NewGuid(), "Empty Colour", Color.Empty);

			diagramEntity.AppliedAffinities.Add(affinity1);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(viewModel, Color.PapayaWhip);

			diagramEntity.AppliedAffinities.Add(affinity2);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(viewModel, Color.PapayaWhip);

			diagramEntity.AppliedAffinities.Remove(affinity1);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(viewModel, Color.White, Color.Beige);
		}

		public void TestStatusBrush_WhenAffinitiesCollectionIsNull_ShouldUseDefaultColours()
		{
			var mocks = new MockRepository(MockBehavior.Loose);
			var entity = mocks.Create<IDiagramEntity>();
			var network = new DummyNetwork { DiagramEntity = entity.Object };

			entity.Setup(e => e.AppliedAffinities).Returns((IObservableReloadableCollection<IAffinity>)null);
			AssertNull(entity.Object.AppliedAffinities);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity.Object, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(viewModel, Color.White, Color.Beige);
		}

		public void TestStatusBrush_ForInactiveEntity_ShouldFadeOpacity()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(diagramEntity, networkViewModel);

			AssertEquals(1.0, viewModel.StatusColors.opacity);

			diagramEntity.EntityState = EntityState.Inactive;
			AssertEquals(0.4, viewModel.StatusColors.opacity);
		}

		public void TestStatusBrush_ForEntityInChannelWithColour()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var channel1 = new DummyChannel("Luminous", 100, Color.PapayaWhip);
			var channel2 = new DummyChannel("Beings", 100, Color.Goldenrod);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var parentEntity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			var childEntity = new Entity { Width = 50, Height = 50, X = 10, Y = 10, Parent = parentEntity };

			network.Entities.Add(parentEntity);
			network.Entities.Add(childEntity);

			var networkViewModel = new NetworkViewModel(network);
			var parentViewModel = new NodeViewModel(parentEntity, networkViewModel);
			var childViewModel = new NodeViewModel(childEntity, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.Goldenrod);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.Goldenrod);

			parentEntity.Y = 0;

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.PapayaWhip);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.PapayaWhip);
		}

		public void TestStatusBrush_ForEntityInChannelWithNoColour()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var channel1 = new DummyChannel("Luminous", 100, Color.PapayaWhip);
			var channel2 = new DummyChannel("Beings", 100, Color.Empty);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var parentEntity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			var childEntity = new Entity { Width = 50, Height = 50, X = 10, Y = 10, Parent = parentEntity };

			network.Entities.Add(parentEntity);
			network.Entities.Add(childEntity);

			var networkViewModel = new NetworkViewModel(network);
			var parentViewModel = new NodeViewModel(parentEntity, networkViewModel);
			var childViewModel = new NodeViewModel(childEntity, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.White, Color.Beige);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.White, Color.Beige);

			parentEntity.Y = 0;

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.PapayaWhip);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.PapayaWhip);
		}

		public void TestStatusBrush_ForEntityInChannelWithNoColour_AndAffinityWithColour()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var channel1 = new DummyChannel("Luminous", 100, Color.PapayaWhip);
			var channel2 = new DummyChannel("Beings", 100, Color.Empty);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var affinity = new DummyAffinity(Guid.NewGuid(), "COSM", Color.Silver);
			diagramEntity.AvailableAffinities.Add(affinity);

			var parentEntity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			var childEntity = new Entity { Width = 50, Height = 50, X = 10, Y = 10, Parent = parentEntity };

			childEntity.AppliedAffinities.Add(affinity);

			network.Entities.Add(parentEntity);
			network.Entities.Add(childEntity);

			var networkViewModel = new NetworkViewModel(network);
			var parentViewModel = new NodeViewModel(parentEntity, networkViewModel);
			var childViewModel = new NodeViewModel(childEntity, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.White, Color.Beige);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.Silver);

			parentEntity.Y = 0;

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.PapayaWhip);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.PapayaWhip, Color.Silver);
		}

		public void TestStatusBrush_ForEntityInChannelWithColour_AndAffinityWithNoColour()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var channel1 = new DummyChannel("Luminous", 100, Color.PapayaWhip);
			var channel2 = new DummyChannel("Beings", 100, Color.Empty);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var affinity = new DummyAffinity(Guid.NewGuid(), "COSM", Color.Empty);
			diagramEntity.AvailableAffinities.Add(affinity);

			var parentEntity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			var childEntity = new Entity { Width = 50, Height = 50, X = 10, Y = 10, Parent = parentEntity };

			childEntity.AppliedAffinities.Add(affinity);

			network.Entities.Add(parentEntity);
			network.Entities.Add(childEntity);

			var networkViewModel = new NetworkViewModel(network);
			var parentViewModel = new NodeViewModel(parentEntity, networkViewModel);
			var childViewModel = new NodeViewModel(childEntity, networkViewModel);

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.White, Color.Beige);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.White, Color.Beige);

			parentEntity.Y = 0;

			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(parentViewModel, Color.PapayaWhip);
			NetworkVisualisationTestHelper.AssertBackgroundColourBrush(childViewModel, Color.PapayaWhip);
		}

		public void TestForegroundColor()
		{
			var entity = new Entity { ForeColor = Color.LightBlue };
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var nodeViewModel = new NodeViewModel(entity, networkViewModel);
			AssertEquals(Color.LightBlue, nodeViewModel.ForegroundColor);

			var entity2 = new Entity { ForeColor = Color.Chartreuse };
			var nodeViewModel2 = new NodeViewModel(entity2, networkViewModel);
			AssertEquals(0xFF7FFF00, nodeViewModel2.ForegroundColor.ToArgb() & 0xFFFFFFFF);
		}

		public void TestForegroundBrushNotTransparent_ForEntityWithEmptyForeColor()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var shapeEntity = new Entity { ForeColor = Color.Empty };

			network.Entities.Add(shapeEntity);

			var networkViewModel = new NetworkViewModel(network);
			var node = new NodeViewModel(shapeEntity, networkViewModel);
			var color = node.ForegroundColor;

			Assert($"Shape's foreground colour should not be transparent even if Entity.{nameof(Entity.ForeColor)} isn't set", color.A != 0);
		}

		#endregion

		#region AppliedAttributesReadableText

		public void TestAppliedAttributesReadableText()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var channel1 = new DummyChannel("Luminous", 100, Color.PapayaWhip);
			var channel2 = new DummyChannel("Beings", 100, Color.Empty);
			var channel3 = new DummyChannel("", 100, Color.Empty);

			diagramEntity.DiagramChannels = new[] { channel1, channel2, channel3 };

			var affinity1 = new DummyAffinity(Guid.NewGuid(), "COSM", Color.Empty);
			var affinity2 = new DummyAffinity(Guid.NewGuid(), "", Color.Empty);
			diagramEntity.AvailableAffinities.Add(affinity1);
			diagramEntity.AvailableAffinities.Add(affinity2);

			var parentEntity = new Entity { Width = 100, Height = 100, X = 100, Y = 0 };
			var childEntity = new Entity { Width = 50, Height = 50, X = 10, Y = 10, Parent = parentEntity };

			childEntity.AppliedAffinities.Add(affinity1);
			childEntity.AppliedAffinities.Add(affinity2);

			network.Entities.Add(parentEntity);
			network.Entities.Add(childEntity);

			var networkViewModel = new NetworkViewModel(network);
			var parentViewModel = new NodeViewModel(parentEntity, networkViewModel);
			var childViewModel = new NodeViewModel(childEntity, networkViewModel);

			AssertEquals("Luminous", parentViewModel.AppliedAttributesReadableText);
			AssertEquals("Luminous, COSM", childViewModel.AppliedAttributesReadableText);

			parentEntity.Y = 100;

			AssertEquals("Beings", parentViewModel.AppliedAttributesReadableText);
			AssertEquals("Beings, COSM", childViewModel.AppliedAttributesReadableText);

			parentEntity.Y = 200;

			AssertEquals("", parentViewModel.AppliedAttributesReadableText);
			AssertEquals("COSM", childViewModel.AppliedAttributesReadableText);
		}

		public void TestAppliedAttributesTooltip_ShouldDependOnWhetherThereAreChannels()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkViewModel = new NetworkViewModel(network);

			var entity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			network.Entities.Add(entity);

			var viewModel = new NodeViewModel(entity, networkViewModel);

			AssertEquals("Applied affinities", viewModel.AppliedAttributesTooltip);

			var channel = new DummyChannel("Solace", 100, Color.PapayaWhip);
			diagramEntity.DiagramChannels = new[] { channel };

			AssertEquals("Channel and applied affinities", viewModel.AppliedAttributesTooltip);
		}

		#endregion

		#region Triggering Binding Updates

		public void TestMoveNodeVertically_EvenWhenChannelsPresent_ShouldNotNotifyStatusBrushPropertyChanged()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkViewModel = new NetworkViewModel(network);

			var entity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			network.Entities.Add(entity);

			var changedProperties = new List<string>();
			var viewModel = new NodeViewModel(entity, networkViewModel);
			viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

			viewModel.X = 69;
			AssertContainsExactElementsInAnyOrder("No need to refresh the background color when moving on the X axis", new[] { nameof(NodeViewModel.X) }, changedProperties);
			changedProperties.Clear();

			viewModel.Y = 69;
			AssertContainsExactElementsInAnyOrder("No need to refresh the background color when there are no channels", new[] { nameof(NodeViewModel.Y) }, changedProperties);
			changedProperties.Clear();

			diagramEntity.DiagramChannels = new[] { new DummyChannel("Singularity", 100, Color.Black) };

			viewModel.Y = 70;
			AssertContainsExactElementsInAnyOrder("Now that there are channels, should not refresh the background color, because that happens after we finish drag/dropping", new[] { nameof(NodeViewModel.Y) }, changedProperties);
			changedProperties.Clear();

			viewModel.X = 70;
			AssertContainsExactElementsInAnyOrder("No need to refresh the background color when moving on the X axis", new[] { nameof(NodeViewModel.X) }, changedProperties);
		}

		public void TestMoveNodeVertically_WhilstDragging_ShouldNotNotifyStatusBrushPropertyChangedUntilDropped()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkViewModel = new NetworkViewModel(network);

			var entity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			network.Entities.Add(entity);

			diagramEntity.DiagramChannels = new[] { new DummyChannel("Singularity", 100, Color.Black) };

			var changedProperties = new List<string>();
			var viewModel = new NodeViewModel(entity, networkViewModel);
			viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

			viewModel.Entity.SuspendCalculation();

			viewModel.Y++;
			viewModel.Y++;
			viewModel.Y++;
			AssertContainsExactElementsInAnyOrder("No need to refresh the background color whilst in the middle of dragging the entity. This would cause a significant performance impact.", new[] { nameof(NodeViewModel.Y), nameof(NodeViewModel.Y), nameof(NodeViewModel.Y) }, changedProperties);
			changedProperties.Clear();

			viewModel.Entity.ResumeCalculation();
			viewModel.OnDragCompleted();

			AssertContainsExactElementsInAnyOrder("Now that we've finished dragging, we should refresh the background color.", new[] { nameof(NodeViewModel.StatusColors), nameof(NodeViewModel.AppliedAttributesReadableText), nameof(NodeViewModel.AppliedAttributesTooltip) }, changedProperties);
		}

		public void TestJobDetailsNotifyPropertyChanged()
		{
			var diagramEntity = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			var networkViewModel = new NetworkViewModel(network);

			var entity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			network.Entities.Add(entity);

			diagramEntity.DiagramChannels = new[] { new DummyChannel("Singularity", 100, Color.Black) };

			var viewModel = new NodeViewModel(entity, networkViewModel);
			var propertyChangedTriggeredDiagramName = 0;
			var propertyChangedTriggeredCompletionCriteria = 0;
			var propertyChangedTriggeredJobName = 0;

			viewModel.PropertyChanged += (s, e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(NodeViewModel.DiagramName):
						propertyChangedTriggeredDiagramName++;
						break;

					case nameof(NodeViewModel.JobName):
						propertyChangedTriggeredJobName++;
						break;

					case nameof(NodeViewModel.CompletionCriteria):
						propertyChangedTriggeredCompletionCriteria++;
						break;

					default:
						break;
				}
			};

			AssertEquals(0, propertyChangedTriggeredDiagramName);
			viewModel.DiagramName = "abc";
			AssertEquals(1, propertyChangedTriggeredDiagramName);

			AssertEquals(0, propertyChangedTriggeredJobName);
			viewModel.JobName = "abc";
			AssertEquals(2, propertyChangedTriggeredJobName);

			AssertEquals(0, propertyChangedTriggeredCompletionCriteria);
			viewModel.CompletionCriteria = "abc";
			AssertEquals(1, propertyChangedTriggeredCompletionCriteria);
		}

		#endregion

		#region Size

		public void TestResizeEntity_CantShrinkIntoChildren()
		{
			var parentEntity = new Entity { Width = 100, Height = 100, X = 100, Y = 100 };
			var childEntity = new Entity { Width = 50, Height = 50, X = 125, Y = 125, Parent = parentEntity };

			var network = new DummyNetwork { DiagramEntity = new Entity() };
			network.Entities.Add(parentEntity);
			network.Entities.Add(childEntity);

			var networkViewModel = new NetworkViewModel(network);
			var parentViewModel = new NodeViewModel(parentEntity, networkViewModel) { Width = 100, Height = 100, X = 100, Y = 100 };
			var childViewModel = new NodeViewModel(childEntity, networkViewModel) { Width = 50, Height = 50, X = 125, Y = 125 };

			AssertEquals(125d, childViewModel.X);
			AssertEquals(100d, childViewModel.Width);

			parentViewModel.Width = 0;
			parentViewModel.Height = 0;

			AssertEquals(125d, parentViewModel.Width);
			AssertEquals(95d, parentViewModel.Height);
		}

		public void TestResizeEntity_ForFixedEntityOffsetFromParent()
		{
			var parentEntity = new Entity { Width = 0, Height = 100, X = -100 };
			var childEntity = new Entity { Width = 100, Height = 100, X = 0, Parent = parentEntity, EntityState = EntityState.Fixed };

			var network = new DummyNetwork { DiagramEntity = new Entity() };
			network.Entities.Add(parentEntity);
			network.Entities.Add(childEntity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(childEntity, networkViewModel) { X = 100, Y = 100 };

			AssertNoExceptionThrown(() =>
			{
				viewModel.GetAppropriateWidth(100);
				viewModel.GetAppropriateHeight(100);
			});
		}

		public void TestResizeEntity_DiagramEntityIsParent()
		{
			var diagramEntity = new Entity();
			var entity = new Entity { Width = 200, Height = 100 };

			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 110, Y = 110 };

			AssertEquals(1000.0, viewModel.GetAppropriateWidth(1000));
			AssertEquals(500.0, viewModel.GetAppropriateHeight(500));
		}

		public void TestResizeEntity_SubDiagramIsParent_ShouldConstrainWithinParentBounds()
		{
			var diagramEntity = new Entity();
			var subDiagramEntity = new Entity { Width = 400, Height = 200, X = 10, Y = 10 };
			var subDiagramChild = new Entity { Parent = subDiagramEntity, X = 110, Y = 110 };

			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(subDiagramEntity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(subDiagramChild, networkViewModel) { X = 110, Y = 110 };

			AssertEquals(300.0, viewModel.GetAppropriateWidth(1000));
			AssertEquals(95.0, viewModel.GetAppropriateHeight(500));
		}

		public void TestResizeShapeToScale_EntityDoesNotCrossChannel()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 50, X = 0, Y = 50 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 200);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			AssertEquals(50d, shapeViewModel.Y);
			AssertEquals(50d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Height = 100d;
			shapeViewModel.AdjustSizeToScale();

			AssertEquals(50d, shapeViewModel.Y);
			AssertEquals("Should remain in Bend -- no snap", 100d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);
		}

		public void TestResizeShapeToScale_EntityCrossesOneChannel()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 50, X = 0, Y = 100 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 200);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			AssertEquals(100d, shapeViewModel.Y);
			AssertEquals(50d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Height = 150d;
			shapeViewModel.AdjustSizeToScale();

			AssertEquals(100d, shapeViewModel.Y);
			AssertEquals("Should bend back to Bend -- aw snap!", 100d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);
		}

		public void TestResizeShapeToScale_EntityCrossesMultipleChannels()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var shapeEntity = new Entity { Width = 100, Height = 50, X = 0, Y = 100 };
			network.Entities.Add(shapeEntity);

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 200);
			var channel3 = new DummyChannel("Works Every Time", 200);

			diagramEntity.DiagramChannels = new[] { channel1, channel2, channel3 };

			var networkViewModel = new NetworkViewModel(network);
			var shapeViewModel = new NodeViewModel(shapeEntity, networkViewModel);

			AssertEquals(100d, shapeViewModel.Y);
			AssertEquals(50d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);

			shapeEntity.Height = 500d;
			shapeViewModel.AdjustSizeToScale();

			AssertEquals(100d, shapeViewModel.Y);
			AssertEquals("Should bend back to Bend -- aw snap!", 100d, shapeViewModel.Height);
			AssertEquals("Bend", shapeViewModel.AppliedAttributesReadableText);
		}

		public void TestAdjustSizeToScale()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100 };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 200, Y = 100 };
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

		public void TestAdjustSizeToScale_ForNonScaledDiagram()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = false,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100 };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 200, Y = 100 };
			viewModel.AdjustSizeToScale();
			AssertEquals(200d, viewModel.Width);

			viewModel.Width = 201;
			AssertEquals(201d, viewModel.Width);
			viewModel.AdjustSizeToScale();
			AssertEquals(201d, viewModel.Width);
		}

		public void TestAdjustSizeToScale_ZeroScale()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 0,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100 };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 200, Y = 100 };
			viewModel.AdjustSizeToScale();
			AssertEquals(200d, viewModel.Width);

			viewModel.Width = 201;
			AssertEquals(201d, viewModel.Width);
			viewModel.AdjustSizeToScale();
			AssertEquals(201d, viewModel.Width);
		}

		public void TestAdjustSizeToScale_ForAnnotation()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100, ShapeType = ShapeTypes.Annotation };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 200, Y = 100 };
			viewModel.AdjustSizeToScale();
			AssertEquals(200d, viewModel.Width);

			viewModel.Width = 201;
			AssertEquals(201d, viewModel.Width);
			viewModel.AdjustSizeToScale();
			AssertEquals(201d, viewModel.Width);
		}

		public void TestAdjustSizeToScale_ForNonScheduledEntity_ShouldDoNothing()
		{
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 201, Height = 100, IsNonScheduled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel);
			AssertEquals(201d, viewModel.Width);

			viewModel.AdjustSizeToScale();
			AssertEquals("Nonscheduled shapes' sizes shouldn't snap to scale. SAD!", 201d, viewModel.Width);
		}

		public void TestResizeEntity_RangeForFixedDiagramSurface()
		{
			var diagramEntity = new Entity { Width = 1100, Height = 800, IsDiagramSurfaceFixed = true, Scale = 100, ResolutionIncrement = 50, ScaleUnitPixelSize = 200 };
			var subDiagramEntity = new Entity { Width = 600, Height = 680, X = 0, IsDiagramSurfaceFixed = true };
			var childEntity = new Entity { Width = 100, Height = 100, X = 0, Parent = subDiagramEntity };

			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(subDiagramEntity);
			network.Entities.Add(childEntity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(childEntity, networkViewModel) { X = 100, Y = 100 };
			AssertEquals(120d, viewModel.GetAppropriateHeight(120));
			AssertEquals(120d, viewModel.GetAppropriateWidth(120));

			//open sub-diagram in its own window
			network = new DummyNetwork { DiagramEntity = subDiagramEntity };
			network.Entities.Add(childEntity);

			networkViewModel = new NetworkViewModel(network);
			viewModel = new NodeViewModel(childEntity, networkViewModel) { X = 100, Y = 100 };
			AssertEquals(580d, viewModel.GetAppropriateHeight(760));
			AssertEquals(500d, viewModel.GetAppropriateWidth(620));
		}

		#endregion

		#region BringToFront

		public void TestBringToFront()
		{
			var entity1 = new Entity();
			var entity2 = new Entity { Parent = entity1 };
			var entity3 = new Entity { Parent = entity2 };
			var entity4 = new Entity { Parent = entity2 };
			var network = new DummyNetwork { DiagramEntity = entity1 };
			var networkViewModel = new NetworkViewModel(network);

			var node1 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1);
			var node2 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2);
			var node3 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3);
			var node4 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity4);

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(0, entity2.ZIndex);
			AssertEquals(0, entity3.ZIndex);
			AssertEquals(0, entity4.ZIndex);

			node4.BringToFront();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(0, entity2.ZIndex);
			AssertEquals(0, entity3.ZIndex);
			AssertEquals(1, entity4.ZIndex);

			node3.BringToFront();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(0, entity2.ZIndex);
			AssertEquals(2, entity3.ZIndex);
			AssertEquals(1, entity4.ZIndex);

			node2.SendToBack();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(3, entity3.ZIndex);
			AssertEquals(2, entity4.ZIndex);

			node3.SendToBack();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(2, entity3.ZIndex);
			AssertEquals(3, entity4.ZIndex);

			node3.BringToFront();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(4, entity3.ZIndex);
			AssertEquals(3, entity4.ZIndex);
		}

		public void TestBringToFront_ShouldBringChildrenInFront()
		{
			var network = new DummyNetwork();
			var entity1 = new Entity { Name = "entity1" };
			var entity2 = new Entity { Name = "entity2", Parent = entity1 };
			var entity3 = new Entity { Name = "entity3", Parent = entity2 };
			var entity4 = new Entity { Name = "entity4", Parent = entity2 };
			var networkViewModel = new NetworkViewModel(network);

			var node1 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1);
			var node2 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2);
			var node3 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3);
			var node4 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity4);

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(0, entity2.ZIndex);
			AssertEquals(0, entity3.ZIndex);
			AssertEquals(0, entity4.ZIndex);

			node2.BringToFront();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(2, entity3.ZIndex);
			AssertEquals(2, entity4.ZIndex);

			node3.BringToFront();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(3, entity3.ZIndex);
			AssertEquals(2, entity4.ZIndex);
		}

		#endregion

		#region SendToBack

		public void TestSendToBack_ShouldKeepChildrenInFrontOfParent()
		{
			var entity1 = new Entity { Name = "entity1", ZIndex = 0 };
			var entity2 = new Entity { Name = "entity2", ZIndex = 1, Parent = entity1 };
			var entity3 = new Entity { Name = "entity3", ZIndex = 2, Parent = entity2 };
			var entity4 = new Entity { Name = "entity4", ZIndex = 2, Parent = entity2 };
			var entity5 = new Entity { Name = "entity5", ZIndex = 2, Parent = entity2 };

			var network = new DummyNetwork { DiagramEntity = entity1 };
			var networkViewModel = new NetworkViewModel(network);

			var node1 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1);
			var node2 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2);
			var node3 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3);
			var node4 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity4);
			var node5 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity5);

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(2, entity3.ZIndex);
			AssertEquals(2, entity4.ZIndex);
			AssertEquals(2, entity5.ZIndex);

			node3.SendToBack();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(2, entity3.ZIndex);
			AssertEquals("Should push all other nodes in front when trying to send one node behind its parent", 3, entity4.ZIndex);
			AssertEquals("Should push all other nodes in front when trying to send one node behind its parent", 3, entity5.ZIndex);
		}

		public void TestSendToBack_OnEntitiesWithRootAsParent()
		{
			var entity1 = new Entity { Name = "Diagram Entity", ZIndex = 0 };
			var entity2 = new Entity { Name = "Root 1", ZIndex = 1, Parent = entity1 };
			var entity3 = new Entity { Name = "Root 2", ZIndex = 2, Parent = entity1 };
			var entity4 = new Entity { Name = "Child 1", ZIndex = 3, Parent = entity2 };
			var entity5 = new Entity { Name = "Child 2", ZIndex = 4, Parent = entity2 };

			var network = new DummyNetwork { DiagramEntity = entity1 };
			var networkViewModel = new NetworkViewModel(network);

			var node1 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1);
			var node2 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2);
			var node3 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3);
			var node4 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity4);
			var node5 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity5);

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(2, entity3.ZIndex);
			AssertEquals(3, entity4.ZIndex);
			AssertEquals(4, entity5.ZIndex);

			node3.SendToBack();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(2, entity2.ZIndex);
			AssertEquals(1, entity3.ZIndex);
			AssertEquals(3, entity4.ZIndex);
			AssertEquals(4, entity5.ZIndex);
		}

		public void TestSendToBack_OnEntitiesWithRootAsOnlyParent()
		{
			var entity1 = new Entity { Name = "Diagram Entity", ZIndex = 0 };
			var entity2 = new Entity { Name = "Root 1", ZIndex = 1, Parent = entity1 };
			var entity3 = new Entity { Name = "Root 2", ZIndex = 2, Parent = entity1 };
			var entity4 = new Entity { Name = "Child 1", ZIndex = 3, Parent = entity1 };
			var entity5 = new Entity { Name = "Child 2", ZIndex = 4, Parent = entity1 };

			var network = new DummyNetwork { DiagramEntity = entity1 };
			var networkViewModel = new NetworkViewModel(network);

			var node1 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1);
			var node2 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2);
			var node3 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3);
			var node4 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity4);
			var node5 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity5);

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(1, entity2.ZIndex);
			AssertEquals(2, entity3.ZIndex);
			AssertEquals(3, entity4.ZIndex);
			AssertEquals(4, entity5.ZIndex);

			node5.SendToBack();

			AssertEquals(0, entity1.ZIndex);
			AssertEquals(2, entity2.ZIndex);
			AssertEquals(3, entity3.ZIndex);
			AssertEquals(4, entity4.ZIndex);
			AssertEquals(1, entity5.ZIndex);
		}

		public void TestSendToBack_HasChildrenAndOnEntitiesWithChildren()
		{
			var entityDiagram = new Entity { Name = "Diagram Entity", ZIndex = 0 };
			var entity1 = new Entity { Name = "Root 1", ZIndex = 1, Parent = entityDiagram };
			var entity1a = new Entity { Name = "Root 1 - Child A", ZIndex = 2, Parent = entity1 };
			var entity1b = new Entity { Name = "Root 1 - Child B", ZIndex = 3, Parent = entity1 };
			var entity2 = new Entity { Name = "Root 2", ZIndex = 4, Parent = entityDiagram };
			var entity2a = new Entity { Name = "Root 2 - Child A", ZIndex = 5, Parent = entity2 };
			var entity2b = new Entity { Name = "Root 2 - Child B", ZIndex = 6, Parent = entity2 };
			var entity2b1 = new Entity { Name = "Root 2 - Child B - GrandChild 1", ZIndex = 7, Parent = entity2 };
			var entity3 = new Entity { Name = "Root 3", ZIndex = 8, Parent = entityDiagram };
			var entity3a = new Entity { Name = "Root 3 - Child A", ZIndex = 9, Parent = entity3 };
			var entity3b = new Entity { Name = "Root 3 - Child B", ZIndex = 10, Parent = entity3 };

			var network = new DummyNetwork { DiagramEntity = entityDiagram };
			var networkViewModel = new NetworkViewModel(network, null);

			var nodeDiagram = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entityDiagram);
			var node1 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1);
			var node1a = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1a);
			var node1b = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1b);
			var node2 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2);
			var node2a = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2a);
			var node2b = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2b);
			var node2b1 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2b1);
			var node3 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3);
			var node3a = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3a);
			var node3b = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3b);

			AssertEquals(0, entityDiagram.ZIndex);
			AssertEquals(1, entity1.ZIndex);
			AssertEquals(2, entity1a.ZIndex);
			AssertEquals(3, entity1b.ZIndex);
			AssertEquals(4, entity2.ZIndex);
			AssertEquals(5, entity2a.ZIndex);
			AssertEquals(6, entity2b.ZIndex);
			AssertEquals(7, entity2b1.ZIndex);
			AssertEquals(8, entity3.ZIndex);
			AssertEquals(9, entity3a.ZIndex);
			AssertEquals(10, entity3b.ZIndex);

			node3.SendToBack();

			AssertEquals(0, entityDiagram.ZIndex);
			AssertEquals("node 1 must be at the middle", 4, entity1.ZIndex);
			AssertEquals(5, entity1a.ZIndex);
			AssertEquals(6, entity1b.ZIndex);
			AssertEquals("node 2 must be at the top", 7, entity2.ZIndex);
			AssertEquals(8, entity2a.ZIndex);
			AssertEquals(9, entity2b.ZIndex);
			AssertEquals(10, entity2b1.ZIndex);
			AssertEquals("Node 3 must be at the bottom i.e. ZIndex=1", 1, entity3.ZIndex);
			AssertEquals(2, entity3a.ZIndex);
			AssertEquals(3, entity3b.ZIndex);

			node2.SendToBack();

			AssertEquals(0, entityDiagram.ZIndex);
			AssertEquals("node 1 must be at the top", 8, entity1.ZIndex);
			AssertEquals(9, entity1a.ZIndex);
			AssertEquals(10, entity1b.ZIndex);
			AssertEquals("Node 2 must be at the bottom i.e. ZIndex=1", 1, entity2.ZIndex);
			AssertEquals(2, entity2a.ZIndex);
			AssertEquals(3, entity2b.ZIndex);
			AssertEquals(4, entity2b1.ZIndex);
			AssertEquals("Node 3 must be at the middle", 5, entity3.ZIndex);
			AssertEquals(6, entity3a.ZIndex);
			AssertEquals(7, entity3b.ZIndex);

			node1.SendToBack();

			AssertEquals(0, entityDiagram.ZIndex);
			AssertEquals("Node 1 must be at the bottom i.e. ZIndex=1", 1, entity1.ZIndex);
			AssertEquals(2, entity1a.ZIndex);
			AssertEquals(3, entity1b.ZIndex);
			AssertEquals("Node 2 must be at the middle", 4, entity2.ZIndex);
			AssertEquals(5, entity2a.ZIndex);
			AssertEquals(6, entity2b.ZIndex);
			AssertEquals(7, entity2b1.ZIndex);
			AssertEquals("Node 3 must be at the top", 8, entity3.ZIndex);
			AssertEquals(9, entity3a.ZIndex);
			AssertEquals(10, entity3b.ZIndex);
		}

		#endregion

		#region Provider

		public void TestProvidesAnnotationViewModel()
		{
			var entity1 = new Entity();
			var entity2 = new Entity { Parent = entity1 };
			var entity3 = new Entity { Parent = entity1, ShapeType = ShapeTypes.Annotation };
			var network = new DummyNetwork { DiagramEntity = entity1 };

			var networkViewModel = new NetworkViewModel(network);

			AssertType<NodeViewModel>(new NodeViewModelProvider().Create(entity1, networkViewModel));
			AssertType<NodeViewModel>(new NodeViewModelProvider().Create(entity2, networkViewModel));
			AssertType<AnnotationViewModel>(new NodeViewModelProvider().Create(entity3, networkViewModel));
		}

		public void TestAnnotationViewModel_HidesUnecessaryDetails()
		{
			var entity1 = new Entity { ShapeType = ShapeTypes.Annotation };
			var network = new DummyNetwork { DiagramEntity = entity1 };

			network.Entities.Add(entity1);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModelProvider().Create(entity1, networkViewModel);
			AssertType<AnnotationViewModel>(viewModel);

			AssertEquals(false, viewModel.IsEntityDecorationVisible);
		}

		public void TestAnnotationViewModel_HidesCompletionCriteria()
		{
			var entity = new Entity { ShapeType = ShapeTypes.Annotation };
			var network = new DummyNetwork { DiagramEntity = entity };

			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModelProvider().Create(entity, networkViewModel);

			AssertType<AnnotationViewModel>(viewModel);

			AssertEquals(false, viewModel.IsCompletionCriteriaVisible);
		}

		#endregion

		#region StatusTextWeight

		public void TestStatusTextWeight()
		{
			var entity1 = new Entity { Name = "entity1" };
			var entity2 = new Entity { Name = "entity2", Parent = entity1 };
			var entity3 = new Entity { Name = "entity3", Parent = entity2 };
			var entity4 = new Entity { Name = "entity4", Parent = entity2, Status = WorkStatus.Blocked };

			var network = new DummyNetwork { DiagramEntity = entity1 };
			var networkViewModel = new NetworkViewModel(network);

			var node1 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity1);
			var node2 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity2);
			var node3 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity3);
			var node4 = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, entity4);

			AssertEquals("node2 is the diagram entity", NodeFontWeight.Black, node1.StatusTextWeight);
			AssertEquals("node2 is a sub-diagram", NodeFontWeight.Black, node2.StatusTextWeight);
			AssertEquals("node3 is startable", NodeFontWeight.Black, node3.StatusTextWeight);
			AssertEquals("node4 is blocked", NodeFontWeight.Medium, node4.StatusTextWeight);
		}

		#endregion

		#region AdditionalDetails

		public void TestDurationReadableField()
		{
			var entity = new Entity { Width = 200, Height = 100, ExplicitDurationMinutes = 10 * 60 };
			var network = new DummyNetwork { DiagramEntity = entity };
			entity.IsDiagramScaled = true;
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel);

			AssertEquals("10:00", viewModel.DurationReadableText);

			entity.ExplicitDurationMinutes = 60;
			AssertEquals("1:00", viewModel.DurationReadableText);
		}

		public void TestStartAndEndDateReadableFields()
		{
			var entity = new Entity { Width = 200, Height = 100, ScheduledStartTimeLocal = new DateTime(2015, 7, 14), ScheduledEndTimeLocal = new DateTime(2016, 10, 25) };
			var network = new DummyNetwork { DiagramEntity = entity };
			entity.IsDiagramScaled = true;
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel);

			AssertEquals("14 Jul 2015 00:00", viewModel.StartDateReadableText);
			AssertEquals("25 Oct 2016 00:00", viewModel.FinishDateReadableText);

			entity.ScheduledStartTimeLocal = default(DateTime);
			entity.ScheduledEndTimeLocal = default(DateTime);

			viewModel = new NodeViewModel(entity, networkViewModel);

			AssertEquals(string.Empty, viewModel.StartDateReadableText);
			AssertEquals(string.Empty, viewModel.FinishDateReadableText);

			AssertEquals("Scheduled Start", viewModel.StartDateToolTip);
			AssertEquals("Scheduled Finish", viewModel.FinishDateToolTip);
		}

		public void TestJobNumberReadableField()
		{
			var entity = new Entity { Width = 200, Height = 100 };
			var network = new DummyNetwork { DiagramEntity = entity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel);
			AssertEquals("N/A", viewModel.JobNumberReadableText);

			entity.JobNumber = "WI007";
			viewModel = new NodeViewModel(entity, networkViewModel);
			AssertEquals("WI007", viewModel.JobNumberReadableText);
		}

		public void TestShowDetailedNode()
		{
			var entity = new Entity { Width = 200, Height = 100 };
			var network = new DummyNetwork { DiagramEntity = entity };
			var aNonDetailedNodeEntity = new Entity { Width = 200, Height = 100 };
			network.Entities.Add(entity);
			network.Entities.Add(aNonDetailedNodeEntity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(aNonDetailedNodeEntity, networkViewModel);

			AssertEquals(false, viewModel.ShowScheduleDetails);

			var aDetailedNodeEntity = new Entity { Width = 200, Height = 100 };
			var anotherViewModel = new NodeViewModel(aDetailedNodeEntity, networkViewModel);
			network.Entities.Add(aDetailedNodeEntity);
			entity.IsDiagramScaled = true;

			AssertEquals(true, anotherViewModel.ShowScheduleDetails);

			var annotation = new Entity { Width = 200, Height = 100 };
			var annotationViewModel = new AnnotationViewModel(annotation, networkViewModel);

			AssertEquals(false, annotationViewModel.ShowScheduleDetails);
		}

		public void TestShowScheduleDetailsWithNullSchedule()
		{
			var entity = new Entity { Width = 200, Height = 100 };
			var network = new DummyNetwork { DiagramEntity = entity };
			var aNonDetailedNodeEntity = new Entity { Width = 200, Height = 100, ExplicitDurationMinutes = 60 };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(aNonDetailedNodeEntity, networkViewModel);

			AssertEquals(string.Empty, viewModel.StartDateReadableText);
			AssertEquals(string.Empty, viewModel.FinishDateReadableText);
			AssertEquals(string.Empty, viewModel.RemainingDurationReadableText);
			AssertEquals("Remaining Duration", viewModel.RemainingDurationToolTip);
			AssertEquals("1:00", viewModel.DurationReadableText);
			AssertEquals("Planned Duration", viewModel.DurationTooltip);
			AssertEquals("Scheduled Start", viewModel.StartDateToolTip);
			AssertEquals("Scheduled Finish", viewModel.FinishDateToolTip);

			var aDetailedNodeEntity = new Entity { Width = 200, Height = 100, ScheduledStartTimeLocal = new DateTime(2015, 7, 14), ScheduledEndTimeLocal = new DateTime(2016, 10, 25), RemainingDurationMinutes = 120, ExplicitDurationMinutes = 60 };
			var anotherViewModel = new NodeViewModel(aDetailedNodeEntity, networkViewModel);
			network.Entities.Add(aDetailedNodeEntity);
			entity.IsDiagramScaled = true;

			AssertEquals("14 Jul 2015 00:00", anotherViewModel.StartDateReadableText);
			AssertEquals("25 Oct 2016 00:00", anotherViewModel.FinishDateReadableText);
			AssertEquals("2:00", anotherViewModel.RemainingDurationReadableText);
			AssertEquals("Remaining Duration", anotherViewModel.RemainingDurationToolTip);
			AssertEquals("1:00", anotherViewModel.DurationReadableText);
			AssertEquals("Planned Duration", viewModel.DurationTooltip);
			AssertEquals("Scheduled Start", anotherViewModel.StartDateToolTip);
			AssertEquals("Scheduled Finish", anotherViewModel.FinishDateToolTip);
		}

		public void TestRemainingDuration_WhenEmpty_ShouldBeBlank()
		{
			var entity = new Entity();
			var network = new DummyNetwork { DiagramEntity = entity };
			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel);
			network.Entities.Add(entity);
			entity.IsDiagramScaled = true;

			AssertEquals(string.Empty, viewModel.RemainingDurationReadableText);
			AssertEquals("Remaining Duration", viewModel.RemainingDurationToolTip);

			entity.RemainingDurationMinutes = 90;
			AssertEquals("1:30", viewModel.RemainingDurationReadableText);
			AssertEquals("Remaining Duration", viewModel.RemainingDurationToolTip);
		}

		#endregion

		#region IsFixed

		public void TestIsFixed_ShouldNotAllowMoving()
		{
			var entity = new Entity();
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel);

			viewModel.X = 10;
			viewModel.Y = 100;

			AssertEquals(10.0, viewModel.X);
			AssertEquals(100.0, viewModel.Y);

			entity.EntityState = EntityState.Fixed;

			viewModel.X = 100;
			viewModel.Y = 1000;

			AssertEquals(10.0, viewModel.X);
			AssertEquals(100.0, viewModel.Y);
		}

		public void TestMoveParent_WhenChildIsFixed_ShouldPreventMoveWhereChildIsOutsideParent()
		{
			var root = new Entity();
			var network = new DummyNetwork { DiagramEntity = root };
			var parent = new Entity { Width = 100, Height = 100, X = 0, Y = 0, Parent = root };
			var child = new Entity { Width = 50, Height = 50, X = 50, Y = 50, Parent = parent };

			var networkViewModel = new NetworkViewModel(network);

			var parentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, parent);
			parentViewModel.X = parent.X;
			parentViewModel.Y = parent.Y;

			var childViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, child);
			childViewModel.X = child.X;
			childViewModel.Y = child.Y;

			parentViewModel.X = 100;
			parentViewModel.Y = 100;
			AssertEquals("Should allow move, child location is not fixed", 100.0, parent.X);
			AssertEquals("Should allow move, child location is not fixed", 100.0, parent.Y);

			AssertEquals(100d, parentViewModel.Height); // This should never have expanded.
			AssertEquals(100d, parentViewModel.Width);
			AssertEquals(50d, childViewModel.Height);
			AssertEquals(50d, childViewModel.Width);

			parentViewModel.X = 0;
			parentViewModel.Y = 0;

			childViewModel.X = 50;
			childViewModel.Y = 50;

			child.EntityState = EntityState.Fixed;

			AssertEquals(50d, childViewModel.X);
			AssertEquals("Within margin of parent. Bottom margin pushes child upwards by 5", 45d, childViewModel.Y);

			parentViewModel.X = 100;
			parentViewModel.Y = 100;
			AssertEquals("Should move parent to furthest point without causing child to leave parent bounds", 50.0, parent.X);
			AssertEquals("Should move parent to furthest point without causing child to leave parent bounds (considering parent internal margin)", 5.0, parent.Y);
		}

		public void TestResizeParent_WhenChildIsFixed_ShouldPreventMoveWhereChildIsOutsideParent()
		{
			var network = new DummyNetwork { DiagramEntity = new Entity() };
			var parent = new Entity { Width = 100, Height = 100, X = 0, Y = 0 };
			var child = new Entity { Width = 50, Height = 50, X = 0, Y = 0, Parent = parent };

			var networkViewModel = new NetworkViewModel(network);

			var parentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, parent);
			var childViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, child);

			AssertEquals(50d, childViewModel.Width);
			AssertEquals(50d, childViewModel.Height);

			childViewModel.X = childViewModel.X; // Force refit.
			childViewModel.Y = childViewModel.Y;

			AssertEquals("Not big enough!", 50.0, parentViewModel.GetAppropriateWidth(20));
			AssertEquals("Not big enough!", 95.0, parentViewModel.GetAppropriateHeight(20));

			child.EntityState = EntityState.Fixed;

			AssertEquals("Not big enough!", 50.0, parentViewModel.GetAppropriateWidth(20));
			AssertEquals("Not big enough!", 95.0, parentViewModel.GetAppropriateHeight(20));
		}

		public void TestMoveParent_WhenGrandchildIsFixed_ShouldPreventMoveWhereChildIsOutsideParent()
		{
			var root = new Entity();
			var network = new DummyNetwork { DiagramEntity = root };
			var parent = new Entity { Width = 100, Height = 100, X = 0, Y = 0, Parent = root };
			var child = new Entity { Width = 50, Height = 50, X = 50, Y = 50, Parent = parent };
			var grandchild = new Entity { Width = 25, Height = 25, X = 75, Y = 25, Parent = child };

			var networkViewModel = new NetworkViewModel(network);

			var parentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, parent);
			parentViewModel.X = parent.X;
			parentViewModel.Y = parent.Y;

			var childViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, child);
			childViewModel.X = child.X;
			childViewModel.Y = child.Y;

			var grandchildViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, grandchild);
			grandchildViewModel.X = grandchild.X;
			grandchildViewModel.Y = grandchild.Y;

			parentViewModel.X = 100;
			parentViewModel.Y = 100;
			AssertEquals("Should allow move, grandchild location is not fixed", 100.0, parent.X);
			AssertEquals("Should allow move, grandchild location is not fixed", 100.0, parent.Y);

			parentViewModel.X = 100;
			parentViewModel.Y = 100;
			childViewModel.X = 100;
			childViewModel.Y = 100;
			grandchildViewModel.X = 100;
			grandchildViewModel.Y = 100;

			grandchild.EntityState = EntityState.Fixed;

			AssertEquals(100d, parentViewModel.X);
			AssertEquals(100d, parentViewModel.Y);
			AssertEquals(100d, childViewModel.X);
			AssertEquals(140d, childViewModel.Y);
			AssertEquals(100d, grandchildViewModel.X);
			AssertEquals(180d, grandchildViewModel.Y);

			parentViewModel.X = 0;
			parentViewModel.Y = 0;
			AssertEquals("Should move parent to furthest point without causing child to leave parent bounds", 75.0, child.X);
			AssertEquals("Should move parent to furthest point without causing child to leave parent bounds (considering parent internal margin)", 160.0, child.Y);
			AssertEquals("Should move parent to furthest point without causing child to leave parent bounds", 25.0, parent.X);
			AssertEquals("Should move parent to furthest point without causing child to leave parent bounds (considering parent internal margin)", 115.0, parent.Y);
		}

		public void TestResizeParent_WhenGrandchildIsFixed_ShouldPreventMoveWhereChildIsOutsideParent()
		{
			var network = new DummyNetwork { DiagramEntity = new Entity() };
			var parent = new Entity { Width = 100, Height = 100, X = 0, Y = 0 };
			var child = new Entity { Width = 50, Height = 50, X = 0, Y = 0, Parent = parent };
			var grandchild = new Entity { Width = 25, Height = 25, X = 0, Y = 0, Parent = child };

			var networkViewModel = new NetworkViewModel(network);

			var parentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, parent);
			var childViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, child);
			var grandchildViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, grandchild);

			AssertEquals(50d, childViewModel.Width);
			AssertEquals(50d, childViewModel.Height);

			childViewModel.X = childViewModel.X; // Force refit.
			childViewModel.Y = childViewModel.Y;
			grandchildViewModel.X = grandchildViewModel.X;
			grandchildViewModel.Y = grandchildViewModel.Y;

			AssertEquals("Not big enough", 50.0, parentViewModel.GetAppropriateWidth(20));
			AssertEquals("Not big enough", 95.0, parentViewModel.GetAppropriateHeight(20));

			grandchild.EntityState = EntityState.Fixed;

			AssertEquals("Not big enough", 50.0, parentViewModel.GetAppropriateWidth(20));
			AssertEquals("Not big enough", 95.0, parentViewModel.GetAppropriateHeight(20));
		}

		public void TestFixedShape_ParentCannotDragItAround()
		{
			var root = new Entity { Width = 300, Height = 300, X = 0, Y = 0, Name = "Decarte" };
			var parent = new Entity { Width = 120, Height = 120, X = 50, Y = 50, Parent = root, Name = "Parent" };
			var child = new Entity { Width = 50, Height = 50, X = 50, Y = 50, Parent = parent, EntityState = EntityState.Fixed, Name = "Pinwheel" };

			var network = new DummyNetwork { DiagramEntity = new Entity() };
			var networkViewModel = new NetworkViewModel(network);

			var parentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, parent);
			var childViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, child);

			AssertEquals("PRE: Construct a square node of the size of the created parent", new Location(120d, 120d), new Location(parentViewModel.Width, parentViewModel.Height));
			AssertEquals("PRE: Parent has the correct coordinates", new Location(50d, 50d), new Location(parentViewModel.X, parentViewModel.Y));

			AssertEquals("PRE: Construct a square node of the size of the created child", new Location(50d, 50d), new Location(childViewModel.Width, childViewModel.Height));
			AssertEquals("PRE: Child has the correct coordinates", new Location(100d, 100d), new Location(childViewModel.X, childViewModel.Y));

			childViewModel.X += 10;
			childViewModel.Y += 10;
			AssertEquals("PRE: Should disallow moving a fixed child, even within a parent.", new Location(100d, 100d), new Location(child.X, child.Y));

			parentViewModel.X += 10;
			parentViewModel.Y += 10;
			AssertEquals("Should allow moving the parent a distance that would not cause intersection with interior child shape, but instead....", new Location(60d, 60d), new Location(parent.X, parent.Y));
			AssertEquals("Moving the parent should never cause a fixed child to be moved, but instead...", new Location(100d, 100d), new Location(child.X, child.Y));

			parentViewModel.X += 2000;
			parentViewModel.Y += 2000;
			AssertEquals("When moving a parent around its fixed child, the parent should only be able to move as far as would keep the child within its bounds, but instead...", new Location(100d, 60d), new Location(parent.X, parent.Y));
			AssertEquals("Moving the parent should never cause a fixed child to be moved, but instead...", new Location(100d, 100d), new Location(child.X, child.Y));
		}

		public void TestFixedShape_GreatAncestorCannotDragItAround()
		{
			// each new parent is 2 pixels wider than its child, and 2 pixels plus 40 plus 5 taller. The weirdness comes from inbuilt margins of 40 (top) and 5 (bottom)
			// the 2 pixel difference on either side allows for 1 pixel movement in any direction that is safe before the shape's child gets a move request
			var root = new Entity { Width = 300, Height = 300, X = 0, Y = 0, Name = "Decarte" };
			var greatestGrandParent = new Entity { Width = 58, Height = 238, X = 1, Y = 41, Parent = root, Name = "GestGP" };
			var greatGrandParent = new Entity { Width = 56, Height = 191, X = 1, Y = 41, Parent = greatestGrandParent, Name = "GGP" };
			var grandParent = new Entity { Width = 54, Height = 144, X = 1, Y = 41, Parent = greatGrandParent, Name = "GP" };
			var parent = new Entity { Width = 52, Height = 97, X = 1, Y = 41, Parent = grandParent, Name = "parent" };
			var child = new Entity { Width = 50, Height = 50, X = 1, Y = 41, Parent = parent, EntityState = EntityState.Fixed, Name = "Pinwheel" };

			var network = new DummyNetwork { DiagramEntity = new Entity() };
			var networkViewModel = new NetworkViewModel(network);

			var greatestGrandParentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, greatestGrandParent);
			var greatGrandParentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, greatGrandParent);
			var grandParentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, grandParent);
			var parentViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, parent);
			var childViewModel = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(networkViewModel, child);

			CombineAssertions("PRE: All coordinates are set correctly.", () =>
			{
				AssertEquals("greatestGrandParent: ", new Location(1, 41), new Location(greatestGrandParentViewModel.X, greatestGrandParentViewModel.Y));
				AssertEquals("greatGrandParent: ", new Location(1 + greatestGrandParentViewModel.X, 41 + greatestGrandParentViewModel.Y), new Location(greatGrandParentViewModel.X, greatGrandParentViewModel.Y));
				AssertEquals("grandparent: ", new Location(1 + greatGrandParentViewModel.X, 41 + greatGrandParentViewModel.Y), new Location(grandParentViewModel.X, grandParentViewModel.Y));
				AssertEquals("parent: ", new Location(1 + grandParentViewModel.X, 41 + grandParentViewModel.Y), new Location(parentViewModel.X, parentViewModel.Y));
				AssertEquals("child: ", new Location(5, 205), new Location(childViewModel.X, childViewModel.Y));
			});

			greatestGrandParentViewModel.X += 4;
			greatestGrandParentViewModel.Y += 4;
			CombineAssertions("Moving the greatestGrandParent shape 2 pixels should allow for movement since it doesn't try to move the fixed child, but instead...", () =>
			{
				AssertNotEquals("greatestGrandParent: ", new Location(1, 41), new Location(greatestGrandParentViewModel.X, greatestGrandParentViewModel.Y));
				AssertNotEquals("greatGrandParent: ", new Location(1 + greatestGrandParentViewModel.X, 41 + greatestGrandParentViewModel.Y), new Location(greatGrandParentViewModel.X, greatGrandParentViewModel.Y));
				AssertNotEquals("grandparent: ", new Location(1 + greatGrandParentViewModel.X, 41 + greatGrandParentViewModel.Y), new Location(grandParentViewModel.X, grandParentViewModel.Y));
				AssertNotEquals("parent: ", new Location(1 + grandParentViewModel.X, 41 + grandParentViewModel.Y), new Location(parentViewModel.X, parentViewModel.Y));
				AssertEquals("child: ", new Location(5, 205), new Location(childViewModel.X, childViewModel.Y));
			});

			greatestGrandParentViewModel.X += 100;
			greatestGrandParentViewModel.Y += 100;
			CombineAssertions("Attempting to move the greatestGrandParent a large distance should cause all our unfixed shapes to bunch together with only our fixed child staying place, but instead...", () =>
			{
				AssertEquals("greatestGrandParent: ", new Location(5, 45), new Location(greatestGrandParentViewModel.X, greatestGrandParentViewModel.Y));
				AssertEquals("greatGrandParent: ", new Location(0 + greatestGrandParentViewModel.X, 40 + greatestGrandParentViewModel.Y), new Location(greatGrandParentViewModel.X, greatGrandParentViewModel.Y));
				AssertEquals("grandparent: ", new Location(0 + greatGrandParentViewModel.X, 40 + greatGrandParentViewModel.Y), new Location(grandParentViewModel.X, grandParentViewModel.Y));
				AssertEquals("parent: ", new Location(0 + grandParentViewModel.X, 40 + grandParentViewModel.Y), new Location(parentViewModel.X, parentViewModel.Y));
				AssertEquals("child: ", new Location(5, 205), new Location(childViewModel.X, childViewModel.Y));
			});
		}

		#endregion

		#region ToolTip

		public void TestToolTip()
		{
			var entity = new Entity { Name = "Michael Scarn" };
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel);

			AssertEquals("Michael Scarn", viewModel.ToolTip);
		}

		#endregion

		#region Notification

		public void TestShouldBeNotifiedByEntity_UsingReliableWeakEventManager()
		{
			var diagramEntity = new Entity();
			var entity = new Entity { Width = 200, Height = 100 };

			var network = new DummyNetwork { DiagramEntity = diagramEntity };
			network.Entities.Add(entity);

			var networkViewModel = new NetworkViewModel(network);
			var viewModel = new NodeViewModel(entity, networkViewModel) { X = 100, Y = 200 };

			var viewModelNotified = false;
			var propertyName = string.Empty;
			viewModel.PropertyChanged += (s, e) =>
			{
				viewModelNotified = true;
				propertyName = e.PropertyName;
			};

			GC.Collect(); // to check that the manager's scope is outside the constructor on the NodeViewModel and the manager is not GC'ed

			entity.JobName = "Test";

			Assert("Should notify the view model about changes in the entity", viewModelNotified);
			AssertEquals("JobName", propertyName);
		}

		#endregion
	}
}
