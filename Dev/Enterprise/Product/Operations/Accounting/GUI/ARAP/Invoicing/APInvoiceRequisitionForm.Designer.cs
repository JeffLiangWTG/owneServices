namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	partial class APInvoiceRequisitionForm
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
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InvoiceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 209, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|85bad99a-d43e-43ec-991d-be91e559f80f", "Cancel");
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 178, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 2;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|e06c1ce0-0502-45a1-9a1f-a6d720bc7ad3", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 178, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// InvoiceGrid
			// 
			this.InvoiceGrid.AllowNavigation = false;
			this.InvoiceGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoiceGrid, "Collection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).HeaderFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_DueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_RequisitionStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_RequisitionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_OSTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_TransactionReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).RelatedTransactionDebtorsAsString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionHeader)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder)(null)).Collection)).SyncRoot)).AH_Ledger)));
			this.InvoiceGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|f1b24f41-0970-4721-9ba3-671253de161f", "Account Code");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|ffc1f13b-7f60-48bb-8090-3fb56fa1e76c", "Account Full Name");
			zTextBoxColumnStyleInfo1.ColumnName = "HeaderFullName";
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|63eac66c-70e3-4862-8d9f-57a73ccd46c7", "Transaction Number");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionNum";
			zDateEditColumnStyleInfo1.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.ColumnName = "AH_DueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|296028d9-0497-455b-8b6e-22c7b0a71999", "Payment Requisition Status");
			zDropEditColumnStyleInfo1.ColumnName = "AH_RequisitionStatus";
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|cab419ea-ff80-4b73-bf53-92e8569a24b6", "Payment Requisition Date");
			zDateEditColumnStyleInfo3.ColumnName = "AH_RequisitionDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo4.ColumnName = "AH_RX_NKTransactionCurrency";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|636dbe3a-1b6d-4aa3-a0a4-ddad8f97c93f", "Invoice Total Excluding Tax");
			zCalcEditColumnStyleInfo2.ColumnName = "AH_OSExTaxAmount";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|cf1ef421-1b9b-46b7-b81b-b5b8b2236955", "Invoice Tax Total");
			zCalcEditColumnStyleInfo3.ColumnName = "AH_OSTaxAmount";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|3cc2af1d-ff3e-4050-8dc3-be0d74b586d9", "Internal Reference");
			zTextBoxColumnStyleInfo5.ColumnName = "AH_TransactionReference";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|5df14e74-bafe-4b93-b2a2-b9d32e25001f", "Related Transaction Debtors");
			zTextBoxColumnStyleInfo6.ColumnName = "RelatedTransactionDebtorsAsString";
			zTextBoxColumnStyleInfo7.ColumnName = "AH_Ledger";
			this.InvoiceGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.InvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoiceGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InvoiceGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.InvoiceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.InvoiceGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.InvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.InvoiceGrid.CopySelectedRowsAllowed = true;
			this.InvoiceGrid.GridId = "ee2b00ed-1628-43cb-bc14-6ec2a857796f";
			this.InvoiceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceGrid.LayoutKey = "InvoiceGrid";
			this.InvoiceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceGrid.Name = "InvoiceGrid";
			this.InvoiceGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoiceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 172, true);
			this.InvoiceGrid.TabIndex = 0;
			// 
			// APInvoiceRequisitionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APInvoiceRequisitionForm|f0e60cff-849b-4b47-b912-d82681eef18f", "Payment Requisition Status Override");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 233, true);
			this.Controls.Add(this.InvoiceGrid);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.APTransactionHeaderCollectionHolder);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 271, true);
			this.Name = "APInvoiceRequisitionForm";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.InvoiceGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.ZGrid InvoiceGrid;
	}
}
