using System.Windows.Forms;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	partial class ReplicaControl
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
			this.roleLabel = new System.Windows.Forms.Label();
			this.serverLabel = new System.Windows.Forms.Label();
			this.availabilityModeLabel = new System.Windows.Forms.Label();
			this.nodeLabel = new System.Windows.Forms.Label();
			this.failoverModeLabel = new System.Windows.Forms.Label();
			this.allowConnectionLabel = new System.Windows.Forms.Label();
			this.databasePanel = new System.Windows.Forms.Panel();
			this.healthPanel = new System.Windows.Forms.Panel();
			this.pendingActionsLink = new System.Windows.Forms.LinkLabel();
			this.databasePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// roleLabel
			// 
			this.roleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.roleLabel.AutoSize = true;
			this.roleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.roleLabel.Location = new System.Drawing.Point(128, 9);
			this.roleLabel.Name = "roleLabel";
			this.roleLabel.Size = new System.Drawing.Size(236, 20);
			this.roleLabel.TabIndex = 37;
			this.roleLabel.Text = "PRIMARY/SECONDARY (FCI)";
			// 
			// serverLabel
			// 
			this.serverLabel.AutoSize = true;
			this.serverLabel.Location = new System.Drawing.Point(129, 31);
			this.serverLabel.Name = "serverLabel";
			this.serverLabel.Size = new System.Drawing.Size(149, 15);
			this.serverLabel.TabIndex = 38;
			this.serverLabel.Text = "Server: SYD-XXX-1\\V2012";
			// 
			// availabilityModeLabel
			// 
			this.availabilityModeLabel.AutoSize = true;
			this.availabilityModeLabel.Location = new System.Drawing.Point(129, 70);
			this.availabilityModeLabel.Name = "availabilityModeLabel";
			this.availabilityModeLabel.Size = new System.Drawing.Size(164, 15);
			this.availabilityModeLabel.TabIndex = 39;
			this.availabilityModeLabel.Text = "ASYNCHRONOUS_COMMIT";
			// 
			// nodeLabel
			// 
			this.nodeLabel.AutoSize = true;
			this.nodeLabel.Location = new System.Drawing.Point(129, 48);
			this.nodeLabel.Name = "nodeLabel";
			this.nodeLabel.Size = new System.Drawing.Size(106, 15);
			this.nodeLabel.TabIndex = 40;
			this.nodeLabel.Text = "Node: SYD-XXX-1";
			// 
			// failoverModeLabel
			// 
			this.failoverModeLabel.AutoSize = true;
			this.failoverModeLabel.Location = new System.Drawing.Point(129, 87);
			this.failoverModeLabel.Name = "failoverModeLabel";
			this.failoverModeLabel.Size = new System.Drawing.Size(106, 15);
			this.failoverModeLabel.TabIndex = 41;
			this.failoverModeLabel.Text = "Failover: MANUAL";
			// 
			// allowConnectionLabel
			// 
			this.allowConnectionLabel.AutoSize = true;
			this.allowConnectionLabel.Location = new System.Drawing.Point(129, 104);
			this.allowConnectionLabel.Name = "allowConnectionLabel";
			this.allowConnectionLabel.Size = new System.Drawing.Size(134, 15);
			this.allowConnectionLabel.TabIndex = 42;
			this.allowConnectionLabel.Text = "Allow Connections: ALL";
			// 
			// databasePanel
			// 
			this.databasePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.databasePanel.BackgroundImage = global::Enterprise.AlwaysOn.Setup.Properties.Resources.DatabaseBig;
			this.databasePanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.databasePanel.Controls.Add(this.healthPanel);
			this.databasePanel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.databasePanel.Location = new System.Drawing.Point(0, 0);
			this.databasePanel.MinimumSize = new System.Drawing.Size(101, 153);
			this.databasePanel.Name = "databasePanel";
			this.databasePanel.Size = new System.Drawing.Size(123, 153);
			this.databasePanel.TabIndex = 35;
			// 
			// healthPanel
			// 
			this.healthPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.healthPanel.BackColor = System.Drawing.Color.Transparent;
			this.healthPanel.BackgroundImage = global::Enterprise.AlwaysOn.Setup.Properties.Resources.SmileyHappy24;
			this.healthPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.healthPanel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.healthPanel.Enabled = false;
			this.healthPanel.Location = new System.Drawing.Point(47, 70);
			this.healthPanel.MinimumSize = new System.Drawing.Size(24, 24);
			this.healthPanel.Name = "healthPanel";
			this.healthPanel.Size = new System.Drawing.Size(24, 24);
			this.healthPanel.TabIndex = 36;
			// 
			// linkLabel1
			// 
			this.pendingActionsLink.AutoSize = true;
			this.pendingActionsLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pendingActionsLink.Location = new System.Drawing.Point(129, 123);
			this.pendingActionsLink.Name = "pendingActionsLink";
			this.pendingActionsLink.Size = new System.Drawing.Size(122, 15);
			this.pendingActionsLink.TabIndex = 43;
			this.pendingActionsLink.TabStop = true;
			this.pendingActionsLink.Text = "Pending Actions...";
			// 
			// ReplicaControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Controls.Add(this.pendingActionsLink);
			this.Controls.Add(this.allowConnectionLabel);
			this.Controls.Add(this.failoverModeLabel);
			this.Controls.Add(this.nodeLabel);
			this.Controls.Add(this.availabilityModeLabel);
			this.Controls.Add(this.serverLabel);
			this.Controls.Add(this.roleLabel);
			this.Controls.Add(this.databasePanel);
			this.MinimumSize = new System.Drawing.Size(315, 154);
			this.Name = "ReplicaControl";
			this.Size = new System.Drawing.Size(319, 154);
			this.databasePanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal System.Windows.Forms.Panel databasePanel;
		protected internal System.Windows.Forms.Panel healthPanel;
		protected internal System.Windows.Forms.Label roleLabel;
		protected internal System.Windows.Forms.Label serverLabel;
		protected internal System.Windows.Forms.Label availabilityModeLabel;
		protected internal System.Windows.Forms.Label nodeLabel;
		protected internal System.Windows.Forms.Label failoverModeLabel;
		protected internal System.Windows.Forms.Label allowConnectionLabel;
		protected internal System.Windows.Forms.ToolTip toolTipService;
		protected internal System.Windows.Forms.LinkLabel pendingActionsLink;
	}
}
