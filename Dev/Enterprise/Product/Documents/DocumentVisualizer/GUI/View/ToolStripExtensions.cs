using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	static class ToolStripExtensions
	{
		#region CreateToolStripItem

		public static ToolStripItem CreateToolStripItem(this IMenuItemDescriptor menuItemDescriptor, List<Action> actions)
		{
			Argument.NotNull(actions, nameof(actions));

			if (menuItemDescriptor == null)
			{
				return null;
			}

			if (menuItemDescriptor.MenuItems.Any())
			{
				return CreateDropDownToolStripItem(menuItemDescriptor, actions);
			}

			return CreateButtonToolStripItem(menuItemDescriptor, actions);
		}

		#endregion

		#region CreateDropDownToolStripItem

		static ToolStripItem CreateDropDownToolStripItem(IMenuItemDescriptor menuItemDescriptor, List<Action> actions)
		{
			var dropDownItem = new ZToolStripDropDownButton();
			dropDownItem.Name = menuItemDescriptor.Caption;
			dropDownItem.Text = menuItemDescriptor.Caption;
			dropDownItem.Enabled = menuItemDescriptor.IsEnabled();
			dropDownItem.Image = menuItemDescriptor.Image as Image;
			dropDownItem.Visible = menuItemDescriptor.IsVisible();

			dropDownItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;

			foreach (var subMenuItem in menuItemDescriptor.MenuItems)
			{
				var dropDownSubItem = subMenuItem.CreateToolStripItem(actions);

				if (dropDownSubItem != null)
				{
					dropDownItem.DropDownItems.Add(dropDownSubItem);
				}
			}

			actions.Add(() =>
			{
				dropDownItem.Enabled = dropDownItem.DropDownItems.Cast<ToolStripItem>().Any(i => i.Enabled);
			});

			ControlDpiScalingHelper.SetWidth(ref dropDownItem, MeasureString(menuItemDescriptor.Caption, dropDownItem.Font), false);

			return dropDownItem;
		}

		#endregion

		#region CreateButtonToolStripItem

		static ToolStripItem CreateButtonToolStripItem(IMenuItemDescriptor menuItemDescriptor, List<Action> actions)
		{
			var item = new ZToolStripButton();
			item.Name = menuItemDescriptor.Caption;
			item.Text = menuItemDescriptor.Caption;
			item.Enabled = menuItemDescriptor.IsEnabled();
			item.Image = menuItemDescriptor.Image as Image;
			item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
			item.Click += (s, e) => menuItemDescriptor.Invoke();
			item.Visible = menuItemDescriptor.IsVisible();

			actions.Add(() =>
			{
				item.Enabled = menuItemDescriptor.IsEnabled();
				item.Visible = menuItemDescriptor.IsVisible();
			});

			ControlDpiScalingHelper.SetWidth(ref item, MeasureString(menuItemDescriptor.Caption, item.Font), false);

			return item;
		}

		#endregion

		#region MeasureString

		static int MeasureString(string text, Font font)
		{
			using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
			{
				return Convert.ToInt32(graphics.MeasureString(text, font).Width) + 1;
			}
		}

		#endregion
	}
}
