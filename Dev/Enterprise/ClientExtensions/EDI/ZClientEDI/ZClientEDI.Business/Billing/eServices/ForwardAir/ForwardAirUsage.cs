using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class ForwardAirUsage : EServicesSystemUsage
	{
		public ForwardAirUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(BillingConstants.BillingSystem.ForwardAir, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.ForwardAir; }
		}

		public override ZString PriceItemCode
		{
			get { return "ECI"; }
		}
	}
}

