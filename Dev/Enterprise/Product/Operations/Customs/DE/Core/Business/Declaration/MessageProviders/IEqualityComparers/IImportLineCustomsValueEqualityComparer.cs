using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	class IImportLineCustomsValueEqualityComparer : IEqualityComparer<IImportLineCustomsValue>
	{
		public bool Equals(IImportLineCustomsValue a, IImportLineCustomsValue b) => ComparerHelper.Compare(a, b, (x, y) =>
			string.Equals(x.CustomsValueDepartureAirport, y.CustomsValueDepartureAirport) &&
			string.Equals(x.CustomsValueDestinationPlace, y.CustomsValueDestinationPlace) &&
			string.Equals(x.CustomsValueAdditionDeductionDescription, y.CustomsValueAdditionDeductionDescription) &&
			new IImportCostsEqualityComparer().Equals(x.CustomsValueNetPrice, y.CustomsValueNetPrice) &&
			new IImportCostsEqualityComparer().Equals(x.CustomsValueIndirectPayment, y.CustomsValueIndirectPayment) &&
			new IAirFreightCostsEqualityComparer().Equals(x.CustomsValueAirFreightCosts, y.CustomsValueAirFreightCosts) &&
			x.CustomsValueAdditionDeduction.EqualIgnoringOrder(y.CustomsValueAdditionDeduction, new IAdditionDeductionEqualityComparer()));

		public int GetHashCode(IImportLineCustomsValue obj) => 0;
	}
}
