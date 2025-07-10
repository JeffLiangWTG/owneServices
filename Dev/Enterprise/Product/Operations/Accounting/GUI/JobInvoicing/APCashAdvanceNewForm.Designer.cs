using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class APCashAdvanceNewForm
	{


		#region Windows Form Designer generated code

		private ZPanel BottomPanel;
		private ZGroupBox TotalsGroupBox;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZGrid ChargesGrid;
		private ZGroupBox InvoiceSummaryGroupBox;
		private ZGuidFindBox CreditorsGuidFindBox;
		private ZTextBox numberTextBox;
		private Container components = null;

		new void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			this.InvoiceSummaryGroupBox = new ZGroupBox();
			this.statusTextBox = new ZTextBox();
			this.dateCreatedTextBox = new ZTextBox();
			this.numberTextBox = new ZTextBox();
			this.CreditorsGuidFindBox = new ZGuidFindBox();
			this.BottomPanel = new ZPanel();
			this.TotalsGroupBox = new ZGroupBox();
			this.TotalAmtOnInvoiceForJobCurrencyTextBox = new ZTextBox();
			this.TotalInvoiceAmountIncGSTCalcEdit = new ZCalcEdit();
			this.GSTTaxCurrencyTextBox = new ZTextBox();
			this.GSTCostTaxAmountCalcEdit = new ZCalcEdit();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.ChargesGrid = new ZGrid();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceSummaryGroupBox.SuspendLayout();
			this.CreditorsGuidFindBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TotalsGroupBox.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			((ISupportInitialize)(this.ChargesGrid)).BeginInit();
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
			this.BindingSource.DataSourceType = typeof(ChargeWithCost);
			// 
			// InvoiceSummaryGroupBox
			// 
			this.InvoiceSummaryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceNewForm|168f372c-fe11-4a25-9176-57437426d90a", "Advance Payment Request Summary");
			this.InvoiceSummaryGroupBox.Controls.Add(this.statusTextBox);
			this.InvoiceSummaryGroupBox.Controls.Add(this.dateCreatedTextBox);
			this.InvoiceSummaryGroupBox.Controls.Add(this.numberTextBox);
			this.InvoiceSummaryGroupBox.Controls.Add(this.CreditorsGuidFindBox);
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
			this.BindingSource.SetBindingMember(this.statusTextBox, "APCashAdvanceStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeWithCost)(null)).APCashAdvanceStatus)));
			this.statusTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("931a3a82-a2ac-46ac-8f8e-c916adf3d9dd", "Status");
			this.statusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 45, true);
			this.statusTextBox.Name = "statusTextBox";
			this.statusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.statusTextBox.TabIndex = 7;
			this.statusTextBox.Tag = "";
			// 
			// dateCreatedTextBox
			// 
			this.dateCreatedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.dateCreatedTextBox, "APCashAdvanceCreatedDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeWithCost)(null)).APCashAdvanceCreatedDateTime)));
			this.dateCreatedTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0b76e85c-83a2-4409-99fc-e62fe79615e4", "Date Created");
			this.dateCreatedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 19, true);
			this.dateCreatedTextBox.Name = "dateCreatedTextBox";
			this.dateCreatedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.dateCreatedTextBox.TabIndex = 6;
			this.dateCreatedTextBox.Tag = "";
			// 
			// numberTextBox
			// 
			this.BindingSource.SetBindingMember(this.numberTextBox, "APCashAdvanceReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeWithCost)(null)).APCashAdvanceReferenceNumber)));
			this.numberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceNewForm|56696843-761b-4bea-8b71-861f3e4ecdbd", "Number");
			this.numberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 45, true);
			this.numberTextBox.Name = "numberTextBox";
			this.numberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.numberTextBox.TabIndex = 5;
			this.numberTextBox.Tag = "";
			// 
			// CreditorsGuidFindBox
			// 
			this.CreditorsGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorsGuidFindBox, "JR_OH_CostAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ChargeWithCost)(null)).JR_OH_CostAccount)));
			this.CreditorsGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceNewForm|92a1222d-f447-40f5-96a9-fab7a3889f9f", "Creditor");
			this.CreditorsGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 19, true);
			this.CreditorsGuidFindBox.Name = "CreditorsGuidFindBox";
			this.CreditorsGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CreditorsGuidFindBox.ParentType = null;
			this.CreditorsGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CreditorsGuidFindBox.TabIndex = 1;
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
			this.TotalsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceNewForm|b66aafe7-f745-44ea-8858-4419c0eea1af", "Totals");
			this.TotalsGroupBox.Controls.Add(this.TotalAmtOnInvoiceForJobCurrencyTextBox);
			this.TotalsGroupBox.Controls.Add(this.TotalInvoiceAmountIncGSTCalcEdit);
			this.TotalsGroupBox.Controls.Add(this.GSTTaxCurrencyTextBox);
			this.TotalsGroupBox.Controls.Add(this.GSTCostTaxAmountCalcEdit);
			this.TotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 5, true);
			this.TotalsGroupBox.Name = "TotalsGroupBox";
			this.TotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 83, true);
			this.TotalsGroupBox.TabIndex = 4;
			this.TotalsGroupBox.TabStop = false;
			// 
			// TotalAmtOnInvoiceForJobCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalAmtOnInvoiceForJobCurrencyTextBox, "JR_OSCostCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeWithCost)(null)).JR_OSCostCurrencyCode)));
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("34c1591c-21c5-4117-8c8a-74d0c13899a6", "Cost Currency");
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 45, true);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Name = "TotalAmtOnInvoiceForJobCurrencyTextBox";
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.TabIndex = 9;
			// 
			// TotalInvoiceAmountIncGSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalInvoiceAmountIncGSTCalcEdit, "APCashAdvanceTotalInvoiceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ChargeWithCost)(null)).APCashAdvanceTotalInvoiceAmount)));
			this.TotalInvoiceAmountIncGSTCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3c9c448d-8066-4b5b-90e9-113703da1c65", "Invoice Amount");
			this.TotalInvoiceAmountIncGSTCalcEdit.DecimalPlaces = 2;
			this.TotalInvoiceAmountIncGSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 45, true);
			this.TotalInvoiceAmountIncGSTCalcEdit.Name = "TotalInvoiceAmountIncGSTCalcEdit";
			this.TotalInvoiceAmountIncGSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.TotalInvoiceAmountIncGSTCalcEdit.TabIndex = 8;
			this.TotalInvoiceAmountIncGSTCalcEdit.Text = "0.00";
			this.TotalInvoiceAmountIncGSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GSTTaxCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.GSTTaxCurrencyTextBox, "JR_OSCostCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeWithCost)(null)).JR_OSCostCurrencyCode)));
			this.GSTTaxCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("690b885d-0bd0-4d34-88a2-2fca17fdb661", "Cost Currency");
			this.GSTTaxCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 19, true);
			this.GSTTaxCurrencyTextBox.Name = "GSTTaxCurrencyTextBox";
			this.GSTTaxCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.GSTTaxCurrencyTextBox.TabIndex = 7;
			// 
			// GSTCostTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GSTCostTaxAmountCalcEdit, "APCashAdvanceTotalTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ChargeWithCost)(null)).APCashAdvanceTotalTax)));
			this.GSTCostTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("18735d46-f888-413a-b94e-7e7a500dc2f2", "Total Tax");
			this.GSTCostTaxAmountCalcEdit.DecimalPlaces = 2;
			this.GSTCostTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 19, true);
			this.GSTCostTaxAmountCalcEdit.Name = "GSTCostTaxAmountCalcEdit";
			this.GSTCostTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.GSTCostTaxAmountCalcEdit.TabIndex = 6;
			this.GSTCostTaxAmountCalcEdit.Text = "0.00";
			this.GSTCostTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
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
			this.BindingSource.SetBindingMember(this.ChargesGrid, "RelevantChargesForAPCashAdvance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_OH_CostAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_RX_NKCostCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_OSCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_AT_CostGSTRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_OSCostGSTAmt_Calc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_OSCostAmtWithGSTAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_LocalCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_Cost_LocalGSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_Calc_LocalCostAmtWithGST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_APInvoiceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Charge)(((System.Collections.IList)(((ChargeWithCost)(null)).RelevantChargesForAPCashAdvance)).SyncRoot)).JR_GE)));
			this.ChargesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JR_AC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "JR_Desc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JR_OH_CostAccount";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8415ac53-57f4-44fb-8a8b-459e9be66073", "Currency");
			zTextBoxColumnStyleInfo2.ColumnName = "JR_RX_NKCostCurrency";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3b804a06-63c4-414f-a8ce-59b83989b676", "OS Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "JR_OSCostAmt";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a9e9eef1-9845-412c-a134-7f686d5477f9", "Tax ID");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JR_AT_CostGSTRate";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("21d53e82-2c92-41eb-9abc-6bc86c1f6eb0", "OS Tax Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "JR_OSCostGSTAmt_Calc";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5A04F12A-AA81-435D-AA27-1474E2264D98", "OS Total Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "JR_OSCostAmtWithGSTAmt";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9458469c-8ba4-4552-8dbf-30b06d199c19", "Local Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "JR_LocalCostAmt";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1C5BA1EF-3A45-45FB-B0F7-A7AB55AA9E74", "Local Tax Amount");
			zCalcEditColumnStyleInfo5.ColumnName = "JR_Cost_LocalGSTAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("06AEA94E-AD7A-4A93-83C9-24578583DB3E", "Local Total Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "JR_Calc_LocalCostAmtWithGST";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3473326d-71bc-4ca4-bb71-6ec252b5447e", "AP Invoice Number");
			zTextBoxColumnStyleInfo3.ColumnName = "JR_APInvoiceNum";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "JR_GB";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "JR_GE";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargesGrid.GridId = "9999724A-4F36-4FED-936F-6609FD717E6B";
			this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargesGrid.LayoutKey = "ChargesGrid";
			this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 101, true);
			this.ChargesGrid.Name = "ChargesGrid";
			this.ChargesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 211, true);
			this.ChargesGrid.TabIndex = 1;
			// 
			// APCashAdvanceNewForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("APCashAdvanceNewForm|62fafddd-9599-4fb0-b132-d815208d6fb4", "AP Advance Payment");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 461, true);
			this.Controls.Add(this.ChargesGrid);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.InvoiceSummaryGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(ChargeWithCost);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.JobInvoicing.ChargeWithCost";
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 500, true);
			this.Name = "APCashAdvanceNewForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InvoiceSummaryGroupBox, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.ChargesGrid, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceSummaryGroupBox.ResumeLayout(false);
			this.InvoiceSummaryGroupBox.PerformLayout();
			this.CreditorsGuidFindBox.ResumeLayout(true);
			this.CreditorsGuidFindBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TotalsGroupBox.ResumeLayout(false);
			this.TotalsGroupBox.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			((ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ChargesGrid.ResumeLayout(false);
			this.ChargesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
