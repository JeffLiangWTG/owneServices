using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DocStlRawUsage : DocBaseWrapper
	{
		protected DocStlRawUsage(StlRawUsage rawUsage, BusinessObjectFactory factory)
			: base(rawUsage, factory)
		{
			AddUsageSummarySections();
		}

		public static DocStlRawUsage New(StlRawUsage rawUsage, BusinessObjectFactory factory)
		{
			return (rawUsage != null) ? new DocStlRawUsage(rawUsage, factory) : null;
		}

		StlRawUsage RawUsage
		{
			get { return (StlRawUsage)WrappedObject; }
		}

		readonly Dictionary<int, SummaryLineCollection> SummaryLineCollections = new Dictionary<int, SummaryLineCollection>();

		SummaryLineCollection GetSummaryLineCollection(int numberOfColumnsUsed) => SummaryLineCollections.GetOrAdd(numberOfColumnsUsed, () => new SummaryLineCollection(Factory));

		#region Add Usage Sections

		void AddUsageSummarySections()
		{
			foreach (SummarySection section in RawUsage.GetRawUsageSummarySections())
			{
				GetSummaryLineCollection(section.NumberOfColumnsUsed).PopulateFromSummarySection(section);
			}
		}

		#endregion

		#region Usage Summary Lines

		public SummaryLineCollection UsageLines1 => GetSummaryLineCollection(1);
		public SummaryLineCollection UsageLines2 => GetSummaryLineCollection(2);
		public SummaryLineCollection UsageLines3 => GetSummaryLineCollection(3);
		public SummaryLineCollection UsageLines4 => GetSummaryLineCollection(4);
		public SummaryLineCollection UsageLines5 => GetSummaryLineCollection(5);
		public SummaryLineCollection UsageLines6 => GetSummaryLineCollection(6);
		public SummaryLineCollection UsageLines7 => GetSummaryLineCollection(7);
		public SummaryLineCollection UsageLines8 => GetSummaryLineCollection(8);
		public SummaryLineCollection UsageLines9 => GetSummaryLineCollection(9);
		public SummaryLineCollection UsageLines10 => GetSummaryLineCollection(10);

		#endregion

		#region Properties

		public ZString ServerCode
		{
			get { return RawUsage.ServerCode; }
		}

		public ZString PriceItemDescription
		{
			get { return RawUsage.PriceItemDescription.Trim(); }
		}

		public ZString Period
		{
			get { return RawUsage.PeriodStart.ToString("MMM yyyy", CultureInfo.InvariantCulture); }
		}

		#endregion
	}
}

