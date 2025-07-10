using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class OceanTracingUsage : EServicesSystemUsage
	{
		public OceanTracingUsage(ZString systemCode, BusinessObjectFactory factory, IUsingParty user, ZDateTime periodStart)
			: base(systemCode, systemCode, factory, user, periodStart)
		{
			PriceHeaderCode = BillingConstants.BillingSystem.ODM;
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections()
		{
			var result = base.GetGeneralSummarySections()[0];
			result.Lines[0].MainDescription = HasPriceItem ? PriceItem.L7_DescriptionLocalized : SystemDescription;
			return new SummarySection[] { result };
		}

		#endregion
	}
}

