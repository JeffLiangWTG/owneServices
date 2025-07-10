using System.Drawing;
using System.Windows.Forms;

namespace CargoWise.Loader.Common
{
	public partial class CWNextProgressForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer Components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (Components != null))
			{
				Components.Dispose();
			}
			base.Dispose(disposing);
		}
		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.Components = new System.ComponentModel.Container();
			this.StatusLabel = new System.Windows.Forms.Label();
			this.BottomPanel = new Panel();
			this.ProgressBar = new CargoWise.BrandManager.CargoWiseProgressBar();
			this.InProgressImage = new PictureBox();
			this.CWNextLogoBox = new PictureBox();
			this.AnimationTimer = new Timer(Components);
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)InProgressImage).BeginInit();
			((System.ComponentModel.ISupportInitialize)CWNextLogoBox).BeginInit();
			this.SuspendLayout();
			float currentDpi = this.DeviceDpi;
			float standardDpi = 96.0f;
			float scalingFactor = currentDpi / standardDpi;

			// 
			// StatusLabel
			// 
			this.StatusLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			this.StatusLabel.AutoSize = true;
			this.StatusLabel.BackColor = Color.FromArgb(20, 30, 48);
			this.StatusLabel.Font = new Font("Segoe UI Variable Text", 9.5F);
			this.StatusLabel.ForeColor = Color.FromArgb(246, 248, 255);
			this.StatusLabel.Location = new Point(32, 16);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new Size(17, 17);
			this.StatusLabel.TabIndex = 1;
			this.StatusLabel.Text = "...";
			this.StatusLabel.TextAlign = ContentAlignment.MiddleLeft;

			// 
			// BottomPanel
			// 
			this.BottomPanel.BackColor = Color.FromArgb(20, 30, 48);
			this.BottomPanel.Controls.Add(StatusLabel);
			this.BottomPanel.Controls.Add(InProgressImage);
			this.BottomPanel.Location = new Point(0, 246);
			this.BottomPanel.Padding = new Padding(12);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = new Size(512, 52);
			this.BottomPanel.TabIndex = 4;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.ProgressBar.Location = new System.Drawing.Point(0, 238);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = new System.Drawing.Size(512, 8);
			this.ProgressBar.TabIndex = 2;
			this.ProgressBar.SetBackgroundColor(Color.FromArgb(0, 0, 0));
			this.ProgressBar.SetForeGroundColor(Color.FromArgb(218, 226, 254));

			// 
			// InProgressImage
			// 
			this.InProgressImage.Image = Properties.Resources.progress;
			this.InProgressImage.Location = new Point(12, 16);
			this.InProgressImage.Name = "InProgressImage";
			this.InProgressImage.Size = new Size(20, 20);
			this.InProgressImage.TabIndex = 6;
			this.InProgressImage.TabStop = false;
			this.InProgressImage.SizeMode = PictureBoxSizeMode.StretchImage;
			var inProgressBitmap = new Bitmap(this.InProgressImage.Image);
			inProgressBitmap.SetResolution(DeviceDpi, DeviceDpi);
			this.InProgressImage.Image = inProgressBitmap;
			// 
			// CWNextLogoBox
			// 
			this.CWNextLogoBox.Location = new Point(142, 91);
			this.CWNextLogoBox.Name = "CWNextLogoBox";
			this.CWNextLogoBox.Size = new Size(229, 64);
			this.CWNextLogoBox.SizeMode = PictureBoxSizeMode.Zoom;
			this.CWNextLogoBox.TabIndex = 5;
			this.CWNextLogoBox.TabStop = false;
			// 
			// CWNextProgressForm
			//
			this.AutoScaleDimensions = new SizeF(96F, 96F);
			this.AutoScaleMode = AutoScaleMode.Dpi;
			this.BackColor = Color.FromArgb(29, 23, 101);
			this.BackgroundImageLayout = ImageLayout.Center;
			this.ClientSize = new Size(512, 298);
			this.Controls.Add(CWNextLogoBox);
			this.Controls.Add(BottomPanel);
			this.Controls.Add(ProgressBar);
			this.ForeColor = Color.FromArgb(55, 30, 225);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new Padding(2);
			this.Name = "CWNextProgressForm";
			this.Padding = new Padding(1);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Starting...";
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)InProgressImage).EndInit();
			((System.ComponentModel.ISupportInitialize)CWNextLogoBox).EndInit();
			ResumeLayout(false);
		}
		#endregion

		internal BrandManager.CargoWiseProgressBar ProgressBar;
		internal Label StatusLabel;
		private Panel BottomPanel;
		private PictureBox CWNextLogoBox;
		private PictureBox InProgressImage;
		private Timer AnimationTimer;
	}
}
