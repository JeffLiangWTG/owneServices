#if !WINZOR
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Main.Navigation.WPF;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Startup;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static CargoWise.Main.Navigation.SnapshotsControl;
using Image = System.Drawing.Image;
using MainFormForTest = Enterprise.Startup.Testing.MainFormTestCase.TestMainForm;
using Size = System.Windows.Size;

namespace CargoWise.Main.Navigation.Test;

sealed class NextHomeNavigationTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestGetBitmapImage()
	{
		var nextHomeNav = new NextHomeNavigation();
		Image testImage = new Bitmap(100, 100);
		var result = nextHomeNav.GetBitmapImage(testImage);
		AssertNotNull(result);
		AssertType<BitmapImage>(result);
	}

	// these two tests call directly to the Core method to bypass chacks - this is wrong
	// we need to fix the security model so that the modules can determine if they should be opened and this is not done at the UI layer
	[RequiresSTA]
	public void TestOpenModuleInNewWindow_NullCheck()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		var testModuleId = DummyModuleIDs.Dummy;

		using (var form = new TestMainFormWithUnreadItems())
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);
			mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

			AssertNoExceptionThrown("Null should have been handled.", () => nav.OpenModuleInNewWindowCore(mainFormModule, null));
		}
	}

	[RequiresSTA]
	public void TestOpenModule_NoPopup()
	{
		var testModuleId = DummyModuleIDs.DummyNoPopup;
		using (var dummyModule = ZModuleFactory.Instance.Create(testModuleId))
		{
			AssertNull("Precondition - test module should return null in ShowPopup()", dummyModule.ShowPopup());
		}

		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems())
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);
			mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

			AssertNoExceptionThrown(() => nav.OpenModuleInNewWindowCore(mainFormModule, null));
		}
	}

	[RequiresSTA]
	public void TestOpenModuleInNewWindowContainsNextAppBar()
	{
		var testModuleId = ModuleIDs.WorkItem;

		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems(true))
		{
			MainForm.IsNextAppBarEnabled = true;
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);
			mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

			nav.OpenModuleInNewWindow(mainFormModule, null);
			var win = nav.newModuleWindowForTest as ModuleWindow;

			AssertNotNull(win);
			AssertNotNull(win.NextAppBar);

			nav.newModuleWindowForTest.Close();
			nav.Dispose();
		}
	}

	[RequiresSTA]
	public void TestOpenModuleInNewWindowDoesNotContainNextAppBar()
	{
		var testModuleId = ModuleIDs.WorkItem;

		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems(false))
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);
			mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

			nav.OpenModuleInNewWindow(mainFormModule, null);
			var host = nav.newModuleFormForTest.Find(c => c.Name == "NextAppBarControl").FirstOrDefault();
			AssertNull(host);
			nav.newModuleFormForTest.Dispose();
		}
	}

	[RequiresSTA]
	public void TestOpenedModuleShouldRestoreAndActivateWindowStateWhenTheyOpenAgainInNextAppBar()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		MainForm.IsNextAppBarEnabled = true;

		using var form = new TestMainFormWithoutUnreadItems(true);
		form.Show();
		System.Windows.Forms.Application.DoEvents();

		var nav = form.NavigationBar;
		nav.ModuleOpener = form;
		nav.LoadModuleTree(ModuleTree.Tree);

		var mainFormModule = new MainFormModule(ModuleIDs.WorkItem);
		mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

		nav.OpenModule(mainFormModule);
		var win = nav.newModuleWindowForTest as ModuleWindow;
		AssertNotNull(win);
		AssertNotNull(win.NextAppBar);

		// Simulate the user clicking on the maximized window
		win.WindowState = WindowState.Maximized;
		AssertEquals(WindowState.Maximized, win.WindowState);

		// Hide the window
		win.WindowState = WindowState.Minimized;
		AssertEquals(WindowState.Minimized, win.WindowState);

		// We need to restore the window state and active it
		nav.OpenModule(mainFormModule);
		AssertEquals(WindowState.Maximized, win.WindowState);
		Assert(win.IsActive);

		nav.newModuleWindowForTest.Close();
		nav.Dispose();
	}

	[RequiresSTA]
	public void TestSnapshotOpenModuleCommand()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems(true))
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;
			var navigationViewModel = nav.navigationViewModel;
			navigationViewModel.SnapshotsViewModel.OpenModuleCommand.Execute(ModuleIDs.WorkItem.ID.ToString());
			var window = nav.newModuleWindowForTest as ModuleWindow;
			AssertNotNull(window);

			var title = window.Title;
			AssertStartsWith("Module Window Title should start with", "Work Item", title);
			nav.newModuleWindowForTest.Close();
		}
	}

	[RequiresSTA]
	public void TestOverlayBackgroundWhenSearchEnabled()
	{
		var testModuleId = ModuleIDs.WorkItem;

		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems(true))
		{
			MainForm.IsNextAppBarEnabled = true;
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);
			mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

			nav.OpenModuleInNewWindow(mainFormModule, null);
			var win = nav.newModuleWindowForTest as ModuleWindow;

			var quickSearchTextBox = FindElementByName<TextBox>(win, "QuickSearch");
			quickSearchTextBox.Focus();

			AssertNotNull(win.WinFormsSnapshot.Source);
			nav.newModuleWindowForTest.Close();
			nav.Dispose();
		}
	}

	[RequiresSTA]
	public void TestOverlayBackgroundWithNoRecentItemsWhenSearchEnabled()
	{
		var testModuleId = ModuleIDs.AccountReports;

		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems(true))
		{
			MainForm.IsNextAppBarEnabled = true;
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);
			mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

			nav.OpenModuleInNewWindow(mainFormModule, null);
			var win = nav.newModuleWindowForTest as ModuleWindow;

			var quickSearchTextBox = FindElementByName<TextBox>(win, "QuickSearch");

			AssertNoExceptionThrown("No Recent Items Module should be captured correctly", () => quickSearchTextBox.Focus());
			AssertNotNull(win.WinFormsSnapshot.Source);
			nav.newModuleWindowForTest.Close();
			nav.Dispose();
		}
	}

	[RequiresSTA]
	public void TestOpenTaskFormCommandCanOpenTaskForm()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems(true))
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;
			var navigationViewModel = nav.navigationViewModel;
			navigationViewModel.MyTasksViewModel.OpenTaskFormCommand.Execute(null);
			var window = nav.newModuleWindowForTest as ModuleWindow;
			AssertNotNull(window);

			var title = window.Title;
			AssertStartsWith("Module Window Title should start with", "Task List", title);
			nav.newModuleWindowForTest.Close();
		}
	}

	[RequiresSTA]
	public void TestOpenHolidayFormCommandCanOpenHolidayForm()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems(true))
		{
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;
			var navigationViewModel = nav.navigationViewModel;
			navigationViewModel.PublicHolidaysViewModel.OpenHolidayFormCommand.Execute(null);
			var window = nav.newModuleWindowForTest as ModuleWindow;
			AssertNotNull(window);

			var title = window.Title;
			AssertStartsWith("Module Window Title should start with", "Holidays", title);
			nav.newModuleWindowForTest.Close();
		}
	}

	[RequiresSTA]
	public void TestNextAppbarGetColorResoucesFromThemeColors()
	{
		var redColor = System.Drawing.Color.Red;
		ThemeColors.Instance.MenuBackgroundColor = redColor;
		var nextAppBar = new NextAppBar();
		var brush = nextAppBar.TopPanel.Background as SolidColorBrush;
		AssertEquals(brush.Color, System.Windows.Media.Color.FromRgb(redColor.R, redColor.G, redColor.B));
	}

	[RequiresSTA]
	public void TestMyTaskRefreshButtonExist()
	{
		var nextHomeNavigation = new NextHomeNavigation();
		AssertNotNull(nextHomeNavigation.MyTaskRefreshButton);
		var columIndex = Grid.GetColumn(nextHomeNavigation.MyTaskRefreshButton);
		AssertEquals(1, columIndex);
		AssertEquals(nextHomeNavigation.MyTaskRefreshButton.HorizontalAlignment, HorizontalAlignment.Right);
	}

	[RequiresSTA]
	public void TestRefreshMyTask()
	{
		var nextHomeNavigation = new NextHomeNavigation();
		var navigationViewModel = new NavigationViewModel();
		nextHomeNavigation.DataContext = navigationViewModel;
		var mockViewModel = new MockMyTaskViewModel();
		navigationViewModel.MyTasksViewModel = mockViewModel;
		Assert(!mockViewModel.Executed);
		NavigationCommands.Refresh.Execute(null, nextHomeNavigation.MyTaskRefreshButton);
		Assert(mockViewModel.Executed);
	}

	[RequiresSTA]
	public void TestRecentMessagesRefreshButtonExist()
	{
		var nextHomeNavigation = new NextHomeNavigation();
		AssertNotNull(nextHomeNavigation.RecentMessagesRefreshButton);
		var columIndex = Grid.GetColumn(nextHomeNavigation.RecentMessagesRefreshButton);
		AssertEquals(1, columIndex);
		AssertEquals(nextHomeNavigation.RecentMessagesRefreshButton.HorizontalAlignment, HorizontalAlignment.Right);
	}

	[RequiresSTA, ExpectNoExceptions]
	public void TestRefreshRecentMessages()
	{
		var mockRecentMessagesRepository = new Mock<IRecentMessagesRepository>();
		mockRecentMessagesRepository
			.Setup(x => x.GetLatestMessages(It.IsAny<int>()))
			.Returns(new List<RecentMessage> { new() { Body = "Message 1" }, new() { Body = "Message 2" } });
		var recentMessagesViewModel = new RecentMessagesViewModel(mockRecentMessagesRepository.Object);
		var navigationViewModel = new NavigationViewModel()
		{
			RecentMessagesViewModel = recentMessagesViewModel
		};
		var nextHomeNavigation = new NextHomeNavigation { DataContext = navigationViewModel };
		NextHomeNavigation.RefreshRecentMessagesCommand.Execute(null, nextHomeNavigation.RecentMessagesRefreshButton);
		mockRecentMessagesRepository.Verify(r => r.GetLatestMessages(It.IsAny<int>()), Times.Exactly(1));
	}

	[RequiresSTA]
	public void TestNoRecentMessagesLabelIsVisibleWhenPanelIsEmpty()
	{
		var mockRecentMessagesRepository = new Mock<IRecentMessagesRepository>();
		mockRecentMessagesRepository
			.Setup(x => x.GetLatestMessages(It.IsAny<int>()))
			.Returns(Enumerable.Empty<RecentMessage>());
		var recentMessagesViewModel = new RecentMessagesViewModel(mockRecentMessagesRepository.Object);
		var navigationViewModel = new NavigationViewModel()
		{
			RecentMessagesViewModel = recentMessagesViewModel
		};

		var uut = new NextHomeNavigation { DataContext = navigationViewModel };
		recentMessagesViewModel.Refresh();
		ForceUpdateLayout(uut);

		var actual = FindElementByName<TextBlock>(uut, "NoRecentMessagesLabel");
		AssertNotNull("No Recent Messages label should not be null", actual);
		AssertEquals("No Recent Messages label should be visible", Visibility.Visible, actual.Visibility);
	}

	[RequiresSTA]
	public void TestRecentMessagesLoadsAndDisplaysMultipleItems()
	{
		var mockRecentMessagesRepository = new Mock<IRecentMessagesRepository>();
		mockRecentMessagesRepository
			.Setup(x => x.GetLatestMessages(It.IsAny<int>()))
			.Returns(new List<RecentMessage> { new() { Body = "Message 1" }, new() { Body = "Message 2" } });
		var recentMessagesViewModel = new RecentMessagesViewModel(mockRecentMessagesRepository.Object);
		var navigationViewModel = new NavigationViewModel()
		{
			RecentMessagesViewModel = recentMessagesViewModel
		};

		var uut = new NextHomeNavigation { DataContext = navigationViewModel };
		recentMessagesViewModel.Refresh();
		ForceUpdateLayout(uut);

		var itemsControl = FindElementByName<ItemsControl>(uut, "RecentMessagesItemsControl");
		AssertNotNull(itemsControl);
		AssertEquals("Should have loaded 2 recent messages", 2, recentMessagesViewModel.RecentMessages.Count);
		AssertEquals("Should show two recent messages", 2, itemsControl.Items.Count);
	}

	[RequiresSTA]
	public void TestNextHomeNavigationContainNextAppBar()
	{
		var nextHomeNav = new NextHomeNavigation();
		var nextAppBar = nextHomeNav.NextAppBar;
		AssertNotNull(nextAppBar);
	}

	[RequiresSTA]
	public void TestTextChangeTriggersPlaceholderCollapse()
	{
		var viewModel = new NavigationViewModel();
		var control = new NextHomeNavigation();
		control.DataContext = viewModel;
		AssertEquals(Visibility.Visible, control.NextAppBar.Placeholder.Visibility);

		var quickSearchTextBox = FindElementByName<TextBox>(control, "QuickSearch");
		AssertNotEquals("Quick Search TextBox is not equal to null", quickSearchTextBox, null);

		var quickSearchShortCutText = FindElementByName<TextBox>(control, "SearchShortCutText");
		AssertNotEquals("Quick Search Short Cut is not equal to 'Q'", quickSearchShortCutText, "Q");

		quickSearchTextBox.Focus();
		quickSearchTextBox.Text = "TestText";

		AssertEquals("Placeholder text is collapsed", Visibility.Collapsed, control.NextAppBar.Placeholder.Visibility);
	}

	[RequiresSTA]
	public void TestToggleNewsButton()
	{
		CreateReleaseNote(NewsSectionTypeList.Codes.ClientNews);
		Factory.Save();

		using var form = new TestMainFormWithoutUnreadItems(true);
		form.Show();
		System.Windows.Forms.Application.DoEvents();

		var nav = (form.NavigationBar as NextNavigation).HomeControl;
		AssertNotNull(nav.ToggleNewsVisibilityButton);
		AssertEquals(true, nav.ToggleNewsVisibilityButton.IsChecked.Value);

		var newsPanelColumnDefinition = nav.NewsPanelColumnDefinition;
		AssertEquals("News panel column definition should be 1 star when ToggleNewsVisibilityButton is checked", new GridLength(1, GridUnitType.Star), newsPanelColumnDefinition.Width);

		nav.ToggleNewsVisibilityButton.IsChecked = false;
		System.Windows.Forms.Application.DoEvents();
		AssertEquals("News panel column definition should be 0 pixel when ToggleNewsVisibilityButton is unchecked", new GridLength(0, GridUnitType.Pixel), newsPanelColumnDefinition.Width);
		form.Close();
	}

	public void TestCloseSearchWindowCommandIsCorrectlyBoundToSearchWindowElements()
	{
		var viewModel = new NavigationViewModel();
		var control = new NextHomeNavigation();
		control.DataContext = viewModel;

		var searchWindow = FindElementByName<Grid>(control, "SearchWindow");
		var background = FindElementByName<Border>(control, "SearchWindowBackground");

		var searchWindowHasBinding = HasBindingCommandInElement(searchWindow, NextHomeNavigation.CloseSearchWindowCommand);

		Assert("CloseSearchWindowCommand found in parent of SearchWindow when it should not be present", !searchWindowHasBinding);
		Assert("CloseSearchWindowCommand not bound to SearchWindowBackground", background.InputBindings.OfType<MouseBinding>().Any(binding => binding.Command.Equals(NextHomeNavigation.CloseSearchWindowCommand)));
	}

	[RequiresSTA]
	public void TestCloseSearchWindowCommandIsCorrectlyBoundToRootBorder()
	{
		var viewModel = new NavigationViewModel();
		var control = new NextHomeNavigation();
		control.DataContext = viewModel;

		var rootBorder = FindElementByName<Border>(control, "RootBorder");
		var searchWindowHasBinding = HasBindingCommandInElement(rootBorder, NextAppBar.CloseSearchWindowCommand);

		Assert("CloseSearchWindowCommand found in parent of SearchWindow when it should not be present", !searchWindowHasBinding);
	}

	[RequiresSTA]
	public void TestOpenModuleWithExistingModule()
	{
		var testModuleId = ModuleIDs.WorkItem;

		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		using (var form = new TestMainFormWithoutUnreadItems(true))
		{
			MainForm.IsNextAppBarEnabled = true;
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			var nav = form.NavigationBar;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);
			mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;

			nav.OpenModuleInNewWindow(mainFormModule, null);
			var win = nav.newModuleWindowForTest;
			AssertNotNull(win);

			var forms = CargoWise.Windows.UI.ZApplication.GetOpenForms();
			Assert("Opened form count should be 2.", forms.Length == 2);

			nav.OpenModule(mainFormModule);
			forms = CargoWise.Windows.UI.ZApplication.GetOpenForms();
			Assert("Opened form count should be 2, after open a module which was already opened", forms.Length == 2);
			nav.newModuleWindowForTest.Close();
		}
	}

	bool HasBindingCommandInElement(FrameworkElement element, ICommand command)
	{
		while (element != null)
		{
			var bindings = element.InputBindings;

			foreach (InputBinding binding in bindings)
			{
				var mouseBinding = binding as MouseBinding;

				if (mouseBinding != null)
				{
					return mouseBinding.Command.Equals(command);
				}
			}

			element = element.Parent as FrameworkElement;
		}

		return false;
	}

	[RequiresSTA]
	public void TestMainMenuGridKeyBinding()
	{
		var control = new NextHomeNavigation();
		var mainMenuGrid = FindElementByName<Grid>(control, "MainMenuGrid");
		AssertEquals(mainMenuGrid.InputBindings.Count, 1);
		var binding = mainMenuGrid.InputBindings[0] as KeyBinding;
		AssertNotNull(binding);
		AssertEquals(binding!.Key, Key.Escape);
		var command = binding.Command as RoutedCommand;
		AssertNotNull(command);
		AssertEquals("KeyBinding command mismatch", command, NextHomeNavigation.CloseMainMenuCommand);
	}

	[RequiresSTA]
	public void TestQuickSearchBoxShouldNotContainMouseBinding()
	{
		var nextAppBar = new NextAppBar();
		Assert("search textbox should not contain mouse binding which prevents text selection.", !nextAppBar.QuickSearch.InputBindings.OfType<MouseBinding>().Any(binding => binding.MouseAction == MouseAction.LeftClick));
	}

	[RequiresSTA]
	public void TestEmptyNavigateSearchResults()
	{
		var viewModel = new NavigationViewModel { IsInSearchMode = true };
		var nav = new NextHomeNavigation { DataContext = viewModel };

		AssertNoExceptionThrown("Empty result should have been handled.", () => viewModel.SearchViewModel.Navigate(SearchResultNavigateDirection.Down));
		AssertNoExceptionThrown("Empty result should have been handled.", () => viewModel.SearchViewModel.Navigate(SearchResultNavigateDirection.Up));
	}

	public void TestNextHomeNavigation_MyTasksIsVisible()
	{
		var uut = new NextHomeNavigation();
		var actual = FindElementByName<Border>(uut, "MyTasksPanel");

		AssertNotNull("MyTask panel should not be null", actual);
		AssertEquals("MyTask panel should be visible", Visibility.Visible, actual.Visibility);
	}

	[RequiresSTA]
	public void TestMyTasksDisplayTextLabelBindingOpenTaskFormCommand()
	{
		var nav = new NextHomeNavigation();
		var label = nav.MyTaskDisplayNameLabel;
		AssertEquals(label.InputBindings.Count, 1);
		var binding = label.InputBindings[0] as MouseBinding;
		AssertNotNull(binding);
		AssertEquals(binding.MouseAction, MouseAction.LeftClick);
		var bindingExpression = BindingOperations.GetBindingExpression(binding, InputBinding.CommandProperty);
		AssertEquals(bindingExpression.ParentBinding.Path.Path, "OpenTaskFormCommand");
	}

	[RequiresSTA]
	public void TestHolidaysDisplayTextLabelBindingOpenHolidayFormCommand()
	{
		var nav = new NextHomeNavigation();
		var label = nav.PublicHolidaysTitle;
		AssertEquals(label.InputBindings.Count, 1);
		var binding = label.InputBindings[0] as MouseBinding;
		AssertNotNull(binding);
		AssertEquals(binding.MouseAction, MouseAction.LeftClick);
		var bindingExpression = BindingOperations.GetBindingExpression(binding, InputBinding.CommandProperty);
		AssertEquals(bindingExpression.ParentBinding.Path.Path, "OpenHolidayFormCommand");
	}

	public void TestThumbnailColumnIsCollapsedWhenImageIsNull()
	{
		var note = CreateReleaseNote(NewsSectionTypeList.Codes.ProductUpdates);
		var newsSection = new NewsSection();
		var viewModel = new NewsItemViewModel(note, newsSection);

		AssertEquals(Visibility.Collapsed, viewModel.ThumbnailVisibility);
	}

	public void TestThumbnailColumnIsNotCollapsedWhenImageIsNotNull()
	{
		var note = CreateReleaseNote(NewsSectionTypeList.Codes.ClientNews);
		note.GF_Thumbnail = CreateMinimalPng();
		var newsSection = new NewsSection();
		var viewModel = new NewsItemViewModel(note, newsSection);

		AssertEquals(Visibility.Visible, viewModel.ThumbnailVisibility);
	}

	[RequiresSTA]
	public void TestRecentMessagesIsVisible()
	{
		var uut = new NextHomeNavigation();
		var actual = FindElementByName<Border>(uut, "RecentMessagesPanel");

		AssertNotNull("Recent Messages panel should not be null", actual);
		AssertEquals("Recent Messages panel should be visible", Visibility.Visible, actual.Visibility);
	}

	[RequiresSTA]
	public void TestPublicHolidaysIsVisible()
	{
		var uut = new NextHomeNavigation();
		var actual = FindElementByName<Border>(uut, "PublicHolidaysPanel");

		AssertNotNull("Public Holidays panel should not be null", actual);
		AssertEquals("Public Holidays panel should be visible", Visibility.Visible, actual.Visibility);
	}

	[RequiresSTA]
	public void TestPublicHolidaysTitleIsVisible()
	{
		var uut = new NextHomeNavigation();
		var actual = FindElementByName<MultilingualTextLabel>(uut, "PublicHolidaysTitle");

		AssertNotNull("Public Holidays title should not be null", actual);
		AssertEquals("Public Holidays title should be visible", Visibility.Visible, actual.Visibility);
	}

	[RequiresSTA]
	public void TestSnapshotsIsVisibleIfFeatureEnabled()
	{
		var uut = new NextHomeNavigation { IsSnapshotsEnabled = true };
		var actual = FindElementByName<Border>(uut, "SnapshotsPanel");

		AssertNotNull("Snapshots border should not be null", actual);
		AssertEquals("Snapshots border should be visible", Visibility.Visible, actual.Visibility);
	}

	[RequiresSTA]
	public void TestSnapshotsIsCollapsedIfFeatureDisabled()
	{
		var uut = new NextHomeNavigation { IsSnapshotsEnabled = false };
		var actual = FindElementByName<Border>(uut, "SnapshotsPanel");

		AssertNotNull("Snapshots border should not be null", actual);
		AssertEquals("Snapshots border should be collapsed", Visibility.Collapsed, actual.Visibility);
	}

	[RequiresSTA]
	public void TestSnapshotsLoadsAndDisplaysMultipleItems()
	{
		var repositoryMock = new Mock<ISnapshotsRepository>();
		repositoryMock
			.Setup(r => r.FindByUserId(It.IsAny<Guid>()))
			.Returns(new List<Snapshot>
			{
				new Snapshot {
					Id = Guid.NewGuid(),
					ModuleFilter = new SnapshotModuleFilter
					{
						ModuleFilterId = Guid.NewGuid(),
						ModuleFilterName = $"USA Export Monitor",
						ModuleId = ModuleIDs.WorkItem,
						ModuleName = $"Consolidation"
					},
					Order = 1,
					Value = $"1510"
				},
				new Snapshot {
					Id = Guid.NewGuid(),
					ModuleFilter = new SnapshotModuleFilter
					{
						ModuleFilterId = Guid.NewGuid(),
						ModuleFilterName = $"SSC Monitor ETD",
						ModuleId = ModuleIDs.WorkItem,
						ModuleName = $"Consolidations"
					},
					Order = 2,
					Value = $"33"
				}
			});

		var snapshotsViewModel = new SnapshotsViewModel(repositoryMock.Object, Guid.NewGuid());

		var navViewModel = new NavigationViewModel
		{
			SnapshotsViewModel = snapshotsViewModel
		};

		var uut = new NextHomeNavigation { DataContext = navViewModel, IsSnapshotsEnabled = true };

		snapshotsViewModel.Refresh();
		ForceUpdateLayout(uut);

		var itemsControl = FindElementByName<ItemsControl>(uut, "SnapshotsItemsControl");
		AssertNotNull(itemsControl);

		AssertEquals("Should have loaded 2 snapshots", 2, snapshotsViewModel.Snapshots.Count);
		AssertEquals("Should show two snapshots", 2, itemsControl.Items.OfType<Snapshot>().Count());
	}

	[RequiresSTA]
	public void TestAddSnapshotCardIsVisible()
	{
		// Visible only if the number of snapshots is between 1 and (MaxSnapshots - 1)
		AssertAddSnapshotCardVisibility(false, 0);
		AssertAddSnapshotCardVisibility(true, 1);
		AssertAddSnapshotCardVisibility(true, 2);
		AssertAddSnapshotCardVisibility(true, 3);
		AssertAddSnapshotCardVisibility(true, 4);
		AssertAddSnapshotCardVisibility(false, 5);
	}

	void AssertAddSnapshotCardVisibility(bool shouldBeVisible, int snapshotsCount)
	{
		var repositoryMock = new Mock<ISnapshotsRepository>();
		var snapshots = CreateSnapshots(snapshotsCount);
		var snapshotsViewModel = new SnapshotsViewModel(repositoryMock.Object, Guid.NewGuid());
		var navigationViewModel = new NavigationViewModel
		{
			SnapshotsViewModel = snapshotsViewModel
		};
		var nextHomeNavigation = new NextHomeNavigation { DataContext = navigationViewModel, IsSnapshotsEnabled = true };

		snapshotsViewModel.Refresh();
		snapshotsViewModel.Snapshots.Clear();
		foreach (var snapshot in snapshots)
		{
			snapshotsViewModel.Snapshots.Add(snapshot);
		}

		nextHomeNavigation.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		nextHomeNavigation.Arrange(new Rect(0, 0, 1000, 1000));
		nextHomeNavigation.UpdateLayout();

		var itemsControl = FindElementByName<ItemsControl>(nextHomeNavigation, "SnapshotsItemsControl");

		AssertNotNull(itemsControl);
		AssertEquals(shouldBeVisible, itemsControl.Items.OfType<AddSnapshotPlaceholder>().Any());
	}

	IEnumerable<Snapshot> CreateSnapshots(int count)
	{
		for (var i = 0; i < count; i++)
		{
			yield return new Snapshot
			{
				Id = Guid.NewGuid(),
				ModuleFilter = new SnapshotModuleFilter
				{
					ModuleFilterId = Guid.NewGuid(),
					ModuleFilterName = $"USA Export Monitor",
					ModuleId = ModuleIDs.WorkItem,
					ModuleName = $"Consolidation"
				},
				Order = 1,
				Value = $"1510"
			};
		}
	}

	[RequiresSTA]
	public void TestSnapshotsArrowBtnMouseBinding()
	{
		var repositoryMock = new Mock<ISnapshotsRepository>();
		var snapshotItem = new Snapshot
		{
			Id = Guid.NewGuid(),
			ModuleFilter = new SnapshotModuleFilter
			{
				ModuleFilterId = Guid.NewGuid(),
				ModuleFilterName = $"USA Export Monitor",
				ModuleId = ModuleIDs.WorkItem,
				ModuleName = $"Consolidation"
			},
			Order = 1,
			Value = $"1510"
		};
		repositoryMock
			.Setup(r => r.FindByUserId(It.IsAny<Guid>()))
			.Returns(new List<Snapshot>
			{
				snapshotItem
			});

		var snapshotsViewModel = new SnapshotsViewModel(repositoryMock.Object, Guid.NewGuid());

		var navViewModel = new NavigationViewModel
		{
			SnapshotsViewModel = snapshotsViewModel
		};

		var uut = new NextHomeNavigation { DataContext = navViewModel, IsSnapshotsEnabled = true };

		snapshotsViewModel.Refresh();

		// Force WPF to create visual tree and bindings
		ForceUpdateLayout(uut);

		var itemsControl = FindElementByName<ItemsControl>(uut, "SnapshotsItemsControl");
		AssertNotNull(itemsControl);
		var container = itemsControl.ItemContainerGenerator.ContainerFromItem(snapshotItem) as FrameworkElement;
		var snapshotArrowBtn = FindElementByNameInVisualTree<Border>(container, "SnapshotsArrowBtn");
		AssertNotNull(snapshotArrowBtn);
		AssertEquals(snapshotArrowBtn.InputBindings.Count, 1);
		var binding = snapshotArrowBtn.InputBindings[0] as MouseBinding;
		AssertNotNull(binding);
		AssertEquals(binding.MouseAction, MouseAction.LeftClick);
		var bindingExpression = BindingOperations.GetBindingExpression(binding, InputBinding.CommandProperty);
		AssertEquals("Binding Command mismatch", bindingExpression.ParentBinding.Path.Path, "DataContext.RunOpenModuleFilterCommand");
	}

	T FindElementByNameInVisualTree<T>(FrameworkElement parent, string name) where T : FrameworkElement
	{
		if (parent == null)
		{
			return null;
		}
		if (parent.Name == name && parent is T result)
		{
			return result;
		}

		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
		{
			var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
			if (child == null)
			{
				continue;
			}

			var foundElement = FindElementByName<T>(child, name);
			if (foundElement != null)
			{
				return foundElement;
			}
		}
		return null;
	}

	[RequiresSTA]
	public void TestSnapshotsWithOptionalAdd_AppendsPlaceholder_IfLessThanFive()
	{
		var control = new SnapshotsControl
		{
			SnapShotsData = new ObservableCollection<Snapshot>
		{
			new Snapshot(), new Snapshot(), new Snapshot()
		}
		};

		var list = control.SnapshotsWithOptionalAdd.ToList();

		AssertEquals(4, list.Count);
		Assert(list.Last() is AddSnapshotPlaceholder);
	}

	[RequiresSTA]
	public void TestSnapshotsWithOptionalAdd_DoesNotAppendPlaceholder_IfFiveOrMore()
	{
		var control = new SnapshotsControl
		{
			SnapShotsData = new ObservableCollection<Snapshot>
		{
			new Snapshot(), new Snapshot(), new Snapshot(), new Snapshot(), new Snapshot()
		}
		};

		var list = control.SnapshotsWithOptionalAdd.ToList();

		AssertEquals(5, list.Count);
		Assert(!list.Any(x => x is AddSnapshotPlaceholder));
	}

	#region Implementations

	T FindElementByName<T>(DependencyObject parent, string name) where T : FrameworkElement
	{
		foreach (var child in LogicalTreeHelper.GetChildren(parent))
		{
			if (child is T frameworkElement && frameworkElement.Name == name)
			{
				return frameworkElement;
			}

			if (child is DependencyObject dependencyObject)
			{
				var result = FindElementByName<T>(dependencyObject, name);

				if (result != null)
				{
					return result;
				}
			}
		}

		return null;
	}

	byte[] CreateMinimalPng()
	{
		using var bmp = new Bitmap(1, 1);
		bmp.SetPixel(0, 0, System.Drawing.Color.White);
		using var ms = new System.IO.MemoryStream();
		bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
		return ms.ToArray();
	}

	void ForceUpdateLayout(NextHomeNavigation uut)
	{
		// Force WPF to create visual tree and bindings
		uut.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		uut.Arrange(new Rect(0, 0, 1000, 1000));
		uut.UpdateLayout();
	}

	[RequiresSTA]
	public void TestOpenModuleInNewWindowFormCloseShouldCloseModuleWindow()
	{
		var testModuleId = ModuleIDs.WorkItem;

		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);
		ModuleNavigation nav = null;
		using (var form = new TestMainFormWithoutUnreadItems(true))
		{
			MainForm.IsNextAppBarEnabled = true;
			form.Show();
			System.Windows.Forms.Application.DoEvents();
			nav = form.NavigationBar;
			nav.ModuleOpener = form;

			nav.LoadModuleTree(ModuleTree.Tree);
			var mainFormModule = new MainFormModule(testModuleId);
			mainFormModule.LicenceCheckpoint = Env.Licence.AlwaysAllow;
			nav.OpenModuleInNewWindow(mainFormModule, null);
			AssertNotNull(nav.newModuleWindowForTest);
			nav.newModuleFormForTest.Close();
		}

		AssertNull(nav.newModuleWindowForTest);
	}

	GlbReleaseNoteCombined CreateReleaseNote(string section)
	{
		// Please note if Factory.Save() is called in the unit test,
		// this method cannot be used to create non-WiseTech News.
		var note = Factory.New<GlbReleaseNoteCombined>();
		note.GF_Section = section;
		note.GF_Summary = "Test News Summary";
		note.GF_URL = "www.test.com";

		return note;
	}

	#endregion
}

class TestMainFormWithUnreadItems : MainFormForTest
{
	internal override bool HasUnReadItemsMandatoryToRead() => true;

	internal override bool HasLicenceAndSecurityPermissions(MainFormModule module) => true;
}

class TestMainFormWithoutUnreadItems : MainFormForTest
{
	public TestMainFormWithoutUnreadItems(bool isCargoWiseNext = false) : base(isCargoWiseNext)
	{
	}

	internal override bool HasUnReadItemsMandatoryToRead() => false;
}

class MockMyTaskViewModel : IMyTasksViewModel
{
	internal bool Executed;
	public ObservableCollection<IMyTasksItemViewModel> MyTasks { get; set; }

	public string NoMyTasksText => string.Empty;

	public string IdHeaderText => string.Empty;

	public string NameHeaderText => string.Empty;

	public string TaskDescriptionHeaderText => string.Empty;

	public string StatusHeaderText => string.Empty;

	public string DisplayName => string.Empty;

	public ICommand OpenTaskFormCommand { get; set; }

	public void Refresh()
	{
		Executed = !Executed;
	}
}
#endif
