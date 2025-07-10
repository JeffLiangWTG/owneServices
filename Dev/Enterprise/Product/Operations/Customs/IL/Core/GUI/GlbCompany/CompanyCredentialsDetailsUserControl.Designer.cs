namespace Enterprise.Customs.IL.GUI
{
	partial class CompanyCredentialsDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.CustomsCredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageSenderVATTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificatePassword = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.Status = new Enterprise.ZArchitecture.ZTextBox();
			this.DefaultUserGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DefaultUserCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsCredentialsGroupBox.SuspendLayout();
			this.CertificateExpiryDateEdit.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.DefaultUserGroupBox.SuspendLayout();
			this.DefaultUserCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.GlbCompanyWrapper);
			// 
			// CustomsCredentialsGroupBox
			// 
			this.CustomsCredentialsGroupBox.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("0E73AE43-F225-44F5-A5B8-979B6FEF0703", "Israel Customs Certificate");
			this.CustomsCredentialsGroupBox.Controls.Add(this.CertificateExpiryDateEdit);
			this.CustomsCredentialsGroupBox.Controls.Add(this.MessageSenderVATTextBox);
			this.CustomsCredentialsGroupBox.Controls.Add(this.CertificatePassword);
			this.CustomsCredentialsGroupBox.Controls.Add(this.CertificateLoaderUserControl);
			this.CustomsCredentialsGroupBox.Controls.Add(this.Status);
			this.CustomsCredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.CustomsCredentialsGroupBox.Name = "CustomsCredentialsGroupBox";
			this.CustomsCredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 191, true);
			this.CustomsCredentialsGroupBox.TabIndex = 0;
			this.CustomsCredentialsGroupBox.TabStop = false;
			// 
			// CertificateExpiryDateEdit
			// 
			this.CertificateExpiryDateEdit.AllowDrop = true;
			this.CertificateExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CertificateExpiryDateEdit, "GlbExternalPassword.GP_ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_ExpiryDate)));
			this.CertificateExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 152, true);
			this.CertificateExpiryDateEdit.Name = "CertificateExpiryDateEdit";
			this.CertificateExpiryDateEdit.TabIndex = 4;
			// 
			// MessageSenderVATTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageSenderVATTextBox, "GlbExternalPassword.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_MailBoxID)));
			this.MessageSenderVATTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 126, true);
			this.MessageSenderVATTextBox.Name = "MessageSenderVATTextBox";
			this.MessageSenderVATTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 18, true);
			this.MessageSenderVATTextBox.TabIndex = 3;
			// 
			// CertificatePassword
			// 
			this.BindingSource.SetBindingMember(this.CertificatePassword, "GlbExternalPassword.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.CurrentDecryptedCertificatePassphrase)));
			this.CertificatePassword.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePassword.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 70, true);
			this.CertificatePassword.Name = "CertificatePassword";
			this.CertificatePassword.PasswordChar = '*';
			this.CertificatePassword.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 18, true);
			this.CertificatePassword.TabIndex = 1;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "GlbExternalPassword.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 38, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 0;
			// 
			// Status
			// 
			this.BindingSource.SetBindingMember(this.Status, "GlbExternalPassword.PasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.PasswordStatus)));
			this.Status.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Status.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 98, true);
			this.Status.Name = "Status";
			this.Status.ReadOnly = true;
			this.Status.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 18, true);
			this.Status.TabIndex = 2;
			this.Status.TabStop = false;
			// 
			// DefaultUserGroupBox
			// 
			this.DefaultUserGroupBox.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("d9556521-14f7-443b-a608-7e8dd90320e7", "Company's default user for message signing");
			this.DefaultUserGroupBox.Controls.Add(this.DefaultUserCodeFindBox);
			this.DefaultUserGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 222, true);
			this.DefaultUserGroupBox.Name = "DefaultUserGroupBox";
			this.DefaultUserGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 64, true);
			this.DefaultUserGroupBox.TabIndex = 1;
			this.DefaultUserGroupBox.TabStop = false;
			// 
			// DefaultUserCodeFindBox
			// 
			this.DefaultUserCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultUserCodeFindBox, "GlbExternalPassword.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_UserID)));
			this.DefaultUserCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 25, true);
			this.DefaultUserCodeFindBox.Name = "DefaultUserCodeFindBox";
			this.DefaultUserCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.DefaultUserCodeFindBox.ParentType = null;
			this.DefaultUserCodeFindBox.PreBoundMaxLength = 3;
			this.DefaultUserCodeFindBox.ShouldResize = false;
			this.DefaultUserCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.DefaultUserCodeFindBox.TabIndex = 5;
			// 
			// CompanyCredentialsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultUserGroupBox);
			this.Controls.Add(this.CustomsCredentialsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
			this.Name = "CompanyCredentialsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 443, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsCredentialsGroupBox.ResumeLayout(false);
			this.CustomsCredentialsGroupBox.PerformLayout();
			this.CertificateExpiryDateEdit.ResumeLayout(true);
			this.CertificateExpiryDateEdit.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.DefaultUserGroupBox.ResumeLayout(false);
			this.DefaultUserGroupBox.PerformLayout();
			this.DefaultUserCodeFindBox.ResumeLayout(true);
			this.DefaultUserCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox CustomsCredentialsGroupBox;
		ZArchitecture.ZTextBox Status;
		Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		ZArchitecture.ZTextBox CertificatePassword;
		ZArchitecture.ZTextBox MessageSenderVATTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit CertificateExpiryDateEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DefaultUserGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox DefaultUserCodeFindBox;
	}
}
