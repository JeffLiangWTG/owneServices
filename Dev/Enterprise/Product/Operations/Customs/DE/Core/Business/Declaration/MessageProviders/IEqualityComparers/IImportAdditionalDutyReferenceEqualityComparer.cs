using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IImportAdditionalDutyReferenceEqualityComparer : IEqualityComparer<IImportAdditionalDutyReference>
	{
		public bool Equals(IImportAdditionalDutyReference px, IImportAdditionalDutyReference py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.ReferenceNumber, y.ReferenceNumber) &&
			x.DutyInterestedPartyPK == y.DutyInterestedPartyPK);

		public int GetHashCode(IImportAdditionalDutyReference obj) => 0;
	}
}
