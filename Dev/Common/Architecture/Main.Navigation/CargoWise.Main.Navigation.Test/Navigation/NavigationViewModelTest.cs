using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.ViewModels;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class NavigationViewModelTest : TestCase
{
	public void TestSearch()
	{
		AssertResults("em 1", 2, 3, new string[] { "Section 1", "Section 2" }, new string[] { "Item 11", "Item 12", "Item 13" });
		AssertResults("em 2", 2, 2, new string[] { "Section 2", "Section 3" }, new string[] { "Item 21", "Item 22" });
		AssertResults("em 21", 1, 1, new string[] { "Section 2" }, new string[] { "Item 21" });
		AssertResults("blah", 0, 0, Array.Empty<string>(), Array.Empty<string>());
		AssertResults(string.Empty, 0, 0, Array.Empty<string>(), Array.Empty<string>());
	}

	public void TestSearchSectionRemoveAmpersand()
	{
		var searchValue = "Category";
		var multilingualTextForTest = (NoResString)"Category && blah";
		var menuItem = new MenuSection("Category && blah", (NoResString)"Category && blah", multilingualTextForTest);
		var searchSection = new SearchResultSection(menuItem.MultilingualText, menuItem.DisplayName, menuItem.Items.Where(i => i.Name.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase) > -1), menuItem.Type);
		AssertEquals("SearchResults should have remove the double & ", "Category & blah", searchSection.DisplayName);
		var multilingualTextForTest2 = (NoResString)"Category & blah";
		var menuItem2 = new MenuSection("Category & blah", (NoResString)"Category & blah", multilingualTextForTest2);
		var searchSection2 = new SearchResultSection(menuItem2.MultilingualText, menuItem2.DisplayName, menuItem2.Items.Where(i => i.Name.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase) > -1), menuItem.Type);
		AssertEquals("SearchResults should have remove the double & ", "Category & blah", searchSection2.DisplayName);
	}

	public void TestSectionNameSearch()
	{
		AssertResults("Section", 2, 3, new string[] { "Section 2", "Section 3" }, new string[] { "Item 13", "Item 21", "Item 22" });
		AssertResults("Section 2", 1, 2, new string[] { "Section 2" }, new string[] { "Item 13", "Item 21" });
	}

	public void TestClearSearch()
	{
		viewModel.IsInSearchMode = true;
		viewModel.SearchViewModel.SearchValue = "blah";
		AssertEquals("Search value is set", "blah", viewModel.SearchViewModel.SearchValue);
		viewModel.ClearSearch.Execute(null);
		AssertEquals("Search value is cleared", string.Empty, viewModel.SearchViewModel.SearchValue);
		Assert("Is in search mode", viewModel.IsInSearchMode);
		viewModel.ClearSearch.Execute(null);
		Assert("Is not search mode", !viewModel.IsInSearchMode);
	}

	public void TestRunFirstSearchResult()
	{
		var hasBeenRun = false;
		var menuItem = new MenuItem("Item 55", (NoResString)"Item 55", () => hasBeenRun = true, null, null);
		var category = viewModel.Categories.FirstOrDefault(c => c.Name == "Category A");
		category.AddItem("Section 1", string.Empty, menuItem);
		viewModel.SearchViewModel.SearchValue = "Item 55";
		WaitForSearchToComplete();
		AssertEquals("Item 55 is found", "Item 55", viewModel.SearchViewModel.SearchResults.SelectMany(s => s.Items).FirstOrDefault().Name);
		Assert("Command has not been run", !hasBeenRun);
		viewModel.RunFirstSearchResult.Execute(null);
		Assert("Command has been run", hasBeenRun);
	}

	public void TestSearchModeAndClear()
	{
		Assert("Not in search mode", !viewModel.IsInSearchMode);
		viewModel.IsInSearchMode = true;
		Assert("In search mode", viewModel.IsInSearchMode);
		viewModel.SearchViewModel.SearchValue = "blah";
		viewModel.ForceClearSearch();
		Assert("Not in search mode", !viewModel.IsInSearchMode);
		AssertEquals("Search value cleared", string.Empty, viewModel.SearchViewModel.SearchValue);
	}

	public void TestSwitchingCategoryClearsSearch()
	{
		var categoryA = viewModel.Categories.First(c => c.Name == "Category A");
		var categoryB = viewModel.Categories.First(c => c.Name == "Category B");
		viewModel.SelectedCategory = categoryA;
		viewModel.IsInSearchMode = true;
		viewModel.SearchViewModel.SearchValue = "blah";
		Assert("In search mode", viewModel.IsInSearchMode);
		viewModel.SelectedCategory = categoryB;
		Assert("Not in search mode", !viewModel.IsInSearchMode);
		AssertEquals("Search value cleared", string.Empty, viewModel.SearchViewModel.SearchValue);
	}

	public void TestSearchFilterDefaultsToAllOff()
	{
		InitializeGlobalSearch();
		Assert(viewModel.SearchViewModel.Filters.Where(f => f.IsSelected).ToList().Count == 0);
	}

	public void TestSearchFiltersCorrectly()
	{
		InitializeGlobalSearch();

		viewModel.SearchViewModel.SearchResults.Clear();
		viewModel.SearchViewModel.SearchValue = "e";

		WaitForSearchToComplete();

		//All off == no filter
		var categoryCount = viewModel.SearchViewModel.SearchResults.Count;
		var totalItemCount = viewModel.SearchViewModel.SearchResults.SelectMany(c => c.Items).ToList().Count;

		Assert(categoryCount == 5);
		Assert(totalItemCount == 8);

		foreach (var filter in viewModel.SearchViewModel.Filters)
		{
			filter.IsSelected = true;
			TestSearchFilterCategory(filter.SectionTypes);
			filter.IsSelected = false;
		}
	}

	void TestSearchFilterCategory(SectionType[] filteredSections)
	{
		WaitForSearchToComplete();
		var results = viewModel.SearchViewModel.SearchResults;

		Assert(results.Count > 0);

		foreach (var result in results)
		{
			Assert(filteredSections.Contains(result.SectionType));
		}
	}

	public void TestMainViewModel()
	{
		var mainViewModel = viewModel.MainViewModel;
		Assert("MainViewModel is not null", mainViewModel != null);
		Assert("MainViewModel is Jump", mainViewModel.Name == ModuleTreeLoaderConstant.Category.Jump.Name);
	}

	public void TestMainMenuCategories()
	{
		var categories = viewModel.MainMenuCategories;
		AssertEquals("2 categories", 2, categories.Count);
		Assert("Category A is first", categories.First().Name == "Category A");
		Assert("Category B is second", categories.ElementAt(1).Name == "Category B");
	}

	void InitializeGlobalSearch()
	{
		viewModel.SearchViewModel.ResetSearchForUnitTesting();
		viewModel.InitializeSearch(
			(searchValue) => new SearchResultSection[] {
				new SearchResultSection(
					displayName: (NoResString)"Entity 1",
					name: "Entity 1",
					items: new MenuItem[] { new MenuItem("Entity Item 1", (NoResString)"Entity Item 1") },
					sectionType: SectionType.GlobalSearch) }
		);
	}

	public void TestGlobalSearchResultsOrder()
	{
		InitializeGlobalSearch();
		AssertResults("em 2", 3, 3, ["Entity 1", "Section 2", "Section 3"], ["Entity Item 1", "Item 21", "Item 22"]);
	}

	public void TestShouldShowNoResultsMessage_EmptySearchValue_ReturnsFalse()
	{
		viewModel.SearchViewModel.SearchValue = string.Empty;
		var shouldShowMessage = viewModel.SearchViewModel.ShouldShowNoResultsMessage;
		var shouldHideShortcuts = viewModel.SearchViewModel.ShouldHideShortcuts;
		AssertEquals("No results message should not show when SearchValue is empty.", false, shouldShowMessage);
		AssertEquals("Shortcuts should not show when there are no matching results.", true, shouldHideShortcuts);
	}

	public void TestShouldShowNoResultsMessage_WithNoSearchResults_ReturnsTrue()
	{
		viewModel.SearchViewModel.SearchValue = "blah";
		var shouldShowMessage = viewModel.SearchViewModel.ShouldShowNoResultsMessage;
		var shouldHideShortcuts = viewModel.SearchViewModel.ShouldHideShortcuts;
		AssertEquals("No results message should show when SearchValue is not empty.", true, shouldShowMessage);
		AssertEquals("Shortcuts should show when there are matching results.", false, shouldHideShortcuts);
	}

	public void TestShouldShowNoResultsMessage_WithSearchResults_ReturnsFalse()
	{
		var menuItem = new MenuItem("Item 55", (NoResString)"Item 55", null, null, null);
		var category = viewModel.Categories.FirstOrDefault(c => c.Name == "Category A");
		category.AddItem("Section 1", string.Empty, menuItem);
		viewModel.SearchViewModel.SearchValue = "Item 55";
		WaitForSearchToComplete();
		var shouldShowMessage = viewModel.SearchViewModel.ShouldShowNoResultsMessage;
		var shouldHideShortcuts = viewModel.SearchViewModel.ShouldHideShortcuts;

		AssertEquals("Item 55 is found", "Item 55", viewModel.SearchViewModel.SearchResults.SelectMany(s => s.Items).FirstOrDefault().Name);
		AssertEquals("No results message should not show when there are matching results.", false, shouldShowMessage);
		AssertEquals("Shortcuts should show when there are matching results.", true, shouldHideShortcuts);
	}

	public void TestSwitchingCategoryClearsPopupMenuSelection()
	{
		var categoryA = viewModel.Categories.First(c => c.Name == "Category A");
		var categoryB = viewModel.Categories.First(c => c.Name == "Category B");
		var popup1 = new PopupMenu { Title = "Popup1" };
		var popup2 = new PopupMenu { Title = "Popup2" };
		viewModel.PopupMenus.Add(popup1);
		viewModel.PopupMenus.Add(popup2);
		viewModel.SelectedPopupMenu = popup1;
		AssertNotNull("Selected Popup should not be null", viewModel.SelectedPopupMenu);
		viewModel.SelectedCategory = categoryA;
		AssertNull("Selected Popup should be null", viewModel.SelectedPopupMenu);
	}

	public void TestSelectingPopupMenuShouldClearCategorySelection()
	{
		var categoryA = viewModel.Categories.First(c => c.Name == "Category A");
		var categoryB = viewModel.Categories.First(c => c.Name == "Category B");
		var popup1 = new PopupMenu { Title = "Popup1" };
		var popup2 = new PopupMenu { Title = "Popup2" };
		viewModel.PopupMenus.Add(popup1);
		viewModel.PopupMenus.Add(popup2);
		viewModel.SelectedCategory = categoryA;
		AssertNotNull("Selected category should not be null", viewModel.SelectedCategory);
		viewModel.SelectedPopupMenu = popup1;

		AssertNull("Selected Category should be null", viewModel.SelectedCategory);
	}

	public void TestSelectingPopupClearsSearch()
	{
		var categoryA = viewModel.Categories.First(c => c.Name == "Category A");
		var categoryB = viewModel.Categories.First(c => c.Name == "Category B");
		var popup1 = new PopupMenu { Title = "Popup1" };
		var popup2 = new PopupMenu { Title = "Popup2" };
		viewModel.PopupMenus.Add(popup1);
		viewModel.PopupMenus.Add(popup2);
		viewModel.SelectedPopupMenu = popup1;
		viewModel.IsInSearchMode = true;
		viewModel.SearchViewModel.SearchValue = "blah";
		Assert("In search mode", viewModel.IsInSearchMode);
		AssertNull("Selected popup cleared", viewModel.SelectedPopupMenu);
		viewModel.SelectedPopupMenu = popup2;
		Assert("Not in search mode", !viewModel.IsInSearchMode);
		AssertEquals("Search value cleared", string.Empty, viewModel.SearchViewModel.SearchValue);
	}

	public void TestHideAllMenusUnselectsPopup()
	{
		var categoryA = viewModel.Categories.First(c => c.Name == "Category A");
		var categoryB = viewModel.Categories.First(c => c.Name == "Category B");
		var popup1 = new PopupMenu { Title = "Popup1" };
		var popup2 = new PopupMenu { Title = "Popup2" };
		viewModel.PopupMenus.Add(popup1);
		viewModel.PopupMenus.Add(popup2);
		viewModel.SelectedPopupMenu = popup1;
		AssertNotNull("Selected Popup should not be null", viewModel.SelectedPopupMenu);
		Assert("Selected popup is selected", viewModel.SelectedPopupMenu.IsSelected);
		viewModel.HideAllMenus();
		AssertNull("Selected popup cleared", viewModel.SelectedPopupMenu);
		Assert("Selected popup is not selected", !popup1.IsSelected);
		Assert("Not in search mode", !viewModel.IsInSearchMode);
		AssertEquals("Search value cleared", string.Empty, viewModel.SearchViewModel.SearchValue);
	}

	public void TestHideAllMenusUnselectsMainMenu()
	{
		var categoryA = viewModel.Categories.First(c => c.Name == "Category A");
		var categoryB = viewModel.Categories.First(c => c.Name == "Category B");
		var popup1 = new PopupMenu { Title = "Popup1" };
		var popup2 = new PopupMenu { Title = "Popup2" };
		viewModel.PopupMenus.Add(popup1);
		viewModel.PopupMenus.Add(popup2);
		viewModel.SelectedCategory = categoryA;
		AssertNotNull("Selected Category should not be null", viewModel.SelectedCategory);
		viewModel.HideAllMenus();
		AssertNull("Selected category is cleared", viewModel.SelectedCategory);
		Assert("Not in search mode", !viewModel.IsInSearchMode);
		AssertEquals("Search value cleared", string.Empty, viewModel.SearchViewModel.SearchValue);
	}

	public void TestSearchResultCanMatchEnglishTextWhenMultilingualStringIsResourceString()
	{
		var navigationViewModel = new NavigationViewModel();
		var section1Multilinge = ResString.GetMultilingualString("d3fef96d-c7f8-45bf-b23f-65d383b2c552", "Section 1");
		var categoryA = new NavigationMenuViewModel(ResString.GetMultilingualString("7c5d49fa-53e6-4f44-a319-eb18baedd059", "Category A"), "Category A", 1, true);
		categoryA.AddSection("Section 1", "Section Display", section1Multilinge, SectionType.Module, 10, "SubCatgoryName", "SubCatgory", "C", section1Multilinge);
		categoryA.AddItem("Section 1", "SubCatgoryName", new MenuItem(ResString.GetMultilingualString("42918706-1dd2-4088-9e17-3aa4893acf60", "Item1")));
		categoryA.AddItem("Section 1", "SubCatgoryName", new MenuItem(ResString.GetMultilingualString("31dafc3c-a940-4e84-b395-757e6d4bc215", "Item2")));
		navigationViewModel.AddCategory(categoryA);
		navigationViewModel.InitializeSearch(null);
		AsyncHelper.RunSync(() => navigationViewModel.SearchViewModel.SearchAsync("Section 1"));
		AssertEquals("Item1", navigationViewModel.SearchViewModel.SearchResults.SelectMany(s => s.Items).FirstOrDefault().Name);
	}

	public void TestSearchResultWouldNotMatchWhenMultilingualStringIsNotResourceString()
	{
		var navigationViewModel = new NavigationViewModel();
		var categoryA = new NavigationMenuViewModel(ResString.GetMultilingualString("7c5d49fa-53e6-4f44-a319-eb18baedd059", "Category A"), "Category A", 1, true);
		categoryA.AddSection("Section 1", "Section Display", (NoResString)"Section 1", SectionType.Module, 10, "SubCatgoryName", "SubCatgory", "C", (NoResString)"Section 1");
		categoryA.AddItem("Section 1", "SubCatgoryName", new MenuItem((NoResString)"Item1"));
		categoryA.AddItem("Section 1", "SubCatgoryName", new MenuItem((NoResString)"Item2"));
		navigationViewModel.AddCategory(categoryA);
		navigationViewModel.InitializeSearch(null);
		AsyncHelper.RunSync(() => navigationViewModel.SearchViewModel.SearchAsync("Section 1"));
		AssertEquals(0, navigationViewModel.SearchViewModel.SearchResults.SelectMany(s => s.Items).Count());
	}

	public void TestSearchSearchesID()
	{
		var viewModel = new NavigationViewModel();
		var categoryA = new NavigationMenuViewModel((NoResString)"Category A", "Category A", 1, true);
		categoryA.AddSection("Section 1", "Section 1", (NoResString)"Section 1", SectionType.Favorite, 10, string.Empty, string.Empty, string.Empty, null);

		var workItem1Number = "WI0011";
		var workItem1Name = "Item 11";
		var workItem2Number = "WI0012";
		var workItem2Name = "Item 12";

		var favoriteItem1 = new MenuItem((NoResString)$"{workItem1Number} - {workItem1Name}");
		favoriteItem1.SetIDWithName();
		var favoriteItem2 = new MenuItem((NoResString)$"{workItem2Number} - {workItem2Name}");
		favoriteItem2.SetIDWithName();

		AssertEquals(workItem1Number, favoriteItem1.ID);
		AssertEquals(workItem1Name, favoriteItem1.Name);
		AssertEquals(workItem2Number, favoriteItem2.ID);
		AssertEquals(workItem2Name, favoriteItem2.Name);

		categoryA.AddItem("Section 1", string.Empty, favoriteItem1);
		categoryA.AddItem("Section 1", string.Empty, favoriteItem2);
		
		viewModel.AddCategory(categoryA);
		viewModel.InitializeSearch(null);
		AsyncHelper.RunSync(() => viewModel.SearchViewModel.SearchAsync(workItem1Number));

		var results = viewModel.SearchViewModel.SearchResults;
		AssertEquals(1, results.Count);
		AssertEquals(1, results[0].Items.Count);
		AssertEquals(workItem1Number, results[0].Items[0].ID);

		AsyncHelper.RunSync(() => viewModel.SearchViewModel.SearchAsync(workItem2Number));

		results = viewModel.SearchViewModel.SearchResults;
		AssertEquals(1, results.Count);
		AssertEquals(1, results[0].Items.Count);
		AssertEquals(workItem2Number, results[0].Items[0].ID);
	}

	internal static class AsyncHelper
	{
		static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None,
		   TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

		public static void RunSync(Func<Task> func)
		{
			var cultureUi = CultureInfo.CurrentUICulture;
			var culture = CultureInfo.CurrentCulture;
			_myTaskFactory.StartNew(() =>
			{
				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = cultureUi;
				return func();
			}).Unwrap().GetAwaiter().GetResult();
		}
	}

	#region Implementation
	void AssertResults(string searchValue, int sections, int items, string[] sectionNames, string[] itemNames)
	{
		viewModel.SearchViewModel.SearchResults.Clear();
		viewModel.SearchViewModel.SearchValue = searchValue;
		WaitForSearchToComplete();
		var results = viewModel.SearchViewModel.SearchResults;
		AssertEquals(string.Format("{0} section(s)", sections), sections, results.Count);
		AssertEquals(string.Format("{0} item(s)", items), items, results.SelectMany(s => s.Items).Count());
		AssertArrayEqualsByElements("Sections are the same", sectionNames, results.Where(s => s.Items.Any()).Select(s => s.Name).OrderBy(x => x).ToArray());
		AssertArrayEqualsByElements("Items are the same", itemNames, results.SelectMany(s => s.Items).Select(i => i.Name).OrderBy(x => x).ToArray());
	}

	void WaitForSearchToComplete()
	{
		viewModel.SearchViewModel.WaitingForSearchToComplete();
	}

	NavigationViewModel viewModel;
	#endregion
	#region Setup
	protected override void SetUp()
	{
		ViewModelWithNotification.IsInUnitTesting = true;

		base.SetUp();
		viewModel = new NavigationViewModel();
		var categoryA = new NavigationMenuViewModel((NoResString)"Category A", "Category A", 1, true);
		categoryA.AddSection("Section 1", "Section 1", (NoResString)"Section 1", SectionType.Favorite, 10, string.Empty, string.Empty, string.Empty, null);
		categoryA.AddItem("Section 1", string.Empty, new MenuItem((NoResString)"Item 11"));
		categoryA.AddItem("Section 1", string.Empty, new MenuItem((NoResString)"Item 12"));
		viewModel.AddCategory(categoryA);
		var categoryB = new NavigationMenuViewModel((NoResString)"Category B", "Category B", 3, false);
		categoryA.AddSection("Section 2", "Section 2", (NoResString)"Section 2", SectionType.Module, -1, "Subcategory 1", "Subcategory 1", "B", (NoResString)"Subcategory 1");
		categoryA.AddItem("Section 2", "Subcategory 1", new MenuItem((NoResString)"Item 13"));
		categoryA.AddItem("Section 2", "Subcategory 1", new MenuItem((NoResString)"Item 21"));
		categoryA.AddSection("Section 3", "Section 3", (NoResString)"Section 3", SectionType.Module, -1, "Subcategory 1", "Subcategory 1", "B", (NoResString)"Subcategory 1");
		categoryA.AddItem("Section 3", "Subcategory 1", new MenuItem((NoResString)"Item 22"));
		viewModel.AddCategory(categoryB);
		var categoryC = new NavigationMenuViewModel((NoResString)ModuleTreeLoaderConstant.Category.Jump.Name, ModuleTreeLoaderConstant.Category.Jump.Name, 1, true);
		categoryC.AddSection("Section 4", "Section 4", (NoResString)"Section 4", SectionType.RecentModule, 10, string.Empty, string.Empty, string.Empty, null);
		categoryC.AddItem("Section 4", string.Empty, new MenuItem((NoResString)"Item 41"));
		categoryC.AddItem("Section 4", string.Empty, new MenuItem((NoResString)"Item 42"));
		viewModel.AddCategory(categoryC);
		viewModel.InitializeSearch(null);
	}

	protected override void TearDown()
	{
		base.TearDown();
		WaitForSearchToComplete();
		viewModel.SearchViewModel.ResetSearchForUnitTesting();
	}
	#endregion
}
