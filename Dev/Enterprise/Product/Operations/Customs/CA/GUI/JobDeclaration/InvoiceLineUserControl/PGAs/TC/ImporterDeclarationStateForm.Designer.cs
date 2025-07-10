namespace Enterprise.Customs.CA.GUI
{
	partial class ImporterDeclarationStateForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StateLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 222, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 24, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3e37eae8-e7d8-4ef4-a5ad-ca6913534b9d", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 193, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// TitleLabel
			// 
			this.TitleLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f48a68ad-4cdb-4d13-8b73-582d06c5d214", "Importer Declaration:");
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TitleLabel.IsFontBold = true;
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 39, true);
			this.TitleLabel.TabIndex = 7;
			this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// StateLabel
			// 
			this.StateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.StateLabel.IsFontBold = true;
			this.StateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 56, true);
			this.StateLabel.Name = "StateLabel";
			this.StateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 123, true);
			this.StateLabel.TabIndex = 7;
			this.StateLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ImporterDeclarationStateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e305a0c3-ee90-4009-8a84-13085c6cfd91", "Close");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 246, true);
			this.Controls.Add(this.StateLabel);
			this.Controls.Add(this.TitleLabel);
			this.Controls.Add(this.CloseButton);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 285, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 285, true);
			this.Name = "ImporterDeclarationStateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TitleLabel, 0);
			this.Controls.SetChildIndex(this.StateLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.ZLabel TitleLabel;
		private ZArchitecture.ZLabel StateLabel;
	}
}
