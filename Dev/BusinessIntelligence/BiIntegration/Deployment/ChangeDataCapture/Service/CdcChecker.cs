using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;

namespace Enterprise.ChangeDataCapture.Service
{
	class CdcChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC cleanup job name")]
		public static bool CheckCdcCleanupJobRunningStatusAndEnforceRetentionPeriod(ILogger logger)
		{
			var cleanUpJobRunning = false;
			var retentionPeriod = cleanUpJobRetentionPeriod;
			var enabled = false;

			using (var connection = Db.NewAdminConnection())
			{
				using (var cmd = connection.Command("dbo.usp_CheckCdcCleanupJobs"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddOutputParameter("@retention_period", SqlDbType.Int, 32, 0, 0, null);
					cmd.AddOutputParameter("@is_cleanup_job_enabled", SqlDbType.Int, 32, 0, 0, null);
					cmd.AddOutputParameter("@is_cleanup_job_running", SqlDbType.Int, 32, 0, 0, null);
					cmd.ExecuteNonQuery();

					retentionPeriod = Convert.ToInt64(cmd.GetParameterValue("@retention_period"), CultureInfo.InvariantCulture);
					enabled = Convert.ToBoolean(cmd.GetParameterValue("@is_cleanup_job_enabled"), CultureInfo.InvariantCulture);
					cleanUpJobRunning = Convert.ToBoolean(cmd.GetParameterValue("@is_cleanup_job_running"), CultureInfo.InvariantCulture);

					if (enabled)
					{
						logger.Log(LogType.Debug, "Disabling CDC cleanup job.");
						var cleanupJobName = "cleanup";
						CdcDatabase.DisableCdcJobs(connection, Db.DatabaseName, cleanupJobName);
						CdcDatabase.StopCdcJobs(connection, Db.DatabaseName, cleanupJobName);
					}

					if (retentionPeriod != cleanUpJobRetentionPeriod)
					{
						logger.Log(LogType.Debug, "Setting CDC data retention period to 30 days.");
						SetCdcCleanupJobRetentionPeriod(connection);
					}
				}
			}

			if (cleanUpJobRunning)
			{
				logger.Log(LogType.Warning, "CDC cleanup job is running. Skipping CDC scan to avoid potential data loss.");
			}

			return cleanUpJobRunning;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Parameter")]
		static void SetCdcCleanupJobRetentionPeriod(AdminConnection connection)
		{
			if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				using (var cmd = connection.Command("sp_cdc_change_job"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@job_type", SqlDbType.NVarChar, 20, cleanUpJobType);
					cmd.AddParameter("@retention", SqlDbType.BigInt, cleanUpJobRetentionPeriod);
					cmd.ExecuteNonQuery();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC cleanup job name")]
		const string cleanUpJobType = "cleanup";
		const Int64 cleanUpJobRetentionPeriod = 43200; // 30 days
	}
}
