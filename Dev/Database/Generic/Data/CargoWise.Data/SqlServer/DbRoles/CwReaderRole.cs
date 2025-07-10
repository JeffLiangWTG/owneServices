using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	#region SuppressResourceStringsCheckRegion

	public class CwReaderRole : DbRole
	{
		public override string Name => DbRoleTypes.CwReaderRole;

		public void GetRefreshRoleSQL(DbConnection connection, string mainDbName, string dbName, bool shouldGrantExecuteRights, out StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			cmdBuilder = new StringBuilder();

			int? dbReaderRoleId = GetDbRoleId(connection, dbName);

			AppendCreateRoleAndDbLevelGrantCommandsIfRequired(connection, mainDbName, dbName, dbReaderRoleId, cmdBuilder);

			if (shouldGrantExecuteRights)
			{
				ChangeOwnerForStoredProceduresToDbReaderRole(connection, dbName, cmdBuilder);
				AppendGrantExecuteToDbReaderRoleCommands(connection, dbReaderRoleId, cmdBuilder);
				AppendGrantExecuteOnTvpUniqueidentifierTypeToDbReaderRole(connection, dbReaderRoleId, cmdBuilder);
			}
		}

		/// <summary>
		/// If required,
		/// appends commands to create the Database Reader role
		/// and to grant it selected database level rights.
		/// </summary>
		/// <param name="connection">Database connection</param>
		/// <param name="mainDbName">Main Database name</param>
		/// <param name="dbName">Database name</param>
		/// <param name="dbReaderRoleId">Database Reader Role ID (null if not yet created)</param>
		/// <param name="cmdBuilder">Command builder to append commands to</param>
		void AppendCreateRoleAndDbLevelGrantCommandsIfRequired(DbConnection connection, string mainDbName, string dbName, int? dbReaderRoleId, StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(cmdBuilder, nameof(cmdBuilder));

			if (dbReaderRoleId.HasValue)
			{
				if (!IsMemberOfDbDataReader(connection, dbName, dbReaderRoleId.Value))
				{
					cmdBuilder.AppendLine(SqlAddCwReaderRoleAsDataReader);
				}

				if (!HasViewDefinitionRights(connection, dbName, dbReaderRoleId.Value))
				{
					cmdBuilder.AppendLine(SqlGrantViewDefinitionToCwReaderRole);
				}

				if (!HasShowPlanRights(connection, dbName, dbReaderRoleId.Value))
				{
					cmdBuilder.AppendLine(SqlGrantShowPlanToCwReaderRole);
				}
			}
			else
			{
				cmdBuilder.AppendLine(SqlCreateCwReaderRole);
				cmdBuilder.AppendLine(SqlAddCwReaderRoleAsDataReader);
				cmdBuilder.AppendLine(SqlGrantViewDefinitionToCwReaderRole);
				cmdBuilder.AppendLine(SqlGrantShowPlanToCwReaderRole);
			}
			//ensure reader login is mapped to cwReaderRole
			cmdBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "if exists (select 1 from sys.Database_Principals where name = '{1}') ALTER ROLE [{0}] ADD MEMBER [{1}]", Name,
				CargoWiseReaderLoginCredentials.UserNameFor(mainDbName)));
		}

		void ChangeOwnerForStoredProceduresToDbReaderRole(DbConnection connection, string dbName, StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(cmdBuilder, nameof(cmdBuilder));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	sch_name = sch.name,
	obj_name = obj.name
FROM
	{0}.sys.schemas      AS sch
	JOIN {0}.sys.objects AS obj ON obj.schema_id = sch.schema_id
	JOIN {0}.sys.sql_modules AS md ON obj.object_id = md.object_id
WHERE 1=1
	AND obj.is_ms_shipped = 0
	AND obj.type = ('P')
	AND obj.principal_id is NULL
	AND md.definition NOT LIKE '%EXECUTE AS OWNER%'
"
				, dbName.QuoteName() // 0
				);

			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var sch_name = (string)reader["sch_name"];
					var obj_name = (string)reader["obj_name"];

					cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
						"ALTER AUTHORIZATION ON {0}.{1} TO {2};"
						, sch_name.QuoteName()     // 0
						, obj_name.QuoteName()     // 1
						, Name.QuoteName() // 2
						)
						?.AppendLine();
				}
			}
		}

		/// <summary>
		/// Appends commands that grant execute rights on selected read-only objects to the Database Reader role.
		/// </summary>
		/// <param name="connection">Database connection</param>
		/// <param name="dbReaderRoleId">Database Reader Role ID (null if not yet created)</param>
		/// <param name="cmdBuilder">Command builder to append commands to</param>
		void AppendGrantExecuteToDbReaderRoleCommands(DbConnection connection, int? dbReaderRoleId, StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 
			Argument.NotNull(cmdBuilder, nameof(cmdBuilder));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	sch_name = sch.name,
	obj_name = obj.name
FROM
	sys.schemas                        AS sch
	JOIN sys.objects                   AS obj ON obj.schema_id = sch.schema_id
	LEFT JOIN sys.database_permissions AS per ON per.major_id = obj.object_id
		AND per.class = 1
		AND per.type = 'EX'
		AND per.state = 'G'
		AND per.grantee_principal_id = {0}
WHERE
	obj.is_ms_shipped = 0
	AND obj.type in ('AF', 'FN', 'FS')
	AND per.major_id is NULL
"
				, (dbReaderRoleId.HasValue) ? dbReaderRoleId.Value.ToString(CultureInfo.InvariantCulture) : "NULL"
				);

			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schemaName = (string)reader["sch_name"];
					var objectName = (string)reader["obj_name"];

					cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
						"GRANT EXECUTE ON {0}.{1} TO {2};"
						, schemaName.QuoteName()   // 0
						, objectName.QuoteName()   // 1
						, Name.QuoteName() // 2
						)
						?.AppendLine();
				}
			}
		}

		void AppendGrantExecuteOnTvpUniqueidentifierTypeToDbReaderRole(DbConnection connection, int? dbReaderRoleId, StringBuilder cmdBuilder)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 
			Argument.NotNull(cmdBuilder, nameof(cmdBuilder));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	sch_name = sch.name,
	obj_name = typ.name
FROM
	sys.schemas                        AS sch
	JOIN sys.types                     AS typ ON typ.schema_id = sch.schema_id
	LEFT JOIN sys.database_permissions AS per ON per.major_id = typ.user_type_id
		AND per.class = 6
		AND per.type = 'EX'
		AND per.state = 'G'
		AND per.grantee_principal_id = {0}
WHERE
	typ.system_type_id = 243
	AND per.major_id is NULL
"
				, (dbReaderRoleId.HasValue) ? dbReaderRoleId.Value.ToString(CultureInfo.InvariantCulture) : "NULL"
				);

			using (var cmd = connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schemaName = (string)reader["sch_name"];
					var objectName = (string)reader["obj_name"];

					if (schemaName != null && objectName != null)
					{
						cmdBuilder.AppendFormat(CultureInfo.InvariantCulture,
							"GRANT EXEC ON TYPE::{0}.{1} TO {2};"
							, schemaName.QuoteName()   // 0
							, objectName.QuoteName()   // 1
							, Name.QuoteName() // 2
							)
							?.AppendLine();
					}
				}
			}
		}

		bool IsMemberOfDbDataReader(DbConnection connection, string dbName, int dbReaderRoleId)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	rm.role_principal_id
FROM
	{0}.sys.database_role_members    AS rm
	JOIN {0}.sys.database_principals AS r  ON r.principal_id = rm.role_principal_id
WHERE
	rm.member_principal_id = {1}
	AND r.name = 'db_datareader'
"
				, dbName.QuoteName()
				, dbReaderRoleId
				);
			return (connection.ExecuteScalar(sql) != null);
		}

		bool HasViewDefinitionRights(DbConnection connection, string dbName, int dbReaderRoleId)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	grantee_principal_id
FROM
	{0}.sys.database_permissions
WHERE
	class = 0
	AND type = 'VW'
	AND state = 'G'
	AND grantee_principal_id = {1}
"
				, dbName.QuoteName()
				, dbReaderRoleId
				);

			return (connection.ExecuteScalar(sql) != null);
		}

		bool HasShowPlanRights(DbConnection connection, string dbName, int dbReaderRoleId)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	grantee_principal_id
FROM
	{0}.sys.database_permissions
WHERE
	class = 0
	AND type = 'SPLN'
	AND state = 'G'
	AND grantee_principal_id = {1}
"
				, dbName.QuoteName()
				, dbReaderRoleId
				);

			return (connection.ExecuteScalar(sql) != null);
		}

		public const string SqlCreateCwReaderRoleAndBaseRights =
			SqlCreateCwReaderRole +
			SqlAddCwReaderRoleAsDataReader +
			SqlGrantViewDefinitionToCwReaderRole +
			SqlGrantShowPlanToCwReaderRole;

		public const string EpClientDatabasesProc = "ep_ClientDatabases";
		public const string ClrFunctionUncompressAsBytes = "CLRUncompressAsBytes";
		public const string TvpUniqueidentifier = "TVP_uniqueidentifier";

		const string SqlCreateCwReaderRole = "CREATE ROLE [" + DbRoleTypes.CwReaderRole + "];";
		const string SqlAddCwReaderRoleAsDataReader = "ALTER ROLE [db_datareader] ADD MEMBER [" + DbRoleTypes.CwReaderRole + "]; ALTER AUTHORIZATION ON ROLE::[" + DbRoleTypes.CwReaderRole + "] TO [" + Db.SqlDbOwnerSchema + "];";
		const string SqlGrantViewDefinitionToCwReaderRole = "GRANT VIEW DEFINITION TO [" + DbRoleTypes.CwReaderRole + "];";
		const string SqlGrantShowPlanToCwReaderRole = "GRANT SHOWPLAN TO [" + DbRoleTypes.CwReaderRole + "];";
	}

	#endregion region //SuppressResourceStringsCheckRegion
}
