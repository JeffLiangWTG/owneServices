namespace Enterprise.AlwaysOn.Setup.GUI
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.ExitImageList = new System.Windows.Forms.ImageList(this.components);
			this.secondaryReplicasPanel = new System.Windows.Forms.Panel();
			this.secondary5ReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.secondary6ReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.secondary7ReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.secondary8ReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.secondary1ReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.secondary2ReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.secondary3ReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.secondary4ReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.primaryReplicaPanel = new System.Windows.Forms.Panel();
			this.primaryReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.ReplicaControl();
			this.groupDatabaseListView = new System.Windows.Forms.ListView();
			this.DbName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.DbStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.groupStructurePanel = new System.Windows.Forms.Panel();
			this.groupInfoPanel = new System.Windows.Forms.Panel();
			this.groupInfoLabel = new System.Windows.Forms.Label();
			this.outputTextBox = new System.Windows.Forms.TextBox();
			this.selectGroupButton = new System.Windows.Forms.Button();
			this.exitButton = new System.Windows.Forms.Button();
			this.statusPanel = new System.Windows.Forms.Panel();
			this.checkPrimaryDatabasesControl = new Enterprise.AlwaysOn.Setup.GUI.CheckPrimaryDatabasesControl();
			this.checkSecondaryDatabasesControl = new Enterprise.AlwaysOn.Setup.GUI.AddDatabasesToSecondaryReplicaControl();
			this.groupConnectionControl = new Enterprise.AlwaysOn.Setup.GUI.GroupConnectionControl();
			this.addNewSecondaryReplicaControl = new Enterprise.AlwaysOn.Setup.GUI.AddNewSecondaryReplicaControl();
			this.changeReplicaSettingControl = new Enterprise.AlwaysOn.Setup.GUI.ChangeReplicaSettingControl();
			this.listenersControl = new Enterprise.AlwaysOn.Setup.GUI.ListenersControl();
			this.secondaryReplicasPanel.SuspendLayout();
			this.primaryReplicaPanel.SuspendLayout();
			this.groupStructurePanel.SuspendLayout();
			this.groupInfoPanel.SuspendLayout();
			this.statusPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExitImageList
			// 
			this.ExitImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ExitImageList.ImageStream")));
			this.ExitImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.ExitImageList.Images.SetKeyName(0, "ErrorCircle.ico");
			this.ExitImageList.Images.SetKeyName(1, "success.png");
			this.ExitImageList.Images.SetKeyName(2, "error.png");
			// 
			// secondaryReplicasPanel
			// 
			this.secondaryReplicasPanel.AutoScroll = true;
			this.secondaryReplicasPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.secondaryReplicasPanel.Controls.Add(this.secondary5ReplicaControl);
			this.secondaryReplicasPanel.Controls.Add(this.secondary6ReplicaControl);
			this.secondaryReplicasPanel.Controls.Add(this.secondary7ReplicaControl);
			this.secondaryReplicasPanel.Controls.Add(this.secondary8ReplicaControl);
			this.secondaryReplicasPanel.Controls.Add(this.secondary1ReplicaControl);
			this.secondaryReplicasPanel.Controls.Add(this.secondary2ReplicaControl);
			this.secondaryReplicasPanel.Controls.Add(this.secondary3ReplicaControl);
			this.secondaryReplicasPanel.Controls.Add(this.secondary4ReplicaControl);
			this.secondaryReplicasPanel.Location = new System.Drawing.Point(10, 225);
			this.secondaryReplicasPanel.Name = "secondaryReplicasPanel";
			this.secondaryReplicasPanel.Size = new System.Drawing.Size(898, 319);
			this.secondaryReplicasPanel.TabIndex = 48;
			this.secondaryReplicasPanel.Tag = "";
			// 
			// secondary5ReplicaControl
			// 
			this.secondary5ReplicaControl.Location = new System.Drawing.Point(748, -2);
			this.secondary5ReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.secondary5ReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.secondary5ReplicaControl.Name = "secondary5ReplicaControl";
			this.secondary5ReplicaControl.Size = new System.Drawing.Size(367, 154);
			this.secondary5ReplicaControl.TabIndex = 9;
			this.secondary5ReplicaControl.Visible = false;
			// 
			// secondary6ReplicaControl
			// 
			this.secondary6ReplicaControl.Location = new System.Drawing.Point(748, 158);
			this.secondary6ReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.secondary6ReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.secondary6ReplicaControl.Name = "secondary6ReplicaControl";
			this.secondary6ReplicaControl.Size = new System.Drawing.Size(367, 154);
			this.secondary6ReplicaControl.TabIndex = 10;
			this.secondary6ReplicaControl.Visible = false;
			// 
			// secondary7ReplicaControl
			// 
			this.secondary7ReplicaControl.Location = new System.Drawing.Point(1121, -2);
			this.secondary7ReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.secondary7ReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.secondary7ReplicaControl.Name = "secondary7ReplicaControl";
			this.secondary7ReplicaControl.Size = new System.Drawing.Size(367, 154);
			this.secondary7ReplicaControl.TabIndex = 11;
			this.secondary7ReplicaControl.Visible = false;
			// 
			// secondary8ReplicaControl
			// 
			this.secondary8ReplicaControl.Location = new System.Drawing.Point(1121, 158);
			this.secondary8ReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.secondary8ReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.secondary8ReplicaControl.Name = "secondary8ReplicaControl";
			this.secondary8ReplicaControl.Size = new System.Drawing.Size(367, 154);
			this.secondary8ReplicaControl.TabIndex = 12;
			this.secondary8ReplicaControl.Visible = false;
			// 
			// secondary1ReplicaControl
			// 
			this.secondary1ReplicaControl.Location = new System.Drawing.Point(-2, -2);
			this.secondary1ReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.secondary1ReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.secondary1ReplicaControl.Name = "secondary1ReplicaControl";
			this.secondary1ReplicaControl.Size = new System.Drawing.Size(367, 154);
			this.secondary1ReplicaControl.TabIndex = 5;
			this.secondary1ReplicaControl.Visible = false;
			// 
			// secondary2ReplicaControl
			// 
			this.secondary2ReplicaControl.Location = new System.Drawing.Point(-2, 158);
			this.secondary2ReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.secondary2ReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.secondary2ReplicaControl.Name = "secondary2ReplicaControl";
			this.secondary2ReplicaControl.Size = new System.Drawing.Size(367, 154);
			this.secondary2ReplicaControl.TabIndex = 6;
			this.secondary2ReplicaControl.Visible = false;
			// 
			// secondary3ReplicaControl
			// 
			this.secondary3ReplicaControl.Location = new System.Drawing.Point(371, -2);
			this.secondary3ReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.secondary3ReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.secondary3ReplicaControl.Name = "secondary3ReplicaControl";
			this.secondary3ReplicaControl.Size = new System.Drawing.Size(367, 154);
			this.secondary3ReplicaControl.TabIndex = 7;
			this.secondary3ReplicaControl.Visible = false;
			// 
			// secondary4ReplicaControl
			// 
			this.secondary4ReplicaControl.Location = new System.Drawing.Point(371, 158);
			this.secondary4ReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.secondary4ReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.secondary4ReplicaControl.Name = "secondary4ReplicaControl";
			this.secondary4ReplicaControl.Size = new System.Drawing.Size(367, 154);
			this.secondary4ReplicaControl.TabIndex = 8;
			this.secondary4ReplicaControl.Visible = false;
			// 
			// primaryReplicaPanel
			// 
			this.primaryReplicaPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.primaryReplicaPanel.Controls.Add(this.primaryReplicaControl);
			this.primaryReplicaPanel.Controls.Add(this.groupDatabaseListView);
			this.primaryReplicaPanel.Location = new System.Drawing.Point(10, 58);
			this.primaryReplicaPanel.Name = "primaryReplicaPanel";
			this.primaryReplicaPanel.Size = new System.Drawing.Size(898, 161);
			this.primaryReplicaPanel.TabIndex = 49;
			this.primaryReplicaPanel.Tag = "";
			// 
			// primaryReplicaControl
			// 
			this.primaryReplicaControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.primaryReplicaControl.Location = new System.Drawing.Point(-2, -2);
			this.primaryReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.primaryReplicaControl.MinimumSize = new System.Drawing.Size(367, 154);
			this.primaryReplicaControl.Name = "primaryReplicaControl";
			this.primaryReplicaControl.Size = new System.Drawing.Size(371, 154);
			this.primaryReplicaControl.TabIndex = 1;
			this.primaryReplicaControl.Visible = false;
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
            this.DbStatus});
			this.groupDatabaseListView.FullRowSelect = true;
			this.groupDatabaseListView.GridLines = true;
			this.groupDatabaseListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.groupDatabaseListView.HideSelection = false;
			this.groupDatabaseListView.Location = new System.Drawing.Point(371, 0);
			this.groupDatabaseListView.MultiSelect = false;
			this.groupDatabaseListView.Name = "groupDatabaseListView";
			this.groupDatabaseListView.Size = new System.Drawing.Size(520, 156);
			this.groupDatabaseListView.TabIndex = 2;
			this.groupDatabaseListView.UseCompatibleStateImageBehavior = false;
			this.groupDatabaseListView.View = System.Windows.Forms.View.Details;
			this.groupDatabaseListView.Visible = false;
			// 
			// DbName
			// 
			this.DbName.Text = "Database";
			this.DbName.Width = 300;
			// 
			// DbStatus
			// 
			this.DbStatus.Text = "Status";
			this.DbStatus.Width = 0;
			// 
			// groupStructurePanel
			// 
			this.groupStructurePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupStructurePanel.Controls.Add(this.groupInfoPanel);
			this.groupStructurePanel.Controls.Add(this.primaryReplicaPanel);
			this.groupStructurePanel.Controls.Add(this.secondaryReplicasPanel);
			this.groupStructurePanel.Location = new System.Drawing.Point(0, 1);
			this.groupStructurePanel.Name = "groupStructurePanel";
			this.groupStructurePanel.Size = new System.Drawing.Size(918, 546);
			this.groupStructurePanel.TabIndex = 0;
			// 
			// groupInfoPanel
			// 
			this.groupInfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupInfoPanel.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.groupInfoPanel.Controls.Add(this.groupInfoLabel);
			this.groupInfoPanel.Location = new System.Drawing.Point(0, 0);
			this.groupInfoPanel.Name = "groupInfoPanel";
			this.groupInfoPanel.Size = new System.Drawing.Size(918, 50);
			this.groupInfoPanel.TabIndex = 54;
			// 
			// groupInfoLabel
			// 
			this.groupInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupInfoLabel.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupInfoLabel.Location = new System.Drawing.Point(0, 0);
			this.groupInfoLabel.Name = "groupInfoLabel";
			this.groupInfoLabel.Size = new System.Drawing.Size(918, 50);
			this.groupInfoLabel.TabIndex = 54;
			this.groupInfoLabel.Text = "No availability group selected";
			this.groupInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// outputTextBox
			// 
			this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.outputTextBox.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.outputTextBox.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.outputTextBox.ForeColor = System.Drawing.Color.Black;
			this.outputTextBox.Location = new System.Drawing.Point(7, 11);
			this.outputTextBox.Multiline = true;
			this.outputTextBox.Name = "outputTextBox";
			this.outputTextBox.ReadOnly = true;
			this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.outputTextBox.Size = new System.Drawing.Size(748, 106);
			this.outputTextBox.TabIndex = 56;
			// 
			// selectGroupButton
			// 
			this.selectGroupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.selectGroupButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.selectGroupButton.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.selectGroupButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.Diagram;
			this.selectGroupButton.Location = new System.Drawing.Point(761, 11);
			this.selectGroupButton.Name = "selectGroupButton";
			this.selectGroupButton.Size = new System.Drawing.Size(150, 50);
			this.selectGroupButton.TabIndex = 57;
			this.selectGroupButton.Text = "Select Group";
			this.selectGroupButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.selectGroupButton.UseVisualStyleBackColor = false;
			this.selectGroupButton.Click += new System.EventHandler(this.connectToGroupButton_Click);
			// 
			// exitButton
			// 
			this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.exitButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.exitButton.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.exitButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.exit;
			this.exitButton.Location = new System.Drawing.Point(761, 67);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = new System.Drawing.Size(150, 50);
			this.exitButton.TabIndex = 2;
			this.exitButton.Text = "Exit";
			this.exitButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.exitButton.UseVisualStyleBackColor = false;
			this.exitButton.Click += new System.EventHandler(this.ExitButton_Click);
			// 
			// statusPanel
			// 
			this.statusPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.statusPanel.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
			this.statusPanel.Controls.Add(this.outputTextBox);
			this.statusPanel.Controls.Add(this.selectGroupButton);
			this.statusPanel.Controls.Add(this.exitButton);
			this.statusPanel.Location = new System.Drawing.Point(0, 553);
			this.statusPanel.Name = "statusPanel";
			this.statusPanel.Size = new System.Drawing.Size(918, 131);
			this.statusPanel.TabIndex = 55;
			// 
			// checkPrimaryDatabasesControl
			// 
			this.checkPrimaryDatabasesControl.Location = new System.Drawing.Point(0, 55);
			this.checkPrimaryDatabasesControl.Margin = new System.Windows.Forms.Padding(5);
			this.checkPrimaryDatabasesControl.Name = "checkPrimaryDatabasesControl";
			this.checkPrimaryDatabasesControl.Size = new System.Drawing.Size(918, 492);
			this.checkPrimaryDatabasesControl.TabIndex = 59;
			// 
			// checkSecondaryDatabasesControl
			// 
			this.checkSecondaryDatabasesControl.Location = new System.Drawing.Point(0, 55);
			this.checkSecondaryDatabasesControl.Margin = new System.Windows.Forms.Padding(5);
			this.checkSecondaryDatabasesControl.Name = "checkSecondaryDatabasesControl";
			this.checkSecondaryDatabasesControl.Size = new System.Drawing.Size(918, 492);
			this.checkSecondaryDatabasesControl.TabIndex = 60;
			// 
			// groupConnectionControl
			// 
			this.groupConnectionControl.Location = new System.Drawing.Point(0, 55);
			this.groupConnectionControl.Margin = new System.Windows.Forms.Padding(2);
			this.groupConnectionControl.Name = "groupConnectionControl";
			this.groupConnectionControl.Size = new System.Drawing.Size(918, 492);
			this.groupConnectionControl.TabIndex = 0;
			// 
			// addNewSecondaryReplicaControl
			// 
			this.addNewSecondaryReplicaControl.Location = new System.Drawing.Point(0, 55);
			this.addNewSecondaryReplicaControl.Margin = new System.Windows.Forms.Padding(5);
			this.addNewSecondaryReplicaControl.Name = "addNewSecondaryReplicaControl";
			this.addNewSecondaryReplicaControl.Size = new System.Drawing.Size(918, 492);
			this.addNewSecondaryReplicaControl.TabIndex = 58;
			// 
			// changeReplicaSettingControl
			// 
			this.changeReplicaSettingControl.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.changeReplicaSettingControl.Location = new System.Drawing.Point(0, 55);
			this.changeReplicaSettingControl.Margin = new System.Windows.Forms.Padding(5);
			this.changeReplicaSettingControl.Name = "changeReplicaSettingControl";
			this.changeReplicaSettingControl.Size = new System.Drawing.Size(206, 68);
			this.changeReplicaSettingControl.TabIndex = 61;
			// 
			// listenersControl
			// 
			this.listenersControl.AvailabilityGroup = null;
			this.listenersControl.DbServer = null;
			this.listenersControl.Location = new System.Drawing.Point(0, 55);
			this.listenersControl.Name = "listenersControl";
			this.listenersControl.Size = new System.Drawing.Size(918, 492);
			this.listenersControl.TabIndex = 62;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(918, 679);
			this.Controls.Add(this.groupStructurePanel);
			this.Controls.Add(this.checkPrimaryDatabasesControl);
			this.Controls.Add(this.checkSecondaryDatabasesControl);
			this.Controls.Add(this.groupConnectionControl);
			this.Controls.Add(this.statusPanel);
			this.Controls.Add(this.addNewSecondaryReplicaControl);
			this.Controls.Add(this.changeReplicaSettingControl);
			this.Controls.Add(this.listenersControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.Opacity = 0.99D;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "AlwaysOn Availability Group Setup";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AlwaysOnSetupForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.secondaryReplicasPanel.ResumeLayout(false);
			this.primaryReplicaPanel.ResumeLayout(false);
			this.groupStructurePanel.ResumeLayout(false);
			this.groupInfoPanel.ResumeLayout(false);
			this.statusPanel.ResumeLayout(false);
			this.statusPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ImageList ExitImageList;
		private System.Windows.Forms.Button exitButton;
		private System.Windows.Forms.Panel secondaryReplicasPanel;
		private ReplicaControl secondary1ReplicaControl;
		private ReplicaControl secondary4ReplicaControl;
		private ReplicaControl secondary3ReplicaControl;
		private System.Windows.Forms.Panel primaryReplicaPanel;
		private ReplicaControl primaryReplicaControl;
		private System.Windows.Forms.ListView groupDatabaseListView;
		private System.Windows.Forms.ColumnHeader DbName;
		private System.Windows.Forms.ColumnHeader DbStatus;
		private System.Windows.Forms.Panel groupStructurePanel;
		private System.Windows.Forms.Panel groupInfoPanel;
		private System.Windows.Forms.Label groupInfoLabel;
		private ReplicaControl secondary2ReplicaControl;
		private System.Windows.Forms.TextBox outputTextBox;
		private System.Windows.Forms.Button selectGroupButton;
		private GroupConnectionControl groupConnectionControl;
		private AddNewSecondaryReplicaControl addNewSecondaryReplicaControl;
		private CheckPrimaryDatabasesControl checkPrimaryDatabasesControl;
		private AddDatabasesToSecondaryReplicaControl checkSecondaryDatabasesControl;
		private ChangeReplicaSettingControl changeReplicaSettingControl;
		private System.Windows.Forms.Panel statusPanel;
		private ListenersControl listenersControl;
		private ReplicaControl secondary5ReplicaControl;
		private ReplicaControl secondary6ReplicaControl;
		private ReplicaControl secondary7ReplicaControl;
		private ReplicaControl secondary8ReplicaControl;
	}
}

