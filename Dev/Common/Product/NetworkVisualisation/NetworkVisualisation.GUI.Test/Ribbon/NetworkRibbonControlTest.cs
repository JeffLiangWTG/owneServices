using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CargoWise.Application;
using CargoWise.Main.Navigation.WPF;
using CargoWise.Main.Navigation.WPF.Test;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using MenuItem = System.Windows.Controls.MenuItem;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(NetworkRibbonControl))]
	public class NetworkRibbonControlTest : WPFControlBasherTest
	{
		#region Population and Visibility

		public void TestNetworkRibbonControl_ShouldBecomeInvisible_WhenPassingNullAsRibbonViewModel()
		{
			var ribbonViewModel = GetDefaultViewModel();
			AssertNotNull("Precondition", ribbonViewModel);

			var control = new NetworkRibbonControl(ribbonViewModel);
			AssertEquals(Visibility.Visible, control.Visibility);

			control.AttachViewModel(null);
			AssertEquals(Visibility.Collapsed, control.Visibility);

			control.AttachViewModel(ribbonViewModel);
			AssertEquals(Visibility.Visible, control.Visibility);
		}

		public void TestNetworkRibbonControl_ShouldBecomeInvisible_WhenRibbonViewModelIsNotSuccessfullyConstructed()
		{
			var networkViewModel = new NetworkViewModel(new DummyNetwork());
			var ribbonViewModel = new TestNetworkRibbonViewModelWithNoResources(networkViewModel, null);

			AssertNotNull("Precondition", ribbonViewModel);
			AssertEquals("Precondition", false, ribbonViewModel.IsSuccessfullyConstructed);

			var control = new NetworkRibbonControl(ribbonViewModel);
			AssertEquals(Visibility.Collapsed, control.Visibility);
		}

		public void TestShouldPopulateControls_WhenPassingInSuccessfullyConstructedRibbonViewModel()
		{
			var config = new RibbonControlTestConfig();
			AssertNotNull("Precondition", config.RibbonViewModel);
			AssertEquals("Precondition: tabs count", 4, config.RibbonViewModel.Tabs.Count);

			AssertEquals("Tabs count", 4, config.Control.RibbonExposed_ForTesting.Items.Count);
		}

		public void TestShouldBindTabs()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: tab header", "Home", config.HomeTab.Header);

			config.HomeTabViewModel.Header = config.TestMultilingual;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: tab header", "Let's change something and see if binding works", config.HomeTab.Header);
		}

		public void TestShouldBindGroups()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: group header", "Extra Group", config.ExtraGroup.Header);

			config.ExtraGroupViewModel.Header = config.TestMultilingual;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: group header", "Let's change something and see if binding works", config.ExtraGroup.Header);
		}

		public void TestShouldBindButtonLabel_ForRibbonButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button label", "Normal Button Name", config.Button.Label);

			config.ButtonViewModel.Label = config.TestMultilingual;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button label", "Let's change something and see if binding works", config.Button.Label);
		}

		public void TestShouldBindButtonLabel_ForRibbonMenuButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button label", "Menu Button Name", config.MenuButton.Label);

			config.MenuButtonViewModel.Label = config.TestMultilingual;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button label", "Let's change something and see if binding works", config.MenuButton.Label);
		}

		public void TestShouldBindButtonLabel_ForRibbonToggleButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button label", "Toggle Button Name", config.ToggleButton.Label);

			config.ToggleButtonViewModel.Label = config.TestMultilingual;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button label", "Let's change something and see if binding works", config.ToggleButton.Label);
		}

		public void TestShouldBindButtonTooltip_ForRibbonButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button tooltip", "Normal Button Description", config.Button.ToolTip);

			config.ButtonViewModel.Tooltip = config.TestMultilingual;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button tooltip", "Let's change something and see if binding works", config.Button.ToolTip);
		}

		public void TestShouldBindButtonTooltip_ForRibbonMenuButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button tooltip", "Menu Button Description", config.MenuButton.ToolTip);

			config.MenuButtonViewModel.Tooltip = config.TestMultilingual;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button tooltip", "Let's change something and see if binding works", config.MenuButton.ToolTip);
		}

		public void TestShouldBindButtonTooltip_ForRibbonToggleButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button tooltip", "Toggle Button Description", config.ToggleButton.ToolTip);

			config.ToggleButtonViewModel.Tooltip = config.TestMultilingual;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button tooltip", "Let's change something and see if binding works", config.ToggleButton.ToolTip);
		}

		public void TestShouldBindButtonImage_ForRibbonButtons()
		{
			var config = new RibbonControlTestConfig();
			var buttonImage = (DrawingImage)config.Button.LargeImageSource;
			AssertNotNull(buttonImage.Drawing);
			AssertEquals("Precondition: button image", config.RibbonViewModel.GetResource("Refresh"), buttonImage.Drawing);

			config.ButtonViewModel.Image = config.RibbonViewModel.GetResource("ZoomIn");
			buttonImage = (DrawingImage)config.Button.LargeImageSource;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button image", config.RibbonViewModel.GetResource("ZoomIn"), buttonImage.Drawing);
		}

		public void TestShouldBindButtonImage_ForRibbonMenuButtons()
		{
			var config = new RibbonControlTestConfig();
			var buttonImage = (DrawingImage)config.MenuButton.LargeImageSource;
			AssertNotNull(buttonImage.Drawing);
			AssertEquals("Precondition: button image", config.RibbonViewModel.GetResource("Plus"), buttonImage.Drawing);

			config.MenuButtonViewModel.Image = config.RibbonViewModel.GetResource("ZoomIn");
			buttonImage = (DrawingImage)config.MenuButton.LargeImageSource;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button image", config.RibbonViewModel.GetResource("ZoomIn"), buttonImage.Drawing);
		}

		public void TestShouldBindButtonImage_ForRibbonToggleButtons()
		{
			var config = new RibbonControlTestConfig();
			var buttonImage = (DrawingImage)config.ToggleButton.LargeImageSource;
			AssertNotNull(buttonImage.Drawing);
			AssertEquals("Precondition: button image", config.RibbonViewModel.GetResource("Refresh"), buttonImage.Drawing);

			config.ToggleButtonViewModel.Image = config.RibbonViewModel.GetResource("ZoomIn");
			buttonImage = (DrawingImage)config.ToggleButton.LargeImageSource;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button image", config.RibbonViewModel.GetResource("ZoomIn"), buttonImage.Drawing);
		}

		public void TestShouldBindButtonEnabledness_ForRibbonButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button enabledness", true, config.Button.IsEnabled);

			config.ButtonViewModel.IsEnabled = false;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button enabledness", false, config.Button.IsEnabled);
		}

		public void TestShouldBindButtonEnabledness_ForRibbonMenuButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button enabledness", true, config.MenuButton.IsEnabled);

			config.MenuButtonViewModel.IsEnabled = false;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button enabledness", false, config.MenuButton.IsEnabled);
		}

		public void TestShouldBindButtonEnabledness_ForRibbonToggleButtons()
		{
			var config = new RibbonControlTestConfig();
			AssertEquals("Precondition: button enabledness", true, config.ToggleButton.IsEnabled);

			config.ToggleButtonViewModel.IsEnabled = false;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button enabledness", false, config.ToggleButton.IsEnabled);
		}

		public void TestShouldBindChildActions_ForRibbonMenuButtons()
		{
			var config = new RibbonControlTestConfig();

			AssertEquals(2, config.MenuButton.Items.Count);
			var buttonViewModel1 = (RibbonButtonViewModel)config.MenuButton.Items[0];
			var buttonViewModel2 = (RibbonButtonViewModel)config.MenuButton.Items[1];
			AssertNotNull(buttonViewModel1);
			AssertNotNull(buttonViewModel2);

			var newButtonViewModel = new RibbonButtonViewModel(config.RibbonViewModel, new StaticNetworkAction());

			config.MenuButtonViewModel.ChildButtons = new RibbonButtonViewModel[] { buttonViewModel1, newButtonViewModel, buttonViewModel2 };

			AssertEquals(3, config.MenuButton.Items.Count);
			AssertEquals(buttonViewModel1, config.MenuButton.Items[0]);
			AssertEquals(newButtonViewModel, config.MenuButton.Items[1]);
			AssertEquals(buttonViewModel2, config.MenuButton.Items[2]);
		}

		public void TestShouldBindButtonIsActivated_ForRibbonToggleButtons()
		{
			var config = new RibbonControlTestConfig();
			config.ToggleButtonViewModel.IsChecked = true;
			AssertEquals(true, config.ToggleButton.IsChecked);

			config.ToggleButtonViewModel.IsChecked = false;
			AssertEquals("NetworkRibbonControl should update when RibbonViewModel is updated: button IsChecked status", false, config.ToggleButton.IsChecked);
		}

		public void TestShouldUseNameAndDescriptionOfAttachedActionForLabelAndTooltip_WhenNoOverridesProvided()
		{
			var networkViewModel = new NetworkViewModel(new DummyNetwork());
			var action = new StaticNetworkAction(name: ResString.GetMultilingualString("57B5AC39-0FD8-4E0B-B476-79B8720BED6E", "Format C:"), description: ResString.GetMultilingualString("3429C7C9-A025-41FE-9D33-970D203CFC31", "Get a special gift"));
			var ribbonViewModel = GetTestViewModelWithButtonWithoutNameAndDescriptionOverrides(action);
			var control = new NetworkRibbonControl(ribbonViewModel);
			var button = GetSingleRibbonButton(control);

			AssertEquals("Format C:", button.Label);
			AssertEquals("Get a special gift", button.ToolTip);
		}

		public void TestShouldUseLabelOverride_WhenLabelOverrideProvided()
		{
			var networkViewModel = new NetworkViewModel(new DummyNetwork());
			var action = new StaticNetworkAction(name: ResString.GetMultilingualString("551168C1-1886-43DF-BE50-6663C5F4C74E", "Format C:"), description: ResString.GetMultilingualString("28995AA6-F3DA-49DC-B79B-B5AD0CE05540", "Get a special gift"));
			var ribbonViewModel = GetTestViewModelWithButtonWithNameOverride(action);
			var control = new NetworkRibbonControl(ribbonViewModel);
			var button = GetSingleRibbonButton(control);

			AssertEquals("Button", button.Label);
			AssertEquals("Get a special gift", button.ToolTip);
		}

		public void TestShouldUseLabelAndTooltipOverrides_WhenOverridesProvided()
		{
			var networkViewModel = new NetworkViewModel(new DummyNetwork());
			var action = new StaticNetworkAction(name: ResString.GetMultilingualString("6A0D4149-DDE8-49FA-B21C-B6FC1902878C", "Format C:"), description: ResString.GetMultilingualString("F5196BBA-2A4B-4FC7-9385-60B48BDB4909", "Get a special gift"));
			var ribbonViewModel = GetTestViewModelWithButtonWithNameAndDescriptionOverrides(action);
			var control = new NetworkRibbonControl(ribbonViewModel);
			var button = GetSingleRibbonButton(control);

			AssertEquals("Button", button.Label);
			AssertEquals("Click me!", button.ToolTip);
		}

		#endregion

		#region Tab Selection

		public void TestTabSelection()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				config.Control.SetDataContext(config.Network, isReloading: false);
				AssertEquals(0, config.Control.RibbonControlExposed_ForTesting.SelectedTabIndex);

				config.Control.RibbonControlExposed_ForTesting.SelectTab(2);
				AssertEquals(2, config.Control.RibbonControlExposed_ForTesting.SelectedTabIndex);

				config.Control.RibbonControlExposed_ForTesting.SelectTab(-1);
				AssertEquals(2, config.Control.RibbonControlExposed_ForTesting.SelectedTabIndex);

				var tabsCount = config.Control.RibbonControlExposed_ForTesting.RibbonExposed_ForTesting.Items.Count;
				config.Control.RibbonControlExposed_ForTesting.SelectTab(tabsCount);
				AssertEquals(2, config.Control.RibbonControlExposed_ForTesting.SelectedTabIndex);
			}
		}

		#endregion

		#region Appearance

		public void TestRibbonElementsShouldNotHaveSillyContextMenu()
		{
			var window = new Window();
			var ribbonViewModel = GetDefaultViewModel();
			var control = new NetworkRibbonControl(ribbonViewModel);
			window.Content = control;
			var retriever = new XamlEmbeddedControlRetriever();
			var controlsWithSillyMenus = new List<string>();
			retriever.OnControlProcessing += (s, e) =>
			{
				if (s is FrameworkElement frameworkElement && !(frameworkElement is Ribbon) && frameworkElement.ContextMenu != null && frameworkElement.ContextMenu.Items.Count != 0)
				{
					controlsWithSillyMenus.Add(e.ControlFullName);
				}
			};
			window.Show();
			retriever.Retrieve(control);
			window.Close();

			AssertEquals($@"The following elements should not have context menus:
				{string.Join(Environment.NewLine, controlsWithSillyMenus)}", 0, controlsWithSillyMenus.Count);
		}

		public void TestRibbonControlShouldNotHaveTitlePanel()
		{
			var window = new Window();
			var ribbonViewModel = GetDefaultViewModel();
			var control = new NetworkRibbonControl(ribbonViewModel);
			window.Content = control;
			var retriever = new XamlEmbeddedControlRetriever();

			DockPanel docPanel = null;

			retriever.OnControlProcessing += (s, e) =>
			{
				if (s is FrameworkElement frameworkElement && frameworkElement is Ribbon)
				{
					var titlePanel = WpfUtils.FindElementWithType<RibbonTitlePanel>(frameworkElement);
					docPanel = (DockPanel)titlePanel?.Parent;
				}
			};

			window.Show();
			retriever.Retrieve(control);
			window.Close();

			AssertNull("Should not have a RibbonTitlePanel Dock Panel", docPanel);
		}

		public void TestRibbonControlShouldNotHaveApplicationMenu()
		{
			var window = new Window();
			var ribbonViewModel = GetDefaultViewModel();
			var control = new NetworkRibbonControl(ribbonViewModel);
			window.Content = control;
			var retriever = new XamlEmbeddedControlRetriever();

			retriever.OnControlProcessing += (s, e) =>
			{
				if (s is RibbonApplicationMenu menu)
				{
					AssertEquals(Visibility.Collapsed, menu.Visibility);
				}
			};

			window.Show();
			retriever.Retrieve(control);
			window.Close();

			Assert(true);
		}

		#endregion

		#region System context menu

		public void TestRibbonContextMenuShouldOnlyShowMinimizeMenuItem()
		{
			var window = new Window();
			var control = new NetworkRibbonControl();
			window.Content = control;
			window.Show();

			try
			{
				SimulateRibbonContextMenuOpeningEvent(control);
				AssertEquals("Ribbon context menu should ONLY contain one item", 1, control.RibbonExposed_ForTesting.ContextMenu.Items.Count);

				var soleContextMenuItem = control.RibbonExposed_ForTesting.ContextMenu.Items[0] as RibbonMenuItem;
				AssertNotNull("Ribbon context menu's only item should be a RibbonMenuItem (as opposed to, for example, a separator)", soleContextMenuItem);
				AssertEquals("Ribbon context menu's only item should be titled 'Minimize the Ribbon'", "Mi_nimize the Ribbon", (string)soleContextMenuItem.Header);
			}
			finally
			{
				window.Close();
			}
		}

		public void TestMinimizeRibbonContextMenuItemShouldNotThrow()
		{
			var control = new NetworkRibbonControl(GetDefaultViewModel());
			var ribbon = control.RibbonExposed_ForTesting;
			var window = new Window { Content = control };
			window.Show();

			try
			{
				Assert("Expecting the button handler to *NOT* have been triggered before our context menu item is clicked", !control.RibbonButtonClicked_ForTest);

				// Show the context menu containing our item - otherwise the Click event handler won't be executed and the test will pass when it shouldn't.
				ribbon.ContextMenu.PlacementTarget = ribbon;
				ribbon.ContextMenu.IsOpen = true;

				var menuItem = ribbon.ContextMenu.Items.OfType<RibbonMenuItem>().Single(i => (string)i.Header == "Mi_nimize the Ribbon");
				AssertNoExceptionThrown(() => menuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)));
				Assert("Expecting the button handler to have been triggered once our context menu item has been clicked", control.RibbonButtonClicked_ForTest);
			}
			finally
			{
				window.Close();
			}
		}

		#endregion

		#region Action Execution

		public void TestButtonClickShouldExecuteForUserChoice_ForDynamicAction()
		{
			var network = new DummyNetwork();
			var refresher = new NetworkRefresher();
			var networkViewModel = new NetworkViewModel(network);

			var strategy = new Mock<INetworkActionExecutionStrategy>();
			var action = new TestDynamicAction(networkViewModel, strategy.Object);

			strategy.Setup(s => s.ExecuteAction(action));
			strategy.Setup(s => s.IsActionApplicable(action)).Returns(NetworkActionAccessibility.Allowed);

			var window = new Window();

#pragma warning disable CS0618
			using (var parentControl = new NetworkUserControl(network.DiagramEntity, refresher))
			{
				parentControl.SetDataContext(network, isReloading: false); // to setup diagram area controls
				window.Content = parentControl;

				var ribbonViewModel = GetTestViewModelWithSingleButton(action);
				var control = new NetworkRibbonControl(ribbonViewModel);
				parentControl.Content = control;

				var button = GetSingleRibbonButton(control);

				button.PerformClick();

				Assert("We just check that click triggers the action", true);
			}
#pragma warning restore CS0618
		}

		public void TestButtonClickShouldExecute_ForStaticAction()
		{
			var network = new DummyNetwork();
			var refresher = new NetworkRefresher();

			var actionExecuted = false;
			var action = new StaticNetworkAction(action: () =>
			{
				actionExecuted = true;
			});

			var window = new Window();

#pragma warning disable CS0618
			using (var parentControl = new NetworkUserControl(network.DiagramEntity, refresher))
			{
				parentControl.SetDataContext(network, isReloading: false); // to setup diagram area controls
				window.Content = parentControl;

				var ribbonViewModel = GetTestViewModelWithSingleButton(action);
				var control = new NetworkRibbonControl(ribbonViewModel);
				parentControl.Content = control;

				var button = GetSingleRibbonButton(control);

				button.PerformClick();
				Assert(actionExecuted);
			}
#pragma warning restore CS0618
		}

		#region Node Creation

		public void TestShouldSetWidthAndHeightForNewlyCreatedNode()
		{
			var node = CreateNodeUsingRibbonButton(new Entity
			{
				Width = 300,
				Height = 150
			});
			AssertNotNull(node);
			AssertEquals(300d, node.Width);
			AssertEquals(150d, node.Height);

			node = CreateNodeUsingRibbonButton(new Entity
			{
				Width = 200,
				Height = 100
			});
			AssertNotNull(node);
			AssertEquals(200d, node.Width);
			AssertEquals(100d, node.Height);
		}

		NodeViewModel CreateNodeUsingRibbonButton(INetworkEntity createdEntity)
		{
			var network = new DummyNetwork();
			var refresher = new NetworkRefresher();

			var action = new StaticNetworkAction(action: () => createdEntity);

			var window = new Window();

#pragma warning disable CS0618
			using (var parentControl = new NetworkUserControl(network.DiagramEntity, refresher))
			{
				parentControl.SetDataContext(network, isReloading: false); // to setup diagram area controls
				window.Content = parentControl;

				var ribbonViewModel = GetTestViewModelWithSingleButton(action);
				var control = new NetworkRibbonControl(ribbonViewModel);
				parentControl.Content = control;

				var button = GetSingleRibbonButton(control);

				button.PerformClick();

				var createdNode = parentControl.NetworkViewModel.Nodes.SingleOrDefault();
				return createdNode;
			}
#pragma warning restore CS0618
		}

		#endregion

		#endregion

		#region Multilingual Label Editing Support

		public void TestF2ClickOnRibbonButtonShouldOpenFeedbackForm()
		{
			var actionExecuted = false;
			var action = new StaticNetworkAction(action: () =>
			{
				actionExecuted = true;
			});

			var ribbonViewModel = GetTestViewModelWithSingleButton(action);
			var buttonViewModel = ribbonViewModel.Tabs.Single().Groups.Single().Items.Single();
			var multilingual = buttonViewModel.Label;

			var translationFeedbackManager = new Mock<IWpfTranslationFeedbackManager>();

			translationFeedbackManager.Setup(m => m.InTranslationFeedbackMode()).Returns(true); //emulate pressing F2 key
			translationFeedbackManager.Setup(m => m.OpenFeedbackForm(multilingual));

			using (ObjectFactory.Substitute(translationFeedbackManager.Object))
			{
				var window = new Window();
				var control = new NetworkRibbonControl(ribbonViewModel);
				window.Content = control;
				var button = GetSingleRibbonButton(control);

				button.PerformClick();

				Assert("The action should not be executed", !actionExecuted);
			}
		}

		#endregion

		#region Search

		public void TestSearchBoxShouldWorkBothBeforeAndAfterRefresh()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var networkViewModel = config.Control.NetworkViewModel;

				var node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1");
				var node2 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity2");
				CombineAssertions("Precondition", () =>
				{
					AssertEquals(0.0, node1.X);
					AssertEquals(0.0, node1.Y);
					AssertEquals(0.0, node2.X);
					AssertEquals(0.0, node2.Y);
					AssertEquals(false, node1.IsSelected);
					AssertEquals(false, node2.IsSelected);
					AssertNull(networkViewModel.FirstSelectedEntity);
				});

				AssertContainsExactElementsInAnyOrder(new[] { "Name", "Description", "JobNumberReadableText", "AppliedAttributesReadableText", "Notes", "JobName", "CompletionCriteria" },
					NetworkSearchConfiguration.GetSearchableProperties().Select(p => p.Name));

				SimulateTypingInSearchBox(config.Control, "Entity");
				CombineAssertions(() =>
				{
					AssertEquals("Node1 should be selected", true, node1.IsSelected);
					AssertEquals("Node2 should not be selected", false, node2.IsSelected);
				});

				config.Network.Refresh(RefreshType.RedrawDiagram);
				System.Windows.Forms.Application.DoEvents();
				var newNetworkViewModel = config.Control.NetworkViewModel;
				AssertNotEquals("New NetworkViewModel should be created during refresh", networkViewModel, newNetworkViewModel);

				var newNode1 = newNetworkViewModel.Nodes.Single(n => n.Entity.Name == "Entity1");
				var newNode2 = newNetworkViewModel.Nodes.Single(n => n.Entity.Name == "Entity2");
				CombineAssertions("Nodes should be different, although they should relate to the same entities", () =>
				{
					Assert(!ReferenceEquals(node1, newNode1));
					Assert(!ReferenceEquals(node2, newNode2));
					AssertEquals(node1, newNode1);
					AssertEquals(node2, newNode2);
				});

				AssertEquals(false, newNode1.IsSelected);
				AssertEquals(false, newNode2.IsSelected);

				SimulateTypingInSearchBox(config.Control, "Entity");
				AssertEquals("Search should work after refresh", true, newNode1.IsSelected);
				AssertEquals(false, newNode2.IsSelected);
			}
		}

		public void TestShouldUpdateSearchResultsAfterAddingNewShape()
		{
			using (var config = NetworkGuiTestConfig.Create())
			{
				var networkViewModel = config.Control.NetworkViewModel;

				var node1 = NetworkVisualisationTestHelper.CreateEntityWithNodeAndAddToNetwork(networkViewModel, "Entity1");
				AssertEquals(1, networkViewModel.Nodes.Count());
				AssertEquals(false, node1.IsSelected);

				SimulateTypingInSearchBox(config.Control, "Entity");
				AssertEquals(true, node1.IsSelected);

				NetworkTestHelper.CreateNewEntityUsingContextMenu(config.Control, "Entity2");
				System.Windows.Forms.Application.DoEvents();
				AssertEquals(networkViewModel, config.Control.NetworkViewModel);
				AssertEquals(2, networkViewModel.Nodes.Count());
				var node2 = networkViewModel.Nodes.Last();

				AssertEquals(true, node1.IsSelected);
				AssertEquals(false, node2.IsSelected);

				SimulateSearchNext(config.Control);
				CombineAssertions("Should find the newly created shape", () =>
				{
					AssertEquals(false, node1.IsSelected);
					AssertEquals(true, node2.IsSelected);
				});
			}
		}

		#endregion

		#region Implementation

		#region Test View Models Providers

		static RibbonViewModel GetDefaultViewModel()
		{
			var provider = new TestRibbonDataProvider();
			var networkViewModel = new NetworkViewModel(new DummyNetwork());
			return provider.GetRibbonViewModel(networkViewModel, control: null);
		}

		static DefaultViewModelWithExtraControls GetDefaultViewModelWithExtraControls()
		{
			var networkViewModel = new NetworkViewModel(new DummyNetwork());
			return new DefaultViewModelWithExtraControls(networkViewModel, control: null);
		}

		static RibbonViewModel GetTestViewModelWithSingleButton(INetworkAction action) => GetTestViewModelWithButtonWithNameAndDescriptionOverrides(action);

		static RibbonViewModel GetTestViewModelWithButtonWithoutNameAndDescriptionOverrides(INetworkAction action)
		{
			var ribbonViewModel = new RibbonViewModel();
			var tabModel = new RibbonTabViewModel(ResString.GetMultilingualString("TestRibbon|Tab", "Tab"));
			ribbonViewModel.Tabs.Add(tabModel);
			var tabGroupModel = CreateRibbonGroupViewModel(ribbonViewModel, ResString.GetMultilingualString("TestRibbon|Tab|Group", "Group"));
			tabModel.Groups.Add(tabGroupModel);
			tabGroupModel.Items.Add(new RibbonButtonViewModel(ribbonViewModel, "button key", action));
			return ribbonViewModel;
		}

		static RibbonViewModel GetTestViewModelWithButtonWithNameOverride(INetworkAction action)
		{
			var ribbonViewModel = new RibbonViewModel();
			var tabModel = new RibbonTabViewModel(ResString.GetMultilingualString("TestRibbon|Tab", "Tab"));
			ribbonViewModel.Tabs.Add(tabModel);
			var tabGroupModel = CreateRibbonGroupViewModel(ribbonViewModel, ResString.GetMultilingualString("TestRibbon|Tab|Group", "Group"));
			tabModel.Groups.Add(tabGroupModel);
			tabGroupModel.Items.Add(new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString("TestRibbon|Tab|Group|Button|Name", "Button"), action));
			return ribbonViewModel;
		}

		static RibbonViewModel GetTestViewModelWithButtonWithNameAndDescriptionOverrides(INetworkAction action)
		{
			var ribbonViewModel = new RibbonViewModel();
			var tabModel = new RibbonTabViewModel(ResString.GetMultilingualString("TestRibbon|Tab", "Tab"));
			ribbonViewModel.Tabs.Add(tabModel);
			var tabGroupModel = CreateRibbonGroupViewModel(ribbonViewModel, ResString.GetMultilingualString("TestRibbon|Tab|Group", "Group"));
			tabModel.Groups.Add(tabGroupModel);
			tabGroupModel.Items.Add(new RibbonButtonViewModel(ribbonViewModel, ResString.GetMultilingualString("TestRibbon|Tab|Group|Button|Name", "Button"),
				ResString.GetMultilingualString("TestRibbon|Tab|Group|Button|Description", "Click me!"), action));
			return ribbonViewModel;
		}

		static RibbonButton GetSingleRibbonButton(NetworkRibbonControl control) => GetSingleRibbonGroup(control).Items.OfType<RibbonButton>().SingleOrDefault();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Baseline")]
		static RibbonMenuButton GetSingleRibbonMenuButton(NetworkRibbonControl control) => GetSingleRibbonGroup(control).Items.OfType<RibbonMenuButton>().SingleOrDefault();

		static RibbonGroup GetSingleRibbonGroup(NetworkRibbonControl control) => (RibbonGroup)((RibbonTab)control.RibbonExposed_ForTesting.Items[0]).Items[0];

		#endregion

		#region Context Menu

		static void SimulateRibbonContextMenuOpeningEvent(NetworkRibbonControl control)
		{
			SimulateEvent(control, "Ribbon_ContextMenuOpening");
		}

		#endregion

		#region Search Box

#pragma warning disable CS0618
		static void SimulateTypingInSearchBox(NetworkUserControl control, string searchText)
		{
			var searchBox = control.RibbonSearchBoxExposed_ForTesting;
			SimulateGettingFocus(control.RibbonControlExposed_ForTesting);
			searchBox.Text = searchText;
			SimulateKeyDown(control.RibbonControlExposed_ForTesting, Key.Enter);
		}
#pragma warning restore CS0618

#pragma warning disable CS0618
		static void SimulateSearchNext(NetworkUserControl control)
		{
			SimulateKeyDown(control.RibbonControlExposed_ForTesting, Key.Enter);
		}
#pragma warning restore CS0618

		static void SimulateGettingFocus(NetworkRibbonControl control)
		{
			SimulateEvent(control, "SearchBox_GotFocus");
		}

		static void SimulateKeyDown(NetworkRibbonControl control, Key key)
		{
			SimulateEvent(control, "SearchBox_KeyDown", new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key));
		}

		#endregion

		#region Event Simulation

		static void SimulateEvent(NetworkRibbonControl control, string methodName, EventArgs eventArgs = null)
		{
			var method = control.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new MissingMethodException($"NetworkRibbonControl Method not found: {methodName}.");

			method.Invoke(control, new object[] { null, eventArgs });
		}

		#endregion

		#region Test Classes
#pragma warning disable CS0618
		class TestNetworkRibbonViewModelWithNoResources : DefaultNetworkRibbonViewModel
		{
			public TestNetworkRibbonViewModelWithNoResources(NetworkViewModel networkViewModel, NetworkUserControl control)
				: base(networkViewModel, control)
			{
			}

			protected override void LoadResources()
			{
			}
		}
#pragma warning restore CS0618

		class TestDynamicAction : DynamicNetworkAction
		{
			public TestDynamicAction(NetworkViewModel networkViewModel, INetworkActionExecutionStrategy strategy)
				: base(networkViewModel, strategy)
			{
			}

			protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("TestDynamicAction: Name", "Name");

			protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("TestDynamicAction: Description", "Description");

			protected override bool IsActivatedCore(INetworkEntity activeEntity) => false;

			protected override IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity) => Array.Empty<INetworkAction>();

			protected override INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionAccessibility PerformPreExecutionChecksForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

			protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entityToExecute)
			{
				throw new NotImplementedException();
			}
		}

		class DefaultViewModelWithExtraControls : DefaultNetworkRibbonViewModel
		{
#pragma warning disable CS0618
			public DefaultViewModelWithExtraControls(NetworkViewModel networkViewModel, NetworkUserControl control)
				: base(networkViewModel, control)
			{
				var extraGroupModel = CreateRibbonGroupViewModel(this, ResString.GetMultilingualString("TestRibbon|Tab|ExtraGroup", "Extra Group"));
				HomeTab.Groups.InsertAtStart(extraGroupModel);

				ButtonAction = new StaticNetworkAction(
						name: ResString.GetMultilingualString("TestViewModel|NormalButton|Name", "Normal Button Name"),
						description: ResString.GetMultilingualString("TestViewModel|NormalButton|Description", "Normal Button Description"),
						iconName: "Refresh");
				ButtonModel = new RibbonButtonViewModel(this, ResString.GetMultilingualString("NormalButtonKey", "Normal Button Name"), RibbonImageLayout.BothLargeAndSmallImages, ButtonAction);
				extraGroupModel.Items.Add(ButtonModel);

				MenuButtonChildAction1 = new StaticNetworkAction(
						name: ResString.GetMultilingualString("TestViewModel|MenuButtonChild1|Name", "Menu Button Child 1 Name"),
						description: ResString.GetMultilingualString("TestViewModel|MenuButtonChild1|Description", "Menu Button Child 1 Description"));
				MenuButtonChildAction2 = new StaticNetworkAction(
						name: ResString.GetMultilingualString("TestViewModel|MenuButtonChild2|Name", "Menu Button Child 2 Name"),
						description: ResString.GetMultilingualString("TestViewModel|MenuButtonChild2|Description", "Menu Button Child 2 Description"));
				MenuButtonAction = new StaticNetworkAction(
						name: ResString.GetMultilingualString("TestViewModel|MenuButton|Name", "Menu Button Name"),
						description: ResString.GetMultilingualString("TestViewModel|MenuButton|Description", "Menu Button Description"),
						childActions: new INetworkAction[] { MenuButtonChildAction1, MenuButtonChildAction2 },
						iconName: "Plus");
				MenuButtonModel = new RibbonMenuButtonViewModel(this, ResString.GetMultilingualString("MenuButtonKey", "Menu Button Name"), RibbonImageLayout.BothLargeAndSmallImages, MenuButtonAction);
				extraGroupModel.Items.Add(MenuButtonModel);

				ToggleButtonAction = new StaticNetworkAction(
						name: ResString.GetMultilingualString("TestViewModel|ToggleButton|Name", "Toggle Button Name"),
						description: ResString.GetMultilingualString("TestViewModel|ToggleButton|Description", "Toggle Button Description"),
						iconName: "Refresh");
				ToggleButtonModel = new RibbonToggleButtonViewModel(this, ResString.GetMultilingualString("ToggleButtonKey", "Toggle Button Name"), RibbonImageLayout.BothLargeAndSmallImages, ToggleButtonAction);
				extraGroupModel.Items.Add(ToggleButtonModel);
			}
#pragma warning restore CS0618

			public INetworkAction ButtonAction;
			public RibbonButtonViewModel ButtonModel;

			public INetworkAction MenuButtonAction;
			public INetworkAction MenuButtonChildAction1;
			public INetworkAction MenuButtonChildAction2;
			public RibbonMenuButtonViewModel MenuButtonModel;

			public INetworkAction ToggleButtonAction;
			public RibbonToggleButtonViewModel ToggleButtonModel;
		}

		class RibbonControlTestConfig
		{
			public RibbonControlTestConfig()
			{
				RibbonViewModel = GetDefaultViewModelWithExtraControls();

				Control = new NetworkRibbonControl(RibbonViewModel);

				TestMultilingual = ResString.GetMultilingualString("A33E7C89-E42C-49CA-8AF0-9140E39A78CA", "Let's change something and see if binding works");

				HomeTab = (RibbonTab)Control.RibbonExposed_ForTesting.Items[0];
				HomeTabViewModel = RibbonViewModel.Tabs[0];

				ExtraGroup = (RibbonGroup)HomeTab.Items[0];
				ExtraGroupViewModel = HomeTabViewModel.Groups[0];

				Button = (RibbonButton)ExtraGroup.Items[0];
				ButtonViewModel = ExtraGroupViewModel.Items[0];

				MenuButton = (RibbonMenuButton)ExtraGroup.Items[1];
				MenuButtonViewModel = (RibbonMenuButtonViewModel)ExtraGroupViewModel.Items[1];

				ToggleButton = (RibbonToggleButton)ExtraGroup.Items[2];
				ToggleButtonViewModel = (RibbonToggleButtonViewModel)ExtraGroupViewModel.Items[2];
			}

			public DefaultViewModelWithExtraControls RibbonViewModel;

			public NetworkRibbonControl Control;

			public ResourceString TestMultilingual;

			public RibbonTab HomeTab;
			public RibbonTabViewModel HomeTabViewModel;

			public RibbonGroup ExtraGroup;
			public RibbonGroupViewModel ExtraGroupViewModel;

			public RibbonButton Button;
			public RibbonButtonViewModel ButtonViewModel;

			public RibbonMenuButton MenuButton;
			public RibbonMenuButtonViewModel MenuButtonViewModel;

			public RibbonToggleButton ToggleButton;
			public RibbonToggleButtonViewModel ToggleButtonViewModel;
		}

		static RibbonGroupViewModel CreateRibbonGroupViewModel(RibbonViewModel ribbonViewModel, ResourceString header)
		{
			return new RibbonGroupViewModel(ribbonViewModel, header, null, RibbonImageLayout.BothLargeAndSmallImages);
		}

		#endregion

		#region Overrides

		protected override Control GetControlToBashCore()
		{
			return new NetworkRibbonControl(GetDefaultViewModel());
		}

		#endregion

		#region Setup and Teardown

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<IBMSRegistry>().NCNRibbonEnabled = true;
		}

		#endregion

		#endregion
	}
}
