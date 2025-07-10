using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IImportSpecialCaseEqualityComparer : IEqualityComparer<IImportSpecialCase>
	{
		public bool Equals(IImportSpecialCase a, IImportSpecialCase b) => ComparerHelper.Compare(a, b, (x, y) =>
			string.Equals(x.Group, y.Group) &&
			string.Equals(x.ApplicationType, y.ApplicationType) &&
			x.RateOrAmountOrFactor.Equals(y.RateOrAmountOrFactor));

		public int GetHashCode(IImportSpecialCase obj) => 0;
	}
}
