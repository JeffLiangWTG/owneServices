using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class APInvoicePrintingUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CreditorFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.JobHeaderFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.TransactionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.InvoiceHidingMessageLabel = new Enterprise.ZArchitecture.ZLabel();
            this.GridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.APInvoicesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CreditorFindBox.SuspendLayout();
            this.JobHeaderFindBox.SuspendLayout();
            this.TransactionTypeDropEdit.SuspendLayout();
            this.FilterPanel.SuspendLayout();
            this.GridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.APInvoicesGrid)).BeginInit();
            this.APInvoicesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter);
            // 
            // ClearButton
            // 
            this.ClearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|28b5b20a-6763-4057-a7fc-1862134dd0e9", "Clear");
            this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 59, true);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.ClearButton.TabIndex = 12;
            this.ClearButton.ToolTipCaption = null;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // FindButton
            // 
            this.FindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|4d4c3bd0-cce5-4856-9c3b-4b1a87bfeb6b", "Find");
            this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 59, true);
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.FindButton.TabIndex = 11;
            this.FindButton.ToolTipCaption = null;
            this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
            // 
            // CreditorFindBox
            // 
            this.CreditorFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CreditorFindBox, "DebtorOrCreditor");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).DebtorOrCreditor)));
            this.CreditorFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|114175c9-5308-4964-bbeb-1a489169a913", "Creditor", "Creditor", "");
            this.CreditorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 11, true);
            this.CreditorFindBox.Name = "CreditorFindBox";
            this.CreditorFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CreditorFindBox.ParentType = null;
            this.CreditorFindBox.PopupCaption = null;
            this.CreditorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 17, true);
            this.CreditorFindBox.TabIndex = 8;
            // 
            // JobHeaderFindBox
            // 
            this.JobHeaderFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.JobHeaderFindBox, "JobNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).JobNumber)));
            this.JobHeaderFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|f2ca8b3f-8396-441d-b0f8-9aa1ffac0bb1", "Job Number", "Job Number", "");
            this.JobHeaderFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 59, true);
            this.JobHeaderFindBox.Name = "JobHeaderFindBox";
            this.JobHeaderFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.JobHeaderFindBox.ParentType = null;
            this.JobHeaderFindBox.PopupCaption = null;
            this.JobHeaderFindBox.ShowDescriptionBox = false;
            this.JobHeaderFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
            this.JobHeaderFindBox.TabIndex = 10;
            // 
            // TransactionTypeDropEdit
            // 
            this.TransactionTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransactionTypeDropEdit, "TransactionType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).TransactionType)));
            this.TransactionTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|716f2fb5-0287-4bbc-ae26-6191e33fb39f", "Transaction Type", "Transaction Type", "");
            this.TransactionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 35, true);
            this.TransactionTypeDropEdit.Name = "TransactionTypeDropEdit";
            this.TransactionTypeDropEdit.PreBoundMaxLength = 3;
            this.TransactionTypeDropEdit.ShowDescriptionBox = false;
            this.TransactionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
            this.TransactionTypeDropEdit.TabIndex = 9;
            // 
            // FilterPanel
            // 
            this.FilterPanel.Controls.Add(this.InvoiceHidingMessageLabel);
            this.FilterPanel.Controls.Add(this.JobHeaderFindBox);
            this.FilterPanel.Controls.Add(this.ClearButton);
            this.FilterPanel.Controls.Add(this.TransactionTypeDropEdit);
            this.FilterPanel.Controls.Add(this.FindButton);
            this.FilterPanel.Controls.Add(this.CreditorFindBox);
            this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FilterPanel.Name = "FilterPanel";
            this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 112, true);
            this.FilterPanel.TabIndex = 13;
            // 
            // InvoiceHidingMessageLabel
            // 
            this.InvoiceHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("856676fc-2103-47e2-b7c1-2b7a35ff5439", "Invoice/credit notes with Job Header branch / dept outside your login permission " +
        "are not listed.");
            this.InvoiceHidingMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.InvoiceHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 92, true);
            this.InvoiceHidingMessageLabel.Name = "InvoiceHidingMessageLabel";
            this.InvoiceHidingMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 15, true);
            this.InvoiceHidingMessageLabel.TabIndex = 0;
            this.InvoiceHidingMessageLabel.UseMnemonic = false;
            this.InvoiceHidingMessageLabel.Visible = false;
            // 
            // GridPanel
            // 
            this.GridPanel.Controls.Add(this.APInvoicesGrid);
            this.GridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
            this.GridPanel.Name = "GridPanel";
            this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 350, true);
            this.GridPanel.TabIndex = 14;
            // 
            // APInvoicesGrid
            // 
            this.APInvoicesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.APInvoicesGrid, "FilteredTransactions");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_ChequeOrReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_OH)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_Ledger)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_TransactionType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_ConsolidatedInvoiceRef)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_TransactionNum)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_PostDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_InvoiceDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_DueDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_FullyPaidDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_Calc_OSOutstandingAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_RX_NKTransactionCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_OSTotalAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_LocalTotalAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).RelatedTransactionDebtorsAsString)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).CreatingUser)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_RequisitionStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_RequisitionDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_ComplianceSubType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).DisplayInvoiceAddressOverride)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).DisplayInvoiceContactOverride)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_TransactionReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_GB)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.JobAPInvoicePrintingFilter)(null)).FilteredTransactions)).SyncRoot)).AH_GB_TaxBranch)));
            this.APInvoicesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2710aa5c-cee1-4134-994d-45d1abf0da55", "Sup. Cost Ref.", "Supplier Cost Reference", "");
            zTextBoxColumnStyleInfo1.ColumnName = "AH_ChequeOrReference";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
            zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.ColumnName = "AH_Ledger";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionType";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|3ad53e1e-953d-43a0-93e1-185233c981ef", "Internal Reference Number");
            zTextBoxColumnStyleInfo4.ColumnName = "AH_ConsolidatedInvoiceRef";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.ColumnName = "AH_TransactionNum";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.ColumnName = "AH_PostDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo2.ColumnName = "AH_InvoiceDate";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo3.ColumnName = "AH_DueDate";
            zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo4.ColumnName = "AH_FullyPaidDate";
            zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|22d2df7c-cd40-4e82-871e-65c6c37a9ec9", "Outstanding Amt");
            zCalcEditColumnStyleInfo1.ColumnName = "AH_Calc_OSOutstandingAmount";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|4cbd4fdc-480a-47c1-acca-2962afe3eee7", "Invoice Amount");
            zCalcEditColumnStyleInfo2.ColumnName = "AH_OSTotalAmount";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|cacbd0a2-b38f-4e8f-8a14-05d76fae2e5a", "Local Invoice Amount");
            zCalcEditColumnStyleInfo3.ColumnName = "AH_LocalTotalAmount";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|7ff1480a-66ce-4359-a442-06dc94255de9", "Related Transaction Debtors");
            zTextBoxColumnStyleInfo6.ColumnName = "RelatedTransactionDebtorsAsString";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.IsVisible = false;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoicePrintingUserControl|d0d54036-878b-40b8-acd3-ad8c5170818b", "Creating User Full Name");
            zTextBoxColumnStyleInfo7.ColumnName = "CreatingUser";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.IsVisible = false;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo8.ColumnName = "AH_RequisitionStatus";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo5.ColumnName = "AH_RequisitionDate";
            zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo9.ColumnName = "AH_ComplianceSubType";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("72c02818-8a02-4658-a10c-7f420adc5c4d", "Address");
            zGuidFindBoxColumnStyleInfo1.ColumnName = "DisplayInvoiceAddressOverride";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bb6dc41b-d799-4032-bf6d-c09ac834bd26", "Contact");
            zGuidFindBoxColumnStyleInfo2.ColumnName = "DisplayInvoiceContactOverride";
            zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d8536a4b-1242-4e8f-a98a-a8508729cd95", "Compliance Number");
            zTextBoxColumnStyleInfo10.ColumnName = "AH_TransactionReference";
            zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo3.ColumnName = "AH_GB";
            zGuidFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
            zGuidFindBoxColumnStyleInfo4.ColumnName = "AH_GB_TaxBranch";
            zGuidFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo11.ColumnName = "EInvoicingBatchNumber";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo6.ColumnName = "EInvoicingLastResponseReceivedUtc";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo7.ColumnName = "EInvoicingLastSentTimeUtc";
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "EInvoicingAuthorisationNumber";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "EInvoicingError";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "EInvoicingeHubAllocatedNumber";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.ColumnName = "EInvoicingGovernmentAllocatedNumber";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.ColumnName = "EInvoicingStatus";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.APInvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.APInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.APInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.APInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
            this.APInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
            this.APInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.APInvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.APInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.APInvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.APInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.APInvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.APInvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
            this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.APInvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
            this.APInvoicesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.APInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.APInvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.APInvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.APInvoicesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.APInvoicesGrid.GridId = "491792f8-4355-45d7-b03f-d03c0a9915af";
            this.APInvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.APInvoicesGrid.LayoutKey = "APInvoicesGrid";
            this.APInvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.APInvoicesGrid.Name = "APInvoicesGrid";
            this.APInvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 350, true);
            this.APInvoicesGrid.TabIndex = 1;
            // 
            // APInvoicePrintingUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.GridPanel);
            this.Controls.Add(this.FilterPanel);
            this.Name = "APInvoicePrintingUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 462, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CreditorFindBox.ResumeLayout(true);
            this.CreditorFindBox.PerformLayout();
            this.JobHeaderFindBox.ResumeLayout(true);
            this.JobHeaderFindBox.PerformLayout();
            this.TransactionTypeDropEdit.ResumeLayout(true);
            this.TransactionTypeDropEdit.PerformLayout();
            this.FilterPanel.ResumeLayout(false);
            this.FilterPanel.PerformLayout();
            this.GridPanel.ResumeLayout(false);
            this.GridPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.APInvoicesGrid)).EndInit();
            this.APInvoicesGrid.ResumeLayout(false);
            this.APInvoicesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid APInvoicesGrid;
		private Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		private Enterprise.ZArchitecture.GUI.ZButton FindButton;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CreditorFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox JobHeaderFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TransactionTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel FilterPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel GridPanel;
		private Enterprise.ZArchitecture.ZLabel InvoiceHidingMessageLabel;
	}
}
