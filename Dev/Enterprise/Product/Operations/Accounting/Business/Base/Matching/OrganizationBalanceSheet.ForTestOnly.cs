#if DEBUG

using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class OrganizationBalanceSheet
	{
		public List<ZGuid> GetUniqueOrganizations_ForTestOnly()
		{
			return GetUniqueOrganizations();
		}
	}
}

#endif
