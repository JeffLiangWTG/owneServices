namespace Enterprise.LogShipping.Setup.GUI
{

	partial class InitializeSecondaryDbControl
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
			this.conditionalLabel = new System.Windows.Forms.Label();
			this.chooseLabel = new System.Windows.Forms.Label();
			this.chooseBackupRadioButton = new System.Windows.Forms.RadioButton();
			this.dbNotInstalledPanel = new System.Windows.Forms.Panel();
			this.skipStepRadioButton = new System.Windows.Forms.RadioButton();
			this.dbInitializedLabel = new System.Windows.Forms.Label();
			this.dbInstalledPanel = new System.Windows.Forms.Panel();
			this.GenerateScriptCheckBox = new System.Windows.Forms.CheckBox();
			this.matchBackupsLinkLabel = new System.Windows.Forms.LinkLabel();
			this.settingsGroupBox.SuspendLayout();
			this.dbNotInstalledPanel.SuspendLayout();
			this.dbInstalledPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.GenerateScriptCheckBox);
			this.settingsGroupBox.Controls.Add(this.matchBackupsLinkLabel);
			this.settingsGroupBox.Controls.Add(this.dbInstalledPanel);
			this.settingsGroupBox.Controls.Add(this.dbNotInstalledPanel);
			this.settingsGroupBox.Text = "Initialize Secondary Database";
			// 
			// conditionalLabel
			// 
			this.conditionalLabel.AutoSize = true;
			this.conditionalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.conditionalLabel.Location = new System.Drawing.Point(16, 3);
			this.conditionalLabel.Name = "conditionalLabel";
			this.conditionalLabel.Size = new System.Drawing.Size(400, 26);
			this.conditionalLabel.TabIndex = 0;
			this.conditionalLabel.Text = "The secondary database/s are not installed yet. You must complete this step befor" +
				"e\r\nprocessing the setup.";
			// 
			// chooseLabel
			// 
			this.chooseLabel.AutoSize = true;
			this.chooseLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.chooseLabel.Location = new System.Drawing.Point(16, 47);
			this.chooseLabel.Name = "chooseLabel";
			this.chooseLabel.Size = new System.Drawing.Size(376, 13);
			this.chooseLabel.TabIndex = 0;
			this.chooseLabel.Text = "Choose the full backup/s to initialize the secondary database/s and click next:";
			// 
			// chooseBackupRadioButton
			// 
			this.chooseBackupRadioButton.AutoSize = true;
			this.chooseBackupRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.chooseBackupRadioButton.Location = new System.Drawing.Point(7, 53);
			this.chooseBackupRadioButton.Name = "chooseBackupRadioButton";
			this.chooseBackupRadioButton.Size = new System.Drawing.Size(288, 17);
			this.chooseBackupRadioButton.TabIndex = 3;
			this.chooseBackupRadioButton.Text = "Reinitialize secondary database from the backup below:";
			this.chooseBackupRadioButton.UseVisualStyleBackColor = true;
			// 
			// dbNotInstalledPanel
			// 
			this.dbNotInstalledPanel.Controls.Add(this.chooseLabel);
			this.dbNotInstalledPanel.Controls.Add(this.conditionalLabel);
			this.dbNotInstalledPanel.Location = new System.Drawing.Point(14, 19);
			this.dbNotInstalledPanel.Name = "dbNotInstalledPanel";
			this.dbNotInstalledPanel.Size = new System.Drawing.Size(433, 76);
			this.dbNotInstalledPanel.TabIndex = 4;
			this.dbNotInstalledPanel.Visible = false;
			// 
			// skipStepRadioButton
			// 
			this.skipStepRadioButton.AutoSize = true;
			this.skipStepRadioButton.Checked = true;
			this.skipStepRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.skipStepRadioButton.Location = new System.Drawing.Point(6, 30);
			this.skipStepRadioButton.Name = "skipStepRadioButton";
			this.skipStepRadioButton.Size = new System.Drawing.Size(403, 17);
			this.skipStepRadioButton.TabIndex = 4;
			this.skipStepRadioButton.TabStop = true;
			this.skipStepRadioButton.Text = "Skip this step and reconfigure Log Shipping for the existing secondary database.";
			this.skipStepRadioButton.UseVisualStyleBackColor = true;
			// 
			// dbInitializedLabel
			// 
			this.dbInitializedLabel.AutoSize = true;
			this.dbInitializedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dbInitializedLabel.Location = new System.Drawing.Point(4, 7);
			this.dbInitializedLabel.Name = "dbInitializedLabel";
			this.dbInitializedLabel.Size = new System.Drawing.Size(220, 13);
			this.dbInitializedLabel.TabIndex = 5;
			this.dbInitializedLabel.Text = "The secondary database is already initialized.";
			// 
			// dbInstalledPanel
			// 
			this.dbInstalledPanel.Controls.Add(this.dbInitializedLabel);
			this.dbInstalledPanel.Controls.Add(this.skipStepRadioButton);
			this.dbInstalledPanel.Controls.Add(this.chooseBackupRadioButton);
			this.dbInstalledPanel.Location = new System.Drawing.Point(14, 19);
			this.dbInstalledPanel.Name = "dbInstalledPanel";
			this.dbInstalledPanel.Size = new System.Drawing.Size(433, 76);
			this.dbInstalledPanel.TabIndex = 6;
			this.dbInstalledPanel.Visible = false;
			// 
			// GenerateScriptCheckBox
			// 
			this.GenerateScriptCheckBox.AutoSize = true;
			this.GenerateScriptCheckBox.Location = new System.Drawing.Point(33, 115);
			this.GenerateScriptCheckBox.Name = "GenerateScriptCheckBox";
			this.GenerateScriptCheckBox.Size = new System.Drawing.Size(325, 17);
			this.GenerateScriptCheckBox.TabIndex = 7;
			this.GenerateScriptCheckBox.Text = "Don not restore the database, just give me the script";
			this.GenerateScriptCheckBox.UseVisualStyleBackColor = true;
			// 
			// matchBackupsLinkLabel
			// 
			this.matchBackupsLinkLabel.AutoSize = true;
			this.matchBackupsLinkLabel.Location = new System.Drawing.Point(30, 90);
			this.matchBackupsLinkLabel.Name = "matchBackupsLinkLabel";
			this.matchBackupsLinkLabel.Size = new System.Drawing.Size(190, 13);
			this.matchBackupsLinkLabel.TabIndex = 7;
			this.matchBackupsLinkLabel.TabStop = true;
			this.matchBackupsLinkLabel.Text = "Click here to select backup files";
			this.matchBackupsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.matchBackupsLinkLabel_LinkClicked);
			// 
			// InitializeSecondaryDbControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Name = "InitializeSecondaryDbControl";
			this.VisibleChanged += new System.EventHandler(this.InitializeSecondaryDbControl_VisibleChanged);
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.dbNotInstalledPanel.ResumeLayout(false);
			this.dbNotInstalledPanel.PerformLayout();
			this.dbInstalledPanel.ResumeLayout(false);
			this.dbInstalledPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label conditionalLabel;
		private System.Windows.Forms.Label chooseLabel;
		private System.Windows.Forms.Panel dbNotInstalledPanel;
		private System.Windows.Forms.RadioButton chooseBackupRadioButton;
		private System.Windows.Forms.RadioButton skipStepRadioButton;
		private System.Windows.Forms.Label dbInitializedLabel;
		private System.Windows.Forms.Panel dbInstalledPanel;
		private System.Windows.Forms.CheckBox GenerateScriptCheckBox;
		private System.Windows.Forms.LinkLabel matchBackupsLinkLabel;
	}
}