using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI;

partial class ImportInvoiceLineUserControl
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
			UnhookJobDeclarationEvents(ESDeclaration);
			components.Dispose();
		}

		AdditionalDocumentsTabPage?.Dispose();
		VehiclesTabPage?.Dispose();
		VehiclesUserControl?.Dispose();

		base.Dispose(disposing);
	}

	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>

	private void InitializeComponent()
	{
		this.PosAdjLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
		this.NegAdjLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
		this.VATAdditionsLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
		this.AdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.additionalDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		this.VehiclesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.VehiclesUserControl = new Enterprise.Customs.ES.GUI.VehiclesUserControl();
		this.PreferenceCodeDropEdit.SuspendLayout();
		this.VatTypeDropEdit.SuspendLayout();
		this.QuotaDropEdit.SuspendLayout();
		this.SecondQuotaDropEdit.SuspendLayout();
		this.ValuationMethodDropEdit.SuspendLayout();
		this.ValuationAdjustmentCodeDropEdit.SuspendLayout();
		this.CountryOfSupplyCodeFindBox.SuspendLayout();
		this.MethodOfPaymentDropEdit.SuspendLayout();
		this.CountryOfDestinationCodeFindBox.SuspendLayout();
		this.TransactionNatureDropEdit.SuspendLayout();
		this.packagesPivotUserControl.SuspendLayout();
		this.VehiclesTabPage.SuspendLayout();
		this.VehiclesUserControl.SuspendLayout();
		this.SupportingDocumentsTabPage.SuspendLayout();
		this.InvoiceLinePaymentTabPage.SuspendLayout();
		this.AdditionalDocumentsTabPage.SuspendLayout();
		this.AdditionalInfosTabPage.SuspendLayout();
		this.PreviousDocumentsTabPage.SuspendLayout();
		this.OrganizationsTabPage.SuspendLayout();
		this.ThirdQtyCalcDropEdit.SuspendLayout();
		this.FourthQtyCalcDropEdit.SuspendLayout();
		this.PackagesPivotTabPage.SuspendLayout();
		this.TaxTabPage.SuspendLayout();
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
		this.FiscalReferencesUserControl.SuspendLayout();
		this.goodsOriginDropEdit.SuspendLayout();
		this.AuthorisationsTabPage.SuspendLayout();
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
		this.PosAdjLocalCurrencyControl.SuspendLayout();
		this.NegAdjLocalCurrencyControl.SuspendLayout();
		this.VATAdditionsLocalCurrencyControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// PreferenceCodeDropEdit
		// 
		this.PreferenceCodeDropEdit.TabIndex = 8;
		// 
		// VatTypeDropEdit
		// 
		this.VatTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(799, 22, true);
		this.VatTypeDropEdit.TabIndex = 100;
		// 
		// QuotaDropEdit
		// 
		this.QuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 120, true);
		// 
		// QuotaTextBox
		// 
		this.QuotaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 120, true);
		this.QuotaTextBox.TabIndex = 17;
		// 
		// SecondQuotaTextBox
		// 
		this.SecondQuotaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 120, true);
		this.SecondQuotaTextBox.TabIndex = 15;
		// 
		// SecondQuotaDropEdit
		// 
		this.SecondQuotaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 120, true);
		// 
		// GDMLink
		// 
		this.GDMLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 20, true);
		this.GDMLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
		this.GDMLink.TabIndex = 6;
		this.GDMLink.TabStop = true;
		// 
		// ValuationMethodDropEdit
		// 
		this.ValuationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 172, true);
		this.ValuationMethodDropEdit.TabIndex = 20;
		// 
		// ValuationAdjustmentCodeDropEdit
		// 
		this.ValuationAdjustmentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 198, true);
		this.ValuationAdjustmentCodeDropEdit.TabIndex = 23;
		// 
		// ValuationAdjustmentPercentageCalcEdit
		// 
		this.ValuationAdjustmentPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 198, true);
		this.ValuationAdjustmentPercentageCalcEdit.TabIndex = 24;
		// 
		// CountryOfDestinationCodeFindBox
		// 
		this.CountryOfDestinationCodeFindBox.TabIndex = 22;
		// 
		// CheckQuotaBalanceLinkLabel
		// 
		this.CheckQuotaBalanceLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 120, true);
		this.CheckQuotaBalanceLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
		this.CheckQuotaBalanceLinkLabel.TabIndex = 14;
		this.CheckQuotaBalanceLinkLabel.TabStop = true;
		// 
		// InvoiceLinePaymentTabPage
		// 
		this.InvoiceLinePaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 327, true);
		// 
		// OrganizationsTabPage
		// 
		this.OrganizationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.OrganizationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 327, true);
		// 
		// VehiclesTabPage
		// 
		this.VehiclesTabPage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("899BF4E2-75CC-43EF-A7F4-3C5302C778AB", "Vehicles");
		this.VehiclesTabPage.Controls.Add(this.VehiclesUserControl);
		this.VehiclesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.VehiclesTabPage.Name = "VehiclesTabPage";
		this.VehiclesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 327, true);
		this.VehiclesTabPage.TabIndex = 10;
		// 
		// VehiclesUserControl
		// 
		this.VehiclesUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.VehiclesUserControl, "FilteredInvoiceLines.Vehicles");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ES.Business.CusVehicle)(((Enterprise.Customs.ES.Business.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)))));
		this.VehiclesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.VehiclesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.VehiclesUserControl.Name = "VehiclesUserControl";
		this.VehiclesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 327, true);
		this.VehiclesUserControl.TabIndex = 0;
		// 
		// StatisticalValueCalcEdit
		// 
		this.StatisticalValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 172, true);
		this.StatisticalValueCalcEdit.TabIndex = 21;
		// 
		// ThirdQtyCalcDropEdit
		// 
		this.ThirdQtyCalcDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("AD504FB2-9806-40C3-8AA4-9B27846A0BAE", "[31] Third Qty");
		this.ThirdQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 120, true);
		this.ThirdQtyCalcDropEdit.TabIndex = 18;
		// 
		// FourthQtyCalcDropEdit
		// 
		this.FourthQtyCalcDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("B3ECFB04-B00C-4716-8165-17B7C6B57F69", "Fourth Qty");
		this.FourthQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 146, true);
		this.FourthQtyCalcDropEdit.TabIndex = 19;
		this.FourthQtyCalcDropEdit.Visible = true;
		// 
		// SupplementaryCode2DropEdit
		// 
		this.SupplementaryCode2DropEdit.TabIndex = 3;
		// 
		// JI_AdditionalSupplementsTextBox
		// 
		this.JI_AdditionalSupplementsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
		this.JI_AdditionalSupplementsTextBox.TabIndex = 4;
		// 
		// AdditionalSupplementaryCodesEditButton
		// 
		this.AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 15, true);
		// 
		// CPCFindBox
		// 
		this.CPCFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 68, true);
		this.CPCFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 20, true);
		this.CPCFindBox.TabIndex = 10;
		// 
		// organizationsUserControl1
		// 
		this.organizationsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 327, true);
		this.organizationsUserControl1.UserControlType = typeof(Enterprise.Customs.EU.GUI.PlugIn.ImportInvoiceLineOrganizationsUserControl);
		// 
		// StatValueManualOverrideCheckBox
		// 
		this.StatValueManualOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(779, 175, true);
		this.StatValueManualOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 17, true);
		this.StatValueManualOverrideCheckBox.TabIndex = 22;
		// 
		// JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl
		// 
		this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 63, true);
		this.JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl.TabIndex = 13;
		// 
		// JI_CustomsQuantityCalcDropEdit
		// 
		this.JI_CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 68, true);
		this.JI_CustomsQuantityCalcDropEdit.TabIndex = 11;
		// 
		// zButtonMoreAdditionalProcedureCode
		// 
		this.zButtonMoreAdditionalProcedureCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 92, true);
		this.zButtonMoreAdditionalProcedureCode.TabIndex = 13;
		// 
		// zTextBoxAddtionalProcedureCodeAsString
		// 
		this.zTextBoxAddtionalProcedureCodeAsString.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 94, true);
		this.zTextBoxAddtionalProcedureCodeAsString.TabIndex = 13;
		// 
		// tariffFindBox
		// 
		this.tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
		// 
		// StatisticalValueLocalCurrencyControl
		// 
		this.StatisticalValueLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 111, true);
		// 
		// ValueForGstVatLocalCurrencyControl
		// 
		this.ValueForGstVatLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 134, true);
		// 
		// CustomsValueLocalCurrencyControl
		// 
		this.CustomsValueLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_ESCustomsValue";
		this.CustomsValueLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 86, true);
		// 
		// AuthorisationsUserControl
		// 
		this.AuthorisationsUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.InvoiceLineAuthorisationsUserControl);
		// 
		// InvoiceLinesSummaryGroupBox
		// 
		this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 0, true);
		this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 394, true);
		// 
		// BottomPanel
		// 
		this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 387, true);
		this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 394, true);
		// 
		// TopPanel
		// 
		this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 56, true);
		this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 725, true);
		// 
		// LineDetailTabControl
		// 
		this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 394, true);
		// 
		// InvoiceDetailsGroupBox
		// 
		this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 114, true);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.TransactionNatureDropEdit, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.goodsOriginDropEdit, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CEIGuidDropEdit, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CountryOfSupplyCodeFindBox, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VatTypeDropEdit, 0);
		this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.MethodOfPaymentDropEdit, 0);
		// 
		// JI_RH_NKCommodity_CodeBoundFindBox
		// 
		this.JI_RH_NKCommodity_CodeBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
		this.JI_RH_NKCommodity_CodeBoundFindBox.TabIndex = 16;
		// 
		// ClassificationDetailsGroupBox
		// 
		this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 208, true);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.FourthQtyCalcDropEdit, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CommercialReferenceBox, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CheckQuotaBalanceLinkLabel, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SecondQuotaDropEdit, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SecondQuotaTextBox, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CountryOfDestinationCodeFindBox, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode1TextBox, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode2TextBox, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode1DropEdit, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCode2DropEdit, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.QuotaDropEdit, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.QuotaTextBox, 0);
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
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ThirdQtyCalcDropEdit, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CPCFindBox, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.PreferenceCodeDropEdit, 0);
		this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.GDMLink, 0);
		// 
		// LineChargesTabPage
		// 
		this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 367, true);
		// 
		// CurrentInvoicePanel
		// 
		this.CurrentInvoicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 83, true);
		// 
		// LineSummaryPanel
		// 
		this.LineSummaryPanel.Controls.Add(this.VATAdditionsLocalCurrencyControl);
		this.LineSummaryPanel.Controls.Add(this.NegAdjLocalCurrencyControl);
		this.LineSummaryPanel.Controls.Add(this.PosAdjLocalCurrencyControl);
		this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 291, true);
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
		this.LineSummaryPanel.Controls.SetChildIndex(this.PosAdjLocalCurrencyControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.NegAdjLocalCurrencyControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.VATAdditionsLocalCurrencyControl, 0);
		// 
		// PendingApportionmentLabel
		// 
		this.PendingApportionmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 377, true);
		// 
		// ContainersTabPage
		// 
		this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 367, true);
		// 
		// ContainersGroupBox
		// 
		this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 367, true);
		// 
		// CusContainerInvoiceLineGrid
		// 
		this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 348, true);
		// 
		// CantCreateInvoiceLinesLabel
		// 
		this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 56, true);
		this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 725, true);
		// 
		// LineDetailsTabPage
		// 
		this.LineDetailsTabPage.AutoScroll = true;
		this.LineDetailsTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 330, true);
		this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 367, true);
		// 
		// NewLineDetailsTabPage
		// 
		this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 367, true);
		// 
		// InvoiceLineDetailsUserControl
		// 
		this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 367, true);
		// 
		// ClassificationPanel
		// 
		this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 253, true);
		this.ClassificationPanel.TabIndex = 1;
		this.ClassificationPanel.Controls.SetChildIndex(this.ClassificationDetailsGroupBox, 0);
		// 
		// CustomsInvoiceLinesBoundGrid
		// 
		this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 725, true);
		// 
		// JI_Calc_CIFConvertToLocalCurrencyControl
		// 
		this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 158, true);
		this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 17;
		// 
		// JI_Calc_InsuranceConvertToLocalCurrencyControl
		// 
		this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 134, true);
		// 
		// JI_Calc_FreightConvertToLocalCurrencyControl
		// 
		this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 111, true);
		// 
		// JI_Calc_FOBConvertToLocalCurrencyControl
		// 
		this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 86, true);
		// 
		// JI_Calc_GSTConvertToLocalCurrencyControl
		// 
		this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 39, true);
		this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 12;
		// 
		// JI_Calc_DutyConvertToLocalCurrencyControl
		// 
		this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 14, true);
		// 
		// JI_Calc_BalanceConvertToLocalCurrencyControl
		// 
		this.JI_Calc_BalanceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 58, true);
		// 
		// JI_Calc_LinesEnteredConvertToLocalCurrencyControl
		// 
		this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 35, true);
		// 
		// JI_Calc_LinesTotalConvertToLocalCurrencyControl
		// 
		this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 13, true);
		// 
		// CustomsQuantityCalcDropEdit
		// 
		this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 94, true);
		this.CustomsQuantityCalcDropEdit.TabIndex = 15;
		// 
		// Splitter
		// 
		this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 377, true);
		this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 10, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
		// 
		// PosAdjLocalCurrencyControl
		// 
		this.PosAdjLocalCurrencyControl.AllowDrop = true;
		this.PosAdjLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_PosAdj";
		this.PosAdjLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
		this.PosAdjLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.PosAdjLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("A45BA686-1308-47A1-AE01-82C531026EF6", "[45a] Positive Adj.");
		this.PosAdjLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 206, true);
		this.PosAdjLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.PosAdjLocalCurrencyControl.Name = "PosAdjLocalCurrencyControl";
		this.PosAdjLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
		this.PosAdjLocalCurrencyControl.TabIndex = 19;
		// 
		// NegAdjLocalCurrencyControl
		// 
		this.NegAdjLocalCurrencyControl.AllowDrop = true;
		this.NegAdjLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_NegAdj";
		this.NegAdjLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
		this.NegAdjLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.NegAdjLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("CCCAC215-ADE1-45BD-97AB-45638F38B20C", "[45b] Negative Adj.");
		this.NegAdjLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 230, true);
		this.NegAdjLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.NegAdjLocalCurrencyControl.Name = "NegAdjLocalCurrencyControl";
		this.NegAdjLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
		this.NegAdjLocalCurrencyControl.TabIndex = 20;
		// 
		// VATAdditionsLocalCurrencyControl
		// 
		this.VATAdditionsLocalCurrencyControl.AllowDrop = true;
		this.VATAdditionsLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_VAT_Additions";
		this.VATAdditionsLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
		this.VATAdditionsLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.VATAdditionsLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 182, true);
		this.VATAdditionsLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.VATAdditionsLocalCurrencyControl.Name = "VATAdditionsLocalCurrencyControl";
		this.VATAdditionsLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
		this.VATAdditionsLocalCurrencyControl.TabIndex = 18;
		// 
		// AdditionalDocumentsTabPage
		// 
		this.AdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("86D297E5-A328-4027-BC69-B90737678E56", "Additional Documents");
		this.AdditionalDocumentsTabPage.Controls.Add(this.additionalDocumentsUserControl);
		this.AdditionalDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.AdditionalDocumentsTabPage.Name = "AdditionalDocumentsTabPage";
		this.AdditionalDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.AdditionalDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
		this.AdditionalDocumentsTabPage.TabIndex = 1;
		this.AdditionalDocumentsTabPage.UseVisualStyleBackColor = true;
		// 
		// additionalDocumentsUserControl
		// 
		this.additionalDocumentsUserControl.AllowDrop = true;
		this.additionalDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.additionalDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.additionalDocumentsUserControl.Name = "additionalDocumentsUserControl";
		this.additionalDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 352, true);
		this.additionalDocumentsUserControl.TabIndex = 1;
		// 
		// ImportInvoiceLineUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Name = "ImportInvoiceLineUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1187, 781, true);
		this.PreferenceCodeDropEdit.ResumeLayout(true);
		this.PreferenceCodeDropEdit.PerformLayout();
		this.VatTypeDropEdit.ResumeLayout(true);
		this.VatTypeDropEdit.PerformLayout();
		this.QuotaDropEdit.ResumeLayout(true);
		this.QuotaDropEdit.PerformLayout();
		this.SecondQuotaDropEdit.ResumeLayout(true);
		this.SecondQuotaDropEdit.PerformLayout();
		this.ValuationMethodDropEdit.ResumeLayout(true);
		this.ValuationMethodDropEdit.PerformLayout();
		this.ValuationAdjustmentCodeDropEdit.ResumeLayout(true);
		this.ValuationAdjustmentCodeDropEdit.PerformLayout();
		this.CountryOfSupplyCodeFindBox.ResumeLayout(true);
		this.CountryOfSupplyCodeFindBox.PerformLayout();
		this.MethodOfPaymentDropEdit.ResumeLayout(true);
		this.MethodOfPaymentDropEdit.PerformLayout();
		this.CountryOfDestinationCodeFindBox.ResumeLayout(true);
		this.CountryOfDestinationCodeFindBox.PerformLayout();
		this.TransactionNatureDropEdit.ResumeLayout(true);
		this.TransactionNatureDropEdit.PerformLayout();
		this.packagesPivotUserControl.ResumeLayout(true);
		this.packagesPivotUserControl.PerformLayout();
		this.VehiclesTabPage.ResumeLayout(false);
		this.VehiclesTabPage.PerformLayout();
		this.VehiclesUserControl.ResumeLayout(true);
		this.VehiclesUserControl.PerformLayout();
		this.SupportingDocumentsTabPage.ResumeLayout(false);
		this.SupportingDocumentsTabPage.PerformLayout();
		this.InvoiceLinePaymentTabPage.ResumeLayout(false);
		this.InvoiceLinePaymentTabPage.PerformLayout();
		this.AdditionalDocumentsTabPage.ResumeLayout(false);
		this.AdditionalDocumentsTabPage.PerformLayout();
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
		this.TaxTabPage.ResumeLayout(false);
		this.TaxTabPage.PerformLayout();
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
		this.FiscalReferencesUserControl.ResumeLayout(true);
		this.FiscalReferencesUserControl.PerformLayout();
		this.goodsOriginDropEdit.ResumeLayout(true);
		this.goodsOriginDropEdit.PerformLayout();
		this.AuthorisationsTabPage.ResumeLayout(false);
		this.AuthorisationsTabPage.PerformLayout();
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
		this.PosAdjLocalCurrencyControl.ResumeLayout(true);
		this.PosAdjLocalCurrencyControl.PerformLayout();
		this.NegAdjLocalCurrencyControl.ResumeLayout(true);
		this.NegAdjLocalCurrencyControl.PerformLayout();
		this.VATAdditionsLocalCurrencyControl.ResumeLayout(true);
		this.VATAdditionsLocalCurrencyControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	Customs.GUI.ConvertToLocalCurrencyControl PosAdjLocalCurrencyControl;
	Customs.GUI.ConvertToLocalCurrencyControl NegAdjLocalCurrencyControl;
	ConvertToLocalCurrencyControl VATAdditionsLocalCurrencyControl;
	protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalDocumentsTabPage;
	private Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl additionalDocumentsUserControl;
	internal ZArchitecture.GUI.ZTabPage VehiclesTabPage;
	internal VehiclesUserControl VehiclesUserControl;
}
