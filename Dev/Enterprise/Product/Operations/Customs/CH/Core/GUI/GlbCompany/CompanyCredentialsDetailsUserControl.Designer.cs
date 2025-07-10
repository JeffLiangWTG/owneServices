namespace Enterprise.Customs.CH.GUI;

partial class CompanyCredentialsDetailsUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.expirDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
        this.passwordStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.certificatePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.CertificateLoaderUserControl = new Enterprise.Customs.CH.GUI.CHDigitalCertificateControl_p12();
        this.TokenCredentialsUserControl = new Enterprise.Customs.CH.GUI.TokenCredentialsUserControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.zGroupBox1.SuspendLayout();
        this.expirDateEdit.SuspendLayout();
        this.CertificateLoaderUserControl.SuspendLayout();
        this.TokenCredentialsUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.GlbCompanyWrapper);
        // 
        // zGroupBox1
        // 
        this.zGroupBox1.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("12a15f6c-6551-4357-b454-0ba3a8b3e342", "Certificate");
        this.zGroupBox1.Controls.Add(this.expirDateEdit);
        this.zGroupBox1.Controls.Add(this.passwordStatusTextBox);
        this.zGroupBox1.Controls.Add(this.certificatePasswordTextBox);
        this.zGroupBox1.Controls.Add(this.CertificateLoaderUserControl);
        this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.zGroupBox1.Name = "zGroupBox1";
        this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 114, true);
        this.zGroupBox1.TabIndex = 1;
        this.zGroupBox1.TabStop = false;
        // 
        // expirDateEdit
        // 
        this.expirDateEdit.AllowDrop = true;
        this.expirDateEdit.AutoCompleteMonthThreshold = 1;
        this.BindingSource.SetBindingMember(this.expirDateEdit, "GlbExternalPassword.GP_ExpiryDate");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_ExpiryDate)));
        this.expirDateEdit.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("d58fa72a-a34b-4175-a405-62dd2653d8a7", "Expiry Date");
        this.expirDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 77, true);
        this.expirDateEdit.Name = "expirDateEdit";
        this.expirDateEdit.TabIndex = 3;
        // 
        // passwordStatusTextBox
        // 
        this.BindingSource.SetBindingMember(this.passwordStatusTextBox, "GlbExternalPassword.PasswordStatus");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.PasswordStatus)));
        this.passwordStatusTextBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("dd0e03d4-5730-424f-a414-c9d712171563", "Password Status");
        this.passwordStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.passwordStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 51, true);
        this.passwordStatusTextBox.Name = "passwordStatusTextBox";
        this.passwordStatusTextBox.ReadOnly = true;
        this.passwordStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
        this.passwordStatusTextBox.TabIndex = 2;
        // 
        // certificatePasswordTextBox
        // 
        this.BindingSource.SetBindingMember(this.certificatePasswordTextBox, "GlbExternalPassword.CurrentDecryptedCertificatePassphrase");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.CurrentDecryptedCertificatePassphrase)));
        this.certificatePasswordTextBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("6293913b-62f0-4b23-a383-701640ffe0ef", "Certificate Password");
        this.certificatePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.certificatePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 51, true);
        this.certificatePasswordTextBox.Name = "certificatePasswordTextBox";
        this.certificatePasswordTextBox.PasswordChar = '*';
        this.certificatePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 17, true);
        this.certificatePasswordTextBox.TabIndex = 1;
        // 
        // CertificateLoaderUserControl
        // 
        this.CertificateLoaderUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "GlbExternalPassword.GP_Certificate");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_Certificate)));
        this.CertificateLoaderUserControl.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("d3e06e78-8cb3-453e-b317-9951fd70188b", "Certificate");
        this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
        this.CertificateLoaderUserControl.FileDataAsString = "";
        this.CertificateLoaderUserControl.FileDialogTitle = "";
        this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
        this.CertificateLoaderUserControl.InitialDirectory = "";
        this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 19, true);
        this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
        this.CertificateLoaderUserControl.ReadOnly = false;
        this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
        this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
        this.CertificateLoaderUserControl.TabIndex = 0;
        // 
        // TokenCredentialsUserControl
        // 
        this.TokenCredentialsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.TokenCredentialsUserControl, ".");
        this.TokenCredentialsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 121, true);
        this.TokenCredentialsUserControl.Name = "TokenCredentialsUserControl";
        this.TokenCredentialsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 175, true);
        this.TokenCredentialsUserControl.TabIndex = 2;
        // 
        // CompanyCredentialsDetailsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Controls.Add(this.TokenCredentialsUserControl);
        this.Controls.Add(this.zGroupBox1);
        this.Name = "CompanyCredentialsDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 297, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.zGroupBox1.ResumeLayout(false);
        this.zGroupBox1.PerformLayout();
        this.expirDateEdit.ResumeLayout(true);
        this.expirDateEdit.PerformLayout();
        this.CertificateLoaderUserControl.ResumeLayout(true);
        this.CertificateLoaderUserControl.PerformLayout();
        this.TokenCredentialsUserControl.ResumeLayout(true);
        this.TokenCredentialsUserControl.PerformLayout();
        this.CaptionRenderingEnabled = true;
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private Enterprise.Customs.CH.GUI.CHDigitalCertificateControl_p12 CertificateLoaderUserControl;
    private ZArchitecture.GUI.ZGroupBox zGroupBox1;
    private ZArchitecture.ZTextBox passwordStatusTextBox;
    private ZArchitecture.ZTextBox certificatePasswordTextBox;
    private ZArchitecture.GUI.ZDateEdit expirDateEdit;
    private TokenCredentialsUserControl TokenCredentialsUserControl;
}
