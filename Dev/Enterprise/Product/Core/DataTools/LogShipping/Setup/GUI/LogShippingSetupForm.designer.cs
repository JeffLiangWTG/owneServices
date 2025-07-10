namespace Enterprise.LogShipping.Setup.GUI
{

	partial class LogShippingSetupForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogShippingSetupForm));
			this.NextButton = new System.Windows.Forms.Button();
			this.NextImageList = new System.Windows.Forms.ImageList(this.components);
			this.ExitButton = new System.Windows.Forms.Button();
			this.ExitImageList = new System.Windows.Forms.ImageList(this.components);
			this.BackButton = new System.Windows.Forms.Button();
			this.BackImageList = new System.Windows.Forms.ImageList(this.components);
			this.stepsGroupBox = new System.Windows.Forms.GroupBox();
			this.TitleLabel_FinalSetUp = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.TitleLabel_InitSecondaryDb = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.TitleLabel_BkpLocalCopyDir = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.TitleLabel_BkpSourceDir = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.TitleLabel_SecondaryDatabase = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.TitleLabel_SecondaryServer = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.TitleLabel_PrimaryDatabase = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.TitleLabel_PrimaryServer = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.secondaryDatabaseControl = new Enterprise.LogShipping.Setup.GUI.ScreenControls.SecondaryDatabaseControl();
			this.primaryServerControl = new Enterprise.LogShipping.Setup.GUI.PrimaryServerControl();
			this.primaryDatabaseControl = new Enterprise.LogShipping.Setup.GUI.PrimaryDatabaseControl();
			this.primaryEDocsDatabaseControl = new Enterprise.LogShipping.Setup.GUI.PrimaryEDocsDatabaseControl();
			this.secondaryServerControl = new Enterprise.LogShipping.Setup.GUI.SecondaryServerControl();
			this.sourceDirectoryControl = new Enterprise.LogShipping.Setup.GUI.SourceDirectoryControl();
			this.localCopyDirectoryControl = new Enterprise.LogShipping.Setup.GUI.LocalCopyDirectoryControl();
			this.initializeSecondaryDbControl = new Enterprise.LogShipping.Setup.GUI.InitializeSecondaryDbControl();
			this.finalSetupScreenControl = new Enterprise.LogShipping.Setup.GUI.FinalSetupScreenControl();
			this.TitleLabel_EDocsDatabases = new Enterprise.LogShipping.Setup.GUI.TaskStatusControl();
			this.stepsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// NextButton
			// 
			this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.NextButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.NextButton.FlatAppearance.BorderSize = 0;
			this.NextButton.ImageIndex = 0;
			this.NextButton.ImageList = this.NextImageList;
			this.NextButton.Location = new System.Drawing.Point(286, 295);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = new System.Drawing.Size(48, 29);
			this.NextButton.TabIndex = 3;
			this.NextButton.UseVisualStyleBackColor = false;
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// NextImageList
			// 
			this.NextImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("NextImageList.ImageStream")));
			this.NextImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.NextImageList.Images.SetKeyName(0, "next.png");
			// 
			// ExitButton
			// 
			this.ExitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExitButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.ExitButton.FlatAppearance.BorderSize = 0;
			this.ExitButton.ImageIndex = 0;
			this.ExitButton.ImageList = this.ExitImageList;
			this.ExitButton.Location = new System.Drawing.Point(648, 290);
			this.ExitButton.Name = "ExitButton";
			this.ExitButton.Size = new System.Drawing.Size(38, 38);
			this.ExitButton.TabIndex = 4;
			this.ExitButton.UseVisualStyleBackColor = false;
			this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
			// 
			// ExitImageList
			// 
			this.ExitImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ExitImageList.ImageStream")));
			this.ExitImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ExitImageList.Images.SetKeyName(0, "ErrorCircle.ico");
			this.ExitImageList.Images.SetKeyName(1, "success.png");
			this.ExitImageList.Images.SetKeyName(2, "error.png");
			// 
			// BackButton
			// 
			this.BackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BackButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.BackButton.FlatAppearance.BorderSize = 0;
			this.BackButton.ImageIndex = 0;
			this.BackButton.ImageList = this.BackImageList;
			this.BackButton.Location = new System.Drawing.Point(232, 295);
			this.BackButton.Name = "BackButton";
			this.BackButton.Size = new System.Drawing.Size(48, 29);
			this.BackButton.TabIndex = 2;
			this.BackButton.UseVisualStyleBackColor = false;
			this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
			// 
			// BackImageList
			// 
			this.BackImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("BackImageList.ImageStream")));
			this.BackImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.BackImageList.Images.SetKeyName(0, "back.png");
			// 
			// stepsGroupBox
			// 
			this.stepsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.stepsGroupBox.Controls.Add(this.TitleLabel_EDocsDatabases);
			this.stepsGroupBox.Controls.Add(this.TitleLabel_FinalSetUp);
			this.stepsGroupBox.Controls.Add(this.TitleLabel_InitSecondaryDb);
			this.stepsGroupBox.Controls.Add(this.TitleLabel_BkpLocalCopyDir);
			this.stepsGroupBox.Controls.Add(this.TitleLabel_BkpSourceDir);
			this.stepsGroupBox.Controls.Add(this.TitleLabel_SecondaryDatabase);
			this.stepsGroupBox.Controls.Add(this.TitleLabel_SecondaryServer);
			this.stepsGroupBox.Controls.Add(this.TitleLabel_PrimaryDatabase);
			this.stepsGroupBox.Controls.Add(this.TitleLabel_PrimaryServer);
			this.stepsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.stepsGroupBox.Location = new System.Drawing.Point(12, 12);
			this.stepsGroupBox.Name = "stepsGroupBox";
			this.stepsGroupBox.Size = new System.Drawing.Size(213, 308);
			this.stepsGroupBox.TabIndex = 0;
			this.stepsGroupBox.TabStop = false;
			this.stepsGroupBox.Text = "Step 1 of 8";
			// 
			// TitleLabel_FinalSetUp
			// 
			this.TitleLabel_FinalSetUp.AutoSize = true;
			this.TitleLabel_FinalSetUp.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TitleLabel_FinalSetUp.ForeColor = System.Drawing.Color.Black;
			this.TitleLabel_FinalSetUp.Location = new System.Drawing.Point(6, 268);
			this.TitleLabel_FinalSetUp.Name = "TitleLabel_FinalSetUp";
			this.TitleLabel_FinalSetUp.Size = new System.Drawing.Size(178, 23);
			this.TitleLabel_FinalSetUp.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_FinalSetUp.TabIndex = 8;
			this.TitleLabel_FinalSetUp.TextLabel = "Set Up Log Shipping";
			// 
			// TitleLabel_InitSecondaryDb
			// 
			this.TitleLabel_InitSecondaryDb.AutoSize = true;
			this.TitleLabel_InitSecondaryDb.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TitleLabel_InitSecondaryDb.ForeColor = System.Drawing.Color.Black;
			this.TitleLabel_InitSecondaryDb.Location = new System.Drawing.Point(6, 237);
			this.TitleLabel_InitSecondaryDb.Name = "TitleLabel_InitSecondaryDb";
			this.TitleLabel_InitSecondaryDb.Size = new System.Drawing.Size(200, 23);
			this.TitleLabel_InitSecondaryDb.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_InitSecondaryDb.TabIndex = 7;
			this.TitleLabel_InitSecondaryDb.TextLabel = "Initialize Secondary Database";
			// 
			// TitleLabel_BkpLocalCopyDir
			// 
			this.TitleLabel_BkpLocalCopyDir.AutoSize = true;
			this.TitleLabel_BkpLocalCopyDir.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TitleLabel_BkpLocalCopyDir.ForeColor = System.Drawing.Color.Black;
			this.TitleLabel_BkpLocalCopyDir.Location = new System.Drawing.Point(6, 206);
			this.TitleLabel_BkpLocalCopyDir.Name = "TitleLabel_BkpLocalCopyDir";
			this.TitleLabel_BkpLocalCopyDir.Size = new System.Drawing.Size(194, 23);
			this.TitleLabel_BkpLocalCopyDir.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_BkpLocalCopyDir.TabIndex = 6;
			this.TitleLabel_BkpLocalCopyDir.TextLabel = "Backup Local Copy Directory";
			// 
			// TitleLabel_BkpSourceDir
			// 
			this.TitleLabel_BkpSourceDir.AutoSize = true;
			this.TitleLabel_BkpSourceDir.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TitleLabel_BkpSourceDir.ForeColor = System.Drawing.Color.Black;
			this.TitleLabel_BkpSourceDir.Location = new System.Drawing.Point(6, 175);
			this.TitleLabel_BkpSourceDir.Name = "TitleLabel_BkpSourceDir";
			this.TitleLabel_BkpSourceDir.Size = new System.Drawing.Size(188, 23);
			this.TitleLabel_BkpSourceDir.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_BkpSourceDir.TabIndex = 5;
			this.TitleLabel_BkpSourceDir.TextLabel = "Backup Source Directory";
			// 
			// TitleLabel_SecondaryDatabase
			// 
			this.TitleLabel_SecondaryDatabase.AutoSize = true;
			this.TitleLabel_SecondaryDatabase.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TitleLabel_SecondaryDatabase.ForeColor = System.Drawing.Color.Black;
			this.TitleLabel_SecondaryDatabase.Location = new System.Drawing.Point(6, 51);
			this.TitleLabel_SecondaryDatabase.Name = "TitleLabel_SecondaryDatabase";
			this.TitleLabel_SecondaryDatabase.Size = new System.Drawing.Size(178, 23);
			this.TitleLabel_SecondaryDatabase.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_SecondaryDatabase.TabIndex = 1;
			this.TitleLabel_SecondaryDatabase.TextLabel = "Secondary Database";
			// 
			// TitleLabel_SecondaryServer
			// 
			this.TitleLabel_SecondaryServer.AutoSize = true;
			this.TitleLabel_SecondaryServer.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TitleLabel_SecondaryServer.ForeColor = System.Drawing.Color.Black;
			this.TitleLabel_SecondaryServer.Location = new System.Drawing.Point(6, 20);
			this.TitleLabel_SecondaryServer.Name = "TitleLabel_SecondaryServer";
			this.TitleLabel_SecondaryServer.Size = new System.Drawing.Size(178, 23);
			this.TitleLabel_SecondaryServer.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_SecondaryServer.TabIndex = 0;
			this.TitleLabel_SecondaryServer.TextLabel = "Secondary Server";
			// 
			// TitleLabel_PrimaryDatabase
			// 
			this.TitleLabel_PrimaryDatabase.AutoSize = true;
			this.TitleLabel_PrimaryDatabase.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TitleLabel_PrimaryDatabase.ForeColor = System.Drawing.Color.Black;
			this.TitleLabel_PrimaryDatabase.Location = new System.Drawing.Point(6, 113);
			this.TitleLabel_PrimaryDatabase.Name = "TitleLabel_PrimaryDatabase";
			this.TitleLabel_PrimaryDatabase.Size = new System.Drawing.Size(178, 23);
			this.TitleLabel_PrimaryDatabase.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_PrimaryDatabase.TabIndex = 3;
			this.TitleLabel_PrimaryDatabase.TextLabel = "Primary Database";
			// 
			// TitleLabel_PrimaryServer
			// 
			this.TitleLabel_PrimaryServer.AutoSize = true;
			this.TitleLabel_PrimaryServer.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TitleLabel_PrimaryServer.Location = new System.Drawing.Point(6, 82);
			this.TitleLabel_PrimaryServer.Name = "TitleLabel_PrimaryServer";
			this.TitleLabel_PrimaryServer.Size = new System.Drawing.Size(178, 23);
			this.TitleLabel_PrimaryServer.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_PrimaryServer.TabIndex = 2;
			this.TitleLabel_PrimaryServer.TextLabel = "Primary Server";
			// 
			// secondaryDatabaseControl
			// 
			this.secondaryDatabaseControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.secondaryDatabaseControl.Location = new System.Drawing.Point(232, 13);
			this.secondaryDatabaseControl.Name = "secondaryDatabaseControl";
			this.secondaryDatabaseControl.Size = new System.Drawing.Size(458, 276);
			this.secondaryDatabaseControl.TabIndex = 38;
			this.secondaryDatabaseControl.Visible = false;
			// 
			// primaryServerControl
			// 
			this.primaryServerControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.primaryServerControl.Location = new System.Drawing.Point(232, 13);
			this.primaryServerControl.Name = "primaryServerControl";
			this.primaryServerControl.Size = new System.Drawing.Size(458, 276);
			this.primaryServerControl.TabIndex = 1;
			this.primaryServerControl.Visible = false;
			// 
			// primaryDatabaseControl
			// 
			this.primaryDatabaseControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.primaryDatabaseControl.Location = new System.Drawing.Point(232, 13);
			this.primaryDatabaseControl.Name = "primaryDatabaseControl";
			this.primaryDatabaseControl.Size = new System.Drawing.Size(458, 276);
			this.primaryDatabaseControl.TabIndex = 5;
			this.primaryDatabaseControl.Visible = false;
			// 
			// primaryEDocsDatabaseControl
			// 
			this.primaryEDocsDatabaseControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.primaryEDocsDatabaseControl.Location = new System.Drawing.Point(232, 13);
			this.primaryEDocsDatabaseControl.Name = "primaryEDocsDatabaseControl";
			this.primaryEDocsDatabaseControl.Size = new System.Drawing.Size(458, 276);
			this.primaryEDocsDatabaseControl.TabIndex = 5;
			this.primaryEDocsDatabaseControl.Visible = false;
			// 
			// secondaryServerControl
			// 
			this.secondaryServerControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.secondaryServerControl.Location = new System.Drawing.Point(232, 13);
			this.secondaryServerControl.Name = "secondaryServerControl";
			this.secondaryServerControl.Size = new System.Drawing.Size(458, 276);
			this.secondaryServerControl.TabIndex = 35;
			this.secondaryServerControl.Visible = false;
			// 
			// sourceDirectoryControl
			// 
			this.sourceDirectoryControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sourceDirectoryControl.Location = new System.Drawing.Point(232, 13);
			this.sourceDirectoryControl.Name = "sourceDirectoryControl";
			this.sourceDirectoryControl.Size = new System.Drawing.Size(458, 276);
			this.sourceDirectoryControl.TabIndex = 34;
			this.sourceDirectoryControl.Visible = false;
			// 
			// localCopyDirectoryControl
			// 
			this.localCopyDirectoryControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.localCopyDirectoryControl.Location = new System.Drawing.Point(232, 13);
			this.localCopyDirectoryControl.Name = "localCopyDirectoryControl";
			this.localCopyDirectoryControl.Size = new System.Drawing.Size(458, 276);
			this.localCopyDirectoryControl.TabIndex = 33;
			this.localCopyDirectoryControl.Visible = false;
			// 
			// initializeSecondaryDbControl
			// 
			this.initializeSecondaryDbControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.initializeSecondaryDbControl.Location = new System.Drawing.Point(232, 13);
			this.initializeSecondaryDbControl.Name = "initializeSecondaryDbControl";
			this.initializeSecondaryDbControl.Size = new System.Drawing.Size(458, 276);
			this.initializeSecondaryDbControl.TabIndex = 32;
			this.initializeSecondaryDbControl.Visible = false;
			// 
			// finalSetupScreenControl
			// 
			this.finalSetupScreenControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.finalSetupScreenControl.Location = new System.Drawing.Point(232, 13);
			this.finalSetupScreenControl.Name = "finalSetupScreenControl";
			this.finalSetupScreenControl.Size = new System.Drawing.Size(458, 276);
			this.finalSetupScreenControl.TabIndex = 31;
			this.finalSetupScreenControl.Visible = false;
			// 
			// TitleLabel_EDocsDatabases
			// 
			this.TitleLabel_EDocsDatabases.AutoSize = true;
			this.TitleLabel_EDocsDatabases.Location = new System.Drawing.Point(6, 144);
			this.TitleLabel_EDocsDatabases.Name = "TitleLabel_EDocsDatabases";
			this.TitleLabel_EDocsDatabases.Size = new System.Drawing.Size(181, 23);
			this.TitleLabel_EDocsDatabases.Status = Enterprise.LogShipping.Setup.GUI.TaskStatus.Initial;
			this.TitleLabel_EDocsDatabases.TabIndex = 4;
			this.TitleLabel_EDocsDatabases.TextLabel = "eDocs Databases";
			// 
			// LogShippingSetupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(698, 332);
			this.Controls.Add(this.primaryServerControl);
			this.Controls.Add(this.primaryDatabaseControl);
			this.Controls.Add(this.primaryEDocsDatabaseControl);
			this.Controls.Add(this.secondaryServerControl);
			this.Controls.Add(this.sourceDirectoryControl);
			this.Controls.Add(this.localCopyDirectoryControl);
			this.Controls.Add(this.initializeSecondaryDbControl);
			this.Controls.Add(this.finalSetupScreenControl);
			this.Controls.Add(this.ExitButton);
			this.Controls.Add(this.NextButton);
			this.Controls.Add(this.stepsGroupBox);
			this.Controls.Add(this.BackButton);
			this.Controls.Add(this.secondaryDatabaseControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LogShippingSetupForm";
			this.Opacity = 0.99;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Database Log Shipping Setup";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogShippingSetupForm_FormClosing);
			this.stepsGroupBox.ResumeLayout(false);
			this.stepsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList ExitImageList;
		private TaskStatusControl TitleLabel_FinalSetUp;
		private TaskStatusControl TitleLabel_InitSecondaryDb;
		private TaskStatusControl TitleLabel_BkpLocalCopyDir;
		private TaskStatusControl TitleLabel_BkpSourceDir;
		private TaskStatusControl TitleLabel_SecondaryServer;
		private TaskStatusControl TitleLabel_PrimaryDatabase;
		private TaskStatusControl TitleLabel_PrimaryServer;
		private System.Windows.Forms.ImageList BackImageList;
		private System.Windows.Forms.ImageList NextImageList;
		private System.Windows.Forms.Button BackButton;
		private System.Windows.Forms.Button ExitButton;
		private System.Windows.Forms.Button NextButton;
		private System.Windows.Forms.GroupBox stepsGroupBox;
		private FinalSetupScreenControl finalSetupScreenControl;
		private InitializeSecondaryDbControl initializeSecondaryDbControl;
		private LocalCopyDirectoryControl localCopyDirectoryControl;
		private SourceDirectoryControl sourceDirectoryControl;
		private SecondaryServerControl secondaryServerControl;
		private PrimaryDatabaseControl primaryDatabaseControl;
		private PrimaryEDocsDatabaseControl primaryEDocsDatabaseControl;
		private PrimaryServerControl primaryServerControl;
		private TaskStatusControl TitleLabel_SecondaryDatabase;
		private Enterprise.LogShipping.Setup.GUI.ScreenControls.SecondaryDatabaseControl secondaryDatabaseControl;
		private TaskStatusControl TitleLabel_EDocsDatabases;




	}
}
