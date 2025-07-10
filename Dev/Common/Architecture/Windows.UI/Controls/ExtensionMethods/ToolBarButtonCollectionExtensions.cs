using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Extension methods for the CargoWise.Windows.UI.KTabControl.ToolBarButtonCollection class.
	/// </summary>
	public static class ToolBarButtonCollectionExtensions
	{
		/// <summary>
		/// Find a tool bar button from it's text.
		/// </summary>
		/// <param name="collection">The ToolBarButtonCollection to search in.</param>
		/// <param name="text">The text to search for.</param>
		public static ToolBarButton FindByText(this ToolBar.ToolBarButtonCollection collection, string text)
		{
			return FindByText((IEnumerable)collection, text);
		}

		public static MenuItem FindByText(this ToolBarButton button, string text)
		{
			return button.DropDownMenu.MenuItems
				.SelectRecursive(m => m.Cast<MenuItem>().Select(mi => mi.MenuItems))
				.SelectMany(items => items.Cast<MenuItem>())
				.Concat(button.DropDownMenu.MenuItems.Cast<MenuItem>())
				.FirstOrDefault(m => m.Text == text);
		}

		/// <summary>
		/// Find a tool bar button from it's text.
		/// </summary>
		/// <param name="collection">The ToolBarButton collection to search in.</param>
		/// <param name="text">The text to search for.</param>
		public static ToolBarButton FindByText(this ToolBarButton[] collection, string text)
		{ return FindByText((IEnumerable)collection, text); }

		/// <summary>
		/// Find a tool bar button from it's text.
		/// </summary>
		/// <param name="collection">The ToolBarButton collection to search in.</param>
		/// <param name="text">The text to search for.</param>
		public static ToolBarButton FindByText(this IEnumerable<ToolBarButton> collection, string text)
		{ return FindByText((IEnumerable)collection, text); }

		static ToolBarButton FindByText(IEnumerable collection, string text)
		{
			foreach (ToolBarButton toolBarButton in collection)
			{
				if (KMenuItem.StripAcceleratorKeys(toolBarButton.Text) == KMenuItem.StripAcceleratorKeys(text))
				{
					return toolBarButton;
				}
			}
			return null;
		}
	}
}
