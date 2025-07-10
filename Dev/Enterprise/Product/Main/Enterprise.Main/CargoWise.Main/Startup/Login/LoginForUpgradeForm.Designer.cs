using System.Drawing;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup
{
	public partial class LoginForUpgradeForm : ZChildForm
	{
		protected internal ZGroupBox LoginGroupBox;
		protected internal ZTextBox UserNameTextBox;
		protected internal ZTextBox PasswordTextBox;
		protected internal ZButton Cancel_Button;
		protected internal ZButton OKButton;
		ZPanel LogoPanel;
		ZPanel InfoPanel;
		ZLabel VersionChangeInfoLabel;
		ZLabel UpgradeMessageLabel;
		ZPanel LoginPanel;
		ZPanel MainPanel;
		protected KPictureBox EnterpriseLogoPictureBox;
		internal ZLabel WarningLabel;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.LogoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EnterpriseLogoPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.LoginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.VersionChangeInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UpgradeMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LoginPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LogoPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogoPictureBox)).BeginInit();
			this.LoginGroupBox.SuspendLayout();
			this.InfoPanel.SuspendLayout();
			this.LoginPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 319, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// LogoPanel
			// 
			this.LogoPanel.BackColor = System.Drawing.Color.Transparent;
			this.LogoPanel.Controls.Add(this.EnterpriseLogoPictureBox);
			this.LogoPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LogoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LogoPanel.Name = "LogoPanel";
			this.LogoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 130, true);
			this.LogoPanel.TabIndex = 14;
			// 
			// EnterpriseLogoPictureBox
			// 
			this.EnterpriseLogoPictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.EnterpriseLogoPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.EnterpriseLogoPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 4, true);
			this.EnterpriseLogoPictureBox.Name = "EnterpriseLogoPictureBox";
			this.EnterpriseLogoPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 120, true);
			this.EnterpriseLogoPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
			this.EnterpriseLogoPictureBox.TabIndex = 23;
			this.EnterpriseLogoPictureBox.TabStop = false;
			this.EnterpriseLogoPictureBox.Dock = DockStyle.Fill;

			// 
			// LoginGroupBox
			// 
			this.LoginGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.LoginGroupBox.CaptionResourceString = CargoWise.Main.Res.GetData("C1E1B1FE-5348-44B4-BDB2-741F5B016383", "Login Information");
			this.LoginGroupBox.Controls.Add(this.UserNameTextBox);
			this.LoginGroupBox.Controls.Add(this.PasswordTextBox);
			this.LoginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.LoginGroupBox.Name = "LoginGroupBox";
			this.LoginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 84, true);
			this.LoginGroupBox.TabIndex = 0;
			this.LoginGroupBox.TabStop = false;
			// 
			// UserNameTextBox
			// 
			this.UserNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.UserNameTextBox.CaptionResourceString = CargoWise.Main.Res.GetData("4871126C-2194-4985-A007-DD17745D0511", "Username");
			this.UserNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserNameTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 24, true);
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 21, true);
			this.UserNameTextBox.TabIndex = 1;
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordTextBox.CaptionResourceString = CargoWise.Main.Res.GetData("DB90FA5D-FAFD-4B1E-AEA5-3931E43BF8C1", "Password");
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 48, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 21, true);
			this.PasswordTextBox.TabIndex = 3;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = CargoWise.Main.Res.GetData("7BB0C4C5-8FF5-4105-9325-0FB00E22C3BA", "Cancel Upgrade");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 96, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.ToolTipCaption = null;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = CargoWise.Main.Res.GetData("1D3981A1-6245-4394-BA67-48F2A61575AF", "Deploy Upgrade");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 96, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// InfoPanel
			// 
			this.InfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InfoPanel.BackColor = System.Drawing.Color.White;
			this.InfoPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.InfoPanel.Controls.Add(this.VersionChangeInfoLabel);
			this.InfoPanel.Controls.Add(this.WarningLabel);
			this.InfoPanel.Controls.Add(this.UpgradeMessageLabel);
			this.InfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.InfoPanel.Name = "InfoPanel";
			this.InfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 261, true);
			this.InfoPanel.TabIndex = 20;
			// 
			// VersionChangeInfoLabel
			// 
			this.VersionChangeInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.VersionChangeInfoLabel.CaptionResourceString = CargoWise.Main.Res.GetData("CF841A5A-2B4F-47CB-9E14-8098AC095689", "Version");
			this.VersionChangeInfoLabel.BackColor = System.Drawing.Color.White;
			this.VersionChangeInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VersionChangeInfoLabel.ForeColor = System.Drawing.Color.MediumBlue;
			this.VersionChangeInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 107, true);
			this.VersionChangeInfoLabel.Name = "VersionChangeInfoLabel";
			this.VersionChangeInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 60, true);
			this.VersionChangeInfoLabel.TabIndex = 22;
			this.VersionChangeInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// WarningLabel
			// 
			this.WarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.WarningLabel.BackColor = System.Drawing.Color.White;
			this.WarningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)(((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)
			| Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WarningLabel.ForeColor = System.Drawing.Color.Red;
			this.WarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 170, true);
			this.WarningLabel.Name = "WarningLabel";
			this.WarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 10, true);
			this.WarningLabel.TabIndex = 21;
			this.WarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// UpgradeMessageLabel
			// 
			this.UpgradeMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.UpgradeMessageLabel.BackColor = System.Drawing.Color.White;
			this.UpgradeMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)(((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)
			| Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UpgradeMessageLabel.ForeColor = System.Drawing.Color.MediumBlue;
			this.UpgradeMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.UpgradeMessageLabel.Name = "UpgradeMessageLabel";
			this.UpgradeMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 107, true);
			this.UpgradeMessageLabel.TabIndex = 20;
			this.UpgradeMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LoginPanel
			// 
			this.LoginPanel.Controls.Add(this.LoginGroupBox);
			this.LoginPanel.Controls.Add(this.Cancel_Button);
			this.LoginPanel.Controls.Add(this.OKButton);
			this.LoginPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LoginPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 343, true);
			this.LoginPanel.Name = "LoginPanel";
			this.LoginPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 128, true);
			this.LoginPanel.TabIndex = 21;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.InfoPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 286, true);
			this.MainPanel.TabIndex = 22;
			// 
			// LoginForUpgradeForm
			// 
			this.AcceptButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWise.Main.Res.GetData("A62B6010-FE57-44D0-B11F-7525EFB7A331", "Database Upgrade Login");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 471, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.LoginPanel);
			this.Controls.Add(this.LogoPanel);
			this.MinimizeBox = false;
			this.Name = "LoginForUpgradeForm";
			this.Load += new System.EventHandler(this.LoginForUpgradeForm_Load);
			this.Controls.SetChildIndex(this.LogoPanel, 0);
			this.Controls.SetChildIndex(this.LoginPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LogoPanel.ResumeLayout(false);
			this.LogoPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogoPictureBox)).EndInit();
			this.LoginGroupBox.ResumeLayout(false);
			this.LoginGroupBox.PerformLayout();
			this.InfoPanel.ResumeLayout(false);
			this.InfoPanel.PerformLayout();
			this.LoginPanel.ResumeLayout(false);
			this.LoginPanel.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
