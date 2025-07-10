#if DEBUG

using System;
using System.Reflection;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	public static class MenuItemExtensions
	{
		public static void OnPopup(this MenuItem menuItem, EventArgs e)
		{
			typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { e });
		}
	}

	public static class ToolStripMenuItemExtensions
	{
		public static void OnOpening(this ToolStripMenuItem menuItem, EventArgs e)
		{
			typeof(ToolStripMenuItem).InvokeMember("OnDropDownShow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { e });
		}
	}
}

#endif
