using System.Collections.Generic;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	class AmountComparer : IEqualityComparer<Amount>
	{
		public bool Equals(Amount px, Amount py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.Qualifier, y.Qualifier)
			&& string.Equals(x.MeasurementUnit, y.MeasurementUnit)
			&& x.Quantity == y.Quantity);

		public int GetHashCode(Amount obj) => 0;
	}
}
