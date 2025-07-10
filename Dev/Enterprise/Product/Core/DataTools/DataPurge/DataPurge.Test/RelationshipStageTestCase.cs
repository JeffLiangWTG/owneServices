using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	public abstract class RelationshipStageTestCase : TransactionedTestCase
	{
		public abstract string[] TablePrefixes { get; }

		public abstract BusinessRelationshipStageForTest RelationshipStage { get; }

		protected virtual List<FloatingForeignKeyRelationship> FloatingForeignKeyRelationships { get; } = new();

		protected override void SetUp()
		{
			base.SetUp();
			RelationshipStage.SetupRelationships();
		}

		public void TestRelationshipsWithPurgingScripts()
		{
			var dataPurger = new DataPurger
			{
				HasRatingInfoPurgeScript = true,
				HasProductInfoPurgeScript = true,
				HasNonSystemChageCodesPurgeScript = true,
				HasTariffInfoPurgeScript = true,
				HasQuotationsPurgeScript = true
			};
			var scripts = dataPurger.GetPurgeScripts();
			var tablesToPurge = new SortedSet<string>();
			foreach (var script in scripts)
			{
				var matches = Regex.Matches(script, @"DELETE( FROM)? (\[?dbo\]?\.)?\[?(?<TableName>\w+)\]?");
				foreach (Match match in matches)
				{
					tablesToPurge.Add(match.Groups["TableName"].Value);
				}
			}

			CombineAssertions(() =>
			{
				foreach (var tableName in tablesToPurge)
				{
					if (!RelationshipStage.RelationshipMap.TryGetValue(tableName, out var relationshipList))
					{
						continue;
					}

					foreach (var businessRelationship in relationshipList)
					{
						var childTableName = businessRelationship.ChildTableName;
						Assert($"Please consider to purge table {childTableName} as well, since we purge the table {tableName} and they have the relationship in {RelationshipStage.GetType()}: {businessRelationship}." +
								$"If this is indeed unnecessary, please give a reasonable justification so ForcingTest of the related BusinessRelationship would be false.",
							!businessRelationship.ForcingTest || tablesToPurge.Contains(childTableName));

						Assert($"Since the scripts purge both tables {tableName} and {childTableName}, please move the relationship out of the whitelist by not setting justification in {RelationshipStage.GetType()}: {businessRelationship}.",
							businessRelationship.ForcingTest || !tablesToPurge.Contains(childTableName));
					}
				}
			});
		}

		public void TestRelationShipStageContainsAllFloatingForeignKeys()
		{
			var floatingColumns = LoadFloatingColumns();
			IntegrateFloatingColumnsToRelationships(floatingColumns);

			CombineAssertions(() =>
			{
				foreach (var floatingForeignKeyRelationship in FloatingForeignKeyRelationships)
				{
					AssertFloatingForeignKeyIsInRelationShipStage(floatingForeignKeyRelationship);
				}
			});
		}

		void IntegrateFloatingColumnsToRelationships(List<FloatingColumn> floatingColumns)
		{
			var columnsGroupByTable = floatingColumns.GroupBy(c => c.TableName).ToArray();

			foreach (var group in columnsGroupByTable)
			{
				FloatingColumn parentDefinitionColumn = null;
				var floatingForeignKeyColumns = new List<FloatingColumn>();
				foreach (var floatingColumn in group)
				{
					if (floatingColumn.ColumnName.EndsWith("ParentTableCode"))
					{
						parentDefinitionColumn = floatingColumn;
					}
					else
					{
						floatingForeignKeyColumns.Add(floatingColumn);
					}
				}

				if (parentDefinitionColumn != null)
				{
					var candidateParentTableList = parentDefinitionColumn.GetCandidateListFromCheckConstraint();
					foreach (var column in floatingForeignKeyColumns)
					{
						FloatingForeignKeyRelationships.Add(new FloatingForeignKeyRelationship
						{
							TableName = column.TableName,
							ColumnName = column.ColumnName,
							CandidateParentTableList = candidateParentTableList
						});
					}
				}
			}
		}

		void AssertFloatingForeignKeyIsInRelationShipStage(FloatingForeignKeyRelationship relationship)
		{
			var candidateCodeList = relationship.CandidateParentTableList;

			foreach (var candidateCode in candidateCodeList)
			{
				var parentTableName = GetTableNameByCode(candidateCode);
				RelationshipStage.RelationshipMap.TryGetValue(parentTableName, out var relationshipList);
				Assert($"Please add the relationship between the floating foreign key {relationship.TableName}.{relationship.ColumnName} and the parent table {parentTableName}, to {RelationshipStage.GetType()}.",
					relationshipList?.Exists(r => r.ChildFkColumn.Name == relationship.ColumnName) ?? false);
			}
		}

		List<FloatingColumn> LoadFloatingColumns()
		{
			var sql = $@"
WITH CTE_FloatingForeignKeysQuery AS
(
	SELECT 
		Columns.column_id AS ColumnID,
		Columns.name AS ColumnName,
		Tables.name AS TableName,
		Tables.object_id AS TableObjectID
	FROM
		sys.columns AS Columns
	INNER JOIN
		sys.tables AS Tables
		ON Columns.object_id = Tables.object_id
	WHERE
		(
			{TableNameSqlCondition()}
		)
		AND
		Columns.name LIKE '%Parent%'
		AND
		Tables.schema_id = SCHEMA_ID('dbo')
		AND
		Tables.type = 'U'
)

SELECT 
	CheckConstraints.name AS ConstraintName,
	CheckConstraints.definition AS ConstraintDefinition,
	FloatingForeignKeys.TableName AS TableName,
	FloatingForeignKeys.ColumnName AS ColumnName
FROM
	CTE_FloatingForeignKeysQuery AS FloatingForeignKeys
LEFT JOIN 
	sys.check_constraints AS CheckConstraints 
	ON CheckConstraints.parent_object_id = FloatingForeignKeys.TableObjectID
		AND CheckConstraints.parent_column_id = FloatingForeignKeys.ColumnID
ORDER BY FloatingForeignKeys.TableName;
";
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			var floatingForeignKeys = new List<FloatingColumn>();
			foreach (DataRow row in dataTable.Rows)
			{
				floatingForeignKeys.Add(new FloatingColumn
				{
					ColumnName = row["ColumnName"].ToString(),
					TableName = row["TableName"].ToString(),
					ConstraintName = row["ConstraintName"]?.ToString(),
					ConstraintDefinition = row["ConstraintDefinition"]?.ToString()
				});
			}
			return floatingForeignKeys;
		}

		string TableNameSqlCondition()
		{
			var prefixPatterns = TablePrefixes.Select(prefix => $"Tables.name LIKE '{prefix}%'").ToArray();
			return string.Join(" OR ", prefixPatterns);
		}

		string GetTableNameByCode(string code)
		{
			if (!tableCodeDict.TryGetValue(code, out var tableName))
			{
				var sql = $@"
SELECT TOP 1
	Tables.name AS TableName
FROM
	sys.columns AS Columns
INNER JOIN
	sys.tables AS Tables
	ON Columns.object_id = Tables.object_id
WHERE
	Columns.name = '{code}_PK'
	AND
	Tables.schema_id = SCHEMA_ID('dbo')
	AND
	Tables.type = 'U'
";
				tableName = TestConnection.ExecuteScalar<string>(sql);
				Assert($"Can't get table name of code {code}, please add it to tableCodeDict manually in SetUp.", !string.IsNullOrEmpty(tableName));
				tableCodeDict[code] = tableName;
			}

			return tableName;
		}

		readonly Dictionary<string, string> tableCodeDict = new();

		record FloatingColumn
		{
			public string ColumnName { get; init; }

			public string TableName { get; init; }

			public string ConstraintName { get; init; }

			public string ConstraintDefinition { get; init; }

			public List<string> GetCandidateListFromCheckConstraint()
			{
				var candidateList = new List<string>();
				if (string.IsNullOrEmpty(ConstraintDefinition))
				{
					return candidateList;
				}

				var pattern = @$"\[{ColumnName}\]='(\w+)'";
				var matches = Regex.Matches(ConstraintDefinition, pattern);

				foreach (Match match in matches)
				{
					candidateList.Add(match.Groups[1].Value);
				}

				return candidateList;
			}
		}

		protected record FloatingForeignKeyRelationship
		{
			public string ColumnName { get; init; }

			public string TableName { get; init; }

			public List<string> CandidateParentTableList { get; init; }
		}
	}
}
