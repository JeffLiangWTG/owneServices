using Enterprise.ZArchitecture;

namespace Enterprise.Core.Forms
{
	public partial class LicenceErrorForm
	{

		#region Designer generated code

		ZLabel PhoneLabel;
		CargoWise.Windows.UI.KLinkLabel WebsiteLinkLabel;
		ZLabel FaxLabel;
		ZLabel EmailLabel;
		CargoWise.Windows.UI.KLinkLabel EmailLinkLabel;
		ZLabel WebsiteLabel;
		Enterprise.ZArchitecture.GUI.ZButton OKButton;
		ZLabel EDISupportLabel;
		internal ZLabel ModuleNameLabel;
		Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox1;
		internal CargoWise.Windows.UI.KRichTextBox LicenceErrorRichBox;
		System.ComponentModel.IContainer components = null;

		new void InitializeComponent()
		{
			this.PhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WebsiteLinkLabel = new CargoWise.Windows.UI.KLinkLabel();
			this.FaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EmailLinkLabel = new CargoWise.Windows.UI.KLinkLabel();
			this.WebsiteLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EDISupportLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModuleNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.pictureBox1 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.LicenceErrorRichBox = new CargoWise.Windows.UI.KRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 360, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 8, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// PhoneLabel
			// 
			this.PhoneLabel.AutoSize = true;
			this.PhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 243, true);
			this.PhoneLabel.Name = "PhoneLabel";
			this.PhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 13, true);
			this.PhoneLabel.TabIndex = 5;
			this.PhoneLabel.Text = "Phone:       +61 2 8001 2200";
			// 
			// WebsiteLinkLabel
			// 
			this.WebsiteLinkLabel.AutoSize = true;
			this.WebsiteLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 294, true);
			this.WebsiteLinkLabel.Name = "WebsiteLinkLabel";
			this.WebsiteLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 13, true);
			this.WebsiteLinkLabel.TabIndex = 10;
			this.WebsiteLinkLabel.TabStop = true;
			this.WebsiteLinkLabel.Text = "www.cargowise.com";
			this.WebsiteLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WebsiteLinkLabel_LinkClicked);
			// 
			// FaxLabel
			// 
			this.FaxLabel.AutoSize = true;
			this.FaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 259, true);
			this.FaxLabel.Name = "FaxLabel";
			this.FaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 13, true);
			this.FaxLabel.TabIndex = 6;
			this.FaxLabel.Text = "Fax:           +61 2 9025 1199";
			// 
			// EmailLabel
			// 
			this.EmailLabel.AutoSize = true;
			this.EmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 277, true);
			this.EmailLabel.Name = "EmailLabel";
			this.EmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.EmailLabel.TabIndex = 7;
			this.EmailLabel.Text = "Email:";
			// 
			// EmailLinkLabel
			// 
			this.EmailLinkLabel.AutoSize = true;
			this.EmailLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 277, true);
			this.EmailLinkLabel.Name = "EmailLinkLabel";
			this.EmailLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 13, true);
			this.EmailLinkLabel.TabIndex = 8;
			this.EmailLinkLabel.TabStop = true;
			this.EmailLinkLabel.Text = "support@cargowise.com";
			this.EmailLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.EmailLinkLabel_LinkClicked);
			// 
			// WebsiteLabel
			// 
			this.WebsiteLabel.AutoSize = true;
			this.WebsiteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 294, true);
			this.WebsiteLabel.Name = "WebsiteLabel";
			this.WebsiteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.WebsiteLabel.TabIndex = 9;
			this.WebsiteLabel.Text = "Website:";
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("LicenceErrorForm|2e2fd426-fc1a-4b56-b818-f2d801f69829", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 316, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OKButton.TabIndex = 11;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// EDISupportLabel
			// 
			this.EDISupportLabel.AutoSize = true;
			this.EDISupportLabel.IsFontBold = true;
			this.EDISupportLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 215, true);
			this.EDISupportLabel.Name = "SupportLabel";
			this.EDISupportLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 13, true);
			this.EDISupportLabel.TabIndex = 4;
			// 
			// ModuleNameLabel
			// 
			this.ModuleNameLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("LicenceErrorForm|99d528dc-16fc-477d-aa6f-6d0535eb3033", "Module Name");
			this.ModuleNameLabel.IsFontBold = true;
			this.ModuleNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 76, true);
			this.ModuleNameLabel.Name = "ModuleNameLabel";
			this.ModuleNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 15, true);
			this.ModuleNameLabel.TabIndex = 2;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.pictureBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 6, true);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 59, true);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// LicenceErrorRichBox
			// 
			this.LicenceErrorRichBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LicenceErrorRichBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 97, true);
			this.LicenceErrorRichBox.Name = "LicenceErrorRichBox";
			this.LicenceErrorRichBox.ReadOnly = true;
			this.LicenceErrorRichBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 113, true);
			this.LicenceErrorRichBox.TabIndex = 3;
			this.LicenceErrorRichBox.Text = "";
			// 
			// LicenceErrorForm
			// 
			this.AcceptButton = this.OKButton;

			this.CancelButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 368, true);
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("LicenceErrorForm|c6166c04-45b3-4b72-8f9a-4f96c7bf1caf", "Access Denied");
			this.Controls.Add(this.LicenceErrorRichBox);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.ModuleNameLabel);
			this.Controls.Add(this.EDISupportLabel);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.WebsiteLinkLabel);
			this.Controls.Add(this.WebsiteLabel);
			this.Controls.Add(this.EmailLinkLabel);
			this.Controls.Add(this.EmailLabel);
			this.Controls.Add(this.FaxLabel);
			this.Controls.Add(this.PhoneLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "LicenceErrorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PhoneLabel, 0);
			this.Controls.SetChildIndex(this.FaxLabel, 0);
			this.Controls.SetChildIndex(this.EmailLabel, 0);
			this.Controls.SetChildIndex(this.EmailLinkLabel, 0);
			this.Controls.SetChildIndex(this.WebsiteLabel, 0);
			this.Controls.SetChildIndex(this.WebsiteLinkLabel, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.EDISupportLabel, 0);
			this.Controls.SetChildIndex(this.ModuleNameLabel, 0);
			this.Controls.SetChildIndex(this.pictureBox1, 0);
			this.Controls.SetChildIndex(this.LicenceErrorRichBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
