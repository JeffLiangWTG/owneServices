using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Core.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZEditMenuItemTest : TestCase
	{
#if !WINZOR
		public void TestMenuItems()
		{
			using (var winform = new ZForm())
			{
				var editMenuItem = FindZEditMenuItem(winform);
				AssertNotNull("Cannot find edit menu on form", editMenuItem);

				AssertEquals("Cut", editMenuItem.cutMenuItem.Text);
				AssertEquals("Copy", editMenuItem.copyMenuItem.Text);
				AssertEquals("Paste", editMenuItem.pasteMenuItem.Text);

				AssertEquals(Shortcut.CtrlX, editMenuItem.cutMenuItem.Shortcut);
				AssertEquals(Shortcut.CtrlC, editMenuItem.copyMenuItem.Shortcut);
				AssertEquals(Shortcut.CtrlP, editMenuItem.pasteMenuItem.Shortcut);

				Assert(editMenuItem.MenuItems.Contains(editMenuItem.cutMenuItem));
				Assert(editMenuItem.MenuItems.Contains(editMenuItem.copyMenuItem));
				Assert(editMenuItem.MenuItems.Contains(editMenuItem.pasteMenuItem));
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyCutPaste()
		{
			using (var form = new TestZForm())
			{
				var editMenu = FindZEditMenuItem(form);

				form.TextBox1.Text = "HELLOWORLD";
				form.ActiveControl = form.TextBox1;
				form.TextBox1.SelectAll();

				var action = new Action(() =>
				{
					editMenu.copyMenuItem.PerformClick();
					Application.DoEvents();
				});
				action.Invoke();

				var text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action, retry: 20);
				AssertEquals("HELLOWORLD", text);

				form.TextBox1.Select(3, 4);
				action = () =>
				{
					editMenu.cutMenuItem.PerformClick();
					Application.DoEvents();
				};
				action.Invoke();

				text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action, retry: 20);
				AssertEquals("LOWO", text);

				form.TextBox1.Clear();
				editMenu.pasteMenuItem.PerformClick();
				Application.DoEvents();
				AssertEquals("LOWO", form.TextBox1.Text);
			}
		}
#endif
		#region Implementation
#if !WINZOR
		ZEditMenuItem FindZEditMenuItem(ZForm form)
		{
			foreach (MenuItem item in form.Menu.MenuItems)
			{
				if (item is ZEditMenuItem)
				{
					return (ZEditMenuItem)item;
				}
			}
			return null;
		}
#endif
#endregion
	}
}
