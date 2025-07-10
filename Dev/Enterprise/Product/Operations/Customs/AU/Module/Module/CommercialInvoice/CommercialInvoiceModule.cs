using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class CommercialInvoiceModule : Customs.Module.CommercialInvoiceModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CommercialInvoiceFilterBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new CommercialInvoiceController();
		}
	}
}
