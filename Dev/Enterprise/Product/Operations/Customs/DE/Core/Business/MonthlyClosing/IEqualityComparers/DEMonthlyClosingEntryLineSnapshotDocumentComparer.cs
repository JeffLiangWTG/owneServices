using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class DEMonthlyClosingEntryLineSnapshotDocumentComparer : IEqualityComparer<DEMonthlyClosingEntryLineSnapshotDocument>
	{
		public bool Equals(DEMonthlyClosingEntryLineSnapshotDocument px, DEMonthlyClosingEntryLineSnapshotDocument py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.ReferenceNumber, y.ReferenceNumber)
			&& x.Division == y.Division
			&& x.IssuingDate == y.IssuingDate
			&& string.Equals(x.AtHandFlag, y.AtHandFlag)
			&& new AmountComparer().Equals(x.WriteOff, y.WriteOff)
			&& string.Equals(x.Type, y.Type));

		public int GetHashCode(DEMonthlyClosingEntryLineSnapshotDocument obj) => 0;
	}
}
