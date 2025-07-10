using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI.CommercialInvoice;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
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
