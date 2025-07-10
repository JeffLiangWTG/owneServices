using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
	{
		public CommercialInvoiceController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity) => new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
	}
}
