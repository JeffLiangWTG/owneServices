using System;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.ViewModels;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class NavigationMenuViewModelTest : TestCase
{
	public void TestShowSearchIsSet()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 0, true);
		Assert("ShowSearch is set", viewModel.ShowSearch);
	}

	public void TestMenuSectionsIsNotNull()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 0, true);
		Assert("MenuSections is not null", viewModel.MenuSections != null);
	}

	public void TestAddSection()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("name", "displayName", (NoResString)"Test", SectionType.Subcategory, 0, "subcategoryName", "subcategoryDisplayText", "subcategoryLetter", (NoResString)"Test");
		AssertEquals("DetailRows is set", 1, viewModel.DetailRows);
	}

	public void TestMenuSectionsContainsOnlyMenuSections()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("B name", "B displayName", (NoResString)"TestB", SectionType.RecentModule, 20, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");

		foreach (var section in viewModel.MenuSections)
		{
			Assert("Section is of type MenuSection", section is MenuSection);
			Assert("Section is not of type MenuDetails", !(section is MenuDetails));
		}
	}

	public void TestMenuSelectedItemDefaultsToFirstSection()
	{
		var viewModel = new NavigationMenuViewModel(ResString.GetMultilingualString("Test.Test", "Test"), "test", 1, true);
		viewModel.AddSection("A name", "A displayName", ResString.GetMultilingualString("Test.TestA", "TestA"), SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", ResString.GetMultilingualString("Test.TestAA", "TestAA"));
		viewModel.AddSection("B name", "B displayName", ResString.GetMultilingualString("Test.TestB", "TestB"), SectionType.RecentModule, 20, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", ResString.GetMultilingualString("Test.TestAA", "TestAA"));
		viewModel.AddSection("C name", "C displayName", ResString.GetMultilingualString("Test.TestC", "TestC"), SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", ResString.GetMultilingualString("Test.TestAA", "TestAA"));

		var selectedItem = viewModel.SelectedItem;
		Assert("SelectedItem is of type MenuSection", selectedItem is MenuSection);
		AssertEquals("SelectedItem is the first section", "subcategoryNameA", selectedItem.Name);
	}

	public void TestMenuDetailsContainsOnlyMenuDetails()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("B name", "B displayName", (NoResString)"TestB", SectionType.RecentModule, 20, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");

		foreach (var section in viewModel.MenuDetails)
		{
			Assert("Section is of type MenuDetails or Subcategory", section is MenuDetails || ((section is MenuSection s) && s.Type == SectionType.Subcategory));
		}
	}

	public void TestRecentModules()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("B name", "B displayName", (NoResString)"TestB", SectionType.RecentModule, 20, null, null, null, (NoResString)"Test");
		viewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");

		AssertEquals("RecentModules is set", 1, viewModel.RecentModules.Count);
	}

	public void TestGetFavoriteMenuItems_WhenFavoritesSectionPresent_ReturnsValidFavorites()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		var favoriteSection = new MenuSection("Favorite Section", "Favorite Section", (NoResString)"Favorite Section", SectionType.Favorite, 2);
		var favoriteItem1 = new MenuItem("Item 1", (NoResString)"WI006859 -Favorite - Item 1", (NoResString)"Test Module", null, null, null, _ => true);
		var favoriteItem2 = new MenuItem("Item 2", (NoResString)"Favorite Item 2", (NoResString)"Test Module", null, null, null, _ => true);
		favoriteSection.AddItem(favoriteItem1);
		favoriteSection.AddItem(favoriteItem2);
		viewModel.Buttons.Add(favoriteSection);

		var actualFavoriteMenuItems = viewModel.GetFavoriteMenuItems();

		AssertEquals("Favorites is set", 1, actualFavoriteMenuItems.Count);
		var actualFavoritesSection = (MenuSection)actualFavoriteMenuItems.First();
		AssertEquals("Favorites Items is set", 2, actualFavoritesSection.Items.Count);
	}

	public void TestGetFavoriteMenuItems_WhenFavoritesSectionAbsent_ReturnsEmptyFavorites()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("B name", "B displayName", (NoResString)"TestB", SectionType.RecentModule, 20, null, null, null, (NoResString)"Test");
		viewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");

		var actualFavoriteMenuItems = viewModel.GetFavoriteMenuItems();
		AssertEquals("Favorites is absent", 0, actualFavoriteMenuItems.Count);
	}

	public void TestRecentItems()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("B name", "B displayName", (NoResString)"TestB", SectionType.RecentItem, 20, null, null, null, (NoResString)"Test");
		viewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");

		AssertEquals("Recent Item is set", 1, viewModel.RecentItems.Count);
	}

	public void TestNoRecentItems()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("B name", "B displayName", (NoResString)"TestB", SectionType.RecentModule, 20, null, null, null, (NoResString)"Test");
		viewModel.AddSection("C name", "C displayName", (NoResString)"TestC", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");

		AssertEquals("Recent Item is set", 0, viewModel.RecentItems.Count);
	}

	public void TestNoRecentMenuItems()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		AssertEquals("RecentMenuItem is empty", 0, viewModel.RecentMenuItems.Count);
	}

	public void TestRecentMenuItems()
	{
		var viewModel = new NavigationMenuViewModel((NoResString)"Test", "test", 1, true);
		viewModel.AddSection("A name", "A displayName", (NoResString)"TestA", SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", (NoResString)"TestAA");
		viewModel.AddSection("B name", "B displayName", (NoResString)"TestB", SectionType.RecentItem, 20, null, null, null, (NoResString)"Test");
		viewModel.AddItem("B name", null, new MenuItem(Guid.NewGuid().ToString(), (NoResString)"Test"));

		AssertEquals("RecentMenuItem has one item", 1, viewModel.RecentMenuItems.Count);
	}

	public void TestUpdateItemWithSameKey()
	{
		var viewModel = new NavigationMenuViewModeUpdateItemWithSameKey(ResString.GetMultilingualString("Test.Test", "Test"), "test", 1, true);
		viewModel.AddSection("A name", "A displayName", ResString.GetMultilingualString("Test.TestA", "TestA"), SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", ResString.GetMultilingualString("Test.TestAA", "TestAA"));
		viewModel.AddSection("C name", "C displayName", ResString.GetMultilingualString("Test.TestC", "TestC"), SectionType.Subcategory, 5, "subcategoryNameA", "subcategoryDisplayTextA", "subcategoryLetterA", ResString.GetMultilingualString("Test.TestAA", "TestAA"));
		var favoriteSection = new MenuSection("Favorite Section", "Favorite Section", ResString.GetMultilingualString("Test.Favorite", "Favorite Section"), SectionType.Favorite, 2);
		var favoriteItem1 = new MenuItem("Item 1", ResString.GetMultilingualString("Test.FavoriteItem1", "WI006859 -Favorite - Item 1"), ResString.GetMultilingualString("Test.Module", "Test Module"), null, null, null, _ => true);
		var favoriteItem2 = new MenuItem("Item 2", ResString.GetMultilingualString("Test.FavoriteItem2", "Favorite Item 2"), ResString.GetMultilingualString("Test.Module", "Test Module"), null, null, null, _ => true);
		favoriteSection.AddItem(favoriteItem1);
		favoriteSection.AddItem(favoriteItem2);
		viewModel.Buttons.Add(favoriteSection);

		viewModel.AddCollectionChangeEvent();
		var favoriteItem3 = new MenuItem("Item 2", ResString.GetMultilingualString("Test.FavoriteItem3", "Favorite Item 3"), ResString.GetMultilingualString("Test.Module", "Test Module"), null, null, null, _ => true);

		AssertNoExceptionThrown("UpdateItemWithSameKey should not throw exception", () => viewModel.UpdateItemWithSameKey("Favorite Section", string.Empty, favoriteItem3));
		AssertEquals("UpdateItemWithSameKey Should only trigger CollectionChanged once", 1, viewModel.CollectionChangeCount);
	}

	internal class NavigationMenuViewModeUpdateItemWithSameKey : NavigationMenuViewModel
	{
		public NavigationMenuViewModeUpdateItemWithSameKey(MultilingualString multilingualText, string name, int columns, bool showSearch) : base(multilingualText, name, columns, showSearch)
		{
		}

		public void AddCollectionChangeEvent()
		{
			var favorites = Buttons.Where(b => b is MenuSection s && s.Type == SectionType.Favorite).ToList();
			if (favorites.Count == 0)
			{
				return;
			}
			var favoritesSection = (MenuSection)favorites.FirstOrDefault();
			var items = new ObservableCollection<MenuItem>(favoritesSection!.Items);
			favoritesSection.Items.Clear();
			foreach (var item in items)
			{
				item.SetIDWithName();
				favoritesSection.AddItem(item);
			}
			favoritesSection.Items.CollectionChanged += FavoriteItems_CollectionChanged1;
		}

		public int CollectionChangeCount;
		public void FavoriteItems_CollectionChanged1(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var collection = sender as ObservableCollection<MenuItem>;
			var distinctCount = collection.Select(t => t.Key).Distinct().Count();
			if (distinctCount != collection.Count)
			{
				throw new InvalidOperationException("Collection has duplicate items");
			}

			CollectionChangeCount++;
		}
	}
}
