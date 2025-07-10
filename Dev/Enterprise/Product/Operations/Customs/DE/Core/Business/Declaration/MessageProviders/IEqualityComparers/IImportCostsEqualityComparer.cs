using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IImportCostsEqualityComparer : IMoneyEqualityComparer, IEqualityComparer<IImportCosts>
	{
		public bool Equals(IImportCosts a, IImportCosts b) => ComparerHelper.Compare(a, b, (x, y) =>
			x.CurrencyRateAgreedFlag.Equals(y.CurrencyRateAgreedFlag) &&
			x.CurrencyRate.Equals(y.CurrencyRate) &&
			base.Equals(x, y));

		public int GetHashCode(IImportCosts obj) => 0;
	}
}
