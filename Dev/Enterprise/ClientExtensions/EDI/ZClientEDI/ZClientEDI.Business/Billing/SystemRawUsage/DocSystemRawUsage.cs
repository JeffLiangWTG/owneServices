using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DocSystemRawUsage : DocBaseWrapper
	{
		protected DocSystemRawUsage(SystemRawUsage rawUsage, BusinessObjectFactory factory)
			: base(rawUsage, factory)
		{
			AddUsageSummarySections();
		}

		public static DocSystemRawUsage New(SystemRawUsage rawUsage, BusinessObjectFactory factory)
		{
			return (rawUsage != null) ? new DocSystemRawUsage(rawUsage, factory) : null;
		}

		SystemRawUsage RawUsage
		{
			get { return (SystemRawUsage)WrappedObject; }
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

		public ZString OrgCode
		{
			get { return RawUsage.OrgCode; }
		}

		public ZString OrgName
		{
			get { return RawUsage.OrgName; }
		}

		public ZString UsageCompanyCode
		{
			get { return RawUsage.CompanyCode; }
		}

		public ZString UsageServerCode
		{
			get { return RawUsage.ServerCode; }
		}

		public ZString Period
		{
			get { return RawUsage.PeriodStart.ToString("MMM yyyy", CultureInfo.InvariantCulture); }
		}

		#endregion
	}
}

