#if DEBUG
using CargoWise.Common;
using CargoWise.Data;
using WTG.DevTools.Database.TestFramework.TestDatabase;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	static class CrikeyDatabaseForTesting
	{
		public static string GetCrikeyTestDatabaseName()
		{
			if (database.Value is null)
			{
				database.Value = SqlDatabaseForTest.CreateDatabaseAsync(Db.ServerName, "ATUTEDI", "AutoTester_UserTests.sql").GetAwaiter().GetResult();
				using var adminConnection = Db.NewAdminConnection(Db.ServerName, database.Value.DatabaseName);
				adminConnection.IsUpgradeCheckDisabled = true;
				((IDbLoginRepair)adminConnection).EnsureDbLoginsHaveRightsToCurrentDatabase();
			}

			return database.Value.DatabaseName;
		}

		static readonly Overridable<SqlDatabaseForTest> database = new (null);
	}
}
#endif
