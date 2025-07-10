using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class TranslationSearchControl : ZUserControl
	{
		public TranslationSearchControl()
		{
			InitializeComponent();
			useRegularExpressionsCheckbox.Visible = true;
		}

		void searchSourceCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			this.sourceTextBox.Enabled = searchSourceCheckbox.Checked;
			if (this.sourceTextBox.Enabled)
			{
				this.sourceTextBox.Focus();
			}
		}

		void searchTargetCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			this.replaceCheckbox.Enabled = searchTargetCheckbox.Checked;
			if (!this.replaceCheckbox.Enabled)
			{
				this.replaceCheckbox.Checked = false;
			}

			this.targetTextBox.Enabled = searchTargetCheckbox.Checked;
			if (this.targetTextBox.Enabled)
			{
				this.targetTextBox.Focus();
			}
		}

		void replaceCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			this.replaceTextBox.Enabled = replaceCheckbox.Checked;
			if (this.replaceTextBox.Enabled)
			{
				this.replaceTextBox.Focus();
			}
		}
	}
}
