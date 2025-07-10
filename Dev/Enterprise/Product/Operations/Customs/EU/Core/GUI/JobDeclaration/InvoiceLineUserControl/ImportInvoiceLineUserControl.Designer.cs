using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class EUImportInvoiceLineUserControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				LineDetailsTabPage.BindingOrFirstShown -= LineDetailsTabPage_BindingOrFirstShown;
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
            this.PreferenceCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.VatTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.VatDetailGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
            this.QuotaTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.QuotaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SecondQuotaTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SecondQuotaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CountryOfSupplyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.MethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ValuationMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ValuationAdjustmentPercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ValuationAdjustmentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CountryOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CheckQuotaBalanceLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.CountryOfDispatchCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.packagesPivotUserControl.SuspendLayout();
            this.SupportingDocumentsTabPage.SuspendLayout();
            this.InvoiceLinePaymentTabPage.SuspendLayout();
            this.AdditionalInfosTabPage.SuspendLayout();
            this.PreviousDocumentsTabPage.SuspendLayout();
            this.OrganizationsTabPage.SuspendLayout();
            this.ThirdQtyCalcDropEdit.SuspendLayout();
            this.FourthQtyCalcDropEdit.SuspendLayout();
            this.PackagesPivotTabPage.SuspendLayout();
            this.NetWeightCalcDropEdit.SuspendLayout();
            this.SupplementaryCode2DropEdit.SuspendLayout();
            this.SupplementaryCode1DropEdit.SuspendLayout();
            this.CPCFindBox.SuspendLayout();
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_CustomsQuantityCalcDropEdit.SuspendLayout();
            this.tariffFindBox.SuspendLayout();
            this.JI_CEIGuidDropEdit.SuspendLayout();
            this.StatisticalValueLocalCurrencyControl.SuspendLayout();
            this.ValueForGstVatLocalCurrencyControl.SuspendLayout();
            this.CustomsValueLocalCurrencyControl.SuspendLayout();
            this.FiscalReferencesTabPage.SuspendLayout();
            this.goodsOriginDropEdit.SuspendLayout();
            this.TransactionNatureDropEdit.SuspendLayout();
            this.AuthorisationsTabPage.SuspendLayout();
            this.ValueIndicatorsTabPage.SuspendLayout();
            this.SupplyChainActorTabPage.SuspendLayout();
            this.SupplyChainActorReferencesUserControl.SuspendLayout();
            this.invoiceLineValuationIndicatorsUserControl.SuspendLayout();
            this.InvoiceLinesSummaryGroupBox.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.LineDetailTabControl.SuspendLayout();
            this.InvoiceDetailsGroupBox.SuspendLayout();
            this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
            this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
            this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
            this.ClassificationDetailsGroupBox.SuspendLayout();
            this.LineChargesTabPage.SuspendLayout();
            this.CurrentInvoicePanel.SuspendLayout();
            this.LineSummaryPanel.SuspendLayout();
            this.ContainersTabPage.SuspendLayout();
            this.ContainersGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
            this.CusContainerInvoiceLineGrid.SuspendLayout();
            this.LineDetailsTabPage.SuspendLayout();
            this.NewLineDetailsTabPage.SuspendLayout();
            this.InvoiceLineDetailsUserControl.SuspendLayout();
            this.ClassificationPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
            this.CustomsInvoiceLinesBoundGrid.SuspendLayout();
            this.JI_Calc_CIFConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_FreightConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_FOBConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_GSTConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_DutyConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_BalanceConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.SuspendLayout();
            this.CustomsQuantityCalcDropEdit.SuspendLayout();
            this.VolumeCalcDropEdit.SuspendLayout();
            this.JI_WeightCalcDropEdit.SuspendLayout();
            this.InvoiceQuantityCalcDropEdit.SuspendLayout();
            this.JI_DescriptionBoundTextBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PreferenceCodeDropEdit.SuspendLayout();
            this.VatTypeDropEdit.SuspendLayout();
            this.VatDetailGuidDropEdit.SuspendLayout();
            this.QuotaDropEdit.SuspendLayout();
            this.SecondQuotaDropEdit.SuspendLayout();
            this.CountryOfSupplyCodeFindBox.SuspendLayout();
            this.MethodOfPaymentDropEdit.SuspendLayout();
            this.ValuationMethodDropEdit.SuspendLayout();
            this.ValuationAdjustmentCodeDropEdit.SuspendLayout();
            this.CountryOfDestinationCodeFindBox.SuspendLayout();
            this.CountryOfDispatchCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // packagesPivotUserControl
            // 
            this.packagesPivotUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 327, true);
            // 
            // SupportingDocumentsTabPage
            // 
            this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // InvoiceLinePaymentTabPage
            // 
            this.InvoiceLinePaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.InvoiceLinePaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // AdditionalInfosTabPage
            // 
            this.AdditionalInfosTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.AdditionalInfosTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // PreviousDocumentsTabPage
            // 
            this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // OrganizationsTabPage
            // 
            this.OrganizationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.OrganizationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // StatisticalValueCalcEdit
            // 
            this.StatisticalValueCalcEdit.TabIndex = 17;
            // 
            // ThirdQtyCalcDropEdit
            // 
            this.ThirdQtyCalcDropEdit.TabIndex = 15;
            // 
            // PackagesPivotTabPage
            // 
            this.PackagesPivotTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.PackagesPivotTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // TaxTabPage
            // 
            this.TaxTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.TaxTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 342, true);
            // 
            // SupplementaryCode2DropEdit
            // 
            this.SupplementaryCode2DropEdit.TabIndex = 4;
            // 
            // AdditionalSupplementaryCodesEditButton
            // 
            this.AdditionalSupplementaryCodesEditButton.TabIndex = 5;
            // 
            // CPCFindBox
            // 
            this.CPCFindBox.TabIndex = 6;
            // 
            // organizationsUserControl1
            // 
            this.organizationsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            this.organizationsUserControl1.UserControlType = typeof(Enterprise.Customs.EU.GUI.PlugIn.InvoiceLineOrganizationsUserControl);
            // 
            // StatValueManualOverrideCheckBox
            // 
            this.StatValueManualOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 16, true);
            this.StatValueManualOverrideCheckBox.TabIndex = 18;
            // 
            // zLabel2
            // 
            this.zLabel2.TabIndex = 1;
            // 
            // zLabel4
            // 
            this.zLabel4.TabIndex = 3;
            // 
            // JI_CustomsQuantityCalcDropEdit
            // 
            this.JI_CustomsQuantityCalcDropEdit.Decimals = 5;
            this.JI_CustomsQuantityCalcDropEdit.TabIndex = 8;
            // 
            // zButtonMoreAdditionalProcedureCode
            // 
            this.zButtonMoreAdditionalProcedureCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(852, 172, true);
            // 
            // zTextBoxAddtionalProcedureCodeAsString
            // 
            this.zTextBoxAddtionalProcedureCodeAsString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 172, true);
            // 
            // FiscalReferencesTabPage
            // 
            this.FiscalReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.FiscalReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // FiscalReferencesUserControl
            // 
            this.FiscalReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            this.FiscalReferencesUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.InvoiceLineFiscalReferencesUserControl);
            // 
            // TransactionNatureDropEdit
            // 
            this.TransactionNatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(687, 89, true);
            // 
            // CommercialReferenceBox
            // 
            this.CommercialReferenceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 146, true);
            this.CommercialReferenceBox.TabIndex = 18;
            // 
            // AuthorisationsTabPage
            // 
            this.AuthorisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.AuthorisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // AuthorisationsUserControl
            // 
            this.AuthorisationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 327, true);
            this.AuthorisationsUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.InvoiceLineAuthorisationsUserControl);
            // 
            // ValueIndicatorsTabPage
            // 
            this.ValueIndicatorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.ValueIndicatorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 342, true);
            // 
            // SupplyChainActorTabPage
            // 
            this.SupplyChainActorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.SupplyChainActorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // SupplyChainActorReferencesUserControl
            // 
            this.SupplyChainActorReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // invoiceLineValuationIndicatorsUserControl
            // 
            this.invoiceLineValuationIndicatorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 337, true);
            this.invoiceLineValuationIndicatorsUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.InvoiceLineValuationIndicatorCheckboxesUserControl);
            // 
            // InvoiceLinesSummaryGroupBox
            // 
            this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 364, true);
            // 
            // BottomPanel
            // 
            this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 220, true);
            this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 364, true);
            // 
            // TopPanel
            // 
            this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 534, true);
            // 
            // LineDetailTabControl
            // 
            this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 364, true);
            // 
            // InvoiceDetailsGroupBox
            // 
            this.InvoiceDetailsGroupBox.Controls.Add(this.TransactionNatureDropEdit);
            this.InvoiceDetailsGroupBox.Controls.Add(this.MethodOfPaymentDropEdit);
            this.InvoiceDetailsGroupBox.Controls.Add(this.VatDetailGuidDropEdit);
            this.InvoiceDetailsGroupBox.Controls.Add(this.VatTypeDropEdit);
            this.InvoiceDetailsGroupBox.Controls.Add(this.CountryOfSupplyCodeFindBox);
            this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 114, true);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.goodsOriginDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CEIGuidDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CountryOfSupplyCodeFindBox, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VatTypeDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VatDetailGuidDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.MethodOfPaymentDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.TransactionNatureDropEdit, 0);
            // 
            // JI_RH_NKCommodity_CodeBoundFindBox
            // 
            this.JI_RH_NKCommodity_CodeBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 17, true);
            // 
            // ClassificationDetailsGroupBox
            // 
            this.ClassificationDetailsGroupBox.Controls.Add(this.CheckQuotaBalanceLinkLabel);
            this.ClassificationDetailsGroupBox.Controls.Add(this.CountryOfDestinationCodeFindBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.SecondQuotaTextBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.SecondQuotaDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.QuotaTextBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.QuotaDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.PreferenceCodeDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.ValuationAdjustmentPercentageCalcEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.ValuationAdjustmentCodeDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.ValuationMethodDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.CountryOfDispatchCodeFindBox);
            this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 228, true);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDispatchCodeFindBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.BondedWHSOrderLineNumberCalcEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.BondedWHSOrderNumberTextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.FourthQtyCalcDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.tariffFindBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.AdditionalSupplementaryCodesEditButton, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_AdditionalSupplementsTextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zLabel5, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_CustomsQuantityCalcDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zButtonMoreAdditionalProcedureCode, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBoxAddtionalProcedureCodeAsString, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zLabel4, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zLabel2, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.StatValueManualOverrideCheckBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ValuationMethodDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ValuationAdjustmentCodeDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ValuationAdjustmentPercentageCalcEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.StatisticalValueCalcEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode1TextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode2TextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode1DropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode2DropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ThirdQtyCalcDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CPCFindBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.PreferenceCodeDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.QuotaDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.QuotaTextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SecondQuotaDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SecondQuotaTextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.GDMLink, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDestinationCodeFindBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CheckQuotaBalanceLinkLabel, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CommercialReferenceBox, 0);
            // 
            // LineChargesTabPage
            // 
            this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 342, true);
            // 
            // LineSummaryPanel
            // 
            this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 261, true);
            // 
            // PendingApportionmentLabel
            // 
            this.PendingApportionmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 309, true);
            // 
            // ContainersTabPage
            // 
            this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 342, true);
            // 
            // ContainersGroupBox
            // 
            this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 342, true);
            // 
            // CusContainerInvoiceLineGrid
            // 
            this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 325, true);
            // 
            // CantCreateInvoiceLinesLabel
            // 
            this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
            this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 534, true);
            // 
            // LineDetailsTabPage
            // 
            this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 342, true);
            // 
            // NewLineDetailsTabPage
            // 
            this.NewLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // InvoiceLineDetailsUserControl
            // 
            this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // ClassificationPanel
            // 
            this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 228, true);
            // 
            // CustomsInvoiceLinesBoundGrid
            // 
            this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 534, true);
            // 
            // CustomsQuantityCalcDropEdit
            // 
            this.CustomsQuantityCalcDropEdit.TabIndex = 11;
            // 
            // Splitter
            // 
            this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 210, true);
            // 
            // PreferenceCodeDropEdit
            // 
            this.PreferenceCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PreferenceCodeDropEdit, "FilteredInvoiceLines.JI_PrimaryPreference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PrimaryPreference)));
            this.PreferenceCodeDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|a93136a0-b403-4179-a190-2ef66c3f8895", "[36] Pref. Code");
            this.PreferenceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 42, true);
            this.PreferenceCodeDropEdit.Name = "PreferenceCodeDropEdit";
            this.PreferenceCodeDropEdit.PreBoundMaxLength = 3;
            this.PreferenceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 17, true);
            this.PreferenceCodeDropEdit.TabIndex = 7;
            // 
            // VatTypeDropEdit
            // 
            this.VatTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VatTypeDropEdit, "FilteredInvoiceLines.JI_ZZF_NKTaxType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ZZF_NKTaxType)));
            this.VatTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(729, 65, true);
            this.VatTypeDropEdit.Name = "VatTypeDropEdit";
            this.VatTypeDropEdit.PreBoundMaxLength = 4;
            this.VatTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
            this.VatTypeDropEdit.TabIndex = 15;
			this.VatTypeDropEdit.ShowDescriptionBox = !IsJI_TaxOrFeeDetailVisible;
			// 
			// VatDetailGuidDropEdit
			// 
			this.VatDetailGuidDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VatDetailGuidDropEdit, "FilteredInvoiceLines.JI_TaxOrFeeDetail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TaxOrFeeDetail)));
			this.VatDetailGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(800, 65, true);
            this.VatDetailGuidDropEdit.Name = "VatDetailGuidDropEdit";
            this.VatDetailGuidDropEdit.PreBoundMaxLength = 4;
            this.VatDetailGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.VatDetailGuidDropEdit.TabIndex = 15;
			// 
			// QuotaTextBox
			// 
			this.BindingSource.SetBindingMember(this.QuotaTextBox, "FilteredInvoiceLines.JI_ConcessionOrder");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ConcessionOrder)));
            this.QuotaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 94, true);
            this.QuotaTextBox.Name = "QuotaTextBox";
            this.QuotaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.QuotaTextBox.TabIndex = 13;
            this.QuotaTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // QuotaDropEdit
            // 
            this.QuotaDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.QuotaDropEdit, "FilteredInvoiceLines.JI_ConcessionOrder");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ConcessionOrder)));
            this.QuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 94, true);
            this.QuotaDropEdit.Name = "QuotaDropEdit";
            this.QuotaDropEdit.PreBoundMaxLength = 6;
            this.QuotaDropEdit.ShowDescriptionBox = false;
            this.QuotaDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.QuotaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.QuotaDropEdit.TabIndex = 13;
            // 
            // SecondQuotaTextBox
            // 
            this.BindingSource.SetBindingMember(this.SecondQuotaTextBox, "FilteredInvoiceLines.ZG_SecondQuota");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_SecondQuota)));
            this.SecondQuotaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 94, true);
            this.SecondQuotaTextBox.Name = "SecondQuotaTextBox";
            this.SecondQuotaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
            this.SecondQuotaTextBox.TabIndex = 14;
            this.SecondQuotaTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // SecondQuotaDropEdit
            // 
            this.SecondQuotaDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SecondQuotaDropEdit, "FilteredInvoiceLines.ZG_SecondQuota");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_SecondQuota)));
            this.SecondQuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 94, true);
            this.SecondQuotaDropEdit.Name = "SecondQuotaDropEdit";
            this.SecondQuotaDropEdit.PreBoundMaxLength = 6;
            this.SecondQuotaDropEdit.ShowDescriptionBox = false;
            this.SecondQuotaDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.SecondQuotaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
            this.SecondQuotaDropEdit.TabIndex = 14;
			// 
			// CountryOfSupplyCodeFindBox
			// 
			this.CountryOfSupplyCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CountryOfSupplyCodeFindBox, "FilteredInvoiceLines.ZG_CountryOfSupply");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_CountryOfSupply)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CountryList)));
            this.CountryOfSupplyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(799, 40, true);
            this.CountryOfSupplyCodeFindBox.Name = "CountryOfSupplyCodeFindBox";
            this.CountryOfSupplyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CountryOfSupplyCodeFindBox.ParentType = null;
            this.CountryOfSupplyCodeFindBox.PreBoundMaxLength = 2;
            this.CountryOfSupplyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
            this.CountryOfSupplyCodeFindBox.TabIndex = 9;
            // 
            // MethodOfPaymentDropEdit
            // 
            this.MethodOfPaymentDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MethodOfPaymentDropEdit, "FilteredInvoiceLines.ZG_MethodOfPayment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_MethodOfPayment)));
            this.MethodOfPaymentDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4aa1ae64-795c-4e97-a5aa-05bb63ca2469", "Method Of Payment");
            this.MethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(799, 89, true);
            this.MethodOfPaymentDropEdit.Name = "MethodOfPaymentDropEdit";
            this.MethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
            this.MethodOfPaymentDropEdit.TabIndex = 18;
            // 
            // ValuationMethodDropEdit
            // 
            this.ValuationMethodDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ValuationMethodDropEdit, "FilteredInvoiceLines.JI_ValuationCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ValuationCode)));
            this.ValuationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 120, true);
            this.ValuationMethodDropEdit.Name = "ValuationMethodDropEdit";
            this.ValuationMethodDropEdit.PreBoundMaxLength = 1;
            this.ValuationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 17, true);
            this.ValuationMethodDropEdit.TabIndex = 16;
            // 
            // ValuationAdjustmentPercentageCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ValuationAdjustmentPercentageCalcEdit, "FilteredInvoiceLines.JI_ValuationMarkup");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ValuationMarkup)));
            this.ValuationAdjustmentPercentageCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|9dfd52a1-58cc-4f5a-8c09-9d881fbbff13", "[45b] Valn. Adjt. Percent");
            this.ValuationAdjustmentPercentageCalcEdit.DecimalPlaces = 2;
            this.ValuationAdjustmentPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 146, true);
            this.ValuationAdjustmentPercentageCalcEdit.Name = "ValuationAdjustmentPercentageCalcEdit";
            this.ValuationAdjustmentPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
            this.ValuationAdjustmentPercentageCalcEdit.TabIndex = 20;
            this.ValuationAdjustmentPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ValuationAdjustmentPercentageCalcEdit.TrackDisposedAccess = true;
            // 
            // ValuationAdjustmentCodeDropEdit
            // 
            this.ValuationAdjustmentCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ValuationAdjustmentCodeDropEdit, "FilteredInvoiceLines.ZG_ValueAdjustmentCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_ValueAdjustmentCode)));
            this.ValuationAdjustmentCodeDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|1866256e-4988-4eaf-a6e5-a6080fe06a60", "[45a] Valn. Adjt. Code");
            this.ValuationAdjustmentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 146, true);
            this.ValuationAdjustmentCodeDropEdit.Name = "ValuationAdjustmentCodeDropEdit";
            this.ValuationAdjustmentCodeDropEdit.PreBoundMaxLength = 3;
            this.ValuationAdjustmentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 17, true);
            this.ValuationAdjustmentCodeDropEdit.TabIndex = 19;
            // 
            // CountryOfDestinationCodeFindBox
            // 
            this.CountryOfDestinationCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CountryOfDestinationCodeFindBox, "FilteredInvoiceLines.ZG_CountryOfDestination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_CountryOfDestination)));
            this.CountryOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 172, true);
            this.CountryOfDestinationCodeFindBox.Name = "CountryOfDestinationCodeFindBox";
            this.CountryOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CountryOfDestinationCodeFindBox.ParentType = null;
            this.CountryOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 17, true);
            this.CountryOfDestinationCodeFindBox.TabIndex = 25;
            // 
            // CheckQuotaBalanceLinkLabel
            // 
            this.CheckQuotaBalanceLinkLabel.AutoSize = true;
            this.CheckQuotaBalanceLinkLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("e6ba5280-b637-446d-8a03-66501d355454", "Check Quota");
            this.CheckQuotaBalanceLinkLabel.IsFontBold = false;
            this.CheckQuotaBalanceLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 96, true);
            this.CheckQuotaBalanceLinkLabel.Name = "CheckQuotaBalanceLinkLabel";
            this.CheckQuotaBalanceLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 13, true);
            this.CheckQuotaBalanceLinkLabel.TabIndex = 15;
            this.CheckQuotaBalanceLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CheckQuotaBalanceLinkLabel_LinkClicked);
            // 
            // CountryOfDispatchCodeFindBox
            // 
            this.CountryOfDispatchCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CountryOfDispatchCodeFindBox, "FilteredInvoiceLines.ZG_CountryOfDispatch");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_CountryOfDispatch)));
            this.CountryOfDispatchCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 198, true);
            this.CountryOfDispatchCodeFindBox.Name = "CountryOfDispatchCodeFindBox";
            this.CountryOfDispatchCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CountryOfDispatchCodeFindBox.ParentType = null;
            this.CountryOfDispatchCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 17, true);
            this.CountryOfDispatchCodeFindBox.TabIndex = 26;
            // 
            // EUImportInvoiceLineUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "EUImportInvoiceLineUserControl";
            this.packagesPivotUserControl.ResumeLayout(true);
            this.packagesPivotUserControl.PerformLayout();
            this.SupportingDocumentsTabPage.ResumeLayout(false);
            this.SupportingDocumentsTabPage.PerformLayout();
            this.InvoiceLinePaymentTabPage.ResumeLayout(false);
            this.InvoiceLinePaymentTabPage.PerformLayout();
            this.AdditionalInfosTabPage.ResumeLayout(false);
            this.AdditionalInfosTabPage.PerformLayout();
            this.PreviousDocumentsTabPage.ResumeLayout(false);
            this.PreviousDocumentsTabPage.PerformLayout();
            this.OrganizationsTabPage.ResumeLayout(false);
            this.OrganizationsTabPage.PerformLayout();
            this.ThirdQtyCalcDropEdit.ResumeLayout(true);
            this.ThirdQtyCalcDropEdit.PerformLayout();
            this.FourthQtyCalcDropEdit.ResumeLayout(true);
            this.FourthQtyCalcDropEdit.PerformLayout();
            this.PackagesPivotTabPage.ResumeLayout(false);
            this.PackagesPivotTabPage.PerformLayout();
            this.NetWeightCalcDropEdit.ResumeLayout(true);
            this.NetWeightCalcDropEdit.PerformLayout();
            this.SupplementaryCode2DropEdit.ResumeLayout(true);
            this.SupplementaryCode2DropEdit.PerformLayout();
            this.SupplementaryCode1DropEdit.ResumeLayout(true);
            this.SupplementaryCode1DropEdit.PerformLayout();
            this.CPCFindBox.ResumeLayout(true);
            this.CPCFindBox.PerformLayout();
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.PerformLayout();
            this.JI_CustomsQuantityCalcDropEdit.ResumeLayout(true);
            this.JI_CustomsQuantityCalcDropEdit.PerformLayout();
            this.tariffFindBox.ResumeLayout(true);
            this.tariffFindBox.PerformLayout();
            this.JI_CEIGuidDropEdit.ResumeLayout(true);
            this.JI_CEIGuidDropEdit.PerformLayout();
            this.StatisticalValueLocalCurrencyControl.ResumeLayout(true);
            this.StatisticalValueLocalCurrencyControl.PerformLayout();
            this.ValueForGstVatLocalCurrencyControl.ResumeLayout(true);
            this.ValueForGstVatLocalCurrencyControl.PerformLayout();
            this.CustomsValueLocalCurrencyControl.ResumeLayout(true);
            this.CustomsValueLocalCurrencyControl.PerformLayout();
            this.FiscalReferencesTabPage.ResumeLayout(false);
            this.FiscalReferencesTabPage.PerformLayout();
            this.goodsOriginDropEdit.ResumeLayout(true);
            this.goodsOriginDropEdit.PerformLayout();
            this.TransactionNatureDropEdit.ResumeLayout(true);
            this.TransactionNatureDropEdit.PerformLayout();
            this.AuthorisationsTabPage.ResumeLayout(false);
            this.AuthorisationsTabPage.PerformLayout();
            this.ValueIndicatorsTabPage.ResumeLayout(false);
            this.ValueIndicatorsTabPage.PerformLayout();
            this.SupplyChainActorTabPage.ResumeLayout(false);
            this.SupplyChainActorTabPage.PerformLayout();
            this.SupplyChainActorReferencesUserControl.ResumeLayout(true);
            this.SupplyChainActorReferencesUserControl.PerformLayout();
            this.invoiceLineValuationIndicatorsUserControl.ResumeLayout(true);
            this.invoiceLineValuationIndicatorsUserControl.PerformLayout();
            this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
            this.InvoiceLinesSummaryGroupBox.PerformLayout();
            this.BottomPanel.ResumeLayout(false);
            this.BottomPanel.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            this.LineDetailTabControl.ResumeLayout(false);
            this.LineDetailTabControl.PerformLayout();
            this.InvoiceDetailsGroupBox.ResumeLayout(false);
            this.InvoiceDetailsGroupBox.PerformLayout();
            this.JI_CountryOfOriginBoundFindBox.ResumeLayout(true);
            this.JI_CountryOfOriginBoundFindBox.PerformLayout();
            this.JI_RH_NKCommodity_CodeBoundFindBox.ResumeLayout(true);
            this.JI_RH_NKCommodity_CodeBoundFindBox.PerformLayout();
            this.JI_LinePriceBoundCurrencyControl.ResumeLayout(true);
            this.JI_LinePriceBoundCurrencyControl.PerformLayout();
            this.ClassificationDetailsGroupBox.ResumeLayout(false);
            this.ClassificationDetailsGroupBox.PerformLayout();
            this.LineChargesTabPage.ResumeLayout(false);
            this.LineChargesTabPage.PerformLayout();
            this.CurrentInvoicePanel.ResumeLayout(false);
            this.CurrentInvoicePanel.PerformLayout();
            this.LineSummaryPanel.ResumeLayout(false);
            this.LineSummaryPanel.PerformLayout();
            this.ContainersTabPage.ResumeLayout(false);
            this.ContainersTabPage.PerformLayout();
            this.ContainersGroupBox.ResumeLayout(false);
            this.ContainersGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
            this.CusContainerInvoiceLineGrid.ResumeLayout(false);
            this.CusContainerInvoiceLineGrid.PerformLayout();
            this.LineDetailsTabPage.ResumeLayout(false);
            this.LineDetailsTabPage.PerformLayout();
            this.NewLineDetailsTabPage.ResumeLayout(false);
            this.NewLineDetailsTabPage.PerformLayout();
            this.InvoiceLineDetailsUserControl.ResumeLayout(true);
            this.InvoiceLineDetailsUserControl.PerformLayout();
            this.ClassificationPanel.ResumeLayout(false);
            this.ClassificationPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
            this.CustomsInvoiceLinesBoundGrid.ResumeLayout(false);
            this.CustomsInvoiceLinesBoundGrid.PerformLayout();
            this.JI_Calc_CIFConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_CIFConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_FreightConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_FreightConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_FOBConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_FOBConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_GSTConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_GSTConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_DutyConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_DutyConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_BalanceConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_BalanceConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.PerformLayout();
            this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.PerformLayout();
            this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
            this.CustomsQuantityCalcDropEdit.PerformLayout();
            this.VolumeCalcDropEdit.ResumeLayout(true);
            this.VolumeCalcDropEdit.PerformLayout();
            this.JI_WeightCalcDropEdit.ResumeLayout(true);
            this.JI_WeightCalcDropEdit.PerformLayout();
            this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
            this.InvoiceQuantityCalcDropEdit.PerformLayout();
            this.JI_DescriptionBoundTextBox.ResumeLayout(true);
            this.JI_DescriptionBoundTextBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PreferenceCodeDropEdit.ResumeLayout(true);
            this.PreferenceCodeDropEdit.PerformLayout();
            this.VatTypeDropEdit.ResumeLayout(true);
            this.VatTypeDropEdit.PerformLayout();
            this.VatDetailGuidDropEdit.ResumeLayout(true);
            this.VatDetailGuidDropEdit.PerformLayout();
            this.QuotaDropEdit.ResumeLayout(true);
            this.QuotaDropEdit.PerformLayout();
            this.SecondQuotaDropEdit.ResumeLayout(true);
            this.SecondQuotaDropEdit.PerformLayout();
            this.CountryOfSupplyCodeFindBox.ResumeLayout(true);
            this.CountryOfSupplyCodeFindBox.PerformLayout();
            this.MethodOfPaymentDropEdit.ResumeLayout(true);
            this.MethodOfPaymentDropEdit.PerformLayout();
            this.ValuationMethodDropEdit.ResumeLayout(true);
            this.ValuationMethodDropEdit.PerformLayout();
            this.ValuationAdjustmentCodeDropEdit.ResumeLayout(true);
            this.ValuationAdjustmentCodeDropEdit.PerformLayout();
            this.CountryOfDestinationCodeFindBox.ResumeLayout(true);
            this.CountryOfDestinationCodeFindBox.PerformLayout();
            this.CountryOfDispatchCodeFindBox.ResumeLayout(true);
            this.CountryOfDispatchCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZDropEdit PreferenceCodeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit VatTypeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGuidDropEdit VatDetailGuidDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit QuotaDropEdit;
		protected Enterprise.ZArchitecture.ZTextBox QuotaTextBox;
		protected Enterprise.ZArchitecture.ZTextBox SecondQuotaTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit SecondQuotaDropEdit;
		private PlugIn.TaxUserControl TaxUserControl;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit ValuationMethodDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit ValuationAdjustmentCodeDropEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit ValuationAdjustmentPercentageCalcEdit;
		protected ZCodeFindBox CountryOfSupplyCodeFindBox;
		protected ZDropEdit MethodOfPaymentDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfDestinationCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZLinkLabel CheckQuotaBalanceLinkLabel;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfDispatchCodeFindBox;
	}
}
