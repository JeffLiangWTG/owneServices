using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	class PurgeOrphanRecordsFromTablesSecondVersion : DataTransformation
	{
		public override string UserDescription => "Purge orphan records from tables second version";

		const string LastProcessedPKInTables = "LastProcessedPKInPurgeOrphanRecordsFromTables";
		const string LastProcessedTableName = "LastProcessedTableInPurgeOrphanRecordsFromTables";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
				var tableList = GetOrphanDataTablesForDeletion();
				var parentChildren = GetParentChildrenByForeignKey(tableList);

				Guid.TryParse(ExtProperty.Database.Select(Db.Connection, LastProcessedPKInTables), out var lastProcessedPK);
				var lastProcessedTableName = ExtProperty.Database.Select(Db.Connection, LastProcessedTableName);

				var currentTableList = lastProcessedTableName.IsNullOrEmpty() ? tableList : tableList.SkipWhile(x => x.TableName != lastProcessedTableName).ToList();
				var timer = Stopwatch.StartNew();
				foreach (var table in currentTableList)
				{
					var rowCountApprox = DataUtils.GetApproximateRowCountForTable(Db.Connection, table.TableName);
					foreach (var chunk in GuidChunker.GenerateChunks(chunkSize: 1000, rowCountApprox, lastProcessedPK))
					{
						token.ThrowIfCancellationRequested();
						PurgeTableOrphanData(table, parentChildren, chunk.LowerBound, chunk.UpperBound);
						if (timer.Elapsed.TotalMinutes >= 1)
						{
							timer.Restart();
							ExtProperty.Database.Update(Db.Connection, LastProcessedPKInTables, chunk.UpperBound.ToString());
							ExtProperty.Database.Update(Db.Connection, LastProcessedTableName, table.TableName);
							var index = currentTableList.FindIndex(x => x.TableName == table.TableName);
							manager?.ShowInfoMessage($"Purged orphan data from {index} tables, remaining {currentTableList.Count - index} tables need to be purged.");
						}
					}
					lastProcessedPK = Guid.Empty;
				}
				ExtProperty.Database.Delete(Db.Connection, LastProcessedPKInTables);
				ExtProperty.Database.Delete(Db.Connection, LastProcessedTableName);
			}

		void ClearTempTable()
		{
			_ = Db.Connection.ExecuteNonQuery("TRUNCATE TABLE #TempTableWithPKForAllOrphans;");
		}

		void PurgeTableOrphanData(OrphanTable table, Dictionary<string, List<OrphanTableWithChild>> parentChildren, Guid lowerBound, Guid upperBound)
		{
			var parentTableList = GetParentTableList(table, lowerBound, upperBound);
			var pkGroups = parentTableList.GroupBy(x => x.PkColumnName).ToList();
			foreach (var pkGroup in pkGroups)
			{
				if (QueryOrphanDataIntoTempTable(table, pkGroup, lowerBound, upperBound) <= 0)
				{
					continue;
				}
				DeleteOrphanRelatedRecordsFromTables(parentChildren, table.TableName, 1);
				DeleteOrphansFromTables(table.TableName, table.TablePkName, 0);
				ClearTempTable();
			}
		}

		int QueryOrphanDataIntoTempTable(OrphanTable table, IGrouping<string, ParentTableTarget> pkGroup, Guid lowerBound, Guid upperBound)
		{
			var sql = new StringBuilder();
			_ = sql.AppendLine($@"
				INSERT INTO #TempTableWithPKForAllOrphans (PK, TableName, Generation)
				SELECT {table.TablePkName.QuoteName()}, {table.TableName.QuoteName('\'')}, 0
				FROM {table.TableName.QuoteName()} as parent
			");

			var tableCodeStatementBuilder = new StringBuilder();
			_ = tableCodeStatementBuilder.AppendLine("WHERE(");

			var exclusionStatementBuilder = new StringBuilder();

			var pkIndex = 1;
			var pkCount = pkGroup.Count();
			foreach (var item in pkGroup)
			{
				var parentTableCode = table.UseTableCode ? item.TableCode : item.TableName;

				_ = tableCodeStatementBuilder.AppendLine($@"parent.{table.ParentTableColumnName.QuoteName()} = {parentTableCode.QuoteName('\'')}");
				if (pkIndex < pkCount)
				{
					_ = tableCodeStatementBuilder.Append(" OR ");
				}

				_ = exclusionStatementBuilder.AppendLine($@"AND NOT EXISTS(SELECT 1 FROM {item.TableName} child WHERE child.{item.PkColumnName.QuoteName()} = parent.{table.ParentIdColumnName})");

				pkIndex++;
			}

			_ = tableCodeStatementBuilder.AppendLine(")");

			_ = sql.AppendLine(tableCodeStatementBuilder.ToString());
			_ = sql.AppendLine(exclusionStatementBuilder.ToString());

			_ = sql.AppendLine($@"AND ({table.TablePkName.QuoteName()} >= @LowerBound) AND ({table.TablePkName.QuoteName()} < @UpperBound)");
			_ = sql.AppendLine("SELECT @@ROWCOUNT AS RecordCount;");

			var number = Db.Connection.ExecuteScalar<int>(
				sql.ToString(),
				cmd =>
				{
					cmd.AddParameter("@LowerBound", SqlDbType.UniqueIdentifier, lowerBound);
					cmd.AddParameter("@UpperBound", SqlDbType.UniqueIdentifier, upperBound);
				});
			return number;
		}

		int QueryOrphanRelatedDataIntoTempTable(OrphanTableWithChild table, string parentTableName, int generation)
		{
			var pks = new List<Guid>();
			using (var findPksCommand = Db.Connection.Command($"SELECT PK from #TempTableWithPKForAllOrphans WHERE TableName = {parentTableName.QuoteName('\'')}"))
			{
				using (var reader = findPksCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						pks.Add(reader.GetGuid(0));
					}
				}
			}

			var sql = $@"
				INSERT INTO #TempTableWithPKForAllOrphans (PK, TableName, Generation)
				SELECT {table.ChildTablePk.QuoteName()}, {table.ChildTable.QuoteName('\'')}, {generation}
				FROM {table.ChildTable.QuoteName()} t
				JOIN #TempTableWithPKForAllOrphans tmp
				ON t.{table.ForeignKeyColumn.QuoteName()} = tmp.PK
				AND tmp.TableName = {parentTableName.QuoteName('\'')}
				AND {table.ChildTablePk.QuoteName()} NOT IN (SELECT Value FROM @loadedPks);

				SELECT @@ROWCOUNT AS RecordCount;
			";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddTableValuedParameter("@loadedPks", "dbo.TVP_uniqueidentifier", pks);
				return (int)command.ExecuteScalar();
			}
		}

		void DeleteOrphansFromTables(string tableName, string pkName, int generation)
		{
			string sql = $@"
				DELETE t
				FROM {tableName.QuoteName()} t
				JOIN #TempTableWithPKForAllOrphans tmp
				ON t.{pkName.QuoteName()} = tmp.PK AND tmp.TableName = {tableName.QuoteName('\'')}
				AND tmp.generation = {generation};
			";
			_ = Db.Connection.ExecuteNonQuery(sql);
		}

		void DeleteOrphanRelatedRecordsFromTables(Dictionary<string, List<OrphanTableWithChild>> parentChildren, string parentTableName, int generation)
		{
			bool found = parentChildren.TryGetValue(parentTableName, out var tablesWithChildren);
			if (!found)
			{
				return;
			}

			foreach (var table in tablesWithChildren)
			{
				if (QueryOrphanRelatedDataIntoTempTable(table, parentTableName, generation) <= 0)
				{
					continue;
				}
				DeleteOrphanRelatedRecordsFromTables(parentChildren, table.ChildTable, generation + 1);
				DeleteOrphansFromTables(table.ChildTable, table.ChildTablePk, generation);
			}
		}

		string GetSqlQueryParentTables(OrphanTable table)
		{
			string sqlUseTableCode = $@"
				SELECT DISTINCT {table.ParentTableColumnName.QuoteName()} AS TableCode, TableName, PkColumnName
				FROM {table.TableName.QuoteName()}
				LEFT JOIN #ParentObjectsAndTheirPkColumnsForAllOrphans
				ON PkColumnName LIKE {table.ParentTableColumnName.QuoteName()} + '_PK'
				WHERE (TableName IS NOT NULL) AND ({table.ParentTableColumnName.QuoteName()} <> '') AND ({table.TablePkName.QuoteName()} >= @LowerBound) AND ({table.TablePkName.QuoteName()} < @UpperBound);
			";
			string sqlUseTableName = $@"
				SELECT DISTINCT {table.ParentTableColumnName.QuoteName()} AS TableCode, TableName, PkColumnName
				FROM {table.TableName.QuoteName()}
				LEFT JOIN #ParentObjectsAndTheirPkColumnsForAllOrphans
				ON TableName = {table.ParentTableColumnName.QuoteName()}
				WHERE (TableName IS NOT NULL) AND ({table.ParentTableColumnName.QuoteName()} <> '') AND ({table.TablePkName.QuoteName()} >= @LowerBound) AND ({table.TablePkName.QuoteName()} < @UpperBound);
			";

			return table.UseTableCode ? sqlUseTableCode : sqlUseTableName;
		}

		List<ParentTableTarget> GetParentTableList(OrphanTable table, Guid lowerBound, Guid upperBound)
		{
			var parentTableList = new List<ParentTableTarget>();
			var sqlQueryParentTables = GetSqlQueryParentTables(table);
			using (var command = Db.Connection.Command(sqlQueryParentTables))
			{
				command.AddParameter("@LowerBound", SqlDbType.UniqueIdentifier, lowerBound);
				command.AddParameter("@UpperBound", SqlDbType.UniqueIdentifier, upperBound);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var tableCode = reader["TableCode"] as string;
						var tableName = reader["TableName"] as string;
						var pkColumnName = reader["PkColumnName"] as string;
						if (tableCode == null || tableName == null || pkColumnName == null)
						{
							continue;
						}
						parentTableList.Add(new ParentTableTarget()
						{
							TableName = tableName,
							TableCode = tableCode,
							PkColumnName = pkColumnName,
						});
					}
				}
			}

			return parentTableList;
		}

		class ParentTableTarget
		{
			public string TableName { get; set; }
			public string TableCode { get; set; }
			public string PkColumnName { get; set; }
		}

		public class OrphanTable
		{
			public string TableName { get; set; }
			public string TablePkName { get; set; }
			public string ParentIdColumnName { get; set; }
			public string ParentTableColumnName { get; set; }
			public bool UseTableCode { get; set; }
		}

		class OrphanTableWithChild
		{
			public string ParentTable { get; set; }
			public string ChildTable { get; set; }
			public string ChildTablePk { get; set; }
			public string ForeignKeyColumn { get; set; }
		}

		public List<OrphanTable> GetOrphanDataTablesForDeletion()
		{
			var tableList = new List<OrphanTable>();
			var tablesWithoutSchemas = new List<OrphanTable>();

			var sql = @"
				DROP TABLE IF EXISTS #TempTableWithPKForAllOrphans;
				CREATE TABLE #TempTableWithPKForAllOrphans
				(
					PK uniqueidentifier NOT NULL,
					TableName VARCHAR(128) COLLATE database_default NOT NULL,
					Generation int DEFAULT 0 NOT NULL,
				);

				DROP TABLE IF EXISTS #ParentObjectsAndTheirPkColumnsForAllOrphans;
				WITH Tables AS
				(
					SELECT t.name TableName, c.name PkColumnName
					FROM sys.columns c
					JOIN sys.tables t
					ON c.object_id = t.object_id AND c.name like '%[_]PK'
					WHERE SCHEMA_NAME(t.schema_id) <> 'cdc' AND SCHEMA_NAME(t.schema_id) <> 'hrm'
				)
				SELECT TableName, PkColumnName INTO #ParentObjectsAndTheirPkColumnsForAllOrphans FROM Tables;

				DROP TABLE IF EXISTS #TableAndColumn;
				CREATE TABLE #TableAndColumn (
					table_name VARCHAR(128) COLLATE database_default NOT NULL,
					schema_name NVARCHAR(128) COLLATE database_default NOT NULL,
					column_name NVARCHAR(128) COLLATE database_default NOT NULL,
					max_length INT
				);

				INSERT INTO #TableAndColumn (table_name, schema_name, column_name, max_length)
				SELECT t.name AS table_name,
				SCHEMA_NAME(t.schema_id) AS schema_name,
				c.name AS column_name,
				isc.character_maximum_length AS max_length
				FROM sys.tables AS t
				INNER JOIN sys.columns c ON t.object_id = c.object_id
				INNER JOIN information_schema.columns isc ON (isc.column_name = c.name AND isc.table_name = t.name)
				WHERE (c.name LIKE '%_%ParentTableCode' OR c.name LIKE '%_ParentTableCode' OR c.name LIKE '%_%TableCode' OR c.name LIKE '%_TableCode'
				OR c.name LIKE '%_ParentTable' OR c.name LIKE '%_Table' OR c.name LIKE '%_%ParentTableName' OR c.name LIKE '%_ParentTableName'
				OR c.name = 'TE_EntityTableCodeFrom' OR c.name = 'TE_EntityTableCodeTo')
				AND (SCHEMA_NAME(t.schema_id) != 'CDC' AND isc.TABLE_SCHEMA != 'cdc'
				AND isc.DATA_TYPE = 'varchar' AND (isc.CHARACTER_MAXIMUM_LENGTH = 3 OR isc.CHARACTER_MAXIMUM_LENGTH = 35))
				AND table_name NOT IN ('StmLoginFailureLog', 'AccCurrencyAdjustmentQueue', 'AccTaxConfiguration',
								'AccTransactionComplianceReportQueue', 'AccTransactionPostingToGLDQueue', 'ArchiveMainItemQueue', 'JobChargePostingQueue', 'JobChargeTarget',
								'EDIMessage', 'ProcessQueue', 'ProcessTaskNotification', 'StmALogQueue', 'StmALogQueueWTE', 'StmJobQueue',
								'StmQueueState', 'P4PlanLineItem', 'GenCustomColumnDefinition', 'GenRegCertAccredMaintList', 'GlbHoliday',
								'GlbPasswordHistory', 'GlbPersonPrimaryRelationship', 'GlbStaffHoliday', 'GlbWorkTime', 'JobDocumentDelivery', 'JobHeader',
								'StmAccessToken', 'StmMenuItem', 'StmModuleFilter', 'StmModuleFilterUserData', 'StmNumberRangeMatchingDetail',
								'StmPrintJob', 'StmProcessQueue', 'StmServiceHeartBeat', 'StmUniversalCopy', 'StmScheduleTask', 'AccCommissionHeader',
								'OrgSales', 'RelatedActivityPivot', 'VoteExamSurveyQuestion', 'AsycudaManifestHeader', 'CusAuthorizationUsage',
								'CusEntryCPDec', 'CusExitHeader', 'CusInBondHeader', 'CusPollingTransaction', 'CusSCAOceanBill',
								'CusUnderbond', 'JobComInvoiceLine', 'JPAFRHeader', 'LandedCostHistory', 'StorageDocs', 'BarcodeRuleSet', 'WhsItemDispatchConsignment',
								'WhsItemDispatchLoadList', 'WhsItemReceiveASN', 'WhsItemReceiveConsignment', 'DtbBookingConsolidation', 'JobCartage',
								'GteGateMovementBooking', 'JobMawb', 'JobScheduleChange', 'StmActivityLog', 'BMNCNShape', 'StmServiceMutex', 'RateEntry',
								'RateLines', 'StmAlog', 'StmActivityLog', 'ProcessHeader', 'ProcessTasks')
				AND column_name NOT IN ('P9_ReferencedTableCode', 'ES_CurrentContextTableCode', 'PJ_SourceTableCode')
				AND table_name NOT LIKE 'Ref%';

				SELECT table_name,column_name,schema_name,max_length, PkColumnName
				FROM #TableAndColumn
				LEFT JOIN #ParentObjectsAndTheirPkColumnsForAllOrphans
				ON table_name  = TableName
				WHERE PkColumnName IS NOT NULL
				ORDER BY table_name;";

			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = reader["table_name"] as string;
					var parentTableColumnName = reader["column_name"] as string;
					var tablePkName = reader["PkColumnName"] as string;
					var maxLen = reader["max_length"] as int?;
					var schema = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetTableSchema(tableName);
					if (schema != null)
					{
						var allColumns = schema.All;
						var parentIdColumnName = TryToGetParentIdColumnName(parentTableColumnName, allColumns);
						if (parentIdColumnName != null)
						{
							tableList.Add(new OrphanTable()
							{
								TableName = tableName,
								TablePkName = tablePkName,
								ParentIdColumnName = parentIdColumnName.Name,
								ParentTableColumnName = parentTableColumnName,
								UseTableCode = (maxLen == 3),
							});
						}
					}
					else
					{
						tablesWithoutSchemas.Add(new OrphanTable()
						{
							TableName = tableName,
							TablePkName = tablePkName,
							ParentIdColumnName = string.Empty,
							ParentTableColumnName = parentTableColumnName,
							UseTableCode = (maxLen == 3),
						});
					}
				}
			}

			foreach (var tableWithoutSchema in tablesWithoutSchemas)
			{
				var parentIdColumnName = TryToMapColumnToExplicitlyNamedParent(tableWithoutSchema.ParentTableColumnName, GetColumnNames(tableWithoutSchema.TableName));
				if (!parentIdColumnName.IsNullOrEmpty())
				{
					tableWithoutSchema.ParentIdColumnName = parentIdColumnName;
					tableList.Add(tableWithoutSchema);
				}
			}

			return tableList;
		}

		Dictionary<string, List<OrphanTableWithChild>> GetParentChildrenByForeignKey(List<OrphanTable> tableList)
		{
			var tableWithChildList = GetOrphanDataTablesWithChildren();
			var parentChildren = new Dictionary<string, List<OrphanTableWithChild>>();
			HashSet<string> tableNameSet = tableList.Select(x => x.TableName).ToHashSet();
			while (tableNameSet.Count > 0)
			{
				var tableName = tableNameSet.First();
				var tablesWithChildren = tableWithChildList.Where(x => x.ParentTable == tableName).ToList();
				if (tablesWithChildren.Count > 0)
				{
					parentChildren.Add(tableName, tablesWithChildren);
					foreach (var tableWithChild in tablesWithChildren)
					{
						if (tableNameSet.Contains(tableWithChild.ChildTable) ||
							parentChildren.ContainsKey(tableWithChild.ChildTable))
						{
							continue;
						}
						tableNameSet.Add(tableWithChild.ChildTable);
					}
				}
				tableNameSet.Remove(tableName);
			}

			return parentChildren;
		}

		List<OrphanTableWithChild> GetOrphanDataTablesWithChildren()
		{
			var tableWithChildList = new List<OrphanTableWithChild>();
			var sqlGetAllTableWithChild = @"
				SELECT 
					object_name(fk.referenced_object_id) AS ParentTable, t.name AS ChildTable, pok.PkColumnName AS ChildPK, c.name AS ForeignKeyColumn
				FROM
					sys.foreign_key_columns AS fk
				INNER JOIN 
					sys.tables AS t ON fk.parent_object_id = t.object_id
				INNER JOIN 
					sys.columns AS c ON fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
				INNER JOIN
					sys.foreign_keys AS fks ON fks.object_id = fk.constraint_object_id
				INNER JOIN		
					#ParentObjectsAndTheirPkColumnsForAllOrphans AS pok ON pok.TableName = t.name
				WHERE
					fks.delete_referential_action = 0
					OR fks.delete_referential_action = 1;";

			using (var reader = Db.Connection.Command(sqlGetAllTableWithChild).ExecuteReader())
			{
				while (reader.Read())
				{
					var parentTable = reader["ParentTable"] as string;
					var childTable = reader["ChildTable"] as string;
					var childTablePk = reader["ChildPK"] as string;
					var foreignKeyColumn = reader["ForeignKeyColumn"] as string;
					if (parentTable != null && childTable != null && foreignKeyColumn != null && childTablePk != null)
					{
						tableWithChildList.Add(new OrphanTableWithChild()
						{
							ParentTable = parentTable,
							ChildTable = childTable,
							ChildTablePk = childTablePk,
							ForeignKeyColumn = foreignKeyColumn,
						});
					}
				}
			}
			return tableWithChildList;
		}

		SchemaColumn TryToGetParentIdColumnName(string parentTableColumnName, SchemaColumnCollection all)
		{
			var nameOfIDColumn = TryToMapColumnToExplicitlyNamedParent(parentTableColumnName, all.Where(c => c is SchemaGuidColumn).Select(c => c.Name));
			return all.FirstOrDefault(c => c.Name == nameOfIDColumn);
		}

		bool IsConstrainedForeignKey(string columnName)
		{
			var colNameParts = columnName.Split('_');
			return (colNameParts.Length >= 3 && colNameParts[0].Length >= 2 && colNameParts[0].Length <= 3 && colNameParts[1].Length >= 2 && colNameParts[1].Length <= 3);
		}

		string TryToMapColumnToExplicitlyNamedParent(string parentTableColumnName, IEnumerable<string> allGuidcolumns)
		{
			var allValidIDColumnCandidates = allGuidcolumns.Where(c => !IsConstrainedForeignKey(c) && !c.EndsWith("_PK"));
			var allPossibleIDColumnSuffixes = new string[] { "ParentID", "Parent", "ParentGuid", "ID", "Guid", "ForeignKey", "Foreign_Key", "PK",
				"JobId", "UniqueID", "IdFrom", "IdTo" };
			foreach (var suffix in new string[] { "Table", "ParentTable", "ParentTableName", "TableCode", "TableCodeFrom", "TableCodeTo" })
			{
				if (parentTableColumnName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
				{
					var match = allValidIDColumnCandidates.FirstOrDefault(c => allPossibleIDColumnSuffixes.Any(idSuffix =>
						c.EndsWith(idSuffix, StringComparison.OrdinalIgnoreCase) &&
						parentTableColumnName.Replace(suffix, idSuffix).Equals(c, StringComparison.OrdinalIgnoreCase)));
					if (match != null)
					{
						return match;
					}
				}
			}
			return string.Empty;
		}

		IEnumerable<string> GetColumnNames(string tableName)
		{
			var columnList = new List<string>();
			var sql = @"
	SELECT c.name from sys.columns c
	INNER JOIN sys.tables t on c.object_id = t.object_id
	WHERE t.name = @TableName
	AND t.schema_id = 1";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@TableName", SqlDbType.VarChar, tableName);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var columnName = reader[0] as string;
						if (columnName != null)
						{
							columnList.Add(columnName);
						}
					}
				}
			}
			return columnList;
		}
	}
}
