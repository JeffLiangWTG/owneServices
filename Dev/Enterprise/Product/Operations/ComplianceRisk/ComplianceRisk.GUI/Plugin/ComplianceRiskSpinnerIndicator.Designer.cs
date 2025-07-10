namespace Enterprise.ComplianceRisk.GUI
{
	partial class ComplianceRiskSpinnerIndicator
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SpinnerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SpinnerIcon = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SpinnerIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// SpinnerLabel
			// 
			this.SpinnerLabel.AutoSize = true;
			this.SpinnerLabel.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("AD1F4F9C-11BB-48C5-91F1-C0A57F60CEBA", "Risk Check in progress");
			this.SpinnerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SpinnerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 5, true);
			this.SpinnerLabel.Name = "SpinnerLabel";
			this.SpinnerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.SpinnerLabel.TabIndex = 0;
			this.SpinnerLabel.UseMnemonic = false;
			// 
			// SpinnerIcon
			// 
			this.SpinnerIcon.Image = global::Enterprise.ComplianceRisk.GUI.Properties.Resources.loader;
			this.SpinnerIcon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.SpinnerIcon.Name = "SpinnerIcon";
			this.SpinnerIcon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 26, true);
			this.SpinnerIcon.TabIndex = 1;
			this.SpinnerIcon.TabStop = false;
			// 
			// ComplianceRiskSpinnerIndicator
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.Controls.Add(this.SpinnerIcon);
			this.Controls.Add(this.SpinnerLabel);
			this.Name = "ComplianceRiskSpinnerIndicator";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 30, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SpinnerIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel SpinnerLabel;
		private ZArchitecture.GUI.ZPictureBox SpinnerIcon;
	}
}
