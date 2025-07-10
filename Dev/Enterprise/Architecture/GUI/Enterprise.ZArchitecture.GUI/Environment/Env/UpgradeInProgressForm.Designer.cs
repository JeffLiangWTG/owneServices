using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Environment
{
	partial class UpgradeInProgressForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "backgroundTimer", Justification = "It is already disposed")]
		protected override void Dispose(bool disposing)
		{

			if (disposing)
			{
				foregroundTimer?.Dispose();
				backgroundTimer?.Dispose();
				components?.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpgradeInProgressForm));
			this.messageTextBox = new CargoWise.Windows.UI.KTextBox();
			this.exitButton = new CargoWise.Windows.UI.KButton();
			this.ConnectingPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.ConnectingPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// messageTextBox
			// 
			this.messageTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.messageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.messageTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.messageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.messageTextBox.Multiline = true;
			this.messageTextBox.Name = "messageTextBox";
			this.messageTextBox.ReadOnly = true;
			this.messageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 76, true);
			this.messageTextBox.TabIndex = 0;
			this.messageTextBox.TabStop = false;
			this.messageTextBox.Text = "Please wait, the database is in the process of being upgraded. The application will restart automatically after the upgrade is complete.";
			// 
			// exitButton
			// 
			this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.exitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 136, true);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.exitButton.TabIndex = 1;
			this.exitButton.Text = "Exit";
			this.exitButton.UseVisualStyleBackColor = true;
			this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
			// 
			// ConnectingPictureBox
			// 
			this.ConnectingPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("ConnectingPictureBox.Image")));
			this.ConnectingPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 94, true);
			this.ConnectingPictureBox.Name = "ConnectingPictureBox";
			this.ConnectingPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 32, true);
			this.ConnectingPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ConnectingPictureBox.TabIndex = 2;
			this.ConnectingPictureBox.TabStop = false;
			// 
			// UpgradeInProgressForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.exitButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 171, true);
			this.Controls.Add(this.ConnectingPictureBox);
			this.Controls.Add(this.exitButton);
			this.Controls.Add(this.messageTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UpgradeInProgressForm";
			((System.ComponentModel.ISupportInitialize)(this.ConnectingPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTextBox messageTextBox;
		private CargoWise.Windows.UI.KButton exitButton;
		private Enterprise.ZArchitecture.GUI.ZPictureBox ConnectingPictureBox;

	}
}
