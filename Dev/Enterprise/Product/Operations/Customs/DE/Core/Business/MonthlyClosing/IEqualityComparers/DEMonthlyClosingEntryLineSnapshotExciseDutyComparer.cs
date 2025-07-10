using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class DEMonthlyClosingEntryLineSnapshotExciseDutyComparer : IEqualityComparer<DEMonthlyClosingEntryLineSnapshotExciseDuty>
	{
		public bool Equals(DEMonthlyClosingEntryLineSnapshotExciseDuty px, DEMonthlyClosingEntryLineSnapshotExciseDuty py) => ComparerHelper.Compare(px, py, (x, y) =>
				x.Value == y.Value
				&& string.Equals(x.Code, y.Code)
				&& x.DegreePercentage == y.DegreePercentage
				&& new AmountComparer().Equals(x.Amount, y.Amount));

		public int GetHashCode(DEMonthlyClosingEntryLineSnapshotExciseDuty obj) => 0;
	}
}
