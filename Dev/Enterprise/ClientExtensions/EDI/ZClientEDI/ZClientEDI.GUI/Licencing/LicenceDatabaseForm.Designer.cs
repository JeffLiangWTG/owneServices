using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class LicenceDatabaseForm
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

		protected new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.FlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.ProductionDatabaseDropEdit = new Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit();
			this.StaffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TenantIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ServerCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LicenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServerSecurityModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PublicEmailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InternalSMTPEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InternalPOP3EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InstallationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeatureSetDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.manualExpiryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.billingModelBox = new Enterprise.ZArchitecture.ZTextBox();
			this.billableDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.canReregisterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LicenseeContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TechContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.productBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DatabaseIdBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DatabaseNumberBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HostedLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.activeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReleaseRingsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.allowAutoLoginCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.enablePackageDownloadOptimizationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.preRegistrationExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.eAdaptorUrlBox = new Enterprise.ZArchitecture.ZTextBox();
			this.registrationStatusBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.HostDBInstanceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HostServerNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox7 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LastHeartbeatDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SoftwareInstallAddressDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.UpgradeMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CurrentSentVersionGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CurrentRunningVersionGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MinutesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MegabytesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MaxEmailSizeLimitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MessageRetryTimeoutCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RequestVersionReportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ResetHeartbeatButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.requestLicenceUsageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.tableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.tabControl1 = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ConfigDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.currentVersionLastTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.currentVersionFirstTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PurchasedLicenceUnitsBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.tabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.DocEngineStatsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NofActivePrintQueuesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.sqlServerVersionDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.sqlServerEditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.sqlServerFullVersionTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.sqlServerVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.ProcessorSpeedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcessorTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsVirtualMachineCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NoOfProcessorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalMemoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BIOSDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SystemManufacturerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OSVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OSNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UserManagementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StaffReportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.WebAccessOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TrustedMessagingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TrustedMessagingControl = new Enterprise.Client.EDI.EndpointManagement.GUI.EDITrustedMessagingControl();
			this.ClientCompaniesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ClientCompaniesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CompaniesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.UpgradeScheduleTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.nextRunTimeUtcMUG = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.nextRunTimeUtcUPG = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.scheduleStateMUG = new Enterprise.ZArchitecture.ZTextBox();
			this.scheduleStateUPG = new Enterprise.ZArchitecture.ZTextBox();
			this.FeatureControlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FeatureControlRuleLastSyncUtcDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FeatureControlRuleLastSyncContentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BottomPanel = new CargoWise.Windows.UI.KPanel();
			this.requestStaffReportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.requestLicenceDatabaseLogsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.tableLayoutPanel2 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.MainDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FlowLayoutPanel.SuspendLayout();
			this.ProductionDatabaseDropEdit.SuspendLayout();
			this.StaffCodeFindBox.SuspendLayout();
			this.LicenceTypeDropEdit.SuspendLayout();
			this.ServerSecurityModeDropEdit.SuspendLayout();
			this.InstallationDetailsGroupBox.SuspendLayout();
			this.FeatureSetDropEdit.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			this.billableDropEdit.SuspendLayout();
			this.LicenseeContactGuidFindBox.SuspendLayout();
			this.TechContactGuidFindBox.SuspendLayout();
			this.productBox.SuspendLayout();
			this.HostedLocationDropEdit.SuspendLayout();
			this.ReleaseRingsDropEdit.SuspendLayout();
			this.preRegistrationExpiryDateEdit.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.registrationStatusBox.SuspendLayout();
			this.ContactDetailsGroupBox.SuspendLayout();
			this.LastHeartbeatDateEdit.SuspendLayout();
			this.SoftwareInstallAddressDropEdit.SuspendLayout();
			this.UpgradeMethodDropEdit.SuspendLayout();
			this.CurrentSentVersionGuidFindBox.SuspendLayout();
			this.CurrentRunningVersionGuidFindBox.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.ConfigDetailsTabPage.SuspendLayout();
			this.currentVersionLastTime.SuspendLayout();
			this.currentVersionFirstTime.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.DocEngineStatsTabPage.SuspendLayout();
			this.sqlServerVersionDetailsTabPage.SuspendLayout();
			this.sqlServerEditionDropEdit.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.BIOSDateDateEdit.SuspendLayout();
			this.UserManagementTabPage.SuspendLayout();
			this.StaffReportDateEdit.SuspendLayout();
			this.WebAccessOrgGuidFindBox.SuspendLayout();
			this.TrustedMessagingTabPage.SuspendLayout();
			this.TrustedMessagingControl.SuspendLayout();
			this.ClientCompaniesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientCompaniesGrid)).BeginInit();
			this.ClientCompaniesGrid.SuspendLayout();
			this.CompaniesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.UpgradeScheduleTabPage.SuspendLayout();
			this.nextRunTimeUtcMUG.SuspendLayout();
			this.nextRunTimeUtcUPG.SuspendLayout();
			this.FeatureControlTabPage.SuspendLayout();
			this.FeatureControlRuleLastSyncUtcDateEdit.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.MainDetailsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 728, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.MainDetailsPanel);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 705, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 685, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 705, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 728, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 628, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(989);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase);
			// 
			// FlowLayoutPanel
			// 
			this.FlowLayoutPanel.Controls.Add(this.ProductionDatabaseDropEdit);
			this.FlowLayoutPanel.Controls.Add(this.StaffCodeFindBox);
			this.FlowLayoutPanel.Controls.Add(this.TenantIDTextBox);
			this.FlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.FlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 334, true);
			this.FlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.FlowLayoutPanel.Name = "FlowLayoutPanel";
			this.FlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 68, true);
			this.FlowLayoutPanel.TabIndex = 18;
			// 
			// ProductionDatabaseDropEdit
			// 
			this.ProductionDatabaseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductionDatabaseDropEdit, "ProductionDatabaseServerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).ProductionDatabaseServerCode)));
			this.ProductionDatabaseDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b8a2f823-890a-4f93-bc18-c431c7cd5671", "Production DB");
			this.ProductionDatabaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 0, true);
			this.ProductionDatabaseDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(99, 0, 0, 4, true);
			this.ProductionDatabaseDropEdit.MaxItemsToShowInDropDown = 30;
			this.ProductionDatabaseDropEdit.Name = "ProductionDatabaseDropEdit";
			this.ProductionDatabaseDropEdit.PreBoundMaxLength = 3;
			this.ProductionDatabaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.ProductionDatabaseDropEdit.TabIndex = 1;
			// 
			// StaffCodeFindBox
			// 
			this.StaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StaffCodeFindBox, "LD_GS_NKOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_GS_NKOwner)));
			this.StaffCodeFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("288e4f62-6ad8-4240-a06d-add643467632", "Staff Owner", "Staff who owns this internal system");
			this.StaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 21, true);
			this.StaffCodeFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(99, 0, 0, 4, true);
			this.StaffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.StaffCodeFindBox.Name = "StaffCodeFindBox";
			this.StaffCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.StaffCodeFindBox.ParentType = null;
			this.StaffCodeFindBox.PreBoundMaxLength = 3;
			this.StaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.StaffCodeFindBox.TabIndex = 2;
			// 
			// TenantIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.TenantIDTextBox, "LD_TenantID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_TenantID)));
			this.TenantIDTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("bea0b6d5-b34a-4f24-942f-d126bf41d335", "Tenant ID", "Tenant ID");
			this.TenantIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 43, true);
			this.TenantIDTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(99, 0, 0, 0, true);
			this.TenantIDTextBox.Name = "TenantIDTextBox";
			this.TenantIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.TenantIDTextBox.TabIndex = 3;
			// 
			// ServerCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ServerCodeTextBox, "LD_ServerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ServerCode)));
			this.ServerCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 41, true);
			this.ServerCodeTextBox.Name = "ServerCodeTextBox";
			this.ServerCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.ServerCodeTextBox.TabIndex = 3;
			// 
			// LicenceTypeDropEdit
			// 
			this.LicenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicenceTypeDropEdit, "LD_LicenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_LicenceType)));
			this.LicenceTypeDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|d5457c8c-e90d-4890-a268-3077ee4fb5a1", "System Type");
			this.LicenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 115, true);
			this.LicenceTypeDropEdit.Name = "LicenceTypeDropEdit";
			this.LicenceTypeDropEdit.PreBoundMaxLength = 3;
			this.LicenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.LicenceTypeDropEdit.TabIndex = 6;
			// 
			// ServerSecurityModeDropEdit
			// 
			this.ServerSecurityModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServerSecurityModeDropEdit, "LD_DBServerSecurityMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_DBServerSecurityMode)));
			this.ServerSecurityModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 239, true);
			this.ServerSecurityModeDropEdit.Name = "ServerSecurityModeDropEdit";
			this.ServerSecurityModeDropEdit.PreBoundMaxLength = 3;
			this.ServerSecurityModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.ServerSecurityModeDropEdit.TabIndex = 11;
			// 
			// PublicEmailAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.PublicEmailAddressTextBox, "LD_PublicEmailAddressForUpdate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_PublicEmailAddressForUpdate)));
			this.PublicEmailAddressTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|7cf986a2-3047-4c33-830c-627d103e5937", "Public Email Address");
			this.PublicEmailAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PublicEmailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 66, true);
			this.PublicEmailAddressTextBox.Name = "PublicEmailAddressTextBox";
			this.PublicEmailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.PublicEmailAddressTextBox.TabIndex = 2;
			// 
			// InternalSMTPEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.InternalSMTPEmailTextBox, "LD_InternalSmtpEmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_InternalSmtpEmailAddress)));
			this.InternalSMTPEmailTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|5788a891-d192-4348-a3f8-2d4b54d98c56", "SMTP Server");
			this.InternalSMTPEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InternalSMTPEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 139, true);
			this.InternalSMTPEmailTextBox.Name = "InternalSMTPEmailTextBox";
			this.InternalSMTPEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.InternalSMTPEmailTextBox.TabIndex = 6;
			// 
			// InternalPOP3EmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.InternalPOP3EmailTextBox, "LD_InternalPop3EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_InternalPop3EmailAddress)));
			this.InternalPOP3EmailTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|5d1c8822-0cca-41b8-86bb-27f3fcdc14a3", "Mail Server");
			this.InternalPOP3EmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InternalPOP3EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 91, true);
			this.InternalPOP3EmailTextBox.Name = "InternalPOP3EmailTextBox";
			this.InternalPOP3EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.InternalPOP3EmailTextBox.TabIndex = 3;
			// 
			// InstallationDetailsGroupBox
			// 
			this.InstallationDetailsGroupBox.Controls.Add(this.FeatureSetDropEdit);
			this.InstallationDetailsGroupBox.Controls.Add(this.manualExpiryButton);
			this.InstallationDetailsGroupBox.Controls.Add(this.zDateEdit2);
			this.InstallationDetailsGroupBox.Controls.Add(this.billingModelBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.billableDropEdit);
			this.InstallationDetailsGroupBox.Controls.Add(this.FlowLayoutPanel);
			this.InstallationDetailsGroupBox.Controls.Add(this.canReregisterCheckBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.LicenseeContactGuidFindBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.TechContactGuidFindBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.productBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.DatabaseIdBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.DatabaseNumberBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.HostedLocationDropEdit);
			this.InstallationDetailsGroupBox.Controls.Add(this.activeCheckBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.ReleaseRingsDropEdit);
			this.InstallationDetailsGroupBox.Controls.Add(this.LicenceTypeDropEdit);
			this.InstallationDetailsGroupBox.Controls.Add(this.ServerCodeTextBox);
			this.InstallationDetailsGroupBox.Controls.Add(this.ServerSecurityModeDropEdit);
			this.InstallationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InstallationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.InstallationDetailsGroupBox.Name = "InstallationDetailsGroupBox";
			this.InstallationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 405, true);
			this.InstallationDetailsGroupBox.TabIndex = 0;
			this.InstallationDetailsGroupBox.TabStop = false;
			this.InstallationDetailsGroupBox.Text = "System Setup";
			// 
			// FeatureSetDropEdit
			// 
			this.FeatureSetDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FeatureSetDropEdit, "LD_FCS_FeatureSet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_FCS_FeatureSet)));
			this.FeatureSetDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FeatureSetDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 91, true);
			this.FeatureSetDropEdit.Name = "FeatureSetDropEdit";
			this.FeatureSetDropEdit.PreBoundMaxLength = 38;
			this.FeatureSetDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FeatureSetDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 17, true);
			this.FeatureSetDropEdit.TabIndex = 5;
			// 
			// manualExpiryButton
			// 
			this.manualExpiryButton.IsCaptionOverridden = true;
			this.manualExpiryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 287, true);
			this.manualExpiryButton.Name = "manualExpiryButton";
			this.manualExpiryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 18, true);
			this.manualExpiryButton.TabIndex = 14;
			this.manualExpiryButton.Text = "...";
			this.manualExpiryButton.ToolTipCaption = null;
			this.manualExpiryButton.Click += new System.EventHandler(this.SendNewSystemShutdownDate);
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "LD_ManualLicenceExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ManualLicenceExpiry)));
			this.zDateEdit2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("fdef2b17-244f-454a-9925-8e523f8354fa", "Manual Shutdown");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 287, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 13;
			// 
			// billingModelBox
			// 
			this.BindingSource.SetBindingMember(this.billingModelBox, "BillingModelDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).BillingModelDesc)));
			this.billingModelBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("07eedebb-dd96-4136-884c-09a786db6f1e", "Billing Model");
			this.billingModelBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.billingModelBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 263, true);
			this.billingModelBox.Name = "billingModelBox";
			this.billingModelBox.ReadOnly = true;
			this.billingModelBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.billingModelBox.TabIndex = 12;
			this.billingModelBox.TabStop = false;
			// 
			// billableDropEdit
			// 
			this.billableDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.billableDropEdit, "LD_BillableDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_BillableDescription)));
			this.billableDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9daae527-7553-49f5-9560-754692fa5461", "Billable?");
			this.billableDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.billableDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 311, true);
			this.billableDropEdit.Name = "billableDropEdit";
			this.billableDropEdit.PreBoundMaxLength = 15;
			this.billableDropEdit.ShowDescriptionBox = false;
			this.billableDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.billableDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.billableDropEdit.TabIndex = 16;
			this.billableDropEdit.UseFullWidthForCodeBox = true;
			// 
			// canReregisterCheckBox
			// 
			this.canReregisterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.canReregisterCheckBox, "LD_CanReregisterToSameServer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_CanReregisterToSameServer)));
			this.canReregisterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 288, true);
			this.canReregisterCheckBox.Name = "canReregisterCheckBox";
			this.canReregisterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 16, true);
			this.canReregisterCheckBox.TabIndex = 15;
			this.canReregisterCheckBox.UseVisualStyleBackColor = true;
			// 
			// LicenseeContactGuidFindBox
			// 
			this.LicenseeContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicenseeContactGuidFindBox, "LD_OC_LicenseeAdminContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_OC_LicenseeAdminContact)));
			this.LicenseeContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 215, true);
			this.LicenseeContactGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.LicenseeContactGuidFindBox.Name = "LicenseeContactGuidFindBox";
			this.LicenseeContactGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LicenseeContactGuidFindBox.ParentType = null;
			this.LicenseeContactGuidFindBox.PopupCaption = "Select the Contact";
			this.LicenseeContactGuidFindBox.PreBoundMaxLength = 32;
			this.LicenseeContactGuidFindBox.ShowDescriptionBox = false;
			this.LicenseeContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 17, true);
			this.LicenseeContactGuidFindBox.TabIndex = 10;
			// 
			// TechContactGuidFindBox
			// 
			this.TechContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TechContactGuidFindBox, "LD_OC_ContractInstallerOrInternalTechContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_OC_ContractInstallerOrInternalTechContact)));
			this.TechContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 189, true);
			this.TechContactGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.TechContactGuidFindBox.Name = "TechContactGuidFindBox";
			this.TechContactGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TechContactGuidFindBox.ParentType = null;
			this.TechContactGuidFindBox.PopupCaption = "Select the Contact";
			this.TechContactGuidFindBox.PreBoundMaxLength = 32;
			this.TechContactGuidFindBox.ShowDescriptionBox = false;
			this.TechContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 17, true);
			this.TechContactGuidFindBox.TabIndex = 9;
			// 
			// productBox
			// 
			this.productBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productBox, "LD_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_Product)));
			this.productBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 66, true);
			this.productBox.Name = "productBox";
			this.productBox.PreBoundMaxLength = 3;
			this.productBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.productBox.TabIndex = 4;
			// 
			// DatabaseIdBox
			// 
			this.BindingSource.SetBindingMember(this.DatabaseIdBox, "DatabaseId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).DatabaseId)));
			this.DatabaseIdBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("499c86ee-091f-4b6f-84fc-95194186e453", "ID", "Database Number in Shorter Encoding Format");
			this.DatabaseIdBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 16, true);
			this.DatabaseIdBox.Name = "DatabaseIdBox";
			this.DatabaseIdBox.ReadOnly = true;
			this.DatabaseIdBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
			this.DatabaseIdBox.TabIndex = 1;
			this.DatabaseIdBox.TabStop = false;
			// 
			// DatabaseNumberBox
			// 
			this.BindingSource.SetBindingMember(this.DatabaseNumberBox, "LD_DatabaseNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_DatabaseNumber)));
			this.DatabaseNumberBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 16, true);
			this.DatabaseNumberBox.Name = "DatabaseNumberBox";
			this.DatabaseNumberBox.ReadOnly = true;
			this.DatabaseNumberBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			this.DatabaseNumberBox.TabIndex = 0;
			this.DatabaseNumberBox.TabStop = false;
			// 
			// HostedLocationDropEdit
			// 
			this.HostedLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HostedLocationDropEdit, "LD_HostedLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_HostedLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).HostedLocationDesciption)));
			this.HostedLocationDropEdit.BindToForDescription = "HostedLocationDesciption";
			this.HostedLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 139, true);
			this.HostedLocationDropEdit.Name = "HostedLocationDropEdit";
			this.HostedLocationDropEdit.PreBoundMaxLength = 3;
			this.HostedLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.HostedLocationDropEdit.TabIndex = 7;
			// 
			// activeCheckBox
			// 
			this.activeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.activeCheckBox, "LD_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_IsActive)));
			this.activeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 17, true);
			this.activeCheckBox.Name = "activeCheckBox";
			this.activeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 16, true);
			this.activeCheckBox.TabIndex = 2;
			this.activeCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReleaseRingsDropEdit
			// 
			this.ReleaseRingsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseRingsDropEdit, "LD_ReleaseRing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ReleaseRing)));
			this.ReleaseRingsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 165, true);
			this.ReleaseRingsDropEdit.Name = "ReleaseRingsDropEdit";
			this.ReleaseRingsDropEdit.PreBoundMaxLength = 3;
			this.ReleaseRingsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.ReleaseRingsDropEdit.TabIndex = 8;
			// 
			// allowAutoLoginCheckBox
			// 
			this.allowAutoLoginCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.allowAutoLoginCheckBox, "LD_AllowAutoLogin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_AllowAutoLogin)));
			this.allowAutoLoginCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.allowAutoLoginCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 71, true);
			this.allowAutoLoginCheckBox.Name = "allowAutoLoginCheckBox";
			this.allowAutoLoginCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 16, true);
			this.allowAutoLoginCheckBox.TabIndex = 21;
			this.allowAutoLoginCheckBox.Text = "Allow Web Auto Login";
			this.allowAutoLoginCheckBox.UseVisualStyleBackColor = true;
			// 
			// enablePackageDownloadOptimizationCheckBox
			// 
			this.enablePackageDownloadOptimizationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.enablePackageDownloadOptimizationCheckBox, "LD_EnablePackageDownloadOptimization");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_EnablePackageDownloadOptimization)));
			this.enablePackageDownloadOptimizationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 148, true);
			this.enablePackageDownloadOptimizationCheckBox.Name = "enablePackageDownloadOptimizationCheckBox";
			this.enablePackageDownloadOptimizationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 16, true);
			this.enablePackageDownloadOptimizationCheckBox.TabIndex = 22;
			this.enablePackageDownloadOptimizationCheckBox.Text = "Package Download Skip Optimization";
			this.enablePackageDownloadOptimizationCheckBox.UseVisualStyleBackColor = true;
			// 
			// preRegistrationExpiryDateEdit
			// 
			this.preRegistrationExpiryDateEdit.AllowDrop = true;
			this.preRegistrationExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.preRegistrationExpiryDateEdit, "LD_PreRegistrationExpiryDateUTC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_PreRegistrationExpiryDateUTC)));
			this.preRegistrationExpiryDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|9791c4e4-30fd-4d8c-9bf6-82306bad94bb", "Pre-Registration Expiry Date (UTC)");
			this.preRegistrationExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 99, true);
			this.preRegistrationExpiryDateEdit.Name = "preRegistrationExpiryDateEdit";
			this.preRegistrationExpiryDateEdit.TabIndex = 21;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "LD_LicenceExpiry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_LicenceExpiry)));
			this.zDateEdit1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|04a7d3b7-4a0d-4124-9df5-f5f0a7bf95cc", "Reported Shutdown");
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 335, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 15;
			// 
			// eAdaptorUrlBox
			// 
			this.eAdaptorUrlBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eAdaptorUrlBox, "LD_OutboundEAdaptorUrl");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_OutboundEAdaptorUrl)));
			this.eAdaptorUrlBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3B44FCDE-2B49-447B-8B91-8393859E18E8", "Outbound eAdaptor Url");
			this.eAdaptorUrlBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.eAdaptorUrlBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 359, true);
			this.eAdaptorUrlBox.Name = "eAdaptorUrlBox";
			this.eAdaptorUrlBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.eAdaptorUrlBox.TabIndex = 16;
			// 
			// registrationStatusBox
			// 
			this.registrationStatusBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.registrationStatusBox, "LD_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_Status)));
			this.registrationStatusBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b5054d8e-bc6f-4890-9145-e394b6a82dab", "Registration");
			this.registrationStatusBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 41, true);
			this.registrationStatusBox.Name = "registrationStatusBox";
			this.registrationStatusBox.PreBoundMaxLength = 3;
			this.registrationStatusBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.registrationStatusBox.TabIndex = 1;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "LD_HostDBName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_HostDBName)));
			this.zTextBox1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|23b26647-89f9-48b9-9063-ba088e382d4c", "Reg. Database Name");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 215, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.zTextBox1.TabIndex = 10;
			// 
			// HostDBInstanceTextBox
			// 
			this.BindingSource.SetBindingMember(this.HostDBInstanceTextBox, "LD_HostDBInstance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_HostDBInstance)));
			this.HostDBInstanceTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|2f388719-0bbf-4f71-a650-128add05ff9e", "Reg. DB Instance Name");
			this.HostDBInstanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 189, true);
			this.HostDBInstanceTextBox.Name = "HostDBInstanceTextBox";
			this.HostDBInstanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.HostDBInstanceTextBox.TabIndex = 9;
			// 
			// HostServerNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.HostServerNameTextBox, "LD_HostServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_HostServerName)));
			this.HostServerNameTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|f271fca6-c497-4e54-b72d-4b99cddb2e01", "Reg. Server Name");
			this.HostServerNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 165, true);
			this.HostServerNameTextBox.Name = "HostServerNameTextBox";
			this.HostServerNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.HostServerNameTextBox.TabIndex = 8;
			// 
			// ContactDetailsGroupBox
			// 
			this.ContactDetailsGroupBox.Controls.Add(this.zTextBox5);
			this.ContactDetailsGroupBox.Controls.Add(this.zTextBox6);
			this.ContactDetailsGroupBox.Controls.Add(this.zTextBox7);
			this.ContactDetailsGroupBox.Controls.Add(this.zTextBox4);
			this.ContactDetailsGroupBox.Controls.Add(this.registrationStatusBox);
			this.ContactDetailsGroupBox.Controls.Add(this.zTextBox2);
			this.ContactDetailsGroupBox.Controls.Add(this.zTextBox1);
			this.ContactDetailsGroupBox.Controls.Add(this.zCalcEdit2);
			this.ContactDetailsGroupBox.Controls.Add(this.LastHeartbeatDateEdit);
			this.ContactDetailsGroupBox.Controls.Add(this.zCalcEdit1);
			this.ContactDetailsGroupBox.Controls.Add(this.InternalPOP3EmailTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.HostDBInstanceTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.InternalSMTPEmailTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.HostServerNameTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.PublicEmailAddressTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.zDateEdit1);
			this.ContactDetailsGroupBox.Controls.Add(this.eAdaptorUrlBox);
			this.ContactDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 2, true);
			this.ContactDetailsGroupBox.Name = "ContactDetailsGroupBox";
			this.ContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 400, true);
			this.ContactDetailsGroupBox.TabIndex = 1;
			this.ContactDetailsGroupBox.TabStop = false;
			this.ContactDetailsGroupBox.Text = "Site Details";
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "LD_ReportedHostServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ReportedHostServerName)));
			this.zTextBox5.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|4f9d36c2-97c3-4a4c-99f1-724744e7c823", "Rpt. Server Name");
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 239, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.ReadOnly = true;
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.zTextBox5.TabIndex = 11;
			// 
			// zTextBox6
			// 
			this.BindingSource.SetBindingMember(this.zTextBox6, "LD_ReportedHostDBInstance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ReportedHostDBInstance)));
			this.zTextBox6.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|4f9d36c2-97c3-4a4c-99f1-724744e7c823", "Rpt. DB Instance Name");
			this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 263, true);
			this.zTextBox6.Name = "zTextBox6";
			this.zTextBox6.ReadOnly = true;
			this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.zTextBox6.TabIndex = 12;
			// 
			// zTextBox7
			// 
			this.BindingSource.SetBindingMember(this.zTextBox7, "LD_ReportedHostDBName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ReportedHostDBName)));
			this.zTextBox7.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|cf75fa56-9de7-43e9-b86e-81d4e184e5af", "Rpt. Database Name");
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 287, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.ReadOnly = true;
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.zTextBox7.TabIndex = 13;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "LD_HostConnectionServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_HostConnectionServerName)));
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 311, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.zTextBox4.TabIndex = 14;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "LD_InternalPop3UserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_InternalPop3UserName)));
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 115, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.zTextBox2.TabIndex = 5;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "LD_InternalSmtpPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_InternalSmtpPort)));
			this.zCalcEdit2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|68876585-9b9b-4909-8ff7-3a34dd9c0a44", "Port");
			this.zCalcEdit2.DecimalPlaces = 0;
			this.zCalcEdit2.Decimals = 0;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 139, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.zCalcEdit2.TabIndex = 7;
			this.zCalcEdit2.Text = "0";
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LastHeartbeatDateEdit
			// 
			this.LastHeartbeatDateEdit.AllowDrop = true;
			this.LastHeartbeatDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.LastHeartbeatDateEdit, "LD_LastHeartbeat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_LastHeartbeat)));
			this.LastHeartbeatDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|b1a8f372-db64-44d3-ab5b-ff1756d40c61", "Last Heartbeat");
			this.LastHeartbeatDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			this.LastHeartbeatDateEdit.Name = "LastHeartbeatDateEdit";
			this.LastHeartbeatDateEdit.TabIndex = 0;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "LD_InternalPop3Port");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_InternalPop3Port)));
			this.zCalcEdit1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|8caf6bca-b44a-42e0-82f4-00b4d38137ef", "Port");
			this.zCalcEdit1.DecimalPlaces = 0;
			this.zCalcEdit1.Decimals = 0;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 91, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.zCalcEdit1.TabIndex = 4;
			this.zCalcEdit1.Text = "0";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SoftwareInstallAddressDropEdit
			// 
			this.SoftwareInstallAddressDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SoftwareInstallAddressDropEdit, "LD_OA_SoftwareInstallAddressDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_OA_SoftwareInstallAddressDetails)));
			this.SoftwareInstallAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 124, true);
			this.SoftwareInstallAddressDropEdit.Name = "SoftwareInstallAddressDropEdit";
			this.SoftwareInstallAddressDropEdit.PreBoundMaxLength = 31;
			this.SoftwareInstallAddressDropEdit.ShowDescriptionBox = false;
			this.SoftwareInstallAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 17, true);
			this.SoftwareInstallAddressDropEdit.TabIndex = 6;
			// 
			// UpgradeMethodDropEdit
			// 
			this.UpgradeMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UpgradeMethodDropEdit, "LD_AvailableUpgradeMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_AvailableUpgradeMethod)));
			this.UpgradeMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 12, true);
			this.UpgradeMethodDropEdit.Name = "UpgradeMethodDropEdit";
			this.UpgradeMethodDropEdit.PreBoundMaxLength = 3;
			this.UpgradeMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.UpgradeMethodDropEdit.TabIndex = 7;
			// 
			// CurrentSentVersionGuidFindBox
			// 
			this.CurrentSentVersionGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrentSentVersionGuidFindBox, "LD_HL_CurrentSentVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_HL_CurrentSentVersion)));
			this.CurrentSentVersionGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 75, true);
			this.CurrentSentVersionGuidFindBox.Name = "CurrentSentVersionGuidFindBox";
			this.CurrentSentVersionGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CurrentSentVersionGuidFindBox.ParentType = null;
			this.CurrentSentVersionGuidFindBox.PreBoundMaxLength = 20;
			this.CurrentSentVersionGuidFindBox.ShowDescriptionBox = false;
			this.CurrentSentVersionGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 17, true);
			this.CurrentSentVersionGuidFindBox.TabIndex = 4;
			// 
			// CurrentRunningVersionGuidFindBox
			// 
			this.CurrentRunningVersionGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrentRunningVersionGuidFindBox, "LD_HL_CurrentRunningVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_HL_CurrentRunningVersion)));
			this.CurrentRunningVersionGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 12, true);
			this.CurrentRunningVersionGuidFindBox.Name = "CurrentRunningVersionGuidFindBox";
			this.CurrentRunningVersionGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CurrentRunningVersionGuidFindBox.ParentType = null;
			this.CurrentRunningVersionGuidFindBox.PreBoundMaxLength = 20;
			this.CurrentRunningVersionGuidFindBox.ShowDescriptionBox = false;
			this.CurrentRunningVersionGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 17, true);
			this.CurrentRunningVersionGuidFindBox.TabIndex = 1;
			// 
			// MinutesLabel
			// 
			this.MinutesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MinutesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 40, true);
			this.MinutesLabel.Name = "MinutesLabel";
			this.MinutesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 17, true);
			this.MinutesLabel.TabIndex = 9;
			this.MinutesLabel.Text = "Minutes";
			this.MinutesLabel.UseMnemonic = false;
			// 
			// MegabytesLabel
			// 
			this.MegabytesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MegabytesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 67, true);
			this.MegabytesLabel.Name = "MegabytesLabel";
			this.MegabytesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 17, true);
			this.MegabytesLabel.TabIndex = 11;
			this.MegabytesLabel.Text = "Megabytes";
			this.MegabytesLabel.UseMnemonic = false;
			// 
			// MaxEmailSizeLimitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxEmailSizeLimitCalcEdit, "LD_MaxDataInMegBeforeAck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_MaxDataInMegBeforeAck)));
			this.MaxEmailSizeLimitCalcEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|a05d6dad-55ae-4164-b04d-8b4aea7058ba", "Max E-Mail Size Limit");
			this.MaxEmailSizeLimitCalcEdit.DecimalPlaces = 0;
			this.MaxEmailSizeLimitCalcEdit.Decimals = 0;
			this.MaxEmailSizeLimitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 67, true);
			this.MaxEmailSizeLimitCalcEdit.Name = "MaxEmailSizeLimitCalcEdit";
			this.MaxEmailSizeLimitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.MaxEmailSizeLimitCalcEdit.TabIndex = 10;
			this.MaxEmailSizeLimitCalcEdit.Text = "0";
			this.MaxEmailSizeLimitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MessageRetryTimeoutCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MessageRetryTimeoutCalcEdit, "LD_RetryTimeoutInMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_RetryTimeoutInMinutes)));
			this.MessageRetryTimeoutCalcEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|c4cad12b-cff7-41da-b421-d038171c57e5", "Message Retry Timeout");
			this.MessageRetryTimeoutCalcEdit.DecimalPlaces = 0;
			this.MessageRetryTimeoutCalcEdit.Decimals = 0;
			this.MessageRetryTimeoutCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 40, true);
			this.MessageRetryTimeoutCalcEdit.Name = "MessageRetryTimeoutCalcEdit";
			this.MessageRetryTimeoutCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.MessageRetryTimeoutCalcEdit.TabIndex = 8;
			this.MessageRetryTimeoutCalcEdit.Text = "0";
			this.MessageRetryTimeoutCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RequestVersionReportButton
			// 
			this.RequestVersionReportButton.IsCaptionOverridden = true;
			this.RequestVersionReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 3, true);
			this.RequestVersionReportButton.Name = "RequestVersionReportButton";
			this.RequestVersionReportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 22, true);
			this.RequestVersionReportButton.TabIndex = 6;
			this.RequestVersionReportButton.Text = "Request Version Report";
			this.RequestVersionReportButton.ToolTipCaption = null;
			this.RequestVersionReportButton.Click += new System.EventHandler(this.RequestVersionReportButton_Click);
			// 
			// ResetHeartbeatButton
			// 
			this.ResetHeartbeatButton.IsCaptionOverridden = true;
			this.ResetHeartbeatButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 3, true);
			this.ResetHeartbeatButton.Name = "ResetHeartbeatButton";
			this.ResetHeartbeatButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 22, true);
			this.ResetHeartbeatButton.TabIndex = 5;
			this.ResetHeartbeatButton.Text = "Reset Heartbeat";
			this.ResetHeartbeatButton.ToolTipCaption = null;
			this.ResetHeartbeatButton.Click += new System.EventHandler(this.ResetHeartbeatButton_Click);
			// 
			// requestLicenceUsageButton
			// 
			this.requestLicenceUsageButton.IsCaptionOverridden = true;
			this.requestLicenceUsageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 3, true);
			this.requestLicenceUsageButton.Name = "requestLicenceUsageButton";
			this.requestLicenceUsageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 22, true);
			this.requestLicenceUsageButton.TabIndex = 1;
			this.requestLicenceUsageButton.Text = "Licence Usage";
			this.requestLicenceUsageButton.ToolTipCaption = null;
			this.requestLicenceUsageButton.Click += new System.EventHandler(this.RequestLicenceUsageButton_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.65229F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.34771F));
			this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.InstallationDetailsGroupBox, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.ContactDetailsGroupBox, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.BottomPanel, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(60)));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 639, true);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tableLayoutPanel1.SetColumnSpan(this.tabControl1, 2);
			this.tabControl1.Controls.Add(this.ConfigDetailsTabPage);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.DocEngineStatsTabPage);
			this.tabControl1.Controls.Add(this.sqlServerVersionDetailsTabPage);
			this.tabControl1.Controls.Add(this.zTabPage1);
			this.tabControl1.Controls.Add(this.UserManagementTabPage);
			this.tabControl1.Controls.Add(this.TrustedMessagingTabPage);
			this.tabControl1.Controls.Add(this.ClientCompaniesTabPage);
			this.tabControl1.Controls.Add(this.CompaniesTabPage);
			this.tabControl1.Controls.Add(this.UpgradeScheduleTabPage);
			this.tabControl1.Controls.Add(this.FeatureControlTabPage);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 406, true);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 202, true);
			this.tabControl1.TabIndex = 16;
			// 
			// ConfigDetailsTabPage
			// 
			this.ConfigDetailsTabPage.Controls.Add(this.currentVersionLastTime);
			this.ConfigDetailsTabPage.Controls.Add(this.currentVersionFirstTime);
			this.ConfigDetailsTabPage.Controls.Add(this.PurchasedLicenceUnitsBox);
			this.ConfigDetailsTabPage.Controls.Add(this.SoftwareInstallAddressDropEdit);
			this.ConfigDetailsTabPage.Controls.Add(this.UpgradeMethodDropEdit);
			this.ConfigDetailsTabPage.Controls.Add(this.CurrentSentVersionGuidFindBox);
			this.ConfigDetailsTabPage.Controls.Add(this.CurrentRunningVersionGuidFindBox);
			this.ConfigDetailsTabPage.Controls.Add(this.MessageRetryTimeoutCalcEdit);
			this.ConfigDetailsTabPage.Controls.Add(this.MinutesLabel);
			this.ConfigDetailsTabPage.Controls.Add(this.MegabytesLabel);
			this.ConfigDetailsTabPage.Controls.Add(this.MaxEmailSizeLimitCalcEdit);
			this.ConfigDetailsTabPage.Controls.Add(this.enablePackageDownloadOptimizationCheckBox);
			this.ConfigDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ConfigDetailsTabPage.Name = "ConfigDetailsTabPage";
			this.ConfigDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConfigDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 180, true);
			this.ConfigDetailsTabPage.TabIndex = 0;
			this.ConfigDetailsTabPage.Text = "Configuration Details";
			// 
			// currentVersionLastTime
			// 
			this.currentVersionLastTime.AllowDrop = true;
			this.currentVersionLastTime.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.currentVersionLastTime, "LD_CurrentVersionLastReportLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_CurrentVersionLastReportLocal)));
			this.currentVersionLastTime.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("99fd4952-49af-4bfd-a22b-eca90d11fdb9", "Last Reported");
			this.currentVersionLastTime.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.currentVersionLastTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 52, true);
			this.currentVersionLastTime.Name = "currentVersionLastTime";
			this.currentVersionLastTime.TabIndex = 3;
			// 
			// currentVersionFirstTime
			// 
			this.currentVersionFirstTime.AllowDrop = true;
			this.currentVersionFirstTime.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.currentVersionFirstTime, "LD_CurrentVersionFirstReportLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_CurrentVersionFirstReportLocal)));
			this.currentVersionFirstTime.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e1032d46-ab5b-45d4-b8c9-9ab53ee277a0", "First Reported");
			this.currentVersionFirstTime.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.currentVersionFirstTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 32, true);
			this.currentVersionFirstTime.Name = "currentVersionFirstTime";
			this.currentVersionFirstTime.TabIndex = 2;
			// 
			// PurchasedLicenceUnitsBox
			// 
			this.BindingSource.SetBindingMember(this.PurchasedLicenceUnitsBox, "LD_PurchasedLicenceUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_PurchasedLicenceUnits)));
			this.PurchasedLicenceUnitsBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("be010a30-e3c9-4882-a295-66e6ce18c0ad", "Purchased License Units");
			this.PurchasedLicenceUnitsBox.DecimalPlaces = 0;
			this.PurchasedLicenceUnitsBox.Decimals = 0;
			this.PurchasedLicenceUnitsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 100, true);
			this.PurchasedLicenceUnitsBox.Name = "PurchasedLicenceUnitsBox";
			this.PurchasedLicenceUnitsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
			this.PurchasedLicenceUnitsBox.TabIndex = 5;
			this.PurchasedLicenceUnitsBox.Text = "0";
			this.PurchasedLicenceUnitsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.panel1);
			this.tabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Database And Backup File Paths";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.zTextBox3);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.panel1.Name = "panel1";
			this.panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 175, true);
			this.panel1.TabIndex = 0;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "LD_DatabaseFilePathDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_DatabaseFilePathDetail)));
			this.zTextBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zTextBox3.Multiline = true;
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 169, true);
			this.zTextBox3.TabIndex = 16;
			// 
			// DocEngineStatsTabPage
			// 
			this.DocEngineStatsTabPage.Controls.Add(this.NofActivePrintQueuesCalcEdit);
			this.DocEngineStatsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DocEngineStatsTabPage.Name = "DocEngineStatsTabPage";
			this.DocEngineStatsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DocEngineStatsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.DocEngineStatsTabPage.TabIndex = 2;
			this.DocEngineStatsTabPage.Text = "Document Engine Statistics";
			// 
			// NofActivePrintQueuesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NofActivePrintQueuesCalcEdit, "LD_NoOfActivePrintQueues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_NoOfActivePrintQueues)));
			this.NofActivePrintQueuesCalcEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|4971b17e-a0bc-4259-add1-45d0c08d2bd6", "No. Of Active Print Queues");
			this.NofActivePrintQueuesCalcEdit.DecimalPlaces = 0;
			this.NofActivePrintQueuesCalcEdit.Decimals = 0;
			this.NofActivePrintQueuesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 38, true);
			this.NofActivePrintQueuesCalcEdit.Name = "NofActivePrintQueuesCalcEdit";
			this.NofActivePrintQueuesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.NofActivePrintQueuesCalcEdit.TabIndex = 2;
			this.NofActivePrintQueuesCalcEdit.Text = "0";
			this.NofActivePrintQueuesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// sqlServerVersionDetailsTabPage
			// 
			this.sqlServerVersionDetailsTabPage.Controls.Add(this.sqlServerEditionDropEdit);
			this.sqlServerVersionDetailsTabPage.Controls.Add(this.sqlServerFullVersionTextTextBox);
			this.sqlServerVersionDetailsTabPage.Controls.Add(this.sqlServerVersionTextBox);
			this.sqlServerVersionDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.sqlServerVersionDetailsTabPage.Name = "sqlServerVersionDetailsTabPage";
			this.sqlServerVersionDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.sqlServerVersionDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.sqlServerVersionDetailsTabPage.TabIndex = 3;
			this.sqlServerVersionDetailsTabPage.Text = "SQL Version Details";
			// 
			// sqlServerEditionDropEdit
			// 
			this.sqlServerEditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sqlServerEditionDropEdit, "LD_SQLEdition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_SQLEdition)));
			this.sqlServerEditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 37, true);
			this.sqlServerEditionDropEdit.Name = "sqlServerEditionDropEdit";
			this.sqlServerEditionDropEdit.PreBoundMaxLength = 3;
			this.sqlServerEditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.sqlServerEditionDropEdit.TabIndex = 8;
			// 
			// sqlServerFullVersionTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.sqlServerFullVersionTextTextBox, "LD_SQLVerStringForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_SQLVerStringForDisplay)));
			this.sqlServerFullVersionTextTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|e6390214-9845-4455-a72f-53a11e1abf1d", "Full Version Text");
			this.sqlServerFullVersionTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 95, true);
			this.sqlServerFullVersionTextTextBox.Multiline = true;
			this.sqlServerFullVersionTextTextBox.Name = "sqlServerFullVersionTextTextBox";
			this.sqlServerFullVersionTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 74, true);
			this.sqlServerFullVersionTextTextBox.TabIndex = 7;
			// 
			// sqlServerVersionTextBox
			// 
			this.BindingSource.SetBindingMember(this.sqlServerVersionTextBox, "LD_SQLVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_SQLVersion)));
			this.sqlServerVersionTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|72fad0db-db8d-4e8f-a061-37043c2e6926", "Version");
			this.sqlServerVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 9, true);
			this.sqlServerVersionTextBox.Name = "sqlServerVersionTextBox";
			this.sqlServerVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.sqlServerVersionTextBox.TabIndex = 1;
			// 
			// zTabPage1
			// 
			this.zTabPage1.Controls.Add(this.zLabel10);
			this.zTabPage1.Controls.Add(this.zLabel9);
			this.zTabPage1.Controls.Add(this.ProcessorSpeedTextBox);
			this.zTabPage1.Controls.Add(this.ProcessorTypeTextBox);
			this.zTabPage1.Controls.Add(this.IsVirtualMachineCheckBox);
			this.zTabPage1.Controls.Add(this.NoOfProcessorsTextBox);
			this.zTabPage1.Controls.Add(this.TotalMemoryTextBox);
			this.zTabPage1.Controls.Add(this.BIOSDateDateEdit);
			this.zTabPage1.Controls.Add(this.SystemManufacturerTextBox);
			this.zTabPage1.Controls.Add(this.OSVersionTextBox);
			this.zTabPage1.Controls.Add(this.OSNameTextBox);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.zTabPage1.TabIndex = 5;
			this.zTabPage1.Text = "Server Details";
			// 
			// zLabel10
			// 
			this.zLabel10.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(454, 120, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 19, true);
			this.zLabel10.TabIndex = 10;
			this.zLabel10.Text = "MHz";
			this.zLabel10.UseMnemonic = false;
			// 
			// zLabel9
			// 
			this.zLabel9.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 93, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 20, true);
			this.zLabel9.TabIndex = 9;
			this.zLabel9.Text = "MB";
			this.zLabel9.UseMnemonic = false;
			// 
			// ProcessorSpeedTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProcessorSpeedTextBox, "LD_ProcessorSpeedMHz");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ProcessorSpeedMHz)));
			this.ProcessorSpeedTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|54a9b3f5-884c-4fda-b6ef-d5edb0f515d7", "Processor Speed");
			this.ProcessorSpeedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProcessorSpeedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 120, true);
			this.ProcessorSpeedTextBox.Name = "ProcessorSpeedTextBox";
			this.ProcessorSpeedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.ProcessorSpeedTextBox.TabIndex = 7;
			this.ProcessorSpeedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProcessorTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProcessorTypeTextBox, "LD_ProcessorType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ProcessorType)));
			this.ProcessorTypeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|1a3691d0-01b7-4ccd-b831-bd581bd04310", "Processor Type");
			this.ProcessorTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProcessorTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 146, true);
			this.ProcessorTypeTextBox.Name = "ProcessorTypeTextBox";
			this.ProcessorTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 17, true);
			this.ProcessorTypeTextBox.TabIndex = 8;
			// 
			// IsVirtualMachineCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsVirtualMachineCheckBox, "LD_VirtualMachineDetected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_VirtualMachineDetected)));
			this.IsVirtualMachineCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|ff965786-0697-4025-90bd-428c7fa7f70a", "Is Virtual Machine");
			this.IsVirtualMachineCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(573, 67, true);
			this.IsVirtualMachineCheckBox.Name = "IsVirtualMachineCheckBox";
			this.IsVirtualMachineCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 22, true);
			this.IsVirtualMachineCheckBox.TabIndex = 3;
			this.IsVirtualMachineCheckBox.UseVisualStyleBackColor = true;
			// 
			// NoOfProcessorsTextBox
			// 
			this.BindingSource.SetBindingMember(this.NoOfProcessorsTextBox, "LD_NoOfProcessorCores");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_NoOfProcessorCores)));
			this.NoOfProcessorsTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|29cfb451-a07d-4859-baf7-2dc595c5b5b5", "No. of Processors");
			this.NoOfProcessorsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NoOfProcessorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 119, true);
			this.NoOfProcessorsTextBox.Name = "NoOfProcessorsTextBox";
			this.NoOfProcessorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.NoOfProcessorsTextBox.TabIndex = 6;
			this.NoOfProcessorsTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalMemoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalMemoryTextBox, "LD_TotalPhysicalMemoryMB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_TotalPhysicalMemoryMB)));
			this.TotalMemoryTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|affa323e-42d0-472a-9450-e6b83a375d95", "Total Physical Memory");
			this.TotalMemoryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TotalMemoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 93, true);
			this.TotalMemoryTextBox.Name = "TotalMemoryTextBox";
			this.TotalMemoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.TotalMemoryTextBox.TabIndex = 4;
			this.TotalMemoryTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BIOSDateDateEdit
			// 
			this.BIOSDateDateEdit.AllowDrop = true;
			this.BIOSDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.BIOSDateDateEdit, "LD_BIOSDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_BIOSDate)));
			this.BIOSDateDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|b51bfd4b-7242-4319-923e-3627e1b23aa8", "BIOS Date");
			this.BIOSDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 93, true);
			this.BIOSDateDateEdit.Name = "BIOSDateDateEdit";
			this.BIOSDateDateEdit.TabIndex = 5;
			// 
			// SystemManufacturerTextBox
			// 
			this.BindingSource.SetBindingMember(this.SystemManufacturerTextBox, "LD_SystemManufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_SystemManufacturer)));
			this.SystemManufacturerTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|5c2d5eb7-80ae-4512-a718-7f2f023c8ce5", "System Manufacturer");
			this.SystemManufacturerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SystemManufacturerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 67, true);
			this.SystemManufacturerTextBox.Name = "SystemManufacturerTextBox";
			this.SystemManufacturerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 17, true);
			this.SystemManufacturerTextBox.TabIndex = 2;
			// 
			// OSVersionTextBox
			// 
			this.BindingSource.SetBindingMember(this.OSVersionTextBox, "LD_OSVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_OSVersion)));
			this.OSVersionTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|26dda57d-a6f9-44ba-8cc1-e37b9e91f04e", "OS Version");
			this.OSVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OSVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 40, true);
			this.OSVersionTextBox.Name = "OSVersionTextBox";
			this.OSVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 17, true);
			this.OSVersionTextBox.TabIndex = 1;
			// 
			// OSNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.OSNameTextBox, "LD_OSName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_OSName)));
			this.OSNameTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|ba3c3081-42f7-4ab8-a2b8-6de99b3d9592", "OS Name");
			this.OSNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OSNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 13, true);
			this.OSNameTextBox.Name = "OSNameTextBox";
			this.OSNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 17, true);
			this.OSNameTextBox.TabIndex = 0;
			// 
			// UserManagementTabPage
			// 
			this.UserManagementTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("0714b3ba-d042-4485-bf43-6721334b43a7", "User Management");
			this.UserManagementTabPage.Controls.Add(this.StaffReportDateEdit);
			this.UserManagementTabPage.Controls.Add(this.WebAccessOrgGuidFindBox);
			this.UserManagementTabPage.Controls.Add(this.allowAutoLoginCheckBox);
			this.UserManagementTabPage.Controls.Add(this.preRegistrationExpiryDateEdit);
			this.UserManagementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.UserManagementTabPage.Name = "UserManagementTabPage";
			this.UserManagementTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UserManagementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.UserManagementTabPage.TabIndex = 7;
			this.UserManagementTabPage.Text = "User Management";
			// 
			// StaffReportDateEdit
			// 
			this.StaffReportDateEdit.AllowDrop = true;
			this.StaffReportDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.StaffReportDateEdit, "LD_StaffFirstReportUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_StaffFirstReportUtc)));
			this.StaffReportDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9c1e723a-26a0-4c9b-9169-79d9df337392", "Full Staff Report Date");
			this.StaffReportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 44, true);
			this.StaffReportDateEdit.Name = "StaffReportDateEdit";
			this.StaffReportDateEdit.TabIndex = 18;
			// 
			// WebAccessOrgGuidFindBox
			// 
			this.WebAccessOrgGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WebAccessOrgGuidFindBox, "LD_OH_WebAccessOrg");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_OH_WebAccessOrg)));
			this.WebAccessOrgGuidFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c000cce5-0f11-4311-be7f-3491e06ece6c", "Master Org");
			this.WebAccessOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 17, true);
			this.WebAccessOrgGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.WebAccessOrgGuidFindBox.Name = "WebAccessOrgGuidFindBox";
			this.WebAccessOrgGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WebAccessOrgGuidFindBox.ParentType = null;
			this.WebAccessOrgGuidFindBox.PopupCaption = "Select Master Org";
			this.WebAccessOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 17, true);
			this.WebAccessOrgGuidFindBox.TabIndex = 17;
			// 
			// TrustedMessagingTabPage
			// 
			this.TrustedMessagingTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4285ae4e-2894-4986-b0ab-9790f9f91fbb", "Trusted Messaging");
			this.TrustedMessagingTabPage.Controls.Add(this.TrustedMessagingControl);
			this.TrustedMessagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.TrustedMessagingTabPage.Name = "TrustedMessagingTabPage";
			this.TrustedMessagingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TrustedMessagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.TrustedMessagingTabPage.TabIndex = 8;
			this.TrustedMessagingTabPage.Text = "Trusted Messaging";
			// 
			// TrustedMessagingControl
			// 
			this.TrustedMessagingControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TrustedMessagingControl, ".");
			this.TrustedMessagingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 13, true);
			this.TrustedMessagingControl.Name = "TrustedMessagingControl";
			this.TrustedMessagingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 162, true);
			this.TrustedMessagingControl.TabIndex = 1;
			// 
			// ClientCompaniesTabPage
			// 
			this.ClientCompaniesTabPage.Controls.Add(this.ClientCompaniesGrid);
			this.ClientCompaniesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ClientCompaniesTabPage.Name = "ClientCompaniesTabPage";
			this.ClientCompaniesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ClientCompaniesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.ClientCompaniesTabPage.TabIndex = 6;
			this.ClientCompaniesTabPage.Text = "Companies";
			this.ClientCompaniesTabPage.UseVisualStyleBackColor = true;
			// 
			// ClientCompaniesGrid
			// 
			this.ClientCompaniesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ClientCompaniesGrid, "ClientCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).ClientCompanies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).ClientCompanies)).SyncRoot)).LCC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).ClientCompanies)).SyncRoot)).LCC_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).ClientCompanies)).SyncRoot)).LCC_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).ClientCompanies)).SyncRoot)).Org.OH_Code)));
			this.ClientCompaniesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo7.Caption = "Code";
			zTextBoxColumnStyleInfo7.ColumnName = "LCC_Code";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.Caption = "Name";
			zTextBoxColumnStyleInfo8.ColumnName = "LCC_Name";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zTextBoxColumnStyleInfo9.Caption = "Country";
			zTextBoxColumnStyleInfo9.ColumnName = "LCC_RN_NKCountryCode";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.Caption = "Organisation";
			zTextBoxColumnStyleInfo10.ColumnName = "Org+OH_Code";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ClientCompaniesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClientCompaniesGrid.GridId = "7d364da0-8aee-4366-bda3-bdd236c700ea";
			this.ClientCompaniesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClientCompaniesGrid.LayoutKey = "zGrid1";
			this.ClientCompaniesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ClientCompaniesGrid.Name = "ClientCompaniesGrid";
			this.ClientCompaniesGrid.ReadOnly = true;
			this.ClientCompaniesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 175, true);
			this.ClientCompaniesGrid.TabIndex = 1;
			// 
			// CompaniesTabPage
			// 
			this.CompaniesTabPage.Controls.Add(this.zGrid1);
			this.CompaniesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CompaniesTabPage.Name = "CompaniesTabPage";
			this.CompaniesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CompaniesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.CompaniesTabPage.TabIndex = 4;
			this.CompaniesTabPage.Text = "Organisations";
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "LicHeadersForAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LicHeadersForAllCompanies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LicHeadersForAllCompanies)).SyncRoot)).OrganisationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LicHeadersForAllCompanies)).SyncRoot)).OrganisationFullName)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|883f8a39-d6f9-48cc-8f5d-89f96ac6a8de", "Org. Code", "Organization Code");
			zTextBoxColumnStyleInfo1.ColumnName = "OrganisationCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|e9c10e1e-53c7-4ccc-af35-b11d96539f3a", "Org. Name", "Organization Name");
			zTextBoxColumnStyleInfo2.ColumnName = "OrganisationFullName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "7d364da0-8aee-4366-bda3-bdd236c700ea";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.ReadOnly = true;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 175, true);
			this.zGrid1.TabIndex = 0;
			// 
			// UpgradeScheduleTabPage
			// 
			this.UpgradeScheduleTabPage.Controls.Add(this.nextRunTimeUtcMUG);
			this.UpgradeScheduleTabPage.Controls.Add(this.nextRunTimeUtcUPG);
			this.UpgradeScheduleTabPage.Controls.Add(this.scheduleStateMUG);
			this.UpgradeScheduleTabPage.Controls.Add(this.scheduleStateUPG);
			this.UpgradeScheduleTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.UpgradeScheduleTabPage.Name = "UpgradeScheduleTabPage";
			this.UpgradeScheduleTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UpgradeScheduleTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.UpgradeScheduleTabPage.TabIndex = 9;
			this.UpgradeScheduleTabPage.Text = "Upgrade Schedule";
			// 
			// nextRunTimeUtcMUG
			// 
			this.nextRunTimeUtcMUG.AllowDrop = true;
			this.nextRunTimeUtcMUG.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.nextRunTimeUtcMUG, "LD_NextRunTimeUtcMUG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_NextRunTimeUtcMUG)));
			this.nextRunTimeUtcMUG.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|1ee58666-86cd-febd-56fe-7efdc025d8d7", "MUG Next Run Time");
			this.nextRunTimeUtcMUG.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.nextRunTimeUtcMUG.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			this.nextRunTimeUtcMUG.Name = "nextRunTimeUtcMUG";
			this.nextRunTimeUtcMUG.TabIndex = 0;
			// 
			// nextRunTimeUtcUPG
			// 
			this.nextRunTimeUtcUPG.AllowDrop = true;
			this.nextRunTimeUtcUPG.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.nextRunTimeUtcUPG, "LD_NextRunTimeUtcUPG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_NextRunTimeUtcUPG)));
			this.nextRunTimeUtcUPG.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|1663d801-9ff2-aeba-6a95-8a3bb6ca750e", "UPG Next Run Time");
			this.nextRunTimeUtcUPG.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.nextRunTimeUtcUPG.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 16, true);
			this.nextRunTimeUtcUPG.Name = "nextRunTimeUtcUPG";
			this.nextRunTimeUtcUPG.TabIndex = 1;
			// 
			// scheduleStateMUG
			// 
			this.BindingSource.SetBindingMember(this.scheduleStateMUG, "LD_ScheduleStateMUG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ScheduleStateMUG)));
			this.scheduleStateMUG.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|d4ebbe22-7d90-f8c1-2fab-60e092d7d849", "MUG Schedule State");
			this.scheduleStateMUG.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 36, true);
			this.scheduleStateMUG.Multiline = true;
			this.scheduleStateMUG.Name = "scheduleStateMUG";
			this.scheduleStateMUG.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 74, true);
			this.scheduleStateMUG.TabIndex = 2;
			// 
			// scheduleStateUPG
			// 
			this.BindingSource.SetBindingMember(this.scheduleStateUPG, "LD_ScheduleStateUPG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ScheduleStateUPG)));
			this.scheduleStateUPG.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|19241b25-2765-ebc7-26f4-acfbb7b1b1d4", "UPG Schedule State");
			this.scheduleStateUPG.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 36, true);
			this.scheduleStateUPG.Multiline = true;
			this.scheduleStateUPG.Name = "scheduleStateUPG";
			this.scheduleStateUPG.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 74, true);
			this.scheduleStateUPG.TabIndex = 3;
			// 
			// FeatureControlTabPage
			// 
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlRuleLastSyncUtcDateEdit);
			this.FeatureControlTabPage.Controls.Add(this.FeatureControlRuleLastSyncContentTextBox);
			this.FeatureControlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.FeatureControlTabPage.Name = "FeatureControlTabPage";
			this.FeatureControlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 180, true);
			this.FeatureControlTabPage.TabIndex = 10;
			this.FeatureControlTabPage.Text = "Feature Control";
			// 
			// FeatureControlRuleLastSyncUtcDateEdit
			// 
			this.FeatureControlRuleLastSyncUtcDateEdit.AllowDrop = true;
			this.FeatureControlRuleLastSyncUtcDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.FeatureControlRuleLastSyncUtcDateEdit, "LD_FeatureControlRuleLastSyncUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_FeatureControlRuleLastSyncUtc)));
			this.FeatureControlRuleLastSyncUtcDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|43077e1b-6b11-48ee-ab65-c0ac13591daa", "Last Successful Syncrhonization");
			this.FeatureControlRuleLastSyncUtcDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FeatureControlRuleLastSyncUtcDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 11, true);
			this.FeatureControlRuleLastSyncUtcDateEdit.Name = "FeatureControlRuleLastSyncUtcDateEdit";
			this.FeatureControlRuleLastSyncUtcDateEdit.TabIndex = 3;
			// 
			// FeatureControlRuleLastSyncContentTextBox
			// 
			this.FeatureControlRuleLastSyncContentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FeatureControlRuleLastSyncContentTextBox, "LD_FeatureControlRuleLastSyncContent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_FeatureControlRuleLastSyncContent)));
			this.FeatureControlRuleLastSyncContentTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceDatabaseForm|03c5ec2b-b173-4dfb-bbdd-750b4e4b610a", "Last Payload Contents");
			this.FeatureControlRuleLastSyncContentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FeatureControlRuleLastSyncContentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 32, true);
			this.FeatureControlRuleLastSyncContentTextBox.Multiline = true;
			this.FeatureControlRuleLastSyncContentTextBox.Name = "FeatureControlRuleLastSyncContentTextBox";
			this.FeatureControlRuleLastSyncContentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.FeatureControlRuleLastSyncContentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(579, 133, true);
			this.FeatureControlRuleLastSyncContentTextBox.TabIndex = 4;
			// 
			// BottomPanel
			//
			this.tableLayoutPanel1.SetColumnSpan(this.BottomPanel, 2);
			this.BottomPanel.Controls.Add(this.requestStaffReportButton);
			this.BottomPanel.Controls.Add(this.zLabel2);
			this.BottomPanel.Controls.Add(this.requestLicenceDatabaseLogsButton);
			this.BottomPanel.Controls.Add(this.requestLicenceUsageButton);
			this.BottomPanel.Controls.Add(this.zLabel1);
			this.BottomPanel.Controls.Add(this.RequestVersionReportButton);
			this.BottomPanel.Controls.Add(this.ResetHeartbeatButton);
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 593, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 54, true);
			this.BottomPanel.TabIndex = 3;
			// 
			// requestStaffReportButton
			// 
			this.requestStaffReportButton.IsCaptionOverridden = true;
			this.requestStaffReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 3, true);
			this.requestStaffReportButton.Name = "requestStaffReportButton";
			this.requestStaffReportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.requestStaffReportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.requestStaffReportButton.TabIndex = 3;
			this.requestStaffReportButton.Text = "Staff List";
			this.requestStaffReportButton.ToolTipCaption = null;
			this.requestStaffReportButton.Click += new System.EventHandler(this.RequestStaffReportButton_Click);
			// 
			// zLabel2
			// 
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 3, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.zLabel2.TabIndex = 4;
			this.zLabel2.Text = "ediEnterprise Only:";
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel2.UseMnemonic = false;
			// 
			// requestLicenceDatabaseLogsButton
			// 
			this.requestLicenceDatabaseLogsButton.IsCaptionOverridden = true;
			this.requestLicenceDatabaseLogsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 3, true);
			this.requestLicenceDatabaseLogsButton.Name = "requestLicenceDatabaseLogsButton";
			this.requestLicenceDatabaseLogsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.requestLicenceDatabaseLogsButton.TabIndex = 2;
			this.requestLicenceDatabaseLogsButton.Text = "Service Task Logs";
			this.requestLicenceDatabaseLogsButton.ToolTipCaption = null;
			this.requestLicenceDatabaseLogsButton.Click += new System.EventHandler(this.RequestLicenceDatabaseLogsButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Request:";
			this.zLabel1.UseMnemonic = false;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 50, true);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// MainDetailsPanel
			// 
			this.MainDetailsPanel.Controls.Add(this.tableLayoutPanel1);
			this.MainDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainDetailsPanel.Name = "MainDetailsPanel";
			this.MainDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 639, true);
			this.MainDetailsPanel.TabIndex = 1;
			this.MainDetailsPanel.TabStop = false;
			// 
			// LicenceDatabaseForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 770, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase);
			this.DataSourceTypeName = "Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 755, true);
			this.Name = "LicenceDatabaseForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Load += new System.EventHandler(this.LicenceDatabaseForm_Load);
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
			this.LicenceTypeDropEdit.ResumeLayout(true);
			this.LicenceTypeDropEdit.PerformLayout();
			this.ServerSecurityModeDropEdit.ResumeLayout(true);
			this.ServerSecurityModeDropEdit.PerformLayout();
			this.InstallationDetailsGroupBox.ResumeLayout(false);
			this.FeatureSetDropEdit.ResumeLayout(true);
			this.FeatureSetDropEdit.PerformLayout();
			this.InstallationDetailsGroupBox.PerformLayout();
			this.FlowLayoutPanel.ResumeLayout(false);
			this.FlowLayoutPanel.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			this.billableDropEdit.ResumeLayout(true);
			this.billableDropEdit.PerformLayout();
			this.ProductionDatabaseDropEdit.ResumeLayout(true);
			this.ProductionDatabaseDropEdit.PerformLayout();
			this.StaffCodeFindBox.ResumeLayout(true);
			this.StaffCodeFindBox.PerformLayout();
			this.LicenseeContactGuidFindBox.ResumeLayout(true);
			this.LicenseeContactGuidFindBox.PerformLayout();
			this.TechContactGuidFindBox.ResumeLayout(true);
			this.TechContactGuidFindBox.PerformLayout();
			this.productBox.ResumeLayout(true);
			this.productBox.PerformLayout();
			this.HostedLocationDropEdit.ResumeLayout(true);
			this.HostedLocationDropEdit.PerformLayout();
			this.ReleaseRingsDropEdit.ResumeLayout(true);
			this.ReleaseRingsDropEdit.PerformLayout();
			this.preRegistrationExpiryDateEdit.ResumeLayout(true);
			this.preRegistrationExpiryDateEdit.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.eAdaptorUrlBox.ResumeLayout(true);
			this.eAdaptorUrlBox.PerformLayout();
			this.registrationStatusBox.ResumeLayout(true);
			this.registrationStatusBox.PerformLayout();
			this.ContactDetailsGroupBox.ResumeLayout(false);
			this.ContactDetailsGroupBox.PerformLayout();
			this.LastHeartbeatDateEdit.ResumeLayout(true);
			this.LastHeartbeatDateEdit.PerformLayout();
			this.SoftwareInstallAddressDropEdit.ResumeLayout(true);
			this.SoftwareInstallAddressDropEdit.PerformLayout();
			this.UpgradeMethodDropEdit.ResumeLayout(true);
			this.UpgradeMethodDropEdit.PerformLayout();
			this.CurrentSentVersionGuidFindBox.ResumeLayout(true);
			this.CurrentSentVersionGuidFindBox.PerformLayout();
			this.CurrentRunningVersionGuidFindBox.ResumeLayout(true);
			this.CurrentRunningVersionGuidFindBox.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabControl1.PerformLayout();
			this.ConfigDetailsTabPage.ResumeLayout(false);
			this.ConfigDetailsTabPage.PerformLayout();
			this.currentVersionLastTime.ResumeLayout(true);
			this.currentVersionLastTime.PerformLayout();
			this.currentVersionFirstTime.ResumeLayout(true);
			this.currentVersionFirstTime.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.DocEngineStatsTabPage.ResumeLayout(false);
			this.DocEngineStatsTabPage.PerformLayout();
			this.sqlServerVersionDetailsTabPage.ResumeLayout(false);
			this.sqlServerVersionDetailsTabPage.PerformLayout();
			this.sqlServerEditionDropEdit.ResumeLayout(true);
			this.sqlServerEditionDropEdit.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			this.BIOSDateDateEdit.ResumeLayout(true);
			this.BIOSDateDateEdit.PerformLayout();
			this.TrustedMessagingTabPage.ResumeLayout(false);
			this.TrustedMessagingTabPage.PerformLayout();
			this.TrustedMessagingControl.ResumeLayout(true);
			this.TrustedMessagingControl.PerformLayout();
			this.ClientCompaniesTabPage.ResumeLayout(false);
			this.ClientCompaniesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientCompaniesGrid)).EndInit();
			this.ClientCompaniesGrid.ResumeLayout(false);
			this.ClientCompaniesGrid.PerformLayout();
			this.CompaniesTabPage.ResumeLayout(false);
			this.CompaniesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.UpgradeScheduleTabPage.ResumeLayout(false);
			this.UpgradeScheduleTabPage.PerformLayout();
			this.nextRunTimeUtcMUG.ResumeLayout(false);
			this.nextRunTimeUtcMUG.PerformLayout();
			this.nextRunTimeUtcUPG.ResumeLayout(false);
			this.nextRunTimeUtcUPG.PerformLayout();
			this.UserManagementTabPage.ResumeLayout(false);
			this.UserManagementTabPage.PerformLayout();
			this.StaffReportDateEdit.ResumeLayout(true);
			this.StaffReportDateEdit.PerformLayout();
			this.WebAccessOrgGuidFindBox.ResumeLayout(true);
			this.WebAccessOrgGuidFindBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MainDetailsPanel.ResumeLayout(false);
			this.MainDetailsPanel.PerformLayout();
			this.FeatureControlTabPage.ResumeLayout(false);
			this.FeatureControlTabPage.PerformLayout();
			this.FeatureControlRuleLastSyncUtcDateEdit.ResumeLayout(true);
			this.FeatureControlRuleLastSyncUtcDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZPanel MainDetailsPanel;
		private CargoWise.Windows.UI.KFlowLayoutPanel FlowLayoutPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox InstallationDetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox ContactDetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZTabPage ConfigDetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage tabPage2;
		Enterprise.ZArchitecture.GUI.ZTabPage DocEngineStatsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage sqlServerVersionDetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage CompaniesTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage UpgradeScheduleTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage zTabPage1;
		Enterprise.ZArchitecture.GUI.ZTabPage ClientCompaniesTabPage;
		Enterprise.ZArchitecture.ZLabel MinutesLabel;
		Enterprise.ZArchitecture.ZLabel MegabytesLabel;
		Enterprise.ZArchitecture.ZLabel zLabel10;
		Enterprise.ZArchitecture.ZLabel zLabel9;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZTextBox ServerCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox PublicEmailAddressTextBox;
		Enterprise.ZArchitecture.ZTextBox InternalSMTPEmailTextBox;
		Enterprise.ZArchitecture.ZTextBox InternalPOP3EmailTextBox;
		Enterprise.ZArchitecture.ZTextBox HostDBInstanceTextBox;
		Enterprise.ZArchitecture.ZTextBox HostServerNameTextBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		Enterprise.ZArchitecture.ZTextBox zTextBox2;
		Enterprise.ZArchitecture.ZTextBox zTextBox3;
		Enterprise.ZArchitecture.ZTextBox sqlServerFullVersionTextTextBox;
		Enterprise.ZArchitecture.ZTextBox sqlServerVersionTextBox;
		Enterprise.ZArchitecture.ZTextBox ProcessorSpeedTextBox;
		Enterprise.ZArchitecture.ZTextBox ProcessorTypeTextBox;
		Enterprise.ZArchitecture.ZTextBox NoOfProcessorsTextBox;
		Enterprise.ZArchitecture.ZTextBox TotalMemoryTextBox;
		Enterprise.ZArchitecture.ZTextBox SystemManufacturerTextBox;
		Enterprise.ZArchitecture.ZTextBox OSVersionTextBox;
		Enterprise.ZArchitecture.ZTextBox OSNameTextBox;
		Enterprise.ZArchitecture.ZTextBox DatabaseNumberBox;
		Enterprise.ZArchitecture.ZTextBox DatabaseIdBox;
		Enterprise.ZArchitecture.ZTextBox billingModelBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox4;
		Enterprise.ZArchitecture.ZTextBox eAdaptorUrlBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit LicenceTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit ServerSecurityModeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit UpgradeMethodDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit ReleaseRingsDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit HostedLocationDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit sqlServerEditionDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit productBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit registrationStatusBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit billableDropEdit;
		Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit ProductionDatabaseDropEdit;
		Enterprise.ZArchitecture.ZCalcEdit MaxEmailSizeLimitCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit MessageRetryTimeoutCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit2;
		Enterprise.ZArchitecture.ZCalcEdit NofActivePrintQueuesCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit PurchasedLicenceUnitsBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		Enterprise.ZArchitecture.GUI.ZDateEdit LastHeartbeatDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit BIOSDateDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit currentVersionFirstTime;
		Enterprise.ZArchitecture.GUI.ZDateEdit currentVersionLastTime;
		Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		Enterprise.ZArchitecture.GUI.ZDateEdit preRegistrationExpiryDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit nextRunTimeUtcMUG;
		Enterprise.ZArchitecture.GUI.ZDateEdit nextRunTimeUtcUPG;
		Enterprise.ZArchitecture.ZTextBox scheduleStateMUG;
		Enterprise.ZArchitecture.ZTextBox scheduleStateUPG;
		protected Enterprise.ZArchitecture.GUI.ZButton ResetHeartbeatButton;
		Enterprise.ZArchitecture.GUI.ZButton requestLicenceDatabaseLogsButton;
		Enterprise.ZArchitecture.GUI.ZButton manualExpiryButton;
		Enterprise.ZArchitecture.GUI.ZButton RequestVersionReportButton;
		Enterprise.ZArchitecture.GUI.ZButton requestLicenceUsageButton;
		CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel1;
		CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel2;
		protected CargoWise.Windows.UI.KPanel BottomPanel;
		CargoWise.Windows.UI.KPanel panel1;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl tabControl1;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox TechContactGuidFindBox;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox LicenseeContactGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox StaffCodeFindBox;
		Enterprise.ZArchitecture.ZTextBox TenantIDTextBox;
		Enterprise.ZArchitecture.GUI.ZGuidDropEdit SoftwareInstallAddressDropEdit;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox CurrentRunningVersionGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox CurrentSentVersionGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox activeCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsVirtualMachineCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox canReregisterCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox enablePackageDownloadOptimizationCheckBox;
		private ZArchitecture.GUI.ZCheckBox allowAutoLoginCheckBox;
		Enterprise.ZArchitecture.ZGrid zGrid1;
		Enterprise.ZArchitecture.ZGrid ClientCompaniesGrid;
		private ZArchitecture.GUI.ZButton requestStaffReportButton;
		private ZArchitecture.GUI.ZTabPage UserManagementTabPage;
		private ZArchitecture.GUI.ZTabPage TrustedMessagingTabPage;
		private ZArchitecture.GUI.ZDateEdit StaffReportDateEdit;
		protected ZArchitecture.GUI.ZGuidFindBox WebAccessOrgGuidFindBox;
		private EndpointManagement.GUI.EDITrustedMessagingControl TrustedMessagingControl;
		private ZArchitecture.ZTextBox zTextBox5;
		private ZArchitecture.ZTextBox zTextBox6;
		private ZArchitecture.ZTextBox zTextBox7;
		private ZArchitecture.GUI.ZTabPage FeatureControlTabPage;
		private ZArchitecture.GUI.ZDateEdit FeatureControlRuleLastSyncUtcDateEdit;
		private ZTextBox FeatureControlRuleLastSyncContentTextBox;
		private ZArchitecture.GUI.ZGuidDropEdit FeatureSetDropEdit;
	}
}
