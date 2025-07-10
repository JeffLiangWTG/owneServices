using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class CommercialInvoiceFormLayoutProvider : Customs.GUI.ICommercialInvoiceFormLayoutProvider
	{
		public IPanelLayoutProvider GetInvoiceHeaderDetailsLayout(Customs.Business.BaseJobComInvoiceHeader invoice)
		{
			IPanelLayoutProvider result;
			if (invoice.IsExport)
			{
				result = new ExportInvoiceDetailsLayout();
			}
			else
			{
				result = new EU.GUI.CommercialInvoice.InvoiceHeaderDetailsLayout();
			}
			return result;
		}
	}
}
