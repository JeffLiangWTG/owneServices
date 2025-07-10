using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI
{
	partial class AccComplianceReportForm
	{
		private ZArchitecture.GUI.ZPanel contentPanel;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private Core.Forms.ZPostingButtonsUserControl postingButtonsControl;
		private ZArchitecture.GUI.ZButton generateButton;
		private ZArchitecture.GUI.ZButton reQueueButton;
		private ZArchitecture.GUI.ZButton generateXmlButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//protected override void Dispose(bool disposing)
		//{
		//	if (disposing && (components != null))
		//	{
		//		components.Dispose();
		//	}
		//	base.Dispose(disposing);
		//}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo19 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo20 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo21 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo22 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo23 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo24 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo25 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo26 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo27 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo28 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo29 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo29 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo30 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo30 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo31 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo32 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo33 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo34 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo35 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo36 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo31 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo32 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo37 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo38 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo39 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo40 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo41 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo42 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo43 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo33 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo34 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo44 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo45 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo46 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zTextBoxColumnStyleInfo45 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.contentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.tabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.reportTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.reportTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.reportLinesPreviousPeriodTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.reportTotalsPreviousPeriodGrid = new Enterprise.ZArchitecture.ZGrid();
			this.reportPreviousPeriodLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.reportLinesCurrentPeriodTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.reportTotalsCurrentPeriodGrid = new Enterprise.ZArchitecture.ZGrid();
			this.reportCurrentPeriodLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GLAccountsOpeningBalanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GLAccountsOpeningBalanceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GLAccountsMovementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GLAccountsMovementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GLAccountsClosingBalanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GLAccountsClosingBalanceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.headerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.pageToCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.pageFromCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dateToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.dateFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.periodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.reportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.periodicityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.stmNoteTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.eventTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.viewAndSubmitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.generateXmlButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.finaliseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.generateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.reQueueButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.postingButtonsControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.vatSummaryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ACR_ARB_ReportingBookGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.contentPanel.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.reportTabPage.SuspendLayout();
			this.reportTabControl.SuspendLayout();
			this.reportLinesPreviousPeriodTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportTotalsPreviousPeriodGrid)).BeginInit();
			this.reportTotalsPreviousPeriodGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportPreviousPeriodLinesGrid)).BeginInit();
			this.reportPreviousPeriodLinesGrid.SuspendLayout();
			this.reportLinesCurrentPeriodTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportTotalsCurrentPeriodGrid)).BeginInit();
			this.reportTotalsCurrentPeriodGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportCurrentPeriodLinesGrid)).BeginInit();
			this.reportCurrentPeriodLinesGrid.SuspendLayout();
			this.GLAccountsOpeningBalanceTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GLAccountsOpeningBalanceGrid)).BeginInit();
			this.GLAccountsOpeningBalanceGrid.SuspendLayout();
			this.GLAccountsMovementsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GLAccountsMovementsGrid)).BeginInit();
			this.GLAccountsMovementsGrid.SuspendLayout();
			this.GLAccountsClosingBalanceTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GLAccountsClosingBalanceGrid)).BeginInit();
			this.GLAccountsClosingBalanceGrid.SuspendLayout();
			this.headerPanel.SuspendLayout();
			this.statusDropEdit.SuspendLayout();
			this.dateToDateEdit.SuspendLayout();
			this.dateFromDateEdit.SuspendLayout();
			this.reportTypeDropEdit.SuspendLayout();
			this.periodicityDropEdit.SuspendLayout();
			this.workflowTabPage.SuspendLayout();
			this.stmNoteTabPage.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.postingButtonsControl.SuspendLayout();
			this.ACR_ARB_ReportingBookGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 659, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 22, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(416);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(416);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport);
			// 
			// contentPanel
			// 
			this.contentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.contentPanel.Controls.Add(this.tabControl);
			this.contentPanel.Controls.Add(this.bottomPanel);
			this.contentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 654, true);
			this.contentPanel.TabIndex = 0;
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tabControl.Controls.Add(this.reportTabPage);
			this.tabControl.Controls.Add(this.workflowTabPage);
			this.tabControl.Controls.Add(this.stmNoteTabPage);
			this.tabControl.Controls.Add(this.eventTabPage);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 617, true);
			this.tabControl.TabIndex = 0;
			// 
			// reportTabPage
			// 
			this.reportTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.reportTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c6c8d87e-5497-483f-8417-b250966b874f", "Report");
			this.reportTabPage.Controls.Add(this.reportTabControl);
			this.reportTabPage.Controls.Add(this.headerPanel);
			this.reportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.reportTabPage.Name = "reportTabPage";
			this.reportTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.reportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 590, true);
			this.reportTabPage.TabIndex = 0;
			// 
			// reportTabControl
			// 
			this.reportTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.reportTabControl.Controls.Add(this.reportLinesPreviousPeriodTabPage);
			this.reportTabControl.Controls.Add(this.reportLinesCurrentPeriodTabPage);
			this.reportTabControl.Controls.Add(this.GLAccountsOpeningBalanceTabPage);
			this.reportTabControl.Controls.Add(this.GLAccountsMovementsTabPage);
			this.reportTabControl.Controls.Add(this.GLAccountsClosingBalanceTabPage);
			this.reportTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reportTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 121, true);
			this.reportTabControl.Name = "reportTabControl";
			this.reportTabControl.SelectedIndex = 0;
			this.reportTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 466, true);
			this.reportTabControl.TabIndex = 0;
			// 
			// reportLinesPreviousPeriodTabPage
			// 
			this.reportLinesPreviousPeriodTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.reportLinesPreviousPeriodTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f0128d13-b955-4f01-8390-6d54beb7266b", "Report Lines Previous Period");
			this.reportLinesPreviousPeriodTabPage.Controls.Add(this.reportTotalsPreviousPeriodGrid);
			this.reportLinesPreviousPeriodTabPage.Controls.Add(this.reportPreviousPeriodLinesGrid);
			this.reportLinesPreviousPeriodTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.reportLinesPreviousPeriodTabPage.Name = "reportLinesPreviousPeriodTabPage";
			this.reportLinesPreviousPeriodTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.reportLinesPreviousPeriodTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 439, true);
			this.reportLinesPreviousPeriodTabPage.TabIndex = 0;
			// 
			// reportTotalsPreviousPeriodGrid
			// 
			this.reportTotalsPreviousPeriodGrid.AllowNavigation = false;
			this.reportTotalsPreviousPeriodGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.reportTotalsPreviousPeriodGrid, "ReportTotalsPreviousPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).GoodsExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).GoodsTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).ServiceExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).ServiceTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).TotalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).TotalTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).GeneralLedgerAmountDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).GeneralLedgerAmountCR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).TaxRecoverableAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).TaxNotRecoverableAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).TaxReverseChargeInputAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsPreviousPeriod)).SyncRoot)).TaxReverseChargeOutputAmount)));
			this.reportTotalsPreviousPeriodGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6d607d70-6224-473d-9e65-b19d942634a5", "Comment");
			zTextBoxColumnStyleInfo1.ColumnName = "Comment";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b50fcc38-2f0b-44d9-bc70-e069251162b0", "Total Goods Ex Tax");
			zCalcEditColumnStyleInfo1.ColumnName = "GoodsExTaxAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ee74d9ad-2851-491b-9726-1b0cc0a3f41e", "Total Goods Tax");
			zCalcEditColumnStyleInfo2.ColumnName = "GoodsTaxAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("317b2ff8-cc5e-4afb-a6d3-02cc14651903", "Total Service Ex Tax");
			zCalcEditColumnStyleInfo3.ColumnName = "ServiceExTaxAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("69f042cb-de50-478a-aef3-0bdb5697b259", "Total Service Tax");
			zCalcEditColumnStyleInfo4.ColumnName = "ServiceTaxAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("144d8a50-f8eb-4bfa-8bb7-0003f54c2698", "Total Ex Tax");
			zCalcEditColumnStyleInfo5.ColumnName = "TotalExTaxAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("11ea9247-fece-4883-9470-bda12dc68957", "Total Tax");
			zCalcEditColumnStyleInfo6.ColumnName = "TotalTaxAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "GeneralLedgerAmountDR";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "GeneralLedgerAmountCR";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3435dfb9-b783-4456-b44f-33b3243336b9", "Tax Recoverable Amount");
			zCalcEditColumnStyleInfo9.ColumnName = "TaxRecoverableAmount";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4ed7e0c4-6cb4-4714-855d-0d932940dbaa", "Tax Non-Recoverable Amount");
			zCalcEditColumnStyleInfo10.ColumnName = "TaxNotRecoverableAmount";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("773f018e-a392-43a0-b8ec-1c04c81db998", "Reverse Charge Input Tax Amount");
			zCalcEditColumnStyleInfo11.ColumnName = "TaxReverseChargeInputAmount";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("29b56aa9-9e3e-4fa9-a27b-5febcd7fc0b8", "Reverse Charge Output Tax Amount");
			zCalcEditColumnStyleInfo12.ColumnName = "TaxReverseChargeOutputAmount";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.reportTotalsPreviousPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.reportTotalsPreviousPeriodGrid.GridId = "8137b764-9cdb-4ab9-a7c8-313fd7d4dd87";
			this.reportTotalsPreviousPeriodGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.reportTotalsPreviousPeriodGrid.LayoutKey = "reportTotalsPreviousPeriodGrid";
			this.reportTotalsPreviousPeriodGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 377, true);
			this.reportTotalsPreviousPeriodGrid.Name = "reportTotalsPreviousPeriodGrid";
			this.reportTotalsPreviousPeriodGrid.ShouldSetErrorsOnTabPage = false;
			this.reportTotalsPreviousPeriodGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 56, true);
			this.reportTotalsPreviousPeriodGrid.TabIndex = 4;
			// 
			// reportPreviousPeriodLinesGrid
			// 
			this.reportPreviousPeriodLinesGrid.AllowNavigation = false;
			this.reportPreviousPeriodLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.reportPreviousPeriodLinesGrid, "ReportLinesPreviousPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).ACL_ReportSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).OK_CustomsRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).ComplianceSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AH_ComplianceSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AT_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AT_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).TaxMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).ReportSubCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AG_AccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AG_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).GB_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).GE_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).AH_TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).RepCountryRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).GoodsExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).GoodsTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).ServiceExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).ServiceTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).TotalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).TotalTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).GeneralLedgerAmountDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).GeneralLedgerAmountCR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).TaxRecoverableAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).TaxNotRecoverableAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).TaxReverseChargeInputAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesPreviousPeriod)).SyncRoot)).TaxReverseChargeOutputAmount)));
			this.reportPreviousPeriodLinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("790df306-38e7-4a06-82c3-0a8cce37fd89", "Line #");
			zCalcEditColumnStyleInfo13.ColumnName = "ACL_ReportSequence";
			zCalcEditColumnStyleInfo13.Decimals = 0;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("dedf6d86-195a-4ffa-9216-73ab2ca56bbb", "Org. Code");
			zTextBoxColumnStyleInfo2.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("49a75023-3783-41bc-8fde-a242ad4e2fa0", "Org. Name");
			zTextBoxColumnStyleInfo3.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9b7cf6cf-6788-4ead-9e52-280a527de27d", "Registration #");
			zTextBoxColumnStyleInfo4.ColumnName = "OK_CustomsRegNo";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("19441e1c-3e62-48df-819d-e8b3c23c2092", "Ledger");
			zTextBoxColumnStyleInfo5.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ca8ff332-95f4-435e-867f-6d1aa5487a4f", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "PostDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("92afaa66-6ebe-4761-9d60-489dc83ce219", "Transaction Date");
			zDateEditColumnStyleInfo2.ColumnName = "InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0dbe4d0f-6780-493d-8d23-5396e1763340", "Type");
			zTextBoxColumnStyleInfo6.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e4de9738-2a24-4272-9cd1-f4a3bbcbaada", "Compliance Sequence");
			zTextBoxColumnStyleInfo7.ColumnName = "ComplianceSequence";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1aff1fbe-b65a-433d-9424-92abc5d81a91", "Transaction #");
			zTextBoxColumnStyleInfo8.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fce19776-24ef-4226-8770-2ece3ea3b3fe", "Sub Type");
			zTextBoxColumnStyleInfo9.ColumnName = "AH_ComplianceSubType";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("83c57dd7-9c7d-4b29-8a41-5828a1a084ed", "Tax");
			zTextBoxColumnStyleInfo10.ColumnName = "AT_Code";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d62f9f48-ee25-400c-a3b6-f370314f2555", "Tax Type");
			zTextBoxColumnStyleInfo11.ColumnName = "AT_Type";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cd88ac84-c9f9-45f2-94f4-64ad7305b539", "Tax Message");
			zTextBoxColumnStyleInfo12.ColumnName = "TaxMessage";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8add5221-499d-4079-b20d-affa573303b0", "Sub Code");
			zTextBoxColumnStyleInfo13.ColumnName = "ReportSubCode";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("145d2ad5-b272-4bf4-aaa9-475b58f5c476", "GL Account");
			zTextBoxColumnStyleInfo14.ColumnName = "AG_AccountNum";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ccb3bf92-5294-494f-8fef-cf2066c606df", "Account Description");
			zTextBoxColumnStyleInfo15.ColumnName = "AG_Description";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b5e56399-4f59-46b8-a94f-8e951d11072d", "Branch");
			zTextBoxColumnStyleInfo16.ColumnName = "GB_Code";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f0f0e5c4-40bc-440d-8aa6-0e36b0056a30", "Department");
			zTextBoxColumnStyleInfo17.ColumnName = "GE_Code";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("abe3a41d-cca9-40d7-96ba-959b4954f4de", "Invoice Ref");
			zTextBoxColumnStyleInfo18.ColumnName = "AH_TransactionReference";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("403d7d55-5900-48ce-b8a3-0e5716b32d48", "2nd Registration #");
			zTextBoxColumnStyleInfo19.ColumnName = "RepCountryRegNo";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bef42003-78d4-4f85-8616-f7606d698b75", "Goods Ex Tax");
			zCalcEditColumnStyleInfo14.ColumnName = "GoodsExTaxAmount";
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5c88d93a-777f-4506-8e2d-12e1bf9c3ca8", "Goods Tax");
			zCalcEditColumnStyleInfo15.ColumnName = "GoodsTaxAmount";
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2c7eef3d-9c4e-4646-8a30-61b88f87c5a1", "Service Ex Tax");
			zCalcEditColumnStyleInfo16.ColumnName = "ServiceExTaxAmount";
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0994e1e8-ea96-4561-b682-acecc448d4ec", "Service Tax");
			zCalcEditColumnStyleInfo17.ColumnName = "ServiceTaxAmount";
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("eb653fcf-b477-445b-8d9b-04bb4edf5ad2", "Goods and Service Ex Tax");
			zCalcEditColumnStyleInfo18.ColumnName = "TotalExTaxAmount";
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo19.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cb3ac6ec-3b99-40f5-bda0-c9a3944b5fd3", "Goods and Service Tax");
			zCalcEditColumnStyleInfo19.ColumnName = "TotalTaxAmount";
			zCalcEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo20.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo20.ColumnName = "GeneralLedgerAmountDR";
			zCalcEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo21.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo21.ColumnName = "GeneralLedgerAmountCR";
			zCalcEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo22.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b4681423-2a33-429e-b426-400cbfffbf86", "Tax Recoverable Amount");
			zCalcEditColumnStyleInfo22.ColumnName = "TaxRecoverableAmount";
			zCalcEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo23.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo23.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("61d8c166-7762-45be-b35a-e6b474540e3f", "Tax Non-Recoverable Amount");
			zCalcEditColumnStyleInfo23.ColumnName = "TaxNotRecoverableAmount";
			zCalcEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo24.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo24.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("32887fc6-3886-43c0-8dbf-71d6388b166f", "Reverse Charge Input Tax Amount");
			zCalcEditColumnStyleInfo24.ColumnName = "TaxReverseChargeInputAmount";
			zCalcEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo25.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo25.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fa38d75e-0a9f-4ed7-bb1f-1af19156c5d7", "Reverse Charge Output Tax Amount");
			zCalcEditColumnStyleInfo25.ColumnName = "TaxReverseChargeOutputAmount";
			zCalcEditColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo44.ColumnName = "TaxGroupDescription";
			zTextBoxColumnStyleInfo44.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo44);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo19);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo20);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo21);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo22);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo23);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo24);
			this.reportPreviousPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo25);
			this.reportPreviousPeriodLinesGrid.GridId = "2baa63bd-cd1c-4ab7-b3ad-d7413d4dee4a";
			this.reportPreviousPeriodLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.reportPreviousPeriodLinesGrid.LayoutKey = "reportPreviousPeriodLinesGrid";
			this.reportPreviousPeriodLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.reportPreviousPeriodLinesGrid.Name = "reportPreviousPeriodLinesGrid";
			this.reportPreviousPeriodLinesGrid.ReadOnly = true;
			this.reportPreviousPeriodLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 368, true);
			this.reportPreviousPeriodLinesGrid.TabIndex = 3;
			// 
			// reportLinesCurrentPeriodTabPage
			// 
			this.reportLinesCurrentPeriodTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.reportLinesCurrentPeriodTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("571e663d-e07b-46d9-8afd-a2542b47c1f2", "Report Lines Current Period");
			this.reportLinesCurrentPeriodTabPage.Controls.Add(this.reportTotalsCurrentPeriodGrid);
			this.reportLinesCurrentPeriodTabPage.Controls.Add(this.reportCurrentPeriodLinesGrid);
			this.reportLinesCurrentPeriodTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.reportLinesCurrentPeriodTabPage.Name = "reportLinesCurrentPeriodTabPage";
			this.reportLinesCurrentPeriodTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.reportLinesCurrentPeriodTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 439, true);
			this.reportLinesCurrentPeriodTabPage.TabIndex = 0;
			// 
			// reportTotalsCurrentPeriodGrid
			// 
			this.reportTotalsCurrentPeriodGrid.AllowNavigation = false;
			this.reportTotalsCurrentPeriodGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.reportTotalsCurrentPeriodGrid, "ReportTotalsCurrentPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).GoodsExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).GoodsTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).ServiceExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).ServiceTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).TotalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).TotalTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).GeneralLedgerAmountDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).GeneralLedgerAmountCR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).TaxRecoverableAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).TaxNotRecoverableAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).TaxReverseChargeInputAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).TaxReverseChargeOutputAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLineBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportTotalsCurrentPeriod)).SyncRoot)).PreCalculatedAmount)));
			this.reportTotalsCurrentPeriodGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo26.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo26.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0b82139f-ded5-416b-8136-9cfdd74e4afa", "Pay Amt", "Pay Amount", "");
			zCalcEditColumnStyleInfo26.ColumnName = "PreCalculatedAmount";
			zCalcEditColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.reportTotalsCurrentPeriodGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo26);
			this.reportTotalsCurrentPeriodGrid.GridId = "7667b7f5-3156-4fc1-a5f5-e6af3fe96485";
			this.reportTotalsCurrentPeriodGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.reportTotalsCurrentPeriodGrid.LayoutKey = "reportTotalsCurrentPeriodGrid";
			this.reportTotalsCurrentPeriodGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 377, true);
			this.reportTotalsCurrentPeriodGrid.Name = "reportTotalsCurrentPeriodGrid";
			this.reportTotalsCurrentPeriodGrid.ShouldSetErrorsOnTabPage = false;
			this.reportTotalsCurrentPeriodGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 62, true);
			this.reportTotalsCurrentPeriodGrid.TabIndex = 4;
			// 
			// reportCurrentPeriodLinesGrid
			// 
			this.reportCurrentPeriodLinesGrid.AllowNavigation = false;
			this.reportCurrentPeriodLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.reportCurrentPeriodLinesGrid, "ReportLinesCurrentPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).ACL_ReportSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).OK_CustomsRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).ComplianceSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AH_ComplianceSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AT_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AT_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).TaxMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).ReportSubCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AG_AccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AG_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).GB_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).GE_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).AH_TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).RepCountryRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).GoodsExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).GoodsTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).ServiceExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).ServiceTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).TotalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).TotalTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).GeneralLedgerAmountDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).GeneralLedgerAmountCR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).TaxRecoverableAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).TaxNotRecoverableAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).TaxReverseChargeInputAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).TaxReverseChargeOutputAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).PreCalculatedAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).PreCalculatedCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).PreCalculatedCount2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).PreCalculatedCount3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).DaysRange)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).DaysRange30_60)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).IsSmallBusiness)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReportLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ReportLinesCurrentPeriod)).SyncRoot)).IsFullyPaid)));
			this.reportCurrentPeriodLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0467bd23-93e7-4e6f-ad61-ac58faba1b2c", "Compliance Sequence");
			zTextBoxColumnStyleInfo20.ColumnName = "ComplianceSequence";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("992fce63-3d23-4de6-bf3e-8a2dbc6ff898", "Tax Type");
			zTextBoxColumnStyleInfo21.ColumnName = "AT_Type";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo27.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo27.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7b9293d7-0499-457e-9895-0289e7f30e1f", "Pay Amt", "Payment Amount", "");
			zCalcEditColumnStyleInfo27.ColumnName = "PreCalculatedAmount";
			zCalcEditColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo28.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo28.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bcb73aed-c32b-4863-baf0-49f928d5fd26", "Payment Time");
			zCalcEditColumnStyleInfo28.ColumnName = "PreCalculatedCount";
			zCalcEditColumnStyleInfo28.Decimals = 0;
			zCalcEditColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCalcEditColumnStyleInfo45.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo45.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("A8A68785-3946-4131-A97B-ED6A514FE073", "On Time Days");
			zCalcEditColumnStyleInfo45.ColumnName = "PreCalculatedCount2";
			zCalcEditColumnStyleInfo45.Decimals = 0;
			zCalcEditColumnStyleInfo45.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCalcEditColumnStyleInfo46.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo46.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("24D94886-D122-4DF2-91C5-2F0FEE77A01D", "Payment Terms");
			zCalcEditColumnStyleInfo46.ColumnName = "PreCalculatedCount3";
			zCalcEditColumnStyleInfo46.Decimals = 0;
			zCalcEditColumnStyleInfo46.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fe2d2224-2596-4816-97f1-461a12ab9624", "Range", "Days Range", "");
			zTextBoxColumnStyleInfo22.ColumnName = "DaysRange";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo45.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f5b5088a-1f96-4de2-b71f-92e9d6ead8e9", "Range", "Days Range", "");
			zTextBoxColumnStyleInfo45.ColumnName = "DaysRange30_60";
			zTextBoxColumnStyleInfo45.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bb5f9fc3-2a1c-42a4-8ce8-505bb0656772", "Small Business");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSmallBusiness";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fdd0816e-0c3c-4be8-87a3-13ab165f4ff6", "Fully Paid");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsFullyPaid";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo44);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo19);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo20);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo21);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo22);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo23);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo24);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo25);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo27);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo28);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo45);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo46);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo45);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.reportCurrentPeriodLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.reportCurrentPeriodLinesGrid.GridId = "2d35ca8b-5932-449e-8580-4fc318a19d38";
			this.reportCurrentPeriodLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.reportCurrentPeriodLinesGrid.LayoutKey = "reportLinesGrid";
			this.reportCurrentPeriodLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.reportCurrentPeriodLinesGrid.Name = "reportCurrentPeriodLinesGrid";
			this.reportCurrentPeriodLinesGrid.ReadOnly = true;
			this.reportCurrentPeriodLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 368, true);
			this.reportCurrentPeriodLinesGrid.TabIndex = 3;
			// 
			// GLAccountsOpeningBalanceTabPage
			// 
			this.GLAccountsOpeningBalanceTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.GLAccountsOpeningBalanceTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d1ae315b-9846-4674-9f89-2f595fac371a", "GL Opening Balance");
			this.GLAccountsOpeningBalanceTabPage.Controls.Add(this.GLAccountsOpeningBalanceGrid);
			this.GLAccountsOpeningBalanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GLAccountsOpeningBalanceTabPage.Name = "GLAccountsOpeningBalanceTabPage";
			this.GLAccountsOpeningBalanceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GLAccountsOpeningBalanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 439, true);
			this.GLAccountsOpeningBalanceTabPage.TabIndex = 0;
			// 
			// GLAccountsOpeningBalanceGrid
			// 
			this.GLAccountsOpeningBalanceGrid.AllowNavigation = false;
			this.GLAccountsOpeningBalanceGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GLAccountsOpeningBalanceGrid, "GLOpeningBalanceDetailsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).AG_AccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).AG_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).AG_AccountType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).AG_DebitCredit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).AG_ConsolidationAccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).AG_ConsolidationDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).AG_ConsolidationAccountType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).GeneralLedgerAmountDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLOpeningBalanceDetailsView)).SyncRoot)).GeneralLedgerAmountCR)));
			this.GLAccountsOpeningBalanceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo23.ColumnName = "AG_AccountNum";
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo24.ColumnName = "AG_Description";
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zTextBoxColumnStyleInfo25.ColumnName = "AG_AccountType";
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo26.ColumnName = "AG_DebitCredit";
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo27.ColumnName = "AG_ConsolidationAccountNum";
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo28.ColumnName = "AG_ConsolidationDescription";
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo29.ColumnName = "AG_ConsolidationAccountType";
			zTextBoxColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zCalcEditColumnStyleInfo29.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo29.ColumnName = "GeneralLedgerAmountDR";
			zCalcEditColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo30.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo30.ColumnName = "GeneralLedgerAmountCR";
			zCalcEditColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo29);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo29);
			this.GLAccountsOpeningBalanceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo30);
			this.GLAccountsOpeningBalanceGrid.GridId = "c70c5a86-ce73-438b-910d-cebd89fed2b0";
			this.GLAccountsOpeningBalanceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GLAccountsOpeningBalanceGrid.LayoutKey = "GLAccountsOpeningBalanceGrid";
			this.GLAccountsOpeningBalanceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.GLAccountsOpeningBalanceGrid.Name = "GLAccountsOpeningBalanceGrid";
			this.GLAccountsOpeningBalanceGrid.ReadOnly = true;
			this.GLAccountsOpeningBalanceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 441, true);
			this.GLAccountsOpeningBalanceGrid.TabIndex = 3;
			// 
			// GLAccountsMovementsTabPage
			// 
			this.GLAccountsMovementsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.GLAccountsMovementsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("38290154-58df-4738-bbbb-536bf3cf3b3f", "GL Movements");
			this.GLAccountsMovementsTabPage.Controls.Add(this.GLAccountsMovementsGrid);
			this.GLAccountsMovementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GLAccountsMovementsTabPage.Name = "GLAccountsMovementsTabPage";
			this.GLAccountsMovementsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GLAccountsMovementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 439, true);
			this.GLAccountsMovementsTabPage.TabIndex = 0;
			// 
			// GLAccountsMovementsGrid
			// 
			this.GLAccountsMovementsGrid.AllowNavigation = false;
			this.GLAccountsMovementsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GLAccountsMovementsGrid, "GLMovementDetailsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).AG_AccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).AG_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).AG_AccountType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).AG_DebitCredit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).AG_ConsolidationAccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).AG_ConsolidationDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).AG_ConsolidationAccountType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).GeneralLedgerAmountDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLMovementDetailsView)).SyncRoot)).GeneralLedgerAmountCR)));
			this.GLAccountsMovementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo30.ColumnName = "AG_AccountNum";
			zTextBoxColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo31.ColumnName = "AG_Description";
			zTextBoxColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zTextBoxColumnStyleInfo32.ColumnName = "AG_AccountType";
			zTextBoxColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo33.ColumnName = "AG_DebitCredit";
			zTextBoxColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo34.ColumnName = "AG_ConsolidationAccountNum";
			zTextBoxColumnStyleInfo34.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo35.ColumnName = "AG_ConsolidationDescription";
			zTextBoxColumnStyleInfo35.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo36.ColumnName = "AG_ConsolidationAccountType";
			zTextBoxColumnStyleInfo36.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zCalcEditColumnStyleInfo31.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo31.ColumnName = "GeneralLedgerAmountDR";
			zCalcEditColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo32.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo32.ColumnName = "GeneralLedgerAmountCR";
			zCalcEditColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo30);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo31);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo32);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo33);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo34);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo35);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo36);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo31);
			this.GLAccountsMovementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo32);
			this.GLAccountsMovementsGrid.GridId = "f9c4ad6f-bfcd-42cc-9337-3298454e9262";
			this.GLAccountsMovementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GLAccountsMovementsGrid.LayoutKey = "GLAccountsMovementsGrid";
			this.GLAccountsMovementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.GLAccountsMovementsGrid.Name = "GLAccountsMovementsGrid";
			this.GLAccountsMovementsGrid.ReadOnly = true;
			this.GLAccountsMovementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 482, true);
			this.GLAccountsMovementsGrid.TabIndex = 3;
			// 
			// GLAccountsClosingBalanceTabPage
			// 
			this.GLAccountsClosingBalanceTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.GLAccountsClosingBalanceTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9c5ce148-a3d3-42da-b097-a4c5b71a7aee", "GL Closing Balance");
			this.GLAccountsClosingBalanceTabPage.Controls.Add(this.GLAccountsClosingBalanceGrid);
			this.GLAccountsClosingBalanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.GLAccountsClosingBalanceTabPage.Name = "GLAccountsClosingBalanceTabPage";
			this.GLAccountsClosingBalanceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GLAccountsClosingBalanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 449, true);
			this.GLAccountsClosingBalanceTabPage.TabIndex = 0;
			// 
			// GLAccountsClosingBalanceGrid
			// 
			this.GLAccountsClosingBalanceGrid.AllowNavigation = false;
			this.GLAccountsClosingBalanceGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GLAccountsClosingBalanceGrid, "GLClosingBalanceDetailsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).AG_AccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).AG_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).AG_AccountType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).AG_DebitCredit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).AG_ConsolidationAccountNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).AG_ConsolidationDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).AG_ConsolidationAccountType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).GeneralLedgerAmountDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.GeneralLedgerBalanceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).GLClosingBalanceDetailsView)).SyncRoot)).GeneralLedgerAmountCR)));
			this.GLAccountsClosingBalanceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo37.ColumnName = "AG_AccountNum";
			zTextBoxColumnStyleInfo37.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo38.ColumnName = "AG_Description";
			zTextBoxColumnStyleInfo38.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zTextBoxColumnStyleInfo39.ColumnName = "AG_AccountType";
			zTextBoxColumnStyleInfo39.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo40.ColumnName = "AG_DebitCredit";
			zTextBoxColumnStyleInfo40.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo41.ColumnName = "AG_ConsolidationAccountNum";
			zTextBoxColumnStyleInfo41.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo42.ColumnName = "AG_ConsolidationDescription";
			zTextBoxColumnStyleInfo42.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo43.ColumnName = "AG_ConsolidationAccountType";
			zTextBoxColumnStyleInfo43.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zCalcEditColumnStyleInfo33.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo33.ColumnName = "GeneralLedgerAmountDR";
			zCalcEditColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo34.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo34.ColumnName = "GeneralLedgerAmountCR";
			zCalcEditColumnStyleInfo34.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo37);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo38);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo39);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo40);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo41);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo42);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo43);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo33);
			this.GLAccountsClosingBalanceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo34);
			this.GLAccountsClosingBalanceGrid.GridId = "ae9e07b9-94ba-4712-ad61-23540a710000";
			this.GLAccountsClosingBalanceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GLAccountsClosingBalanceGrid.LayoutKey = "GLAccountsClosingBalanceGrid";
			this.GLAccountsClosingBalanceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.GLAccountsClosingBalanceGrid.Name = "GLAccountsClosingBalanceGrid";
			this.GLAccountsClosingBalanceGrid.ReadOnly = true;
			this.GLAccountsClosingBalanceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 427, true);
			this.GLAccountsClosingBalanceGrid.TabIndex = 3;
			// 
			// headerPanel
			//
			this.headerPanel.Controls.Add(this.ACR_ARB_ReportingBookGuidFindBox);
			this.headerPanel.Controls.Add(this.statusDropEdit);
			this.headerPanel.Controls.Add(this.pageToCalcEdit);
			this.headerPanel.Controls.Add(this.pageFromCalcEdit);
			this.headerPanel.Controls.Add(this.dateToDateEdit);
			this.headerPanel.Controls.Add(this.dateFromDateEdit);
			this.headerPanel.Controls.Add(this.periodCalcEdit);
			this.headerPanel.Controls.Add(this.reportTypeDropEdit);
			this.headerPanel.Controls.Add(this.periodicityDropEdit);
			this.headerPanel.Controls.Add(this.descriptionTextBox);
			this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 118, true);
			this.headerPanel.TabIndex = 2;
			// 
			// statusDropEdit
			// 
			this.statusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusDropEdit, "ACR_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_Status)));
			this.statusDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cae3ee04-2362-4a50-8ec1-cdb72a711f39", "Status");
			this.statusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 60, true);
			this.statusDropEdit.Name = "statusDropEdit";
			this.statusDropEdit.ShouldResizeByMaxLength = true;
			this.statusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.statusDropEdit.TabIndex = 8;
			// 
			// pageToCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.pageToCalcEdit, "ACR_PageNumberTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_PageNumberTo)));
			this.pageToCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("65927767-8bc2-4607-8d5e-c5b78ff58af7", "Page To");
			this.pageToCalcEdit.DecimalPlaces = 2;
			this.pageToCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 89, true);
			this.pageToCalcEdit.Name = "pageToCalcEdit";
			this.pageToCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.pageToCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.pageToCalcEdit.TabIndex = 7;
			this.pageToCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// pageFromCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.pageFromCalcEdit, "ACR_PageNumberFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_PageNumberFrom)));
			this.pageFromCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ca1f03d7-3c0c-4367-b7f1-81840b544fd5", "Page From");
			this.pageFromCalcEdit.DecimalPlaces = 2;
			this.pageFromCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 89, true);
			this.pageFromCalcEdit.Name = "pageFromCalcEdit";
			this.pageFromCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.pageFromCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 20, true);
			this.pageFromCalcEdit.TabIndex = 6;
			this.pageFromCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// dateToDateEdit
			// 
			this.dateToDateEdit.AllowDrop = true;
			this.dateToDateEdit.AutoCompleteMonthThreshold = 1;
			this.dateToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateToDateEdit, "ACR_DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_DateTo)));
			this.dateToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 86, true);
			this.dateToDateEdit.Name = "dateToDateEdit";
			this.dateToDateEdit.TabIndex = 5;
			// 
			// dateFromDateEdit
			// 
			this.dateFromDateEdit.AllowDrop = true;
			this.dateFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.dateFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateFromDateEdit, "ACR_DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_DateFrom)));
			this.dateFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 86, true);
			this.dateFromDateEdit.Name = "dateFromDateEdit";
			this.dateFromDateEdit.TabIndex = 4;
			// 
			// periodCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.periodCalcEdit, "AccountingPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).AccountingPeriod)));
			this.periodCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("76952415-2951-4701-be70-bcb1af20c791", "Period");
			this.periodCalcEdit.DecimalPlaces = 0;
			this.periodCalcEdit.Decimals = 0;
			this.periodCalcEdit.IsCalculatorEnabled = false;
			this.periodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 60, true);
			this.periodCalcEdit.Name = "periodCalcEdit";
			this.periodCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.periodCalcEdit.ShowGroupSeparators = false;
			this.periodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 20, true);
			this.periodCalcEdit.TabIndex = 3;
			this.periodCalcEdit.Text = "0";
			this.periodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// reportTypeDropEdit
			// 
			this.reportTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reportTypeDropEdit, "ACR_ReportType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_ReportType)));
			this.reportTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8a03d025-d398-4c3b-a405-d7a9a4a2f2e9", "Code");
			this.reportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 11, true);
			this.reportTypeDropEdit.Name = "reportTypeDropEdit";
			this.reportTypeDropEdit.ShouldResizeByMaxLength = true;
			this.reportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.reportTypeDropEdit.TabIndex = 0;
			// 
			// periodicityDropEdit
			// 
			this.periodicityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.periodicityDropEdit, "ACR_Periodicity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_Periodicity)));
			this.periodicityDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f6a5ef4c-db47-4439-9097-972ce4a8a601", "Periodicity");
			this.periodicityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 60, true);
			this.periodicityDropEdit.Name = "periodicityDropEdit";
			this.periodicityDropEdit.ShouldResizeByMaxLength = true;
			this.periodicityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.periodicityDropEdit.TabIndex = 2;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "ACR_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_Description)));
			this.descriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d2d167eb-46b4-49f4-a4e3-5bb621a31a42", "Description");
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 36, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 20, true);
			this.descriptionTextBox.TabIndex = 1;
			// 
			// workflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "workflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 590, true);
			this.workflowTabPage.TabIndex = 1;
			// 
			// stmNoteTabPage
			// 
			this.stmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.stmNoteTabPage.Name = "stmNoteTabPage";
			this.stmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 590, true);
			this.stmNoteTabPage.TabIndex = 2;
			// 
			// eventTabPage
			// 
			this.eventTabPage.ExcludeFromBindingOnSave = true;
			this.eventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.eventTabPage.Name = "eventTabPage";
			this.eventTabPage.ShouldBeReadOnlyInViewMode = false;
			this.eventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 590, true);
			this.eventTabPage.TabIndex = 3;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.viewAndSubmitButton);
			this.bottomPanel.Controls.Add(this.generateXmlButton);
			this.bottomPanel.Controls.Add(this.finaliseButton);
			this.bottomPanel.Controls.Add(this.generateButton);
			this.bottomPanel.Controls.Add(this.reQueueButton);
			this.bottomPanel.Controls.Add(this.postingButtonsControl);
			this.bottomPanel.Controls.Add(this.vatSummaryButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 617, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 37, true);
			this.bottomPanel.TabIndex = 5;
			// 
			// viewAndSubmitButton
			// 
			this.viewAndSubmitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.viewAndSubmitButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3f26dad1-8f90-48cf-9c83-d30665c3de75", "View and Submit");
			this.viewAndSubmitButton.IsCaptionOverridden = false;
			this.viewAndSubmitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 11, true);
			this.viewAndSubmitButton.Name = "viewAndSubmitButton";
			this.viewAndSubmitButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.viewAndSubmitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 21, true);
			this.viewAndSubmitButton.TabIndex = 3;
			this.viewAndSubmitButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.viewAndSubmitButton.ToolTipCaption = null;
			this.viewAndSubmitButton.UseVisualStyleBackColor = true;
			this.viewAndSubmitButton.Click += new System.EventHandler(this.ViewAndSubmitButton_Click);
			// 
			// generateXmlButton
			// 
			this.generateXmlButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.generateXmlButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c6adde8f-cea0-4703-ac3f-96f03215de4f", "Generate XML");
			this.generateXmlButton.IsCaptionOverridden = false;
			this.generateXmlButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 11, true);
			this.generateXmlButton.Name = "generateXmlButton";
			this.generateXmlButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.generateXmlButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 21, true);
			this.generateXmlButton.TabIndex = 4;
			this.generateXmlButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.generateXmlButton.ToolTipCaption = null;
			this.generateXmlButton.UseVisualStyleBackColor = true;
			// 
			// finaliseButton
			// 
			this.finaliseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.finaliseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("74d2b1b1-28a9-4b85-bc42-aeaa26c280c0", "Finalize");
			this.finaliseButton.IsCaptionOverridden = false;
			this.finaliseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(668, 11, true);
			this.finaliseButton.Name = "finaliseButton";
			this.finaliseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.finaliseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.finaliseButton.TabIndex = 3;
			this.finaliseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.finaliseButton.ToolTipCaption = null;
			this.finaliseButton.UseVisualStyleBackColor = true;
			this.finaliseButton.Click += new System.EventHandler(this.FinaliseButton_Click);
			// 
			// generateButton
			// 
			this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.generateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8629d9c8-4034-4c22-aa28-2ac391311c26", "Generate Report");
			this.generateButton.IsCaptionOverridden = false;
			this.generateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 11, true);
			this.generateButton.Name = "generateButton";
			this.generateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.generateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 21, true);
			this.generateButton.TabIndex = 2;
			this.generateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.generateButton.ToolTipCaption = null;
			this.generateButton.UseVisualStyleBackColor = true;
			this.generateButton.Click += new System.EventHandler(this.GenerateButton_Click);
			// 
			// reQueueButton
			// 
			this.reQueueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.reQueueButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0f7877f8-4201-42aa-a087-9c4184dc4f96", "Re-Queue");
			this.reQueueButton.IsCaptionOverridden = false;
			this.reQueueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 11, true);
			this.reQueueButton.Name = "reQueueButton";
			this.reQueueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.reQueueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 21, true);
			this.reQueueButton.TabIndex = 1;
			this.reQueueButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.reQueueButton.ToolTipCaption = null;
			this.reQueueButton.UseVisualStyleBackColor = true;
			this.reQueueButton.Click += new System.EventHandler(this.ReQueueButton_Click);
			// 
			// postingButtonsControl
			// 
			this.postingButtonsControl.AllowDrop = true;
			this.postingButtonsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(763, 8, true);
			this.postingButtonsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsControl.Name = "postingButtonsControl";
			this.postingButtonsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsControl.TabIndex = 3;
			// 
			// vatSummaryButton
			// 
			this.vatSummaryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.vatSummaryButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("63c69c9c-920d-49df-bf8d-1ae21d44a4e8", "VAT Summary Report");
			this.vatSummaryButton.IsCaptionOverridden = false;
			this.vatSummaryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 11, true);
			this.vatSummaryButton.Name = "vatSummaryButton";
			this.vatSummaryButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.vatSummaryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 21, true);
			this.vatSummaryButton.TabIndex = 5;
			this.vatSummaryButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.vatSummaryButton.ToolTipCaption = null;
			this.vatSummaryButton.UseVisualStyleBackColor = true;
			this.vatSummaryButton.Click += new System.EventHandler(this.vatSummaryButton_Click);
			//
			// ACR_ARB_ReportingBookGuidFindBox
			// 
			this.ACR_ARB_ReportingBookGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACR_ARB_ReportingBookGuidFindBox, "ACR_ARB_ReportingBook");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport)(null)).ACR_ARB_ReportingBook)));
			this.ACR_ARB_ReportingBookGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8a03d025-d398-4c3b-a405-d7a9a4a2f2e9", "Reporting Book");
			this.ACR_ARB_ReportingBookGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 11, true);
			this.ACR_ARB_ReportingBookGuidFindBox.Name = "ACR_ARB_ReportingBookGuidFindBox";
			this.ACR_ARB_ReportingBookGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.ACR_ARB_ReportingBookGuidFindBox.TabIndex = 9;
			// 
			// AccComplianceReportForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 681, true);
			this.Controls.Add(this.contentPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 720, true);
			this.Name = "AccComplianceReportForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.contentPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.contentPanel.ResumeLayout(false);
			this.contentPanel.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tabControl.PerformLayout();
			this.reportTabPage.ResumeLayout(false);
			this.reportTabPage.PerformLayout();
			this.reportTabControl.ResumeLayout(false);
			this.reportTabControl.PerformLayout();
			this.reportLinesPreviousPeriodTabPage.ResumeLayout(false);
			this.reportLinesPreviousPeriodTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportTotalsPreviousPeriodGrid)).EndInit();
			this.reportTotalsPreviousPeriodGrid.ResumeLayout(false);
			this.reportTotalsPreviousPeriodGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportPreviousPeriodLinesGrid)).EndInit();
			this.reportPreviousPeriodLinesGrid.ResumeLayout(false);
			this.reportPreviousPeriodLinesGrid.PerformLayout();
			this.reportLinesCurrentPeriodTabPage.ResumeLayout(false);
			this.reportLinesCurrentPeriodTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportTotalsCurrentPeriodGrid)).EndInit();
			this.reportTotalsCurrentPeriodGrid.ResumeLayout(false);
			this.reportTotalsCurrentPeriodGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.reportCurrentPeriodLinesGrid)).EndInit();
			this.reportCurrentPeriodLinesGrid.ResumeLayout(false);
			this.reportCurrentPeriodLinesGrid.PerformLayout();
			this.GLAccountsOpeningBalanceTabPage.ResumeLayout(false);
			this.GLAccountsOpeningBalanceTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GLAccountsOpeningBalanceGrid)).EndInit();
			this.GLAccountsOpeningBalanceGrid.ResumeLayout(false);
			this.GLAccountsOpeningBalanceGrid.PerformLayout();
			this.GLAccountsMovementsTabPage.ResumeLayout(false);
			this.GLAccountsMovementsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GLAccountsMovementsGrid)).EndInit();
			this.GLAccountsMovementsGrid.ResumeLayout(false);
			this.GLAccountsMovementsGrid.PerformLayout();
			this.GLAccountsClosingBalanceTabPage.ResumeLayout(false);
			this.GLAccountsClosingBalanceTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GLAccountsClosingBalanceGrid)).EndInit();
			this.GLAccountsClosingBalanceGrid.ResumeLayout(false);
			this.GLAccountsClosingBalanceGrid.PerformLayout();
			this.headerPanel.ResumeLayout(false);
			this.headerPanel.PerformLayout();
			this.statusDropEdit.ResumeLayout(true);
			this.statusDropEdit.PerformLayout();
			this.dateToDateEdit.ResumeLayout(true);
			this.dateToDateEdit.PerformLayout();
			this.dateFromDateEdit.ResumeLayout(true);
			this.dateFromDateEdit.PerformLayout();
			this.reportTypeDropEdit.ResumeLayout(true);
			this.reportTypeDropEdit.PerformLayout();
			this.periodicityDropEdit.ResumeLayout(true);
			this.periodicityDropEdit.PerformLayout();
			this.workflowTabPage.ResumeLayout(false);
			this.workflowTabPage.PerformLayout();
			this.stmNoteTabPage.ResumeLayout(false);
			this.stmNoteTabPage.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.postingButtonsControl.ResumeLayout(true);
			this.postingButtonsControl.PerformLayout();
			this.ACR_ARB_ReportingBookGuidFindBox.ResumeLayout(true);
			this.ACR_ARB_ReportingBookGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton finaliseButton;
		private ZArchitecture.GUI.ZButton viewAndSubmitButton;
		private ZArchitecture.GUI.ZLogsTabPage eventTabPage;
		private ZArchitecture.GUI.ZStmNoteTabPage stmNoteTabPage;
		private MasterFiles.GUI.ZWorkflowTabPage workflowTabPage;
		private ZArchitecture.GUI.ZTabPage reportTabPage;
		private ZArchitecture.ZGrid reportTotalsCurrentPeriodGrid;
		private ZArchitecture.ZGrid reportTotalsPreviousPeriodGrid;
		private ZArchitecture.ZGrid reportCurrentPeriodLinesGrid;
		private ZArchitecture.ZGrid reportPreviousPeriodLinesGrid;
		private ZArchitecture.ZGrid GLAccountsOpeningBalanceGrid;
		private ZArchitecture.ZGrid GLAccountsClosingBalanceGrid;
		private ZArchitecture.ZGrid GLAccountsMovementsGrid;
		private ZArchitecture.GUI.ZPanel headerPanel;
		private ZArchitecture.GUI.ZDropEdit statusDropEdit;
		private ZArchitecture.ZCalcEdit pageToCalcEdit;
		private ZArchitecture.ZCalcEdit pageFromCalcEdit;
		private ZArchitecture.GUI.ZDateEdit dateToDateEdit;
		private ZArchitecture.GUI.ZDateEdit dateFromDateEdit;
		private ZArchitecture.ZCalcEdit periodCalcEdit;
		private ZArchitecture.GUI.ZDropEdit reportTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit periodicityDropEdit;
		private ZArchitecture.ZTextBox descriptionTextBox;
		private ZArchitecture.GUI.ZTemplateTabControl tabControl;
		private ZArchitecture.GUI.ZTemplateTabControl reportTabControl;
		private ZArchitecture.GUI.ZTabPage reportLinesCurrentPeriodTabPage;
		private ZArchitecture.GUI.ZTabPage reportLinesPreviousPeriodTabPage;
		public ZArchitecture.GUI.ZTabPage GLAccountsOpeningBalanceTabPage;
		private ZArchitecture.GUI.ZTabPage GLAccountsClosingBalanceTabPage;
		private ZArchitecture.GUI.ZTabPage GLAccountsMovementsTabPage;
		private ZArchitecture.GUI.ZButton vatSummaryButton;
		private ZArchitecture.GUI.ZGuidFindBox ACR_ARB_ReportingBookGuidFindBox;
	}
}
