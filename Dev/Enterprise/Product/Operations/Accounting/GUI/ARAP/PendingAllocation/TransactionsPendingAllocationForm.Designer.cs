namespace Enterprise.Accounting.GUI.ARAP
{
    partial class TransactionsPendingAllocationForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TransactionsPendingAllocationlGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.BatchTotalsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LocalInvoiceTotalAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.LocalTotalTaxAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransactionsPendingAllocationlGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.BatchTotalsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation);
			// 
			// TransactionsPendingAllocationlGroupBox
			// 
			this.TransactionsPendingAllocationlGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.TransactionsPendingAllocationlGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|6c9ef639-0b68-44ef-bfd9-280ed311ae07", "Invoices Pending Allocation");
			this.TransactionsPendingAllocationlGroupBox.Controls.Add(this.zGrid1);
			this.TransactionsPendingAllocationlGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.TransactionsPendingAllocationlGroupBox.Name = "TransactionsPendingAllocationlGroupBox";
			this.TransactionsPendingAllocationlGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 344, true);
			this.TransactionsPendingAllocationlGroupBox.TabIndex = 0;
			this.TransactionsPendingAllocationlGroupBox.TabStop = false;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "Transactions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).ExchangeRate.Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).ExchangeRate.Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionPendingAllocation)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation)(null)).Transactions)).SyncRoot)).AH_OSTaxAmount)));
			this.zGrid1.CaptionVisible = false;
			zDateEditColumnStyleInfo4.ColumnName = "AH_InvoiceDate";
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|6658318e-01c0-4b96-8f93-bf070248995e", "Creditor");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "AH_OH";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|289d3c36-ea72-48f3-b9b6-e2a9e576cb5a", "Transaction Num.", "Transaction Number");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionNum";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|c3e7016f-aabc-45b7-8631-9c5e360b58f0", "Currency");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ExchangeRate+Currency";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|e1d3a581-d86a-4ab6-a1c9-16c7e600fd54", "Ex. Rate");
			zCalcEditColumnStyleInfo4.ColumnName = "ExchangeRate+Rate";
			zDateEditColumnStyleInfo5.ColumnName = "AH_DueDate";
			zTextBoxColumnStyleInfo4.ColumnName = "AH_Desc";
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|a9c9a8f8-688a-436d-8b09-0b139d1007e6", "Post Date");
			zDateEditColumnStyleInfo6.ColumnName = "AH_PostDate";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AH_GB";
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AH_GE";
			zGuidDropEditColumnStyleInfo1.ColumnName = "DisplayInvoiceAddressOverride";
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|d57df5d8-c022-425f-b092-ae4dc3f71fe4", "Address");
			zGuidDropEditColumnStyleInfo2.ColumnName = "DisplayInvoiceContactOverride";
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|c8797d1a-846d-41cd-bb40-bcb69ffe3804", "Contact");
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|d84bfc10-f8f5-4946-8539-0ddd5b6c5d55", "Amount Excluding Tax");
			zCalcEditColumnStyleInfo5.ColumnName = "AH_OSExTaxAmount";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|1f7ccf08-07c1-400a-b528-5b4969b4ddf8", "Tax Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "AH_OSTaxAmount";
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.zGrid1.GridId = "d8df5915-71c3-4877-a932-c1a91ffb920e";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 325, true);
			this.zGrid1.TabIndex = 0;
			// 
			// BatchTotalsGroupBox
			// 
			this.BatchTotalsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BatchTotalsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|0ccc3ce7-c1e5-4523-b41f-bd302fcd1aae", "Batch Totals");
			this.BatchTotalsGroupBox.Controls.Add(this.LocalInvoiceTotalAmountCalcFindBox);
			this.BatchTotalsGroupBox.Controls.Add(this.LocalTotalTaxAmountCalcFindBox);
			this.BatchTotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 353, true);
			this.BatchTotalsGroupBox.Name = "BatchTotalsGroupBox";
			this.BatchTotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 67, true);
			this.BatchTotalsGroupBox.TabIndex = 1;
			this.BatchTotalsGroupBox.TabStop = false;
			// 
			// LocalInvoiceTotalAmountCalcFindBox
			// 
			this.LocalInvoiceTotalAmountCalcFindBox.BindToAmount = "BatchLocalTaxTotal";
			this.LocalInvoiceTotalAmountCalcFindBox.BindToUnit = "Currency";
			this.LocalInvoiceTotalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|e23a2be7-ccf9-4852-83bc-098747bbe7e1", "Tax Amount");
			this.LocalInvoiceTotalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalInvoiceTotalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 36, true);
			this.LocalInvoiceTotalAmountCalcFindBox.Name = "LocalInvoiceTotalAmountCalcFindBox";
			this.LocalInvoiceTotalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.LocalInvoiceTotalAmountCalcFindBox.TabIndex = 1;
			// 
			// LocalTotalTaxAmountCalcFindBox
			// 
			this.LocalTotalTaxAmountCalcFindBox.BindToAmount = "BatchLocalTotal";
			this.LocalTotalTaxAmountCalcFindBox.BindToUnit = "Currency";
			this.LocalTotalTaxAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|81168d93-cd1a-4a4d-af4f-15270948f540", "Invoice Amount");
			this.LocalTotalTaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalTotalTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 13, true);
			this.LocalTotalTaxAmountCalcFindBox.Name = "LocalTotalTaxAmountCalcFindBox";
			this.LocalTotalTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.LocalTotalTaxAmountCalcFindBox.TabIndex = 0;
			// 
			// PostingButtons
			// 
			this.PostingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 386, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtons.TabIndex = 2;
			// 
			// TransactionsPendingAllocationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 450, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionsPendingAllocationForm|58da29fa-a4bb-431d-a620-c2a1f504b1b3", "Bulk Unallocated Transactions");
			this.Controls.Add(this.TransactionsPendingAllocationlGroupBox);
			this.Controls.Add(this.BatchTotalsGroupBox);
			this.Controls.Add(this.PostingButtons);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.TransactionsPendingAllocation);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 460, true);
			this.Name = "TransactionsPendingAllocationForm";
			this.Text = "TransactionsPendingApprovalForm";
			this.Controls.SetChildIndex(this.PostingButtons, 0);
			this.Controls.SetChildIndex(this.BatchTotalsGroupBox, 0);
			this.Controls.SetChildIndex(this.TransactionsPendingAllocationlGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransactionsPendingAllocationlGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.BatchTotalsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

        private Enterprise.ZArchitecture.GUI.ZGroupBox TransactionsPendingAllocationlGroupBox;
        private Enterprise.ZArchitecture.ZGrid zGrid1;
        private Enterprise.ZArchitecture.GUI.ZGroupBox BatchTotalsGroupBox;
        private Enterprise.ZArchitecture.GUI.ZCalcFindBox LocalInvoiceTotalAmountCalcFindBox;
        private Enterprise.ZArchitecture.GUI.ZCalcFindBox LocalTotalTaxAmountCalcFindBox;
        private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtons;
    }
}
