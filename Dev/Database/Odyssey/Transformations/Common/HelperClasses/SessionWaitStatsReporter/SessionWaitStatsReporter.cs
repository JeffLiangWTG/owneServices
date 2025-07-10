using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.Common;

public class SessionWaitStatsReporter : ISessionWaitStatsReporter
{
	public SessionWaitStatsReporter(string tempTableName)
	{
		this.tempTableName = tempTableName.QuoteName();
	}

	public void StartRecording()
	{
		Db.Connection.ExecuteNonQuery($"DROP TABLE IF EXISTS {tempTableName}");
		Db.Connection.ExecuteNonQuery($"SELECT * INTO {tempTableName} FROM sys.dm_exec_session_wait_stats WHERE session_id = @@SPID");
	}

	public IReadOnlyCollection<SessionWaitStats> GetSessionWaits()
	{
		var query =	$@"
WITH waits AS
(
	SELECT
		after.wait_type,
		after.wait_time_ms - ISNULL(before.wait_time_ms, 0) AS wait_time_ms,
		after.waiting_tasks_count - ISNULL(before.waiting_tasks_count, 0) AS waiting_tasks_count,
		after.max_wait_time_ms,
		after.signal_wait_time_ms - ISNULL(before.signal_wait_time_ms, 0) AS signal_wait_time_ms 
	FROM
		sys.dm_exec_session_wait_stats after 
		LEFT JOIN {tempTableName} before ON after.session_id = before.session_id AND after.wait_type = before.wait_type 
	WHERE 
		after.session_id = @@SPID 
)
SELECT * FROM waits
WHERE 1 = 0
	OR wait_time_ms > 0
	OR waiting_tasks_count > 0
	OR signal_wait_time_ms > 0";

		try
		{
			var list = new List<SessionWaitStats>();
			Db.Connection.ExecuteReader(query, record =>
			{
				list.Add(new SessionWaitStats(record));
			});

			return list;
		}
		catch (SqlException ex) when (DbErrorMatch.GetExceptionType(ex) == DbErrorType.InvalidObjectName)
		{
			throw new InvalidOperationException($"Recording has not been started. You need to call {nameof(StartRecording)} method first.");
		}
	}

	readonly string tempTableName;
}
