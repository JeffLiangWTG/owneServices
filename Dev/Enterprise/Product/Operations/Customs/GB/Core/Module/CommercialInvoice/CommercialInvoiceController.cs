using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module
{
	public class CommercialInvoiceController : EU.Module.CommercialInvoiceController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GUI.CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
		}
	}
}
