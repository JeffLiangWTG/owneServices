using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace CargoWise.Bi.Maintenance
{
	#region DropOldAuditColumnsResult

	public enum DropOldAuditColumnsResultCode
	{
		Success = 0,
		UnexpectedFailure = 1
	}

	public class DropOldAuditColumnsResult
	{
		public DropOldAuditColumnsResult(DropOldAuditColumnsResultCode errorCode, string infoMessage)
		{
			ErrorCode = errorCode;
			InfoMessage = infoMessage;
		}
		public readonly DropOldAuditColumnsResultCode ErrorCode;
		public readonly string InfoMessage;
	}

	#endregion

	public class DropOldAuditColumnsScriptRunner
	{
		public DropOldAuditColumnsScriptRunner(DbConnection biConnection, ILogger logger)
		{
			this.biConnection = biConnection;
			this.logger = logger;
		}
		readonly DbConnection biConnection;
		readonly ILogger logger;

		public void Run()
		{
			var result = RunDropOldColumnsScriptUnsafe();
			if (result.ErrorCode == DropOldAuditColumnsResultCode.Success)
			{
				logger.Log(LogType.Debug, result.InfoMessage);
			}
			else
			{
				throw new BiMaintenanceException($"Failed to drop legacy columns in Audit Database");
			}
		}

		DropOldAuditColumnsResult RunDropOldColumnsScriptUnsafe()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var auditRetentionPeriod = SystemDataRegistry.Instance.AuditRetentionPeriod.Value;

				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"[{0}].[{1}].[usp_DropOldAuditColumns]",
					Db.AuditDatabaseName,
					BiConstants.BiAdminSchemaName);

				using (var cmd = biConnection.Command(sqlText, 1000))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@AuditRetentionPeriod", SqlDbType.Int, auditRetentionPeriod);
					cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, 1);
					cmd.AddOutputParameter("@InfoMessage", SqlDbType.VarChar, -1, 0, 0, "");

					cmd.ExecuteNonQuery();

					var errorCode = (DropOldAuditColumnsResultCode)cmd.GetParameterValue("@ErrorCode");
					var infoMessage = cmd.GetParameterValue("@InfoMessage").ToString();

					return new DropOldAuditColumnsResult(errorCode, infoMessage);
				}
			}
		}
	}
}
