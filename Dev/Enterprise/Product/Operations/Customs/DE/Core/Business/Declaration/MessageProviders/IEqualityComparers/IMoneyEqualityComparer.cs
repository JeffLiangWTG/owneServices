using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IMoneyEqualityComparer : IEqualityComparer<IMoney>
	{
		public bool Equals(IMoney px, IMoney py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.CurrencyCode, y.CurrencyCode) &&
			x.Value.Equals(y.Value));

		public int GetHashCode(IMoney obj) => 0;
	}
}
