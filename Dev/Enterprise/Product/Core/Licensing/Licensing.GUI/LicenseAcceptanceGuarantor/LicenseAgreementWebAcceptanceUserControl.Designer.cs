namespace Enterprise.Licensing.GUI
{
	partial class LicenseAgreementWebAcceptanceUserControl
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.recheckStatusButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.exitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.titleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.linkInstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.noteLabel = new Enterprise.ZArchitecture.ZLabel();
			this.agreementLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Licensing.LicenseAgreement);
			// 
			// recheckStatusButton
			// 
			this.recheckStatusButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.recheckStatusButton.CaptionResourceString = Enterprise.Licensing.GUI.Res.GetData("9c5753cb-d513-4238-b4fc-c4747544461c", "Recheck License Status");
			this.recheckStatusButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 216, true);
			this.recheckStatusButton.Name = "recheckStatusButton";
			this.recheckStatusButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 32, true);
			this.recheckStatusButton.TabIndex = 6;
			this.recheckStatusButton.ToolTipCaption = null;
			this.recheckStatusButton.UseVisualStyleBackColor = true;
			this.recheckStatusButton.Click += new System.EventHandler(this.RecheckLicenseStatus_Click);
			// 
			// exitButton
			// 
			this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.exitButton.CaptionResourceString = Enterprise.Licensing.GUI.Res.GetData("324a4fbe-7f1e-412f-a28a-569587176e48", "Exit");
			this.exitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 216, true);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 32, true);
			this.exitButton.TabIndex = 4;
			this.exitButton.ToolTipCaption = null;
			this.exitButton.UseVisualStyleBackColor = true;
			this.exitButton.Click += new System.EventHandler(this.ExitButton_Click);
			// 
			// titleLabel
			// 
			this.titleLabel.CaptionResourceString = Enterprise.Licensing.GUI.Res.GetData("6f1c55a6-5fbe-43fb-903b-26edb8580ed0", "CargoWise Next - License Agreement Required");
			this.titleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.titleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 23, true);
			this.titleLabel.TabIndex = 0;
			this.titleLabel.UseMnemonic = false;
			// 
			// linkInstructionsLabel
			// 
			this.linkInstructionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.linkInstructionsLabel.CaptionResourceString = Enterprise.Licensing.GUI.Res.GetData("42eb3597-49de-4acf-8ccb-278b9cf3be9e", "Usage of this software requires the signing of a License Agreement\r\n\r\nYou can rev" +
        "iew and accept the license agreement by visiting the following link:");
			this.linkInstructionsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.linkInstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 41, true);
			this.linkInstructionsLabel.Name = "linkInstructionsLabel";
			this.linkInstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 47, true);
			this.linkInstructionsLabel.TabIndex = 7;
			this.linkInstructionsLabel.UseMnemonic = false;
			// 
			// noteLabel
			// 
			this.noteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.noteLabel.CaptionResourceString = Enterprise.Licensing.GUI.Res.GetData("9f1ca0f9-ceed-410c-9561-d4f51d1ce906", "Note: This link is unique to your organization and allows signing of the agreemen" +
        "t.\r\n\r\nOnce accepted online, you can recheck your license status to continue.");
			this.noteLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.noteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 114, true);
			this.noteLabel.Name = "noteLabel";
			this.noteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 48, true);
			this.noteLabel.TabIndex = 8;
			this.noteLabel.UseMnemonic = false;
			// 
			// agreementLinkLabel
			// 
			this.agreementLinkLabel.AutoSize = true;
			this.agreementLinkLabel.IsFontBold = false;
			this.agreementLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 93, true);
			this.agreementLinkLabel.Name = "agreementLinkLabel";
			this.agreementLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 13, true);
			this.agreementLinkLabel.TabIndex = 9;
			this.agreementLinkLabel.Text = "agreementLinkLabel";
			this.agreementLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.AgreementLinkLabel_LinkClicked);
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.exitButton);
			this.mainPanel.Controls.Add(this.titleLabel);
			this.mainPanel.Controls.Add(this.noteLabel);
			this.mainPanel.Controls.Add(this.agreementLinkLabel);
			this.mainPanel.Controls.Add(this.linkInstructionsLabel);
			this.mainPanel.Controls.Add(this.recheckStatusButton);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 252, true);
			this.mainPanel.TabIndex = 10;
			// 
			// LicenseAgreementWebAcceptanceUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainPanel);
			this.Name = "LicenseAgreementWebAcceptanceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 252, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZButton recheckStatusButton;
		private ZArchitecture.GUI.ZButton exitButton;
		private ZArchitecture.ZLabel titleLabel;
		private ZArchitecture.ZLabel linkInstructionsLabel;
		private ZArchitecture.ZLabel noteLabel;
		private ZArchitecture.GUI.ZLinkLabel agreementLinkLabel;
		private ZArchitecture.GUI.ZPanel mainPanel;
	}
}
