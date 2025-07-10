using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class APCashAdvanceViewForm
	{
		#region Windows Form Designer generated code

		private ZPanel BottomPanel;
		private ZGroupBox TotalsGroupBox;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZGrid ChargesGrid;
		private ZGroupBox InvoiceSummaryGroupBox;
		private ZTextBox CreditorsTextBox;
		private ZTextBox numberTextBox;
		private Container components = null;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.InvoiceSummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.statusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dateCreatedTextBox = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.numberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreditorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TotalsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TotalAmtOnInvoiceForJobCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalInvoiceAmountIncGSTCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GSTTaxCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GSTCostTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.ChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceSummaryGroupBox.SuspendLayout();
			this.dateCreatedTextBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TotalsGroupBox.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
			this.ChargesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 435, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader);
			// 
			// InvoiceSummaryGroupBox
			// 
			this.InvoiceSummaryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceViewForm|168f372c-fe11-4a25-9176-57437426d90a", "Advance Payment Request Summary");
			this.InvoiceSummaryGroupBox.Controls.Add(this.statusTextBox);
			this.InvoiceSummaryGroupBox.Controls.Add(this.dateCreatedTextBox);
			this.InvoiceSummaryGroupBox.Controls.Add(this.numberTextBox);
			this.InvoiceSummaryGroupBox.Controls.Add(this.CreditorsTextBox);
			this.InvoiceSummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceSummaryGroupBox.Name = "InvoiceSummaryGroupBox";
			this.InvoiceSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 101, true);
			this.InvoiceSummaryGroupBox.TabIndex = 0;
			this.InvoiceSummaryGroupBox.TabStop = false;
			// 
			// statusTextBox
			// 
			this.statusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.statusTextBox, "StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).StatusDescription)));
			this.statusTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("931a3a82-a2ac-46ac-8f8e-c916adf3d9dd", "Status");
			this.statusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 45, true);
			this.statusTextBox.Name = "statusTextBox";
			this.statusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 44, true);
			this.statusTextBox.TabIndex = 7;
			this.statusTextBox.Tag = "";
			// 
			// dateCreatedTextBox
			// 
			this.dateCreatedTextBox.AllowDrop = true;
			this.dateCreatedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.dateCreatedTextBox.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.dateCreatedTextBox, "CreatedDateTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).CAH_SystemCreateTimeUtc)));
			this.dateCreatedTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0b76e85c-83a2-4409-99fc-e62fe79615e4", "Date Created");
			this.dateCreatedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 19, true);
			this.dateCreatedTextBox.Name = "dateCreatedTextBox";
			this.dateCreatedTextBox.TabIndex = 6;
			this.dateCreatedTextBox.Tag = "";
			// 
			// numberTextBox
			// 
			this.BindingSource.SetBindingMember(this.numberTextBox, "CAH_RequestReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).CAH_RequestReferenceNumber)));
			this.numberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceViewForm|56696843-761b-4bea-8b71-861f3e4ecdbd", "Number");
			this.numberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 45, true);
			this.numberTextBox.Name = "numberTextBox";
			this.numberTextBox.ReadOnly = true;
			this.numberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 44, true);
			this.numberTextBox.TabIndex = 5;
			this.numberTextBox.Tag = "";
			// 
			// CreditorsTextBox
			// 
			this.CreditorsTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorsTextBox, "OrganizationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).OrganizationCode)));
			this.CreditorsTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceViewForm|92a1222d-f447-40f5-96a9-fab7a3889f9f", "Creditor");
			this.CreditorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 19, true);
			this.CreditorsTextBox.Name = "CreditorsTextBox";
			this.CreditorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 44, true);
			this.CreditorsTextBox.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.TotalsGroupBox);
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 312, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 123, true);
			this.BottomPanel.TabIndex = 4;
			// 
			// TotalsGroupBox
			// 
			this.TotalsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceViewForm|b66aafe7-f745-44ea-8858-4419c0eea1af", "Totals");
			this.TotalsGroupBox.Controls.Add(this.TotalAmtOnInvoiceForJobCurrencyTextBox);
			this.TotalsGroupBox.Controls.Add(this.TotalInvoiceAmountIncGSTCalcEdit);
			this.TotalsGroupBox.Controls.Add(this.GSTTaxCurrencyTextBox);
			this.TotalsGroupBox.Controls.Add(this.GSTCostTaxAmountCalcEdit);
			this.TotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 5, true);
			this.TotalsGroupBox.Name = "TotalsGroupBox";
			this.TotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 83, true);
			this.TotalsGroupBox.TabIndex = 4;
			this.TotalsGroupBox.TabStop = false;
			// 
			// TotalAmtOnInvoiceForJobCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalAmtOnInvoiceForJobCurrencyTextBox, "CAH_RX_NKTransactionCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).CAH_RX_NKTransactionCurrency)));
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("34c1591c-21c5-4117-8c8a-74d0c13899a6", "Cost Currency");
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 45, true);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Name = "TotalAmtOnInvoiceForJobCurrencyTextBox";
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 44, true);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.TabIndex = 9;
			// 
			// TotalInvoiceAmountIncGSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalInvoiceAmountIncGSTCalcEdit, "CAH_OSAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).CAH_OSAmount)));
			this.TotalInvoiceAmountIncGSTCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3c9c448d-8066-4b5b-90e9-113703da1c65", "Invoice Amount");
			this.TotalInvoiceAmountIncGSTCalcEdit.DecimalPlaces = 2;
			this.TotalInvoiceAmountIncGSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 45, true);
			this.TotalInvoiceAmountIncGSTCalcEdit.Name = "TotalInvoiceAmountIncGSTCalcEdit";
			this.TotalInvoiceAmountIncGSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 44, true);
			this.TotalInvoiceAmountIncGSTCalcEdit.TabIndex = 8;
			this.TotalInvoiceAmountIncGSTCalcEdit.Text = "0.00";
			this.TotalInvoiceAmountIncGSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalInvoiceAmountIncGSTCalcEdit.TrackDisposedAccess = true;
			// 
			// GSTTaxCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.GSTTaxCurrencyTextBox, "CAH_RX_NKTransactionCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).CAH_RX_NKTransactionCurrency)));
			this.GSTTaxCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("690b885d-0bd0-4d34-88a2-2fca17fdb661", "Cost Currency");
			this.GSTTaxCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 19, true);
			this.GSTTaxCurrencyTextBox.Name = "GSTTaxCurrencyTextBox";
			this.GSTTaxCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 44, true);
			this.GSTTaxCurrencyTextBox.TabIndex = 7;
			// 
			// GSTCostTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GSTCostTaxAmountCalcEdit, "CAH_OSOutstandingAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).CAH_OSOutstandingAmount)));
			this.GSTCostTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("18735d46-f888-413a-b94e-7e7a500dc2f2", "Outstanding Amount");
			this.GSTCostTaxAmountCalcEdit.DecimalPlaces = 2;
			this.GSTCostTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 19, true);
			this.GSTCostTaxAmountCalcEdit.Name = "GSTCostTaxAmountCalcEdit";
			this.GSTCostTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 44, true);
			this.GSTCostTaxAmountCalcEdit.TabIndex = 6;
			this.GSTCostTaxAmountCalcEdit.Text = "0.00";
			this.GSTCostTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GSTCostTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 94, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 5;
			// 
			// ChargesGrid
			// 
			this.ChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargesGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).RelatedChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).OrganizationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_OSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_OSPaidAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_OSOutstandingAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).StatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCashAdvanceRequestLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader)(null)).Lines)).SyncRoot)).CAL_LocalPaidAmount)));
			this.ChargesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a608e2fa-aa60-4715-b121-8e4c836eadef", "Charge Code");
			zTextBoxColumnStyleInfo1.ColumnName = "RelatedChargeCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("006ae60a-5b0b-48bd-91ab-1e7929d40581", "Organization");
			zTextBoxColumnStyleInfo2.ColumnName = "OrganizationCode";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3783df86-01cd-4de8-9dff-284a30bd7651", "Currency");
			zTextBoxColumnStyleInfo3.ColumnName = "Currency";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7f0b7b74-602e-44a2-ac4d-169e1df68f55", "OS Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CAL_OSAmount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f6e81b11-369e-4d19-a110-f6dc3e6af616", "OS Paid Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "CAL_OSPaidAmount";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c75cf2f0-30dd-4b7a-b1b4-609544bd10cd", "OS Outstanding Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CAL_OSOutstandingAmount";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9a9fbbd0-0028-40e8-b3ad-28dc6aea3b86", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c53d4706-d3a2-4ca4-a2a2-ea9bfd96b7a4", "Local Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "CAL_LocalAmount";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ca762f69-4c3a-4db8-ae1c-9b4521082f27", "Local Paid Amount");
			zCalcEditColumnStyleInfo5.ColumnName = "CAL_LocalPaidAmount";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargesGrid.GridId = "81EE65E1-8580-4645-B7EF-A7E613C32369";
			this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargesGrid.LayoutKey = "ChargesGrid";
			this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 101, true);
			this.ChargesGrid.Name = "ChargesGrid";
			this.ChargesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 211, true);
			this.ChargesGrid.TabIndex = 1;
			// 
			// APCashAdvanceViewForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceViewForm|62fafddd-9599-4fb0-b132-d815208d6fb4", "AP Advance Payment");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 461, true);
			this.Controls.Add(this.ChargesGrid);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.InvoiceSummaryGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.CashAdvance.CashAdvanceRequestHeader";
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			this.Name = "APCashAdvanceViewForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InvoiceSummaryGroupBox, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.ChargesGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceSummaryGroupBox.ResumeLayout(false);
			this.InvoiceSummaryGroupBox.PerformLayout();
			this.dateCreatedTextBox.ResumeLayout(true);
			this.dateCreatedTextBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TotalsGroupBox.ResumeLayout(false);
			this.TotalsGroupBox.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ChargesGrid.ResumeLayout(false);
			this.ChargesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

	}
}
