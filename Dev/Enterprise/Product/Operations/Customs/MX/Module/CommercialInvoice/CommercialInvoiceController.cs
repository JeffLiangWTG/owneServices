using CargoWise.EntityFramework;
using Enterprise.Customs.MX.Business;
using Enterprise.Customs.MX.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
	{
		public CommercialInvoiceController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity) => new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
	}
}
