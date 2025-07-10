using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparer : IEqualityComparer<DEMonthlyClosingEntryLineSnapshotSupplementaryCodes>
	{
		public bool Equals(DEMonthlyClosingEntryLineSnapshotSupplementaryCodes px, DEMonthlyClosingEntryLineSnapshotSupplementaryCodes py) => ComparerHelper.Compare(px, py, (x, y) => string.Equals(x.Code, y.Code));

		public int GetHashCode(DEMonthlyClosingEntryLineSnapshotSupplementaryCodes obj) => 0;
	}
}
