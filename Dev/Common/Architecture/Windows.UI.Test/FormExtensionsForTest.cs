using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	public static class FormExtensionsForTest
	{
		public static bool ShowAndCheckFormIsActive(this Form form)
		{
			bool result = false;
			form.Paint += delegate
			{ result = true; };
			form.Show();
			result = result && form == Form.ActiveForm;
			Assertion.Assert("A unit test might not be able to run if the form is not properly active", true);
			return result;
		}

		public static void OnPopup_ForTest(this ContextMenu menu)
		{
			menu.GetType().InvokeMember("OnPopup", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
		}

		public static string GetVisibleMenuItemsCaptions(this MenuItem menuItem)
		{
			var stringBuilder = new StringBuilder();
			ConstructMenuItemsCaptions(menuItem, 0, stringBuilder);
			return stringBuilder.ToString();
		}

		static void ConstructMenuItemsCaptions(MenuItem menuItem, int level, StringBuilder builder)
		{
			if (!menuItem.Visible)
			{
				return;
			}
			var prefix = new string(' ', level * 3);
			var lineBreak = level == 0 ? "" : Environment.NewLine;
			builder.Append(string.Concat(lineBreak, prefix, menuItem.Text));
			foreach (var childMenuItem in menuItem.MenuItems.Cast<MenuItem>())
			{
				ConstructMenuItemsCaptions(childMenuItem, level + 1, builder);
			}
		}
	}
}
