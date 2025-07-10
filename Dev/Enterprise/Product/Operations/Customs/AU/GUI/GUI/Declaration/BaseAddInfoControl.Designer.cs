using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class BaseAddInfoControl
	{
		private void InitializeComponent()
		{
			this.AddInfoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddInfoTextBox = new ZTextBox.Bare();
			this.SuspendLayout();
			// 
			// AddInfoButton
			// 
			this.AddInfoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddInfoButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.AddInfoButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AddInfoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 0, true);
			this.AddInfoButton.Name = "AddInfoButton";
			this.AddInfoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.AddInfoButton.TabIndex = 1;
			this.AddInfoButton.Text = "...";
			// 
			// AddInfoTextBox
			// 
			this.AddInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.AddInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddInfoTextBox.Name = "AddInfoTextBox";
			this.AddInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.AddInfoTextBox.TabIndex = 0;
			this.AddInfoTextBox.Text = "";
			// 
			// BaseAddInfoControl
			// 
			this.AllowDrop = true;
			this.Controls.Add(this.AddInfoTextBox);
			this.Controls.Add(this.AddInfoButton);
			this.Name = "BaseAddInfoControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.ResumeLayout(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				AddInfoTextBox.ReadOnlyChanged -= new EventHandler(AddInfoTextBox_ReadOnlyChanged);
			}
			base.Dispose(disposing);
		}

		protected internal ZButton AddInfoButton;
		protected internal ZTextBox AddInfoTextBox;
	}
}
