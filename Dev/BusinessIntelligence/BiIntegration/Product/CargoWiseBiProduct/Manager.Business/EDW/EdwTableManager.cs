using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public static class EdwTableManager
	{
		public static void RunInitialLoad(IEnumerable<ZString> stagingTables, IEnumerable<ZString> baseTables, IEnumerable<ZString> aggregateTables, IEnumerable<ZString> customTables)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
				if (!string.IsNullOrEmpty(dwServer))
				{
					using (var biConnection = Db.NewExtraUnrestrictedWriterConnection(dwServer, Db.EdwDatabaseName))
					{
						ClearMasterState(biConnection);

						var runForAllTables = !stagingTables.Any() && !baseTables.Any() && !aggregateTables.Any() && !customTables.Any();

						if (runForAllTables || stagingTables.Any())
						{
							RunInitialLoadForStagingTables(biConnection, stagingTables);
						}
						if (runForAllTables || baseTables.Any())
						{
							RunInitialLoadForBaseTables(biConnection, baseTables);
						}
						if (runForAllTables || aggregateTables.Any())
						{
							RunInitialLoadForAggregateTables(biConnection, aggregateTables);
						}
						if (runForAllTables || customTables.Any())
						{
							RunInitialLoadForCustomTables(biConnection, customTables);
						}
					}
				}
			}
		}

		public static void UpdateTranslationTable()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
				if (!string.IsNullOrEmpty(dwServer))
				{
					using (var biConnection = Db.NewExtraUnrestrictedWriterConnection(dwServer, Db.EdwDatabaseName))
					{
						BiMasterState.SetParameter(biConnection, BiConstants.UpdateTranslationTableFlag, "1");
					}
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		static void ClearMasterState(DbConnection biConnection)
		{
			biConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}].[MasterState]", BiConstants.BiAdminSchemaName));
		}

		static void RunInitialLoadForStagingTables(DbConnection biConnection, IEnumerable<ZString> stagingTables)
		{
			var whereCondition = stagingTables.Any() ? string.Format(CultureInfo.InvariantCulture, @"WHERE SourceTableName IN ('{0}')", string.Join("', '", stagingTables.Select(t => "dbo." + t))) : "";
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].StagingTableState
	SET InitialLoadRequired = 1,
			CurrentState = 'New',
			CurrentMaxLsn = 0x0,
			StateModifiedTimestamp = GETDATE(),
			InitialLoadRecordCount = 0,
			InitialLoadDurationMs = 0,
			IncrementalLoadRecordCount = 0,
			IncrementalLoadDurationMs = 0,
			SqlErrorMessage = NULL
{1}", BiConstants.BiAdminSchemaName, whereCondition);

			biConnection.ExecuteNonQuery(sqlText);
		}

		static void RunInitialLoadForBaseTables(DbConnection biConnection, IEnumerable<ZString> baseTables)
		{
			var whereCondition = baseTables.Any() ? string.Format(CultureInfo.InvariantCulture, @"WHERE ModelTableName IN ('{0}')", string.Join("', '", baseTables)) : "";
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].TransformTableState
	SET InitialLoadRequired = 1,
			CurrentState = 'New',
			StateModifiedTimestamp = GETDATE(),
			InitialTransformRecordCount = 0,
			InitialTransformDurationMs = 0,
			InitialTransformTimeStamp = NULL,
			MergeTransformInsertRecordCount = 0,
			MergeTransformInsertDurationMs = 0,
			MergeTransformDeleteRecordCount = 0,
			MergeTransformDeleteDurationMs = 0,
			MergeTransformTimestamp = NULL,
			IndexReorganizeDurationMs = 0,
			IsIndexReorganized = NULL,
			SqlErrorMessage = NULL,
			IndexReorganizationSqlErrorMessage = NULL
{1}", BiConstants.BiAdminSchemaName, whereCondition);

			biConnection.ExecuteNonQuery(sqlText);
		}

		static void RunInitialLoadForAggregateTables(DbConnection biConnection, IEnumerable<ZString> aggregateTables)
		{
			var whereCondition = aggregateTables.Any() ? string.Format(CultureInfo.InvariantCulture, @"WHERE ModelTableName IN ('{0}')", string.Join("', '", aggregateTables)) : "";
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].ModelTableState
	SET InitialLoadRequired = 1,
			CurrentState = 'New',
			StateModifiedTimestamp = GETDATE(),
			InitialLoadRecordCount = 0,
			InitialLoadDurationMs = 0,
			IncrementalInsertRecordCount = 0,
			IncrementalInsertDurationMs = 0,
			IncrementalDeleteRecordCount = 0,
			IncrementalDeleteDurationMs = 0,
			IndexReorganizeDurationMs = 0,
			IsIndexReorganized = NULL,
			SqlErrorMessage = NULL,
			IndexReorganizationSqlErrorMessage = NULL
{1}", BiConstants.BiAdminSchemaName, whereCondition);

			biConnection.ExecuteNonQuery(sqlText);
		}

		static void RunInitialLoadForCustomTables(DbConnection biConnection, IEnumerable<ZString> customTables)
		{
			var whereCondition = customTables.Any() ? string.Format(CultureInfo.InvariantCulture, @"WHERE ModelTableName IN ('{0}')", string.Join("', '", customTables)) : "";
			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].CustomTableState
	SET InitialLoadRequired = 1,
			CurrentState = 'New',
			StateModifiedTimestamp = GETDATE(),
			InitialTransformRecordCount = 0,
			InitialTransformDurationMs = 0,
			InitialTransformTimeStamp = NULL,
			MergeTransformInsertRecordCount = 0,
			MergeTransformInsertDurationMs = 0,
			MergeTransformDeleteRecordCount = 0,
			MergeTransformDeleteDurationMs = 0,
			MergeTransformTimestamp = NULL,
			IndexReorganizeDurationMs = 0,
			IsIndexReorganized = NULL,
			SqlErrorMessage = NULL,
			IndexReorganizationSqlErrorMessage = NULL
{1}", BiConstants.BiAdminSchemaName, whereCondition);

			biConnection.ExecuteNonQuery(sqlText);
		}

		#endregion
	}
}
