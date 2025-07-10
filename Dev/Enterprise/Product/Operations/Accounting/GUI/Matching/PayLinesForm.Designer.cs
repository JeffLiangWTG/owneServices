using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.Matching
{
	partial class PayLinesForm
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.InvoiceLinesToPayGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TotalOSPaidAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLocalPaidAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FiltersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PayAllLinesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ChargeCurrencyFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ChargeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChargeGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChargeCodeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FullyPaySelectedLinesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FullyPayAllLinesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeselectFullyPayAllLinesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceLinesToPayGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLinesGrid)).BeginInit();
			this.FiltersGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 297, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 24, true);
			this.MainStatusBar.TabIndex = 11;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator);
			// 
			// InvoiceLinesToPayGroupBox
			// 
			this.InvoiceLinesToPayGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceLinesToPayGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|d9801b7f-f09c-448e-acee-dbb2ac846321", "Transaction Lines to Match");
			this.InvoiceLinesToPayGroupBox.Controls.Add(this.InvoiceLinesGrid);
			this.InvoiceLinesToPayGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 99, true);
			this.InvoiceLinesToPayGroupBox.Name = "InvoiceLinesToPayGroupBox";
			this.InvoiceLinesToPayGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 137, true);
			this.InvoiceLinesToPayGroupBox.TabIndex = 1;
			this.InvoiceLinesToPayGroupBox.TabStop = false;
			// 
			// InvoiceLinesGrid
			// 
			this.InvoiceLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceLinesGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).IsFullyPay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).GenericCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).AL_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).AL_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).TransactionNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).ConsolidatedInvoiceRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).AL_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).AL_OSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).AL_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).AL_AT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).AL_OSTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).OutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).LocalOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).PaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).LocalPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).ChargeCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).ChargeExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.Base.Transaction.ILineMatching)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).Lines)).SyncRoot)).PaidAmountInChargeCurrency)));
			this.InvoiceLinesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|20eff5fa-2924-4340-9c07-40783a60867e", "Fully Pay");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsFullyPay";
			zCheckBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|e6c04868-f895-47a3-9596-e8d050ba553a", "Charges");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GenericCharge";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AL_JH";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AL_GB";
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AL_GE";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|ea80be7b-2cbe-4cc7-883f-6769e66be2ae", "Transaction");
			zTextBoxColumnStyleInfo1.ColumnName = "TransactionNumber";
			zTextBoxColumnStyleInfo2.ColumnName = "ConsolidatedInvoiceRef";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AL_RX_NKTransactionCurrency";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|4a5036c3-f1c0-424a-9437-dc9dd6008402", "Total Line");
			zCalcEditColumnStyleInfo1.ColumnName = "AL_OSAmount";
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|0abb79b2-3990-4f68-8dbe-068e6894e4a0", "Amt Excl. Tax");
			zCalcEditColumnStyleInfo2.ColumnName = "AL_OSExTaxAmount";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AL_AT";
			zGuidFindBoxColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|5b7e0dd7-25cb-48ce-96dc-dc33e1a30a05", "Tax");
			zCalcEditColumnStyleInfo3.ColumnName = "AL_OSTaxAmount";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|728fca31-65d5-41ce-a989-8af9dd934192", "Outstanding");
			zCalcEditColumnStyleInfo4.ColumnName = "OutstandingAmount";
			zCalcEditColumnStyleInfo4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|b7e208aa-1d99-4eb8-a032-578bfc01dbb8", "Local Outstanding");
			zCalcEditColumnStyleInfo5.ColumnName = "LocalOutstandingAmount";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|a6732927-cfaa-45d8-ac62-366f557d33f8", "Paid Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "PaidAmount";
			zCalcEditColumnStyleInfo6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|4e372f54-6a0e-459a-afc3-3a26547411f4", "Local Paid Amt");
			zCalcEditColumnStyleInfo7.ColumnName = "LocalPaidAmount";
			zCalcEditColumnStyleInfo7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|116adcb8-906c-4ea3-a733-bfb5f500ec23", "Charge Type");
			zTextBoxColumnStyleInfo3.ColumnName = "ChargeType";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|39467333-8f1c-46a5-ba51-e3499e51ff6a", "Charge Amt");
			zCalcEditColumnStyleInfo8.ColumnName = "ChargeAmount";
			zCalcEditColumnStyleInfo8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|7252d125-2d86-4b05-9759-69e6850f9b43", "Charge Cur");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ChargeCurrency";
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|af829483-0e58-4837-af77-487652e7f2d5", "Charge Exch.");
			zCalcEditColumnStyleInfo9.ColumnName = "ChargeExRate";
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|3b10e411-ac7b-4c7c-9a9c-14152414ade2", "Amount Paid Charge Cur");
			zCalcEditColumnStyleInfo10.ColumnName = "PaidAmountInChargeCurrency";
			zCalcEditColumnStyleInfo10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InvoiceLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InvoiceLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.InvoiceLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.InvoiceLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.InvoiceLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.InvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoiceLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.InvoiceLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.InvoiceLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.InvoiceLinesGrid.GridId = "d2f8b0d7-3304-4e39-b7a3-519b9ab966ce";
			this.InvoiceLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceLinesGrid.LayoutKey = "InvoiceLinesGrid";
			this.InvoiceLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvoiceLinesGrid.Name = "InvoiceLinesGrid";
			this.InvoiceLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 118, true);
			this.InvoiceLinesGrid.TabIndex = 0;
			// 
			// TotalOSPaidAmountCalcEdit
			// 
			this.TotalOSPaidAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TotalOSPaidAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|848e5228-603d-4a3f-8e90-f7f7efa558ad", "Total Paid Amount");
			this.BindingSource.SetBindingMember(this.TotalOSPaidAmountCalcEdit, "LineTotalPaidAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).LineTotalPaidAmount)));
			this.TotalOSPaidAmountCalcEdit.DecimalPlaces = 2;
			this.TotalOSPaidAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 270, true);
			this.TotalOSPaidAmountCalcEdit.Name = "TotalOSPaidAmountCalcEdit";
			this.TotalOSPaidAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalOSPaidAmountCalcEdit.TabIndex = 6;
			this.TotalOSPaidAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalLocalPaidAmountCalcEdit
			// 
			this.TotalLocalPaidAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TotalLocalPaidAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|057aa3e7-9d97-4dfe-868b-7cfa2ffcc0ca", "Local Paid Amount");
			this.BindingSource.SetBindingMember(this.TotalLocalPaidAmountCalcEdit, "LineTotalLocalPaidAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).LineTotalLocalPaidAmount)));
			this.TotalLocalPaidAmountCalcEdit.DecimalPlaces = 2;
			this.TotalLocalPaidAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 270, true);
			this.TotalLocalPaidAmountCalcEdit.Name = "TotalLocalPaidAmountCalcEdit";
			this.TotalLocalPaidAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalLocalPaidAmountCalcEdit.TabIndex = 8;
			this.TotalLocalPaidAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|df34d30f-7c5e-4f9c-92d2-09d4eb9a254f", "&OK");
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 268, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 9;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|8f6d7290-b7eb-4f91-ad41-506ec0cb0f0a", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(572, 268, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// FiltersGroupBox
			// 
			this.FiltersGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|02693c9b-fc4e-4f64-8fe7-bd66d07a195f", "Filters");
			this.FiltersGroupBox.Controls.Add(this.PayAllLinesButton);
			this.FiltersGroupBox.Controls.Add(this.ChargeCurrencyFindBox);
			this.FiltersGroupBox.Controls.Add(this.ChargeTypeDropEdit);
			this.FiltersGroupBox.Controls.Add(this.ChargeGroupDropEdit);
			this.FiltersGroupBox.Controls.Add(this.ChargeCodeGuidFindBox);
			this.FiltersGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FiltersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FiltersGroupBox.Name = "FiltersGroupBox";
			this.FiltersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 93, true);
			this.FiltersGroupBox.TabIndex = 0;
			this.FiltersGroupBox.TabStop = false;
			// 
			// PayAllLinesButton
			// 
			this.PayAllLinesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|dff7194c-2da8-4d7e-b665-7f56512207c6", "Pay all lines matching the filter");
			this.PayAllLinesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 61, true);
			this.PayAllLinesButton.Name = "PayAllLinesButton";
			this.PayAllLinesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 23, true);
			this.PayAllLinesButton.TabIndex = 4;
			this.PayAllLinesButton.UseVisualStyleBackColor = true;
			this.PayAllLinesButton.Click += new System.EventHandler(this.PayAllLinesButton_Click);
			// 
			// ChargeCurrencyFindBox
			// 
			this.BindingSource.SetBindingMember(this.ChargeCurrencyFindBox, "ChargeCurrencyFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).ChargeCurrencyFilter)));
			this.ChargeCurrencyFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|a121ca55-95f1-41ac-a05e-2f465a6c0c79", "Charge Currency");
			this.ChargeCurrencyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 12, true);
			this.ChargeCurrencyFindBox.Name = "ChargeCurrencyFindBox";
			this.ChargeCurrencyFindBox.ShowNewFormWhenEmpty = false;
			this.ChargeCurrencyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.ChargeCurrencyFindBox.TabIndex = 3;
			// 
			// ChargeTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ChargeTypeDropEdit, "ChargeTypeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).ChargeTypeFilter)));
			this.ChargeTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|8fdb9a5e-2835-4b2b-b7a7-94eaf1a1f94c", "Charge Type");
			this.ChargeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 64, true);
			this.ChargeTypeDropEdit.Name = "ChargeTypeDropEdit";
			this.ChargeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ChargeTypeDropEdit.TabIndex = 2;
			// 
			// ChargeGroupDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ChargeGroupDropEdit, "ChargeGroupFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).ChargeGroupFilter)));
			this.ChargeGroupDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|5ea35ced-4814-4d09-be26-0e488c3063ad", "Charge Group");
			this.ChargeGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 38, true);
			this.ChargeGroupDropEdit.Name = "ChargeGroupDropEdit";
			this.ChargeGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ChargeGroupDropEdit.TabIndex = 1;
			// 
			// ChargeCodeGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.ChargeCodeGuidFindBox, "ChargeCodeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator)(null)).ChargeCodeFilter)));
			this.ChargeCodeGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|a929dbf6-ad81-43e1-bd26-bc086572e82d", "Charge Code");
			this.ChargeCodeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 12, true);
			this.ChargeCodeGuidFindBox.Name = "ChargeCodeGuidFindBox";
			this.ChargeCodeGuidFindBox.ShowNewFormWhenEmpty = false;
			this.ChargeCodeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ChargeCodeGuidFindBox.TabIndex = 0;
			// 
			// FullyPaySelectedLinesButton
			// 
			this.FullyPaySelectedLinesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FullyPaySelectedLinesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|ab20c2f8-3a11-410f-aba3-4f0adb7d331b", "Fully Pay Selected Lines");
			this.FullyPaySelectedLinesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 239, true);
			this.FullyPaySelectedLinesButton.Name = "FullyPaySelectedLinesButton";
			this.FullyPaySelectedLinesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 23, true);
			this.FullyPaySelectedLinesButton.TabIndex = 2;
			this.FullyPaySelectedLinesButton.UseVisualStyleBackColor = true;
			this.FullyPaySelectedLinesButton.Click += new System.EventHandler(this.FullyPaySelectedLinesButton_Click);
			// 
			// FullyPayAllLinesButton
			// 
			this.FullyPayAllLinesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FullyPayAllLinesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|268c5e82-7b59-4e97-aa51-1452412697af", "Fully Pay All Lines");
			this.FullyPayAllLinesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 239, true);
			this.FullyPayAllLinesButton.Name = "FullyPayAllLinesButton";
			this.FullyPayAllLinesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
			this.FullyPayAllLinesButton.TabIndex = 3;
			this.FullyPayAllLinesButton.UseVisualStyleBackColor = true;
			this.FullyPayAllLinesButton.Click += new System.EventHandler(this.FullyPayAllLinesButton_Click);
			// 
			// DeselectFullyPayAllLinesButton
			// 
			this.DeselectFullyPayAllLinesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeselectFullyPayAllLinesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|12e44e48-a1a8-4829-b6f9-eb02f9c42e2a", "Deselect Fully Pay All Lines");
			this.DeselectFullyPayAllLinesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 239, true);
			this.DeselectFullyPayAllLinesButton.Name = "DeselectFullyPayAllLinesButton";
			this.DeselectFullyPayAllLinesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 23, true);
			this.DeselectFullyPayAllLinesButton.TabIndex = 4;
			this.DeselectFullyPayAllLinesButton.UseVisualStyleBackColor = true;
			this.DeselectFullyPayAllLinesButton.Click += new System.EventHandler(this.DeselectFullyPayAllLinesButton_Click);
			// 
			// PayLinesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 321, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayLinesForm|8d7f464e-ba30-4e25-b8e0-1f226da5ac48", "Match Transaction Lines");
			this.Controls.Add(this.DeselectFullyPayAllLinesButton);
			this.Controls.Add(this.FullyPayAllLinesButton);
			this.Controls.Add(this.FullyPaySelectedLinesButton);
			this.Controls.Add(this.FiltersGroupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.TotalLocalPaidAmountCalcEdit);
			this.Controls.Add(this.TotalOSPaidAmountCalcEdit);
			this.Controls.Add(this.InvoiceLinesToPayGroupBox);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBasePayLineMediator);
			this.IsPostOnly = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 357, true);
			this.Name = "PayLinesForm";
			this.Text = "PayLinesForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InvoiceLinesToPayGroupBox, 0);
			this.Controls.SetChildIndex(this.TotalOSPaidAmountCalcEdit, 0);
			this.Controls.SetChildIndex(this.TotalLocalPaidAmountCalcEdit, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.FiltersGroupBox, 0);
			this.Controls.SetChildIndex(this.FullyPaySelectedLinesButton, 0);
			this.Controls.SetChildIndex(this.FullyPayAllLinesButton, 0);
			this.Controls.SetChildIndex(this.DeselectFullyPayAllLinesButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceLinesToPayGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLinesGrid)).EndInit();
			this.FiltersGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceLinesToPayGroupBox;
		private Enterprise.ZArchitecture.ZGrid InvoiceLinesGrid;
		private Enterprise.ZArchitecture.ZCalcEdit TotalOSPaidAmountCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit TotalLocalPaidAmountCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZButton okButton;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox FiltersGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ChargeCurrencyFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChargeTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ChargeGroupDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ChargeCodeGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton PayAllLinesButton;
		private Enterprise.ZArchitecture.GUI.ZButton FullyPaySelectedLinesButton;
		private Enterprise.ZArchitecture.GUI.ZButton FullyPayAllLinesButton;
		private Enterprise.ZArchitecture.GUI.ZButton DeselectFullyPayAllLinesButton;
	}
}
