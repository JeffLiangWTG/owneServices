using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	internal class IImportSpecificRateEqualityComparer : IEqualityComparer<IImportSpecificRate>
	{
		public bool Equals(IImportSpecificRate px, IImportSpecificRate py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.Type, y.Type) &&
			x.Value.Equals(y.Value));

		public int GetHashCode(IImportSpecificRate obj) => 0;
	}
}
