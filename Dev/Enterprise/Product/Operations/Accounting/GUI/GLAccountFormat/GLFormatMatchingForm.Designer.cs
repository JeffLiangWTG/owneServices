using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GLAccountFormat
{
	public partial class GLFormatMatchingForm
	{
		System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OKButton = new ZButton();
			this.MyCancelButton = new ZButton();
			this.label1 = new ZLabel();
			this.label2 = new ZLabel();
			this.ResetButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Enabled = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 248, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Text = "OK";
			// 
			// MyCancelButton
			// 
			this.MyCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MyCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.MyCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 248, true);
			this.MyCancelButton.Name = "MyCancelButton";
			this.MyCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.MyCancelButton.TabIndex = 2;
			this.MyCancelButton.Text = "Cancel";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.label1.TabIndex = 3;
			this.label1.Text = "Old format:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 176, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			this.label2.TabIndex = 4;
			this.label2.Text = "New format:";
			// 
			// ResetButton
			// 
			this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 248, true);
			this.ResetButton.Name = "ResetButton";
			this.ResetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ResetButton.TabIndex = 5;
			this.ResetButton.Text = "Reset";
			this.ResetButton.Click += new EventHandler(this.ResetButton_Click);
			// 
			// GLFormatMatchingForm
			// 
			this.AcceptButton = this.OKButton;

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 277, true);
			this.Controls.Add(this.ResetButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.MyCancelButton);
			this.Controls.Add(this.OKButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GLFormatMatchingForm";
			this.Text = "GL Account format change";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
