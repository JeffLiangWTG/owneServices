using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoiceBatchForm
	{


		#region Windows Form Designer generated code

		protected System.ComponentModel.Container components = null;
		protected InvoiceBatchOnFormFilterControl FilterControl;
		protected InvoiceBatchHeaderFilterBusinessObject FilterBuisnessObject;
		protected ZPanel FilterPanel;
		protected ZLabel CancelledBatchLabel;
		protected Core.Forms.ZPostOrCancelButton CloseButton;
		protected Core.Forms.ZPostOrCancelButton PostButton;
		protected ZCalcFindBox TransAmountTotalCalcFindBox;
		protected ZCalcFindBox LocalAmountCalcFindBox;
		protected ZCalcFindBox GSTCalcFindBox;
		protected ZDateEdit DateEdit;
		protected ZTextBox BatchNumberTextBox;
		protected ZGroupBox BatchFilterGroupBox;
		protected ZPanel BottomButtonPanel;
		protected ZPanel BatchDatePanel;
		protected ZGroupBox BatchTotalGroupBox;
		protected ZDateEdit DueDateDateEdit;
		protected ZCalcEdit TermDaysCalcEdit;
		protected ZDropEdit TermsDropEdit;
		protected ZGuidFindBox DebtorGuidFindBox;
		protected ZCheckedListBox JobTypeCheckedListBox;

		new void InitializeComponent()
		{
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			this.FilterPanel = new ZPanel();
			this.NotificationGridContainer = new ZPanel();
			this.BatchInvoiceLinesGrid = new ZGrid();
			this.NotificationPanel = new ZPanel();
			this.NotificationLabel = new ZLabel();
			this.CancelledBatchLabel = new ZLabel();
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.PostButton = new Core.Forms.ZPostOrCancelButton();
			this.TransAmountTotalCalcFindBox = new ZCalcFindBox();
			this.LocalAmountCalcFindBox = new ZCalcFindBox();
			this.GSTCalcFindBox = new ZCalcFindBox();
			this.DateEdit = new ZDateEdit();
			this.BatchNumberTextBox = new ZTextBox();
			this.BatchFilterGroupBox = new ZGroupBox();
			this.BottomButtonPanel = new ZPanel();
			this.kFlowLayoutPanel1 = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.BatchTotalGroupBox = new ZGroupBox();
			this.BatchDatePanel = new ZPanel();
			this.CurrencyCodeFindBox = new ZCodeFindBox();
			this.JobTypeCheckedListBox = new ZCheckedListBox();
			this.DebtorGuidFindBox = new ZGuidFindBox();
			this.TermDaysCalcEdit = new ZCalcEdit();
			this.TermsDropEdit = new ZDropEdit();
			this.DueDateDateEdit = new ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterPanel.SuspendLayout();
			this.NotificationGridContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BatchInvoiceLinesGrid)).BeginInit();
			this.NotificationPanel.SuspendLayout();
			this.BatchFilterGroupBox.SuspendLayout();
			this.BottomButtonPanel.SuspendLayout();
			this.kFlowLayoutPanel1.SuspendLayout();
			this.BatchTotalGroupBox.SuspendLayout();
			this.BatchDatePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 575, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(436);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(437);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(InvoiceBatchHeader);
			// 
			// FilterPanel
			// 
			this.FilterPanel.Controls.Add(this.CancelledBatchLabel);
			this.FilterPanel.Controls.Add(this.NotificationGridContainer);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 380, true);
			this.FilterPanel.TabIndex = 0;
			// 
			// NotificationGridContainer
			// 
			this.NotificationGridContainer.Controls.Add(this.BatchInvoiceLinesGrid);
			this.NotificationGridContainer.Controls.Add(this.NotificationPanel);
			this.NotificationGridContainer.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NotificationGridContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
			this.NotificationGridContainer.Name = "NotificationGridContainer";
			this.NotificationGridContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 254, true);
			this.NotificationGridContainer.TabIndex = 0;
			// 
			// BatchInvoiceLinesGrid
			// 
			this.BatchInvoiceLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BatchInvoiceLinesGrid, "Line");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((IList)(((InvoiceBatchHeader)(null)).Line)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).IncludeInTheBatch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_InvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_GSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_OSExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoicingBase)(((IList)(((InvoiceBatchHeader)(null)).Line)).SyncRoot)).AH_LocalExtraTaxAmount)));
			this.BatchInvoiceLinesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|e1d6eacb-12c4-400c-9a15-b750bcb35544", "Batch");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInTheBatch";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|b7863873-0f39-4e4d-914a-6385f9171f1c", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|85a100f6-61e0-4bd2-af42-f463a2f00f98", "Transaction No.");
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ColumnName = "AH_ConsolidatedInvoiceRef";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ColumnName = "AH_TransactionCategory";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|6afe8b65-7b1b-43fd-a294-766153c7fc42", "Post Date");
			zDateEditColumnStyleInfo1.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo2.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|23bdd48c-27ff-455c-98fe-32f0565cc1ed", "Trans. Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|45356f9d-244d-4a89-8b67-7e9a024871e4", "Debtor");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|5ac9a4c6-2782-4b02-9419-1a7f109315d6", "Local Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "AH_InvoiceAmount";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|0c0336c9-633e-4e05-9a2d-96a42e6b02c7", "Tax Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "AH_GSTAmount";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "AH_OutstandingAmount";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "AH_ExchangeRate";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|3b4fe7f2-3f35-4d7b-a92f-66fa129ddaf8", "EDU Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "AH_OSExtraTaxAmount";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|80a9b9aa-e7e1-4d19-b2b0-882117c7a7d0", "EDU Local Amount");
			zCalcEditColumnStyleInfo7.ColumnName = "AH_LocalExtraTaxAmount";
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.BatchInvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.BatchInvoiceLinesGrid.CopySelectedRowsAllowed = true;
			this.BatchInvoiceLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BatchInvoiceLinesGrid.GridId = "4E52D81D-76AA-4B59-BDAD-99220AFF20D2";
			this.BatchInvoiceLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BatchInvoiceLinesGrid.LayoutKey = "BatchInvoiceLinesGrid";
			this.BatchInvoiceLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.BatchInvoiceLinesGrid.Name = "BatchInvoiceLinesGrid";
			this.BatchInvoiceLinesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.BatchInvoiceLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 222, true);
			this.BatchInvoiceLinesGrid.TabIndex = 6;
			// 
			// NotificationPanel
			// 
			this.NotificationPanel.Controls.Add(this.NotificationLabel);
			this.NotificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.NotificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotificationPanel.Name = "NotificationPanel";
			this.NotificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 32, true);
			this.NotificationPanel.TabIndex = 5;
			// 
			// NotificationLabel
			// 
			this.NotificationLabel.AutoSize = true;
			this.NotificationLabel.ForeColor = System.Drawing.SystemColors.InfoText;
			this.NotificationLabel.IsFontBold = true;
			this.NotificationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.NotificationLabel.Name = "NotificationLabel";
			this.NotificationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.NotificationLabel.TabIndex = 0;
			// 
			// CancelledBatchLabel
			// 
			this.CancelledBatchLabel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.CancelledBatchLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|dc04eaab-6f74-4a97-b1ed-e92843437a48", "This Invoice Batch is canceled.");
			this.CancelledBatchLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.CancelledBatchLabel.IsFontBold = true;
			this.CancelledBatchLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 41, true);
			this.CancelledBatchLabel.Name = "CancelledBatchLabel";
			this.CancelledBatchLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 23, true);
			this.CancelledBatchLabel.TabIndex = 2;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.AutoSize = true;
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|2216edf5-dbfd-4861-a9bb-2403953a4c6b", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(406, 2, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			// 
			// PostButton
			// 
			this.PostButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButton.AutoSize = true;
			this.PostButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|0090dd51-63d5-4c42-8de6-5513d6f69044", "Post");
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 2, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PostButton.TabIndex = 1;
			// 
			// TransAmountTotalCalcFindBox
			// 
			this.TransAmountTotalCalcFindBox.AllowDrop = true;
			this.TransAmountTotalCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TransAmountTotalCalcFindBox.BindToAmount = "AH_OSTotal";
			this.TransAmountTotalCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.TransAmountTotalCalcFindBox.BindToUnit = "AH_Readonly_RXCode";
			this.TransAmountTotalCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|4bb19168-3748-40d9-af48-45a152dbabd8", "Trans. Amount", "Trans. Amount Total (incl. Tax):", "Transaction Amount in Transaction currency.");
			this.TransAmountTotalCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TransAmountTotalCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 18, true);
			this.TransAmountTotalCalcFindBox.Name = "TransAmountTotalCalcFindBox";
			this.TransAmountTotalCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.TransAmountTotalCalcFindBox.TabIndex = 0;
			// 
			// LocalAmountCalcFindBox
			// 
			this.LocalAmountCalcFindBox.AllowDrop = true;
			this.LocalAmountCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LocalAmountCalcFindBox.BindToAmount = "AH_InvoiceAmount";
			this.LocalAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.LocalAmountCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.LocalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|e5e8890d-77d3-4111-aa26-c0034307982c", "Local Amount Total");
			this.LocalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 18, true);
			this.LocalAmountCalcFindBox.Name = "LocalAmountCalcFindBox";
			this.LocalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.LocalAmountCalcFindBox.TabIndex = 2;
			// 
			// GSTCalcFindBox
			// 
			this.GSTCalcFindBox.AllowDrop = true;
			this.GSTCalcFindBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GSTCalcFindBox.BindToAmount = "AH_GSTAmount";
			this.GSTCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.GSTCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|640c6dd6-3d97-4f52-afd2-5a4fdca7c222", "GST Total", "GST Total", "");
			this.GSTCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 18, true);
			this.GSTCalcFindBox.Name = "GSTCalcFindBox";
			this.GSTCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.GSTCalcFindBox.TabIndex = 1;
			// 
			// DateEdit
			// 
			this.DateEdit.AllowDrop = true;
			this.DateEdit.AutoCompleteMonthThreshold = 1;
			this.DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoiceBatchHeader)(null)).AH_InvoiceDate)));
			this.DateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|869bd85b-1c94-4789-a66d-ba8028885eb8", "Invoice Date", "Invoice Date", "");
			this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 37, true);
			this.DateEdit.Name = "DateEdit";
			this.DateEdit.TabIndex = 1;
			// 
			// BatchNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BatchNumberTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceBatchHeader)(null)).AH_TransactionNum)));
			this.BatchNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|663b4792-a995-4ddf-ac2f-fdae07ebfc73", "Invoice Statement Number");
			this.BatchNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 67, true);
			this.BatchNumberTextBox.Name = "BatchNumberTextBox";
			this.BatchNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.BatchNumberTextBox.TabIndex = 6;
			// 
			// BatchFilterGroupBox
			// 
			this.BatchFilterGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|a6f1778a-4ca3-4699-a302-b7c5b0fe1d5d", "Selection Filters");
			this.BatchFilterGroupBox.Controls.Add(this.FilterPanel);
			this.BatchFilterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BatchFilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 94, true);
			this.BatchFilterGroupBox.Name = "BatchFilterGroupBox";
			this.BatchFilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 399, true);
			this.BatchFilterGroupBox.TabIndex = 1;
			this.BatchFilterGroupBox.TabStop = false;
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.kFlowLayoutPanel1);
			this.BottomButtonPanel.Controls.Add(this.BatchTotalGroupBox);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 493, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 82, true);
			this.BottomButtonPanel.TabIndex = 5;
			// 
			// kFlowLayoutPanel1
			// 
			this.kFlowLayoutPanel1.Controls.Add(this.CloseButton);
			this.kFlowLayoutPanel1.Controls.Add(this.PostButton);
			this.kFlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.kFlowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 50, true);
			this.kFlowLayoutPanel1.Name = "kFlowLayoutPanel1";
			this.kFlowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 30, true);
			this.kFlowLayoutPanel1.TabIndex = 3;
			// 
			// BatchTotalGroupBox
			// 
			this.BatchTotalGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|bfd3878c-f8c7-42cd-ba86-4968bb677dfb", "Batch Total");
			this.BatchTotalGroupBox.Controls.Add(this.LocalAmountCalcFindBox);
			this.BatchTotalGroupBox.Controls.Add(this.GSTCalcFindBox);
			this.BatchTotalGroupBox.Controls.Add(this.TransAmountTotalCalcFindBox);
			this.BatchTotalGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.BatchTotalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BatchTotalGroupBox.Name = "BatchTotalGroupBox";
			this.BatchTotalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 45, true);
			this.BatchTotalGroupBox.TabIndex = 0;
			this.BatchTotalGroupBox.TabStop = false;
			// 
			// BatchDatePanel
			// 
			this.BatchDatePanel.Controls.Add(this.CurrencyCodeFindBox);
			this.BatchDatePanel.Controls.Add(this.JobTypeCheckedListBox);
			this.BatchDatePanel.Controls.Add(this.DebtorGuidFindBox);
			this.BatchDatePanel.Controls.Add(this.TermDaysCalcEdit);
			this.BatchDatePanel.Controls.Add(this.TermsDropEdit);
			this.BatchDatePanel.Controls.Add(this.DueDateDateEdit);
			this.BatchDatePanel.Controls.Add(this.DateEdit);
			this.BatchDatePanel.Controls.Add(this.BatchNumberTextBox);
			this.BatchDatePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BatchDatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BatchDatePanel.Name = "BatchDatePanel";
			this.BatchDatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 94, true);
			this.BatchDatePanel.TabIndex = 0;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "AH_RX_NKTransactionCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((InvoiceBatchHeader)(null)).AH_RX_NKTransactionCurrency)));
			this.CurrencyCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|8a8311cc-8b22-40a9-b639-6fe96c8708f6", "Currency");
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 66, true);
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.CurrencyCodeFindBox.TabIndex = 3;
			// 
			// JobTypeCheckedListBox
			// 
			this.JobTypeCheckedListBox.BindingItems = null;
			this.BindingSource.SetBindingMember(this.JobTypeCheckedListBox, "JobTypeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZBoolDescriptionPairList)(((InvoiceBatchHeader)(null)).JobTypeList)));
			this.JobTypeCheckedListBox.CheckOnClick = true;
			this.JobTypeCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(663, 8, true);
			this.JobTypeCheckedListBox.Name = "JobTypeCheckedListBox";
			this.JobTypeCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 79, true);
			this.JobTypeCheckedListBox.TabIndex = 7;
			// 
			// DebtorGuidFindBox
			// 
			this.DebtorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DebtorGuidFindBox, "AH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((InvoiceBatchHeader)(null)).AH_OH)));
			this.DebtorGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|00b172c0-0ccf-4cc4-b1f4-ca1f75c76965", "Debtor", "Debtor", "");
			this.DebtorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			this.DebtorGuidFindBox.Name = "DebtorGuidFindBox";
			this.DebtorGuidFindBox.PopupCaption = null;
			this.DebtorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.DebtorGuidFindBox.TabIndex = 0;
			this.DebtorGuidFindBox.Leave += new EventHandler(this.DebtorGuidFindBox_Leave);
			// 
			// TermDaysCalcEdit
			// 
			this.TermDaysCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TermDaysCalcEdit, "AH_InvoiceTermDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((InvoiceBatchHeader)(null)).AH_InvoiceTermDays)));
			this.TermDaysCalcEdit.DecimalPlaces = 0;
			this.TermDaysCalcEdit.Decimals = 0;
			this.TermDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 41, true);
			this.TermDaysCalcEdit.Name = "TermDaysCalcEdit";
			this.TermDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.TermDaysCalcEdit.TabIndex = 5;
			this.TermDaysCalcEdit.Text = "0";
			this.TermDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TermsDropEdit
			// 
			this.TermsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TermsDropEdit, "AH_InvoiceTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoiceBatchHeader)(null)).AH_InvoiceTerm)));
			this.TermsDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|d7d9a046-a1c5-452d-8ee0-e87d85c2fc1c", "Invoice Terms", "Invoice Terms", "");
			this.TermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 13, true);
			this.TermsDropEdit.Name = "TermsDropEdit";
			this.TermsDropEdit.ShowDescriptionBox = false;
			this.TermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TermsDropEdit.TabIndex = 4;
			// 
			// DueDateDateEdit
			// 
			this.DueDateDateEdit.AllowDrop = true;
			this.DueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DueDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DueDateDateEdit, "AH_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((InvoiceBatchHeader)(null)).AH_DueDate)));
			this.DueDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|2998d588-96d0-422e-98b4-a5b54e6fbe7f", "Due Date", "Due Date", "");
			this.DueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 37, true);
			this.DueDateDateEdit.Name = "DueDateDateEdit";
			this.DueDateDateEdit.TabIndex = 2;
			// 
			// InvoiceBatchForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceBatchForm|5877969d-7e6f-44bc-b7fa-d2fc23649f1b", "Invoice Batch");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 599, true);
			this.Controls.Add(this.BatchFilterGroupBox);
			this.Controls.Add(this.BottomButtonPanel);
			this.Controls.Add(this.BatchDatePanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(InvoiceBatchHeader);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 638, true);
			this.Name = "InvoiceBatchForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BatchDatePanel, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.BatchFilterGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterPanel.ResumeLayout(false);
			this.NotificationGridContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BatchInvoiceLinesGrid)).EndInit();
			this.NotificationPanel.ResumeLayout(false);
			this.NotificationPanel.PerformLayout();
			this.BatchFilterGroupBox.ResumeLayout(false);
			this.BottomButtonPanel.ResumeLayout(false);
			this.kFlowLayoutPanel1.ResumeLayout(false);
			this.kFlowLayoutPanel1.PerformLayout();
			this.BatchTotalGroupBox.ResumeLayout(false);
			this.BatchDatePanel.ResumeLayout(false);
			this.BatchDatePanel.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}