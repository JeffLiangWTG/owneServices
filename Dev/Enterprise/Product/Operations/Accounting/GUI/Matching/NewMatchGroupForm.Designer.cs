using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.Matching;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class NewMatchGroupForm
	{


		#region Windows Form Designer generated code

		internal ZButton CloseButton;
		internal ZButton MatchAndCloseButton;
		internal ZButton MatchAndContinueButton;
		internal ZButton SaveAsDraftButton;
		private ZCalcFindBox zCalcFindBox5;
		private ZButton MoveDownButton;
		private ZButton MoveUpButton;
		private ZGrid OrgInfoGrid;
		private ZButton SelectAllButton;
		private ZGuidFindBox PrimaryOrgGuidFindBox;
		private ZGroupBox MiscMatchingTransactionsGroupBox;
		private ZCalcFindBox zCalcFindBox4;
		private ZCalcFindBox zCalcFindBox3;
		private ZCalcFindBox zCalcFindBox2;
		private ZGroupBox MatchTransactionsGroupBox;
		private ZGrid MatchTransactionsGrid;
		private ZGroupBox UnmatchedTransactionsGroupBox;
		private ZDisplayGrid UnmatchedTransactionsGrid;
		private ZPanel TransactionsTopPanel;
		private ZButton UnselectAllButton;
		private ZTemplateTabControl MatchingTabControl;
		private ZButton OverpaymentButton;
		private ZButton DiscountButton;
		private ZButton ExchangeDiffButton;
		private ZGroupBox BalanceGroupBox;
		private ZTabPage GridsTabPage;
		private ZTabPage SettlementOrgsTabPage;
		private ZTabPage PrimaryOrganisationTabPage;
		private ZLabel FilterResultsLabel;
		private ZButton ChangePaymentAmountButton;
		private ZButton CurrencySummaryButton;
		private ZDisplayGrid CurrencySummaryGrid;
		private ZGroupBox CurrencySummaryGroupBox;
		private ZTemplateTabControl OrganisationTabControl;

		new void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZDateEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo9 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo10 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new ZDateEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo29 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo11 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo12 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo13 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo30 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo31 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo14 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo32 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo15 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo16 = new ZGuidFindBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo17 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo33 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo34 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo35 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo36 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo14 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo37 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo38 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo15 = new ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo18 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo19 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo39 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo40 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo41 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo42 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo43 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo44 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo20 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo45 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo46 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo47 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo19 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo20 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo21 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo22 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo23 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo24 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo25 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo26 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo27 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo28 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo29 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo30 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo31 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo32 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo33 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo48 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo49 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo50 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo51 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo52 = new ZTextBoxColumnStyleInfo();
			this.MatchDateGroupBox = new ZGroupBox();
			this.MatchDateEdit = new ZDateEdit();
			this.CloseButton = new ZButton();
			this.MatchAndCloseButton = new ZButton();
			this.MoveDownButton = new ZButton();
			this.MoveUpButton = new ZButton();
			this.MatchAndContinueButton = new ZButton();
			this.SaveAsDraftButton = new ZButton();
			this.zCalcFindBox5 = new ZCalcFindBox();
			this.UnselectAllButton = new ZButton();
			this.SelectAllButton = new ZButton();
			this.PrimaryOrgGuidFindBox = new ZGuidFindBox();
			this.MiscMatchingTransactionsGroupBox = new ZGroupBox();
			this.BankFeeButton = new ZButton();
			this.BankFeeCalcFindBox = new ZCalcFindBox();
			this.OverpaymentButton = new ZButton();
			this.zCalcFindBox4 = new ZCalcFindBox();
			this.zCalcFindBox3 = new ZCalcFindBox();
			this.zCalcFindBox2 = new ZCalcFindBox();
			this.DiscountButton = new ZButton();
			this.ExchangeDiffButton = new ZButton();
			this.MatchingTabControl = new ZTemplateTabControl();
			this.GridsTabPage = new ZTabPage();
			this.TransactionsTopPanel = new ZPanel();
			this.TransactionSearchGroupBox = new ZGroupBox();
			this.OutstandingTransactionsPanel = new ZPanel();
			this.OutstandingTransactionsGridPanel = new ZPanel();
			this.UnmatchedTransactionsGroupBox = new ZGroupBox();
			this.UnmatchedTransactionsGrid = new ZDisplayGrid();
			this.FilterResultsLabel = new ZLabel();
			this.CashAdvanceTabPage = new ZTabPage();
			this.CashAdvanceSplitContainer = new KSplitContainer();
			this.CashAdvanceSearchGroupBox = new ZGroupBox();
			this.RequestedCashAdvanceGroupBox = new ZGroupBox();
			this.UnmatchedCashAdvanceRequestsGrid = new ZDisplayGrid();
			this.APJournalsTabPage = new ZTabPage();
			this.APJournalsGrid = new ZGrid();
			this.APCoveringLabel = new ZLabel();
			this.ARJournalsTabPage = new ZTabPage();
			this.ARJournalsGrid = new ZGrid();
			this.ARCoveringLabel = new ZLabel();
			this.OrganisationTabControl = new ZTemplateTabControl();
			this.PrimaryOrganisationTabPage = new ZTabPage();
			this.SettlementOrgsTabPage = new ZTabPage();
			this.OrgInfoGrid = new ZGrid();
			this.SettlementOrgTabTopPanel = new ZPanel();
			this.ExpandViewButton = new ZButton();
			this.IncludeAllARTransactions = new ZCheckBox();
			this.IncludeAllAPTransactionsCheckBox = new ZCheckBox();
			this.MatchTransactionsGroupBox = new ZGroupBox();
			this.MatchTransactionsGrid = new ZGrid();
			this.BalanceGroupBox = new ZGroupBox();
			this.ChangePaymentAmountButton = new ZButton();
			this.CurrencySummaryButton = new ZButton();
			this.CurrencySummaryGrid = new ZDisplayGrid();
			this.CurrencySummaryGroupBox = new ZGroupBox();
			this.RightPanel = new ZPanel();
			this.MainSplitContainer = new KSplitContainer();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MatchDateGroupBox.SuspendLayout();
			this.MatchDateEdit.SuspendLayout();
			this.zCalcFindBox5.SuspendLayout();
			this.PrimaryOrgGuidFindBox.SuspendLayout();
			this.MiscMatchingTransactionsGroupBox.SuspendLayout();
			this.BankFeeCalcFindBox.SuspendLayout();
			this.zCalcFindBox4.SuspendLayout();
			this.zCalcFindBox3.SuspendLayout();
			this.zCalcFindBox2.SuspendLayout();
			this.MatchingTabControl.SuspendLayout();
			this.GridsTabPage.SuspendLayout();
			this.TransactionsTopPanel.SuspendLayout();
			this.TransactionSearchGroupBox.SuspendLayout();
			this.OutstandingTransactionsPanel.SuspendLayout();
			this.OutstandingTransactionsGridPanel.SuspendLayout();
			this.UnmatchedTransactionsGroupBox.SuspendLayout();
			((ISupportInitialize)(this.UnmatchedTransactionsGrid)).BeginInit();
			this.UnmatchedTransactionsGrid.SuspendLayout();
			this.CashAdvanceTabPage.SuspendLayout();
			((ISupportInitialize)(this.CashAdvanceSplitContainer)).BeginInit();
			this.CashAdvanceSplitContainer.Panel1.SuspendLayout();
			this.CashAdvanceSplitContainer.Panel2.SuspendLayout();
			this.CashAdvanceSplitContainer.SuspendLayout();
			this.RequestedCashAdvanceGroupBox.SuspendLayout();
			((ISupportInitialize)(this.UnmatchedCashAdvanceRequestsGrid)).BeginInit();
			this.UnmatchedCashAdvanceRequestsGrid.SuspendLayout();
			this.APJournalsTabPage.SuspendLayout();
			((ISupportInitialize)(this.APJournalsGrid)).BeginInit();
			this.APJournalsGrid.SuspendLayout();
			this.ARJournalsTabPage.SuspendLayout();
			((ISupportInitialize)(this.ARJournalsGrid)).BeginInit();
			this.ARJournalsGrid.SuspendLayout();
			this.OrganisationTabControl.SuspendLayout();
			this.PrimaryOrganisationTabPage.SuspendLayout();
			this.SettlementOrgsTabPage.SuspendLayout();
			((ISupportInitialize)(this.OrgInfoGrid)).BeginInit();
			this.OrgInfoGrid.SuspendLayout();
			this.SettlementOrgTabTopPanel.SuspendLayout();
			this.MatchTransactionsGroupBox.SuspendLayout();
			((ISupportInitialize)(this.MatchTransactionsGrid)).BeginInit();
			this.MatchTransactionsGrid.SuspendLayout();
			this.BalanceGroupBox.SuspendLayout();
			((ISupportInitialize)(this.CurrencySummaryGrid)).BeginInit();
			this.CurrencySummaryGrid.SuspendLayout();
			this.CurrencySummaryGroupBox.SuspendLayout();
			this.RightPanel.SuspendLayout();
			((ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 662, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1169, 24, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1052);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MatchingBase);
			// 
			// MatchDateGroupBox
			// 
			this.MatchDateGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f98bf29a-bc3b-4f4b-8d77-bb10c5bf6cb7", "Match Date");
			this.MatchDateGroupBox.Controls.Add(this.MatchDateEdit);
			this.MatchDateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 4, true);
			this.MatchDateGroupBox.Name = "MatchDateGroupBox";
			this.MatchDateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 44, true);
			this.MatchDateGroupBox.TabIndex = 1;
			this.MatchDateGroupBox.TabStop = false;
			// 
			// MatchDateEdit
			// 
			this.MatchDateEdit.AllowDrop = true;
			this.MatchDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.MatchDateEdit, "MatchDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((MatchingBase)(null)).MatchDate)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MatchDateEdit, false);
			this.MatchDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 16, true);
			this.MatchDateEdit.Name = "MatchDateEdit";
			this.MatchDateEdit.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|2750ab83-b977-4cb6-8036-ccca400b3cdd", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1093, 635, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.ToolTipCaption = null;
			// 
			// MatchAndCloseButton
			// 
			this.MatchAndCloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MatchAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(926, 635, true);
			this.MatchAndCloseButton.Name = "MatchAndCloseButton";
			this.MatchAndCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.MatchAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23, true);
			this.MatchAndCloseButton.TabIndex = 8;
			this.MatchAndCloseButton.ToolTipCaption = null;
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.MoveDownButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|dbbd6d2a-0f69-477c-9a9c-2a2bddb96f76", "Move Down");
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 322, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 50, true);
			this.MoveDownButton.TabIndex = 2;
			this.MoveDownButton.ToolTipCaption = null;
			this.MoveDownButton.Click += new EventHandler(this.MoveDownButton_Click);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.MoveUpButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|9dced0bf-6d2d-4a7f-8825-ea4294b47d73", "Move Up");
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 379, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 50, true);
			this.MoveUpButton.TabIndex = 3;
			this.MoveUpButton.ToolTipCaption = null;
			this.MoveUpButton.Click += new EventHandler(this.MoveUpButton_Click);
			// 
			// MatchAndContinueButton
			// 
			this.MatchAndContinueButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MatchAndContinueButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|cf425c94-d49c-48a4-9473-b9e7fc748855", "Match");
			this.MatchAndContinueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(823, 635, true);
			this.MatchAndContinueButton.Name = "MatchAndContinueButton";
			this.MatchAndContinueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.MatchAndContinueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.MatchAndContinueButton.TabIndex = 7;
			this.MatchAndContinueButton.ToolTipCaption = null;
			// 
			// SaveAsDraftButton
			// 
			this.SaveAsDraftButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAsDraftButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|ef301670-8d41-4649-9d72-b00be7477d14", "Save as Draft");
			this.SaveAsDraftButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(813, 635, true);
			this.SaveAsDraftButton.Name = "SaveAsDraftButton";
			this.SaveAsDraftButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.SaveAsDraftButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.SaveAsDraftButton.TabIndex = 6;
			this.SaveAsDraftButton.ToolTipCaption = null;
			// 
			// zCalcFindBox5
			// 
			this.zCalcFindBox5.AllowDrop = true;
			this.zCalcFindBox5.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zCalcFindBox5.BindToAmount = "Balance";
			this.zCalcFindBox5.BindToUnit = "LocalCurrency";
			this.zCalcFindBox5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|4c36f1c9-3630-4634-a40c-6f256f9f4705", "Balance");
			this.zCalcFindBox5.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcFindBox5, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcFindBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 34, true);
			this.zCalcFindBox5.Name = "zCalcFindBox5";
			this.zCalcFindBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.zCalcFindBox5.TabIndex = 0;
			// 
			// UnselectAllButton
			// 
			this.UnselectAllButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.UnselectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|edfbb8f0-5419-45dd-9f7c-3a0dec278f75", "Move All &Up");
			this.UnselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 436, true);
			this.UnselectAllButton.Name = "UnselectAllButton";
			this.UnselectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.UnselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 60, true);
			this.UnselectAllButton.TabIndex = 4;
			this.UnselectAllButton.ToolTipCaption = null;
			this.UnselectAllButton.Click += new EventHandler(this.UnselectAllButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.SelectAllButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|fb3c5a9e-e8c2-42c8-8965-081bcd124cf1", "Move All &Down");
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 256, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 60, true);
			this.SelectAllButton.TabIndex = 1;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.Click += new EventHandler(this.SelectAllButton_Click);
			// 
			// PrimaryOrgGuidFindBox
			// 
			this.PrimaryOrgGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrimaryOrgGuidFindBox, "PrimaryOrganisationForGUINotification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((MatchingBase)(null)).PrimaryOrganisationForGUINotification)));
			this.PrimaryOrgGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|fc78498f-c12a-4b50-97b2-be51645aca3f", "Primary Org.", "Primary Org.", "Primary Organization.");
			this.PrimaryOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 14, true);
			this.PrimaryOrgGuidFindBox.Name = "PrimaryOrgGuidFindBox";
			this.PrimaryOrgGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PrimaryOrgGuidFindBox.ParentType = null;
			this.PrimaryOrgGuidFindBox.PopupCaption = null;
			this.PrimaryOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 20, true);
			this.PrimaryOrgGuidFindBox.TabIndex = 0;
			// 
			// MiscMatchingTransactionsGroupBox
			// 
			this.MiscMatchingTransactionsGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MiscMatchingTransactionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|0dc0572b-3cff-4541-a11f-5d836da64388", "Miscellaneous Matching Transactions");
			this.MiscMatchingTransactionsGroupBox.Controls.Add(this.BankFeeButton);
			this.MiscMatchingTransactionsGroupBox.Controls.Add(this.BankFeeCalcFindBox);
			this.MiscMatchingTransactionsGroupBox.Controls.Add(this.OverpaymentButton);
			this.MiscMatchingTransactionsGroupBox.Controls.Add(this.zCalcFindBox4);
			this.MiscMatchingTransactionsGroupBox.Controls.Add(this.zCalcFindBox3);
			this.MiscMatchingTransactionsGroupBox.Controls.Add(this.zCalcFindBox2);
			this.MiscMatchingTransactionsGroupBox.Controls.Add(this.DiscountButton);
			this.MiscMatchingTransactionsGroupBox.Controls.Add(this.ExchangeDiffButton);
			this.MiscMatchingTransactionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 555, true);
			this.MiscMatchingTransactionsGroupBox.Name = "MiscMatchingTransactionsGroupBox";
			this.MiscMatchingTransactionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 70, true);
			this.MiscMatchingTransactionsGroupBox.TabIndex = 4;
			this.MiscMatchingTransactionsGroupBox.TabStop = false;
			// 
			// BankFeeButton
			// 
			this.BankFeeButton.AutoSize = true;
			this.BankFeeButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|437685b8-b87d-4e9b-9d88-9c4bc81aef72", "New");
			this.BankFeeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 32, true);
			this.BankFeeButton.Name = "BankFeeButton";
			this.BankFeeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.BankFeeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 23, true);
			this.BankFeeButton.TabIndex = 7;
			this.BankFeeButton.ToolTipCaption = null;
			this.BankFeeButton.Click += new EventHandler(this.NewBankFeeButton_Click);
			// 
			// BankFeeCalcFindBox
			// 
			this.BankFeeCalcFindBox.AllowDrop = true;
			this.BankFeeCalcFindBox.BindToAmount = "BankFeeAmount";
			this.BankFeeCalcFindBox.BindToUnit = "LocalCurrency";
			this.BankFeeCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|44cd8cda-2a45-4367-94ec-9623ff3ed35a", "Bank Fee", "Bank Fee", "Bank Fee", "");
			this.BankFeeCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.BankFeeCalcFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.BankFeeCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(582, 34, true);
			this.BankFeeCalcFindBox.Name = "BankFeeCalcFindBox";
			this.BankFeeCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.BankFeeCalcFindBox.TabIndex = 6;
			// 
			// OverpaymentButton
			// 
			this.OverpaymentButton.AutoSize = true;
			this.OverpaymentButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|876b7d0d-1610-42f8-8f4b-3e93c3c3b93c", "New");
			this.OverpaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 32, true);
			this.OverpaymentButton.Name = "OverpaymentButton";
			this.OverpaymentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.OverpaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 23, true);
			this.OverpaymentButton.TabIndex = 1;
			this.OverpaymentButton.ToolTipCaption = null;
			this.OverpaymentButton.Click += new EventHandler(this.NewOverpaymentButton_Click);
			// 
			// zCalcFindBox4
			// 
			this.zCalcFindBox4.AllowDrop = true;
			this.zCalcFindBox4.BindToAmount = "ExchangeDifferenceAmount";
			this.zCalcFindBox4.BindToUnit = "LocalCurrency";
			this.zCalcFindBox4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|a1c75eff-bcd9-426d-b573-bc35bc217ccc", "Exchange Gain/Loss");
			this.zCalcFindBox4.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcFindBox4, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcFindBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 34, true);
			this.zCalcFindBox4.Name = "zCalcFindBox4";
			this.zCalcFindBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.zCalcFindBox4.TabIndex = 4;
			// 
			// zCalcFindBox3
			// 
			this.zCalcFindBox3.AllowDrop = true;
			this.zCalcFindBox3.BindToAmount = "DiscountAmount";
			this.zCalcFindBox3.BindToUnit = "LocalCurrency";
			this.zCalcFindBox3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f0e500d1-80e7-4b8d-80da-70d854c1a326", "Discount");
			this.zCalcFindBox3.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcFindBox3, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcFindBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 34, true);
			this.zCalcFindBox3.Name = "zCalcFindBox3";
			this.zCalcFindBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.zCalcFindBox3.TabIndex = 2;
			// 
			// zCalcFindBox2
			// 
			this.zCalcFindBox2.AllowDrop = true;
			this.zCalcFindBox2.BindToAmount = "OSOverpaymentAmount";
			this.zCalcFindBox2.BindToUnit = "ForeignCurrency";
			this.zCalcFindBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|db241c48-186e-4bd8-95ec-7cb397ee6cd8", "Overpayment");
			this.zCalcFindBox2.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcFindBox2, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 34, true);
			this.zCalcFindBox2.Name = "zCalcFindBox2";
			this.zCalcFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.zCalcFindBox2.TabIndex = 0;
			// 
			// DiscountButton
			// 
			this.DiscountButton.AutoSize = true;
			this.DiscountButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|63823293-03df-4d5a-9862-e0e9257a73b7", "New");
			this.DiscountButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 32, true);
			this.DiscountButton.Name = "DiscountButton";
			this.DiscountButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.DiscountButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 23, true);
			this.DiscountButton.TabIndex = 3;
			this.DiscountButton.ToolTipCaption = null;
			this.DiscountButton.Click += new EventHandler(this.NewDiscountButton_Click);
			// 
			// ExchangeDiffButton
			// 
			this.ExchangeDiffButton.AutoSize = true;
			this.ExchangeDiffButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|82687528-36cf-422c-9c8e-0d41bb65c1f8", "New");
			this.ExchangeDiffButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 32, true);
			this.ExchangeDiffButton.Name = "ExchangeDiffButton";
			this.ExchangeDiffButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.ExchangeDiffButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 23, true);
			this.ExchangeDiffButton.TabIndex = 5;
			this.ExchangeDiffButton.ToolTipCaption = null;
			this.ExchangeDiffButton.Click += new EventHandler(this.NewExchangeDiffButton_Click);
			// 
			// MatchingTabControl
			// 
			this.MatchingTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MatchingTabControl.Controls.Add(this.GridsTabPage);
			this.MatchingTabControl.Controls.Add(this.CashAdvanceTabPage);
			this.MatchingTabControl.Controls.Add(this.APJournalsTabPage);
			this.MatchingTabControl.Controls.Add(this.ARJournalsTabPage);
			this.MatchingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MatchingTabControl.Name = "MatchingTabControl";
			this.MatchingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1162, 283, true);
			this.MatchingTabControl.TabIndex = 2;
			// 
			// GridsTabPage
			// 
			this.GridsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|2bddd33b-64e6-4c2d-aea3-d287a57c0da6", "Transactions");
			this.GridsTabPage.Controls.Add(this.TransactionsTopPanel);
			this.GridsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GridsTabPage.Name = "GridsTabPage";
			this.GridsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 256, true);
			this.GridsTabPage.TabIndex = 0;
			// 
			// TransactionsTopPanel
			// 
			this.TransactionsTopPanel.Controls.Add(this.TransactionSearchGroupBox);
			this.TransactionsTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionsTopPanel.Name = "TransactionsTopPanel";
			this.TransactionsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 256, true);
			this.TransactionsTopPanel.TabIndex = 2;
			// 
			// TransactionSearchGroupBox
			// 
			this.TransactionSearchGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("055c1ac2-030c-4ffa-a5fd-ebb51146fb9b", "Transaction Search");
			this.TransactionSearchGroupBox.Controls.Add(this.OutstandingTransactionsPanel);
			this.TransactionSearchGroupBox.Controls.Add(this.FilterResultsLabel);
			this.TransactionSearchGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionSearchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionSearchGroupBox.Name = "TransactionSearchGroupBox";
			this.TransactionSearchGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TransactionSearchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 256, true);
			this.TransactionSearchGroupBox.TabIndex = 0;
			this.TransactionSearchGroupBox.TabStop = false;
			// 
			// OutstandingTransactionsPanel
			// 
			this.OutstandingTransactionsPanel.Controls.Add(this.OutstandingTransactionsGridPanel);
			this.OutstandingTransactionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutstandingTransactionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 13, true);
			this.OutstandingTransactionsPanel.Name = "OutstandingTransactionsPanel";
			this.OutstandingTransactionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 243, true);
			this.OutstandingTransactionsPanel.TabIndex = 1;
			// 
			// OutstandingTransactionsGridPanel
			// 
			this.OutstandingTransactionsGridPanel.Controls.Add(this.UnmatchedTransactionsGroupBox);
			this.OutstandingTransactionsGridPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OutstandingTransactionsGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 59, true);
			this.OutstandingTransactionsGridPanel.Name = "OutstandingTransactionsGridPanel";
			this.OutstandingTransactionsGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 184, true);
			this.OutstandingTransactionsGridPanel.TabIndex = 11;
			// 
			// UnmatchedTransactionsGroupBox
			// 
			this.UnmatchedTransactionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|0633c648-5df2-421d-b27f-cc5c1417c75d", "Outstanding Transactions");
			this.UnmatchedTransactionsGroupBox.Controls.Add(this.UnmatchedTransactionsGrid);
			this.UnmatchedTransactionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnmatchedTransactionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnmatchedTransactionsGroupBox.Name = "UnmatchedTransactionsGroupBox";
			this.UnmatchedTransactionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 184, true);
			this.UnmatchedTransactionsGroupBox.TabIndex = 0;
			this.UnmatchedTransactionsGroupBox.TabStop = false;
			// 
			// UnmatchedTransactionsGrid
			// 
			this.UnmatchedTransactionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnmatchedTransactionsGrid, "UnmatchedTransactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).MatchDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).CurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).LoginCompanyCurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).ExchangeRateAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).ConsolidatedRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).InvoiceTransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).BranchGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).DepartmentGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).InvoiceBatchNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).VoyageVesselOrFlightDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).ShipmentHouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).ShipmentMasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).DisplayInvoiceAddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).RelatedClaimStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).QueryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).InvoiceRemittanceReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).LoginCompanyCurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).NotionalWHTTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedTransactions)).SyncRoot)).RealizedWHTTax)));
			this.UnmatchedTransactionsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.Caption = "";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f630df01-6b36-49cd-a6da-d1ef126ee187", "Organization");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Organisation";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|8edb298a-3ced-466f-9c98-accce4317c2d", "Ledger");
			zTextBoxColumnStyleInfo1.ColumnName = "Ledger";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|2a946eae-b444-4b70-bec2-78047e512a20", "Trans. Type");
			zTextBoxColumnStyleInfo2.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo52.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|8a153005-f1d9-48a6-bfc1-51e8a868125d", "Disbursement Relating To");
			zTextBoxColumnStyleInfo52.ColumnName = "RelatedDisbursementTransactions";
			zTextBoxColumnStyleInfo52.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|b305c9e2-6e8a-4331-9c89-2d39bcb249cc", "Transaction Num.", "Transaction Number");
			zTextBoxColumnStyleInfo3.ColumnName = "TransactionNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f7ed4085-428a-4eee-8de4-e40bad4d657f", "Check/Reference");
			zTextBoxColumnStyleInfo4.ColumnName = "ChequeOrReference";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|ea1e9fb5-2a13-4e80-876c-d1d471a4dbff", "Trans. Date");
			zDateEditColumnStyleInfo1.ColumnName = "InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|b9a328b4-615d-4e19-8e5c-00eee2cfd719", "Due Date");
			zDateEditColumnStyleInfo2.ColumnName = "DueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|9fce8ecd-f0bb-4863-bf94-092855863226", "Match Date");
			zDateEditColumnStyleInfo3.ColumnName = "MatchDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|88fae933-7a0e-42a1-b8e1-58583c64f94a", "Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CurrencyCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "CurrencyDecimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|336a81a6-9015-4c7c-94ff-e496fce1b17a", "Outstanding Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "OSOutstandingAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "LoginCompanyCurrencyDecimals";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f996df98-f21b-4920-a41a-e19bf9d8a930", "Local Outstanding Amt");
			zCalcEditColumnStyleInfo2.ColumnName = "OutstandingAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|6320adce-2741-4d1c-aedb-0aafae88491b", "Ex. Rate");
			zCalcEditColumnStyleInfo3.ColumnName = "ExchangeRateAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|72b6f95d-8210-43b4-ba65-6d058cf82af6", "Job Inv. Num.", "Job Invoice Number.");
			zTextBoxColumnStyleInfo5.ColumnName = "ConsolidatedRef";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|0935bda5-2b1b-4ef6-a609-4a9765d474a8", "Compliance Number");
			zTextBoxColumnStyleInfo6.ColumnName = "TransactionReference";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo47.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|262C4202-595B-4452-B098-F23341A37CB7", "Invoice Transaction Reference");
			zTextBoxColumnStyleInfo47.ColumnName = "InvoiceTransactionReference";
			zTextBoxColumnStyleInfo47.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|ad4022ad-b984-4268-ac48-fec47fbbe19d", "Post Date");
			zDateEditColumnStyleInfo4.ColumnName = "PostDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|e06ceb69-4175-4411-8ebf-135c010ee5b8", "Branch");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "BranchGuid";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|e0d7ebfd-cbe7-41fb-945b-dcfe3b156cf2", "Department");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "DepartmentGuid";
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|1710f5b8-b405-48a8-9994-07ccb4a7826a", "Description");
			zTextBoxColumnStyleInfo7.ColumnName = "Description";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|94fbcea1-e82f-4260-a0d7-2490c4dcee6c", "Inv. Batch Number", "Invoice Batch Number.");
			zTextBoxColumnStyleInfo8.ColumnName = "InvoiceBatchNumber";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|42b06264-8ff5-48a0-8fe7-9862dff45b28", "Flight/Date or Voyage/Vessel");
			zTextBoxColumnStyleInfo9.ColumnName = "VoyageVesselOrFlightDate";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|dec5b678-46ae-410c-9536-7093fbbeaf7d", "House Bill");
			zTextBoxColumnStyleInfo10.ColumnName = "ShipmentHouseBill";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|465007c0-5a46-49c2-8b34-841c2d46b747", "Master Bill");
			zTextBoxColumnStyleInfo11.ColumnName = "ShipmentMasterBill";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|e9e8acd4-f3f1-4623-8e12-b10db73261c3", "Category");
			zTextBoxColumnStyleInfo12.ColumnName = "TransactionCategory";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|37dd96ee-2586-42df-b421-84bfc35b1784", "Address");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "DisplayInvoiceAddressOverride";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e352679d-f043-4e3b-9b54-6a41e293b2a2", "Related Claim Status");
			zTextBoxColumnStyleInfo13.ColumnName = "RelatedClaimStatus";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("618f615b-567a-4538-b8eb-73048f4637b0", "Query Number");
			zTextBoxColumnStyleInfo14.ColumnName = "QueryNumber";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|85aac959-e8e3-4f80-a8df-753758285ce5", "Invoice Remittance Reference");
			zTextBoxColumnStyleInfo15.ColumnName = "InvoiceRemittanceReference";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = "LoginCompanyCurrencyDecimals";
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|7547bb72-9004-4034-a262-fe05c216d06d", "Notional WHT");
			zCalcEditColumnStyleInfo4.ColumnName = "NotionalWHTTax";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|303eba5e-0d4b-4add-a679-8296844fe0cb", "Realized WHT");
			zCalcEditColumnStyleInfo5.ColumnName = "RealizedWHTTax";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo52);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo47);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.UnmatchedTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.UnmatchedTransactionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnmatchedTransactionsGrid.GridId = "90d94c51-3ee6-45d6-b32a-788c2d1f62b4";
			this.UnmatchedTransactionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnmatchedTransactionsGrid.IsWholeRowSelectedOnClick = true;
			this.UnmatchedTransactionsGrid.LayoutKey = "UnmatchedTransactionsGrid";
			this.UnmatchedTransactionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.UnmatchedTransactionsGrid.Name = "UnmatchedTransactionsGrid";
			this.UnmatchedTransactionsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.UnmatchedTransactionsGrid.ShouldSetErrorsOnTabPage = false;
			this.UnmatchedTransactionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1148, 165, true);
			this.UnmatchedTransactionsGrid.TabIndex = 0;
			this.UnmatchedTransactionsGrid.DoubleClick += new EventHandler(this.UnmatchedTransactionsGrid_DoubleClick);
			// 
			// FilterResultsLabel
			// 
			this.FilterResultsLabel.AutoSize = true;
			this.FilterResultsLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FilterResultsLabel.ForeColor = System.Drawing.Color.Red;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FilterResultsLabel, false);
			this.FilterResultsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 137, true);
			this.FilterResultsLabel.Name = "FilterResultsLabel";
			this.FilterResultsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.FilterResultsLabel.TabIndex = 18;
			// 
			// CashAdvanceTabPage
			// 
			this.CashAdvanceTabPage.Controls.Add(this.CashAdvanceSplitContainer);
			this.CashAdvanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CashAdvanceTabPage.Name = "CashAdvanceTabPage";
			this.CashAdvanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 256, true);
			this.CashAdvanceTabPage.TabIndex = 5;
			this.CashAdvanceTabPage.Text = "Advance Payments";
			// 
			// CashAdvanceSplitContainer
			// 
			this.CashAdvanceSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CashAdvanceSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CashAdvanceSplitContainer.Name = "CashAdvanceSplitContainer";
			this.CashAdvanceSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// CashAdvanceSplitContainer.Panel1
			// 
			this.CashAdvanceSplitContainer.Panel1.Controls.Add(this.CashAdvanceSearchGroupBox);
			this.CashAdvanceSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 256, true);
			this.CashAdvanceSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			// 
			// CashAdvanceSplitContainer.Panel2
			// 
			this.CashAdvanceSplitContainer.Panel2.Controls.Add(this.RequestedCashAdvanceGroupBox);
			this.CashAdvanceSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(79);
			this.CashAdvanceSplitContainer.TabIndex = 0;
			// 
			// CashAdvanceSearchGroupBox
			// 
			this.CashAdvanceSearchGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7e3c0bbb-fe30-4963-b99d-7b0574e9443d", "Advance Payment Search");
			this.CashAdvanceSearchGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CashAdvanceSearchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CashAdvanceSearchGroupBox.Name = "CashAdvanceSearchGroupBox";
			this.CashAdvanceSearchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 79, true);
			this.CashAdvanceSearchGroupBox.TabIndex = 1;
			this.CashAdvanceSearchGroupBox.TabStop = false;
			// 
			// RequestedCashAdvanceGroupBox
			// 
			this.RequestedCashAdvanceGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4a05aa1b-d978-4af9-a89c-37346a90bb00", "Outstanding Advance Payment Requests");
			this.RequestedCashAdvanceGroupBox.Controls.Add(this.UnmatchedCashAdvanceRequestsGrid);
			this.RequestedCashAdvanceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequestedCashAdvanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequestedCashAdvanceGroupBox.Name = "RequestedCashAdvanceGroupBox";
			this.RequestedCashAdvanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 173, true);
			this.RequestedCashAdvanceGroupBox.TabIndex = 1;
			this.RequestedCashAdvanceGroupBox.TabStop = false;
			// 
			// UnmatchedCashAdvanceRequestsGrid
			// 
			this.UnmatchedCashAdvanceRequestsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnmatchedCashAdvanceRequestsGrid, "UnmatchedCashAdvanceRequests");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).OrganizationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).CAH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).CAH_RequestReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).CAH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).CAH_OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).CAH_LocalOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).CAH_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).JobBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).JobDepartment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).ShipmentMasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).ShipmentHouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((CashAdvanceRequestHeader)(((System.Collections.IList)(((MatchingBase)(null)).UnmatchedCashAdvanceRequests)).SyncRoot)).DebtorAddress)));
			this.UnmatchedCashAdvanceRequestsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b4cdec42-bd94-4dc9-9131-3931a3516c57", "Organization");
			zTextBoxColumnStyleInfo16.ColumnName = "OrganizationCode";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("dd36b68c-680f-46b1-8df7-434899a47d58", "Ledger");
			zTextBoxColumnStyleInfo17.ColumnName = "CAH_Ledger";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("48a567c5-08b6-4d86-b11d-becc0a79ee9c", "Request Id");
			zTextBoxColumnStyleInfo18.ColumnName = "CAH_RequestReferenceNumber";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d800a3ab-2474-4734-9403-0e71749728aa", "Currency");
			zTextBoxColumnStyleInfo19.ColumnName = "CAH_RX_NKTransactionCurrency";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("02b19cbb-d83b-4036-8114-78db7d0da7ae", "OS Out. Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "CAH_OSOutstandingAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f933f686-66ce-4d83-acb4-a184996c4cd0", "Local Out. Amount");
			zCalcEditColumnStyleInfo7.ColumnName = "CAH_LocalOutstandingAmount";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("898e514b-28e2-4d74-9820-ade1c63a4799", "Request Date (UTC)");
			zDateEditColumnStyleInfo5.ColumnName = "CAH_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c438dcc4-fb5f-4aa3-9456-f454dc6874dd", "Branch");
			zTextBoxColumnStyleInfo20.ColumnName = "JobBranch";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2129556b-7dbd-441b-a401-db0fc11bec97", "Department");
			zTextBoxColumnStyleInfo21.ColumnName = "JobDepartment";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cc4a3236-655f-4f5b-a668-0e1d54dea296", "Job Number");
			zTextBoxColumnStyleInfo22.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cac519bd-c7f6-45ff-bd31-6fd7bdbda353", "Master Bill");
			zTextBoxColumnStyleInfo23.ColumnName = "ShipmentMasterBill";
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0dc9c5f4-d5c0-4e48-9284-542c43e4a16a", "House Bill");
			zTextBoxColumnStyleInfo24.ColumnName = "ShipmentHouseBill";
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d1b6723f-b0b5-4c92-944c-72cec3b8dc15", "Address");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "DebtorAddress";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.UnmatchedCashAdvanceRequestsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.UnmatchedCashAdvanceRequestsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnmatchedCashAdvanceRequestsGrid.GridId = "7EFB2FBA-DBA2-4115-8D52-368178D2FCFD";
			this.UnmatchedCashAdvanceRequestsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnmatchedCashAdvanceRequestsGrid.LayoutKey = "zGrid1";
			this.UnmatchedCashAdvanceRequestsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.UnmatchedCashAdvanceRequestsGrid.Name = "UnmatchedCashAdvanceRequestsGrid";
			this.UnmatchedCashAdvanceRequestsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.UnmatchedCashAdvanceRequestsGrid.ShouldSetErrorsOnTabPage = false;
			this.UnmatchedCashAdvanceRequestsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1148, 154, true);
			this.UnmatchedCashAdvanceRequestsGrid.TabIndex = 1;
			this.UnmatchedCashAdvanceRequestsGrid.DoubleClick += new EventHandler(this.UnmatchedCashAdvanceRequestsGrid_DoubleClick);
			// 
			// APJournalsTabPage
			// 
			this.APJournalsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|afc9c046-6a0d-49ae-b65c-7629eb8d2b84", "AP Journals");
			this.APJournalsTabPage.Controls.Add(this.APJournalsGrid);
			this.APJournalsTabPage.Controls.Add(this.APCoveringLabel);
			this.APJournalsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.APJournalsTabPage.Name = "APJournalsTabPage";
			this.APJournalsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.APJournalsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 256, true);
			this.APJournalsTabPage.TabIndex = 2;
			this.APJournalsTabPage.UseVisualStyleBackColor = true;
			// 
			// APJournalsGrid
			// 
			this.APJournalsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.APJournalsGrid, "BalancingAPJournals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_AgreedPaymentMethodOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).ExchangeRate.Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).DebitCreditSign)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_AG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_Calc_FirstSubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_Calc_FirstSubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_Calc_SecondSubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AH_Calc_SecondSubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AlternateGLAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((APJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingAPJournals)).SyncRoot)).AlternateGLAccountDescription)));
			this.APJournalsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "AH_TransactionCategory";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo6.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo6.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo7.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo7.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo8.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo8.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|72DE91DF-B8A5-4314-9E55-45778CD2FFD8", "Agreed Payment Method");
			zDropEditColumnStyleInfo2.ColumnName = "AH_AgreedPaymentMethodOverride";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo25.ColumnName = "AH_Desc";
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|852ef602-60ce-4f69-8306-9b3257bd1f9d", "Exchange Rate");
			zCalcEditColumnStyleInfo8.ColumnName = "ExchangeRate+Rate";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "AH_OSExTaxAmount";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "AH_LocalExTaxAmount";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|50a6078e-7752-42c0-861a-be7e8e4e586e", "Debit Credit Sign");
			zDropEditColumnStyleInfo3.ColumnName = "DebitCreditSign";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "AH_AG";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.ColumnName = "AH_GB";
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo8.ColumnName = "AH_GE";
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo26.ColumnName = "AH_ChequeOrReference";
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo27.ColumnName = "AH_Calc_FirstSubClassParent";
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo9.ColumnName = "AH_Calc_FirstSubClassParentId";
			zGuidFindBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo28.ColumnName = "AH_Calc_SecondSubClassParent";
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo10.ColumnName = "AH_Calc_SecondSubClassParentId";
			zGuidFindBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo48.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|C7B6912D-FCC9-405A-B32A-F20F036D7D2B", "Alternate Account");
			zTextBoxColumnStyleInfo48.ColumnName = "AlternateGLAccountNumber";
			zTextBoxColumnStyleInfo48.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo49.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|10A594AC-C37B-477A-B670-62D9AFD35FD1", "Alternate Account Name");
			zTextBoxColumnStyleInfo49.ColumnName = "AlternateGLAccountDescription";
			zTextBoxColumnStyleInfo49.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.APJournalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.APJournalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.APJournalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.APJournalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.APJournalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.APJournalsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.APJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.APJournalsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.APJournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.APJournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.APJournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.APJournalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.APJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.APJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.APJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.APJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.APJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.APJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo9);
			this.APJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.APJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo10);
			this.APJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo48);
			this.APJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo49);
			this.APJournalsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APJournalsGrid.GridId = "c1abe43b-37ba-4b46-a6b0-33e1d2bf51e8";
			this.APJournalsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.APJournalsGrid.LayoutKey = "zGrid1";
			this.APJournalsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.APJournalsGrid.Name = "APJournalsGrid";
			this.APJournalsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1148, 250, true);
			this.APJournalsGrid.TabIndex = 14;
			// 
			// APCoveringLabel
			// 
			this.APCoveringLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.APCoveringLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.APCoveringLabel.ForeColor = System.Drawing.Color.Black;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.APCoveringLabel, false);
			this.APCoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.APCoveringLabel.Name = "APCoveringLabel";
			this.APCoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1148, 250, true);
			this.APCoveringLabel.TabIndex = 13;
			this.APCoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ARJournalsTabPage
			// 
			this.ARJournalsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|13ab59ec-5a22-47da-8f76-b9d87e046789", "AR Journals");
			this.ARJournalsTabPage.Controls.Add(this.ARJournalsGrid);
			this.ARJournalsTabPage.Controls.Add(this.ARCoveringLabel);
			this.ARJournalsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ARJournalsTabPage.Name = "ARJournalsTabPage";
			this.ARJournalsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ARJournalsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1154, 256, true);
			this.ARJournalsTabPage.TabIndex = 3;
			this.ARJournalsTabPage.UseVisualStyleBackColor = true;
			// 
			// ARJournalsGrid
			// 
			this.ARJournalsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ARJournalsGrid, "BalancingARJournals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_AgreedPaymentMethodOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).ExchangeRate.Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_LocalExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).DebitCreditSign)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_AG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_Calc_FirstSubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_Calc_FirstSubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_Calc_SecondSubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AH_Calc_SecondSubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AlternateGLAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ARJournal)(((System.Collections.IList)(((MatchingBase)(null)).BalancingARJournals)).SyncRoot)).AlternateGLAccountDescription)));
			this.ARJournalsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.ColumnName = "AH_TransactionCategory";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo9.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo9.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo10.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo10.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo11.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo11.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("72DE91DF-B8A5-4314-9E55-45778CD2FFD8", "Agreed Payment Method");
			zDropEditColumnStyleInfo5.ColumnName = "AH_AgreedPaymentMethodOverride";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo29.ColumnName = "AH_Desc";
			zTextBoxColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|39ca3e2b-cda8-4dd2-aafe-ab9988b6a70c", "Exchange Rate");
			zCalcEditColumnStyleInfo11.ColumnName = "ExchangeRate+Rate";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "AH_OSExTaxAmount";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "AH_LocalExTaxAmount";
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|50a6078e-7752-42c0-861a-be7e8e4e586e", "Debit Credit Sign");
			zDropEditColumnStyleInfo6.ColumnName = "DebitCreditSign";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo11.ColumnName = "AH_AG";
			zGuidFindBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo12.ColumnName = "AH_GB";
			zGuidFindBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo13.ColumnName = "AH_GE";
			zGuidFindBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo30.ColumnName = "AH_ChequeOrReference";
			zTextBoxColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo31.ColumnName = "AH_Calc_FirstSubClassParent";
			zTextBoxColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo14.ColumnName = "AH_Calc_FirstSubClassParentId";
			zGuidFindBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo32.ColumnName = "AH_Calc_SecondSubClassParent";
			zTextBoxColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo15.ColumnName = "AH_Calc_SecondSubClassParentId";
			zGuidFindBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo50.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|97DAC0D3-380D-4317-972A-0B2285AF0174", "Alternate Account");
			zTextBoxColumnStyleInfo50.ColumnName = "AlternateGLAccountNumber";
			zTextBoxColumnStyleInfo50.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo51.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|2C5CF2CD-A8E8-43D0-87C9-971E990CD7FB", "Alternate Account Name");
			zTextBoxColumnStyleInfo51.ColumnName = "AlternateGLAccountDescription";
			zTextBoxColumnStyleInfo51.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ARJournalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ARJournalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.ARJournalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.ARJournalsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.ARJournalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ARJournalsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.ARJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo29);
			this.ARJournalsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ARJournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.ARJournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.ARJournalsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.ARJournalsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ARJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo11);
			this.ARJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo12);
			this.ARJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo13);
			this.ARJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo30);
			this.ARJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo31);
			this.ARJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo14);
			this.ARJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo32);
			this.ARJournalsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo15);
			this.ARJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo50);
			this.ARJournalsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo51);
			this.ARJournalsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ARJournalsGrid.GridId = "dc34aecc-d0ce-4da0-b391-96798be23e45";
			this.ARJournalsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ARJournalsGrid.LayoutKey = "zGrid1";
			this.ARJournalsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ARJournalsGrid.Name = "ARJournalsGrid";
			this.ARJournalsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1148, 250, true);
			this.ARJournalsGrid.TabIndex = 14;
			// 
			// ARCoveringLabel
			// 
			this.ARCoveringLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ARCoveringLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ARCoveringLabel.ForeColor = System.Drawing.Color.Black;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ARCoveringLabel, false);
			this.ARCoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ARCoveringLabel.Name = "ARCoveringLabel";
			this.ARCoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1148, 250, true);
			this.ARCoveringLabel.TabIndex = 13;
			this.ARCoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// OrganisationTabControl
			// 
			this.OrganisationTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.OrganisationTabControl.Controls.Add(this.PrimaryOrganisationTabPage);
			this.OrganisationTabControl.Controls.Add(this.SettlementOrgsTabPage);
			this.OrganisationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.OrganisationTabControl.Name = "OrganisationTabControl";
			this.OrganisationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 99, true);
			this.OrganisationTabControl.TabIndex = 1;
			// 
			// PrimaryOrganisationTabPage
			// 
			this.PrimaryOrganisationTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f337fe4b-9ce8-4da9-841f-a7b48f11a482", "Primary Organization");
			this.PrimaryOrganisationTabPage.Controls.Add(this.PrimaryOrgGuidFindBox);
			this.PrimaryOrganisationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PrimaryOrganisationTabPage.Name = "PrimaryOrganisationTabPage";
			this.PrimaryOrganisationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 72, true);
			this.PrimaryOrganisationTabPage.TabIndex = 1;
			// 
			// SettlementOrgsTabPage
			// 
			this.SettlementOrgsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|292305d9-0670-47fd-be00-1d739e285b55", "Settlement Organizations");
			this.SettlementOrgsTabPage.Controls.Add(this.OrgInfoGrid);
			this.SettlementOrgsTabPage.Controls.Add(this.SettlementOrgTabTopPanel);
			this.SettlementOrgsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SettlementOrgsTabPage.Name = "SettlementOrgsTabPage";
			this.SettlementOrgsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 72, true);
			this.SettlementOrgsTabPage.TabIndex = 1;
			// 
			// OrgInfoGrid
			// 
			this.OrgInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgInfoGrid, "MatchingFilterBizO+SettlementOrgInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MatchingBase)(null)).MatchingFilterBizO.SettlementOrgInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Business.Base.Filters.OrgLedgerFilter)(((System.Collections.IList)(((MatchingBase)(null)).MatchingFilterBizO.SettlementOrgInfos)).SyncRoot)).Organization)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Business.Base.Filters.OrgLedgerFilter)(((System.Collections.IList)(((MatchingBase)(null)).MatchingFilterBizO.SettlementOrgInfos)).SyncRoot)).APLedger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Business.Base.Filters.OrgLedgerFilter)(((System.Collections.IList)(((MatchingBase)(null)).MatchingFilterBizO.SettlementOrgInfos)).SyncRoot)).ARLedger)));
			this.OrgInfoGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|1b7e9400-9ce9-414b-acc8-378514f63fe6", "Org.", "Organization");
			zGuidFindBoxColumnStyleInfo16.ColumnName = "Organization";
			zGuidFindBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|d6a84fcb-d1e1-4d4e-a7d6-4d08fc83444e", "AP");
			zCheckBoxColumnStyleInfo1.ColumnName = "APLedger";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|df02b088-5c9b-4091-a902-bd1ad2428209", "AR");
			zCheckBoxColumnStyleInfo2.ColumnName = "ARLedger";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.OrgInfoGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo16);
			this.OrgInfoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OrgInfoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.OrgInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgInfoGrid.GridId = "5cf40d0c-0454-4f70-88fa-197721a5ff26";
			this.OrgInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgInfoGrid.LayoutKey = "zGrid1";
			this.OrgInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.OrgInfoGrid.Name = "OrgInfoGrid";
			this.OrgInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 43, true);
			this.OrgInfoGrid.TabIndex = 2;
			// 
			// SettlementOrgTabTopPanel
			// 
			this.SettlementOrgTabTopPanel.Controls.Add(this.ExpandViewButton);
			this.SettlementOrgTabTopPanel.Controls.Add(this.IncludeAllARTransactions);
			this.SettlementOrgTabTopPanel.Controls.Add(this.IncludeAllAPTransactionsCheckBox);
			this.SettlementOrgTabTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SettlementOrgTabTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SettlementOrgTabTopPanel.Name = "SettlementOrgTabTopPanel";
			this.SettlementOrgTabTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 29, true);
			this.SettlementOrgTabTopPanel.TabIndex = 3;
			// 
			// ExpandViewButton
			// 
			this.ExpandViewButton.IsCaptionOverridden = true;
			this.ExpandViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 6, true);
			this.ExpandViewButton.Name = "ExpandViewButton";
			this.ExpandViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExpandViewButton.TabIndex = 4;
			this.ExpandViewButton.Text = Res.GetString("NewMatchGroupForm|3825d3bc-c1d9-4f5b-9f95-3a8bb376701a", "Expand View");
			this.ExpandViewButton.ToolTipCaption = null;
			this.ExpandViewButton.UseVisualStyleBackColor = true;
			this.ExpandViewButton.Click += new EventHandler(this.ExpandViewButton_Click);
			// 
			// IncludeAllARTransactions
			// 
			this.BindingSource.SetBindingMember(this.IncludeAllARTransactions, "MatchingFilterBizO.IncludeAllAR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((MatchingBase)(null)).MatchingFilterBizO.IncludeAllAR)));
			this.IncludeAllARTransactions.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|a46593d3-c1de-495d-a4ad-54726b68c293", "Include all AR", "Include all AR", "");
			this.IncludeAllARTransactions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 0, true);
			this.IncludeAllARTransactions.Name = "IncludeAllARTransactions";
			this.IncludeAllARTransactions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 25, true);
			this.IncludeAllARTransactions.TabIndex = 3;
			// 
			// IncludeAllAPTransactionsCheckBox
			// 
			this.IncludeAllAPTransactionsCheckBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.IncludeAllAPTransactionsCheckBox, "MatchingFilterBizO.IncludeAllAP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((MatchingBase)(null)).MatchingFilterBizO.IncludeAllAP)));
			this.IncludeAllAPTransactionsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|1e8674d7-b0ec-4c85-b7a7-6079f9274826", "Include all AP", "Include all AP", "");
			this.IncludeAllAPTransactionsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 0, true);
			this.IncludeAllAPTransactionsCheckBox.Name = "IncludeAllAPTransactionsCheckBox";
			this.IncludeAllAPTransactionsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 25, true);
			this.IncludeAllAPTransactionsCheckBox.TabIndex = 2;
			this.IncludeAllAPTransactionsCheckBox.UseVisualStyleBackColor = false;
			// 
			// MatchTransactionsGroupBox
			// 
			this.MatchTransactionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f6946e4d-c282-4d69-9b1f-b89963497410", "Transactions Selected for Matching");
			this.MatchTransactionsGroupBox.Controls.Add(this.MatchTransactionsGrid);
			this.MatchTransactionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchTransactionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MatchTransactionsGroupBox.Name = "MatchTransactionsGroupBox";
			this.MatchTransactionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1162, 161, true);
			this.MatchTransactionsGroupBox.TabIndex = 0;
			this.MatchTransactionsGroupBox.TabStop = false;
			// 
			// MatchTransactionsGrid
			// 
			this.MatchTransactionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MatchTransactionsGrid, "MatchedTransactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).Organisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).MatchDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).CurrencyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).CurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).CurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).OSPartialPaymentAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).LoginCompanyCurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).LocalPartialPaymentAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).ExchangeRateAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).ConsolidatedRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).InvoiceTransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).BranchGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).DepartmentGuid)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).InvoiceBatchNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).VoyageVesselOrFlightDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).ShipmentHouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).ShipmentMasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).DisplayInvoiceAddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).InvoiceRemittanceReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).MatchStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).MatchStatusReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).LoginCompanyCurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).NotionalWHTTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IMatching)(((System.Collections.IList)(((MatchingBase)(null)).MatchedTransactions)).SyncRoot)).RealizedWHTTax)));
			this.MatchTransactionsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|ce335045-959b-4464-bb36-24fe2141a950", "Organization");
			zGuidFindBoxColumnStyleInfo17.ColumnName = "Organisation";
			zGuidFindBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo33.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f380e3b0-11d1-4e6f-81e6-04a9147d2711", "Ledger");
			zTextBoxColumnStyleInfo33.ColumnName = "Ledger";
			zTextBoxColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo34.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|25104ac4-5eb5-4b81-adcc-14a9347c707a", "Type");
			zTextBoxColumnStyleInfo34.ColumnName = "TransactionType";
			zTextBoxColumnStyleInfo34.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo35.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|83db49b0-14f3-4e9a-a52c-9dcfd29f0521", "Transaction Num.", "Transaction Number.");
			zTextBoxColumnStyleInfo35.ColumnName = "TransactionNumber";
			zTextBoxColumnStyleInfo35.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo36.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|961ca690-0332-4dd1-a08c-2ba5421dcd57", "Check/Reference");
			zTextBoxColumnStyleInfo36.ColumnName = "ChequeOrReference";
			zTextBoxColumnStyleInfo36.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|43a0c5a9-5110-40f9-bdbf-fa91fa60fafe", "Trans. Date");
			zDateEditColumnStyleInfo12.ColumnName = "InvoiceDate";
			zDateEditColumnStyleInfo12.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|311d7f27-3569-48fa-9de3-71ecb59404cf", "Due Date");
			zDateEditColumnStyleInfo13.ColumnName = "DueDate";
			zDateEditColumnStyleInfo13.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f859014d-cd75-4dd8-aef1-bb5d6c19403b", "Match Date");
			zDateEditColumnStyleInfo14.ColumnName = "MatchDate";
			zDateEditColumnStyleInfo14.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|712c1b39-efca-41d3-b926-d6ba5dadc088", "Currency");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CurrencyCode";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = "CurrencyDecimals";
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|879dcb5b-0c97-4ff7-84be-357e09e8c984", "Outstanding Amount");
			zCalcEditColumnStyleInfo14.ColumnName = "OSOutstandingAmount";
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = "CurrencyDecimals";
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|ac02bda0-abc7-48d3-83d2-0063bc00f744", "Paid Amount");
			zCalcEditColumnStyleInfo15.ColumnName = "OSPartialPaymentAmount";
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = "LoginCompanyCurrencyDecimals";
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|f488bd2e-0648-4ba9-8bd9-1f7d56b0d4cf", "Local Paid Amount");
			zCalcEditColumnStyleInfo16.ColumnName = "LocalPartialPaymentAmount";
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|026ff883-453b-4adf-92dd-551e37705546", "Ex. Rate");
			zCalcEditColumnStyleInfo17.ColumnName = "ExchangeRateAmount";
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo37.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|a8632c07-4e6a-4846-899f-ffa109a53f3a", "Job Inv. Num.", "Job Invoice Number.");
			zTextBoxColumnStyleInfo37.ColumnName = "ConsolidatedRef";
			zTextBoxColumnStyleInfo37.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo38.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|e009e877-5e39-4c8b-a8e3-167edc66336a", "Compliance Number");
			zTextBoxColumnStyleInfo38.ColumnName = "TransactionReference";
			zTextBoxColumnStyleInfo38.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo46.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|54A02BBB-E58B-4A36-B101-CE85281BC022", "Invoice Transaction Reference");
			zTextBoxColumnStyleInfo46.ColumnName = "InvoiceTransactionReference";
			zTextBoxColumnStyleInfo46.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|3253b8f3-c309-4bca-8ac4-9b5d78f684c2", "Post Date");
			zDateEditColumnStyleInfo15.ColumnName = "PostDate";
			zDateEditColumnStyleInfo15.IsReadOnly = true;
			zDateEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|4aeafcd9-9292-4817-b909-767e3188f9f6", "Branch");
			zGuidFindBoxColumnStyleInfo18.ColumnName = "BranchGuid";
			zGuidFindBoxColumnStyleInfo18.IsVisible = false;
			zGuidFindBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|7a09a2f1-f50c-4e59-a339-94fbeceaeb89", "Department");
			zGuidFindBoxColumnStyleInfo19.ColumnName = "DepartmentGuid";
			zGuidFindBoxColumnStyleInfo19.IsVisible = false;
			zGuidFindBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo39.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|a3ee32ad-2770-468c-83ac-03a229a66dd9", "Description");
			zTextBoxColumnStyleInfo39.ColumnName = "Description";
			zTextBoxColumnStyleInfo39.IsVisible = false;
			zTextBoxColumnStyleInfo39.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo40.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|2fc2f2bc-13da-4bac-89b6-92fcf23305a6", "Inv. Batch Number", "Invoice Batch Number.");
			zTextBoxColumnStyleInfo40.ColumnName = "InvoiceBatchNumber";
			zTextBoxColumnStyleInfo40.IsReadOnly = true;
			zTextBoxColumnStyleInfo40.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo41.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|91eb671a-613b-4de7-bded-4017b6149a9e", "Flight/Date or Voyage/Vessel");
			zTextBoxColumnStyleInfo41.ColumnName = "VoyageVesselOrFlightDate";
			zTextBoxColumnStyleInfo41.IsReadOnly = true;
			zTextBoxColumnStyleInfo41.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo42.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|9be3679a-62c3-4562-9691-b5473ab57e07", "House Bill");
			zTextBoxColumnStyleInfo42.ColumnName = "ShipmentHouseBill";
			zTextBoxColumnStyleInfo42.IsReadOnly = true;
			zTextBoxColumnStyleInfo42.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo43.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|e1ba7be5-7f91-474f-b9d3-4b29eb849a76", "Master Bill");
			zTextBoxColumnStyleInfo43.ColumnName = "ShipmentMasterBill";
			zTextBoxColumnStyleInfo43.IsReadOnly = true;
			zTextBoxColumnStyleInfo43.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo44.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|afac53da-a944-475e-8519-bfe503342293", "Category");
			zTextBoxColumnStyleInfo44.ColumnName = "TransactionCategory";
			zTextBoxColumnStyleInfo44.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|a445ad37-ceeb-4e9b-a183-a434d46aa874", "Address");
			zGuidFindBoxColumnStyleInfo20.ColumnName = "DisplayInvoiceAddressOverride";
			zGuidFindBoxColumnStyleInfo20.IsVisible = false;
			zGuidFindBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo45.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|7a545757-fb00-4504-9622-496216c1d10e", "Invoice Remittance Reference");
			zTextBoxColumnStyleInfo45.ColumnName = "InvoiceRemittanceReference";
			zTextBoxColumnStyleInfo45.IsReadOnly = true;
			zTextBoxColumnStyleInfo45.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|65459166-2d62-4f97-b0ad-461f1116d0f5", "Match Status");
			zDropEditColumnStyleInfo7.ColumnName = "MatchStatus";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|751b9fa3-f3b1-44d1-9cc4-5ed70f23995b", "Reason", "Match Status Reason", "Transaction Match Status Reason");
			zDropEditColumnStyleInfo8.ColumnName = "MatchStatusReasonCode";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = "LoginCompanyCurrencyDecimals";
			zCalcEditColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|7547bb72-9004-4034-a262-fe05c216d06d", "Notional WHT");
			zCalcEditColumnStyleInfo18.ColumnName = "NotionalWHTTax";
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo19.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|303eba5e-0d4b-4add-a679-8296844fe0cb", "Realized WHT");
			zCalcEditColumnStyleInfo19.ColumnName = "RealizedWHTTax";
			zCalcEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MatchTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo17);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo33);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo34);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo35);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo36);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo14);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo37);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo38);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo46);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo15);
			this.MatchTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo18);
			this.MatchTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo19);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo39);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo40);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo41);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo42);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo43);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo44);
			this.MatchTransactionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo20);
			this.MatchTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo45);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.MatchTransactionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.MatchTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo19);
			this.MatchTransactionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchTransactionsGrid.GridId = "5f3974df-09f2-493f-8b69-4f7fe9d26fd0";
			this.MatchTransactionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MatchTransactionsGrid.LayoutKey = "zGrid1";
			this.MatchTransactionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MatchTransactionsGrid.Name = "MatchTransactionsGrid";
			this.MatchTransactionsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.MatchTransactionsGrid.ShouldSetErrorsOnTabPage = false;
			this.MatchTransactionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 142, true);
			this.MatchTransactionsGrid.TabIndex = 0;
			this.MatchTransactionsGrid.DoubleClick += new EventHandler(this.MatchTransactionsGrid_DoubleClick);
			// 
			// BalanceGroupBox
			// 
			this.BalanceGroupBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BalanceGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|709e9296-11b3-4b08-9c02-b9d6bacdb10f", "Balance");
			this.BalanceGroupBox.Controls.Add(this.zCalcFindBox5);
			this.BalanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(985, 556, true);
			this.BalanceGroupBox.Name = "BalanceGroupBox";
			this.BalanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 70, true);
			this.BalanceGroupBox.TabIndex = 5;
			this.BalanceGroupBox.TabStop = false;
			// 
			// ChangePaymentAmountButton
			// 
			this.ChangePaymentAmountButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ChangePaymentAmountButton.AutoSize = true;
			this.ChangePaymentAmountButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|7ca6a036-ef26-4907-aa9c-8687a2c88ec3", "Change Payment Amount");
			this.ChangePaymentAmountButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 635, true);
			this.ChangePaymentAmountButton.Name = "ChangePaymentAmountButton";
			this.ChangePaymentAmountButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ChangePaymentAmountButton.TabIndex = 6;
			this.ChangePaymentAmountButton.ToolTipCaption = null;
			this.ChangePaymentAmountButton.Click += new EventHandler(this.ChangePaymentAmountButton_Click);
			// 
			// CurrencySummaryButton
			// 
			this.CurrencySummaryButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.CurrencySummaryButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|460b12f4-312f-40fa-84f6-61fbf4416433", "Detail");
			this.CurrencySummaryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 107, true);
			this.CurrencySummaryButton.Name = "CurrencySummaryButton";
			this.CurrencySummaryButton.ShouldSetReadOnlyWhenSettingIncludingChildren = true;
			this.CurrencySummaryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 23, true);
			this.CurrencySummaryButton.TabIndex = 0;
			this.CurrencySummaryButton.ToolTipCaption = null;
			this.CurrencySummaryButton.Click += new EventHandler(this.CurrencySummaryButton_Click);
			// 
			// CurrencySummaryGrid
			// 
			this.CurrencySummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CurrencySummaryGrid, "CurrencySummary.SummaryRows");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).CurrencyDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).AverageExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).AdjustmentNoteTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).ContraTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).CreditNoteTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).DiscountAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).ExchangeDifferenceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).InvoiceTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).JournalTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).OverpaymentAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).PaymentTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).ReceiptTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((CurrencySummaryRow)(((System.Collections.IList)(((MatchingBase)(null)).CurrencySummary.SummaryRows)).SyncRoot)).TransferTotal)));
			this.CurrencySummaryGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|e7bb2c14-b9c2-46bf-86fc-eaef9c0dc969", "Curr.");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "Currency";
			zCodeFindBoxColumnStyleInfo5.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo20.BindToDecimalPlaces = "CurrencyDecimals";
			zCalcEditColumnStyleInfo20.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|d1da9418-696f-444b-ba36-3a71c30b9595", "Total Amt.");
			zCalcEditColumnStyleInfo20.ColumnName = "Amount";
			zCalcEditColumnStyleInfo20.IsMandatory = true;
			zCalcEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo21.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|6ca70d7c-42a8-409a-815a-d5b00eaa1fa1", "Avg Ex Rate");
			zCalcEditColumnStyleInfo21.ColumnName = "AverageExRate";
			zCalcEditColumnStyleInfo21.IsMandatory = true;
			zCalcEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo22.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|6d222808-73ef-49c0-8378-6bf3fcc9c49c", "Local Amt.");
			zCalcEditColumnStyleInfo22.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo22.IsMandatory = true;
			zCalcEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo23.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo23.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|52843d50-2ece-411d-b527-84fbd5f49b7c", "Adjustment Note Total");
			zCalcEditColumnStyleInfo23.ColumnName = "AdjustmentNoteTotal";
			zCalcEditColumnStyleInfo23.IsVisible = false;
			zCalcEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo24.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo24.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|49bb474b-5f8a-44d9-9410-a15c9e4e4cd1", "Contra Total");
			zCalcEditColumnStyleInfo24.ColumnName = "ContraTotal";
			zCalcEditColumnStyleInfo24.IsVisible = false;
			zCalcEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo25.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo25.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|e1920325-4177-4999-86c4-10a3dc7f2686", "Credit Note Total");
			zCalcEditColumnStyleInfo25.ColumnName = "CreditNoteTotal";
			zCalcEditColumnStyleInfo25.IsVisible = false;
			zCalcEditColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo26.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo26.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|a1649ee4-bf24-45c1-a387-b2196daf6385", "Discount Amount");
			zCalcEditColumnStyleInfo26.ColumnName = "DiscountAmount";
			zCalcEditColumnStyleInfo26.IsVisible = false;
			zCalcEditColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo27.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo27.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|1c76beef-ce9e-4899-9f01-64f8589ee983", "Exchange Difference Amount");
			zCalcEditColumnStyleInfo27.ColumnName = "ExchangeDifferenceAmount";
			zCalcEditColumnStyleInfo27.IsVisible = false;
			zCalcEditColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo28.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo28.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|cc3ac337-e76e-44c1-9fdf-41dda69d4645", "Overpayment Amount");
			zCalcEditColumnStyleInfo28.ColumnName = "InvoiceTotal";
			zCalcEditColumnStyleInfo28.IsVisible = false;
			zCalcEditColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo29.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo29.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|8bf64f34-7f01-47f9-9a38-b6f45c3fea40", "Journal Total");
			zCalcEditColumnStyleInfo29.ColumnName = "JournalTotal";
			zCalcEditColumnStyleInfo29.IsVisible = false;
			zCalcEditColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo30.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo30.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|cc3ac337-e76e-44c1-9fdf-41dda69d4645", "Overpayment Amount");
			zCalcEditColumnStyleInfo30.ColumnName = "OverpaymentAmount";
			zCalcEditColumnStyleInfo30.IsVisible = false;
			zCalcEditColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo31.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo31.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|fee1745a-76d2-490b-a7e9-ceca96736d30", "Payment Total");
			zCalcEditColumnStyleInfo31.ColumnName = "PaymentTotal";
			zCalcEditColumnStyleInfo31.IsVisible = false;
			zCalcEditColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo32.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo32.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|33315c2e-0b1a-4b5d-9026-f8b5389b2054", "Receipt Total");
			zCalcEditColumnStyleInfo32.ColumnName = "ReceiptTotal";
			zCalcEditColumnStyleInfo32.IsVisible = false;
			zCalcEditColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo33.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo33.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|5855b457-d6bb-4905-8232-f6b417cffcf1", "Transfer");
			zCalcEditColumnStyleInfo33.ColumnName = "TransferTotal";
			zCalcEditColumnStyleInfo33.IsVisible = false;
			zCalcEditColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo20);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo21);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo22);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo23);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo24);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo25);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo26);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo27);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo28);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo29);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo30);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo31);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo32);
			this.CurrencySummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo33);
			this.CurrencySummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrencySummaryGrid.GridId = "1505d968-881b-4756-9c8e-272c1d892840";
			this.CurrencySummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CurrencySummaryGrid.IsWholeRowSelectedOnClick = true;
			this.CurrencySummaryGrid.LayoutKey = "zGrid1";
			this.CurrencySummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CurrencySummaryGrid.Name = "CurrencySummaryGrid";
			this.CurrencySummaryGrid.ReadOnly = true;
			this.CurrencySummaryGrid.ShouldSetErrorsOnTabPage = false;
			this.CurrencySummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 76, true);
			this.CurrencySummaryGrid.TabIndex = 0;
			// 
			// CurrencySummaryGroupBox
			// 
			this.CurrencySummaryGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CurrencySummaryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewMatchGroupForm|e14bd77c-5897-4622-8b6e-aee50c11a40c", "Matching Summary by Currency");
			this.CurrencySummaryGroupBox.Controls.Add(this.CurrencySummaryGrid);
			this.CurrencySummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(708, 4, true);
			this.CurrencySummaryGroupBox.Name = "CurrencySummaryGroupBox";
			this.CurrencySummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 95, true);
			this.CurrencySummaryGroupBox.TabIndex = 1;
			this.CurrencySummaryGroupBox.TabStop = false;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.CurrencySummaryButton);
			this.RightPanel.Controls.Add(this.SelectAllButton);
			this.RightPanel.Controls.Add(this.UnselectAllButton);
			this.RightPanel.Controls.Add(this.MoveDownButton);
			this.RightPanel.Controls.Add(this.MoveUpButton);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1169, 0, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 686, true);
			this.RightPanel.TabIndex = 3;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 102, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.MatchingTabControl);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.MatchTransactionsGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1162, 448, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(283);
			this.MainSplitContainer.TabIndex = 4;
			// 
			// NewMatchGroupForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 686, true);
			this.Controls.Add(this.MainSplitContainer);
			this.Controls.Add(this.RightPanel);
			this.Controls.Add(this.MatchDateGroupBox);
			this.Controls.Add(this.ChangePaymentAmountButton);
			this.Controls.Add(this.BalanceGroupBox);
			this.Controls.Add(this.MiscMatchingTransactionsGroupBox);
			this.Controls.Add(this.SaveAsDraftButton);
			this.Controls.Add(this.MatchAndContinueButton);
			this.Controls.Add(this.MatchAndCloseButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.CurrencySummaryGroupBox);
			this.Controls.Add(this.OrganisationTabControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(MatchingBase);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Base.Matching.MatchingBase";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1250, 725, true);
			this.Name = "NewMatchGroupForm";
			this.RememberFormSize = false;
			this.KeyDown += new KeyEventHandler(this.TestMatchingForm_KeyDown);
			this.Controls.SetChildIndex(this.OrganisationTabControl, 0);
			this.Controls.SetChildIndex(this.CurrencySummaryGroupBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MatchAndCloseButton, 0);
			this.Controls.SetChildIndex(this.MatchAndContinueButton, 0);
			this.Controls.SetChildIndex(this.SaveAsDraftButton, 0);
			this.Controls.SetChildIndex(this.MiscMatchingTransactionsGroupBox, 0);
			this.Controls.SetChildIndex(this.BalanceGroupBox, 0);
			this.Controls.SetChildIndex(this.ChangePaymentAmountButton, 0);
			this.Controls.SetChildIndex(this.MatchDateGroupBox, 0);
			this.Controls.SetChildIndex(this.RightPanel, 0);
			this.Controls.SetChildIndex(this.MainSplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.MatchDateGroupBox.ResumeLayout(false);
			this.MatchDateGroupBox.PerformLayout();
			this.MatchDateEdit.ResumeLayout(true);
			this.MatchDateEdit.PerformLayout();
			this.zCalcFindBox5.ResumeLayout(true);
			this.zCalcFindBox5.PerformLayout();
			this.PrimaryOrgGuidFindBox.ResumeLayout(true);
			this.PrimaryOrgGuidFindBox.PerformLayout();
			this.MiscMatchingTransactionsGroupBox.ResumeLayout(false);
			this.MiscMatchingTransactionsGroupBox.PerformLayout();
			this.BankFeeCalcFindBox.ResumeLayout(true);
			this.BankFeeCalcFindBox.PerformLayout();
			this.zCalcFindBox4.ResumeLayout(true);
			this.zCalcFindBox4.PerformLayout();
			this.zCalcFindBox3.ResumeLayout(true);
			this.zCalcFindBox3.PerformLayout();
			this.zCalcFindBox2.ResumeLayout(true);
			this.zCalcFindBox2.PerformLayout();
			this.MatchingTabControl.ResumeLayout(false);
			this.MatchingTabControl.PerformLayout();
			this.GridsTabPage.ResumeLayout(false);
			this.GridsTabPage.PerformLayout();
			this.TransactionsTopPanel.ResumeLayout(false);
			this.TransactionsTopPanel.PerformLayout();
			this.TransactionSearchGroupBox.ResumeLayout(false);
			this.TransactionSearchGroupBox.PerformLayout();
			this.OutstandingTransactionsPanel.ResumeLayout(false);
			this.OutstandingTransactionsPanel.PerformLayout();
			this.OutstandingTransactionsGridPanel.ResumeLayout(false);
			this.OutstandingTransactionsGridPanel.PerformLayout();
			this.UnmatchedTransactionsGroupBox.ResumeLayout(false);
			this.UnmatchedTransactionsGroupBox.PerformLayout();
			((ISupportInitialize)(this.UnmatchedTransactionsGrid)).EndInit();
			this.UnmatchedTransactionsGrid.ResumeLayout(false);
			this.UnmatchedTransactionsGrid.PerformLayout();
			this.CashAdvanceTabPage.ResumeLayout(false);
			this.CashAdvanceTabPage.PerformLayout();
			this.CashAdvanceSplitContainer.Panel1.ResumeLayout(false);
			this.CashAdvanceSplitContainer.Panel2.ResumeLayout(false);
			((ISupportInitialize)(this.CashAdvanceSplitContainer)).EndInit();
			this.CashAdvanceSplitContainer.ResumeLayout(false);
			this.CashAdvanceSplitContainer.PerformLayout();
			this.RequestedCashAdvanceGroupBox.ResumeLayout(false);
			this.RequestedCashAdvanceGroupBox.PerformLayout();
			((ISupportInitialize)(this.UnmatchedCashAdvanceRequestsGrid)).EndInit();
			this.UnmatchedCashAdvanceRequestsGrid.ResumeLayout(false);
			this.UnmatchedCashAdvanceRequestsGrid.PerformLayout();
			this.APJournalsTabPage.ResumeLayout(false);
			this.APJournalsTabPage.PerformLayout();
			((ISupportInitialize)(this.APJournalsGrid)).EndInit();
			this.APJournalsGrid.ResumeLayout(false);
			this.APJournalsGrid.PerformLayout();
			this.ARJournalsTabPage.ResumeLayout(false);
			this.ARJournalsTabPage.PerformLayout();
			((ISupportInitialize)(this.ARJournalsGrid)).EndInit();
			this.ARJournalsGrid.ResumeLayout(false);
			this.ARJournalsGrid.PerformLayout();
			this.OrganisationTabControl.ResumeLayout(false);
			this.OrganisationTabControl.PerformLayout();
			this.PrimaryOrganisationTabPage.ResumeLayout(false);
			this.PrimaryOrganisationTabPage.PerformLayout();
			this.SettlementOrgsTabPage.ResumeLayout(false);
			this.SettlementOrgsTabPage.PerformLayout();
			((ISupportInitialize)(this.OrgInfoGrid)).EndInit();
			this.OrgInfoGrid.ResumeLayout(false);
			this.OrgInfoGrid.PerformLayout();
			this.SettlementOrgTabTopPanel.ResumeLayout(false);
			this.SettlementOrgTabTopPanel.PerformLayout();
			this.MatchTransactionsGroupBox.ResumeLayout(false);
			this.MatchTransactionsGroupBox.PerformLayout();
			((ISupportInitialize)(this.MatchTransactionsGrid)).EndInit();
			this.MatchTransactionsGrid.ResumeLayout(false);
			this.MatchTransactionsGrid.PerformLayout();
			this.BalanceGroupBox.ResumeLayout(false);
			this.BalanceGroupBox.PerformLayout();
			((ISupportInitialize)(this.CurrencySummaryGrid)).EndInit();
			this.CurrencySummaryGrid.ResumeLayout(false);
			this.CurrencySummaryGrid.PerformLayout();
			this.CurrencySummaryGroupBox.ResumeLayout(false);
			this.CurrencySummaryGroupBox.PerformLayout();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
