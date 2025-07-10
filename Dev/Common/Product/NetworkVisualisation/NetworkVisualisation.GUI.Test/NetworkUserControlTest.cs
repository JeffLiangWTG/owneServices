using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CargoWise.Main.Navigation.WPF.Test;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class NetworkUserControlTest : TransactionedTestCase
	{
		#region Selection

		public void TestSelecting_WhenClickLeftMouseButtonAndMove_ShouldCreateSelectRectagle()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.NetworkView.Focus();

				config.NetworkView.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(0, 0));
				config.NetworkView.RaiseEvent(config.MouseController.CreateMouseMoveEvent(100, 100));

				var dragSelection = (Canvas)config.NetworkView.FindName("DragSelection");

				AssertEquals(Visibility.Visible, dragSelection.Visibility);

				config.NetworkView.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));

				AssertEquals(Visibility.Collapsed, dragSelection.Visibility);
			}
		}

		public void TestSelecting_WhenRectagleTouchAnyShape_ShouldSelectIt()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.Width = 300;
				config.Node1.Height = 150;
				config.Node1.X = 35;
				config.Node1.Y = 35;
				config.Node2.Width = 300;
				config.Node2.Height = 150;
				config.Node2.X = 435;
				config.Node2.Y = 435;

				config.NetworkView.Focus();

				config.NetworkView.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(0, 0));
				config.NetworkView.RaiseEvent(config.MouseController.CreateMouseMoveEvent(100, 100));
				config.NetworkView.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));

				AssertEquals(true, config.Node1.IsSelected);
				AssertNotEquals(true, config.Node2.IsSelected);
			}
		}

		public void TestSelecting_CtrlKeyPressPreviousSelectedShapes_ShouldStaySelected()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.Width = 300;
				config.Node1.Height = 150;
				config.Node1.X = 35;
				config.Node1.Y = 35;
				config.Node2.Width = 300;
				config.Node2.Height = 150;
				config.Node2.X = 435;
				config.Node2.Y = 435;

				config.Node2.IsSelected = true;

				config.NetworkView.Focus();
				var dragSelection = (Canvas)config.NetworkView.FindName("DragSelection");
				config.MouseController.SetCurrentMousePosition_ForTest(10, 10);
				config.NetworkView.StartSelection(dragSelection, retainExisting: true);
				config.MouseController.SetCurrentMousePosition_ForTest(300, 300);
				config.NetworkView.UpdateSelection();
				config.NetworkView.EndSelection();

				AssertEquals(true, config.Node1.IsSelected);
				AssertEquals(true, config.Node2.IsSelected);
			}
		}

		public void TestAddingToSelectionByCtrlClick_ShouldHappenOnMouseDown()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem1 = config.NetworkView.FindChildren<NodeItem>().First();
				var nodeItem2 = config.NetworkView.FindChildren<NodeItem>().Last();
				AssertNotEquals(nodeItem1, nodeItem2);

				//coordinates are not important here as the events go directly to the desired node item
				nodeItem1.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(0, 0));
				nodeItem1.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(0, 0));
				Assert(nodeItem1.IsSelected);

				config.MouseController.ControlPressed_ForTest = true;
				nodeItem2.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(0, 0));

				Assert("Second item should be selected on mouse down, not on mouse up", nodeItem2.IsSelected);

				nodeItem2.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(0, 0));

				Assert(nodeItem2.IsSelected);
				Assert("The first one should stay selected", nodeItem1.IsSelected);
			}
		}

		public void TestClickingOnConnectorShouldSelectParentNode()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				//coordinates are not important here as the events go directly to the desired connector item
				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(0, 0));
				Assert(nodeItem.IsSelected);

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(0, 0));
				Assert(nodeItem.IsSelected);
			}
		}

		public void TestSelecting_ClickOnBlankArea_ShouldDeselectAll()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.Width = 300;
				config.Node1.Height = 150;
				config.Node1.X = 35;
				config.Node1.Y = 35;
				config.Node2.Width = 300;
				config.Node2.Height = 150;
				config.Node2.X = 435;
				config.Node2.Y = 435;

				config.Node1.IsSelected = true;
				config.Node2.IsSelected = true;

				config.NetworkView.Focus();
				config.MouseController.SetCurrentMousePosition_ForTest(0, 0);

				var dragSelection = (Canvas)config.NetworkView.FindName("DragSelection");
				config.NetworkView.StartSelection(dragSelection);
				config.NetworkView.UpdateSelection();

				AssertEquals(false, config.Node1.IsSelected);
				AssertEquals(false, config.Node2.IsSelected);
			}
		}

		public void TestOnNodeRightClick_ShouldKeepSelected()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var node = config.NetworkView.FindChildren<NodeItem>().First();
				config.NetworkView.Focus();

				node.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(0, 0));
				node.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(0, 0));

				AssertEquals(true, config.Node1.IsSelected);

				node.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(0, 0));
				node.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(0, 0));

				AssertEquals(true, config.Node1.IsSelected);
			}
		}

		public void TestOnNodeRightClick_ShouldSelect()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var node = config.NetworkView.FindChildren<NodeItem>().First();
				config.NetworkView.Focus();

				node.RaiseEvent(config.MouseController.CreateRightMouseDownEvent(0, 0));
				node.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(0, 0));

				AssertEquals(true, config.Node1.IsSelected);
			}
		}

		#region SelectionChanged

		public void TestShouldNotifyOnSelectionChanged()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				RefreshArgs eventArgs = null;
				config.Control.SelectionChanged += (s, e) =>
				{
					AssertEquals(config.Control, s);
					eventArgs = e;
				};

				AssertEquals("Precondition", false, config.Node1.IsSelected);
				AssertEquals("Precondition", false, config.Node2.IsSelected);

				CombineAssertions("Adding node1 to selection", () =>
				{
					config.Node1.IsSelected = true;
					AssertNotNull(eventArgs);
					AssertContainsExactElementsInAnyOrder(new[] { config.Node1.Entity }, eventArgs.Entities);
				});

				eventArgs = null;
				CombineAssertions("Adding node2 to selection", () =>
				{
					config.Node2.IsSelected = true;
					AssertNotNull(eventArgs);
					AssertContainsExactElementsInAnyOrder(new[] { config.Node1.Entity, config.Node2.Entity }, eventArgs.Entities);
				});

				eventArgs = null;
				CombineAssertions("Removing node1 from selection", () =>
				{
					config.Node1.IsSelected = false;
					AssertNotNull(eventArgs);
					AssertContainsExactElementsInAnyOrder(new[] { config.Node2.Entity }, eventArgs.Entities);
				});

				eventArgs = null;
				CombineAssertions("Removing node2 from selection", () =>
				{
					config.Node2.IsSelected = false;
					AssertNotNull(eventArgs);
					AssertContainsExactElementsInAnyOrder(Array.Empty<INetworkEntity>(), eventArgs.Entities);
				});
			}
		}

		public void TestShouldUnsubscribeFromNetworkViewModelSelectionChanged_WhenChangingDataContext()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				RefreshArgs eventArgs = null;
				config.Control.SelectionChanged += (s, e) =>
				{
					AssertEquals(config.Control, s);
					eventArgs = e;
				};

				CombineAssertions("Changes from the network", () =>
				{
					config.Node1.IsSelected = true;
					AssertNotNull(eventArgs);
					AssertContainsExactElementsInAnyOrder(new[] { config.Node1.Entity }, eventArgs.Entities);
				});

				var oldNode = config.Node2;

				var newNetwork = new DummyNetwork();
				config.Control.SetDataContext(newNetwork, isReloading: false);
				var newNetworkViewModel = config.Control.NetworkViewModel;

				var newNode = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(newNetworkViewModel, "newEntity");

				eventArgs = null;
				CombineAssertions("Changes from old network", () =>
				{
					oldNode.IsSelected = true;
					AssertNull(eventArgs);
				});

				eventArgs = null;
				CombineAssertions("Changes from new network", () =>
				{
					newNode.IsSelected = true;
					AssertNotNull(eventArgs);
					AssertContainsExactElementsInAnyOrder(new[] { newNode.Entity }, eventArgs.Entities);
				});
			}
		}

		#endregion

		#endregion

		#region Data Context

		public void TestShouldHaveNetworkActionsAlreadySetUpForCreatingContextMenu_WhenSettingDataContext()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				config.Control.DataContextChanged += (s, e) =>
				{
					Assert("Should be able to construct menu items before setting data context - network action should be already prepared", config.Control.ViewModel.MenuItems.Count != 0);
				};
				config.Control.SetDataContext(config.Network, isReloading: false);
			}
		}

		public void TestShouldPreserveCurrentlyOpenedTabInRibbon_WhenSettingDataContext()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				config.Control.SetDataContext(config.Network, isReloading: false);

				config.Control.RibbonControlExposed_ForTesting.SelectTab(2);
				AssertEquals("Precondition", 2, config.Control.RibbonControlExposed_ForTesting.SelectedTabIndex);

				config.Control.SetDataContext(config.Network, isReloading: false);
				AssertEquals("The tab should stay opened", 2, config.Control.RibbonControlExposed_ForTesting.SelectedTabIndex);
			}
		}

		public void TestControlsShouldBeDisabled_UntilDataContextIsSet()
		{
			using (var config = NetworkGuiTestConfig.Create(shouldSetDataContext: false))
			{
				AssertEquals("Should be disabled before setting data context", false, config.Control.IsEnabled);

				config.Control.SetDataContext(config.Network, isReloading: false);
				AssertEquals("Should be enabled after setting data context", true, config.Control.IsEnabled);
			}
		}

		#endregion

		public void TestFit_ShouldNotThrow()
		{
			using (var config = NetworkGuiTestConfig.Create(shouldSetDataContext: false))
			{
				var fitButton = config.Control.FindChildren<Button>().Single(b => b.Name == "FitButton");
				AssertNoExceptionThrown("Should not throw", fitButton.PerformClick);
			}
		}

		public void TestSearchNextShouldScrollTheContent()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.NetworkView.Width = 200;
				config.NetworkView.Height = 100;

				config.Node1.X = 10;
				config.Node1.Y = 10;
				config.Node2.X = 500;
				config.Node2.Y = 500;

				var networkViewModel = config.Control.NetworkViewModel;
				var viewModel = new SearchFinderViewModel(networkViewModel);

				networkViewModel.SelectSingleEntity(config.Node2.Entity, shouldFocusOnSelection: true);
				var initialContentOffsetX = config.Control.MainDiagramControl.ViewModel.ContentOffsetX;
				var initialContentOffsetY = config.Control.MainDiagramControl.ViewModel.ContentOffsetY;
				Assert("Should scroll to node2", initialContentOffsetX > 0);
				Assert("Should scroll to node2", initialContentOffsetY > 0);

				viewModel.PerformSearch_ForTest("Node");
				Assert("View model should have performed the search.", viewModel.HasPerformedSearch_ForTest);
				AssertEquals("View model should have found 3 shapes.", 3, viewModel.GetSearchResults_ForTest().Count);

				AssertNotEquals("Should scroll horizontally", initialContentOffsetX, config.Control.MainDiagramControl.ViewModel.ContentOffsetX);
				AssertNotEquals("Should scroll vertically", initialContentOffsetY, config.Control.MainDiagramControl.ViewModel.ContentOffsetY);
			}
		}

		public void TestZoom_CannotGoLowerThanTenPercent()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var zoomOutButton = config.Control.FindChildren<Button>().Single(b => b.Name == "ZoomOutButton");
				var zoomInButton = config.Control.FindChildren<Button>().Single(b => b.Name == "ZoomInButton");
				var zoomLabel = config.Control.FindChildren<TextBlock>().Single(b => b.Name == "ContentScaleTextBlock");

				AssertEquals(true, zoomOutButton.IsEnabled);

				for (var i = 100; i >= 10; i -= 10)
				{
					AssertEquals(i.ToString(), zoomLabel.Text);
					zoomOutButton.PerformClick();
				}

				AssertEquals("10", zoomLabel.Text);

				zoomOutButton.PerformClick();
				AssertEquals("Zoom shouldn't go lower than 10%", "10", zoomLabel.Text);
				AssertEquals(false, zoomOutButton.IsEnabled);

				zoomInButton.PerformClick();

				AssertEquals(true, zoomOutButton.IsEnabled);
			}
		}

		public void TestChangingScaleChangesScaleLabel()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var textBlocks = config.Control.FindChildren<TextBlock>().Single(b => b.Name == "ContentScaleTextBlock");

				AssertEquals("100", textBlocks.Text);

				config.NetworkUserControlViewModel.ContentScale = 0.9;
				textBlocks = config.Control.FindChildren<TextBlock>().Single(b => b.Name == "ContentScaleTextBlock");

				AssertEquals("90", textBlocks.Text);
			}
		}

		const double expectedContentWidth = 773d;
		const double expectedContentHeight = 423d;

		public void TestOnStartup_ShouldAdjustViewWidthAndHeight()
		{
			using (var config = NetworkGuiTestConfig.Create(shouldShowWindow: false))
			{
				AssertEquals("Sanity check", 0d, config.Control.MainDiagramControl.ViewModel.ContentWidth);
				AssertEquals("Sanity check", 0d, config.Control.MainDiagramControl.ViewModel.ContentHeight);

				config.Window.Show();

				AssertEquals(expectedContentWidth, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentWidth));
				AssertEquals(expectedContentHeight, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentHeight));
			}
		}

		#region Mouse Capture

		public void TestShoudCaptureMouse_OnLeftMouseDown_OnNode()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				AssertEquals(nodeItem, config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudCaptureMouse_OnLeftMouseDown_OnConnector()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				AssertEquals(connectorItem, config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudNotCaptureMouse_OnRightMouseDown_OnNode()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				nodeItem.RaiseEvent(config.MouseController.CreateRightMouseDownEvent(100, 100));
				AssertNull(config.MouseController.ElementThatCapturedMouse);

				nodeItem.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(100, 100));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudNotCaptureMouse_OnRightMouseDown_OnConnector()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				connectorItem.RaiseEvent(config.MouseController.CreateRightMouseDownEvent(100, 100));
				AssertNull(config.MouseController.ElementThatCapturedMouse);

				connectorItem.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(100, 100));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudNotCaptureMouse_OnDoubleClick_OnNode()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				config.MouseController.ProcessDoubleClick(config.MouseController.CreateMouseDoubleClickEvent(10, 10));

				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudNotCaptureMouse_OnDoubleClick_OnConnector()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));

				config.MouseController.ProcessDoubleClick(config.MouseController.CreateMouseDoubleClickEvent(10, 10));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_OnLeftMouseUp_OnNode()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_OnLeftMouseUp_OnConnector()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_WhenFinishedDragging_Node()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 200));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_WhenFinishedDragging_Connector()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				connectorItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 200));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_WhenFinishedDragging_Node_ByRightClick()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				nodeItem.RaiseEvent(config.MouseController.CreateRightMouseDownEvent(200, 200));
				AssertNull(config.MouseController.ElementThatCapturedMouse);

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 200));
				nodeItem.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(200, 200));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_WhenFinishedDragging_Connector_ByRightClick()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				connectorItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				connectorItem.RaiseEvent(config.MouseController.CreateRightMouseDownEvent(200, 200));
				AssertNull(config.MouseController.ElementThatCapturedMouse);

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 200));
				connectorItem.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(200, 200));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_OnLeftAndThenRightMouseDown_OnNode()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem.RaiseEvent(config.MouseController.CreateRightMouseDownEvent(100, 100));

				AssertNull(config.MouseController.ElementThatCapturedMouse);

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));
				nodeItem.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(100, 100));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_OnLeftAndThenRightMouseDown_OnConnector()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				connectorItem.RaiseEvent(config.MouseController.CreateRightMouseDownEvent(100, 100));

				AssertNull(config.MouseController.ElementThatCapturedMouse);

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));
				connectorItem.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(100, 100));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_OnLostMouseCapture_WhenDraggingNode()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				nodeItem.RaiseEvent(config.MouseController.CreateLostMouseCaptureEvent());
				AssertNull(config.MouseController.ElementThatCapturedMouse);

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 200));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestShoudReleaseMouseCapture_OnLostMouseCapture_WhenDraggingConnector()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				connectorItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				connectorItem.RaiseEvent(config.MouseController.CreateLostMouseCaptureEvent());
				AssertNull(config.MouseController.ElementThatCapturedMouse);

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 200));
				AssertNull(config.MouseController.ElementThatCapturedMouse);
			}
		}

		public void TestCtrlScrollZoomsInAndOut()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var originalContentScale = config.Control.MainDiagramControl.ContentScale;
				config.Control.MainDiagramControl.IsCtrlDown_ForTest = true;
				config.NetworkView.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 10) { RoutedEvent = UIElement.MouseWheelEvent });
				AssertEquals(originalContentScale + 0.1, config.Control.MainDiagramControl.ContentScale);

				config.NetworkView.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -10) { RoutedEvent = UIElement.MouseWheelEvent });
				AssertEquals(originalContentScale, config.Control.MainDiagramControl.ContentScale);
			}
		}

		#endregion

		#region Dragging

		public void TestOnNodeDrag_Should()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.X = 1000;
				config.Node1.Y = 1000;

				AssertEquals("Sanity check", expectedContentWidth, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentWidth));
				AssertEquals("Sanity check", expectedContentHeight, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentHeight));

				NetworkTestHelper.SimulateNodeDragCompletedEvent(config.NetworkView, config.Node1);

				AssertEquals(1305d, config.Control.MainDiagramControl.ViewModel.ContentWidth);
				AssertEquals(1195d, config.Control.MainDiagramControl.ViewModel.ContentHeight);
			}
		}

		public void TestShouldAddToSelectionAndStartDragging_WhenDraggingUnselectedNodeWithCtrlPressed()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem1 = config.NetworkView.FindChildren<NodeItem>().First();
				var nodeItem2 = config.NetworkView.FindChildren<NodeItem>().Last();
				AssertNotEquals(nodeItem1, nodeItem2);

				nodeItem1.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem1.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));
				Assert(nodeItem1.IsSelected);
				Assert(!nodeItem2.IsSelected);

				Assert(!nodeItem1.IsContinuouslyChanging);
				Assert(!nodeItem2.IsContinuouslyChanging);

				config.MouseController.ControlPressed_ForTest = true;
				nodeItem2.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(200, 100));
				Assert("Should add the second node to selection", nodeItem2.IsSelected);

				nodeItem2.RaiseEvent(config.MouseController.CreateMouseMoveEvent(300, 100)); //the first time we switch to the dragging mode
				nodeItem2.RaiseEvent(config.MouseController.CreateMouseMoveEvent(400, 100)); //the second time we mode
				Assert(config.MouseController.IsDragging);
				Assert("Should drag the first node", nodeItem1.IsContinuouslyChanging);
				Assert("Should drag the second node", nodeItem2.IsContinuouslyChanging);

				nodeItem2.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(400, 100));
				Assert(!nodeItem1.IsContinuouslyChanging);
				Assert(!nodeItem2.IsContinuouslyChanging);
				Assert("Should stay selected", nodeItem2.IsSelected);
			}
		}

		public void TestShouldDeselectClickedNodeAndNotDragAnything_WhenDraggingAnotherSelectedNodeWithCtrlPressed()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem1 = config.NetworkView.FindChildren<NodeItem>().First();
				var nodeItem2 = config.NetworkView.FindChildren<NodeItem>().Last();
				AssertNotEquals(nodeItem1, nodeItem2);

				nodeItem1.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem1.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));
				Assert(nodeItem1.IsSelected);

				config.MouseController.ControlPressed_ForTest = true;
				nodeItem2.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem2.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(100, 100));
				Assert(nodeItem2.IsSelected);

				Assert(!nodeItem1.IsContinuouslyChanging);
				Assert(!nodeItem2.IsContinuouslyChanging);

				nodeItem2.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(200, 100));
				Assert("Should remove the second node from selection", !nodeItem2.IsSelected);

				nodeItem2.RaiseEvent(config.MouseController.CreateMouseMoveEvent(300, 100)); //the first time we switch to the dragging mode
				nodeItem2.RaiseEvent(config.MouseController.CreateMouseMoveEvent(400, 100)); //the second time we mode
				Assert("Should not drag anything", !config.MouseController.IsDragging);
				Assert("Should not drag the first node", !nodeItem1.IsContinuouslyChanging);
				Assert("Should not drag the second node", !nodeItem2.IsContinuouslyChanging);

				nodeItem2.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(400, 100));
				Assert(!nodeItem1.IsContinuouslyChanging);
				Assert(!nodeItem2.IsContinuouslyChanging);
				Assert("Should stay unselected", !nodeItem2.IsSelected);
			}
		}

		public void TestShouldNotThrow_WhenFinishingDraggingConnectorCausesLosingMouseCapture()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorItem = config.NetworkView.FindChildren<ConnectorItem>().First();

				var connectionDragCompletedCallsCount = 0;
				config.NetworkView.ConnectionDragCompleted += (s, e) =>
				{
					connectorItem.RaiseEvent(config.MouseController.CreateLostMouseCaptureEvent());
					connectionDragCompletedCallsCount++;
				};

				connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				connectorItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				AssertNoExceptionThrown(() =>
				{
					connectorItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 200));
				});
				AssertEquals("The handler should be called just once", 1, connectionDragCompletedCallsCount);
			}
		}

		public void TestShouldNotThrow_WhenFinishingDraggingNodeCausesLosingMouseCapture()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				var nodeDragCompletedCallsCount = 0;
				config.NetworkView.NodeDragCompleted += (s, e) =>
				{
					nodeItem.RaiseEvent(config.MouseController.CreateLostMouseCaptureEvent());
					nodeDragCompletedCallsCount++;
				};

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				AssertNoExceptionThrown(() =>
				{
					nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 200));
				});
				AssertEquals("The handler should be called just once", 1, nodeDragCompletedCallsCount);
			}
		}

		public void TestShouldNotThrow_WhenDeleteNodeWhileFinishingDragging()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First();

				config.NetworkView.NodeDragging += (s, e) =>
				{
					nodeItem.DataContext = null;
					nodeItem.RaiseEvent(config.MouseController.CreateLostMouseCaptureEvent());
				};

				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(100, 100));
				nodeItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(200, 200));
				nodeItem.RaiseEvent(config.MouseController.CreateMouseMoveEvent(400, 400));

				AssertNoExceptionThrown(() =>
				{
					nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(400, 400));
				});
			}
		}

		#endregion

		#region Resizing

		public void TestOnEntityResize_IfResultIsOutOfBoundsInPositiveDirection_ShouldResizeViewport()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var window = config.Window;
				window.Show();
				ApplicationHelper.DoEvents();

				var scrollViewer = (ScrollViewer)config.Control.MainDiagramControl.FindName("ScrollViewer");
				var isScrolled = false;
				scrollViewer.ScrollChanged += (_, __) => isScrolled = true;

				var initialWidth = window.Width;
				var initialHeight = window.Height;

				var node1 = config.Node1;

				ResizeShapeAndDoEvents(node1, (aNode, newWidth) => aNode.Width = newWidth, initialWidth * 2, config);
				AssertEquals("The window should update to still be displaying the node", true, isScrolled);

				scrollViewer.ScrollChanged += (_, __) => isScrolled = false;
				ResizeShapeAndDoEvents(node1, (aNode, newWidth) => aNode.Width = newWidth, 300, config);
				AssertEquals("The window should update to still be displaying the node", false, isScrolled);

				scrollViewer.ScrollChanged += (_, __) => isScrolled = true;
				ResizeShapeAndDoEvents(node1, (aNode, newHeight) => aNode.Height = newHeight, initialHeight, config);
				AssertEquals("The window should update to still be displaying the node", true, isScrolled);

				scrollViewer.ScrollChanged += (_, __) => isScrolled = false;
				ResizeShapeAndDoEvents(node1, (aNode, newHeight) => aNode.Height = newHeight, 150, config);
				AssertEquals("The window should update to still be displaying the node", false, isScrolled);
			}
		}

		static void ResizeShapeAndDoEvents(NodeViewModel resizeMe, Action<NodeViewModel, double> resizingAction, double resizeBy, NetworkGuiTestConfig config)
		{
			var nodeItem = config.NetworkView.FindChildren<NodeItem>().First(x => (x.DataContext as NodeViewModel).Name == resizeMe.Name);
			resizingAction(resizeMe, resizeBy);
			config.NetworkView.RaiseEvent(new NodeResizeCompletedEventArgs(NetworkView.NodeResizeCompletedEvent, resizeMe, new[] { nodeItem }));

			config.Window.Show();
			ApplicationHelper.DoEvents();
		}

		public void TestResizeThumbsVisibility_ShouldBeAllVisibleForSelectedNodeAndOnlyRBVisibleForUnselected()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeItem = config.NetworkView.FindChildren<NodeItem>().First(x => (x.DataContext as NodeViewModel).Name == "Node3");
				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(200, 100)); // select Node3, leave Node2 unselected
				nodeItem.RaiseEvent(config.MouseController.CreateLeftMouseUpEvent(200, 100));

				var thumbs = config.NetworkView.FindChildren<ResizeThumb>();
				foreach (var t in thumbs)
				{
					if ((t.DataContext as NodeViewModel).Name == "Node3")
					{
						AssertEquals(Visibility.Visible, t.Visibility);
					}
					else
					{
						if (t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom)
						{
							AssertEquals(Visibility.Visible, t.Visibility);
						}
						else
						{
							AssertEquals(Visibility.Collapsed, t.Visibility);
						}
					}
				}
			}
		}

		public void TestResizingWidthHeightBeyondMinimumValues()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(100d, NodeViewModel.MinAllowedNodeWidth);
					AssertEquals(50d, NodeViewModel.MinAllowedNodeHeight);
				});

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(x => (x.DataContext as NodeViewModel).Name == "Node3" &&
							x.HorizontalAlignment == HorizontalAlignment.Left && x.VerticalAlignment == VerticalAlignment.Top);

				config.Node3.Width = 1;
				config.Node3.Height = 1;
				CombineAssertions("The width and height should not go beyond minimum values", () =>
				{
					AssertEquals(NodeViewModel.MinAllowedNodeWidth, config.Node3.Width);
					AssertEquals(NodeViewModel.MinAllowedNodeHeight, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftTop_Bigger()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftTop;
				double xShift = -5, yShift = -10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The LeftTop corner should move left and up", () =>
				{
					AssertEquals(5d, config.Node3.X);
					AssertEquals(10d, config.Node3.Y);
					AssertEquals(305d, config.Node3.Width);
					AssertEquals(260d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftTop_Smaller()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftTop;
				double xShift = 5, yShift = 10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The LeftTop corner should move right and down", () =>
				{
					AssertEquals(15d, config.Node3.X);
					AssertEquals(30d, config.Node3.Y);
					AssertEquals(295d, config.Node3.Width);
					AssertEquals(240d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftTop_Bigger_BeyondPossible()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftTop;
				double xShift = -25, yShift = -30;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The LeftTop corner should not move left and up beyond 0", () =>
				{
					double newWidth = 300d + 10d;
					double newHeight = 250d + 20d;
					AssertEquals(310d, newWidth);
					AssertEquals(270d, newHeight);

					AssertEquals(0d, config.Node3.X);
					AssertEquals(0d, config.Node3.Y);
					AssertEquals(newWidth, config.Node3.Width);
					AssertEquals(newHeight, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftTop_Smaller_BeyondPossible()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(100d, NodeViewModel.MinAllowedNodeWidth);
					AssertEquals(50d, NodeViewModel.MinAllowedNodeHeight);

					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftTop;
				double xShift = 299, yShift = 249;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The LeftTop corner should not move right and down beyond MIN allowed values", () =>
				{
					double newX = 10d + 300d - 100d;
					double newY = 20d + 250d - 50d;
					AssertEquals(210d, newX);
					AssertEquals(220d, newY);

					AssertEquals(newX, config.Node3.X);
					AssertEquals(newY, config.Node3.Y);
					AssertEquals(NodeViewModel.MinAllowedNodeWidth, config.Node3.Width);
					AssertEquals(NodeViewModel.MinAllowedNodeHeight, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromRightTop_Bigger()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.RightTop;
				double xShift = 5, yShift = -10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The RightTop corner should move right and up", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(10d, config.Node3.Y);
					AssertEquals(305d, config.Node3.Width);
					AssertEquals(260d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromRightTop_Smaller()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.RightTop;
				double xShift = -5, yShift = 10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The RightTop corner should move left and down", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(30d, config.Node3.Y);
					AssertEquals(295d, config.Node3.Width);
					AssertEquals(240d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromRightTop_Bigger_BeyondPossible()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.RightTop;
				double xShift = 0, yShift = -30;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The RightTop corner should not move up beyond 0", () =>
				{
					double newHeight = 250d + 20d;
					AssertEquals(270d, newHeight);

					AssertEquals(10d, config.Node3.X);
					AssertEquals(0d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(newHeight, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromRightTop_Smaller_BeyondPossible()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(100d, NodeViewModel.MinAllowedNodeWidth);
					AssertEquals(50d, NodeViewModel.MinAllowedNodeHeight);

					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.RightTop;
				double xShift = -299, yShift = 249;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The RightTop corner should not move left and down beyond MIN allowed values", () =>
				{
					double newY = 20d + 250d - 50d;
					AssertEquals(220d, newY);

					AssertEquals(10d, config.Node3.X);
					AssertEquals(newY, config.Node3.Y);
					AssertEquals(NodeViewModel.MinAllowedNodeWidth, config.Node3.Width);
					AssertEquals(NodeViewModel.MinAllowedNodeHeight, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftBottom_Bigger()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftBottom;
				double xShift = -5, yShift = 10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The LeftBottom corner should move left and down", () =>
				{
					AssertEquals(5d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(305d, config.Node3.Width);
					AssertEquals(260d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftBottom_Smaller()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftBottom;
				double xShift = 5, yShift = -10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The LeftBottom corner should move right and up", () =>
				{
					AssertEquals(15d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(295d, config.Node3.Width);
					AssertEquals(240d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftBottom_Bigger_BeyondPossible()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftBottom;
				double xShift = -25, yShift = 0;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The LeftBottom corner should not move left beyond 0", () =>
				{
					double newWidth = 300d + 10d;
					AssertEquals(310d, newWidth);

					AssertEquals(0d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(newWidth, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftBottom_Smaller_BeyondPossible()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(100d, NodeViewModel.MinAllowedNodeWidth);
					AssertEquals(50d, NodeViewModel.MinAllowedNodeHeight);

					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftBottom;
				double xShift = 299, yShift = -249;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The LeftBottom corner should not move right and up beyond MIN allowed values", () =>
				{
					double newX = 10d + 300d - 100d;
					AssertEquals(210d, newX);

					AssertEquals(newX, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(NodeViewModel.MinAllowedNodeWidth, config.Node3.Width);
					AssertEquals(NodeViewModel.MinAllowedNodeHeight, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromRightBottom_Bigger()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.RightBottom;
				double xShift = 5, yShift = 10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The RightBottom corner should move right and down", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(305d, config.Node3.Width);
					AssertEquals(260d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromRightBottom_Smaller()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.RightBottom;
				double xShift = -5, yShift = -10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The RightBottom corner should move left and up", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(295d, config.Node3.Width);
					AssertEquals(240d, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromRightBottom_Smaller_BeyondPossible()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(100d, NodeViewModel.MinAllowedNodeWidth);
					AssertEquals(50d, NodeViewModel.MinAllowedNodeHeight);

					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.RightBottom;
				double xShift = -299, yShift = -249;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("The RightBottom corner should not move left and up beyond MIN allowed values", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(NodeViewModel.MinAllowedNodeWidth, config.Node3.Width);
					AssertEquals(NodeViewModel.MinAllowedNodeHeight, config.Node3.Height);
				});
			}
		}

		public void TestResizingFromLeftTop_WithChildren_Bigger()
		{
			var diagram = new Entity();
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.LeftTop;
				double xShift = -99, yShift = -99;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("Parent should remain unchanged", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Child LeftTop corner should move left and up to parent boundaries", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(40d, margin.Top);

					double newY = 20d + 40d;
					double newWidth = 200d + (60d - 10d);
					double newHeight = 150d + (70d - 20d) - 40d;
					AssertEquals(60d, newY);
					AssertEquals(250d, newWidth);
					AssertEquals(160d, newHeight);

					AssertEquals(10d, config.Node3Child.X);
					AssertEquals(newY, config.Node3Child.Y);
					AssertEquals(newWidth, config.Node3Child.Width);
					AssertEquals(newHeight, config.Node3Child.Height);
				});

				CombineAssertions("Grandchild should compensate for the child move with its size remaining unchanged", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(40d, margin.Top);

					double newX = 120d + (60d - 10d);
					double newY = 130d + (70d - 20d) - 40d;
					AssertEquals(170d, newX);
					AssertEquals(140d, newY);

					AssertEquals(newX, config.Node3GrandChild.X);
					AssertEquals(newY, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromLeftTop_WithChildren_Smaller()
		{
			var diagram = new Entity();
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.LeftTop;
				double xShift = 99, yShift = 99;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("Parent should remain unchanged", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Child LeftTop corner should move right and down to parent boundaries", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(40d, margin.Top);

					double newY = 130d - 40d;
					double newWidth = 200d - (120d - 60d);
					double newHeight = 150d - (130d - 70d) + 40d;
					AssertEquals(90d, newY);
					AssertEquals(140d, newWidth);
					AssertEquals(130d, newHeight);

					AssertEquals(120d, config.Node3Child.X);
					AssertEquals(newY, config.Node3Child.Y);
					AssertEquals(newWidth, config.Node3Child.Width);
					AssertEquals(newHeight, config.Node3Child.Height);
				});

				CombineAssertions("Grandchild should compensate for the child move with its size remaining unchanged", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(40d, margin.Top);

					double newX = 120d - (120d - 60d);
					double newY = 130d - (130d - 70d) + 40d;
					AssertEquals(60d, newX);
					AssertEquals(110d, newY);

					AssertEquals(newX, config.Node3GrandChild.X);
					AssertEquals(newY, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromRightTop_WithChildren_Bigger()
		{
			var diagram = new Entity();
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.RightTop;
				double xShift = 99, yShift = -99;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("Parent should remain unchanged", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Child RightTop corner should move right and up to parent boundaries", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(40d, margin.Top);

					double newY = 20d + 40d;
					double newWidth = 200d + ((10d + 300d) - (60d + 200d));
					double newHeight = 150d + (70d - 20d) - 40d;
					AssertEquals(60d, newY);
					AssertEquals(250d, newWidth);
					AssertEquals(160d, newHeight);

					AssertEquals(60d, config.Node3Child.X);
					AssertEquals(newY, config.Node3Child.Y);
					AssertEquals(newWidth, config.Node3Child.Width);
					AssertEquals(newHeight, config.Node3Child.Height);
				});

				CombineAssertions("Grandchild should compensate for the child move with its size remaining unchanged", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(40d, margin.Top);

					double newY = 130d + (70d - 20d) - 40d;
					AssertEquals(140d, newY);

					AssertEquals(120d, config.Node3GrandChild.X);
					AssertEquals(newY, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromRightTop_WithChildren_Smaller()
		{
			var diagram = new Entity();
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.RightTop;
				double xShift = -99, yShift = 99;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("Parent should remain unchanged", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Child RightTop corner should move left and down to parent boundaries", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(40d, margin.Top);

					double newY = 130d - 40d;
					double newWidth = 200d - ((60d + 200d) - (120d + 100d));
					double newHeight = 150d - (130d - 70d) + 40d;
					AssertEquals(90d, newY);
					AssertEquals(160d, newWidth);
					AssertEquals(130d, newHeight);

					AssertEquals(60d, config.Node3Child.X);
					AssertEquals(newY, config.Node3Child.Y);
					AssertEquals(newWidth, config.Node3Child.Width);
					AssertEquals(newHeight, config.Node3Child.Height);
				});

				CombineAssertions("Grandchild should compensate for the child move with its size remaining unchanged", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(40d, margin.Top);

					double newY = 130d - (130d - 70d) + 40d;
					AssertEquals(110d, newY);

					AssertEquals(120d, config.Node3GrandChild.X);
					AssertEquals(newY, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromLeftBottom_WithChildren_Bigger()
		{
			var diagram = new Entity();
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.LeftBottom;
				double xShift = -99, yShift = 99;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("Parent should remain unchanged", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Child LiftBottom corner should move left and down to parent boundaries", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(5d, margin.Bottom);

					double newWidth = 200d + (60d - 10d);
					double newHeight = 150d + ((20d + 250d) - (70d + 150d)) - 5d;
					AssertEquals(250d, newWidth);
					AssertEquals(195d, newHeight);

					AssertEquals(10d, config.Node3Child.X);
					AssertEquals(70d, config.Node3Child.Y);
					AssertEquals(newWidth, config.Node3Child.Width);
					AssertEquals(newHeight, config.Node3Child.Height);
				});

				CombineAssertions("Grandchild should compensate for the child move with its size remaining unchanged", () =>
				{
					double newX = 120d + (60d - 10d);
					AssertEquals(170d, newX);

					AssertEquals(newX, config.Node3GrandChild.X);
					AssertEquals(130d, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromLeftBottom_WithChildren_Smaller()
		{
			var diagram = new Entity();
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.LeftBottom;
				double xShift = 99, yShift = -99;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("Parent should remain unchanged", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Child LeftBottom corner should move right and up to parent boundaries", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(5d, margin.Bottom);

					double newWidth = 200d - (120d - 60d);
					double newHeight = 150d - ((70d + 150d) - (130d + 50d)) + 5d;
					AssertEquals(140d, newWidth);
					AssertEquals(115d, newHeight);

					AssertEquals(120d, config.Node3Child.X);
					AssertEquals(70d, config.Node3Child.Y);
					AssertEquals(newWidth, config.Node3Child.Width);
					AssertEquals(newHeight, config.Node3Child.Height);
				});

				CombineAssertions("Grandchild should compensate for the child move with its size remaining unchanged", () =>
				{
					double newX = 120d - (120d - 60d);
					AssertEquals(60d, newX);

					AssertEquals(newX, config.Node3GrandChild.X);
					AssertEquals(130d, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromRightBottom_WithChildren_Bigger()
		{
			var diagram = new Entity();
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.RightBottom;
				double xShift = 99, yShift = 99;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("Parent should remain unchanged", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Child RightBottom corner should move right and down to parent boundaries", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(5d, margin.Bottom);

					double newWidth = 200d + ((10d + 300d) - (60d + 200d));
					double newHeight = 150d + ((20d + 250d) - (70d + 150d)) - 5d;
					AssertEquals(250d, newWidth);
					AssertEquals(195d, newHeight);

					AssertEquals(60d, config.Node3Child.X);
					AssertEquals(70d, config.Node3Child.Y);
					AssertEquals(newWidth, config.Node3Child.Width);
					AssertEquals(newHeight, config.Node3Child.Height);
				});

				CombineAssertions("Grandchild should not change", () =>
				{
					AssertEquals(120d, config.Node3GrandChild.X);
					AssertEquals(130d, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromRightBottom_WithChildren_Smaller()
		{
			var diagram = new Entity();
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.RightBottom;
				double xShift = -99, yShift = -99;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));

				CombineAssertions("Parent should remain unchanged", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Child RightBottom corner should move left and up to parent boundaries", () =>
				{
					var margin = config.Node3.Network.EntityPositionStrategy.GetInternalMargin();
					AssertEquals(5d, margin.Bottom);

					double newWidth = 200d - ((60d + 200d) - (120d + 100d));
					double newHeight = 150d - ((70d + 150d) - (130d + 50d)) + 5d;
					AssertEquals(160d, newWidth);
					AssertEquals(115d, newHeight);

					AssertEquals(60d, config.Node3Child.X);
					AssertEquals(70d, config.Node3Child.Y);
					AssertEquals(newWidth, config.Node3Child.Width);
					AssertEquals(newHeight, config.Node3Child.Height);
				});

				CombineAssertions("Grandchild should not change", () =>
				{
					AssertEquals(120d, config.Node3GrandChild.X);
					AssertEquals(130d, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromLeftTop_TopLevelWithChildren_ScaledDiagram()
		{
			var diagram = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 50,
			};

			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3.AdjustToScale();
				config.Node3Child.AdjustToScale();
				config.Node3GrandChild.AdjustToScale();

				CombineAssertions("Precondition - Snapping to scale", () =>
				{
					AssertEquals(0d, config.Node3.X);
					AssertEquals(50d, config.Node3Child.X);
					AssertEquals(100d, config.Node3GrandChild.X);
				});

				config.Node3.ReferencePoint = NodeResizeReferencePoint.LeftTop;
				double xShift = 5, yShift = 10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));
				config.Node3.OnResizeCompleted();

				CombineAssertions("Parent should snap to scale (X-coordinate) and move Y-coordinate appropriately", () =>
				{
					AssertEquals(0d, config.Node3.X);
					AssertEquals(30d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(240d, config.Node3.Height);
				});

				CombineAssertions("Child should only compensate for Y-coordinate move", () =>
				{
					AssertEquals(50d, config.Node3Child.X);
					AssertEquals(60d, config.Node3Child.Y);
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);
				});

				CombineAssertions("Grandhild should only compensate for Y-coordinate move", () =>
				{
					AssertEquals(100d, config.Node3GrandChild.X);
					AssertEquals(120d, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		public void TestResizingFromLeftTop_MiddleLevelWithParentAndChild_ScaledDiagram()
		{
			var diagram = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 50,
			};

			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren(diagram))
			{
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(10d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);

					AssertEquals(60d, config.Node3Child.X);       // relative to Node3
					AssertEquals(70d, config.Node3Child.Y);       // relative to Node3
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(150d, config.Node3Child.Height);

					AssertEquals(120d, config.Node3GrandChild.X);       // relative to Node3 and Node3Child
					AssertEquals(130d, config.Node3GrandChild.Y);       // relative to Node3 and Node3Child
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});

				config.Node3.AdjustToScale();
				config.Node3Child.AdjustToScale();
				config.Node3GrandChild.AdjustToScale();

				CombineAssertions("Precondition - Snapping to scale", () =>
				{
					AssertEquals(0d, config.Node3.X);
					AssertEquals(50d, config.Node3Child.X);
					AssertEquals(100d, config.Node3GrandChild.X);
				});

				config.Node3Child.ReferencePoint = NodeResizeReferencePoint.LeftTop;
				double xShift = -5, yShift = -10;

				var thumb = config.NetworkView.FindChildren<ResizeThumb>().First(t => (t.DataContext as NodeViewModel).Name == "Node3Child" &&
					t.HorizontalAlignment == HorizontalAlignment.Left && t.VerticalAlignment == VerticalAlignment.Top);
				thumb.RaiseEvent(new DragDeltaEventArgs(xShift, yShift));
				config.Node3Child.OnResizeCompleted();

				CombineAssertions("Parent should not change", () =>
				{
					AssertEquals(0d, config.Node3.X);
					AssertEquals(20d, config.Node3.Y);
					AssertEquals(300d, config.Node3.Width);
					AssertEquals(250d, config.Node3.Height);
				});

				CombineAssertions("Parent should snap to scale (X-coordinate) and move Y-coordinate appropriately", () =>
				{
					AssertEquals(50d, config.Node3Child.X);
					AssertEquals(60d, config.Node3Child.Y);
					AssertEquals(200d, config.Node3Child.Width);
					AssertEquals(160d, config.Node3Child.Height);
				});

				CombineAssertions("Grandhild should only compensate for Y-coordinate move", () =>
				{
					AssertEquals(100d, config.Node3GrandChild.X);
					AssertEquals(140d, config.Node3GrandChild.Y);
					AssertEquals(100d, config.Node3GrandChild.Width);
					AssertEquals(50d, config.Node3GrandChild.Height);
				});
			}
		}

		#endregion

		#region Width and Height Adjustment

		public void TestOnResizingMainWindow_ShouldAdjustViewPort()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var zoomAndPanControl = config.Control.MainDiagramControl.FindChildren<ZoomAndPanControl>().Single();
				zoomAndPanControl.Width = 20;
				zoomAndPanControl.Height = 20;

				AssertEquals("Sanity check", expectedContentWidth, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentWidth));
				AssertEquals("Sanity check", expectedContentHeight, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentHeight));

				ApplicationHelper.DoEvents();

				AssertEquals(config.Control.MainDiagramControl.ViewModel.ContentWidth, 10D);
				AssertEquals(config.Control.MainDiagramControl.ViewModel.ContentHeight, 10D);
			}
		}

		public void TestWithMoreThanOneNode_ShouldAdjustWithMostFarItem()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				NetworkTestHelper.SimulateDraggingNodeToNewPosition(config.Node2, 2000, 2000, config.NetworkView);

				AssertEquals(2305d, config.Control.MainDiagramControl.ViewModel.ContentWidth);
				AssertEquals(2195d, config.Control.MainDiagramControl.ViewModel.ContentHeight);
			}
		}

		public void TestWithRefresh_ShouldAdjust()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.X = 1000;
				config.Node1.Y = 1000;

				AssertEquals("Sanity check", expectedContentWidth, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentWidth));
				AssertEquals("Sanity check", expectedContentHeight, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentHeight));
				AssertEquals("Sanity check", 1000d, config.Node1.X);
				AssertEquals("Sanity check", 1000d, config.Node1.Y);

				config.Network.Refresh(RefreshType.RedrawDiagram);

				ApplicationHelper.DoEvents();

				var newViewModel = (DiagramAreaUserControlViewModel)config.Control.MainDiagramControl.DataContext;

				AssertEquals(1305d, newViewModel.ContentWidth);
				AssertEquals(1195d, newViewModel.ContentHeight);
			}
		}

		#endregion

		public void TestAdjustEntityLocationToChannelBounds_ResizeChannel_EntitiesShouldSnap()
		{
			var channel1 = new DummyChannel("Bend", 500);
			var channel2 = new DummyChannel("Snap", 500);

			var diagram = new Entity();
			diagram.IsDiagramScaled = true;
			diagram.CanHaveChildren = true;
			diagram.DiagramChannels = new[] { channel1, channel2 };

			using (var config = NetworkGuiTestConfig.Create(diagram, shouldShowWindow: false))
			{
				config.Control.Height = 1200;

				var shapeEntity = (Entity)new StaticNetworkAction(() => config.Network.CreateNewEntity(ShapeTypes.Shape)).Execute();
				var node = config.Control.MainDiagramControl.ViewModel.CreateNewNode(new Location(10, 300), shapeEntity);
				node.Name = "Node1";
				node.Width = 300;
				node.Height = 150;
				shapeEntity.Parent = diagram;

				config.Window.Show();

				var shapeAttributesLabel = config.Control.MainDiagramControl.FindChildren<Label>().SingleOrDefault(l => l.Content.ToString() == "Bend");

				AssertEquals(300d, node.Y);
				AssertEquals(150d, node.Height);
				AssertEquals("Bend", shapeAttributesLabel.Content);

				channel1 = new DummyChannel("Bend", 350);
				diagram.DiagramChannels = new[] { channel1, channel2 };

				config.Network.Refresh(RefreshType.RefreshButton);
				ApplicationHelper.DoEvents();

				shapeAttributesLabel = config.Control.MainDiagramControl.FindChildren<Label>().SingleOrDefault(l => l.Content.ToString() == "Snap");

				CombineAssertions("Should snap to Snap after refresh", () =>
				{
					AssertEquals(350d, node.Y);
					AssertEquals(150d, node.Height);
					AssertEquals("Snap", shapeAttributesLabel.Content);
				});
			}
		}

		public void TestWithMovingNodeOutOfViewPort()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.X = -1000;
				config.Node1.Y = -1000;

				AssertEquals("Sanity check", expectedContentWidth, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentWidth));
				AssertEquals("Sanity check", expectedContentHeight, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentHeight));
				AssertEquals("Sanity check - No negative positions allowed!", 0d, config.Node1.X);
				AssertEquals("Sanity check - No negative positions allowed!", 0d, config.Node1.Y);

				NetworkTestHelper.SimulateNodeDragCompletedEvent(config.NetworkView, config.Node1);

				AssertEquals("Sanity check", expectedContentWidth, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentWidth));
				AssertEquals("Sanity check", expectedContentHeight, Math.Round(config.Control.MainDiagramControl.ViewModel.ContentHeight));
			}
		}

		public void TestMakeVisible_ShouldNotScroll()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.X = 2000;
				config.Node1.Y = 2000;

				ApplicationHelper.DoEvents();

				NetworkTestHelper.SimulateNodeDragCompletedEvent(config.NetworkView, config.Node1);

				ApplicationHelper.DoEvents();

				var scrollViewer = (ScrollViewer)config.Control.MainDiagramControl.FindName("ScrollViewer");
				var isScrolled = false;
				scrollViewer.ScrollChanged += (_, x_) => isScrolled = true;

				var zoomAndPanControl = config.Control.MainDiagramControl.FindChildren<ZoomAndPanControl>().Single();
				zoomAndPanControl.MakeVisible(config.NetworkView, new Rect(0, 0, 2718, 0));
				ApplicationHelper.DoEvents();

				Assert("The scroll should not have happened", !isScrolled);
			}
		}

		public void TestMinusKeyPress_OnTextBoxDoesNotZoomOut()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var slider = config.Control.FindChildren<Slider>().FirstOrDefault();
				var previousValue = slider.Value;

				var textcontrol = config.Control.FindChildren<TextBoxWithPlaceholder>().LastOrDefault();
				textcontrol.Focus();
				textcontrol.Select(0, 0);
				ApplicationHelper.DoEvents();

				var key = Key.OemMinus;
				var routedEvent = Keyboard.KeyDownEvent;
				textcontrol.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(textcontrol), 0, key) { RoutedEvent = routedEvent });

				ApplicationHelper.DoEvents();

				AssertEquals("Should not zoom out without Ctrl+-", previousValue, slider.Value);

				key = Key.Subtract;
				textcontrol.Select(0, 0);
				textcontrol.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(textcontrol), 0, key) { RoutedEvent = routedEvent });
				ApplicationHelper.DoEvents();

				AssertEquals("Should not zoom out without Ctrl+- (on Num Pad)", previousValue, slider.Value);
			}
		}

		public void TestPlusKeyPress_OnTextBoxDoesNotZoomIn()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var slider = config.Control.FindChildren<Slider>().FirstOrDefault();
				var previousValue = slider.Value;

				var textcontrol = config.Control.FindChildren<TextBoxWithPlaceholder>().LastOrDefault();
				textcontrol.Focus();
				textcontrol.Select(0, 0);
				ApplicationHelper.DoEvents();

				var key = Key.OemPlus;
				var routedEvent = Keyboard.KeyDownEvent;
				textcontrol.Select(0, 0);
				textcontrol.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(textcontrol), 0, key) { RoutedEvent = routedEvent });
				ApplicationHelper.DoEvents();

				AssertEquals("Should not zoom in without Ctrl++", previousValue, slider.Value);

				key = Key.Add;
				textcontrol.Select(0, 0);
				textcontrol.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(textcontrol), 0, key) { RoutedEvent = routedEvent });
				ApplicationHelper.DoEvents();

				AssertEquals("Should not zoom in without Ctrl++ (on Num Pad)", previousValue, slider.Value);
			}
		}

		public void TestRefreshSavesCurrentOffSetAndScale()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.Node1.X = 1000;
				config.Node1.Y = 1000;

				config.Control.MainDiagramControl.ViewModel.ContentWidth = expectedContentWidth;
				config.Control.MainDiagramControl.ViewModel.ContentHeight = expectedContentHeight;

				var zoomAndPanControl = config.Control.MainDiagramControl.FindChildren<ZoomAndPanControl>().Single();

				zoomAndPanControl.ContentScale = 1.4d;
				zoomAndPanControl.ContentOffsetX = 200d;
				zoomAndPanControl.ContentOffsetY = 50d;

				ApplicationHelper.DoEvents();

				AssertEquals(200d, zoomAndPanControl.ContentOffsetX);
				AssertEquals(50d, zoomAndPanControl.ContentOffsetY);
				AssertEquals(1.4d, zoomAndPanControl.ContentScale);

				config.Network.Refresh(RefreshType.RedrawDiagram);

				ApplicationHelper.DoEvents();

				zoomAndPanControl = config.Control.MainDiagramControl.FindChildren<ZoomAndPanControl>().Single();

				AssertEquals(200d, zoomAndPanControl.ContentOffsetX);
				AssertEquals(50d, zoomAndPanControl.ContentOffsetY);
				AssertEquals(1.4d, zoomAndPanControl.ContentScale);
			}
		}

		public void TestDraggingConnection_WhenClickingTheRightMouseButton_ShouldCeaseDragging()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorControl = config.Control.FindChildren<ConnectorItem>().First(c => ((NodeViewModel)c.ParentNodeItem.DataContext).Name == "Node1");

				connectorControl.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(10, 10));
				connectorControl.RaiseEvent(config.MouseController.CreateMouseMoveEvent(20, 20));
				ApplicationHelper.DoEvents();

				AssertEquals("A connection should be created, since we are dragging it out", 1, config.Node1.AttachedConnections.Count);

				connectorControl.RaiseEvent(config.MouseController.CreateRightMouseDownEvent(20, 20));
				connectorControl.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(20, 20));
				ApplicationHelper.DoEvents();

				AssertEquals("The right mouse button has been clicked, so the connection should disappear", 0, config.Node1.AttachedConnections.Count);
			}
		}

		public void TestDraggingConnection_WhenLostMouseCapture_ShouldCeaseDragging()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var connectorControl = config.Control.FindChildren<ConnectorItem>().First(c => ((NodeViewModel)c.ParentNodeItem.DataContext).Name == "Node1");

				connectorControl.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(10, 10));
				connectorControl.RaiseEvent(config.MouseController.CreateMouseMoveEvent(20, 20));
				ApplicationHelper.DoEvents();
				AssertEquals("A connection should be created, since we are dragging it out", 1, config.Node1.AttachedConnections.Count);
				Assert(config.MouseController.IsDragging);

				connectorControl.RaiseEvent(config.MouseController.CreateLostMouseCaptureEvent());
				ApplicationHelper.DoEvents();

				AssertEquals("The application has lost the mouse capture during a dragging event, should have 0 connections attached", 0, config.Node1.AttachedConnections.Count);
				Assert(!config.MouseController.IsDragging);
			}
		}

		public void TestDraggingNode_WhenLostMouseCapture_ShouldCeaseDragging()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var nodeControl = config.Control.FindChildren<NodeItem>().First();

				nodeControl.RaiseEvent(config.MouseController.CreateLeftMouseDownEvent(10, 10));
				nodeControl.RaiseEvent(config.MouseController.CreateMouseMoveEvent(20, 20));
				Assert(config.MouseController.IsDragging);

				nodeControl.RaiseEvent(config.MouseController.CreateLostMouseCaptureEvent());
				Assert(!config.MouseController.IsDragging);
			}
		}

		public void TestDoNotMoveShapeOutsideDiagramBounds()
		{
			var window = new Window();
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
				Width = 900,
				CornerRadius = 10
			};

#pragma warning disable CS0618
			using (var control = new NetworkUserControl(network.DiagramEntity, refresher))
			{
				control.SetDataContext(network, isReloading: false);
				window.Content = control;
				control.Height = 600;
				control.Width = 1000;

				var entity = new Entity { ShapeType = ShapeTypes.Shape };
				entity.Parent = network.DiagramEntity;
				network.Entities.Add(entity);

				var viewModel = (DiagramAreaUserControlViewModel)control.MainDiagramControl.DataContext;

				var node = viewModel.CreateNewNode(new Location(9, 19), entity);
				node.Name = "Node";
				node.X = 1310;

				var mainNetworkView = (NetworkView)control.MainDiagramControl.FindName("NetworkControl");
				NetworkTestHelper.SimulateNodeDragCompletedEvent(mainNetworkView, node);

				AssertEquals(1600d, viewModel.ContentWidth);
				AssertEquals(1300d, node.X);
			}
#pragma warning restore CS0618
		}

		public void TestSubDiagram_DoNotMoveShapeOutsideParentBounds()
		{
			var diagram = new Entity
			{
				IsDiagramScaled = true,
				IsDiagramSurfaceFixed = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
				Width = 900,
				CornerRadius = 10,
				Name = "Main Diagram"
			};

			using (var config = NetworkGuiTestConfig.Create(diagram))
			{
				var subDiagramEntity = new Entity
				{
					IsDiagramScaled = true,
					IsDiagramSurfaceFixed = true,
					CanHaveChildren = true,
					ShapeType = ShapeTypes.Shape,
					Scale = 100,
					ResolutionIncrement = 50,
					ScaleUnitPixelSize = 200,
					Width = 600,
					Height = 195,
					CornerRadius = 10,
					Name = "sub-diagram",
					Parent = config.Network.DiagramEntity,
				}; //sub-diagram

				config.Network.Entities.Add(subDiagramEntity);

				var viewModel = config.Control.MainDiagramControl.ViewModel;
				var subDiagramNode = viewModel.CreateNewNode(new Location(35, 111), subDiagramEntity);
				var entity = new Entity { ShapeType = ShapeTypes.Shape, Name = "Shape1" };
				entity.Parent = subDiagramEntity;
				config.Network.Entities.Add(entity);

				//open sub-diagram in its own window
				var window2 = new Window();
				var refresher2 = new NetworkRefresher();
				var network2 = new DummyNetwork { Refresher = refresher2 };
				refresher2.AssociateWithNetwork(network2);
				network2.DiagramEntity = subDiagramEntity;

#pragma warning disable CS0618
				using (var control2 = new NetworkUserControl(config.Network.DiagramEntity, refresher2))
				{
					control2.SetDataContext(network2, isReloading: false);
					window2.Content = control2;
					control2.Height = 600;
					control2.Width = 1000;
					network2.Entities.Add(entity);
					var viewModel2 = (DiagramAreaUserControlViewModel)control2.MainDiagramControl.DataContext;

					var node = viewModel2.CreateNewNode(new Location(9, 19), entity);
					node.Name = "Node";
					node.Y = 260;

					var networkView = (NetworkView)control2.MainDiagramControl.FindName("NetworkControl");
					NetworkTestHelper.SimulateNodeDragCompletedEvent(networkView, node);

					AssertEquals(215d, viewModel2.ContentHeight);
					AssertEquals(251d, node.Y);
				}
#pragma warning restore CS0618
			}
		}

		public void TestWhenNetworkReloads_RibbonViewModelShouldReload()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				AssertEquals("Precondition", "Home", config.Control.ViewModel.RibbonViewModel.Tabs[0].Header);
				config.Control.ViewModel.RibbonViewModel.Tabs[0].Header = ResString.GetMultilingualString("TestRefreshSavesCurrentOffSetAndScale", "Let's change the RibbonViewModel a little");

				config.Refresher.Refresh(new RefreshArgs(RefreshType.RedrawDiagram));
				ApplicationHelper.DoEvents();
				AssertEquals("RibbonViewModel should reload when network reloads", "Home", config.Control.ViewModel.RibbonViewModel.Tabs[0].Header);
			}
		}

#pragma warning disable CS0618
		public void TestOriginalEntityShouldBeRemoved_WhenNetworkReloads()
		{
			var window = new Window();
			var refresher = new NetworkRefresher();
			var network = new DummyNetwork { Refresher = refresher };
			refresher.AssociateWithNetwork(network);
			network.DiagramEntity = new Entity();

			var control = new NetworkUserControl(network.DiagramEntity, refresher);
			control.SetDataContext(network, isReloading: false);

			var control2 = new NetworkUserControl(network.DiagramEntity, refresher);
			control2.SetDataContext(refresher.GetReloadedNetwork(), isReloading: false);

			window.Content = control;

			var entity = new Entity { ShapeType = ShapeTypes.Shape };
			entity.Parent = network.DiagramEntity;
			network.Entities.Add(entity);

			var viewModel = (DiagramAreaUserControlViewModel)control.MainDiagramControl.DataContext;
			var node = viewModel.CreateNewNode(new Location(0, 0), entity);

			var window2 = new Window();
			window2.Content = control2;

			var viewModel2 = (DiagramAreaUserControlViewModel)control2.MainDiagramControl.DataContext;
			var node2 = viewModel2.CreateNewNode(new Location(0, 0), entity);

			Assert("Precondition", viewModel.NetworkViewModel.Nodes.Any());
			Assert("Precondition", viewModel2.NetworkViewModel.Nodes.Any());

			viewModel2.DeleteNode(node2);

			Assert("An entity should not exist in the ViewModel", !viewModel2.NetworkViewModel.Nodes.Any());
		}
#pragma warning restore CS0618

		#region NonScheduled Section

		public void TestDiagram_WithShowNonScheduledSectionNotEnabled_ShouldOnlyShowOneDiagramSurface()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var grid = config.Control.FindChildren<Grid>().Single(x => x.Name == "DiagramAreaControlsGrid");

				AssertEquals(800d, grid.ColumnDefinitions[0].ActualWidth);
				AssertEquals("The spliter column should be hidden. SAD!", 0d, grid.ColumnDefinitions[1].ActualWidth);
				AssertEquals("The non-scheduled diagram area column should be hidden. SAD!", 0d, grid.ColumnDefinitions[2].ActualWidth);

				AssertNull("The control should be set to null so we don't bother performing actions on it. SAD!", config.Control.NonScheduledDiagramControl);
			}
		}

		public void TestDiagram_WithShowNonScheduledSectionEnabled_ShouldShowTwoDiagramSurfaces()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.Create())
			{
				var grid = config.Control.FindChildren<Grid>().Single(x => x.Name == "DiagramAreaControlsGrid");

				AssertEquals(292d, grid.ColumnDefinitions[0].ActualWidth);
				AssertEquals("The spliter column should be shown. SAD!", 8d, grid.ColumnDefinitions[1].ActualWidth);
				AssertEquals("The non-scheduled diagram area column should be shown. SAD!", 500d, grid.ColumnDefinitions[2].ActualWidth);

				AssertNotNull("The control should be instantiated. SAD!", config.Control.NonScheduledDiagramControl);
			}
		}

		public void TestNonScheduledDiagramControl_ShouldBeMarkedAsNonScheduled()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.Create())
			{
				AssertEquals(false, config.Control.MainDiagramControl.ViewModel.IsNonScheduled);
				AssertEquals(true, config.Control.NonScheduledDiagramControl.ViewModel.IsNonScheduled);
			}
		}

		public void TestMainDiagramControl_WhenNotShowingNonScheduledSection_ShouldNotBeMarkedAsNonScheduled()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				AssertEquals(false, config.Control.MainDiagramControl.ViewModel.IsNonScheduled);
			}
		}

		public void TestMainDiagramControl_ShouldExcludeNonScheduledNodes()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				var viewModel = config.Control.MainDiagramControl.NetworkViewModel;
				AssertContainsExactElementsInAnyOrder(new[] { "Regular Entity 1", "Regular Entity 2" }, viewModel.ScheduledNodes.Select(x => x.Name));
			}
		}

		public void TestNonScheduledControl_ShouldOnlyHaveNonScheduledNodes()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				var viewModel = config.Control.NonScheduledDiagramControl.NetworkViewModel;
				AssertContainsExactElementsInAnyOrder(new[] { "Nonscheduled Entity 1", "Nonscheduled Entity 2" }, viewModel.NonScheduledNodes.Select(x => x.Name));
			}
		}

		public void TestMainDiagramControl_ShouldExcludeNonScheduledConnections()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				var viewModel = config.Control.MainDiagramControl.NetworkViewModel;
				AssertContainsExactElementsInAnyOrder(new[] { "Regular Entity 1" }, viewModel.ScheduledConnections.Select(x => x.SourceConnector.ParentNode.Name));
			}
		}

		public void TestNonScheduledControl_ShouldOnlyHaveNonScheduledConnections()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				var viewModel = config.Control.NonScheduledDiagramControl.NetworkViewModel;
				AssertContainsExactElementsInAnyOrder(new[] { "Nonscheduled Entity 1" }, viewModel.NonScheduledConnections.Select(x => x.SourceConnector.ParentNode.Name));
			}
		}

		public void TestMoveNodesInMainSection_ShouldNotAdjustDiagramWidthInNonScheduledSection_ButShouldStillAdjustHeight()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				var startingDimensions = DiagramDimensionSet.Get(config.Control);

				NetworkTestHelper.SimulateDraggingNodeToNewPosition(config.RegularNode1, 1000, 1000, config.NetworkView);

				var dimensionsAfterMovingNonRegularNode = DiagramDimensionSet.Get(config.Control);

				AssertGreaterThan(dimensionsAfterMovingNonRegularNode.RegularWidth, startingDimensions.RegularWidth);
				AssertGreaterThan(dimensionsAfterMovingNonRegularNode.RegularHeight, startingDimensions.RegularHeight);

				AssertEquals("The nonscheduled section should not have resized when regular nodes were moved. SAD!", startingDimensions.NonScheduledWidth, dimensionsAfterMovingNonRegularNode.NonScheduledWidth);
				AssertEquals("The nonscheduled section should increase in height to match the regular section when regular nodes are moved. SAD!", dimensionsAfterMovingNonRegularNode.RegularHeight, dimensionsAfterMovingNonRegularNode.NonScheduledHeight);

				NetworkTestHelper.SimulateDraggingNodeToNewPosition(config.NonScheduledNode1, 800, 800, config.NonScheduledNetworkView);

				var dimensionsAfterMovingNonScheduledNode = DiagramDimensionSet.Get(config.Control);

				AssertGreaterThan(dimensionsAfterMovingNonScheduledNode.NonScheduledWidth, dimensionsAfterMovingNonRegularNode.NonScheduledWidth);
				AssertEquals("The nonscheduled node was moved down, but not beyond the height of the nonscheduled section. So no height changes should have occured. SAD!", dimensionsAfterMovingNonRegularNode.NonScheduledHeight, dimensionsAfterMovingNonScheduledNode.NonScheduledHeight);

				AssertEquals("Moving a nonscheduled node should not affect the width of the regular section. SAD!", dimensionsAfterMovingNonRegularNode.RegularWidth, dimensionsAfterMovingNonScheduledNode.RegularWidth);
				AssertEquals("The nonscheduled node was moved down, but not beyond the height of the regular section. So the regular section should not have been affected. SAD!", dimensionsAfterMovingNonRegularNode.RegularHeight, dimensionsAfterMovingNonScheduledNode.RegularHeight);
			}
		}

		public void TestMoveNodesInNonScheduledSection_ShouldNotAdjustDiagramWidthInMainSection_ButShouldStillAdjustHeight()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				var startingDimensions = DiagramDimensionSet.Get(config.Control);

				NetworkTestHelper.SimulateDraggingNodeToNewPosition(config.NonScheduledNode1, 1000, 1000, config.NonScheduledNetworkView);

				var dimensionsAfterMovingNonScheduledNode = DiagramDimensionSet.Get(config.Control);

				AssertGreaterThan(dimensionsAfterMovingNonScheduledNode.NonScheduledWidth, startingDimensions.NonScheduledWidth);
				AssertGreaterThan(dimensionsAfterMovingNonScheduledNode.NonScheduledHeight, startingDimensions.NonScheduledHeight);

				AssertEquals("The main section should not have resized when nonscheduled nodes were moved. SAD!", startingDimensions.RegularWidth, dimensionsAfterMovingNonScheduledNode.RegularWidth);
				AssertEquals("The main section should increase in height to match the nonscheduled section when nonscheduled nodes are moved. SAD!", dimensionsAfterMovingNonScheduledNode.NonScheduledHeight, dimensionsAfterMovingNonScheduledNode.RegularHeight);

				NetworkTestHelper.SimulateDraggingNodeToNewPosition(config.RegularNode1, 800, 800, config.NetworkView);

				var dimensionsAfterMovingRegularNode = DiagramDimensionSet.Get(config.Control);

				AssertGreaterThan(dimensionsAfterMovingRegularNode.RegularWidth, dimensionsAfterMovingNonScheduledNode.RegularWidth);
				AssertEquals("The regular node was moved down, but not beyond the height of the regular section. So no height changes should have occured. SAD!", dimensionsAfterMovingNonScheduledNode.RegularHeight, dimensionsAfterMovingRegularNode.RegularHeight);

				AssertEquals("Moving a regular node should not affect the width of the nonscheduled section. SAD!", dimensionsAfterMovingNonScheduledNode.NonScheduledWidth, dimensionsAfterMovingRegularNode.NonScheduledWidth);
				AssertEquals("The regular node was moved down, but not beyond the height of the nonscheduled section. So the nonscheduled section should not have been affected. SAD!", dimensionsAfterMovingNonScheduledNode.NonScheduledHeight, dimensionsAfterMovingRegularNode.NonScheduledHeight);
			}
		}

		class DiagramDimensionSet
		{
#pragma warning disable CS0618
			public static DiagramDimensionSet Get(NetworkUserControl control)
			{
				var regularViewModel = control.MainDiagramControl.ViewModel;
				var nonScheduledViewModel = control.NonScheduledDiagramControl.ViewModel;

				return new DiagramDimensionSet(regularViewModel.ContentWidth, regularViewModel.ContentHeight, nonScheduledViewModel.ContentWidth, nonScheduledViewModel.ContentHeight);
			}
#pragma warning restore CS0618

			DiagramDimensionSet(double regularWidth, double regularHeight, double nonScheduledWidth, double nonScheduledHeight)
			{
				RegularWidth = regularWidth;
				RegularHeight = regularHeight;
				NonScheduledWidth = nonScheduledWidth;
				NonScheduledHeight = nonScheduledHeight;
			}

			public double RegularWidth { get; }
			public double RegularHeight { get; }
			public double NonScheduledWidth { get; }
			public double NonScheduledHeight { get; }
		}

		public void TestClickOnNonScheduledBackground_ShouldDeselectAllNodes_IncludingRegularNodes()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				config.NetworkView.SelectAll();
				AssertContainsExactElementsInAnyOrder(new[] { config.RegularNode1, config.RegularNode2 }, config.NetworkView.SelectedNodes);

				config.Control.NonScheduledDiagramControl.UpdateSelection_ForTest();
				AssertContainsExactElementsInAnyOrder("Clicking the background of the nonscheduled section should clear the selected nodes in the main area. SAD!", Array.Empty<NodeViewModel>(), config.NetworkView.SelectedNodes);
			}
		}

		public void TestClickOnRegularBackground_ShouldDeselectAllNodes_IncludingNonscheduledNodes()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				config.NonScheduledNetworkView.SelectAll();
				AssertContainsExactElementsInAnyOrder(new[] { config.NonScheduledNode1, config.NonScheduledNode2 }, config.NonScheduledNetworkView.SelectedNodes);

				config.Control.MainDiagramControl.UpdateSelection_ForTest();
				AssertContainsExactElementsInAnyOrder("Clicking the background of the main section should clear the selected nodes in the nonscheduled area. SAD!", Array.Empty<NodeViewModel>(), config.NonScheduledNetworkView.SelectedNodes);
			}
		}

		public void TestRightClickOnNonScheduledBackground_ShouldDeselectAllNodes_IncludingScheduledNodes()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				config.NetworkView.SelectAll();
				AssertContainsExactElementsInAnyOrder(new[] { config.RegularNode1, config.RegularNode2 }, config.NetworkView.SelectedNodes);

				config.NonScheduledNetworkView.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(0, 0));
				AssertContainsExactElementsInAnyOrder("Right-clicking the background of the nonscheduled section should clear the selected nodes in the main area. SAD!", Array.Empty<NodeViewModel>(), config.NetworkView.SelectedNodes);
			}
		}

		public void TestRightClickOnScheduledBackground_ShouldDeselectAllNodes_IncludingNonscheduledNodes()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				config.NonScheduledNetworkView.SelectAll();
				AssertContainsExactElementsInAnyOrder(new[] { config.NonScheduledNode1, config.NonScheduledNode2 }, config.NonScheduledNetworkView.SelectedNodes);

				config.NetworkView.RaiseEvent(config.MouseController.CreateRightMouseUpEvent(0, 0));
				AssertContainsExactElementsInAnyOrder("Right-clicking the background of the main section should clear the selected nodes in the nonscheduled area. SAD!", Array.Empty<NodeViewModel>(), config.NonScheduledNetworkView.SelectedNodes);
			}
		}

		public void TestClickOnNonScheduledShape_WhenRegularShapeIsSelected_ShouldSelectNonScheduledShape_AndDeselectRegularShape()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				config.NetworkView.SelectedNode = config.RegularNode1;
				AssertContainsExactElementsInAnyOrder(new[] { config.RegularNode1 }, config.NetworkView.SelectedNodes);
				AssertContainsExactElementsInAnyOrder(Array.Empty<NodeViewModel>(), config.NonScheduledNetworkView.SelectedNodes);

				config.NonScheduledNetworkView.SelectedNode = config.NonScheduledNode1;
				AssertContainsExactElementsInAnyOrder("Clicking a nonscheduled shape should select it. SAD!", new[] { config.NonScheduledNode1 }, config.NonScheduledNetworkView.SelectedNodes);
				AssertContainsExactElementsInAnyOrder("Clicking a nonscheduled shape should deselect the regular one. SAD!", Array.Empty<NodeViewModel>(), config.NetworkView.SelectedNodes);
			}
		}

		public void TestClickOnRegularShape_WhenNonScheduledShapeIsSelected_ShouldSelectRegularShape_AndDeselectNonScheduledShape()
		{
			using (var config = NetworkWithNonScheduledGuiTestConfig.CreateWithNodes())
			{
				config.NonScheduledNetworkView.SelectedNode = config.NonScheduledNode1;
				AssertContainsExactElementsInAnyOrder(new[] { config.NonScheduledNode1 }, config.NonScheduledNetworkView.SelectedNodes);
				AssertContainsExactElementsInAnyOrder(Array.Empty<NodeViewModel>(), config.NetworkView.SelectedNodes);

				config.NetworkView.SelectedNode = config.RegularNode1;
				AssertContainsExactElementsInAnyOrder("Clicking a regular shape should select it. SAD!", new[] { config.RegularNode1 }, config.NetworkView.SelectedNodes);
				AssertContainsExactElementsInAnyOrder("Clicking a regular shape should deselect the nonscheduled one. SAD!", Array.Empty<NodeViewModel>(), config.NonScheduledNetworkView.SelectedNodes);
			}
		}

		#endregion

		#region Layout

		public void TestEntityDetals_WhenStyleDiagramSupported_ShouldBeVisibile()
		{
			AssertControlVisibilityForSupportedActions(NetworkActions.StyleDiagram, true, networkControl => networkControl.FindChildren<EntityDetails>().Single());
		}

		public void TestEntityDetals_WhenStyleDiagramNotSupported_ShouldNotBeVisibile()
		{
			AssertControlVisibilityForSupportedActions(NetworkActions.None, false, networkControl => networkControl.FindChildren<EntityDetails>().Single());
		}

		public void TestHeaderLine_WhenStyleDiagramSupported_ShouldBeVisibile()
		{
			AssertControlVisibilityForSupportedActions(NetworkActions.StyleDiagram, true, networkControl => networkControl.FindChildren<Line>().Single(x => x.Name == "HeaderLine"));
		}

		public void TestHeaderLine_WhenStyleDiagramNotSupported_ShouldNotBeVisibile()
		{
			AssertControlVisibilityForSupportedActions(NetworkActions.None, false, networkControl => networkControl.FindChildren<Line>().Single(x => x.Name == "HeaderLine"));
		}

#pragma warning disable CS0618
		static void AssertControlVisibilityForSupportedActions(NetworkActions supportedActions, bool expectedVisible, Func<NetworkUserControl, UIElement> elementGetter)
		{
			var diagram = new Entity { SupportedActions = supportedActions };

			using (var config = NetworkGuiTestConfig.Create(diagram))
			{
				var element = elementGetter.Invoke(config.Control);
				AssertEquals(expectedVisible, element.IsVisible);
			}
		}
#pragma warning restore CS0618

		#endregion

		public void TestChangeTextColor()
		{
			var channel1 = new DummyChannel("Bend", 500);
			var channel2 = new DummyChannel("Snap", 500);

			var diagram = new Entity();
			diagram.CompletionCriteria = "Completion Criteria";
			diagram.Name = "Diagram";
			diagram.IsDiagramScaled = true;
			diagram.CanHaveChildren = true;
			diagram.DiagramChannels = new[] { channel1, channel2 };
			diagram.ForeColor = System.Drawing.Color.Black;

			using (var config = NetworkGuiTestConfig.Create(diagram, shouldShowWindow: false))
			{
				config.Control.Height = 1600;

				var shapeEntity = (Entity)new StaticNetworkAction(() => config.Network.CreateNewEntity(ShapeTypes.Shape)).Execute();
				var node = config.Control.MainDiagramControl.ViewModel.CreateNewNode(new Location(10, 300), shapeEntity);
				node.Name = "Node";
				node.Width = 300;
				node.Height = 150;
				shapeEntity.ForeColor = System.Drawing.Color.Black;
				shapeEntity.Parent = diagram;

				config.Window.Show();

				var entityDetailDiagram = config.Control.FindChildren<EntityDetails>().Single(e => (e.DataContext as NodeViewModel).Name == "Diagram");
				var entityDetailNode = config.Control.FindChildren<EntityDetails>().Single(e => (e.DataContext as NodeViewModel).Name == "Node");
				var grid = config.Control.FindChildren<DynamicGrid>().Single();

				CombineAssertions("Initial state", () =>
				{
					AssertEquals("Diagram header", Brushes.Black.ToString(), (entityDetailDiagram.FindName("HeadingTextBox") as TextBox).Foreground.ToString());
					AssertEquals("Shape header", Brushes.Black.ToString(), (entityDetailNode.FindName("HeadingTextBox") as TextBox).Foreground.ToString());

					AssertEquals("Diagram non scheduled header", Brushes.Black.ToString(), (entityDetailDiagram.FindName("NonScheduledHeadingTextBox") as TextBox).Foreground.ToString());
					AssertEquals("Shape non scheduled header", Brushes.Black.ToString(), (entityDetailNode.FindName("NonScheduledHeadingTextBox") as TextBox).Foreground.ToString());

					AssertEquals("Diagram criteria", Brushes.Gray.ToString(), (entityDetailDiagram.FindName("CompletionCriteriaTextBox") as TextBox).Foreground.ToString());
					AssertEquals("Shape criteria", Brushes.Black.ToString(), (entityDetailNode.FindName("CompletionCriteriaTextBox") as TextBox).Foreground.ToString());
				});

				var rowLabels = grid.FindChildren<TextBlock>().Where(t => t.Tag.ToString() == "ChannelHeaderTag");
				AssertEquals("Number of channel rows", 2, rowLabels.Count());
				foreach (var label in rowLabels)
				{
					AssertEquals(Brushes.Black.ToString(), label.Foreground.ToString());
				}

				var textColorActions = config.Control.ViewModel.MenuItems.Single(m => m?.Name == "Text Color").Action.GetChildActions();
				textColorActions.Single(x => x.GetName().ToString() == "Chartreuse").Execute();
				System.Windows.Forms.Application.DoEvents();

				CombineAssertions("After changing the foreground text colour", () =>
				{
					AssertEquals(System.Drawing.Color.Chartreuse, diagram.ForeColor);
					AssertEquals(System.Drawing.Color.Black, shapeEntity.ForeColor);

					AssertEquals("Diagram criteria", Brushes.Gray.ToString(), (entityDetailDiagram.FindName("CompletionCriteriaTextBox") as TextBox).Foreground.ToString());
					AssertEquals("Shape criteria", Brushes.Black.ToString(), (entityDetailNode.FindName("CompletionCriteriaTextBox") as TextBox).Foreground.ToString());
				});
			}
		}

		#region Ribbon

		public void TestShouldRecalculateNameAndDescription_OnCertainNetworkEvents()
		{
			CombineAssertions(nameof(RefreshType.Affinities), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new ResponsiveActionToTestNameAndDescription(networkViewModel), "Name", (action) => action.NameRequestsCounter);
				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new ResponsiveActionToTestNameAndDescription(networkViewModel), "Description", (action) => action.DescriptionRequestsCounter);

				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new PickyActionToTestNameAndDescription(networkViewModel), "Name", (action) => action.NameRequestsCounter);
				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new PickyActionToTestNameAndDescription(networkViewModel), "Description", (action) => action.DescriptionRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.Affinities, (networkViewModel) => new IndifferentActionToTestNameAndDescription(networkViewModel), "Name", (action) => action.NameRequestsCounter);
				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.Affinities, (networkViewModel) => new IndifferentActionToTestNameAndDescription(networkViewModel), "Description", (action) => action.DescriptionRequestsCounter);
			});

			CombineAssertions(nameof(RefreshType.RedrawDiagram), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new ResponsiveActionToTestNameAndDescription(networkViewModel), "Name", (action) => action.NameRequestsCounter);
				AssertActionPropertyRecalculatesOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new ResponsiveActionToTestNameAndDescription(networkViewModel), "Description", (action) => action.DescriptionRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new PickyActionToTestNameAndDescription(networkViewModel), "Name", (action) => action.NameRequestsCounter);
				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new PickyActionToTestNameAndDescription(networkViewModel), "Description", (action) => action.DescriptionRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new IndifferentActionToTestNameAndDescription(networkViewModel), "Name", (action) => action.NameRequestsCounter);
				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new IndifferentActionToTestNameAndDescription(networkViewModel), "Description", (action) => action.DescriptionRequestsCounter);
			});
		}

		public void TestShouldRecalculateIsActivated_OnCertainNetworkEvents()
		{
			CombineAssertions(nameof(RefreshType.Affinities), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new ResponsiveActionToTestIsActivated(networkViewModel), "IsActivated", (action) => action.IsActivatedRequestsCounter);

				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new PickyActionToTestIsActivated(networkViewModel), "IsActivated", (action) => action.IsActivatedRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.Affinities, (networkViewModel) => new IndifferentActionToTestIsActivated(networkViewModel), "IsActivated", (action) => action.IsActivatedRequestsCounter);
			});

			CombineAssertions(nameof(RefreshType.RedrawDiagram), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new ResponsiveActionToTestIsActivated(networkViewModel), "IsActivated", (action) => action.IsActivatedRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new PickyActionToTestIsActivated(networkViewModel), "IsActivated", (action) => action.IsActivatedRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new IndifferentActionToTestIsActivated(networkViewModel), "IsActivated", (action) => action.IsActivatedRequestsCounter);
			});
		}

		public void TestShouldRecalculateChildActions_OnCertainNetworkEvents()
		{
			CombineAssertions(nameof(RefreshType.Affinities), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new ResponsiveActionToTestChildActions(networkViewModel), "ChildActions", (action) => action.ChildActionsRequestsCounter);

				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new PickyActionToTestChildActions(networkViewModel), "ChildActions", (action) => action.ChildActionsRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.Affinities, (networkViewModel) => new IndifferentActionToTestChildActions(networkViewModel), "ChildActions", (action) => action.ChildActionsRequestsCounter);
			});

			CombineAssertions(nameof(RefreshType.RedrawDiagram), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new ResponsiveActionToTestChildActions(networkViewModel), "ChildActions", (action) => action.ChildActionsRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new PickyActionToTestChildActions(networkViewModel), "ChildActions", (action) => action.ChildActionsRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new IndifferentActionToTestChildActions(networkViewModel), "ChildActions", (action) => action.ChildActionsRequestsCounter);
			});
		}

		public void TestShouldRecalculateApplicability_OnCertainNetworkEvents()
		{
			// we expect two requests to applicability as it is also required for enabledness calculation
			// this number will be reduced to 1 after optimising network actions within WI00230632

			CombineAssertions(nameof(RefreshType.Affinities), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new ResponsiveActionToTestAccessibility(networkViewModel), "IsApplicable", (action) => action.IsApplicableRequestsCounter, expectedCounts: 2);

				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new PickyActionToTestAccessibility(networkViewModel), "IsApplicable", (action) => action.IsApplicableRequestsCounter, expectedCounts: 2);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.Affinities, (networkViewModel) => new IndifferentActionToTestAccessibility(networkViewModel), "IsApplicable", (action) => action.IsApplicableRequestsCounter);
			});

			CombineAssertions(nameof(RefreshType.RedrawDiagram), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new ResponsiveActionToTestAccessibility(networkViewModel), "IsApplicable", (action) => action.IsApplicableRequestsCounter, expectedCounts: 2);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new PickyActionToTestAccessibility(networkViewModel), "IsApplicable", (action) => action.IsApplicableRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new IndifferentActionToTestAccessibility(networkViewModel), "IsApplicable", (action) => action.IsApplicableRequestsCounter);
			});
		}

		public void TestShouldRecalculateEnabledness_OnCertainNetworkEvents()
		{
			CombineAssertions(nameof(RefreshType.Affinities), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new ResponsiveActionToTestAccessibility(networkViewModel), "IsEnabled", (action) => action.IsEnabledRequestsCounter);

				AssertActionPropertyRecalculatesOnEvent(RefreshType.Affinities, (networkViewModel) => new PickyActionToTestAccessibility(networkViewModel), "IsEnabled", (action) => action.IsEnabledRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.Affinities, (networkViewModel) => new IndifferentActionToTestAccessibility(networkViewModel), "IsEnabled", (action) => action.IsEnabledRequestsCounter);
			});

			CombineAssertions(nameof(RefreshType.RedrawDiagram), () =>
			{
				AssertActionPropertyRecalculatesOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new ResponsiveActionToTestAccessibility(networkViewModel), "IsEnabled", (action) => action.IsEnabledRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new PickyActionToTestAccessibility(networkViewModel), "IsEnabled", (action) => action.IsEnabledRequestsCounter);

				AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType.RedrawDiagram, (networkViewModel) => new IndifferentActionToTestAccessibility(networkViewModel), "IsEnabled", (action) => action.IsEnabledRequestsCounter);
			});
		}

		static void AssertActionPropertyRecalculatesOnEvent(RefreshType eventType, Func<NetworkViewModel, TestDynamicNetworkAction> actionGetter, string propertyName, Func<TestDynamicNetworkAction, int> propertyRequestedCounterGetter, int expectedCounts = 1)
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var action = actionGetter(config.Control.NetworkViewModel);
				config.Control.RibbonControlExposed_ForTesting.Model.Tabs[0].Groups[0].Items.Add(new RibbonButtonViewModel(config.RibbonViewModel, action));
				action.ResetCounters();
				AssertEquals("Precondition", 0, propertyRequestedCounterGetter(action));

				config.Network.Refresh(eventType);
				ApplicationHelper.DoEvents();

				AssertEquals($"{FullActionPropertyName(action, propertyName)} should recalculate on event {eventType}", expectedCounts, propertyRequestedCounterGetter(action));
			}
		}

		static void AssertActionPropertyDoesNotRecalculateOnEvent(RefreshType eventType, Func<NetworkViewModel, TestDynamicNetworkAction> actionGetter, string propertyName, Func<TestDynamicNetworkAction, int> propertyRequestedCounterGetter)
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var action = actionGetter(config.Control.NetworkViewModel);
				config.Control.RibbonControlExposed_ForTesting.Model.Tabs[0].Groups[0].Items.Add(new RibbonButtonViewModel(config.RibbonViewModel, action));
				action.ResetCounters();
				AssertEquals("Precondition", 0, propertyRequestedCounterGetter(action));

				config.Network.Refresh(eventType);
				ApplicationHelper.DoEvents();

				AssertEquals($"{FullActionPropertyName(action, propertyName)} should not recalculate on event {eventType}", 0, propertyRequestedCounterGetter(action));
			}
		}

		static string FullActionPropertyName(TestDynamicNetworkAction action, string propertyName) => $"{action.GetType().Name}.{propertyName}";

		#endregion

		#region Shape Inspector

		public void TestShapeInspectorVisibility()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodesAndGrandChildren())
			{
				config.Refresher.Refresh(new RefreshArgs(RefreshType.ShapeInspectorVisibilityChanged));
				ApplicationHelper.DoEvents();
				Assert(config.Control.ViewModel.ShapeInspectorVisible);

				config.Refresher.Refresh(new RefreshArgs(RefreshType.ShapeInspectorVisibilityChanged));
				ApplicationHelper.DoEvents();
				Assert(!config.Control.ViewModel.ShapeInspectorVisible);
			}
		}

		public void TestShapeInspectorNodeViewModel()
		{
			var diagram = new Entity();
			var network = new DummyNetwork { DiagramEntity = diagram };
			var networkControlViewModel = new NetworkUserControlViewModel(network, shapeInspectorVisible: true);
			var viewModel = new DiagramAreaUserControlViewModel(networkControlViewModel, false);

			var annotation = new Entity { Width = 100, Height = 100, ShapeType = ShapeTypes.Annotation, Name = "An Annotation" };
			var shapeStandAlone = new Entity { Width = 110, Height = 110, ShapeType = ShapeTypes.Shape, Name = "A Standalone Shape" };
			var shapeWithChild = new Entity { Width = 120, Height = 120, ShapeType = ShapeTypes.Shape, Name = "A Shape With a Child" };
			var shapeChild = new Entity { Width = 50, Height = 50, ShapeType = ShapeTypes.Shape, Name = "A Child Shape" };
			shapeWithChild.AddChildEntity(shapeChild);

			viewModel.ShowEntity(annotation, diagram, new Point());
			viewModel.ShowEntity(shapeStandAlone, diagram, new Point());
			viewModel.ShowEntity(shapeWithChild, diagram, new Point());
			viewModel.ShowEntity(shapeChild, shapeWithChild, new Point());

			Assert(networkControlViewModel.ShapeInspectorVisible);
			AssertNull(networkControlViewModel.ShapeInspectorNodeViewModel);

			networkControlViewModel.NetworkViewModel.SelectEntities(new INetworkEntity[] { annotation });
			AssertEquals("An Annotation", networkControlViewModel.ShapeInspectorNodeViewModel.Entity.Name);

			networkControlViewModel.NetworkViewModel.SelectEntities(new INetworkEntity[] { shapeStandAlone });
			AssertEquals("A Standalone Shape", networkControlViewModel.ShapeInspectorNodeViewModel.Entity.Name);

			networkControlViewModel.NetworkViewModel.SelectEntities(new INetworkEntity[] { shapeChild });
			AssertEquals("A Child Shape", networkControlViewModel.ShapeInspectorNodeViewModel.Entity.Name);

			networkControlViewModel.NetworkViewModel.SelectEntities(new INetworkEntity[] { shapeWithChild });
			AssertEquals("A Shape With a Child", networkControlViewModel.ShapeInspectorNodeViewModel.Entity.Name);

			networkControlViewModel.NetworkViewModel.SelectEntities(new INetworkEntity[] { shapeStandAlone, annotation });
			AssertNull(networkControlViewModel.ShapeInspectorNodeViewModel);
		}

		#endregion
	}

#pragma warning disable CS0618
	[TestedType(typeof(NetworkUserControl))]
	class NetworkUserControlBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			return new NetworkUserControl(new Entity(), new NetworkRefresher(), ribbonDataProvider: new TestRibbonDataProvider());
		}

		protected override bool LabelTextBlockIsWrappedWithMultilingualTextLabel_ProcessElementDecider(DependencyObject control)
		{
			var isSplitterButton = control is Button button && button.Name == "GridSplitterNonFunctionalButton";

			return !isSplitterButton && base.LabelTextBlockIsWrappedWithMultilingualTextLabel_ProcessElementDecider(control);
		}
	}
#pragma warning restore CS0618

}
