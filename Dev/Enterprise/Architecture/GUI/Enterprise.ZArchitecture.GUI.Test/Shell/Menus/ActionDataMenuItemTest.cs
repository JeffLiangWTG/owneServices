using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ActionDataMenuItemTest : TestCaseWithDummy
	{
		public void TestActionsDataMenuItemIsEmptyByDefault()
		{
			using (var form = new ZForm(Dummy))
			{
				using (var menuItem = ActionDataMenuItem.New(form))
				{
					AssertEquals("Menu Item's collection should be empty - populated by subclasses", 0, menuItem.MenuItems.Count);
				}

				ActionDataMenuSubclassForTesting.Initialise();
				using (var menuItem = ActionDataMenuItem.New(form))
				{
					menuItem.OnPopup(EventArgs.Empty);
					AssertEquals("Menu item count should be 1, same as whatever is added to the menu in AddCustomMenuItems() override", 1, menuItem.MenuItems.Count);
				}

				using (var menuItem = ActionDataMenuItem.New(form))
				{
					AssertEquals(0, menuItem.MenuItems.Count);
				}
			}
		}

		public void TestActionDataMenuNew()
		{
			using (var form = new ZForm(Dummy))
			{
				using (var menuItem = ActionDataMenuItem.New(form))
				{
					AssertEquals("Should be ActionDataMenu", typeof(ActionDataMenuItem), menuItem.GetType());
				}

				ActionDataMenuSubclassForTesting.Initialise();
				using (var menuItem = ActionDataMenuItem.New(form))
				{
					AssertEquals("Should return ActionDataSubclass type", typeof(ActionDataMenuSubclassForTesting), menuItem.GetType());
				}
			}
		}

		public void TestDefaults()
		{
			using (var form = new ZForm(Dummy))
			using (var menuItem = new ActionDataMenuSubclassForTesting(form))
			{
				menuItem.OnPopup(EventArgs.Empty);
				AssertEquals("Menu's text should say 'Data'", "&Data", menuItem.Text);
				AssertEquals("Menu Item's Business entity should be Dummy", Dummy, menuItem.BusinessEntityForTesting);
				AssertEquals("Menu Item's menu item count should be same as number added in AddCustomMenuItems()", 1, menuItem.MenuItems.Count);
			}
		}
	}
}
