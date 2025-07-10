
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CH.GUI;

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
        this.CalcCustomsValue = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
        this.TaxesAndFeesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.TaxesAndFeesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
        this.AdditionalTaxesUserControl = new Enterprise.Customs.CH.GUI.AdditionalTaxesUserControl();
        this.AdditionalFeesUserControl = new Enterprise.Customs.CH.GUI.AdditionalFeesUserControl();
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
        this.CalcCustomsValue.SuspendLayout();
        this.TaxesAndFeesTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TaxesAndFeesSplitContainer)).BeginInit();
        this.TaxesAndFeesSplitContainer.Panel1.SuspendLayout();
        this.TaxesAndFeesSplitContainer.Panel2.SuspendLayout();
        this.TaxesAndFeesSplitContainer.SuspendLayout();
        this.AdditionalTaxesUserControl.SuspendLayout();
        this.AdditionalFeesUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // TopPanel
        // 
        this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
        this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 533, true);
        // 
        // LineDetailTabControl
        // 
        this.LineDetailTabControl.Controls.Add(this.TaxesAndFeesTabPage);
        this.LineDetailTabControl.Controls.SetChildIndex(this.TaxesAndFeesTabPage, 0);
        this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
        this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
        this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
        this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
        // 
        // InvoiceDetailsGroupBox
        // 
        this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 96, true);
        // 
        // LineChargesTabPage
        // 
        this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 293, true);
        // 
        // LineSummaryPanel
        // 
        this.LineSummaryPanel.Controls.Add(this.CalcCustomsValue);
        this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
        this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
        this.LineSummaryPanel.Controls.SetChildIndex(this.CalcCustomsValue, 0);
        this.LineSummaryPanel.Controls.SetChildIndex(this.oLabel8, 0);
        this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
        this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
        this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
        this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
        // 
        // ContainersTabPage
        // 
        this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 293, true);
        // 
        // ContainersGroupBox
        // 
        this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 293, true);
        // 
        // CusContainerInvoiceLineGrid
        // 
        this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 274, true);
        // 
        // CantCreateInvoiceLinesLabel
        // 
        this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
        this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 533, true);
        // 
        // LineDetailsTabPage
        // 
        this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 293, true);
        // 
        // NewLineDetailsTabPage
        // 
        this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 293, true);
        // 
        // InvoiceLineDetailsUserControl
        // 
        this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 293, true);
        // 
        // CustomsInvoiceLinesBoundGrid
        // 
        this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 533, true);
        // 
        // JI_Calc_CIFConvertToLocalCurrencyControl
        // 
        this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 146, true);
        this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 3;
        this.JI_Calc_CIFConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_InsuranceConvertToLocalCurrencyControl
        // 
        this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 189, true);
        this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_FreightConvertToLocalCurrencyControl
        // 
        this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 172, true);
        this.JI_Calc_FreightConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_FOBConvertToLocalCurrencyControl
        // 
        this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 120, true);
        this.JI_Calc_FOBConvertToLocalCurrencyControl.TabIndex = 2;
        this.JI_Calc_FOBConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_GSTConvertToLocalCurrencyControl
        // 
        this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 94, true);
        this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 5;
        // 
        // JI_Calc_DutyConvertToLocalCurrencyControl
        // 
        this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 68, true);
        this.JI_Calc_DutyConvertToLocalCurrencyControl.TabIndex = 4;
        // 
        // Splitter
        // 
        this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
        //
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobDeclaration);
        // 
        // CalcCustomsValue
        // 
        this.CalcCustomsValue.AllowDrop = true;
        this.CalcCustomsValue.BindToAmount = "FilteredInvoiceLines.JI_CustomsValue";
        this.CalcCustomsValue.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
        this.CalcCustomsValue.BindToUnit = "FilteredInvoiceLines.LocalCurrency.RX_Code";
        this.CalcCustomsValue.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
        this.CalcCustomsValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 42, true);
        this.CalcCustomsValue.Name = "CalcCustomsValue";
        this.CalcCustomsValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
        this.CalcCustomsValue.TabIndex = 1;
        // 
        // TaxesAndFeesTabPage
        // 
        this.TaxesAndFeesTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("40a9b2cc-9f8d-42ca-bdac-d7f8d86c3fff", "Taxes And Fees");
        this.TaxesAndFeesTabPage.Controls.Add(this.TaxesAndFeesSplitContainer);
        this.TaxesAndFeesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.TaxesAndFeesTabPage.Name = "TaxesAndFeesTabPage";
        this.TaxesAndFeesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 293, true);
        this.TaxesAndFeesTabPage.TabIndex = 11;
        // 
        // TaxesAndFeesSplitContainer
        // 
        this.TaxesAndFeesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TaxesAndFeesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.TaxesAndFeesSplitContainer.Name = "TaxesAndFeesSplitContainer";
        this.TaxesAndFeesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
        // 
        // TaxesAndFeesSplitContainer.Panel1
        // 
        this.TaxesAndFeesSplitContainer.Panel1.Controls.Add(this.AdditionalTaxesUserControl);
        // 
        // TaxesAndFeesSplitContainer.Panel2
        // 
        this.TaxesAndFeesSplitContainer.Panel2.Controls.Add(this.AdditionalFeesUserControl);
        this.TaxesAndFeesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 293, true);
        this.TaxesAndFeesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(146);
        this.TaxesAndFeesSplitContainer.TabIndex = 0;
        // 
        // AdditionalTaxesUserControl
        // 
        this.AdditionalTaxesUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.AdditionalTaxesUserControl, "FilteredInvoiceLines.AdditionalTaxes");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.CusLineTariffDetailCollection)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalTaxes)));
        this.AdditionalTaxesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalTaxesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalTaxesUserControl.Name = "AdditionalTaxesUserControl";
        this.AdditionalTaxesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 146, true);
        this.AdditionalTaxesUserControl.TabIndex = 0;
        // 
        // AdditionalFeesUserControl
        // 
        this.AdditionalFeesUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.AdditionalFeesUserControl, "FilteredInvoiceLines.AdditionalFees");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.CusLineTariffDetailCollection)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalFees)));
        this.AdditionalFeesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalFeesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalFeesUserControl.Name = "AdditionalFeesUserControl";
        this.AdditionalFeesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 143, true);
        this.AdditionalFeesUserControl.TabIndex = 0;
        // 
        // ImportInvoiceLineUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Name = "ImportInvoiceLineUserControl";
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
        this.CalcCustomsValue.ResumeLayout(true);
        this.CalcCustomsValue.PerformLayout();
        this.TaxesAndFeesTabPage.ResumeLayout(false);
        this.TaxesAndFeesTabPage.PerformLayout();
        this.TaxesAndFeesSplitContainer.Panel1.ResumeLayout(false);
        this.TaxesAndFeesSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.TaxesAndFeesSplitContainer)).EndInit();
        this.TaxesAndFeesSplitContainer.ResumeLayout(false);
        this.TaxesAndFeesSplitContainer.PerformLayout();
        this.AdditionalTaxesUserControl.ResumeLayout(true);
        this.AdditionalTaxesUserControl.PerformLayout();
        this.AdditionalFeesUserControl.ResumeLayout(true);
        this.AdditionalFeesUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private ConvertToLocalCurrencyControl CalcCustomsValue;
    internal ZArchitecture.GUI.ZTabPage TaxesAndFeesTabPage;
    internal CargoWise.Windows.UI.KSplitContainer TaxesAndFeesSplitContainer;
    internal AdditionalTaxesUserControl AdditionalTaxesUserControl;
    internal AdditionalFeesUserControl AdditionalFeesUserControl;
}
