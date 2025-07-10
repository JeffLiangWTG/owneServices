using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	internal class IExciseDutyEqualityComparer : IEqualityComparer<IExciseDuty>
	{
		public bool Equals(IExciseDuty px, IExciseDuty py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.Code, y.Code) &&
			x.DegreePercentage.Equals(y.DegreePercentage) &&
			x.Value.Equals(y.Value) &&
			new IAmountEqualityComparer().Equals(x.Amount, y.Amount));

		public int GetHashCode(IExciseDuty obj) => 0;
	}
}
