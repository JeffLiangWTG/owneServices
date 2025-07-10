using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CargoWise.Main.Navigation;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Main.Navigation.WPF.Test;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using MenuItem = CargoWise.Main.Navigation.MenuItem;

namespace CargoWise.GUI.TileBar.Testing
{
	[TestedType(typeof(TileBarControl))]
	class TileBarControlBasherTest : WPFControlBasherTest
	{
		public void TestCategoryDetailVerticalScrollBarVisibility()
		{
			var viewModel = GetViewModelForTest();
			var control = new TileBarControl();

			control.DataContext = viewModel;

			var window = new Window { Content = new Grid { Children = { control } }, Left = 0, Top = 0, WindowStartupLocation = WindowStartupLocation.Manual, WindowState = WindowState.Maximized };
			window.Show();
			DoEvents();

			try
			{
				var categoryA = viewModel.Categories.First(c => c.Name == "Category A");
				var categoryB = viewModel.Categories.First(c => c.Name == "Category B");

				viewModel.SelectedCategory = categoryA;
				DoEvents();
				var scrollViewer = control.FindName("CategoryDetail") as ScrollViewer;
				AssertEquals("ShowSearch is true in categoryA.", ScrollBarVisibility.Auto, scrollViewer.VerticalScrollBarVisibility);

				viewModel.SelectedCategory = categoryB;
				DoEvents();
				AssertEquals("ShowSearch is false in categoryB.", ScrollBarVisibility.Disabled, scrollViewer.VerticalScrollBarVisibility);

				viewModel.SearchViewModel.WaitingForSearchToComplete();
			}
			finally
			{
				window.Close();
			}
		}

		protected override Control GetControlToBashCore()
		{
			var viewModel = new NavigationViewModel();
			var control = new TileBarControl();
			control.DataContext = viewModel;
			return control;
		}

		#region Implementation
		NavigationViewModel GetViewModelForTest()
		{
			var viewModel = new NavigationViewModel();
			var categoryA = new NavigationMenuViewModel((NoResString)"Category A", "Category A", 1, true);
			categoryA.AddSection("Section 1", "Section 1", (NoResString)"Section 1", SectionType.Favorite, 10, string.Empty, string.Empty, string.Empty, null);
			categoryA.AddItem("Section 1", string.Empty, new MenuItem((NoResString)"Item 11"));
			categoryA.AddItem("Section 1", string.Empty, new MenuItem((NoResString)"Item 12"));
			viewModel.AddCategory(categoryA);
			var categoryB = new NavigationMenuViewModel((NoResString)"Category B", "Category B", 3, false);
			categoryB.AddSection("Section 2", "Section 2", (NoResString)"Section 2", SectionType.Module, -1, "Subcategory 1", "Subcategory 1", "B", (NoResString)"Subcategory 1");
			categoryB.AddItem("Section 2", "Subcategory 1", new MenuItem((NoResString)"Item 13"));
			categoryB.AddItem("Section 2", "Subcategory 1", new MenuItem((NoResString)"Item 21"));
			categoryB.AddSection("Section 3", "Section 3", (NoResString)"Section 3", SectionType.Module, -1, "Subcategory 1", "Subcategory 1", "B", (NoResString)"Subcategory 1");
			categoryB.AddItem("Section 3", "Subcategory 1", new MenuItem((NoResString)"Item 22"));
			viewModel.AddCategory(categoryB);
			return viewModel;
		}

		static void DoEvents()
		{
			var frame = new DispatcherFrame();
			Action doExit = () => frame.Continue = false;
			Dispatcher.CurrentDispatcher.BeginInvoke(doExit, DispatcherPriority.ApplicationIdle);
			Dispatcher.PushFrame(frame);
		}
		#endregion
	}
}
