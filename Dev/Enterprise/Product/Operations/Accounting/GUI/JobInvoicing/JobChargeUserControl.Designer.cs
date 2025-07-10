using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.GUI;
using Enterprise.Core.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Organisation.UserControls.Address;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobChargeUserControl
	{

		#region Windows Form Designer generated code

		ZCalcEdit zCalcEdit1;
		ZTextBox OSEstimatedCostAmountCurrencyTextBox;
		ZTextBox OSSellEstimatedAmountCurrency;
		ZLabel AutoAllocateZLabel;
		protected internal ZPanel SellExtraTaxPanel;
		ZTextBox ExtraTaxSellTaxCurrencyTextBox;
		ZCalcEdit ExtraTaxSellTaxAmountCalcEdit;
		protected internal ZPanel CostExtraTaxPanel;
		ZTextBox ExtraTaxCurrencyTextBox;
		ZCalcEdit ExtraTaxCostTaxAmountCalcEdit;
		ZCalcEdit OSSellEstimatedAmountCalcEdit;
		protected internal ZLabel ChargeableUnitLabel;
		ZCalcEdit ChargeableWgtVol;
		ZButton AutoPopulateButton;
		ZCalcEdit CFXCalcEdit;
		ZTextBox LocalCostAmountCurrencyTextBox;
		ZCheckBox PreventInvoicePrintGroupingCheckBox;
		ZDropEdit InvoiceTypeDropEdit;
		ZGuidFindBox DebtorFindBox;
		ZCheckBox RevenuePostedCheckBox;
		ZTextBox InvoiceNumberTextBox;
		ZCheckBox ApportionedCheckBox;
		ZCheckBox CostPostedCheckBox;
		ZGuidFindBox CreditorsGuidFindBox;
		ZCalcEdit LocalCostAmountCalcEdit;
		ZCalcEdit OSCostAmountCalcEdit;
		ZTextBox WHTSellTaxCurrencyTextBox;
		ZPanel CFXAmtPanel;
		ZTextBox CFXAmtCurrencyTextBox;
		ZCalcEdit CFXAmtCalcEdit;
		ZCalcEdit OSSellAmountCalcEdit;
		ZTextBox LocalSellAmountCurrency;
		ZCalcEdit LocalSellAmountCalcEdit;
		ZCalcEdit WHTSellTaxAmountCalcEdit;
		KPanel SellPanel;
		internal protected ZPanel SellGSTPanel;
		internal protected ZPanel SellWHTPanel;
		ZCalcEdit SellGSTAmtCalcEdit;
		ZTextBox SellGSTAmountCurrencyTextBox;
		ZGuidFindBox SellGSTRateGuidFindBox;
		ZDateEdit SellTaxDateEdit;
		ZGuidFindBox SellWHTRateGuidFindBox;
		internal protected Job Job;
		ZTabPage AutoratingSellTabPage;
		ZTabPage TaxTransactionTabPage;
		TaxTransactionsLinkedToJobChargeControl TaxTransactionControl;
		ZStmNotePopupButton AutoRateNotePopupButton;
		ZTabPage AutoratingCostTabPage;
		ZTextBox AutoRateDescRevenueTextBox;
		ZStmNotePopupButton AutoRateNotePopupButton2;
		WiseRatesRawDataUserControl WiseRatesRawDataUserControl;
		ZTextBox AutoRateDescCostTextBox;
		internal protected KPanel CostGSTPanel;
		ZGuidFindBox JR_AT_CostBoundFindBox;
		ZDateEdit JR_CostTaxDateEdit;
		ZGuidFindBox JR_AW_CostBoundFindBox;
		internal protected KPanel CostWHTPanel;
		KPanel panel3;
		public ZAddressWithContactControl JH_OH_LocalChargesBoundOrgCard;
		internal protected ZTabControl ChargesDetailsTabControl;
		ZTabPage CostTabPage;
		ZTabPage RevenueTabPage;
		ZTextBox GSTTaxCurrencyTextBox;
		ZTextBox WHTTaxCurrencyTextBox;
		ZDropEdit JR_PaymentTypeDropDownEdit;
		ZGuidFindBox ChequeBookFindBox;
		ZDateEdit JR_PaymentDateBoundDateEdit;
		ZTextBox JR_ChequeNoBoundTextEdit;
		ZGuidFindBox JR_ABBoundFindBox;
		ZCalcEdit LineTotalSellAmountCalcEdit;
		ZCalcEdit GSTCostTaxAmountCalcEdit;
		ZCalcEdit WHTCostTaxAmountCalcEdit;
		ZDateEdit JR_APInvoiceDateBoundDateEdit;
		ZDateEdit JR_APDocumentReceivedDateBoundDateEdit;
		ZTextBox JR_APInvoiceNumBoundTextEdit;
		public ZCalcEdit TotalCostAmountCalcEdit;
		public ZCalcEdit TotalAgentAmountCalcEdit;
		public ZCalcEdit ProfitLossAmountCalcEdit;
		ZTextBox LineTotalSellAmountCurrentTextBox;
		ZTextBox ProfitLossCurrencytTextBox;
		ZPanel OverseasAgentPanel;
		public ZOrgAddressControl JH_OH_AgentCollectBoundOrgCard;
		ZCodeFindBox SalesRepFindBox;
		ZCodeFindBox OperatorFindBox;
		ZTextBox HoldReasonTextBox;
		ZTextBox JobLocalReferenceTextBox;
		ZDateEdit JobCloseDateEdit;
		ZDateEdit JobOpeningDateEdit;
		ZDropEdit JobStatusDropDownEdit;
		ZGuidFindBox JH_GEBoundFindBox;
		ZGuidFindBox JH_GBBoundFindBox;
		KPanel SellTotalsPanel;
		protected internal ZPanel CostTotalsPanel;
		KPanel CostTotalPanel;
		ZTextBox TotalAmtOnInvoiceForJobCurrencyTextBox;
		ZCalcEdit TotalInvoiceAmountIncGSTCalcEdit;
		ZCalcEdit InvoiceGSTAmountCalcEdit;
		ZTextBox TotalTaxOnInvoiceForJobCurrencyTextBox;
		ZTextBox LineTotalCostCurrencyTextBox;
		ZPanel InvoicingFieldsPanel;
		ZTabPage DetailsTabPage;
		ZPanel RevenueOtherFieldsPanel;
		ZCalcEdit MarginPercentageCalcEdit;
		ZTextBox ChargeTypeTextBox;
		ZGuidFindBox ChargeGuidFindBox;
		ZGuidFindBox DeptGuidFindBox;
		ZGuidFindBox BranchesGuidFindBox;
		ZPanel ProfitSharePanel;
		ZGroupBox ProfitShareDetailsGroupBox;
		ZCalcEdit LineTotalCostAmountCalcEdit;
		ZCheckBox IncludeInProfitShareCheckbox;
		ZLabel LocalAmountDeclaredForProfitShareLabel;
		ZLabel OSAmountDeclaredForProfitShareLabel;
		ZCalcEdit LocalAgentDeclaredSellAmountCalcEdit;
		ZCalcEdit AgentDeclaredSellAmountCalcEdit;
		ZCalcEdit LocalAgentDeclaredCostAmountCalcEdit;
		ZTextBox LocalAgentDeclaredSellCurrencyTextBox;
		ZCalcEdit AgentDeclaredCostAmountCalcEdit;
		ZTextBox LocalAgentDeclaredCostCurrencyTextBox;
		ZTextBox AgentDeclaredSellCurrencyTextBox;
		ZTextBox AgentDeclaredCostCurrencyTextBox;
		ZTextBox SellRatingOverrideCommentTextBox;
		ZDropEdit CostRatingBehaviorEdit;
		ZDropEdit SellRatingBehaviorEdit;
		ZCalcEdit PLMarginCalcEdit;
		ZDropEdit PLReasonDropEdit;
		ZLabel ChargeHidingMessageLabel;
		ZDropEdit CostSupplyTypeDropEdit;
		ZDropEdit SellSupplyTypeDropEdit;

		protected void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZCheckBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new ZOrganisationFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZCalcEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new ZCheckBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo8 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo9 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo10 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo11 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo12 = new ZGuidFindBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo19 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo20 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new ZDropEditColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new ZOrganisationFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo21 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo22 = new ZCalcEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new ZTextBoxColumnStyleInfo();
			this.ChargesDetailsTabControl = new ZTabControl();
			this.DetailsTabPage = new ZTabPage();
			this.ProfitSharePanel = new ZPanel();
			this.ProfitShareDetailsGroupBox = new ZGroupBox();
			this.IncludeInProfitShareCheckbox = new ZCheckBox();
			this.LocalAmountDeclaredForProfitShareLabel = new ZLabel();
			this.OSAmountDeclaredForProfitShareLabel = new ZLabel();
			this.LocalAgentDeclaredSellAmountCalcEdit = new ZCalcEdit();
			this.AgentDeclaredSellAmountCalcEdit = new ZCalcEdit();
			this.LocalAgentDeclaredCostAmountCalcEdit = new ZCalcEdit();
			this.LocalAgentDeclaredSellCurrencyTextBox = new ZTextBox();
			this.AgentDeclaredCostAmountCalcEdit = new ZCalcEdit();
			this.LocalAgentDeclaredCostCurrencyTextBox = new ZTextBox();
			this.AgentDeclaredSellCurrencyTextBox = new ZTextBox();
			this.AgentDeclaredCostCurrencyTextBox = new ZTextBox();
			this.RevenueOtherFieldsPanel = new ZPanel();
			this.JR_GB_InternalBranchGuidFindBox = new ZGuidFindBox();
			this.JR_JH_InternalJobGuidFindBox = new ZGuidFindBox();
			this.JR_GE_InternalDeptGuidFindBox = new ZGuidFindBox();
			this.ChargeableUnitLabel = new ZLabel();
			this.ChargeableWgtVol = new ZCalcEdit();
			this.MarginPercentageCalcEdit = new ZCalcEdit();
			this.ChargeTypeTextBox = new ZTextBox();
			this.ChargeGuidFindBox = new ZGuidFindBox();
			this.DeptGuidFindBox = new ZGuidFindBox();
			this.BranchesGuidFindBox = new ZGuidFindBox();
			this.CostTabPage = new ZTabPage();
			this.CostTotalsPanel = new ZPanel();
			this.CashAdvanceButton = new ZButton();
			this.JR_CostReferenceTextBox = new ZTextBox();
			this.CostRatingBehaviorEdit = new ZDropEdit();
			this.costRatingOverrideCommentTextBox = new ZTextBox();
			this.costRatedCheckBox = new ZCheckBox();
			this.OSCostCurrencyCodeFindBox = new ZCodeFindBox();
			this.AutoAllocateZLabel = new ZLabel();
			this.OSEstimatedCostAmountCurrencyTextBox = new ZTextBox();
			this.zCalcEdit1 = new ZCalcEdit();
			this.AutoPopulateButton = new ZButton();
			this.JR_APInvoiceDateBoundDateEdit = new ZDateEdit();
			this.JR_APDocumentReceivedDateBoundDateEdit = new ZDateEdit();
			this.JR_APInvoiceNumBoundTextEdit = new ZTextBox();
			this.CreditorsGuidFindBox = new ZGuidFindBox();
			this.JR_PaymentDateBoundDateEdit = new ZDateEdit();
			this.JR_PaymentTypeDropDownEdit = new ZDropEdit();
			this.JR_ABBoundFindBox = new ZGuidFindBox();
			this.ChequeBookFindBox = new ZGuidFindBox();
			this.JR_ChequeNoBoundTextEdit = new ZTextBox();
			this.CostPostedCheckBox = new ZCheckBox();
			this.ApportionedCheckBox = new ZCheckBox();
			this.OSCostAmountCalcEdit = new ZCalcEdit();
			this.LocalCostAmountCurrencyTextBox = new ZTextBox();
			this.LocalCostAmountCalcEdit = new ZCalcEdit();
			this.CostTotalPanel = new KPanel();
			this.CostExtraTaxPanel = new ZPanel();
			this.ExtraTaxCurrencyTextBox = new ZTextBox();
			this.ExtraTaxCostTaxAmountCalcEdit = new ZCalcEdit();
			this.LineTotalCostCurrencyTextBox = new ZTextBox();
			this.LineTotalCostAmountCalcEdit = new ZCalcEdit();
			this.TotalAmtOnInvoiceForJobCurrencyTextBox = new ZTextBox();
			this.TotalInvoiceAmountIncGSTCalcEdit = new ZCalcEdit();
			this.InvoiceGSTAmountCalcEdit = new ZCalcEdit();
			this.TotalTaxOnInvoiceForJobCurrencyTextBox = new ZTextBox();
			this.CostSupplyTypeDropEdit = new ZDropEdit();
			this.CostWHTPanel = new KPanel();
			this.JR_AW_CostBoundFindBox = new ZGuidFindBox();
			this.WHTTaxCurrencyTextBox = new ZTextBox();
			this.WHTCostTaxAmountCalcEdit = new ZCalcEdit();
			this.CostGSTPanel = new KPanel();
			this.CostTaxBranchGuidFindBox = new ZGuidFindBox();
			this.GSTTaxCurrencyTextBox = new ZTextBox();
			this.JR_AT_CostBoundFindBox = new ZGuidFindBox();
			this.JR_CostTaxDateEdit = new ZDateEdit();
			this.GSTCostTaxAmountCalcEdit = new ZCalcEdit();
			this.RevenueTabPage = new ZTabPage();
			this.panel3 = new KPanel();
			this.CashAdvancePanel = new ZPanel();
			this.cashAdvanceRequestStatusTextBox = new ZTextBox();
			this.cashAdvanceRequestIDTextBox = new ZTextBox();
			this.cashAdvanceRequiedCheckBox = new ZCheckBox();
			this.SellTotalsPanel = new KPanel();
			this.SellExtraTaxPanel = new ZPanel();
			this.ExtraTaxSellTaxCurrencyTextBox = new ZTextBox();
			this.ExtraTaxSellTaxAmountCalcEdit = new ZCalcEdit();
			this.SellGSTPanel = new ZPanel();
			this.SellTaxBranchGuidFindBox = new ZGuidFindBox();
			this.SellGSTAmtCalcEdit = new ZCalcEdit();
			this.SellGSTAmountCurrencyTextBox = new ZTextBox();
			this.SellGSTRateGuidFindBox = new ZGuidFindBox();
			this.SellTaxDateEdit = new ZDateEdit();
			this.CFXAmtPanel = new ZPanel();
			this.CFXAmtCurrencyTextBox = new ZTextBox();
			this.CFXAmtCalcEdit = new ZCalcEdit();
			this.LineTotalSellAmountCurrentTextBox = new ZTextBox();
			this.LineTotalSellAmountCalcEdit = new ZCalcEdit();
			this.SellWHTPanel = new ZPanel();
			this.SellWHTRateGuidFindBox = new ZGuidFindBox();
			this.WHTSellTaxCurrencyTextBox = new ZTextBox();
			this.WHTSellTaxAmountCalcEdit = new ZCalcEdit();
			this.InvoiceNumberTextBox = new ZTextBox();
			this.SellSupplyTypeDropEdit = new ZDropEdit();
			this.SellPanel = new KPanel();
			this.OSSellAmountCurrencyCodeFindBox = new ZCodeFindBox();
			this.OSSellEstimatedAmountCurrency = new ZTextBox();
			this.OSSellEstimatedAmountCalcEdit = new ZCalcEdit();
			this.SellRatingBehaviorEdit = new ZDropEdit();
			this.SellRatingOverrideCommentTextBox = new ZTextBox();
			this.sellRatedCheckBox = new ZCheckBox();
			this.CFXCalcEdit = new ZCalcEdit();
			this.PreventInvoicePrintGroupingCheckBox = new ZCheckBox();
			this.InvoiceTypeDropEdit = new ZDropEdit();
			this.OSSellAmountCalcEdit = new ZCalcEdit();
			this.LocalSellAmountCurrency = new ZTextBox();
			this.LocalSellAmountCalcEdit = new ZCalcEdit();
			this.DebtorFindBox = new ZGuidFindBox();
			this.RevenuePostedCheckBox = new ZCheckBox();
			this.hasDebtorAcceptedThisSellCharge = new ZCheckBox();
			this.AutoratingCostTabPage = new ZTabPage();
			this.CostRateCalculationXMLUserControl = new ConsolCosting.CalculationXMLUserControl();
			this.CostPaymentBasisUserControl = new PaymentBasisUserControl();
			this.AutoRateNotePopupButton2 = new ZStmNotePopupButton();
			this.AutoRateDescCostTextBox = new ZTextBox();
			this.WiseRatesRawDataUserControl = new WiseRatesRawDataUserControl();
			this.AutoratingSellTabPage = new ZTabPage();
			this.RevenueRateCalculationXMLUserControl = new ConsolCosting.CalculationXMLUserControl();
			this.RevenuePaymentBasisUserControl = new PaymentBasisUserControl();
			this.AutoRateDescRevenueTextBox = new ZTextBox();
			this.AutoRateNotePopupButton = new ZStmNotePopupButton();
			this.TaxTransactionTabPage = new ZTabPage();
			this.TaxTransactionControl = new TaxTransactionsLinkedToJobChargeControl();
			this.JH_OH_LocalChargesBoundOrgCard = new ZAddressWithContactControl();
			this.TotalCostAmountCalcEdit = new ZCalcEdit();
			this.TotalAgentAmountCalcEdit = new ZCalcEdit();
			this.ProfitLossCurrencytTextBox = new ZTextBox();
			this.ProfitLossAmountCalcEdit = new ZCalcEdit();
			this.OverseasAgentPanel = new ZPanel();
			this.JH_OH_AgentCollectBoundOrgCard = new ZOrgAddressControl();
			this.InvoicingFieldsPanel = new ZPanel();
			this.TaxBranchGuidFindBox = new ZGuidFindBox();
			this.IsJobDescriptionOverriden = new ZCheckBox();
			this.JobDescriptionTextBox = new ZTextBox();
			this.PLMarginCalcEdit = new ZCalcEdit();
			this.SalesRepFindBox = new ZCodeFindBox();
			this.OperatorFindBox = new ZCodeFindBox();
			this.HoldReasonTextBox = new ZTextBox();
			this.JobLocalReferenceTextBox = new ZTextBox();
			this.JobCloseDateEdit = new ZDateEdit();
			this.JobOpeningDateEdit = new ZDateEdit();
			this.PLReasonDropEdit = new ZDropEdit();
			this.JobStatusDropDownEdit = new ZDropEdit();
			this.JH_GEBoundFindBox = new ZGuidFindBox();
			this.JH_GBBoundFindBox = new ZGuidFindBox();
			this.TotalCFXAmountCalcEdit = new ZCalcEdit();
			this.TotalsPanel = new ZPanel();
			this.TotalsLabel = new ZLabel();
			this.JobChargeBoundGrid = new ZGrid();
			this.ChargeHidingMessageLabel = new ZLabel();
			this.ExcludeFromPeriodicRatingCheckbox = new ZCheckBox();
			this.ShipmentAndBillingDetailsLinkLabel = new ZLinkLabel();
			this.IsChargeCostReferenceFilterEnabledCheckBox = new ZCheckBox();
			this.ClientContractNumberTextBox = new ZTextBox();
			this.ClientContractNumberButton = new ZButton.Bare();
			this.QuotesCodeFindBox = new ZCodeFindBox();
			this.ExchangeRatesZPanel = new ZPanel();
			this.JobExRateBoundGrid = new ZGrid();
			this.TaxExpenseTotalsPanel = new ZPanel();
			this.TaxExpenseTotalsLabel = new ZLabel();
			this.TotalTaxExpenseRevenueCalcEdit = new ZCalcEdit();
			this.TotalTaxExpenseCostCalcEdit = new ZCalcEdit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ChargesDetailsTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.ProfitSharePanel.SuspendLayout();
			this.ProfitShareDetailsGroupBox.SuspendLayout();
			this.RevenueOtherFieldsPanel.SuspendLayout();
			this.JR_GB_InternalBranchGuidFindBox.SuspendLayout();
			this.JR_JH_InternalJobGuidFindBox.SuspendLayout();
			this.JR_GE_InternalDeptGuidFindBox.SuspendLayout();
			this.ChargeGuidFindBox.SuspendLayout();
			this.DeptGuidFindBox.SuspendLayout();
			this.BranchesGuidFindBox.SuspendLayout();
			this.CostTabPage.SuspendLayout();
			this.CostTotalsPanel.SuspendLayout();
			this.CostRatingBehaviorEdit.SuspendLayout();
			this.OSCostCurrencyCodeFindBox.SuspendLayout();
			this.JR_APInvoiceDateBoundDateEdit.SuspendLayout();
			this.JR_APDocumentReceivedDateBoundDateEdit.SuspendLayout();
			this.CreditorsGuidFindBox.SuspendLayout();
			this.JR_PaymentDateBoundDateEdit.SuspendLayout();
			this.JR_PaymentTypeDropDownEdit.SuspendLayout();
			this.JR_ABBoundFindBox.SuspendLayout();
			this.ChequeBookFindBox.SuspendLayout();
			this.CostTotalPanel.SuspendLayout();
			this.CostExtraTaxPanel.SuspendLayout();
			this.CostSupplyTypeDropEdit.SuspendLayout();
			this.CostWHTPanel.SuspendLayout();
			this.JR_AW_CostBoundFindBox.SuspendLayout();
			this.CostGSTPanel.SuspendLayout();
			this.CostTaxBranchGuidFindBox.SuspendLayout();
			this.JR_AT_CostBoundFindBox.SuspendLayout();
			this.JR_CostTaxDateEdit.SuspendLayout();
			this.RevenueTabPage.SuspendLayout();
			this.panel3.SuspendLayout();
			this.CashAdvancePanel.SuspendLayout();
			this.SellTotalsPanel.SuspendLayout();
			this.SellExtraTaxPanel.SuspendLayout();
			this.SellGSTPanel.SuspendLayout();
			this.SellTaxBranchGuidFindBox.SuspendLayout();
			this.SellGSTRateGuidFindBox.SuspendLayout();
			this.SellTaxDateEdit.SuspendLayout();
			this.CFXAmtPanel.SuspendLayout();
			this.SellWHTPanel.SuspendLayout();
			this.SellWHTRateGuidFindBox.SuspendLayout();
			this.SellSupplyTypeDropEdit.SuspendLayout();
			this.SellPanel.SuspendLayout();
			this.OSSellAmountCurrencyCodeFindBox.SuspendLayout();
			this.SellRatingBehaviorEdit.SuspendLayout();
			this.InvoiceTypeDropEdit.SuspendLayout();
			this.DebtorFindBox.SuspendLayout();
			this.AutoratingCostTabPage.SuspendLayout();
			this.CostRateCalculationXMLUserControl.SuspendLayout();
			this.CostPaymentBasisUserControl.SuspendLayout();
			this.AutoRateDescCostTextBox.SuspendLayout();
			this.WiseRatesRawDataUserControl.SuspendLayout();
			this.AutoratingSellTabPage.SuspendLayout();
			this.RevenueRateCalculationXMLUserControl.SuspendLayout();
			this.RevenuePaymentBasisUserControl.SuspendLayout();
			this.AutoRateDescRevenueTextBox.SuspendLayout();
			this.TaxTransactionTabPage.SuspendLayout();
			this.TaxTransactionControl.SuspendLayout();
			this.JH_OH_LocalChargesBoundOrgCard.SuspendLayout();
			this.OverseasAgentPanel.SuspendLayout();
			this.JH_OH_AgentCollectBoundOrgCard.SuspendLayout();
			this.InvoicingFieldsPanel.SuspendLayout();
			this.TaxBranchGuidFindBox.SuspendLayout();
			this.SalesRepFindBox.SuspendLayout();
			this.OperatorFindBox.SuspendLayout();
			this.JobCloseDateEdit.SuspendLayout();
			this.JobOpeningDateEdit.SuspendLayout();
			this.PLReasonDropEdit.SuspendLayout();
			this.JobStatusDropDownEdit.SuspendLayout();
			this.JH_GEBoundFindBox.SuspendLayout();
			this.JH_GBBoundFindBox.SuspendLayout();
			this.TotalsPanel.SuspendLayout();
			((ISupportInitialize)(this.JobChargeBoundGrid)).BeginInit();
			this.JobChargeBoundGrid.SuspendLayout();
			this.QuotesCodeFindBox.SuspendLayout();
			this.ExchangeRatesZPanel.SuspendLayout();
			((ISupportInitialize)(this.JobExRateBoundGrid)).BeginInit();
			this.JobExRateBoundGrid.SuspendLayout();
			this.TaxExpenseTotalsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Job);
			// 
			// ChargesDetailsTabControl
			// 
			this.ChargesDetailsTabControl.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ChargesDetailsTabControl.Controls.Add(this.DetailsTabPage);
			this.ChargesDetailsTabControl.Controls.Add(this.CostTabPage);
			this.ChargesDetailsTabControl.Controls.Add(this.RevenueTabPage);
			this.ChargesDetailsTabControl.Controls.Add(this.AutoratingCostTabPage);
			this.ChargesDetailsTabControl.Controls.Add(this.AutoratingSellTabPage);
			this.ChargesDetailsTabControl.Controls.Add(this.TaxTransactionTabPage);
			this.ChargesDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 306, true);
			this.ChargesDetailsTabControl.Name = "ChargesDetailsTabControl";
			this.ChargesDetailsTabControl.SelectedIndex = 0;
			this.ChargesDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 191, true);
			this.ChargesDetailsTabControl.TabIndex = 7;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|4d0cbb5d-95cc-4f27-8835-15986226c341", "Detail");
			this.DetailsTabPage.Controls.Add(this.ProfitSharePanel);
			this.DetailsTabPage.Controls.Add(this.RevenueOtherFieldsPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 164, true);
			this.DetailsTabPage.TabIndex = 5;
			// 
			// ProfitSharePanel
			// 
			this.ProfitSharePanel.Controls.Add(this.ProfitShareDetailsGroupBox);
			this.ProfitSharePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.ProfitSharePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 0, true);
			this.ProfitSharePanel.Name = "ProfitSharePanel";
			this.ProfitSharePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 164, true);
			this.ProfitSharePanel.TabIndex = 1;
			// 
			// ProfitShareDetailsGroupBox
			// 
			this.ProfitShareDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|b8be0094-d758-4a91-8d9e-9c9f7d17202c", "Profit Share");
			this.ProfitShareDetailsGroupBox.Controls.Add(this.IncludeInProfitShareCheckbox);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.LocalAmountDeclaredForProfitShareLabel);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.OSAmountDeclaredForProfitShareLabel);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.LocalAgentDeclaredSellAmountCalcEdit);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.AgentDeclaredSellAmountCalcEdit);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.LocalAgentDeclaredCostAmountCalcEdit);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.LocalAgentDeclaredSellCurrencyTextBox);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.AgentDeclaredCostAmountCalcEdit);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.LocalAgentDeclaredCostCurrencyTextBox);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.AgentDeclaredSellCurrencyTextBox);
			this.ProfitShareDetailsGroupBox.Controls.Add(this.AgentDeclaredCostCurrencyTextBox);
			this.ProfitShareDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 2, true);
			this.ProfitShareDetailsGroupBox.Name = "ProfitShareDetailsGroupBox";
			this.ProfitShareDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 118, true);
			this.ProfitShareDetailsGroupBox.TabIndex = 0;
			this.ProfitShareDetailsGroupBox.TabStop = false;
			// 
			// IncludeInProfitShareCheckbox
			// 
			this.IncludeInProfitShareCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeInProfitShareCheckbox, "FilteredCharges.JR_IsIncludedInProfitShare");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsIncludedInProfitShare)));
			this.IncludeInProfitShareCheckbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|949bd0c0-5162-4f71-9bf3-2a39f775d24e", "Include this Charge in Profit Share");
			this.IncludeInProfitShareCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 15, true);
			this.IncludeInProfitShareCheckbox.Name = "IncludeInProfitShareCheckbox";
			this.IncludeInProfitShareCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 17, true);
			this.IncludeInProfitShareCheckbox.TabIndex = 0;
			// 
			// LocalAmountDeclaredForProfitShareLabel
			// 
			this.LocalAmountDeclaredForProfitShareLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|6ad32b6d-de79-41ea-8831-5454aa027fb9", "Local Amount");
			this.LocalAmountDeclaredForProfitShareLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LocalAmountDeclaredForProfitShareLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 38, true);
			this.LocalAmountDeclaredForProfitShareLabel.Name = "LocalAmountDeclaredForProfitShareLabel";
			this.LocalAmountDeclaredForProfitShareLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.LocalAmountDeclaredForProfitShareLabel.TabIndex = 3;
			// 
			// OSAmountDeclaredForProfitShareLabel
			// 
			this.OSAmountDeclaredForProfitShareLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|1fece175-ed74-40f7-878e-9da75793060d", "OS Amount");
			this.OSAmountDeclaredForProfitShareLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OSAmountDeclaredForProfitShareLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 38, true);
			this.OSAmountDeclaredForProfitShareLabel.Name = "OSAmountDeclaredForProfitShareLabel";
			this.OSAmountDeclaredForProfitShareLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.OSAmountDeclaredForProfitShareLabel.TabIndex = 1;
			// 
			// LocalAgentDeclaredSellAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LocalAgentDeclaredSellAmountCalcEdit, "FilteredCharges.JR_AgentDeclaredSellAmtLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AgentDeclaredSellAmtLocal)));
			this.LocalAgentDeclaredSellAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|d72400b0-6113-4b51-b54e-5dcda2aa46d5", "Agent Declared Local Sell Amount");
			this.LocalAgentDeclaredSellAmountCalcEdit.DecimalPlaces = 2;
			this.LocalAgentDeclaredSellAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 88, true);
			this.LocalAgentDeclaredSellAmountCalcEdit.Name = "LocalAgentDeclaredSellAmountCalcEdit";
			this.LocalAgentDeclaredSellAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.LocalAgentDeclaredSellAmountCalcEdit.TabIndex = 10;
			this.LocalAgentDeclaredSellAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AgentDeclaredSellAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AgentDeclaredSellAmountCalcEdit, "FilteredCharges.JR_AgentDeclaredSellAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AgentDeclaredSellAmt)));
			this.AgentDeclaredSellAmountCalcEdit.CaptionResourceString = null;
			this.AgentDeclaredSellAmountCalcEdit.DecimalPlaces = 2;
			this.AgentDeclaredSellAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 88, true);
			this.AgentDeclaredSellAmountCalcEdit.Name = "AgentDeclaredSellAmountCalcEdit";
			this.AgentDeclaredSellAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.AgentDeclaredSellAmountCalcEdit.TabIndex = 8;
			this.AgentDeclaredSellAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalAgentDeclaredCostAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LocalAgentDeclaredCostAmountCalcEdit, "FilteredCharges.JR_AgentDeclaredCostAmtLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AgentDeclaredCostAmtLocal)));
			this.LocalAgentDeclaredCostAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|434bf24d-1ce1-4f20-b03a-c8aa462b8502", "Agent Declared Local Cost Amount");
			this.LocalAgentDeclaredCostAmountCalcEdit.DecimalPlaces = 2;
			this.LocalAgentDeclaredCostAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 64, true);
			this.LocalAgentDeclaredCostAmountCalcEdit.Name = "LocalAgentDeclaredCostAmountCalcEdit";
			this.LocalAgentDeclaredCostAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.LocalAgentDeclaredCostAmountCalcEdit.TabIndex = 6;
			this.LocalAgentDeclaredCostAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalAgentDeclaredSellCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalAgentDeclaredSellCurrencyTextBox, "JH_LocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_LocalCurrency)));
			this.LocalAgentDeclaredSellCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|09a2bf3d-dba0-477a-a3dd-83de4cbfc926", "Local Currency", "Displays the currency of the tax amount.");
			this.LocalAgentDeclaredSellCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 88, true);
			this.LocalAgentDeclaredSellCurrencyTextBox.Name = "LocalAgentDeclaredSellCurrencyTextBox";
			this.LocalAgentDeclaredSellCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.LocalAgentDeclaredSellCurrencyTextBox.TabIndex = 11;
			// 
			// AgentDeclaredCostAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AgentDeclaredCostAmountCalcEdit, "FilteredCharges.JR_AgentDeclaredCostAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AgentDeclaredCostAmt)));
			this.AgentDeclaredCostAmountCalcEdit.CaptionResourceString = null;
			this.AgentDeclaredCostAmountCalcEdit.DecimalPlaces = 2;
			this.AgentDeclaredCostAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 64, true);
			this.AgentDeclaredCostAmountCalcEdit.Name = "AgentDeclaredCostAmountCalcEdit";
			this.AgentDeclaredCostAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.AgentDeclaredCostAmountCalcEdit.TabIndex = 4;
			this.AgentDeclaredCostAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalAgentDeclaredCostCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalAgentDeclaredCostCurrencyTextBox, "JH_LocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_LocalCurrency)));
			this.LocalAgentDeclaredCostCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|9f2548d1-6d29-4169-95e1-6129c5f2b8cf", "Local Currency", "Displays the currency of the tax amount.");
			this.LocalAgentDeclaredCostCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 64, true);
			this.LocalAgentDeclaredCostCurrencyTextBox.Name = "LocalAgentDeclaredCostCurrencyTextBox";
			this.LocalAgentDeclaredCostCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.LocalAgentDeclaredCostCurrencyTextBox.TabIndex = 7;
			// 
			// AgentDeclaredSellCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentDeclaredSellCurrencyTextBox, "FilteredCharges.JR_OSSellCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellCurrencyCode)));
			this.AgentDeclaredSellCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|dff2baf9-487b-412a-9194-397dc0b7cc0e", "Sell Currency", "Currency of the WHT Tax.");
			this.AgentDeclaredSellCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 88, true);
			this.AgentDeclaredSellCurrencyTextBox.Name = "AgentDeclaredSellCurrencyTextBox";
			this.AgentDeclaredSellCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AgentDeclaredSellCurrencyTextBox.TabIndex = 9;
			// 
			// AgentDeclaredCostCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgentDeclaredCostCurrencyTextBox, "FilteredCharges.JR_OSCostCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSCostCurrencyCode)));
			this.AgentDeclaredCostCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|353bbf3b-6ef9-4435-b94d-975cbf88c487", "Cost Currency", "Displays the currency of the tax amount.");
			this.AgentDeclaredCostCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 64, true);
			this.AgentDeclaredCostCurrencyTextBox.Name = "AgentDeclaredCostCurrencyTextBox";
			this.AgentDeclaredCostCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AgentDeclaredCostCurrencyTextBox.TabIndex = 5;
			// 
			// RevenueOtherFieldsPanel
			// 
			this.RevenueOtherFieldsPanel.Controls.Add(this.JR_GB_InternalBranchGuidFindBox);
			this.RevenueOtherFieldsPanel.Controls.Add(this.JR_JH_InternalJobGuidFindBox);
			this.RevenueOtherFieldsPanel.Controls.Add(this.JR_GE_InternalDeptGuidFindBox);
			this.RevenueOtherFieldsPanel.Controls.Add(this.ChargeableUnitLabel);
			this.RevenueOtherFieldsPanel.Controls.Add(this.ChargeableWgtVol);
			this.RevenueOtherFieldsPanel.Controls.Add(this.MarginPercentageCalcEdit);
			this.RevenueOtherFieldsPanel.Controls.Add(this.ChargeTypeTextBox);
			this.RevenueOtherFieldsPanel.Controls.Add(this.ChargeGuidFindBox);
			this.RevenueOtherFieldsPanel.Controls.Add(this.DeptGuidFindBox);
			this.RevenueOtherFieldsPanel.Controls.Add(this.BranchesGuidFindBox);
			this.RevenueOtherFieldsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.RevenueOtherFieldsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RevenueOtherFieldsPanel.Name = "RevenueOtherFieldsPanel";
			this.RevenueOtherFieldsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 164, true);
			this.RevenueOtherFieldsPanel.TabIndex = 0;
			// 
			// JR_GB_InternalBranchGuidFindBox
			// 
			this.JR_GB_InternalBranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JR_GB_InternalBranchGuidFindBox, "FilteredCharges.JR_GB_InternalBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GB_InternalBranch)));
			this.JR_GB_InternalBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 92, true);
			this.JR_GB_InternalBranchGuidFindBox.Name = "JR_GB_InternalBranchGuidFindBox";
			this.JR_GB_InternalBranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JR_GB_InternalBranchGuidFindBox.ParentType = null;
			this.JR_GB_InternalBranchGuidFindBox.PreBoundMaxLength = 3;
			this.JR_GB_InternalBranchGuidFindBox.ShowDescriptionBox = false;
			this.JR_GB_InternalBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.JR_GB_InternalBranchGuidFindBox.TabIndex = 10;
			// 
			// JR_JH_InternalJobGuidFindBox
			// 
			this.JR_JH_InternalJobGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JR_JH_InternalJobGuidFindBox, "FilteredCharges.JR_JH_InternalJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_JH_InternalJob)));
			this.JR_JH_InternalJobGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 40, true);
			this.JR_JH_InternalJobGuidFindBox.Name = "JR_JH_InternalJobGuidFindBox";
			this.JR_JH_InternalJobGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JR_JH_InternalJobGuidFindBox.ParentType = null;
			this.JR_JH_InternalJobGuidFindBox.ShowDescriptionBox = false;
			this.JR_JH_InternalJobGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JR_JH_InternalJobGuidFindBox.TabIndex = 8;
			// 
			// JR_GE_InternalDeptGuidFindBox
			// 
			this.JR_GE_InternalDeptGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JR_GE_InternalDeptGuidFindBox, "FilteredCharges.JR_GE_InternalDept");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GE_InternalDept)));
			this.JR_GE_InternalDeptGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 66, true);
			this.JR_GE_InternalDeptGuidFindBox.Name = "JR_GE_InternalDeptGuidFindBox";
			this.JR_GE_InternalDeptGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JR_GE_InternalDeptGuidFindBox.ParentType = null;
			this.JR_GE_InternalDeptGuidFindBox.PreBoundMaxLength = 3;
			this.JR_GE_InternalDeptGuidFindBox.ShowDescriptionBox = false;
			this.JR_GE_InternalDeptGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.JR_GE_InternalDeptGuidFindBox.TabIndex = 9;
			// 
			// ChargeableUnitLabel
			// 
			this.ChargeableUnitLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChargeableUnitLabel, "ChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).ChargeableUnit)));
			this.ChargeableUnitLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|bbc2984e-98eb-4991-b388-312f1fe6029e", "(CBM)");
			this.ChargeableUnitLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChargeableUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 17, true);
			this.ChargeableUnitLabel.Name = "ChargeableUnitLabel";
			this.ChargeableUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 13, true);
			this.ChargeableUnitLabel.TabIndex = 7;
			// 
			// ChargeableWgtVol
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWgtVol, "ChargeableWgtVol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Job)(null)).ChargeableWgtVol)));
			this.ChargeableWgtVol.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|2dd83d3d-09de-434b-9736-8542c286afab", "Chrg.", "Chargeable", "Chargeable Weight/Vol", "Chargeable Weight or Volume from the operations job.");
			this.ChargeableWgtVol.DecimalPlaces = 3;
			this.ChargeableWgtVol.Decimals = 3;
			this.ChargeableWgtVol.IsCalculatorEnabled = false;
			this.ChargeableWgtVol.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 16, true);
			this.ChargeableWgtVol.Name = "ChargeableWgtVol";
			this.ChargeableWgtVol.ReadOnly = true;
			this.ChargeableWgtVol.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.ChargeableWgtVol.TabIndex = 6;
			this.ChargeableWgtVol.TabStop = false;
			this.ChargeableWgtVol.Text = "0.000";
			this.ChargeableWgtVol.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarginPercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MarginPercentageCalcEdit, "FilteredCharges.MarginPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).MarginPercentage)));
			this.MarginPercentageCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|b3c472f7-222f-4648-b6e0-0bc4d9b11002", "Margin %");
			this.MarginPercentageCalcEdit.DecimalPlaces = 2;
			this.MarginPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 92, true);
			this.MarginPercentageCalcEdit.Name = "MarginPercentageCalcEdit";
			this.MarginPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.MarginPercentageCalcEdit.TabIndex = 5;
			this.MarginPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChargeTypeTextBox, "FilteredCharges.ChargeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ChargeType)));
			this.ChargeTypeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|7dd036fa-1e52-406a-a9bf-851b51758233", "Charge Type");
			this.ChargeTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 92, true);
			this.ChargeTypeTextBox.Name = "ChargeTypeTextBox";
			this.ChargeTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.ChargeTypeTextBox.TabIndex = 4;
			// 
			// ChargeGuidFindBox
			// 
			this.ChargeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeGuidFindBox, "FilteredCharges.JR_AC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AC)));
			this.ChargeGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|a1096f79-80fa-4b15-97b9-c1d428cd985c", "Charge Code");
			this.ChargeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			this.ChargeGuidFindBox.Name = "ChargeGuidFindBox";
			this.ChargeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ChargeGuidFindBox.ParentType = null;
			this.ChargeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.ChargeGuidFindBox.TabIndex = 0;
			// 
			// DeptGuidFindBox
			// 
			this.DeptGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeptGuidFindBox, "FilteredCharges.JR_GE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GE)));
			this.DeptGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|7d1bdf74-2f68-42e5-ba05-bdab835d0dd8", "Dept");
			this.DeptGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			this.DeptGuidFindBox.Name = "DeptGuidFindBox";
			this.DeptGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DeptGuidFindBox.ParentType = null;
			this.DeptGuidFindBox.PreBoundMaxLength = 3;
			this.DeptGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.DeptGuidFindBox.TabIndex = 1;
			// 
			// BranchesGuidFindBox
			// 
			this.BranchesGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchesGuidFindBox, "FilteredCharges.JR_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GB)));
			this.BranchesGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|c28b296c-7828-4793-8b10-efeb7c98258d", "Branch");
			this.BranchesGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 66, true);
			this.BranchesGuidFindBox.Name = "BranchesGuidFindBox";
			this.BranchesGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BranchesGuidFindBox.ParentType = null;
			this.BranchesGuidFindBox.PreBoundMaxLength = 3;
			this.BranchesGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.BranchesGuidFindBox.TabIndex = 3;
			// 
			// CostTabPage
			// 
			this.CostTabPage.AutoScroll = true;
			this.CostTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|30ea0719-63bf-439c-871d-2ec1ee6cc4f0", "Cost");
			this.CostTabPage.Controls.Add(this.CostTotalsPanel);
			this.CostTabPage.Controls.Add(this.CostTotalPanel);
			this.CostTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CostTabPage.Name = "CostTabPage";
			this.CostTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 164, true);
			this.CostTabPage.TabIndex = 0;
			// 
			// CostTotalsPanel
			// 
			this.CostTotalsPanel.Controls.Add(this.CashAdvanceButton);
			this.CostTotalsPanel.Controls.Add(this.JR_CostReferenceTextBox);
			this.CostTotalsPanel.Controls.Add(this.CostRatingBehaviorEdit);
			this.CostTotalsPanel.Controls.Add(this.costRatingOverrideCommentTextBox);
			this.CostTotalsPanel.Controls.Add(this.costRatedCheckBox);
			this.CostTotalsPanel.Controls.Add(this.OSCostCurrencyCodeFindBox);
			this.CostTotalsPanel.Controls.Add(this.AutoAllocateZLabel);
			this.CostTotalsPanel.Controls.Add(this.OSEstimatedCostAmountCurrencyTextBox);
			this.CostTotalsPanel.Controls.Add(this.zCalcEdit1);
			this.CostTotalsPanel.Controls.Add(this.AutoPopulateButton);
			this.CostTotalsPanel.Controls.Add(this.JR_APInvoiceDateBoundDateEdit);
			this.CostTotalsPanel.Controls.Add(this.JR_APDocumentReceivedDateBoundDateEdit);
			this.CostTotalsPanel.Controls.Add(this.JR_APInvoiceNumBoundTextEdit);
			this.CostTotalsPanel.Controls.Add(this.CreditorsGuidFindBox);
			this.CostTotalsPanel.Controls.Add(this.JR_PaymentDateBoundDateEdit);
			this.CostTotalsPanel.Controls.Add(this.JR_PaymentTypeDropDownEdit);
			this.CostTotalsPanel.Controls.Add(this.JR_ABBoundFindBox);
			this.CostTotalsPanel.Controls.Add(this.ChequeBookFindBox);
			this.CostTotalsPanel.Controls.Add(this.JR_ChequeNoBoundTextEdit);
			this.CostTotalsPanel.Controls.Add(this.CostPostedCheckBox);
			this.CostTotalsPanel.Controls.Add(this.ApportionedCheckBox);
			this.CostTotalsPanel.Controls.Add(this.OSCostAmountCalcEdit);
			this.CostTotalsPanel.Controls.Add(this.LocalCostAmountCurrencyTextBox);
			this.CostTotalsPanel.Controls.Add(this.LocalCostAmountCalcEdit);
			this.CostTotalsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.CostTotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CostTotalsPanel.Name = "CostTotalsPanel";
			this.CostTotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 164, true);
			this.CostTotalsPanel.TabIndex = 0;
			// 
			// CashAdvanceButton
			// 
			this.CashAdvanceButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0c3b2fd5-4136-4d64-a9d2-1b0fb99842ef", "Advance Payment");
			this.CashAdvanceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(607, 124, true);
			this.CashAdvanceButton.Name = "CashAdvanceButton";
			this.CashAdvanceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.CashAdvanceButton.TabIndex = 24;
			this.CashAdvanceButton.ToolTipCaption = null;
			this.CashAdvanceButton.UseVisualStyleBackColor = true;
			this.CashAdvanceButton.Click += new EventHandler(this.CashAdvanceButton_Click);
			// 
			// JR_CostReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.JR_CostReferenceTextBox, "FilteredCharges.JR_CostReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostReference)));
			this.JR_CostReferenceTextBox.CaptionResourceString = null;
			this.JR_CostReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 100, true);
			this.JR_CostReferenceTextBox.Name = "JR_CostReferenceTextBox";
			this.JR_CostReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JR_CostReferenceTextBox.TabIndex = 10;
			this.JR_CostReferenceTextBox.Tag = "";
			// 
			// CostRatingBehaviorEdit
			// 
			this.CostRatingBehaviorEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CostRatingBehaviorEdit, "FilteredCharges.JR_Calc_CostRatingBehavior");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_CostRatingBehavior)));
			this.CostRatingBehaviorEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|e9e3e224-e923-42dd-8b87-46e339c9023a", "Rating Behavior", "Cost Rating Behavior.");
			this.CostRatingBehaviorEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 100, true);
			this.CostRatingBehaviorEdit.Name = "CostRatingBehaviorEdit";
			this.CostRatingBehaviorEdit.PreBoundMaxLength = 2;
			this.CostRatingBehaviorEdit.ShowDescriptionBox = false;
			this.CostRatingBehaviorEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.CostRatingBehaviorEdit.TabIndex = 16;
			// 
			// costRatingOverrideCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.costRatingOverrideCommentTextBox, "FilteredCharges.JR_CostRatingOverrideComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostRatingOverrideComment)));
			this.costRatingOverrideCommentTextBox.CaptionResourceString = null;
			this.costRatingOverrideCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.costRatingOverrideCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 126, true);
			this.costRatingOverrideCommentTextBox.Name = "costRatingOverrideCommentTextBox";
			this.costRatingOverrideCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.costRatingOverrideCommentTextBox.TabIndex = 18;
			// 
			// costRatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.costRatedCheckBox, "FilteredCharges.JR_CostRated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostRated)));
			this.costRatedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.costRatedCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.costRatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 100, true);
			this.costRatedCheckBox.Name = "costRatedCheckBox";
			this.costRatedCheckBox.ReadOnly = true;
			this.costRatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.costRatedCheckBox.TabIndex = 17;
			// 
			// OSCostCurrencyCodeFindBox
			// 
			this.OSCostCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OSCostCurrencyCodeFindBox, "FilteredCharges.JR_RX_NKCostCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_RX_NKCostCurrency)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OSCostCurrencyCodeFindBox, false);
			this.OSCostCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 29, true);
			this.OSCostCurrencyCodeFindBox.Name = "OSCostCurrencyCodeFindBox";
			this.OSCostCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OSCostCurrencyCodeFindBox.ParentType = null;
			this.OSCostCurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.OSCostCurrencyCodeFindBox.ShowDescriptionBox = false;
			this.OSCostCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.OSCostCurrencyCodeFindBox.TabIndex = 5;
			// 
			// AutoAllocateZLabel
			// 
			this.AutoAllocateZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAllocateZLabel, "FilteredCharges.Calc_ChequeNumberIsAutoAllocatedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).Calc_ChequeNumberIsAutoAllocatedLabel)));
			this.AutoAllocateZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|a247ee35-8fb6-4b2a-9b3b-7b0820eb0c49", "Auto Allocate");
			this.AutoAllocateZLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AutoAllocateZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoAllocateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(682, 7, true);
			this.AutoAllocateZLabel.Name = "AutoAllocateZLabel";
			this.AutoAllocateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.AutoAllocateZLabel.TabIndex = 1;
			// 
			// OSEstimatedCostAmountCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.OSEstimatedCostAmountCurrencyTextBox, "FilteredCharges.JR_OSCostCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSCostCurrencyCode)));
			this.OSEstimatedCostAmountCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OSEstimatedCostAmountCurrencyTextBox, false);
			this.OSEstimatedCostAmountCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 53, true);
			this.OSEstimatedCostAmountCurrencyTextBox.Name = "OSEstimatedCostAmountCurrencyTextBox";
			this.OSEstimatedCostAmountCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.OSEstimatedCostAmountCurrencyTextBox.TabIndex = 7;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "FilteredCharges.JR_EstimatedCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_EstimatedCost)));
			this.zCalcEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|25b1e1c7-5d11-4536-8fc0-8965155656a3", "OS Est. Amt");
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 53, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit1.TabIndex = 6;
			this.zCalcEdit1.Text = "0.00";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AutoPopulateButton
			// 
			this.AutoPopulateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|1c4e82ee-a18e-4e48-bae7-1cea9a3873fc", "Populate AP Details");
			this.AutoPopulateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(607, 100, true);
			this.AutoPopulateButton.Name = "AutoPopulateButton";
			this.AutoPopulateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.AutoPopulateButton.TabIndex = 23;
			this.AutoPopulateButton.ToolTipCaption = null;
			this.AutoPopulateButton.UseVisualStyleBackColor = true;
			this.AutoPopulateButton.Click += new EventHandler(this.AutoPopulateButton_Click);
			// 
			// JR_APInvoiceDateBoundDateEdit
			// 
			this.JR_APInvoiceDateBoundDateEdit.AllowDrop = true;
			this.JR_APInvoiceDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JR_APInvoiceDateBoundDateEdit, "FilteredCharges.JR_APInvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_APInvoiceDate)));
			this.JR_APInvoiceDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|bb4ebb4c-ecbd-4177-a549-233bb14d9513", "Invoice Date");
			this.JR_APInvoiceDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 29, true);
			this.JR_APInvoiceDateBoundDateEdit.Name = "JR_APInvoiceDateBoundDateEdit";
			this.JR_APInvoiceDateBoundDateEdit.TabIndex = 13;
			// 
			// JR_APDocumentReceivedDateBoundDateEdit
			// 
			this.JR_APDocumentReceivedDateBoundDateEdit.AllowDrop = true;
			this.JR_APDocumentReceivedDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JR_APDocumentReceivedDateBoundDateEdit, "FilteredCharges.JR_APDocumentReceivedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_APDocumentReceivedDate)));
			this.JR_APDocumentReceivedDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|08C79488-E0FB-4A5D-88A0-8630B8C48F8F", "Doc Rec Date", "Document Received Date");
			this.JR_APDocumentReceivedDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 53, true);
			this.JR_APDocumentReceivedDateBoundDateEdit.Name = "JR_APDocumentReceivedDateBoundDateEdit";
			this.JR_APDocumentReceivedDateBoundDateEdit.TabIndex = 14;
			// 
			// JR_APInvoiceNumBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.JR_APInvoiceNumBoundTextEdit, "FilteredCharges.JR_APInvoiceNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_APInvoiceNum)));
			this.JR_APInvoiceNumBoundTextEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|2e4efa2c-0848-4dc7-bdff-6151e9caf20d", "Invoice No");
			this.JR_APInvoiceNumBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 5, true);
			this.JR_APInvoiceNumBoundTextEdit.Name = "JR_APInvoiceNumBoundTextEdit";
			this.JR_APInvoiceNumBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.JR_APInvoiceNumBoundTextEdit.TabIndex = 12;
			this.JR_APInvoiceNumBoundTextEdit.Tag = "";
			// 
			// CreditorsGuidFindBox
			// 
			this.CreditorsGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorsGuidFindBox, "FilteredCharges.JR_OH_CostAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OH_CostAccount)));
			this.CreditorsGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|6698c1c6-5188-4d3f-b692-ac3bcc463a64", "Creditor");
			this.CreditorsGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 126, true);
			this.CreditorsGuidFindBox.Name = "CreditorsGuidFindBox";
			this.CreditorsGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CreditorsGuidFindBox.ParentType = null;
			this.CreditorsGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CreditorsGuidFindBox.TabIndex = 11;
			// 
			// JR_PaymentDateBoundDateEdit
			// 
			this.JR_PaymentDateBoundDateEdit.AllowDrop = true;
			this.JR_PaymentDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JR_PaymentDateBoundDateEdit, "FilteredCharges.JR_PaymentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_PaymentDate)));
			this.JR_PaymentDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|b82ad7d8-ee00-4638-bb50-338f5fdb1cba", "Due Date");
			this.JR_PaymentDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 76, true);
			this.JR_PaymentDateBoundDateEdit.Name = "JR_PaymentDateBoundDateEdit";
			this.JR_PaymentDateBoundDateEdit.TabIndex = 15;
			// 
			// JR_PaymentTypeDropDownEdit
			// 
			this.JR_PaymentTypeDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JR_PaymentTypeDropDownEdit, "FilteredCharges.JR_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_PaymentType)));
			this.JR_PaymentTypeDropDownEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|e7ef5437-8925-4b72-b92c-1cd78617a0c5", "Pay Type", "Payment Type.");
			this.JR_PaymentTypeDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 5, true);
			this.JR_PaymentTypeDropDownEdit.Name = "JR_PaymentTypeDropDownEdit";
			this.JR_PaymentTypeDropDownEdit.PreBoundMaxLength = 3;
			this.JR_PaymentTypeDropDownEdit.ShowDescriptionBox = false;
			this.JR_PaymentTypeDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JR_PaymentTypeDropDownEdit.TabIndex = 19;
			// 
			// JR_ABBoundFindBox
			// 
			this.JR_ABBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JR_ABBoundFindBox, "FilteredCharges.JR_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AB)));
			this.JR_ABBoundFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|4770a2df-c7f2-4358-9d92-f7a757624a8d", "Bank Account");
			this.JR_ABBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 29, true);
			this.JR_ABBoundFindBox.Name = "JR_ABBoundFindBox";
			this.JR_ABBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JR_ABBoundFindBox.ParentType = null;
			this.JR_ABBoundFindBox.ShowDescriptionBox = false;
			this.JR_ABBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JR_ABBoundFindBox.TabIndex = 20;
			// 
			// ChequeBookFindBox
			// 
			this.ChequeBookFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChequeBookFindBox, "FilteredCharges.JR_AK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AK)));
			this.ChequeBookFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|e20b76a8-9a61-452a-9623-1af2a69c7d31", "Check Book");
			this.ChequeBookFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 53, true);
			this.ChequeBookFindBox.Name = "ChequeBookFindBox";
			this.ChequeBookFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ChequeBookFindBox.ParentType = null;
			this.ChequeBookFindBox.ShowDescriptionBox = false;
			this.ChequeBookFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ChequeBookFindBox.TabIndex = 21;
			// 
			// JR_ChequeNoBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.JR_ChequeNoBoundTextEdit, "FilteredCharges.JR_ChequeNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_ChequeNo)));
			this.JR_ChequeNoBoundTextEdit.CaptionResourceString = null;
			this.JR_ChequeNoBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 77, true);
			this.JR_ChequeNoBoundTextEdit.Name = "JR_ChequeNoBoundTextEdit";
			this.JR_ChequeNoBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JR_ChequeNoBoundTextEdit.TabIndex = 22;
			// 
			// CostPostedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CostPostedCheckBox, "FilteredCharges.JR_IsCostPosted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsCostPosted)));
			this.CostPostedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|b59bc20f-042e-4b69-8f57-41fdeace686d", "Posted");
			this.CostPostedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CostPostedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.CostPostedCheckBox.Name = "CostPostedCheckBox";
			this.CostPostedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
			this.CostPostedCheckBox.TabIndex = 0;
			// 
			// ApportionedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ApportionedCheckBox, "FilteredCharges.JR_IsApportioned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsApportioned)));
			this.ApportionedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|e03fd223-fb5c-43c7-b06e-7595fc72745c", "Apportioned");
			this.ApportionedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ApportionedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 3, true);
			this.ApportionedCheckBox.Name = "ApportionedCheckBox";
			this.ApportionedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
			this.ApportionedCheckBox.TabIndex = 1;
			// 
			// OSCostAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OSCostAmountCalcEdit, "FilteredCharges.JR_OSCostAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSCostAmt)));
			this.OSCostAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|e8eb131f-0936-4abc-9b91-4541815ce030", "OS Amount");
			this.OSCostAmountCalcEdit.DecimalPlaces = 2;
			this.OSCostAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 29, true);
			this.OSCostAmountCalcEdit.Name = "OSCostAmountCalcEdit";
			this.OSCostAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.OSCostAmountCalcEdit.TabIndex = 4;
			this.OSCostAmountCalcEdit.Text = "0.00";
			this.OSCostAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalCostAmountCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalCostAmountCurrencyTextBox, "FilteredCharges.JR_LocalCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LocalCurrencyCode)));
			this.LocalCostAmountCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.LocalCostAmountCurrencyTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalCostAmountCurrencyTextBox, false);
			this.LocalCostAmountCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 76, true);
			this.LocalCostAmountCurrencyTextBox.Name = "LocalCostAmountCurrencyTextBox";
			this.LocalCostAmountCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.LocalCostAmountCurrencyTextBox.TabIndex = 9;
			// 
			// LocalCostAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LocalCostAmountCalcEdit, "FilteredCharges.JR_LocalCostAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LocalCostAmt)));
			this.LocalCostAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|28a9c9ed-46c9-4644-803a-eb6b823674ae", "Local Amt");
			this.LocalCostAmountCalcEdit.DecimalPlaces = 2;
			this.LocalCostAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 76, true);
			this.LocalCostAmountCalcEdit.Name = "LocalCostAmountCalcEdit";
			this.LocalCostAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.LocalCostAmountCalcEdit.TabIndex = 8;
			this.LocalCostAmountCalcEdit.Text = "0.00";
			this.LocalCostAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CostTotalPanel
			// 
			this.CostTotalPanel.Controls.Add(this.CostExtraTaxPanel);
			this.CostTotalPanel.Controls.Add(this.LineTotalCostCurrencyTextBox);
			this.CostTotalPanel.Controls.Add(this.LineTotalCostAmountCalcEdit);
			this.CostTotalPanel.Controls.Add(this.TotalAmtOnInvoiceForJobCurrencyTextBox);
			this.CostTotalPanel.Controls.Add(this.TotalInvoiceAmountIncGSTCalcEdit);
			this.CostTotalPanel.Controls.Add(this.InvoiceGSTAmountCalcEdit);
			this.CostTotalPanel.Controls.Add(this.TotalTaxOnInvoiceForJobCurrencyTextBox);
			this.CostTotalPanel.Controls.Add(this.CostSupplyTypeDropEdit);
			this.CostTotalPanel.Controls.Add(this.CostWHTPanel);
			this.CostTotalPanel.Controls.Add(this.CostGSTPanel);
			this.CostTotalPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.CostTotalPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(772, 0, true);
			this.CostTotalPanel.Name = "CostTotalPanel";
			this.CostTotalPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 164, true);
			this.CostTotalPanel.TabIndex = 1;
			// 
			// CostExtraTaxPanel
			// 
			this.CostExtraTaxPanel.Controls.Add(this.ExtraTaxCurrencyTextBox);
			this.CostExtraTaxPanel.Controls.Add(this.ExtraTaxCostTaxAmountCalcEdit);
			this.CostExtraTaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 101, true);
			this.CostExtraTaxPanel.Name = "CostExtraTaxPanel";
			this.CostExtraTaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 24, true);
			this.CostExtraTaxPanel.TabIndex = 5;
			// 
			// ExtraTaxCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExtraTaxCurrencyTextBox, "FilteredCharges.CurrencyCodeOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CurrencyCodeOnInvForJob)));
			this.ExtraTaxCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExtraTaxCurrencyTextBox, false);
			this.ExtraTaxCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 2, true);
			this.ExtraTaxCurrencyTextBox.Name = "ExtraTaxCurrencyTextBox";
			this.ExtraTaxCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ExtraTaxCurrencyTextBox.TabIndex = 1;
			// 
			// ExtraTaxCostTaxAmountCalcEdit
			// 
			this.ExtraTaxCostTaxAmountCalcEdit.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.ExtraTaxCostTaxAmountCalcEdit, "FilteredCharges.LineExtraTaxAmountOnInvoiceForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).LineExtraTaxAmountOnInvoiceForJob)));
			this.ExtraTaxCostTaxAmountCalcEdit.CaptionResourceString = null;
			this.ExtraTaxCostTaxAmountCalcEdit.DecimalPlaces = 2;
			this.ExtraTaxCostTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 2, true);
			this.ExtraTaxCostTaxAmountCalcEdit.Name = "ExtraTaxCostTaxAmountCalcEdit";
			this.ExtraTaxCostTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.ExtraTaxCostTaxAmountCalcEdit.TabIndex = 0;
			this.ExtraTaxCostTaxAmountCalcEdit.Text = "0.00";
			this.ExtraTaxCostTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LineTotalCostCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCostCurrencyTextBox, "FilteredCharges.CurrencyCodeOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CurrencyCodeOnInvForJob)));
			this.LineTotalCostCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LineTotalCostCurrencyTextBox, false);
			this.LineTotalCostCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 5, true);
			this.LineTotalCostCurrencyTextBox.Name = "LineTotalCostCurrencyTextBox";
			this.LineTotalCostCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.LineTotalCostCurrencyTextBox.TabIndex = 1;
			// 
			// LineTotalCostAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineTotalCostAmountCalcEdit, "FilteredCharges.LineTotalAmountOnInvoiceForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).LineTotalAmountOnInvoiceForJob)));
			this.LineTotalCostAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|24a9f7be-3ae0-4552-9971-6c064420e137", "Line Total");
			this.LineTotalCostAmountCalcEdit.DecimalPlaces = 2;
			this.LineTotalCostAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 5, true);
			this.LineTotalCostAmountCalcEdit.Name = "LineTotalCostAmountCalcEdit";
			this.LineTotalCostAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.LineTotalCostAmountCalcEdit.TabIndex = 0;
			this.LineTotalCostAmountCalcEdit.Text = "0.00";
			this.LineTotalCostAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalAmtOnInvoiceForJobCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalAmtOnInvoiceForJobCurrencyTextBox, "FilteredCharges.CurrencyCodeOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CurrencyCodeOnInvForJob)));
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalAmtOnInvoiceForJobCurrencyTextBox, false);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 126, true);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Name = "TotalAmtOnInvoiceForJobCurrencyTextBox";
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.TotalAmtOnInvoiceForJobCurrencyTextBox.TabIndex = 7;
			// 
			// TotalInvoiceAmountIncGSTCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalInvoiceAmountIncGSTCalcEdit, "FilteredCharges.TotalAmountOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).TotalAmountOnInvForJob)));
			this.TotalInvoiceAmountIncGSTCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|384b480a-1e4b-4b55-ac35-bc2f6a38ac8d", "Invoice Total");
			this.TotalInvoiceAmountIncGSTCalcEdit.DecimalPlaces = 2;
			this.TotalInvoiceAmountIncGSTCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 126, true);
			this.TotalInvoiceAmountIncGSTCalcEdit.Name = "TotalInvoiceAmountIncGSTCalcEdit";
			this.TotalInvoiceAmountIncGSTCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TotalInvoiceAmountIncGSTCalcEdit.TabIndex = 6;
			this.TotalInvoiceAmountIncGSTCalcEdit.Text = "0.00";
			this.TotalInvoiceAmountIncGSTCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InvoiceGSTAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceGSTAmountCalcEdit, "FilteredCharges.TotalTaxOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).TotalTaxOnInvForJob)));
			this.InvoiceGSTAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|18f21f9e-07a4-429a-8d2c-a840616c0531", "Invoice Tax");
			this.InvoiceGSTAmountCalcEdit.DecimalPlaces = 2;
			this.InvoiceGSTAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 126, true);
			this.InvoiceGSTAmountCalcEdit.Name = "InvoiceGSTAmountCalcEdit";
			this.InvoiceGSTAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.InvoiceGSTAmountCalcEdit.TabIndex = 8;
			this.InvoiceGSTAmountCalcEdit.Text = "0.00";
			this.InvoiceGSTAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalTaxOnInvoiceForJobCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalTaxOnInvoiceForJobCurrencyTextBox, "FilteredCharges.CurrencyCodeOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CurrencyCodeOnInvForJob)));
			this.TotalTaxOnInvoiceForJobCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalTaxOnInvoiceForJobCurrencyTextBox, false);
			this.TotalTaxOnInvoiceForJobCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 126, true);
			this.TotalTaxOnInvoiceForJobCurrencyTextBox.Name = "TotalTaxOnInvoiceForJobCurrencyTextBox";
			this.TotalTaxOnInvoiceForJobCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.TotalTaxOnInvoiceForJobCurrencyTextBox.TabIndex = 9;
			// 
			// CostSupplyTypeDropEdit
			// 
			this.CostSupplyTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CostSupplyTypeDropEdit, "FilteredCharges.JR_CostSupplyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostSupplyType)));
			this.CostSupplyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 5, true);
			this.CostSupplyTypeDropEdit.Name = "CostSupplyTypeDropEdit";
			this.CostSupplyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CostSupplyTypeDropEdit.TabIndex = 3;
			// 
			// CostWHTPanel
			// 
			this.CostWHTPanel.Controls.Add(this.JR_AW_CostBoundFindBox);
			this.CostWHTPanel.Controls.Add(this.WHTTaxCurrencyTextBox);
			this.CostWHTPanel.Controls.Add(this.WHTCostTaxAmountCalcEdit);
			this.CostWHTPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
			this.CostWHTPanel.Name = "CostWHTPanel";
			this.CostWHTPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 24, true);
			this.CostWHTPanel.TabIndex = 4;
			// 
			// JR_AW_CostBoundFindBox
			// 
			this.JR_AW_CostBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JR_AW_CostBoundFindBox, "FilteredCharges.JR_AW_CostWHTRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AW_CostWHTRate)));
			this.JR_AW_CostBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 2, true);
			this.JR_AW_CostBoundFindBox.Name = "JR_AW_CostBoundFindBox";
			this.JR_AW_CostBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JR_AW_CostBoundFindBox.ParentType = null;
			this.JR_AW_CostBoundFindBox.ShowDescriptionBox = false;
			this.JR_AW_CostBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JR_AW_CostBoundFindBox.TabIndex = 3;
			// 
			// WHTTaxCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.WHTTaxCurrencyTextBox, "FilteredCharges.CurrencyCodeOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CurrencyCodeOnInvForJob)));
			this.WHTTaxCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WHTTaxCurrencyTextBox, false);
			this.WHTTaxCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 2, true);
			this.WHTTaxCurrencyTextBox.Name = "WHTTaxCurrencyTextBox";
			this.WHTTaxCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.WHTTaxCurrencyTextBox.TabIndex = 1;
			// 
			// WHTCostTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WHTCostTaxAmountCalcEdit, "FilteredCharges.LineWHTAmountOnInvoiceForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).LineWHTAmountOnInvoiceForJob)));
			this.WHTCostTaxAmountCalcEdit.CaptionResourceString = null;
			this.WHTCostTaxAmountCalcEdit.DecimalPlaces = 2;
			this.WHTCostTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 2, true);
			this.WHTCostTaxAmountCalcEdit.Name = "WHTCostTaxAmountCalcEdit";
			this.WHTCostTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.WHTCostTaxAmountCalcEdit.TabIndex = 0;
			this.WHTCostTaxAmountCalcEdit.Text = "0.00";
			this.WHTCostTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CostGSTPanel
			// 
			this.CostGSTPanel.Controls.Add(this.CostTaxBranchGuidFindBox);
			this.CostGSTPanel.Controls.Add(this.GSTTaxCurrencyTextBox);
			this.CostGSTPanel.Controls.Add(this.JR_AT_CostBoundFindBox);
			this.CostGSTPanel.Controls.Add(this.JR_CostTaxDateEdit);
			this.CostGSTPanel.Controls.Add(this.GSTCostTaxAmountCalcEdit);
			this.CostGSTPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.CostGSTPanel.Name = "CostGSTPanel";
			this.CostGSTPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 48, true);
			this.CostGSTPanel.TabIndex = 3;
			// 
			// CostTaxBranchGuidFindBox
			// 
			this.CostTaxBranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CostTaxBranchGuidFindBox, "FilteredCharges.JR_GB_CostTaxBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GB_CostTaxBranch)));
			this.CostTaxBranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("28ee187c-5b10-4dbf-a936-f82c17351e37", "Tax Branch");
			this.CostTaxBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 26, true);
			this.CostTaxBranchGuidFindBox.Name = "CostTaxBranchGuidFindBox";
			this.CostTaxBranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CostTaxBranchGuidFindBox.ParentType = null;
			this.CostTaxBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.CostTaxBranchGuidFindBox.TabIndex = 6;
			// 
			// GSTTaxCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.GSTTaxCurrencyTextBox, "FilteredCharges.CurrencyCodeOnInvForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CurrencyCodeOnInvForJob)));
			this.GSTTaxCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GSTTaxCurrencyTextBox, false);
			this.GSTTaxCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 3, true);
			this.GSTTaxCurrencyTextBox.Name = "GSTTaxCurrencyTextBox";
			this.GSTTaxCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.GSTTaxCurrencyTextBox.TabIndex = 1;
			// 
			// JR_AT_CostBoundFindBox
			// 
			this.JR_AT_CostBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JR_AT_CostBoundFindBox, "FilteredCharges.JR_AT_CostGSTRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AT_CostGSTRate)));
			this.JR_AT_CostBoundFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|ee995f92-87de-48a9-b106-afddc2a4ebe9", "Tax ID");
			this.JR_AT_CostBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 3, true);
			this.JR_AT_CostBoundFindBox.Name = "JR_AT_CostBoundFindBox";
			this.JR_AT_CostBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JR_AT_CostBoundFindBox.ParentType = null;
			this.JR_AT_CostBoundFindBox.ShowDescriptionBox = false;
			this.JR_AT_CostBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JR_AT_CostBoundFindBox.TabIndex = 4;
			// 
			// JR_CostTaxDateEdit
			// 
			this.JR_CostTaxDateEdit.AllowDrop = true;
			this.JR_CostTaxDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JR_CostTaxDateEdit, "FilteredCharges.JR_CostTaxDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostTaxDate)));
			this.JR_CostTaxDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|d64b36a6-f770-43ee-9a08-387cd30c148b", "Tax Date");
			this.JR_CostTaxDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 3, true);
			this.JR_CostTaxDateEdit.Name = "JR_CostTaxDateEdit";
			this.JR_CostTaxDateEdit.TabIndex = 5;
			// 
			// GSTCostTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GSTCostTaxAmountCalcEdit, "FilteredCharges.LineGSTAmountOnInvoiceForJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).LineGSTAmountOnInvoiceForJob)));
			this.GSTCostTaxAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|38c8e2a5-c08e-4572-a9c9-72601bacd13c", "Tax Amount");
			this.GSTCostTaxAmountCalcEdit.DecimalPlaces = 2;
			this.GSTCostTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 3, true);
			this.GSTCostTaxAmountCalcEdit.Name = "GSTCostTaxAmountCalcEdit";
			this.GSTCostTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.GSTCostTaxAmountCalcEdit.TabIndex = 0;
			this.GSTCostTaxAmountCalcEdit.Text = "0.00";
			this.GSTCostTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RevenueTabPage
			// 
			this.RevenueTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|0a4682b5-97fa-4649-ad4a-85d5d6dd5fce", "Revenue");
			this.RevenueTabPage.Controls.Add(this.panel3);
			this.RevenueTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RevenueTabPage.Name = "RevenueTabPage";
			this.RevenueTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 164, true);
			this.RevenueTabPage.TabIndex = 1;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.CashAdvancePanel);
			this.panel3.Controls.Add(this.SellTotalsPanel);
			this.panel3.Controls.Add(this.SellPanel);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panel3.Name = "panel3";
			this.panel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 164, true);
			this.panel3.TabIndex = 0;
			// 
			// CashAdvancePanel
			// 
			this.CashAdvancePanel.Controls.Add(this.cashAdvanceRequestStatusTextBox);
			this.CashAdvancePanel.Controls.Add(this.cashAdvanceRequestIDTextBox);
			this.CashAdvancePanel.Controls.Add(this.cashAdvanceRequiedCheckBox);
			this.CashAdvancePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CashAdvancePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1113, 0, true);
			this.CashAdvancePanel.Name = "CashAdvancePanel";
			this.CashAdvancePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 164, true);
			this.CashAdvancePanel.TabIndex = 2;
			// 
			// cashAdvanceRequestStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.cashAdvanceRequestStatusTextBox, "FilteredCharges.ARCashAdvanceRequestStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ARCashAdvanceRequestStatusDescription)));
			this.cashAdvanceRequestStatusTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e2944de9-b3e8-4e1f-8fc0-f94fbadc64e6", "Status");
			this.cashAdvanceRequestStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 29, true);
			this.cashAdvanceRequestStatusTextBox.Name = "cashAdvanceRequestStatusTextBox";
			this.cashAdvanceRequestStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.cashAdvanceRequestStatusTextBox.TabIndex = 17;
			// 
			// cashAdvanceRequestIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.cashAdvanceRequestIDTextBox, "FilteredCharges.ARCashAdvanceRequestID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ARCashAdvanceRequestID)));
			this.cashAdvanceRequestIDTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8a0ee03b-c404-4196-a6c0-46e8e59399bb", "Req. ID");
			this.cashAdvanceRequestIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 55, true);
			this.cashAdvanceRequestIDTextBox.Name = "cashAdvanceRequestIDTextBox";
			this.cashAdvanceRequestIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 20, true);
			this.cashAdvanceRequestIDTextBox.TabIndex = 16;
			// 
			// cashAdvanceRequiedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.cashAdvanceRequiedCheckBox, "FilteredCharges.JR_IsARCashAdvance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsARCashAdvance)));
			this.cashAdvanceRequiedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("526e535f-e914-4b53-a57f-5f1b2e3d2d7d", "Advance Payment Req.");
			this.cashAdvanceRequiedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.cashAdvanceRequiedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.cashAdvanceRequiedCheckBox.Name = "cashAdvanceRequiedCheckBox";
			this.cashAdvanceRequiedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.cashAdvanceRequiedCheckBox.TabIndex = 15;
			// 
			// SellTotalsPanel
			// 
			this.SellTotalsPanel.Controls.Add(this.SellExtraTaxPanel);
			this.SellTotalsPanel.Controls.Add(this.SellGSTPanel);
			this.SellTotalsPanel.Controls.Add(this.CFXAmtPanel);
			this.SellTotalsPanel.Controls.Add(this.LineTotalSellAmountCurrentTextBox);
			this.SellTotalsPanel.Controls.Add(this.LineTotalSellAmountCalcEdit);
			this.SellTotalsPanel.Controls.Add(this.SellWHTPanel);
			this.SellTotalsPanel.Controls.Add(this.InvoiceNumberTextBox);
			this.SellTotalsPanel.Controls.Add(this.SellSupplyTypeDropEdit);
			this.SellTotalsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.SellTotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 0, true);
			this.SellTotalsPanel.Name = "SellTotalsPanel";
			this.SellTotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 164, true);
			this.SellTotalsPanel.TabIndex = 1;
			// 
			// SellExtraTaxPanel
			// 
			this.SellExtraTaxPanel.Controls.Add(this.ExtraTaxSellTaxCurrencyTextBox);
			this.SellExtraTaxPanel.Controls.Add(this.ExtraTaxSellTaxAmountCalcEdit);
			this.SellExtraTaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 134, true);
			this.SellExtraTaxPanel.Name = "SellExtraTaxPanel";
			this.SellExtraTaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 24, true);
			this.SellExtraTaxPanel.TabIndex = 6;
			// 
			// ExtraTaxSellTaxCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExtraTaxSellTaxCurrencyTextBox, "FilteredCharges.JR_OSSellCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellCurrencyCode)));
			this.ExtraTaxSellTaxCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|8c80e45e-b9e2-48fd-b5b5-2e68eef35d28", "Sell Currency", "Currency of the WHT Tax.");
			this.ExtraTaxSellTaxCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 3, true);
			this.ExtraTaxSellTaxCurrencyTextBox.Name = "ExtraTaxSellTaxCurrencyTextBox";
			this.ExtraTaxSellTaxCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ExtraTaxSellTaxCurrencyTextBox.TabIndex = 1;
			// 
			// ExtraTaxSellTaxAmountCalcEdit
			// 
			this.ExtraTaxSellTaxAmountCalcEdit.AcceptsReturn = true;
			this.BindingSource.SetBindingMember(this.ExtraTaxSellTaxAmountCalcEdit, "FilteredCharges.JR_Calc_OSSellExtraTaxAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_OSSellExtraTaxAmt)));
			this.ExtraTaxSellTaxAmountCalcEdit.CaptionResourceString = null;
			this.ExtraTaxSellTaxAmountCalcEdit.DecimalPlaces = 2;
			this.ExtraTaxSellTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 3, true);
			this.ExtraTaxSellTaxAmountCalcEdit.Name = "ExtraTaxSellTaxAmountCalcEdit";
			this.ExtraTaxSellTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.ExtraTaxSellTaxAmountCalcEdit.TabIndex = 0;
			this.ExtraTaxSellTaxAmountCalcEdit.Text = "0.00";
			this.ExtraTaxSellTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SellGSTPanel
			// 
			this.SellGSTPanel.Controls.Add(this.SellTaxBranchGuidFindBox);
			this.SellGSTPanel.Controls.Add(this.SellGSTAmtCalcEdit);
			this.SellGSTPanel.Controls.Add(this.SellGSTAmountCurrencyTextBox);
			this.SellGSTPanel.Controls.Add(this.SellGSTRateGuidFindBox);
			this.SellGSTPanel.Controls.Add(this.SellTaxDateEdit);
			this.SellGSTPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 53, true);
			this.SellGSTPanel.Name = "SellGSTPanel";
			this.SellGSTPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 53, true);
			this.SellGSTPanel.TabIndex = 4;
			// 
			// SellTaxBranchGuidFindBox
			// 
			this.SellTaxBranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellTaxBranchGuidFindBox, "FilteredCharges.JR_GB_SellTaxBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GB_SellTaxBranch)));
			this.SellTaxBranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2ff120ec-22d5-4f24-b8e0-3e1e930f8a07", "Tax Branch");
			this.SellTaxBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 28, true);
			this.SellTaxBranchGuidFindBox.Name = "SellTaxBranchGuidFindBox";
			this.SellTaxBranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SellTaxBranchGuidFindBox.ParentType = null;
			this.SellTaxBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 20, true);
			this.SellTaxBranchGuidFindBox.TabIndex = 6;
			// 
			// SellGSTAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SellGSTAmtCalcEdit, "FilteredCharges.JR_OSSellGSTAmt_Calc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellGSTAmt_Calc)));
			this.SellGSTAmtCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|23ebfe86-f67a-40cb-bbc5-49588b09116c", "Tax Amount");
			this.SellGSTAmtCalcEdit.DecimalPlaces = 2;
			this.SellGSTAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 2, true);
			this.SellGSTAmtCalcEdit.Name = "SellGSTAmtCalcEdit";
			this.SellGSTAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.SellGSTAmtCalcEdit.TabIndex = 0;
			this.SellGSTAmtCalcEdit.Text = "0.00";
			this.SellGSTAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SellGSTAmountCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.SellGSTAmountCurrencyTextBox, "FilteredCharges.JR_OSSellCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellCurrencyCode)));
			this.SellGSTAmountCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|efe34c5e-d71c-438c-b13c-1378c2231f36", "Sell Currency", "Currency of the WHT Tax.");
			this.SellGSTAmountCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 2, true);
			this.SellGSTAmountCurrencyTextBox.Name = "SellGSTAmountCurrencyTextBox";
			this.SellGSTAmountCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.SellGSTAmountCurrencyTextBox.TabIndex = 1;
			// 
			// SellGSTRateGuidFindBox
			// 
			this.SellGSTRateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellGSTRateGuidFindBox, "FilteredCharges.JR_AT_SellGSTRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AT_SellGSTRate)));
			this.SellGSTRateGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|906aa2e6-e151-4509-adea-5999df237e77", "Tax ID");
			this.SellGSTRateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 2, true);
			this.SellGSTRateGuidFindBox.Name = "SellGSTRateGuidFindBox";
			this.SellGSTRateGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SellGSTRateGuidFindBox.ParentType = null;
			this.SellGSTRateGuidFindBox.ShowDescriptionBox = false;
			this.SellGSTRateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.SellGSTRateGuidFindBox.TabIndex = 4;
			// 
			// SellTaxDateEdit
			// 
			this.SellTaxDateEdit.AllowDrop = true;
			this.SellTaxDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.SellTaxDateEdit, "FilteredCharges.JR_SellTaxDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellTaxDate)));
			this.SellTaxDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|b410ce8f-b4a8-4470-b794-d9157f61c3ac", "Tax Date");
			this.SellTaxDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 2, true);
			this.SellTaxDateEdit.Name = "SellTaxDateEdit";
			this.SellTaxDateEdit.TabIndex = 5;
			// 
			// CFXAmtPanel
			// 
			this.CFXAmtPanel.Controls.Add(this.CFXAmtCurrencyTextBox);
			this.CFXAmtPanel.Controls.Add(this.CFXAmtCalcEdit);
			this.CFXAmtPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 134, true);
			this.CFXAmtPanel.Name = "CFXAmtPanel";
			this.CFXAmtPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 24, true);
			this.CFXAmtPanel.TabIndex = 7;
			// 
			// CFXAmtCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CFXAmtCurrencyTextBox, "FilteredCharges.JR_LocalCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LocalCurrencyCode)));
			this.CFXAmtCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|492782f9-682c-4066-8dbd-144d2291e23d", "", "Local Currency");
			this.CFXAmtCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 2, true);
			this.CFXAmtCurrencyTextBox.Name = "CFXAmtCurrencyTextBox";
			this.CFXAmtCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.CFXAmtCurrencyTextBox.TabIndex = 1;
			// 
			// CFXAmtCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CFXAmtCalcEdit, "FilteredCharges.JR_CFXAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CFXAmt)));
			this.CFXAmtCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|21eb5694-458f-4a03-98e8-db2495bf3ab8", "CFX");
			this.CFXAmtCalcEdit.DecimalPlaces = 2;
			this.CFXAmtCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 2, true);
			this.CFXAmtCalcEdit.Name = "CFXAmtCalcEdit";
			this.CFXAmtCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.CFXAmtCalcEdit.TabIndex = 0;
			this.CFXAmtCalcEdit.Text = "0.00";
			this.CFXAmtCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LineTotalSellAmountCurrentTextBox
			// 
			this.BindingSource.SetBindingMember(this.LineTotalSellAmountCurrentTextBox, "FilteredCharges.JR_OSSellCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellCurrencyCode)));
			this.LineTotalSellAmountCurrentTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|3482b4a7-a0f1-444a-806d-aff74b4384cc", "Sell Currency", "Currency of the WHT Tax.");
			this.LineTotalSellAmountCurrentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 29, true);
			this.LineTotalSellAmountCurrentTextBox.Name = "LineTotalSellAmountCurrentTextBox";
			this.LineTotalSellAmountCurrentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.LineTotalSellAmountCurrentTextBox.TabIndex = 3;
			// 
			// LineTotalSellAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineTotalSellAmountCalcEdit, "FilteredCharges.JR_Calc_OSSellAmtWithGST");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_OSSellAmtWithGST)));
			this.LineTotalSellAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|60533077-a802-4f85-b61f-95a384767fb4", "Line Total");
			this.LineTotalSellAmountCalcEdit.DecimalPlaces = 2;
			this.LineTotalSellAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 29, true);
			this.LineTotalSellAmountCalcEdit.Name = "LineTotalSellAmountCalcEdit";
			this.LineTotalSellAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.LineTotalSellAmountCalcEdit.TabIndex = 1;
			this.LineTotalSellAmountCalcEdit.Text = "0.00";
			this.LineTotalSellAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SellWHTPanel
			// 
			this.SellWHTPanel.Controls.Add(this.SellWHTRateGuidFindBox);
			this.SellWHTPanel.Controls.Add(this.WHTSellTaxCurrencyTextBox);
			this.SellWHTPanel.Controls.Add(this.WHTSellTaxAmountCalcEdit);
			this.SellWHTPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 107, true);
			this.SellWHTPanel.Name = "SellWHTPanel";
			this.SellWHTPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 24, true);
			this.SellWHTPanel.TabIndex = 5;
			// 
			// SellWHTRateGuidFindBox
			// 
			this.SellWHTRateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellWHTRateGuidFindBox, "FilteredCharges.JR_AW_SellWHTRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AW_SellWHTRate)));
			this.SellWHTRateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 2, true);
			this.SellWHTRateGuidFindBox.Name = "SellWHTRateGuidFindBox";
			this.SellWHTRateGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SellWHTRateGuidFindBox.ParentType = null;
			this.SellWHTRateGuidFindBox.ShowDescriptionBox = false;
			this.SellWHTRateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.SellWHTRateGuidFindBox.TabIndex = 3;
			// 
			// WHTSellTaxCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.WHTSellTaxCurrencyTextBox, "FilteredCharges.JR_OSSellCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellCurrencyCode)));
			this.WHTSellTaxCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|35bc8e61-b105-4ca4-af2d-0e2002f61cd7", "Sell Currency", "Currency of the WHT Tax.");
			this.WHTSellTaxCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 1, true);
			this.WHTSellTaxCurrencyTextBox.Name = "WHTSellTaxCurrencyTextBox";
			this.WHTSellTaxCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.WHTSellTaxCurrencyTextBox.TabIndex = 1;
			// 
			// WHTSellTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WHTSellTaxAmountCalcEdit, "FilteredCharges.JR_OSSellWHTAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellWHTAmt)));
			this.WHTSellTaxAmountCalcEdit.CaptionResourceString = null;
			this.WHTSellTaxAmountCalcEdit.DecimalPlaces = 2;
			this.WHTSellTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 1, true);
			this.WHTSellTaxAmountCalcEdit.Name = "WHTSellTaxAmountCalcEdit";
			this.WHTSellTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.WHTSellTaxAmountCalcEdit.TabIndex = 0;
			this.WHTSellTaxAmountCalcEdit.Text = "0.00";
			this.WHTSellTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InvoiceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceNumberTextBox, "FilteredCharges.JR_ARInvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_ARInvoiceNumber)));
			this.InvoiceNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|088c0d7a-6912-44b5-af00-f9934022996f", "Invoice");
			this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 5, true);
			this.InvoiceNumberTextBox.Name = "InvoiceNumberTextBox";
			this.InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.InvoiceNumberTextBox.TabIndex = 0;
			// 
			// SellSupplyTypeDropEdit
			// 
			this.SellSupplyTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellSupplyTypeDropEdit, "FilteredCharges.JR_SellSupplyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellSupplyType)));
			this.SellSupplyTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 29, true);
			this.SellSupplyTypeDropEdit.Name = "SellSupplyTypeDropEdit";
			this.SellSupplyTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.SellSupplyTypeDropEdit.TabIndex = 3;
			// 
			// SellPanel
			// 
			this.SellPanel.Controls.Add(this.OSSellAmountCurrencyCodeFindBox);
			this.SellPanel.Controls.Add(this.OSSellEstimatedAmountCurrency);
			this.SellPanel.Controls.Add(this.OSSellEstimatedAmountCalcEdit);
			this.SellPanel.Controls.Add(this.SellRatingBehaviorEdit);
			this.SellPanel.Controls.Add(this.SellRatingOverrideCommentTextBox);
			this.SellPanel.Controls.Add(this.sellRatedCheckBox);
			this.SellPanel.Controls.Add(this.CFXCalcEdit);
			this.SellPanel.Controls.Add(this.PreventInvoicePrintGroupingCheckBox);
			this.SellPanel.Controls.Add(this.InvoiceTypeDropEdit);
			this.SellPanel.Controls.Add(this.OSSellAmountCalcEdit);
			this.SellPanel.Controls.Add(this.LocalSellAmountCurrency);
			this.SellPanel.Controls.Add(this.LocalSellAmountCalcEdit);
			this.SellPanel.Controls.Add(this.DebtorFindBox);
			this.SellPanel.Controls.Add(this.RevenuePostedCheckBox);
			this.SellPanel.Controls.Add(this.hasDebtorAcceptedThisSellCharge);
			this.SellPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.SellPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SellPanel.Name = "SellPanel";
			this.SellPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 164, true);
			this.SellPanel.TabIndex = 0;
			// 
			// OSSellAmountCurrencyCodeFindBox
			// 
			this.OSSellAmountCurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OSSellAmountCurrencyCodeFindBox, "FilteredCharges.JR_RX_NKSellCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_RX_NKSellCurrency)));
			this.OSSellAmountCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 25, true);
			this.OSSellAmountCurrencyCodeFindBox.Name = "OSSellAmountCurrencyCodeFindBox";
			this.OSSellAmountCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OSSellAmountCurrencyCodeFindBox.ParentType = null;
			this.OSSellAmountCurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.OSSellAmountCurrencyCodeFindBox.ShowDescriptionBox = false;
			this.OSSellAmountCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.OSSellAmountCurrencyCodeFindBox.TabIndex = 3;
			// 
			// OSSellEstimatedAmountCurrency
			// 
			this.BindingSource.SetBindingMember(this.OSSellEstimatedAmountCurrency, "FilteredCharges.JR_OSSellCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellCurrencyCode)));
			this.OSSellEstimatedAmountCurrency.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|be1bcf95-8f72-4835-a1d9-40d37931c538", "Sell Currency", "Currency of the WHT Tax.");
			this.OSSellEstimatedAmountCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 48, true);
			this.OSSellEstimatedAmountCurrency.Name = "OSSellEstimatedAmountCurrency";
			this.OSSellEstimatedAmountCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.OSSellEstimatedAmountCurrency.TabIndex = 5;
			// 
			// OSSellEstimatedAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OSSellEstimatedAmountCalcEdit, "FilteredCharges.JR_EstimatedRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_EstimatedRevenue)));
			this.OSSellEstimatedAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|922b4217-1b52-4a0e-8726-895d118439aa", "OS Est.Amt", "OS Est. Amt.");
			this.OSSellEstimatedAmountCalcEdit.DecimalPlaces = 2;
			this.OSSellEstimatedAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 48, true);
			this.OSSellEstimatedAmountCalcEdit.Name = "OSSellEstimatedAmountCalcEdit";
			this.OSSellEstimatedAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.OSSellEstimatedAmountCalcEdit.TabIndex = 4;
			this.OSSellEstimatedAmountCalcEdit.Text = "0.00";
			this.OSSellEstimatedAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SellRatingBehaviorEdit
			// 
			this.SellRatingBehaviorEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellRatingBehaviorEdit, "FilteredCharges.JR_Calc_SellRatingBehavior");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_SellRatingBehavior)));
			this.SellRatingBehaviorEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|d0118864-d9ac-4fd2-8c2e-9e9a792500d5", "Rating Behavior", "Sell Rating Behavior.");
			this.SellRatingBehaviorEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 48, true);
			this.SellRatingBehaviorEdit.Name = "SellRatingBehaviorEdit";
			this.SellRatingBehaviorEdit.PreBoundMaxLength = 3;
			this.SellRatingBehaviorEdit.ShowDescriptionBox = false;
			this.SellRatingBehaviorEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.SellRatingBehaviorEdit.TabIndex = 11;
			// 
			// SellRatingOverrideCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.SellRatingOverrideCommentTextBox, "FilteredCharges.JR_SellRatingOverrideComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellRatingOverrideComment)));
			this.SellRatingOverrideCommentTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|b9136d95-b060-4b8f-99fb-633e727a214b", "Override Comment");
			this.SellRatingOverrideCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SellRatingOverrideCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 72, true);
			this.SellRatingOverrideCommentTextBox.Name = "SellRatingOverrideCommentTextBox";
			this.SellRatingOverrideCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 20, true);
			this.SellRatingOverrideCommentTextBox.TabIndex = 13;
			// 
			// sellRatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.sellRatedCheckBox, "FilteredCharges.JR_SellRated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellRated)));
			this.sellRatedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.sellRatedCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.sellRatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 96, true);
			this.sellRatedCheckBox.Name = "sellRatedCheckBox";
			this.sellRatedCheckBox.ReadOnly = true;
			this.sellRatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.sellRatedCheckBox.TabIndex = 15;
			// 
			// CFXCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CFXCalcEdit, "FilteredCharges.JR_LineCFX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LineCFX)));
			this.CFXCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|e66e8df9-b1c7-4453-b04e-9610636b2261", "CFX %");
			this.CFXCalcEdit.DecimalPlaces = 2;
			this.CFXCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 48, true);
			this.CFXCalcEdit.Name = "CFXCalcEdit";
			this.CFXCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.CFXCalcEdit.TabIndex = 12;
			this.CFXCalcEdit.Text = "0.00";
			this.CFXCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PreventInvoicePrintGroupingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PreventInvoicePrintGroupingCheckBox, "FilteredCharges.JR_PreventInvoicePrintGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_PreventInvoicePrintGrouping)));
			this.PreventInvoicePrintGroupingCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|a989fd1f-8118-469e-8ec6-bf811222cf68", "Prevent Invoice Print");
			this.PreventInvoicePrintGroupingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PreventInvoicePrintGroupingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 4, true);
			this.PreventInvoicePrintGroupingCheckBox.Name = "PreventInvoicePrintGroupingCheckBox";
			this.PreventInvoicePrintGroupingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 16, true);
			this.PreventInvoicePrintGroupingCheckBox.TabIndex = 9;
			// 
			// InvoiceTypeDropEdit
			// 
			this.InvoiceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceTypeDropEdit, "FilteredCharges.JR_InvoiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_InvoiceType)));
			this.InvoiceTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|9f2d9bc1-cb18-4f7c-a741-06a9d974230e", "Invoice Type");
			this.InvoiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(402, 25, true);
			this.InvoiceTypeDropEdit.Name = "InvoiceTypeDropEdit";
			this.InvoiceTypeDropEdit.PreBoundMaxLength = 3;
			this.InvoiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.InvoiceTypeDropEdit.TabIndex = 10;
			// 
			// OSSellAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OSSellAmountCalcEdit, "FilteredCharges.JR_OSSellAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellAmt)));
			this.OSSellAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|f5e796d2-be5c-4335-816c-8be86075fb4d", "OS Amount");
			this.OSSellAmountCalcEdit.DecimalPlaces = 2;
			this.OSSellAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 25, true);
			this.OSSellAmountCalcEdit.Name = "OSSellAmountCalcEdit";
			this.OSSellAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.OSSellAmountCalcEdit.TabIndex = 1;
			this.OSSellAmountCalcEdit.Text = "0.00";
			this.OSSellAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LocalSellAmountCurrency
			// 
			this.BindingSource.SetBindingMember(this.LocalSellAmountCurrency, "FilteredCharges.JR_LocalCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LocalCurrencyCode)));
			this.LocalSellAmountCurrency.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|c59cdc8d-71a7-41b9-9013-f27ae3aea68c", "", "Local Currency");
			this.LocalSellAmountCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 72, true);
			this.LocalSellAmountCurrency.Name = "LocalSellAmountCurrency";
			this.LocalSellAmountCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.LocalSellAmountCurrency.TabIndex = 7;
			// 
			// LocalSellAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LocalSellAmountCalcEdit, "FilteredCharges.JR_LocalSellAmt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LocalSellAmt)));
			this.LocalSellAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|fccc5a50-02b0-415b-88a1-5488851dc24c", "Local Amt");
			this.LocalSellAmountCalcEdit.DecimalPlaces = 2;
			this.LocalSellAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 72, true);
			this.LocalSellAmountCalcEdit.Name = "LocalSellAmountCalcEdit";
			this.LocalSellAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.LocalSellAmountCalcEdit.TabIndex = 6;
			this.LocalSellAmountCalcEdit.Text = "0.00";
			this.LocalSellAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DebtorFindBox
			// 
			this.DebtorFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DebtorFindBox, "FilteredCharges.JR_OH_SellAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OH_SellAccount)));
			this.DebtorFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|86bb96d2-02fe-4b88-8d53-fb457fcc0ced", "Debtor");
			this.DebtorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 96, true);
			this.DebtorFindBox.Name = "DebtorFindBox";
			this.DebtorFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DebtorFindBox.ParentType = null;
			this.DebtorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.DebtorFindBox.TabIndex = 8;
			// 
			// RevenuePostedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RevenuePostedCheckBox, "FilteredCharges.JR_IsRevenuePosted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsRevenuePosted)));
			this.RevenuePostedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|0781c7de-89e6-46af-96f5-78e924c9c4f1", "Posted");
			this.RevenuePostedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RevenuePostedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.RevenuePostedCheckBox.Name = "RevenuePostedCheckBox";
			this.RevenuePostedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 16, true);
			this.RevenuePostedCheckBox.TabIndex = 0;
			// 
			// hasDebtorAcceptedThisSellCharge
			// 
			this.BindingSource.SetBindingMember(this.hasDebtorAcceptedThisSellCharge, "FilteredCharges.JR_Calc_HasDebtorAcceptedThisSellCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_HasDebtorAcceptedThisSellCharge)));
			this.hasDebtorAcceptedThisSellCharge.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|748da067-07ef-4486-94f8-376d57609a13", "Accepted by Debtor", "Debtor has accepted this Group Company Cost");
			this.hasDebtorAcceptedThisSellCharge.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.hasDebtorAcceptedThisSellCharge.ForeColor = System.Drawing.SystemColors.GrayText;
			this.hasDebtorAcceptedThisSellCharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 96, true);
			this.hasDebtorAcceptedThisSellCharge.Name = "hasDebtorAcceptedThisSellCharge";
			this.hasDebtorAcceptedThisSellCharge.ReadOnly = true;
			this.hasDebtorAcceptedThisSellCharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 19, true);
			this.hasDebtorAcceptedThisSellCharge.TabIndex = 16;
			// 
			// AutoratingCostTabPage
			// 
			this.AutoratingCostTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|512b2c00-64cf-4ab1-a259-05a060e71aaf", "Cost Rate Audit");
			this.AutoratingCostTabPage.Controls.Add(this.CostRateCalculationXMLUserControl);
			this.AutoratingCostTabPage.Controls.Add(this.CostPaymentBasisUserControl);
			this.AutoratingCostTabPage.Controls.Add(this.AutoRateNotePopupButton2);
			this.AutoratingCostTabPage.Controls.Add(this.AutoRateDescCostTextBox);
			this.AutoratingCostTabPage.Controls.Add(this.WiseRatesRawDataUserControl);
			this.AutoratingCostTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AutoratingCostTabPage.Name = "AutoratingCostTabPage";
			this.AutoratingCostTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 164, true);
			this.AutoratingCostTabPage.TabIndex = 4;
			// 
			// CostRateCalculationXMLUserControl
			// 
			this.CostRateCalculationXMLUserControl.AllowDrop = true;
			this.CostRateCalculationXMLUserControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0663576e-da73-4f47-b6c3-9b32847d95b8", "Calculation XML");
			this.CostRateCalculationXMLUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(887, 35, true);
			this.CostRateCalculationXMLUserControl.Name = "CostRateCalculationXMLUserControl";
			this.CostRateCalculationXMLUserControl.ShowRevenue = false;
			this.CostRateCalculationXMLUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 24, true);
			this.CostRateCalculationXMLUserControl.TabIndex = 3;
			// 
			// CostPaymentBasisUserControl
			// 
			this.CostPaymentBasisUserControl.AllowDrop = true;
			this.CostPaymentBasisUserControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e424b79e-3dcc-4ca3-bf6c-1e85a1bc5e35", "Itemized Cost");
			this.CostPaymentBasisUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(887, 60, true);
			this.CostPaymentBasisUserControl.Name = "CostPaymentBasisUserControl";
			this.CostPaymentBasisUserControl.ShowRevenue = false;
			this.CostPaymentBasisUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 24, true);
			this.CostPaymentBasisUserControl.TabIndex = 4;
			// 
			// AutoRateNotePopupButton2
			// 
			this.AutoRateNotePopupButton2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|52b60a97-6934-484a-99de-6b7ced964d48", "Autorating Log");
			this.AutoRateNotePopupButton2.CreateNewNoteIfNotFound = false;
			this.AutoRateNotePopupButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(887, 8, true);
			this.AutoRateNotePopupButton2.Name = "AutoRateNotePopupButton2";
			this.AutoRateNotePopupButton2.NoteType = "AutoRating Log";
			this.AutoRateNotePopupButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 24, true);
			this.AutoRateNotePopupButton2.TabIndex = 1;
			this.AutoRateNotePopupButton2.ToolTipCaption = null;
			// 
			// AutoRateDescCostTextBox
			// 
			this.BindingSource.SetBindingMember(this.AutoRateDescCostTextBox, "FilteredCharges.CostCalculationDescriptionString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBlob)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CostCalculationDescription)));
			this.AutoRateDescCostTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.AutoRateDescCostTextBox.MaxLength = 10000000;
			this.AutoRateDescCostTextBox.Name = "AutoRateDescCostTextBox";
			this.AutoRateDescCostTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 112, true);
			this.AutoRateDescCostTextBox.TabIndex = 0;
			this.AutoRateDescCostTextBox.Font = new Font("Lucida Console", 10);
			this.AutoRateDescCostTextBox.CharacterCasing = CharacterCasing.Normal;
			this.AutoRateDescCostTextBox.Multiline = true;
			this.AutoRateDescCostTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			// 
			// WiseRatesRawDataUserControl
			// 
			this.WiseRatesRawDataUserControl.AllowDrop = true;
			this.WiseRatesRawDataUserControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("C09B806A-F6EC-4363-A13A-28681674768B", "Raw Data");
			this.WiseRatesRawDataUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(887, 85, true);
			this.WiseRatesRawDataUserControl.Name = "WiseRatesRawDataUserControl";
			this.WiseRatesRawDataUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 24, true);
			this.WiseRatesRawDataUserControl.TabIndex = 5;
			// 
			// AutoratingSellTabPage
			// 
			this.AutoratingSellTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|993e9e06-5774-4af3-9bc9-f2d3ab458017", "Revenue Rate Audit");
			this.AutoratingSellTabPage.Controls.Add(this.RevenueRateCalculationXMLUserControl);
			this.AutoratingSellTabPage.Controls.Add(this.RevenuePaymentBasisUserControl);
			this.AutoratingSellTabPage.Controls.Add(this.AutoRateDescRevenueTextBox);
			this.AutoratingSellTabPage.Controls.Add(this.AutoRateNotePopupButton);
			this.AutoratingSellTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AutoratingSellTabPage.Name = "AutoratingSellTabPage";
			this.AutoratingSellTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 164, true);
			this.AutoratingSellTabPage.TabIndex = 3;
			// 
			// RevenueRateCalculationXMLUserControl
			// 
			this.RevenueRateCalculationXMLUserControl.AllowDrop = true;
			this.RevenueRateCalculationXMLUserControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b22d5646-e1e8-46be-82ea-2c4e2c66b35d", "Calculation XML");
			this.RevenueRateCalculationXMLUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(889, 36, true);
			this.RevenueRateCalculationXMLUserControl.Name = "RevenueRateCalculationXMLUserControl";
			this.RevenueRateCalculationXMLUserControl.ShowRevenue = true;
			this.RevenueRateCalculationXMLUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 24, true);
			this.RevenueRateCalculationXMLUserControl.TabIndex = 4;
			// 
			// RevenuePaymentBasisUserControl
			// 
			this.RevenuePaymentBasisUserControl.AllowDrop = true;
			this.RevenuePaymentBasisUserControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("59e8e57b-b357-4778-8c96-837b824826f4", "Itemized Sell");
			this.RevenuePaymentBasisUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(889, 61, true);
			this.RevenuePaymentBasisUserControl.Name = "RevenuePaymentBasisUserControl";
			this.RevenuePaymentBasisUserControl.ShowRevenue = true;
			this.RevenuePaymentBasisUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 24, true);
			this.RevenuePaymentBasisUserControl.TabIndex = 5;
			// 
			// AutoRateDescRevenueTextBox
			// 
			this.BindingSource.SetBindingMember(this.AutoRateDescRevenueTextBox, "FilteredCharges.RevenueCalculationDescriptionString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBlob)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).RevenueCalculationDescription)));
			this.AutoRateDescRevenueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.AutoRateDescRevenueTextBox.MaxLength = 10000000;
			this.AutoRateDescRevenueTextBox.Name = "AutoRateDescRevenueTextBox";
			this.AutoRateDescRevenueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 112, true);
			this.AutoRateDescRevenueTextBox.TabIndex = 0;
			this.AutoRateDescRevenueTextBox.Font = new Font("Lucida Console", 10);
			this.AutoRateDescRevenueTextBox.CharacterCasing = CharacterCasing.Normal;
			this.AutoRateDescRevenueTextBox.Multiline = true;
			this.AutoRateDescRevenueTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			// 
			// AutoRateNotePopupButton
			// 
			this.AutoRateNotePopupButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|41ae6985-0d5e-4ade-8757-d284cd2be343", "Autorating Log");
			this.AutoRateNotePopupButton.CreateNewNoteIfNotFound = false;
			this.AutoRateNotePopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(889, 8, true);
			this.AutoRateNotePopupButton.Name = "AutoRateNotePopupButton";
			this.AutoRateNotePopupButton.NoteType = "AutoRating Log";
			this.AutoRateNotePopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 24, true);
			this.AutoRateNotePopupButton.TabIndex = 1;
			this.AutoRateNotePopupButton.ToolTipCaption = null;
			// 
			// TaxTransactionTabPage
			// 
			this.TaxTransactionTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("F173CCFB-42CF-4FAD-909D-31C23765A729", "Tax Transaction Audit");
			this.TaxTransactionTabPage.Controls.Add(this.TaxTransactionControl);
			this.TaxTransactionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TaxTransactionTabPage.Name = "TaxTransactionTabPage";
			this.TaxTransactionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 164, true);
			this.TaxTransactionTabPage.TabIndex = 3;
			// 
			// TaxTransactionControl
			// 
			this.TaxTransactionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxTransactionControl, "FilteredCharges.TaxTransactionsLinkedToJobChargeForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TaxFramework.Business.TaxTransactionsLinkedToJobChargeForDisplay)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).TaxTransactionsLinkedToJobChargeForDisplay)));
			this.TaxTransactionControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxTransactionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxTransactionControl.Name = "TaxTransactionControl";
			this.TaxTransactionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 164, true);
			this.TaxTransactionControl.TabIndex = 0;
			// 
			// JH_OH_LocalChargesBoundOrgCard
			// 
			this.JH_OH_LocalChargesBoundOrgCard.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JH_OH_LocalChargesBoundOrgCard, "LocalZAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZAddressWithContact)(((Job)(null)).LocalZAddressWithContact)));
			this.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString = null;
			this.JH_OH_LocalChargesBoundOrgCard.ContactInfoTabVisible = true;
			this.JH_OH_LocalChargesBoundOrgCard.InvoiceContactTabCaption = Enterprise.Accounting.GUI.Res.GetData("ZAddressWithContactControl|79ad8d2f-19b7-4049-a41c-3a36c594be57", "Invoice Contact");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JH_OH_LocalChargesBoundOrgCard, false);
			this.JH_OH_LocalChargesBoundOrgCard.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.JH_OH_LocalChargesBoundOrgCard.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.JH_OH_LocalChargesBoundOrgCard.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.JH_OH_LocalChargesBoundOrgCard.Name = "JH_OH_LocalChargesBoundOrgCard";
			this.JH_OH_LocalChargesBoundOrgCard.PopupCaption = "";
			this.JH_OH_LocalChargesBoundOrgCard.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.JH_OH_LocalChargesBoundOrgCard.TabIndex = 0;
			// 
			// TotalCostAmountCalcEdit
			// 
			this.TotalCostAmountCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalCostAmountCalcEdit, "JH_TotalCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Job)(null)).JH_TotalCost)));
			this.TotalCostAmountCalcEdit.BindToDecimalPlaces = "JH_LocalCurrencyDecimals";
			this.TotalCostAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|5b37b53f-3269-4ee3-b508-457db996b957", "Cost", "Total Cost.");
			this.TotalCostAmountCalcEdit.DecimalPlaces = 2;
			this.TotalCostAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 0, true);
			this.TotalCostAmountCalcEdit.Name = "TotalCostAmountCalcEdit";
			this.TotalCostAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.TotalCostAmountCalcEdit.TabIndex = 9;
			this.TotalCostAmountCalcEdit.Text = "0.00";
			this.TotalCostAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalAgentAmountCalcEdit
			// 
			this.TotalAgentAmountCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalAgentAmountCalcEdit, "JH_TotalRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Job)(null)).JH_TotalRevenue)));
			this.TotalAgentAmountCalcEdit.BindToDecimalPlaces = "JH_LocalCurrencyDecimals";
			this.TotalAgentAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|20cc137d-e7b9-4e92-a1d3-43f826d78899", "Revenue", "Total Revenue.");
			this.TotalAgentAmountCalcEdit.DecimalPlaces = 2;
			this.TotalAgentAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 0, true);
			this.TotalAgentAmountCalcEdit.Name = "TotalAgentAmountCalcEdit";
			this.TotalAgentAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.TotalAgentAmountCalcEdit.TabIndex = 12;
			this.TotalAgentAmountCalcEdit.Text = "0.00";
			this.TotalAgentAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProfitLossCurrencytTextBox
			// 
			this.ProfitLossCurrencytTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProfitLossCurrencytTextBox, "JH_LocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_LocalCurrency)));
			this.ProfitLossCurrencytTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ProfitLossCurrencytTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ProfitLossCurrencytTextBox, false);
			this.ProfitLossCurrencytTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 0, true);
			this.ProfitLossCurrencytTextBox.Name = "ProfitLossCurrencytTextBox";
			this.ProfitLossCurrencytTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.ProfitLossCurrencytTextBox.TabIndex = 17;
			// 
			// ProfitLossAmountCalcEdit
			// 
			this.ProfitLossAmountCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProfitLossAmountCalcEdit, "JH_ProfitLoss");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Job)(null)).JH_ProfitLoss)));
			this.ProfitLossAmountCalcEdit.BindToDecimalPlaces = "JH_LocalCurrencyDecimals";
			this.ProfitLossAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|1dc3b388-1e19-43d8-a128-7097e3f059b0", "Profit", "Total Profit.");
			this.ProfitLossAmountCalcEdit.DecimalPlaces = 2;
			this.ProfitLossAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 0, true);
			this.ProfitLossAmountCalcEdit.Name = "ProfitLossAmountCalcEdit";
			this.ProfitLossAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.ProfitLossAmountCalcEdit.TabIndex = 16;
			this.ProfitLossAmountCalcEdit.Text = "0.00";
			this.ProfitLossAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverseasAgentPanel
			// 
			this.OverseasAgentPanel.Controls.Add(this.JH_OH_AgentCollectBoundOrgCard);
			this.OverseasAgentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 0, true);
			this.OverseasAgentPanel.Name = "OverseasAgentPanel";
			this.OverseasAgentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 162, true);
			this.OverseasAgentPanel.TabIndex = 4;
			// 
			// JH_OH_AgentCollectBoundOrgCard
			// 
			this.JH_OH_AgentCollectBoundOrgCard.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JH_OH_AgentCollectBoundOrgCard, "JH_OA_AgentCollectAddr_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZAddress)(((Job)(null)).JH_OA_AgentCollectAddr_ZAddress)));
			this.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JH_OH_AgentCollectBoundOrgCard, false);
			this.JH_OH_AgentCollectBoundOrgCard.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.JH_OH_AgentCollectBoundOrgCard.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.JH_OH_AgentCollectBoundOrgCard.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.JH_OH_AgentCollectBoundOrgCard.Name = "JH_OH_AgentCollectBoundOrgCard";
			this.JH_OH_AgentCollectBoundOrgCard.PopupCaption = "";
			this.JH_OH_AgentCollectBoundOrgCard.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.JH_OH_AgentCollectBoundOrgCard.TabIndex = 0;
			// 
			// InvoicingFieldsPanel
			// 
			this.InvoicingFieldsPanel.Controls.Add(this.TaxBranchGuidFindBox);
			this.InvoicingFieldsPanel.Controls.Add(this.IsJobDescriptionOverriden);
			this.InvoicingFieldsPanel.Controls.Add(this.JobDescriptionTextBox);
			this.InvoicingFieldsPanel.Controls.Add(this.PLMarginCalcEdit);
			this.InvoicingFieldsPanel.Controls.Add(this.SalesRepFindBox);
			this.InvoicingFieldsPanel.Controls.Add(this.OperatorFindBox);
			this.InvoicingFieldsPanel.Controls.Add(this.HoldReasonTextBox);
			this.InvoicingFieldsPanel.Controls.Add(this.JobLocalReferenceTextBox);
			this.InvoicingFieldsPanel.Controls.Add(this.JobCloseDateEdit);
			this.InvoicingFieldsPanel.Controls.Add(this.JobOpeningDateEdit);
			this.InvoicingFieldsPanel.Controls.Add(this.PLReasonDropEdit);
			this.InvoicingFieldsPanel.Controls.Add(this.JobStatusDropDownEdit);
			this.InvoicingFieldsPanel.Controls.Add(this.JH_GEBoundFindBox);
			this.InvoicingFieldsPanel.Controls.Add(this.JH_GBBoundFindBox);
			this.InvoicingFieldsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 4, true);
			this.InvoicingFieldsPanel.Name = "InvoicingFieldsPanel";
			this.InvoicingFieldsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 151, true);
			this.InvoicingFieldsPanel.TabIndex = 5;
			// 
			// TaxBranchGuidFindBox
			// 
			this.TaxBranchGuidFindBox.AllowDrop = true;
			this.TaxBranchGuidFindBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TaxBranchGuidFindBox, "JH_GB_TaxBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Job)(null)).JH_GB_TaxBranch)));
			this.TaxBranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1ba355b5-ddbf-4b7f-afd5-4c1e3a7a54b7", "Tax Branch");
			this.TaxBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 85, true);
			this.TaxBranchGuidFindBox.Name = "TaxBranchGuidFindBox";
			this.TaxBranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TaxBranchGuidFindBox.ParentType = null;
			this.TaxBranchGuidFindBox.PreBoundMaxLength = 5;
			this.TaxBranchGuidFindBox.ShowDescriptionBox = false;
			this.TaxBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.TaxBranchGuidFindBox.TabIndex = 9;
			// 
			// IsJobDescriptionOverriden
			// 
			this.IsJobDescriptionOverriden.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.IsJobDescriptionOverriden.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsJobDescriptionOverriden, "IsJobDescriptionOverriden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Job)(null)).IsJobDescriptionOverriden)));
			this.IsJobDescriptionOverriden.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("049a9917-7c59-4728-9495-04f4c69b6869", "Override Desc.", "Override Job Description");
			this.IsJobDescriptionOverriden.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 20, true);
			this.IsJobDescriptionOverriden.Name = "IsJobDescriptionOverriden";
			this.IsJobDescriptionOverriden.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.IsJobDescriptionOverriden.TabIndex = 1;
			this.IsJobDescriptionOverriden.UseVisualStyleBackColor = true;
			// 
			// JobDescriptionTextBox
			// 
			this.JobDescriptionTextBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobDescriptionTextBox, "JobDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JobDescription)));
			this.JobDescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("486e33d5-6063-4f17-bb4c-d667ee06d74e", "Job Desc.", "Job Description");
			this.JobDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JobDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 0, true);
			this.JobDescriptionTextBox.Name = "JobDescriptionTextBox";
			this.JobDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 20, true);
			this.JobDescriptionTextBox.TabIndex = 0;
			// 
			// PLMarginCalcEdit
			// 
			this.PLMarginCalcEdit.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PLMarginCalcEdit, "JH_ProfitRevenueMargin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Job)(null)).JH_ProfitRevenueMargin)));
			this.PLMarginCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|a9e8f16f-a016-4cd7-9b73-ecb92fb13ed7", "Margin %");
			this.PLMarginCalcEdit.DecimalPlaces = 2;
			this.PLMarginCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 62, true);
			this.PLMarginCalcEdit.Name = "PLMarginCalcEdit";
			this.PLMarginCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.PLMarginCalcEdit.TabIndex = 6;
			this.PLMarginCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SalesRepFindBox
			// 
			this.SalesRepFindBox.AllowDrop = true;
			this.SalesRepFindBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SalesRepFindBox, "JH_GS_NKRepSales");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_GS_NKRepSales)));
			this.SalesRepFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 85, true);
			this.SalesRepFindBox.Name = "SalesRepFindBox";
			this.SalesRepFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SalesRepFindBox.ParentType = null;
			this.SalesRepFindBox.PreBoundMaxLength = 5;
			this.SalesRepFindBox.ShowDescriptionBox = false;
			this.SalesRepFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.SalesRepFindBox.TabIndex = 10;
			// 
			// OperatorFindBox
			// 
			this.OperatorFindBox.AllowDrop = true;
			this.OperatorFindBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OperatorFindBox, "JH_GS_NKRepOps");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_GS_NKRepOps)));
			this.OperatorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 106, true);
			this.OperatorFindBox.Name = "OperatorFindBox";
			this.OperatorFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OperatorFindBox.ParentType = null;
			this.OperatorFindBox.PreBoundMaxLength = 5;
			this.OperatorFindBox.ShowDescriptionBox = false;
			this.OperatorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.OperatorFindBox.TabIndex = 12;
			// 
			// HoldReasonTextBox
			// 
			this.HoldReasonTextBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HoldReasonTextBox, "JH_HoldReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_HoldReason)));
			this.HoldReasonTextBox.CaptionResourceString = null;
			this.HoldReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HoldReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 126, true);
			this.HoldReasonTextBox.Name = "HoldReasonTextBox";
			this.HoldReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.HoldReasonTextBox.TabIndex = 14;
			// 
			// JobLocalReferenceTextBox
			// 
			this.JobLocalReferenceTextBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobLocalReferenceTextBox, "JH_JobLocalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_JobLocalReference)));
			this.JobLocalReferenceTextBox.CaptionResourceString = null;
			this.JobLocalReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JobLocalReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 20, true);
			this.JobLocalReferenceTextBox.Name = "JobLocalReferenceTextBox";
			this.JobLocalReferenceTextBox.ReadOnly = true;
			this.JobLocalReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 20, true);
			this.JobLocalReferenceTextBox.TabIndex = 3;
			// 
			// JobCloseDateEdit
			// 
			this.JobCloseDateEdit.AllowDrop = true;
			this.JobCloseDateEdit.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.JobCloseDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JobCloseDateEdit, "JH_A_JCL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Job)(null)).JH_A_JCL)));
			this.JobCloseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 40, true);
			this.JobCloseDateEdit.Name = "JobCloseDateEdit";
			this.JobCloseDateEdit.TabIndex = 5;
			// 
			// JobOpeningDateEdit
			// 
			this.JobOpeningDateEdit.AllowDrop = true;
			this.JobOpeningDateEdit.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.JobOpeningDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.JobOpeningDateEdit, "JH_A_JOP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Job)(null)).JH_A_JOP)));
			this.JobOpeningDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 41, true);
			this.JobOpeningDateEdit.Name = "JobOpeningDateEdit";
			this.JobOpeningDateEdit.TabIndex = 4;
			// 
			// PLReasonDropEdit
			// 
			this.PLReasonDropEdit.AllowDrop = true;
			this.PLReasonDropEdit.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PLReasonDropEdit, "JH_ProfitLossReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Job)(null)).JH_ProfitLossReasonCode)));
			this.PLReasonDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("19900e2a-b204-4e13-acbe-7b1c2dde1437", "P/L Reason");
			this.PLReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 63, true);
			this.PLReasonDropEdit.Name = "PLReasonDropEdit";
			this.PLReasonDropEdit.PreBoundMaxLength = 3;
			this.PLReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.PLReasonDropEdit.TabIndex = 7;
			// 
			// JobStatusDropDownEdit
			// 
			this.JobStatusDropDownEdit.AllowDrop = true;
			this.JobStatusDropDownEdit.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobStatusDropDownEdit, "JH_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Job)(null)).JH_Status)));
			this.JobStatusDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 126, true);
			this.JobStatusDropDownEdit.Name = "JobStatusDropDownEdit";
			this.JobStatusDropDownEdit.PreBoundMaxLength = 3;
			this.JobStatusDropDownEdit.ShowDescriptionBox = false;
			this.JobStatusDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JobStatusDropDownEdit.TabIndex = 13;
			// 
			// JH_GEBoundFindBox
			// 
			this.JH_GEBoundFindBox.AllowDrop = true;
			this.JH_GEBoundFindBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JH_GEBoundFindBox, "JH_GE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Job)(null)).JH_GE)));
			this.JH_GEBoundFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|28c90566-223a-453d-b254-65fb762cb523", "Dept");
			this.JH_GEBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 105, true);
			this.JH_GEBoundFindBox.Name = "JH_GEBoundFindBox";
			this.JH_GEBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JH_GEBoundFindBox.ParentType = null;
			this.JH_GEBoundFindBox.PreBoundMaxLength = 5;
			this.JH_GEBoundFindBox.ShowDescriptionBox = false;
			this.JH_GEBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JH_GEBoundFindBox.TabIndex = 11;
			// 
			// JH_GBBoundFindBox
			// 
			this.JH_GBBoundFindBox.AllowDrop = true;
			this.JH_GBBoundFindBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JH_GBBoundFindBox, "JH_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Job)(null)).JH_GB)));
			this.JH_GBBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 84, true);
			this.JH_GBBoundFindBox.Name = "JH_GBBoundFindBox";
			this.JH_GBBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JH_GBBoundFindBox.ParentType = null;
			this.JH_GBBoundFindBox.PreBoundMaxLength = 5;
			this.JH_GBBoundFindBox.ShowDescriptionBox = false;
			this.JH_GBBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.JH_GBBoundFindBox.TabIndex = 8;
			// 
			// TotalCFXAmountCalcEdit
			// 
			this.TotalCFXAmountCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalCFXAmountCalcEdit, "JH_TotalCFX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Job)(null)).JH_TotalCFX)));
			this.TotalCFXAmountCalcEdit.BindToDecimalPlaces = "JH_LocalCurrencyDecimals";
			this.TotalCFXAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|e9b16cf3-2ed9-4711-adac-6e222f5a6ab8", "CFX");
			this.TotalCFXAmountCalcEdit.DecimalPlaces = 2;
			this.TotalCFXAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 0, true);
			this.TotalCFXAmountCalcEdit.Name = "TotalCFXAmountCalcEdit";
			this.TotalCFXAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.TotalCFXAmountCalcEdit.TabIndex = 15;
			this.TotalCFXAmountCalcEdit.Text = "0.00";
			this.TotalCFXAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalsPanel
			// 
			this.TotalsPanel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalsPanel.Controls.Add(this.TotalsLabel);
			this.TotalsPanel.Controls.Add(this.ProfitLossCurrencytTextBox);
			this.TotalsPanel.Controls.Add(this.TotalCFXAmountCalcEdit);
			this.TotalsPanel.Controls.Add(this.TotalAgentAmountCalcEdit);
			this.TotalsPanel.Controls.Add(this.TotalCostAmountCalcEdit);
			this.TotalsPanel.Controls.Add(this.ProfitLossAmountCalcEdit);
			this.TotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 282, true);
			this.TotalsPanel.Name = "TotalsPanel";
			this.TotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 21, true);
			this.TotalsPanel.TabIndex = 18;
			// 
			// TotalsLabel
			// 
			this.TotalsLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalsLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
			this.TotalsLabel.Name = "TotalsLabel";
			this.TotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 14, true);
			this.TotalsLabel.TabIndex = 18;
			this.TotalsLabel.Text = "Totals:";
			// 
			// JobChargeBoundGrid
			// 
			this.JobChargeBoundGrid.AllowNavigation = false;
			this.JobChargeBoundGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobChargeBoundGrid, "FilteredCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Job)(null)).FilteredCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).MarginPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_RX_NKCostCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostGovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_EstimatedCost)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellGovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LocalCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OH_CostAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AT_CostGSTRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDate)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostTaxDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CostRecognition)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsCostPosted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsApproved)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsApportioned)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_RX_NKSellCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_EstimatedRevenue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LocalSellAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OH_SellAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AT_SellGSTRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDate)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellTaxDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LineCFX)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).SellRecognition)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsRevenuePosted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_PreventInvoicePrintGrouping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_ARInvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_InvoiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_RelatedJobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_InvoiceTarget)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JobReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellRatingOverrideComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_DisplaySequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_RelatedConsolRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CFXAmtReverseSign)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).CostAccount.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).SellAccount.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ChargeCodePrintSequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_JobInvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellPlaceOfSupply)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostPlaceOfSupply)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_APInvoiceNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_APInvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_PaymentDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_APDocumentReceivedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_A9_CostVATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_A9_SellVATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).DisplaySellInvoiceAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).DisplaySellInvoiceContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AgentDeclaredCostAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AgentDeclaredCostAmtLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AgentDeclaredSellAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_AgentDeclaredSellAmtLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsIncludedInProfitShare)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GE_InternalDept)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GB_InternalBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_JH_InternalJob)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_LocalSellInvoiceAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_RX_NKSellInvoiceCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellInvoiceAmt_ForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellInvoiceGSTAmt_ForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellInvoiceExRate_ForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSCostExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_OSSellExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_SellRatingBehavior)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_Calc_CostRatingBehavior)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostRatingOverrideComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_CostSupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_SellSupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).SellComplianceDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GB_CostTaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_GB_SellTaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).JR_IsARCashAdvance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ARCashAdvanceRequestStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ARCashAdvanceRequestID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ChargeGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Charge)(((System.Collections.IList)(((Job)(null)).FilteredCharges)).SyncRoot)).ChargeCodeSubGroup)));
			this.JobChargeBoundGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JR_AC";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|4b80b18e-1058-481a-9953-94e4079ed062", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeType";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|3028b863-f55a-449f-a741-99d7631b6928", "Margin %");
			zCalcEditColumnStyleInfo1.ColumnName = "MarginPercentage";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zMultiLineTextBoxColumnInfo1.ColumnName = "JR_Desc";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 370;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JR_GB";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "JR_GE";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JR_RX_NKCostCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|9f2d8248-ac4c-419c-bd8f-3666a7bbc479", "OS Cost Amt", "Overseas Cost Amount", "");
			zCalcEditColumnStyleInfo2.ColumnName = "JR_OSCostAmt";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bb53c511-1711-4378-a0cf-8583c8b37341", "Cost Government Charge Code");
			zTextBoxColumnStyleInfo2.ColumnName = "JR_CostGovtChargeCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JR_EstimatedCost";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("05395b7e-3b1c-4814-acb0-b8162542f1b7", "Sell Government Charge Code");
			zTextBoxColumnStyleInfo3.ColumnName = "JR_SellGovtChargeCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|dc2ba4b8-ee88-473e-9d1a-710c99d8bef7", "Local Cost Amt", "Local Cost Amount", "");
			zCalcEditColumnStyleInfo4.ColumnName = "JR_LocalCostAmt";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ffade182-d906-44b6-aebf-695de1b3ce85", "Creditor");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JR_OH_CostAccount";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "JR_AT_CostGSTRate";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "JR_CostTaxDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|8804595c-51b2-429e-8749-22284e9516d9", "Cost Recognition");
			zTextBoxColumnStyleInfo4.ColumnName = "CostRecognition";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|b46e89b2-a7a0-4020-aa75-6ffea8065a9a", "Posted - Cost");
			zCheckBoxColumnStyleInfo1.ColumnName = "JR_IsCostPosted";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|a83f7694-ccef-419d-9512-18e8174fa341", "Approved");
			zCheckBoxColumnStyleInfo2.ColumnName = "JR_IsApproved";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|3ba0c202-043f-49e1-9d51-8e0e8df69c6e", "Apt", "Apportioned", "");
			zCheckBoxColumnStyleInfo3.ColumnName = "JR_IsApportioned";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JR_RX_NKSellCurrency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|4db64b71-441f-44e6-9a47-70937d8f8d86", "OS Sell Amt", "Overseas Sell Amount", "");
			zCalcEditColumnStyleInfo5.ColumnName = "JR_OSSellAmt";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JR_EstimatedRevenue";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|3f7170a1-0daf-40d5-b326-6a683ba88c1e", "Local Sell Amt", "Local Sell Amount", "");
			zCalcEditColumnStyleInfo7.ColumnName = "JR_LocalSellAmt";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "JR_OH_SellAccount";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "JR_AT_SellGSTRate";
			zGuidFindBoxColumnStyleInfo5.IsVisible = false;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "JR_SellTaxDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "JR_LineCFX";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|6418e29c-f240-4d57-9259-ead1b39c194f", "Sell Recognition");
			zTextBoxColumnStyleInfo5.ColumnName = "SellRecognition";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|f092637f-f095-48ef-b589-628011f835ec", "Posted - Revenue");
			zCheckBoxColumnStyleInfo4.ColumnName = "JR_IsRevenuePosted";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|5f8c6836-97e8-4290-9ef0-62b154cc4b2a", "Prevent Inv. Print Grouping", "Prevent Invoice Print Grouping.");
			zCheckBoxColumnStyleInfo5.ColumnName = "JR_PreventInvoicePrintGrouping";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|76840e2e-0f3d-4db4-b640-35cb2daef607", "AR Invoice Number");
			zTextBoxColumnStyleInfo6.ColumnName = "JR_ARInvoiceNumber";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|7e6fe2cb-69ee-4113-8478-840dbbce5cc0", "Inv. Type", "Invoice Type.");
			zDropEditColumnStyleInfo1.ColumnName = "JR_InvoiceType";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.ColumnName = "JR_Calc_RelatedJobNumber";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.ColumnName = "JR_Calc_InvoiceTarget";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|3dc91c11-32f7-4f42-85a3-842dac0e67df", "Job Num.", "Job Number");
			zTextBoxColumnStyleInfo7.ColumnName = "JobReference";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo8.ColumnName = "JR_SellRatingOverrideComment";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "JR_DisplaySequence";
			zCalcEditColumnStyleInfo9.Decimals = 0;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|9d10dbea-a30e-41e5-812e-cbc673666280", "Related Consol Ref", "Displays a reference of the consol that this charge is related to.");
			zTextBoxColumnStyleInfo9.ColumnName = "JR_RelatedConsolRef";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|30898402-7ceb-4ec2-8d45-7f009142a711", "CFX Jnl.", "CFX Journal");
			zCalcEditColumnStyleInfo10.ColumnName = "JR_CFXAmtReverseSign";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|e7b9cd23-9fe7-4e02-8363-d257f211c5c2", "Creditor Name");
			zTextBoxColumnStyleInfo10.ColumnName = "CostAccount+OH_FullName";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|ec4f286d-2dcc-4198-a4fe-78754aa51b33", "Debtor Name");
			zTextBoxColumnStyleInfo11.ColumnName = "SellAccount+OH_FullName";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("JobChargeUserControl|1e8ca685-1db7-4a45-bf21-76b970b289e1", "Charge Code Print Sequence");
			zCalcEditColumnStyleInfo11.ColumnName = "ChargeCodePrintSequence";
			zCalcEditColumnStyleInfo11.Decimals = 0;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e05c84a7-702b-482a-9693-3c6c34f0de4b", "Job Invoice Number");
			zTextBoxColumnStyleInfo12.ColumnName = "JR_JobInvoiceNumber";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "JR_SellPlaceOfSupply";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "JR_CostPlaceOfSupply";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2025e260-b075-4bfc-8853-38ad85f47166", "Invoice No");
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "JR_APInvoiceNum";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("efe90a5e-3be6-48b1-9a2f-f38dca06e11b", "Invoice Date");
			zDateEditColumnStyleInfo3.ColumnName = "JR_APInvoiceDate";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5207ef2f-4cd7-40e9-b96c-f59a19aad5ac", "Due Date");
			zDateEditColumnStyleInfo4.ColumnName = "JR_PaymentDate";
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("621911B7-45AA-4513-B745-84A04025302C", "Document Received Date");
			zDateEditColumnStyleInfo5.ColumnName = "JR_APDocumentReceivedDate";
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo14.ColumnName = "JR_CostReference";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "JR_A9_CostVATClass";
			zGuidFindBoxColumnStyleInfo6.IsVisible = false;
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.ColumnName = "JR_A9_SellVATClass";
			zGuidFindBoxColumnStyleInfo7.IsVisible = false;
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1251f872-8581-4153-9237-39f02c13c4fc", "Sell Address");
			zAddressDropEditColumnStyleInfo1.ColumnName = "DisplaySellInvoiceAddress";
			zAddressDropEditColumnStyleInfo1.IsVisible = false;
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ad551f42-fe3e-4143-bce3-c3aca88a8051", "Sell Contact");
			zGuidDropEditColumnStyleInfo1.ColumnName = "DisplaySellInvoiceContact";
			zGuidDropEditColumnStyleInfo1.IsVisible = false;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("10c32518-259c-432c-a13c-74282c2acdb4", "Agent Declared Cost Amt");
			zCalcEditColumnStyleInfo12.ColumnName = "JR_AgentDeclaredCostAmt";
			zCalcEditColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e6b4879b-1bd9-4d40-896c-47a10d1d4c22", "Agent Declared Local Cost Amt");
			zCalcEditColumnStyleInfo13.ColumnName = "JR_AgentDeclaredCostAmtLocal";
			zCalcEditColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1acce101-06e6-4b54-b452-0070ae47bdc3", "Agent Declared Sell Amt");
			zCalcEditColumnStyleInfo14.ColumnName = "JR_AgentDeclaredSellAmt";
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5571b662-7b61-4472-8dc2-1a1b24e26d3d", "Agent Declared Sell Local Amt");
			zCalcEditColumnStyleInfo15.ColumnName = "JR_AgentDeclaredSellAmtLocal";
			zCalcEditColumnStyleInfo15.IsVisible = false;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3ec7bf3f-bdbf-4ff6-b3b2-7dc8ec321b30", "Include In Profit Share");
			zCheckBoxColumnStyleInfo6.ColumnName = "JR_IsIncludedInProfitShare";
			zCheckBoxColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo8.ColumnName = "JR_GE_InternalDept";
			zGuidFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo9.ColumnName = "JR_GB_InternalBranch";
			zGuidFindBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo10.ColumnName = "JR_JH_InternalJob";
			zGuidFindBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo15.ColumnName = "JR_SellReference";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e805f5f1-1384-4d3a-a8a1-7da8c4b8c96d", "Local Sell Inv. Amount");
			zCalcEditColumnStyleInfo16.ColumnName = "JR_LocalSellInvoiceAmt";
			zCalcEditColumnStyleInfo16.IsVisible = false;
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ffeff1bc-2a91-447c-bc2a-b569f523a678", "Sell Inv. Currency", "Sell Invoice Currency");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "JR_RX_NKSellInvoiceCurrency";
			zCodeFindBoxColumnStyleInfo3.IsVisible = false;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("36e6a3fc-28b0-44b0-b8bb-65accdd97714", "Sell Inv. Amt");
			zTextBoxColumnStyleInfo16.ColumnName = "JR_OSSellInvoiceAmt_ForDisplay";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e2fd5fc5-4b3b-4777-ade0-3d3b70f8fa4d", "Sell Inv. Tax Amt");
			zTextBoxColumnStyleInfo17.ColumnName = "JR_OSSellInvoiceGSTAmt_ForDisplay";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ae211f24-bb21-42ca-8675-25b9ff0a06bb", "Sell Inv. Ex Rate");
			zTextBoxColumnStyleInfo18.ColumnName = "JR_OSSellInvoiceExRate_ForDisplay";
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.ColumnName = "JR_OSCostExRate";
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.ColumnName = "JR_OSSellExRate";
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ff83e05f-1329-4590-a566-e2e5681e6646", "Sell Rating Behavior");
			zDropEditColumnStyleInfo6.ColumnName = "JR_Calc_SellRatingBehavior";
			zDropEditColumnStyleInfo6.IsVisible = false;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("420b080a-b66c-4b7d-8097-aab2c26fb87c", "Cost Rating Behavior");
			zDropEditColumnStyleInfo7.ColumnName = "JR_Calc_CostRatingBehavior";
			zDropEditColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a5b31d44-4995-4b01-9d60-601a9d49b6c4", "Cost Rating Override Comment");
			zTextBoxColumnStyleInfo19.ColumnName = "JR_CostRatingOverrideComment";
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo8.ColumnName = "JR_CostSupplyType";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo9.ColumnName = "JR_SellSupplyType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo20.ColumnName = "SellComplianceDescription";
			zTextBoxColumnStyleInfo20.IsVisible = false;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo11.ColumnName = "JR_GB_CostTaxBranch";
			zGuidFindBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo12.ColumnName = "JR_GB_SellTaxBranch";
			zGuidFindBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("114861c3-104d-4e9b-84a9-2562a6047076", "Advance Payment Req.");
			zCheckBoxColumnStyleInfo7.ColumnName = "JR_IsARCashAdvance";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("69966b83-fd7b-4da7-9701-9b1c2a9ef7d6", "Advance Payment Status");
			zTextBoxColumnStyleInfo21.ColumnName = "ARCashAdvanceRequestStatus";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("36296291-2ec3-46ca-8504-35fad475f322", "Advance Payment Request ID");
			zTextBoxColumnStyleInfo22.ColumnName = "ARCashAdvanceRequestID";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("722DD46D-3C3D-40D0-9DF1-B5864DE7DF45", "Charge Group");
			zTextBoxColumnStyleInfo23.ColumnName = "ChargeGroup";
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.IsReadOnly = true;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("B5B50945-6905-43DF-A6A4-EF9583DD8360", "Charge Code Sub Group");
			zTextBoxColumnStyleInfo24.ColumnName = "ChargeCodeSubGroup";
			zTextBoxColumnStyleInfo24.IsVisible = false;
			zTextBoxColumnStyleInfo24.IsReadOnly = true;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.JobChargeBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.JobChargeBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.JobChargeBoundGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo8);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo9);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo10);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.JobChargeBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo11);
			this.JobChargeBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo12);
			this.JobChargeBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.JobChargeBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.JobChargeBoundGrid.GridId = "b2686645-1fb3-452c-bb58-474d2e011eb7";
			this.JobChargeBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobChargeBoundGrid.LayoutKey = "JobChargeBoundGrid";
			this.JobChargeBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 194, true);
			this.JobChargeBoundGrid.Name = "JobChargeBoundGrid";
			this.JobChargeBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1262, 79, true);
			this.JobChargeBoundGrid.TabIndex = 6;
			this.JobChargeBoundGrid.CurrentCellChanged += new EventHandler(this.JobChargeBoundGrid_CurrentCellChanged);
			this.JobChargeBoundGrid.Click += new EventHandler(this.JobChargeBoundGrid_Click);
			// 
			// ChargeHidingMessageLabel
			// 
			this.ChargeHidingMessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("324179d4-63b3-47a1-a429-68de7a1c7759", "Charge lines with branch / dept outside your login permission are not listed.");
			this.ChargeHidingMessageLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChargeHidingMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 168, true);
			this.ChargeHidingMessageLabel.Name = "ChargeHidingMessageLabel";
			this.ChargeHidingMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 16, true);
			this.ChargeHidingMessageLabel.TabIndex = 18;
			this.ChargeHidingMessageLabel.Visible = false;
			// 
			// ExcludeFromPeriodicRatingCheckbox
			// 
			this.ExcludeFromPeriodicRatingCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExcludeFromPeriodicRatingCheckbox, "JH_ExcludeFromPeriodicRating");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Job)(null)).JH_ExcludeFromPeriodicRating)));
			this.ExcludeFromPeriodicRatingCheckbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1ad71512-ff22-4123-b787-c28a80881b04", "Exclude from Periodic Auto-Rating");
			this.ExcludeFromPeriodicRatingCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 169, true);
			this.ExcludeFromPeriodicRatingCheckbox.Name = "ExcludeFromPeriodicRatingCheckbox";
			this.ExcludeFromPeriodicRatingCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.ExcludeFromPeriodicRatingCheckbox.TabIndex = 16;
			this.ExcludeFromPeriodicRatingCheckbox.UseVisualStyleBackColor = true;
			// 
			// ShipmentAndBillingDetailsLinkLabel
			// 
			this.ShipmentAndBillingDetailsLinkLabel.AutoSize = true;
			this.ShipmentAndBillingDetailsLinkLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4cd6b2e1-3b90-4cc5-a536-cd88eeef4f07", "Shipment and Billing Details");
			this.ShipmentAndBillingDetailsLinkLabel.IsFontBold = false;
			this.ShipmentAndBillingDetailsLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
			this.ShipmentAndBillingDetailsLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 170, true);
			this.ShipmentAndBillingDetailsLinkLabel.Name = "ShipmentAndBillingDetailsLinkLabel";
			this.ShipmentAndBillingDetailsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 13, true);
			this.ShipmentAndBillingDetailsLinkLabel.TabIndex = 16;
			this.ShipmentAndBillingDetailsLinkLabel.Visible = false;
			this.ShipmentAndBillingDetailsLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.ShipmentAndBillingDetailsLinkLabel_Clicked);
			// 
			// IsChargeCostReferenceFilterEnabledCheckBox
			// 
			this.IsChargeCostReferenceFilterEnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsChargeCostReferenceFilterEnabledCheckBox, "JH_IsChargeCostReferenceFilterEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Job)(null)).JH_IsChargeCostReferenceFilterEnabled)));
			this.IsChargeCostReferenceFilterEnabledCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7e185dda-cd2d-4a06-a8e5-8e9ad95edf0b", "Only Show Charges for this Job");
			this.IsChargeCostReferenceFilterEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(739, 166, true);
			this.IsChargeCostReferenceFilterEnabledCheckBox.Name = "IsChargeCostReferenceFilterEnabledCheckBox";
			this.IsChargeCostReferenceFilterEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
			this.IsChargeCostReferenceFilterEnabledCheckBox.TabIndex = 17;
			this.IsChargeCostReferenceFilterEnabledCheckBox.UseVisualStyleBackColor = true;
			// 
			// ClientContractNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientContractNumberTextBox, "JH_ClientContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_ClientContractNumber)));
			this.ClientContractNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("983bddd6-67b5-4a99-9da8-3390a1d18f3b", "Client Contract No.");
			this.ClientContractNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientContractNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 166, true);
			this.ClientContractNumberTextBox.Name = "ClientContractNumberTextBox";
			this.ClientContractNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.ClientContractNumberTextBox.TabIndex = 2;
			this.ClientContractNumberTextBox.Visible = false;
			// 
			// ClientContractNumberButton
			// 
			this.ClientContractNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 166, true);
			this.ClientContractNumberButton.Name = "ClientContractNumberButton";
			this.ClientContractNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ClientContractNumberButton.TabIndex = 8;
			this.ClientContractNumberButton.Font = OFont.GetFontBold();
			this.ClientContractNumberButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClientContractNumberButton.Text = "...";
			this.ClientContractNumberButton.ToolTipCaption = null;
			this.ClientContractNumberButton.Click += new EventHandler(this.ClientContractNumberButton_Click);
			// 
			// QuotesCodeFindBox
			// 
			this.QuotesCodeFindBox.AllowDrop = true;
			this.QuotesCodeFindBox.AllowNewForm = false;
			this.BindingSource.SetBindingMember(this.QuotesCodeFindBox, "JH_TH_NKQuoteNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Job)(null)).JH_TH_NKQuoteNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Job)(null)).Quotes)));
			this.QuotesCodeFindBox.BindToList = "Quotes";
			this.QuotesCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 166, true);
			this.QuotesCodeFindBox.Name = "QuotesCodeFindBox";
			this.QuotesCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.QuotesCodeFindBox.ParentType = null;
			this.QuotesCodeFindBox.ShouldResize = false;
			this.QuotesCodeFindBox.ShowDescriptionBox = false;
			this.QuotesCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.QuotesCodeFindBox.TabIndex = 3;
			// 
			// ExchangeRatesZPanel
			// 
			this.ExchangeRatesZPanel.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ExchangeRatesZPanel.Controls.Add(this.JobExRateBoundGrid);
			this.ExchangeRatesZPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(952, 4, true);
			this.ExchangeRatesZPanel.Name = "ExchangeRatesZPanel";
			this.ExchangeRatesZPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 151, true);
			this.ExchangeRatesZPanel.TabIndex = 20;
			// 
			// JobExRateBoundGrid
			// 
			this.JobExRateBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobExRateBoundGrid, "ExchangeRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Job)(null)).ExchangeRates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_RX_NKRateCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_BaseRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_TodayRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_OrgType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_OH_Org)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_CFXPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_CFXMinimum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_IsTransformed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ExchangeRate)(((System.Collections.IList)(((Job)(null)).ExchangeRates)).SyncRoot)).JF_InvoiceCurrencyType)));
			this.JobExRateBoundGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo4.ColumnName = "JF_RX_NKRateCurrency";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo19.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo19.ColumnName = "JF_BaseRate";
			zCalcEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo20.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo20.ColumnName = "JF_TodayRate";
			zCalcEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo10.ColumnName = "JF_OrgType";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.ColumnName = "JF_InvoiceCurrencyType";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("95b570c2-c8e5-476c-af21-b556529a6c6d", "Organization");
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "JF_OH_Org";
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo21.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo21.ColumnName = "JF_CFXPercent";
			zCalcEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo22.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo22.ColumnName = "JF_CFXMinimum";
			zCalcEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo8.ColumnName = "JF_IsTransformed";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobExRateBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.JobExRateBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo19);
			this.JobExRateBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo20);
			this.JobExRateBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.JobExRateBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.JobExRateBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo21);
			this.JobExRateBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo22);
			this.JobExRateBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.JobExRateBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.JobExRateBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobExRateBoundGrid.GridId = "66b904f8-f2da-48b6-9ab4-520fcccc381e";
			this.JobExRateBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobExRateBoundGrid.LayoutKey = "JobExRateBoundGrid";
			this.JobExRateBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobExRateBoundGrid.Name = "JobExRateBoundGrid";
			this.JobExRateBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 151, true);
			this.JobExRateBoundGrid.TabIndex = 22;
			// 
			// TaxExpenseTotalsPanel
			// 
			this.TaxExpenseTotalsPanel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TaxExpenseTotalsPanel.Controls.Add(this.TaxExpenseTotalsLabel);
			this.TaxExpenseTotalsPanel.Controls.Add(this.TotalTaxExpenseRevenueCalcEdit);
			this.TaxExpenseTotalsPanel.Controls.Add(this.TotalTaxExpenseCostCalcEdit);
			this.TaxExpenseTotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 282, true);
			this.TaxExpenseTotalsPanel.Name = "TaxExpenseTotalsPanel";
			this.TaxExpenseTotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 21, true);
			this.TaxExpenseTotalsPanel.TabIndex = 21;
			// 
			// TaxExpenseTotalsLabel
			// 
			this.TaxExpenseTotalsLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TaxExpenseTotalsLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TaxExpenseTotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.TaxExpenseTotalsLabel.Name = "TaxExpenseTotalsLabel";
			this.TaxExpenseTotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 14, true);
			this.TaxExpenseTotalsLabel.TabIndex = 19;
			this.TaxExpenseTotalsLabel.Text = "Tax Expense Totals:";
			// 
			// TotalTaxExpenseRevenueCalcEdit
			// 
			this.TotalTaxExpenseRevenueCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalTaxExpenseRevenueCalcEdit, "JH_TotalTaxExpenseRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Job)(null)).JH_TotalTaxExpenseRevenue)));
			this.TotalTaxExpenseRevenueCalcEdit.BindToDecimalPlaces = "JH_LocalCurrencyDecimals";
			this.TotalTaxExpenseRevenueCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("611bf923-bac2-488d-9fdd-33395089350f", "Revenue");
			this.TotalTaxExpenseRevenueCalcEdit.DecimalPlaces = 2;
			this.TotalTaxExpenseRevenueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 0, true);
			this.TotalTaxExpenseRevenueCalcEdit.Name = "TotalTaxExpenseRevenueCalcEdit";
			this.TotalTaxExpenseRevenueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.TotalTaxExpenseRevenueCalcEdit.TabIndex = 13;
			this.TotalTaxExpenseRevenueCalcEdit.Text = "0.00";
			this.TotalTaxExpenseRevenueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalTaxExpenseCostCalcEdit
			// 
			this.TotalTaxExpenseCostCalcEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalTaxExpenseCostCalcEdit, "JH_TotalTaxExpenseCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Job)(null)).JH_TotalTaxExpenseCost)));
			this.TotalTaxExpenseCostCalcEdit.BindToDecimalPlaces = "JH_LocalCurrencyDecimals";
			this.TotalTaxExpenseCostCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4cde9cb5-ce20-4454-932a-634370379279", "Cost");
			this.TotalTaxExpenseCostCalcEdit.DecimalPlaces = 2;
			this.TotalTaxExpenseCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 0, true);
			this.TotalTaxExpenseCostCalcEdit.Name = "TotalTaxExpenseCostCalcEdit";
			this.TotalTaxExpenseCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.TotalTaxExpenseCostCalcEdit.TabIndex = 10;
			this.TotalTaxExpenseCostCalcEdit.Text = "0.00";
			this.TotalTaxExpenseCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JobChargeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TaxExpenseTotalsPanel);
			this.Controls.Add(this.ExchangeRatesZPanel);
			this.Controls.Add(this.IsChargeCostReferenceFilterEnabledCheckBox);
			this.Controls.Add(this.ShipmentAndBillingDetailsLinkLabel);
			this.Controls.Add(this.ExcludeFromPeriodicRatingCheckbox);
			this.Controls.Add(this.ClientContractNumberTextBox);
			this.Controls.Add(this.ClientContractNumberButton);
			this.Controls.Add(this.QuotesCodeFindBox);
			this.Controls.Add(this.TotalsPanel);
			this.Controls.Add(this.JH_OH_LocalChargesBoundOrgCard);
			this.Controls.Add(this.ChargesDetailsTabControl);
			this.Controls.Add(this.ChargeHidingMessageLabel);
			this.Controls.Add(this.JobChargeBoundGrid);
			this.Controls.Add(this.OverseasAgentPanel);
			this.Controls.Add(this.InvoicingFieldsPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 406, true);
			this.Name = "JobChargeUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 500, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ChargesDetailsTabControl.ResumeLayout(false);
			this.ChargesDetailsTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.ProfitSharePanel.ResumeLayout(false);
			this.ProfitSharePanel.PerformLayout();
			this.ProfitShareDetailsGroupBox.ResumeLayout(false);
			this.ProfitShareDetailsGroupBox.PerformLayout();
			this.RevenueOtherFieldsPanel.ResumeLayout(false);
			this.RevenueOtherFieldsPanel.PerformLayout();
			this.JR_GB_InternalBranchGuidFindBox.ResumeLayout(true);
			this.JR_GB_InternalBranchGuidFindBox.PerformLayout();
			this.JR_JH_InternalJobGuidFindBox.ResumeLayout(true);
			this.JR_JH_InternalJobGuidFindBox.PerformLayout();
			this.JR_GE_InternalDeptGuidFindBox.ResumeLayout(true);
			this.JR_GE_InternalDeptGuidFindBox.PerformLayout();
			this.ChargeGuidFindBox.ResumeLayout(true);
			this.ChargeGuidFindBox.PerformLayout();
			this.DeptGuidFindBox.ResumeLayout(true);
			this.DeptGuidFindBox.PerformLayout();
			this.BranchesGuidFindBox.ResumeLayout(true);
			this.BranchesGuidFindBox.PerformLayout();
			this.CostTabPage.ResumeLayout(false);
			this.CostTabPage.PerformLayout();
			this.CostTotalsPanel.ResumeLayout(false);
			this.CostTotalsPanel.PerformLayout();
			this.CostRatingBehaviorEdit.ResumeLayout(true);
			this.CostRatingBehaviorEdit.PerformLayout();
			this.OSCostCurrencyCodeFindBox.ResumeLayout(true);
			this.OSCostCurrencyCodeFindBox.PerformLayout();
			this.JR_APInvoiceDateBoundDateEdit.ResumeLayout(true);
			this.JR_APInvoiceDateBoundDateEdit.PerformLayout();
			this.JR_APDocumentReceivedDateBoundDateEdit.ResumeLayout(true);
			this.JR_APDocumentReceivedDateBoundDateEdit.PerformLayout();
			this.CreditorsGuidFindBox.ResumeLayout(true);
			this.CreditorsGuidFindBox.PerformLayout();
			this.JR_PaymentDateBoundDateEdit.ResumeLayout(true);
			this.JR_PaymentDateBoundDateEdit.PerformLayout();
			this.JR_PaymentTypeDropDownEdit.ResumeLayout(true);
			this.JR_PaymentTypeDropDownEdit.PerformLayout();
			this.JR_ABBoundFindBox.ResumeLayout(true);
			this.JR_ABBoundFindBox.PerformLayout();
			this.ChequeBookFindBox.ResumeLayout(true);
			this.ChequeBookFindBox.PerformLayout();
			this.CostTotalPanel.ResumeLayout(false);
			this.CostTotalPanel.PerformLayout();
			this.CostExtraTaxPanel.ResumeLayout(false);
			this.CostExtraTaxPanel.PerformLayout();
			this.CostSupplyTypeDropEdit.ResumeLayout(true);
			this.CostSupplyTypeDropEdit.PerformLayout();
			this.CostWHTPanel.ResumeLayout(false);
			this.CostWHTPanel.PerformLayout();
			this.JR_AW_CostBoundFindBox.ResumeLayout(true);
			this.JR_AW_CostBoundFindBox.PerformLayout();
			this.CostGSTPanel.ResumeLayout(false);
			this.CostGSTPanel.PerformLayout();
			this.CostTaxBranchGuidFindBox.ResumeLayout(true);
			this.CostTaxBranchGuidFindBox.PerformLayout();
			this.JR_AT_CostBoundFindBox.ResumeLayout(true);
			this.JR_AT_CostBoundFindBox.PerformLayout();
			this.JR_CostTaxDateEdit.ResumeLayout(true);
			this.JR_CostTaxDateEdit.PerformLayout();
			this.RevenueTabPage.ResumeLayout(false);
			this.RevenueTabPage.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.CashAdvancePanel.ResumeLayout(false);
			this.CashAdvancePanel.PerformLayout();
			this.SellTotalsPanel.ResumeLayout(false);
			this.SellTotalsPanel.PerformLayout();
			this.SellExtraTaxPanel.ResumeLayout(false);
			this.SellExtraTaxPanel.PerformLayout();
			this.SellGSTPanel.ResumeLayout(false);
			this.SellGSTPanel.PerformLayout();
			this.SellTaxBranchGuidFindBox.ResumeLayout(true);
			this.SellTaxBranchGuidFindBox.PerformLayout();
			this.SellGSTRateGuidFindBox.ResumeLayout(true);
			this.SellGSTRateGuidFindBox.PerformLayout();
			this.SellTaxDateEdit.ResumeLayout(true);
			this.SellTaxDateEdit.PerformLayout();
			this.CFXAmtPanel.ResumeLayout(false);
			this.CFXAmtPanel.PerformLayout();
			this.SellWHTPanel.ResumeLayout(false);
			this.SellWHTPanel.PerformLayout();
			this.SellWHTRateGuidFindBox.ResumeLayout(true);
			this.SellWHTRateGuidFindBox.PerformLayout();
			this.SellSupplyTypeDropEdit.ResumeLayout(true);
			this.SellSupplyTypeDropEdit.PerformLayout();
			this.SellPanel.ResumeLayout(false);
			this.SellPanel.PerformLayout();
			this.OSSellAmountCurrencyCodeFindBox.ResumeLayout(true);
			this.OSSellAmountCurrencyCodeFindBox.PerformLayout();
			this.SellRatingBehaviorEdit.ResumeLayout(true);
			this.SellRatingBehaviorEdit.PerformLayout();
			this.InvoiceTypeDropEdit.ResumeLayout(true);
			this.InvoiceTypeDropEdit.PerformLayout();
			this.DebtorFindBox.ResumeLayout(true);
			this.DebtorFindBox.PerformLayout();
			this.AutoratingCostTabPage.ResumeLayout(false);
			this.AutoratingCostTabPage.PerformLayout();
			this.CostRateCalculationXMLUserControl.ResumeLayout(true);
			this.CostRateCalculationXMLUserControl.PerformLayout();
			this.CostPaymentBasisUserControl.ResumeLayout(true);
			this.CostPaymentBasisUserControl.PerformLayout();
			this.AutoRateDescCostTextBox.ResumeLayout(true);
			this.AutoRateDescCostTextBox.PerformLayout();
			this.WiseRatesRawDataUserControl.ResumeLayout(true);
			this.WiseRatesRawDataUserControl.PerformLayout();
			this.AutoratingSellTabPage.ResumeLayout(false);
			this.AutoratingSellTabPage.PerformLayout();
			this.RevenueRateCalculationXMLUserControl.ResumeLayout(true);
			this.RevenueRateCalculationXMLUserControl.PerformLayout();
			this.RevenuePaymentBasisUserControl.ResumeLayout(true);
			this.RevenuePaymentBasisUserControl.PerformLayout();
			this.AutoRateDescRevenueTextBox.ResumeLayout(true);
			this.AutoRateDescRevenueTextBox.PerformLayout();
			this.TaxTransactionTabPage.ResumeLayout(false);
			this.TaxTransactionTabPage.PerformLayout();
			this.TaxTransactionControl.ResumeLayout(true);
			this.TaxTransactionControl.PerformLayout();
			this.JH_OH_LocalChargesBoundOrgCard.ResumeLayout(true);
			this.JH_OH_LocalChargesBoundOrgCard.PerformLayout();
			this.OverseasAgentPanel.ResumeLayout(false);
			this.OverseasAgentPanel.PerformLayout();
			this.JH_OH_AgentCollectBoundOrgCard.ResumeLayout(true);
			this.JH_OH_AgentCollectBoundOrgCard.PerformLayout();
			this.InvoicingFieldsPanel.ResumeLayout(false);
			this.InvoicingFieldsPanel.PerformLayout();
			this.TaxBranchGuidFindBox.ResumeLayout(true);
			this.TaxBranchGuidFindBox.PerformLayout();
			this.SalesRepFindBox.ResumeLayout(true);
			this.SalesRepFindBox.PerformLayout();
			this.OperatorFindBox.ResumeLayout(true);
			this.OperatorFindBox.PerformLayout();
			this.JobCloseDateEdit.ResumeLayout(true);
			this.JobCloseDateEdit.PerformLayout();
			this.JobOpeningDateEdit.ResumeLayout(true);
			this.JobOpeningDateEdit.PerformLayout();
			this.PLReasonDropEdit.ResumeLayout(true);
			this.PLReasonDropEdit.PerformLayout();
			this.JobStatusDropDownEdit.ResumeLayout(true);
			this.JobStatusDropDownEdit.PerformLayout();
			this.JH_GEBoundFindBox.ResumeLayout(true);
			this.JH_GEBoundFindBox.PerformLayout();
			this.JH_GBBoundFindBox.ResumeLayout(true);
			this.JH_GBBoundFindBox.PerformLayout();
			this.TotalsPanel.ResumeLayout(false);
			this.TotalsPanel.PerformLayout();
			((ISupportInitialize)(this.JobChargeBoundGrid)).EndInit();
			this.JobChargeBoundGrid.ResumeLayout(false);
			this.JobChargeBoundGrid.PerformLayout();
			this.QuotesCodeFindBox.ResumeLayout(true);
			this.QuotesCodeFindBox.PerformLayout();
			this.ExchangeRatesZPanel.ResumeLayout(false);
			this.ExchangeRatesZPanel.PerformLayout();
			((ISupportInitialize)(this.JobExRateBoundGrid)).EndInit();
			this.JobExRateBoundGrid.ResumeLayout(false);
			this.JobExRateBoundGrid.PerformLayout();
			this.TaxExpenseTotalsPanel.ResumeLayout(false);
			this.TaxExpenseTotalsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
