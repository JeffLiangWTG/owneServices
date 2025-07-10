using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	class PurgeProcessHeaderOrphans : DataTransformation
	{
		public override string UserDescription => "Purge orphan records from dbo.ProcessHeader and dbo.ProcessTasks";

		const string LastProcessedPkInHeader = "LastProcessedPKInProcessHeader";
		const string LastProcessedPkInTasks = "LastProcessedPKInProcessTasks";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			InitParentTableList();
			Guid.TryParse(ExtProperty.Database.Select(Db.Connection, LastProcessedPkInHeader), out var lastProcessedPkInHeader);
			Guid.TryParse(ExtProperty.Database.Select(Db.Connection, LastProcessedPkInTasks), out var lastProcessedPkInTasks);

			var parentChildRelationsInSchema = GetParentChildrenByForeignKey();

			var timer = Stopwatch.StartNew();
			if (lastProcessedPkInTasks.Equals(Guid.Empty))
			{
				manager?.ShowInfoMessage($"No previously processed ProcessTask rows detected, commencing with ProcessHeader");

				var headerRowCountApprox = DataUtils.GetApproximateRowCountForTable(Db.Connection, ProcessHeaderSchema.Constants.TableName);
				foreach (var chunk in GuidChunker.GenerateChunks(chunkSize: 1000, headerRowCountApprox, lastProcessedPkInHeader))
				{
					manager?.ShowInfoMessage($"Now Processing ProcessHeader between Lower Bound {chunk.LowerBound} and Upper Bound {chunk.UpperBound}");

					token.ThrowIfCancellationRequested();
					PurgeProcessHeaderOrphanData(parentChildRelationsInSchema, chunk.LowerBound, chunk.UpperBound);
					if (timer.Elapsed.TotalMinutes >= 1)
					{
						timer.Restart();
						ExtProperty.Database.Update(Db.Connection, LastProcessedPkInHeader, chunk.UpperBound.ToString());
						manager?.ShowInfoMessage($"Last processed batch primary key in ProcessHeader: {chunk.UpperBound.ToString()}.");
					}
				}
			}

			if (lastProcessedPkInTasks.Equals(Guid.Empty))
			{
				manager?.ShowInfoMessage($"ProcessHeader complete. Commencing with ProcessTasks");
			}
			else
			{
				manager?.ShowInfoMessage($"Previously processed ProcessTask rows successfully detected");
			}

			var tasksRowCountApprox = DataUtils.GetApproximateRowCountForTable(Db.Connection, ProcessTasksSchema.Constants.TableName);
			foreach (var chunk in GuidChunker.GenerateChunks(chunkSize: 1000, tasksRowCountApprox, lastProcessedPkInTasks))
			{
				manager?.ShowInfoMessage($"Now Processing ProcessTasks between Lower Bound {chunk.LowerBound} and Upper Bound {chunk.UpperBound}");

				token.ThrowIfCancellationRequested();
				PurgeProcessTasksOrphanData(parentChildRelationsInSchema, chunk.LowerBound, chunk.UpperBound);
				if (timer.Elapsed.TotalMinutes >= 1)
				{
					timer.Restart();
					ExtProperty.Database.Update(Db.Connection, LastProcessedPkInTasks, chunk.UpperBound.ToString());
					manager?.ShowInfoMessage($"Last processed batch primary key in ProcessTasks: {chunk.UpperBound.ToString()}.");
				}
			}
			ExtProperty.Database.Delete(Db.Connection, LastProcessedPkInHeader);
			ExtProperty.Database.Delete(Db.Connection, LastProcessedPkInTasks);
		}

		List<OrphanTableWithChild> GetOrphanDataTablesWithChildren()
		{
			var tableWithChildList = new List<OrphanTableWithChild>();
			string sqlGetAllTableWithChild = $@"
				SELECT 
					object_name(fk.referenced_object_id) AS ParentTable, t.name AS ChildTable, pok.ColumnName AS ChildPK, c.name AS ForeignKeyColumn
				FROM
					sys.foreign_key_columns AS fk
				INNER JOIN 
					sys.tables AS t ON fk.parent_object_id = t.object_id
				INNER JOIN 
					sys.columns AS c ON fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
				INNER JOIN
					sys.foreign_keys AS fks ON fks.object_id = fk.constraint_object_id
				INNER JOIN		
					#ParentObjectsAndTheirPkColumnsForProcessHeader AS pok ON pok.TableName = t.name
				WHERE
					fks.delete_referential_action = 0;";

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

		Dictionary<string, List<OrphanTableWithChild>> GetParentChildrenByForeignKey()
		{
			var tableWithChildList = GetOrphanDataTablesWithChildren();

			var initialTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				ProcessHeaderSchema.Constants.TableName,
				ProcessTasksSchema.Constants.TableName
			};

			var lookup = tableWithChildList
				.ToLookup(x => x.ParentTable, StringComparer.OrdinalIgnoreCase);

			var parentChildRelationsInSchema = new Dictionary<string, List<OrphanTableWithChild>>(StringComparer.OrdinalIgnoreCase);

			var queue = new Queue<string>(initialTables);

			while (queue.Count > 0)
			{
				var tableName = queue.Dequeue();

				var tablesWithChildren = lookup[tableName].ToList();

				if (tablesWithChildren.Any())
				{
					parentChildRelationsInSchema[tableName] = tablesWithChildren;

					foreach (var childTable in tablesWithChildren.Select(x => x.ChildTable))
					{
						if (!parentChildRelationsInSchema.ContainsKey(childTable) && !queue.Contains(childTable))
						{
							queue.Enqueue(childTable);
						}
					}
				}
			}

			return parentChildRelationsInSchema;
		}

		int DbExecuteWithPkRange(string sql, Guid lowerBound, Guid upperBound)
		{
			var number = Db.Connection.ExecuteScalar<int>(
				sql,
				cmd =>
				{
					cmd.AddParameter("@LowerBound", SqlDbType.UniqueIdentifier, lowerBound);
					cmd.AddParameter("@UpperBound", SqlDbType.UniqueIdentifier, upperBound);
				});
			return number;
		}

		void ClearTempTable()
		{
			_ = Db.Connection.ExecuteNonQuery("TRUNCATE TABLE #TempTableWithPK;");
		}
		int QueryOrphanDataIntoTempTable(OrphanTableWithChild table, string rootRecord, int generation)
		{
			var sql = $@"
				INSERT INTO #TempTableWithPK (PK, TableName, Generation)
				SELECT {table.ChildTablePk.QuoteName()}, {table.ChildTable.QuoteName('\'')}, {generation}
				FROM {table.ChildTable.QuoteName()} t
				JOIN #TempTableWithPK tmp
				ON t.{table.ForeignKeyColumn.QuoteName()} = tmp.PK
				AND tmp.TableName = {rootRecord.QuoteName('\'')}
				AND {table.ChildTablePk.QuoteName()} NOT IN (SELECT PK from #TempTableWithPK where TableName = {rootRecord.QuoteName('\'')});

				SELECT @@ROWCOUNT AS RecordCount;
			";

			using (var command = Db.Connection.Command(sql))
			{
				return (int)command.ExecuteScalar();
			}
		}

		void DeleteOrphansFromTables(string tableName, string pkName, int generation)
		{
			string sql = $@"
				DELETE t
				FROM {tableName.QuoteName()} t
				JOIN #TempTableWithPK tmp
				ON t.{pkName.QuoteName()} = tmp.PK
				AND tmp.TableName = {tableName.QuoteName('\'')}
				AND tmp.Generation = {generation};
			";

			var numberOfRowsPurged = Db.Connection.ExecuteNonQuery(sql);
			manager?.ShowInfoMessage($"Purged {numberOfRowsPurged} rows from {tableName}");
		}

		void DeleteOrphanRelatedRecordsFromTables(Dictionary<string, List<OrphanTableWithChild>> parentChildRelationsInSchema, string rootRecord, int generation)
		{
			bool found = parentChildRelationsInSchema.TryGetValue(rootRecord, out var tablesWithChildren);
			if (!found)
			{
				return;
			}

			foreach (var tableChildPair in tablesWithChildren)
			{
				if (QueryOrphanDataIntoTempTable(tableChildPair, rootRecord, generation) <= 0)
				{
					continue;
				}
				DeleteOrphanRelatedRecordsFromTables(parentChildRelationsInSchema, tableChildPair.ChildTable, generation + 1);
				DeleteOrphansFromTables(tableChildPair.ChildTable, tableChildPair.ChildTablePk, generation);
			}
		}

		void DeleteSpecialOrphansFromProcessHeader(Guid lowerBound, Guid upperBound)
		{
			var numberOfRowsPurged = 0;

			string sqlDeleteOrphansFromProcessHeader = @"DELETE FROM dbo.ProcessHeader
														FROM dbo.ProcessHeader WITH (FORCESEEK, INDEX(PK_UX__FH_PK))
														WHERE (FH_ParentId IS NULL) AND (FH_P0_Template IS NULL)
														AND (FH_PK >= @LowerBound) AND (FH_PK < @UpperBound);";
			using (var command = Db.Connection.Command(sqlDeleteOrphansFromProcessHeader))
			{
				command.AddParameter("@LowerBound", SqlDbType.UniqueIdentifier, lowerBound);
				command.AddParameter("@UpperBound", SqlDbType.UniqueIdentifier, upperBound);
				numberOfRowsPurged = command.ExecuteNonQuery();
			}

			manager?.ShowInfoMessage($"Purged {numberOfRowsPurged} 'Special Orphans' from ProcessHeader");
		}
		void PurgeProcessHeaderOrphanData(Dictionary<string, List<OrphanTableWithChild>> parentChildRelationsInSchema, Guid lowerBound, Guid upperBound)
		{
			var headerParentList = GetParentTableList(SqlForGetProcessHeaderParentTable, lowerBound, upperBound);
			var pkGroups = headerParentList.GroupBy(x => x.PkColumnName).ToList();

			foreach (var pkGroup in pkGroups)
			{
				var itemCount = QueryProcessHeaderOrphanDataIntoTempTable(pkGroup, lowerBound, upperBound);
				if (itemCount <= 0)
				{
					continue;
				}
				else
				{
					DeleteOrphanRelatedRecordsFromTables(parentChildRelationsInSchema, ProcessHeaderSchema.Constants.TableName, 1);
					DeleteOrphansFromTables(ProcessHeaderSchema.Constants.TableName, ProcessHeaderSchema.PK.Name, 0);
					ClearTempTable();
				}
			}

			DeleteSpecialOrphansFromProcessHeader(lowerBound, upperBound);
		}

		int QueryProcessHeaderOrphanDataIntoTempTable(IGrouping<string, ParentTableTarget> pkGroup, Guid lowerBound, Guid upperBound)
		{
			var sql = new StringBuilder();
			_ = sql.AppendLine($@"
			INSERT INTO #TempTableWithPK (PK, TableName)
			SELECT FH_PK, 'ProcessHeader' AS Source
			FROM dbo.ProcessHeader AS PH WITH (FORCESEEK, INDEX(PK_UX__FH_PK))
			WHERE(PH.FH_ParentTableCode = {pkGroup.First().TableCode.QuoteName('\'')})  AND PH.FH_P0_Template IS NULL");

			foreach (var item in pkGroup)
			{
				_ = sql.AppendLine($@"AND NOT EXISTS(SELECT 1 FROM {item.TableName} WHERE {item.PkColumnName.QuoteName()} = PH.FH_ParentId)");
			}

			sql.AppendLine("AND(PH.FH_PK >= @LowerBound) AND(PH.FH_PK < @UpperBound)");
			sql.AppendLine("SELECT @@ROWCOUNT AS RecordCount;");
			return DbExecuteWithPkRange(sql.ToString(), lowerBound, upperBound);
		}

		void PurgeProcessTasksOrphanData(Dictionary<string, List<OrphanTableWithChild>> parentChildRelationsInSchema, Guid lowerBound, Guid upperBound)
		{
			var tasksParentList = GetParentTableList(SqlForGetProcessTasksParentTable, lowerBound, upperBound);
			var pkGroups = tasksParentList.GroupBy(x => x.PkColumnName).ToList();
			foreach (var pkGroup in pkGroups)
			{
				var itemCount = QueryProcessTasksOrphanDataIntoTempTable(pkGroup, lowerBound, upperBound);

				if (itemCount <= 0)
				{
					continue;
				}

				DeleteOrphanRelatedRecordsFromTables(parentChildRelationsInSchema, ProcessTasksSchema.Constants.TableName, 1);
				DeleteOrphansFromTables(ProcessTasksSchema.Constants.TableName, ProcessTasksSchema.PK.Name, 0);
				ClearTempTable();
			}
		}

		int QueryProcessTasksOrphanDataIntoTempTable(IGrouping<string, ParentTableTarget> pkGroup, Guid lowerBound, Guid upperBound)
		{
			var sql = new StringBuilder();
			_ = sql.AppendLine($@"
			INSERT INTO #TempTableWithPK (PK, TableName)
			SELECT P9_PK, 'ProcessTasks' AS Source
			FROM dbo.ProcessTasks AS PT WITH (FORCESEEK, INDEX(PK_UX__P9_PK))
			WHERE(PT.P9_ParentTableCode = {pkGroup.First().TableCode.QuoteName('\'')})");

			foreach (var item in pkGroup)
			{
				_ = sql.AppendLine($@"AND NOT EXISTS(SELECT 1 FROM {item.TableName} WHERE {item.PkColumnName.QuoteName()} = PT.P9_ParentId)");
			}

			sql.AppendLine("AND(PT.P9_PK >= @LowerBound) AND(PT.P9_PK < @UpperBound)");
			sql.AppendLine("SELECT @@ROWCOUNT AS RecordCount;");
			return DbExecuteWithPkRange(sql.ToString(), lowerBound, upperBound);
		}

		class ParentTableTarget
		{
			public string TableName { get; set; }
			public string TableCode { get; set; }
			public string PkColumnName { get; set; }
		}

		class OrphanTableWithChild
		{
			public string ParentTable { get; set; }
			public string ChildTable { get; set; }
			public string ChildTablePk { get; set; }
			public string ForeignKeyColumn { get; set; }
		}

		const string SqlForGettingAllParentTableAndTheirPkColumn = @"
			DROP TABLE IF EXISTS #TempTableWithPK;
			CREATE TABLE #TempTableWithPK
			(
				PK uniqueidentifier NOT NULL,
				TableName VARCHAR(128) COLLATE database_default NOT NULL,
				Generation int default 0 NOT NULL,
			);

			DROP TABLE IF EXISTS #ParentObjectsAndTheirPkColumnsForProcessHeader;
			WITH Tables AS
			(
				SELECT t.name TableName, c.name ColumnName
				FROM sys.columns c
				JOIN sys.tables t
				ON c.object_id = t.object_id AND c.name like '%[_]PK'
				WHERE SCHEMA_NAME(t.schema_id) <> 'cdc' AND SCHEMA_NAME(t.schema_id) <> 'hrm'
			)
			SELECT TableName, ColumnName INTO #ParentObjectsAndTheirPkColumnsForProcessHeader FROM Tables;";

		const string SqlForGetProcessHeaderParentTable = @"
			SELECT DISTINCT FH_ParentTableCode AS TableCode, TableName, ColumnName
			FROM dbo.ProcessHeader WITH (FORCESEEK, INDEX(PK_UX__FH_PK))
			LEFT JOIN #ParentObjectsAndTheirPkColumnsForProcessHeader
			ON ColumnName LIKE FH_ParentTableCode + '_PK'
			WHERE TableName IS NOT NULL AND FH_ParentTableCode <> '' AND (FH_PK >= @LowerBound) AND (FH_PK < @UpperBound);";

		const string SqlForGetProcessTasksParentTable = @"
			SELECT DISTINCT P9_ParentTableCode AS TableCode, TableName, ColumnName
			FROM dbo.ProcessTasks
			LEFT JOIN #ParentObjectsAndTheirPkColumnsForProcessHeader
			ON ColumnName LIKE P9_ParentTableCode + '_PK'
			WHERE TableName IS NOT NULL AND P9_ParentTableCode <> '' AND (P9_PK >= @LowerBound) AND (P9_PK < @UpperBound);";

		void InitParentTableList()
		{
			_ = Db.Connection.ExecuteNonQuery(SqlForGettingAllParentTableAndTheirPkColumn);
		}

		List<ParentTableTarget> GetParentTableList(string sql, Guid lowerBound, Guid upperBound)
		{
			var list = new List<ParentTableTarget>();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@LowerBound", SqlDbType.UniqueIdentifier, lowerBound);
				command.AddParameter("@UpperBound", SqlDbType.UniqueIdentifier, upperBound);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var tableCode = reader["TableCode"] as string;
						var tableName = reader["TableName"] as string;
						var columnName = reader["ColumnName"] as string;
						if (tableCode == null || tableName == null || columnName == null)
						{
							continue;
						}
						list.Add(new ParentTableTarget()
						{
							TableName = tableName,
							TableCode = tableCode,
							PkColumnName = columnName,
						});
					}
				}
			}
			return list;
		}
	}
}
