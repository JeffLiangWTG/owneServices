using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;

namespace Enterprise.Core.Forms
{
	public static class MenuDisposer
	{
		[ThreadStatic]
		static Hashtable AllMenuItems;

		public static void ClearAndDispose(Menu.MenuItemCollection items)
		{
			foreach (MenuItem item in items)
			{
				DisposeMenu(item);
			}

			items.Clear();
		}

		public static void DisposeMenu(Menu menuToDispose)
		{
			// The .Net MenuItem class has a private static Hashtable allCreatedMenuItems. There is a bug when rearranging the menu
			// that causes menu items to be placed in the hash multiple times, with a new key each time, but .Net only keeps track of
			// the latest key. Hence, the other references cannot be removed on Dispose, and there is a memory leak. This code
			// looks for all instances of the MenuItem being disposed in the hash and removes every entry.
			if (AllMenuItems == null)
			{
#if WINZOR
				throw new NotImplementedException("Winzor does not seem to have an equivalent field.");
#elif NET
				const string fieldName = "s_allCreatedMenuItems";
#else
				const string fieldName = "allCreatedMenuItems";
#endif
				var menuItemsInfo = typeof(MenuItem).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
				AllMenuItems = (Hashtable)menuItemsInfo.GetValue(null);
			}

			var itemsToRemove = new ArrayList();
			lock (AllMenuItems)
			{
				foreach (DictionaryEntry entry in AllMenuItems)
				{
					if (entry.Value == menuToDispose)
					{
						itemsToRemove.Add(entry.Key);
					}
				}
			}

			foreach (MenuItem item in menuToDispose.MenuItems)
			{
				DisposeMenu(item);
			}

			foreach (var key in itemsToRemove)
			{
				AllMenuItems.Remove(key);
			}
		}
	}
}
