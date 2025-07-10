using CargoWise.Windows.UI;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	partial class DbSecurityControl
	{
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			CargoWise.Windows.UI.KLabel DbSecurityConfigServerLabel;
			CargoWise.Windows.UI.KLabel SysadminNewPasswordLabel;
			this.SecurityPanel = new CargoWise.Windows.UI.KPanel();
			this.ResetSysadminGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.SysadminNewPasswordTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ExecuteActionButton = new CargoWise.Windows.UI.KButton();
			this.DbSecurityActionComboBox = new CargoWise.Windows.UI.KComboBox();
			this.EnterpriseDbConfigGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.SaPasswordTextBox = new CargoWise.Windows.UI.KTextBox();
			this.DbSecurityDatabaseComboBox = new CargoWise.Windows.UI.KComboBox();
			this.DatabaseLabel = new CargoWise.Windows.UI.KLabel();
			this.RefreshDatabasesButton = new CargoWise.Windows.UI.KButton();
			this.DbSecurityServerTextBox = new CargoWise.Windows.UI.KTextBox();
			this.UseClientsOwnSaPwdCheckBox = new CargoWise.Windows.UI.KCheckBox();
			DbSecurityConfigServerLabel = new CargoWise.Windows.UI.KLabel();
			SysadminNewPasswordLabel = new CargoWise.Windows.UI.KLabel();
			this.SecurityPanel.SuspendLayout();
			this.ResetSysadminGroupBox.SuspendLayout();
			this.EnterpriseDbConfigGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(629, 206, true);
			// 
			// DbSecurityConfigServerLabel
			// 
			DbSecurityConfigServerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 25, true);
			DbSecurityConfigServerLabel.Name = "DbSecurityConfigServerLabel";
			DbSecurityConfigServerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 13, true);
			DbSecurityConfigServerLabel.TabIndex = 12;
			DbSecurityConfigServerLabel.Text = "Server Name (and Instance if applicable) as in the .ini file";
			DbSecurityConfigServerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SysadminNewPasswordLabel
			// 
			SysadminNewPasswordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 14, true);
			SysadminNewPasswordLabel.Name = "SysadminNewPasswordLabel";
			SysadminNewPasswordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 22, true);
			SysadminNewPasswordLabel.TabIndex = 12;
			SysadminNewPasswordLabel.Text = "New Password:";
			SysadminNewPasswordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SecurityPanel.Controls.Add(this.ResetSysadminGroupBox);
			this.SecurityPanel.Controls.Add(this.ExecuteActionButton);
			this.SecurityPanel.Controls.Add(this.DbSecurityActionComboBox);
			this.SecurityPanel.Controls.Add(this.EnterpriseDbConfigGroupBox);
			this.SecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 4, true);
			this.SecurityPanel.Name = "SecurityPanel";
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 346, true);
			this.SecurityPanel.TabIndex = 17;
			// 
			// ResetSysadminGroupBox
			// 
			this.ResetSysadminGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ResetSysadminGroupBox.Controls.Add(this.SysadminNewPasswordTextBox);
			this.ResetSysadminGroupBox.Controls.Add(SysadminNewPasswordLabel);
			this.ResetSysadminGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 258, true);
			this.ResetSysadminGroupBox.Name = "ResetSysadminGroupBox";
			this.ResetSysadminGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 45, true);
			this.ResetSysadminGroupBox.TabIndex = 19;
			this.ResetSysadminGroupBox.TabStop = false;
			this.ResetSysadminGroupBox.Text = "Reactivate and reset password for CW1 sysadmin account";
			this.ResetSysadminGroupBox.Visible = false;
			// 
			// SysadminNewPasswordTextBox
			// 
			this.SysadminNewPasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SysadminNewPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 16, true);
			this.SysadminNewPasswordTextBox.Name = "SysadminNewPasswordTextBox";
			this.SysadminNewPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 20, true);
			this.SysadminNewPasswordTextBox.TabIndex = 18;
			// 
			// ExecuteActionButton
			// 
			this.ExecuteActionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ExecuteActionButton.Enabled = false;
			this.ExecuteActionButton.IsCaptionOverridden = true;
			this.ExecuteActionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 309, true);
			this.ExecuteActionButton.Name = "ExecuteActionButton";
			this.ExecuteActionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 22, true);
			this.ExecuteActionButton.TabIndex = 14;
			this.ExecuteActionButton.Text = "Execute Action";
			this.ExecuteActionButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ExecuteActionButton.ToolTipCaption = null;
			this.ExecuteActionButton.Click += new System.EventHandler(this.ExecuteActionButton_Click);
			// 
			// DbSecurityActionComboBox
			// 
			this.DbSecurityActionComboBox.AllowDrop = true;
			this.DbSecurityActionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbSecurityActionComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.DbSecurityActionComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.DbSecurityActionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DbSecurityActionComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DbSecurityActionComboBox.FormattingEnabled = true;
			this.DbSecurityActionComboBox.Items.AddRange(new object[] {
            "(Please select an action)",
            "Reactivate and reset password for CW1 sysadmin account",
            "Reset all staff's local password",
			});
			this.DbSecurityActionComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 310, true);
			this.DbSecurityActionComboBox.Name = "DbSecurityActionComboBox";
			this.DbSecurityActionComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 21, true);
			this.DbSecurityActionComboBox.TabIndex = 15;
			this.DbSecurityActionComboBox.Tag = "Please Select An Action To Execute";
			this.DbSecurityActionComboBox.SelectedIndexChanged += new System.EventHandler(this.DbSecurityActionComboBox_SelectedIndexChanged);
			// 
			// EnterpriseDbConfigGroupBox
			// 
			this.EnterpriseDbConfigGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EnterpriseDbConfigGroupBox.Controls.Add(this.SaPasswordTextBox);
			this.EnterpriseDbConfigGroupBox.Controls.Add(this.DbSecurityDatabaseComboBox);
			this.EnterpriseDbConfigGroupBox.Controls.Add(this.DatabaseLabel);
			this.EnterpriseDbConfigGroupBox.Controls.Add(this.RefreshDatabasesButton);
			this.EnterpriseDbConfigGroupBox.Controls.Add(this.DbSecurityServerTextBox);
			this.EnterpriseDbConfigGroupBox.Controls.Add(DbSecurityConfigServerLabel);
			this.EnterpriseDbConfigGroupBox.Controls.Add(this.UseClientsOwnSaPwdCheckBox);
			this.EnterpriseDbConfigGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 18, true);
			this.EnterpriseDbConfigGroupBox.Name = "EnterpriseDbConfigGroupBox";
			this.EnterpriseDbConfigGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 129, true);
			this.EnterpriseDbConfigGroupBox.TabIndex = 0;
			this.EnterpriseDbConfigGroupBox.TabStop = false;
			this.EnterpriseDbConfigGroupBox.Text = "Database Server Login";
			// 
			// SaPasswordTextBox
			// 
			this.SaPasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SaPasswordTextBox.Enabled = false;
			this.SaPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 64, true);
			this.SaPasswordTextBox.Name = "SaPasswordTextBox";
			this.SaPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 20, true);
			this.SaPasswordTextBox.TabIndex = 8;
			this.SaPasswordTextBox.UseSystemPasswordChar = true;
			this.SaPasswordTextBox.TextChanged += new System.EventHandler(this.SaPasswordTextBox_TextChanged);
			// 
			// DbSecurityDatabaseComboBox
			// 
			this.DbSecurityDatabaseComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbSecurityDatabaseComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.DbSecurityDatabaseComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.DbSecurityDatabaseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DbSecurityDatabaseComboBox.Enabled = false;
			this.DbSecurityDatabaseComboBox.FormattingEnabled = true;
			this.DbSecurityDatabaseComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 100, true);
			this.DbSecurityDatabaseComboBox.Name = "DbSecurityDatabaseComboBox";
			this.DbSecurityDatabaseComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 21, true);
			this.DbSecurityDatabaseComboBox.TabIndex = 16;
			this.DbSecurityDatabaseComboBox.SelectedIndexChanged += new System.EventHandler(this.DbSecurityDatabaseComboBox_SelectedIndexChanged);
			// 
			// DatabaseLabel
			// 
			this.DatabaseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 83, true);
			this.DatabaseLabel.Name = "DatabaseLabel";
			this.DatabaseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 13, true);
			this.DatabaseLabel.TabIndex = 17;
			this.DatabaseLabel.Text = "Database Name";
			this.DatabaseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// RefreshDatabasesButton
			// 
			this.RefreshDatabasesButton.Enabled = false;
			this.RefreshDatabasesButton.IsCaptionOverridden = true;
			this.RefreshDatabasesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 98, true);
			this.RefreshDatabasesButton.Name = "RefreshDatabasesButton";
			this.RefreshDatabasesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 21, true);
			this.RefreshDatabasesButton.TabIndex = 15;
			this.RefreshDatabasesButton.Text = "Refresh DBs";
			this.RefreshDatabasesButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RefreshDatabasesButton.ToolTipCaption = null;
			this.RefreshDatabasesButton.Click += new System.EventHandler(this.RefreshDatabasesButton_Click);
			// 
			// DbSecurityServerTextBox
			// 
			this.DbSecurityServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbSecurityServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 42, true);
			this.DbSecurityServerTextBox.Name = "DbSecurityServerTextBox";
			this.DbSecurityServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 20, true);
			this.DbSecurityServerTextBox.TabIndex = 13;
			this.DbSecurityServerTextBox.TextChanged += new System.EventHandler(this.DBScecurityConfigServerTextBox_TextChanged);
			this.DbSecurityServerTextBox.Leave += new System.EventHandler(this.DbSecurityServerTextBox_Leave);
			// 
			// UseClientsOwnSaPwdCheckBox
			// 
			this.UseClientsOwnSaPwdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.UseClientsOwnSaPwdCheckBox.Name = "UseClientsOwnSaPwdCheckBox";
			this.UseClientsOwnSaPwdCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 17, true);
			this.UseClientsOwnSaPwdCheckBox.TabIndex = 11;
			this.UseClientsOwnSaPwdCheckBox.Text = "I want to use my own \'sa\' password";
			this.UseClientsOwnSaPwdCheckBox.UseVisualStyleBackColor = true;
			this.UseClientsOwnSaPwdCheckBox.CheckedChanged += new System.EventHandler(this.UseClientsOwnSaPwdCheckBox_CheckedChanged);
			// 
			// DbSecurityControl
			// 
			this.Controls.Add(this.SecurityPanel);
			this.Name = "DbSecurityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 641, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			this.ResetSysadminGroupBox.ResumeLayout(false);
			this.ResetSysadminGroupBox.PerformLayout();
			this.EnterpriseDbConfigGroupBox.ResumeLayout(false);
			this.EnterpriseDbConfigGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KPanel SecurityPanel;
		private KButton ExecuteActionButton;
		private KComboBox DbSecurityActionComboBox;
		private KGroupBox EnterpriseDbConfigGroupBox;
		private KTextBox DbSecurityServerTextBox;
		private KTextBox SaPasswordTextBox;
		private KCheckBox UseClientsOwnSaPwdCheckBox;
		private KButton RefreshDatabasesButton;
		private KComboBox DbSecurityDatabaseComboBox;
		private KLabel DatabaseLabel;
		private KGroupBox ResetSysadminGroupBox;
		private KTextBox SysadminNewPasswordTextBox;
	}
}
