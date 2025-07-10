using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Navigation.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class MenuSectionTest : TestCase
{
	readonly MultilingualString multilingualTextForTest = (NoResString)"Section 1";
	public void TestDropMenuItem()
	{
		var section = new MenuSection("Section 1", "Section 1", multilingualTextForTest);
		var menuItem1 = new MenuItem("Item 1", (NoResString)"Item 1", (NoResString)"Module 1", null, null, null, _ => true);
		var menuItem2 = new MenuItem("Item 2", (NoResString)"Item 2", (NoResString)"Module 2", null, null, null, _ => true);
		var menuItem3 = new MenuItem("Item 3", (NoResString)"Item 3", (NoResString)"Module 3", null, null, null, _ => true);
		section.AddItem(menuItem1);
		section.AddItem(menuItem2);
		section.AddItem(menuItem3);
		AssertOrder("Initial Order", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem1, -1, true);
		AssertOrder("Index too low", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem1, 3, false);
		AssertOrder("Index too high", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem1, 0, true);
		AssertOrder("Same index, above item", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem1, 0, false);
		AssertOrder("Same index, below item", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem1, 1, true);
		AssertOrder("Next index, above item", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem1, 1, false);
		AssertOrder("Next index, below item", section, menuItem2, menuItem1, menuItem3);
		section.Drop(menuItem2, 2, true);
		AssertOrder("Bottom of list, above item", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem1, 2, false);
		AssertOrder("Bottom of list, below item", section, menuItem2, menuItem3, menuItem1);
	}

	public void TestFavoriteSwitch()
	{
		var favoriteSection = new MenuSection("Favorite Section", "Favorite Section", (NoResString)"Favorite Section", SectionType.Favorite, 2);
		AssertEquals("Switch view", favoriteSection.SwitchViewText);
		AssertNotNull(favoriteSection.SwitchViewAction);
		AssertEquals(favoriteSection.IsCompressedView, false);
		favoriteSection.SwitchViewAction.Execute(null);
		AssertEquals(favoriteSection.IsCompressedView, true);
	}

	public void TestUnableToDropMenuItem()
	{
		var section = new MenuSection("Section 1", "Section 1", multilingualTextForTest);
		var menuItem1 = new MenuItem("Item 1", (NoResString)"Item 1", (NoResString)"Module 1", null, null, null, _ => false);
		var menuItem2 = new MenuItem("Item 2", (NoResString)"Item 2", (NoResString)"Module 2", null, null, null, _ => false);
		var menuItem3 = new MenuItem("Item 3", (NoResString)"Item 3", (NoResString)"Module 3", null, null, null, _ => false);
		section.AddItem(menuItem1);
		section.AddItem(menuItem2);
		section.AddItem(menuItem3);
		section.Drop(menuItem1, 3, false);
		AssertOrder("Item not moved by MoveAction", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem2, 0, true);
		AssertOrder("Item not moved by MoveAction", section, menuItem1, menuItem2, menuItem3);
		section.Drop(menuItem3, 0, true);
		AssertOrder("Item not moved by MoveAction", section, menuItem1, menuItem2, menuItem3);
	}

	public void TestDropDataType()
	{
		var section = new MenuSection("Section 1", "Section 1", multilingualTextForTest);
		AssertEquals(typeof(MenuItem), section.DataType);
	}

	public void TestDataForRemoveAllLinks()
	{
		var favoriteSection = new MenuSection("Favorite Section", "Favorite Section", (NoResString)"Favorite Section", SectionType.Favorite, 2);
		var favoriteItem1 = new MenuItem("Item 1", (NoResString)"Favorite Item 1", null, null, null, null, _ => true);
		var favoriteItem2 = new MenuItem("Item 2", (NoResString)"Favorite Item 2", null, null, null, null, _ => true);
		var favoriteItem3 = new MenuItem("Item 3", (NoResString)"Favorite Item 3", null, null, null, null, _ => true);
		AssertEquals("Remove All Links", favoriteSection.RemoveAllLinksText);
		AssertNotNull(favoriteSection.RemoveAllLinksAction);
		favoriteSection.AddItem(favoriteItem1);
		favoriteSection.AddItem(favoriteItem2);
		favoriteSection.AddItem(favoriteItem3);
		AssertEquals("The first one will be removed, only remain 2 in collection", 2, favoriteSection.Items.Count);
		favoriteSection.RemoveAllLinksAction.Execute(null);
		AssertEquals("All items should be removed", 0, favoriteSection.Items.Count);
		var recentItemSection = new MenuSection("RecentItem Section", "RecentItem Section", (NoResString)"RecentItem Section", SectionType.RecentItem, 2);
		var recentItemItem1 = new MenuItem("Item 1", (NoResString)"RecentItem Item 1", null, null, null, null, _ => true);
		var recentItemItem2 = new MenuItem("Item 2", (NoResString)"RecentItem Item 2", null, null, null, null, _ => true);
		var recentItemItem3 = new MenuItem("Item 3", (NoResString)"RecentItem Item 3", null, null, null, null, _ => true);
		recentItemSection.AddItem(recentItemItem1);
		recentItemSection.AddItem(recentItemItem2);
		recentItemSection.AddItem(recentItemItem3);
		AssertEquals("The first one will be removed, only remain 2 in collection", 2, recentItemSection.Items.Count);
		recentItemSection.RemoveAllLinksAction.Execute(null);
		AssertEquals("All items should be removed", 0, recentItemSection.Items.Count);
		var recentModuleSection = new MenuSection("RecentModule Section", "RecentModule Section", (NoResString)"RecentModule Section", SectionType.RecentModule, 2);
		var recentModuleItem1 = new MenuItem("Item 1", (NoResString)"RecentModule Item 1", null, null, null, null, _ => true);
		var recentModuleItem2 = new MenuItem("Item 2", (NoResString)"RecentModule Item 2", null, null, null, null, _ => true);
		var recentModuleItem3 = new MenuItem("Item 3", (NoResString)"RecentModule Item 3", null, null, null, null, _ => true);
		recentModuleSection.AddItem(recentModuleItem1);
		recentModuleSection.AddItem(recentModuleItem2);
		recentModuleSection.AddItem(recentModuleItem3);
		AssertEquals("The first one will be removed, only remain 2 in collection", 2, recentModuleSection.Items.Count);
		recentModuleSection.RemoveAllLinksAction.Execute(null);
		AssertEquals("All items should be removed", 0, recentModuleSection.Items.Count);
	}

	void AssertOrder(string description, MenuSection section, params MenuItem[] orderedList)
	{
		Argument.NotNull(orderedList, nameof(orderedList)); // Suggested By ReviewBot 
		for (var i = 0; i < orderedList.Length; i++)
		{
			AssertEquals(string.Format("Item is in correct order: {0}, {1}, {2}", description, orderedList[i].Name, section.Items[i].Name), orderedList[i].Name, section.Items[i].Name);
		}
	}
}
