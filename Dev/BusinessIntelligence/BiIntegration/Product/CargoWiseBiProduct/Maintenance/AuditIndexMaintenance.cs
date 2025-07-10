namespace CargoWise.Bi.Maintenance
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Bi.Common;
	using CargoWise.Data;
	using CargoWise.Types;
	using Enterprise.Integration;

	public class AuditIndexMaintenance : IndexMaintenance
	{
		public static AuditIndexMaintenance New(DbConnection biConnection, ILogger logger)
		{
			return new AuditIndexMaintenance(biConnection, logger);
		}

		protected AuditIndexMaintenance(DbConnection biConnection, ILogger logger)
			: base(biConnection, logger)
		{
		}

		protected override string BiDatabaseName
		{
			get
			{
				return Db.AuditDatabaseName;
			}
		}

		protected override IndexMaintenanceResultCode OrganizeIndex()
		{
			var errorCode = IndexErrorCode.NoIndexesReorganized;
			var isIndexRebuilt = IndexRebuildResultCode.NoIndexCorruption;
			var cdcHistorySummaryErrorCode = CdcHistorySummaryErrorCode.NoAttempt;

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				@"[{0}].[usp_IndexMaintenance]", // This is an SQL query
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			using (var cmd = biConnection.Command(sqlText, CommandTimeout))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.AddParameter("@timer_in_seconds", SqlDbType.Int, DBNull.Value);
				cmd.AddParameter("@print_messages", SqlDbType.Int, 0);

				cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, null);
				cmd.AddOutputParameter("@IsIndexRebuilt", SqlDbType.Int, 0, 0, 0, null);
				cmd.AddOutputParameter("@CdcHistorySummaryErrorCode", SqlDbType.Int, 0, 0, 0, null);

				cmd.ExecuteNonQuery();

				errorCode = (IndexErrorCode)cmd.GetParameterValue("@ErrorCode");
				isIndexRebuilt = (IndexRebuildResultCode)cmd.GetParameterValue("@IsIndexRebuilt");
				cdcHistorySummaryErrorCode = (CdcHistorySummaryErrorCode)cmd.GetParameterValue("@CdcHistorySummaryErrorCode");
			}
			return new IndexMaintenanceResultCode(errorCode, isIndexRebuilt, cdcHistorySummaryErrorCode);
		}

		public override bool HasOnlyDeadlockError()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
	@"SELECT DISTINCT IndexReorganizationSqlErrorNumber
	FROM [{0}].[TableState]
	WHERE IndexReorganizationSqlErrorNumber IS NOT NULL", BiConstants.BiAdminSchemaName);

				var errorNumbers = DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);
				return errorNumbers.Count() == 1 && errorNumbers.First() == "1205";
			}
		}

		public override ZDateTime GetLastIndexRebuildUtcDate()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				return BiMasterState.GetParameterDate(biConnection, BiConstants.LastIndexRebuildUtcDt) ?? ZDateTime.Empty;
			}
		}

		public override IEnumerable<string> GetErrorList()
		{
			var result = new List<string>();

			using var cmd = biConnection.Command($"[{BiConstants.BiAdminSchemaName}].usp_GetIndexReorganizationErrors");
			cmd.CommandType = CommandType.StoredProcedure;

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = reader.GetString(0) + "." + reader.GetString(1);
					var errorMsg = reader.GetString(2);

					result.Add(string.Format(CultureInfo.InvariantCulture, "[{0}] - {1}", tableName, errorMsg));
				}
			}
			return result;
		}

		public override IEnumerable<string> GetReorganizedTableIndexList()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"SELECT SourceSchemaName + '.' + SourceTableName
FROM [{0}].[TableState]
WHERE IsIndexReorganized = 1",  // SQL query
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				return DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);
			}
		}

		public override IEnumerable<string> GetRebuiltTableIndexList()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"SELECT SourceSchemaName + '.' + SourceTableName
FROM [{0}].[TableState]
WHERE IndexReorganizationSqlErrorMessage = 'Index corruption is detected, index has been rebuilt'",  // SQL query
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				return DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log message")]
		protected override void LogCdcHistorySummaryError(IndexMaintenanceResultCode result)
		{
			switch (result.CdcHistorySummaryErrorCode)
			{
				case CdcHistorySummaryErrorCode.NoAttempt:
					break;
				case CdcHistorySummaryErrorCode.NoIndexCorruption:
					Log(LogType.Debug, "CDC History Summary index has been reorganized.");
					break;
				case CdcHistorySummaryErrorCode.IndexRebuilt:
					Log(LogType.Warning, "CDC History Summary index was corrupted, but is now rebuilt.");
					break;
				case CdcHistorySummaryErrorCode.IndexNotRebuilt:
					Log(LogType.Warning, "CDC History Summary index is corrupted.");
					break;
				case CdcHistorySummaryErrorCode.SqlError:
					var errorNumber = BiMasterState.GetParameter(biConnection, BiConstants.CdcHistorySummaryErrorNumberParamName);
					var errorMessage = BiMasterState.GetParameter(biConnection, BiConstants.CdcHistorySummaryErrorMessageParamName);
					Log(LogType.Warning, $"CDC History Summary index reorganization failed.\r\nError number: {errorNumber}\r\nError message: {errorMessage}");
					break;
				default:
					throw new BiMaintenanceException(string.Format(CultureInfo.InvariantCulture, "Unknown CDC History Summary error code ({0}) returned.", result.ErrorCode));
			}
		}
	}
}
