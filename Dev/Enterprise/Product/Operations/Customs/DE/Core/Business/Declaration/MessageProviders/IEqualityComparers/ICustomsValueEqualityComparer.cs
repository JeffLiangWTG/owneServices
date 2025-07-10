using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	sealed class ICustomsValueEqualityComparer : IEqualityComparer<ICustomsValue>
	{
		public bool Equals(ICustomsValue px, ICustomsValue py) => ComparerHelper.Compare(px, py, (x, y) =>
			string.Equals(x.FormerDecisions, y.FormerDecisions) &&
			x.VendorPK == y.VendorPK &&
			x.VendeePK == y.VendeePK &&
			string.Equals(x.AffiliationType, y.AffiliationType) &&
			string.Equals(x.AffiliationDescription, y.AffiliationDescription) &&
			x.RestrictionFlag.Equals(y.RestrictionFlag) &&
			x.ConditionFlag.Equals(y.ConditionFlag) &&
			string.Equals(x.RestrictionOrConditionDescription, y.RestrictionOrConditionDescription) &&
			x.LicenseFeeFlag.Equals(y.LicenseFeeFlag) &&
			string.Equals(x.LicenseFeeDescription, y.LicenseFeeDescription) &&
			x.ResaleFlag.Equals(y.ResaleFlag) &&
			string.Equals(x.ResaleDescription, y.ResaleDescription));

		public int GetHashCode(ICustomsValue obj) => 0;
	}
}
