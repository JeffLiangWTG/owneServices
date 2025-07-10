using System;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration;

namespace CargoWise.Bi.Maintenance
{
	public class CdcHistorySummaryPopulator
	{
		#region SuppressResourceStringsCheckRegion

		public CdcHistorySummaryPopulator(DbConnection biConnection, ILogger logger)
		{
			this.biConnection = biConnection;
			this.logger = logger;
		}
		readonly DbConnection biConnection;
		readonly ILogger logger;

		void Log(LogType type, string message)
		{
			logger?.Log(type, message);
		}

		void LogError(CdcHistorySummaryResult result)
		{
			Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Populating CDC History Summary failed with the error:\r\nError Code: {0}\r\nMessage: {1}", result.ErrorNumber, result.ErrorMessage));
		}

		public void Run()
		{
			PopulateCdcHistorySummary(true);
		}

		void PopulateCdcHistorySummary(bool retryIfItFails)
		{
			Log(LogType.Debug, "Populating CDC History Summary");
			var result = ExecutePopulateCdcHistorySummaryScript();
			switch (result.ErrorCode)
			{
				case CdcHistorySummaryResultCode.NoProcessingRequired:
					Log(LogType.Debug, "No transactions to process");
					break;
				case CdcHistorySummaryResultCode.Success:
					Log(LogType.Debug, "CDC History Summary successfully populated");
					break;
				case CdcHistorySummaryResultCode.SqlError:
					LogError(result);
					break;
				case CdcHistorySummaryResultCode.Timeout:
					if (retryIfItFails)
					{
						Log(LogType.Warning, "Retrying due to timeout");
						PopulateCdcHistorySummary(false);
					}
					else
					{
						LogError(result);
					}
					break;
				default:
					Log(LogType.Error, "Unknown CdcHistorySummaryResultCode: " + result.ErrorCode);
					break;
			}
		}

		CdcHistorySummaryResult ExecutePopulateCdcHistorySummaryScript()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"[{0}].[{1}].[usp_PopulateCdcHistorySummary]",
					Db.AuditDatabaseName,
					BiConstants.BiAdminSchemaName);

				using (var cmd = biConnection.Command(sqlText))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.AddOutputParameter("@error_code", SqlDbType.Int, 0, 0, 0, -1);
					cmd.AddOutputParameter("@error_number", SqlDbType.Int, 0, 0, 0, "");
					cmd.AddOutputParameter("@error_message", SqlDbType.VarChar, -1, 0, 0, "");

					cmd.ExecuteNonQuery();

					var errorCode = (CdcHistorySummaryResultCode)cmd.GetParameterValue("@error_code");
					var errorNumber = Convert.ToInt32(cmd.GetParameterValue("@error_number"), CultureInfo.InvariantCulture);
					var errorMessage = cmd.GetParameterValue("@error_message").ToString();

					return new CdcHistorySummaryResult(errorCode, errorNumber, errorMessage);
				}
			}
		}

		#endregion

		class CdcHistorySummaryResult
		{
			public CdcHistorySummaryResult(CdcHistorySummaryResultCode errorCode, int errorNumber, string errorMessage)
			{
				ErrorCode = errorCode;
				ErrorNumber = errorNumber;
				ErrorMessage = errorMessage;
			}

			public readonly CdcHistorySummaryResultCode ErrorCode;
			public readonly int ErrorNumber;
			public readonly string ErrorMessage;
		}

		enum CdcHistorySummaryResultCode
		{
			NoProcessingRequired = 0,
			Success = 1,
			SqlError = 2,
			Timeout = 3,
			IncompleteRun
		}
	}
}
