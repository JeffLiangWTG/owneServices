
using System.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	partial class DbOverwriteReleaseForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components;

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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();

			this.ReleaseKeyTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ReleaseKeyLabel = new CargoWise.Windows.UI.KLabel();
			this.OkButton = new CargoWise.Windows.UI.KButton();
			this.HeaderLabel = new CargoWise.Windows.UI.KLabel();
			this.ClientInfoTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ClientInfoLabel = new CargoWise.Windows.UI.KLabel();
			this.SuspendLayout();
			// 
			// ReleaseKeyTextBox
			// 
			this.ReleaseKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ReleaseKeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 188, true);
			this.ReleaseKeyTextBox.Name = "ReleaseKeyTextBox";
			this.ReleaseKeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 20, true);
			this.ReleaseKeyTextBox.TabIndex = 2;
			// 
			// ReleaseKeyLabel
			// 
			this.ReleaseKeyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 167, true);
			this.ReleaseKeyLabel.Name = "ReleaseKeyLabel";
			this.ReleaseKeyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.ReleaseKeyLabel.TabIndex = 1;
			this.ReleaseKeyLabel.Text = "Key:";
			this.ReleaseKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 212, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 28, true);
			this.OkButton.TabIndex = 5;
			this.OkButton.Text = "OK";
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HeaderLabel.ForeColor = System.Drawing.Color.Red;
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 68, true);
			this.HeaderLabel.TabIndex = 0;
			this.HeaderLabel.Text = "Please be aware this will delete/overwrite all current production info in the dat" +
    "abase.";
			this.HeaderLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// ClientInfoTextBox
			// 
			this.ClientInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ClientInfoTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 108, true);
			this.ClientInfoTextBox.Multiline = true;
			this.ClientInfoTextBox.Name = "ClientInfoTextBox";
			this.ClientInfoTextBox.ReadOnly = true;
			this.ClientInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 51, true);
			this.ClientInfoTextBox.TabIndex = 7;
			// 
			// ClientInfoLabel
			// 
			this.ClientInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 59, true);
			this.ClientInfoLabel.Name = "ClientInfoLabel";
			this.ClientInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 46, true);
			this.ClientInfoLabel.TabIndex = 8;
			this.ClientInfoLabel.Text = "If you\'re really sure this is what you want to do, please contact \'support@wisete" +
    "chglobal.com\' and ask for a release key. Then type the provided key in the box b" +
    "ellow.";
			this.ClientInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DbOverwriteReleaseForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 241, true);
			this.Controls.Add(this.ClientInfoTextBox);
			this.Controls.Add(this.ClientInfoLabel);
			this.Controls.Add(this.ReleaseKeyTextBox);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.ReleaseKeyLabel);
			this.Controls.Add(this.HeaderLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 280, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 280, true);
			this.Name = "DbOverwriteReleaseForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Database Overwrite";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KTextBox ReleaseKeyTextBox;
		private KLabel ReleaseKeyLabel;
		private KButton OkButton;
		private KLabel HeaderLabel;
		private KTextBox ClientInfoTextBox;
		private KLabel ClientInfoLabel;
	}
}