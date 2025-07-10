using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Startup.Login
{
	partial class LoginUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		void InitializeComponent()
		{
			this.loginLabel = new Enterprise.ZArchitecture.ZHeaderLabel();
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.twoFactorAuthenticationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.passwordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.userNameTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.oidcLabel = new Enterprise.ZArchitecture.ZLabel();
			this.errorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.panel2 = new CargoWise.Windows.UI.KPanel();
			this.ADLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.ADLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// loginLabel
			// 
			this.loginLabel.AutoSize = true;
			this.loginLabel.CaptionResourceString = CargoWise.Main.Res.GetData("60f27283-ed9d-4ebd-b16e-c9df706de39c", "Login");
			this.loginLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.loginLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(52)))), ((int)(((byte)(121)))));
			this.loginLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.loginLabel.Name = "loginLabel";
			this.loginLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 19, true);
			this.loginLabel.TabIndex = 0;
			// 
			// toolStrip
			// 
			this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 143, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 25, true);
			this.toolStrip.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.twoFactorAuthenticationTextBox);
			this.panel1.Controls.Add(this.passwordTextBox);
			this.panel1.Controls.Add(this.userNameTextbox);
			this.panel1.Controls.Add(this.toolStrip);
			this.panel1.Controls.Add(this.oidcLabel);
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 44, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 174, true);
			this.panel1.TabIndex = 5;
			// 
			// twoFactorAuthenticationTextBox
			// 
			this.twoFactorAuthenticationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.twoFactorAuthenticationTextBox.CaptionResourceString = CargoWise.Main.Res.GetData("f7fe62ef-0b0b-43cf-96e9-c11959a07474", "Verification Code");
			this.twoFactorAuthenticationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.twoFactorAuthenticationTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.twoFactorAuthenticationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 125, true);
			this.twoFactorAuthenticationTextBox.Name = "twoFactorAuthenticationTextBox";
			this.twoFactorAuthenticationTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.twoFactorAuthenticationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.twoFactorAuthenticationTextBox.TabIndex = 5;
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.passwordTextBox.CaptionResourceString = CargoWise.Main.Res.GetData("d81aec8b-d076-4fe2-a747-6ca8fed34cd1", "Password");
			this.passwordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.passwordTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.passwordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 74, true);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.PasswordChar = '*';
			this.passwordTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.passwordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.passwordTextBox.TabIndex = 4;
			// 
			// userNameTextbox
			// 
			this.userNameTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.userNameTextbox.CaptionResourceString = CargoWise.Main.Res.GetData("624c2be0-493f-4e4e-93f7-74eef1bfac19", "Username");
			this.userNameTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.userNameTextbox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.userNameTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.userNameTextbox.Name = "userNameTextbox";
			this.userNameTextbox.ShouldEscapeAllSpecialCharacters = false;
			this.userNameTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.userNameTextbox.TabIndex = 3;
			// 
			// oidcLabel
			// 
			this.oidcLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.oidcLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Largest;
			this.oidcLabel.ForeColor = System.Drawing.SystemColors.Highlight;
			this.oidcLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.oidcLabel.Name = "oidcLabel";
			this.oidcLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 172, true);
			this.oidcLabel.TabIndex = 9;
			this.oidcLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// errorLabel
			// 
			this.errorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.errorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.errorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(0)))), ((int)(((byte)(5)))));
			this.errorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 223, true);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 98, true);
			this.errorLabel.TabIndex = 6;
			this.errorLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.Controls.Add(this.ADLinkLabel);
			this.panel2.Controls.Add(this.ADLabel);
			this.panel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 295, true);
			this.panel2.Name = "panel2";
			this.panel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 138, true);
			this.panel2.TabIndex = 7;
			// 
			// ADLinkLabel
			// 
			this.ADLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ADLinkLabel.CaptionResourceString = CargoWise.Main.Res.GetData("467db157-2c0f-453a-8100-d50e1ce03040", "More Info...");
			this.ADLinkLabel.IsFontBold = true;
			this.ADLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 104, true);
			this.ADLinkLabel.Name = "ADLinkLabel";
			this.ADLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 24, true);
			this.ADLinkLabel.TabIndex = 5;
			this.ADLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ADLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ADLinkLabel_LinkClicked);
			// 
			// ADLabel
			// 
			this.ADLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ADLabel.CaptionResourceString = CargoWise.Main.Res.GetData("58a627de-94b6-4b60-91cb-d232dfd14393", "Did you know that this Product now integrates with Active Directory?\r\n\r\nIf you enable this feature, you will automatically be logged into your account every time! Click the below link for more information.");
			this.ADLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ADLabel.IsFontBold = true;
			this.ADLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 5, true);
			this.ADLabel.Name = "ADLabel";
			this.ADLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 99, true);
			this.ADLabel.TabIndex = 4;
			this.ADLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// PictureBox
			// 
			this.PictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 475, true);
			this.PictureBox.Name = "PictureBox";
			this.PictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 94, true);
			this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.PictureBox.TabIndex = 8;
			this.PictureBox.TabStop = false;
			// 
			// LoginUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PictureBox);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.loginLabel);
			this.Name = "LoginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 572, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZHeaderLabel loginLabel;
		private CargoWise.Windows.UI.KPanel panel1;
		internal ZArchitecture.ZTextBox passwordTextBox;
		internal ZArchitecture.ZTextBox userNameTextbox;
		internal ZArchitecture.ZLabel errorLabel;
		private CargoWise.Windows.UI.KPanel panel2;
		internal ZArchitecture.ZLabel ADLabel;
		private ZArchitecture.GUI.ZLinkLabel ADLinkLabel;
		private Enterprise.ZArchitecture.GUI.ZPictureBox PictureBox;
		internal ZToolStrip toolStrip;
		internal ZArchitecture.ZTextBox twoFactorAuthenticationTextBox;
		internal ZArchitecture.ZLabel oidcLabel;
	}
}
