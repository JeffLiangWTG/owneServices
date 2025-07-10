using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZLazyPopulatingMenuItemTest : TestCase
	{
		public void TestPopulate_ChildMenuDisposed()
		{
			var zMenuItem = new ZMenuItem("Menu Test");
			var zChildMenuItem = new ZMenuItem("Child Menu");
			zMenuItem.MenuItems.Add(zChildMenuItem);
			AssertEquals("Parent ZMenuItem should have 1 child menu", 1, zMenuItem.MenuItems.Count);

			zChildMenuItem.Dispose();
			AssertEquals("Parent ZMenuItem should have no child menu", 0, zMenuItem.MenuItems.Count);

			zChildMenuItem = new ZMenuItem("Child Menu");
			var zLazyMenuItem = new DummyZLazyPopulatingMenuItem((NoResString)"Menu Test", () => new[] { zChildMenuItem });
			AssertEquals("Parent ZLazyPopulatingMenuItem should have 1 child menu", 1, zLazyMenuItem.MenuItems.Count);

			zChildMenuItem.Dispose();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => zLazyMenuItem.OnPopup());
				AssertEquals("Parent ZLazyPopulatingMenuItem should have no child menu", 0, zLazyMenuItem.MenuItems.Count);
			});
		}

		public void TestPopulate_EmptyMenu()
		{
			var menuItem = new DummyZLazyPopulatingMenuItem((NoResString)"Booo", () => EmptyMenu);
			AssertEquals("Booo", menuItem.Text);

			AssertEquals("Should contain one child menu item so the dropdown can be accessed", 1, menuItem.MenuItems.Count);
			AssertEquals(ZMenuItem.Separator, menuItem.MenuItems[0].Text);

			menuItem.OnPopup();
			AssertEquals(0, menuItem.MenuItems.Count);
		}

		public void TestPopulate_NonEmptyMenu()
		{
			var menuItem = new DummyZLazyPopulatingMenuItem((NoResString)"Booo", () => TwoMenuItems);
			AssertEquals("Booo", menuItem.Text);

			AssertEquals("Should contain one child menu item so the dropdown can be accessed", 1, menuItem.MenuItems.Count);
			AssertEquals(ZMenuItem.Separator, menuItem.MenuItems[0].Text);

			menuItem.OnPopup();
			AssertEquals(2, menuItem.MenuItems.Count);
			AssertEquals("King Prince Chambermaid", menuItem.MenuItems[0].Text);
			AssertEquals("Bisquiteen Trisket", menuItem.MenuItems[1].Text);
		}

		public void TestShouldCallFuncJustOnce_WhenCachingEnabled()
		{
			var callCounter = 0;
			var menuItem = new DummyZLazyPopulatingMenuItem((NoResString)"Booo", () =>
			{
				callCounter++;
				return Array.Empty<ZMenuItem>();
			}, shouldCache: true);

			menuItem.OnPopup();
			AssertEquals("Should call the function", 1, callCounter);

			menuItem.OnPopup();
			AssertEquals("Should not call the function again - should take from cache instead", 1, callCounter);
		}

		public void TestShouldCallFuncEachTimeOnPopup_WhenCachingDisabled()
		{
			var callCounter = 0;
			var menuItem = new DummyZLazyPopulatingMenuItem((NoResString)"Booo", () =>
			{
				callCounter++;
				return Array.Empty<ZMenuItem>();
			}, shouldCache: false);

			menuItem.OnPopup();
			AssertEquals("Should call the function", 1, callCounter);

			menuItem.OnPopup();
			AssertEquals("Should call the function again", 2, callCounter);
		}

		#region Implementation

		static IEnumerable<ZMenuItem> EmptyMenu
		{
			get { yield break; }
		}

		static IEnumerable<ZMenuItem> TwoMenuItems
		{
			get
			{
				yield return new ZMenuItem("King Prince Chambermaid");
				yield return new ZMenuItem("Bisquiteen Trisket");
			}
		}

		class DummyZLazyPopulatingMenuItem : ZLazyPopulatingMenuItem
		{
			internal DummyZLazyPopulatingMenuItem(MultilingualString caption, Func<IEnumerable<ZMenuItem>> menuItemFactory, bool shouldCache = false)
				: base(caption, menuItemFactory, shouldCache)
			{
			}

			internal void OnPopup()
			{
				base.OnPopup(EventArgs.Empty);
			}
		}

		#endregion
	}
}
