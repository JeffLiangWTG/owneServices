using CargoWise.EntityFramework;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Module;

public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
{
	public CommercialInvoiceController()
	{
	}

	protected override IZForm GetForm(IBusiness businessEntity) => new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
}
