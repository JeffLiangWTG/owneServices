#if !WINZOR
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using CargoWise.Main.Navigation.ViewModels;

namespace CargoWise.Main.Navigation
{
	/// <summary>
	/// Interaction logic for NextAppBar.xaml
	/// </summary>
	public partial class NextAppBar : UserControl
	{
		public NextAppBar()
		{
			InitializeComponent();
		}

#pragma warning disable CW1021 // Static Fields Are Thread Static Rule
		public static RoutedCommand NavigateSearchResultsCommand = new RoutedCommand();
		public static RoutedCommand CloseSearchWindowCommand = new RoutedCommand();
		public static RoutedCommand ExecuteSearchItemHyperlinkCommand = new RoutedCommand();
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule

		void NavigateSearchResults(object target, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is SearchResultNavigateDirection direction
				&& DataContext is NavigationViewModel viewModel
				&& viewModel.IsInSearchMode)
			{
				viewModel.SearchViewModel.Navigate(direction);

				QuickSearch.Focus();
			}
		}

		void ExecuteSearchItemHyperlink(object target, ExecutedRoutedEventArgs e)
		{
			if (DataContext is NavigationViewModel viewModel && viewModel.IsInSearchMode)
			{
				ExecuteCurrentItem(viewModel.SearchViewModel.SearchResults);
			}
		}

		void ExecuteCurrentItem(ObservableCollection<SearchResultSection> searchResultSection)
		{
			if (DataContext is NavigationViewModel viewModel
					&& viewModel.SearchViewModel.CurrentSectionIndex >= 0
					&& viewModel.SearchViewModel.CurrentSectionIndex < searchResultSection.Count)
			{
				var section = searchResultSection[viewModel.SearchViewModel.CurrentSectionIndex];
				if (viewModel.SearchViewModel.CurrentItemIndex >= 0 && viewModel.SearchViewModel.CurrentItemIndex < section.Items.Count)
				{
					section.Items[viewModel.SearchViewModel.CurrentItemIndex].LinkAction?.Execute(null);
				}
			}
			else
			{
				var item = searchResultSection
				.FirstOrDefault(s => s.Items.Count > 0)?
				.Items.FirstOrDefault();
				item?.LinkAction?.Execute(null);
			}
		}

		void ClearSelections()
		{
			if (DataContext is NavigationViewModel viewModel)
			{
				viewModel.ForceClearSearch();
				viewModel.HideAllMenus();
				QuickSearch.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			}
		}

		void CloseSearchWindow(object target, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string parameter
				&& parameter == "Escape"
				&& DataContext is NavigationViewModel viewModel
				&& !string.IsNullOrEmpty(viewModel.SearchViewModel.SearchValue))
			{
				viewModel.SearchViewModel.SearchValue = string.Empty;
				QuickSearch.Focus();
				e.Handled = true;
				return;
			}
			ClearSelections();
			e.Handled = false;
		}

		void CloseMainMenu(object target, ExecutedRoutedEventArgs e)
		{
			if (DataContext is NavigationViewModel viewModel)
			{
				viewModel.HideAllMenus();
			}
		}
	}
}
#endif
