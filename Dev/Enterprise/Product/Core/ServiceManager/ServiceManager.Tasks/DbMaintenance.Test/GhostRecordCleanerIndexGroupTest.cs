using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	class GhostRecordCleanerIndexGroupTest : TestCase
	{
		public void TestGetIndexNames()
		{
			var excludedTables = new[]
			{
				"HRJobApplicationParsingQueue",
				"StmProcessQueue",
				"ProcessQueue",
				"EDIMessageQueueState",
				"AccTransactionComplianceReportQueue"
			};

			var includedTables = new[]
			{
				"TimeActionSchedule",
			};

			var includedFilteredIndexTables = new[]
			{
				"EDIInterchange",
				"EDIMessage",
				"MailDBItems",
			};

			var excludedIndexes = new[]
			{
				"",
			};

			var sql = $@"
				select i.name, o.name, i.has_filter, i.type_desc from sys.indexes i join sys.objects o on i.object_id = o.object_id 
				where 
					o.type = 'U'
					and (
						i.type <> 0 and o.name like '%Queue%' and o.name not in ('{string.Join("', '", excludedTables)}')
						or
						o.name in ('{string.Join("', '", includedTables)}')
						or
						i.has_filter = 1 and i.filter_definition like '%Status%' and o.name in ('{string.Join("', '", excludedTables)}', '{string.Join("', '", includedFilteredIndexTables)}')
						)";
			var expectedIndexes = new List<string>();
			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					expectedIndexes.Add(reader.GetString(0));
				}
			}

			expectedIndexes.RemoveAll(excludedIndexes.Contains);

			var indexesList = GhostRecordCleanerIndexGroup.GhostRecordCleanerIndexes.Select(tuple => tuple.indexName).ToList();
			expectedIndexes.Sort();
			indexesList.Sort();
			AssertContainsExactElementsInAnyOrder("Expected amount of indexes", expectedIndexes, indexesList);
		}

		public void TestNoDuplicateIndexes()
		{
			var duplicates = GhostRecordCleanerIndexGroup.GhostRecordCleanerIndexes.GroupBy(x => x)
				.Where(g => g.Count() > 1)
				.Select(x => x.Key)
				.ToList();

			var failureMessage = $"Should be no duplicate indexes however found duplicates: {(string.Join("\n", duplicates.Select(x => $"{x.schemaName}.{x.tableName}.{x.indexName}")))}";
			AssertEquals(failureMessage, 0, duplicates.Count);
		}

		public void TestAllListedIndexesExistAndUnique()
		{
			// Arrange
			using (var command = Db.Connection.Command(@"
SELECT 
	INDEX_EXISTS = IIF(
		EXISTS(
			SELECT NULL
			FROM sys.indexes as i
			WHERE 1=1
			 AND object_id = OBJECT_ID(@schemaName + N'.' + @tableName, 'U')
			 AND i.name = @indexName
		)
	, 1, 0);"))
			{
				command.AddParameter("@schemaName", SqlDbType.NVarChar, 128, string.Empty);
				command.AddParameter("@tableName", SqlDbType.NVarChar, 128, string.Empty);
				command.AddParameter("@indexName", SqlDbType.NVarChar, 128, string.Empty);

				// Act
				var result = GhostRecordCleanerIndexGroup
					.GhostRecordCleanerIndexes
					.Where(tuple => !IndexExistsAndUnique(command, tuple.schemaName, tuple.tableName, tuple.indexName))
					.ToArray();

				// Assert
				AssertContainsExactElementsInAnyOrder(
					"All indexes should exist in the database",
					Enumerable.Empty<(string, string)>(),
					result);
			}

			bool IndexExistsAndUnique(DbCommand command, string schemaName, string tableName, string indexName)
			{
				command.SetParameterValue("@schemaName", schemaName);
				command.SetParameterValue("@tableName", tableName);
				command.SetParameterValue("@indexName", indexName);
				return Convert.ToInt32(command.ExecuteScalar()) == 1;
			}
		}
	}
}
