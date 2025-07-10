using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public class BoardPickerButton : ZButton
	{
		public BoardPickerButton()
		{
			BackgroundImage = Properties.Resources.visual_boards;
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (ContextMenuStrip == null || cachedMenuItems == null)
			{
				ContextMenuStrip = new ContextMenuStrip();
				ContextMenuStrip.BackColor = SystemColors.Control;

				var provider = ObjectFactory.Get<IVisualBoardMenuItemProvider>("VisualBoardMenuItemProvider");
				cachedMenuItems = provider.GetMenuItems().ToArray();

				foreach (var item in cachedMenuItems)
				{
					var result = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(item);
					ContextMenuStrip.Items.Add(result);
				}
			}

			ContextMenuStrip.Show(this, new Point(0, this.Height + ControlDpiScalingHelper.ScaleToCurrentDpiX(1)));
		}

		MenuItem[] cachedMenuItems;

		internal void ClearMenuItemCache()
		{
			if (cachedMenuItems != null)
			{
				foreach (var item in cachedMenuItems)
				{
					item.Dispose();
				}

				cachedMenuItems = null;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				ContextMenuStrip?.Dispose();
				ClearMenuItemCache();
			}
		}
	}
}
