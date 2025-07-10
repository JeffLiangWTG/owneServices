namespace Enterprise.Customs.AR.Manifest.GUI
{
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
			this.CertificatePassword = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.Status = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AR.Manifest.Business.GlbCompanyWrapper);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.AR.Manifest.GUI.Res.GetData("GlbCompanyForm|78AB97F0-9F71-4444-96F6-219E4CE56BB1", "Company Credentials");
			this.zGroupBox1.Controls.Add(this.CertificatePassword);
			this.zGroupBox1.Controls.Add(this.CertificateLoaderUserControl);
			this.zGroupBox1.Controls.Add(this.Status);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 138, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// CertificatePassword
			// 
			this.BindingSource.SetBindingMember(this.CertificatePassword, "GlbExternalPassword.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AR.Manifest.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.CurrentDecryptedCertificatePassphrase)));
			this.CertificatePassword.CaptionResourceString = Enterprise.Customs.AR.Manifest.GUI.Res.GetData("32784D24-2371-430B-8A8B-B3D1E9EEB368", "Certificate Password");
			this.CertificatePassword.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePassword.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 72, true);
			this.CertificatePassword.Name = "CertificatePassword";
			this.CertificatePassword.PasswordChar = '*';
			this.CertificatePassword.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 13, true);
			this.CertificatePassword.TabIndex = 4;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "GlbExternalPassword.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AR.Manifest.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_Certificate)));
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
			this.CertificateLoaderUserControl.TabIndex = 3;
			// 
			// Status
			// 
			this.BindingSource.SetBindingMember(this.Status, "GlbExternalPassword.PasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AR.Manifest.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.PasswordStatus)));
			this.Status.CaptionResourceString = Enterprise.Customs.AR.Manifest.GUI.Res.GetData("D78E26EA-D892-4231-9CE4-27CE21222977", "Certificate Status");
			this.Status.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Status.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 98, true);
			this.Status.Name = "CertificateStatus";
			this.Status.ReadOnly = true;
			this.Status.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 13, true);
			this.Status.TabIndex = 5;
			// 
			// CompanyCredentialsDetailsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zGroupBox1);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
			this.Name = "CompanyCredentialsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZTextBox Status;
		private Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		private ZArchitecture.ZTextBox CertificatePassword;
	}
}
