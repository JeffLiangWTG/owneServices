namespace Enterprise.Customs.KR.GUI
{
	partial class UnipassCertificateUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MailBoxTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SenderIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificatePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateFileTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateLoaderUserControl = new Enterprise.Customs.KR.GUI.KRDigitalCertificateControl_p12();
			this.UserIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.GlbCompanyWrapper);
			// 
			// MailBoxTextBox
			// 
			this.BindingSource.SetBindingMember(this.MailBoxTextBox, "CertificateForUnipass.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GlbCompanyWrapper)(null)).CertificateForUnipass.GP_MailBoxID)));
			this.MailBoxTextBox.CaptionResourceString = null;
			this.MailBoxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 12, true);
			this.MailBoxTextBox.Name = "MailBoxTextBox";
			this.MailBoxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.MailBoxTextBox.TabIndex = 0;
			// 
			// SenderIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.SenderIDTextBox, "CertificateForUnipass.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GlbCompanyWrapper)(null)).CertificateForUnipass.GP_UserID)));
			this.SenderIDTextBox.CaptionResourceString = null;
			this.SenderIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 38, true);
			this.SenderIDTextBox.Name = "SenderIDTextBox";
			this.SenderIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.SenderIDTextBox.TabIndex = 1;
			// 
			// CertificatePasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificatePasswordTextBox, "CertificateForUnipass.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GlbCompanyWrapper)(null)).CertificateForUnipass.CurrentDecryptedCertificatePassphrase)));
			this.CertificatePasswordTextBox.CaptionResourceString = null;
			this.CertificatePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 64, true);
			this.CertificatePasswordTextBox.Name = "CertificatePasswordTextBox";
			this.CertificatePasswordTextBox.PasswordChar = '*';
			this.CertificatePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CertificatePasswordTextBox.TabIndex = 2;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "CertificateForUnipass.GP_PasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GlbCompanyWrapper)(null)).CertificateForUnipass.GP_PasswordStatus)));
			this.StatusTextBox.CaptionResourceString = null;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 90, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.StatusTextBox.TabIndex = 3;
			// 
			// StatusReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusReasonTextBox, "CertificateForUnipass.GP_StatusReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GlbCompanyWrapper)(null)).CertificateForUnipass.GP_StatusReason)));
			this.StatusReasonTextBox.CaptionResourceString = null;
			this.StatusReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 116, true);
			this.StatusReasonTextBox.Name = "StatusReasonTextBox";
			this.StatusReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.StatusReasonTextBox.TabIndex = 4;
			// 
			// CertificateFileTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificateFileTextBox, "CertificateForUnipass.CertificateStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GlbCompanyWrapper)(null)).CertificateForUnipass.CertificateStatus)));
			this.CertificateFileTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0c47fb16-89bf-43f7-9dac-2a3d843766b3", "Certificate File");
			this.CertificateFileTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 141, true);
			this.CertificateFileTextBox.Name = "CertificateFileTextBox";
			this.CertificateFileTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CertificateFileTextBox.TabIndex = 5;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "CertificateForUnipass.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.GlbCompanyWrapper)(null)).CertificateForUnipass.GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 167, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 6;
			// 
			// UserIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.UserIDTextBox, "CertificateForUnipass.GP_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GlbCompanyWrapper)(null)).CertificateForUnipass.GP_Name)));
			this.UserIDTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("05151B01-33C7-4311-A35F-414A0C025DDC", "User ID");
			this.UserIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 199, true);
			this.UserIDTextBox.Name = "UserIDTextBox";
			this.UserIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.UserIDTextBox.TabIndex = 7;
			// 
			// UnipassCertificateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UserIDTextBox);
			this.Controls.Add(this.CertificateFileTextBox);
			this.Controls.Add(this.StatusReasonTextBox);
			this.Controls.Add(this.StatusTextBox);
			this.Controls.Add(this.CertificatePasswordTextBox);
			this.Controls.Add(this.SenderIDTextBox);
			this.Controls.Add(this.MailBoxTextBox);
			this.Controls.Add(this.CertificateLoaderUserControl);
			this.Name = "UnipassCertificateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 247, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.ZTextBox SenderIDTextBox;
		internal ZArchitecture.ZTextBox CertificatePasswordTextBox;
		internal ZArchitecture.ZTextBox MailBoxTextBox;
		internal ZArchitecture.ZTextBox StatusTextBox;
		internal ZArchitecture.ZTextBox StatusReasonTextBox;
		internal ZArchitecture.ZTextBox CertificateFileTextBox;
		internal KRDigitalCertificateControl_p12 CertificateLoaderUserControl;
		internal ZArchitecture.ZTextBox UserIDTextBox;
	}
}
