namespace Enterprise.Registry.GUI
{
	partial class DigitalCertificateControl_p12
	{

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
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
			this.LoadButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("67cd616b-c9d4-4ee4-b3a5-aa3df45f6da0", "Load");
			this.LoadButton.IsCaptionOverridden = false;
			this.LoadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.LoadButton.Name = "LoadButton";
			this.LoadButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LoadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.LoadButton.TabIndex = 0;
			this.LoadButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.LoadButton.ToolTipCaption = null;
			this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
			// 
			// UserFeedbackLabel
			// 
			this.UserFeedbackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.UserFeedbackLabel.AutoSize = true;
			this.UserFeedbackLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UserFeedbackLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 5, true);
			this.UserFeedbackLabel.Name = "UserFeedbackLabel";
			this.UserFeedbackLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.UserFeedbackLabel.TabIndex = 3;
			this.UserFeedbackLabel.Text = "No Data";
			// 
			// ClearButton
			// 
			this.ClearButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8aea9b6b-9e9e-4d22-8795-c9c3007194b0", "Clear");
			this.ClearButton.IsCaptionOverridden = false;
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 1, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearButton.TabIndex = 1;
			this.ClearButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ClearButton.ToolTipCaption = null;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// FileDialog
			// 
			this.FileDialog.AddExtension = true;
			this.FileDialog.CheckFileExists = true;
			this.FileDialog.CheckPathExists = true;
			this.FileDialog.DefaultExt = "key";
			this.FileDialog.DereferenceLinks = true;
			this.FileDialog.Filter = "All files (*.*)|*.*";
			this.FileDialog.FilterIndex = 1;
			this.FileDialog.InitialDirectory = "";
			this.FileDialog.Multiselect = false;
			this.FileDialog.ReadOnlyChecked = false;
			this.FileDialog.RestoreDirectory = false;
			this.FileDialog.ShowHelp = false;
			this.FileDialog.SupportMultiDottedExtensions = false;
			this.FileDialog.Title = "";
			this.FileDialog.ValidateNames = true;
			// 
			// ViewButton
			// 
			this.ViewButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("29fa3cb3-becc-4b76-a0ad-b5718ed7348a", "View");
			this.ViewButton.IsCaptionOverridden = false;
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 1, true);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ViewButton.TabIndex = 2;
			this.ViewButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ViewButton.ToolTipCaption = null;
			this.ViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// DigitalCertificateControl_p12
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.UserFeedbackLabel);
			this.Controls.Add(this.LoadButton);
			this.Controls.Add(this.ViewButton);
			this.Name = "DigitalCertificateControl_p12";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected Enterprise.ZArchitecture.GUI.ZButton ViewButton;
		protected Enterprise.ZArchitecture.GUI.ZButton LoadButton;
		protected Enterprise.ZArchitecture.ZLabel UserFeedbackLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		private Enterprise.ZArchitecture.GUI.ZOpenFileDialog FileDialog;

		#endregion
	}
}
