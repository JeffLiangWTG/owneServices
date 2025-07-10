using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace CargoWise.Main.Navigation.ViewModels;

#nullable disable
public class NavigationViewModel : INotifyPropertyChanged
{
	public NavigationViewModel()
	{
		Categories = new ObservableCollection<NavigationMenuViewModel>();
		ClearSearch = new ClickCommand(ClearSearchCommand);
		RunFirstSearchResult = new ClickCommand(RunFirstSearchResultCommand);
		SearchHint = NavigationMultilingualResources.SearchHint;
		SearchHeaderHint = NavigationMultilingualResources.SearchHeaderHint;
		SearchPlaceHolderText = NavigationMultilingualResources.SearchPlaceHolderText;
		ShortcutCtrl = NavigationMultilingualResources.ShortcutCtrl;
		SearchShortcutKey = NavigationMultilingualResources.SearchShortcutKey;
		NavigateHint = NavigationMultilingualResources.NavigateHint;
		OpenHint = NavigationMultilingualResources.OpenHint;
		CloseHint = NavigationMultilingualResources.CloseHint;
		FooterBranchText = NavigationMultilingualResources.FooterBranchText;
		FooterCompanyText = NavigationMultilingualResources.FooterCompanyText;
		FooterDepartmentText = NavigationMultilingualResources.FooterDepartmentText;
		NoResultsText = NavigationMultilingualResources.NoResultsText;
		TryAnotherTermText = NavigationMultilingualResources.TryAnotherTermText;
		PopupMenus = [];
		SearchViewModel = new SearchViewModel();
		IsRightContentVisible = true;
	}

	public void AddCategory(NavigationMenuViewModel category)
	{
		Argument.NotNull(this.Categories, nameof(this.Categories)); // Suggested By ReviewBot
		Argument.NotNull(category, nameof(category)); // Suggested By ReviewBot
		if (category.Name == ModuleTreeLoaderConstant.Category.Jump.Name)
		{
			MainViewModel = category;
		}
		Categories.Add(category);
		category.ShortcutIndex = Categories.Count;
	}

	public SearchViewModel SearchViewModel { get; }

	NavigationMenuViewModel mainViewModel;
	public NavigationMenuViewModel MainViewModel
	{
		get => mainViewModel;
		private set
		{
			mainViewModel = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainViewModel)));
		}
	}

	IMyTasksViewModel myTasksViewModel;
	public IMyTasksViewModel MyTasksViewModel
	{
		get => myTasksViewModel;
		set
		{
			if (myTasksViewModel != value)
			{
				myTasksViewModel = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MyTasksViewModel)));
			}
		}
	}
	public RecentMessagesViewModel RecentMessagesViewModel { get; init; }
	public PublicHolidaysViewModel PublicHolidaysViewModel { get; init; }
	public SnapshotsViewModel SnapshotsViewModel { get; init; }

	public ObservableCollection<NavigationMenuViewModel> Categories { get; set; }
	public ObservableCollection<NavigationMenuViewModel> MainMenuCategories => [.. Categories.Where(c => c.Name != ModuleTreeLoaderConstant.Category.Jump.Name)];
	public NavigationMenuViewModel SelectedCategory
	{
		get { return selectedCategory; }
		set
		{
			if (selectedCategory != value)
			{
				selectedCategory = value;
				if (selectedCategory != null)
				{
					ForceClearSearch();
					SelectedPopupMenu = null;
				}
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedCategory)));
				}
			}
		}
	}
	NavigationMenuViewModel selectedCategory;

	public event PropertyChangedEventHandler PropertyChanged;

	#region Popup Menus
	public string OptionMenuTitle => ResString.GetMultilingualString("MenuItem.Main.UserOptions", "&Options");
	public string HelpMenuTitle => ResString.GetMultilingualString("MenuItem.Main.Help", "&Help");
	public string TestingMenuTitle => ResString.GetMultilingualString("MenuItem.Main.Testing", "Te&sting");
	public ObservableCollection<PopupMenu> PopupMenus { get; private set; }

	PopupMenu selectedPopupMenu;
	public PopupMenu SelectedPopupMenu
	{
		get => selectedPopupMenu;
		set
		{
			if (selectedPopupMenu != value)
			{
				if (selectedPopupMenu != null)
				{
					selectedPopupMenu.IsSelected = false;
				}
				selectedPopupMenu = value;
				if (selectedPopupMenu != null)
				{
					selectedPopupMenu.IsSelected = true;
					SelectedCategory = null;
					ForceClearSearch();
				}

				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedPopupMenu)));
				}
			}
		}
	}

	#endregion Popup Menus

	public void HideAllMenus()
	{
		SelectedCategory = null;
		SelectedPopupMenu = null;
	}
	public void Dispose()
	{
		Argument.NotNull(this.Categories, nameof(this.Categories)); // Suggested By ReviewBot
		foreach (var category in Categories)
		{
			if (category != null)
			{
				foreach (var button in category.Buttons)
				{
					var menuSection = button as MenuSection;
					if (menuSection != null)
					{
						menuSection.Dispose();
					}
				}
			}
		}

		if (ClearSearch != null)
		{
			ClearSearch.Clear();
		}

		if (RunFirstSearchResult != null)
		{
			RunFirstSearchResult.Clear();
		}
	}

	#region Search
	public void InitializeSearch(Func<string, IEnumerable<SearchResultSection>> globalSearch)
	{
		GlobalSearch = globalSearch;

		SearchViewModel.AddSearchSource(new SearchSource
		{
			UniqueId = "IndexSearch",
			ResultOrder = 1,
			Filters = new ObservableCollection<SearchFilter> {
				new SearchFilter { SectionTypes = [SectionType.RecentItem, SectionType.RecentModule], Name = ResString.GetMultilingualString("2eeb8597-abdc-47ab-9140-5d1f007c7141", "Recent") },
				new SearchFilter { SectionTypes = [SectionType.Favorite], Name = ResString.GetMultilingualString("9b7844ab-e723-4dfa-afc9-2d46448a2237", "Favorites") },
				new SearchFilter { SectionTypes = [SectionType.Module], Name = ResString.GetMultilingualString("416efeac-51dd-41a0-ba90-704b14fd01e8", "Modules") }
			},
			SearchAsync = (parameters) =>
			{
				parameters.CancellationToken.ThrowIfCancellationRequested();

				if (Categories == null)
				{
					return Task.FromResult(new SearchSource.SearchResults(Enumerable.Empty<SearchResultSection>()));
				}

				var sections = Categories.SelectMany(c => c.Buttons).OfType<MenuSection>();
				var subsections = sections.SelectMany(m => m.Subsections).OfType<MenuSection>();

				var sectionResults = sections
					.AsParallel()
					.WithCancellation(parameters.CancellationToken)
					.Select(m =>
						new SearchResultSection(
							displayName: m.DisplayName,
							name: m.DisplayName,
							items: m.Items.Where(i =>
								i.Name.IndexOf(parameters.SearchValue, StringComparison.OrdinalIgnoreCase) > -1 ||
								i.ID?.IndexOf(parameters.SearchValue, StringComparison.OrdinalIgnoreCase) > -1 ||
								i.MultilingualText?.ToString().IndexOf(parameters.SearchValue, StringComparison.OrdinalIgnoreCase) > -1 ||
								i.MultilingualText is ResourceString resourceString && resourceString.EnglishText.IndexOf(parameters.SearchValue, StringComparison.OrdinalIgnoreCase) > -1),
							sectionType: m.Type));

				var subsectionResults = subsections
					.AsParallel()
					.Select(m =>
						new SearchResultSection(
							displayName: m.DisplayName,
							name: m.DisplayName,
							items: m.Items.Where(i =>
								i.Name.IndexOf(parameters.SearchValue, StringComparison.OrdinalIgnoreCase) > -1 ||
								m.DisplayName.IndexOf(parameters.SearchValue, StringComparison.OrdinalIgnoreCase) > -1 ||
								((i.MultilingualText is ResourceString resourceString) && resourceString.EnglishText.IndexOf(parameters.SearchValue, StringComparison.OrdinalIgnoreCase) > -1) ||
								((m.MultilingualText is ResourceString resourceString1) && resourceString1.EnglishText.IndexOf(parameters.SearchValue, StringComparison.OrdinalIgnoreCase) > -1)),
							sectionType: m.Type));

				var filteredSections = new HashSet<SectionType>(parameters.SelectedFilters.SelectMany(f => f.SectionTypes));

				var results = sectionResults
					.Union(subsectionResults)
					.Where(m => m.Items.Any() && filteredSections.Contains(m.SectionType))
					.OrderBy(m => m.DisplayName)
					.ToList();

				parameters.CancellationToken.ThrowIfCancellationRequested();
				return Task.FromResult(new SearchSource.SearchResults(results));
			},
		});

		if (globalSearch != null)
		{
			SearchViewModel.AddSearchSource(new SearchSource
			{
				UniqueId = "GlobalSearch",
				ResultOrder = 2, //Lower than 1 to show after the index search
				Filters = new ObservableCollection<SearchFilter> { new SearchFilter { SectionTypes = [SectionType.GlobalSearch], Name = ResString.GetMultilingualString("a1c4be08-65ce-4f63-9029-ac0a121165ae", "Global") } },
				SearchAsync = (parameters) =>
				{
					var results = GetGlobalSearchResults(parameters.SearchValue)
						.Where(m => m.Items.Any());

					parameters.CancellationToken.ThrowIfCancellationRequested();
					return Task.FromResult(new SearchSource.SearchResults(results));
				},
			});
		}
	}
	public Func<string, IEnumerable<SearchResultSection>> GlobalSearch { get; private set; }

	IEnumerable<SearchResultSection> GetGlobalSearchResults(string searchValue)
	{
		if (string.IsNullOrEmpty(searchValue) || GlobalSearch == null)
		{
			return Enumerable.Empty<SearchResultSection>();
		}

		return GlobalSearch.Invoke(searchValue);
	}

	void ClearSearchCommand()
	{
		if (!string.IsNullOrEmpty(SearchViewModel.SearchValue))
		{
			SearchViewModel.SearchValue = string.Empty;
		}
		else
		{
			IsInSearchMode = false;
		}
	}

	public void ForceClearSearch()
	{
		SearchViewModel.SearchValue = string.Empty;
		IsInSearchMode = false;
	}

	void RunFirstSearchResultCommand()
	{
		var item = SearchViewModel.SearchResults
			.FirstOrDefault(s => s.Items.Count > 0)?
			.Items.FirstOrDefault();
		if (item != null)
		{
			item.LinkAction?.Execute(null);
		}
	}

	public ClickCommand ClearSearch { get; private set; }
	public ClickCommand RunFirstSearchResult { get; private set; }

	public bool IsInSearchMode
	{
		get { return isInSearchMode; }
		set
		{
			isInSearchMode = value;
			if (isInSearchMode)
			{
				SelectedCategory = null;
				SelectedPopupMenu = null;
				IsRecentItemsSelected = false;
			}
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsInSearchMode)));
			}
		}
	}
	bool isInSearchMode;

	public bool IsRecentItemsSelected
	{
		get { return isRecentItemsSelected; }
		set
		{
			isRecentItemsSelected = value;
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsRecentItemsSelected)));
			}
		}
	}
	bool isRecentItemsSelected;

	bool isRightContentVisible;
	public bool IsRightContentVisible
	{
		get { return isRightContentVisible; }
		set
		{
			isRightContentVisible = value;
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsRightContentVisible)));
			}
			ToggleRightContentVisibilityButtonTooltip = isRightContentVisible
				? NavigationMultilingualResources.HideRightContentToolTip
				: NavigationMultilingualResources.ShowRightContentToolTip;
		}
	}

	string toggleRightContentVisibilityButtonTooltip = NavigationMultilingualResources.ShowRightContentToolTip;
	public string ToggleRightContentVisibilityButtonTooltip
	{
		get { return toggleRightContentVisibilityButtonTooltip; }
		set
		{
			toggleRightContentVisibilityButtonTooltip = value;
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(nameof(ToggleRightContentVisibilityButtonTooltip)));
			}
		}
	}

	public MultilingualString SearchHint { get; private set; }
	public MultilingualString SearchHeaderHint { get; private set; }
	public MultilingualString SearchPlaceHolderText { get; private set; }
	public MultilingualString ShortcutCtrl { get; private set; }
	public MultilingualString SearchShortcutKey { get; private set; }
	public MultilingualString NavigateHint { get; private set; }
	public MultilingualString OpenHint { get; private set; }
	public MultilingualString CloseHint { get; private set; }
	public MultilingualString NoResultsText { get; private set; }
	public MultilingualString TryAnotherTermText { get; private set; }

	#endregion

	#region Footer
	public MultilingualString FooterBranchText { get; private set; }
	public MultilingualString FooterCompanyText { get; private set; }
	public MultilingualString FooterDepartmentText { get; private set; }
	#endregion
}
