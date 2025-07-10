using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DigitalCertificateControl : ZUserControl
	{
		internal Enterprise.ZArchitecture.GUI.ZButton ViewButton;
		protected Enterprise.ZArchitecture.GUI.ZButton LoadButton;
		protected ZLabel UserFeedbackLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		protected Enterprise.ZArchitecture.GUI.ZOpenFileDialog FileDialog;

		void InitializeComponent()
		{
			this.LoadButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UserFeedbackLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LoadButton
			// 
			this.LoadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.LoadButton.Name = "LoadButton";
			this.LoadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.LoadButton.TabIndex = 0;
			this.LoadButton.Text = "Load";
			this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
			// 
			// UserFeedbackLabel
			// 
			this.UserFeedbackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.UserFeedbackLabel.AutoSize = true;
			this.UserFeedbackLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 5, true);
			this.UserFeedbackLabel.Name = "UserFeedbackLabel";
			this.UserFeedbackLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.UserFeedbackLabel.TabIndex = 3;
			this.UserFeedbackLabel.Text = "No Data";
			// 
			// ClearButton
			// 
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 1, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearButton.TabIndex = 1;
			this.ClearButton.Text = "Clear";
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// FileDialog
			// 
			this.FileDialog.DefaultExt = "key";
			this.FileDialog.Filter = "All files (*.*)|*.*";
			// 
			// ViewButton
			// 
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 1, true);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ViewButton.TabIndex = 2;
			this.ViewButton.Text = "View";
			this.ViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// DigitalCertificateControl
			// 
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.UserFeedbackLabel);
			this.Controls.Add(this.LoadButton);
			this.Controls.Add(this.ViewButton);
			this.Name = "DigitalCertificateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
