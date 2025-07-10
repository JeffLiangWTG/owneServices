using System;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace CargoWise.Bi.Maintenance
{
	public class AuditPartitionScriptRunner
	{
		public AuditPartitionScriptRunner(DbConnection biConnection, ILogger logger)
		{
			this.biConnection = biConnection;
			this.logger = logger;
		}
		readonly DbConnection biConnection;
		readonly ILogger logger;

		protected virtual DateTime? GetCurrentTimeUTC()
		{
			return null;
		}

		public void Run()
		{
			logger.Log(LogType.Debug, "Partitioning Audit database.");
			var result = RunPartitioningScriptUnsafe();
			if (result.IsSuccess)
			{
				logger.Log(LogType.Debug, $"Audit database was successfully partitioned.\r\n{result.InfoMessage}");
			}
			else
			{
				throw new BiMaintenanceException($"Audit database partitioning failed.\r\n{result.InfoMessage}\r\n{result.GetErrorDescription()}\r\n{result.ErrorMessage}");
			}
		}

		AuditPartitionResult RunPartitioningScriptUnsafe()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var auditRetentionPeriod = SystemDataRegistry.Instance.AuditRetentionPeriod.Value;

				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"[{0}].[{1}].[usp_RecreatePartitionsAndPurgeOldData]",
					Db.AuditDatabaseName,
					BiConstants.BiAdminSchemaName);

				using (var cmd = biConnection.Command(sqlText, commandTimeout))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@DataRetentionMonth", SqlDbType.Int, auditRetentionPeriod);
					cmd.AddParameter("@PrintMessages", SqlDbType.Bit, 0);
					cmd.AddParameter("@CurrentTimeUTC", SqlDbType.DateTime, GetCurrentTimeUTC());

					cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, -1);
					cmd.AddOutputParameter("@InfoMessage", SqlDbType.VarChar, -1, 0, 0, "");
					cmd.AddOutputParameter("@ErrorMessage", SqlDbType.VarChar, -1, 0, 0, "");

					cmd.ExecuteNonQuery();

					var errorCode = (AuditPartitionResultCode)cmd.GetParameterValue("@ErrorCode");
					var infoMessage = cmd.GetParameterValue("@InfoMessage").ToString();
					var errorMessage = cmd.GetParameterValue("@ErrorMessage").ToString();

					return new AuditPartitionResult(errorCode, infoMessage, errorMessage);
				}
			}
		}

		#region Time Calculations

		protected int commandTimeout
		{
			get
			{
				return Convert.ToInt32(TimeSpan.FromHours(2).TotalSeconds);
			}
		}

		#endregion
	}
}
