using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	class ToolStripItemTranslationFeedbackManager
	{
		public ToolStripItemTranslationFeedbackManager(ToolStripItem item)
		{
			this.item = item;
			item.MouseMove += new MouseEventHandler(item_MouseMove);
			item.MouseLeave += new EventHandler(item_MouseLeave);
		}

		void item_MouseMove(object sender, MouseEventArgs e)
		{
			if (translationFeedbackActive || TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				item.Invalidate();
				item.Owner.Update();
			}
			if (TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				translationFeedbackActive = true;
				TranslationFeedbackManager.PaintCaptionHighlight(item.Owner, item.Bounds);
			}
			else
			{
				translationFeedbackActive = false;
			}
		}

		void item_MouseLeave(object sender, EventArgs e)
		{
			if (translationFeedbackActive)
			{
				item.Invalidate();
				item.Owner.Update();
				translationFeedbackActive = false;
			}
		}

		public void OpenFeedbackForm()
		{
			var resCaptionedItem = item as IResCaptionedControl;
			if (resCaptionedItem != null && resCaptionedItem.CaptionResourceString != null && !resCaptionedItem.CaptionResourceString.IsEmpty())
			{
				TranslationFeedbackManager.OpenFeedbackForm(item.Owner, resCaptionedItem.CaptionResourceString, item.Text.Trim(), true);
			}
			else
			{
				var convertedFromMenuItem = item as IConvertedFromMenuItem;
				if (convertedFromMenuItem != null && convertedFromMenuItem.SourceMenuItem != null && convertedFromMenuItem.SourceMenuItem is ZMenuItem &&
					(convertedFromMenuItem.SourceMenuItem.Text == item.Text || KMenuItem.StripAcceleratorKeys(convertedFromMenuItem.SourceMenuItem.Text) == item.Text))
				{
					((ZMenuItem)convertedFromMenuItem.SourceMenuItem).OpenFeedbackForm(item.Owner);
				}
				else
				{
					TranslationFeedbackManager.OpenFeedbackForm(item.Owner, item.Text.Trim());
				}
			}
		}

		readonly ToolStripItem item;
		bool translationFeedbackActive;
	}
}
