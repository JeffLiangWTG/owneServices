namespace Enterprise.Client.EDI.EndpointManagement.GUI
{
	partial class EdiTrustedSystemForm
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
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TrustedMessagingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CertificateControl = new Enterprise.Client.EDI.EndpointManagement.GUI.EDIDigitalCertificateControl();
			this.HasSecretKeyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OAuthConfigGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AccessTokenExpiryCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IssueRefreshTokenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.SystemIDTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProductBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SystemNumberTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.LicenceDatabaseTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DatabaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DatabasesModuleButtonGrid = new Enterprise.Client.EDI.EndpointManagement.GUI.LicenceDatabaseModuleButtonGrid();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zZGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.TrustedMessagingGroupBox.SuspendLayout();
			this.CertificateControl.SuspendLayout();
			this.OAuthConfigGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ProductBox.SuspendLayout();
			this.LicenceDatabaseTabPage.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.DatabaseGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabasesModuleButtonGrid.InnerGrid)).BeginInit();
			this.DatabasesModuleButtonGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 373, true);
			this.MainTabControl.Controls.Add(this.LicenceDatabaseTabPage);
			this.MainTabControl.Controls.SetChildIndex(this.LicenceDatabaseTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 346, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 346, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 346, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 373, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.TrustedMessagingGroupBox);
			this.zPanel1.Controls.Add(this.OAuthConfigGroupBox);
			this.zPanel1.Controls.Add(this.DetailsGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 346, true);
			this.zPanel1.TabIndex = 3;
			// 
			// TrustedMessagingGroupBox
			// 
			this.TrustedMessagingGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1fac280f-354f-457c-9cfd-b88d6f930854", "Trusted Messaging");
			this.TrustedMessagingGroupBox.Controls.Add(this.CertificateLabel);
			this.TrustedMessagingGroupBox.Controls.Add(this.CertificateControl);
			this.TrustedMessagingGroupBox.Controls.Add(this.HasSecretKeyCheckBox);
			this.TrustedMessagingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 142, true);
			this.TrustedMessagingGroupBox.Name = "TrustedMessagingGroupBox";
			this.TrustedMessagingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 100, true);
			this.TrustedMessagingGroupBox.TabIndex = 0;
			this.TrustedMessagingGroupBox.TabStop = false;
			// 
			// CertificateLabel
			// 
			this.CertificateLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e6b4c057-4d42-4955-b479-21eac6cbd458", "TSC Certificate");
			this.CertificateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CertificateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 43, true);
			this.CertificateLabel.Name = "CertificateLabel";
			this.CertificateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 26, true);
			this.CertificateLabel.TabIndex = 0;
			this.CertificateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CertificateControl
			// 
			this.CertificateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateControl, "CertificateConfig.ETM_CertificateData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).CertificateConfig.ETM_CertificateData)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CertificateControl, false);
			this.CertificateControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateControl.FileDataAsString = "";
			this.CertificateControl.FileDialogTitle = "";
			this.CertificateControl.FileFilter = "X.509 Certificate files (*.cer)|*.cer|All files (*.*)|*.*";
			this.CertificateControl.InitialDirectory = "";
			this.CertificateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 42, true);
			this.CertificateControl.Name = "CertificateControl";
			this.CertificateControl.ReadOnly = false;
			this.CertificateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 26, true);
			this.CertificateControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateControl.TabIndex = 5;
			this.CertificateControl.DataLoaded += new System.EventHandler(this.CertificateControl_DataChanged);
			this.CertificateControl.DataCleared += new System.EventHandler(this.CertificateControl_DataChanged);
			// 
			// HasSecretKeyCheckBox
			// 
			this.HasSecretKeyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasSecretKeyCheckBox, "HasSecretKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).HasSecretKey)));
			this.HasSecretKeyCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7a9130ed-a211-4d81-b3f2-6d80b4f07dae", "Has Secret Key");
			this.HasSecretKeyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasSecretKeyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 19, true);
			this.HasSecretKeyCheckBox.Name = "HasSecretKeyCheckBox";
			this.HasSecretKeyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.HasSecretKeyCheckBox.TabIndex = 4;
			this.HasSecretKeyCheckBox.UseVisualStyleBackColor = true;
			// 
			// OAuthConfigGroupBox
			// 
			this.OAuthConfigGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("98de9e6d-8788-4c91-89c7-8f7f1802e7c4", "OAuth Config");
			this.OAuthConfigGroupBox.Controls.Add(this.AccessTokenExpiryCalcEdit);
			this.OAuthConfigGroupBox.Controls.Add(this.IssueRefreshTokenCheckBox);
			this.OAuthConfigGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 248, true);
			this.OAuthConfigGroupBox.Name = "OAuthConfigGroupBox";
			this.OAuthConfigGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 95, true);
			this.OAuthConfigGroupBox.TabIndex = 0;
			this.OAuthConfigGroupBox.TabStop = false;
			// 
			// AccessTokenExpiryCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AccessTokenExpiryCalcEdit, "ETS_AccessTokenExpiryOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).ETS_AccessTokenExpiryOverride)));
			this.AccessTokenExpiryCalcEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8bbb9466-d702-4486-89fd-93ec911fedd6", "Access Token Expiry");
			this.AccessTokenExpiryCalcEdit.DecimalPlaces = 2;
			this.AccessTokenExpiryCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 20, true);
			this.AccessTokenExpiryCalcEdit.Name = "AccessTokenExpiryCalcEdit";
			this.AccessTokenExpiryCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.AccessTokenExpiryCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.AccessTokenExpiryCalcEdit.TabIndex = 6;
			this.AccessTokenExpiryCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IssueRefreshTokenCheckBox
			// 
			this.IssueRefreshTokenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IssueRefreshTokenCheckBox, "ETS_IssueRefreshToken");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).ETS_IssueRefreshToken)));
			this.IssueRefreshTokenCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f1885695-91eb-429b-bea8-eadfe17aa867", "Issue Refresh Token");
			this.IssueRefreshTokenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IssueRefreshTokenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 46, true);
			this.IssueRefreshTokenCheckBox.Name = "IssueRefreshTokenCheckBox";
			this.IssueRefreshTokenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 17, true);
			this.IssueRefreshTokenCheckBox.TabIndex = 7;
			this.IssueRefreshTokenCheckBox.UseVisualStyleBackColor = true;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d5784d13-e011-4057-a619-b1c86ae4d995", "Details");
			this.DetailsGroupBox.Controls.Add(this.SystemNumberTextbox);
			this.DetailsGroupBox.Controls.Add(this.DescriptionTextbox);
			this.DetailsGroupBox.Controls.Add(this.SystemIDTextbox);
			this.DetailsGroupBox.Controls.Add(this.ProductBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 133, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// DescriptionTextbox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextbox, "ETS_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).ETS_Description)));
			this.DescriptionTextbox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("da1be089-5c44-4c92-8eef-02e9fc641384", "Description");
			this.DescriptionTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 97, true);
			this.DescriptionTextbox.Name = "DescriptionTextbox";
			this.DescriptionTextbox.ShouldEscapeAllSpecialCharacters = false;
			this.DescriptionTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.DescriptionTextbox.TabIndex = 3;
			// 
			// SystemIDTextbox
			// 
			this.BindingSource.SetBindingMember(this.SystemIDTextbox, "ETS_SystemID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).ETS_SystemID)));
			this.SystemIDTextbox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7b7d5f72-11d1-4de0-afae-101d2569a060", "System ID");
			this.SystemIDTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 71, true);
			this.SystemIDTextbox.Name = "SystemIDTextbox";
			this.SystemIDTextbox.ShouldEscapeAllSpecialCharacters = false;
			this.SystemIDTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.SystemIDTextbox.TabIndex = 2;
			// 
			// ProductBox
			// 
			this.ProductBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductBox, "ETS_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).ETS_Product)));
			this.ProductBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("92b158c9-d87e-496e-b577-9c2682d67803", "Product");
			this.ProductBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 45, true);
			this.ProductBox.Name = "ProductBox";
			this.ProductBox.PreBoundMaxLength = 2;
			this.ProductBox.ShouldResizeByMaxLength = true;
			this.ProductBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.ProductBox.TabIndex = 1;
			// 
			// SystemNumberTextbox
			// 
			this.BindingSource.SetBindingMember(this.SystemNumberTextbox, "ETS_SystemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).ETS_SystemNumber)));
			this.SystemNumberTextbox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f30fbdc4-440b-421d-a56c-89b3d932a263", "System #");
			this.SystemNumberTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 19, true);
			this.SystemNumberTextbox.Name = "SystemNumberTextbox";
			this.SystemNumberTextbox.ReadOnly = true;
			this.SystemNumberTextbox.ShouldEscapeAllSpecialCharacters = false;
			this.SystemNumberTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.SystemNumberTextbox.TabIndex = 0;
			// 
			// LicenceDatabaseTabPage
			// 
			this.LicenceDatabaseTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("E895FB4F-0FA4-4130-8E65-4EC2A58BF2A6", "Licence Databases");
			this.LicenceDatabaseTabPage.Controls.Add(this.zPanel2);
			this.LicenceDatabaseTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LicenceDatabaseTabPage.Name = "LicenceDatabaseTabPage";
			this.LicenceDatabaseTabPage.ShouldBeReadOnlyInViewMode = false;
			this.LicenceDatabaseTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 373, true);
			this.LicenceDatabaseTabPage.TabIndex = 3;
			this.LicenceDatabaseTabPage.UseVisualStyleBackColor = true;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.DatabaseGroupBox);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 346, true);
			this.zPanel2.TabIndex = 3;
			// 
			// DatabaseGroupBox
			// 
			this.DatabaseGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("C5559C63-7076-434F-97D6-240443A64ACD", "Database");
			this.DatabaseGroupBox.Controls.Add(this.DatabasesModuleButtonGrid);
			this.DatabaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.DatabaseGroupBox.Name = "DatabaseGroupBox";
			this.DatabaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 133, true);
			this.DatabaseGroupBox.TabIndex = 0;
			this.DatabaseGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabaseGroupBox.TabStop = false;
			// 
			// DatabasesModuleButtonGrid
			// 
			this.DatabasesModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DatabasesModuleButtonGrid, "AllLinkedLicenceDatabases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).AllLinkedLicenceDatabases)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem)(null)).AllDatabasesNotLinked)));
			this.DatabasesModuleButtonGrid.BindToFindBoxList = "AllDatabasesNotLinked";
			zCheckBoxColumnStyleInfo1.Caption = "Active";
			zCheckBoxColumnStyleInfo1.ColumnName = "LD_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo1.Caption = "Product";
			zTextBoxColumnStyleInfo1.ColumnName = "LD_Product";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.Caption = "Database Number";
			zTextBoxColumnStyleInfo2.ColumnName = "LD_DatabaseNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.Caption = "Hosted Location";
			zTextBoxColumnStyleInfo3.ColumnName = "LD_HostedLocation";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = "License Type";
			zTextBoxColumnStyleInfo4.ColumnName = "LD_LicenceType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo5.Caption = "Release Ring";
			zTextBoxColumnStyleInfo5.ColumnName = "LD_ReleaseRing";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "Server Code";
			zTextBoxColumnStyleInfo6.ColumnName = "LD_ServerCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo7.Caption = "Tenant ID";
			zTextBoxColumnStyleInfo7.ColumnName = "LD_TenantID";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.Caption = "Model";
			zTextBoxColumnStyleInfo8.ColumnName = "BillingModel";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo9.Caption = "Master Org";
			zTextBoxColumnStyleInfo9.ColumnName = "WebAccessOrg+OH_Code";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zZGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+CurrentVersions";
			zZGuidFindBoxColumnStyleInfo1.Caption = "Current Version";
			zZGuidFindBoxColumnStyleInfo1.ColumnName = "LD_HL_CurrentRunningVersion";
			zZGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.Caption = "Enterprise Code";
			zTextBoxColumnStyleInfo11.ColumnName = "EnterpriseCode";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.Caption = "Enterprise ID";
			zTextBoxColumnStyleInfo12.ColumnName = "EnterpriseID";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zZGuidFindBoxColumnStyleInfo1);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DatabasesModuleButtonGrid.DetachMessage = CargoWiseOne.ResourceStrings.Res.GetData("6B523A8C-EA1D-443B-8046-A13B49E7A872", "Are you sure you want to detach this Database from the Trusted System?");
			this.DatabasesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabasesModuleButtonGrid.GridId = "594D9EA3-1EE7-4A77-A5C9-C3D6BF6A5C3F";
			this.DatabasesModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DatabasesModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DatabasesModuleButtonGrid.InnerGrid.GridId = null;
			this.DatabasesModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DatabasesModuleButtonGrid.InnerGrid.LayoutKey = "DatabasesModuleButtonGridInnerGrid";
			this.DatabasesModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DatabasesModuleButtonGrid.InnerGrid.Name = "DatabasesModuleButtonGridInnerGrid";
			this.DatabasesModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.DatabasesModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 74, true);
			this.DatabasesModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DatabasesModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DatabasesModuleButtonGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 0, true);
			this.DatabasesModuleButtonGrid.Name = "DatabasesModuleButtonGrid";
			this.DatabasesModuleButtonGrid.NameOfAGridElement = CargoWiseOne.ResourceStrings.Res.GetData("695164A3-B2E8-4767-83CA-E3C262253BA2", "Database");
			this.DatabasesModuleButtonGrid.ReadOnly = true;
			this.DatabasesModuleButtonGrid.ShowEditButton = false;
			this.DatabasesModuleButtonGrid.ShowNewButton = false;
			this.DatabasesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 112, true);
			this.DatabasesModuleButtonGrid.TabIndex = 0;
			// 
			// EdiTrustedSystemForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("547fd026-0755-498a-bce4-5260cdd5cc40", "Trusted System");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 429, true);
			this.DataSourceType = typeof(Enterprise.Client.EDI.TrustedMessaging.Business.EdiTrustedSystem);
			this.Name = "EdiTrustedSystemForm";
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
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.TrustedMessagingGroupBox.ResumeLayout(false);
			this.TrustedMessagingGroupBox.PerformLayout();
			this.CertificateControl.ResumeLayout(true);
			this.CertificateControl.PerformLayout();
			this.OAuthConfigGroupBox.ResumeLayout(false);
			this.OAuthConfigGroupBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ProductBox.ResumeLayout(true);
			this.ProductBox.PerformLayout();
			this.LicenceDatabaseTabPage.ResumeLayout(false);
			this.LicenceDatabaseTabPage.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.DatabaseGroupBox.ResumeLayout(false);
			this.DatabaseGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabasesModuleButtonGrid.InnerGrid)).EndInit();
			this.DatabasesModuleButtonGrid.ResumeLayout(true);
			this.DatabasesModuleButtonGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		ZArchitecture.GUI.ZPanel zPanel1;
		ZArchitecture.GUI.ZDropEdit ProductBox;
		ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		ZArchitecture.ZTextBox DescriptionTextbox;
		ZArchitecture.ZTextBox SystemIDTextbox;
		ZArchitecture.GUI.ZGroupBox OAuthConfigGroupBox;
		ZArchitecture.GUI.ZCheckBox IssueRefreshTokenCheckBox;
		ZArchitecture.GUI.ZGroupBox TrustedMessagingGroupBox;
		ZArchitecture.GUI.ZCheckBox HasSecretKeyCheckBox;
		ZArchitecture.ZCalcEdit AccessTokenExpiryCalcEdit;
		ZArchitecture.ZLabel CertificateLabel;
		EDIDigitalCertificateControl CertificateControl;
		ZArchitecture.ZTextBox SystemNumberTextbox;
		Enterprise.ZArchitecture.GUI.ZTabPage LicenceDatabaseTabPage;
		ZArchitecture.GUI.ZPanel zPanel2;
		ZArchitecture.GUI.ZGroupBox DatabaseGroupBox;
		Enterprise.Client.EDI.EndpointManagement.GUI.LicenceDatabaseModuleButtonGrid DatabasesModuleButtonGrid;
	}
}
