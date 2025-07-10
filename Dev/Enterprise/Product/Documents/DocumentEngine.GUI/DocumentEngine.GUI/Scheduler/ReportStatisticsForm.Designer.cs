using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class ReportStatisticsForm : ZTemplateForm
	{
		ZTextBox runningServerTextBox;
		ZDropEdit statusDropEdit;
		ZCalcEdit rowsReturnedCalcEdit;
		ZButton openScheduledReportButton;
		ZTextBox reportDescriptionTextBox;
		ZDateEdit endDateEdit;
		ZDateEdit startDateEdit;
		ZDateEdit durationEdit;
		ZGuidFindBox printUserGuidFindBox;
		ZCalcEdit reportSizeBytesCalcEdit;
		KSplitContainer kSplitContainer2;
		ZGroupBox zGroupBox1;
		ZTextBox queryTextBox;
		ZGroupBox zGroupBox2;
		ZTextBox executionPlanTextBox;
		ZDateEdit zDateEdit5;
		ZDateEdit zDateEdit4;
		ZDateEdit zDateEdit3;
		ZDateEdit zDateEdit2;
		ZDateEdit zDateEdit1;
		ZTextBox zTextBox1;
		ZTextBox previewedTextBox;
		ZTextBox reportSourceTextBox;
		ZTextBox reportNameTextBox;

		new void InitializeComponent()
		{
			this.reportNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.reportDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.openScheduledReportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.rowsReturnedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.runningServerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.startDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.endDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.durationEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.printUserGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.reportSizeBytesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.kSplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.queryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.executionPlanTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit3 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit4 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit5 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.previewedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.reportSourceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.statusDropEdit.SuspendLayout();
			this.startDateEdit.SuspendLayout();
			this.endDateEdit.SuspendLayout();
			this.durationEdit.SuspendLayout();
			this.printUserGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).BeginInit();
			this.kSplitContainer2.Panel1.SuspendLayout();
			this.kSplitContainer2.Panel2.SuspendLayout();
			this.kSplitContainer2.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			this.zDateEdit3.SuspendLayout();
			this.zDateEdit4.SuspendLayout();
			this.zDateEdit5.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 531, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zTextBox1);
			this.MainTabPage.Controls.Add(this.zDateEdit5);
			this.MainTabPage.Controls.Add(this.zDateEdit4);
			this.MainTabPage.Controls.Add(this.zDateEdit3);
			this.MainTabPage.Controls.Add(this.zDateEdit2);
			this.MainTabPage.Controls.Add(this.zDateEdit1);
			this.MainTabPage.Controls.Add(this.reportSizeBytesCalcEdit);
			this.MainTabPage.Controls.Add(this.printUserGuidFindBox);
			this.MainTabPage.Controls.Add(this.durationEdit);
			this.MainTabPage.Controls.Add(this.endDateEdit);
			this.MainTabPage.Controls.Add(this.startDateEdit);
			this.MainTabPage.Controls.Add(this.runningServerTextBox);
			this.MainTabPage.Controls.Add(this.statusDropEdit);
			this.MainTabPage.Controls.Add(this.rowsReturnedCalcEdit);
			this.MainTabPage.Controls.Add(this.openScheduledReportButton);
			this.MainTabPage.Controls.Add(this.reportDescriptionTextBox);
			this.MainTabPage.Controls.Add(this.reportNameTextBox);
			this.MainTabPage.Controls.Add(this.kSplitContainer2);
			this.MainTabPage.Controls.Add(this.previewedTextBox);
			this.MainTabPage.Controls.Add(this.reportSourceTextBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 509, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 483, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 483, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 531, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Scheduler.Business.StmReportRun);
			// 
			// reportNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.reportNameTextBox, "RRI_ReportName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_ReportName)));
			this.reportNameTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ff4ab3d3-5963-4788-96b7-a60dcb7c6c71", "Report Name");
			this.reportNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.reportNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 14, true);
			this.reportNameTextBox.Name = "reportNameTextBox";
			this.reportNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.reportNameTextBox.TabIndex = 0;
			// 
			// reportDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.reportDescriptionTextBox, "RRI_ReportDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_ReportDescription)));
			this.reportDescriptionTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("247d20c3-ee06-489b-b31d-2d53169ce262", "Report Description");
			this.reportDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.reportDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 40, true);
			this.reportDescriptionTextBox.Name = "reportDescriptionTextBox";
			this.reportDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.reportDescriptionTextBox.TabIndex = 1;
			// 
			// openScheduledReportButton
			// 
			this.openScheduledReportButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("58f0c460-d730-4ee4-ae07-e876c06bf57b", "Open Scheduled Report");
			this.openScheduledReportButton.IsCaptionOverridden = false;
			this.openScheduledReportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 90, true);
			this.openScheduledReportButton.Name = "openScheduledReportButton";
			this.openScheduledReportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.openScheduledReportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 23, true);
			this.openScheduledReportButton.TabIndex = 3;
			this.openScheduledReportButton.ToolTipCaption = null;
			this.openScheduledReportButton.UseVisualStyleBackColor = true;
			this.openScheduledReportButton.Click += new System.EventHandler(this.openScheduledReportButton_Click);
			this.openScheduledReportButton.EditableInViewMode = true;
			// 
			// rowsReturnedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.rowsReturnedCalcEdit, "RRI_RowsReturned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_RowsReturned)));
			this.rowsReturnedCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("3f47f228-c356-4cdd-8cd1-2ecbb864fd62", "Rows Returned");
			this.rowsReturnedCalcEdit.DecimalPlaces = 0;
			this.rowsReturnedCalcEdit.Decimals = 0;
			this.rowsReturnedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 302, true);
			this.rowsReturnedCalcEdit.Name = "rowsReturnedCalcEdit";
			this.rowsReturnedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.rowsReturnedCalcEdit.TabIndex = 11;
			this.rowsReturnedCalcEdit.Text = "0";
			this.rowsReturnedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// statusDropEdit
			// 
			this.statusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusDropEdit, "RRI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_Status)));
			this.statusDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("fbbcf2c8-e12a-4d3a-815d-f832e8c4c9a2", "Status");
			this.statusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 119, true);
			this.statusDropEdit.Name = "statusDropEdit";
			this.statusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.statusDropEdit.TabIndex = 4;
			// 
			// runningServerTextBox
			// 
			this.BindingSource.SetBindingMember(this.runningServerTextBox, "RRI_RunningServer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_RunningServer)));
			this.runningServerTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("19517306-d04f-4d58-bffa-d716fd8c0249", "Running Server");
			this.runningServerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.runningServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 144, true);
			this.runningServerTextBox.Name = "runningServerTextBox";
			this.runningServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.runningServerTextBox.TabIndex = 5;
			// 
			// startDateEdit
			// 
			this.startDateEdit.AllowDrop = true;
			this.startDateEdit.AutoCompleteMonthThreshold = 1;
			this.startDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.startDateEdit, "StartTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).StartTimeLocal)));
			this.startDateEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("3921111d-7f8c-4359-98d8-0d94794be773", "Start Date");
			this.startDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.startDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 196, true);
			this.startDateEdit.Name = "startDateEdit";
			this.startDateEdit.TabIndex = 7;
			// 
			// endDateEdit
			// 
			this.endDateEdit.AllowDrop = true;
			this.endDateEdit.AutoCompleteMonthThreshold = 1;
			this.endDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.endDateEdit, "EndTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).EndTimeLocal)));
			this.endDateEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("0c5ff4a6-6c1b-4389-b7d8-340ccb11bfb2", "End Date");
			this.endDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.endDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 222, true);
			this.endDateEdit.Name = "endDateEdit";
			this.endDateEdit.TabIndex = 8;
			// 
			// durationEdit
			// 
			this.durationEdit.AllowDrop = true;
			this.durationEdit.AutoCompleteMonthThreshold = 1;
			this.durationEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.durationEdit, "Duration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).Duration)));
			this.durationEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("69ca0233-dfc3-43c1-8839-e11fe0fba8ee", "Duration");
			this.durationEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			this.durationEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 251, true);
			this.durationEdit.Name = "durationEdit";
			this.durationEdit.TabIndex = 9;
			// 
			// printUserGuidFindBox
			// 
			this.printUserGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.printUserGuidFindBox, "UserFK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Scheduler.Business.StmReportRun)(null)).UserFK)));
			this.printUserGuidFindBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ff60861a-ba3f-476b-8b1c-9f4472a7e881", "Print User");
			this.printUserGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.printUserGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 276, true);
			this.printUserGuidFindBox.Name = "printUserGuidFindBox";
			this.printUserGuidFindBox.ShouldResize = true;
			this.printUserGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.printUserGuidFindBox.TabIndex = 10;
			// 
			// reportSizeBytesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.reportSizeBytesCalcEdit, "RRI_ReportSizeBytes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_ReportSizeBytes)));
			this.reportSizeBytesCalcEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("088c9136-d4c4-43b5-9720-2522b846d3ea", "Report Size (Bytes)");
			this.reportSizeBytesCalcEdit.DecimalPlaces = 0;
			this.reportSizeBytesCalcEdit.Decimals = 0;
			this.reportSizeBytesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 328, true);
			this.reportSizeBytesCalcEdit.Name = "reportSizeBytesCalcEdit";
			this.reportSizeBytesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.reportSizeBytesCalcEdit.TabIndex = 12;
			this.reportSizeBytesCalcEdit.Text = "0";
			this.reportSizeBytesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// kSplitContainer2
			// 
			this.kSplitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.kSplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 6, true);
			this.kSplitContainer2.Name = "kSplitContainer2";
			this.kSplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer2.Panel1
			// 
			this.kSplitContainer2.Panel1.Controls.Add(this.zGroupBox1);
			// 
			// kSplitContainer2.Panel2
			// 
			this.kSplitContainer2.Panel2.Controls.Add(this.zGroupBox2);
			this.kSplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 430, true);
			this.kSplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(197);
			this.kSplitContainer2.TabIndex = 16;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("97899489-13af-4a40-81ad-dd277f79d1b4", "Query Plan");
			this.zGroupBox1.Controls.Add(this.queryTextBox);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 197, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// queryTextBox
			// 
			this.BindingSource.SetBindingMember(this.queryTextBox, "RRI_QueryText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_QueryText)));
			this.queryTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("97899489-13af-4a40-81ad-dd277f79d1b4", "Query Plan");
			this.queryTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.queryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.queryTextBox.Multiline = true;
			this.queryTextBox.Name = "queryTextBox";
			this.queryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.queryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 178, true);
			this.queryTextBox.TabIndex = 0;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("d2bbf159-c190-417b-90a3-0622299f8691", "Execution Plan");
			this.zGroupBox2.Controls.Add(this.executionPlanTextBox);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 229, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// executionPlanTextBox
			// 
			this.BindingSource.SetBindingMember(this.executionPlanTextBox, "RRI_ExecutionPlanText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_ExecutionPlanText)));
			this.executionPlanTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("d2bbf159-c190-417b-90a3-0622299f8691", "Execution Plan");
			this.executionPlanTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.executionPlanTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.executionPlanTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.executionPlanTextBox.Multiline = true;
			this.executionPlanTextBox.Name = "executionPlanTextBox";
			this.executionPlanTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.executionPlanTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 210, true);
			this.executionPlanTextBox.TabIndex = 0;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "SQLCPUDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).SQLCPUDuration)));
			this.zDateEdit1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("482f7ffa-2d03-4de4-a772-5dda0c7bf164", "SQL CPU Duration");
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 354, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 13;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "SQLRunDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).SQLRunDuration)));
			this.zDateEdit2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ba7f5145-1461-494f-92d1-af010c372528", "SQL Run Duration");
			this.zDateEdit2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 380, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 14;
			// 
			// zDateEdit3
			// 
			this.zDateEdit3.AllowDrop = true;
			this.zDateEdit3.AutoCompleteMonthThreshold = 1;
			this.zDateEdit3.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit3, "ClientCPUDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).ClientCPUDuration)));
			this.zDateEdit3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("c91ea10f-6dd6-43bc-a1b2-829712450054", "Client CPU Duration");
			this.zDateEdit3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			this.zDateEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 406, true);
			this.zDateEdit3.Name = "zDateEdit3";
			this.zDateEdit3.TabIndex = 15;
			// 
			// zDateEdit4
			// 
			this.zDateEdit4.AllowDrop = true;
			this.zDateEdit4.AutoCompleteMonthThreshold = 1;
			this.zDateEdit4.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit4, "ClientRunDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).ClientRunDuration)));
			this.zDateEdit4.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("3312afd2-1b17-4dc5-88c4-9434a2bbf0c2", "Client Run Duration");
			this.zDateEdit4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			this.zDateEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 432, true);
			this.zDateEdit4.Name = "zDateEdit4";
			this.zDateEdit4.TabIndex = 16;
			// 
			// zDateEdit5
			// 
			this.zDateEdit5.AllowDrop = true;
			this.zDateEdit5.AutoCompleteMonthThreshold = 1;
			this.zDateEdit5.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit5, "QueueDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmReportRun)(null)).QueueDuration)));
			this.zDateEdit5.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("fdddf8ee-5dbb-468a-b5b7-0693fc4ef1da", "Queue Duration");
			this.zDateEdit5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			this.zDateEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 458, true);
			this.zDateEdit5.Name = "zDateEdit5";
			this.zDateEdit5.TabIndex = 17;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "RRI_SQLServer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_SQLServer)));
			this.zTextBox1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("a4f08618-bb5f-4ec5-a74e-86d04441160d", "SQL Server");
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 170, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.zTextBox1.TabIndex = 6;
			// 
			// previewedTextBox
			// 
			this.BindingSource.SetBindingMember(this.previewedTextBox, "IsPreview");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Scheduler.Business.StmReportRun)(null)).IsPreview)));
			this.previewedTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("afb13028-6487-4981-881a-076ada7725e7", "Is Preview");
			this.previewedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.previewedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 484, true);
			this.previewedTextBox.Name = "previewedTextBox";
			this.previewedTextBox.ReadOnly = true;
			this.previewedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.previewedTextBox.TabIndex = 18;
			// 
			// reportSourceTextBox
			// 
			this.BindingSource.SetBindingMember(this.reportSourceTextBox, "RRI_IsSystemDefined");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Scheduler.Business.StmReportRun)(null)).RRI_IsSystemDefined)));
			this.reportSourceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.reportSourceTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("473707C1-3D29-462B-AD8A-1EBCBB2C3EFC", "System");
			this.reportSourceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 66, true);
			this.reportSourceTextBox.Name = "reportSourceTextBox";
			this.reportSourceTextBox.ReadOnly = true;
			this.reportSourceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.reportSourceTextBox.TabIndex = 2;
			// 
			// ReportStatisticsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("44d0a27e-1933-44fb-90d6-b4cac166a5ca", "Report Statistics");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 597, true);
			this.DataSourceAssemblyName = "Enterprise.Scheduler.Business";
			this.DataSourceType = typeof(Enterprise.Scheduler.Business.StmReportRun);
			this.DataSourceTypeName = "Enterprise.Scheduler.Business.StmReportRun";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(809, 598, true);
			this.Name = "ReportStatisticsForm";
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
			this.statusDropEdit.ResumeLayout(true);
			this.statusDropEdit.PerformLayout();
			this.startDateEdit.ResumeLayout(true);
			this.startDateEdit.PerformLayout();
			this.endDateEdit.ResumeLayout(true);
			this.endDateEdit.PerformLayout();
			this.durationEdit.ResumeLayout(true);
			this.durationEdit.PerformLayout();
			this.printUserGuidFindBox.ResumeLayout(true);
			this.printUserGuidFindBox.PerformLayout();
			this.kSplitContainer2.Panel1.ResumeLayout(false);
			this.kSplitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).EndInit();
			this.kSplitContainer2.ResumeLayout(false);
			this.kSplitContainer2.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			this.zDateEdit3.ResumeLayout(true);
			this.zDateEdit3.PerformLayout();
			this.zDateEdit4.ResumeLayout(true);
			this.zDateEdit4.PerformLayout();
			this.zDateEdit5.ResumeLayout(true);
			this.zDateEdit5.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
