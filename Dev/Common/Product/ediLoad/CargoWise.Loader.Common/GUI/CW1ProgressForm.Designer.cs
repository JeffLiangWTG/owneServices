using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.IO;

namespace CargoWise.Loader.Common
{
	public partial class CW1ProgressForm
	{
		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CW1ProgressForm));
			this.ProgressBar = new CargoWiseProgressBar();
			this.StatusLabel = new Label();
			this.SuspendLayout();
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.ProgressBar.Location = new System.Drawing.Point(12, 136);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = new System.Drawing.Size(415, 23);
			this.ProgressBar.TabIndex = 2;
			// 
			// StatusLabel
			// 
			this.StatusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left
						| AnchorStyles.Right;
			this.StatusLabel.BackColor = System.Drawing.Color.Transparent;
			this.StatusLabel.Location = new System.Drawing.Point(0, 160);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(439, 37);
			this.StatusLabel.TabIndex = 3;
			this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.StatusLabel.UseMnemonic = false;
			// 
			// ProgressForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.BackgroundImageLayout = ImageLayout.Stretch;
			this.ClientSize = new System.Drawing.Size(439, 194);
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.ProgressBar);
			this.FormBorderStyle = FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressForm";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.ResumeLayout(false);
		}
		#endregion

		internal CargoWiseProgressBar ProgressBar;
		internal Label StatusLabel;
	}
}
