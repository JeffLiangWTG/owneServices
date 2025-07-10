namespace Enterprise.Customs.CH.GUI;

partial class TokenCredentialsUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.TokenCredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.ExpiryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
        this.ClientSecretTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.UserIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.CertificatePassPhraseTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.AccessTokenTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.SetTokenCredentialsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.TokenCredentialsGroupBox.SuspendLayout();
        this.ExpiryDateDateEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.GlbCompanyWrapper);
        // 
        // TokenCredentialsGroupBox
        // 
        this.TokenCredentialsGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("52fc1f83-8aaf-4434-bfeb-5c1f6307a211", "Token Credentials");
        this.TokenCredentialsGroupBox.Controls.Add(this.ExpiryDateDateEdit);
        this.TokenCredentialsGroupBox.Controls.Add(this.ClientSecretTextBox);
        this.TokenCredentialsGroupBox.Controls.Add(this.UserIdTextBox);
        this.TokenCredentialsGroupBox.Controls.Add(this.CertificatePassPhraseTextBox);
        this.TokenCredentialsGroupBox.Controls.Add(this.AccessTokenTextBox);
        this.TokenCredentialsGroupBox.Controls.Add(this.SetTokenCredentialsCheckBox);
        this.TokenCredentialsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TokenCredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.TokenCredentialsGroupBox.Name = "TokenCredentialsGroupBox";
        this.TokenCredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(638, 182, true);
        this.TokenCredentialsGroupBox.TabIndex = 0;
        this.TokenCredentialsGroupBox.TabStop = false;
        // 
        // ExpiryDateDateEdit
        // 
        this.ExpiryDateDateEdit.AllowDrop = true;
        this.ExpiryDateDateEdit.AutoCompleteMonthThreshold = 1;
        this.BindingSource.SetBindingMember(this.ExpiryDateDateEdit, "TokenCredentials+GP_ExpiryDate");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).TokenCredentials.GP_ExpiryDate)));
        this.ExpiryDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
        this.ExpiryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 67, true);
        this.ExpiryDateDateEdit.Name = "ExpiryDateDateEdit";
        this.ExpiryDateDateEdit.TabIndex = 2;
        // 
        // ClientSecretTextBox
        // 
        this.ClientSecretTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.ClientSecretTextBox, "TokenCredentials+CurrentDecryptedPassword");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).TokenCredentials.CurrentDecryptedPassword)));
        this.ClientSecretTextBox.CaptionResourceString = null;
        this.ClientSecretTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.ClientSecretTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 145, true);
        this.ClientSecretTextBox.Name = "ClientSecretTextBox";
        this.ClientSecretTextBox.PasswordChar = '*';
        this.ClientSecretTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 20, true);
        this.ClientSecretTextBox.TabIndex = 5;
        // 
        // UserIdTextBox
        // 
        this.UserIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.UserIdTextBox, "TokenCredentials+GP_UserID");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).TokenCredentials.GP_UserID)));
        this.UserIdTextBox.CaptionResourceString = null;
        this.UserIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.UserIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 119, true);
        this.UserIdTextBox.Name = "UserIdTextBox";
        this.UserIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 20, true);
        this.UserIdTextBox.TabIndex = 4;
        // 
        // CertificatePassPhraseTextBox
        // 
        this.CertificatePassPhraseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.CertificatePassPhraseTextBox, "TokenCredentials+RefreshTokenText");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).TokenCredentials.RefreshTokenText)));
        this.CertificatePassPhraseTextBox.CaptionResourceString = null;
        this.CertificatePassPhraseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.CertificatePassPhraseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 93, true);
        this.CertificatePassPhraseTextBox.Name = "CertificatePassPhraseTextBox";
        this.CertificatePassPhraseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 20, true);
        this.CertificatePassPhraseTextBox.TabIndex = 3;
        // 
        // AccessTokenTextBox
        // 
        this.AccessTokenTextBox.AllowDrop = true;
        this.AccessTokenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.AccessTokenTextBox, "TokenCredentials+GP_CertificateText");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).TokenCredentials.GP_CertificateText)));
        this.AccessTokenTextBox.CaptionResourceString = null;
        this.AccessTokenTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.AccessTokenTextBox.IsDynamicMultiline = true;
        this.AccessTokenTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 42, true);
        this.AccessTokenTextBox.Name = "AccessTokenTextBox";
        this.AccessTokenTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 20, true);
        this.AccessTokenTextBox.TabIndex = 1;
        // 
        // SetTokenCredentialsCheckBox
        // 
        this.SetTokenCredentialsCheckBox.AutoSize = true;
        this.BindingSource.SetBindingMember(this.SetTokenCredentialsCheckBox, "TokenCredentialsEnabled");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.GlbCompanyWrapper)(null)).TokenCredentialsEnabled)));
        this.SetTokenCredentialsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
        this.SetTokenCredentialsCheckBox.Name = "SetTokenCredentialsCheckBox";
        this.SetTokenCredentialsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
        this.SetTokenCredentialsCheckBox.TabIndex = 0;
        this.SetTokenCredentialsCheckBox.UseVisualStyleBackColor = true;
        // 
        // TokenCredentialsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.AutoSize = true;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.TokenCredentialsGroupBox);
        this.Name = "TokenCredentialsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(638, 182, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.TokenCredentialsGroupBox.ResumeLayout(false);
        this.TokenCredentialsGroupBox.PerformLayout();
        this.ExpiryDateDateEdit.ResumeLayout(true);
        this.ExpiryDateDateEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZGroupBox TokenCredentialsGroupBox;
    internal ZArchitecture.GUI.ZCheckBox SetTokenCredentialsCheckBox;
    internal ZArchitecture.ZTextBox AccessTokenTextBox;
    internal ZArchitecture.ZTextBox CertificatePassPhraseTextBox;
    internal ZArchitecture.ZTextBox UserIdTextBox;
    internal ZArchitecture.ZTextBox ClientSecretTextBox;
    internal ZArchitecture.GUI.ZDateEdit ExpiryDateDateEdit;
}
