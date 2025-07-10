namespace Enterprise.Customs.IN.GUI;

partial class BaseCustomsSupplierHeaderUserControl
{
	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.SupportingDocumentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.SupportingDocumentUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		this.DetailsGroupBox.SuspendLayout();
		this.InvoiceDetailUserControl.SuspendLayout();
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
		this.SupportingDocumentTabPage.SuspendLayout();
		this.SuspendLayout();
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
		// Splitter
		// 
		// 
		// ChargesGroupsSplitterContainer
		// 
		// 
		// InvoiceTabControl
		// 
		this.InvoiceTabControl.Controls.Add(this.SupportingDocumentTabPage);
		//
		// SupportingDocumentTabPage
		//
		this.SupportingDocumentTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("0c04ba9e-75c4-4a09-83c8-d9f6ca78957d", "Supporting Documents");
		this.SupportingDocumentTabPage.Controls.Add(this.SupportingDocumentUserControl);
		this.SupportingDocumentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.SupportingDocumentTabPage.Name = "SupportingDocumentTabPage";
		this.SupportingDocumentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.SupportingDocumentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 274, true);
		this.SupportingDocumentTabPage.TabIndex = 0;
		this.SupportingDocumentTabPage.UseVisualStyleBackColor = true;
		//
		// SupportingDocumentUserControl
		//
		this.SupportingDocumentUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SupportingDocumentUserControl, "Invoices.SupportingDocuments");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.IN.Business.SupportingDocumentCollection)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).SupportingDocuments)));
		this.SupportingDocumentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SupportingDocumentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
		this.SupportingDocumentUserControl.Name = "SupportingDocumentUserControl";
		this.SupportingDocumentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 268, true);
		this.SupportingDocumentUserControl.TabIndex = 0;
		// 
		// BaseCustomsSupplierHeaderUserControl
		// 
		this.Name = "BaseCustomsSupplierHeaderUserControl";
		this.DetailsGroupBox.ResumeLayout(false);
		this.DetailsGroupBox.PerformLayout();
		this.InvoiceDetailUserControl.ResumeLayout(true);
		this.InvoiceDetailUserControl.PerformLayout();
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
		this.SupportingDocumentTabPage.ResumeLayout(false);
		this.SupportingDocumentTabPage.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion
	
	protected ZArchitecture.GUI.ZTabPage SupportingDocumentTabPage;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl SupportingDocumentUserControl;
}
