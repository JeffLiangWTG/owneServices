using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class MenuItemTest : TestCase
{
	readonly MultilingualString resStringForTest = (NoResString)"item 1";
	public void TestDragDataType()
	{
		var item = new MenuItem(resStringForTest);
		AssertEquals(typeof(MenuItem), item.DataType);
	}

	public void TestFavoriteAction()
	{
		var item = new MenuItem(resStringForTest);
		AssertNull("Favorite Action is not set", item.FavoriteAction);
		item.SetFavoriteAction(() =>
		{
		});
		AssertNotNull("Favorite Action is set", item.FavoriteAction);
	}

	public void TestIsInFavorites()
	{
		var item = new MenuItem(resStringForTest);
		var propertyChangedCalled = false;
		Assert("Not in favorites", !item.IsInFavorites);
		item.PropertyChanged += (sender, e) =>
		{
			propertyChangedCalled = true;
		};
		Assert("Property changed not called", !propertyChangedCalled);
		item.SetFavoriteAction(() =>
		{
			item.IsInFavorites = true;
		});
		item.FavoriteAction.Execute(null);
		Assert("Item in favorites", item.IsInFavorites);
		Assert("Property changed called", propertyChangedCalled);
	}

	public void TestSetIDWithName()
	{
		var testKey = "WorkItem48dc7703-61fc-405b-9aa7-84eb0117a156";
		var testResString = (NoResString)"W000001 - Test WorkItem";
		var item = new MenuItem(testKey, testResString);
		item.SetIDWithName();
		AssertEquals("ID is set", "W000001", item.ID);
		AssertEquals("Name is set", "Test WorkItem", item.Name);
	}

	public void TestSetIDWithNameRefContainer()
	{
		var testKey = "RefContainer48dc7703-61fc-405b-9aa7-84eb0117a156";
		var testResString = (NoResString)"LIVE2 - LIVESTOCK TRAILER";
		var item = new MenuItem(testKey, testResString);
		item.SetIDWithName();
		AssertEquals("ID is set", "LIVE2", item.ID);
		AssertEquals("Name is set", "LIVESTOCK TRAILER", item.Name);
	}

	public void TestSetIDWithNameTwoHyphens()
	{
		var testKey = "RefContainer48dc7703-61fc-405b-9aa7-84eb0117a156";
		var testResString = (NoResString)"LIVE2 - LIVESTOCK - TRAILER";
		var item = new MenuItem(testKey, testResString);
		item.SetIDWithName();
		AssertEquals("ID is set", "LIVE2", item.ID);
		AssertEquals("Name is set", "LIVESTOCK - TRAILER", item.Name);
	}

	public void TestSetIDForBlackListModules1()
	{
		string testKey = "NetworkDiagram48dc7703-61fc-405b-9aa7-84eb0117a156";
		MultilingualString testResString = ResString.GetMultilingualString("E740A76F-EBAC-4E2E-BF3F-F121B51BDAF2", "GLOW: Client-to-Client Communications Channel");
		var menuItem = new MenuItem(testKey, testResString, module: ResString.GetMultilingualString("E740A76F-EBAC-4E2E-BF3F-F121B51BDAF1", "Network Diagrams"), null, null, null, null);
		menuItem.SetIDWithName();
		AssertNullOrEmptyOrWhitespace(menuItem.ID);
		AssertEquals("Name is not split", "GLOW: Client-to-Client Communications Channel", menuItem.Name);
	}

	public void TestSetIDForBlackListModules2()
	{
		string testKey = "VisualBoard48dc7703-61fc-405b-9aa7-84eb0117a156";
		MultilingualString testResString = ResString.GetMultilingualString("E740A76F-EBAC-4E2E-BF3F-F121B51BDAF2", "Buffer-And-Webpage");
		var menuItem = new MenuItem(testKey, testResString, module: ResString.GetMultilingualString("E740A76F-EBAC-4E2E-BF3F-F121B51BDAF1", "Visual Boards"), null, null, null, null);
		menuItem.SetIDWithName();
		AssertNullOrEmptyOrWhitespace(menuItem.ID);
		AssertEquals("Name is not split", "Buffer-And-Webpage", menuItem.Name);
	}

	public void TestBothIDandModuleNotSet()
	{
		var testKey = "RefContainer48dc7703-61fc-405b-9aa7-84eb0117a156";
		var testResString = (NoResString)"LIVE2 - LIVESTOCK TRAILER";
		var item = new MenuItem(testKey, testResString);

		AssertNull("ID is not set", item.ID);
		AssertNull("Module is not set", item.Module);

		Assert(!item.AreBothIDandModuleSet);
	}

	public void TestBothIDandModuleSet()
	{
		var testKey = "RefContainer48dc7703-61fc-405b-9aa7-84eb0117a156";
		var testResString = (NoResString)"LIVE2 - LIVESTOCK TRAILER";
		var testModuleResString = (NoResString)"Module Name";
		var item = new MenuItem(testKey, testResString, module: testModuleResString, null, null, null, null);
		item.SetIDWithName();
		
		AssertNotNull("ID is set", item.ID);
		AssertNotNull("Module is set", item.Module);

		Assert(item.AreBothIDandModuleSet);
	}
}
