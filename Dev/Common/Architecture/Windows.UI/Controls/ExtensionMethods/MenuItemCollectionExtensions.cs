using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Extension methods for the System.Windows.Forms.MenuItem class.
	/// </summary>
	public static class MenuItemCollectionExtensions
	{
		/// <summary>
		/// Find a menu item from it's name.
		/// </summary>
		/// <param name="collection">The MenuItem collection to search in.</param>
		/// <param name="name">The name to search for.</param>
		/// <param name="findSubitems">Look also in item's subitems.</param>
		public static MenuItem FindByName(this Menu.MenuItemCollection collection, string name, bool findSubitems)
		{ return FindByName((IEnumerable)collection, name, findSubitems); }

		/// <summary>
		/// Find a menu item from it's name.
		/// </summary>
		/// <param name="collection">The MenuItem collection to search in.</param>
		/// <param name="name">The name to search for.</param>
		public static MenuItem FindByName(this Menu.MenuItemCollection collection, string name)
		{ return FindByName((IEnumerable)collection, name, false); }

		static MenuItem FindByName(IEnumerable collection, string name, bool findSubitems)
		{
			foreach (MenuItem menuItem in collection)
			{
				if (menuItem.Name == name)
				{
					return menuItem;
				}
				if (findSubitems && menuItem.MenuItems.Count > 0)
				{
					MenuItem subItem = FindByName((IEnumerable)menuItem.MenuItems, name, true);
					if (subItem != null)
					{
						return subItem;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Set the Visible flag for all MenuItems.
		/// </summary>
		/// <param name="collection">The collection of MenuItems to set visible for.</param>
		/// <param name="visible">The value of Visible to set the MenuItems to.</param>
		public static void SetAllVisible(this IEnumerable<MenuItem> collection, bool visible)
		{
			foreach (MenuItem menuItem in collection)
			{
				menuItem.Visible = visible;
			}
		}
	}
}
