using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module
{
	public class CommercialInvoiceController : EU.Module.CommercialInvoiceController
	{
		public CommercialInvoiceController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
		}
	}
}
