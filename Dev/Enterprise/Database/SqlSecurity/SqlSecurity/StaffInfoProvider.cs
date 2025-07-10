using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.SqlSecurity
{
	class StaffInfoProvider : IStaffInfoProvider
	{
		DbUserManager dbUserManager;
		List<DbUserManager.StaffLoginInfo> usersThatShouldHaveDbLogin;

		readonly CancellationToken cancellationToken;
		readonly ILogger logger;

		internal StaffInfoProvider(CancellationToken cancellationToken, ILogger logger)
		{
			this.cancellationToken = cancellationToken;
			this.logger = logger;
		}

		public IEnumerable<DbUserManager.StaffLoginInfo> GetStaffLoginsInfo()
		{
			if (usersThatShouldHaveDbLogin == null)
			{
				usersThatShouldHaveDbLogin = new List<DbUserManager.StaffLoginInfo>();

				Helper.PerformActionWithElevatedPermissions(() =>
				{
					dbUserManager = new DbUserManager(logger);
					usersThatShouldHaveDbLogin = dbUserManager.GetUsersThatShouldHaveDbLogins(Db.Connection);
				}, cancellationToken);
			}

			return usersThatShouldHaveDbLogin;
		}
	}
}
