namespace Enterprise.Server.Setup
{
	partial class InstallationSettingsForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallationSettingsForm));
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.installationSettingsBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.databaseListBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonHelp = new System.Windows.Forms.Button();
			this.groupBoxLicense = new System.Windows.Forms.GroupBox();
			this.labelInfoLicense = new System.Windows.Forms.Label();
			this.textBoxLicenseCode = new Enterprise.Server.Setup.AutoValidatingTextBox();
			this.labelLicenseCode = new System.Windows.Forms.Label();
			this.groupBoxDatabases = new System.Windows.Forms.GroupBox();
			this.textBoxUserNominatedInstance = new System.Windows.Forms.TextBox();
			this.checkBoxSQLServerMachineNameUseDefault = new CargoWise.Loader.Common.AutoValidatingCheckBox();
			this.textBoxSQLServerMachineName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBoxDbNameUseDefault = new CargoWise.Loader.Common.AutoValidatingCheckBox();
			this.textBoxDbName = new System.Windows.Forms.TextBox();
			this.labelDbName = new System.Windows.Forms.Label();
			this.labelInfo2 = new System.Windows.Forms.Label();
			this.comboBoxDatabase = new System.Windows.Forms.ComboBox();
			this.buttonBrowseLog = new System.Windows.Forms.Button();
			this.buttonBrowseData = new System.Windows.Forms.Button();
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.labelDatabaseHelp = new System.Windows.Forms.Label();
			this.labelDatabase = new System.Windows.Forms.Label();
			this.labelLog = new System.Windows.Forms.Label();
			this.textBoxData = new System.Windows.Forms.TextBox();
			this.labelData = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.instanceNameTextbox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.registerInstanceCheckbox = new CargoWise.Loader.Common.AutoValidatingCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.installationSettingsBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.databaseListBindingSource)).BeginInit();
			this.groupBoxLicense.SuspendLayout();
			this.groupBoxDatabases.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// installationSettingsBindingSource
			// 
			this.installationSettingsBindingSource.DataSource = typeof(Enterprise.Server.Setup.InstallationSettings);
			// 
			// databaseListBindingSource
			// 
			this.databaseListBindingSource.DataMember = "DatabaseList";
			this.databaseListBindingSource.DataSource = this.installationSettingsBindingSource;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(674, 418);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 6;
			this.buttonOK.Text = "&Install";
			this.buttonOK.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(593, 418);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonHelp
			// 
			this.buttonHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonHelp.Location = new System.Drawing.Point(12, 418);
			this.buttonHelp.Name = "buttonHelp";
			this.buttonHelp.Size = new System.Drawing.Size(75, 23);
			this.buttonHelp.TabIndex = 4;
			this.buttonHelp.Text = "Help";
			this.buttonHelp.UseVisualStyleBackColor = true;
			this.buttonHelp.Click += new System.EventHandler(this.ButtonHelp_Click);
			// 
			// groupBoxLicense
			// 
			this.groupBoxLicense.Controls.Add(this.labelInfoLicense);
			this.groupBoxLicense.Controls.Add(this.textBoxLicenseCode);
			this.groupBoxLicense.Controls.Add(this.labelLicenseCode);
			this.groupBoxLicense.Location = new System.Drawing.Point(13, 13);
			this.groupBoxLicense.Name = "groupBoxLicense";
			this.groupBoxLicense.Size = new System.Drawing.Size(735, 80);
			this.groupBoxLicense.TabIndex = 1;
			this.groupBoxLicense.TabStop = false;
			this.groupBoxLicense.Text = "License";
			// 
			// labelInfoLicense
			// 
			this.labelInfoLicense.AutoSize = true;
			this.labelInfoLicense.Location = new System.Drawing.Point(8, 19);
			this.labelInfoLicense.Name = "labelInfoLicense";
			this.labelInfoLicense.Size = new System.Drawing.Size(458, 16);
			this.labelInfoLicense.TabIndex = 4;
			this.labelInfoLicense.Text = "Enter your six letter product key to download the correct installation package.";
			// 
			// textBoxLicenseCode
			// 
			this.textBoxLicenseCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLicenseCode.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.installationSettingsBindingSource, "LicenseCode", true));
			this.textBoxLicenseCode.Location = new System.Drawing.Point(97, 42);
			this.textBoxLicenseCode.Name = "textBoxLicenseCode";
			this.textBoxLicenseCode.Size = new System.Drawing.Size(281, 22);
			this.textBoxLicenseCode.TabIndex = 1;
			// 
			// labelLicenseCode
			// 
			this.labelLicenseCode.AutoSize = true;
			this.labelLicenseCode.Location = new System.Drawing.Point(8, 45);
			this.labelLicenseCode.Name = "labelLicenseCode";
			this.labelLicenseCode.Size = new System.Drawing.Size(82, 16);
			this.labelLicenseCode.TabIndex = 3;
			this.labelLicenseCode.Text = "Product Key:";
			this.labelLicenseCode.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// groupBoxDatabases
			// 
			this.groupBoxDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxDatabases.Controls.Add(this.textBoxUserNominatedInstance);
			this.groupBoxDatabases.Controls.Add(this.checkBoxSQLServerMachineNameUseDefault);
			this.groupBoxDatabases.Controls.Add(this.textBoxSQLServerMachineName);
			this.groupBoxDatabases.Controls.Add(this.label1);
			this.groupBoxDatabases.Controls.Add(this.checkBoxDbNameUseDefault);
			this.groupBoxDatabases.Controls.Add(this.textBoxDbName);
			this.groupBoxDatabases.Controls.Add(this.labelDbName);
			this.groupBoxDatabases.Controls.Add(this.labelInfo2);
			this.groupBoxDatabases.Controls.Add(this.comboBoxDatabase);
			this.groupBoxDatabases.Controls.Add(this.buttonBrowseLog);
			this.groupBoxDatabases.Controls.Add(this.buttonBrowseData);
			this.groupBoxDatabases.Controls.Add(this.textBoxLog);
			this.groupBoxDatabases.Controls.Add(this.labelDatabaseHelp);
			this.groupBoxDatabases.Controls.Add(this.labelDatabase);
			this.groupBoxDatabases.Controls.Add(this.labelLog);
			this.groupBoxDatabases.Controls.Add(this.textBoxData);
			this.groupBoxDatabases.Controls.Add(this.labelData);
			this.groupBoxDatabases.Location = new System.Drawing.Point(13, 183);
			this.groupBoxDatabases.Name = "groupBoxDatabases";
			this.groupBoxDatabases.Size = new System.Drawing.Size(735, 217);
			this.groupBoxDatabases.TabIndex = 3;
			this.groupBoxDatabases.TabStop = false;
			this.groupBoxDatabases.Text = "Databases";
			// 
			// textBoxUserNominatedInstance
			// 
			this.textBoxUserNominatedInstance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxUserNominatedInstance.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.installationSettingsBindingSource, "UserNominatedInstance", true));
			this.textBoxUserNominatedInstance.Location = new System.Drawing.Point(145, 187);
			this.textBoxUserNominatedInstance.Name = "textBoxUserNominatedInstance";
			this.textBoxUserNominatedInstance.Size = new System.Drawing.Size(519, 22);
			this.textBoxUserNominatedInstance.TabIndex = 16;
			// 
			// checkBoxSQLServerMachineNameUseDefault
			// 
			this.checkBoxSQLServerMachineNameUseDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxSQLServerMachineNameUseDefault.AutoSize = true;
			this.checkBoxSQLServerMachineNameUseDefault.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.installationSettingsBindingSource, "UseDefaultSQLServerMachineName", true));
			this.checkBoxSQLServerMachineNameUseDefault.Location = new System.Drawing.Point(629, 163);
			this.checkBoxSQLServerMachineNameUseDefault.Name = "checkBoxSQLServerMachineNameUseDefault";
			this.checkBoxSQLServerMachineNameUseDefault.Size = new System.Drawing.Size(99, 20);
			this.checkBoxSQLServerMachineNameUseDefault.TabIndex = 15;
			this.checkBoxSQLServerMachineNameUseDefault.Text = "Use Default";
			this.checkBoxSQLServerMachineNameUseDefault.UseVisualStyleBackColor = true;
			this.checkBoxSQLServerMachineNameUseDefault.CheckedChanged += new System.EventHandler(this.checkBoxSQLServerMachineNameUseDefault_CheckedChanged);
			// 
			// textBoxSQLServerMachineName
			// 
			this.textBoxSQLServerMachineName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxSQLServerMachineName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.installationSettingsBindingSource, "SQLServerMachineName", true));
			this.textBoxSQLServerMachineName.DataBindings.Add(new System.Windows.Forms.Binding("ReadOnly", this.installationSettingsBindingSource, "UseDefaultSQLServerMachineName", true));
			this.textBoxSQLServerMachineName.Location = new System.Drawing.Point(145, 161);
			this.textBoxSQLServerMachineName.Name = "textBoxSQLServerMachineName";
			this.textBoxSQLServerMachineName.Size = new System.Drawing.Size(466, 22);
			this.textBoxSQLServerMachineName.TabIndex = 14;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(5, 164);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(119, 16);
			this.label1.TabIndex = 13;
			this.label1.Text = "SQL Server Name:";
			// 
			// checkBoxDbNameUseDefault
			// 
			this.checkBoxDbNameUseDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxDbNameUseDefault.AutoSize = true;
			this.checkBoxDbNameUseDefault.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.installationSettingsBindingSource, "UseDefaultDbName", true));
			this.checkBoxDbNameUseDefault.Location = new System.Drawing.Point(629, 60);
			this.checkBoxDbNameUseDefault.Name = "checkBoxDbNameUseDefault";
			this.checkBoxDbNameUseDefault.Size = new System.Drawing.Size(99, 20);
			this.checkBoxDbNameUseDefault.TabIndex = 3;
			this.checkBoxDbNameUseDefault.Text = "Use Default";
			this.checkBoxDbNameUseDefault.UseVisualStyleBackColor = true;
			// 
			// textBoxDbName
			// 
			this.textBoxDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDbName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.installationSettingsBindingSource, "DbName", true));
			this.textBoxDbName.DataBindings.Add(new System.Windows.Forms.Binding("ReadOnly", this.installationSettingsBindingSource, "UseDefaultDbName", true));
			this.textBoxDbName.Location = new System.Drawing.Point(85, 58);
			this.textBoxDbName.Name = "textBoxDbName";
			this.textBoxDbName.Size = new System.Drawing.Size(528, 22);
			this.textBoxDbName.TabIndex = 2;
			// 
			// labelDbName
			// 
			this.labelDbName.AutoSize = true;
			this.labelDbName.Location = new System.Drawing.Point(6, 61);
			this.labelDbName.Name = "labelDbName";
			this.labelDbName.Size = new System.Drawing.Size(69, 16);
			this.labelDbName.TabIndex = 1;
			this.labelDbName.Text = "DB Name:";
			// 
			// labelInfo2
			// 
			this.labelInfo2.AutoSize = true;
			this.labelInfo2.Location = new System.Drawing.Point(6, 20);
			this.labelInfo2.Name = "labelInfo2";
			this.labelInfo2.Size = new System.Drawing.Size(665, 32);
			this.labelInfo2.TabIndex = 0;
			this.labelInfo2.Text = resources.GetString("labelInfo2.Text");
			// 
			// comboBoxDatabase
			// 
			this.comboBoxDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxDatabase.DataBindings.Add(new System.Windows.Forms.Binding("SelectedItem", this.installationSettingsBindingSource, "SelectedDatabase", true));
			this.comboBoxDatabase.DataSource = this.databaseListBindingSource;
			this.comboBoxDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDatabase.ItemHeight = 16;
			this.comboBoxDatabase.Location = new System.Drawing.Point(145, 187);
			this.comboBoxDatabase.Name = "comboBoxDatabase";
			this.comboBoxDatabase.Size = new System.Drawing.Size(575, 24);
			this.comboBoxDatabase.TabIndex = 16;
			// 
			// buttonBrowseLog
			// 
			this.buttonBrowseLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseLog.AutoSize = true;
			this.buttonBrowseLog.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.buttonBrowseLog.Location = new System.Drawing.Point(702, 108);
			this.buttonBrowseLog.Name = "buttonBrowseLog";
			this.buttonBrowseLog.Size = new System.Drawing.Size(26, 26);
			this.buttonBrowseLog.TabIndex = 9;
			this.buttonBrowseLog.Text = "...";
			this.buttonBrowseLog.UseVisualStyleBackColor = true;
			// 
			// buttonBrowseData
			// 
			this.buttonBrowseData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseData.AutoSize = true;
			this.buttonBrowseData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.buttonBrowseData.Location = new System.Drawing.Point(702, 82);
			this.buttonBrowseData.Name = "buttonBrowseData";
			this.buttonBrowseData.Size = new System.Drawing.Size(26, 26);
			this.buttonBrowseData.TabIndex = 6;
			this.buttonBrowseData.Text = "...";
			this.buttonBrowseData.UseVisualStyleBackColor = true;
			// 
			// textBoxLog
			// 
			this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLog.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.installationSettingsBindingSource, "LogPath", true));
			this.textBoxLog.Location = new System.Drawing.Point(85, 110);
			this.textBoxLog.MaxLength = 200;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.Size = new System.Drawing.Size(611, 22);
			this.textBoxLog.TabIndex = 8;
			// 
			// labelDatabaseHelp
			// 
			this.labelDatabaseHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelDatabaseHelp.Location = new System.Drawing.Point(7, 138);
			this.labelDatabaseHelp.Name = "labelDatabaseHelp";
			this.labelDatabaseHelp.Size = new System.Drawing.Size(722, 20);
			this.labelDatabaseHelp.TabIndex = 10;
			this.labelDatabaseHelp.Text = "You must install Microsoft SQL Server Enterprise Edition before installing this p" +
    "roduct.";
			// 
			// labelDatabase
			// 
			this.labelDatabase.AutoSize = true;
			this.labelDatabase.Location = new System.Drawing.Point(4, 190);
			this.labelDatabase.Name = "labelDatabase";
			this.labelDatabase.Size = new System.Drawing.Size(132, 16);
			this.labelDatabase.TabIndex = 11;
			this.labelDatabase.Text = "&SQL Server Instance:";
			// 
			// labelLog
			// 
			this.labelLog.AutoSize = true;
			this.labelLog.Location = new System.Drawing.Point(6, 113);
			this.labelLog.Name = "labelLog";
			this.labelLog.Size = new System.Drawing.Size(65, 16);
			this.labelLog.TabIndex = 7;
			this.labelLog.Text = "&Log Files:";
			// 
			// textBoxData
			// 
			this.textBoxData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxData.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.installationSettingsBindingSource, "DataPath", true));
			this.textBoxData.Location = new System.Drawing.Point(85, 84);
			this.textBoxData.MaxLength = 200;
			this.textBoxData.Name = "textBoxData";
			this.textBoxData.Size = new System.Drawing.Size(611, 22);
			this.textBoxData.TabIndex = 5;
			// 
			// labelData
			// 
			this.labelData.AutoSize = true;
			this.labelData.Location = new System.Drawing.Point(6, 87);
			this.labelData.Name = "labelData";
			this.labelData.Size = new System.Drawing.Size(71, 16);
			this.labelData.TabIndex = 4;
			this.labelData.Text = "&Data Files:";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.instanceNameTextbox);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.registerInstanceCheckbox);
			this.groupBox1.Location = new System.Drawing.Point(13, 100);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(737, 77);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Register Instance On Domain";
			// 
			// instanceNameTextbox
			// 
			this.instanceNameTextbox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.installationSettingsBindingSource, "InstanceName", true));
			this.instanceNameTextbox.DataBindings.Add(new System.Windows.Forms.Binding("ReadOnly", this.installationSettingsBindingSource, "NotRegisterInstance", true));
			this.instanceNameTextbox.Location = new System.Drawing.Point(115, 49);
			this.instanceNameTextbox.Name = "instanceNameTextbox";
			this.instanceNameTextbox.Size = new System.Drawing.Size(103, 22);
			this.instanceNameTextbox.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 52);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Instance Name:";
			// 
			// registerInstanceCheckbox
			// 
			this.registerInstanceCheckbox.AutoSize = true;
			this.registerInstanceCheckbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.installationSettingsBindingSource, "RegisterInstance", true));
			this.registerInstanceCheckbox.Location = new System.Drawing.Point(12, 20);
			this.registerInstanceCheckbox.Name = "registerInstanceCheckbox";
			this.registerInstanceCheckbox.Size = new System.Drawing.Size(633, 20);
			this.registerInstanceCheckbox.TabIndex = 0;
			this.registerInstanceCheckbox.Text = "Register this instance of the application on the current Active Directory Domain " +
    "for automatic discovery.";
			this.registerInstanceCheckbox.UseVisualStyleBackColor = true;
			// 
			// InstallationSettingsForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(761, 450);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBoxDatabases);
			this.Controls.Add(this.groupBoxLicense);
			this.Controls.Add(this.buttonHelp);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "InstallationSettingsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Installation";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InstallationSettingsForm_FormClosing);
			this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.InstallationSettingsForm_HelpRequested);
			((System.ComponentModel.ISupportInitialize)(this.installationSettingsBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.databaseListBindingSource)).EndInit();
			this.groupBoxLicense.ResumeLayout(false);
			this.groupBoxLicense.PerformLayout();
			this.groupBoxDatabases.ResumeLayout(false);
			this.groupBoxDatabases.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.BindingSource installationSettingsBindingSource;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.BindingSource databaseListBindingSource;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonHelp;
		private System.Windows.Forms.GroupBox groupBoxLicense;
		private AutoValidatingTextBox textBoxLicenseCode;
		private System.Windows.Forms.Label labelLicenseCode;
		private System.Windows.Forms.GroupBox groupBoxDatabases;
		private System.Windows.Forms.TextBox textBoxUserNominatedInstance;
		private CargoWise.Loader.Common.AutoValidatingCheckBox checkBoxSQLServerMachineNameUseDefault;
		private System.Windows.Forms.TextBox textBoxSQLServerMachineName;
		private System.Windows.Forms.Label label1;
		private CargoWise.Loader.Common.AutoValidatingCheckBox checkBoxDbNameUseDefault;
		private System.Windows.Forms.TextBox textBoxDbName;
		private System.Windows.Forms.Label labelDbName;
		private System.Windows.Forms.Label labelInfo2;
		private System.Windows.Forms.ComboBox comboBoxDatabase;
		private System.Windows.Forms.Button buttonBrowseLog;
		private System.Windows.Forms.Button buttonBrowseData;
		private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.Label labelDatabaseHelp;
		private System.Windows.Forms.Label labelDatabase;
		private System.Windows.Forms.Label labelLog;
		private System.Windows.Forms.TextBox textBoxData;
		private System.Windows.Forms.Label labelData;
		private System.Windows.Forms.Label labelInfoLicense;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox instanceNameTextbox;
		private System.Windows.Forms.Label label2;
		private CargoWise.Loader.Common.AutoValidatingCheckBox registerInstanceCheckbox;
	}
}

