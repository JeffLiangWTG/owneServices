using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.GUI.CommercialInvoice;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
	{
		public CommercialInvoiceController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity) => new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
	}
}
