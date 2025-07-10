using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class MultiSectionSystemRawUsage : SystemCodeRawUsage
	{
		public MultiSectionSystemRawUsage(BillingLoadRawUsageContext context, ZString systemCode)
			: base(context, systemCode)
		{
			SectionHeadersInitialized = false;
		}

		bool SectionHeadersInitialized { get; set; }

		public List<SummarySection> SummarySections
		{
			get { return summarySections ?? (summarySections = new List<SummarySection>(1)); }
		}
		List<SummarySection> summarySections;

		public override SummarySection[] GetRawUsageSummarySections()
		{
			if (!SectionHeadersInitialized)
			{
				ZString desc = !SummaryHeaderDescription.IsEmpty ? SummaryHeaderDescription : DefaultSummaryHeaderDescription;
				foreach (var section in SummarySections)
				{
					section.Header.TopLevelDescription = desc + section.Header.TopLevelDescription;
				}
				SectionHeadersInitialized = true;
			}
			return SummarySections.ToArray();
		}
	}
}

