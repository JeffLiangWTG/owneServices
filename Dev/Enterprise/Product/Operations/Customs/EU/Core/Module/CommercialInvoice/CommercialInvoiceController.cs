using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.CommercialInvoice;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
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
