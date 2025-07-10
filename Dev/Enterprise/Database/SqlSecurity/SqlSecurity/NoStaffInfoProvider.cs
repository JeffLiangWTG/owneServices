using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.SqlSecurity
{
	class NoStaffInfoProvider : IStaffInfoProvider
	{
		internal NoStaffInfoProvider()
		{
		}

		public IEnumerable<DbUserManager.StaffLoginInfo> GetStaffLoginsInfo()
		{
			return Array.Empty<DbUserManager.StaffLoginInfo>();
		}
	}
}
