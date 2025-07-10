using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.Billing.BillingPrices
{
	[ModuleID("BillingPrices")]
	public class BillingPricesCollection : ActiveBusinessObjectCollection<ClientLicencePriceItem>
	{
		public BillingPricesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BillingPricesCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}



