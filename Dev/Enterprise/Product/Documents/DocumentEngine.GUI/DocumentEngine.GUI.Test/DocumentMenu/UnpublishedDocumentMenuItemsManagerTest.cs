using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
	sealed class UnpublishedDocumentMenuItemsManagerTest : TestCase
	{
		public void TestJustOneUnpublishedDocumentMenuInTheListIsUnchangedAfterMovedToBottom_MenuItem()
		{
			using (var menu = new MainMenu())
			{
				var menuItems = menu.MenuItems;

				menuItems.Add(UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText);
				AssertEquals("Pre-condition: Should be 1 menu items in menu.", 1, menuItems.Count);

				UnpublishedDocumentMenuItemsManager.MoveUnpublishedDocumentMenuItemsToBottom(menuItems, new ZDocumentsMenuItemMenuHelper());

				AssertEquals("Should be 1 menu items in menu.", 1, menuItems.Count);
				AssertEquals("menuItems[0].Text", UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText, menuItems[0].Text);
			}
		}

		public void TestJustOneUnpublishedDocumentMenuInTheListIsUnchangedAfterMovedToBottom_ToolStripItem()
		{
			using (var menu = new MenuStrip())
			{
				var menuItems = menu.Items;

				menuItems.Add(new ToolStripMenuItem(UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText, null, null, UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText));
				AssertEquals("Pre-condition: Should be 1 menu items in menu.", 1, menuItems.Count);

				UnpublishedDocumentMenuItemsManager.MoveUnpublishedDocumentMenuItemsToBottom(menuItems, new ZDocumentsToolStripMenuHelper());

				AssertEquals("Should be 1 menu items in menu.", 1, menuItems.Count);
				AssertEquals("menuItems[0].Text", UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText, menuItems[0].Text);
			}
		}

		public void TestUnpublishedDocumentMenuIsMovedToBottom_MenuItem()
		{
			using (var menu = new MainMenu())
			{
				var menuItems = menu.MenuItems;

				menuItems.Add("Menu1");
				menuItems.Add("Menu2");
				menuItems.Add(UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText);
				menuItems.Add("Menu3");
				menuItems.Add("Menu4");
				menuItems.Add("Menu5");
				menuItems.Add("Menu6");

				AssertEquals("Pre-condition: Should be 7 menu items in menu.", 7, menuItems.Count);

				UnpublishedDocumentMenuItemsManager.MoveUnpublishedDocumentMenuItemsToBottom(menuItems, new ZDocumentsMenuItemMenuHelper());

				AssertEquals("Should be 8 menu items in menu.", 8, menuItems.Count);
				AssertEquals("menuItems[6].Text", "-", menuItems[6].Text);
				AssertEquals("menuItems[7].Text", UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText, menuItems[7].Text);
			}
		}

		public void TestUnpublishedDocumentMenuIsMovedToBottom_ToolStripItem()
		{
			using (var menu = new MenuStrip())
			{
				var menuItems = menu.Items;

				menuItems.Add(new ToolStripMenuItem("Menu1", null, null, "Menu1"));
				menuItems.Add(new ToolStripMenuItem("Menu2", null, null, "Menu2"));
				menuItems.Add(new ToolStripMenuItem(UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText, null, null, UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText));
				menuItems.Add(new ToolStripMenuItem("Menu3", null, null, "Menu3"));
				menuItems.Add(new ToolStripMenuItem("Menu4", null, null, "Menu4"));
				menuItems.Add(new ToolStripMenuItem("Menu5", null, null, "Menu5"));
				menuItems.Add(new ToolStripMenuItem("Menu6", null, null, "Menu6"));

				AssertEquals("Pre-condition: Should be 7 menu items in menu.", 7, menuItems.Count);

				UnpublishedDocumentMenuItemsManager.MoveUnpublishedDocumentMenuItemsToBottom(menuItems, new ZDocumentsToolStripMenuHelper());

				AssertEquals("Should be 8 menu items in menu.", 8, menuItems.Count);
				AssertEquals("menuItems[6].GetType()", typeof(ToolStripSeparator), menuItems[6].GetType());
				AssertEquals("menuItems[7].Text", UnpublishedDocumentMenuItemsManager.UnpublishedDocumentsMenuText, menuItems[7].Text);
			}
		}
	}
}
