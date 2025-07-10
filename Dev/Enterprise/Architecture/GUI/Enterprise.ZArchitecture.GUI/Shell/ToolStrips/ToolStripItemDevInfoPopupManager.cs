using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	class ToolStripItemDevInfoPopupManager
	{
		public ToolStripItemDevInfoPopupManager(ToolStripItem item)
		{
			this.item = item;
			item.MouseMove += new MouseEventHandler(item_MouseMove);
			item.MouseLeave += new EventHandler(item_MouseLeave);
		}

		void item_MouseMove(object sender, MouseEventArgs e)
		{
			if (devInfoPopupActive || DevInfoPopupManager.InDevelopInformationMode())
			{
				item.Invalidate();
				item.Owner.Update();
			}
			if (DevInfoPopupManager.InDevelopInformationMode())
			{
				devInfoPopupActive = true;
				DevInfoPopupManager.PaintCaptionHighlight(item.Owner, item.Bounds);
			}
			else
			{
				devInfoPopupActive = false;
			}
		}

		void item_MouseLeave(object sender, EventArgs e)
		{
			if (devInfoPopupActive)
			{
				item.Invalidate();
				item.Owner.Update();
				devInfoPopupActive = false;
			}
		}

		public void ShowDevelopInfoForm()
		{
			DevInfoPopupManager.ShowDevelopInfoForm(item.Owner);
		}

		readonly ToolStripItem item;
		bool devInfoPopupActive;
	}
}
