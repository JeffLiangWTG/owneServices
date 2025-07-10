using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class DEMonthlyClosingEntrySnapshotDocumentEqualityComparer : IEqualityComparer<DEMonthlyClosingEntrySnapshotDocument>
	{
		public bool Equals(DEMonthlyClosingEntrySnapshotDocument x, DEMonthlyClosingEntrySnapshotDocument y) => ComparerHelper.Compare(x, y, (a, b) =>
			string.Equals(a.ReferenceNumber, b.ReferenceNumber) &&
			a.Division == b.Division &&
			a.IssuingDate == b.IssuingDate &&
			string.Equals(a.Type, b.Type));

		public int GetHashCode(DEMonthlyClosingEntrySnapshotDocument obj) => 0;
	}
}
