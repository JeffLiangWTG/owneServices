using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
namespace CargoWise.Main.Navigation;

static class ToolStripItemExtensions
{
	public static PopupMenu ToPopupMenu(this ToolStripItem menuItem, Action<PopupMenu> afterCreate = null)
	{
		if (menuItem == null)
		{
			return null;
		}

		if (menuItem is ToolStripSeparator separator)
		{
			return new PopupMenu { IsSeparator = true, ToolStripDropDown = menuItem };
		}

		var popupMenu = new PopupMenu
		{
			Title = menuItem.Text,
			Command = new MenuCommand(menuItem.PerformClick),
			ToolStripDropDown = menuItem,
			InputGestureText = menuItem.GetInputGestureText(),
		};

		var attributes = TypeDescriptor.GetAttributes(menuItem);
		if (attributes[typeof(SuppressFormsLocalizedTestAttribute)] != null)
		{
			TypeDescriptor.AddAttributes(popupMenu, new SuppressFormsLocalizedTestAttribute());
		}

		if (menuItem is ToolStripMenuItem item)
		{
			foreach (var subItem in item.DropDownItems)
			{
				if (subItem is ToolStripItem subMenuItem)
				{
					popupMenu.SubMenuItems.Add(subMenuItem.ToPopupMenu(afterCreate));
				}
			}
		}

		afterCreate?.Invoke(popupMenu);

		return popupMenu;
	}

	public static string GetInputGestureText(this ToolStripItem menuItem)
	{
		if (menuItem is ToolStripMenuItem item)
		{
			if (item.ShortcutKeys == Keys.None)
			{
				return string.Empty;
			}

			return TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(item.ShortcutKeys);
		}

		return string.Empty;
	}
}
