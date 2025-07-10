namespace Enterprise.Customs.ES.GUI;

partial class ImportSupplierHeaderUserControl
{
	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			AdditionalDocumentsTabPage?.Dispose();
			additionalDocumentsUserControl?.Dispose();
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
		this.AdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.additionalDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
		this.GroupInvoiceDropEdit.SuspendLayout();
		this.NoOfPacksCalcDropEdit.SuspendLayout();
		this.GrossWeightCalcDropEdit.SuspendLayout();
		this.NetWeightCalcDropEdit.SuspendLayout();
		this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
		this.JZ_ValuationCodeDropEdit.SuspendLayout();
		this.JZ_InvoiceDateEdit.SuspendLayout();
		this.AgreedPlaceCodeFindBox.SuspendLayout();
		this.HeaderDescriptionsTabPage.SuspendLayout();
		this.SupportingDocumentsTabPage.SuspendLayout();
		this.InvoicePaymentTabPage.SuspendLayout();
		this.AdditionalDocumentsTabPage.SuspendLayout();
		this.AdditionalInfoTabPage.SuspendLayout();
		this.PreviousDocumentsTabPage.SuspendLayout();
		this.ValueIndicatorsTabPage.SuspendLayout();
		this.previousDocumentsUserControl1.SuspendLayout();
		this.ValueIndicatorsUserControl.SuspendLayout();
		this.GroupChargesButton.SuspendLayout();
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
		this.SuspendLayout();
		// 
		// HeaderDescriptionsTabPage
		// 
		this.HeaderDescriptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
		// 
		// SupportingDocumentsTabPage
		// 
		this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
		// 
		// InvoicePaymentTabPage
		// 
		this.InvoicePaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
		// 
		// AdditionalDocumentsTabPage
		// 
		this.AdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("4FEAD6AE-E0CF-47A6-8862-1AA3D74C2F86", "Additional Documents");
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
		// AdditionalInfoTabPage
		// 
		this.AdditionalInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
		// 
		// PreviousDocumentsTabPage
		// 
		this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
		// 
		// ValueIndicatorsTabPage
		// 
		this.ValueIndicatorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
		// 
		// previousDocumentsUserControl1
		// 
		this.previousDocumentsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 352, true);
		// 
		// ValueIndicatorsUserControl
		// 
		this.ValueIndicatorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 352, true);
		// 
		// GroupChargesButton
		// 
		this.GroupChargesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 146, true);
		// 
		// ComInvoiceDetailsTabPage
		// 
		this.ComInvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
		// 
		// ChargesGroupBox
		// 
		this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 187, true);
		// 
		// ChargesTabControl
		// 
		this.ChargesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 168, true);
		// 
		// InvoiceChargesTabPage
		// 
		this.InvoiceChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 141, true);
		// 
		// InvoiceChargesGrid
		// 
		this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 141, true);
		// 
		// ApportionedTabPage
		// 
		this.ApportionedTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 141, true);
		// 
		// ApportionedChargesGrid
		// 
		this.ApportionedChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 141, true);
		// 
		// BaseGroupChargesGroupBox
		// 
		this.BaseGroupChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 165, true);
		// 
		// BaseGroupChargesGrid
		// 
		this.BaseGroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 146, true);
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
		this.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutuPuRmiBKZyWIsTINXw2I/g==";
		this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
		this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
		this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 109, true);
		this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
		// 
		// Splitter
		// 
		// 
		// InvDetailLeftPanel
		// 
		this.InvDetailLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 358, true);
		// 
		// InvDetailRightPanel
		// 
		this.InvDetailRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 358, true);
		// 
		// ChargesGroupsSplitterContainer
		// 
		this.ChargesGroupsSplitterContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 358, true);
		this.ChargesGroupsSplitterContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(187);
		// 
		// ImportSupplierHeaderUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Name = "ImportSupplierHeaderUserControl";
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
		this.JZ_ValuationCodeDropEdit.ResumeLayout(true);
		this.JZ_ValuationCodeDropEdit.PerformLayout();
		this.JZ_InvoiceDateEdit.ResumeLayout(true);
		this.JZ_InvoiceDateEdit.PerformLayout();
		this.AgreedPlaceCodeFindBox.ResumeLayout(true);
		this.AgreedPlaceCodeFindBox.PerformLayout();
		this.HeaderDescriptionsTabPage.ResumeLayout(false);
		this.HeaderDescriptionsTabPage.PerformLayout();
		this.SupportingDocumentsTabPage.ResumeLayout(false);
		this.SupportingDocumentsTabPage.PerformLayout();
		this.InvoicePaymentTabPage.ResumeLayout(false);
		this.InvoicePaymentTabPage.PerformLayout();
		this.AdditionalDocumentsTabPage.ResumeLayout(false);
		this.AdditionalDocumentsTabPage.PerformLayout();
		this.AdditionalInfoTabPage.ResumeLayout(false);
		this.AdditionalInfoTabPage.PerformLayout();
		this.PreviousDocumentsTabPage.ResumeLayout(false);
		this.PreviousDocumentsTabPage.PerformLayout();
		this.ValueIndicatorsTabPage.ResumeLayout(false);
		this.ValueIndicatorsTabPage.PerformLayout();
		this.previousDocumentsUserControl1.ResumeLayout(true);
		this.previousDocumentsUserControl1.PerformLayout();
		this.ValueIndicatorsUserControl.ResumeLayout(true);
		this.ValueIndicatorsUserControl.PerformLayout();
		this.GroupChargesButton.ResumeLayout(false);
		this.GroupChargesButton.PerformLayout();
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
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalDocumentsTabPage;
	private Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl additionalDocumentsUserControl;
}
