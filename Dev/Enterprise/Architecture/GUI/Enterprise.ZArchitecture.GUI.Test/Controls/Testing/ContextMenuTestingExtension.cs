using System;
using System.Reflection;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class ContextMenuTestingExtension
	{
		public static void ShowPopupMenu(this ContextMenu contextMenu)
		{
			contextMenu.GetType().GetMethod("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(contextMenu, new object[] { EventArgs.Empty });
		}

		public static void ShowPopupMenu(this MenuItem menuItem)
		{
			var popupMethod = menuItem.GetType().GetMethod("OnPopup", BindingFlags.Instance | BindingFlags.NonPublic);
			popupMethod.Invoke(menuItem, new object[] { EventArgs.Empty });
		}
	}
}
