using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IdentityApplication.GUI
{
	partial class EdiIdentityCustomerApplicationForm
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zPermissionDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zScopeTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ApplicationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApplicationNameTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PermissionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermissionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ParentOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.parentApplicationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PermissionGrid)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.CertificateGroupBox.SuspendLayout();
			this.ApplicationGroupBox.SuspendLayout();
			this.PermissionGroupBox.SuspendLayout();
			this.CertificatesGrid.SuspendLayout();
			this.PermissionGrid.SuspendLayout();
			this.ParentOrgGuidFindBox.SuspendLayout();
			this.parentApplicationGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 552, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 525, true);
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
			this.zPanel1.Controls.Add(this.PermissionGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1155, 525, true);
			this.zPanel1.TabIndex = 0;
			// 
			// ApplicationGroupBox
			//
			this.ApplicationGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("D7B08D0E-9E9F-4BE1-BE2D-749FAA227942", "Application");
			this.ApplicationGroupBox.Controls.Add(this.ApplicationNameTextbox);
			this.ApplicationGroupBox.Controls.Add(this.ClientIdTextBox);
			this.ApplicationGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.ApplicationGroupBox.Controls.Add(this.ParentOrgGuidFindBox);
			this.ApplicationGroupBox.Controls.Add(this.parentApplicationGuidFindBox);
			this.ApplicationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.ApplicationGroupBox.Name = "ApplicationGroupBox";
			this.ApplicationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 78, true);
			this.ApplicationGroupBox.TabIndex = 0;
			this.ApplicationGroupBox.TabStop = false;
			this.ApplicationGroupBox.Text = "Application";
			// 
			// ApplicationNameTextbox
			// 
			this.BindingSource.SetBindingMember(this.ApplicationNameTextbox, "IDA_ApplicationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityApplication)(null)).IDA_ApplicationName)));
			this.ApplicationNameTextbox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("34E218A7-3716-495E-9CBD-4E0B99A17239", "Application Name");
			this.ApplicationNameTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ApplicationNameTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 19, true);
			this.ApplicationNameTextbox.Name = "ApplicationNameTextbox";
			this.ApplicationNameTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.ApplicationNameTextbox.TabIndex = 0;
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
			// ParentOrgGuidFindBox
			// 
			this.ParentOrgGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentOrgGuidFindBox, "IDA_OH_ParentOrg");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((EdiIdentityApplication)(null)).IDA_OH_ParentOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((EdiIdentityApplication)(null)).Lookups.ParentOrgs)));
			this.ParentOrgGuidFindBox.BindToList = "Lookups+ParentOrgs";
			this.ParentOrgGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ParentOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 20, true);
			this.ParentOrgGuidFindBox.Name = "ParentOrgGuidFindBox";
			this.ParentOrgGuidFindBox.ShouldResize = true;
			this.ParentOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ParentOrgGuidFindBox.TabIndex = 3;
			// 
			// parentApplicationGuidFindBox
			// 
			this.parentApplicationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.parentApplicationGuidFindBox, "IDA_IDA_ParentApplication");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((EdiIdentityApplication)(null)).IDA_IDA_ParentApplication)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((EdiIdentityApplication)(null)).Lookups.ParentApplications)));
			this.parentApplicationGuidFindBox.BindToList = "Lookups+ParentApplications";
			this.parentApplicationGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.parentApplicationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 49, true);
			this.parentApplicationGuidFindBox.Name = "parentApplicationGuidFindBox";
			this.parentApplicationGuidFindBox.ShouldResize = true;
			this.parentApplicationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.parentApplicationGuidFindBox.TabIndex = 4;
			// 
			// IsActiveCheckBox
			//
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "IDA_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((EdiIdentityApplication)(null)).IDA_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2655D0D2-4CE0-48E3-AE55-5F14A74E4C24", "Is Active");
			this.IsActiveCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(800, 20, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.IsActiveCheckBox.TabIndex = 5;
			this.IsActiveCheckBox.Enabled = false;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// CertificateGroupBox
			//
			this.CertificateGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5F5FA3CD-8F84-4581-83C7-BE6C9F620275", "Certificate");
			this.CertificateGroupBox.Controls.Add(this.CertificatesGrid);
			this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 85, true);
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
			this.CertificatesGrid.GridId = "C7ED07DC-13B4-47DC-A8FE-3B908AF18AAC";
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
			this.PermissionGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5D2042EE-BF76-4712-9E52-A7C67D16EB3D", "Permissions");
			this.PermissionGroupBox.Controls.Add(this.PermissionGrid);
			this.PermissionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 210, true);
			this.PermissionGroupBox.Name = "PermissionGroupBox";
			this.PermissionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1140, 120, true);
			this.PermissionGroupBox.TabIndex = 0;
			this.PermissionGroupBox.TabStop = false;
			this.PermissionGroupBox.Text = "Permissions";
			// 
			// RedirectUrlGrid
			// 
			this.PermissionGrid.AllowNavigation = false;
			this.PermissionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PermissionGrid, "Permissions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Permissions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityApplicationPermission.Business.EdiIdentityApplicationPermission)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Permissions)).SyncRoot)).IAP_Scope)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IdentityApplicationPermission.Business.EdiIdentityApplicationPermission)(((System.Collections.IList)(((Enterprise.Client.EDI.IdentityApplication.Business.EdiIdentityApplication)(null)).Permissions)).SyncRoot)).Permission)));
			this.PermissionGrid.CaptionVisible = false;
			zScopeTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8B0E6C30-70BA-44DA-82BD-E1B51E72211B", "Scope");
			zScopeTextBoxColumnStyleInfo1.ColumnName = "IAP_Scope";
			zScopeTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zPermissionDropEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9EB9CB5B-D82D-4F45-8E3E-B2CA0A31D0D7", "Permission");
			zPermissionDropEditColumnStyleInfo1.ColumnName = "Permission";
			zPermissionDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(139);
			this.PermissionGrid.ColumnStyles.Add(zPermissionDropEditColumnStyleInfo1);
			this.PermissionGrid.ColumnStyles.Add(zScopeTextBoxColumnStyleInfo1);
			this.PermissionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermissionGrid.GridId = "186ABF97-4C87-4E18-BFD6-73E138D10AD9";
			this.PermissionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermissionGrid.LayoutKey = "PermissionGrid";
			this.PermissionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
			this.PermissionGrid.Name = "PermissionGrid";
			this.PermissionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 64, true);
			this.PermissionGrid.TabIndex = 0;
			// 
			// EdiIdentityCustomerApplicationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("6153B278-A24E-48F5-96E8-3ED289F34540", "Identity Customer Application");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 608, true);
			this.DataSourceType = typeof(EdiIdentityApplication);
			this.Name = "EdiIdentityCustomerApplicationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CertificatesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PermissionGrid)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ApplicationGroupBox.ResumeLayout(false);
			this.ApplicationGroupBox.PerformLayout();
			this.CertificateGroupBox.ResumeLayout(false);
			this.CertificateGroupBox.PerformLayout();
			this.PermissionGroupBox.ResumeLayout(false);
			this.PermissionGroupBox.PerformLayout();
			this.CertificatesGrid.ResumeLayout(false);
			this.CertificatesGrid.PerformLayout();
			this.PermissionGrid.ResumeLayout(false);
			this.PermissionGrid.PerformLayout();
			this.ParentOrgGuidFindBox.ResumeLayout(false);
			this.ParentOrgGuidFindBox.PerformLayout();
			this.parentApplicationGuidFindBox.ResumeLayout(false);
			this.parentApplicationGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		ZArchitecture.GUI.ZPanel zPanel1;
		ZArchitecture.GUI.ZGroupBox ApplicationGroupBox;
		protected ZArchitecture.ZTextBox ApplicationNameTextbox;
		protected ZArchitecture.ZTextBox ClientIdTextBox;
		protected ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		ZArchitecture.GUI.ZGroupBox CertificateGroupBox;
		protected Enterprise.ZArchitecture.ZGrid CertificatesGrid;
		ZArchitecture.GUI.ZGroupBox PermissionGroupBox;
		protected Enterprise.ZArchitecture.ZGrid PermissionGrid;
		ZGuidFindBox ParentOrgGuidFindBox;
		ZGuidFindBox parentApplicationGuidFindBox;
	}
}
