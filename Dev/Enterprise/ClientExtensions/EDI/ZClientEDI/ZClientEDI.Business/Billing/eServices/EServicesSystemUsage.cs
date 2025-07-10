using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class EServicesSystemUsage : PriceItemUsage
	{
		public EServicesSystemUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: this("", "", factory, user, periodStart)
		{
		}

		public EServicesSystemUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: this("", subCode, factory, user, periodStart)
		{
		}

		public EServicesSystemUsage(ZString systemCode, ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(factory, user, periodStart, systemCode, true)
		{
			SubCode = subCode;
			PriceHeaderCode = BillingConstants.PriceHeaderType.Other;
		}

		#region Price

		public override ZString PriceItemCode
		{
			get { return SubCode; }
		}

		#endregion
	}
}

