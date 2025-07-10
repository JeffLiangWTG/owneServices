using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using static System.FormattableString;

namespace CargoWise.Data
{
	public static partial class DbConnectionKiller
	{
		public static bool KillOtherConnectionsUsingAuxiliaryConnection(
			string dbName,
			DbConnection connectionToKeep,
			Action<string> feedbackMethod,
			DateTime loginCutoffTime,
			TimeSpan lockTimeout)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.NotNull(connectionToKeep, nameof(connectionToKeep));
			Argument.NotNullOrEmpty(connectionToKeep.ServerName, nameof(connectionToKeep.ServerName));

			using (var connection = Db.NewAdminConnection(connectionToKeep.ServerName, Db.SqlMasterDb))
			{
				connection.IsUpgradeCheckDisabled = true;
				connection.SetLockTimeout((int)lockTimeout.TotalMilliseconds);
				return KillOtherUserSessions(connection, dbName, connectionToKeep.SPID, feedbackMethod, loginCutoffTime);
			}
		}

		public static bool KillOtherConnections(DbConnection connection, string dbName, Action<string> feedbackMethod = null)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			return KillOtherConnections(connection, dbName, feedbackMethod, DateTime.MaxValue);
		}

		public static bool KillOtherConnections(DbConnection connection, string dbName, Action<string> feedbackMethod, DateTime loginCutoffTime)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			return KillOtherUserSessions(connection, dbName, connection.SPID, feedbackMethod, loginCutoffTime);
		}

		public static bool KillAllConnectionsOfTheCurrentProcess(Action<string> feedbackMethod)
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.IsUpgradeCheckDisabled = true;
				var (hostName, hostProcessId) = GetCurrentProcessInfo(connection);
				var sql = FormattableString.Invariant($@"
DECLARE
	@rn  nchar(2) = CHAR(13) + CHAR(10),
	@rnt nchar(3) = CHAR(13) + CHAR(10) + CHAR(9)
	, @stmt_kill nvarchar(max)
	, @stmt_spid nvarchar(max)
	, @stmt_info nvarchar(max)
	, @xml       xml

SELECT @xml =
(
	SELECT TOP(200)
		_kill = CONCAT(@rn, N'BEGIN TRY KILL ', s.session_id, N'; END TRY BEGIN CATCH if (ERROR_NUMBER() NOT in ({ErrorsToIgnore})) THROW; END CATCH;'),
		_spid = CONCAT(N',', s.session_id)
		, _info = CONCAT(@rnt
			, N'-----------------------------------------------------------------'
			, @rnt, N'session_id                   : ', s.session_id                                              
			, @rnt, N'is_user_process              : ', MIN(CONVERT(int, s.is_user_process)                      )
			, @rnt, N'status                       : ', MIN(s.status                                             )
			, @rnt, N'open_transaction_count       : ', MIN(s.open_transaction_count                             )
			, @rnt, N'host_name                    : ', MIN(s.host_name                                          )
			, @rnt, N'program_name                 : ', MIN(s.program_name                                       )
			, @rnt, N'client_interface_name        : ', MIN(s.client_interface_name                              )
			, @rnt, N'host_process_id              : ', MIN(s.host_process_id                                    )
			, @rnt, N'authenticating_database_name : ', MIN(DB_NAME(s.authenticating_database_id)                )
			, @rnt, N'database_name                : ', MIN(DB_NAME(s.database_id)                               )
			, @rnt, N'nt_domain                    : ', MIN(s.nt_domain                                          )
			, @rnt, N'nt_user_name                 : ', MIN(s.nt_user_name                                       )
			, @rnt, N'original_login_name          : ', MIN(s.original_login_name                                )
			, @rnt, N'login_name                   : ', MIN(s.login_name                                         )
			, @rnt, N'login_time                   : ', MIN(CONVERT(nvarchar(23), s.login_time, 121)             )
			, @rnt, N'cpu_time                     : ', MIN(s.cpu_time                                           )
			, @rnt, N'total_elapsed_time           : ', MIN(s.total_elapsed_time                                 )
			, @rnt, N'last_request_start_time      : ', MIN(CONVERT(nvarchar(23), s.last_request_start_time, 121))
			, @rnt, N'last_request_end_time        : ', MIN(CONVERT(nvarchar(23), s.last_request_end_time  , 121))
			, @rnt, N'reads                        : ', MIN(s.reads                                              )
			, @rnt, N'writes                       : ', MIN(s.writes                                             )
			, @rnt, N'logical_reads                : ', MIN(s.logical_reads                                      )
			, @rnt, N'row_count                    : ', MIN(s.row_count                                          )
			)
	FROM
		sys.dm_exec_sessions AS s
	WHERE 1=1
		AND s.is_user_process = 1
		AND s.session_id NOT in (@@SPID)
		AND s.host_name = N{hostName.QuoteName('\'')}
		AND s.host_process_id = {hostProcessId}
		AND s.login_time <= @loginCutoffTime
	GROUP BY
		s.session_id
	FOR XML PATH('')
)
OPTION (RECOMPILE)

SELECT
	@stmt_kill = STUFF(@xml.query('_kill').value('.', 'nvarchar(max)'), 1, LEN(@rn) , N''),
	@stmt_spid = STUFF(@xml.query('_spid').value('.', 'nvarchar(max)'), 1, 1        , N'')

if (@stmt_kill is NOT NULL)
begin
	BEGIN TRY
		EXEC (@stmt_kill);
	END TRY
	BEGIN CATCH
			SELECT
				@stmt_info = STUFF(@xml.query('_info').value('.', 'nvarchar(max)'), 1, LEN(@rnt), N'')

		SET @stmt_info = CONCAT(
			N'Original SQL error:'
			, @rnt, 'Error number: ', ERROR_NUMBER()
			, @rnt, 'Error message: ', ERROR_MESSAGE()
			, @rn, N'Session info before kill:'
			, @rnt, N'resource_database_name : ', DB_NAME()
			, @rnt, N'spid to kill           : ', ISNULL(@stmt_spid, N'')
			, @rnt, N'spid to keep           : ', @@SPID
			, @rnt, @stmt_info
			)
	END CATCH
end

SELECT
	stmt_spid = ISNULL(@stmt_spid, N'')
	, stmt_info = ISNULL(@stmt_info, N'')

");

				if (string.IsNullOrWhiteSpace(sql))
				{
					throw new InvalidOperationException("sql cannot be empty");
				}

				return KillConnections(connection, sql, DateTime.MaxValue, feedbackMethod);
			}
		}

		public static DateTime GetSqlServerDateForLoginCutoff(AdminConnection connection)
		{
			return (DateTime)connection.ExecuteScalar("SELECT getdate()"); // Direct SQL statement
		}

		static bool KillOtherUserSessions(DbConnection connection, string dbName, int spidToKeep, Action<string> feedbackMethod, DateTime loginCutoffTime)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			Stopwatch timer = null;
			if (feedbackMethod == null)
			{
				timer = Stopwatch.StartNew();
			}

			var killTimeout = TimeSpan.FromMinutes(5);
			var allConnectionsTerminated = false;
			do
			{
				allConnectionsTerminated = KillOtherUserSessionsToDatabase(connection, dbName, spidToKeep, loginCutoffTime, feedbackMethod);

				if (timer != null && !allConnectionsTerminated && timer.Elapsed >= killTimeout)
				{
					throw new TimeoutException("Unable to kill database connections in a timely manner. All killed connections with a pending rollback must complete before proceeding.");
				}
			}
			while (!allConnectionsTerminated);

			return true;
		}

		/// <summary>
		/// Runs kill database connection command handling some specific SQL exceptions.
		/// Returns: all connection terminated = TRUE/FALSE
		/// </summary>
		static bool KillOtherUserSessionsToDatabase(DbConnection connection, string dbName, int spidToKeep, DateTime loginCutoffTime, Action<string> feedbackMethod, bool isFirstRun = true)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (KillDirectConnections(connection, dbName, spidToKeep, loginCutoffTime, feedbackMethod))
			{
				if (KillSessionedTransactions(connection, dbName, spidToKeep, loginCutoffTime, feedbackMethod))
				{
					if (KillRemoteConnectionsAndDistributedOrphanTransactions(connection, dbName, spidToKeep, loginCutoffTime, feedbackMethod))
					{
						return true;
					}
				}
			}

			if (isFirstRun)
			{
				return KillOtherUserSessionsToDatabase(connection, dbName, spidToKeep, loginCutoffTime, feedbackMethod, isFirstRun: false);
			}

			return false;
		}

		static bool KillDirectConnections(DbConnection connection, string dbName, int spidToKeep, DateTime loginCutoffTime, Action<string> feedbackMethod)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = FormattableString.Invariant($@"
DECLARE
	@rn  nchar(2) = CHAR(13) + CHAR(10),
	@rnt nchar(3) = CHAR(13) + CHAR(10) + CHAR(9)
	, @stmt_kill nvarchar(max)
	, @stmt_spid nvarchar(max)
	, @stmt_info nvarchar(max)
	, @xml       xml

SELECT @xml =
(
	SELECT TOP(200)
		_kill = CONCAT(@rn, N'BEGIN TRY KILL ', s.session_id, N'; END TRY BEGIN CATCH if (ERROR_NUMBER() NOT in ({ErrorsToIgnore})) THROW; END CATCH;'),
		_spid = CONCAT(N',', s.session_id)
		, _info = CONCAT(@rnt
			, N'-----------------------------------------------------------------'
			, @rnt, N'session_id                   : ', s.session_id                                              
			, @rnt, N'is_user_process              : ', MIN(CONVERT(int, s.is_user_process)                      )
			, @rnt, N'status                       : ', MIN(s.status                                             )
			, @rnt, N'open_transaction_count       : ', MIN(s.open_transaction_count                             )
			, @rnt, N'host_name                    : ', MIN(s.host_name                                          )
			, @rnt, N'program_name                 : ', MIN(s.program_name                                       )
			, @rnt, N'client_interface_name        : ', MIN(s.client_interface_name                              )
			, @rnt, N'host_process_id              : ', MIN(s.host_process_id                                    )
			, @rnt, N'authenticating_database_name : ', MIN(DB_NAME(s.authenticating_database_id)                )
			, @rnt, N'database_name                : ', MIN(DB_NAME(s.database_id)                               )
			, @rnt, N'nt_domain                    : ', MIN(s.nt_domain                                          )
			, @rnt, N'nt_user_name                 : ', MIN(s.nt_user_name                                       )
			, @rnt, N'original_login_name          : ', MIN(s.original_login_name                                )
			, @rnt, N'login_name                   : ', MIN(s.login_name                                         )
			, @rnt, N'login_time                   : ', MIN(CONVERT(nvarchar(23), s.login_time, 121)             )
			, @rnt, N'cpu_time                     : ', MIN(s.cpu_time                                           )
			, @rnt, N'total_elapsed_time           : ', MIN(s.total_elapsed_time                                 )
			, @rnt, N'last_request_start_time      : ', MIN(CONVERT(nvarchar(23), s.last_request_start_time, 121))
			, @rnt, N'last_request_end_time        : ', MIN(CONVERT(nvarchar(23), s.last_request_end_time  , 121))
			, @rnt, N'reads                        : ', MIN(s.reads                                              )
			, @rnt, N'writes                       : ', MIN(s.writes                                             )
			, @rnt, N'logical_reads                : ', MIN(s.logical_reads                                      )
			, @rnt, N'row_count                    : ', MIN(s.row_count                                          )
			)
	FROM
		sys.dm_exec_sessions AS s
	WHERE 1=1
		AND s.is_user_process = 1
		AND s.session_id NOT in (@@SPID, {spidToKeep})
		AND s.database_id = DB_ID(N{dbName.QuoteName('\'')})
		AND s.login_time <= @loginCutoffTime
	GROUP BY
		s.session_id
	FOR XML PATH('')
)
OPTION (RECOMPILE)

SELECT
	@stmt_kill = STUFF(@xml.query('_kill').value('.', 'nvarchar(max)'), 1, LEN(@rn) , N''),
	@stmt_spid = STUFF(@xml.query('_spid').value('.', 'nvarchar(max)'), 1, 1        , N'')

if (@stmt_kill is NOT NULL)
begin
	BEGIN TRY
		EXEC (@stmt_kill);
	END TRY
	BEGIN CATCH
			SELECT
				@stmt_info = STUFF(@xml.query('_info').value('.', 'nvarchar(max)'), 1, LEN(@rnt), N'')

		SET @stmt_info = CONCAT(
			N'Original SQL error:'
			, @rnt, 'Error number: ', ERROR_NUMBER()
			, @rnt, 'Error message: ', ERROR_MESSAGE()
			, @rn, N'Session info before kill:'
			, @rnt, N'resource_database_name : ', N{dbName.QuoteName('\'')}
			, @rnt, N'spid to kill           : ', ISNULL(@stmt_spid, N'')
			, @rnt, N'spid to keep           : ', @@SPID, N', {spidToKeep}'
			, @rnt, @stmt_info
			)
	END CATCH
end

SELECT
	stmt_spid = ISNULL(@stmt_spid, N'')
	, stmt_info = ISNULL(@stmt_info, N'')

"
				);

			if (string.IsNullOrWhiteSpace(sql))
			{
				throw new InvalidOperationException("sql cannot be empty");
			}

			return KillConnections(connection, sql, loginCutoffTime, feedbackMethod);
		}

		static bool KillSessionedTransactions(DbConnection connection, string dbName, int spidToKeep, DateTime loginCutoffTime, Action<string> feedbackMethod)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = FormattableString.Invariant($@"
DECLARE
	@rn  nchar(2) = CHAR(13) + CHAR(10),
	@rnt nchar(3) = CHAR(13) + CHAR(10) + CHAR(9)
	, @stmt_kill nvarchar(max) = N''
	, @stmt_spid nvarchar(max)
	, @stmt_info nvarchar(max)

SELECT
	@stmt_kill += N'BEGIN TRY KILL ' + CONVERT(nvarchar(9), s.session_id) + N'; END TRY BEGIN CATCH if (ERROR_NUMBER() NOT in ({ErrorsToIgnore})) THROW; END CATCH;' + @rn,
	@stmt_spid = ISNULL(@stmt_spid + N',', N'') + CONVERT(nvarchar(9), s.session_id)
	, @stmt_info = ISNULL(@stmt_info + @rnt, N'')
		+ N'-----------------------------------------------------------------'
		+ CONCAT(@rnt, N'session_id                   : ', s.session_id                                              )
		+ CONCAT(@rnt, N'is_user_process              : ', MIN(CONVERT(int, s.is_user_process)                      ))
		+ CONCAT(@rnt, N'status                       : ', MIN(s.status                                             ))
		+ CONCAT(@rnt, N'open_transaction_count       : ', MIN(s.open_transaction_count                             ))
		+ CONCAT(@rnt, N'host_name                    : ', MIN(s.host_name                                          ))
		+ CONCAT(@rnt, N'program_name                 : ', MIN(s.program_name                                       ))
		+ CONCAT(@rnt, N'client_interface_name        : ', MIN(s.client_interface_name                              ))
		+ CONCAT(@rnt, N'host_process_id              : ', MIN(s.host_process_id                                    ))
		+ CONCAT(@rnt, N'authenticating_database_name : ', MIN(DB_NAME(s.authenticating_database_id)                ))
		+ CONCAT(@rnt, N'database_name                : ', MIN(DB_NAME(s.database_id)                               ))
		+ CONCAT(@rnt, N'nt_domain                    : ', MIN(s.nt_domain                                          ))
		+ CONCAT(@rnt, N'nt_user_name                 : ', MIN(s.nt_user_name                                       ))
		+ CONCAT(@rnt, N'original_login_name          : ', MIN(s.original_login_name                                ))
		+ CONCAT(@rnt, N'login_name                   : ', MIN(s.login_name                                         ))
		+ CONCAT(@rnt, N'login_time                   : ', MIN(CONVERT(nvarchar(23), s.login_time, 121)             ))
		+ CONCAT(@rnt, N'cpu_time                     : ', MIN(s.cpu_time                                           ))
		+ CONCAT(@rnt, N'total_elapsed_time           : ', MIN(s.total_elapsed_time                                 ))
		+ CONCAT(@rnt, N'last_request_start_time      : ', MIN(CONVERT(nvarchar(23), s.last_request_start_time, 121)))
		+ CONCAT(@rnt, N'last_request_end_time        : ', MIN(CONVERT(nvarchar(23), s.last_request_end_time  , 121)))
		+ CONCAT(@rnt, N'reads                        : ', MIN(s.reads                                              ))
		+ CONCAT(@rnt, N'writes                       : ', MIN(s.writes                                             ))
		+ CONCAT(@rnt, N'logical_reads                : ', MIN(s.logical_reads                                      ))
		+ CONCAT(@rnt, N'row_count                    : ', MIN(s.row_count                                          ))
FROM
	sys.dm_tran_database_transactions     AS dt
	JOIN sys.dm_tran_session_transactions AS st ON st.transaction_id = dt.transaction_id
	JOIN sys.dm_exec_sessions             AS s  ON s.session_id = st.session_id
WHERE 1=1
	AND s.session_id NOT in (@@SPID, {spidToKeep})
	AND s.is_user_process = 1
	AND dt.database_id = DB_ID(N{dbName.QuoteName('\'')})
	AND s.login_time <= @loginCutoffTime
GROUP BY
	s.session_id
OPTION (RECOMPILE)

if (@stmt_kill <> N'')
begin
	BEGIN TRY
		EXEC (@stmt_kill);

		SET @stmt_info = NULL;
	END TRY
	BEGIN CATCH
		SET @stmt_info = CONCAT(N'Original SQL error:'
			, @rnt, 'Error number: ', ERROR_NUMBER()
			, @rnt, 'Error message: ', ERROR_MESSAGE()
			, @rn, N'Session info before kill:'
			, @rnt, N'resource_database_name : ', N{dbName.QuoteName('\'')}
			, @rnt, N'spid to kill           : ', ISNULL(@stmt_spid, N'')
			, @rnt, N'spid to keep           : ', @@SPID, N', {spidToKeep}'
			, @rnt, @stmt_info
			)
	END CATCH
end

SELECT
	stmt_spid = ISNULL(@stmt_spid, N'')
	, stmt_info = ISNULL(@stmt_info, N'')

"
				);

			if (string.IsNullOrWhiteSpace(sql))
			{
				throw new InvalidOperationException("sql cannot be empty");
			}

			return KillConnections(connection, sql, loginCutoffTime, feedbackMethod);
		}

		static bool KillRemoteConnectionsAndDistributedOrphanTransactions(DbConnection connection, string dbName, int spidToKeep, DateTime loginCutoffTime, Action<string> feedbackMethod)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var sql = FormattableString.Invariant($@"
DECLARE
	@rn  nchar(2) = CHAR(13) + CHAR(10),
	@rnt nchar(3) = CHAR(13) + CHAR(10) + CHAR(9)
	, @stmt_kill nvarchar(max) = N''
	, @stmt_spid nvarchar(max)
	, @stmt_info nvarchar(max)

;WITH
	data AS (
			SELECT TOP (10)
				s.*
				, request_owner_guid = CASE WHEN l.request_session_id = -2 THEN l.request_owner_guid ELSE NULL END
			FROM
				sys.dm_tran_locks              AS l
				LEFT JOIN sys.dm_exec_sessions AS s ON s.session_id = l.request_session_id
					AND s.is_user_process = 1
					AND l.request_session_id <> -2
			WHERE 1=1
				AND l.resource_database_id = DB_ID(N{dbName.QuoteName('\'')})
				AND (s.login_time is NULL OR s.login_time <= @loginCutoffTime)
				AND (l.resource_Type = N'DATABASE' AND l.request_session_id NOT in (@@SPID, {spidToKeep}) AND l.request_session_id <> -2 OR l.request_session_id = -2)
				AND
				(1=2
					OR
					(1=1
						-- Remote connections
						AND l.resource_Type = N'DATABASE'
						AND l.request_session_id NOT in (@@SPID, {spidToKeep})
						AND l.request_session_id <> -2
						AND s.session_id is NOT NULL
					)
					OR
					(1=1
						-- Distributed orphan transactions
						AND l.request_session_id = -2
						AND l.request_owner_guid is NOT NULL
					)
				)
		)
SELECT
	@stmt_kill += N'BEGIN TRY KILL ' + ISNULL(CONVERT(nvarchar(38), session_id), QUOTENAME(CONVERT(nvarchar(36), request_owner_guid), N''''))
		+ N'; END TRY BEGIN CATCH if (ERROR_NUMBER() NOT in ({ErrorsToIgnore})) THROW; END CATCH;' + @rn,
	@stmt_spid = ISNULL(@stmt_spid + N',', N'') + ISNULL(CONVERT(nvarchar(38), session_id), QUOTENAME(CONVERT(nvarchar(36), request_owner_guid), N''''))
	, @stmt_info = ISNULL(@stmt_info + @rnt, N'')
		+ N'-----------------------------------------------------------------'
		+ CONCAT(@rnt, N'session_id                   : ', s.session_id                                              )
		+ CONCAT(@rnt, N'is_user_process              : ', MIN(CONVERT(int, s.is_user_process)                      ))
		+ CONCAT(@rnt, N'status                       : ', MIN(s.status                                             ))
		+ CONCAT(@rnt, N'open_transaction_count       : ', MIN(s.open_transaction_count                             ))
		+ CONCAT(@rnt, N'host_name                    : ', MIN(s.host_name                                          ))
		+ CONCAT(@rnt, N'program_name                 : ', MIN(s.program_name                                       ))
		+ CONCAT(@rnt, N'client_interface_name        : ', MIN(s.client_interface_name                              ))
		+ CONCAT(@rnt, N'host_process_id              : ', MIN(s.host_process_id                                    ))
		+ CONCAT(@rnt, N'authenticating_database_name : ', MIN(DB_NAME(s.authenticating_database_id)                ))
		+ CONCAT(@rnt, N'database_name                : ', MIN(DB_NAME(s.database_id)                               ))
		+ CONCAT(@rnt, N'nt_domain                    : ', MIN(s.nt_domain                                          ))
		+ CONCAT(@rnt, N'nt_user_name                 : ', MIN(s.nt_user_name                                       ))
		+ CONCAT(@rnt, N'original_login_name          : ', MIN(s.original_login_name                                ))
		+ CONCAT(@rnt, N'login_name                   : ', MIN(s.login_name                                         ))
		+ CONCAT(@rnt, N'login_time                   : ', MIN(CONVERT(nvarchar(23), s.login_time, 121)             ))
		+ CONCAT(@rnt, N'cpu_time                     : ', MIN(s.cpu_time                                           ))
		+ CONCAT(@rnt, N'total_elapsed_time           : ', MIN(s.total_elapsed_time                                 ))
		+ CONCAT(@rnt, N'last_request_start_time      : ', MIN(CONVERT(nvarchar(23), s.last_request_start_time, 121)))
		+ CONCAT(@rnt, N'last_request_end_time        : ', MIN(CONVERT(nvarchar(23), s.last_request_end_time  , 121)))
		+ CONCAT(@rnt, N'reads                        : ', MIN(s.reads                                              ))
		+ CONCAT(@rnt, N'writes                       : ', MIN(s.writes                                             ))
		+ CONCAT(@rnt, N'logical_reads                : ', MIN(s.logical_reads                                      ))
		+ CONCAT(@rnt, N'row_count                    : ', MIN(s.row_count                                          ))
FROM
	data AS s
GROUP BY
	s.session_id, s.request_owner_guid
OPTION (RECOMPILE)

if (@stmt_kill <> N'')
begin
	BEGIN TRY
		EXEC (@stmt_kill);

		SET @stmt_info = NULL;
	END TRY
	BEGIN CATCH
		SET @stmt_info = CONCAT(N'Original SQL error:'
			, @rnt, 'Error number: ', ERROR_NUMBER()
			, @rnt, 'Error message: ', ERROR_MESSAGE()
			, @rn, N'Session info before kill:'
			, @rnt, N'resource_database_name : ', N{dbName.QuoteName('\'')}
			, @rnt, N'spid to kill           : ', ISNULL(@stmt_spid, N'')
			, @rnt, N'spid to keep           : ', @@SPID, N', {spidToKeep}'
			, @rnt, @stmt_info
			)
	END CATCH
end

SELECT
	stmt_spid = ISNULL(@stmt_spid, N'')
	, stmt_info = ISNULL(@stmt_info, N'')

"
				);

			if (string.IsNullOrWhiteSpace(sql))
			{
				throw new InvalidOperationException("sql cannot be empty");
			}

			return KillConnections(connection, sql, loginCutoffTime, feedbackMethod);
		}

		static string SQL_KillWithStatusonlyFormat
		{
			get
			{
				return @"
BEGIN TRY
	KILL {0} WITH STATUSONLY;
END TRY
BEGIN CATCH
	SET @spid = CONCAT(@spid + N',', {0});
END CATCH
"; // Direct SQL statement
			}
		}

		static bool KillConnections(DbConnection connection, string sql, DateTime loginCutoffTime, Action<string> feedbackMethod)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(sql, nameof(sql));

			var sessions = string.Empty;
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@loginCutoffTime", SqlDbType.DateTime, loginCutoffTime);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						sessions = (string)reader["stmt_spid"];
						var errorSessionInfo = (string)reader["stmt_info"];
						if (!string.IsNullOrWhiteSpace(errorSessionInfo))
						{
							throw new InvalidOperationException(errorSessionInfo);
						}
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(sessions))
			{
				var infos = sessions.Split(',').Select(spid => new ConnectionInfo(sessionId: spid)).ToList();
				PendingRollback(connection, infos, feedbackMethod);

				return false;
			}

			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1060:Do not use System.DateTime.Now Rule", Justification = "To compute a period")]
		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "full name needed")]
		public static void PendingRollback(DbConnection connection, List<ConnectionInfo> infos, Action<string> feedbackMethod)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(infos, nameof(infos));

#if NET
			Microsoft.Data.SqlClient.SqlInfoMessageEventHandler infoMessageHandlerMS = null;
			if (feedbackMethod != null)
			{
				infoMessageHandlerMS = new Microsoft.Data.SqlClient.SqlInfoMessageEventHandler((object sender, Microsoft.Data.SqlClient.SqlInfoMessageEventArgs e) =>
				{
					var messages = e.Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
					feedbackMethod(GetFormattedMessage(messages.Length, messages[0]));
				});
			}
#endif

			System.Data.SqlClient.SqlInfoMessageEventHandler infoMessageHandlerSys = null;
			if (feedbackMethod != null)
			{
				infoMessageHandlerSys = new System.Data.SqlClient.SqlInfoMessageEventHandler((object sender, System.Data.SqlClient.SqlInfoMessageEventArgs e) =>
				{
					var messages = e.Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
					feedbackMethod(GetFormattedMessage(messages.Length, messages[0]));
				});
			}

			try
			{
				if (feedbackMethod != null)
				{
					var adoConnection = ((IDbConnectionInternals)connection).ADOConnection;
					if (adoConnection != null)
					{
#if NET
						if (adoConnection is Microsoft.Data.SqlClient.SqlConnection sqlParameterMS)
						{
							sqlParameterMS.InfoMessage += infoMessageHandlerMS;
						}
#endif
						if(adoConnection is System.Data.SqlClient.SqlConnection sqlConnectionSys)
						{
							sqlConnectionSys.InfoMessage += infoMessageHandlerSys;
						}
					}
				}

				var timeStarted = DateTime.Now; // used to check timeout time
				while (infos.Count > 0)
				{
					var spids = string.Join(Environment.NewLine, infos.Select(info => string.Format(CultureInfo.InvariantCulture, SQL_KillWithStatusonlyFormat, info.SessionId)));
					var sql = string.Format(CultureInfo.InvariantCulture, @"
DECLARE
	@spid  nvarchar(max),
	@error int

{0}

SELECT
	completed = ISNULL(@spid, N'')
" // Direct SQL statement
						, spids // 0
						);

					var completed = ExecutePendingRollback(connection, sql);

					if (!string.IsNullOrWhiteSpace(completed))
					{
						foreach (var spid in completed.Split(','))
						{
							var info = infos.FirstOrDefault(i => i.SessionId == spid);
							if (info != null)
							{
								infos.Remove(info);
							}
						}
					}

					if (infos.Any())
					{
						if (DateTime.Now.Subtract(rollingBackTimeout) > timeStarted) // used to check timeout time
						{
							var spidInfos = GetSpidInfos(
								connection,
								infos
									.Select(x => new { isInteger = int.TryParse(x.SessionId, out var value), spid = value })
									.Where(x => x.isInteger)
									.Select(x => x.spid).ToArray());

							ThrowTimeoutException(spidInfos);
						}

						Thread.Sleep(500);
					}
				}
			}
			finally
			{
				if (feedbackMethod != null)
				{
					var adoConnection = ((IDbConnectionInternals)connection).ADOConnection;
					if (adoConnection != null)
					{
#if NET
						if (adoConnection is Microsoft.Data.SqlClient.SqlConnection sqlParameterMS)
						{
							sqlParameterMS.InfoMessage -= infoMessageHandlerMS;
						}
#endif
						if (adoConnection is System.Data.SqlClient.SqlConnection sqlConnectionSys)
						{
							sqlConnectionSys.InfoMessage -= infoMessageHandlerSys;
						}
					}
				}
			}
		}

		static string ExecutePendingRollback(DbConnection connection, string sql)
		{
#if DEBUG
			if (SkipExecution_ForTest.Value)
			{
				return string.Empty;
			}
#endif

			return connection.ExecuteScalar<string>(sql);
		}

		static string GetSpidInfos(DbConnection connection, IEnumerable<int> spids)
		{
			var spidString = string.Join(",", spids);
			var sql = Invariant($@"
DECLARE
	@rnt nchar(3) = CHAR(13) + CHAR(10) + CHAR(9)

SELECT 
	spid_info = CONCAT(@rnt
		, N'-----------------------------------------------------------------'
		, @rnt, N'session_id                   : ', s.session_id                                              
		, @rnt, N'is_user_process              : ', MIN(CONVERT(int, s.is_user_process)                      )
		, @rnt, N'status                       : ', MIN(s.status                                             )
		, @rnt, N'open_transaction_count       : ', MIN(s.open_transaction_count                             )
		, @rnt, N'host_name                    : ', MIN(s.host_name                                          )
		, @rnt, N'program_name                 : ', MIN(s.program_name                                       )
		, @rnt, N'client_interface_name        : ', MIN(s.client_interface_name                              )
		, @rnt, N'host_process_id              : ', MIN(s.host_process_id                                    )
		, @rnt, N'authenticating_database_name : ', MIN(DB_NAME(s.authenticating_database_id)                )
		, @rnt, N'database_name                : ', MIN(DB_NAME(s.database_id)                               )
		, @rnt, N'nt_domain                    : ', MIN(s.nt_domain                                          )
		, @rnt, N'nt_user_name                 : ', MIN(s.nt_user_name                                       )
		, @rnt, N'original_login_name          : ', MIN(s.original_login_name                                )
		, @rnt, N'login_name                   : ', MIN(s.login_name                                         )
		, @rnt, N'login_time                   : ', MIN(CONVERT(nvarchar(23), s.login_time, 121)             )
		, @rnt, N'cpu_time                     : ', MIN(s.cpu_time                                           )
		, @rnt, N'total_elapsed_time           : ', MIN(s.total_elapsed_time                                 )
		, @rnt, N'last_request_start_time      : ', MIN(CONVERT(nvarchar(23), s.last_request_start_time, 121))
		, @rnt, N'last_request_end_time        : ', MIN(CONVERT(nvarchar(23), s.last_request_end_time  , 121))
		, @rnt, N'reads                        : ', MIN(s.reads                                              )
		, @rnt, N'writes                       : ', MIN(s.writes                                             )
		, @rnt, N'logical_reads                : ', MIN(s.logical_reads                                      )
		, @rnt, N'row_count                    : ', MIN(s.row_count                                          )
		)
FROM
	sys.dm_exec_sessions AS s
WHERE 1=1
	AND s.is_user_process = 1
	AND s.session_id in ({spidString})
GROUP BY
	s.session_id
OPTION(RECOMPILE)

");

			var spidInfos = new List<string>();
			connection.ExecuteReader(sql, reader => spidInfos.Add((string)reader["spid_info"]));

			return string.Join(Environment.NewLine, spidInfos);
		}

		static string GetFormattedMessage(int totalCount, string message)
		{
			return string.Format(CultureInfo.InvariantCulture, "1/{0} => {1}", totalCount, message);
		}

		const string ErrorsToIgnore = "6106, 6107, 6120, 6109";

		#region Implementation

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static TimeSpan rollingBackTimeout = TimeSpan.FromMinutes(10);
		public const string TimeoutExceptionMessagePrefix = "Killing user connections has exceeded timeout limit";

		static void ThrowTimeoutException(string spidInfos)
		{
			throw new TimeoutException(
				Invariant($"{TimeoutExceptionMessagePrefix} of {rollingBackTimeout.TotalMinutes:0} minutes.\r\n{spidInfos}")); // Backend Information Messages
		}

		static (string HostName, int HostProcessId) GetCurrentProcessInfo(AdminConnection connection)
		{
			var host_name = "";
			var host_process_id = 0;
			connection.ExecuteReader(@"
SELECT
	host_name,
	host_process_id
FROM
	sys.dm_exec_sessions
WHERE 1=1
	AND session_id = @@SPID

" // Direct SQL statement
				, (reader) =>
				{
					host_name = (string)reader["host_name"];
					host_process_id = (int)reader["host_process_id"];
				});

			return (host_name, host_process_id);
		}

		#endregion // Implementation

		#region Helper classes

		[DebuggerDisplay("SPID: {SessionId}, Message: {Message}")]
		public class ConnectionInfo
		{
			public ConnectionInfo(string sessionId)
			{
				SessionId = sessionId;
			}

			// Connection
			public string NetTransport { get; }
			public string EncryptOption { get; }
			public string AuthScheme { get; }
			public DateTime ConnectTime { get; }

			// Session
			public string SessionId { get; }
			public string HostName { get; }
			public string ProgramName { get; }
			public int HostProcessId { get; }
			public string ClientInterfaceName { get; }
			public string LoginName { get; }
			public string OriginalLoginName { get; }
			public string NtDomain { get; }
			public string NtUserName { get; }
			public DateTime LoginTime { get; }

			public string Message { get; set; }
		}

		#endregion // Helper classes
	}
}

#region Test
#if DEBUG

namespace CargoWise.Data
{
	using System.Diagnostics.CodeAnalysis;

	#region Partial class

	public static partial class DbConnectionKiller
	{
		public static IDisposable OverrideRollingBackTimeout_ForTest(TimeSpan value)
		{
			var savedValue = rollingBackTimeout;
			rollingBackTimeout = value;
			return new DisposableAction(() => rollingBackTimeout = savedValue);
		}

		public static (string HostName, int HostProcessId) GetCurrentProcessInfo_Exposed()
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.IsUpgradeCheckDisabled = true;
				return GetCurrentProcessInfo(connection);
			}
		}

		public static int GetConnectionsCount_ForTest(string hostName, int hostProcessId)
		{
			using (var connection = Db.NewAdminConnection())
			{
				return connection.ExecuteScalar<int>(@"
SELECT
	cnt = COUNT(*)
FROM
	sys.dm_exec_sessions
WHERE 1=1
	AND session_id <> @@SPID
	AND host_name = @host_name
	AND host_process_id = @host_process_id

"
					, (cmd) =>
					{
						cmd.AddParameter("@host_name", SqlDbType.NVarChar, 128, hostName);
						cmd.AddParameter("@host_process_id", SqlDbType.Int, hostProcessId);
					});
			}
		}

		public static IDisposable SetRollingBackTimeout_ForTest(TimeSpan value)
		{
			var savedValue = rollingBackTimeout;
			rollingBackTimeout = value;

			return new DisposableAction(() =>
			{
				rollingBackTimeout = savedValue;
			});
		}

		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static Overridable<bool> SkipExecution_ForTest = new Overridable<bool>(false);

		public static void ThrowTimeoutException_Exposed(string spidInfos)
		{
			ThrowTimeoutException(spidInfos);
		}
	}

	#endregion

}

#endif
#endregion
