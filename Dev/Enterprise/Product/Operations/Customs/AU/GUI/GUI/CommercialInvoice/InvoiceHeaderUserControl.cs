using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.GUI.CommercialInvoice
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
		}

		public JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.Invoice;

		protected override void ChangeControlsVisibilityWhenMessageTypeChanges()
		{
			base.ChangeControlsVisibilityWhenMessageTypeChanges();

			zA_VALB_HiddenDropEdit.Visible = Invoice.IsImport;
			zA_HeaderREL_HiddenDropEdit.Visible = Invoice.IsImport;
			pOCCodeFindBox.Visible = Invoice.IsImport;
			preferenceSchemeTypeDropDown.Visible = Invoice.IsImport;
			preferenceRuleTypeDropEdit.Visible = Invoice.IsImport;
			zA_GSTECodeFindBox.Visible = Invoice.IsImport;
			jZ_AddInfoBoundAddInfoControl.Visible = Invoice.IsImport;
			invoiceOriginCodeFindBox.Visible = Invoice.IsImport;
			exporterReferenceTextBox.Visible = InvoiceHeader.IsQuarantine;
		}
	}
}
