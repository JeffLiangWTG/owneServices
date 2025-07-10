using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI
{
	partial class InvoiceBulkBatchForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.BatchInvoiceLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.BatchInvoicesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BatchInvoicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NotificationGridContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BatchInvoiceLinesGrid)).BeginInit();
			this.NotificationPanel.SuspendLayout();
			this.FilterPanel.SuspendLayout();
			this.BatchFilterGroupBox.SuspendLayout();
			this.BottomButtonPanel.SuspendLayout();
			this.BatchDatePanel.SuspendLayout();
			this.BatchTotalGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BatchInvoiceLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zSplitContainer)).BeginInit();
			this.zSplitContainer.Panel1.SuspendLayout();
			this.zSplitContainer.Panel2.SuspendLayout();
			this.zSplitContainer.SuspendLayout();
			this.BatchInvoicesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BatchInvoicesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			// 
			// NotificationGridContainer
			// 
			this.NotificationGridContainer.Controls.Add(this.zSplitContainer);
			this.NotificationGridContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.NotificationGridContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 230, true);
			this.NotificationGridContainer.Controls.SetChildIndex(this.NotificationPanel, 0);
			this.NotificationGridContainer.Controls.SetChildIndex(this.zSplitContainer, 0);
			// 
			// BatchInvoiceLinesGrid
			// 
			this.BindingSource.SetBindingMember(this.BatchInvoiceLinesGrid, "InvoiceBatchHeaders.Line");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).IncludeInTheBatch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_InvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_GSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_OSExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).Line)).SyncRoot)).AH_LocalExtraTaxAmount)));
			this.BatchInvoiceLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BatchInvoiceLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 74, true);
			// 
			// FilterPanel
			// 
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 403, true);
			// 
			// CancelledBatchLabel
			// 
			this.CancelledBatchLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 23, true);
			// 
			// DateEdit
			// 
			this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 11, true);
			// 
			// BatchNumberTextBox
			// 
			this.BatchNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(577, 173, true);
			this.BatchNumberTextBox.Visible = false;
			// 
			// BatchFilterGroupBox
			// 
			this.BatchFilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 71, true);
			this.BatchFilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 422, true);
			// 
			// BatchDatePanel
			// 
			this.BatchDatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 71, true);
			// 
			// DueDateDateEdit
			// 
			this.DueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 97, true);
			this.DueDateDateEdit.Visible = false;
			// 
			// TermDaysCalcEdit
			// 
			this.TermDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(710, 198, true);
			this.TermDaysCalcEdit.Visible = false;
			// 
			// TermsDropEdit
			// 
			this.TermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 198, true);
			this.TermsDropEdit.Visible = false;
			// 
			// DebtorGuidFindBox
			// 
			this.DebtorGuidFindBox.Enabled = false;
			this.DebtorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 173, true);
			this.DebtorGuidFindBox.Visible = false;
			// 
			// JobTypeCheckedListBox
			// 
			this.JobTypeCheckedListBox.ColumnWidth = 137;
			this.JobTypeCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 8, true);
			this.JobTypeCheckedListBox.MultiColumn = true;
			this.JobTypeCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 49, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch);
			// 
			// BatchInvoiceLinesGroupBox
			// 
			this.BatchInvoiceLinesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("42c4aeda-46f6-4951-a377-6042c9edad0f", "Batch Invoices List");
			this.BatchInvoiceLinesGroupBox.Controls.Add(this.BatchInvoiceLinesGrid);
			this.BatchInvoiceLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BatchInvoiceLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BatchInvoiceLinesGroupBox.Name = "BatchInvoiceLinesGroupBox";
			this.BatchInvoiceLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 93, true);
			this.BatchInvoiceLinesGroupBox.TabIndex = 10;
			this.BatchInvoiceLinesGroupBox.TabStop = false;
			// 
			// zSplitContainer
			// 
			this.zSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.zSplitContainer.Name = "zSplitContainer";
			this.zSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// zSplitContainer.Panel1
			// 
			this.zSplitContainer.Panel1.Controls.Add(this.BatchInvoicesGroupBox);
			this.zSplitContainer.Panel1MinSize = 80;
			// 
			// zSplitContainer.Panel2
			// 
			this.zSplitContainer.Panel2.Controls.Add(this.BatchInvoiceLinesGroupBox);
			this.zSplitContainer.Panel2MinSize = 80;
			this.zSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 198, true);
			this.zSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(101);
			this.zSplitContainer.TabIndex = 11;
			// 
			// BatchInvoicesGroupBox
			// 
			this.BatchInvoicesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ebd99490-46b1-4430-8919-12f16a183ebc", "Batch Invoices");
			this.BatchInvoicesGroupBox.Controls.Add(this.BatchInvoicesGrid);
			this.BatchInvoicesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BatchInvoicesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BatchInvoicesGroupBox.Name = "BatchInvoicesGroupBox";
			this.BatchInvoicesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 101, true);
			this.BatchInvoicesGroupBox.TabIndex = 4;
			this.BatchInvoicesGroupBox.TabStop = false;
			// 
			// BatchInvoicesGrid
			// 
			this.BatchInvoicesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BatchInvoicesGrid, "InvoiceBatchHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_InvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_GSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_InvoiceTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBatchHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch)(null)).InvoiceBatchHeaders)).SyncRoot)).AH_InvoiceTermDays)));
			this.BatchInvoicesGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo2.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "AH_InvoiceAmount";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "AH_GSTAmount";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "AH_InvoiceTerm";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "AH_InvoiceTermDays";
			this.BatchInvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.BatchInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BatchInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.BatchInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.BatchInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.BatchInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.BatchInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.BatchInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.BatchInvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.BatchInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BatchInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.BatchInvoicesGrid.CopySelectedRowsAllowed = true;
			this.BatchInvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BatchInvoicesGrid.GridId = "dc5ec84b-b6a4-4580-abfd-1b454b826662";
			this.BatchInvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BatchInvoicesGrid.IsWholeRowSelectedOnClick = true;
			this.BatchInvoicesGrid.LayoutKey = "BatchInvoicesGrid";
			this.BatchInvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BatchInvoicesGrid.Name = "BatchInvoicesGrid";
			this.BatchInvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 82, true);
			this.BatchInvoicesGrid.TabIndex = 1;
			// 
			// InvoiceBulkBatchForm
			// 
			this.AutoAddPreviousNextButtons = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 599, true);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Invoicing.InvoiceBulkBatch";
			this.Name = "InvoiceBulkBatchForm";
			this.NotificationGridContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BatchInvoiceLinesGrid)).EndInit();
			this.NotificationPanel.ResumeLayout(false);
			this.NotificationPanel.PerformLayout();
			this.FilterPanel.ResumeLayout(false);
			this.BatchFilterGroupBox.ResumeLayout(false);
			this.BottomButtonPanel.ResumeLayout(false);
			this.BatchDatePanel.ResumeLayout(false);
			this.BatchDatePanel.PerformLayout();
			this.BatchTotalGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BatchInvoiceLinesGroupBox.ResumeLayout(false);
			this.zSplitContainer.Panel1.ResumeLayout(false);
			this.zSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zSplitContainer)).EndInit();
			this.zSplitContainer.ResumeLayout(false);
			this.BatchInvoicesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BatchInvoicesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox BatchInvoicesGroupBox;
		protected ZArchitecture.ZGrid BatchInvoicesGrid;
		protected CargoWise.Windows.UI.KSplitContainer zSplitContainer;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox BatchInvoiceLinesGroupBox;
	}
}
