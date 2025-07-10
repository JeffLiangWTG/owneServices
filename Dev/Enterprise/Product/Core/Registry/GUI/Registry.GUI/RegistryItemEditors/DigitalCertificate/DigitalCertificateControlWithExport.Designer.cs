namespace Enterprise.Registry.GUI
{
	public partial class DigitalCertificateControlWithExport : DigitalCertificateControl
	{
		internal Enterprise.ZArchitecture.GUI.ZButton SaveToDiskButton;

		private void InitializeComponent()
		{
			this.SaveToDiskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// UserFeedbackLabel
			// 
			this.UserFeedbackLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 5, true);
			this.UserFeedbackLabel.TabIndex = 4;
			// 
			// SaveToDiskButton
			// 
			this.SaveToDiskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 1, true);
			this.SaveToDiskButton.Name = "SaveToDiskButton";
			this.SaveToDiskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveToDiskButton.TabIndex = 3;
			this.SaveToDiskButton.Text = "Save to Disk";
			this.SaveToDiskButton.Click += new System.EventHandler(this.SaveToDiskButton_Click);
			// 
			// DigitalCertificateControlWithExport
			// 
			this.Controls.Add(this.SaveToDiskButton);
			this.Name = "DigitalCertificateControlWithExport";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 26, true);
			this.Controls.SetChildIndex(this.SaveToDiskButton, 0);
			this.Controls.SetChildIndex(this.LoadButton, 0);
			this.Controls.SetChildIndex(this.UserFeedbackLabel, 0);
			this.Controls.SetChildIndex(this.ClearButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
