using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using MenuItem = System.Windows.Forms.MenuItem;

namespace Enterprise.Accounting.GUI.Testing
{
	public class GUIComponentsStateRestorerTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice));
			using (var form = new BaseInvoicingForm(invoice))
			{
				AssertType<GUIComponentsStateRestorer>(form.GUIComponentsStateRestorer_ExposedForTestOnly);
			}
		}

		public void TestDisableAndRestoreStatesControlsAndMenuItems()
		{
			using (var form = new ZForm())
			{
				var button1 = new ZButton();
				button1.Enabled = false;
				form.Controls.Add(button1);
				var button2 = new ZButton();
				button2.Enabled = true;
				form.Controls.Add(button2);
				var button3 = new ZButton();
				button3.Enabled = true;
				form.Controls.Add(button3);
				var button4 = new ZButton();
				button4.Enabled = false;
				form.Controls.Add(button4);

				var grid = new ZGrid();
				form.Controls.Add(grid);
				var menuItem1 = new MenuItem();
				menuItem1.Enabled = true;
				grid.ContextMenu.MenuItems.Add(menuItem1);
				var menuItem2 = new MenuItem();
				menuItem2.Enabled = true;
				grid.ContextMenu.MenuItems.Add(menuItem2);
				var menuItem3 = new MenuItem();
				menuItem3.Enabled = false;
				grid.ContextMenu.MenuItems.Add(menuItem3);
				var menuItem4 = new MenuItem();
				menuItem4.Enabled = false;
				grid.ContextMenu.MenuItems.Add(menuItem4);

				var restorer = new GUIComponentsStateRestorer() as IGUIComponentsStateRestorer;
				AssertNoExceptionThrown(() => restorer.DisableControlsAndMenuItems(null, null));
				AssertNoExceptionThrown(() => restorer.RestoreControlsAndMenuItems(null, null));
				AssertNoExceptionThrown(() => restorer.DisableControlsAndMenuItems(new ZButton[] { null }, null));
				AssertNoExceptionThrown(() => restorer.RestoreControlsAndMenuItems(null, new MenuItem[] { null }));
				AssertNoExceptionThrown(() => restorer.DisableControlsAndMenuItems(new ZButton[] { null }, new MenuItem[] { null }));
				AssertNoExceptionThrown(() => restorer.RestoreControlsAndMenuItems(new ZButton[] { null }, new MenuItem[] { null }));
				restorer.DisableControlsAndMenuItems(new[] { button1, button2, button3, button4 }, new[] { menuItem1, menuItem2, menuItem3, menuItem4 });
				CombineAssertions("All buttons and menu items are disabled", () =>
				{
					Assert(!button1.Enabled);
					Assert(!button2.Enabled);
					Assert(!button3.Enabled);
					Assert(!button4.Enabled);
					Assert(!menuItem1.Enabled);
					Assert(!menuItem2.Enabled);
					Assert(!menuItem3.Enabled);
					Assert(!menuItem4.Enabled);
				});

				var anotherRestorer = new GUIComponentsStateRestorer() as IGUIComponentsStateRestorer;
				anotherRestorer.RestoreControlsAndMenuItems(new[] { button1, button2, button3, button4 }, new[] { menuItem1, menuItem2, menuItem3, menuItem4 });
				CombineAssertions("States remain unchanged if another restorer is used", () =>
				{
					Assert(!button1.Enabled);
					Assert(!button2.Enabled);
					Assert(!button3.Enabled);
					Assert(!button4.Enabled);
					Assert(!menuItem1.Enabled);
					Assert(!menuItem2.Enabled);
					Assert(!menuItem3.Enabled);
					Assert(!menuItem4.Enabled);
				});

				restorer.RestoreControlsAndMenuItems(new[] { button1, null, button3, button4 }, new[] { menuItem1, null, menuItem3, menuItem4 });
				CombineAssertions("Can restore states of buttons and menu items selectively", () =>
				{
					Assert(!button1.Enabled);
					Assert(!button2.Enabled);
					Assert(button3.Enabled);
					Assert(!button4.Enabled);
					Assert(menuItem1.Enabled);
					Assert(!menuItem2.Enabled);
					Assert(!menuItem3.Enabled);
					Assert(!menuItem4.Enabled);
				});

				restorer.RestoreControlsAndMenuItems(new[] { button2 }, new[] { menuItem2 });
				CombineAssertions("It will still restore states of remaining buttons and menu items which were remaining to be restored", () =>
				{
					Assert(button2.Enabled);
					Assert(menuItem2.Enabled);
				});

				restorer.DisableControlsAndMenuItems(new[] { button1, button2, button3, button4 }, new[] { menuItem1, menuItem2, menuItem3, menuItem4 });
				CombineAssertions("All buttons and menu items are disabled, restorer can be reused to diable", () =>
				{
					Assert(!button1.Enabled);
					Assert(!button2.Enabled);
					Assert(!button3.Enabled);
					Assert(!button4.Enabled);
					Assert(!menuItem1.Enabled);
					Assert(!menuItem2.Enabled);
					Assert(!menuItem3.Enabled);
					Assert(!menuItem4.Enabled);
				});

				restorer.DisableControlsAndMenuItems(new[] { button1, button2, button3, button4 }, new[] { menuItem1, menuItem2, menuItem3, menuItem4 });
				restorer.RestoreControlsAndMenuItems(new[] { button1, button2, button3, button4 }, new[] { menuItem1, menuItem2, menuItem3, menuItem4 });
				CombineAssertions("Can restore states of all buttons and menu items at once, disabling twice does not have any effect on restoring states, restorer can be reused", () =>
				{
					Assert(!button1.Enabled);
					Assert(button2.Enabled);
					Assert(button3.Enabled);
					Assert(!button4.Enabled);
					Assert(menuItem1.Enabled);
					Assert(menuItem2.Enabled);
					Assert(!menuItem3.Enabled);
					Assert(!menuItem4.Enabled);
				});

				restorer.RestoreControlsAndMenuItems(new[] { button1, button2, button3, button4 }, new[] { menuItem1, menuItem2, menuItem3, menuItem4 });
				CombineAssertions("Calling Restore again keeps the existing states of buttons and menu items intact", () =>
				{
					Assert(!button1.Enabled);
					Assert(button2.Enabled);
					Assert(button3.Enabled);
					Assert(!button4.Enabled);
					Assert(menuItem1.Enabled);
					Assert(menuItem2.Enabled);
					Assert(!menuItem3.Enabled);
					Assert(!menuItem4.Enabled);
				});
			}
		}

		public void TestDisableControlIfApplicableAndUpdateCurrentState()
		{
			using (var form = new ZForm())
			{
				var button = new ZButton();
				button.Enabled = true;
				form.Controls.Add(button);
				var restorer = new GUIComponentsStateRestorer() as IGUIComponentsStateRestorer;

				restorer.DisableControlIfApplicableAndUpdateCurrentState(button, false);
				Assert(!button.Enabled);
				restorer.DisableControlIfApplicableAndUpdateCurrentState(button, true);
				Assert(button.Enabled);

				restorer.DisableControlsAndMenuItems(new[] { button }, null);
				restorer.DisableControlIfApplicableAndUpdateCurrentState(button, false);
				Assert(!button.Enabled);
				restorer.RestoreControlsAndMenuItems(new[] { button }, null);
				Assert(!button.Enabled);
				restorer.DisableControlsAndMenuItems(new[] { button }, null);
				restorer.DisableControlIfApplicableAndUpdateCurrentState(button, true);
				Assert(!button.Enabled);
				restorer.RestoreControlsAndMenuItems(new[] { button }, null);
				Assert(button.Enabled);
			}
		}

		public void TestDisableMenuItemIfApplicableAndUpdateCurrentState()
		{
			using (var form = new ZForm())
			{
				var grid = new ZGrid();
				form.Controls.Add(grid);
				var menuItem = new MenuItem();
				menuItem.Enabled = true;
				grid.ContextMenu.MenuItems.Add(menuItem);
				var restorer = new GUIComponentsStateRestorer() as IGUIComponentsStateRestorer;

				restorer.DisableMenuItemIfApplicableAndUpdateCurrentState(menuItem, false);
				Assert(!menuItem.Enabled);
				restorer.DisableMenuItemIfApplicableAndUpdateCurrentState(menuItem, true);
				Assert(menuItem.Enabled);

				restorer.DisableControlsAndMenuItems(null, new[] { menuItem });
				restorer.DisableMenuItemIfApplicableAndUpdateCurrentState(menuItem, false);
				Assert(!menuItem.Enabled);
				restorer.RestoreControlsAndMenuItems(null, new[] { menuItem });
				Assert(!menuItem.Enabled);
				restorer.DisableControlsAndMenuItems(null, new[] { menuItem });
				restorer.DisableMenuItemIfApplicableAndUpdateCurrentState(menuItem, true);
				Assert(!menuItem.Enabled);
				restorer.RestoreControlsAndMenuItems(null, new[] { menuItem });
				Assert(menuItem.Enabled);
			}
		}
	}
}
