using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Business
{
	class IAmountEqualityComparer : IEqualityComparer<IAmount>
	{
		public bool Equals(IAmount px, IAmount py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.Qualifier, y.Qualifier) &&
			string.Equals(x.MeasurementUnit, y.MeasurementUnit) &&
			x.Quantity.Equals(y.Quantity));

		public int GetHashCode(IAmount obj) => 0;
	}
}
