using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class LinePreferentialTreatmentFromSnapshotProvider : ILinePreferentialTreatment
	{
		public static ILinePreferentialTreatment NewOrNull(DEMonthlyClosingEntryLineSnapshotPreferentialTreatment linePreferentialTreatment) => linePreferentialTreatment == null ? null : (ILinePreferentialTreatment)new LinePreferentialTreatmentFromSnapshotProvider(linePreferentialTreatment);

		LinePreferentialTreatmentFromSnapshotProvider(DEMonthlyClosingEntryLineSnapshotPreferentialTreatment linePreferentialTreatment)
		{
			this.linePreferentialTreatment = Argument.NotNull(linePreferentialTreatment, nameof(linePreferentialTreatment));
		}

		readonly DEMonthlyClosingEntryLineSnapshotPreferentialTreatment linePreferentialTreatment;

		public string RequestedPreferentialTreatment => linePreferentialTreatment.RequestedPreferentialTreatment;

		public IReadOnlyCollection<string> ContingentNumber => contingentNumber ?? (contingentNumber = linePreferentialTreatment.Declaration?.Contingent?.Select(c => c.ContingentNumber).ToArray() ?? Array.Empty<string>());
		IReadOnlyCollection<string> contingentNumber;

		public IAmount Quantity => CachedValueHelper.GetValue(ref quantity, () => AmountFromSnapshotProvider.NewOrNull(linePreferentialTreatment.Declaration?.PreferentialTreatmentQuantity));
		CachedValue<IAmount> quantity;
	}
}
