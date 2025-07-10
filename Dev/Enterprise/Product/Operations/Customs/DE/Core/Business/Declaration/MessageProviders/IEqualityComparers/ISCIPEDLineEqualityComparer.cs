using System.Collections.Generic;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	sealed class ISCIPEDLineEqualityComparer : IMonthlyClosingDecLineEqualityComparer, IEqualityComparer<ISCIPEDLine>
	{
		public bool Equals(ISCIPEDLine px, ISCIPEDLine py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.RequestedPreferentialTreatment, y.RequestedPreferentialTreatment) &&
			new IAmountEqualityComparer().Equals(x.InwardMovementAmount, y.InwardMovementAmount) &&
			base.Equals(x, y));

		public int GetHashCode(ISCIPEDLine obj) => 0;
	}
}
