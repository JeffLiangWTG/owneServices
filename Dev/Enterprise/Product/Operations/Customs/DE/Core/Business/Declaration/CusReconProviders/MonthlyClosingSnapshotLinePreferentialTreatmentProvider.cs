using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.Declaration
{
	class MonthlyClosingSnapshotLinePreferentialTreatmentProvider : ILinePreferentialTreatment
	{
		public MonthlyClosingSnapshotLinePreferentialTreatmentProvider(string requestedPreferentialTreatment)
		{
			RequestedPreferentialTreatment = requestedPreferentialTreatment;
		}

		public string RequestedPreferentialTreatment { get; }

		public IReadOnlyCollection<string> ContingentNumber => Array.Empty<string>();

		public IAmount Quantity => null;
	}
}
