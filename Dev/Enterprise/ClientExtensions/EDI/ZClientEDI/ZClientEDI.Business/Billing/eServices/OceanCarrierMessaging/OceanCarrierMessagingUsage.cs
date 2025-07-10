using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class OceanCarrierMessagingUsage : EServicesSystemUsage
	{
		public OceanCarrierMessagingUsage(ZString subCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(subCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override ZString SystemCode
		{
			get { return BillingConstants.BillingSystem.OceanCarrierMessaging; }
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = base.GetGeneralSummarySections()[0];
			var caption = OceanCarrierMessagingBillingSystem.GetUsageCaptions().FirstOrDefault(x => x.UsageCode == SubCode);
			result.Lines[0].MainDescription = caption?.UsageDescription ?? SubCode;
			return new SummarySection[] { result };
		}

		#endregion
	}
}

