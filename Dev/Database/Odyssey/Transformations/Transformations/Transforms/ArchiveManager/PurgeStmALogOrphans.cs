using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	class PurgeStmALogOrphans : DataTransformation
	{
		public override string UserDescription => "Purge orphan records from dbo.StmALog";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedDate = (DateTime?)null;

			if (!string.IsNullOrEmpty(ExtProperty.Table.Select(Db.Connection, "dbo", nameof(PurgeStmALogOrphans), "LastProcessedDate")))
			{
				_ = DateTime.TryParse(ExtProperty.Table.Select(Db.Connection, "dbo", nameof(PurgeStmALogOrphans), "LastProcessedDate"), out var retrievedLastProcessedDate);
				lastProcessedDate = retrievedLastProcessedDate;
			}

			var tablesAndPkColumns = CreateTempTables();

			var stopwatch = Stopwatch.StartNew();

			foreach (var chunk in DateTimeChunker.GenerateChunks(1000, lastProcessedDate, StmALogSchema.SL_PostedTimeUtc))
			{
				PurgeDELEventsInCurrentBatch(chunk.LowerBound, chunk.UpperBound);
				PurgeRowsWithInvalidParentObjects(chunk.LowerBound, chunk.UpperBound);
				PurgeRemainingOrphansInBatch(chunk.LowerBound, chunk.UpperBound, tablesAndPkColumns);

				if (stopwatch.Elapsed.TotalMinutes > 1 || token.IsCancellationRequested)
				{
					manager?.ShowInfoMessage($"Processed orphan StmALog records until SL_PostedTimeUtc {lastProcessedDate}");
					ExtProperty.Table.Update(Db.Connection, "dbo", nameof(PurgeStmALogOrphans), "LastProcessedDate", SqlFormatInfo.ToSqlDateTimeString(chunk.UpperBound));

					token.ThrowIfCancellationRequested();
					stopwatch.Restart();
				}
			}

			ExtProperty.Table.Delete(Db.Connection, "dbo", nameof(PurgeStmALogOrphans), "LastProcessedDate");
		}

		DataTable CreateTempTables()
		{
			var setupAllTempTables = @"
				DROP TABLE IF EXISTS #ParentObjectsAndTheirPkColumns;			
				CREATE TABLE #ParentObjectsAndTheirPkColumns (
					TableName VARCHAR(35) COLLATE DATABASE_DEFAULT, 
					ColumnName NVARCHAR(128) COLLATE DATABASE_DEFAULT,
					SchemaName NVARCHAR(128),
					isValid int
				);
				
				INSERT INTO #ParentObjectsAndTheirPkColumns (TableName, ColumnName, SchemaName, isValid)
				SELECT
								T.Name as TableName, C.name as ColumnName, S.NAME AS SchemaName, 1 AS isValid
				FROM
					SYS.TABLES T
					INNER JOIN SYS.INDEXES I ON I.object_id = T.object_id
					INNER JOIN SYS.INDEX_COLUMNS IC ON IC.object_id = T.object_id AND IC.index_id = I.index_id
					INNER JOIN SYS.COLUMNS C ON C.object_id = T.object_id AND C.column_id = IC.column_id
					JOIN SYS.SCHEMAS S ON T.SCHEMA_ID = S.SCHEMA_ID
				WHERE
					T.is_ms_shipped = 0
					AND I.type != 0
					AND I.is_primary_key = 1
					AND IC.column_id = 1
					AND S.NAME <> 'CDC'
					AND LEN(T.NAME) <= 35
				UNION ALL
				SELECT V.NAME AS TableName, C.NAME AS ColumnName, S.NAME AS SchemaName, 1 AS isValid
					FROM SYS.COLUMNS C
					JOIN SYS.VIEWS V ON C.OBJECT_ID = V.OBJECT_ID 
					JOIN SYS.SCHEMAS S ON V.SCHEMA_ID = S.SCHEMA_ID
					WHERE (C.NAME LIKE '%[_]PK')
					AND (C.COLUMN_ID = 1)
					AND LEN(V.NAME) <= 35
					AND (V.NAME NOT LIKE 'vw[_]r%');
				
				INSERT INTO #ParentObjectsAndTheirPkColumns (TableName, ColumnName, SchemaName, isValid)
				SELECT DISTINCT T.NAME AS TableName, C.NAME AS ColumnName, S.NAME AS SchemaName, 0 AS isValid
					FROM SYS.COLUMNS C 
					JOIN SYS.TABLES T ON C.OBJECT_ID = T.OBJECT_ID 
					JOIN SYS.SCHEMAS S ON T.SCHEMA_ID = S.SCHEMA_ID
					WHERE (T.Name NOT IN (select tablename from #ParentObjectsAndTheirPkColumns) )
					AND (C.COLUMN_ID = 1)
					AND (S.NAME <> 'CDC')
					AND LEN(T.NAME) <= 35
				UNION
				SELECT DISTINCT V.NAME AS TableName, C.NAME AS ColumnName, S.NAME AS SchemaName, 0 AS isValid
					FROM SYS.COLUMNS C 
					JOIN SYS.VIEWS V ON C.OBJECT_ID = V.OBJECT_ID 
					JOIN SYS.SCHEMAS S ON V.SCHEMA_ID = S.SCHEMA_ID
					WHERE (V.Name NOT IN (select tablename from #ParentObjectsAndTheirPkColumns) )
					AND (C.COLUMN_ID = 1)
					AND (S.NAME <> 'CDC')
					AND LEN(V.NAME) <= 35;

				DROP TABLE IF EXISTS #OrphansToBeDeleted;
				CREATE TABLE #OrphansToBeDeleted (
					Orphan_PK uniqueidentifier
				);
			";

			_ = Db.Connection.ExecuteNonQuery(setupAllTempTables);

			var allTablesAndPkColumns = new DataTable();

			using (var command = Db.Connection.Command("SELECT * FROM #ParentObjectsAndTheirPkColumns"))
			{
				using (var adapter = command.NewDataAdapter())
				{
					adapter.Fill(allTablesAndPkColumns);
				}
			}

			return allTablesAndPkColumns;
		}

		int GetBatchSize(DateTime lowerBound, DateTime upperBound)
		{
			var getBatchSizeCommand = @"
				SELECT Count(*) FROM dbo.StmALog
				WHERE (SL_PostedTimeUtc BETWEEN @lowerBound AND @upperBound);
			";

			using (var command = Db.Connection.Command(getBatchSizeCommand))
			{
				command.AddParameter("@lowerBound", SqlDbType.DateTime, lowerBound);
				command.AddParameter("@upperBound", SqlDbType.DateTime, upperBound);

				return (int)command.ExecuteScalar();
			}
		}

		int GetNumberOfRowsCurrentlyChecking(string parentTable, DateTime lowerBound, DateTime upperBound)
		{
			var getBatchSizeCommand = @"
				SELECT Count(*) FROM dbo.StmALog
				WHERE SL_Table = @parentTable
				AND (SL_PostedTimeUtc BETWEEN @lowerBound AND @upperBound);
			";

			using (var command = Db.Connection.Command(getBatchSizeCommand))
			{
				command.AddParameter("@parentTable", SqlDbType.VarChar, parentTable);
				command.AddParameter("@lowerBound", SqlDbType.DateTime, lowerBound);
				command.AddParameter("@upperBound", SqlDbType.DateTime, upperBound);

				return (int)command.ExecuteScalar();
			}
		}

		void PurgeDELEventsInCurrentBatch(DateTime lowerBound, DateTime upperBound)
		{
			var purgeOrphanDelEventsAndTheirRelativesWithinBatch = @"
				WITH ParentIDsToDelete AS (
					SELECT DISTINCT SL_Parent FROM dbo.StmALog 
					WHERE (SL_PostedTimeUtc BETWEEN @lowerBound AND @upperBound)
					AND (SL_SE_NKEvent = 'DEL')
					AND (SL_Table <> 'GlbCompany'		OR SL_Reference NOT LIKE 'Signature Credential % is deleted%')														-- GlbCompanySignatureCredential
					AND (SL_Table <> 'JobDeclaration'	OR SL_Reference NOT LIKE 'Dec#: %')																					-- AU JobDeclaration
					AND (SL_Table <> 'OrgCompanyData'	OR SL_Reference NOT LIKE 'Tax Code: %; Start date %; End date %; Source %; Rate Numerator %; Rate Denominator %')	-- AccOrgTaxRate
					AND (SL_Table <> 'CusEntryHeader'	OR SL_Reference NOT LIKE 'SED Delete: %')																			-- AESCommodityShipmentProcessor
					AND (SL_Table <> 'OrgHeader'		OR SL_Reference NOT LIKE 'deleted license database server%')														-- ModuleButtonGridForLicencing
					AND (SL_Table <> 'OrgCusCode'		OR SL_Reference NOT LIKE 'Registration No.%')																		-- OrgCusCode
					AND (SL_Table <> 'JPAFRHeader'		OR SL_Reference NOT LIKE 'Parent % is deleted')
				)
				INSERT INTO #OrphansToBeDeleted
				SELECT SL_Parent FROM ParentIDsToDelete;
			";

			using (var command = Db.Connection.Command(purgeOrphanDelEventsAndTheirRelativesWithinBatch))
			{
				command.AddParameter("@lowerBound", SqlDbType.DateTime, lowerBound);
				command.AddParameter("@upperBound", SqlDbType.DateTime, upperBound);

				_ = command.ExecuteNonQuery();
			}

			PurgeStoredOrphans(lowerBound, upperBound, StmALogSchema.Constants.SL_Parent);
		}

		void PurgeRowsWithInvalidParentObjects(DateTime lowerBound, DateTime upperBound)
		{
			var purgeRecordsWithInvalidParentTable = @"
				DELETE TOP(100) RowsWithInvalidParent 
				FROM dbo.StmALog RowsWithInvalidParent
				LEFT JOIN #ParentObjectsAndTheirPkColumns Parent ON SL_Table = Parent.TableName
				WHERE (Parent.TableName IS NULL)
				AND (SL_PostedTimeUtc BETWEEN @lowerBound AND @upperBound);
			";

			var numberOfErasedRows = 0;

			do
			{
				using (var command = Db.Connection.Command(purgeRecordsWithInvalidParentTable))
				{
					command.AddParameter("@lowerBound", SqlDbType.DateTime, lowerBound);
					command.AddParameter("@upperBound", SqlDbType.DateTime, upperBound);

					numberOfErasedRows = command.ExecuteNonQuery();
				}
			} while (numberOfErasedRows == 100);
		}

		void PurgeRemainingOrphansInBatch(DateTime lowerBound, DateTime upperBound, DataTable tablesAndPkColumns)
		{
			int batchSize = GetBatchSize(lowerBound, upperBound);
			manager?.ShowInfoMessage($"Now attempting final purge stage on batch size: {batchSize}");
			manager?.ShowInfoMessage($"Lower bound is {lowerBound} and upper bound is {upperBound}");

			var batchOfParents = new DataTable();
			var getParentsAndPkColumnNames = @"
				SELECT DISTINCT SL_Table FROM dbo.StmALog
				WHERE (SL_PostedTimeUtc BETWEEN @lowerBound AND @upperBound);	
			";

			using (var command = Db.Connection.Command(getParentsAndPkColumnNames))
			{
				command.AddParameter("@lowerBound", SqlDbType.DateTime, lowerBound);
				command.AddParameter("@upperBound", SqlDbType.DateTime, upperBound);

				using (var adapter = command.NewDataAdapter())
				{
					adapter.Fill(batchOfParents);
				}
			}

			manager?.ShowInfoMessage($"Successfully got batch of parents");

			foreach (DataRow row in batchOfParents.Rows)
			{
				var nameOfParentTable = (string)row[0];

				if (string.Equals(StmALogSchema.Constants.TableName, nameOfParentTable, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				manager?.ShowInfoMessage($"Now processing {nameOfParentTable}");

				var allCorrespondingParentTables = RetrievePossibleDuplicateTableNames(nameOfParentTable, tablesAndPkColumns);

				foreach (DataRow correspondingParent in allCorrespondingParentTables.Rows)
				{
					var nameOfPkColumn = (string)correspondingParent[1];
					var nameOfSchema = (string)correspondingParent[2];
					var parentHasPkColumn = (int)correspondingParent[3] == 1;

					if (!parentHasPkColumn)
					{
						break;
					}

					if (allCorrespondingParentTables.Rows.Count == 1)
					{
						StoreOrphans(nameOfParentTable, nameOfPkColumn, nameOfSchema, lowerBound, upperBound);
						break;
					}

					var currentOrphans = GetCountOfOrphans(nameOfParentTable, nameOfPkColumn, nameOfSchema, lowerBound, upperBound);
					var totalNumberOfRowsBeingChecked = GetNumberOfRowsCurrentlyChecking(nameOfParentTable, lowerBound, upperBound);

					if (currentOrphans < totalNumberOfRowsBeingChecked)
					{
						StoreOrphans(nameOfParentTable, nameOfPkColumn, nameOfSchema, lowerBound, upperBound);
						break;
					}

					if (correspondingParent == allCorrespondingParentTables.AsEnumerable().Last())
					{
						StoreOrphans(nameOfParentTable, nameOfPkColumn, nameOfSchema, lowerBound, upperBound);
					}
				}
			}

			var numberOfOrphansFound = (int)Db.Connection.ExecuteScalar("SELECT COUNT(1) FROM #OrphansToBeDeleted");
			manager?.ShowInfoMessage($"Preparation steps executed successfully. Now attempting purge of {numberOfOrphansFound} detected StmALog Orphans.");

			PurgeStoredOrphans(lowerBound, upperBound, StmALogSchema.Constants.PK);
		}

		void PurgeStoredOrphans(DateTime lowerBound, DateTime upperBound, string columnToErase)
		{
			var deleteBatchOfPks = @$"
				DELETE TOP (100) FROM dbo.StmALog
				FROM dbo.StmALog WITH (FORCESEEK, INDEX([NR_RC__SL_PostedTimeUtc]))
				WHERE {columnToErase.QuoteName()} IN (SELECT TOP(100) Orphan_PK FROM #OrphansToBeDeleted)
				AND (SL_PostedTimeUtc BETWEEN @lowerBound AND @upperBound);

				DELETE TOP(100) FROM #OrphansToBeDeleted;";

			var deletedRecords = 0;

			do
			{
				using (var command = Db.Connection.Command(deleteBatchOfPks))
				{
					command.AddParameter("@lowerBound", SqlDbType.DateTime, lowerBound);
					command.AddParameter("@upperBound", SqlDbType.DateTime, upperBound);

					deletedRecords = command.ExecuteNonQuery();
				}
			} while (deletedRecords == 100);
		}

		DataTable RetrievePossibleDuplicateTableNames(string tableName, DataTable tablesAndPkColumns)
		{
			var allCorrespondingParentTables = tablesAndPkColumns.Clone();

			DataRow[] matches = tablesAndPkColumns.Select($"{"TableName"} = '{tableName}'");

			foreach (DataRow row in matches)
			{
				allCorrespondingParentTables.ImportRow(row);
			}

			return allCorrespondingParentTables;
		}

		int GetCountOfOrphans(string nameOfParentTable, string nameOfPkColumn, string nameOfSchema, DateTime lowerBound, DateTime upperBound)
		{
			var nameOfParentToSearch = nameOfParentTable;

			if (string.Equals(RefExchangeRateSchema.Constants.TableName, nameOfParentToSearch, StringComparison.OrdinalIgnoreCase))
			{
				nameOfParentToSearch = ZZRefExchangeRateSchema.Constants.TableName;
			}

			var orphanCommand = $@"
				DECLARE @top bigint = 9223372036854775807;

				SELECT COUNT(*)
				FROM (
					SELECT TOP(@top) 1 SL_PK FROM dbo.StmALog
					WHERE (SL_Table = {nameOfParentTable.QuoteName('\'')})
					AND (SL_PostedTimeUtc BETWEEN @lowerBound AND @upperBound)
					AND NOT EXISTS
					(
						SELECT NULL FROM {nameOfSchema.QuoteName()}.{nameOfParentToSearch.QuoteName()}
						WHERE SL_Parent = {nameOfPkColumn.QuoteName()}
					)
				) AS CountQuery

				OPTION (OPTIMIZE FOR(@top = 1));
			";

			using (var command = Db.Connection.Command(orphanCommand))
			{
				command.AddParameter("@lowerBound", SqlDbType.DateTime, lowerBound);
				command.AddParameter("@upperBound", SqlDbType.DateTime, upperBound);

				return (int)command.ExecuteScalar();
			}
		}

		void StoreOrphans(string nameOfParentTable, string nameOfPkColumn, string nameOfSchema, DateTime lowerBound, DateTime upperBound)
		{
			var nameOfParentToSearch = nameOfParentTable;

			if (string.Equals(RefExchangeRateSchema.Constants.TableName, nameOfParentToSearch, StringComparison.OrdinalIgnoreCase))
			{
				nameOfParentToSearch = ZZRefExchangeRateSchema.Constants.TableName;
			}

			var orphanCommand = $@"
				DECLARE @top bigint = 9223372036854775807;
				
				INSERT INTO #OrphansToBeDeleted
				SELECT TOP(@top) SL_PK FROM dbo.StmALog
				WHERE (SL_Table = {nameOfParentTable.QuoteName('\'')})
				AND (SL_PostedTimeUtc BETWEEN @lowerBound AND @upperBound)
				AND NOT EXISTS
				(
					SELECT NULL FROM {nameOfSchema.QuoteName()}.{nameOfParentToSearch.QuoteName()}
					WHERE SL_Parent = {nameOfPkColumn.QuoteName()}
				)

				OPTION (OPTIMIZE FOR(@top = 1));
			";

			using (var command = Db.Connection.Command(orphanCommand))
			{
				command.AddParameter("@lowerBound", SqlDbType.DateTime, lowerBound);
				command.AddParameter("@upperBound", SqlDbType.DateTime, upperBound);

				_ = command.ExecuteNonQuery();
			}
		}
	}
}
