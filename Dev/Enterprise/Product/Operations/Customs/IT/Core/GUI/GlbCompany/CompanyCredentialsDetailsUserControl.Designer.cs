namespace Enterprise.Customs.IT.GUI
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AccAndUserGroupIT = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ITCertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.CertificateExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CertificatePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificatePasswordStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccountUCCListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ITAccUserGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ITCredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AccAndUserGroupIT.SuspendLayout();
			this.CertificateGroupBox.SuspendLayout();
			this.ITCertificatePanel.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.CertificateExpiryDateEdit.SuspendLayout();
			this.AccountUCCListGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ITAccUserGrid)).BeginInit();
			this.ITAccUserGrid.SuspendLayout();
			this.ITCredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.GlbCompanyWrapper);
			// 
			// AccAndUserGroupIT
			// 
			this.AccAndUserGroupIT.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("GlbCompanyForm|0C872489-CE3D-4C73-A7B8-EED440E36388", "Subscriptions Management");
			this.AccAndUserGroupIT.Controls.Add(this.CertificateGroupBox);
			this.AccAndUserGroupIT.Controls.Add(this.AccountUCCListGroupBox);
			this.AccAndUserGroupIT.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccAndUserGroupIT.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AccAndUserGroupIT.Name = "AccAndUserGroupIT";
			this.AccAndUserGroupIT.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 340, true);
			this.AccAndUserGroupIT.TabIndex = 1;
			this.AccAndUserGroupIT.TabStop = false;
			// 
			// CertificateGroupBox
			// 
			this.CertificateGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("GlbCompanyForm|FA3C2FAC-74D7-450B-BA5B-66EAA0209B20", "MAU Certificate");
			this.CertificateGroupBox.Controls.Add(this.ITCertificatePanel);
			this.CertificateGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 190, true);
			this.CertificateGroupBox.Name = "CertificateGroupBox";
			this.CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 147, true);
			this.CertificateGroupBox.TabIndex = 0;
			this.CertificateGroupBox.TabStop = false;
			// 
			// ITCertificatePanel
			// 
			this.ITCertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ITCertificatePanel.Controls.Add(this.CertificateLabel);
			this.ITCertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.ITCertificatePanel.Controls.Add(this.CertificateExpiryDateEdit);
			this.ITCertificatePanel.Controls.Add(this.CertificatePasswordTextBox);
			this.ITCertificatePanel.Controls.Add(this.CertificatePasswordStatusTextBox);
			this.ITCertificatePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ITCertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ITCertificatePanel.Name = "ITCertificatePanel";
			this.ITCertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ITCertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 128, true);
			this.ITCertificatePanel.TabIndex = 2;
			// 
			// CertificateLabel
			// 
			this.CertificateLabel.AutoSize = true;
			this.CertificateLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("1b440dbd-19ae-47a9-a56e-adac7e1c8b26", "Certificate");
			this.CertificateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CertificateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 13, true);
			this.CertificateLabel.Name = "CertificateLabel";
			this.CertificateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.CertificateLabel.TabIndex = 0;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "PasswordCollection.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.GlbMauExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 8, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 1;
			// 
			// CertificateExpiryDateEdit
			// 
			this.CertificateExpiryDateEdit.AllowDrop = true;
			this.CertificateExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CertificateExpiryDateEdit, "PasswordCollection.GP_ExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.GlbMauExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).GP_ExpiryDate)));
			this.CertificateExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 63, true);
			this.CertificateExpiryDateEdit.Name = "CertificateExpiryDateEdit";
			this.CertificateExpiryDateEdit.TabIndex = 4;
			// 
			// CertificatePasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificatePasswordTextBox, "PasswordCollection.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.GlbMauExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).CurrentDecryptedCertificatePassphrase)));
			this.CertificatePasswordTextBox.CaptionResourceString = null;
			this.CertificatePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 37, true);
			this.CertificatePasswordTextBox.Name = "CertificatePasswordTextBox";
			this.CertificatePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.CertificatePasswordTextBox.TabIndex = 2;
			this.CertificatePasswordTextBox.UseSystemPasswordChar = true;
			// 
			// CertificatePasswordStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertificatePasswordStatusTextBox, "PasswordCollection.PasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.GlbMauExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).PasswordStatus)));
			this.CertificatePasswordStatusTextBox.CaptionResourceString = null;
			this.CertificatePasswordStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePasswordStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 37, true);
			this.CertificatePasswordStatusTextBox.Name = "CertificatePasswordStatusTextBox";
			this.CertificatePasswordStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.CertificatePasswordStatusTextBox.TabIndex = 3;
			// 
			// AccountUCCListGroupBox
			// 
			this.AccountUCCListGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("GlbCompanyForm|91DDA3A6-C052-4578-9645-33F3672C6249", "Account UCC List");
			this.AccountUCCListGroupBox.Controls.Add(this.ITAccUserGrid);
			this.AccountUCCListGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountUCCListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AccountUCCListGroupBox.Name = "AccountUCCListGroupBox";
			this.AccountUCCListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 321, true);
			this.AccountUCCListGroupBox.TabIndex = 3;
			this.AccountUCCListGroupBox.TabStop = false;
			// 
			// ITAccUserGrid
			// 
			this.ITAccUserGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ITAccUserGrid, "PasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbCompanyWrapper)(null)).PasswordCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.GlbMauExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).GP_UserID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.GlbMauExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).GP_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.GlbMauExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbCompanyWrapper)(null)).PasswordCollection)).SyncRoot)).GP_MailBoxID)));
			this.ITAccUserGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "GP_UserID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "GP_Name";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo2.ColumnName = "GP_MailBoxID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ITAccUserGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ITAccUserGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ITAccUserGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ITAccUserGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ITAccUserGrid.GridId = "31636bbf-ac4a-43e8-af5c-f68b108f19ba";
			this.ITAccUserGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ITAccUserGrid.LayoutKey = "ITAccUserGrid";
			this.ITAccUserGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ITAccUserGrid.Name = "ITAccUserGrid";
			this.ITAccUserGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 302, true);
			this.ITAccUserGrid.TabIndex = 1;
			this.ITAccUserGrid.AfterBind += new System.EventHandler(this.ITAccUserGrid_AfterBind);
			this.ITAccUserGrid.CurrentCellChanged += new System.EventHandler(this.ITAccUserGrid_CurrentCellChanged);
			// 
			// ITCredentialsPanel
			// 
			this.ITCredentialsPanel.Controls.Add(this.AccAndUserGroupIT);
			this.ITCredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ITCredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ITCredentialsPanel.Name = "ITCredentialsPanel";
			this.ITCredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 340, true);
			this.ITCredentialsPanel.TabIndex = 1;
			// 
			// CompanyCredentialsDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ITCredentialsPanel);
			this.Name = "CompanyCredentialsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 340, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AccAndUserGroupIT.ResumeLayout(false);
			this.AccAndUserGroupIT.PerformLayout();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			this.ITCertificatePanel.ResumeLayout(false);
			this.ITCertificatePanel.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.CertificateExpiryDateEdit.ResumeLayout(true);
			this.CertificateExpiryDateEdit.PerformLayout();
			this.AccountUCCListGroupBox.ResumeLayout(false);
			this.AccountUCCListGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ITAccUserGrid)).EndInit();
			this.ITAccUserGrid.ResumeLayout(false);
			this.ITAccUserGrid.PerformLayout();
			this.ITCredentialsPanel.ResumeLayout(false);
			this.ITCredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel ITCertificatePanel;
		Enterprise.ZArchitecture.GUI.ZPanel ITCredentialsPanel;
		Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
		Enterprise.ZArchitecture.ZGrid ITAccUserGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox AccAndUserGroupIT;
		Enterprise.ZArchitecture.ZTextBox CertificatePasswordTextBox;
		Enterprise.ZArchitecture.ZTextBox CertificatePasswordStatusTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit CertificateExpiryDateEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox AccountUCCListGroupBox;
		Enterprise.ZArchitecture.ZLabel CertificateLabel;
		Enterprise.ZArchitecture.GUI.ZGroupBox CertificateGroupBox;
	}
}
