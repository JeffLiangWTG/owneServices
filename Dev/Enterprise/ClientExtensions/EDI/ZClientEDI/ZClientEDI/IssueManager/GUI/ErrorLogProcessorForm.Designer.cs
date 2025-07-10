namespace Enterprise.Client.EDI.IssueManager.GUI
{
	internal partial class ErrorLogProcessorForm
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
			this.progressLabel = new CargoWise.Windows.UI.KLabel();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SuspendLayout();
			// 
			// progressLabel
			// 
			this.progressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.progressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 15, true);
			this.progressLabel.Name = "progressLabel";
			this.progressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 143, true);
			this.progressLabel.TabIndex = 6;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 135, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 23, true);
			this.closeButton.TabIndex = 7;
			this.closeButton.Text = "Close";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// ErrorLogSplitForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 170, true);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.progressLabel);
			this.Name = "ErrorLogSplitForm";
			this.Text = "Log Split";
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KLabel progressLabel;
		private Enterprise.ZArchitecture.GUI.ZButton closeButton;
	}
}
