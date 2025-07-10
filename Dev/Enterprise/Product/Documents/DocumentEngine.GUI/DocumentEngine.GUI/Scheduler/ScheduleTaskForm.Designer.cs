using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class ScheduleTaskForm : ZTemplateForm
	{
		ZGroupBox RecipientGroupBox;
		ZGrid RecipientsGrid;
		ZGroupBox OtherGroupBox;
		ZGuidFindBox MenuItemGuidFindBox;
		internal ZButton ChangeReportFilters;
		ZPanel TopPanel;
		ZPanel DescriptionPanel;
		ZDateEdit DateScheduleFirstRun;
		ZDateEdit NextScheduledPrintRunTime;
		ZCheckBox IsPrivate;
		ZCheckBox IsActive;
		ZCalcEdit ScheduleActualRunCount;
		ZTextBox Description;
		Enterprise.MasterFiles.GUI.Scheduler.RecurrenceControl RecurrenceControl;
		ZGuidFindBox BranchFindBox;
		ZGuidFindBox UserFindBox;
		ZTabPage ReportStatisticsTabPage;
		ReportStatisticsModuleButtonGrid ReportStatisticsModuleButtonGrid;

#if DEBUG
		public ZGuidFindBox GetUserFindBoxForTest()
		{
			return UserFindBox;
		}
#endif

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReportStatisticsModuleButtonGrid = new Enterprise.DocumentEngine.GUI.Scheduler.ReportStatisticsModuleButtonGrid();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.RecipientGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RecipientsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OtherGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MenuItemGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ChangeReportFilters = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DescriptionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.UserFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DateScheduleFirstRun = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.NextScheduledPrintRunTime = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IsPrivate = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActive = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ScheduleActualRunCount = new Enterprise.ZArchitecture.ZCalcEdit();
			this.Description = new Enterprise.ZArchitecture.ZTextBox();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RecurrenceControl = new Enterprise.MasterFiles.GUI.Scheduler.RecurrenceControl();
			this.ReportStatisticsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportStatisticsModuleButtonGrid.InnerGrid)).BeginInit();
			this.ReportStatisticsModuleButtonGrid.SuspendLayout();
			this.RecipientGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).BeginInit();
			this.RecipientsGrid.SuspendLayout();
			this.OtherGroupBox.SuspendLayout();
			this.MenuItemGuidFindBox.SuspendLayout();
			this.DescriptionPanel.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.UserFindBox.SuspendLayout();
			this.DateScheduleFirstRun.SuspendLayout();
			this.NextScheduledPrintRunTime.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.RecurrenceControl.SuspendLayout();
			this.ReportStatisticsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ReportStatisticsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 530, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ReportStatisticsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.RecipientGroupBox);
			this.MainTabPage.Controls.Add(this.OtherGroupBox);
			this.MainTabPage.Controls.Add(this.TopPanel);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 482, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 482, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 482, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 530, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(879);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskCollection);
			// 
			// ReportStatisticsModuleButtonGrid
			// 
			this.ReportStatisticsModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportStatisticsModuleButtonGrid, "StmReportRuns");
			this.ReportStatisticsModuleButtonGrid.BindToFindBoxList = "StmReportRuns";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).StmReportRuns)));
			this.ReportStatisticsModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportStatisticsModuleButtonGrid.GridId = "d55e49ef-c651-4d88-b524-755775210acd";
			// 
			// 
			// 
			this.ReportStatisticsModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ReportStatisticsModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ReportStatisticsModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ReportStatisticsModuleButtonGrid.InnerGrid.GridId = null;
			this.ReportStatisticsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportStatisticsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ReportStatisticsModuleButtonGrid.InnerGrid.LimitedColumns = null;
			this.ReportStatisticsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ReportStatisticsModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ReportStatisticsModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.ReportStatisticsModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ReportStatisticsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(962, 465, true);
			this.ReportStatisticsModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ReportStatisticsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportStatisticsModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ReportStatistics;
			this.ReportStatisticsModuleButtonGrid.Name = "ReportStatisticsModuleButtonGrid";
			this.ReportStatisticsModuleButtonGrid.NameOfAGridElement = Enterprise.DocumentEngine.GUI.Res.GetData("25d6250e-5c3d-45d6-adf6-b3cbda972577", "Report Statistics");
			this.ReportStatisticsModuleButtonGrid.ReadOnly = true;
			this.ReportStatisticsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 503, true);
			this.ReportStatisticsModuleButtonGrid.TabIndex = 1;
			// 
			// RecipientGroupBox
			// 
			this.RecipientGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|e4090686-5271-4498-a8eb-629c455d2044", "Recipients");
			this.RecipientGroupBox.Controls.Add(this.RecipientsGrid);
			this.RecipientGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipientGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 288, true);
			this.RecipientGroupBox.Name = "RecipientGroupBox";
			this.RecipientGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 135, true);
			this.RecipientGroupBox.TabIndex = 0;
			this.RecipientGroupBox.TabStop = false;
			// 
			// RecipientsGrid
			// 
			this.RecipientsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RecipientsGrid, "Recipients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).DeliveryRecipientType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).Lookups.DeliveryRecipientTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).ContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).Lookups.ContactNames)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_GS_NKRecipient)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_GG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_DeliveryMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).Lookups.NotifyModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_AttachmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).Lookups.AttachmentTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_SQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).Lookups.PrintersWithParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).DeliveryAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_EmptyReportDeliveryOptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).Lookups.BlankReportActivities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_FtpAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_UserName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).S6_Password)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskRecipient)(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recipients)).SyncRoot)).EmailFromAddress)));
			this.RecipientsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.DeliveryRecipientTypes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|f678d12c-87e9-4ebd-b6ef-902232de3098", "Deliver To");
			zDropEditColumnStyleInfo1.ColumnName = "DeliveryRecipientType";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "S6_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|80849c63-b3f9-4654-9325-d070068d8fc3", "Organization Name");
			zTextBoxColumnStyleInfo1.ColumnName = "OrganisationName";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.ContactNames";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|dcf5ccda-e6a9-4402-a47e-510dcf605c14", "Contact");
			zDropEditColumnStyleInfo2.ColumnName = "ContactName";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "S6_GS_NKRecipient";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "S6_GG";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.NotifyModes";
			zDropEditColumnStyleInfo3.ColumnName = "S6_DeliveryMethod";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zDropEditColumnStyleInfo4.BindToList = "Lookups.AttachmentTypes";
			zDropEditColumnStyleInfo4.ColumnName = "S6_AttachmentType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zGuidDropEditColumnStyleInfo1.BindToList = "Lookups.PrintersWithParent";
			zGuidDropEditColumnStyleInfo1.ColumnName = "S6_SQ";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|15ae6e23-82bd-41a6-8084-37981d76ed6f", "Delivery Address");
			zTextBoxColumnStyleInfo2.ColumnName = "DeliveryAddress";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo5.BindToList = "Lookups.BlankReportActivities";
			zDropEditColumnStyleInfo5.ColumnName = "S6_EmptyReportDeliveryOptions";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("97C08BAB-9780-43AB-BE50-691F458C3FFA", "FTP Address", "FTP server address, may include a path to a folder.");
			zTextBoxColumnStyleInfo3.ColumnName = "S6_FtpAddress";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("10f668e3-b18d-4945-aa3d-6bbeb0d5219e", "User Name", "User name for logging in to FTP server.");
			zTextBoxColumnStyleInfo4.ColumnName = "S6_UserName";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("3a7e5ff3-04c3-471d-bbc8-015ee673c035", "Password", "Password for logging in to FTP server.");
			zTextBoxColumnStyleInfo5.ColumnName = "S6_Password";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.PasswordChar = '*';
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.ColumnName = "EmailFromAddress";
			zDropEditColumnStyleInfo6.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|25E66A1B-564E-41D6-A0F5-71834ECB324B", "Send From");
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RecipientsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RecipientsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RecipientsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipientsGrid.GridId = "54dd80b8-cfca-4b43-ae35-83e136214e7c";
			this.RecipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecipientsGrid.LayoutKey = "zGrid1";
			this.RecipientsGrid.LimitedColumns = null;
			this.RecipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RecipientsGrid.Name = "RecipientsGrid";
			this.RecipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1132, 116, true);
			this.RecipientsGrid.TabIndex = 0;
			// 
			// OtherGroupBox
			// 
			this.OtherGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|06e5d8a7-3252-4f13-9c1f-4ce679bd6495", "Report Details");
			this.OtherGroupBox.Controls.Add(this.MenuItemGuidFindBox);
			this.OtherGroupBox.Controls.Add(this.ChangeReportFilters);
			this.OtherGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OtherGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 423, true);
			this.OtherGroupBox.Name = "OtherGroupBox";
			this.OtherGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 59, true);
			this.OtherGroupBox.TabIndex = 1;
			this.OtherGroupBox.TabStop = false;
			// 
			// MenuItemGuidFindBox
			// 
			this.MenuItemGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MenuItemGuidFindBox, "S5_ParentID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).S5_ParentID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Lookups.MenuItems)));
			this.MenuItemGuidFindBox.BindToList = "Lookups+MenuItems";
			this.MenuItemGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 22, true);
			this.MenuItemGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.StmMenuItem;
			this.MenuItemGuidFindBox.Name = "MenuItemGuidFindBox";
			this.MenuItemGuidFindBox.PreBoundMaxLength = 90;
			this.MenuItemGuidFindBox.ShowDescriptionBox = false;
			this.MenuItemGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 20, true);
			this.MenuItemGuidFindBox.TabIndex = 1;
			// 
			// ChangeReportFilters
			// 
			this.ChangeReportFilters.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|46c40f22-5771-4c09-8bb9-81424d58782b", "Change Report Filters");
			this.ChangeReportFilters.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(728, 19, true);
			this.ChangeReportFilters.Name = "ChangeReportFilters";
			this.ChangeReportFilters.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ChangeReportFilters.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 23, true);
			this.ChangeReportFilters.TabIndex = 2;
			this.ChangeReportFilters.ToolTipCaption = null;
			this.ChangeReportFilters.UseVisualStyleBackColor = true;
			this.ChangeReportFilters.Click += new System.EventHandler(this.ChangeReportFilters_Click);
			// 
			// DescriptionPanel
			// 
			this.DescriptionPanel.Controls.Add(this.BranchFindBox);
			this.DescriptionPanel.Controls.Add(this.UserFindBox);
			this.DescriptionPanel.Controls.Add(this.DateScheduleFirstRun);
			this.DescriptionPanel.Controls.Add(this.NextScheduledPrintRunTime);
			this.DescriptionPanel.Controls.Add(this.IsPrivate);
			this.DescriptionPanel.Controls.Add(this.IsActive);
			this.DescriptionPanel.Controls.Add(this.ScheduleActualRunCount);
			this.DescriptionPanel.Controls.Add(this.Description);
			this.DescriptionPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DescriptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DescriptionPanel.Name = "DescriptionPanel";
			this.DescriptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 93, true);
			this.DescriptionPanel.TabIndex = 0;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "S5_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).S5_GB)));
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 60, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.BranchFindBox.TabIndex = 11;
			// 
			// UserFindBox
			// 
			this.UserFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UserFindBox, "UserFK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).UserFK)));
			this.UserFindBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|ad77a208-db8a-4852-9b75-d35d278720d6", "User", "Print user", "A user, whose login will be used to print report.");
			this.UserFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 60, true);
			this.UserFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.UserFindBox.Name = "UserFindBox";
			this.UserFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.UserFindBox.TabIndex = 12;
			// 
			// DateScheduleFirstRun
			// 
			this.DateScheduleFirstRun.AllowDrop = true;
			this.DateScheduleFirstRun.AutoCompleteMonthThreshold = 1;
			this.DateScheduleFirstRun.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateScheduleFirstRun, "S5_DateScheduleFirstRun");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).S5_DateScheduleFirstRun)));
			this.DateScheduleFirstRun.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 34, true);
			this.DateScheduleFirstRun.Name = "DateScheduleFirstRun";
			this.DateScheduleFirstRun.TabIndex = 5;
			// 
			// NextScheduledPrintRunTime
			// 
			this.NextScheduledPrintRunTime.AllowDrop = true;
			this.NextScheduledPrintRunTime.AutoCompleteMonthThreshold = 1;
			this.NextScheduledPrintRunTime.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.NextScheduledPrintRunTime, "CalcNextRunTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).CalcNextRunTimeLocal)));
			this.NextScheduledPrintRunTime.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|d720f0b5-fab5-4a62-b02d-3a6ba171984a", "Next Run Time (local)");
			this.NextScheduledPrintRunTime.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.NextScheduledPrintRunTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 34, true);
			this.NextScheduledPrintRunTime.Name = "NextScheduledPrintRunTime";
			this.NextScheduledPrintRunTime.TabIndex = 7;
			// 
			// IsPrivate
			// 
			this.BindingSource.SetBindingMember(this.IsPrivate, "S5_IsPrivate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).S5_IsPrivate)));
			this.IsPrivate.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPrivate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 10, true);
			this.IsPrivate.Name = "IsPrivate";
			this.IsPrivate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 22, true);
			this.IsPrivate.TabIndex = 3;
			// 
			// IsActive
			// 
			this.BindingSource.SetBindingMember(this.IsActive, "S5_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).S5_IsActive)));
			this.IsActive.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActive.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 10, true);
			this.IsActive.Name = "IsActive";
			this.IsActive.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 22, true);
			this.IsActive.TabIndex = 2;
			// 
			// ScheduleActualRunCount
			// 
			this.BindingSource.SetBindingMember(this.ScheduleActualRunCount, "S5_ScheduleActualRunCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).S5_ScheduleActualRunCount)));
			this.ScheduleActualRunCount.DecimalPlaces = 2;
			this.ScheduleActualRunCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(737, 34, true);
			this.ScheduleActualRunCount.Name = "ScheduleActualRunCount";
			this.ScheduleActualRunCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.ScheduleActualRunCount.TabIndex = 9;
			this.ScheduleActualRunCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Description
			// 
			this.BindingSource.SetBindingMember(this.Description, "S5_ScheduleDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).S5_ScheduleDescription)));
			this.Description.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Description.SupportsMacroTemplates = true;
			this.Description.MacroFieldsOnly = true;
			this.Description.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 10, true);
			this.Description.Name = "Description";
			this.Description.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.Description.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.RecurrenceControl);
			this.TopPanel.Controls.Add(this.DescriptionPanel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 288, true);
			this.TopPanel.TabIndex = 24;
			// 
			// RecurrenceControl
			// 
			this.RecurrenceControl.AllowDrop = true;
			this.RecurrenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RecurrenceControl, "Recurrence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Scheduler.Business.StmScheduleTaskRecurrence)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(null)).Recurrence)));
			this.RecurrenceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 99, true);
			this.RecurrenceControl.Name = "RecurrenceControl";
			this.RecurrenceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 183, true);
			this.RecurrenceControl.TabIndex = 1;
			// 
			// ReportStatisticsTabPage
			// 
			this.ReportStatisticsTabPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|ReportStatisticsTabPage", "Report Statistics");
			this.ReportStatisticsTabPage.Controls.Add(this.ReportStatisticsModuleButtonGrid);
			this.ReportStatisticsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReportStatisticsTabPage.Name = "ReportStatisticsTabPage";
			this.ReportStatisticsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 503, true);
			this.ReportStatisticsTabPage.TabIndex = 0;
			// 
			// ScheduleTaskForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleTaskForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "Scheduled Report");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 586, true);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskCollection);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTaskCollection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 625, true);
			this.Name = "ScheduleTaskForm";
			this.ShouldSerializeTabPageMethods = false;
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
			((System.ComponentModel.ISupportInitialize)(this.ReportStatisticsModuleButtonGrid.InnerGrid)).EndInit();
			this.ReportStatisticsModuleButtonGrid.ResumeLayout(true);
			this.ReportStatisticsModuleButtonGrid.PerformLayout();
			this.RecipientGroupBox.ResumeLayout(false);
			this.RecipientGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).EndInit();
			this.RecipientsGrid.ResumeLayout(false);
			this.RecipientsGrid.PerformLayout();
			this.OtherGroupBox.ResumeLayout(false);
			this.OtherGroupBox.PerformLayout();
			this.MenuItemGuidFindBox.ResumeLayout(true);
			this.MenuItemGuidFindBox.PerformLayout();
			this.DescriptionPanel.ResumeLayout(false);
			this.DescriptionPanel.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.UserFindBox.ResumeLayout(true);
			this.UserFindBox.PerformLayout();
			this.DateScheduleFirstRun.ResumeLayout(true);
			this.DateScheduleFirstRun.PerformLayout();
			this.NextScheduledPrintRunTime.ResumeLayout(true);
			this.NextScheduledPrintRunTime.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.RecurrenceControl.ResumeLayout(true);
			this.RecurrenceControl.PerformLayout();
			this.ReportStatisticsTabPage.ResumeLayout(false);
			this.ReportStatisticsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
