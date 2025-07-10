using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class CommercialInvoiceModule : Customs.Module.CommercialInvoiceModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CommercialInvoiceFilterBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new CommercialInvoiceController();
	}
}
