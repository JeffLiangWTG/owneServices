using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.Hosting
{
	public class HostingDataAccessBill : HostingBill
	{
		public HostingDataAccessBill(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override SummarySection[] GetGroupSummarySections()
		{
			return new[] { GetGroupSummarySection(SystemDescription + " Group Summary", delegate(SystemUsage x) { return x.Amount; }) };
		}

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			return BuildGeneralSummarySections(organisationPK, true);
		}
	}
}

