using System.Windows.Forms;
using Enterprise.Customs.CH.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class ExportInvoiceLineUserControl
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
        this.RestrictionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.RestrictionsUserControl = new Enterprise.Customs.CH.GUI.RestrictionsUserControl();
        this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.PreviousDocumentsUserControl = new Enterprise.Customs.CH.GUI.PlugIn.PreviousDocumentsUserControl();
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
        this.PreviousDocumentsTabPage.SuspendLayout();
        this.PreviousDocumentsUserControl.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.RestrictionsTabPage.SuspendLayout();
        this.RestrictionsUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // TopPanel
        // 
        this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
        this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
        // 
        // CantCreateInvoiceLinesLabel
        // 
        this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
        this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
        //
        // CustomsInvoiceLinesBoundGrid
        // 
        this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
        // 
        // JI_Calc_CIFConvertToLocalCurrencyControl
        // 
        this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 70, true);
        this.JI_Calc_CIFConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_InsuranceConvertToLocalCurrencyControl
        // 
        this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 122, true);
        this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_FreightConvertToLocalCurrencyControl
        // 
        this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 96, true);
        this.JI_Calc_FreightConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_FOBConvertToLocalCurrencyControl
        // 
        this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 44, true);
        this.JI_Calc_FOBConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_GSTConvertToLocalCurrencyControl
        // 
        this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 174, true);
        this.JI_Calc_GSTConvertToLocalCurrencyControl.Visible = false;
        // 
        // JI_Calc_DutyConvertToLocalCurrencyControl
        // 
        this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 148, true);
        this.JI_Calc_DutyConvertToLocalCurrencyControl.Visible = false;
        // 
        // Splitter
        // 
        this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
        // 
        // PreviousDocumentsTabPage
        // 
        this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("{BD2896C-2EB5-405F-9179-15D67E08405D", "[40] Previous Docs");
        this.PreviousDocumentsTabPage.Controls.Add(this.PreviousDocumentsUserControl);
        this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.PreviousDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
        this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 271, true);
        this.PreviousDocumentsTabPage.TabIndex = 1;
        this.PreviousDocumentsTabPage.UseVisualStyleBackColor = true;
        // 
        // PreviousDocumentsUserControl
        // 
        this.PreviousDocumentsUserControl.AllowDrop = true;
        this.PreviousDocumentsUserControl.AutoSize = true;
        this.BindingSource.SetBindingMember(this.PreviousDocumentsUserControl, "FilteredInvoiceLines.PreviousDocuments");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.PreviousDocument)(((Enterprise.Customs.CH.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)))));
        this.PreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.PreviousDocumentsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.PreviousDocumentsUserControl.Name = "PreviousDocumentsUserControl";
        this.PreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1260, 265, true);
        this.PreviousDocumentsUserControl.TabIndex = 1;
        // 
        // RestrictionsTabPage
        // 
        this.RestrictionsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("CDE20874-CFBB-4B25-8605-6658FFF3C843", "Restrictions");
        this.RestrictionsTabPage.Controls.Add(this.RestrictionsUserControl);
        this.RestrictionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.RestrictionsTabPage.Name = "RestrictionsTabPage";
        this.RestrictionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 327, true);
        this.RestrictionsTabPage.TabIndex = 9;
        // 
        // RestrictionsUserControl
        // 
        this.RestrictionsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.RestrictionsUserControl, "FilteredInvoiceLines.Restrictions");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.Restriction)(((Enterprise.Customs.CH.Business.Restriction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Restrictions)).SyncRoot)))));
        this.RestrictionsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.RestrictionsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.RestrictionsUserControl.Name = "RestrictionsUserControl";
        this.RestrictionsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 327, true);
        this.RestrictionsUserControl.TabIndex = 0;
        // 
        // ExportInvoiceLineUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Name = "ExportInvoiceLineUserControl";
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
        this.RestrictionsTabPage.ResumeLayout(false);
        this.RestrictionsTabPage.PerformLayout();
        this.RestrictionsUserControl.ResumeLayout(true);
        this.RestrictionsUserControl.PerformLayout();
        this.PreviousDocumentsTabPage.ResumeLayout(false);
        this.PreviousDocumentsTabPage.PerformLayout();
        this.PreviousDocumentsUserControl.ResumeLayout(true);
        this.PreviousDocumentsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZTabPage RestrictionsTabPage;
    internal RestrictionsUserControl RestrictionsUserControl;
    internal ZTabPage PreviousDocumentsTabPage;
    internal PreviousDocumentsUserControl PreviousDocumentsUserControl;
}
