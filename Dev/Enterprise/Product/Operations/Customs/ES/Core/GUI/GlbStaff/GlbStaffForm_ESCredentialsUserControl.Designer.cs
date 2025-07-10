namespace Enterprise.Customs.ES.GUI
{
	partial class GlbStaffForm_ESCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.ESCertGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ESAuthorisationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CertGroupES = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ESCertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificateLoaderUserControl = new Enterprise.Registry.GUI.DigitalCertificateControl_p12();
			this.LoadCertLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ESCredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ESCertGrid)).BeginInit();
			this.ESCertGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ESAuthorisationsGrid)).BeginInit();
			this.ESAuthorisationsGrid.SuspendLayout();
			this.CertGroupES.SuspendLayout();
			this.ESCertificatePanel.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.ESCredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.GlbStaffWrapper);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ESCertGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 376, true);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ESAuthorisationsGrid);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(272);
			this.splitContainer1.TabIndex = 0;
			// 
			// ESCertGrid
			// 
			this.ESCertGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ESCertGrid, "ESBPasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).GP_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).CertificateStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).CurrentDecryptedCertificatePassphrase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).GP_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).GP_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).GP_UserID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).GP_MailBoxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).GP_PasswordStatus)));
			this.ESCertGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("d347b134-67af-4954-a7f0-b5ddd7f09bd5", "Certificate Name");
			zTextBoxColumnStyleInfo1.ColumnName = "GP_Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("8e2057a1-009c-4201-a063-cee51d619530", "Certificate");
			zDropEditColumnStyleInfo1.ColumnName = "CertificateStatus";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("4b1427ab-cd9d-46cc-b057-2d5719891127", "Certificate Password");
			zTextBoxColumnStyleInfo2.ColumnName = "CurrentDecryptedCertificatePassphrase";
			zTextBoxColumnStyleInfo2.PasswordChar = '*';
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("3d71966f-d5b7-4cfd-a7de-444a6489ccb2", "Issue Date");
			zDateEditColumnStyleInfo1.ColumnName = "GP_IssueDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("a3215e64-b029-487f-9d4d-6b81e8df688b", "Expiry Date");
			zDateEditColumnStyleInfo2.ColumnName = "GP_ExpiryDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("b813f771-bf6e-4775-b0c5-2855a4bbf0b3", "Thumb-print");
			zTextBoxColumnStyleInfo3.ColumnName = "GP_UserID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(256);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("7d2d294e-cf49-4e86-a227-56d71d5c7e71", "Certificate NIF");
			zTextBoxColumnStyleInfo4.ColumnName = "GP_MailBoxID";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("2741E230-50F9-4A16-82A2-46778A116940", "Status");
			zTextBoxColumnStyleInfo5.ColumnName = "GP_PasswordStatus";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ESCertGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ESCertGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ESCertGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ESCertGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ESCertGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ESCertGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ESCertGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ESCertGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ESCertGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ESCertGrid.GridId = "8263C625-5E61-4B60-8847-0FB29F7F4ED6";
			this.ESCertGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ESCertGrid.LayoutKey = "ESCertGrid";
			this.ESCertGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ESCertGrid.Name = "ESCertGrid";
			this.ESCertGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 272, true);
			this.ESCertGrid.TabIndex = 1;
			this.ESCertGrid.AfterBind += new System.EventHandler(this.ESCertGrid_AfterBind);
			this.ESCertGrid.CurrentCellChanged += new System.EventHandler(this.ESCertGrid_CurrentCellChanged);
			this.ESCertGrid.Leave += new System.EventHandler(this.ESCertGrid_Leave);
			// 
			// ESAuthorisationsGrid
			// 
			this.ESAuthorisationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ESAuthorisationsGrid, "ESBPasswordCollection.Authorisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).Authorisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ES.Business.GlbExternalPasswordAuthorisation)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).Authorisations)).SyncRoot)).GEA_GS_AuthorisedStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.GlbExternalPasswordAuthorisation)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).Authorisations)).SyncRoot)).StaffName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.GlbExternalPasswordAuthorisation)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).Authorisations)).SyncRoot)).GEA_SystemCreateTimeUtc)));
			this.ESAuthorisationsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("a0179b83-ac51-44a7-a705-4245dab6d912", "Authorized Staff");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GEA_GS_AuthorisedStaff";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("9a2644ff-e4fc-4b7f-97c0-bc02658bff3f", "Full Name");
			zTextBoxColumnStyleInfo6.ColumnName = "StaffName";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("be581c9c-fb71-4cd5-aa5f-3ce12e31fb71", "Date of Permission Creation");
			zDateEditColumnStyleInfo3.ColumnName = "GEA_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(158);
			this.ESAuthorisationsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ESAuthorisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ESAuthorisationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ESAuthorisationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ESAuthorisationsGrid.GridId = "22C11C68-225A-4FC1-B2C9-56450BFFE210";
			this.ESAuthorisationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ESAuthorisationsGrid.LayoutKey = "ESAuthorisationsGrid";
			this.ESAuthorisationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ESAuthorisationsGrid.Name = "ESAuthorisationsGrid";
			this.ESAuthorisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 100, true);
			this.ESAuthorisationsGrid.TabIndex = 1;
			// 
			// CertGroupES
			// 
			this.CertGroupES.Controls.Add(this.splitContainer1);
			this.CertGroupES.Controls.Add(this.ESCertificatePanel);
			this.CertGroupES.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertGroupES.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertGroupES.Name = "CertGroupES";
			this.CertGroupES.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.CertGroupES.TabIndex = 1;
			this.CertGroupES.TabStop = false;
			// 
			// ESCertificatePanel
			// 
			this.ESCertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ESCertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.ESCertificatePanel.Controls.Add(this.LoadCertLabel);
			this.ESCertificatePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ESCertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 392, true);
			this.ESCertificatePanel.Name = "ESCertificatePanel";
			this.ESCertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(58, 7, 58, 7, true);
			this.ESCertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 40, true);
			this.ESCertificatePanel.TabIndex = 14;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "ESBPasswordCollection.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.GlbExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.GlbStaffWrapper)(null)).ESBPasswordCollection)).SyncRoot)).GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 7, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 0;
			// 
			// LoadCertLabel
			// 
			this.LoadCertLabel.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("08f164b9-b2b3-409c-8afc-2ab6ebb00f87", "Load Certificate:");
			this.LoadCertLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.LoadCertLabel.IsFontBold = true;
			this.LoadCertLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 9, true);
			this.LoadCertLabel.Name = "LoadCertLabel";
			this.LoadCertLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.LoadCertLabel.TabIndex = 0;
			// 
			// ESCredentialsPanel
			// 
			this.ESCredentialsPanel.Controls.Add(this.CertGroupES);
			this.ESCredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ESCredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.ESCredentialsPanel.Name = "ESCredentialsPanel";
			this.ESCredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.ESCredentialsPanel.TabIndex = 5;
			// 
			// GlbStaffForm_ESCredentialsUserControl
			// 
			this.Controls.Add(this.ESCredentialsPanel);
			this.Name = "GlbStaffForm_ESCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.ESCredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ESCertGrid)).EndInit();
			this.ESCertGrid.ResumeLayout(false);
			this.ESCertGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ESAuthorisationsGrid)).EndInit();
			this.ESAuthorisationsGrid.ResumeLayout(false);
			this.ESAuthorisationsGrid.PerformLayout();
			this.CertGroupES.ResumeLayout(false);
			this.CertGroupES.PerformLayout();
			this.ESCertificatePanel.ResumeLayout(false);
			this.ESCertificatePanel.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.ESCredentialsPanel.ResumeLayout(false);
			this.ESCredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		Enterprise.ZArchitecture.GUI.ZPanel ESCertificatePanel;
		Enterprise.ZArchitecture.GUI.ZPanel ESCredentialsPanel;
		Enterprise.ZArchitecture.ZGrid ESCertGrid;
		Enterprise.ZArchitecture.ZGrid ESAuthorisationsGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox CertGroupES;
		Enterprise.ZArchitecture.ZLabel LoadCertLabel;
		private Enterprise.Registry.GUI.DigitalCertificateControl_p12 CertificateLoaderUserControl;
	}
}
