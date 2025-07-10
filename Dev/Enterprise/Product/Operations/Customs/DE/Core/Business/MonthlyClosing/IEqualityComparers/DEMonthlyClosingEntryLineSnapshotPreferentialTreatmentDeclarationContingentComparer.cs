using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparer : IEqualityComparer<DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent>
	{
		public bool Equals(DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent px, DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent py) =>
			ComparerHelper.Compare(px, py, (x, y) => string.Equals(x.ContingentNumber, y.ContingentNumber));

		public int GetHashCode(DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent obj) => 0;
	}
}
