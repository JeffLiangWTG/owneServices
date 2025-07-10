using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	sealed class ICFCPEDBodyEqualityComparer : IMonthlyClosingDecBodyEqualityComparer, IEqualityComparer<ICFCPEDBody>
	{
		public bool Equals(ICFCPEDBody px, ICFCPEDBody py)
		{
			return ComparerHelper.Compare(px, py, (x, y) =>
				x.ConsignorPK == y.ConsignorPK &&
				x.AdditionalDutyReferences.EqualIgnoringOrder(y.AdditionalDutyReferences, new IImportAdditionalDutyReferenceEqualityComparer()) &&
				base.Equals(x, y));
		}

		public int GetHashCode(ICFCPEDBody obj) => 0;
	}
}
