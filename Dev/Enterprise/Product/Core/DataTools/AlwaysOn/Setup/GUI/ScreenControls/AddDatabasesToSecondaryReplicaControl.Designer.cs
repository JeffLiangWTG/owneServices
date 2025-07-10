namespace Enterprise.AlwaysOn.Setup.GUI
{
	partial class AddDatabasesToSecondaryReplicaControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddDatabasesToSecondaryReplicaControl));
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
			this.suspendResumeButton = new System.Windows.Forms.Button();
			this.IsDataMovementSuspended = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
			this.cancelButton.Location = new System.Drawing.Point(758, 383);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(150, 50);
			this.cancelButton.TabIndex = 12;
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
			this.confirmButton.Location = new System.Drawing.Point(758, 439);
			this.confirmButton.Name = "confirmButton";
			this.confirmButton.Size = new System.Drawing.Size(150, 50);
			this.confirmButton.TabIndex = 13;
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
            this.DbFilesMatch,
            this.IsDataMovementSuspended});
			this.groupDatabaseListView.FullRowSelect = true;
			this.groupDatabaseListView.GridLines = true;
			this.groupDatabaseListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.groupDatabaseListView.HideSelection = false;
			this.groupDatabaseListView.Location = new System.Drawing.Point(14, 35);
			this.groupDatabaseListView.MultiSelect = false;
			this.groupDatabaseListView.Name = "groupDatabaseListView";
			this.groupDatabaseListView.ShowItemToolTips = true;
			this.groupDatabaseListView.Size = new System.Drawing.Size(738, 453);
			this.groupDatabaseListView.SmallImageList = this.joinLevelImageList;
			this.groupDatabaseListView.TabIndex = 8;
			this.groupDatabaseListView.UseCompatibleStateImageBehavior = false;
			this.groupDatabaseListView.View = System.Windows.Forms.View.Details;
			this.groupDatabaseListView.SelectedIndexChanged += new System.EventHandler(this.groupDatabaseListView_SelectedIndexChanged);
			// 
			// DbName
			// 
			this.DbName.Text = "Database";
			this.DbName.Width = 221;
			// 
			// BackupLsn
			// 
			this.BackupLsn.Text = "Last Primary Backup LSN";
			this.BackupLsn.Width = 150;
			// 
			// IsSecondaryInRecoveryState
			// 
			this.SecondaryState.Text = "State";
			this.SecondaryState.Width = 79;
			// 
			// RedoLsn
			// 
			this.RedoLsn.Text = "Redo LSN";
			this.RedoLsn.Width = 139;
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
			this.dbServerLabel.Location = new System.Drawing.Point(14, 9);
			this.dbServerLabel.Name = "dbServerLabel";
			this.dbServerLabel.Size = new System.Drawing.Size(207, 19);
			this.dbServerLabel.TabIndex = 3;
			this.dbServerLabel.Text = "Secondary Database Server:";
			// 
			// suspendResumeButton
			// 
			this.suspendResumeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.suspendResumeButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.suspendResumeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.suspendResumeButton.Location = new System.Drawing.Point(758, 327);
			this.suspendResumeButton.Name = "suspendResumeButton";
			this.suspendResumeButton.Size = new System.Drawing.Size(150, 50);
			this.suspendResumeButton.TabIndex = 15;
			this.suspendResumeButton.Text = "Suspend Replication";
			this.suspendResumeButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.suspendResumeButton.UseVisualStyleBackColor = false;
			this.suspendResumeButton.Click += new System.EventHandler(this.suspendResumeButton_Click);
			// 
			// IsDataMovementSuspended
			// 
			this.IsDataMovementSuspended.Text = "Is Data Movement Suspended";
			this.IsDataMovementSuspended.Width = 111;
			// 
			// AddDatabasesToSecondaryReplicaControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Controls.Add(this.suspendResumeButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.confirmButton);
			this.Controls.Add(this.groupDatabaseListView);
			this.Controls.Add(this.dbServerLabel);
			this.Name = "AddDatabasesToSecondaryReplicaControl";
			this.Size = new System.Drawing.Size(918, 492);
			this.Load += new System.EventHandler(this.AddDatabasesToSecondaryReplicaControl_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label dbServerLabel;
		private System.Windows.Forms.ListView groupDatabaseListView;
		private System.Windows.Forms.ColumnHeader DbName;
		private System.Windows.Forms.ColumnHeader BackupLsn;
		private System.Windows.Forms.ColumnHeader SecondaryState;
		private System.Windows.Forms.ColumnHeader RedoLsn;
		private System.Windows.Forms.ColumnHeader DbFilesMatch;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button confirmButton;
		private System.Windows.Forms.ImageList joinLevelImageList;
		private System.Windows.Forms.Button suspendResumeButton;
		private System.Windows.Forms.ColumnHeader IsDataMovementSuspended;
	}
}
