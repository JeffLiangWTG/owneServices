using Enterprise.ZArchitecture.GUI;


namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class LicenceEnterpriseForm
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

		ZGroupBox EnterpriseLicenceGroupBox;
		Enterprise.ZArchitecture.ZGrid DbDetailsGrid;
		Enterprise.ZArchitecture.ZLabel DbDetailsLabel;
		Enterprise.ZArchitecture.ZLabel EnterpriseCodeLabel;
		Enterprise.ZArchitecture.ZLabel ParentOrgLabel;
		Enterprise.ZArchitecture.ZTextBox EnterpriseCodeTextBox;
		ZGuidFindBox ParentOrgGuidFindBox;

		
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.EnterpriseCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ParentOrgLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ParentOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.EnterpriseLicenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsInternalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DbDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DbDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentOrgGuidFindBox.SuspendLayout();
			this.EnterpriseLicenceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DbDetailsGrid)).BeginInit();
			this.DbDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 519, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.EnterpriseLicenceGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 497, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 497, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 497, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 519, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(411);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(412);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise);
			// 
			// EnterpriseCodeLabel
			// 
			this.EnterpriseCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EnterpriseCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 46, true);
			this.EnterpriseCodeLabel.Name = "EnterpriseCodeLabel";
			this.EnterpriseCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.EnterpriseCodeLabel.TabIndex = 1;
			this.EnterpriseCodeLabel.Text = "Enterprise Code:";
			// 
			// ParentOrgLabel
			// 
			this.ParentOrgLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ParentOrgLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 69, true);
			this.ParentOrgLabel.Name = "ParentOrgLabel";
			this.ParentOrgLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 21, true);
			this.ParentOrgLabel.TabIndex = 3;
			this.ParentOrgLabel.Text = "Parent Organisation:";
			// 
			// EnterpriseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EnterpriseCodeTextBox, "LE_EnterpriseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).LE_EnterpriseCode)));
			this.EnterpriseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 47, true);
			this.EnterpriseCodeTextBox.Name = "EnterpriseCodeTextBox";
			this.EnterpriseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.EnterpriseCodeTextBox.TabIndex = 1;
			// 
			// ParentOrgGuidFindBox
			// 
			this.ParentOrgGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentOrgGuidFindBox, "LE_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).LE_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Lookups.Headers)));
			this.ParentOrgGuidFindBox.BindToList = "Lookups+Headers";
			this.ParentOrgGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ParentOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 71, true);
			this.ParentOrgGuidFindBox.Name = "ParentOrgGuidFindBox";
			this.ParentOrgGuidFindBox.ShouldResize = true;
			this.ParentOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 17, true);
			this.ParentOrgGuidFindBox.TabIndex = 3;
			// 
			// EnterpriseLicenceGroupBox
			// 
			this.EnterpriseLicenceGroupBox.Controls.Add(this.IsInternalCheckBox);
			this.EnterpriseLicenceGroupBox.Controls.Add(this.DbDetailsGrid);
			this.EnterpriseLicenceGroupBox.Controls.Add(this.DbDetailsLabel);
			this.EnterpriseLicenceGroupBox.Controls.Add(this.zTextBox1);
			this.EnterpriseLicenceGroupBox.Controls.Add(this.zLabel1);
			this.EnterpriseLicenceGroupBox.Controls.Add(this.EnterpriseCodeTextBox);
			this.EnterpriseLicenceGroupBox.Controls.Add(this.EnterpriseCodeLabel);
			this.EnterpriseLicenceGroupBox.Controls.Add(this.ParentOrgGuidFindBox);
			this.EnterpriseLicenceGroupBox.Controls.Add(this.ParentOrgLabel);
			this.EnterpriseLicenceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnterpriseLicenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EnterpriseLicenceGroupBox.Name = "EnterpriseLicenceGroupBox";
			this.EnterpriseLicenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 497, true);
			this.EnterpriseLicenceGroupBox.TabIndex = 1;
			this.EnterpriseLicenceGroupBox.TabStop = false;
			this.EnterpriseLicenceGroupBox.Text = "Enterprise Licence Details";
			// 
			// IsInternalCheckBox
			// 
			this.IsInternalCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.IsInternalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsInternalCheckBox, "LE_IsInternal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).LE_IsInternal)));
			this.IsInternalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsInternalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 49, true);
			this.IsInternalCheckBox.Name = "IsInternalCheckBox";
			this.IsInternalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 16, true);
			this.IsInternalCheckBox.TabIndex = 2;
			this.IsInternalCheckBox.Text = "WiseTech Internal (not billed)";
			this.IsInternalCheckBox.UseVisualStyleBackColor = true;
			// 
			// DbDetailsGrid
			// 
			this.DbDetailsGrid.AllowNavigation = false;
			this.DbDetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DbDetailsGrid, "Databases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_ServerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_LicenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_ReleaseRing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_DBServerSecurityMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).CurrentVersion.HL_ExeVersionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_PublicEmailAddressForUpdate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_InternalPop3EmailAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_InternalPop3UserName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_InternalPop3Port)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_InternalSmtpEmailAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_InternalSmtpPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_OC_ContractInstallerOrInternalTechContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).Lookups.ContractInstallerOrInternalTechContacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_OA_SoftwareInstallAddressDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).Lookups.SoftwareInstallAddressDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).LD_OC_LicenseeAdminContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).Databases)).SyncRoot)).Lookups.LicenseeAdminContacts)));
			this.DbDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Server Code";
			zTextBoxColumnStyleInfo1.ColumnName = "LD_ServerCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Licence Type";
			zTextBoxColumnStyleInfo2.ColumnName = "LD_LicenceType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Release Type";
			zTextBoxColumnStyleInfo3.ColumnName = "LD_ReleaseRing";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo10.Caption = "Product";
			zTextBoxColumnStyleInfo10.ColumnName = "LD_Product";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = "Server Security";
			zTextBoxColumnStyleInfo4.ColumnName = "LD_DBServerSecurityMode";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "EXE Date";
			zDateEditColumnStyleInfo1.ColumnName = "CurrentVersion+HL_ExeVersionDate";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo4.Caption = "Current Version";
			zGuidFindBoxColumnStyleInfo4.ColumnName = "LD_HL_CurrentRunningVersion";
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.Caption = "Email Address For Updates ";
			zTextBoxColumnStyleInfo6.ColumnName = "LD_PublicEmailAddressForUpdate";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo7.Caption = "Public Email Address";
			zTextBoxColumnStyleInfo7.ColumnName = "LD_InternalPop3EmailAddress";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo8.Caption = "POP3 User Name";
			zTextBoxColumnStyleInfo8.ColumnName = "LD_InternalPop3UserName";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "POP3 Port";
			zCalcEditColumnStyleInfo1.ColumnName = "LD_InternalPop3Port";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo9.Caption = "SMTP Server";
			zTextBoxColumnStyleInfo9.ColumnName = "LD_InternalSmtpEmailAddress";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "SMTP Port";
			zCalcEditColumnStyleInfo2.ColumnName = "LD_InternalSmtpPort";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+ContractInstallerOrInternalTechContacts";
			zGuidFindBoxColumnStyleInfo1.Caption = "Tech Contact";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "LD_OC_ContractInstallerOrInternalTechContact";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo2.BindToList = "Lookups+SoftwareInstallAddressDetails";
			zGuidFindBoxColumnStyleInfo2.Caption = "Install Address";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "LD_OA_SoftwareInstallAddressDetails";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgAddresses;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo3.BindToList = "Lookups+LicenseeAdminContacts";
			zGuidFindBoxColumnStyleInfo3.Caption = "Admin Contact";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "LD_OC_LicenseeAdminContact";
			zGuidFindBoxColumnStyleInfo3.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo11.Caption = "Master Org";
			zTextBoxColumnStyleInfo11.ColumnName = "WebAccessOrg+OH_Code";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.Caption = "Token Authentication";
			zCheckBoxColumnStyleInfo1.ColumnName = "LD_TokenAuthenticationEnabled";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DbDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DbDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DbDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DbDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DbDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.DbDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.DbDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.DbDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.DbDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DbDetailsGrid.GridId = "1eb5d32d-d9d8-45fe-b7d6-81c386ccc561";
			this.DbDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DbDetailsGrid.LayoutKey = "zGrid1";
			this.DbDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 111, true);
			this.DbDetailsGrid.Name = "DbDetailsGrid";
			this.DbDetailsGrid.ReadOnly = true;
			this.DbDetailsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DbDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 372, true);
			this.DbDetailsGrid.TabIndex = 4;
			this.DbDetailsGrid.DoubleClick += DbDetailsGrid_DoubleClick;
			// 
			// DbDetailsLabel
			// 
			this.DbDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DbDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 96, true);
			this.DbDetailsLabel.Name = "DbDetailsLabel";
			this.DbDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 13, true);
			this.DbDetailsLabel.TabIndex = 7;
			this.DbDetailsLabel.Text = "Database Details:";
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "LE_EnterpriseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise)(null)).LE_EnterpriseID)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 23, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 22, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "Enterprise ID:";
			// 
			// LicenceEnterpriseForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 575, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise);
			this.DataSourceTypeName = "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceEnterprise";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			this.Name = "LicenceEnterpriseForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Enterprise Licence";
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
			this.ParentOrgGuidFindBox.ResumeLayout(true);
			this.ParentOrgGuidFindBox.PerformLayout();
			this.EnterpriseLicenceGroupBox.ResumeLayout(false);
			this.EnterpriseLicenceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DbDetailsGrid)).EndInit();
			this.DbDetailsGrid.ResumeLayout(false);
			this.DbDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZCheckBox IsInternalCheckBox;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.ZLabel zLabel1;
	}
}
