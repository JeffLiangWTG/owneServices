using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class ShippingPortMessagingUsage : EServicesSystemUsage
	{
		public ShippingPortMessagingUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(subCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.ShippingPortMessaging; }
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = base.GetGeneralSummarySections()[0];
			if (PriceItem != null)
			{
				result.Lines[0].MainDescription = PriceItem.L7_DescriptionLocalized;
			}
			return new SummarySection[] { result };
		}

		#endregion
	}
}

