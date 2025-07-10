using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ZMenuItemCollectionExtensions
	{
		public static MenuItem Find(this MenuItem[] collection, string key)
		{
			return Array.Find(collection, menuItem => menuItem.Name == key);
		}

#if DEBUG
		/// <summary>
		/// Find a menu item from it's text, for Unit Testing only
		/// </summary>
		/// <param name="collection">The MenuItemCollection to search in.</param>
		/// <param name="text">The text to search for.</param>
		/// <param name="findSubitems">Look also in item's subitems.</param>
		public static MenuItem FindByText(this Menu.MenuItemCollection collection, string text, bool findSubitems)
		{ return FindByText((IEnumerable)collection, text, findSubitems); }

		/// <summary>
		/// Find a menu item from it's text, for Unit Testing only
		/// </summary>
		/// <param name="collection">The MenuItemCollection to search in.</param>
		/// <param name="text">The text to search for.</param>
		public static MenuItem FindByText(this Menu.MenuItemCollection collection, string text)
		{ return FindByText((IEnumerable)collection, text, false); }

		/// <summary>
		/// Find a menu item from it's text, for Unit Testing only
		/// </summary>
		/// <param name="collection">The MenuItem collection to search in.</param>
		/// <param name="text">The text to search for.</param>
		/// <param name="findSubitems">Look also in item's subitems.</param>
		public static MenuItem FindByText(this MenuItem[] collection, string text, bool findSubitems)
		{ return FindByText((IEnumerable)collection, text, findSubitems); }

		/// <summary>
		/// Find a menu item from it's text, for Unit Testing only
		/// </summary>
		/// <param name="collection">The MenuItem collection to search in.</param>
		/// <param name="text">The text to search for.</param>
		public static MenuItem FindByText(this MenuItem[] collection, string text)
		{ return FindByText((IEnumerable)collection, text, false); }

		/// <summary>
		/// Find a menu item from it's text, for Unit Testing only
		/// </summary>
		/// <param name="collection">The MenuItem collection to search in.</param>
		/// <param name="text">The text to search for.</param>
		/// <param name="findSubitems">Look also in item's subitems.</param>
		public static MenuItem FindByText(this IEnumerable<MenuItem> collection, string text, bool findSubitems)
		{ return FindByText((IEnumerable)collection, text, findSubitems); }

		/// <summary>
		/// Find a menu item from it's text, for Unit Testing only
		/// </summary>
		/// <param name="collection">The MenuItem collection to search in.</param>
		/// <param name="text">The text to search for.</param>
		public static MenuItem FindByText(this IEnumerable<MenuItem> collection, string text)
		{ return FindByText((IEnumerable)collection, text, false); }

		static MenuItem FindByText(IEnumerable collection, string text, bool findSubitems)
		{
			foreach (MenuItem menuItem in collection)
			{
				if (CleanUpText(menuItem.Text) == CleanUpText(text))
				{
					return menuItem;
				}

				if (findSubitems && menuItem.MenuItems.Count > 0)
				{
					var subItem = FindByText((IEnumerable)menuItem.MenuItems, text, true);
					if (subItem != null)
					{
						return subItem;
					}
				}
			}
			return null;
		}

		static string CleanUpText(string text)
		{
			return text?.Replace("&", "") ?? "";
		}
#endif

		public static MenuItem Add(this Menu.MenuItemCollection @this, string caption, EventHandler onClick)
		{
			var item = new ZMenuItem(caption, onClick);
			@this.Add(item);
			return item;
		}

		public static MenuItem Add(this Menu.MenuItemCollection @this, MultilingualString caption, EventHandler onClick)
		{
			var item = new ZMenuItem(caption, onClick);
			@this.Add(item);
			return item;
		}

		public static MenuItem Add(this Menu.MenuItemCollection @this, MultilingualString caption)
		{
			var item = new ZMenuItem(caption);
			@this.Add(item);
			return item;
		}
	}
}
