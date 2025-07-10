using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	sealed class ISCWPEDLineEqualityComparer : IMonthlyClosingDecLineEqualityComparer, IEqualityComparer<ISCWPEDLine>
	{
		public bool Equals(ISCWPEDLine px, ISCWPEDLine py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.RequestedPreferentialTreatment, y.RequestedPreferentialTreatment) &&
			new IAmountEqualityComparer().Equals(x.InwardMovementAmount, y.InwardMovementAmount) &&
			string.Equals(x.ForeignTradeImportEarlyClearanceFlag, y.ForeignTradeImportEarlyClearanceFlag) &&
			base.Equals(x, y));

		public int GetHashCode(ISCWPEDLine obj) => 0;
	}
}
