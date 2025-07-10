using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZLazyPopulatingMenuItem : ZMenuItem
	{
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public ZLazyPopulatingMenuItem(string text, Func<IEnumerable<ZMenuItem>> menuItemFunc, bool shouldCache = false)
			: this(menuItemFunc, shouldCache)
		{
			Text = text;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public ZLazyPopulatingMenuItem(MultilingualString caption, Func<IEnumerable<ZMenuItem>> menuItemFunc, bool shouldCache = false)
			: this(menuItemFunc, shouldCache)
		{
			Caption = caption;
		}

		ZLazyPopulatingMenuItem(Func<IEnumerable<ZMenuItem>> menuItemFunc, bool shouldCache)
		{
			createStack = new StackTrace();
			this.menuItemFunc = menuItemFunc;
			this.shouldCache = shouldCache;

			MenuItems.Add(Separator);
			Popup += MenuItem_Popup;
		}

		readonly Func<IEnumerable<ZMenuItem>> menuItemFunc;
		readonly bool shouldCache;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "message is only for developer")]
		void MenuItem_Popup(object sender, EventArgs e)
		{
			try
			{
				MenuItems.Clear();

				foreach (var menuItem in GetMenuItems())
				{
					if (!menuItem.IsMenuDisposed)
					{
						MenuItems.Add(menuItem);
					}
				}
			}
			catch (NullReferenceException ex)
			{
				var msg = string.Format("MenuItem Name: {0}\r\nType: {1}\r\nCreate Stack:\r\n{2}", Name, GetType().FullName, createStack);
				ErrorReporter.ReportOnce("NullReferenceExceptionInZLazyPopulatingMenuItem", msg, ex);
			}
		}

		IEnumerable<ZMenuItem> GetMenuItems() => shouldCache
			? cachedMenuItems ?? (cachedMenuItems = menuItemFunc?.Invoke())
			: menuItemFunc?.Invoke();
		IEnumerable<ZMenuItem> cachedMenuItems;

		readonly StackTrace createStack;
	}
}
