using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class ImagePageBufferMenuTest : TestCaseWithFactory
	{
		public void TestAddMenuItems()
		{
			ContextMenu originalMenu = new ContextMenu();
			int oldCount = originalMenu.MenuItems.Count;
			AssertEquals("Original menu has no menu items", 0, oldCount);

			ImagePageBufferMenu menu = new ImagePageBufferMenu(originalMenu, false);
			Assert("Original menu now has more menu items", oldCount < originalMenu.MenuItems.Count);

			foreach (MenuItem item in originalMenu.MenuItems)
			{
				Assert("for nonreadonly menu items, all items enabled", item.Enabled);
			}

			ContextMenu anotherMenu = new ContextMenu();
			int oldCount2 = anotherMenu.MenuItems.Count;
			ImagePageBufferMenu menu2 = new ImagePageBufferMenu(anotherMenu, true);
			foreach (MenuItem item in anotherMenu.MenuItems)
			{
				if (!(item.Text == "-" || item.Text == "&Copy" || item.Text == "Select &All"))
				{
					Assert("menu items should be disabled for readonly context menu", !item.Enabled);
				}
			}
		}
	}
}
