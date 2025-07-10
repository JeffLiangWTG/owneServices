namespace Enterprise.ChangeDataCapture.Common
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using CargoWise.Data;

	#region SuppressResourceStringsCheckRegion

	public static class CdcDatabase
	{
		#region Enable CDC

		public static string CheckIsEnabled()
		{
			if (IsEnabled(Db.Connection, Db.DatabaseName))
			{
				return string.Empty;
			}

			return "Cdc is not enabled."; // information for logging only.
		}

		public static bool IsEnabled(DbConnection conn, string dbName)
		{
			bool result = false;
			if (conn.DatabaseExists(dbName))
			{
				var sqlText = $"IF EXISTS(SELECT NULL FROM SYS.Schemas WHERE [Name] = 'CDC') SELECT 1 ELSE SELECT 0";
				object objResult;
				using (((ICurrentDbControl)conn).UseDatabase(dbName))
				{
					objResult = conn.ExecuteScalar(sqlText);
					result = Convert.ToBoolean(objResult, CultureInfo.InvariantCulture);
				}
			}
			return result;
		}

		public static void Enable(AdminConnection conn, string dbName)
		{
			string sqlProc = String.Format(CultureInfo.InvariantCulture, "[{0}].sys.sp_cdc_enable_db", dbName); // database command

			using (var cmd = conn.Command(sqlProc))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
			}

			DisableCdcTrigger(conn, dbName);
			EnsureCdcUserIsMemberOfdbOwnerRole(conn, dbName);
			ResetCdcTableList(conn, dbName);
		}

		static void DisableCdcTrigger(DbConnection conn, string dbName)
		{
			var sqlText = String.Format(CultureInfo.InvariantCulture, @"
				EXEC [{0}]..sp_executesql N'
					DECLARE @TriggerName sysname = ''{1}'';
					IF EXISTS(SELECT null FROM sys.triggers WHERE name = @TriggerName AND parent_class = 0)
					BEGIN
						DISABLE TRIGGER [{1}] ON DATABASE;
					END'",
				dbName,
				CdcTriggerName);
			conn.ExecuteNonQuery(sqlText);
		}
		internal const string CdcTriggerName = "tr_MScdc_ddl_event";

		static void EnsureCdcUserIsMemberOfdbOwnerRole(DbConnection conn, string dbName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, "EXEC [{0}]..sp_executesql N'ALTER ROLE [db_owner] ADD MEMBER [cdc]'", dbName);
			conn.ExecuteNonQuery(sqlText);
		}

		static void ResetCdcTableList(DbConnection conn, string dbName)
		{
			string sqlText = @"
IF OBJECT_ID('cdc.CdcTables') IS NULL
BEGIN
	IF SCHEMA_ID('cdc') IS NULL
	BEGIN
		EXEC sp_executesql N'CREATE SCHEMA cdc';
	END

	CREATE TABLE cdc.CdcTables (CaptureInstance SYSNAME NOT NULL, Lsn BINARY(10) NOT NULL, LastDUProcessedLsn BINARY(10) DEFAULT 0x0000000000, PkColumnList VARCHAR(MAX) DEFAULT NULL)

	ALTER TABLE cdc.CdcTables
	ADD CONSTRAINT PK_UX__CdcTables_CaptureInstance PRIMARY KEY CLUSTERED (CaptureInstance)
END
ELSE
	TRUNCATE TABLE cdc.CdcTables";
			conn.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Disable CDC

		public static void DisableCaptureInstancesAndEnableChangeTrackingIfRequired(AdminConnection conn, bool shouldEnableChangeTracking)
		{
			DisableCaptureInstances(conn);
			if (shouldEnableChangeTracking)
			{
				EnableChangeTracking(conn);
			}
		}

		public static void DisableCaptureInstances(AdminConnection conn)
		{
			foreach (var cdcTable in GetCdcTableList(conn))
			{
				cdcTable.DisableCdcInstances(conn);
			}
		}

		public static void EnableChangeTracking(AdminConnection conn)
		{
			using (var stream = Assembly.Load("CargoWise.Glow.Model.CW.Resources").GetManifestResourceStream("CargoWise.Glow.Model.CW1.Resources.EnableChangeTracking.sql"))
			{
				if (stream == null)
				{
					throw new InvalidOperationException("EnableChangeTracking.SQL Resource Stream is null");
				}
				using (var reader = new StreamReader(stream))
				{
					var sql = reader.ReadToEnd();
					using (var command = conn.Command(sql))
					{
						command.ExecuteNonQuery();
					}
				}
			}
		}

		public static void Disable(AdminConnection conn, string dbName)
		{
			string sqlProc = string.Format(CultureInfo.InvariantCulture, "[{0}].sys.sp_cdc_disable_db", dbName); // database command

			using (var cmd = conn.Command(sqlProc))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
			}

			DropCdcTableListOnMainDb(conn, dbName);
		}

		static void DropCdcTableListOnMainDb(DbConnection conn, string dbName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
			IF EXISTS (select * from [{0}].sys.tables where name = 'CdcTables' and schema_name(schema_id) = 'cdc')
			DROP TABLE [{0}].cdc.CdcTables", dbName); // database command
			conn.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region CDC Internal Upgrade

		public static void RunInternalCdcUpgrade(AdminConnection connection, string dbName)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, "[{0}].sys.sp_cdc_vupgrade", dbName); // database command

			using (var command = connection.Command(sqlText, 3600))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.ExecuteNonQuery();
			}
		}

		public static bool NullableEndLsnExists(DbConnection connection, string dbName)
		{
			var sqlText = @"
if exists
	(select null
		from sys.tables ct
		join sys.columns c on c.object_id = ct.object_id
	where
		schema_name(ct.schema_id) = 'cdc'
		and ct.name like '%[_]%[_]CT'
		and c.name = '__$end_lsn'
		and c.is_nullable = 1)
	select 1
else
	select 0";

			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				return Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
			}
		}

		#endregion

		#region CDC Jobs

		public static void DisableAndStopJobsAndTrigger(DbConnection conn, string dbName)
		{
			DisableCdcTrigger(conn, dbName);
			DisableCdcJobs(conn, dbName);
			StopCdcJobs(conn, dbName);
		}

		public static bool IsCdcJobEnabled(DbConnection conn, string dbName, string jobType)
		{
			try
			{
				return IsCdcJobEnabledUnsafe(conn, dbName, jobType);
			}
			catch (SqlException ex)
			{
				if (IsMissingMsdbCdcObjectError(ex))
				{
					return false;
				}
				else
				{
					throw;
				}
			}
		}

		static bool IsCdcJobEnabledUnsafe(DbConnection conn, string dbName, string jobType)
		{
			string sqlText = @"
SET @IsEnabled = 0
SELECT 
	@IsEnabled = sysjob.enabled
FROM
	msdb.dbo.cdc_jobs cdcjob
	INNER JOIN sys.databases db ON cdcjob.database_id = db.database_id
	INNER JOIN msdb.dbo.sysjobs sysjob ON cdcjob.job_id = sysjob.job_id
WHERE
	db.name = @DbName
	AND cdcjob.job_type = @JobType";

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, dbName);
				cmd.AddParameter("@JobType", SqlDbType.NVarChar, 128, jobType);
				cmd.AddOutputParameter("@IsEnabled", SqlDbType.Bit, 0, 0, 0, null);

				cmd.ExecuteNonQuery();
				return (bool)cmd.GetParameterValue("@IsEnabled");
			}
		}

		public static bool IsCdcJobRunning(DbConnection conn, string dbName, string jobType)
		{
			try
			{
				return IsCdcJobRunningUnsafe(conn, dbName, jobType);
			}
			catch (SqlException ex)
			{
				if (IsMissingMsdbCdcObjectError(ex))
				{
					return false;
				}
				else
				{
					throw;
				}
			}
		}

		static bool IsCdcJobRunningUnsafe(DbConnection conn, string dbName, string jobType)
		{
			string sqlText = @"
IF EXISTS (SELECT NULL
	FROM
		msdb.dbo.sysjobs job
		INNER JOIN msdb.dbo.sysjobactivity activity	ON job.job_id = activity.job_id 
		INNER JOIN msdb.dbo.syssessions sess ON sess.session_id = activity.session_id
		INNER JOIN msdb.dbo.cdc_jobs cdcjob ON cdcjob.job_id = job.job_id
		INNER JOIN sys.databases db ON db.database_id = cdcjob.database_id
	WHERE
		sess.agent_start_date = (
			SELECT MAX(agent_start_date) 
			FROM msdb.dbo.syssessions s 
			INNER JOIN msdb.dbo.sysjobactivity a ON s.session_id = a.session_id
			WHERE a.job_id = job.job_id)
		AND activity.run_requested_date is not null
		AND	activity.stop_execution_date is null
		AND db.name = @DbName
		AND cdcjob.job_type = @JobType)
	SET @IsRunning = 1
ELSE
	SET @IsRunning = 0";

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, dbName);
				cmd.AddParameter("@JobType", SqlDbType.NVarChar, 128, jobType);
				cmd.AddOutputParameter("@IsRunning", SqlDbType.Bit, 0, 0, 0, null);

				cmd.ExecuteNonQuery();
				return (bool)cmd.GetParameterValue("@IsRunning");
			}
		}

		static bool IsMissingMsdbCdcObjectError(SqlException ex)
		{
			return (
				new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName
				&& new Regex(@"msdb\.\w*\.cdc_", RegexOptions.IgnoreCase).IsMatch(ex.Message)
			);
		}

		public static void DisableCdcJobs(DbConnection conn, string dbName, string jobType = null)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
DECLARE @SqlText nvarchar(max);

IF EXISTS (SELECT null FROM msdb.sys.objects WHERE name = 'cdc_jobs')
BEGIN
	SELECT 
		@SqlText =
			isnull(@SqlText, '')
			+ 'EXEC msdb.dbo.sp_update_job @job_name = ''' + sysjob.name + ''', @enabled = 0;'
	FROM
		msdb.dbo.cdc_jobs cdcjob
		INNER JOIN sys.databases db ON cdcjob.database_id = db.database_id
		INNER JOIN msdb.dbo.sysjobs sysjob ON cdcjob.job_id = sysjob.job_id
	WHERE
		db.name = @DbName
		AND sysjob.enabled = 1{0};

	IF (@SqlText is not null) EXEC (@SqlText);
END", !string.IsNullOrEmpty(jobType) ? "\r\n\t\tAND cdcjob.job_type = @JobType" : "");

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, dbName);
				if (!string.IsNullOrEmpty(jobType))
				{
					cmd.AddParameter("@JobType", SqlDbType.NVarChar, 128, jobType);
				}
				cmd.ExecuteNonQuery();
			}
		}

		public static void StopCdcJobs(DbConnection conn, string dbName, string jobType = null)
		{
			try
			{
				string sqlText = String.Format(CultureInfo.InvariantCulture, @"
DECLARE @SqlText nvarchar(max);

IF EXISTS (SELECT null FROM msdb.sys.objects WHERE name = 'cdc_jobs')
BEGIN
	SELECT
		@SqlText =
			isnull(@SqlText, '')
			+ 'EXEC msdb.dbo.sp_stop_job @job_name = ''' + job.name + ''';'
	FROM
		msdb.dbo.sysjobs job
		INNER JOIN msdb.dbo.sysjobactivity activity	ON job.job_id = activity.job_id 
		INNER JOIN msdb.dbo.syssessions sess ON sess.session_id = activity.session_id
		INNER JOIN msdb.dbo.cdc_jobs cdcjob ON cdcjob.job_id = job.job_id
		INNER JOIN sys.databases db ON db.database_id = cdcjob.database_id
	WHERE
		sess.agent_start_date = (
			SELECT MAX(agent_start_date) 
			FROM msdb.dbo.syssessions s 
			INNER JOIN msdb.dbo.sysjobactivity a ON s.session_id = a.session_id
			WHERE a.job_id = job.job_id)
		AND activity.run_requested_date is not null
		AND	activity.stop_execution_date is null
		AND db.name = @DbName{0};

	IF (@SqlText is not null) EXEC (@SqlText);
END", !string.IsNullOrEmpty(jobType) ? "\r\n\t\tAND cdcjob.job_type = @JobType" : "");

				using (var cmd = conn.Command(sqlText))
				{
					cmd.AddParameter("@DbName", SqlDbType.NVarChar, 128, dbName);
					if (!string.IsNullOrEmpty(jobType))
					{
						cmd.AddParameter("@JobType", SqlDbType.NVarChar, 128, jobType);
					}
					cmd.ExecuteNonQuery();
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.RequestToStopCdcJobRefused)
			{
				// Ignored because there is already a pending request or job has been stopped already
			}
		}

		#endregion

		#region CDC Table List

		static List<CdcTable> GetCdcTableList(DbConnection connection)
		{
			var result = new List<CdcTable>();

			var sqlText = "SELECT schema_name(schema_id), name FROM sys.tables WHERE is_tracked_by_cdc = 1";
			using (var cmd = connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schemaName = reader.GetString(0);
					var tableName = reader.GetString(1);
					result.Add(new CdcTable(schemaName, tableName));
				}
			}

			return result;
		}

		#endregion

		public const string IsFirstCdcRunExtPropertyName = "IsFirstCdcRun";
		public const string FileGroup = "CDCGROUP";
	}

	#endregion
}
