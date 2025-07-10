using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.ChangeDataCapture.Service.CleanupTask.ServiceTaskCode,
	Enterprise.ChangeDataCapture.Service.CleanupTask.ServiceTaskName,
	"BI",
	typeof(Enterprise.ChangeDataCapture.Service.CleanupTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MinimumPeriod = "1hour",
	MaximumPeriod = "1day",
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "1hour",
	DefaultScheduleStartAtLocal = "1hour",
	ActiveByDefault = true
	)
]

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(BiConstants))]

namespace Enterprise.ChangeDataCapture.Service
{
	public class CleanupTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "CDN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDN service task name")]
		public const string ServiceTaskName = "Change Data Capture - cleanup service";

		internal virtual TimeSpan CDCRetentionPeriod => IsGlowConfigured ? TimeSpan.FromDays(1) : TimeSpan.Zero;

		bool IsGlowConfigured => isGlowConfigured ??= !string.IsNullOrEmpty(GlowRegistry.Instance.GlowServiceUri?.Trim('/'));
		bool? isGlowConfigured;

		public bool BiDisableChangeDataCapture => biDisableChangeDataCapture ??= DbRegistry.BiDisableChangeDataCapture.LoadValue(Db.Connection);
		bool? biDisableChangeDataCapture;

		[HostedServiceRequirement]
		public static string CheckCdcIsEnabled() => CdcDatabase.CheckIsEnabled();

		[HostedServiceRequirement]
		public static string CheckIsBiEnabled() => BiServiceTaskHelpers.IsBusinessIntelligenceEnabled();

		protected DbConnection MainDbConnection;

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var cdcEnabled = CdcDatabase.IsEnabled(Db.Connection, Db.DatabaseName);
			if (cdcEnabled && BiDisableChangeDataCapture)
			{
				var message = IsGlowConfigured ? $" Enabling Change Tracking as a fallback." : string.Empty;
				ServiceLogger.Information($"Disabling CDC for tables on database '{Db.DatabaseName}'.{message}");

				using (var adminConnection = Db.NewAdminConnection())
				{
					CdcDatabase.DisableCaptureInstancesAndEnableChangeTrackingIfRequired(adminConnection, shouldEnableChangeTracking: IsGlowConfigured);
				}
			}
			else if (cdcEnabled && !BiDisableChangeDataCapture)
			{
				using (MainDbConnection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName))
				using (var auditConnection = string.IsNullOrWhiteSpace(AuditServer) ? null : Db.NewExtraConnectionWithMainDbCredentials(AuditServer, Db.SqlMasterDb))
				using (var dwConnection = string.IsNullOrWhiteSpace(DataWarehouseServer) ? null : Db.NewExtraConnectionWithMainDbCredentials(DataWarehouseServer, Db.SqlMasterDb))
				{
					CleanUpChangeTablesWithProcessedRows(auditConnection, dwConnection, youMustReactToThisToken);
				}
			}
			else if (!cdcEnabled && !BiDisableChangeDataCapture)
			{
				ServiceLogger.Information($"Database is not enabled for change data capture. Check if Data Warehouse and Audit registry items are set and perform database upgrade.");
			}
		}

		protected virtual string AuditServer => auditServer ??= BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
		string auditServer;

		// If Data Warehouse server were to be accidentally removed from the registry, it will still wait for the service controller to restart to clean it up.
		protected virtual string DataWarehouseServer => dataWarehouseServer ??= BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
		string dataWarehouseServer;

		byte[] GetMaxLsn()
		{
			var sqlText = @"SELECT sys.fn_cdc_get_max_lsn()"; // This is an SQL expression
			return GetBinaryValueOrZeroIfNull(MainDbConnection, sqlText);
		}

		// Cleans up all change tables up to what has been processed by BI's ETL's
		protected void CleanUpChangeTablesWithProcessedRows(DbConnection auditConnection, DbConnection dwConnection, CancellationToken token)
		{
			var lowWaterMark = GetLsnWatermark(auditConnection, dwConnection);
			var etlHasNotProcessedAnythingYet = lowWaterMark.All(b => b == 0);
			if (etlHasNotProcessedAnythingYet)
			{
				ServiceLogger.Log(LogType.Debug, $"No BI extraction service tasks have run. Skipping CDC clean up task.");
				return;
			}

			var capturedInstanceList = GetCapturedInstancesToCleanup(lowWaterMark);
			if (!capturedInstanceList.Any())
			{
				ServiceLogger.Log(LogType.Debug, $"No records to clean up.");
				return;
			}

			foreach (var capturedInstance in capturedInstanceList)
			{
				token.ThrowIfCancellationRequested();
				var cleanupSuccessful = CleanupChangeTable(capturedInstance, lowWaterMark);
				if (!cleanupSuccessful)
				{
					return;
				}
			}
			ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Cleaned up change tables up to LSN 0x{0}", BitConverter.ToString(lowWaterMark).Replace("-", "")));
		}

		public const int MaxRetires = 3;

		// Returns if the cleanup was successful
		bool CleanupChangeTable(string captureInstance, byte[] lowWaterMark)
		{
			ServiceLogger.Log(LogType.Debug, $"Cleaning up capture instance [{captureInstance}].");
			long initialUnprocessedRowCount = 0;
			var retriesCount = 0;
			bool hasTimedOut = false;

			long currentUnprocessedRowCount;
			do
			{
				try
				{
					initialUnprocessedRowCount = GetUnprocessedRowCount(captureInstance, lowWaterMark);
					ExecuteCleanupProcedure(captureInstance, lowWaterMark, defaultDeleteThreshold);
					hasTimedOut = false;
				}
				catch (CdcCleanupProcedureFailedException ex)
				{
					HandleProcedureFailedException(retriesCount, ex);
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired)
				{
					HandleLockTimeoutException(retriesCount, ex);
					hasTimedOut = true;
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.TimeoutExpired)
				{
					HandleDbTimeoutException(captureInstance, lowWaterMark, initialUnprocessedRowCount, ex);
					hasTimedOut = true;
				}
				currentUnprocessedRowCount = GetUnprocessedRowCount(captureInstance, lowWaterMark);
				// Only increment retries if we didnt make any progress, we will always keep trying if we are making progress.
				if (currentUnprocessedRowCount >= initialUnprocessedRowCount)
				{
					retriesCount++;
				}
				// If we had a timeout, something has failed to complete, so retry
			} while ((currentUnprocessedRowCount > 0 || hasTimedOut) && retriesCount <= MaxRetires);

			// If we have processed all rows but then have timed out, it is likely getting blocked on a control table such as cdc.change_tables
			// in this case we don't want to return a success because that would lead to waiting for a timeout once for every tracked table.
			return currentUnprocessedRowCount == 0 && !hasTimedOut;
		}

		void HandleProcedureFailedException(int retriesCount, Exception ex)
		{
			if (retriesCount >= MaxRetires)
			{
				ServiceLogger.Log(LogType.Error, ex.Message);
				throw new CdcException($"Cleanup procedure has failed with an exception {MaxRetires} times", ex);
			}
		}

		void HandleLockTimeoutException(int retriesCount, SqlException ex)
		{
			if (retriesCount >= MaxRetires)
			{
				var timeSinceLastCleanup = TimeSinceLastCleanup(MainDbConnection);
				if (timeSinceLastCleanup > TimeSpan.FromDays(3))
				{
					var msg = $"CDC Cleanup has failed to run for greater than 3 days.";
					ServiceLogger.Error(msg, ex);
					throw new Exception(msg, ex);
				}
				else
				{
					var msg = new StringBuilder();
					msg.AppendLine($"Lock request timeout. Retrying in the next run.");
					var e = ex;
					while (e != null)
					{
						msg.AppendLine($"Error Number: {e.Number}, Message: {e.Message}");
						e = e.InnerException as SqlException;
					}

					var hasQueue = (new CleanupTaskQueue() as IHostedServiceQueueProvider).QueueResult.QueueSize > 0;
					var severity = hasQueue ? LogType.Warning : LogType.Information;
					ServiceLogger.Log(severity, msg.ToString());
				}
			}
		}

		void HandleDbTimeoutException(string captureInstance, byte[] lowWaterMark, long initialUnprocessedRowCount, SqlException ex)
		{
			var currentUnprocessedRowCount = GetUnprocessedRowCount(captureInstance, lowWaterMark);
			if (currentUnprocessedRowCount > 0)
			{
				if (currentUnprocessedRowCount < initialUnprocessedRowCount)
				{
					ServiceLogger.Log(LogType.Warning,
						$"Connection timed out. Cleaning up capture instance [{captureInstance}] with triple timeout. {currentUnprocessedRowCount} unprocessed row{(currentUnprocessedRowCount > 1 ? $"s" : "")} remaining.");
					CmdTimeoutInSeconds = defaultCommandTimeoutInSeconds * 3;
				}
				else
				{
					ServiceLogger.Log(LogType.Warning, $"No progress made in cleaning unprocessed rows.");
					throw new CdcException($"Failed to clean up capture instance [{captureInstance}].", ex);
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Debug, $"No unprocessed rows left. Skip handling of timeout");
			}
		}

		protected virtual long GetUnprocessedRowCount(string captureInstance, byte[] waterMark)
		{
			var sqlText = $"select count(*) from [cdc].[{captureInstance}_CT] with (nolock) where __$start_lsn < @watermark";
			using (var cmd = MainDbConnection.Command(sqlText))
			{
				cmd.AddParameter($"@watermark", SqlDbType.Binary, waterMark);
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		List<string> GetCapturedInstancesToCleanup(byte[] lowWaterMark)
		{
			var sqlText = @"select capture_instance from cdc.change_tables where start_lsn < @lowWaterMark";

			using (var cmd = MainDbConnection.Command(sqlText))
			{
				cmd.AddParameter("@lowWaterMark", SqlDbType.Binary, lowWaterMark);

				List<string> capturedInstanceList = new List<string>();
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						capturedInstanceList.Add(reader.GetString(0));
					}
				}

				return capturedInstanceList;
			}
		}

		#region Get LSN values

		// Will be 0x00 if one of edw or audit is enabled but has not run yet, meaning nothing should be cleaned up as all changes are yet to be processed
		protected byte[] GetLsnWatermark(DbConnection auditConnection, DbConnection dwConnection)
		{
			var auditProcessedLsn = GetAuditProcessedLsn(auditConnection);
			var edwProcessedLsn = GetEdwProcessedLsn(dwConnection);

			if (auditProcessedLsn == null || edwProcessedLsn == null)
			{
				byte[] minProcessedLsn = auditProcessedLsn ?? edwProcessedLsn ?? GetMaxLsn();
				return GetLsnByTimeOffset(minProcessedLsn, CDCRetentionPeriod);
			}
			else
			{
				var minProcessedLsn = GetLowerLsn(auditProcessedLsn, edwProcessedLsn);
				return GetLsnByTimeOffset(minProcessedLsn, CDCRetentionPeriod);
			}
		}

		byte[] GetLowerLsn(byte[] auditProcessedLsn, byte[] edwProcessedLsn)
		{
			if (auditProcessedLsn.Length != edwProcessedLsn.Length)
			{
				throw new ArgumentException("Audit LSN and EDW LSN lengths do not match.");
			}

			var lowerLsn = auditProcessedLsn;

			for (var i = 0; i < auditProcessedLsn.Length; i++)
			{
				if (auditProcessedLsn[i] < edwProcessedLsn[i])
				{
					lowerLsn = auditProcessedLsn;
					break;
				}
				else if (auditProcessedLsn[i] > edwProcessedLsn[i])
				{
					lowerLsn = edwProcessedLsn;
					break;
				}
			}

			return lowerLsn;
		}

		protected virtual byte[] GetLsnByTimeOffset(byte[] minLSN, TimeSpan timespan)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT ISNULL(
					{0}.sys.fn_cdc_map_time_to_lsn(
						'smallest greater than or equal',
						DATEADD(hh, @Hours, {0}.sys.fn_cdc_map_lsn_to_time(@MinLsn))
					),
					0x00000000000000000000
				) AS LSN",
				Db.DatabaseName); // sql query

			using (var cmd = MainDbConnection.Command(sqlText))
			{
				cmd.AddParameter("@Hours", SqlDbType.Int, -timespan.TotalHours);
				cmd.AddParameter("@MinLsn", SqlDbType.Binary, 10, (object)minLSN ?? DBNull.Value);
				return (byte[])cmd.ExecuteScalar();
			}
		}

		protected virtual byte[] GetAuditProcessedLsn(DbConnection auditConnection)
		{
			if (!BiDatabaseExistsAndUpdated(auditConnection, Db.AuditDatabaseName))
			{
				return null;
			}

			using (((ICurrentDbControl)auditConnection).UseDatabase(Db.AuditDatabaseName))
			{
				return BiMasterState.GetParameterAsByteArray(auditConnection, BiConstants.LastMaxLsnProcessed);
			}
		}

		protected virtual byte[] GetEdwProcessedLsn(DbConnection dwConnection)
		{
			if (!BiDatabaseExistsAndUpdated(dwConnection, Db.EdwDatabaseName))
			{
				return null;
			}

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT MIN(CurrentMaxLsn) FROM [{0}].[{1}].StagingTableState",
				Db.EdwDatabaseName,
				BiConstants.BiAdminSchemaName);
			return GetBinaryValueOrZeroIfNull(dwConnection, sqlText);
		}

		bool BiDatabaseExistsAndUpdated(DbConnection biConnection, string dbName)
		{
			var result = false;

			if (biConnection == null)
			{
				return false;
			}

			if (biConnection.DatabaseExists(dbName))
			{
				var biDbVersion = BiMasterState.GetBiDatabaseExtPty(biConnection, dbName, BiConstants.MainDbSchemaVersionExtPtyName);
				result = SchemaVersion.Application.ToString() == biDbVersion;
			}

			return result;
		}

		byte[] GetBinaryValueOrZeroIfNull(DbConnection connection, string getLsnQuery)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT ISNULL(({0}), 0x00000000000000000000)", // This is an SQL expression
				getLsnQuery);
			return (byte[])connection.ExecuteScalar(sqlText);
		}

		public TimeSpan TimeSinceLastCleanup(DbConnection mainDbConnection)
		{
			var sqlText = "SELECT ISNULL(DATEDIFF(ss, (select top 1 tran_end_time from cdc.lsn_time_mapping order by tran_end_time), GETDATE()), 0);";
			var differenceSeconds = mainDbConnection.ExecuteScalar<int>(sqlText);
			return TimeSpan.FromSeconds(differenceSeconds);
		}

		#endregion

		protected virtual bool ExecuteCleanupProcedure(string captureInstance, byte[] lowWaterMark, int deleteThreshold)
		{
			try
			{
				return ExecuteCleanupProcedure_Unsafe(captureInstance, lowWaterMark, deleteThreshold, withCleanupFailedParameter: true);
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.IncorrectNumberOfParametersForProcedure)
			{
				return ExecuteCleanupProcedure_Unsafe(captureInstance, lowWaterMark, deleteThreshold, withCleanupFailedParameter: false);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Parameter value")]
		protected virtual bool ExecuteCleanupProcedure_Unsafe(string captureInstance, byte[] lowWaterMark, int deleteThreshold, bool withCleanupFailedParameter)
		{
			using (var cmd = MainDbConnection.Command("dbo.CdcCleanup", CmdTimeoutInSeconds))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@capture_instance", SqlDbType.NVarChar, -1, captureInstance);
				cmd.AddParameter("@low_water_mark", SqlDbType.Binary, lowWaterMark);
				cmd.AddParameter("@threshold", SqlDbType.Int, deleteThreshold);
				cmd.AddOutputParameter("@retVal", SqlDbType.Int, 32, 0, 0, null);
				if (withCleanupFailedParameter)
				{
					cmd.AddOutputParameter("@cleanup_failed", SqlDbType.Bit, 32, 0, 0, 0);
				}

				cmd.ExecuteNonQuery();

				var result = Convert.ToInt32(cmd.GetParameterValue("@retVal"));

				if (withCleanupFailedParameter)
				{
					var cleanupFailedRaw = cmd.GetParameterValue("@cleanup_failed");
					var cleanupFailed = Convert.ToBoolean(cleanupFailedRaw == DBNull.Value ? false : cleanupFailedRaw);
					if (cleanupFailed)
					{
						throw new CdcCleanupProcedureFailedException(captureInstance, lowWaterMark, deleteThreshold, result);
					}
				}

				return result == 0;
			}
		}

		protected const int defaultDeleteThreshold = 4999;
		internal virtual int defaultCommandTimeoutInSeconds => 1800;

		int? cmdTimeoutInSeconds;
		public virtual int CmdTimeoutInSeconds
		{
			get
			{
				return cmdTimeoutInSeconds ??= defaultCommandTimeoutInSeconds;
			}
			private set
			{
				cmdTimeoutInSeconds = value;
			}
		}
	}
}
