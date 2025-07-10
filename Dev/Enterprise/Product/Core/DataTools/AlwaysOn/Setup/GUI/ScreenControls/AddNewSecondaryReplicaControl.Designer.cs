using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	partial class AddNewSecondaryReplicaControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.toolTipService = new System.Windows.Forms.ToolTip(this.components);
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddNewSecondaryReplicaControl));
			this.joinLevelImageList = new System.Windows.Forms.ImageList(this.components);
			this.cancelButton = new System.Windows.Forms.Button();
			this.confirmButton = new System.Windows.Forms.Button();
			this.groupDatabaseListView = new System.Windows.Forms.ListView();
			this.DbName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BackupLsn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SecondaryState = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.RedoLsn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.DbFilesMatch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.dbServerLabel = new System.Windows.Forms.Label();
			this.serverInstanceNameTextBox = new System.Windows.Forms.TextBox();
			this.connectButton = new System.Windows.Forms.Button();
			this.replicaOptionsGroupBox = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.allowConnectionsComboBox = new System.Windows.Forms.ComboBox();
			this.commitModeComboBox = new System.Windows.Forms.ComboBox();
			this.commitModeLabel = new System.Windows.Forms.Label();
			this.failoverModeComboBox = new System.Windows.Forms.ComboBox();
			this.replicaNodeDropDown = new System.Windows.Forms.ComboBox();
			this.portTextBox = new System.Windows.Forms.TextBox();
			this.portLabel = new System.Windows.Forms.Label();
			this.instanceLabel = new System.Windows.Forms.Label();
			this.replicaOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// joinLevelImageList
			// 
			this.joinLevelImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("joinLevelImageList.ImageStream")));
			this.joinLevelImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.joinLevelImageList.Images.SetKeyName(0, "basic1-137_cross_no.png");
			this.joinLevelImageList.Images.SetKeyName(1, "basic1-062_Battery1of4.png");
			this.joinLevelImageList.Images.SetKeyName(2, "basic1-063_Battery2of4.png");
			this.joinLevelImageList.Images.SetKeyName(3, "basic1-064_Battery3of4.png");
			this.joinLevelImageList.Images.SetKeyName(4, "basic1-065_Battery4of4.png");
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancelButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CrossNo;
			this.cancelButton.Location = new System.Drawing.Point(759, 382);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(150, 50);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// confirmButton
			// 
			this.confirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.confirmButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.confirmButton.Enabled = false;
			this.confirmButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.confirmButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CheckYes;
			this.confirmButton.Location = new System.Drawing.Point(759, 438);
			this.confirmButton.Name = "confirmButton";
			this.confirmButton.Size = new System.Drawing.Size(150, 50);
			this.confirmButton.TabIndex = 9;
			this.confirmButton.Text = "Confirm";
			this.confirmButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.confirmButton.UseVisualStyleBackColor = false;
			this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
			// 
			// groupDatabaseListView
			// 
			this.groupDatabaseListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.groupDatabaseListView.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.groupDatabaseListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.groupDatabaseListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.DbName,
			this.BackupLsn,
			this.SecondaryState,
			this.RedoLsn,
			this.DbFilesMatch});
			this.groupDatabaseListView.FullRowSelect = true;
			this.groupDatabaseListView.GridLines = true;
			this.groupDatabaseListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.groupDatabaseListView.HideSelection = false;
			this.groupDatabaseListView.Location = new System.Drawing.Point(14, 115);
			this.groupDatabaseListView.MultiSelect = false;
			this.groupDatabaseListView.Name = "groupDatabaseListView";
			this.groupDatabaseListView.ShowItemToolTips = true;
			this.groupDatabaseListView.Size = new System.Drawing.Size(739, 373);
			this.groupDatabaseListView.SmallImageList = this.joinLevelImageList;
			this.groupDatabaseListView.TabIndex = 6;
			this.groupDatabaseListView.UseCompatibleStateImageBehavior = false;
			this.groupDatabaseListView.View = System.Windows.Forms.View.Details;
			// 
			// DbName
			// 
			this.DbName.Text = "Database";
			this.DbName.Width = 250;
			// 
			// BackupLsn
			// 
			this.BackupLsn.Text = "Last Primary Backup LSN";
			this.BackupLsn.Width = 150;
			// 
			// IsSecondaryInRecoveryState
			// 
			this.SecondaryState.Text = "State";
			this.SecondaryState.Width = 100;
			// 
			// RedoLsn
			// 
			this.RedoLsn.Text = "Redo LSN";
			this.RedoLsn.Width = 150;
			// 
			// DbFilesMatch
			// 
			this.DbFilesMatch.Text = "DB Files";
			this.DbFilesMatch.Width = 70;
			// 
			// dbServerLabel
			// 
			this.dbServerLabel.AutoSize = true;
			this.dbServerLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dbServerLabel.Location = new System.Drawing.Point(12, 9);
			this.dbServerLabel.Name = "dbServerLabel";
			this.dbServerLabel.Size = new System.Drawing.Size(261, 19);
			this.dbServerLabel.TabIndex = 0;
			this.dbServerLabel.Text = "Secondary AlwaysOn Replica Server Name:";
			// 
			// serverInstanceNameTextBox
			// 
			this.serverInstanceNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.serverInstanceNameTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.serverInstanceNameTextBox.Enabled = false;
			this.serverInstanceNameTextBox.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.serverInstanceNameTextBox.Location = new System.Drawing.Point(138, 76);
			this.serverInstanceNameTextBox.Name = "serverInstanceNameTextBox";
			this.serverInstanceNameTextBox.Size = new System.Drawing.Size(412, 30);
			this.serverInstanceNameTextBox.TabIndex = 4;
			this.serverInstanceNameTextBox.Tag = "(instance name or leave blank for default)";
			this.serverInstanceNameTextBox.Enter += new System.EventHandler(this.serverInstanceNameTextBox_Enter);
			this.serverInstanceNameTextBox.Leave += new System.EventHandler(this.serverInstanceNameTextBox_Leave);
			// 
			// connectButton
			// 
			this.connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.connectButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.connectButton.Enabled = false;
			this.connectButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.connectButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.Database16;
			this.connectButton.Location = new System.Drawing.Point(674, 74);
			this.connectButton.Name = "connectButton";
			this.connectButton.Size = new System.Drawing.Size(79, 33);
			this.connectButton.TabIndex = 5;
			this.connectButton.Text = "Connect";
			this.connectButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.connectButton.UseVisualStyleBackColor = false;
			this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
			// 
			// replicaOptionsGroupBox
			// 
			this.replicaOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.replicaOptionsGroupBox.Controls.Add(this.label2);
			this.replicaOptionsGroupBox.Controls.Add(this.label1);
			this.replicaOptionsGroupBox.Controls.Add(this.allowConnectionsComboBox);
			this.replicaOptionsGroupBox.Controls.Add(this.commitModeComboBox);
			this.replicaOptionsGroupBox.Controls.Add(this.commitModeLabel);
			this.replicaOptionsGroupBox.Controls.Add(this.failoverModeComboBox);
			this.replicaOptionsGroupBox.Location = new System.Drawing.Point(759, 161);
			this.replicaOptionsGroupBox.Name = "replicaOptionsGroupBox";
			this.replicaOptionsGroupBox.Size = new System.Drawing.Size(150, 181);
			this.replicaOptionsGroupBox.TabIndex = 7;
			this.replicaOptionsGroupBox.TabStop = false;
			this.replicaOptionsGroupBox.Text = "Replica Options";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 129);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(97, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Allow Connections:";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 79);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Failover Mode:";
			// 
			// allowConnectionsComboBox
			// 
			this.allowConnectionsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.allowConnectionsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.allowConnectionsComboBox.FormattingEnabled = true;
			this.allowConnectionsComboBox.Items.AddRange(new object[] {
			"No",
			"Read-Only",
			"All"});
			this.allowConnectionsComboBox.Location = new System.Drawing.Point(6, 145);
			this.allowConnectionsComboBox.Name = "allowConnectionsComboBox";
			this.allowConnectionsComboBox.Size = new System.Drawing.Size(138, 21);
			this.allowConnectionsComboBox.TabIndex = 5;
			this.allowConnectionsComboBox.SelectedIndexChanged += new System.EventHandler(this.allowConnectionsComboBox_SelectedIndexChanged);
			// 
			// commitModeComboBox
			// 
			this.commitModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.commitModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.commitModeComboBox.FormattingEnabled = true;
			this.commitModeComboBox.Items.AddRange(new object[] {
			"Asynchronous",
			"Synchronous"});
			this.commitModeComboBox.Location = new System.Drawing.Point(6, 45);
			this.commitModeComboBox.Name = "commitModeComboBox";
			this.commitModeComboBox.Size = new System.Drawing.Size(138, 21);
			this.commitModeComboBox.TabIndex = 1;
			// 
			// commitModeLabel
			// 
			this.commitModeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.commitModeLabel.AutoSize = true;
			this.commitModeLabel.Location = new System.Drawing.Point(6, 29);
			this.commitModeLabel.Name = "commitModeLabel";
			this.commitModeLabel.Size = new System.Drawing.Size(74, 13);
			this.commitModeLabel.TabIndex = 0;
			this.commitModeLabel.Text = "Commit Mode:";
			// 
			// failoverModeComboBox
			// 
			this.failoverModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.failoverModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.failoverModeComboBox.FormattingEnabled = true;
			this.failoverModeComboBox.Items.AddRange(new object[] {
			"Manual",
			"Automatic"});
			this.failoverModeComboBox.Location = new System.Drawing.Point(6, 95);
			this.failoverModeComboBox.Name = "failoverModeComboBox";
			this.failoverModeComboBox.Size = new System.Drawing.Size(138, 21);
			this.failoverModeComboBox.TabIndex = 3;
			// 
			// replicaNodeDropDown
			// 
			this.replicaNodeDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.replicaNodeDropDown.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.replicaNodeDropDown.Enabled = false;
			this.replicaNodeDropDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.replicaNodeDropDown.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.replicaNodeDropDown.FormattingEnabled = true;
			this.replicaNodeDropDown.Location = new System.Drawing.Point(14, 37);
			this.replicaNodeDropDown.Name = "replicaNodeDropDown";
			this.replicaNodeDropDown.Size = new System.Drawing.Size(739, 31);
			this.replicaNodeDropDown.TabIndex = 2;
			this.replicaNodeDropDown.TextChanged += new System.EventHandler(this.replicaNodeDropDown_Changed);
			// 
			// portTextBox
			// 
			this.portTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.portTextBox.Enabled = false;
			this.portTextBox.Font = new System.Drawing.Font("Tahoma", 15.75F);
			this.portTextBox.Location = new System.Drawing.Point(606, 74);
			this.portTextBox.MaxLength = 5;
			this.portTextBox.Name = "portTextBox";
			this.portTextBox.Size = new System.Drawing.Size(62, 33);
			this.portTextBox.TabIndex = 29;
			this.portTextBox.Tag = "(enter port number or leave it blank for default value. Default value = 1433)";
			this.portTextBox.Text = "1433";
			this.portTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// portLabel
			// 
			this.portLabel.AutoSize = true;
			this.portLabel.Font = new System.Drawing.Font("Tahoma", 12F);
			this.portLabel.Location = new System.Drawing.Point(556, 80);
			this.portLabel.Name = "portLabel";
			this.portLabel.Size = new System.Drawing.Size(44, 19);
			this.portLabel.TabIndex = 30;
			this.portLabel.Text = "Port:";
			// 
			// instanceLabel
			// 
			this.instanceLabel.AutoSize = true;
			this.instanceLabel.Font = new System.Drawing.Font("Tahoma", 12F);
			this.instanceLabel.Location = new System.Drawing.Point(12, 82);
			this.instanceLabel.Name = "instanceLabel";
			this.instanceLabel.Size = new System.Drawing.Size(120, 19);
			this.instanceLabel.TabIndex = 31;
			this.instanceLabel.Text = "Instance Name:";
			// 
			// AddNewSecondaryReplicaControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Controls.Add(this.instanceLabel);
			this.Controls.Add(this.portTextBox);
			this.Controls.Add(this.portLabel);
			this.Controls.Add(this.replicaNodeDropDown);
			this.Controls.Add(this.replicaOptionsGroupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.confirmButton);
			this.Controls.Add(this.groupDatabaseListView);
			this.Controls.Add(this.dbServerLabel);
			this.Controls.Add(this.serverInstanceNameTextBox);
			this.Controls.Add(this.connectButton);
			this.Name = "AddNewSecondaryReplicaControl";
			this.Size = new System.Drawing.Size(918, 492);
			this.Load += new System.EventHandler(this.SecondaryReplicaActionControl_Load);
			this.replicaOptionsGroupBox.ResumeLayout(false);
			this.replicaOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label dbServerLabel;
		protected System.Windows.Forms.TextBox serverInstanceNameTextBox;
		private System.Windows.Forms.ListView groupDatabaseListView;
		private System.Windows.Forms.ColumnHeader DbName;
		private System.Windows.Forms.ColumnHeader BackupLsn;
		private System.Windows.Forms.ColumnHeader SecondaryState;
		private System.Windows.Forms.ColumnHeader RedoLsn;
		private System.Windows.Forms.ColumnHeader DbFilesMatch;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button confirmButton;
		private System.Windows.Forms.ImageList joinLevelImageList;
		private System.Windows.Forms.Button connectButton;
		private System.Windows.Forms.GroupBox replicaOptionsGroupBox;
		private System.Windows.Forms.Label commitModeLabel;
		private System.Windows.Forms.ComboBox failoverModeComboBox;
		private System.Windows.Forms.ComboBox allowConnectionsComboBox;
		private System.Windows.Forms.ComboBox commitModeComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox replicaNodeDropDown;
		protected System.Windows.Forms.TextBox portTextBox;
		private System.Windows.Forms.Label portLabel;
		private System.Windows.Forms.Label instanceLabel;
		private System.Windows.Forms.ToolTip toolTipService;
	}
}
