using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUASPEntryChargeTypeListForTest : AUASPEntryChargeTypeList
	{
		public AUASPEntryChargeTypeListForTest() : base()
		{ }

		public new IEnumerable<ZGuid> GetSpecialChargeCodePks(ZGuid companyPK) => base.GetSpecialChargeCodePks(companyPK);
	}
}
