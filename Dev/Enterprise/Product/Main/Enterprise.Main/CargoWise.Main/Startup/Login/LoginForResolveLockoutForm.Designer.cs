using System.Drawing;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup
{
	public partial class LoginForResolveLockoutForm : KForm
	{
		protected internal ZGroupBox LoginGroupBox;
		protected internal CargoWise.Windows.UI.KTextBox UserNameTextBox;
		protected internal CargoWise.Windows.UI.KTextBox PasswordTextBox;
		protected internal ZLabel PasswordLabel;
		protected internal ZLabel UserNameLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		protected internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		ZPanel LogoPanel;
		CargoWise.Windows.UI.KPanel InfoPanel;
		CargoWise.Windows.UI.KLabel MessageLabel;
		Enterprise.ZArchitecture.GUI.ZPanel LoginPanel;
		Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		protected Enterprise.ZArchitecture.GUI.ZPictureBox EnterpriseLogoPictureBox;
		private CargoWise.Windows.UI.KLabel DetailsLabel;
		protected internal KRadioButton radioResetLockout;
		protected internal KRadioButton radioUpgrade;

		void InitializeComponent()
		{
			this.LogoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EnterpriseLogoPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.LoginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.radioUpgrade = new CargoWise.Windows.UI.KRadioButton();
			this.radioResetLockout = new CargoWise.Windows.UI.KRadioButton();
			this.UserNameTextBox = new CargoWise.Windows.UI.KTextBox();
			this.PasswordTextBox = new CargoWise.Windows.UI.KTextBox();
			this.PasswordLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InfoPanel = new CargoWise.Windows.UI.KPanel();
			this.DetailsLabel = new CargoWise.Windows.UI.KLabel();
			this.MessageLabel = new CargoWise.Windows.UI.KLabel();
			this.LoginPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LogoPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogoPictureBox)).BeginInit();
			this.LoginGroupBox.SuspendLayout();
			this.InfoPanel.SuspendLayout();
			this.LoginPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// LogoPanel
			// 
			this.LogoPanel.BackColor = System.Drawing.Color.Transparent;
			this.LogoPanel.Controls.Add(this.EnterpriseLogoPictureBox);
			this.LogoPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.LogoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LogoPanel.Name = "LogoPanel";
			this.LogoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 57, true);
			this.LogoPanel.TabIndex = 14;
			// 
			// EnterpriseLogoPictureBox
			// 
			this.EnterpriseLogoPictureBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.EnterpriseLogoPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.EnterpriseLogoPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 4, true);
			this.EnterpriseLogoPictureBox.Name = "EnterpriseLogoPictureBox";
			this.EnterpriseLogoPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 47, true);
			this.EnterpriseLogoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.EnterpriseLogoPictureBox.TabIndex = 23;
			this.EnterpriseLogoPictureBox.TabStop = false;
			// 
			// LoginGroupBox
			// 
			this.LoginGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LoginGroupBox.Controls.Add(this.radioUpgrade);
			this.LoginGroupBox.Controls.Add(this.radioResetLockout);
			this.LoginGroupBox.Controls.Add(this.UserNameTextBox);
			this.LoginGroupBox.Controls.Add(this.PasswordTextBox);
			this.LoginGroupBox.Controls.Add(this.PasswordLabel);
			this.LoginGroupBox.Controls.Add(this.UserNameLabel);
			this.LoginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.LoginGroupBox.Name = "LoginGroupBox";
			this.LoginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 123, true);
			this.LoginGroupBox.TabIndex = 0;
			this.LoginGroupBox.TabStop = false;
			this.LoginGroupBox.Text = " Login Information ";
			// 
			// radioUpgrade
			// 
			this.radioUpgrade.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioUpgrade.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 99, true);
			this.radioUpgrade.Name = "radioUpgrade";
			this.radioUpgrade.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 24, true);
			this.radioUpgrade.TabIndex = 5;
			this.radioUpgrade.Text = "Keep Lock and Retry Upgrade";
			this.radioUpgrade.UseVisualStyleBackColor = true;
			// 
			// radioResetLockout
			// 
			this.radioResetLockout.Checked = true;
			this.radioResetLockout.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioResetLockout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 76, true);
			this.radioResetLockout.Name = "radioResetLockout";
			this.radioResetLockout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 24, true);
			this.radioResetLockout.TabIndex = 4;
			this.radioResetLockout.TabStop = true;
			this.radioResetLockout.Text = "Reset Lock";
			this.radioResetLockout.UseVisualStyleBackColor = true;
			// 
			// UserNameTextBox
			// 
			this.UserNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.UserNameTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 24, true);
			this.UserNameTextBox.MaxLength = 35;
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 21, true);
			this.UserNameTextBox.TabIndex = 1;
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 48, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 21, true);
			this.PasswordTextBox.TabIndex = 3;
			// 
			// PasswordLabel
			// 
			this.PasswordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.PasswordLabel.Name = "PasswordLabel";
			this.PasswordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 21, true);
			this.PasswordLabel.TabIndex = 2;
			this.PasswordLabel.Text = "Password:";
			// 
			// UserNameLabel
			// 
			this.UserNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.UserNameLabel.Name = "UserNameLabel";
			this.UserNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 21, true);
			this.UserNameLabel.TabIndex = 0;
			this.UserNameLabel.Text = "Username:";
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 129, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.Text = "Cancel";
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 129, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Text = "OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// InfoPanel
			// 
			this.InfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.InfoPanel.BackColor = System.Drawing.Color.White;
			this.InfoPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.InfoPanel.Controls.Add(this.DetailsLabel);
			this.InfoPanel.Controls.Add(this.MessageLabel);
			this.InfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.InfoPanel.Name = "InfoPanel";
			this.InfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 208, true);
			this.InfoPanel.TabIndex = 20;
			// 
			// DetailsLabel
			// 
			this.DetailsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsLabel.BackColor = System.Drawing.Color.White;
			this.DetailsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DetailsLabel.ForeColor = System.Drawing.Color.MediumBlue;
			this.DetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 99, true);
			this.DetailsLabel.Name = "DetailsLabel";
			this.DetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 109, true);
			this.DetailsLabel.TabIndex = 23;
			this.DetailsLabel.Text = "Select the Reset Lock option to allow users to continue using the previous versio" +
				"n.\r\nSelect the Keep Lock and Retry Upgrade option to run the {0} upgrades module" +
				" while keeping applications locked.";
			this.DetailsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MessageLabel
			// 
			this.MessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageLabel.BackColor = System.Drawing.Color.White;
			this.MessageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MessageLabel.ForeColor = System.Drawing.Color.MediumBlue;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 99, true);
			this.MessageLabel.TabIndex = 20;
			this.MessageLabel.Text = "Application access to the database is locked, but no upgrade process is running. " +
				"Please log in as System Administrator to resolve the lock or select cancel.";
			this.MessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LoginPanel
			// 
			this.LoginPanel.Controls.Add(this.LoginGroupBox);
			this.LoginPanel.Controls.Add(this.Cancel_Button);
			this.LoginPanel.Controls.Add(this.OKButton);
			this.LoginPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LoginPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 290, true);
			this.LoginPanel.Name = "LoginPanel";
			this.LoginPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 161, true);
			this.LoginPanel.TabIndex = 21;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.InfoPanel);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 57, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 233, true);
			this.zPanel1.TabIndex = 22;
			// 
			// LoginForResolveLockoutForm
			// 
			this.AcceptButton = this.OKButton;

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 451, true);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.LoginPanel);
			this.Controls.Add(this.LogoPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoginForResolveLockoutForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Unlock Application Access Login";
			this.Load += new System.EventHandler(this.LoginForResolveLockoutForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LogoPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogoPictureBox)).EndInit();
			this.LoginGroupBox.ResumeLayout(false);
			this.LoginGroupBox.PerformLayout();
			this.InfoPanel.ResumeLayout(false);
			this.LoginPanel.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
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
