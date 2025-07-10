using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class DbConnectionCrikey
	{
		public static DbConnection GetAutoTesterUserTestsConnection()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				return GetAutoTesterUserTestsConnectionForTesting();
			}
#endif
			var server = EDIDataRegistry.Instance.DbConnectionCrikeyServer.Value;
			return !string.IsNullOrEmpty(server)
				? Db.NewExtraConnection(server, "AutoTester_UserTests", "AutoTester", "builder")
				: null;
		}

#if DEBUG
		static DbConnection GetAutoTesterUserTestsConnectionForTesting()
		{
			if (NoCrikeyConnection.Value)
			{
				return null;
			}

			var databaseName = CrikeyDatabaseForTesting.GetCrikeyTestDatabaseName();
			return Db.NewAdminConnection(Db.ServerName, databaseName);
		}

		public static Overridable<bool> NoCrikeyConnection { get; } = new Overridable<bool>(false);
#endif
	}
}
