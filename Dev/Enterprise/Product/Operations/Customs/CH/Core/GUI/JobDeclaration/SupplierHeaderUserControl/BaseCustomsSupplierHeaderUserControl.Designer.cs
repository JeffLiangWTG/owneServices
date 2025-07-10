namespace Enterprise.Customs.CH.GUI;

partial class BaseCustomsSupplierHeaderUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.InvoiceChargesButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.InvoiceChargesCalculateFreightButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.GroupChargesButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
        this.GroupChargesCalculateFreightButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.PreviousDocumentsUserControl = new Enterprise.Customs.CH.GUI.PlugIn.PreviousDocumentsUserControl();
        this.SpecialMentionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.SpecialMentionsUserControl = new Enterprise.Customs.CH.GUI.SpecialMentionsUserControl();
        this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.SupportingDocumentsUserControl = new Enterprise.Customs.CH.GUI.SupportingDocumentsUserControl();
        this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
        this.GroupInvoiceDropEdit.SuspendLayout();
        this.NoOfPacksCalcDropEdit.SuspendLayout();
        this.GrossWeightCalcDropEdit.SuspendLayout();
        this.NetWeightCalcDropEdit.SuspendLayout();
        this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
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
        this.BottomPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.InvoiceChargesButtonPanel.SuspendLayout();
        this.GroupChargesButtonPanel.SuspendLayout();
        this.PreviousDocumentsTabPage.SuspendLayout();
        this.PreviousDocumentsUserControl.SuspendLayout();
        this.SpecialMentionsTabPage.SuspendLayout();
        this.SpecialMentionsUserControl.SuspendLayout();
        this.SupportingDocumentsTabPage.SuspendLayout();
        this.SupportingDocumentsUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // InvoiceTabControl
        // 
        this.InvoiceTabControl.Controls.Add(this.SpecialMentionsTabPage);
        this.InvoiceTabControl.Controls.Add(this.PreviousDocumentsTabPage);
        this.InvoiceTabControl.Controls.Add(this.SupportingDocumentsTabPage);
        this.InvoiceTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
        this.InvoiceTabControl.Controls.SetChildIndex(this.PreviousDocumentsTabPage, 0);
        this.InvoiceTabControl.Controls.SetChildIndex(this.SpecialMentionsTabPage, 0);
        this.InvoiceTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
        this.InvoiceTabControl.Controls.SetChildIndex(this.ComInvoiceDetailsTabPage, 0);
        // 
        // InvoiceChargesTabPage
        // 
        this.InvoiceChargesTabPage.Controls.Add(this.InvoiceChargesButtonPanel);
        this.InvoiceChargesTabPage.Controls.SetChildIndex(this.InvoiceChargesButtonPanel, 0);
        this.InvoiceChargesTabPage.Controls.SetChildIndex(this.InvoiceChargesGrid, 0);
        // 
        // InvoiceChargesGrid
        // 
        this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 82, true);
        // 
        // BaseGroupChargesGroupBox
        // 
        this.BaseGroupChargesGroupBox.Controls.Add(this.GroupChargesButtonPanel);
        this.BaseGroupChargesGroupBox.Controls.SetChildIndex(this.GroupChargesButtonPanel, 0);
        this.BaseGroupChargesGroupBox.Controls.SetChildIndex(this.BaseGroupChargesGrid, 0);
        // 
        // BaseGroupChargesGrid
        // 
        this.BaseGroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 90, true);
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
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 109, true);
        this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
        // 
        // InvCustomFieldsDisplayControl
        // 
        this.InvCustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
        // 
        // CustomFieldsTabPage
        // 
        this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
        // 
        // Splitter
        // 
        // 
        // ChargesGroupsSplitterContainer
        // 
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobDeclaration);
        // 
        // InvoiceChargesButtonPanel
        // 
        this.InvoiceChargesButtonPanel.Controls.Add(this.InvoiceChargesCalculateFreightButton);
        this.InvoiceChargesButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
        this.InvoiceChargesButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 0, true);
        this.InvoiceChargesButtonPanel.Name = "InvoiceChargesButtonPanel";
        this.InvoiceChargesButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 82, true);
        this.InvoiceChargesButtonPanel.TabIndex = 1;
        // 
        // InvoiceChargesCalculateFreightButton
        // 
        this.InvoiceChargesCalculateFreightButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("19e45aa1-1835-4012-a807-093e9db6f2da", "Calculate Freight");
        this.InvoiceChargesCalculateFreightButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
        this.InvoiceChargesCalculateFreightButton.Name = "InvoiceChargesCalculateFreightButton";
        this.InvoiceChargesCalculateFreightButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 35, true);
        this.InvoiceChargesCalculateFreightButton.TabIndex = 0;
        this.InvoiceChargesCalculateFreightButton.ToolTipCaption = null;
        this.InvoiceChargesCalculateFreightButton.UseVisualStyleBackColor = true;
        this.InvoiceChargesCalculateFreightButton.Click += new System.EventHandler(this.InvoiceChargesCalculateFreightButton_Click);
        // 
        // GroupChargesButtonPanel
        // 
        this.GroupChargesButtonPanel.Controls.Add(this.GroupChargesCalculateFreightButton);
        this.GroupChargesButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
        this.GroupChargesButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 16, true);
        this.GroupChargesButtonPanel.Name = "GroupChargesButtonPanel";
        this.GroupChargesButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 90, true);
        this.GroupChargesButtonPanel.TabIndex = 2;
        // 
        // GroupChargesCalculateFreightButton
        // 
        this.GroupChargesCalculateFreightButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("4f826fc4-2d09-4618-9948-3285c2914bce", "Calculate Freight");
        this.GroupChargesCalculateFreightButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
        this.GroupChargesCalculateFreightButton.Name = "GroupChargesCalculateFreightButton";
        this.GroupChargesCalculateFreightButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 35, true);
        this.GroupChargesCalculateFreightButton.TabIndex = 0;
        this.GroupChargesCalculateFreightButton.ToolTipCaption = null;
        this.GroupChargesCalculateFreightButton.UseVisualStyleBackColor = true;
        this.GroupChargesCalculateFreightButton.Click += new System.EventHandler(this.GroupChargesCalculateFreightButton_Click);
        // 
        // PreviousDocumentsTabPage
        // 
        this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("7dadda62-542e-4eb7-9b71-b5c3c1b4e53a", "[40] Previous Docs");
        this.PreviousDocumentsTabPage.Controls.Add(this.PreviousDocumentsUserControl);
        this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
        this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
        this.PreviousDocumentsTabPage.TabIndex = 101;
        // 
        // PreviousDocumentsUserControl
        // 
        this.PreviousDocumentsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.PreviousDocumentsUserControl, "Invoices.PreviousDocuments");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).Invoices)).SyncRoot)))));
        this.PreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.PreviousDocumentsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.PreviousDocumentsUserControl.Name = "PreviousDocumentsUserControl";
        this.PreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
        this.PreviousDocumentsUserControl.TabIndex = 0;
        // 
        // SpecialMentionsTabPage
        // 
        this.SpecialMentionsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("084c4348-c1c0-4774-9c7e-66e66432cab2", "Special Mentions");
        this.SpecialMentionsTabPage.Controls.Add(this.SpecialMentionsUserControl);
        this.SpecialMentionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.SpecialMentionsTabPage.Name = "SpecialMentionsTabPage";
        this.SpecialMentionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
        this.SpecialMentionsTabPage.TabIndex = 102;
        // 
        // SpecialMentionsUserControl
        // 
        this.SpecialMentionsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.SpecialMentionsUserControl, "Invoices");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.ISpecialMentions)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).Invoices)).SyncRoot)))));
        this.SpecialMentionsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SpecialMentionsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SpecialMentionsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
        this.SpecialMentionsUserControl.Name = "SpecialMentionsUserControl";
        this.SpecialMentionsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
        this.SpecialMentionsUserControl.TabIndex = 2;
        // 
        // SupportingDocumentsTabPage
        // 
        this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("E172887C-D341-4FC6-A92D-DC5786914285", "[44] Supporting Documents");
        this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsUserControl);
        this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
        this.SupportingDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
        this.SupportingDocumentsTabPage.TabIndex = 2;
        // 
        // SupportingDocumentsUserControl
        // 
        this.SupportingDocumentsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.SupportingDocumentsUserControl, "Invoices.SupportingDocuments");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.SupportingDocument)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)).SyncRoot)))));
        this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
        this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 237, true);
        this.SupportingDocumentsUserControl.TabIndex = 0;
        // 
        // BaseCustomsSupplierHeaderUserControl
        // 
        this.Name = "BaseCustomsSupplierHeaderUserControl";
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
        this.BottomPanel.ResumeLayout(false);
        this.BottomPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.InvoiceChargesButtonPanel.ResumeLayout(false);
        this.InvoiceChargesButtonPanel.PerformLayout();
        this.GroupChargesButtonPanel.ResumeLayout(false);
        this.GroupChargesButtonPanel.PerformLayout();
        this.PreviousDocumentsTabPage.ResumeLayout(false);
        this.PreviousDocumentsTabPage.PerformLayout();
        this.PreviousDocumentsUserControl.ResumeLayout(true);
        this.PreviousDocumentsUserControl.PerformLayout();
        this.SpecialMentionsTabPage.ResumeLayout(false);
        this.SpecialMentionsTabPage.PerformLayout();
        this.SpecialMentionsUserControl.ResumeLayout(true);
        this.SpecialMentionsUserControl.PerformLayout();
        this.SupportingDocumentsTabPage.ResumeLayout(false);
        this.SupportingDocumentsTabPage.PerformLayout();
        this.SupportingDocumentsUserControl.ResumeLayout(true);
        this.SupportingDocumentsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private PlugIn.PreviousDocumentsUserControl PreviousDocumentsUserControl;
    internal ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
    internal ZArchitecture.GUI.ZTabPage SpecialMentionsTabPage;
    internal SpecialMentionsUserControl SpecialMentionsUserControl;
    internal ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
    private SupportingDocumentsUserControl SupportingDocumentsUserControl;
    private Enterprise.ZArchitecture.GUI.ZPanel InvoiceChargesButtonPanel;
    private Enterprise.ZArchitecture.GUI.ZPanel GroupChargesButtonPanel;
    internal ZArchitecture.GUI.ZButton InvoiceChargesCalculateFreightButton;
    internal ZArchitecture.GUI.ZButton GroupChargesCalculateFreightButton;
}
