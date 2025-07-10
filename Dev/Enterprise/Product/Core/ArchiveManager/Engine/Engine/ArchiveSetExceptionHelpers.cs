using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "They are error messages")]
	[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Querying sys tables")]
	static class ArchiveSetExceptionHelpers
	{
		internal static bool IsDoNotRetryException(Exception e)
			=> IsSqlTimeoutException(e) || IsMainRecordMissingException(e);

		internal static bool IsMainRecordMissingException(Exception e)
			=> e.Message.Contains("The expected corresponding record in ArchiveMainItemQueue was not found.");

		internal static bool IsSqlTimeoutException(Exception e)
			=> e is SqlException sqlEx && sqlEx.IsTimeoutExpired();

		internal static bool IsLockRequestTimeoutException(Exception e)
			=> e.Message.Contains("Lock request time out period exceeded");

		internal static string GetLockRequestTimeoutExceptionMessage(Exception e)
		{
			var sqlToFindLocks = @"WITH LocksHeld(spid, table_name, mode) AS
(
	SELECT DISTINCT request_session_id, OBJECT_NAME(resource_associated_entity_id), request_mode
	FROM sys.dm_tran_locks
	WHERE resource_database_id = DB_ID()
	AND request_mode LIKE '%X%'
	AND request_status = 'GRANT'
	AND resource_type = 'OBJECT'
)
SELECT
	l.spid AS [spid],
	STRING_AGG(CONCAT(l.table_name, ' - ', l.mode), ', ') AS [locks_held],
	s.text AS [text]
FROM LocksHeld l
LEFT JOIN sys.dm_exec_connections c on l.spid = c.session_id
OUTER APPLY sys.dm_exec_sql_text(c.most_recent_sql_handle) s
GROUP BY l.spid, s.text
ORDER BY l.spid";

			var errorMessage = "Archive Manager has encountered lock contention with an unknown process, where one or more of the queries below may be the culprit. "
				+ "If this error occurs frequently, and the contenting query belongs to a periodically scheduled process, you may want to consider "
				+ "staggering its scheduled time so that it differs to ARC. Below is a list of SPIDs currently holding exclusive locks on the database, "
				+ "the locks they hold, and their most recently executed statements:";

			using var cmd = Db.Connection.Command(sqlToFindLocks);
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				var spid = reader.GetInt32(reader.GetOrdinal("spid"));
				var locksHeld = reader.GetString(reader.GetOrdinal("locks_held"));
				var text = reader[reader.GetOrdinal("text")] as string ?? "UNKNOWN";

				errorMessage += $"\n\n[{spid}] {locksHeld}\n{text}";
			}

			return errorMessage + $"\n\nThe error message was: {e}";
		}
	}
}
