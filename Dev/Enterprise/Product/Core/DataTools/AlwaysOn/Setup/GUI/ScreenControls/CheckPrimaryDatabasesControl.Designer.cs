namespace Enterprise.AlwaysOn.Setup.GUI
{
	//[System.Diagnostics.Contracts.ContractVerification(false)]
	partial class CheckPrimaryDatabasesControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckPrimaryDatabasesControl));
			this.groupLabel = new System.Windows.Forms.Label();
			this.groupDatabaseListView = new System.Windows.Forms.ListView();
			this.DbName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.AvailabilityGroup = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.HasFullBackup = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.IsDataMovementSuspended = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.joinLevelImageList = new System.Windows.Forms.ImageList(this.components);
			this.suspendResumeButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.addToGroupButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// groupLabel
			// 
			this.groupLabel.AutoSize = true;
			this.groupLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupLabel.Location = new System.Drawing.Point(14, 9);
			this.groupLabel.Name = "groupLabel";
			this.groupLabel.Size = new System.Drawing.Size(53, 19);
			this.groupLabel.TabIndex = 3;
			this.groupLabel.Text = "Group";
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
            this.AvailabilityGroup,
            this.HasFullBackup,
            this.IsDataMovementSuspended});
			this.groupDatabaseListView.FullRowSelect = true;
			this.groupDatabaseListView.GridLines = true;
			this.groupDatabaseListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.groupDatabaseListView.HideSelection = false;
			this.groupDatabaseListView.Location = new System.Drawing.Point(14, 35);
			this.groupDatabaseListView.MultiSelect = false;
			this.groupDatabaseListView.Name = "groupDatabaseListView";
			this.groupDatabaseListView.ShowItemToolTips = true;
			this.groupDatabaseListView.Size = new System.Drawing.Size(739, 453);
			this.groupDatabaseListView.SmallImageList = this.joinLevelImageList;
			this.groupDatabaseListView.TabIndex = 8;
			this.groupDatabaseListView.UseCompatibleStateImageBehavior = false;
			this.groupDatabaseListView.View = System.Windows.Forms.View.Details;
			this.groupDatabaseListView.SelectedIndexChanged += new System.EventHandler(this.groupDatabaseListView_SelectedIndexChanged);
			// 
			// DbName
			// 
			this.DbName.Text = "Database";
			this.DbName.Width = 311;
			// 
			// AvailabilityGroup
			// 
			this.AvailabilityGroup.Text = "Availability Group";
			this.AvailabilityGroup.Width = 130;
			// 
			// HasFullBackup
			// 
			this.HasFullBackup.Text = "Has Full Backup";
			this.HasFullBackup.Width = 102;
			// 
			// IsDataMovementSuspended
			// 
			this.IsDataMovementSuspended.Text = "Is Data Movement Suspended";
			this.IsDataMovementSuspended.Width = 161;
			// 
			// joinLevelImageList
			// 
			this.joinLevelImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("joinLevelImageList.ImageStream")));
			this.joinLevelImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.joinLevelImageList.Images.SetKeyName(0, "basic1-180_cross_no.png");
			this.joinLevelImageList.Images.SetKeyName(1, "AlreadyInThisGroup1.png");
			this.joinLevelImageList.Images.SetKeyName(2, "basic1-179_check_yes.png");
			// 
			// suspendResumeButton
			// 
			this.suspendResumeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.suspendResumeButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.suspendResumeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.suspendResumeButton.Location = new System.Drawing.Point(758, 326);
			this.suspendResumeButton.Name = "suspendResumeButton";
			this.suspendResumeButton.Size = new System.Drawing.Size(150, 50);
			this.suspendResumeButton.TabIndex = 14;
			this.suspendResumeButton.Text = "Suspend Replication";
			this.suspendResumeButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.suspendResumeButton.UseVisualStyleBackColor = false;
			this.suspendResumeButton.Click += new System.EventHandler(this.suspendResumeButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancelButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CrossNo;
			this.cancelButton.Location = new System.Drawing.Point(758, 438);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(150, 50);
			this.cancelButton.TabIndex = 12;
			this.cancelButton.Text = "Close";
			this.cancelButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// addToGroupButton
			// 
			this.addToGroupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.addToGroupButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.addToGroupButton.Enabled = false;
			this.addToGroupButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.addToGroupButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CheckYes;
			this.addToGroupButton.Location = new System.Drawing.Point(758, 382);
			this.addToGroupButton.Name = "addToGroupButton";
			this.addToGroupButton.Size = new System.Drawing.Size(150, 50);
			this.addToGroupButton.TabIndex = 13;
			this.addToGroupButton.Text = "Add to Group";
			this.addToGroupButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.addToGroupButton.UseVisualStyleBackColor = false;
			this.addToGroupButton.Click += new System.EventHandler(this.addToGroupButton_Click);
			// 
			// CheckPrimaryDatabasesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Controls.Add(this.suspendResumeButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.addToGroupButton);
			this.Controls.Add(this.groupDatabaseListView);
			this.Controls.Add(this.groupLabel);
			this.Name = "CheckPrimaryDatabasesControl";
			this.Size = new System.Drawing.Size(918, 492);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label groupLabel;
		private System.Windows.Forms.ListView groupDatabaseListView;
		private System.Windows.Forms.ColumnHeader DbName;
		private System.Windows.Forms.ColumnHeader AvailabilityGroup;
		private System.Windows.Forms.ColumnHeader HasFullBackup;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button addToGroupButton;
		private System.Windows.Forms.ImageList joinLevelImageList;
		private System.Windows.Forms.Button suspendResumeButton;
		private System.Windows.Forms.ColumnHeader IsDataMovementSuspended;
	}
}
