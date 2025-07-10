using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ROSCredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageSenderEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificatePassword = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.Status = new Enterprise.ZArchitecture.ZTextBox();
			this.MailboxRequestButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EMCSROSCredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EMCSCertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.EMCSExternalPasswordCertificateGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ROSCredentialsGroupBox.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.EMCSROSCredentialsGroupBox.SuspendLayout();
			this.EMCSCertificateLoaderUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EMCSExternalPasswordCertificateGrid)).BeginInit();
			this.EMCSExternalPasswordCertificateGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.GlbCompanyWrapper);
			// 
			// ROSCredentialsGroupBox
			// 
			this.ROSCredentialsGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("1B08C616-58C6-47B6-AF34-37A82772C527", "Revenue Online Service Credentials – AES/AIS/NCTS");
			this.ROSCredentialsGroupBox.Controls.Add(this.MessageSenderEORITextBox);
			this.ROSCredentialsGroupBox.Controls.Add(this.CertificatePassword);
			this.ROSCredentialsGroupBox.Controls.Add(this.CertificateLoaderUserControl);
			this.ROSCredentialsGroupBox.Controls.Add(this.Status);
			this.ROSCredentialsGroupBox.Controls.Add(this.MailboxRequestButton);
			this.ROSCredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.ROSCredentialsGroupBox.Name = "ROSCredentialsGroupBox";
			this.ROSCredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 182, true);
			this.ROSCredentialsGroupBox.TabIndex = 0;
			this.ROSCredentialsGroupBox.TabStop = false;
			// 
			// MessageSenderEORITextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageSenderEORITextBox, "GlbExternalPassword.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_MailBoxID)));
			this.MessageSenderEORITextBox.CaptionResourceString = null;
			this.MessageSenderEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 126, true);
			this.MessageSenderEORITextBox.Name = "MessageSenderEORITextBox";
			this.MessageSenderEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 23, true);
			this.MessageSenderEORITextBox.TabIndex = 3;
			// 
			// CertificatePassword
			// 
			this.BindingSource.SetBindingMember(this.CertificatePassword, "GlbExternalPassword.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.CurrentDecryptedCertificatePassphrase)));
			this.CertificatePassword.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("e5539309-d5f3-475d-b56d-fdf55a233d6d", "Certificate Password");
			this.CertificatePassword.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePassword.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 70, true);
			this.CertificatePassword.Name = "CertificatePassword";
			this.CertificatePassword.PasswordChar = '*';
			this.CertificatePassword.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 23, true);
			this.CertificatePassword.TabIndex = 1;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "GlbExternalPassword.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_Certificate)));
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.PasswordStatus)));
			this.Status.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("76cedcb5-55a6-4d4b-9f33-419da5533f08", "Certificate Status");
			this.Status.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Status.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 98, true);
			this.Status.Name = "Status";
			this.Status.ReadOnly = true;
			this.Status.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 23, true);
			this.Status.TabIndex = 2;
			// 
			// MailboxRequestButton
			// 
			this.MailboxRequestButton.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("D67E51E9-4912-4E76-A325-2FA0C38A8284", "Mailbox Request");
			this.MailboxRequestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 154, true);
			this.MailboxRequestButton.Name = "MailboxRequestButton";
			this.MailboxRequestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.MailboxRequestButton.TabIndex = 4;
			this.MailboxRequestButton.ToolTipCaption = null;
			this.MailboxRequestButton.Click += new System.EventHandler(this.MailboxRequestButton_Click);
			// 
			// EMCSROSCredentialsGroupBox
			// 
			this.EMCSROSCredentialsGroupBox.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("58dc7128-0e4f-4398-8b69-14efc7c62b55", "Revenue Online Service Credentials – EMCS");
			this.EMCSROSCredentialsGroupBox.Controls.Add(this.EMCSCertificateLoaderUserControl);
			this.EMCSROSCredentialsGroupBox.Controls.Add(this.EMCSExternalPasswordCertificateGrid);
			this.EMCSROSCredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 210, true);
			this.EMCSROSCredentialsGroupBox.Name = "EMCSROSCredentialsGroupBox";
			this.EMCSROSCredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 237, true);
			this.EMCSROSCredentialsGroupBox.TabIndex = 1;
			this.EMCSROSCredentialsGroupBox.TabStop = false;
			// 
			// EMCSCertificateLoaderUserControl
			// 
			this.EMCSCertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EMCSCertificateLoaderUserControl, "EMCSGlbExternalPasswordCollection.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.EMCSGlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).EMCSGlbExternalPasswordCollection)).SyncRoot)).GP_Certificate)));
			this.EMCSCertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.EMCSCertificateLoaderUserControl.FileDataAsString = "";
			this.EMCSCertificateLoaderUserControl.FileDialogTitle = "";
			this.EMCSCertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.EMCSCertificateLoaderUserControl.InitialDirectory = "";
			this.EMCSCertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 205, true);
			this.EMCSCertificateLoaderUserControl.Name = "EMCSCertificateLoaderUserControl";
			this.EMCSCertificateLoaderUserControl.ReadOnly = false;
			this.EMCSCertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.EMCSCertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.EMCSCertificateLoaderUserControl.TabIndex = 1;
			// 
			// EMCSExternalPasswordCertificateGrid
			// 
			this.EMCSExternalPasswordCertificateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EMCSExternalPasswordCertificateGrid, "EMCSGlbExternalPasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).EMCSGlbExternalPasswordCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.EMCSGlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).EMCSGlbExternalPasswordCollection)).SyncRoot)).GP_MailBoxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.EMCSGlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).EMCSGlbExternalPasswordCollection)).SyncRoot)).CurrentDecryptedCertificatePassphrase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.IE.Business.EMCSGlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).EMCSGlbExternalPasswordCollection)).SyncRoot)).GP_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.EMCSGlbCompanyCredential)(((System.Collections.IList)(((Enterprise.Customs.IE.Business.GlbCompanyWrapper)(null)).EMCSGlbExternalPasswordCollection)).SyncRoot)).PasswordStatus)));
			this.EMCSExternalPasswordCertificateGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "GP_MailBoxID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "CurrentDecryptedCertificatePassphrase";
			zTextBoxColumnStyleInfo2.PasswordChar = '*';
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo1.ColumnName = "GP_ExpiryDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "PasswordStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EMCSExternalPasswordCertificateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EMCSExternalPasswordCertificateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EMCSExternalPasswordCertificateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EMCSExternalPasswordCertificateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EMCSExternalPasswordCertificateGrid.GridId = "f85919f9-8eae-4b21-a7e6-92fc20122998";
			this.EMCSExternalPasswordCertificateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EMCSExternalPasswordCertificateGrid.LayoutKey = "EMCSExternalPasswordCertificateGrid";
			this.EMCSExternalPasswordCertificateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
			this.EMCSExternalPasswordCertificateGrid.Name = "EMCSExternalPasswordCertificateGrid";
			this.EMCSExternalPasswordCertificateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 184, true);
			this.EMCSExternalPasswordCertificateGrid.TabIndex = 0;
			// 
			// CompanyCredentialsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EMCSROSCredentialsGroupBox);
			this.Controls.Add(this.ROSCredentialsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
			this.Name = "CompanyCredentialsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 443, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ROSCredentialsGroupBox.ResumeLayout(false);
			this.ROSCredentialsGroupBox.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.EMCSROSCredentialsGroupBox.ResumeLayout(false);
			this.EMCSROSCredentialsGroupBox.PerformLayout();
			this.EMCSCertificateLoaderUserControl.ResumeLayout(true);
			this.EMCSCertificateLoaderUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EMCSExternalPasswordCertificateGrid)).EndInit();
			this.EMCSExternalPasswordCertificateGrid.ResumeLayout(false);
			this.EMCSExternalPasswordCertificateGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ROSCredentialsGroupBox;
		ZArchitecture.ZTextBox Status;
		internal Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		ZArchitecture.ZTextBox CertificatePassword;
		internal ZArchitecture.GUI.ZGroupBox EMCSROSCredentialsGroupBox;
		internal ZArchitecture.ZGrid EMCSExternalPasswordCertificateGrid;
		internal Registry.GUI.DigitalCertificateControl_p12 EMCSCertificateLoaderUserControl;
		internal ZArchitecture.ZTextBox MessageSenderEORITextBox;
		internal ZButton MailboxRequestButton;
	}
}
