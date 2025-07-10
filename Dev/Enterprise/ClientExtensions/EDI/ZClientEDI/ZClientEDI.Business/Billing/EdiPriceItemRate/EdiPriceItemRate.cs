using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	[DependentBusinessObject(typeof(ClientLicencePriceItem), "Rates")]
	public class EdiPriceItemRate : AutoEdiPriceItemRate
	{
		public EdiPriceItemRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ClientLicencePriceItem PriceItem
		{
			get { return Factory.Load<ClientLicencePriceItem>(PIR_L7); }
		}
	}
}

