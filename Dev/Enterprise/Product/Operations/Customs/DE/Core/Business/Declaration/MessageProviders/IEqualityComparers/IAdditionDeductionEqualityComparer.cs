using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IAdditionDeductionEqualityComparer : IImportCostsEqualityComparer, IEqualityComparer<IAdditionDeduction>
	{
		public bool Equals(IAdditionDeduction a, IAdditionDeduction b) => ComparerHelper.Compare(a, b, (x, y) =>
			string.Equals(x.Type, y.Type) &&
			x.CurrencyRateIATA.Equals(y.CurrencyRateIATA) &&
			x.CurrencyRateDate.Equals(y.CurrencyRateDate) &&
			x.Percentage.Equals(y.Percentage) &&
			base.Equals(x, y));

		public int GetHashCode(IAdditionDeduction obj) => 0;
	}
}
