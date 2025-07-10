namespace Enterprise.AlwaysOn.Setup.GUI
{
	partial class GroupConnectionControl
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
			this.dbServerLabel = new System.Windows.Forms.Label();
			this.dbServerTextBox = new System.Windows.Forms.TextBox();
			this.mainDatabaseNameLabel = new System.Windows.Forms.Label();
			this.databaseComboBox = new System.Windows.Forms.ComboBox();
			this.agTextBox = new System.Windows.Forms.TextBox();
			this.agLabel = new System.Windows.Forms.Label();
			this.endpointPortTextBox = new System.Windows.Forms.TextBox();
			this.endpointPortLabel = new System.Windows.Forms.Label();
			this.connectButton = new System.Windows.Forms.Button();
			this.createGroupButton = new System.Windows.Forms.Button();
			this.showReplicasButton = new System.Windows.Forms.Button();
			this.showListenersButton = new System.Windows.Forms.Button();
			this.portTextBox = new System.Windows.Forms.TextBox();
			this.portLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// dbServerLabel
			// 
			this.dbServerLabel.AutoSize = true;
			this.dbServerLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dbServerLabel.Location = new System.Drawing.Point(11, 10);
			this.dbServerLabel.Name = "dbServerLabel";
			this.dbServerLabel.Size = new System.Drawing.Size(376, 19);
			this.dbServerLabel.TabIndex = 0;
			this.dbServerLabel.Text = "Primary Database Server (including instance name):";
			// 
			// dbServerTextBox
			// 
			this.dbServerTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.dbServerTextBox.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dbServerTextBox.Location = new System.Drawing.Point(12, 32);
			this.dbServerTextBox.Name = "dbServerTextBox";
			this.dbServerTextBox.Size = new System.Drawing.Size(736, 33);
			this.dbServerTextBox.TabIndex = 1;
			this.dbServerTextBox.Tag = "(enter database server + instance)";
			this.dbServerTextBox.TextChanged += new System.EventHandler(this.dbServerTextBox_TextChanged);
			this.dbServerTextBox.Enter += new System.EventHandler(this.dbServerTextBox_Enter);
			this.dbServerTextBox.Leave += new System.EventHandler(this.dbServerTextBox_Leave);
			// 
			// mainDatabaseNameLabel
			// 
			this.mainDatabaseNameLabel.AutoSize = true;
			this.mainDatabaseNameLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.mainDatabaseNameLabel.Location = new System.Drawing.Point(11, 77);
			this.mainDatabaseNameLabel.Name = "mainDatabaseNameLabel";
			this.mainDatabaseNameLabel.Size = new System.Drawing.Size(79, 19);
			this.mainDatabaseNameLabel.TabIndex = 3;
			this.mainDatabaseNameLabel.Text = "Database:";
			// 
			// databaseComboBox
			// 
			this.databaseComboBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.databaseComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.databaseComboBox.Enabled = false;
			this.databaseComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.databaseComboBox.Font = new System.Drawing.Font("Verdana", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.databaseComboBox.FormattingEnabled = true;
			this.databaseComboBox.Location = new System.Drawing.Point(13, 99);
			this.databaseComboBox.Name = "databaseComboBox";
			this.databaseComboBox.Size = new System.Drawing.Size(891, 33);
			this.databaseComboBox.TabIndex = 4;
			this.databaseComboBox.SelectedIndexChanged += new System.EventHandler(this.databaseComboBox_SelectedIndexChanged);
			this.databaseComboBox.Enter += new System.EventHandler(this.databaseComboBox_Enter);
			this.databaseComboBox.Leave += new System.EventHandler(this.databaseComboBox_Leave);
			// 
			// agTextBox
			// 
			this.agTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.agTextBox.Enabled = false;
			this.agTextBox.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.agTextBox.Location = new System.Drawing.Point(12, 235);
			this.agTextBox.Name = "agTextBox";
			this.agTextBox.Size = new System.Drawing.Size(807, 33);
			this.agTextBox.TabIndex = 8;
			this.agTextBox.Tag = "(enter new group name)";
			this.agTextBox.TextChanged += new System.EventHandler(this.agTextBox_TextChanged);
			this.agTextBox.Enter += new System.EventHandler(this.agTextBox_Enter);
			this.agTextBox.Leave += new System.EventHandler(this.agTextBox_Leave);
			// 
			// agLabel
			// 
			this.agLabel.AutoSize = true;
			this.agLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.agLabel.Location = new System.Drawing.Point(11, 213);
			this.agLabel.Name = "agLabel";
			this.agLabel.Size = new System.Drawing.Size(187, 19);
			this.agLabel.TabIndex = 7;
			this.agLabel.Text = "Availability Group Name:";
			// 
			// endpointPortTextBox
			// 
			this.endpointPortTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.endpointPortTextBox.Enabled = false;
			this.endpointPortTextBox.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.endpointPortTextBox.Location = new System.Drawing.Point(12, 167);
			this.endpointPortTextBox.Name = "endpointPortTextBox";
			this.endpointPortTextBox.Size = new System.Drawing.Size(892, 33);
			this.endpointPortTextBox.TabIndex = 5;
			this.endpointPortTextBox.Tag = "(enter endpoint port)";
			this.endpointPortTextBox.TextChanged += new System.EventHandler(this.endpointPortTextBox_TextChanged);
			this.endpointPortTextBox.Enter += new System.EventHandler(this.endpointPortTextBox_Enter);
			this.endpointPortTextBox.Leave += new System.EventHandler(this.endpointPortTextBox_Leave);
			// 
			// endpointPortLabel
			// 
			this.endpointPortLabel.AutoSize = true;
			this.endpointPortLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.endpointPortLabel.Location = new System.Drawing.Point(11, 145);
			this.endpointPortLabel.Name = "endpointPortLabel";
			this.endpointPortLabel.Size = new System.Drawing.Size(188, 19);
			this.endpointPortLabel.TabIndex = 5;
			this.endpointPortLabel.Text = "AlwaysOn Endpoint Port:";
			// 
			// connectButton
			// 
			this.connectButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.connectButton.Enabled = false;
			this.connectButton.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.connectButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.Database16;
			this.connectButton.Location = new System.Drawing.Point(825, 31);
			this.connectButton.Margin = new System.Windows.Forms.Padding(2);
			this.connectButton.Name = "connectButton";
			this.connectButton.Size = new System.Drawing.Size(79, 34);
			this.connectButton.TabIndex = 3;
			this.connectButton.Text = "Connect";
			this.connectButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.connectButton.UseVisualStyleBackColor = false;
			this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
			// 
			// createGroupButton
			// 
			this.createGroupButton.Enabled = false;
			this.createGroupButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.createGroupButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.Plus16;
			this.createGroupButton.Location = new System.Drawing.Point(825, 234);
			this.createGroupButton.Margin = new System.Windows.Forms.Padding(2);
			this.createGroupButton.Name = "createGroupButton";
			this.createGroupButton.Size = new System.Drawing.Size(79, 34);
			this.createGroupButton.TabIndex = 9;
			this.createGroupButton.Text = "Create";
			this.createGroupButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.createGroupButton.UseVisualStyleBackColor = false;
			this.createGroupButton.Click += new System.EventHandler(this.createGroupButton_Click);
			// 
			// showReplicasButton
			// 
			this.showReplicasButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.showReplicasButton.Enabled = false;
			this.showReplicasButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.showReplicasButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CheckYes;
			this.showReplicasButton.Location = new System.Drawing.Point(598, 300);
			this.showReplicasButton.Margin = new System.Windows.Forms.Padding(2);
			this.showReplicasButton.Name = "showReplicasButton";
			this.showReplicasButton.Size = new System.Drawing.Size(150, 50);
			this.showReplicasButton.TabIndex = 11;
			this.showReplicasButton.Text = "Show Replicas";
			this.showReplicasButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.showReplicasButton.UseVisualStyleBackColor = false;
			this.showReplicasButton.Click += new System.EventHandler(this.showReplicasButton_Click);
			// 
			// showListenersButton
			// 
			this.showListenersButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.showListenersButton.Enabled = false;
			this.showListenersButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.showListenersButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.Plus32;
			this.showListenersButton.Location = new System.Drawing.Point(754, 300);
			this.showListenersButton.Name = "showListenersButton";
			this.showListenersButton.Size = new System.Drawing.Size(150, 50);
			this.showListenersButton.TabIndex = 12;
			this.showListenersButton.Text = "Show Listener";
			this.showListenersButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.showListenersButton.UseVisualStyleBackColor = false;
			this.showListenersButton.Click += new System.EventHandler(this.showListenersButton_Click);
			// 
			// portTextBox
			// 
			this.portTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.portTextBox.Font = new System.Drawing.Font("Tahoma", 15.75F);
			this.portTextBox.Location = new System.Drawing.Point(758, 32);
			this.portTextBox.MaxLength = 5;
			this.portTextBox.Name = "portTextBox";
			this.portTextBox.Size = new System.Drawing.Size(62, 33);
			this.portTextBox.TabIndex = 2;
			this.portTextBox.Tag = "(enter port number. Default value = 1433)";
			this.portTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.portTextBox.TextChanged += new System.EventHandler(this.portTextBox_TextChanged);
			// 
			// portLabel
			// 
			this.portLabel.AutoSize = true;
			this.portLabel.Font = new System.Drawing.Font("Tahoma", 12F);
			this.portLabel.Location = new System.Drawing.Point(761, 10);
			this.portLabel.Name = "portLabel";
			this.portLabel.Size = new System.Drawing.Size(44, 19);
			this.portLabel.TabIndex = 32;
			this.portLabel.Text = "Port:";
			// 
			// GroupConnectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.portTextBox);
			this.Controls.Add(this.portLabel);
			this.Controls.Add(this.showListenersButton);
			this.Controls.Add(this.showReplicasButton);
			this.Controls.Add(this.dbServerLabel);
			this.Controls.Add(this.dbServerTextBox);
			this.Controls.Add(this.mainDatabaseNameLabel);
			this.Controls.Add(this.databaseComboBox);
			this.Controls.Add(this.connectButton);
			this.Controls.Add(this.agTextBox);
			this.Controls.Add(this.agLabel);
			this.Controls.Add(this.endpointPortTextBox);
			this.Controls.Add(this.endpointPortLabel);
			this.Controls.Add(this.createGroupButton);
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "GroupConnectionControl";
			this.Size = new System.Drawing.Size(918, 492);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label dbServerLabel;
		private System.Windows.Forms.TextBox dbServerTextBox;
		private System.Windows.Forms.Label mainDatabaseNameLabel;
		protected System.Windows.Forms.ComboBox databaseComboBox;
		private System.Windows.Forms.Button connectButton;
		private System.Windows.Forms.TextBox agTextBox;
		private System.Windows.Forms.Label agLabel;
		private System.Windows.Forms.TextBox endpointPortTextBox;
		private System.Windows.Forms.Label endpointPortLabel;
		private System.Windows.Forms.Button createGroupButton;
		private System.Windows.Forms.Button showReplicasButton;
		private System.Windows.Forms.Button showListenersButton;
		protected System.Windows.Forms.TextBox portTextBox;
		private System.Windows.Forms.Label portLabel;
	}
}
