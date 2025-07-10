using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class EUInvoiceLineUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>

		void InitializeComponent()
		{
            this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.SupportingDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.InvoiceLinePaymentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.invoiceLinePaymentUserControl1 = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.AdditionalInfosTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.additionalInfosUserControl1 = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.PreviousDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.StatisticalValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ThirdQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.FourthQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.PackagesPivotTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.packagesPivotUserControl = new Enterprise.Customs.GUI.BaseLineLevelPackingPivotControl();
            this.TaxTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.SupplementaryCode2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SupplementaryCode1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SupplementaryCode2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SupplementaryCode1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.JI_AdditionalSupplementsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AdditionalSupplementaryCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.CPCFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.StatValueManualOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.JI_CustomsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.zButtonMoreAdditionalProcedureCode = new Enterprise.ZArchitecture.GUI.ZButton();
            this.zTextBoxAddtionalProcedureCodeAsString = new Enterprise.ZArchitecture.ZTextBox();
            this.CustomsValueLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.ValueForGstVatLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.StatisticalValueLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.FiscalReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.FiscalReferencesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.goodsOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CommercialReferenceBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TransactionNatureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.VehicleTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.VehicleUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.AuthorisationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.AuthorisationsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.OrganizationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.organizationsUserControl1 = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.ValueIndicatorsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.invoiceLineValuationIndicatorsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.GDMLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.SupplyChainActorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.SupplyChainActorReferencesUserControl = new Enterprise.Customs.EU.GUI.PlugIn.SupplyChainActorReferencesUserControl();
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
            this.SupportingDocumentsTabPage.SuspendLayout();
            this.InvoiceLinePaymentTabPage.SuspendLayout();
            this.AdditionalInfosTabPage.SuspendLayout();
            this.PreviousDocumentsTabPage.SuspendLayout();
            this.ThirdQtyCalcDropEdit.SuspendLayout();
            this.FourthQtyCalcDropEdit.SuspendLayout();
            this.PackagesPivotTabPage.SuspendLayout();
            this.packagesPivotUserControl.SuspendLayout();
            this.SupplementaryCode2DropEdit.SuspendLayout();
            this.SupplementaryCode1DropEdit.SuspendLayout();
            this.NetWeightCalcDropEdit.SuspendLayout();
            this.CPCFindBox.SuspendLayout();
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.SuspendLayout();
            this.JI_CustomsQuantityCalcDropEdit.SuspendLayout();
            this.CustomsValueLocalCurrencyControl.SuspendLayout();
            this.ValueForGstVatLocalCurrencyControl.SuspendLayout();
            this.StatisticalValueLocalCurrencyControl.SuspendLayout();
            this.FiscalReferencesTabPage.SuspendLayout();
            this.goodsOriginDropEdit.SuspendLayout();
            this.TransactionNatureDropEdit.SuspendLayout();
            this.VehicleTabPage.SuspendLayout();
            this.AuthorisationsTabPage.SuspendLayout();
            this.OrganizationsTabPage.SuspendLayout();
            this.ValueIndicatorsTabPage.SuspendLayout();
            this.SupplyChainActorTabPage.SuspendLayout();
            this.SupplyChainActorReferencesUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // InvoiceLinesSummaryGroupBox
            // 
            this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(734, 0, true);
            this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 354, true);
            // 
            // BottomPanel
            // 
            this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
            this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 354, true);
            // 
            // TopPanel
            // 
            this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 534, true);
            // 
            // LineDetailTabControl
            // 
            this.LineDetailTabControl.Controls.Add(this.OrganizationsTabPage);
            this.LineDetailTabControl.Controls.Add(this.ValueIndicatorsTabPage);
            this.LineDetailTabControl.Controls.Add(this.AuthorisationsTabPage);
            this.LineDetailTabControl.Controls.Add(this.FiscalReferencesTabPage);
            this.LineDetailTabControl.Controls.Add(this.SupportingDocumentsTabPage);
            this.LineDetailTabControl.Controls.Add(this.AdditionalInfosTabPage);
            this.LineDetailTabControl.Controls.Add(this.PreviousDocumentsTabPage);
            this.LineDetailTabControl.Controls.Add(this.PackagesPivotTabPage);
            this.LineDetailTabControl.Controls.Add(this.TaxTabPage);
            this.LineDetailTabControl.Controls.Add(this.VehicleTabPage);
            this.LineDetailTabControl.Controls.Add(this.InvoiceLinePaymentTabPage);
            this.LineDetailTabControl.Controls.Add(this.SupplyChainActorTabPage);
            this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 354, true);
            this.LineDetailTabControl.Controls.SetChildIndex(this.SupplyChainActorTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.InvoiceLinePaymentTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.VehicleTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.TaxTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.PackagesPivotTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.PreviousDocumentsTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.AdditionalInfosTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.FiscalReferencesTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.AuthorisationsTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.ValueIndicatorsTabPage, 0);
            this.LineDetailTabControl.Controls.SetChildIndex(this.OrganizationsTabPage, 0);
            // 
            // InvoiceDetailsGroupBox
            // 
            this.InvoiceDetailsGroupBox.Controls.Add(this.goodsOriginDropEdit);
            this.InvoiceDetailsGroupBox.Controls.Add(this.NetWeightCalcDropEdit);
            this.InvoiceDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 114, true);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
            this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.goodsOriginDropEdit, 0);
            // 
            // JI_CountryOfOriginBoundFindBox
            // 
            this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 40, true);
            this.JI_CountryOfOriginBoundFindBox.TabIndex = 7;
            this.JI_CountryOfOriginBoundFindBox.Visible = false;
            // 
            // JI_RH_NKCommodity_CodeBoundFindBox
            // 
            this.JI_RH_NKCommodity_CodeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 89, true);
            this.JI_RH_NKCommodity_CodeBoundFindBox.TabIndex = 15;
            // 
            // JI_LinePriceBoundCurrencyControl
            // 
            this.JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 40, true);
            this.JI_LinePriceBoundCurrencyControl.TabIndex = 5;
            // 
            // ClassificationDetailsGroupBox
            // 
            this.ClassificationDetailsGroupBox.Controls.Add(this.TransactionNatureDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.CommercialReferenceBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBoxAddtionalProcedureCodeAsString);
            this.ClassificationDetailsGroupBox.Controls.Add(this.zButtonMoreAdditionalProcedureCode);
            this.ClassificationDetailsGroupBox.Controls.Add(this.StatValueManualOverrideCheckBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.CPCFindBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.FourthQtyCalcDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.GDMLink);
            this.ClassificationDetailsGroupBox.Controls.Add(this.ThirdQtyCalcDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.SupplementaryCode2TextBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.SupplementaryCode1TextBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.SupplementaryCode2DropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.SupplementaryCode1DropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.StatisticalValueCalcEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.JI_CustomsQuantityCalcDropEdit);
            this.ClassificationDetailsGroupBox.Controls.Add(this.zLabel2);
            this.ClassificationDetailsGroupBox.Controls.Add(this.zLabel5);
            this.ClassificationDetailsGroupBox.Controls.Add(this.zLabel4);
            this.ClassificationDetailsGroupBox.Controls.Add(this.JI_AdditionalSupplementsTextBox);
            this.ClassificationDetailsGroupBox.Controls.Add(this.AdditionalSupplementaryCodesEditButton);
            this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 218, true);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.BondedWHSOrderLineNumberCalcEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.BondedWHSOrderNumberTextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.AdditionalSupplementaryCodesEditButton, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_AdditionalSupplementsTextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zLabel4, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zLabel5, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zLabel2, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_CustomsQuantityCalcDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.StatisticalValueCalcEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode1DropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode2DropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode1TextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode2TextBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ThirdQtyCalcDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.GDMLink, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.FourthQtyCalcDropEdit, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CPCFindBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.StatValueManualOverrideCheckBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zButtonMoreAdditionalProcedureCode, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBoxAddtionalProcedureCodeAsString, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CommercialReferenceBox, 0);
            this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.TransactionNatureDropEdit, 0);
            // 
            // LineChargesTabPage
            // 
            this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // LineSummaryPanel
            // 
            this.LineSummaryPanel.Controls.Add(this.CustomsValueLocalCurrencyControl);
            this.LineSummaryPanel.Controls.Add(this.StatisticalValueLocalCurrencyControl);
            this.LineSummaryPanel.Controls.Add(this.ValueForGstVatLocalCurrencyControl);
            this.LineSummaryPanel.Controls.Add(this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl);
            this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 251, true);
            this.LineSummaryPanel.Controls.SetChildIndex(this.oLabel8, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.ValueForGstVatLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.StatisticalValueLocalCurrencyControl, 0);
            this.LineSummaryPanel.Controls.SetChildIndex(this.CustomsValueLocalCurrencyControl, 0);
            // 
            // ContainersTabPage
            // 
            this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // ContainersGroupBox
            // 
            this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            // 
            // CusContainerInvoiceLineGrid
            // 
            this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 315, true);
            // 
            // CantCreateInvoiceLinesLabel
            // 
            this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
            this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 534, true);
            // 
            // LineDetailsTabPage
            // 
            this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
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
            this.ClassificationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClassificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 114, true);
            this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 218, true);
            // 
            // CustomsInvoiceLinesBoundGrid
            // 
            this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 534, true);
            // 
            // JI_Calc_CIFConvertToLocalCurrencyControl
            // 
            this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 164, true);
            // 
            // JI_Calc_InsuranceConvertToLocalCurrencyControl
            // 
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 140, true);
            this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Visible = false;
            // 
            // JI_Calc_FreightConvertToLocalCurrencyControl
            // 
            this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 116, true);
            this.JI_Calc_FreightConvertToLocalCurrencyControl.Visible = false;
            // 
            // JI_Calc_FOBConvertToLocalCurrencyControl
            // 
            this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 92, true);
            // 
            // JI_Calc_GSTConvertToLocalCurrencyControl
            // 
            this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 41, true);
            // 
            // JI_Calc_DutyConvertToLocalCurrencyControl
            // 
            this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 17, true);
            // 
            // CustomsQuantityCalcDropEdit
            // 
            this.CustomsQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsSecondQuantity";
            this.CustomsQuantityCalcDropEdit.BindToList = "";
            this.CustomsQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsSecondUnitQty";
            this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 68, true);
            this.CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
            this.CustomsQuantityCalcDropEdit.TabIndex = 14;
            this.CustomsQuantityCalcDropEdit.UnitPreBoundMaxLength = 4;
            // 
            // VolumeCalcDropEdit
            // 
            this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 65, true);
            this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
            this.VolumeCalcDropEdit.TabIndex = 10;
            // 
            // JI_WeightCalcDropEdit
            // 
            this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 65, true);
            this.JI_WeightCalcDropEdit.TabIndex = 12;
            // 
            // InvoiceQuantityCalcDropEdit
            // 
            this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 40, true);
            // 
            // JI_DescriptionBoundTextBox
            // 
            this.JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 16, true);
            this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 21, true);
            // 
            // Splitter
            // 
            this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 220, true);
            this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 10, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // SupportingDocumentsTabPage
            // 
            this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsUserControl);
            this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
            this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.SupportingDocumentsTabPage.TabIndex = 3;
            // 
            // SupportingDocumentsUserControl
            // 
            this.SupportingDocumentsUserControl.AllowDrop = true;
            this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
            this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.SupportingDocumentsUserControl.TabIndex = 0;
            // 
            // InvoiceLinePaymentTabPage
            // 
            this.InvoiceLinePaymentTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|F2E181A3-6F5C-4D92-AE17-4F8FE7EDDCEC", "Payment");
            this.InvoiceLinePaymentTabPage.Controls.Add(this.invoiceLinePaymentUserControl1);
            this.InvoiceLinePaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.InvoiceLinePaymentTabPage.Name = "InvoiceLinePaymentTabPage";
            this.InvoiceLinePaymentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.InvoiceLinePaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            this.InvoiceLinePaymentTabPage.TabIndex = 2;
            this.InvoiceLinePaymentTabPage.UseVisualStyleBackColor = true;
            // 
            // invoiceLinePaymentUserControl1
            // 
            this.invoiceLinePaymentUserControl1.AllowDrop = true;
            this.invoiceLinePaymentUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.invoiceLinePaymentUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.invoiceLinePaymentUserControl1.Name = "invoiceLinePaymentUserControl1";
            this.invoiceLinePaymentUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 327, true);
            this.invoiceLinePaymentUserControl1.TabIndex = 1;
            // 
            // AdditionalInfosTabPage
            // 
            this.AdditionalInfosTabPage.Controls.Add(this.additionalInfosUserControl1);
            this.AdditionalInfosTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.AdditionalInfosTabPage.Name = "AdditionalInfosTabPage";
            this.AdditionalInfosTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.AdditionalInfosTabPage.TabIndex = 4;
            // 
            // additionalInfosUserControl1
            // 
            this.additionalInfosUserControl1.AllowDrop = true;
            this.additionalInfosUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.additionalInfosUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.additionalInfosUserControl1.Name = "additionalInfosUserControl1";
            this.additionalInfosUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.additionalInfosUserControl1.TabIndex = 0;
            // 
            // PreviousDocumentsTabPage
            // 
            this.PreviousDocumentsTabPage.Controls.Add(this.PreviousDocumentsUserControl);
            this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
            this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.PreviousDocumentsTabPage.TabIndex = 5;
            // 
            // PreviousDocumentsUserControl
            // 
            this.PreviousDocumentsUserControl.AllowDrop = true;
            this.PreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.PreviousDocumentsUserControl.Name = "PreviousDocumentsUserControl";
            this.PreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.PreviousDocumentsUserControl.TabIndex = 0;
            // 
            // StatisticalValueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.StatisticalValueCalcEdit, "FilteredInvoiceLines.ZG_StatisticalValue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_StatisticalValue)));
            this.StatisticalValueCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|ed2137f3-1bd7-41e7-b045-f058126ef751", "[46] Stat. Value");
            this.StatisticalValueCalcEdit.DecimalPlaces = 5;
            this.StatisticalValueCalcEdit.Decimals = 5;
            this.StatisticalValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 120, true);
            this.StatisticalValueCalcEdit.Name = "StatisticalValueCalcEdit";
            this.StatisticalValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 17, true);
            this.StatisticalValueCalcEdit.TabIndex = 19;
            this.StatisticalValueCalcEdit.Text = "0.00";
            this.StatisticalValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.StatisticalValueCalcEdit.TrackDisposedAccess = true;
            // 
            // ThirdQtyCalcDropEdit
            // 
            this.ThirdQtyCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ThirdQtyCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdUnitQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
            this.ThirdQtyCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsThirdQuantity";
            this.ThirdQtyCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+CustomsUQList";
            this.ThirdQtyCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsThirdUnitQty";
            this.ThirdQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 94, true);
            this.ThirdQtyCalcDropEdit.Name = "ThirdQtyCalcDropEdit";
            this.ThirdQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
            this.ThirdQtyCalcDropEdit.TabIndex = 17;
            this.ThirdQtyCalcDropEdit.UnitPreBoundMaxLength = 4;
            // 
            // FourthQtyCalcDropEdit
            // 
            this.FourthQtyCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FourthQtyCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsFourthQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsFourthUnitQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
            this.FourthQtyCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsFourthQuantity";
            this.FourthQtyCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+CustomsUQList";
            this.FourthQtyCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsFourthUnitQty";
            this.FourthQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 120, true);
            this.FourthQtyCalcDropEdit.Name = "FourthQtyCalcDropEdit";
            this.FourthQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
            this.FourthQtyCalcDropEdit.TabIndex = 18;
            this.FourthQtyCalcDropEdit.UnitPreBoundMaxLength = 4;
            this.FourthQtyCalcDropEdit.Visible = false;
            // 
            // PackagesPivotTabPage
            // 
            this.PackagesPivotTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("8A8FD82F-3D9E-4193-83D1-D0B28F71FC90", "[31] Packages");
            this.PackagesPivotTabPage.Controls.Add(this.packagesPivotUserControl);
            this.PackagesPivotTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.PackagesPivotTabPage.Name = "PackagesPivotTabPage";
            this.PackagesPivotTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.PackagesPivotTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.PackagesPivotTabPage.TabIndex = 6;
            // 
            // packagesPivotUserControl
            // 
            this.packagesPivotUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.packagesPivotUserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.packagesPivotUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packagesPivotUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.packagesPivotUserControl.Name = "packagesPivotUserControl";
            this.packagesPivotUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 327, true);
            this.packagesPivotUserControl.TabIndex = 0;
            // 
            // TaxTabPage
            // 
            this.TaxTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("C261E664-7ED9-444C-BEF0-CC414F106C95", "[47] Tax");
            this.TaxTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.TaxTabPage.Name = "TaxTabPage";
            this.TaxTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.TaxTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.TaxTabPage.TabIndex = 7;
            // 
            // SupplementaryCode2DropEdit
            // 
            this.SupplementaryCode2DropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupplementaryCode2DropEdit, "FilteredInvoiceLines.JI_SupplementaryCode2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_SupplementaryCode2)));
            this.SupplementaryCode2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 16, true);
            this.SupplementaryCode2DropEdit.Name = "SupplementaryCode2DropEdit";
            this.SupplementaryCode2DropEdit.PreBoundMaxLength = 3;
            this.SupplementaryCode2DropEdit.ShouldResizeByMaxLength = false;
            this.SupplementaryCode2DropEdit.ShowDescriptionBox = false;
            this.SupplementaryCode2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
            this.SupplementaryCode2DropEdit.TabIndex = 3;
            // 
            // SupplementaryCode1DropEdit
            // 
            this.SupplementaryCode1DropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupplementaryCode1DropEdit, "FilteredInvoiceLines.JI_SupplementaryCode1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_SupplementaryCode1)));
            this.SupplementaryCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 16, true);
            this.SupplementaryCode1DropEdit.Name = "SupplementaryCode1DropEdit";
            this.SupplementaryCode1DropEdit.PreBoundMaxLength = 3;
            this.SupplementaryCode1DropEdit.ShouldResizeByMaxLength = false;
            this.SupplementaryCode1DropEdit.ShowDescriptionBox = false;
            this.SupplementaryCode1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
            this.SupplementaryCode1DropEdit.TabIndex = 2;
            // 
            // SupplementaryCode2TextBox
            // 
            this.SupplementaryCode2TextBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupplementaryCode2TextBox, "FilteredInvoiceLines.JI_SupplementaryCode2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_SupplementaryCode2)));
            this.SupplementaryCode2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 16, true);
            this.SupplementaryCode2TextBox.Name = "SupplementaryCode2TextBox";
            this.SupplementaryCode2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
            this.SupplementaryCode2TextBox.TabIndex = 3;
            // 
            // SupplementaryCode1TextBox
            // 
            this.SupplementaryCode1TextBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupplementaryCode1TextBox, "FilteredInvoiceLines.JI_SupplementaryCode1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_SupplementaryCode1)));
            this.SupplementaryCode1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 16, true);
            this.SupplementaryCode1TextBox.Name = "SupplementaryCode1TextBox";
            this.SupplementaryCode1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
            this.SupplementaryCode1TextBox.TabIndex = 2;
            // 
            // JI_AdditionalSupplementsTextBox
            // 
            this.BindingSource.SetBindingMember(this.JI_AdditionalSupplementsTextBox, "FilteredInvoiceLines.JI_AdditionalSupplements");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AdditionalSupplements)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JI_AdditionalSupplementsTextBox, false);
            this.JI_AdditionalSupplementsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 16, true);
            this.JI_AdditionalSupplementsTextBox.Name = "JI_AdditionalSupplementsTextBox";
            this.JI_AdditionalSupplementsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
            this.JI_AdditionalSupplementsTextBox.TabIndex = 21;
            this.JI_AdditionalSupplementsTextBox.TabStop = false;
            // 
            // AdditionalSupplementaryCodesEditButton
            // 
            this.AdditionalSupplementaryCodesEditButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("AdditionalSupplementaryCodesEditButton|DACAAF16-DC3A-45FA-961A-DFCE774C64D1", "More...");
            this.AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 14, true);
            this.AdditionalSupplementaryCodesEditButton.Name = "AdditionalSupplementaryCodesEditButton";
            this.AdditionalSupplementaryCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
            this.AdditionalSupplementaryCodesEditButton.TabIndex = 4;
            this.AdditionalSupplementaryCodesEditButton.ToolTipCaption = null;
            this.AdditionalSupplementaryCodesEditButton.Click += new System.EventHandler(this.AdditionalSupplementaryCodesEditButton_Click);
            // 
            // NetWeightCalcDropEdit
            // 
            this.NetWeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NetWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NetWeightUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.WeightUQList)));
            this.NetWeightCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_NetWeight";
            this.NetWeightCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+WeightUQList";
            this.NetWeightCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_NetWeightUQ";
            this.NetWeightCalcDropEdit.Decimals = 3;
            this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 65, true);
            this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
            this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 17, true);
            this.NetWeightCalcDropEdit.TabIndex = 14;
            this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // CPCFindBox
            // 
            this.CPCFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CPCFindBox, "FilteredInvoiceLines.JI_FormattedProcedure");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedProcedure)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CPCList)));
            this.CPCFindBox.BindToList = "FilteredInvoiceLines.Lookups+CPCList";
            this.CPCFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 16, true);
            this.CPCFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
            this.CPCFindBox.Name = "CPCFindBox";
            this.CPCFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CPCFindBox.ParentType = null;
            this.CPCFindBox.PreBoundMaxLength = 8;
            this.CPCFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 17, true);
            this.CPCFindBox.TabIndex = 10;
            // 
            // StatValueManualOverrideCheckBox
            // 
            this.StatValueManualOverrideCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.StatValueManualOverrideCheckBox, "FilteredInvoiceLines.ZG_StatisticalValueManualOverride");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_StatisticalValueManualOverride)));
            this.StatValueManualOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(779, 122, true);
            this.StatValueManualOverrideCheckBox.Name = "StatValueManualOverrideCheckBox";
            this.StatValueManualOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.StatValueManualOverrideCheckBox.TabIndex = 20;
            this.StatValueManualOverrideCheckBox.UseVisualStyleBackColor = true;
            // 
            // zLabel2
            // 
            this.zLabel2.AutoSize = true;
            this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 19, true);
            this.zLabel2.Name = "zLabel2";
            this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(9, 13, true);
            this.zLabel2.TabIndex = 21;
            this.zLabel2.Text = "/";
            // 
            // zLabel4
            // 
            this.zLabel4.AutoSize = true;
            this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 19, true);
            this.zLabel4.Name = "zLabel4";
            this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(9, 13, true);
            this.zLabel4.TabIndex = 22;
            this.zLabel4.Text = "/";
            this.zLabel4.UseMnemonic = false;
            // 
            // zLabel5
            // 
            this.zLabel5.AutoSize = true;
            this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 19, true);
            this.zLabel5.Name = "zLabel5";
            this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(9, 13, true);
            this.zLabel5.TabIndex = 22;
            this.zLabel5.Text = "/";
            this.zLabel5.UseMnemonic = false;
            // 
            // JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl
            // 
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.AllowDrop = true;
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.BackColor = System.Drawing.SystemColors.Control;
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_GSTVATDeferred";
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|0ddf8805-5203-429c-b2a4-ea4145a0f927", "Def. VAT");
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64, true);
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.Name = "JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl";
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.TabIndex = 14;
            // 
            // JI_CustomsQuantityCalcDropEdit
            // 
            this.JI_CustomsQuantityCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.JI_CustomsQuantityCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsUnitQty)));
            this.JI_CustomsQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsQuantity";
            this.JI_CustomsQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsUnitQty";
            this.JI_CustomsQuantityCalcDropEdit.Decimals = 3;
            this.JI_CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 42, true);
            this.JI_CustomsQuantityCalcDropEdit.Name = "JI_CustomsQuantityCalcDropEdit";
            this.JI_CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
            this.JI_CustomsQuantityCalcDropEdit.TabIndex = 11;
            this.JI_CustomsQuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // zButtonMoreAdditionalProcedureCode
            // 
            this.zButtonMoreAdditionalProcedureCode.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c1cbdb14-7746-4f12-9ecf-abe80b139d55", "More...");
            this.zButtonMoreAdditionalProcedureCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(852, 146, true);
            this.zButtonMoreAdditionalProcedureCode.Name = "zButtonMoreAdditionalProcedureCode";
            this.zButtonMoreAdditionalProcedureCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
            this.zButtonMoreAdditionalProcedureCode.TabIndex = 24;
            this.zButtonMoreAdditionalProcedureCode.ToolTipCaption = null;
            this.zButtonMoreAdditionalProcedureCode.Click += new System.EventHandler(this.ZButtonMoreAdditionalProcedureCode_Click);
            // 
            // zTextBoxAddtionalProcedureCodeAsString
            // 
            this.BindingSource.SetBindingMember(this.zTextBoxAddtionalProcedureCodeAsString, "FilteredInvoiceLines.AdditionalProcedureCodesAsString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalProcedureCodesAsString)));
            this.zTextBoxAddtionalProcedureCodeAsString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 146, true);
            this.zTextBoxAddtionalProcedureCodeAsString.Name = "zTextBoxAddtionalProcedureCodeAsString";
            this.zTextBoxAddtionalProcedureCodeAsString.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 17, true);
            this.zTextBoxAddtionalProcedureCodeAsString.TabIndex = 23;
            // 
            // CustomsValueLocalCurrencyControl
            // 
            this.CustomsValueLocalCurrencyControl.AllowDrop = true;
            this.CustomsValueLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_CustomsValue";
            this.CustomsValueLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
            this.CustomsValueLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
            this.CustomsValueLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|CustomsValueLocalCurrencyControl", "Customs Value");
            this.CustomsValueLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 149, true);
            this.CustomsValueLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.CustomsValueLocalCurrencyControl.Name = "CustomsValueLocalCurrencyControl";
            this.CustomsValueLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
            this.CustomsValueLocalCurrencyControl.TabIndex = 14;
            // 
            // ValueForGstVatLocalCurrencyControl
            // 
            this.ValueForGstVatLocalCurrencyControl.AllowDrop = true;
            this.ValueForGstVatLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_ValueForVat";
            this.ValueForGstVatLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
            this.ValueForGstVatLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
            this.ValueForGstVatLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|ValueForGstVatLocalCurrencyControl", "VAT Value");
            this.ValueForGstVatLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 291, true);
            this.ValueForGstVatLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.ValueForGstVatLocalCurrencyControl.Name = "ValueForGstVatLocalCurrencyControl";
            this.ValueForGstVatLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
            this.ValueForGstVatLocalCurrencyControl.TabIndex = 16;
            // 
            // StatisticalValueLocalCurrencyControl
            // 
            this.StatisticalValueLocalCurrencyControl.AllowDrop = true;
            this.StatisticalValueLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_StatisticalValue";
            this.StatisticalValueLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
            this.StatisticalValueLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
            this.StatisticalValueLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("EUInvoiceLineUserControl|StatisticalValueLocalCurrencyControl", "Stat. Value");
            this.StatisticalValueLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 170, true);
            this.StatisticalValueLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
            this.StatisticalValueLocalCurrencyControl.Name = "StatisticalValueLocalCurrencyControl";
            this.StatisticalValueLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
            this.StatisticalValueLocalCurrencyControl.TabIndex = 15;
            // 
            // FiscalReferencesTabPage
            // 
            this.FiscalReferencesTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1b1324a0-eae8-4b0d-9d52-6bf4c3b8b482", "Fiscal References");
            this.FiscalReferencesTabPage.Controls.Add(this.FiscalReferencesUserControl);
            this.FiscalReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.FiscalReferencesTabPage.Name = "FiscalReferencesTabPage";
            this.FiscalReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            this.FiscalReferencesTabPage.TabIndex = 8;
            // 
            // FiscalReferencesUserControl
            // 
            this.FiscalReferencesUserControl.AllowDrop = true;
            this.FiscalReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FiscalReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FiscalReferencesUserControl.Name = "FiscalReferencesUserControl";
            this.FiscalReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            this.FiscalReferencesUserControl.TabIndex = 1;
            // 
            // goodsOriginDropEdit
            // 
            this.goodsOriginDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.goodsOriginDropEdit, "FilteredInvoiceLines.JI_CountryOfOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CountryOfOrigin)));
            this.goodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 40, true);
            this.goodsOriginDropEdit.Name = "goodsOriginDropEdit";
            this.goodsOriginDropEdit.PreBoundMaxLength = 3;
            this.goodsOriginDropEdit.ShouldResizeByMaxLength = false;
            this.goodsOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 17, true);
            this.goodsOriginDropEdit.TabIndex = 7;
            // 
            // CommercialReferenceBox
            // 
            this.BindingSource.SetBindingMember(this.CommercialReferenceBox, "FilteredInvoiceLines.ZG_CommercialReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_CommercialReference)));
            this.CommercialReferenceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 94, true);
            this.CommercialReferenceBox.Name = "CommercialReferenceBox";
            this.CommercialReferenceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 17, true);
            this.CommercialReferenceBox.TabIndex = 25;
            // 
            // TransactionNatureDropEdit
            // 
            this.TransactionNatureDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransactionNatureDropEdit, "FilteredInvoiceLines.ZG_TransNature");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_TransNature)));
            this.TransactionNatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 68, true);
            this.TransactionNatureDropEdit.Name = "TransactionNatureDropEdit";
            this.TransactionNatureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 17, true);
            this.TransactionNatureDropEdit.TabIndex = 26;
            // 
            // VehicleTabPage
            // 
            this.VehicleTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("2fddc87c-0f3a-4352-8d7b-e2352f1318d0", "Vehicle/Engine");
            this.VehicleTabPage.Controls.Add(this.VehicleUserControl);
            this.VehicleTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.VehicleTabPage.Name = "VehicleTabPage";
            this.VehicleTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.VehicleTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.VehicleTabPage.TabIndex = 9;
            // 
            // VehicleUserControl
            // 
            this.VehicleUserControl.AllowDrop = true;
            this.VehicleUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VehicleUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.VehicleUserControl.Name = "VehicleUserControl";
            this.VehicleUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 327, true);
            this.VehicleUserControl.TabIndex = 1;
            // 
            // AuthorisationsTabPage
            // 
            this.AuthorisationsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("DE8B214F-0904-4BDA-B100-F441C179C557", "Authorizations");
            this.AuthorisationsTabPage.Controls.Add(this.AuthorisationsUserControl);
            this.AuthorisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.AuthorisationsTabPage.Name = "AuthorisationsTabPage";
            this.AuthorisationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.AuthorisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 332, true);
            this.AuthorisationsTabPage.TabIndex = 1;
            this.AuthorisationsTabPage.UseVisualStyleBackColor = true;
            // 
            // AuthorisationsUserControl
            // 
            this.AuthorisationsUserControl.AllowDrop = true;
            this.AuthorisationsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AuthorisationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.AuthorisationsUserControl.Name = "AuthorisationsUserControl";
            this.AuthorisationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 327, true);
            this.AuthorisationsUserControl.TabIndex = 0;
            // 
            // OrganizationsTabPage
            // 
            this.OrganizationsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("28EAF700-68AD-4CEF-939B-4912C956F666", "Organizations");
            this.OrganizationsTabPage.Controls.Add(this.organizationsUserControl1);
            this.OrganizationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.OrganizationsTabPage.Name = "OrganizationsTabPage";
            this.OrganizationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.OrganizationsTabPage.TabIndex = 7;
            // 
            // organizationsUserControl1
            // 
            this.organizationsUserControl1.AllowDrop = true;
            this.organizationsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.organizationsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.organizationsUserControl1.Name = "organizationsUserControl1";
            this.organizationsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.organizationsUserControl1.TabIndex = 0;
            // 
            // ValueIndicatorsTabPage
            // 
            this.ValueIndicatorsTabPage.Controls.Add(this.invoiceLineValuationIndicatorsUserControl);
            this.ValueIndicatorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.ValueIndicatorsTabPage.Name = "ValueIndicatorsTabPage";
            this.ValueIndicatorsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.ValueIndicatorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.ValueIndicatorsTabPage.TabIndex = 101;
            // 
            // invoiceLineValuationIndicatorsUserControl
            // 
            this.invoiceLineValuationIndicatorsUserControl.AllowDrop = true;
            this.invoiceLineValuationIndicatorsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.invoiceLineValuationIndicatorsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.invoiceLineValuationIndicatorsUserControl.Name = "invoiceLineValuationIndicatorsUserControl";
            this.invoiceLineValuationIndicatorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 327, true);
            this.invoiceLineValuationIndicatorsUserControl.TabIndex = 0;
            // 
            // GDMLink
            // 
            this.GDMLink.AutoSize = true;
            this.GDMLink.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("77ec5abd-685f-49db-b96e-81a25f1897a7", "GDM");
            this.GDMLink.IsFontBold = false;
            this.GDMLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(561, 19, true);
            this.GDMLink.Name = "GDMLink";
            this.GDMLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 13, true);
            this.GDMLink.TabIndex = 12;
            this.GDMLink.TabStop = false;
            this.GDMLink.Click += new System.EventHandler(this.GDMLink_Clicked);
            // 
            // SupplyChainActorTabPage
            // 
            this.SupplyChainActorTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("154eca5a-7cee-40ca-b2b5-9a6e94cb964d", "Add. Supply Chain Actors");
            this.SupplyChainActorTabPage.Controls.Add(this.SupplyChainActorReferencesUserControl);
            this.SupplyChainActorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
            this.SupplyChainActorTabPage.Name = "SupplyChainActorTabPage";
            this.SupplyChainActorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.SupplyChainActorTabPage.TabIndex = 102;
            // 
            // SupplyChainActorReferencesUserControl
            // 
            this.SupplyChainActorReferencesUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupplyChainActorReferencesUserControl, "FilteredInvoiceLines.CusSupplyChainActorReferences");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference>)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CusSupplyChainActorReferences)));
            this.SupplyChainActorReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SupplyChainActorReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SupplyChainActorReferencesUserControl.Name = "SupplyChainActorReferencesUserControl";
            this.SupplyChainActorReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 332, true);
            this.SupplyChainActorReferencesUserControl.TabIndex = 0;
            // 
            // EUInvoiceLineUserControl
            // 
            this.Name = "EUInvoiceLineUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 584, true);
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
            this.SupportingDocumentsTabPage.ResumeLayout(false);
            this.SupportingDocumentsTabPage.PerformLayout();
            this.InvoiceLinePaymentTabPage.ResumeLayout(false);
            this.InvoiceLinePaymentTabPage.PerformLayout();
            this.AdditionalInfosTabPage.ResumeLayout(false);
            this.AdditionalInfosTabPage.PerformLayout();
            this.PreviousDocumentsTabPage.ResumeLayout(false);
            this.PreviousDocumentsTabPage.PerformLayout();
            this.ThirdQtyCalcDropEdit.ResumeLayout(true);
            this.ThirdQtyCalcDropEdit.PerformLayout();
            this.FourthQtyCalcDropEdit.ResumeLayout(true);
            this.FourthQtyCalcDropEdit.PerformLayout();
            this.PackagesPivotTabPage.ResumeLayout(false);
            this.PackagesPivotTabPage.PerformLayout();
            this.packagesPivotUserControl.ResumeLayout(true);
            this.packagesPivotUserControl.PerformLayout();
            this.SupplementaryCode2DropEdit.ResumeLayout(true);
            this.SupplementaryCode2DropEdit.PerformLayout();
            this.SupplementaryCode1DropEdit.ResumeLayout(true);
            this.SupplementaryCode1DropEdit.PerformLayout();
            this.NetWeightCalcDropEdit.ResumeLayout(true);
            this.NetWeightCalcDropEdit.PerformLayout();
            this.CPCFindBox.ResumeLayout(true);
            this.CPCFindBox.PerformLayout();
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.ResumeLayout(true);
            this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.PerformLayout();
            this.JI_CustomsQuantityCalcDropEdit.ResumeLayout(true);
            this.JI_CustomsQuantityCalcDropEdit.PerformLayout();
            this.CustomsValueLocalCurrencyControl.ResumeLayout(true);
            this.CustomsValueLocalCurrencyControl.PerformLayout();
            this.ValueForGstVatLocalCurrencyControl.ResumeLayout(true);
            this.ValueForGstVatLocalCurrencyControl.PerformLayout();
            this.StatisticalValueLocalCurrencyControl.ResumeLayout(true);
            this.StatisticalValueLocalCurrencyControl.PerformLayout();
            this.FiscalReferencesTabPage.ResumeLayout(false);
            this.FiscalReferencesTabPage.PerformLayout();
            this.goodsOriginDropEdit.ResumeLayout(true);
            this.goodsOriginDropEdit.PerformLayout();
            this.TransactionNatureDropEdit.ResumeLayout(true);
            this.TransactionNatureDropEdit.PerformLayout();
            this.VehicleTabPage.ResumeLayout(false);
            this.VehicleTabPage.PerformLayout();
            this.AuthorisationsTabPage.ResumeLayout(false);
            this.AuthorisationsTabPage.PerformLayout();
            this.OrganizationsTabPage.ResumeLayout(false);
            this.OrganizationsTabPage.PerformLayout();
            this.ValueIndicatorsTabPage.ResumeLayout(false);
            this.ValueIndicatorsTabPage.PerformLayout();
            this.SupplyChainActorTabPage.ResumeLayout(false);
            this.SupplyChainActorTabPage.PerformLayout();
            this.SupplyChainActorReferencesUserControl.ResumeLayout(true);
            this.SupplyChainActorReferencesUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected Customs.GUI.BaseLineLevelPackingPivotControl packagesPivotUserControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage InvoiceLinePaymentTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalInfosTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage OrganizationsTabPage;
		protected Enterprise.ZArchitecture.ZCalcEdit StatisticalValueCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit ThirdQtyCalcDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit FourthQtyCalcDropEdit;
		public Enterprise.ZArchitecture.GUI.ZTabPage PackagesPivotTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage TaxTabPage;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit SupplementaryCode2DropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit SupplementaryCode1DropEdit;
		protected Enterprise.ZArchitecture.ZTextBox SupplementaryCode2TextBox;
		protected Enterprise.ZArchitecture.ZTextBox SupplementaryCode1TextBox;
		protected Enterprise.ZArchitecture.ZTextBox JI_AdditionalSupplementsTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton AdditionalSupplementaryCodesEditButton;
		public Enterprise.ZArchitecture.GUI.ZCodeFindBox CPCFindBox;
		ZArchitecture.GUI.ZDynamicControlCreationUserControl SupportingDocumentsUserControl;
		private Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl invoiceLinePaymentUserControl1;
		ZArchitecture.GUI.ZDynamicControlCreationUserControl PreviousDocumentsUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl organizationsUserControl1;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox StatValueManualOverrideCheckBox;
		protected Enterprise.ZArchitecture.ZLabel zLabel2;
		protected Enterprise.ZArchitecture.ZLabel zLabel4;
		protected Enterprise.ZArchitecture.ZLabel zLabel5;
		protected Enterprise.Customs.GUI.ConvertToLocalCurrencyControl JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit JI_CustomsQuantityCalcDropEdit;
		protected ZButton zButtonMoreAdditionalProcedureCode;
		protected ZArchitecture.ZTextBox zTextBoxAddtionalProcedureCodeAsString;
		ZArchitecture.GUI.ZDynamicControlCreationUserControl additionalInfosUserControl1;
		public ZCodeFindBox tariffFindBox;
		protected ZGuidDropEdit JI_CEIGuidDropEdit;

		protected ConvertToLocalCurrencyControl StatisticalValueLocalCurrencyControl;
		protected ConvertToLocalCurrencyControl ValueForGstVatLocalCurrencyControl;
		protected ConvertToLocalCurrencyControl CustomsValueLocalCurrencyControl;
		protected ZTabPage FiscalReferencesTabPage;
		protected ZDynamicControlCreationUserControl FiscalReferencesUserControl;
		protected ZDropEdit goodsOriginDropEdit;
		protected internal ZDropEdit TransactionNatureDropEdit;
		protected Enterprise.ZArchitecture.ZTextBox CommercialReferenceBox;
		private ZTabPage VehicleTabPage;
		private ZDynamicControlCreationUserControl VehicleUserControl;
		protected ZArchitecture.GUI.ZTabPage AuthorisationsTabPage;
		protected ZDynamicControlCreationUserControl AuthorisationsUserControl;
		protected ZTabPage ValueIndicatorsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZLinkLabel GDMLink;
		protected ZTabPage SupplyChainActorTabPage;
		protected PlugIn.SupplyChainActorReferencesUserControl SupplyChainActorReferencesUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl invoiceLineValuationIndicatorsUserControl;
	}
}
