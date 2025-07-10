using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CargoWise.Common;
using CargoWise.Main.Navigation.WPF;
using CargoWise.Main.Navigation.WPF.Test;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class DiagramAreaUserControlTest : TransactionedTestCase
	{
		#region Right Clicking

		public void TestRightClickingOnDiagram_ShouldUpdateDiagramContextMenu()
		{
			var action = new Mock<IDynamicNetworkAction>();
			var actionApplicability = NetworkActionAccessibility.Allowed;

			action.Setup(m => m.IsApplicable()).Returns(() =>
			{
				return actionApplicability;
			});
			action.Setup(m => m.IsEnabled()).Returns(NetworkActionAccessibility.Allowed);
			action.Setup(m => m.GetName()).Returns(ResString.GetMultilingualString("5AA4C502-323A-4930-A77E-D55DFB8EFBFA", "Frivolous action"));
			action.Setup(m => m.GetChildActions()).Returns(Array.Empty<INetworkAction>());

			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Network.AddCustomNetworkAction_ForTest(action.Object);

				config.Control.MainDiagramControl.OnDiagramContextMenuOpening();
				AssertNotNull(config.Control.ViewModel.MenuItems.SingleOrDefault(i => i?.Name == "Frivolous action"));
				config.Control.MainDiagramControl.CloseContextMenu();

				actionApplicability = NetworkActionAccessibility.Denied_ForTesting;

				config.Control.MainDiagramControl.OnDiagramContextMenuOpening();
				AssertNull("Context menu should update: the action should not appear in menu items", config.Control.ViewModel.MenuItems.SingleOrDefault(i => i?.Name == "Frivolous action"));
				config.Control.MainDiagramControl.CloseContextMenu();
			}
		}

		public void TestRightClickingOnNode_ShouldUpdateNodeContextMenu()
		{
			var action = new Mock<IDynamicNetworkAction>();
			var actionApplicability = NetworkActionAccessibility.Allowed;

			action.Setup(m => m.IsApplicable()).Returns(() =>
			{
				return actionApplicability;
			});
			action.Setup(m => m.IsEnabled()).Returns(NetworkActionAccessibility.Allowed);
			action.Setup(m => m.GetName()).Returns(ResString.GetMultilingualString("5AA4C502-323A-4930-A77E-D55DFB8EFBFA", "Frivolous action"));
			action.Setup(m => m.GetChildActions()).Returns(Array.Empty<INetworkAction>());

			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Network.AddCustomNetworkAction_ForTest(action.Object);

				config.Control.MainDiagramControl.OnNodeContextMenuOpening(config.Node1);
				AssertNotNull(config.Node1.MenuItems.SingleOrDefault(i => i?.Name == "Frivolous action"));
				config.Control.MainDiagramControl.CloseContextMenu();

				actionApplicability = NetworkActionAccessibility.Denied_ForTesting;

				config.Control.MainDiagramControl.OnNodeContextMenuOpening(config.Node1);
				AssertNull("Context menu should update: the action should not appear in menu items", config.Node1.MenuItems.SingleOrDefault(i => i?.Name == "Frivolous action"));
				config.Control.MainDiagramControl.CloseContextMenu();
			}
		}

		public void TestContextMenu_Separator_WhenClicked_ShouldNotThrowExceptions()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				AssertNoExceptionThrown(() => config.Control.MainDiagramControl.ExecuteContextMenuAction(menuItem: null));
			}
		}

		public void TestShouldSetRightClickedNode()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.IsSelected = true;
				config.Node2.IsSelected = true;

				CombineAssertions("Right clicking on node2", () =>
				{
					config.Control.MainDiagramControl.OnNodeContextMenuOpening(config.Node2);
					AssertEquals(config.Node2, config.NetworkUserControlViewModel.NetworkViewModel.RightClickedNode);
				});

				CombineAssertions("Closing the context menu", () =>
				{
					config.Control.MainDiagramControl.CloseContextMenu();
					AssertEquals(null, config.NetworkUserControlViewModel.NetworkViewModel.RightClickedNode);
				});

				CombineAssertions("Right clicking somewhere else on the diagram", () =>
				{
					config.Control.MainDiagramControl.OnDiagramContextMenuOpening();
					AssertEquals(config.NetworkUserControlViewModel.NetworkViewModel.DiagramNodeViewModel, config.NetworkUserControlViewModel.NetworkViewModel.RightClickedNode);
				});
			}
		}

		#endregion

		#region New Nodes Placement

		public void TestShouldPlaceNewNodeCenteredAtCursor_WhenCreatingWithContextMenu()
		{
			AssertPlacesNewNodeCenteredAtCursor_WhenCreatingWithContextMenu(
				mouseCursor: new Point(300, 100),
				expectedNodeBounds: new Rect(
					x: 300 - 270d / 2,
					y: 100 - 75d / 2,
					width: 270d,
					height: 75d));
		}

		public void TestShouldPlaceNewNodeCenteredAtCursor_WhenCreatingWithContextMenu_ForScaledDiagram()
		{
			AssertEquals("Precondition", 100, new Entity().ScaleUnitPixelSize);

			AssertPlacesNewNodeCenteredAtCursor_WhenCreatingWithContextMenu(
				mouseCursor: new Point(300, 100),
				expectedNodeBounds: new Rect(
					x: 300,
					y: 100,
					width: 300d, // standard entity width rounded by ScaleUnitPixelSize
					height: 75d),
				scaledDiagram: true);
		}

		void AssertPlacesNewNodeCenteredAtCursor_WhenCreatingWithContextMenu(Point mouseCursor, Rect expectedNodeBounds, bool scaledDiagram = false)
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				((Entity)config.Network.DiagramEntity).IsDiagramScaled = scaledDiagram;

				var initialNodesCount = config.NetworkViewModel.Nodes.Count();

				var diagramControl = config.Control.MainDiagramControl;
				using (OpenContextMenuAtPosition(mouseCursor.X, mouseCursor.Y, diagramControl))
				{
					var action = GetActionToCreateEntity(config.NetworkViewModel);
					diagramControl.ExecuteContextMenuAction(action);
				}
				System.Windows.Forms.Application.DoEvents();

				AssertEquals(initialNodesCount + 1, config.NetworkViewModel.Nodes.Count());
				var node = config.NetworkViewModel.Nodes.Last();

				CombineAssertions("Should place the node around the creation point", () =>
				{
					AssertEquals("Width", expectedNodeBounds.Width, node.Width);
					AssertEquals("Height", expectedNodeBounds.Height, node.Height);
					AssertEquals("X", expectedNodeBounds.X, node.X);
					AssertEquals("Y", expectedNodeBounds.Y, node.Y);
				});
			}
		}

		public void TestShouldPlaceTheFirstCreatedNodeIntoLeftTopCorner()
		{
			AssertPlacesTheFirstCreatedNodeIntoLeftTopCorner(expectedNodeLocation: new Point(30d, 25d));
		}

		public void TestShouldPlaceTheFirstCreatedNodeIntoLeftTopCorner_ForScaledDiagram()
		{
			var scaleUnitPixelSize = new Entity().ScaleUnitPixelSize;
			AssertEquals("Precondition", 100, scaleUnitPixelSize);

			AssertPlacesTheFirstCreatedNodeIntoLeftTopCorner(expectedNodeLocation: new Point(scaleUnitPixelSize, 25d), scaledDiagram: true);
		}

		void AssertPlacesTheFirstCreatedNodeIntoLeftTopCorner(Point expectedNodeLocation, bool scaledDiagram = false)
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				((Entity)config.Network.DiagramEntity).IsDiagramScaled = scaledDiagram;

				var initialNodesCount = config.NetworkViewModel.Nodes.Count();

				var diagramControl = config.Control.MainDiagramControl;
				using (OpenContextMenuAtPosition(300, 100, diagramControl)) // some random values to ensure that the result does not depend on the mouse position
				{
					var action = GetActionToCreateEntity(config.NetworkViewModel);
					diagramControl.ExecuteRibbonAction(action);
				}
				System.Windows.Forms.Application.DoEvents();

				AssertEquals(initialNodesCount + 1, config.NetworkViewModel.Nodes.Count());
				var node = config.NetworkViewModel.Nodes.Last();

				CombineAssertions("Should place the node in the left top corner", () =>
				{
					AssertEquals("X", expectedNodeLocation.X, node.X);
					AssertEquals("Y", expectedNodeLocation.Y, node.Y);
				});
			}
		}

		public void TestShouldPlaceNewNodeStaggeredNextToPreviousOne_WhenCreatingWithRibbon()
		{
			AssertPlacesNewNodeStaggeredNextToPreviousOne_WhenCreatingWithRibbon(
				node1CreationPosition: new Point(300, 110),
				expectedNode1Bounds: new Rect(
					x: 165d, // 300 - 270d / 2
					y: 72.5d, // 110 - 75d / 2
					width: 270d, // standard node width
					height: 75d), // standard node height
				expectedNode2Location: new Point(
					x: 165d + 30 /* StaggeringXOffset */,
					y: 72.5d + 25 /* StaggeringYOffset */),
				expectedNode3Location: new Point(
					x: 165d + 2 * 30,
					y: 72.5d + 2 * 25));
		}

		public void TestShouldPlaceNewNodeStaggeredNextToPreviousOne_WhenCreatingWithRibbon_ForScaledDiagram()
		{
			AssertEquals("Precondition", 100, new Entity().ScaleUnitPixelSize);

			AssertPlacesNewNodeStaggeredNextToPreviousOne_WhenCreatingWithRibbon(
				node1CreationPosition: new Point(300, 110),
				expectedNode1Bounds: new Rect(
					x: 300,
					y: 110,
					width: 300d, // standard node width rounded by ScaleUnitPixelSize
					height: 75d), // standard node height
				expectedNode2Location: new Point(
					x: 300d + 100 /* ScaleUnitPixelSize */,
					y: 110d + 25 /* StaggeringYOffset */),
				expectedNode3Location: new Point(
					x: 300d + 2 * 100,
					y: 110d + 2 * 25),
				scaledDiagram: true);
		}

		void AssertPlacesNewNodeStaggeredNextToPreviousOne_WhenCreatingWithRibbon(
			Point node1CreationPosition,
			Rect expectedNode1Bounds,
			Point expectedNode2Location,
			Point expectedNode3Location,
			bool scaledDiagram = false)
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				((Entity)config.Network.DiagramEntity).IsDiagramScaled = scaledDiagram;

				var initialNodesCount = config.NetworkViewModel.Nodes.Count();

				var diagramControl = config.Control.MainDiagramControl;
				var node1 = CreateNodeAtPosition(node1CreationPosition.X, node1CreationPosition.Y, diagramControl, config.NetworkViewModel);
				AssertEquals(initialNodesCount + 1, config.NetworkViewModel.Nodes.Count());

				CombineAssertions("Precondition: should place the node around the creation point", () =>
				{
					AssertEquals("Width", expectedNode1Bounds.Width, node1.Width);
					AssertEquals("Height", expectedNode1Bounds.Height, node1.Height);
					AssertEquals("X", expectedNode1Bounds.X, node1.X);
					AssertEquals("Y", expectedNode1Bounds.Y, node1.Y);
				});

				var action = GetActionToCreateEntity(config.NetworkViewModel);
				diagramControl.ExecuteRibbonAction(action);
				System.Windows.Forms.Application.DoEvents();

				AssertEquals(initialNodesCount + 2, config.NetworkViewModel.Nodes.Count());
				var node2 = config.NetworkViewModel.Nodes.Last();

				CombineAssertions("Should place the node staggered next to the previous one", () =>
				{
					AssertEquals("X", expectedNode2Location.X, node2.X);
					AssertEquals("Y", expectedNode2Location.Y, node2.Y);
				});

				diagramControl.ExecuteRibbonAction(action);
				System.Windows.Forms.Application.DoEvents();

				AssertEquals(initialNodesCount + 3, config.NetworkViewModel.Nodes.Count());
				var node3 = config.NetworkViewModel.Nodes.Last();

				CombineAssertions("Should place another one staggered next to the previous one", () =>
				{
					AssertEquals("X", expectedNode3Location.X, node3.X);
					AssertEquals("Y", expectedNode3Location.Y, node3.Y);
				});
			}
		}

		public void TestShouldPlaceNewNodeStaggeredNextToTheLastOneFromCollectionOfPreviouslyCreatedNodes_WhenCreatingWithRibbon()
		{
			AssertPlacesNewNodeStaggeredNextToTheLastOneFromCollectionOfPreviouslyCreatedNodes_WhenCreatingWithRibbon(
				node1CreationPosition: new Point(300, 110),
				expectedNode1Location: new Point(
					x: 300d,
					y: 110d),
				expectedNode2Location: new Point(
					x: 300d + 30 /* StaggeringXOffset */,
					y: 110d + 25 /* StaggeringYOffset */),
				expectedNode3Location: new Point(
					x: 300d + 2 * 30,
					y: 110d + 2 * 25));
		}

		public void TestShouldPlaceNewNodeStaggeredNextToTheLastOneFromCollectionOfPreviouslyCreatedNodes_WhenCreatingWithRibbon_ForScaledDiagram()
		{
			AssertEquals("Precondition", 100, new Entity().ScaleUnitPixelSize);

			AssertPlacesNewNodeStaggeredNextToTheLastOneFromCollectionOfPreviouslyCreatedNodes_WhenCreatingWithRibbon(
				node1CreationPosition: new Point(300, 110),
				expectedNode1Location: new Point(
					x: 300d,
					y: 110d),
				expectedNode2Location: new Point(
					x: 300d + 100 /* ScaleUnitPixelSize */,
					y: 110d + 25 /* StaggeringYOffset */),
				expectedNode3Location: new Point(
					x: 300d + 2 * 100,
					y: 110d + 2 * 25),
				scaledDiagram: true);
		}

		void AssertPlacesNewNodeStaggeredNextToTheLastOneFromCollectionOfPreviouslyCreatedNodes_WhenCreatingWithRibbon(
			Point node1CreationPosition,
			Point expectedNode1Location,
			Point expectedNode2Location,
			Point expectedNode3Location,
			bool scaledDiagram = false)
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				((Entity)config.Network.DiagramEntity).IsDiagramScaled = scaledDiagram;

				var initialNodesCount = config.NetworkViewModel.Nodes.Count();

				var diagramControl = config.Control.MainDiagramControl;
				var node1 = CreateSeveralNodesAtPositionAndGetLastNode(node1CreationPosition.X, node1CreationPosition.Y, diagramControl, config.NetworkViewModel);
				AssertEquals(initialNodesCount + 3, config.NetworkViewModel.Nodes.Count());

				CombineAssertions("Precondition: should place new nodes at the creation point", () =>
				{
					AssertEquals("X", expectedNode1Location.X, node1.X);
					AssertEquals("Y", expectedNode1Location.Y, node1.Y);
				});

				var action = GetActionToCreateCollectionOfEntities(config.NetworkViewModel);
				diagramControl.ExecuteRibbonAction(action);
				System.Windows.Forms.Application.DoEvents();

				AssertEquals(initialNodesCount + 6, config.NetworkViewModel.Nodes.Count());
				var node2 = config.NetworkViewModel.Nodes.Last();

				CombineAssertions("Should place the node staggered next to the previous one", () =>
				{
					AssertEquals("X", expectedNode2Location.X, node2.X);
					AssertEquals("Y", expectedNode2Location.Y, node2.Y);
				});

				diagramControl.ExecuteRibbonAction(action);
				System.Windows.Forms.Application.DoEvents();

				AssertEquals(initialNodesCount + 9, config.NetworkViewModel.Nodes.Count());
				var node3 = config.NetworkViewModel.Nodes.Last();

				CombineAssertions("Should place another one staggered next to the previous one", () =>
				{
					AssertEquals("X", expectedNode3Location.X, node3.X);
					AssertEquals("Y", expectedNode3Location.Y, node3.Y);
				});
			}
		}

		public void TestShouldPlaceNewNodeWithinViewport_EvenIfPreviousNodeIsOutsideViewport_WhenCreatingWithRibbon()
		{
			var node1CreationPosition = new Point(300, 110);

			var expectedNode1Location = new Point(
				x: 300 - 270d /* Standard node width */ / 2,
				y: 110 - 75d /* Standard node height */ / 2);
			AssertEquals(165d, expectedNode1Location.X);
			AssertEquals(72.5d, expectedNode1Location.Y);

			var expectedNode2Location = new Point(
				x: 165d + 30 /* StaggeringXOffset */,
				y: 72.5d + 25 /* StaggeringYOffset */);

			var scrollPositionBeforeCreatingNode3 = new Point( // some random scrolling
				x: 650,
				y: 430);

			var expectedNode3Location = new Point(
				x: 165d + 2 * 30 + 650,
				y: 72.5d + 2 * 25 + 430);

			AssertPlacesNewNodeWithinViewport_EvenIfPreviousNodeIsOutsideViewport_WhenCreatingWithRibbon(
				node1CreationPosition,
				expectedNode1Location,
				expectedNode2Location,
				scrollPositionBeforeCreatingNode3,
				expectedNode3Location);
		}

		public void TestShouldPlaceNewNodeWithinViewport_EvenIfPreviousNodeIsOutsideViewport_WhenCreatingWithRibbon_ForScaledDiagram()
		{
			AssertEquals("Precondition", 100, new Entity().ScaleUnitPixelSize);

			var node1CreationPosition = new Point(300, 110);
			var expectedNode1Location = new Point(300, 110);

			var expectedNode2Location = new Point(
				x: 300d + 100 /* ScaleUnitPixelSize */,
				y: 110d + 25 /* StaggeringYOffset */);

			var scrollPositionBeforeCreatingNode3 = new Point( // some random scrolling
				x: 650,
				y: 430);

			var expectedNode3LocationNotSnappedToGrid = new Point(
				x: 300d + 2 * 100 + 650,
				y: 110d + 2 * 25 + 430);
			AssertEquals(1150d, expectedNode3LocationNotSnappedToGrid.X);
			AssertEquals(590d, expectedNode3LocationNotSnappedToGrid.Y);
			var expectedNode3LocationSnappedToGrid = new Point(1200 /* Rounded by ScaleUnitPixelSize */, 590);

			AssertPlacesNewNodeWithinViewport_EvenIfPreviousNodeIsOutsideViewport_WhenCreatingWithRibbon(
				node1CreationPosition,
				expectedNode1Location,
				expectedNode2Location,
				scrollPositionBeforeCreatingNode3,
				expectedNode3LocationSnappedToGrid,
				scaledDiagram: true);
		}

		void AssertPlacesNewNodeWithinViewport_EvenIfPreviousNodeIsOutsideViewport_WhenCreatingWithRibbon(
			Point node1CreationPosition,
			Point expectedNode1Location,
			Point expectedNode2Location,
			Point scrollPositionBeforeCreatingNode3,
			Point expectedNode3Location,
			bool scaledDiagram = false)
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				((Entity)config.Network.DiagramEntity).IsDiagramScaled = scaledDiagram;

				var diagramControl = config.Control.MainDiagramControl;
				var farmostNodeToExtendContentBoundaries = CreateNodeAtPosition(10000, 5000, diagramControl, config.NetworkViewModel);

				var initialNodesCount = config.NetworkViewModel.Nodes.Count();

				// scroll back to left top
				diagramControl.ViewModel.ContentOffsetX = 0;
				config.NetworkUserControlViewModel.ContentOffsetY = 0;

				var node1 = CreateNodeAtPosition(node1CreationPosition.X, node1CreationPosition.Y, diagramControl, config.NetworkViewModel);
				AssertEquals(initialNodesCount + 1, config.NetworkViewModel.Nodes.Count());

				CombineAssertions("Precondition: should place the node around the creation point", () =>
				{
					AssertEquals("X", expectedNode1Location.X, node1.X);
					AssertEquals("Y", expectedNode1Location.Y, node1.Y);
				});

				var action = GetActionToCreateEntity(config.NetworkViewModel);
				diagramControl.ExecuteRibbonAction(action);
				System.Windows.Forms.Application.DoEvents();

				AssertEquals(initialNodesCount + 2, config.NetworkViewModel.Nodes.Count());
				var node2 = config.NetworkViewModel.Nodes.Last();

				CombineAssertions("Should place the node staggered next to the previous one", () =>
				{
					AssertEquals("X", expectedNode2Location.X, node2.X);
					AssertEquals("Y", expectedNode2Location.Y, node2.Y);
				});

				// scroll
				diagramControl.ViewModel.ContentOffsetX = scrollPositionBeforeCreatingNode3.X;
				config.NetworkUserControlViewModel.ContentOffsetY = scrollPositionBeforeCreatingNode3.Y;

				diagramControl.ExecuteRibbonAction(action);
				System.Windows.Forms.Application.DoEvents();

				AssertEquals(initialNodesCount + 3, config.NetworkViewModel.Nodes.Count());
				var node3 = config.NetworkViewModel.Nodes.Last();

				CombineAssertions("Should place another one staggered but withing the viewport", () =>
				{
					AssertEquals("X", expectedNode3Location.X, node3.X);
					AssertEquals("Y", expectedNode3Location.Y, node3.Y);
				});
			}
		}

		public void TestShouldPlaceNewNodesVisibleWithinViewport()
		{
			AssertPlacesNewNodesVisibleWithinViewport(
				scrollPositionBeforeCreatingNodes: new Point(650, 430), // some random scrolling
				numOfCreatedNodes: 50);
		}

		public void TestShouldPlaceNewNodesVisibleWithinViewport_ForScaledDiagram()
		{
			AssertPlacesNewNodesVisibleWithinViewport(
				scrollPositionBeforeCreatingNodes: new Point(650, 430), // some random scrolling
				numOfCreatedNodes: 50,
				scaledDiagram: true);
		}

		void AssertPlacesNewNodesVisibleWithinViewport(Point scrollPositionBeforeCreatingNodes,
			int numOfCreatedNodes,
			bool scaledDiagram = false)
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				((Entity)config.Network.DiagramEntity).IsDiagramScaled = scaledDiagram;

				var diagramControl = config.Control.MainDiagramControl;
				var farmostNodeToExtendContentBoundaries = CreateNodeAtPosition(10000, 5000, diagramControl, config.NetworkViewModel);

				var initialNodesCount = config.NetworkViewModel.Nodes.Count();

				diagramControl.ViewModel.ContentOffsetX = scrollPositionBeforeCreatingNodes.X;
				config.NetworkUserControlViewModel.ContentOffsetY = scrollPositionBeforeCreatingNodes.Y;

				var action = GetActionToCreateEntity(config.NetworkViewModel);

				for (int i = 0; i < numOfCreatedNodes; i++)
				{
					diagramControl.ExecuteRibbonAction(action);
					System.Windows.Forms.Application.DoEvents();

					AssertEquals(initialNodesCount + 1, config.NetworkViewModel.Nodes.Count());
					initialNodesCount++;
					var node = config.NetworkViewModel.Nodes.Last();

					CombineAssertions("Should place the node within the viewport", () =>
					{
						Assert("Should be to the right from the left edge of the viewport", node.X + node.Width > diagramControl.ViewModel.ContentOffsetX);
						Assert("Should be to the left from the right edge of the viewport", node.X < diagramControl.ViewModel.ContentOffsetX + diagramControl.ViewModel.ContentViewportWidth);

						Assert("Should be to the bottom from the top edge of the viewport", node.Y + node.Height > diagramControl.ViewModel.ContentOffsetY);
						Assert("Should be to the top from the bottom edge of the viewport", node.Y < diagramControl.ViewModel.ContentOffsetY + diagramControl.ViewModel.ContentViewportHeight);
					});
				}
			}
		}

#pragma warning disable CS0618
		DisposableAction OpenContextMenuAtPosition(double mousePositionX, double mousePositionY, DiagramAreaUserControl diagramControl)
		{
			var mouse = new Point(mousePositionX, mousePositionY);
			diagramControl.SetMousePosition_ForTest(mouse);
			diagramControl.OnDiagramContextMenuOpening();

			return new DisposableAction(() => diagramControl.CloseContextMenu());
		}
#pragma warning restore CS0618

		INetworkAction GetActionToCreateEntity(INetworkViewModel networkViewModel)
		{
			return new StaticNetworkAction(() =>
			{
				var entity = new Entity();
				NetworkVisualisationTestHelper.AddEntityToNetwork(networkViewModel.Network, entity);
				return entity;
			});
		}

		INetworkAction GetActionToCreateCollectionOfEntities(INetworkViewModel networkViewModel)
		{
			return new StaticNetworkAction(() =>
			{
				var entity1 = new Entity();
				var entity2 = new Entity();
				var entity3 = new Entity();
				NetworkVisualisationTestHelper.AddEntityToNetwork(networkViewModel.Network, entity1);
				NetworkVisualisationTestHelper.AddEntityToNetwork(networkViewModel.Network, entity2);
				NetworkVisualisationTestHelper.AddEntityToNetwork(networkViewModel.Network, entity3);
				return new EntityCollection(new INetworkEntity[] { entity1, entity2, entity3 });
			});
		}

#pragma warning disable CS0618
		NodeViewModel CreateNodeAtPosition(double positionX, double positionY, DiagramAreaUserControl diagramControl, NetworkViewModel networkViewModel)
		{
			return ExecuteActionAtPositionAndGetLastNode(GetActionToCreateEntity(networkViewModel), positionX, positionY, diagramControl, networkViewModel);
		}
#pragma warning restore CS0618

#pragma warning disable CS0618
		NodeViewModel CreateSeveralNodesAtPositionAndGetLastNode(double positionX, double positionY, DiagramAreaUserControl diagramControl, NetworkViewModel networkViewModel)
		{
			return ExecuteActionAtPositionAndGetLastNode(GetActionToCreateCollectionOfEntities(networkViewModel), positionX, positionY, diagramControl, networkViewModel);
		}
#pragma warning restore CS0618

#pragma warning disable CS0618
		NodeViewModel ExecuteActionAtPositionAndGetLastNode(INetworkAction action, double positionX, double positionY, DiagramAreaUserControl diagramControl, NetworkViewModel networkViewModel)
		{
			using (OpenContextMenuAtPosition(positionX, positionY, diagramControl))
			{
				diagramControl.ExecuteContextMenuAction(action);
			}
			System.Windows.Forms.Application.DoEvents();
			return networkViewModel.Nodes.Last();
		}
#pragma warning restore CS0618

		#endregion

		#region Adjust Width and Height

		public void TestAdjustWidthAndHeight_ShouldAdjustZoomAndPanViewport()
		{
			// This test ensures that the nonscheduled section does not scroll oddly (you should not be able to scroll down beyond the diagram boundaries and see white space).
			// This is currently achieved by calling ZoomAndPanControl.UpdateContentViewportSize() in AdjustContentHeight.

			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				var mainZoomControl = config.Control.MainDiagramControl.FindChildren<ZoomAndPanControl>().Single();
				var nonScheduledZoomControl = config.Control.NonScheduledDiagramControl.FindChildren<ZoomAndPanControl>().Single();

				var mainContentScaleChangedCount = 0;
				var nonScheduledContentScaleChangedCount = 0;

				mainZoomControl.ContentViewportSizeUpdated += (sender, args) => mainContentScaleChangedCount++;
				nonScheduledZoomControl.ContentViewportSizeUpdated += (sender, args) => nonScheduledContentScaleChangedCount++;

				NetworkTestHelper.SimulateDraggingNodeToNewPosition(config.RegularNode1, 1000, 1000, config.NetworkView);

				CombineAssertions("Both viewport sizes should be updated whenever the regular width and height is adjusted. SAD!", () =>
				{
					AssertEquals("Regular zoom control", 1, mainContentScaleChangedCount);
					AssertEquals("Nonscheduled zoom control", 1, nonScheduledContentScaleChangedCount);
				});

				NetworkTestHelper.SimulateDraggingNodeToNewPosition(config.NonScheduledNode1, 1000, 1000, config.NonScheduledNetworkView);

				CombineAssertions("Both viewport sizes should be updated whenever the nonscheduled width and height is adjusted. SAD!", () =>
				{
					AssertEquals("Regular zoom control", 2, mainContentScaleChangedCount);
					AssertEquals("Nonscheduled zoom control", 2, nonScheduledContentScaleChangedCount);
				});
			}
		}

		#endregion

		#region Channels

		public void TestContentHeight_ForNonchanneledDiagram()
		{
			AssertContentHeightForChannels("There are no channels, so the default height should be used. SAD!", expectedContentHeight, new Entity(), Array.Empty<IDiagramChannel>());
		}

		public void TestContentHeight_ForChanneledDiagram_WithOneChannel()
		{
			AssertContentHeightForChannels("There is one channel, so the height should be 600px for the channel plus 600px for the Non-channeled space. SAD!", 1200 + HeightOffset, new Entity(), DummyChannel.Create("Shmlonathan", "Non-channeled"));
		}

		public void TestContentHeight_ForChanneledDiagram_WithTwoChannels()
		{
			AssertContentHeightForChannels("There are two channels, so the height should be 600px for each channel plus 600px for the Non-channeled space. SAD!", 1800 + HeightOffset, new Entity(), DummyChannel.Create("Shmlonathan", "Shmlangela", "Non-channeled"));
		}

		public void TestContentHeight_ForChanneledDiagram_WithChannelsOfVaryingHeights()
		{
			AssertContentHeightForChannels("There are is a custom height specified, so the height should be the sum of the actual heights of the channels. SAD!", 1600 + HeightOffset, new Entity(), new[] { new DummyChannel("Shmlonathan"), new DummyChannel("Shmlangela", 400), new DummyChannel("Non-channeled") });
		}

		public void TestContentHeight_ForChanneledDiagram_WithChannelLessThanMinimumHeight_ShouldUseMinimumHeight()
		{
			AssertContentHeightForChannels("There are is a custom height specified which is less than the minimum height, so the minimum height should be used instead. SAD!", 700 + HeightOffset, new Entity(), new[] { new DummyChannel("Shmlonathan", 69), new DummyChannel("Non-channeled") });
		}

		public void TestContentHeight_ForChanneledDiagram_WithChannelGreaterThanMaximumHeight_ShouldUseMaximumHeight()
		{
			AssertContentHeightForChannels("There are is a custom height specified which is greater than the maximum height, so the maximum height should be used instead. SAD!", 2600 + HeightOffset, new Entity(), new[] { new DummyChannel("Shmlonathan", 3000), new DummyChannel("Non-channeled") });
		}

		public void TestContentHeight_ForFixedChanneledDiagram_SpecifiedHeightLessThanChannels()
		{
			var diagram = new Entity
			{
				IsDiagramSurfaceFixed = true,
				Height = 1000
			};

			AssertContentHeightForChannels("The fixed height is less than required for the channels, so the minimum channel height should be used. SAD!", 1200 + HeightOffset, diagram, DummyChannel.Create("Shmlonathan", "Non-channeled"));
		}

		public void TestContentHeight_ForFixedChanneledDiagram_SpecifiedHeightGreaterThanChannels()
		{
			var diagram = new Entity { IsDiagramSurfaceFixed = true, Height = 1300 };

			AssertContentHeightForChannels("The fixed height is greater than required for the channels, so the fixed height should be used. SAD!", 1300 + HeightOffset, diagram, DummyChannel.Create("Shmlonathan", "Non-channeled"));
		}

		public void TestContentHeight_ForChanneledDiagram_WithNodesNotExceedingDefaultHeight()
		{
			var diagram = new Entity();
			var entity = new Entity
			{
				Y = 1100,
				Height = 50,
				Parent = diagram
			};

			AssertContentHeightForChannels("There is a node on the unfixed diagram, but its bounds do not exceed the minimum for the number of channels specified, so the channel height should be used. SAD!",
				1200 + HeightOffset, diagram, DummyChannel.Create("Shmlonathan", "Non-channeled"));
		}

		public void TestContentHeight_ForChanneledDiagram_WithNodesExceedingDefaultHeight()
		{
			var diagram = new Entity();

			var entity = new Entity
			{
				Y = 1300,
				Height = 50,
				Parent = diagram
			};

			AssertContentHeightForChannels("There is a node on the unfixed diagram whose bounds exceed the minimum for the number of channels specified, so the height specified by that node should be used. SAD!", 1350 + HeightOffset, diagram, DummyChannel.Create("Shmlonathan", "Non-channeled"));
		}

		static void AssertContentHeightForChannels(string message, double expectedHeight, Entity diagram, IEnumerable<IDiagramChannel> channels)
		{
			diagram.IsDiagramScaled = true;
			diagram.DiagramChannels = channels;

			using (var config = NetworkGuiTestConfig.Create(diagram))
			{
				AssertEquals(message, expectedHeight, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentHeight));
			}
		}

		const int HeightOffset = 45;
		const double ChannelHeaderWidth = 100d;

		public void TestContentWidth_ForChanneledDiagram_ShouldBeSameAsWithoutChannels()
		{
			var diagram = new Entity
			{
				IsDiagramScaled = true,
				DiagramChannels = new[] { new DummyChannel("Shmlonathan"), new DummyChannel("Non-channeled") }
			};

			using (var config = NetworkGuiTestConfig.Create(diagram))
			{
				AssertEquals("There are channels, but there's also a margin, so the net ContentWidth should be the same. SAD!", expectedContentWidth, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentWidth));
			}
		}

		public void TestAdjustWidthAndHeight_ForFixedDiagram_WithChannels_ShouldAccommodateChannelHeaders()
		{
			var diagram = new Entity
			{
				IsDiagramScaled = true,
				IsDiagramSurfaceFixed = true,
				Width = 1000,
				Height = 1000,
				DiagramChannels = new[] { new DummyChannel("Shmlonathan"), new DummyChannel("Non-channeled") }
			};

			using (var config = NetworkGuiTestConfig.Create(diagram))
			{
				AssertEquals("The fixed diagram has channels, so the width should accommodate the channel headers. SAD!", 1105d, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentWidth));
			}
		}

		public void TestMainContentStackPanel_WhenChannelHeadersPresent_ShouldHaveLeftMargin()
		{
			var diagram = new Entity
			{
				IsDiagramScaled = true,
				DiagramChannels = new[] { new DummyChannel("Shmlonathan"), new DummyChannel("Non-channeled") },
			};

			using (var config = NetworkGuiTestConfig.Create(diagram))
			{
				AssertEquals("The StackPanel should have a left margin equal to the channel header width. SAD!", ChannelHeaderWidth, config.Control.MainDiagramControl.ViewModel.ContentLeftMargin, 0.5);
				AssertEquals("The ContentWidth should be the same with/without channel headers so that we don't have unnecessary scrollbars. SAD!", expectedContentWidth, config.Control.MainDiagramControl.ViewModel.ContentWidth, 0.5);
			}
		}

		public void TestChannelHeaders_WithNoChannelsDefined_ShouldNotBeVisible()
		{
			var diagram = new Entity { IsDiagramScaled = true };

			using (var config = NetworkGuiTestConfig.Create(diagram))
			{
				var channelHeaders = NetworkTestHelper.GetChannelHeaders(config.Window);
				AssertContainsExactElementsInAnyOrder("No channel headers should have been created. SAD!", Array.Empty<string>(), channelHeaders.Select(x => x.Text));

				var lines = NetworkTestHelper.GetChannelLines(config.Window);
				AssertEquals("No lines should have been drawn for channels. SAD!", 0, lines.Count());
			}
		}

		public void TestControl_ForShapeOnChanneledDiagram_WhenOpenedAsDiagram_ShouldNotShowChannelsOrHeaders()
		{
			var diagram = new Entity
			{
				IsDiagramScaled = true,
				DiagramChannels = new[] { new DummyChannel("Jan Michael Vincent"), new DummyChannel("Non-channeled") },
			};

			using (var config = NetworkGuiTestConfig.Create(diagram))
			{
				var shape = new Entity
				{
					IsDiagramScaled = true,
					IsDiagramSurfaceFixed = true,
					CanHaveChildren = true,
					ShapeType = ShapeTypes.Shape,
					Scale = 100,
					ResolutionIncrement = 50,
					ScaleUnitPixelSize = 200,
					Width = 600,
					CornerRadius = 10,
					Name = "sub-diagram",
					Height = 195,
					Parent = config.Network.DiagramEntity,
				};

				config.Network.Entities.Add(shape);

				config.Control.MainDiagramControl.ViewModel.CreateNewNode(new Location(35, 111), shape);
				var entity = new Entity { ShapeType = ShapeTypes.Shape, Name = "Shape1" };
				entity.Parent = shape;
				config.Network.Entities.Add(entity);

				//open sub-diagram in its own window
				var windowForShapeAsDiagram = new Window();
				var refresher2 = new NetworkRefresher();
				var network2 = new DummyNetwork { Refresher = refresher2 };
				refresher2.AssociateWithNetwork(network2);
				network2.DiagramEntity = shape;

#pragma warning disable CS0618
				using (var control2 = new NetworkUserControl(diagram, refresher2))
				{
					control2.SetDataContext(network2, isReloading: false);
					windowForShapeAsDiagram.Content = control2;
					control2.Height = 600;
					control2.Width = 1000;
					network2.Entities.Add(entity);

					var channelHeadersForMainDiagram = NetworkTestHelper.GetChannelHeaders(config.Window);
					AssertContainsExactElementsInAnyOrder("The main diagram should have channel headers. SAD!", new[] { "Jan Michael Vincent", "Non-channeled" }, channelHeadersForMainDiagram.Select(x => x.Text));

					var channelHeadersForShapeAsDiagram = NetworkTestHelper.GetChannelHeaders(windowForShapeAsDiagram);
					AssertContainsExactElementsInAnyOrder("No channel headers should have been created when showing the child shape as a diagram surface, even though its parent is a diagram with channels. SAD!", Array.Empty<string>(), channelHeadersForShapeAsDiagram.Select(x => x.Text));
				}
#pragma warning restore CS0618
			}
		}

		const double expectedContentWidth = 773d;
		const double expectedContentHeight = 423d;

		#endregion

		#region Zoom

		public void TestZoomIn_ShouldRefreshScrollViewer()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var hasScrollViewerRefreshed = false;
				var control = config.Control.MainDiagramControl;
				control.ScrollViewerRefreshed += (sender, args) => hasScrollViewerRefreshed = true;

				control.ZoomIn();
				ApplicationHelper.DoEvents();
				AssertEquals("Zooming in should have refreshed the scroll position. If this doesn't happen, sometimes the diagram background doesn't hug the edges of the form properly. SAD!", true, hasScrollViewerRefreshed);
			}
		}

#pragma warning disable CS0618
		public void TestZoomOut_ShouldNotThrow_WhenDataContextIsNotSet()
		{
			var control = new DiagramAreaUserControl();
			var viewModel = control.DataContext as DiagramAreaUserControlViewModel;
			AssertNotNull("Precondition: Initial context should be set", viewModel);
			AssertNull("Precondition: ...but NetworkControlViewModel on it should not yet be set", viewModel.NetworkControlViewModel);

			AssertNoExceptionThrown("Should not throw", () => control.ZoomOut());
		}
#pragma warning restore CS0618

		#endregion

		#region Connectors

		public void TestConnectorRemoveAdorner_PositionedCorrectlyWhenMousedOver_NoChannels()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var networkViewModel = new NetworkViewModel(network);
			var node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "First entity");
			var node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Second entity");
			node2.X = 400;

			var relationship = network.CreateRelationship(node1.Entity, node2.Entity);
			Relationship rel = (Relationship)network.Entities[1].Links.Single();
			rel.IsVisible = true;

			using (var config = NetworkGuiTestConfig.Create(diagramEntity))
			{
				var diagramControl = config.Control.MainDiagramControl;
				var mousePos = new Point(800, 600); // Just a random location on the diagram
				var mouseOverride = new OverridableMouse(mousePos);

				var adornedControl = (AdornedControl)WpfUtils.FindElementWithType<CurvedArrow>(diagramControl).Parent;
				adornedControl.OverrideMouse_ForTest(mouseOverride);
				adornedControl.ShowAdorner();

				AssertNotNull(adornedControl);

				var canvas = adornedControl.AdornerContent;
				var canvasPos = canvas.TranslatePoint(new Point(0.0, 0.0), null);

				AssertEquals(mousePos.X, canvasPos.X);
				AssertEquals(mousePos.Y - 30, canvasPos.Y);
			}
		}

		public void TestConnectorRemoveAdorner_PositionedCorrectlyWhenMousedOver_Channels()
		{
			var diagramEntity = new Entity { IsDiagramScaled = true };
			var network = new DummyNetwork { DiagramEntity = diagramEntity };

			var channel1 = new DummyChannel("Bend", 200);
			var channel2 = new DummyChannel("Snap", 400);

			diagramEntity.DiagramChannels = new[] { channel1, channel2 };

			var networkViewModel = new NetworkViewModel(network);
			var node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "First entity");
			var node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Second entity");
			node2.X = 400;

			var relationship = network.CreateRelationship(node1.Entity, node2.Entity);
			Relationship rel = (Relationship)network.Entities[1].Links.Single();
			rel.IsVisible = true;

			using (var config = NetworkGuiTestConfig.Create(diagramEntity))
			{
				var diagramControl = config.Control.MainDiagramControl;
				var mousePos = new Point(800, 600); // Just a random location on the diagram
				var mouseOverride = new OverridableMouse(mousePos);

				var adornedControl = (AdornedControl)WpfUtils.FindElementWithType<CurvedArrow>(diagramControl).Parent;
				adornedControl.OverrideMouse_ForTest(mouseOverride);
				adornedControl.ShowAdorner();

				AssertNotNull(adornedControl);

				var canvas = adornedControl.AdornerContent;
				var canvasPos = canvas.TranslatePoint(new Point(0.0, 0.0), null);

				AssertEquals(mousePos.X, canvasPos.X);
				AssertEquals(mousePos.Y - 30, canvasPos.Y);
			}
		}

		#endregion
	}

#pragma warning disable CS0618
	[TestedType(typeof(DiagramAreaUserControl))]
	class DiagramAreaUserControlBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				return config.Control.MainDiagramControl;
			}
		}
	}
#pragma warning restore CS0618

}
