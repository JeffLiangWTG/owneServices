using System.Linq;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class MultiSectionStlRawUsage : StlRawUsage
	{
		public MultiSectionStlRawUsage(BillingLoadRawUsageContext context, ZString systemCode)
			: base(context, systemCode)
		{ }

		public override SummarySection[] GetRawUsageSummarySections()
		{
			ZString desc = !SummaryHeaderDescription.IsEmpty ? SummaryHeaderDescription : DefaultSummaryHeaderDescription;
			foreach (var section in SummarySections)
			{
				section.Header.TopLevelDescription = desc + section.Header.TopLevelDescription;
			}
			return SummarySections.ToArray();
		}
	}
}

