using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.SqlSecurity
{
	interface IStaffInfoProvider
	{
		IEnumerable<DbUserManager.StaffLoginInfo> GetStaffLoginsInfo();
	}
}
