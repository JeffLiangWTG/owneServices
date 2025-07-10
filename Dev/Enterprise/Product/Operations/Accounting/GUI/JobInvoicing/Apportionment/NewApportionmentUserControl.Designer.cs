using System.Drawing;
using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	partial class NewApportionmentUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ConsolCostGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CostSummaryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CostHidingMessage = new Enterprise.ZArchitecture.ZLabel();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoiceAndPaymentDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CostTaxBranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CostSupplyTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RatingBehaviourDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocReceivedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.WithholdingTaxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WHTAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.WHTRateFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.InvoiceTotalsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalTotalAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OSTotalAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LocalTotalCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OSTotalCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CostReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsolCostOwnerFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OverrideTaxAmountCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ApportionToChildShipmentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExtraTaxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExtraTaxAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AutoAllocateZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TaxRateFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TaxDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TaxAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.LineTotalCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.IncludeOnCollectCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsPostedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LocalCostAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.OSCostAmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ChequeBookFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BankAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PlaceOfSupplyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ChequeNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.CreditorFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CostRateAuditTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.auditLogNoteUserControl = new Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.CalculationXMLUserControl();
			this.CostRateAuditTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AutoRatingNotePopupButton = new Enterprise.ZArchitecture.GUI.ZStmNotePopupButton();
			this.paymentBasisUserControl = new Enterprise.Accounting.GUI.JobInvoicing.PaymentBasisUserControl();
			this.WiseRatesRawDataUserControl = new Enterprise.Accounting.GUI.JobInvoicing.WiseRatesRawDataUserControl();
			this.ApportionedChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApportionedChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsolCostGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CostSummaryGrid)).BeginInit();
			this.CostSummaryGrid.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.DetailTabPage.SuspendLayout();
			this.InvoiceAndPaymentDetailsPanel.SuspendLayout();
			this.CostTaxBranchGuidFindBox.SuspendLayout();
			this.CostSupplyTypeDropEdit.SuspendLayout();
			this.RatingBehaviourDropEdit.SuspendLayout();
			this.DocReceivedDateEdit.SuspendLayout();
			this.WithholdingTaxPanel.SuspendLayout();
			this.WHTAmountCalcFindBox.SuspendLayout();
			this.WHTRateFindBox.SuspendLayout();
			this.InvoiceTotalsGroupBox.SuspendLayout();
			this.ConsolCostOwnerFindBox.SuspendLayout();
			this.ExtraTaxPanel.SuspendLayout();
			this.ExtraTaxAmountCalcFindBox.SuspendLayout();
			this.TaxRateFindBox.SuspendLayout();
			this.TaxDateEdit.SuspendLayout();
			this.TaxAmountCalcFindBox.SuspendLayout();
			this.LineTotalCalcFindBox.SuspendLayout();
			this.LocalCostAmountCalcFindBox.SuspendLayout();
			this.OSCostAmountCalcFindBox.SuspendLayout();
			this.ChequeBookFindBox.SuspendLayout();
			this.BankAccountFindBox.SuspendLayout();
			this.PlaceOfSupplyDropEdit.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.CreditorFindBox.SuspendLayout();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.CostRateAuditTabPage.SuspendLayout();
			this.auditLogNoteUserControl.SuspendLayout();
			this.CostRateAuditTextBox.SuspendLayout();
			this.paymentBasisUserControl.SuspendLayout();
			this.WiseRatesRawDataUserControl.SuspendLayout();
			this.ApportionedChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.ApportionedChargesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing);
			// 
			// ConsolCostGroupBox
			// 
			this.ConsolCostGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|03a0aaa7-903d-4df7-ae75-f27f5cbe794f", "Cost Summary");
			this.ConsolCostGroupBox.Controls.Add(this.CostSummaryGrid);
			this.ConsolCostGroupBox.Controls.Add(this.CostHidingMessage);
			this.ConsolCostGroupBox.Controls.Add(this.DetailsTabControl);
			this.ConsolCostGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolCostGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolCostGroupBox.Name = "ConsolCostGroupBox";
			this.ConsolCostGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1045, 440, true);
			this.ConsolCostGroupBox.TabIndex = 0;
			this.ConsolCostGroupBox.TabStop = false;
			// 
			// CostSummaryGrid
			// 
			this.CostSummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CostSummaryGrid, "CostsFilteredCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AC_ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).CostExchangeRate.Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_OSCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AT_TaxRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_TaxDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_A9_VATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_OSGSTAmount_Calc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_OSGSTRealAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_OSExtraTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).CostExchangeRate.Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_LocalCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_PPDCLT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_ApportionmentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).UnApportionedAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_OH_Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_InvoiceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_PaymentDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_DocumentReceivedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_PaymentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_PlaceOfSupply)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AB_BankAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AK_ChequeBook)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).IsPosted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_IsTaxAmountOverridden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).ARInvoice.AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_IsForCollectInvoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).IsApproved)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_ApportionToRelatedShipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_CostReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceOSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceOSTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceLocalTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceLocalTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_CostGovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_SellGovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AW)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_OSWHTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_GB_CostTaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).ChargeGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).ChargeCodeSubGroup)));
			this.CostSummaryGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "E6_AC_ChargeCode";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("42ba3693-b578-4a49-80f8-fb09ac78ca0e", "Charge Code Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|536beb5d-181a-48bd-81e8-2574301fd70c", "Cur.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CostExchangeRate+Currency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "E6_OSCostAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|1572f6a4-48d2-428f-a37c-5edc73bab416", "Tax Rate");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "E6_AT_TaxRate";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "E6_TaxDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|7ef2c38c-17ff-4e70-953d-2769405f5a06", "Tax Msg.");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "E6_A9_VATClass";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "E6_OSGSTAmount_Calc";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|3b2cd7e0-73b1-4496-a4ea-74469a389b7c", "GST Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "E6_OSGSTRealAmount";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "E6_OSExtraTaxAmount";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|1facae6f-2cb6-453c-b5bb-670a14c8f1dc", "Ex. Rate");
			zCalcEditColumnStyleInfo5.ColumnName = CostExchangeRateColumnName;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "E6_LocalCostAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|5d61b4f2-a4fe-48fe-baf9-9fe2dbb7d99f", "Apportionment Filter");
			zDropEditColumnStyleInfo1.ColumnName = "E6_PPDCLT";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "E6_ApportionmentMethod";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|540885d1-14ee-42e2-8220-573e0aedaadb", "Unapportioned");
			zCalcEditColumnStyleInfo7.ColumnName = "UnApportionedAmount";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "E6_OH_Creditor";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|962f5c36-2e9f-4dd8-84cc-3937e7782379", "AP Invoice #");
			zTextBoxColumnStyleInfo2.ColumnName = "E6_InvoiceNum";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "E6_InvoiceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "E6_PaymentDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.ColumnName = "E6_DocumentReceivedDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = "E6_PaymentType";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "E6_PlaceOfSupply";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "E6_ChequeOrReference";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "E6_AB_BankAccount";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "E6_AK_ChequeBook";
			zGuidFindBoxColumnStyleInfo5.IsVisible = false;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|783ab108-161c-4dd1-b960-dc9134b2e468", "Posted");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsPosted";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "E6_IsTaxAmountOverridden";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|8bf578d6-1cd3-4e62-8fee-549b38114d6b", "AR Invoice #");
			zTextBoxColumnStyleInfo4.ColumnName = "ARInvoice+AH_TransactionNum";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "E6_IsForCollectInvoice";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|84e0e677-1298-4b0b-950a-dc2d0b8d32b9", "Is Approved");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsApproved";
			zCheckBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|cae7add1-766a-4ade-ab1f-a9deec1b2517", "Display Rel. Shipments");
			zCheckBoxColumnStyleInfo5.ColumnName = "E6_ApportionToRelatedShipments";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("07752954-b47b-4014-8669-69becfd1c685", "Sup. Cost Ref.", "Supplier Cost Reference", "");
			zTextBoxColumnStyleInfo5.ColumnName = "E6_CostReference";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f7a3b633-45a8-4668-9db0-cbc1caeb89d1", "Inv. OS Total");
			zCalcEditColumnStyleInfo8.ColumnName = "InvoiceOSTotal";
			zCalcEditColumnStyleInfo8.IsReadOnly = true;
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f564137a-f53e-42cc-b6ca-4e341d29ab3e", "Inv. OS Tax");
			zCalcEditColumnStyleInfo9.ColumnName = "InvoiceOSTax";
			zCalcEditColumnStyleInfo9.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5a29fda0-963b-4f94-9420-68ff9761d53a", "Inv. Local Total");
			zCalcEditColumnStyleInfo10.ColumnName = "InvoiceLocalTotal";
			zCalcEditColumnStyleInfo10.IsReadOnly = true;
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("287dcc1b-5b1b-4b65-8656-8e3dc2f29b19", "Inv. Local Tax");
			zCalcEditColumnStyleInfo11.ColumnName = "InvoiceLocalTax";
			zCalcEditColumnStyleInfo11.IsReadOnly = true;
			zCalcEditColumnStyleInfo11.IsVisible = false;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("813894e8-2fd7-4c49-a77c-30494828434e", "Cost Government Charge Code");
			zTextBoxColumnStyleInfo6.ColumnName = "E6_CostGovtChargeCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bcb24955-5543-49f9-a914-0157a87bd87b", "Sell Government Charge Code");
			zTextBoxColumnStyleInfo7.ColumnName = "E6_SellGovtChargeCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "E6_AW";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "E6_OSWHTAmount";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "E6_SupplyType";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.ColumnName = "E6_GB_CostTaxBranch";
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5AF7FA24-1A35-4712-842C-698EDA1F440B", "Charge Group");
			zTextBoxColumnStyleInfo21.ColumnName = "ChargeGroup";
			zTextBoxColumnStyleInfo21.IsVisible = false;
			zTextBoxColumnStyleInfo21.IsReadOnly = true;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("F3F2D88F-AE41-4591-976B-9A707CF224C0", "Charge Code Sub Group");
			zTextBoxColumnStyleInfo22.ColumnName = "ChargeCodeSubGroup";
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.IsReadOnly = true;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CostSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CostSummaryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CostSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CostSummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CostSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.CostSummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CostSummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.CostSummaryGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CostSummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CostSummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.CostSummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.CostSummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CostSummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CostSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.CostSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.CostSummaryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CostSummaryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.CostSummaryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.CostSummaryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.CostSummaryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.CostSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.CostSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.CostSummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.CostSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.CostSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.CostSummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CostSummaryGrid.GridId = "e4cfd46d-3034-4f9f-b555-70f8a47f4a13";
			this.CostSummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CostSummaryGrid.LayoutKey = "CostSummaryGrid";
			this.CostSummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.CostSummaryGrid.Name = "CostSummaryGrid";
			this.CostSummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 168, true);
			this.CostSummaryGrid.TabIndex = 0;
			// 
			// CostHidingMessage
			// 
			this.CostHidingMessage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("365d67f0-39fe-4e70-a7c7-705b976bb30b", "Consol costs containing lines apportioned  to branch / dept outside your login permission are not listed.");
			this.CostHidingMessage.Dock = System.Windows.Forms.DockStyle.Top;
			this.CostHidingMessage.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CostHidingMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CostHidingMessage.Name = "CostHidingMessage";
			this.CostHidingMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 23, true);
			this.CostHidingMessage.TabIndex = 0;
			this.CostHidingMessage.Visible = false;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.DetailTabPage);
			this.DetailsTabControl.Controls.Add(this.CostRateAuditTabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 207, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 230, true);
			this.DetailsTabControl.TabIndex = 2;
			// 
			// DetailTabPage
			// 
			this.DetailTabPage.Controls.Add(this.InvoiceAndPaymentDetailsPanel);
			this.DetailTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailTabPage.Name = "DetailTabPage";
			this.DetailTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1031, 203, true);
			this.DetailTabPage.TabIndex = 0;
			// 
			// InvoiceAndPaymentDetailsPanel
			// 
			this.InvoiceAndPaymentDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.CostTaxBranchGuidFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.CostSupplyTypeDropEdit);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.RatingBehaviourDropEdit);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.DocReceivedDateEdit);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.WithholdingTaxPanel);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.InvoiceTotalsGroupBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.CostReferenceTextBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.ConsolCostOwnerFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.OverrideTaxAmountCheckBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.ApportionToChildShipmentsCheckBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.ExtraTaxPanel);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.AutoAllocateZLabel);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.TaxRateFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.TaxDateEdit);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.TaxAmountCalcFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.LineTotalCalcFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.IncludeOnCollectCheckBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.IsPostedCheckBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.LocalCostAmountCalcFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.OSCostAmountCalcFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.ChequeBookFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.BankAccountFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.PlaceOfSupplyDropEdit);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.zDateEdit2);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.zDateEdit1);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.ChequeNumberTextBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.zTextBox1);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.CreditorFindBox);
			this.InvoiceAndPaymentDetailsPanel.Controls.Add(this.PaymentTypeDropEdit);
			this.InvoiceAndPaymentDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.InvoiceAndPaymentDetailsPanel.Name = "InvoiceAndPaymentDetailsPanel";
			this.InvoiceAndPaymentDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1025, 196, true);
			this.InvoiceAndPaymentDetailsPanel.TabIndex = 1;
			// 
			// CostTaxBranchGuidFindBox
			// 
			this.CostTaxBranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CostTaxBranchGuidFindBox, "CostsFilteredCollection.E6_GB_CostTaxBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_GB_CostTaxBranch)));
			this.CostTaxBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(782, 73, true);
			this.CostTaxBranchGuidFindBox.Name = "CostTaxBranchGuidFindBox";
			this.CostTaxBranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CostTaxBranchGuidFindBox.ParentType = null;
			this.CostTaxBranchGuidFindBox.PopupCaption = null;
			this.CostTaxBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.CostTaxBranchGuidFindBox.TabIndex = 22;
			// 
			// CostSupplyTypeDropEdit
			// 
			this.CostSupplyTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CostSupplyTypeDropEdit, "CostsFilteredCollection.E6_SupplyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_SupplyType)));
			this.CostSupplyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 3, true);
			this.CostSupplyTypeDropEdit.Name = "CostSupplyTypeDropEdit";
			this.CostSupplyTypeDropEdit.ShouldResizeByMaxLength = true;
			this.CostSupplyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CostSupplyTypeDropEdit.TabIndex = 15;
			// 
			// RatingBehaviourDropEdit
			// 
			this.RatingBehaviourDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RatingBehaviourDropEdit, "CostsFilteredCollection.E6_RatingBehaviour");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_RatingBehaviour)));
			this.RatingBehaviourDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 121, true);
			this.RatingBehaviourDropEdit.Name = "RatingBehaviourDropEdit";
			this.RatingBehaviourDropEdit.PreBoundMaxLength = 3;
			this.RatingBehaviourDropEdit.ShouldResizeByMaxLength = true;
			this.RatingBehaviourDropEdit.ShowDescriptionBox = false;
			this.RatingBehaviourDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.RatingBehaviourDropEdit.TabIndex = 27;
			// 
			// DocReceivedDateEdit
			// 
			this.DocReceivedDateEdit.AllowDrop = true;
			this.DocReceivedDateEdit.AutoCompleteMonthThreshold = 1;
			this.DocReceivedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DocReceivedDateEdit, "CostsFilteredCollection.E6_DocumentReceivedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_DocumentReceivedDate)));
			this.DocReceivedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|574816bc-b379-48e5-a7ee-907e04278e1d", "Doc Rec Date", "Document Received Date");
			this.DocReceivedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 50, true);
			this.DocReceivedDateEdit.Name = "DocReceivedDateEdit";
			this.DocReceivedDateEdit.TabIndex = 9;
			// 
			// WithholdingTaxPanel
			// 
			this.WithholdingTaxPanel.Controls.Add(this.WHTAmountCalcFindBox);
			this.WithholdingTaxPanel.Controls.Add(this.WHTRateFindBox);
			this.WithholdingTaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 95, true);
			this.WithholdingTaxPanel.Name = "WithholdingTaxPanel";
			this.WithholdingTaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 24, true);
			this.WithholdingTaxPanel.TabIndex = 18;
			// 
			// WHTAmountCalcFindBox
			// 
			this.WHTAmountCalcFindBox.AllowDrop = true;
			this.WHTAmountCalcFindBox.BindToAmount = "CostsFilteredCollection.E6_OSWHTAmount";
			this.WHTAmountCalcFindBox.BindToUnit = "CostsFilteredCollection.E6_RX_NKCurrencyReadOnly";
			this.WHTAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.WHTAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 3, true);
			this.WHTAmountCalcFindBox.Name = "WHTAmountCalcFindBox";
			this.WHTAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.WHTAmountCalcFindBox.TabIndex = 18;
			// 
			// WHTRateFindBox
			// 
			this.WHTRateFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WHTRateFindBox, "CostsFilteredCollection.E6_AW");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AW)));
			this.WHTRateFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 3, true);
			this.WHTRateFindBox.Name = "WHTRateFindBox";
			this.WHTRateFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WHTRateFindBox.ParentType = null;
			this.WHTRateFindBox.PopupCaption = null;
			this.WHTRateFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.WHTRateFindBox.TabIndex = 23;
			// 
			// InvoiceTotalsGroupBox
			// 
			this.InvoiceTotalsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.InvoiceTotalsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7a062eb4-1b87-4ebb-8ddd-8302a4cefe74", "Invoice Totals");
			this.InvoiceTotalsGroupBox.Controls.Add(this.zCalcEdit1);
			this.InvoiceTotalsGroupBox.Controls.Add(this.zCalcEdit2);
			this.InvoiceTotalsGroupBox.Controls.Add(this.zTextBox2);
			this.InvoiceTotalsGroupBox.Controls.Add(this.zTextBox3);
			this.InvoiceTotalsGroupBox.Controls.Add(this.LocalTotalAmountCalcEdit);
			this.InvoiceTotalsGroupBox.Controls.Add(this.OSTotalAmountCalcEdit);
			this.InvoiceTotalsGroupBox.Controls.Add(this.LocalTotalCurrencyTextBox);
			this.InvoiceTotalsGroupBox.Controls.Add(this.OSTotalCurrencyTextBox);
			this.InvoiceTotalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 148, true);
			this.InvoiceTotalsGroupBox.Name = "InvoiceTotalsGroupBox";
			this.InvoiceTotalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 46, true);
			this.InvoiceTotalsGroupBox.TabIndex = 26;
			this.InvoiceTotalsGroupBox.TabStop = false;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "CostsFilteredCollection.InvoiceLocalTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceLocalTax)));
			this.zCalcEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|5d398e81-0a8f-4d83-9908-287be51303b3", "Local Tax");
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 21, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.ReadOnly = true;
			this.zCalcEdit1.ShouldEscapeAllSpecialCharacters = false;
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.zCalcEdit1.TabIndex = 11;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "CostsFilteredCollection.InvoiceLocalTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceLocalTotal)));
			this.zCalcEdit2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|850d82d8-04f6-4104-933b-fec80a232ed5", "Local Total");
			this.zCalcEdit2.DecimalPlaces = 2;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 21, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.ReadOnly = true;
			this.zCalcEdit2.ShouldEscapeAllSpecialCharacters = false;
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.zCalcEdit2.TabIndex = 9;
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "CostsFilteredCollection.LocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).LocalCurrency)));
			this.zTextBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2c245dad-ce98-4957-a1ab-d5d4103b35c1", "Local Tax Currency");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 21, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.ReadOnly = true;
			this.zTextBox2.ShouldEscapeAllSpecialCharacters = false;
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.zTextBox2.TabIndex = 12;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "CostsFilteredCollection.LocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).LocalCurrency)));
			this.zTextBox3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3a82e85d-37bf-4acc-8acb-6458ddd23aea", "Local Total Currency");
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 21, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.ReadOnly = true;
			this.zTextBox3.ShouldEscapeAllSpecialCharacters = false;
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.zTextBox3.TabIndex = 10;
			// 
			// LocalTotalAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LocalTotalAmountCalcEdit, "CostsFilteredCollection.InvoiceOSTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceOSTax)));
			this.LocalTotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|1966526b-edd5-487a-abd7-19c701783d1a", "OS Tax");
			this.LocalTotalAmountCalcEdit.DecimalPlaces = 2;
			this.LocalTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 21, true);
			this.LocalTotalAmountCalcEdit.Name = "LocalTotalAmountCalcEdit";
			this.LocalTotalAmountCalcEdit.ReadOnly = true;
			this.LocalTotalAmountCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.LocalTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.LocalTotalAmountCalcEdit.TabIndex = 7;
			this.LocalTotalAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OSTotalAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OSTotalAmountCalcEdit, "CostsFilteredCollection.InvoiceOSTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceOSTotal)));
			this.OSTotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|09dd906e-93f0-4b3c-8325-dda2a49c424b", "OS Total");
			this.OSTotalAmountCalcEdit.DecimalPlaces = 2;
			this.OSTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 21, true);
			this.OSTotalAmountCalcEdit.Name = "OSTotalAmountCalcEdit";
			this.OSTotalAmountCalcEdit.ReadOnly = true;
			this.OSTotalAmountCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.OSTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OSTotalAmountCalcEdit.TabIndex = 3;
			this.OSTotalAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalTotalCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalTotalCurrencyTextBox, "CostsFilteredCollection.InvoiceOSCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceOSCurrency)));
			this.LocalTotalCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|2ad6ee7c-c624-468c-b69b-62f77234861a", "OS Tax Currency");
			this.LocalTotalCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 21, true);
			this.LocalTotalCurrencyTextBox.Name = "LocalTotalCurrencyTextBox";
			this.LocalTotalCurrencyTextBox.ReadOnly = true;
			this.LocalTotalCurrencyTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.LocalTotalCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.LocalTotalCurrencyTextBox.TabIndex = 8;
			// 
			// OSTotalCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.OSTotalCurrencyTextBox, "CostsFilteredCollection.InvoiceOSCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).InvoiceOSCurrency)));
			this.OSTotalCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|7171ec09-5ec8-4f4e-ac67-fad639f12f31", "OS Total Currency");
			this.OSTotalCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 21, true);
			this.OSTotalCurrencyTextBox.Name = "OSTotalCurrencyTextBox";
			this.OSTotalCurrencyTextBox.ReadOnly = true;
			this.OSTotalCurrencyTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.OSTotalCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.OSTotalCurrencyTextBox.TabIndex = 4;
			// 
			// CostReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.CostReferenceTextBox, "CostsFilteredCollection.E6_CostReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_CostReference)));
			this.CostReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("866dd01e-b1a1-4515-a495-08a0d4ea6031", "Sup. Cost Ref.", "Supplier Cost Reference", "");
			this.CostReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 120, true);
			this.CostReferenceTextBox.Name = "CostReferenceTextBox";
			this.CostReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CostReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.CostReferenceTextBox.TabIndex = 7;
			// 
			// ConsolCostOwnerFindBox
			// 
			this.ConsolCostOwnerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolCostOwnerFindBox, "CostsFilteredCollection.E6_GS_NKConsolCostOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_GS_NKConsolCostOwner)));
			this.ConsolCostOwnerFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C6CAF154-14DC-4A41-9249-76822C824C83", "Cost Owner");
			this.ConsolCostOwnerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 144, true);
			this.ConsolCostOwnerFindBox.Name = "ConsolCostOwnerFindBox";
			this.ConsolCostOwnerFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConsolCostOwnerFindBox.ParentType = null;
			this.ConsolCostOwnerFindBox.PreBoundMaxLength = 3;
			this.ConsolCostOwnerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.ConsolCostOwnerFindBox.TabIndex = 7;
			// 
			// OverrideTaxAmountCheckBox
			// 
			this.OverrideTaxAmountCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideTaxAmountCheckBox, "CostsFilteredCollection.E6_IsTaxAmountOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_IsTaxAmountOverridden)));
			this.OverrideTaxAmountCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|24a549d0-bfe9-48b2-a503-29ddd7c952e7", "Override Tax Amount");
			this.OverrideTaxAmountCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideTaxAmountCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(782, 28, true);
			this.OverrideTaxAmountCheckBox.Name = "OverrideTaxAmountCheckBox";
			this.OverrideTaxAmountCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 17, true);
			this.OverrideTaxAmountCheckBox.TabIndex = 20;
			this.OverrideTaxAmountCheckBox.UseVisualStyleBackColor = true;
			// 
			// ApportionToChildShipmentsCheckBox
			// 
			this.ApportionToChildShipmentsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ApportionToChildShipmentsCheckBox, "CostsFilteredCollection.E6_ApportionToRelatedShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_ApportionToRelatedShipments)));
			this.ApportionToChildShipmentsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|0e1f6b8c-24a3-4bc6-80ad-fdfe809d2251", "Display Related Shipments");
			this.ApportionToChildShipmentsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ApportionToChildShipmentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ApportionToChildShipmentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(425, 3, true);
			this.ApportionToChildShipmentsCheckBox.Name = "ApportionToChildShipmentsCheckBox";
			this.ApportionToChildShipmentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ApportionToChildShipmentsCheckBox.TabIndex = 2;
			this.ApportionToChildShipmentsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExtraTaxPanel
			// 
			this.ExtraTaxPanel.Controls.Add(this.ExtraTaxAmountCalcFindBox);
			this.ExtraTaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(733, 120, true);
			this.ExtraTaxPanel.Name = "ExtraTaxPanel";
			this.ExtraTaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 28, true);
			this.ExtraTaxPanel.TabIndex = 19;
			// 
			// ExtraTaxAmountCalcFindBox
			// 
			this.ExtraTaxAmountCalcFindBox.AllowDrop = true;
			this.ExtraTaxAmountCalcFindBox.BindToAmount = "CostsFilteredCollection.E6_OSExtraTaxAmount";
			this.ExtraTaxAmountCalcFindBox.BindToUnit = "CostsFilteredCollection.E6_RX_NKCurrencyReadOnly";
			this.ExtraTaxAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|17be5754-7fd8-477b-a5e1-a2416a155abd", "QST Amt", "QST Amount", "");
			this.ExtraTaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ExtraTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 3, true);
			this.ExtraTaxAmountCalcFindBox.Name = "ExtraTaxAmountCalcFindBox";
			this.ExtraTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.ExtraTaxAmountCalcFindBox.TabIndex = 19;
			// 
			// AutoAllocateZLabel
			// 
			this.AutoAllocateZLabel.AutoSize = true;
			this.AutoAllocateZLabel.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.AutoAllocateZLabel, "CostsFilteredCollection.Calc_ChequeNumberIsAutoAllocatedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).Calc_ChequeNumberIsAutoAllocatedLabel)));
			this.AutoAllocateZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|6dc13b33-912b-490b-87da-89033196bde8", "Auto Allocate");
			this.AutoAllocateZLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoAllocateZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoAllocateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 100, true);
			this.AutoAllocateZLabel.Name = "AutoAllocateZLabel";
			this.AutoAllocateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.AutoAllocateZLabel.TabIndex = 10;
			// 
			// TaxRateFindBox
			// 
			this.TaxRateFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxRateFindBox, "CostsFilteredCollection.E6_AT_TaxRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AT_TaxRate)));
			this.TaxRateFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(782, 50, true);
			this.TaxRateFindBox.Name = "TaxRateFindBox";
			this.TaxRateFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TaxRateFindBox.ParentType = null;
			this.TaxRateFindBox.PopupCaption = null;
			this.TaxRateFindBox.ShowDescriptionBox = false;
			this.TaxRateFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.TaxRateFindBox.TabIndex = 21;
			// 
			// TaxDateEdit
			// 
			this.TaxDateEdit.AllowDrop = true;
			this.TaxDateEdit.AutoCompleteMonthThreshold = 1;
			this.TaxDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TaxDateEdit, "CostsFilteredCollection.E6_TaxDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_TaxDate)));
			this.TaxDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(930, 50, true);
			this.TaxDateEdit.Name = "TaxDateEdit";
			this.TaxDateEdit.TabIndex = 23;
			// 
			// TaxAmountCalcFindBox
			// 
			this.TaxAmountCalcFindBox.AllowDrop = true;
			this.TaxAmountCalcFindBox.BindToAmount = "CostsFilteredCollection.E6_OSGSTAmount_Calc";
			this.TaxAmountCalcFindBox.BindToUnit = "CostsFilteredCollection.E6_RX_NKCurrencyReadOnly";
			this.TaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 50, true);
			this.TaxAmountCalcFindBox.Name = "TaxAmountCalcFindBox";
			this.TaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.TaxAmountCalcFindBox.TabIndex = 17;
			// 
			// LineTotalCalcFindBox
			// 
			this.LineTotalCalcFindBox.AllowDrop = true;
			this.LineTotalCalcFindBox.BindToAmount = "CostsFilteredCollection.E6_Calc_OSTotalAmount";
			this.LineTotalCalcFindBox.BindToUnit = "CostsFilteredCollection.E6_RX_NKCurrencyReadOnly";
			this.LineTotalCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|3a126e39-4376-4cf6-a782-869dc6e5e2dc", "Total");
			this.LineTotalCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LineTotalCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 27, true);
			this.LineTotalCalcFindBox.Name = "LineTotalCalcFindBox";
			this.LineTotalCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.LineTotalCalcFindBox.TabIndex = 16;
			// 
			// IncludeOnCollectCheckBox
			// 
			this.IncludeOnCollectCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeOnCollectCheckBox, "CostsFilteredCollection.E6_IsForCollectInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_IsForCollectInvoice)));
			this.IncludeOnCollectCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IncludeOnCollectCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeOnCollectCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 3, true);
			this.IncludeOnCollectCheckBox.Name = "IncludeOnCollectCheckBox";
			this.IncludeOnCollectCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IncludeOnCollectCheckBox.TabIndex = 1;
			this.IncludeOnCollectCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsPostedCheckBox
			// 
			this.IsPostedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsPostedCheckBox, "CostsFilteredCollection.IsPosted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).IsPosted)));
			this.IsPostedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|cb06b407-2c1d-4626-8932-f1437b363d23", "Posted");
			this.IsPostedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsPostedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPostedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 3, true);
			this.IsPostedCheckBox.Name = "IsPostedCheckBox";
			this.IsPostedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsPostedCheckBox.TabIndex = 0;
			this.IsPostedCheckBox.UseVisualStyleBackColor = true;
			// 
			// LocalCostAmountCalcFindBox
			// 
			this.LocalCostAmountCalcFindBox.AllowDrop = true;
			this.LocalCostAmountCalcFindBox.BindToAmount = "CostsFilteredCollection.E6_LocalCostAmount";
			this.LocalCostAmountCalcFindBox.BindToUnit = "CostsFilteredCollection.LocalCurrency";
			this.LocalCostAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalCostAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 50, true);
			this.LocalCostAmountCalcFindBox.Name = "LocalCostAmountCalcFindBox";
			this.LocalCostAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.LocalCostAmountCalcFindBox.TabIndex = 4;
			// 
			// OSCostAmountCalcFindBox
			// 
			this.OSCostAmountCalcFindBox.AllowDrop = true;
			this.OSCostAmountCalcFindBox.BindToAmount = "CostsFilteredCollection.E6_OSCostAmount";
			this.OSCostAmountCalcFindBox.BindToUnit = "CostsFilteredCollection.CostExchangeRate+Currency";
			this.OSCostAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSCostAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 27, true);
			this.OSCostAmountCalcFindBox.Name = "OSCostAmountCalcFindBox";
			this.OSCostAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.OSCostAmountCalcFindBox.TabIndex = 3;
			// 
			// ChequeBookFindBox
			// 
			this.ChequeBookFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChequeBookFindBox, "CostsFilteredCollection.E6_AK_ChequeBook");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AK_ChequeBook)));
			this.ChequeBookFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 73, true);
			this.ChequeBookFindBox.Name = "ChequeBookFindBox";
			this.ChequeBookFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ChequeBookFindBox.ParentType = null;
			this.ChequeBookFindBox.PopupCaption = null;
			this.ChequeBookFindBox.ShowDescriptionBox = false;
			this.ChequeBookFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ChequeBookFindBox.TabIndex = 13;
			// 
			// BankAccountFindBox
			// 
			this.BankAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountFindBox, "CostsFilteredCollection.E6_AB_BankAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_AB_BankAccount)));
			this.BankAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 50, true);
			this.BankAccountFindBox.Name = "BankAccountFindBox";
			this.BankAccountFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BankAccountFindBox.ParentType = null;
			this.BankAccountFindBox.PopupCaption = null;
			this.BankAccountFindBox.ShowDescriptionBox = false;
			this.BankAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BankAccountFindBox.TabIndex = 12;
			// 
			// PlaceOfSupplyDropEdit
			// 
			this.PlaceOfSupplyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PlaceOfSupplyDropEdit, "CostsFilteredCollection.E6_PlaceOfSupply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_PlaceOfSupply)));
			this.PlaceOfSupplyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(962, 26, true);
			this.PlaceOfSupplyDropEdit.Name = "PlaceOfSupplyDropEdit";
			this.PlaceOfSupplyDropEdit.PreBoundMaxLength = 3;
			this.PlaceOfSupplyDropEdit.ShouldResizeByMaxLength = true;
			this.PlaceOfSupplyDropEdit.ShowDescriptionBox = false;
			this.PlaceOfSupplyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.PlaceOfSupplyDropEdit.TabIndex = 24;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "CostsFilteredCollection.E6_PaymentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_PaymentDate)));
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 73, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 10;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "CostsFilteredCollection.E6_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_InvoiceDate)));
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 27, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 8;
			// 
			// ChequeNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeNumberTextBox, "CostsFilteredCollection.E6_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_ChequeOrReference)));
			this.ChequeNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|618b20a5-002e-4330-8b83-414cf0e36814", "Reference #");
			this.ChequeNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 96, true);
			this.ChequeNumberTextBox.Name = "ChequeNumberTextBox";
			this.ChequeNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ChequeNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ChequeNumberTextBox.TabIndex = 14;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "CostsFilteredCollection.E6_InvoiceNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_InvoiceNum)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 73, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ShouldEscapeAllSpecialCharacters = false;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.zTextBox1.TabIndex = 5;
			// 
			// CreditorFindBox
			// 
			this.CreditorFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorFindBox, "CostsFilteredCollection.E6_OH_Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_OH_Creditor)));
			this.CreditorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 96, true);
			this.CreditorFindBox.Name = "CreditorFindBox";
			this.CreditorFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CreditorFindBox.ParentType = null;
			this.CreditorFindBox.PopupCaption = null;
			this.CreditorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.CreditorFindBox.TabIndex = 6;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "CostsFilteredCollection.E6_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).E6_PaymentType)));
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 27, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 3;
			this.PaymentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentTypeDropEdit.ShowDescriptionBox = false;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 11;
			// 
			// CostRateAuditTabPage
			// 
			this.CostRateAuditTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.CostRateAuditTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|8f06aea4-8725-41de-98ce-d3a5fc189882", "Cost Rate Audit");
			this.CostRateAuditTabPage.Controls.Add(this.auditLogNoteUserControl);
			this.CostRateAuditTabPage.Controls.Add(this.CostRateAuditTextBox);
			this.CostRateAuditTabPage.Controls.Add(this.AutoRatingNotePopupButton);
			this.CostRateAuditTabPage.Controls.Add(this.paymentBasisUserControl);
			this.CostRateAuditTabPage.Controls.Add(this.WiseRatesRawDataUserControl);
			this.CostRateAuditTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CostRateAuditTabPage.Name = "CostRateAuditTabPage";
			this.CostRateAuditTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CostRateAuditTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1031, 203, true);
			this.CostRateAuditTabPage.TabIndex = 1;
			// 
			// auditLogNoteUserControl
			// 
			this.auditLogNoteUserControl.AllowDrop = true;
			this.auditLogNoteUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.auditLogNoteUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(911, 30, true);
			this.auditLogNoteUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.BindingSource.SetBindingMember(this.auditLogNoteUserControl, "CostsFilteredCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)))));
			this.auditLogNoteUserControl.Name = "auditLogNoteUserControl";
			this.auditLogNoteUserControl.ShowRevenue = false;
			this.auditLogNoteUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.auditLogNoteUserControl.TabIndex = 21;
			// 
			// CostRateAuditTextBox
			// 
			this.CostRateAuditTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CostRateAuditTextBox, "CostsFilteredCollection.CostCalculationDescriptionString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).CostCalculationDescription)));
			this.CostRateAuditTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|01b17505-63b7-450d-a508-fcf266dcc8b4", "Cost Rate Audit");
			this.CostRateAuditTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CostRateAuditTextBox.MaxLength = 10000000;
			this.CostRateAuditTextBox.Name = "CostRateAuditTextBox";
			this.CostRateAuditTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(901, 150, true);
			this.CostRateAuditTextBox.TabIndex = 0;
			this.CostRateAuditTextBox.Font = new Font("Lucida Console", 10);
			this.CostRateAuditTextBox.CharacterCasing = CharacterCasing.Normal;
			this.CostRateAuditTextBox.Multiline = true;
			this.CostRateAuditTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			// 
			// AutoRatingNotePopupButton
			// 
			this.AutoRatingNotePopupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AutoRatingNotePopupButton.AutoSize = true;
			this.AutoRatingNotePopupButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|2e6b9190-998b-4c9e-8748-f8f2e7df3f79", "Autorating Log");
			this.AutoRatingNotePopupButton.CreateNewNoteIfNotFound = false;
			this.AutoRatingNotePopupButton.IsCaptionOverridden = false;
			this.AutoRatingNotePopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(911, 3, true);
			this.AutoRatingNotePopupButton.Name = "AutoRatingNotePopupButton";
			this.AutoRatingNotePopupButton.NoteType = "AutoRating Log";
			this.AutoRatingNotePopupButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AutoRatingNotePopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.AutoRatingNotePopupButton.TabIndex = 1;
			this.AutoRatingNotePopupButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AutoRatingNotePopupButton.ToolTipCaption = null;
			this.AutoRatingNotePopupButton.UseVisualStyleBackColor = true;
			// 
			// paymentBasisUserControl
			// 
			this.paymentBasisUserControl.AllowDrop = true;
			this.paymentBasisUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.paymentBasisUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(911, 57, true);
			this.paymentBasisUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.paymentBasisUserControl.Name = "paymentBasisUserControl";
			this.paymentBasisUserControl.ShowRevenue = false;
			this.paymentBasisUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.paymentBasisUserControl.TabIndex = 23;
			// 
			// WiseRatesRawDataUserControl
			// 
			this.WiseRatesRawDataUserControl.AllowDrop = true;
			this.WiseRatesRawDataUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.WiseRatesRawDataUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(911, 84, true);
			this.WiseRatesRawDataUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.WiseRatesRawDataUserControl.Name = "WiseRatesRawDataUserControl";
			this.WiseRatesRawDataUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.WiseRatesRawDataUserControl.TabIndex = 24;
			// 
			// ApportionedChargesGroupBox
			// 
			this.ApportionedChargesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|8b4c6c4f-50a1-4515-aa8b-dabb272a8516", "Apportioned Charges");
			this.ApportionedChargesGroupBox.Controls.Add(this.ApportionedChargesGrid);
			this.ApportionedChargesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApportionedChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApportionedChargesGroupBox.Name = "ApportionedChargesGroupBox";
			this.ApportionedChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1045, 212, true);
			this.ApportionedChargesGroupBox.TabIndex = 0;
			this.ApportionedChargesGroupBox.TabStop = false;
			// 
			// ApportionedChargesGrid
			// 
			this.ApportionedChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ApportionedChargesGrid, "CostsFilteredCollection.FilteredApportionmentCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_RL_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_RL_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_OSCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_IsUsedForApportionment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_LocalCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_Chargeable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_ChargeableUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_OSCostGSTAmt_Calc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_ShipmentNumberOfColoadMaster)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_ActualWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_PrepaidCollect)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_Calc_RelatedJobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_JobLocalRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_JH_InternalJob)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_GB_InternalBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_GE_InternalDept)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_CostGovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_SellGovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).ChargeableRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_AW_CostWHTRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_OSCostWHTAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_CostSupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_SellSupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_GB_CostTaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.ApportionSplitCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.JobConsolCost)(((System.Collections.IList)(((Enterprise.Accounting.Business.ConsolCosting.ApportionmentListing)(null)).CostsFilteredCollection)).SyncRoot)).FilteredApportionmentCharges)).SyncRoot)).JR_GB_SellTaxBranch)));
			this.ApportionedChargesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|73212e6e-8c47-416e-93da-fddfe9e62430", "House Bill");
			zTextBoxColumnStyleInfo8.ColumnName = "JR_HouseBill";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "JR_RL_NKOrigin";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "JR_RL_NKDestination";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo8.ColumnName = "JR_GB";
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo9.ColumnName = "JR_GE";
			zGuidFindBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "JR_OSCostAmt";
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|2b6ffd00-7d95-42a2-be98-012c11807750", "Is Used");
			zCheckBoxColumnStyleInfo6.ColumnName = "JR_IsUsedForApportionment";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "JR_LocalCostAmt";
			zCalcEditColumnStyleInfo14.IsReadOnly = true;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|3d968bcb-ea8d-446b-9f5a-8959d43d9150", "Chargeable");
			zCalcEditColumnStyleInfo15.ColumnName = "JR_Chargeable";
			zCalcEditColumnStyleInfo15.Decimals = 3;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|ee8264db-068f-4836-b01c-9da1d865fbbb", "Chargeable Unit");
			zTextBoxColumnStyleInfo11.ColumnName = "JR_ChargeableUnit";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.ColumnName = "JR_OSCostGSTAmt_Calc";
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|013fad69-3de0-40b8-a61a-5fa5563e7a0a", "Coload Master");
			zTextBoxColumnStyleInfo12.ColumnName = "JR_ShipmentNumberOfColoadMaster";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|f6c1de46-ae98-447e-8aa7-e2bc98a86cbe", "Actual Weight");
			zCalcEditColumnStyleInfo17.ColumnName = "JR_ActualWeight";
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|a696a907-1f1b-481f-9b44-b916b4f54a17", "Actual Weight Units");
			zTextBoxColumnStyleInfo13.ColumnName = "JR_ActualWeightUnit";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|5c3a4616-f89a-4512-beab-17945edc7bdf", "PPD/CCX");
			zTextBoxColumnStyleInfo14.ColumnName = "JR_PrepaidCollect";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("NewApportionmentUserControl|549d0ee4-2553-4001-9afd-94dce29de711", "Job Number");
			zDropEditColumnStyleInfo6.ColumnName = "JR_JobNumber";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.ColumnName = "JR_Calc_RelatedJobNumber";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.ColumnName = "JR_JobLocalRef";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("16a7c6a4-d80e-4713-afad-ba2f9b3ec40c", "Charge Type");
			zTextBoxColumnStyleInfo17.ColumnName = "ChargeType";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo10.ColumnName = "JR_JH_InternalJob";
			zGuidFindBoxColumnStyleInfo10.IsVisible = false;
			zGuidFindBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo11.ColumnName = "JR_GB_InternalBranch";
			zGuidFindBoxColumnStyleInfo11.IsVisible = false;
			zGuidFindBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo12.ColumnName = "JR_GE_InternalDept";
			zGuidFindBoxColumnStyleInfo12.IsVisible = false;
			zGuidFindBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7e46cf1d-4390-466c-a3af-e2ded293cb5b", "Cost Government Charge Code");
			zTextBoxColumnStyleInfo18.ColumnName = "JR_CostGovtChargeCode";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("73b1dd5f-2681-4309-b98b-0f5b9c42b354", "Sell Government Charge Code");
			zTextBoxColumnStyleInfo19.ColumnName = "JR_SellGovtChargeCode";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.ColumnName = "ChargeableRate";
			zTextBoxColumnStyleInfo20.IsVisible = false;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo13.ColumnName = "JR_AW_CostWHTRate";
			zGuidFindBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.ColumnName = "JR_OSCostWHTAmt";
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "JR_CostSupplyType";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo8.ColumnName = "JR_SellSupplyType";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo14.ColumnName = "JR_GB_CostTaxBranch";
			zGuidFindBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo15.ColumnName = "JR_GB_SellTaxBranch";
			zGuidFindBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ApportionedChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.ApportionedChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo9);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ApportionedChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.ApportionedChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo10);
			this.ApportionedChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo11);
			this.ApportionedChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo12);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.ApportionedChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo13);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.ApportionedChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.ApportionedChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.ApportionedChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo14);
			this.ApportionedChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo15);
			this.ApportionedChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApportionedChargesGrid.GridId = "8bc30d50-0bdc-4e2b-a2af-5c52c4cedd4a";
			this.ApportionedChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApportionedChargesGrid.LayoutKey = "zGrid1";
			this.ApportionedChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ApportionedChargesGrid.Name = "ApportionedChargesGrid";
			this.ApportionedChargesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ApportionedChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1039, 193, true);
			this.ApportionedChargesGrid.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ConsolCostGroupBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ApportionedChargesGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1045, 656, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(440);
			this.splitContainer1.TabIndex = 2;
			// 
			// NewApportionmentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1033, 460, true);
			this.Name = "NewApportionmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1045, 656, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsolCostGroupBox.ResumeLayout(false);
			this.ConsolCostGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CostSummaryGrid)).EndInit();
			this.CostSummaryGrid.ResumeLayout(false);
			this.CostSummaryGrid.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.DetailTabPage.ResumeLayout(false);
			this.DetailTabPage.PerformLayout();
			this.InvoiceAndPaymentDetailsPanel.ResumeLayout(false);
			this.InvoiceAndPaymentDetailsPanel.PerformLayout();
			this.CostTaxBranchGuidFindBox.ResumeLayout(true);
			this.CostTaxBranchGuidFindBox.PerformLayout();
			this.CostSupplyTypeDropEdit.ResumeLayout(true);
			this.CostSupplyTypeDropEdit.PerformLayout();
			this.RatingBehaviourDropEdit.ResumeLayout(true);
			this.RatingBehaviourDropEdit.PerformLayout();
			this.DocReceivedDateEdit.ResumeLayout(true);
			this.DocReceivedDateEdit.PerformLayout();
			this.WithholdingTaxPanel.ResumeLayout(false);
			this.WithholdingTaxPanel.PerformLayout();
			this.WHTAmountCalcFindBox.ResumeLayout(true);
			this.WHTAmountCalcFindBox.PerformLayout();
			this.WHTRateFindBox.ResumeLayout(true);
			this.WHTRateFindBox.PerformLayout();
			this.InvoiceTotalsGroupBox.ResumeLayout(false);
			this.InvoiceTotalsGroupBox.PerformLayout();
			this.ConsolCostOwnerFindBox.ResumeLayout(true);
			this.ConsolCostOwnerFindBox.PerformLayout();
			this.ExtraTaxPanel.ResumeLayout(false);
			this.ExtraTaxPanel.PerformLayout();
			this.ExtraTaxAmountCalcFindBox.ResumeLayout(true);
			this.ExtraTaxAmountCalcFindBox.PerformLayout();
			this.TaxRateFindBox.ResumeLayout(true);
			this.TaxRateFindBox.PerformLayout();
			this.TaxDateEdit.ResumeLayout(true);
			this.TaxDateEdit.PerformLayout();
			this.TaxAmountCalcFindBox.ResumeLayout(true);
			this.TaxAmountCalcFindBox.PerformLayout();
			this.LineTotalCalcFindBox.ResumeLayout(true);
			this.LineTotalCalcFindBox.PerformLayout();
			this.LocalCostAmountCalcFindBox.ResumeLayout(true);
			this.LocalCostAmountCalcFindBox.PerformLayout();
			this.OSCostAmountCalcFindBox.ResumeLayout(true);
			this.OSCostAmountCalcFindBox.PerformLayout();
			this.ChequeBookFindBox.ResumeLayout(true);
			this.ChequeBookFindBox.PerformLayout();
			this.BankAccountFindBox.ResumeLayout(true);
			this.BankAccountFindBox.PerformLayout();
			this.PlaceOfSupplyDropEdit.ResumeLayout(true);
			this.PlaceOfSupplyDropEdit.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.CreditorFindBox.ResumeLayout(true);
			this.CreditorFindBox.PerformLayout();
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.CostRateAuditTabPage.ResumeLayout(false);
			this.CostRateAuditTabPage.PerformLayout();
			this.auditLogNoteUserControl.ResumeLayout(true);
			this.auditLogNoteUserControl.PerformLayout();
			this.CostRateAuditTextBox.ResumeLayout(true);
			this.CostRateAuditTextBox.PerformLayout();
			this.paymentBasisUserControl.ResumeLayout(true);
			this.paymentBasisUserControl.PerformLayout();
			this.WiseRatesRawDataUserControl.ResumeLayout(true);
			this.WiseRatesRawDataUserControl.PerformLayout();
			this.ApportionedChargesGroupBox.ResumeLayout(false);
			this.ApportionedChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.ApportionedChargesGrid.ResumeLayout(false);
			this.ApportionedChargesGrid.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ConsolCostGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ApportionedChargesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel InvoiceAndPaymentDetailsPanel;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CreditorFindBox;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PlaceOfSupplyDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox BankAccountFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ChequeBookFindBox;
		private Enterprise.ZArchitecture.ZTextBox ChequeNumberTextBox;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox OSCostAmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsPostedCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox LocalCostAmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox WHTAmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox TaxAmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox LineTotalCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox WHTRateFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox TaxRateFindBox;
		private Enterprise.ZArchitecture.GUI.ZStmNotePopupButton AutoRatingNotePopupButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IncludeOnCollectCheckBox;
		private Enterprise.ZArchitecture.ZLabel AutoAllocateZLabel;
		protected Enterprise.ZArchitecture.GUI.ZPanel ExtraTaxPanel;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox ExtraTaxAmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZTabControl DetailsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage CostRateAuditTabPage;
		private Enterprise.ZArchitecture.ZTextBox CostRateAuditTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ApportionToChildShipmentsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OverrideTaxAmountCheckBox;
		private ZArchitecture.ZTextBox CostReferenceTextBox;
		private ZArchitecture.GUI.ZCodeFindBox ConsolCostOwnerFindBox;
		private ZArchitecture.GUI.ZGroupBox InvoiceTotalsGroupBox;
		private ZArchitecture.ZCalcEdit LocalTotalAmountCalcEdit;
		private ZArchitecture.ZCalcEdit OSTotalAmountCalcEdit;
		private ZArchitecture.ZTextBox LocalTotalCurrencyTextBox;
		private ZArchitecture.ZTextBox OSTotalCurrencyTextBox;
		private CalculationXMLUserControl auditLogNoteUserControl;
		private PaymentBasisUserControl paymentBasisUserControl;
		private WiseRatesRawDataUserControl WiseRatesRawDataUserControl;
		protected ZArchitecture.ZGrid CostSummaryGrid;
		private ZArchitecture.ZGrid ApportionedChargesGrid;
		private ZArchitecture.ZCalcEdit zCalcEdit1;
		private ZArchitecture.ZCalcEdit zCalcEdit2;
		private ZArchitecture.ZTextBox zTextBox2;
		private ZArchitecture.ZTextBox zTextBox3;
		private ZArchitecture.ZLabel CostHidingMessage;
		protected ZArchitecture.GUI.ZPanel WithholdingTaxPanel;
		ZDateEdit TaxDateEdit;
		private ZDateEdit DocReceivedDateEdit;
		private ZDropEdit RatingBehaviourDropEdit;
		private ZDropEdit CostSupplyTypeDropEdit;
		private ZGuidFindBox CostTaxBranchGuidFindBox;
	}
}
