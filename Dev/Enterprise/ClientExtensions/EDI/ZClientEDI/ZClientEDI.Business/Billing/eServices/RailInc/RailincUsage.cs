using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class RailincUsage : EServicesSystemUsage
	{
		public RailincUsage(BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(BillingConstants.BillingSystem.RailincByMessage, "RIC", factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = base.GetGeneralSummarySections()[0];
			result.Header.MainDescription = PriceItem != null ? PriceItem.L7_DescriptionLocalized.Trim() : SystemDescription;
			result.Lines[0].MainDescription = "RAILINC Usage";
			return new SummarySection[] { result };
		}
	}
}

