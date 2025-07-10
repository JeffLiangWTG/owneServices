using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class CommercialInvoiceFormLayoutProvider : Customs.GUI.ICommercialInvoiceFormLayoutProvider
	{
		public IPanelLayoutProvider GetInvoiceHeaderDetailsLayout(Customs.Business.BaseJobComInvoiceHeader invoice) => new CommercialInvoice.InvoiceHeaderDetailsLayout();
	}
}
