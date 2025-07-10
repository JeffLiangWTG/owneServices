using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	//[System.Diagnostics.Contracts.ContractVerification(false)]
	partial class ChangeReplicaSettingControl
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a ZButton")]
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.toolTipService = new System.Windows.Forms.ToolTip(this.components);
			this.errorProvider = new ErrorProvider();
			this.errorProvider.Icon = SystemIcons.Warning;
			this.errorProvider.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
			this.allowConnectionsComboBox = new System.Windows.Forms.ComboBox();
			this.failoverModeComboBox = new System.Windows.Forms.ComboBox();
			this.commitModeComboBox = new System.Windows.Forms.ComboBox();
			this.allowConnectionLabel = new System.Windows.Forms.Label();
			this.failoverModeLabel = new System.Windows.Forms.Label();
			this.commitModeLabel = new System.Windows.Forms.Label();
			this.buttonsToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.cancelButton = new System.Windows.Forms.Button();
			this.confirmButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// allowConnectionsComboBox
			// 
			this.allowConnectionsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.allowConnectionsComboBox.FormattingEnabled = true;
			this.allowConnectionsComboBox.Items.AddRange(new object[] {
            "Read-Only",
            "All"});
			this.allowConnectionsComboBox.Location = new System.Drawing.Point(74, 44);
			this.allowConnectionsComboBox.Name = "allowConnectionsComboBox";
			this.allowConnectionsComboBox.Size = new System.Drawing.Size(100, 24);
			this.allowConnectionsComboBox.TabIndex = 63;
			this.allowConnectionsComboBox.SelectedIndexChanged += new System.EventHandler(this.allowConnectionsComboBox_SelectedIndexChanged);
			// 
			// failoverModeComboBox
			// 
			this.failoverModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.failoverModeComboBox.FormattingEnabled = true;
			this.failoverModeComboBox.Items.AddRange(new object[] {
			"Automatic",
			"Manual"});
			this.failoverModeComboBox.Location = new System.Drawing.Point(74, 24);
			this.failoverModeComboBox.Name = "failoverModeComboBox";
			this.failoverModeComboBox.Size = new System.Drawing.Size(100, 24);
			this.failoverModeComboBox.TabIndex = 62;
			// 
			// commitModeComboBox
			// 
			this.commitModeComboBox.BackColor = System.Drawing.SystemColors.Window;
			this.commitModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.commitModeComboBox.FormattingEnabled = true;
			this.commitModeComboBox.Items.AddRange(new object[] {
			"Asynchronous",
			"Synchronous"});
			this.commitModeComboBox.Location = new System.Drawing.Point(74, 4);
			this.commitModeComboBox.Name = "commitModeComboBox";
			this.commitModeComboBox.Size = new System.Drawing.Size(100, 24);
			this.commitModeComboBox.TabIndex = 61;
			// 
			// allowConnectionLabel
			// 
			this.allowConnectionLabel.AutoSize = true;
			this.allowConnectionLabel.Location = new System.Drawing.Point(4, 47);
			this.allowConnectionLabel.Name = "allowConnectionLabel";
			this.allowConnectionLabel.Size = new System.Drawing.Size(90, 17);
			this.allowConnectionLabel.TabIndex = 60;
			this.allowConnectionLabel.Text = "Connections:";
			// 
			// failoverModeLabel
			// 
			this.failoverModeLabel.AutoSize = true;
			this.failoverModeLabel.Location = new System.Drawing.Point(4, 27);
			this.failoverModeLabel.Name = "failoverModeLabel";
			this.failoverModeLabel.Size = new System.Drawing.Size(62, 17);
			this.failoverModeLabel.TabIndex = 59;
			this.failoverModeLabel.Text = "Failover:";
			// 
			// commitModeLabel
			// 
			this.commitModeLabel.AutoSize = true;
			this.commitModeLabel.Location = new System.Drawing.Point(4, 7);
			this.commitModeLabel.Name = "commitModeLabel";
			this.commitModeLabel.Size = new System.Drawing.Size(58, 17);
			this.commitModeLabel.TabIndex = 58;
			this.commitModeLabel.Text = "Commit:";
			// 
			// cancelButton
			// 
			this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancelButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CrossNo16;
			this.cancelButton.Location = new System.Drawing.Point(180, 10);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(22, 22);
			this.cancelButton.TabIndex = 64;
			this.cancelButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.buttonsToolTip.SetToolTip(this.cancelButton, "Cancel");
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// confirmButton
			// 
			this.confirmButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this.confirmButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.confirmButton.Image = global::Enterprise.AlwaysOn.Setup.Properties.Resources.CheckYes16;
			this.confirmButton.Location = new System.Drawing.Point(180, 38);
			this.confirmButton.Name = "confirmButton";
			this.confirmButton.Size = new System.Drawing.Size(22, 22);
			this.confirmButton.TabIndex = 65;
			this.confirmButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.buttonsToolTip.SetToolTip(this.confirmButton, "Apply");
			this.confirmButton.UseVisualStyleBackColor = false;
			this.confirmButton.Click += new System.EventHandler(this.confirmButton_Click);
			// 
			// ChangeReplicaSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.allowConnectionsComboBox);
			this.Controls.Add(this.failoverModeComboBox);
			this.Controls.Add(this.commitModeComboBox);
			this.Controls.Add(this.allowConnectionLabel);
			this.Controls.Add(this.failoverModeLabel);
			this.Controls.Add(this.commitModeLabel);
			this.Controls.Add(this.confirmButton);
			this.Name = "ChangeReplicaSettingControl";
			this.Size = new System.Drawing.Size(206, 68);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ToolTip buttonsToolTip;
		private System.Windows.Forms.ComboBox allowConnectionsComboBox;
		private System.Windows.Forms.ComboBox failoverModeComboBox;
		private System.Windows.Forms.ComboBox commitModeComboBox;
		private System.Windows.Forms.Label allowConnectionLabel;
		private System.Windows.Forms.Label failoverModeLabel;
		private System.Windows.Forms.Label commitModeLabel;
		private System.Windows.Forms.Button confirmButton;
		private System.Windows.Forms.ToolTip toolTipService;
		private ErrorProvider errorProvider;
	}
}
