#if DEBUG

using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Filters;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class OrgLedgerFilterCollection
	{
		public OrgLedgerFilter GetLedgerFilterForOrg_ForTestOnly(ZGuid orgPK)
		{
			return GetLedgerFilterForOrg(orgPK);
		}
	}
}

#endif
