using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	internal class ILinePreferentialTreatmentEqualityComparer : IEqualityComparer<ILinePreferentialTreatment>
	{
		public bool Equals(ILinePreferentialTreatment px, ILinePreferentialTreatment py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.RequestedPreferentialTreatment, y.RequestedPreferentialTreatment) &&
			x.ContingentNumber.EqualIgnoringOrder(y.ContingentNumber) &&
			new IAmountEqualityComparer().Equals(x.Quantity, y.Quantity));

		public int GetHashCode(ILinePreferentialTreatment obj) => 0;
	}
}
