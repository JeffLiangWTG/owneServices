using System;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	[Flags]
	public enum IncidentStatus
	{
		Open = 1,
		Closed = 2
	}

	public enum SimilarIncidentSearchOptionsRestriction
	{
		None,
		Date,
		Full,
	}

	public class SimilarIncidentSearchOptions
	{
		readonly int earliestYear = 2000;

		public ZGuid SourceIncidentPK { get; set; }

		public DateTime? FromTime { get; set; } = DateTime.UtcNow.AddDays(-7.0);
		public DateTime? ToTime { get; set; } = DateTime.UtcNow;

		public IncidentStatus IncidentStatus { get; set; } = IncidentStatus.Open | IncidentStatus.Closed;
		public ZGuid? CompanyGuid { get; set; }

		public double? MinimumSimilarity { get; set; }  // On the interval [-1, +1]

		public int StartIndex { get; set; }
		public int Count { get; set; } = 10;
		public ZString? Product { get; internal set; }
		public ZString? ProductArea { get; set; }

		public SimilarIncidentSearchOptionsRestriction GetRestriction()
		{
			if (IncidentStatus != (IncidentStatus.Open | IncidentStatus.Closed) || CompanyGuid.HasValue || Product.HasValue || ProductArea.HasValue)
			{
				return SimilarIncidentSearchOptionsRestriction.Full;
			}

			if ((FromTime.HasValue && FromTime.Value.Year >= earliestYear) || (ToTime.HasValue && ToTime.Value < DateTime.UtcNow.AddMinutes(-10)))
			{
				return SimilarIncidentSearchOptionsRestriction.Date;
			}

			return SimilarIncidentSearchOptionsRestriction.None;
		}
	}
}
