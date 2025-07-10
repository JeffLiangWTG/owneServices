using CargoWise.EntityFramework;
using Enterprise.Customs.MY.Business;
using Enterprise.Customs.MY.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MY.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
	{
		public CommercialInvoiceController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity) => new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
	}
}
