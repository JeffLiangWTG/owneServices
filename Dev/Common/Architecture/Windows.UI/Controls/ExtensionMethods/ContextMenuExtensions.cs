
namespace CargoWise.Windows.UI
{
	using System;
	using System.Reflection;
	using System.Windows.Forms;

	public static class ContextMenuExtensions
	{
		public static void DoPopup(this ContextMenu menu)
		{
			typeof(ContextMenu).InvokeMember("OnPopup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
		}

		public static void SetSourceControl(this ContextMenu menu, Control control)
		{
			typeof(ContextMenu).GetField("sourceControl", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod).SetValue(menu, control);
		}
	}
}
