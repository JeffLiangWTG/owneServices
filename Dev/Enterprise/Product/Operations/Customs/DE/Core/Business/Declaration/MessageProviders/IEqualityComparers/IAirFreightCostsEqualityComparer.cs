using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IAirFreightCostsEqualityComparer : IImportCostsEqualityComparer, IEqualityComparer<IAirFreightCosts>
	{
		public bool Equals(IAirFreightCosts a, IAirFreightCosts b) => ComparerHelper.Compare(a, b, (x, y) =>
			x.CurrencyRateIATA.Equals(y.CurrencyRateIATA) &&
			x.CurrencyRateDate.Equals(y.CurrencyRateDate) &&
			base.Equals(x, y));

		public int GetHashCode(IAirFreightCosts obj) => 0;
	}
}
