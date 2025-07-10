using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Startup
{
	public sealed class DatabasePadlock
	{
		/// <summary>
		/// Places a Sch-M lock on all user tables of the given databases.
		/// Used to lock out other connections for the offline part of an upgrade.
		///
		/// Given connection should be in a transaction, the upgrade transaction.
		/// If a table lock attempt times out, SQL server will automatically rollback the entire transaction
		/// even with XACT_ABORT off. The padlock will need to be called again with a fresh transaction.
		/// </summary>
		public bool LockDatabaseResources(AdminConnection connection, IEnumerable<string> databasesToLock, Action<string> feedbackMethod = null)
		{
			foreach (var dbName in databasesToLock)
			{
				try
				{
					LockResourcesOfGivenDatabase(connection, dbName);
				}
				catch (SqlException ex)
				{
					var dbErrorMatch = new DbErrorMatch(ex);
					if (dbErrorMatch.ExceptionType == DbErrorType.GeneralUserException ||
						dbErrorMatch.ExceptionType == DbErrorType.LockTimeoutExpired)
					{
						feedbackMethod?.Invoke("Unable to lock database resources. Please stop all other processes connected to the database " + dbName + ".\r\n" + ex.Message);
						return false;
					}
					else
					{
						throw;
					}
				}
			}

			return true;
		}

		void LockResourcesOfGivenDatabase(AdminConnection connection, string dbName)
		{
			var sql = GetObjectsToLockQuery(dbName);

			var objectsToLock = new List<string>();
			connection.ExecuteReader(sql
				, (reader) =>
				{
					var sch_name = (string)reader["sch_name"];
					var tab_name = (string)reader["tab_name"];
					var tab_id = Convert.ToInt32(reader["tab_id"], CultureInfo.InvariantCulture);

					objectsToLock.Add(FormattableString.Invariant($@"SET @TableId = {tab_id};
IF EXISTS (SELECT NULL FROM [{dbName}].sys.extended_properties WHERE class = 1 AND [major_id] = {tab_id} AND [name] = @extendedPropertyName)
	EXEC [{dbName}].sys.sp_dropextendedproperty @extendedPropertyName, N'SCHEMA', [{sch_name}], N'TABLE', [{tab_name}];

EXEC [{dbName}].sys.sp_addextendedproperty @extendedPropertyName, @value, N'SCHEMA', [{sch_name}], N'TABLE', [{tab_name}];
EXEC [{dbName}].sys.sp_dropextendedproperty @extendedPropertyName, N'SCHEMA', [{sch_name}], N'TABLE', [{tab_name}];"));
				});

			if (objectsToLock.Count > 0)
			{
				var sqlToLock = string.Join(System.Environment.NewLine, objectsToLock);
				var sqlLock = FormattableString.Invariant($@"-- LockResourcesOfGivenDatabase
SET NOCOUNT ON;
SET LOCK_TIMEOUT 60000;

DECLARE @TableId int;

BEGIN TRY

{sqlToLock}

END TRY
BEGIN CATCH
	DECLARE @ErrorNumber int = ERROR_NUMBER(), @ErrorSeverity int = ERROR_SEVERITY(), @ErrorState int = ERROR_STATE(), @ErrorMessage nvarchar(4000) = ERROR_MESSAGE();
	DECLARE @FriendlyMessage nvarchar(4000) = (
		SELECT TOP(1)
			msg = N'[{dbName}].[' + sch.name + N'].[' + tab.name + N']'
				+ N' locked by SPID [' + CONVERT(nvarchar(10), s.session_id) + N']'
				+ N', LOGIN [' + s.login_name + N']'
				+ N', CLIENT COMPUTER [' + ISNULL(s.host_name, N'') + N']'
				+ N', CLIENT PROCESS [' + ISNULL(CONVERT(nvarchar(10), s.host_process_id), N'') + N']'
				+ N', CLIENT PROGRAM [' + ISNULL(s.program_name, N'') + N']'
				+ N'.' COLLATE {Db.DatabaseCollation}
		FROM
			sys.dm_tran_locks           AS l
			JOIN sys.dm_exec_sessions   AS s   ON s.session_id = l.request_session_id
			JOIN [{dbName}].sys.tables  AS tab WITH (NOLOCK) ON tab.object_id = l.resource_associated_entity_id
			JOIN [{dbName}].sys.schemas AS sch WITH (NOLOCK) ON sch.schema_id = tab.schema_id
		WHERE
			l.resource_database_id = DB_ID(N'{dbName}')
			AND l.resource_associated_entity_id = @TableId
		);

	if (XACT_STATE() = -1) ROLLBACK;

	if (@ErrorNumber = 1222 AND @FriendlyMessage <> N'') RAISERROR (@FriendlyMessage, 16, 1);
	else THROW;
END CATCH

"
					);

				using (var cmd = connection.Command(sqlLock))
				{
					cmd.AddParameter("@extendedPropertyName", System.Data.SqlDbType.NVarChar, 128, LockPropertyName);
					cmd.AddParameter("@value", System.Data.SqlDbType.Bit, true);

					cmd.ExecuteNonQuery();
				}
			}
		}

		internal static string GetObjectsToLockQuery(string dbName)
		{
			return FormattableString.Invariant($@"
SELECT
	sch_name = sch.name,
	tab_name = tab.name,
	tab_id   = tab.object_id
FROM
	[{dbName}].sys.objects      AS tab WITH (NOLOCK)
	JOIN [{dbName}].sys.schemas AS sch WITH (NOLOCK) ON sch.schema_id = tab.schema_id
WHERE 1=1
	AND tab.type = 'U'
	AND tab.is_ms_shipped = 0
ORDER BY
	sch.name, tab.name

"
				);
		}

		public const string LockPropertyName = "LockResourcesOfGivenDatabase";
	}
}
