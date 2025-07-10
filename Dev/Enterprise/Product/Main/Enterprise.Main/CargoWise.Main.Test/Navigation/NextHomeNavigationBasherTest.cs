#if !WINZOR
using System;
using System.Windows.Controls;
using CargoWise.Main.Data;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Main.Navigation.WPF.Test;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test
{
	[TestedType(typeof(ModuleWindow))]
	class ModuleWindowBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var viewModel = new NavigationViewModel();
			var win = new ModuleWindow();
			win.DataContext = viewModel;
			return win;
		}
	}

	[TestedType(typeof(NextHomeNavigation))]
	class NextHomeNavigationBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var viewModel = new NavigationViewModel();
			var control = new NextHomeNavigation();
			control.DataContext = viewModel;

			return control;
		}
	}

	[TestedType(typeof(NewsPanel))]
	class NewsPanelBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore() => new NewsPanel()
		{
			DataContext = new NewsViewModel()
		};
	}

	[TestedType(typeof(SearchFilter))]
	class SearchFilterBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore() => new SearchFilter()
		{
			DataContext = new SearchViewModel()
		};
	}

	[TestedType(typeof(NextAppBar))]
	class NextAppBarBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var nextAppBar = new NextAppBar();
			var navigationViewModel = new NavigationViewModel();
			nextAppBar.DataContext = navigationViewModel;
			return nextAppBar;
		}
	}

	[TestedType(typeof(SearchResultPanel))]
	class SearchResultPanelBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore() => new SearchResultPanel()
		{
			DataContext = new NavigationViewModel()
		};
	}

	[TestedType(typeof(SnapshotLoading))]
	class SnapshotLoadingBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore() => new SnapshotLoading()
		{
			DataContext = new NavigationViewModel()
		};
	}

	[TestedType(typeof(SnapshotsControl))]
	class SnapshotsControlBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var viewModel = new SnapshotsViewModel(new SnapshotsRepository(), Guid.NewGuid());
			var control = new SnapshotsControl();
			control.DataContext = viewModel;

			return control;
		}
	}

	[TestedType(typeof(SnapshotModuleAndLayout))]
	class SnapshotModuleAndLayoutBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var s = new SnapshotModuleAndLayout();
			s.DataContext = new SnapshotsViewModel(new SnapshotsRepository(), GlbStaff.CurrentUser.PK.ToGuid());
			return s;
		}
	}
}
#endif
