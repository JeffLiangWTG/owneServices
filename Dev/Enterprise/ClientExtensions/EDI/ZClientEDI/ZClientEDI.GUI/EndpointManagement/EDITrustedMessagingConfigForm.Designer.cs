namespace Enterprise.Client.EDI.EndpointManagement.GUI
{
    partial class EDITrustedMessagingConfigForm
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
        protected override void InitializeComponent()
        {
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificatePwdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateTypeBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertificateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertificateThumbprintTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateControl = new Enterprise.Client.EDI.EndpointManagement.GUI.EDIDigitalCertificateControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.CertificateTypeBox.SuspendLayout();
			this.ProductBox.SuspendLayout();
			this.CertificateControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 373, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 346, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 346, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 346, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 373, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedMessagingConfig);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.CertificatePwdTextBox);
			this.zPanel1.Controls.Add(this.CertificateTypeBox);
			this.zPanel1.Controls.Add(this.CertificateLabel);
			this.zPanel1.Controls.Add(this.ProductBox);
			this.zPanel1.Controls.Add(this.CertificateThumbprintTextBox);
			this.zPanel1.Controls.Add(this.CertificateControl);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 346, true);
			this.zPanel1.TabIndex = 3;
			// 
			// CertificatePwdTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificatePwdTextBox, "ETM_CertificatePassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedMessagingConfig)(null)).ETM_CertificatePassword)));
			this.CertificatePwdTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("72e4a98c-dfe0-4b7a-a912-9ecff251c4bc", "Certificate Password");
			this.CertificatePwdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePwdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 123, true);
			this.CertificatePwdTextBox.Name = "CertificatePwdTextBox";
			this.CertificatePwdTextBox.PasswordChar = '*';
			this.CertificatePwdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
			this.CertificatePwdTextBox.TabIndex = 4;
			// 
			// CertificateTypeBox
			// 
			this.CertificateTypeBox.AllowDrop = true;
			this.CertificateTypeBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CertificateTypeBox, "ETM_CertificateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedMessagingConfig)(null)).ETM_CertificateType)));
			this.CertificateTypeBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ebdfc672-bb63-405b-8177-b90e1f2b60c8", "Certificate Type");
			this.CertificateTypeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 160, true);
			this.CertificateTypeBox.Name = "CertificateTypeBox";
			this.CertificateTypeBox.ShouldResizeByMaxLength = true;
			this.CertificateTypeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 23, true);
			this.CertificateTypeBox.TabIndex = 5;
			// 
			// CertificateLabel
			// 
			this.CertificateLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e8995bbc-15ec-40f0-a64b-3e3a1b994a77", "Certificate");
			this.CertificateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CertificateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 86, true);
			this.CertificateLabel.Name = "CertificateLabel";
			this.CertificateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 26, true);
			this.CertificateLabel.TabIndex = 0;
			this.CertificateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ProductBox
			// 
			this.ProductBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductBox, "ETM_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedMessagingConfig)(null)).ETM_Product)));
			this.ProductBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c2b42070-7e77-48d4-8035-279f0e3912e1", "Product");
			this.ProductBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 19, true);
			this.ProductBox.Name = "ProductBox";
			this.ProductBox.PreBoundMaxLength = 2;
			this.ProductBox.ShouldResizeByMaxLength = true;
			this.ProductBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
			this.ProductBox.TabIndex = 1;
			// 
			// CertificateThumbprintTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificateThumbprintTextBox, "ETM_CertificateThumbprint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedMessagingConfig)(null)).ETM_CertificateThumbprint)));
			this.CertificateThumbprintTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c184eaaa-dbb0-4e57-bdd3-df78839b31e6", "Certificate Thumbprint");
			this.CertificateThumbprintTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificateThumbprintTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 51, true);
			this.CertificateThumbprintTextBox.Name = "CertificateThumbprintTextBox";
			this.CertificateThumbprintTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
			this.CertificateThumbprintTextBox.TabIndex = 2;
			// 
			// CertificateControl
			// 
			this.CertificateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateControl, "ETM_CertificateData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedMessagingConfig)(null)).ETM_CertificateData)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CertificateControl, false);
			this.CertificateControl.FileDataAsString = "";
			this.CertificateControl.FileDialogTitle = "";
			this.CertificateControl.FileFilter = "X.509 Certificate files (*.cer;*.pfx)|*.cer;*.pfx|All files (*.*)|*.*";
			this.CertificateControl.InitialDirectory = "";
			this.CertificateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 85, true);
			this.CertificateControl.Name = "CertificateControl";
			this.CertificateControl.ReadOnly = false;
			this.CertificateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 26, true);
			this.CertificateControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateControl.TabIndex = 3;
			this.CertificateControl.DataLoaded += new System.EventHandler(this.CertificateControl_DataChanged);
			this.CertificateControl.DataCleared += new System.EventHandler(this.CertificateControl_DataChanged);
			// 
			// EDITrustedMessagingConfigForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1967ae20-9614-488c-95b0-7961bec7c72c", "Trusted Messaging Configuration");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 429, true);
			this.DataSourceType = typeof(Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedMessagingConfig);
			this.Name = "EDITrustedMessagingConfigForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.CertificateTypeBox.ResumeLayout(true);
			this.CertificateTypeBox.PerformLayout();
			this.ProductBox.ResumeLayout(true);
			this.ProductBox.PerformLayout();
			this.CertificateControl.ResumeLayout(true);
			this.CertificateControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.ZLabel CertificateLabel;
		private ZArchitecture.GUI.ZDropEdit ProductBox;
		private ZArchitecture.ZTextBox CertificateThumbprintTextBox;
		private EDIDigitalCertificateControl CertificateControl;
		private ZArchitecture.GUI.ZDropEdit CertificateTypeBox;
		private ZArchitecture.ZTextBox CertificatePwdTextBox;
	}
}
