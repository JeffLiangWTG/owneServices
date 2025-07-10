namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	partial class SalesRestoreDbForm
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
			this.ouputGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.outputTextBox = new CargoWise.Windows.UI.KRichTextBox();
			this.hideButton = new CargoWise.Windows.UI.KButton();
			this.restoreNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.ouputGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ouputGroupBox
			// 
			this.ouputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ouputGroupBox.Controls.Add(this.outputTextBox);
			this.ouputGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ouputGroupBox.Name = "ouputGroupBox";
			this.ouputGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 398, true);
			this.ouputGroupBox.TabIndex = 9;
			this.ouputGroupBox.TabStop = false;
			// 
			// outputTextBox
			// 
			this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.outputTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.outputTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.outputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.outputTextBox.Name = "outputTextBox";
			this.outputTextBox.ReadOnly = true;
			this.outputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 373, true);
			this.outputTextBox.TabIndex = 0;
			this.outputTextBox.TabStop = false;
			this.outputTextBox.Text = "";
			// 
			// hideButton
			// 
			this.hideButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.hideButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.hideButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 416, true);
			this.hideButton.Name = "hideButton";
			this.hideButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 28, true);
			this.hideButton.TabIndex = 10;
			this.hideButton.Text = "Hide";
			this.hideButton.Click += new System.EventHandler(this.hideButton_Click);
			// 
			// restoreNotifyIcon
			// 
			this.restoreNotifyIcon.Text = "Database Restoring.";
			this.restoreNotifyIcon.Visible = true;
			this.restoreNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.restoreNotifyIcon_MouseDoubleClick);
			// 
			// SalesRestoreDbForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 456, true);
			this.Controls.Add(this.hideButton);
			this.Controls.Add(this.ouputGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 300, true);
			this.Name = "SalesRestoreDbForm";
			this.Text = "Sales Database Restore Tool";
			this.Load += new System.EventHandler(this.SalesRestoreDbForm_Load);
			this.Shown += new System.EventHandler(this.SalesRestoreDbForm_Shown);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SalesRestoreDbForm_FormClosing);
			this.Resize += new System.EventHandler(this.SalesRestoreDbForm_Resize);
			this.ouputGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KGroupBox ouputGroupBox;
		private CargoWise.Windows.UI.KRichTextBox outputTextBox;
		private CargoWise.Windows.UI.KButton hideButton;
		private System.Windows.Forms.NotifyIcon restoreNotifyIcon;
	}
}

