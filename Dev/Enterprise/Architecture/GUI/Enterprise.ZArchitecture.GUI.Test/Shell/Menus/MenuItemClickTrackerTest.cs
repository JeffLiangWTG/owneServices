using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.Core.Forms.Internal.Testing
{
	public abstract class MenuItemClickTrackerTest : TransactionedTestCase
	{
		public void TestTrackClick()
		{
			var i = 0;
			KForm testPopupForm = null;

			using (var control = GetMenuOwnerControl())
			{
				var menu = GetMenuToTest();

				menu.MenuItems.Add(new MenuItem("Test1"));
				menu.MenuItems.Add(new MenuItem("Test2"));
				var testItem3 = new MenuItem("Test3");
				menu.MenuItems.Add(testItem3);
				testItem3.Click += delegate
				{
					i++;
					testPopupForm.Show();
					testPopupForm.Dispose();
					Application.DoEvents(); // this causes the second message to be processed while the first is still executing.
				};

				var menuId = new IntPtr((int)typeof(MenuItem).GetProperty("MenuID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(testItem3, null));
				using (testPopupForm = new KForm())
				{
					UnsafeNativeMethods.PostMessage(new HandleRef(control, control.Handle), WindowsMessage.WM_COMMAND, menuId, IntPtr.Zero);
					UnsafeNativeMethods.PostMessage(new HandleRef(control, control.Handle), WindowsMessage.WM_COMMAND, menuId, IntPtr.Zero);
					Application.DoEvents();
				}
			}

			AssertEquals("Click handler invoke count", 1, i);
		}

		protected abstract Control GetMenuOwnerControl();
		protected abstract Menu GetMenuToTest();
	}
}
