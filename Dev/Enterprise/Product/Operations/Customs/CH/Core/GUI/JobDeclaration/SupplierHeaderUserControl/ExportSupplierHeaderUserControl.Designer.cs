using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.GUI;

partial class ExportSupplierHeaderUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.TransportDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.TransportDocumentsUserControl = new Enterprise.Customs.CH.GUI.TransportDocumentsUserControl();
        this.ReferenceNumberUCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
        this.GroupInvoiceDropEdit.SuspendLayout();
        this.NoOfPacksCalcDropEdit.SuspendLayout();
        this.GrossWeightCalcDropEdit.SuspendLayout();
        this.NetWeightCalcDropEdit.SuspendLayout();
        this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
        this.BottomPanel.SuspendLayout();
        this.LeftBottomPanel.SuspendLayout();
        this.RightBottomPanel.SuspendLayout();
        this.InvoiceTabControl.SuspendLayout();
        this.ComInvoiceDetailsTabPage.SuspendLayout();
        this.ChargesGroupBox.SuspendLayout();
        this.ChargesTabControl.SuspendLayout();
        this.InvoiceChargesTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
        this.InvoiceChargesGrid.SuspendLayout();
        this.ApportionedTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
        this.ApportionedChargesGrid.SuspendLayout();
        this.BaseGroupChargesGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).BeginInit();
        this.BaseGroupChargesGrid.SuspendLayout();
        this.JZ_FOBAmountBoundCurrencyControl.SuspendLayout();
        this.JZ_Calc_TNIBoundInvoiceCurrencyControl.SuspendLayout();
        this.JZ_CIFAmountBoundCurrencyControl.SuspendLayout();
        this.LineTotalBoundConvertToLocalCurrencyControl.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).BeginInit();
        this.JobComInvoiceHeadersBoundGrid.SuspendLayout();
        this.InvCustomFieldsDisplayControl.SuspendLayout();
        this.CustomFieldsTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
        this.Splitter.Panel1.SuspendLayout();
        this.Splitter.Panel2.SuspendLayout();
        this.Splitter.SuspendLayout();
        this.InvDetailLeftPanel.SuspendLayout();
        this.InvDetailRightPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).BeginInit();
        this.ChargesGroupsSplitterContainer.Panel1.SuspendLayout();
        this.ChargesGroupsSplitterContainer.Panel2.SuspendLayout();
        this.ChargesGroupsSplitterContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.TransportDocumentsTabPage.SuspendLayout();
        this.TransportDocumentsUserControl.SuspendLayout();
        this.SuspendLayout();
        // InvoiceTabControl
        // 
        this.InvoiceTabControl.Controls.Add(this.TransportDocumentsTabPage);
        // 
        // JobComInvoiceHeadersBoundGrid
        // 
        // 
        // 
        // 
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.AllowNavigation = false;
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.CaptionVisible = false;
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutUU5vFvxYnT4ITmuJkyIQgg==";
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
        // 
        // 
        // InvDetailLeftPanel
        // 
        this.InvDetailLeftPanel.Controls.Add(this.ReferenceNumberUCRTextBox);
        this.InvDetailLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 249, true);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.ReferenceNumberUCRTextBox, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.NoOfPacksCalcDropEdit, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrLandedCostExRateCalcEdit, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermPlaceTextBox, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermBoundDropDownEdit, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceAmountBoundCurrencyControl, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.GroupInvoiceDropEdit, 0);
        this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceNumberBoundTextBox, 0);
        // 
        // TransportDocumentsTabPage
        // 
        this.TransportDocumentsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("EC98D0DA-4070-44C0-BCB3-890D10AD73E8", "Transport Documents");
        this.TransportDocumentsTabPage.Controls.Add(this.TransportDocumentsUserControl);
        this.TransportDocumentsTabPage.Name = "TransportDocumentsTabPage";
        this.TransportDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.TransportDocumentsTabPage.TabIndex = 2;
        // 
        // TransportDocumentsUserControl
        // 
        this.TransportDocumentsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.TransportDocumentsUserControl, "Invoices.TransportDocuments");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.TransportDocument)(((Enterprise.Customs.CH.Business.TransportDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).TransportDocuments)).SyncRoot)))));
        this.TransportDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TransportDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.TransportDocumentsUserControl.Name = "TransportDocumentsUserControl";
        this.TransportDocumentsUserControl.TabIndex = 0;
        // 
        // ReferenceNumberUCR
        // 
        this.BindingSource.SetBindingMember(this.ReferenceNumberUCRTextBox, "Invoices.JZ_UCR");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_UCR)));
        this.ReferenceNumberUCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 227, true);
        this.ReferenceNumberUCRTextBox.Name = "ReferenceNumberUCRTextBox";
        this.ReferenceNumberUCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
        this.ReferenceNumberUCRTextBox.TabIndex = 10;
        // 
        // ExportSupplierHeaderUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Name = "ExportSupplierHeaderUserControl";
        this.JZ_IncoTermBoundDropDownEdit.ResumeLayout(true);
        this.JZ_IncoTermBoundDropDownEdit.PerformLayout();
        this.GroupInvoiceDropEdit.ResumeLayout(true);
        this.GroupInvoiceDropEdit.PerformLayout();
        this.NoOfPacksCalcDropEdit.ResumeLayout(true);
        this.NoOfPacksCalcDropEdit.PerformLayout();
        this.GrossWeightCalcDropEdit.ResumeLayout(true);
        this.GrossWeightCalcDropEdit.PerformLayout();
        this.NetWeightCalcDropEdit.ResumeLayout(true);
        this.NetWeightCalcDropEdit.PerformLayout();
        this.JZ_InvoiceAmountBoundCurrencyControl.ResumeLayout(true);
        this.JZ_InvoiceAmountBoundCurrencyControl.PerformLayout();
        this.BottomPanel.ResumeLayout(false);
        this.BottomPanel.PerformLayout();
        this.LeftBottomPanel.ResumeLayout(false);
        this.LeftBottomPanel.PerformLayout();
        this.RightBottomPanel.ResumeLayout(false);
        this.RightBottomPanel.PerformLayout();
        this.InvoiceTabControl.ResumeLayout(false);
        this.InvoiceTabControl.PerformLayout();
        this.ComInvoiceDetailsTabPage.ResumeLayout(false);
        this.ComInvoiceDetailsTabPage.PerformLayout();
        this.ChargesGroupBox.ResumeLayout(false);
        this.ChargesGroupBox.PerformLayout();
        this.ChargesTabControl.ResumeLayout(false);
        this.ChargesTabControl.PerformLayout();
        this.InvoiceChargesTabPage.ResumeLayout(false);
        this.InvoiceChargesTabPage.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
        this.InvoiceChargesGrid.ResumeLayout(false);
        this.InvoiceChargesGrid.PerformLayout();
        this.ApportionedTabPage.ResumeLayout(false);
        this.ApportionedTabPage.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
        this.ApportionedChargesGrid.ResumeLayout(false);
        this.ApportionedChargesGrid.PerformLayout();
        this.BaseGroupChargesGroupBox.ResumeLayout(false);
        this.BaseGroupChargesGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).EndInit();
        this.BaseGroupChargesGrid.ResumeLayout(false);
        this.BaseGroupChargesGrid.PerformLayout();
        this.JZ_FOBAmountBoundCurrencyControl.ResumeLayout(true);
        this.JZ_FOBAmountBoundCurrencyControl.PerformLayout();
        this.JZ_Calc_TNIBoundInvoiceCurrencyControl.ResumeLayout(true);
        this.JZ_Calc_TNIBoundInvoiceCurrencyControl.PerformLayout();
        this.JZ_CIFAmountBoundCurrencyControl.ResumeLayout(true);
        this.JZ_CIFAmountBoundCurrencyControl.PerformLayout();
        this.LineTotalBoundConvertToLocalCurrencyControl.ResumeLayout(true);
        this.LineTotalBoundConvertToLocalCurrencyControl.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).EndInit();
        this.JobComInvoiceHeadersBoundGrid.ResumeLayout(true);
        this.JobComInvoiceHeadersBoundGrid.PerformLayout();
        this.InvCustomFieldsDisplayControl.ResumeLayout(true);
        this.InvCustomFieldsDisplayControl.PerformLayout();
        this.CustomFieldsTabPage.ResumeLayout(false);
        this.CustomFieldsTabPage.PerformLayout();
        this.Splitter.Panel1.ResumeLayout(false);
        this.Splitter.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
        this.Splitter.ResumeLayout(false);
        this.Splitter.PerformLayout();
        this.InvDetailLeftPanel.ResumeLayout(false);
        this.InvDetailLeftPanel.PerformLayout();
        this.InvDetailRightPanel.ResumeLayout(false);
        this.InvDetailRightPanel.PerformLayout();
        this.ChargesGroupsSplitterContainer.Panel1.ResumeLayout(false);
        this.ChargesGroupsSplitterContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).EndInit();
        this.ChargesGroupsSplitterContainer.ResumeLayout(false);
        this.ChargesGroupsSplitterContainer.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.TransportDocumentsTabPage.ResumeLayout(false);
        this.TransportDocumentsTabPage.PerformLayout();
        this.TransportDocumentsUserControl.ResumeLayout(true);
        this.TransportDocumentsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZTabPage TransportDocumentsTabPage;
    private TransportDocumentsUserControl TransportDocumentsUserControl;
    internal ZTextBox ReferenceNumberUCRTextBox;
}
