using CargoWise.Windows.UI;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	partial class DbRestoreControl
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
			this.RestorePanel = new CargoWise.Windows.UI.KPanel();
			this.RefreshAvailabilityGroupsButton = new CargoWise.Windows.UI.KButton();
			this.AvailabilityGroupLabel = new CargoWise.Windows.UI.KLabel();
			this.AddDbToAvailabilityGroupCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.EdwRestoreBrowseButton = new CargoWise.Windows.UI.KButton();
			this.EdwDbRestoreFilePathTextBox = new CargoWise.Windows.UI.KTextBox();
			this.EdwBackupFileLabel = new CargoWise.Windows.UI.KLabel();
			this.AuditRestoreBrowseButton = new CargoWise.Windows.UI.KButton();
			this.AuditDbRestoreFilePathTextBox = new CargoWise.Windows.UI.KTextBox();
			this.AuditBackupFileLabel = new CargoWise.Windows.UI.KLabel();
			this.DbRestoreAuditServer = new CargoWise.Windows.UI.KLabel();
			this.DbRestoreAuditServerTextBox = new CargoWise.Windows.UI.KTextBox();
			this.DbRestoreDwServerTextBox = new CargoWise.Windows.UI.KTextBox();
			this.DbRestoreDwServer = new CargoWise.Windows.UI.KLabel();
			this.RestoreBiDatabasesCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.TransactionLogBackupsCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.DifferentialBackupsCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.DbRestoreOptionComboBox = new CargoWise.Windows.UI.KComboBox();
			this.RestoreOperationalDatabasesCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.DbRestoreRelativeToServerTextBox = new CargoWise.Windows.UI.KTextBox();
			this.RestoreReferenceFilesDatabasesCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.DbRestoreDatabaseNameTextBox = new CargoWise.Windows.UI.KTextBox();
			this.RestoreBrowseButton = new CargoWise.Windows.UI.KButton();
			this.DbRestoreListView = new CargoWise.Windows.UI.KDataGridView();
			this.LogicalName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Folder = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.DbRestoreExamineFileButton = new CargoWise.Windows.UI.KButton();
			this.DbRestoreButton = new CargoWise.Windows.UI.KButton();
			this.DbRestoreFilePathTextBox = new CargoWise.Windows.UI.KTextBox();
			this.DbRestoreFilePathLabel = new CargoWise.Windows.UI.KLabel();
			this.DbRestoreDatabaseNameLabel = new CargoWise.Windows.UI.KLabel();
			this.DbRestoreServerTextBox = new CargoWise.Windows.UI.KTextBox();
			this.DbRestoreServerLabel = new CargoWise.Windows.UI.KLabel();
			this.AvailabilityGroupComboBox = new CargoWise.Windows.UI.KComboBox();
			this.RestorePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DbRestoreListView)).BeginInit();
			this.DbRestoreListView.SuspendLayout();
			this.SuspendLayout();
			// 
			// RestorePanel
			// 
			this.RestorePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RestorePanel.Controls.Add(this.RefreshAvailabilityGroupsButton);
			this.RestorePanel.Controls.Add(this.AvailabilityGroupLabel);
			this.RestorePanel.Controls.Add(this.AddDbToAvailabilityGroupCheckBox);
			this.RestorePanel.Controls.Add(this.EdwRestoreBrowseButton);
			this.RestorePanel.Controls.Add(this.EdwDbRestoreFilePathTextBox);
			this.RestorePanel.Controls.Add(this.EdwBackupFileLabel);
			this.RestorePanel.Controls.Add(this.AuditRestoreBrowseButton);
			this.RestorePanel.Controls.Add(this.AuditDbRestoreFilePathTextBox);
			this.RestorePanel.Controls.Add(this.AuditBackupFileLabel);
			this.RestorePanel.Controls.Add(this.DbRestoreAuditServer);
			this.RestorePanel.Controls.Add(this.DbRestoreAuditServerTextBox);
			this.RestorePanel.Controls.Add(this.DbRestoreDwServerTextBox);
			this.RestorePanel.Controls.Add(this.DbRestoreDwServer);
			this.RestorePanel.Controls.Add(this.RestoreBiDatabasesCheckBox);
			this.RestorePanel.Controls.Add(this.TransactionLogBackupsCheckBox);
			this.RestorePanel.Controls.Add(this.DifferentialBackupsCheckBox);
			this.RestorePanel.Controls.Add(this.DbRestoreOptionComboBox);
			this.RestorePanel.Controls.Add(this.RestoreOperationalDatabasesCheckBox);
			this.RestorePanel.Controls.Add(this.DbRestoreRelativeToServerTextBox);
			this.RestorePanel.Controls.Add(this.RestoreReferenceFilesDatabasesCheckBox);
			this.RestorePanel.Controls.Add(this.DbRestoreDatabaseNameTextBox);
			this.RestorePanel.Controls.Add(this.RestoreBrowseButton);
			this.RestorePanel.Controls.Add(this.DbRestoreListView);
			this.RestorePanel.Controls.Add(this.DbRestoreExamineFileButton);
			this.RestorePanel.Controls.Add(this.DbRestoreButton);
			this.RestorePanel.Controls.Add(this.DbRestoreFilePathTextBox);
			this.RestorePanel.Controls.Add(this.DbRestoreFilePathLabel);
			this.RestorePanel.Controls.Add(this.DbRestoreDatabaseNameLabel);
			this.RestorePanel.Controls.Add(this.DbRestoreServerTextBox);
			this.RestorePanel.Controls.Add(this.DbRestoreServerLabel);
			this.RestorePanel.Controls.Add(this.AvailabilityGroupComboBox);
			this.RestorePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.RestorePanel.Name = "RestorePanel";
			this.RestorePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 345, true);
			this.RestorePanel.TabIndex = 15;
			// 
			// RefreshAvailabilityGroupsButton
			// 
			this.RefreshAvailabilityGroupsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RefreshAvailabilityGroupsButton.Enabled = false;
			this.RefreshAvailabilityGroupsButton.IsCaptionOverridden = true;
			this.RefreshAvailabilityGroupsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 11, true);
			this.RefreshAvailabilityGroupsButton.Name = "RefreshAvailabilityGroupsButton";
			this.RefreshAvailabilityGroupsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 22, true);
			this.RefreshAvailabilityGroupsButton.TabIndex = 16;
			this.RefreshAvailabilityGroupsButton.Text = "Refresh AGs";
			this.RefreshAvailabilityGroupsButton.ToolTipCaption = null;
			this.RefreshAvailabilityGroupsButton.Click += new System.EventHandler(this.RefreshAvailabilityGroupsButton_Click);
			// 
			// AvailabilityGroupLabel
			// 
			this.AvailabilityGroupLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 180, true);
			this.AvailabilityGroupLabel.Name = "AvailabilityGroupLabel";
			this.AvailabilityGroupLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
			this.AvailabilityGroupLabel.TabIndex = 49;
			this.AvailabilityGroupLabel.Text = "Availability Group:";
			this.AvailabilityGroupLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AvailabilityGroupLabel.UseMnemonic = false;
			// 
			// AddDbToAvailabilityGroupCheckBox
			// 
			this.AddDbToAvailabilityGroupCheckBox.AutoSize = true;
			this.AddDbToAvailabilityGroupCheckBox.Enabled = false;
			this.AddDbToAvailabilityGroupCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 193, true);
			this.AddDbToAvailabilityGroupCheckBox.Name = "AddDbToAvailabilityGroupCheckBox";
			this.AddDbToAvailabilityGroupCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 14, true);
			this.AddDbToAvailabilityGroupCheckBox.TabIndex = 46;
			this.AddDbToAvailabilityGroupCheckBox.Text = "Add Database to Always On Availability Group";
			this.AddDbToAvailabilityGroupCheckBox.UseVisualStyleBackColor = true;
			this.AddDbToAvailabilityGroupCheckBox.CheckedChanged += new System.EventHandler(this.AddDbToAvailabilityGroupCheckBox_CheckedChanged);
			// 
			// EdwRestoreBrowseButton
			// 
			this.EdwRestoreBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EdwRestoreBrowseButton.Enabled = false;
			this.EdwRestoreBrowseButton.IsCaptionOverridden = true;
			this.EdwRestoreBrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(566, 156, true);
			this.EdwRestoreBrowseButton.Name = "EdwRestoreBrowseButton";
			this.EdwRestoreBrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 22, true);
			this.EdwRestoreBrowseButton.TabIndex = 45;
			this.EdwRestoreBrowseButton.Text = "Browse";
			this.EdwRestoreBrowseButton.ToolTipCaption = null;
			this.EdwRestoreBrowseButton.Click += new System.EventHandler(this.EdwRestoreBrowseButton_Click);
			// 
			// EdwDbRestoreFilePathTextBox
			// 
			this.EdwDbRestoreFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EdwDbRestoreFilePathTextBox.Enabled = false;
			this.EdwDbRestoreFilePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 156, true);
			this.EdwDbRestoreFilePathTextBox.Name = "EdwDbRestoreFilePathTextBox";
			this.EdwDbRestoreFilePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 15, true);
			this.EdwDbRestoreFilePathTextBox.TabIndex = 44;
			this.EdwDbRestoreFilePathTextBox.TextChanged += new System.EventHandler(this.EdwDbRestoreFilePathTextBox_TextChanged);
			// 
			// EdwBackupFileLabel
			// 
			this.EdwBackupFileLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 156, true);
			this.EdwBackupFileLabel.Name = "EdwBackupFileLabel";
			this.EdwBackupFileLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
			this.EdwBackupFileLabel.TabIndex = 43;
			this.EdwBackupFileLabel.Text = "EDW Backup File:";
			this.EdwBackupFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.EdwBackupFileLabel.UseMnemonic = false;
			// 
			// AuditRestoreBrowseButton
			// 
			this.AuditRestoreBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AuditRestoreBrowseButton.Enabled = false;
			this.AuditRestoreBrowseButton.IsCaptionOverridden = true;
			this.AuditRestoreBrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 110, true);
			this.AuditRestoreBrowseButton.Name = "AuditRestoreBrowseButton";
			this.AuditRestoreBrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 22, true);
			this.AuditRestoreBrowseButton.TabIndex = 42;
			this.AuditRestoreBrowseButton.Text = "Browse";
			this.AuditRestoreBrowseButton.ToolTipCaption = null;
			this.AuditRestoreBrowseButton.Click += new System.EventHandler(this.AuditRestoreBrowseButton_Click);
			// 
			// AuditDbRestoreFilePathTextBox
			// 
			this.AuditDbRestoreFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AuditDbRestoreFilePathTextBox.Enabled = false;
			this.AuditDbRestoreFilePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 111, true);
			this.AuditDbRestoreFilePathTextBox.Name = "AuditDbRestoreFilePathTextBox";
			this.AuditDbRestoreFilePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 15, true);
			this.AuditDbRestoreFilePathTextBox.TabIndex = 40;
			this.AuditDbRestoreFilePathTextBox.TextChanged += new System.EventHandler(this.AuditDbRestoreFilePathTextBox_TextChanged);
			// 
			// AuditBackupFileLabel
			// 
			this.AuditBackupFileLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 111, true);
			this.AuditBackupFileLabel.Name = "AuditBackupFileLabel";
			this.AuditBackupFileLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 17, true);
			this.AuditBackupFileLabel.TabIndex = 39;
			this.AuditBackupFileLabel.Text = "Audit Backup File:";
			this.AuditBackupFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AuditBackupFileLabel.UseMnemonic = false;
			// 
			// DbRestoreAuditServer
			// 
			this.DbRestoreAuditServer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 90, true);
			this.DbRestoreAuditServer.Name = "DbRestoreAuditServer";
			this.DbRestoreAuditServer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.DbRestoreAuditServer.TabIndex = 38;
			this.DbRestoreAuditServer.Text = "Audit Server:";
			this.DbRestoreAuditServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DbRestoreAuditServer.UseMnemonic = false;
			// 
			// DbRestoreAuditServerTextBox
			// 
			this.DbRestoreAuditServerTextBox.Enabled = false;
			this.DbRestoreAuditServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 90, true);
			this.DbRestoreAuditServerTextBox.Name = "DbRestoreAuditServerTextBox";
			this.DbRestoreAuditServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 15, true);
			this.DbRestoreAuditServerTextBox.TabIndex = 37;
			this.DbRestoreAuditServerTextBox.TextChanged += new System.EventHandler(this.DbRestoreAuditServerTextBox_TextChanged);
			this.DbRestoreAuditServerTextBox.Leave += new System.EventHandler(this.DbRestoreAuditServerTextBox_Leave);
			// 
			// DbRestoreDwServerTextBox
			// 
			this.DbRestoreDwServerTextBox.Enabled = false;
			this.DbRestoreDwServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 134, true);
			this.DbRestoreDwServerTextBox.Name = "DbRestoreDwServerTextBox";
			this.DbRestoreDwServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 15, true);
			this.DbRestoreDwServerTextBox.TabIndex = 34;
			this.DbRestoreDwServerTextBox.TextChanged += new System.EventHandler(this.DbRestoreDwServerTextBox_TextChanged);
			this.DbRestoreDwServerTextBox.Leave += new System.EventHandler(this.DbRestoreDwServerTextBox_Leave);
			// 
			// DbRestoreDwServer
			// 
			this.DbRestoreDwServer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 138, true);
			this.DbRestoreDwServer.Name = "DbRestoreDwServer";
			this.DbRestoreDwServer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 17, true);
			this.DbRestoreDwServer.TabIndex = 33;
			this.DbRestoreDwServer.Text = "Data Warehouse Server:";
			this.DbRestoreDwServer.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.DbRestoreDwServer.UseMnemonic = false;
			// 
			// RestoreBiDatabasesCheckBox
			// 
			this.RestoreBiDatabasesCheckBox.AutoSize = true;
			this.RestoreBiDatabasesCheckBox.Enabled = false;
			this.RestoreBiDatabasesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 130, true);
			this.RestoreBiDatabasesCheckBox.Name = "RestoreBiDatabasesCheckBox";
			this.RestoreBiDatabasesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 14, true);
			this.RestoreBiDatabasesCheckBox.TabIndex = 32;
			this.RestoreBiDatabasesCheckBox.Text = "Restore Business Intelligence Databases";
			this.RestoreBiDatabasesCheckBox.UseVisualStyleBackColor = true;
			this.RestoreBiDatabasesCheckBox.CheckedChanged += new System.EventHandler(this.RestoreBiDatabasesCheckBox_CheckedChanged);
			// 
			// TransactionLogBackupsCheckBox
			// 
			this.TransactionLogBackupsCheckBox.AutoSize = true;
			this.TransactionLogBackupsCheckBox.Checked = true;
			this.TransactionLogBackupsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.TransactionLogBackupsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 150, true);
			this.TransactionLogBackupsCheckBox.Name = "TransactionLogBackupsCheckBox";
			this.TransactionLogBackupsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 14, true);
			this.TransactionLogBackupsCheckBox.TabIndex = 31;
			this.TransactionLogBackupsCheckBox.Text = "Restore Transaction Log";
			this.TransactionLogBackupsCheckBox.UseVisualStyleBackColor = true;
			// 
			// DifferentialBackupsCheckBox
			// 
			this.DifferentialBackupsCheckBox.AutoSize = true;
			this.DifferentialBackupsCheckBox.Checked = true;
			this.DifferentialBackupsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.DifferentialBackupsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 172, true);
			this.DifferentialBackupsCheckBox.Name = "DifferentialBackupsCheckBox";
			this.DifferentialBackupsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 14, true);
			this.DifferentialBackupsCheckBox.TabIndex = 30;
			this.DifferentialBackupsCheckBox.Text = "Differential Backup";
			this.DifferentialBackupsCheckBox.UseVisualStyleBackColor = true;
			// 
			// DbRestoreOptionComboBox
			// 
			this.DbRestoreOptionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreOptionComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.DbRestoreOptionComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.DbRestoreOptionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.DbRestoreOptionComboBox.Enabled = false;
			this.DbRestoreOptionComboBox.FormattingEnabled = true;
			this.DbRestoreOptionComboBox.Items.AddRange(new object[] {
            "Restore with RECOVERY (completes the recovery process)",
            "Restore with NORECOVERY (allows for a differential backup to take place next)",
            "Copy Production to Test Database (keeps test registry info and clears email/print" +
                " jobs)"});
			this.DbRestoreOptionComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 322, true);
			this.DbRestoreOptionComboBox.Name = "DbRestoreOptionComboBox";
			this.DbRestoreOptionComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 16, true);
			this.DbRestoreOptionComboBox.TabIndex = 12;
			// 
			// RestoreOperationalDatabasesCheckBox
			// 
			this.RestoreOperationalDatabasesCheckBox.AutoSize = true;
			this.RestoreOperationalDatabasesCheckBox.Enabled = false;
			this.RestoreOperationalDatabasesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 90, true);
			this.RestoreOperationalDatabasesCheckBox.Name = "RestoreOperationalDatabasesCheckBox";
			this.RestoreOperationalDatabasesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 14, true);
			this.RestoreOperationalDatabasesCheckBox.TabIndex = 28;
			this.RestoreOperationalDatabasesCheckBox.Text = "Restore Operational Databases";
			this.RestoreOperationalDatabasesCheckBox.UseVisualStyleBackColor = true;
			this.RestoreOperationalDatabasesCheckBox.Click += new System.EventHandler(this.RestoreOperationalDatabasesCheckBox_Click);
			// 
			// DbRestoreRelativeToServerTextBox
			// 
			this.DbRestoreRelativeToServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreRelativeToServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 64, true);
			this.DbRestoreRelativeToServerTextBox.Name = "DbRestoreRelativeToServerTextBox";
			this.DbRestoreRelativeToServerTextBox.ReadOnly = true;
			this.DbRestoreRelativeToServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 15, true);
			this.DbRestoreRelativeToServerTextBox.TabIndex = 6;
			this.DbRestoreRelativeToServerTextBox.Text = "( Relative to DB server ) ";
			this.DbRestoreRelativeToServerTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RestoreReferenceFilesDatabasesCheckBox
			// 
			this.RestoreReferenceFilesDatabasesCheckBox.AutoSize = true;
			this.RestoreReferenceFilesDatabasesCheckBox.Enabled = false;
			this.RestoreReferenceFilesDatabasesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 110, true);
			this.RestoreReferenceFilesDatabasesCheckBox.Name = "RestoreReferenceFilesDatabasesCheckBox";
			this.RestoreReferenceFilesDatabasesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 14, true);
			this.RestoreReferenceFilesDatabasesCheckBox.TabIndex = 29;
			this.RestoreReferenceFilesDatabasesCheckBox.Text = "Restore Reference Databases";
			this.RestoreReferenceFilesDatabasesCheckBox.UseVisualStyleBackColor = true;
			this.RestoreReferenceFilesDatabasesCheckBox.Click += new System.EventHandler(this.RestoreReferenceFilesDatabasesCheckBox_Click);
			// 
			// DbRestoreDatabaseNameTextBox
			//
			this.DbRestoreDatabaseNameTextBox.Enabled = false;
			this.DbRestoreDatabaseNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreDatabaseNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 37, true);
			this.DbRestoreDatabaseNameTextBox.Name = "DbRestoreDatabaseNameTextBox";
			this.DbRestoreDatabaseNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 15, true);
			this.DbRestoreDatabaseNameTextBox.TabIndex = 3;
			this.DbRestoreDatabaseNameTextBox.TextChanged += new System.EventHandler(this.DbRestoreDatabaseNameTextBox_TextChanged);
			this.DbRestoreDatabaseNameTextBox.Leave += new System.EventHandler(this.DbRestoreDatabaseNameTextBox_Leave);
			// 
			// RestoreBrowseButton
			// 
			this.RestoreBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RestoreBrowseButton.Enabled = false;
			this.RestoreBrowseButton.IsCaptionOverridden = true;
			this.RestoreBrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 64, true);
			this.RestoreBrowseButton.Name = "RestoreBrowseButton";
			this.RestoreBrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 22, true);
			this.RestoreBrowseButton.TabIndex = 7;
			this.RestoreBrowseButton.Text = "Browse";
			this.RestoreBrowseButton.ToolTipCaption = null;
			this.RestoreBrowseButton.Click += new System.EventHandler(this.RestoreBrowseButton_Click);
			// 
			// DbRestoreListView
			// 
			this.DbRestoreListView.AllowUserToAddRows = false;
			this.DbRestoreListView.AllowUserToDeleteRows = false;
			this.DbRestoreListView.AllowUserToResizeRows = false;
			this.DbRestoreListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreListView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.DbRestoreListView.ColumnHeadersHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(18);
			this.DbRestoreListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.DbRestoreListView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LogicalName,
            this.Folder});
			this.DbRestoreListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 233, true);
			this.DbRestoreListView.Name = "DbRestoreListView";
			this.DbRestoreListView.RowHeadersVisible = false;
			this.DbRestoreListView.RowHeadersWidth = 82;
			this.DbRestoreListView.RowTemplate.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(16);
			this.DbRestoreListView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.DbRestoreListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 81, true);
			this.DbRestoreListView.TabIndex = 11;
			// 
			// LogicalName
			// 
			this.LogicalName.FillWeight = 40F;
			this.LogicalName.HeaderText = "Logical Name";
			this.LogicalName.MinimumWidth = 6;
			this.LogicalName.Name = "LogicalName";
			this.LogicalName.ReadOnly = true;
			// 
			// Folder
			// 
			this.Folder.HeaderText = "Folder";
			this.Folder.MinimumWidth = 6;
			this.Folder.Name = "Folder";
			// 
			// DbRestoreExamineFileButton
			// 
			this.DbRestoreExamineFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreExamineFileButton.Enabled = false;
			this.DbRestoreExamineFileButton.IsCaptionOverridden = true;
			this.DbRestoreExamineFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 210, true);
			this.DbRestoreExamineFileButton.Name = "DbRestoreExamineFileButton";
			this.DbRestoreExamineFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.DbRestoreExamineFileButton.TabIndex = 36;
			this.DbRestoreExamineFileButton.Text = "Examine File";
			this.DbRestoreExamineFileButton.ToolTipCaption = null;
			this.DbRestoreExamineFileButton.Click += new System.EventHandler(this.DbRestoreExamineFileButton_Click);
			// 
			// DbRestoreButton
			// 
			this.DbRestoreButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.DbRestoreButton.Enabled = false;
			this.DbRestoreButton.IsCaptionOverridden = true;
			this.DbRestoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 321, true);
			this.DbRestoreButton.Name = "DbRestoreButton";
			this.DbRestoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.DbRestoreButton.TabIndex = 13;
			this.DbRestoreButton.Text = "Restore";
			this.DbRestoreButton.ToolTipCaption = null;
			this.DbRestoreButton.Click += new System.EventHandler(this.DbRestoreButton_Click);
			// 
			// DbRestoreFilePathTextBox
			// 
			this.DbRestoreFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreFilePathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 64, true);
			this.DbRestoreFilePathTextBox.Name = "DbRestoreFilePathTextBox";
			this.DbRestoreFilePathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 15, true);
			this.DbRestoreFilePathTextBox.TabIndex = 5;
			this.DbRestoreFilePathTextBox.TextChanged += new System.EventHandler(this.DbRestoreFilePathTextBox_TextChanged);
			this.DbRestoreFilePathTextBox.Leave += new System.EventHandler(this.DbRestoreFilePathTextBox_Leave);
			// 
			// DbRestoreFilePathLabel
			// 
			this.DbRestoreFilePathLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 64, true);
			this.DbRestoreFilePathLabel.Name = "DbRestoreFilePathLabel";
			this.DbRestoreFilePathLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.DbRestoreFilePathLabel.TabIndex = 4;
			this.DbRestoreFilePathLabel.Text = "Backup File:";
			this.DbRestoreFilePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DbRestoreFilePathLabel.UseMnemonic = false;
			// 
			// DbRestoreDatabaseNameLabel
			// 
			this.DbRestoreDatabaseNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 37, true);
			this.DbRestoreDatabaseNameLabel.Name = "DbRestoreDatabaseNameLabel";
			this.DbRestoreDatabaseNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.DbRestoreDatabaseNameLabel.TabIndex = 2;
			this.DbRestoreDatabaseNameLabel.Text = "Database:";
			this.DbRestoreDatabaseNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DbRestoreDatabaseNameLabel.UseMnemonic = false;
			// 
			// DbRestoreServerTextBox
			// 
			this.DbRestoreServerTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DbRestoreServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 12, true);
			this.DbRestoreServerTextBox.Name = "DbRestoreServerTextBox";
			this.DbRestoreServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 15, true);
			this.DbRestoreServerTextBox.TabIndex = 1;
			this.DbRestoreServerTextBox.TextChanged += new System.EventHandler(this.DbRestoreServerTextBox_TextChanged);
			this.DbRestoreServerTextBox.Leave += new System.EventHandler(this.DbRestoreServerTextBox_Leave);
			// 
			// DbRestoreServerLabel
			// 
			this.DbRestoreServerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 12, true);
			this.DbRestoreServerLabel.Name = "DbRestoreServerLabel";
			this.DbRestoreServerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.DbRestoreServerLabel.TabIndex = 0;
			this.DbRestoreServerLabel.Text = "Server:";
			this.DbRestoreServerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DbRestoreServerLabel.UseMnemonic = false;
			// 
			// AvailabilityGroupComboBox
			// 
			this.AvailabilityGroupComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AvailabilityGroupComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.AvailabilityGroupComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.AvailabilityGroupComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AvailabilityGroupComboBox.Enabled = false;
			this.AvailabilityGroupComboBox.FormattingEnabled = true;
			this.AvailabilityGroupComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 181, true);
			this.AvailabilityGroupComboBox.Name = "AvailabilityGroupComboBox";
			this.AvailabilityGroupComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 16, true);
			this.AvailabilityGroupComboBox.TabIndex = 48;
			// 
			// DbRestoreControl
			// 
			this.Controls.Add(this.RestorePanel);
			this.Name = "DbRestoreControl";
			this.Controls.SetChildIndex(this.RestorePanel, 0);
			this.RestorePanel.ResumeLayout(false);
			this.RestorePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DbRestoreListView)).EndInit();
			this.DbRestoreListView.ResumeLayout(false);
			this.DbRestoreListView.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected KPanel RestorePanel;
		protected KComboBox DbRestoreOptionComboBox;
		protected KCheckBox RestoreOperationalDatabasesCheckBox;
		protected KTextBox DbRestoreRelativeToServerTextBox;
		protected KCheckBox RestoreReferenceFilesDatabasesCheckBox;
		protected KTextBox DbRestoreDatabaseNameTextBox;
		protected KButton RestoreBrowseButton;
		protected KDataGridView DbRestoreListView;
		protected KButton DbRestoreExamineFileButton;
		protected KButton DbRestoreButton;
		protected KTextBox DbRestoreFilePathTextBox;
		protected KLabel DbRestoreFilePathLabel;
		protected KLabel DbRestoreDatabaseNameLabel;
		protected KTextBox DbRestoreServerTextBox;
		protected KLabel DbRestoreServerLabel;
		protected KCheckBox RestoreBiDatabasesCheckBox;
		protected KTextBox DbRestoreDwServerTextBox;
		protected KLabel DbRestoreDwServer;
		protected KCheckBox TransactionLogBackupsCheckBox;
		protected KCheckBox DifferentialBackupsCheckBox;
		protected KTextBox DbRestoreAuditServerTextBox;
		protected KLabel DbRestoreAuditServer;
		protected KButton EdwRestoreBrowseButton;
		protected KTextBox EdwDbRestoreFilePathTextBox;
		protected KLabel EdwBackupFileLabel;
		protected KButton AuditRestoreBrowseButton;
		protected KTextBox AuditDbRestoreFilePathTextBox;
		protected KLabel AuditBackupFileLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn LogicalName;
		private System.Windows.Forms.DataGridViewTextBoxColumn Folder;
		protected KCheckBox AddDbToAvailabilityGroupCheckBox;
		protected KComboBox AvailabilityGroupComboBox;
		protected KLabel AvailabilityGroupLabel;
		private KButton RefreshAvailabilityGroupsButton;
	}
}
