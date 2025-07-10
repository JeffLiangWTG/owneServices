using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IdentityApplication.GUI
{
	partial class EdiIdentityApplicationForm
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zUrlTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zUrlDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zUrlTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zScopeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			//
			//Database
			//
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo01 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo01 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo02 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo03 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo04 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo05 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo06 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo07 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo08 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo09 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ApplicationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApplicationNameTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TenantIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApplicationTypeDropBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductDropBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsRollbackCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RedirectUrlGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RedirectUrlGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PermissionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermissionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LicenceDatabaseTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DatabaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DatabasesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RedirectUrlGrid)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.CertificateGroupBox.SuspendLayout();
			this.ApplicationGroupBox.SuspendLayout();
			this.CertificatesGrid.SuspendLayout();
			this.RedirectUrlGroupBox.SuspendLayout();
			this.RedirectUrlGrid.SuspendLayout();
			this.LicenceDatabaseTabPage.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.DatabaseGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabasesGrid)).BeginInit();
			this.DatabasesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 552, true);
			this.MainTabControl.Controls.Add(this.LicenceDatabaseTabPage);
			this.MainTabControl.Controls.SetChildIndex(this.LicenceDatabaseTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 525, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(933, 515, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(933, 515, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 552, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(941, 24, true);
			// 
			// HelpMenuItem
			// 
			this.HelpMenuItem.Index = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EdiIdentityApplication);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.ApplicationGroupBox);
			this.zPanel1.Controls.Add(this.CertificateGroupBox);
			this.zPanel1.Controls.Add(this.RedirectUrlGroupBox);
			this.zPanel1.Controls.Add(this.PermissionGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1155, 675, true);
			this.zPanel1.TabIndex = 0;
			// 
			// ApplicationGroupBox
			//
			this.ApplicationGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("D7B08D0E-9E9F-4BE1-BE2D-749FAA227942", "Application");
			this.ApplicationGroupBox.Controls.Add(this.ApplicationNameTextbox);
			this.ApplicationGroupBox.Controls.Add(this.ClientIdTextBox);
			this.ApplicationGroupBox.Controls.Add(this.TenantIdTextBox);
			this.ApplicationGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.ApplicationGroupBox.Controls.Add(this.ApplicationTypeDropBox);
			this.ApplicationGroupBox.Controls.Add(this.IsRollbackCheckBox);
			this.ApplicationGroupBox.Controls.Add(this.ProductDropBox);
			this.ApplicationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.ApplicationGroupBox.Name = "ApplicationGroupBox";
			this.ApplicationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 108, true);
			this.ApplicationGroupBox.TabIndex = 0;
			this.ApplicationGroupBox.TabStop = false;
			this.ApplicationGroupBox.Text = "Application";
			// 
			// TenantIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.TenantIdTextBox, "TenantID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityApplication)(null)).TenantID)));
			this.TenantIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TenantIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 19, true);
			this.TenantIdTextBox.Name = "TenantIdTextBox";
			this.TenantIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.TenantIdTextBox.TabIndex = 0;
			// 
			// ClientIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientIdTextBox, "IDA_ClientID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityApplication)(null)).IDA_ClientID)));
			this.ClientIdTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("E1699189-AA6E-4F0D-8D81-13810ED2FDFF", "Client ID");
			this.ClientIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 49, true);
			this.ClientIdTextBox.Name = "ClientIdTextBox";
			this.ClientIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.ClientIdTextBox.TabIndex = 2;
			this.ClientIdTextBox.ReadOnly = true;
			// 
			// ApplicationTypeDropBox
			// 
			this.ApplicationTypeDropBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicationTypeDropBox, "ApplicationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((EdiIdentityApplication)(null)).ApplicationType)));
			this.ApplicationTypeDropBox.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.ApplicationTypeDropBox.BindToList = "LicenceDatabase.Lookups.DatabaseTypesList";
			this.ApplicationTypeDropBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ApplicationTypeDropBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 20, true);
			this.ApplicationTypeDropBox.Name = "ApplicationTypeDropBox";
			this.ApplicationTypeDropBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ApplicationTypeDropBox.TabIndex = 3;
			this.ApplicationTypeDropBox.ShowDescriptionBox = false;
			this.ApplicationTypeDropBox.PreBoundMaxLength = 3;
			this.ApplicationTypeDropBox.ShouldResizeByMaxLength = false;
			// 
			// ProductDropBox
			//
			this.ProductDropBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductDropBox, "Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((EdiIdentityApplication)(null)).Product)));
			this.ProductDropBox.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.ProductDropBox.BindToList = "LicenceDatabase.Lookups.ProductTypeList";
			this.ProductDropBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ProductDropBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 49, true);
			this.ProductDropBox.Name = "ProductDropBox";
			this.ProductDropBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ProductDropBox.TabIndex = 4;
			this.ProductDropBox.ShowDescriptionBox = false;
			this.ProductDropBox.PreBoundMaxLength = 3;
			this.ProductDropBox.ShouldResizeByMaxLength = false;
			// 
			// IsActiveCheckBox
			//
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "IDA_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((EdiIdentityApplication)(null)).IDA_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2655D0D2-4CE0-48E3-AE55-5F14A74E4C24", "Is Active");
			this.IsActiveCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 20, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.IsActiveCheckBox.TabIndex = 5;
			this.IsActiveCheckBox.Enabled = false;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsRollbackCheckBox
			//
			this.IsRollbackCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsRollbackCheckBox, "IDA_IsRollback");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((EdiIdentityApplication)(null)).IDA_IsRollback)));
			this.IsRollbackCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("41406751-2B09-4902-A67B-C1F8990D6BB2", "Is Roll-back");
			this.IsRollbackCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IsRollbackCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(610, 49, true);
			this.IsRollbackCheckBox.Name = "IsRollbackCheckBox";
			this.IsRollbackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.IsRollbackCheckBox.TabIndex = 6;
			this.IsRollbackCheckBox.Enabled = false;
			this.IsRollbackCheckBox.UseVisualStyleBackColor = true;
			// 
			// ApplicationNameTextbox
			// 
			this.BindingSource.SetBindingMember(this.ApplicationNameTextbox, "IDA_ApplicationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityApplication)(null)).IDA_ApplicationName)));
			this.ApplicationNameTextbox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("34E218A7-3716-495E-9CBD-4E0B99A17239", "Application Name");
			this.ApplicationNameTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ApplicationNameTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 79, true);
			this.ApplicationNameTextbox.Name = "ApplicationNameTextbox";
			this.ApplicationNameTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.ApplicationNameTextbox.TabIndex = 7;
			// 
			// CertificateGroupBox
			//
			this.CertificateGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5F5FA3CD-8F84-4581-83C7-BE6C9F620275", "Certificate");
			this.CertificateGroupBox.Controls.Add(this.CertificatesGrid);
			this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 115, true);
			this.CertificateGroupBox.Name = "CertificateGroupBox";
			this.CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 120, true);
			this.CertificateGroupBox.TabIndex = 0;
			this.CertificateGroupBox.TabStop = false;
			this.CertificateGroupBox.Text = "Certificate Info";
			// 
			// CertificatesGrid
			// 
			this.CertificatesGrid.AllowNavigation = false;
			this.CertificatesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CertificatesGrid, "Certificates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_CARoot)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_CertificateSigningRequest)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_CertificateThumbprint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_CertificateIssuedBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_CertificateIssuedTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_IsCertificateRevoked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_ProcessingStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IdentityCertificate.Business.EdiIdentityCertificate)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Certificates)).SyncRoot)).ICE_SystemCreateTimeUtc)));
			this.CertificatesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("79763775-25EE-4E2B-B24F-E8C01A36DF51", "AWS Issuing CA");
			zDropEditColumnStyleInfo1.ColumnName = "ICE_CARoot";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("04B478C4-05A9-449B-B2E0-7D2EDD75875A", "CSR");
			zTextBoxColumnStyleInfo1.ColumnName = "ICE_CertificateSigningRequest";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("CC1E5613-2E7F-4503-AC4D-AAB5A25AD157", "Thumbprint");
			zTextBoxColumnStyleInfo2.ColumnName = "ICE_CertificateThumbprint";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("74F02B91-2124-46E3-844C-24F6A018A187", "Issued By");
			zTextBoxColumnStyleInfo3.ColumnName = "ICE_CertificateIssuedBy";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("633C8B65-B339-4A40-A927-E97F2469F96C", "Issued To");
			zTextBoxColumnStyleInfo4.ColumnName = "ICE_CertificateIssuedTo";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("B9143BD5-990E-4940-A7EE-566D72FF332A", "Is Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "ICE_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7D80CEB9-F0C1-49E1-9FF3-C73629D4445E", "Is Revoked");
			zCheckBoxColumnStyleInfo2.ColumnName = "ICE_IsCertificateRevoked";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8B0E6C30-70BA-44DA-82BD-E1B51E72211B", "Status");
			zTextBoxColumnStyleInfo5.ColumnName = "ICE_ProcessingStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo6.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1304242C-BE07-4C56-BF26-671A8364EE63", "Create Time");
			zDateEditColumnStyleInfo6.ColumnName = "ICE_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo6.IsReadOnly = true;
			this.CertificatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CertificatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CertificatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CertificatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CertificatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.CertificatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesGrid.GridId = "7e74c22a-d9c3-42f4-a22c-3aa70216d8b8";
			this.CertificatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CertificatesGrid.LayoutKey = "zGrid1";
			this.CertificatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
			this.CertificatesGrid.Name = "zGrid1";
			this.CertificatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 64, true);
			this.CertificatesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.CertificatesGrid.TabIndex = 0;
			// 
			// RedirectUrlGroupBox
			//
			this.RedirectUrlGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5F5FA3CD-8F84-4581-83C7-BE6C9F620275", "RedirectUrl");
			this.RedirectUrlGroupBox.Controls.Add(this.RedirectUrlGrid);
			this.RedirectUrlGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 240, true);
			this.RedirectUrlGroupBox.Name = "RedirectUrlGroupBox";
			this.RedirectUrlGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 120, true);
			this.RedirectUrlGroupBox.TabIndex = 0;
			this.RedirectUrlGroupBox.TabStop = false;
			this.RedirectUrlGroupBox.Text = "Redirect URL";
			// 
			// RedirectUrlGrid
			// 
			this.RedirectUrlGrid.AllowNavigation = false;
			this.RedirectUrlGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RedirectUrlGrid, "RedirectUrls");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).RedirectUrls)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityRedirectUrl.Business.EdiIdentityRedirectUrl)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).RedirectUrls)).SyncRoot)).IAR_ApplicationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityRedirectUrl.Business.EdiIdentityRedirectUrl)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).RedirectUrls)).SyncRoot)).IAR_RedirectType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityRedirectUrl.Business.EdiIdentityRedirectUrl)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).RedirectUrls)).SyncRoot)).IAR_RedirectUrl)));
			this.RedirectUrlGrid.CaptionVisible = false;
			zUrlTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("E79DA036-1966-44A3-9BA4-6E29BADCF9C9", "Application Name");
			zUrlTextBoxColumnStyleInfo1.ColumnName = "IAR_ApplicationName";
			zUrlTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(139);
			zUrlDropEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4889989D-6E57-46FA-85C2-3CF17F58214F", "Redirect Type");
			zUrlDropEditColumnStyleInfo1.ColumnName = "IAR_RedirectType";
			zUrlDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(139);
			zUrlTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("816E8A9C-DC1F-4756-90BB-CEC55BD7701C", "Redirect URL");
			zUrlTextBoxColumnStyleInfo2.ColumnName = "IAR_RedirectUrl";
			zUrlTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(339);
			this.RedirectUrlGrid.ColumnStyles.Add(zUrlTextBoxColumnStyleInfo1);
			this.RedirectUrlGrid.ColumnStyles.Add(zUrlDropEditColumnStyleInfo1);
			this.RedirectUrlGrid.ColumnStyles.Add(zUrlTextBoxColumnStyleInfo2);
			this.RedirectUrlGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RedirectUrlGrid.GridId = "6018FF8D-3ADC-4D04-9A5E-E78FA5CC915F";
			this.RedirectUrlGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RedirectUrlGrid.LayoutKey = "RedirectUrlGrid";
			this.RedirectUrlGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
			this.RedirectUrlGrid.Name = "RedirectUrlGrid";
			this.RedirectUrlGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 64, true);
			this.RedirectUrlGrid.TabIndex = 0;
			// 
			// PermissionGroupBox
			//
			this.PermissionGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5D2042EE-BF76-4712-9E52-A7C67D16EB3D", "Permissions");
			this.PermissionGroupBox.Controls.Add(this.PermissionGrid);
			this.PermissionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 370, true);
			this.PermissionGroupBox.Name = "PermissionGroupBox";
			this.PermissionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 120, true);
			this.PermissionGroupBox.TabIndex = 0;
			this.PermissionGroupBox.TabStop = false;
			this.PermissionGroupBox.Text = "Permissions";
			// 
			// PermissionGrid
			// 
			this.PermissionGrid.AllowNavigation = false;
			this.PermissionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PermissionGrid, "Permissions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Permissions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityApplicationPermission.Business.EdiIdentityApplicationPermission)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Permissions)).SyncRoot)).IAP_Scope)));
			this.PermissionGrid.CaptionVisible = false;
			zScopeDropEditColumnStyleInfo.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8B0E6C30-70BA-44DA-82BD-E1B51E72211B", "Scope");
			zScopeDropEditColumnStyleInfo.ColumnName = "Permission";
			zScopeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(139);
			zScopeDropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.PermissionGrid.ColumnStyles.Add(zScopeDropEditColumnStyleInfo);
			this.PermissionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermissionGrid.GridId = "611c5c50-bc3f-45c5-b36b-06d15bafa6ca";
			this.PermissionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermissionGrid.LayoutKey = "PermissionGrid";
			this.PermissionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
			this.PermissionGrid.Name = "PermissionGrid";
			this.PermissionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 64, true);
			this.PermissionGrid.TabIndex = 0;
			// 
			// LicenceDatabaseTabPage
			// 
			this.LicenceDatabaseTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("20FE0964-E855-4C1B-A67F-CB88A1595CA3", "Licence Databases");
			this.LicenceDatabaseTabPage.Controls.Add(this.zPanel2);
			this.LicenceDatabaseTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LicenceDatabaseTabPage.Name = "LicenceDatabaseTabPage";
			this.LicenceDatabaseTabPage.ShouldBeReadOnlyInViewMode = false;
			this.LicenceDatabaseTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 373, true);
			this.LicenceDatabaseTabPage.TabIndex = 3;
			this.LicenceDatabaseTabPage.UseVisualStyleBackColor = true;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.DatabaseGroupBox);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 346, true);
			this.zPanel2.TabIndex = 3;
			// 
			// DatabaseGroupBox
			// 
			this.DatabaseGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EDBE6556-2223-4586-98FD-29C69A08F173", "Database");
			this.DatabaseGroupBox.Controls.Add(this.DatabasesGrid);
			this.DatabaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.DatabaseGroupBox.Name = "DatabaseGroupBox";
			this.DatabaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 133, true);
			this.DatabaseGroupBox.TabIndex = 0;
			this.DatabaseGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabaseGroupBox.TabStop = false;
			// 
			// DatabasesGrid
			// 
			this.DatabasesGrid.AllowNavigation = false;
			this.DatabasesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DatabasesGrid, "Licenses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).LD_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).LD_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).LD_DatabaseNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).LD_HostedLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).LD_LicenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).LD_ReleaseRing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).LD_ServerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).WebAccessOrg.OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).EnterpriseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Licenses)).SyncRoot)).EnterpriseID)));
			this.DatabasesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo01.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("F7FD6512-7A7B-47FC-9F13-7E9D30124224", "Active");
			zCheckBoxColumnStyleInfo01.ColumnName = "LD_IsActive";
			zCheckBoxColumnStyleInfo01.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo01.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("B0B13007-8BF7-41C1-B96A-1E9EEEBC40FB", "Product");
			zTextBoxColumnStyleInfo01.ColumnName = "LD_Product";
			zTextBoxColumnStyleInfo01.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo02.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("AB5A7669-8EF7-4C62-AED1-AC1B82921F56", "Database Number");
			zTextBoxColumnStyleInfo02.ColumnName = "LD_DatabaseNumber";
			zTextBoxColumnStyleInfo02.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo03.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1E8343FF-68A5-4FC3-AA65-B34439373865", "Hosted Location");
			zTextBoxColumnStyleInfo03.ColumnName = "LD_HostedLocation";
			zTextBoxColumnStyleInfo03.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo04.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("D051277A-B097-45D8-A764-C118B8BF46F5", "License Type");
			zTextBoxColumnStyleInfo04.ColumnName = "LD_LicenceType";
			zTextBoxColumnStyleInfo04.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo05.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4D49B155-5108-44A7-ADA6-A76439B4EC33", "Release Ring");
			zTextBoxColumnStyleInfo05.ColumnName = "LD_ReleaseRing";
			zTextBoxColumnStyleInfo05.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo06.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("22C2AB43-937B-4341-9088-47A9A6B400D6", "Server Code");
			zTextBoxColumnStyleInfo06.ColumnName = "LD_ServerCode";
			zTextBoxColumnStyleInfo06.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo07.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7757B040-DB10-483B-B65C-2FD48CD3B7D1", "Model");
			zTextBoxColumnStyleInfo07.ColumnName = "BillingModel";
			zTextBoxColumnStyleInfo07.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo08.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("D7CAA048-26A2-4C41-A90E-512F2803A25B", "Master Org");
			zTextBoxColumnStyleInfo08.ColumnName = "WebAccessOrg+OH_Code";
			zTextBoxColumnStyleInfo08.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo09.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("01CEB3C1-5ADE-498E-A74B-8ACDD7384445", "Enterprise Code");
			zTextBoxColumnStyleInfo09.ColumnName = "EnterpriseCode";
			zTextBoxColumnStyleInfo09.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo10.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EA645FB9-3B2A-4D01-BDF6-C7B79895E49C", "Enterprise ID");
			zTextBoxColumnStyleInfo10.ColumnName = "EnterpriseID";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DatabasesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo01);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo01);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo02);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo03);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo04);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo05);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo06);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo07);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo08);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo09);
			this.DatabasesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DatabasesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabasesGrid.GridId = "819B3F89-8C7E-46A9-866C-8777F810F1F8";
			this.DatabasesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DatabasesGrid.LayoutKey = "DatabasesGrid";
			this.DatabasesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
			this.DatabasesGrid.Name = "DatabasesGrid";
			this.DatabasesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 64, true);
			this.DatabasesGrid.TabIndex = 0;
			this.DatabasesGrid.ReadOnly = true;
			// 
			// EdiIdentityApplicationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("DA2FAE95-F8A7-482F-BEC4-767C3F0DAA1C", "Identity Application");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 608, true);
			this.DataSourceType = typeof(EdiIdentityApplication);
			this.Name = "EdiIdentityApplicationForm";
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
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RedirectUrlGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ApplicationGroupBox.ResumeLayout(false);
			this.ApplicationGroupBox.PerformLayout();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			this.CertificatesGrid.ResumeLayout(false);
			this.CertificatesGrid.PerformLayout();
			this.RedirectUrlGroupBox.ResumeLayout(false);
			this.RedirectUrlGroupBox.PerformLayout();
			this.RedirectUrlGrid.ResumeLayout(false);
			this.RedirectUrlGrid.PerformLayout();
			this.PermissionGroupBox.ResumeLayout(false);
			this.PermissionGroupBox.PerformLayout();
			this.PermissionGrid.ResumeLayout(false);
			this.PermissionGrid.PerformLayout();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			this.LicenceDatabaseTabPage.ResumeLayout(false);
			this.LicenceDatabaseTabPage.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.DatabaseGroupBox.ResumeLayout(false);
			this.DatabaseGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabasesGrid)).EndInit();
			this.DatabasesGrid.ResumeLayout(true);
			this.DatabasesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		ZArchitecture.GUI.ZPanel zPanel1;
		ZArchitecture.GUI.ZGroupBox ApplicationGroupBox;
		protected ZArchitecture.ZTextBox ApplicationNameTextbox;
		protected ZArchitecture.ZTextBox ClientIdTextBox;
		protected ZArchitecture.ZTextBox TenantIdTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit ApplicationTypeDropBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit ProductDropBox;
		protected ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		protected ZArchitecture.GUI.ZCheckBox IsRollbackCheckBox;
		ZArchitecture.GUI.ZGroupBox CertificateGroupBox;
		protected Enterprise.ZArchitecture.ZGrid CertificatesGrid;
		ZArchitecture.GUI.ZGroupBox RedirectUrlGroupBox;
		ZArchitecture.GUI.ZGroupBox PermissionGroupBox;
		protected Enterprise.ZArchitecture.ZGrid RedirectUrlGrid;
		protected Enterprise.ZArchitecture.ZGrid PermissionGrid;
		Enterprise.ZArchitecture.GUI.ZTabPage LicenceDatabaseTabPage;
		ZArchitecture.GUI.ZPanel zPanel2;
		ZArchitecture.GUI.ZGroupBox DatabaseGroupBox;
		protected Enterprise.ZArchitecture.ZGrid DatabasesGrid;
	}
}
