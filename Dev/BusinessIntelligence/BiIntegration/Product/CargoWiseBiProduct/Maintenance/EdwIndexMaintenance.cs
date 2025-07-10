namespace CargoWise.Bi.Maintenance
{
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Bi.Common;
	using CargoWise.Data;
	using CargoWise.Types;
	using Enterprise.Integration;

	public class EdwIndexMaintenance : IndexMaintenance
	{
		public static EdwIndexMaintenance New(DbConnection biConnection, ILogger logger)
		{
			return new EdwIndexMaintenance(biConnection, logger);
		}

		protected EdwIndexMaintenance(DbConnection biConnection, ILogger logger)
			: base(biConnection, logger)
		{
		}

		protected override string BiDatabaseName
		{
			get
			{
				return Db.EdwDatabaseName;
			}
		}

		protected override IndexMaintenanceResultCode OrganizeIndex()
		{
			var errorCode = IndexErrorCode.NoIndexesReorganized;
			var isIndexRebuilt = IndexRebuildResultCode.NoIndexCorruption;

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				@"EXEC [{0}].[usp_IndexMaintenance]
					@timer_in_seconds = NULL,
					@print_messages = 0,
					@ErrorCode = @ErrorCode OUTPUT,
					@IsIndexRebuilt = @IsIndexRebuilt OUTPUT", // This is an SQL query
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			using (var cmd = biConnection.Command(sqlText, CommandTimeout))
			{
				cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, null);
				cmd.AddOutputParameter("@IsIndexRebuilt", SqlDbType.Int, 0, 0, 0, null);

				cmd.ExecuteNonQuery();

				errorCode = (IndexErrorCode)cmd.GetParameterValue("@ErrorCode");
				isIndexRebuilt = (IndexRebuildResultCode)cmd.GetParameterValue("@IsIndexRebuilt");
			}
			return new IndexMaintenanceResultCode(errorCode, isIndexRebuilt);
		}

		public override bool HasOnlyDeadlockError()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
@"SELECT DISTINCT IndexReorganizationSqlErrorNumber
FROM [{0}].[TransformTableState]
WHERE IndexReorganizationSqlErrorNumber IS NOT NULL

UNION

SELECT DISTINCT IndexReorganizationSqlErrorNumber
FROM [{0}].[ModelTableState]
WHERE IndexReorganizationSqlErrorNumber IS NOT NULL

UNION

SELECT DISTINCT IndexReorganizationSqlErrorNumber
FROM [{0}].[CustomTableState]
WHERE IndexReorganizationSqlErrorNumber IS NOT NULL", BiConstants.BiAdminSchemaName);

				var errorNumbers = DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);
				return errorNumbers.Distinct().Count() == 1 && errorNumbers.First() == "1205";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Date format")]
		public override ZDateTime GetLastIndexRebuildUtcDate()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
@"DECLARE @paramValue nvarchar(max)
EXEC[{0}].usp_GetMasterStateParameter @ParamName = 'LAST_INDEX_REBUILD_UTC_DT', @ParamValue = @paramValue OUTPUT
SELECT ISNULL(@paramValue, '')", BiConstants.BiAdminSchemaName);

				var dateObj = biConnection.ExecuteScalar(sqlText).ToString();

				ZDateTime result;
				if (ZDateTime.TryParseExact(dateObj, out result, "yyyy-MM-dd hh:mm:ss.fff"))
				{
					return result;
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		public override IEnumerable<string> GetErrorList()
		{
			var result = new List<string>();
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT ModelSchemaName, ModelTableName, IndexReorganizationSqlErrorMessage
FROM [{0}].[TransformTableState]
WHERE IndexReorganizationSqlErrorMessage <> ''

UNION

SELECT  ModelSchemaName, ModelTableName, IndexReorganizationSqlErrorMessage
FROM [{0}].[ModelTableState]
WHERE IndexReorganizationSqlErrorMessage <> ''

UNION

SELECT  ModelSchemaName, ModelTableName, IndexReorganizationSqlErrorMessage
FROM [{0}].[CustomTableState]
WHERE IndexReorganizationSqlErrorMessage <> ''", BiConstants.BiAdminSchemaName); // SQL query

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			using (var cmd = biConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var schemaName = reader.GetString(0);
					var tableName = reader.GetString(1);
					var errorMsg = reader.GetString(2);

					result.Add(string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}] - {2}", schemaName, tableName, errorMsg));
				}
			}
			return result;
		}

		public override IEnumerable<string> GetReorganizedTableIndexList()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"SELECT ModelTableName
FROM [{0}].[TransformTableState]
WHERE IsIndexReorganized = 1

UNION

SELECT ModelTableName
FROM [{0}].[ModelTableState]
WHERE IsIndexReorganized = 1

UNION

SELECT ModelTableName
FROM [{0}].[CustomTableState]
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
@"SELECT ModelTableName
FROM [{0}].[TransformTableState]
WHERE IndexReorganizationSqlErrorMessage = 'Index corruption is detected, index has been rebuilt'

UNION

SELECT ModelTableName
FROM [{0}].[ModelTableState]
WHERE IndexReorganizationSqlErrorMessage = 'Index corruption is detected, index has been rebuilt'

UNION

SELECT ModelTableName
FROM [{0}].[CustomTableState]
WHERE IndexReorganizationSqlErrorMessage = 'Index corruption is detected, index has been rebuilt'",  // SQL query
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)biConnection).UseDatabase(BiDatabaseName))
			{
				return DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);
			}
		}
	}
}
