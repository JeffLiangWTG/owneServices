namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	partial class MainForm
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
		private void InitializeComponent()
		{
			this.DbBackupAndRestoreTabControl = new CargoWise.Windows.UI.KTabControl();
			this.BackupTabPage = new CargoWise.Windows.UI.KTabPage();
			this.DbBackupControl = new Enterprise.DataTools.DbBackupAndRestore.GUI.DbBackupControl();
			this.RestoreTabPage = new CargoWise.Windows.UI.KTabPage();
			this.DbRestoreControl = new Enterprise.DataTools.DbBackupAndRestore.GUI.DbRestoreControl();
			this.MaintenanceTabPage = new CargoWise.Windows.UI.KTabPage();
			this.DbMaintenanceControl = new Enterprise.DataTools.DbBackupAndRestore.GUI.DBMaintenanceControl(new MessageBoxHelper());
			this.SecurityTabPage = new CargoWise.Windows.UI.KTabPage();
			this.DbSecurityControl = new Enterprise.DataTools.DbBackupAndRestore.GUI.DbSecurityControl();
			this.DbBackupAndRestoreTabControl.SuspendLayout();
			this.BackupTabPage.SuspendLayout();
			this.RestoreTabPage.SuspendLayout();
			this.MaintenanceTabPage.SuspendLayout();
			this.SecurityTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// DbBackupAndRestoreTabControl
			// 
			this.DbBackupAndRestoreTabControl.Controls.Add(this.BackupTabPage);
			this.DbBackupAndRestoreTabControl.Controls.Add(this.RestoreTabPage);
			this.DbBackupAndRestoreTabControl.Controls.Add(this.MaintenanceTabPage);
			this.DbBackupAndRestoreTabControl.Controls.Add(this.SecurityTabPage);
			this.DbBackupAndRestoreTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DbBackupAndRestoreTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DbBackupAndRestoreTabControl.Name = "DbBackupAndRestoreTabControl";
			this.DbBackupAndRestoreTabControl.SelectedIndex = 0;
			this.DbBackupAndRestoreTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 664, true);
			this.DbBackupAndRestoreTabControl.TabIndex = 1;
			// 
			// BackupTabPage
			// 
			this.BackupTabPage.Controls.Add(this.DbBackupControl);
			this.BackupTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.BackupTabPage.Name = "BackupTabPage";
			this.BackupTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 638, true);
			this.BackupTabPage.TabIndex = 0;
			this.BackupTabPage.Text = "Backup";
			this.BackupTabPage.UseVisualStyleBackColor = true;
			// 
			// DbBackupControl
			// 
			this.DbBackupControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbBackupControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.DbBackupControl.Name = "DbBackupControl";
			this.DbBackupControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 638, true);
			this.DbBackupControl.TabIndex = 0;
			// 
			// RestoreTabPage
			// 
			this.RestoreTabPage.Controls.Add(this.DbRestoreControl);
			this.RestoreTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.RestoreTabPage.Name = "RestoreTabPage";
			this.RestoreTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 638, true);
			this.RestoreTabPage.TabIndex = 1;
			this.RestoreTabPage.Text = "Restore";
			this.RestoreTabPage.UseVisualStyleBackColor = true;
			this.RestoreTabPage.Visible = false;
			// 
			// DbRestoreControl
			// 
			this.DbRestoreControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.DbRestoreControl.Name = "DbRestoreControl";
			this.DbRestoreControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 638, true);
			this.DbRestoreControl.TabIndex = 0;
			// 
			// MaintenanceTabPage
			// 
			this.MaintenanceTabPage.Controls.Add(this.DbMaintenanceControl);
			this.MaintenanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.MaintenanceTabPage.Name = "MaintenanceTabPage";
			this.MaintenanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 638, true);
			this.MaintenanceTabPage.TabIndex = 2;
			this.MaintenanceTabPage.Text = "Remove";
			this.MaintenanceTabPage.UseVisualStyleBackColor = true;
			this.MaintenanceTabPage.Visible = false;
			// 
			// DbMaintenanceControl
			// 
			this.DbMaintenanceControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbMaintenanceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.DbMaintenanceControl.Name = "DbMaintenanceControl";
			this.DbMaintenanceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 638, true);
			this.DbMaintenanceControl.TabIndex = 0;
			// 
			// SecurityTabPage
			// 
			this.SecurityTabPage.Controls.Add(this.DbSecurityControl);
			this.SecurityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.SecurityTabPage.Name = "SecurityTabPage";
			this.SecurityTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SecurityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 638, true);
			this.SecurityTabPage.TabIndex = 3;
			this.SecurityTabPage.Text = "Security Configuration";
			this.SecurityTabPage.UseVisualStyleBackColor = true;
			// 
			// DbSecurityControl
			// 
			this.DbSecurityControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbSecurityControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.DbSecurityControl.Name = "DbSecurityControl";
			this.DbSecurityControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 638, true);
			this.DbSecurityControl.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 664, true);
			this.Controls.Add(this.DbBackupAndRestoreTabControl);
			this.MaximizeBox = false;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 800, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 600, true);
			this.Name = "MainForm";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.DbBackupAndRestoreTabControl.ResumeLayout(false);
			this.BackupTabPage.ResumeLayout(false);
			this.RestoreTabPage.ResumeLayout(false);
			this.MaintenanceTabPage.ResumeLayout(false);
			this.SecurityTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KTabControl DbBackupAndRestoreTabControl;
		private CargoWise.Windows.UI.KTabPage BackupTabPage;
		private CargoWise.Windows.UI.KTabPage RestoreTabPage;
		private CargoWise.Windows.UI.KTabPage SecurityTabPage;
		private CargoWise.Windows.UI.KTabPage MaintenanceTabPage;
		private DbBackupControl DbBackupControl;
		private DbRestoreControl DbRestoreControl;
		private DbSecurityControl DbSecurityControl;
		private DBMaintenanceControl DbMaintenanceControl;
	}
}
