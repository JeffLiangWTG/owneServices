using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	class UnpublishedDocumentMenuItemsManager
	{
		public static MultilingualString UnpublishedDocumentsMenuText
		{
			get { return ResString.GetMultilingualString("2bc2312a-86bd-4446-aff3-84fa13c1095d", "Un-published Documents"); }
		}

		public static void MoveUnpublishedDocumentMenuItemsToBottom<T>(IList menuItems, ZDocumentsMenuItemHelper<T> helper)
			where T : class
		{
			var foundMenuItems = helper.FindMenuItemsByText(menuItems, UnpublishedDocumentsMenuText);

			foreach (var unpublishedDocumentsMenuItem in foundMenuItems)
			{
				if (menuItems.Count > 1)
				{
					menuItems.Remove(unpublishedDocumentsMenuItem);

					if (!helper.IsLastMenuItemASeparator(menuItems))
					{
						menuItems.Add(helper.GetSeparator());
						menuItems.Add(unpublishedDocumentsMenuItem);
					}
				}
			}
		}
	}

	static class Extensions
	{
		internal static MenuItem[] FindMenuItemsByText(this Menu.MenuItemCollection menuItems, string text)
		{
			var foundMenuItems = new List<MenuItem>();

			foreach (MenuItem menuItem in menuItems)
			{
				if (menuItem.Text == text)
				{
					foundMenuItems.Add(menuItem);
				}
			}

			return foundMenuItems.ToArray();
		}
	}
}
