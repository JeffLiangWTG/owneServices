using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlRawUsage : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StlRawUsage(BillingLoadRawUsageContext context)
			: base(context.Factory)
		{
			this.context = context;
		}

		public StlRawUsage(BillingLoadRawUsageContext context, ZString systemCode)
			: this(context)
		{
			this.systemCode = systemCode;
		}

		readonly BillingLoadRawUsageContext context;
		readonly ZString systemCode;

		public ZDateTime PeriodStart
		{
			get { return context.Period; }
		}

		public ZString DatabaseId
		{
			get { return context.DatabaseId; }
		}

		public ZString ServerCode
		{
			get { return context.ServerCode; }
		}

		public ZString PriceItemDescription
		{
			get { return context.PriceItemDescription; }
		}

		public void MergeSummarySections(StlRawUsage anotherRawUsage)
		{
			var summarySectionLookup = new HashSet<SummarySection>(SummarySections);

			foreach (var summarySection in anotherRawUsage.GetRawUsageSummarySections().Distinct().Where(x => !summarySectionLookup.Contains(x)))
			{
				SummarySections.Add(summarySection);
			}
		}

		#region Summary Section

		public virtual SummarySection[] GetRawUsageSummarySections()
		{
			if (!SummaryHeaderDescription.IsEmpty || !systemCode.IsEmpty)
			{
				Summary.Header.TopLevelDescription = !SummaryHeaderDescription.IsEmpty ? SummaryHeaderDescription : DefaultSummaryHeaderDescription;
			}
			return SummarySections.ToArray();
		}

		public ZString SummaryHeaderDescription { get; set; }

		protected ZString DefaultSummaryHeaderDescription
		{
			get { return (Res.GetString("7516fb76-1c37-4276-9ebe-738191082f88", "{0} Usage Summary", BillingConstants.BillingSystemList.GetDescriptionFromCode(systemCode)).Trim()); }
		}

		public SummarySection Summary
		{
			get
			{
				if (SummarySections.Count == 0)
				{
					var summary = new SummarySection(Factory);
					SummarySections.Add(summary);
					return summary;
				}
				else
				{
					return SummarySections[0];
				}
			}
		}

		public Collection<SummarySection> SummarySections
		{
			get { return summarySections ?? (summarySections = new Collection<SummarySection>()); }
		}
		Collection<SummarySection> summarySections;

		#endregion
	}
}

