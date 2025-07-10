using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class InvoiceLineExportSupportingDocumentsFieldsUserControl : BaseCustomsEntryUserControl
	{
		public InvoiceLineExportSupportingDocumentsFieldsUserControl()
		{
			InitializeComponent();
			PopulateDynamicLayoutPanel();
		}

		void PopulateDynamicLayoutPanel()
		{
			InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel.UpdateLayout(new InvoiceLineExportSupportingDocumentsFieldsLayout());
		}
	}
}
