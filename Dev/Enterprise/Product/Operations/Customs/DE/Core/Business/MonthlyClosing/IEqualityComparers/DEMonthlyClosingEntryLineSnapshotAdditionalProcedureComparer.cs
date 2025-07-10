using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparer : IEqualityComparer<DEMonthlyClosingEntryLineSnapshotAdditionalProcedure>
	{
		public bool Equals(DEMonthlyClosingEntryLineSnapshotAdditionalProcedure px, DEMonthlyClosingEntryLineSnapshotAdditionalProcedure py) => ComparerHelper.Compare(px, py, (x, y) => string.Equals(x.Code, y.Code));

		public int GetHashCode(DEMonthlyClosingEntryLineSnapshotAdditionalProcedure obj) => 0;
	}
}
