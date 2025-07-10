using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	/// <summary>
	/// A strategy that checks for Instant File Intitialization via a query of the DMV data
	/// </summary>

	class InstantFileInitializationFastQueryStrategy : IInstantFileInitializationStrategy
	{
		readonly string query;

		public InstantFileInitializationFastQueryStrategy()
			: this(GetDefaultQuery())
		{
		}

		internal InstantFileInitializationFastQueryStrategy(string query)
		{
			this.query = query;
		}

		public bool? IsInstantFileInitializationEnabled(ILogger logger)
		{
			switch (GetServerServicesValue())
			{
				case "Y":
					return true;

				case "N":
					return false;

				default:
					return null;
			}
		}

		string GetServerServicesValue()
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				return connection.ExecuteScalar(query) as string;
			}
		}

		static string GetDefaultQuery()
		{
			return @"
IF EXISTS(
	SELECT 1 
	FROM sys.system_views sv JOIN sys.system_columns sc
		ON sv.object_id = sc.object_id
	WHERE sv.name = 'dm_server_services' AND sc.name = 'instant_file_initialization_enabled'
)
	EXEC ('
		SELECT instant_file_initialization_enabled
		FROM sys.dm_server_services
		WHERE servicename = ''SQL Server ('' + @@SERVICENAME + '')''');
ELSE
	SELECT null;
";
		}
	}
}
