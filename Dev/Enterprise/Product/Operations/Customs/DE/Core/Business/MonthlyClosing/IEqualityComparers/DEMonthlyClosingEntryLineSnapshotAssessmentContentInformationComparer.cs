using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer : IEqualityComparer<DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation>
	{
		public bool Equals(DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation px, DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.Type, y.Type)
			&& x.DegreePercentage == y.DegreePercentage);

		public int GetHashCode(DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation obj) => 0;
	}
}
