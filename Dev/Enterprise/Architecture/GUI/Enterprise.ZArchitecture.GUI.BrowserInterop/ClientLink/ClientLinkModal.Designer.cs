using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	partial class ClientLinkModal : ZChildForm
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

		new void InitializeComponent()
		{
			this.closeButton = new ZButton();
			this.statusLabel = new ZLabel();
			this.SuspendLayout();
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 90);
			this.closeButton.Name = nameof(this.closeButton);
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23);
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = Res.GetString("97cf1ce5-c990-29ad-4d5e-39cdc4bf9fe0", "Close");
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);

			this.statusLabel.AutoSize = false;
			this.statusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10);
			this.statusLabel.Name = nameof(this.statusLabel);
			this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.statusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 60);
			this.statusLabel.TabIndex = 1;
			this.statusLabel.Text = Res.GetString("9df80065-fea0-799a-40de-ffd73d5fab62", "Initializing");

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 130);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.closeButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public void SetStatusText(string text)
		{
			this.statusLabel.Text = text;
		}

		#endregion

		public bool isError = false;
		private ZButton closeButton;
		internal ZLabel statusLabel;
	}
}
