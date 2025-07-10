using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.AE.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Module;

public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
{
	protected override IZForm GetForm(IBusiness businessEntity)
	{
		return new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
	}
}
