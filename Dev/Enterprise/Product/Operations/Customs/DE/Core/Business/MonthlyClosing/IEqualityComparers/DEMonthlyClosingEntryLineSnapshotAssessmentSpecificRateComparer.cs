using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer : IEqualityComparer<DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate>
	{
		public bool Equals(DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate px, DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.Type, y.Type)
			&& x.Value == y.Value);

		public int GetHashCode(DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate obj) => 0;
	}
}
