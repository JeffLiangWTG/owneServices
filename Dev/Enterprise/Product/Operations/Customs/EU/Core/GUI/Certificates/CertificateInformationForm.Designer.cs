namespace Enterprise.Customs.EU.GUI.Certificates
{
	partial class CertificateInformationForm
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
		new void InitializeComponent()
		{
			this.InfoZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 379, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 24, true);
			// 
			// InfoZTextBox
			// 
			this.InfoZTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InfoZTextBox.CaptionResourceString = null;
			this.InfoZTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InfoZTextBox, false);
			this.InfoZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.InfoZTextBox.Multiline = true;
			this.InfoZTextBox.Name = "InfoZTextBox";
			this.InfoZTextBox.ReadOnly = true;
			this.InfoZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 337, true);
			this.InfoZTextBox.TabIndex = 0;
			this.InfoZTextBox.TabStop = false;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("43175934-2F78-4BF1-BB61-99E3512F07E3", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 351, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 23, true);
			this.CloseButton.TabIndex = 0;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// InformationCertificateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("87C1735C-8961-4C17-8C91-206A59582138", "Certificate Information");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 403, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.InfoZTextBox);
			this.Name = "InformationCertificateForm";
			this.Load += new System.EventHandler(this.ShowCertificate_Load);
			this.Controls.SetChildIndex(this.InfoZTextBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox InfoZTextBox;
		private ZArchitecture.GUI.ZButton CloseButton;
	}
}
