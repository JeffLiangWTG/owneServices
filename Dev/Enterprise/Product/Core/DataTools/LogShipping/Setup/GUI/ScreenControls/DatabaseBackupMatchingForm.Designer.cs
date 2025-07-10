namespace Enterprise.LogShipping.Setup.GUI
{

	partial class DatabaseBackupMatchingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			this.dbMatchingDataGridView = new System.Windows.Forms.DataGridView();
			this.DatabaseName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.ShouldInitialise = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.BackupFilename = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.DatabasePhysicalDirectoryOverrideGroupBox = new System.Windows.Forms.GroupBox();
			this.logDirectoryOverrideBrowseButton = new System.Windows.Forms.Button();
			this.logDirectoryOverrideTextBox = new System.Windows.Forms.TextBox();
			this.logDirectoryOverrideLabel = new System.Windows.Forms.Label();
			this.dataDirectoryOverrideBrowseButton = new System.Windows.Forms.Button();
			this.dataDirectoryOverrideTextBox = new System.Windows.Forms.TextBox();
			this.dataDirectoryOverrideLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.dbMatchingDataGridView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.DatabasePhysicalDirectoryOverrideGroupBox.SuspendLayout();
			this.SuspendLayout();

			// 
			// dbMatchingDataGridView
			// 
			this.dbMatchingDataGridView.AllowUserToAddRows = false;
			this.dbMatchingDataGridView.AllowUserToDeleteRows = false;
			this.dbMatchingDataGridView.AllowUserToOrderColumns = true;
			this.dbMatchingDataGridView.AllowUserToResizeRows = false;
			this.dbMatchingDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dbMatchingDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(201)))), ((int)(((byte)(218)))), ((int)(((byte)(228)))));
			this.dbMatchingDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dbMatchingDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.DatabaseName,
			this.ShouldInitialise,
			this.BackupFilename});
			this.dbMatchingDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.dbMatchingDataGridView.Location = new System.Drawing.Point(12, 12);
			this.dbMatchingDataGridView.MultiSelect = false;
			this.dbMatchingDataGridView.Name = "dbMatchingDataGridView";
			this.dbMatchingDataGridView.RowHeadersVisible = false;
			this.dbMatchingDataGridView.Size = new System.Drawing.Size(610, 129);
			this.dbMatchingDataGridView.TabIndex = 0;
			this.dbMatchingDataGridView.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.dbMatchingDataGridView_CellValidated);
			this.dbMatchingDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dbMatchingDataGridView_DataError);
			this.dbMatchingDataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dbMatchingDataGridView_RowEnter);
			// 
			// DatabaseName
			// 
			this.DatabaseName.DataPropertyName = "DatabaseName";
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.DatabaseName.DefaultCellStyle = dataGridViewCellStyle1;
			this.DatabaseName.HeaderText = "Name";
			this.DatabaseName.Name = "DatabaseName";
			this.DatabaseName.ReadOnly = true;
			this.DatabaseName.Width = 150;
			// 
			// ShouldInitialise
			// 
			this.ShouldInitialise.DataPropertyName = "ShouldInitialise";
			this.ShouldInitialise.HeaderText = "Init";
			this.ShouldInitialise.Name = "ShouldInitialise";
			this.ShouldInitialise.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.ShouldInitialise.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.ShouldInitialise.Width = 30;
			// 
			// BackupFilename
			// 
			this.BackupFilename.DataPropertyName = "BackupFilename";
			this.BackupFilename.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
			this.BackupFilename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.BackupFilename.HeaderText = "Backup File Name";
			this.BackupFilename.Name = "BackupFilename";
			this.BackupFilename.Width = 400;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(466, 231);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "&OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(547, 231);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// errorProvider
			// 
			this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider.ContainerControl = this;
			// 
			// DatabasePhysicalDirectoryOverrideGroupBox
			// 
			this.DatabasePhysicalDirectoryOverrideGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DatabasePhysicalDirectoryOverrideGroupBox.Controls.Add(this.logDirectoryOverrideBrowseButton);
			this.DatabasePhysicalDirectoryOverrideGroupBox.Controls.Add(this.logDirectoryOverrideTextBox);
			this.DatabasePhysicalDirectoryOverrideGroupBox.Controls.Add(this.logDirectoryOverrideLabel);
			this.DatabasePhysicalDirectoryOverrideGroupBox.Controls.Add(this.dataDirectoryOverrideBrowseButton);
			this.DatabasePhysicalDirectoryOverrideGroupBox.Controls.Add(this.dataDirectoryOverrideTextBox);
			this.DatabasePhysicalDirectoryOverrideGroupBox.Controls.Add(this.dataDirectoryOverrideLabel);
			this.DatabasePhysicalDirectoryOverrideGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DatabasePhysicalDirectoryOverrideGroupBox.Location = new System.Drawing.Point(12, 147);
			this.DatabasePhysicalDirectoryOverrideGroupBox.Name = "DatabasePhysicalDirectoryOverrideGroupBox";
			this.DatabasePhysicalDirectoryOverrideGroupBox.Size = new System.Drawing.Size(610, 72);
			this.DatabasePhysicalDirectoryOverrideGroupBox.TabIndex = 9;
			this.DatabasePhysicalDirectoryOverrideGroupBox.TabStop = false;
			this.DatabasePhysicalDirectoryOverrideGroupBox.Text = "Database Physical Directory Override";
			// 
			// logDirectoryOverrideBrowseButton
			// 
			this.logDirectoryOverrideBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.logDirectoryOverrideBrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.logDirectoryOverrideBrowseButton.Location = new System.Drawing.Point(529, 43);
			this.logDirectoryOverrideBrowseButton.Name = "logDirectoryOverrideBrowseButton";
			this.logDirectoryOverrideBrowseButton.Size = new System.Drawing.Size(75, 23);
			this.logDirectoryOverrideBrowseButton.TabIndex = 14;
			this.logDirectoryOverrideBrowseButton.Text = "Browse";
			this.logDirectoryOverrideBrowseButton.UseVisualStyleBackColor = true;
			this.logDirectoryOverrideBrowseButton.Click += new System.EventHandler(this.logDirectoryOverrideBrowseButton_Click);
			// 
			// logDirectoryOverrideTextBox
			// 
			this.logDirectoryOverrideTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.logDirectoryOverrideTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.logDirectoryOverrideTextBox.Location = new System.Drawing.Point(90, 45);
			this.logDirectoryOverrideTextBox.Name = "logDirectoryOverrideTextBox";
			this.logDirectoryOverrideTextBox.Size = new System.Drawing.Size(420, 20);
			this.logDirectoryOverrideTextBox.TabIndex = 13;
			this.logDirectoryOverrideTextBox.Tag = "Log Directory Override";
			this.logDirectoryOverrideTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.logDirectoryOverrideTextBox_Validating);
			// 
			// logDirectoryOverrideLabel
			// 
			this.logDirectoryOverrideLabel.AutoSize = true;
			this.logDirectoryOverrideLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.logDirectoryOverrideLabel.Location = new System.Drawing.Point(6, 48);
			this.logDirectoryOverrideLabel.Name = "logDirectoryOverrideLabel";
			this.logDirectoryOverrideLabel.Size = new System.Drawing.Size(73, 13);
			this.logDirectoryOverrideLabel.TabIndex = 12;
			this.logDirectoryOverrideLabel.Text = "Log Directory:";
			// 
			// dataDirectoryOverrideBrowseButton
			// 
			this.dataDirectoryOverrideBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.dataDirectoryOverrideBrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dataDirectoryOverrideBrowseButton.Location = new System.Drawing.Point(529, 17);
			this.dataDirectoryOverrideBrowseButton.Name = "dataDirectoryOverrideBrowseButton";
			this.dataDirectoryOverrideBrowseButton.Size = new System.Drawing.Size(75, 23);
			this.dataDirectoryOverrideBrowseButton.TabIndex = 11;
			this.dataDirectoryOverrideBrowseButton.Text = "Browse";
			this.dataDirectoryOverrideBrowseButton.UseVisualStyleBackColor = true;
			this.dataDirectoryOverrideBrowseButton.Click += new System.EventHandler(this.dataDirectoryOverrideBrowseButton_Click);
			// 
			// dataDirectoryOverrideTextBox
			// 
			this.dataDirectoryOverrideTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dataDirectoryOverrideTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dataDirectoryOverrideTextBox.Location = new System.Drawing.Point(90, 19);
			this.dataDirectoryOverrideTextBox.Name = "dataDirectoryOverrideTextBox";
			this.dataDirectoryOverrideTextBox.Size = new System.Drawing.Size(420, 20);
			this.dataDirectoryOverrideTextBox.TabIndex = 10;
			this.dataDirectoryOverrideTextBox.Tag = "Data Directory Override";
			this.dataDirectoryOverrideTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.dataDirectoryOverrideTextBox_Validating);
			// 
			// dataDirectoryOverrideLabel
			// 
			this.dataDirectoryOverrideLabel.AutoSize = true;
			this.dataDirectoryOverrideLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dataDirectoryOverrideLabel.Location = new System.Drawing.Point(6, 22);
			this.dataDirectoryOverrideLabel.Name = "dataDirectoryOverrideLabel";
			this.dataDirectoryOverrideLabel.Size = new System.Drawing.Size(78, 13);
			this.dataDirectoryOverrideLabel.TabIndex = 9;
			this.dataDirectoryOverrideLabel.Text = "Data Directory:";
			// 
			// DatabaseBackupMatchingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(634, 266);
			this.Controls.Add(this.DatabasePhysicalDirectoryOverrideGroupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.dbMatchingDataGridView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(450, 250);
			this.Name = "DatabaseBackupMatchingForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Database Backups";
			((System.ComponentModel.ISupportInitialize)(this.dbMatchingDataGridView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.DatabasePhysicalDirectoryOverrideGroupBox.ResumeLayout(false);
			this.DatabasePhysicalDirectoryOverrideGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dbMatchingDataGridView;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.GroupBox DatabasePhysicalDirectoryOverrideGroupBox;
		private System.Windows.Forms.Button logDirectoryOverrideBrowseButton;
		private System.Windows.Forms.TextBox logDirectoryOverrideTextBox;
		private System.Windows.Forms.Label logDirectoryOverrideLabel;
		private System.Windows.Forms.Button dataDirectoryOverrideBrowseButton;
		private System.Windows.Forms.TextBox dataDirectoryOverrideTextBox;
		private System.Windows.Forms.Label dataDirectoryOverrideLabel;
		private System.Windows.Forms.DataGridViewTextBoxColumn DatabaseName;
		private System.Windows.Forms.DataGridViewCheckBoxColumn ShouldInitialise;
		private System.Windows.Forms.DataGridViewComboBoxColumn BackupFilename;
	}
}