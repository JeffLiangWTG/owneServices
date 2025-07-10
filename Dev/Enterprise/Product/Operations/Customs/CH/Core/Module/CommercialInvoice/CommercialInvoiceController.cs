using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.Module;

public class CommercialInvoiceController : Customs.Module.CommercialInvoiceController
{
	public CommercialInvoiceController()
	{
	}

	protected override IZForm GetForm(IBusiness businessEntity) => new CommercialInvoiceForm((JobComInvoiceHeader)businessEntity);
}
