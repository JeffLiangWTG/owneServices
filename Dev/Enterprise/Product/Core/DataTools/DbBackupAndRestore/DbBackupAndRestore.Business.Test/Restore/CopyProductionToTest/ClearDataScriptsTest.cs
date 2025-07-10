using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Testing.Restore.CopyProductionToTest
{
	class ClearDataScriptsTest : TestCase
	{
		public void TestCleanupScriptBuilder_DeleteStatement()
		{
			var builder = new ClearDataScriptBuilder(Db.DatabaseName);
			builder.DeleteRecords().From("MyTable").Where("MyField = 'MyValue'");
			var sql = "\n" + builder.Build().Trim();
			AssertEqualsIgnoreLineBreaks("Unexpected query built by DeleteStatementBuilder", $@"
IF EXISTS(SELECT null FROM [{Db.DatabaseName}].sys.tables WHERE name = 'MyTable')
BEGIN
	DELETE FROM [{Db.DatabaseName}]..MyTable WHERE MyField = 'MyValue'
END"
			, sql);
		}

		public void TestCleanupScriptBuilder_UpdateStatement()
		{
			var builder = new ClearDataScriptBuilder(Db.DatabaseName);
			builder
				.UpdateRecords().From("MyTable")
				.Set("MyField", "'NewValue'")
				.Where("MyField = 'MyValue'");
			var sql = builder.Build();
			AssertEqualsIgnoreLineBreaks("Unexpected query built by UpdateStatementBuilder", $@"
IF EXISTS(SELECT null FROM [{Db.DatabaseName}].sys.tables WHERE name = 'MyTable')
BEGIN
	UPDATE [{Db.DatabaseName}]..MyTable SET MyField = 'NewValue' WHERE MyField = 'MyValue'
END"
			, sql);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestClearDataScripts_AllScripts_CanRunOnLatestSchema()
		{
			var scripts = CopyProductionToTestScriptManager.GetClearDataScripts().ToList();
			foreach (var script in scripts)
			{
				var cleanupScriptBuilder = new ClearDataScriptBuilder(Db.DatabaseName);
				cleanupScriptBuilder.AddComment().ForStarting(script);
				script.BuildScript(cleanupScriptBuilder);
				var sql = cleanupScriptBuilder.Build();

				using var connection = Db.NewAdminConnection();
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown($"{script.GetType()} threw an exception when trying to run on the latest schema", () =>
					{
						connection.ExecuteNonQuery(sql);
					});
				});
			}
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestClearDataScripts_AllScripts_CoverForeignKeyConstraints()
		{
			var scripts = CopyProductionToTestScriptManager.GetClearDataScripts().ToList();

			foreach (var script in scripts)
			{
				var cleanupScriptBuilder = new CleanupScriptBuilderForTest(Db.DatabaseName);
				cleanupScriptBuilder.AddComment().ForStarting(script);
				script.BuildScript(cleanupScriptBuilder);

				var deletedTables = new HashSet<string>();
				var updatedFields = new HashSet<(string, string)>();

				foreach (var statement in cleanupScriptBuilder.StatementBuilders)
				{
					if (statement is UpdateStatementBuilder updateStatement)
					{
						foreach (var set in updateStatement.setStatements)
						{
							updatedFields.Add((updateStatement.tableName, set.Field));
						}
					}

					if (statement is DeleteStatementBuilder delStatement)
					{
						deletedTables.Add(delStatement.tableName);
						var sql = $@"
							SELECT 
							   OBJECT_NAME(f.parent_object_id) TableName,
							   COL_NAME(fc.parent_object_id,fc.parent_column_id) ColName
							FROM 
							   sys.foreign_keys AS f
							INNER JOIN 
							   sys.foreign_key_columns AS fc 
								  ON f.OBJECT_ID = fc.constraint_object_id
							INNER JOIN 
							   sys.tables t 
								  ON t.OBJECT_ID = fc.referenced_object_id
							WHERE 
							   OBJECT_NAME (f.referenced_object_id) = '{delStatement.tableName}'
							   AND f.delete_referential_action_desc = 'NO_ACTION'
						";

						Db.NewAdminConnection().ExecuteReader(sql, dr =>
						{
							var referencingTable = dr.GetString(0);
							var referencingField = dr.GetString(1);
							Assert($"Attempting to delete records from '{delStatement.tableName}' in {script.GetType().Name} might fail due to a referencing foreign key from column '{referencingField}' on table '{referencingTable}'. Please make sure the referencing records are removed by updating/deleting records of '{referencingTable}' table."
								, deletedTables.Contains(referencingTable) || updatedFields.Contains((referencingTable, referencingField)));
						});
					}
				}
			}
		}

		class CleanupScriptBuilderForTest : ClearDataScriptBuilder
		{
			public CleanupScriptBuilderForTest(string databaseName) : base(databaseName)
			{
			}

			public List<IStatementBuilder> StatementBuilders => statementBuilders;
		}
	}
}
