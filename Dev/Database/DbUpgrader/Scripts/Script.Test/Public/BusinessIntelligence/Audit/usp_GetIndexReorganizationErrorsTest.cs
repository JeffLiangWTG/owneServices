using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_GetIndexReorganizationErrors))]
	class usp_GetIndexReorganizationErrorsTest : BiCreateScriptTest
	{
		public void TestGetIndexReorganizationErrors()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(ScriptDbName))
			{
				var prepareData = $@"
				INSERT INTO [{BiConstants.BiAdminSchemaName}].[TableState]
					(SourceSchemaName, SourceTableName, IndexReorganizationSqlErrorMessage)
					VALUES
						('dbo', 'Table1', 'An error message'),
						('dbo', 'Table2', ''),
						('dbo', 'Table3', 'A useful error message'),
						('dbo', 'Table4', NULL)
				";
				TestConnection.ExecuteNonQuery(prepareData);

				var sql = $"EXEC [{BiConstants.BiAdminSchemaName}].[usp_GetIndexReorganizationErrors]";
				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sql);

				AssertEquals("Row count should be 2", 2, dataTable.Rows.Count);
				CombineAssertions(() =>
				{
					AssertEquals("Table1", dataTable.Rows[0][1]);
					AssertEquals("Table3", dataTable.Rows[1][1]);
				});
			}
		}

		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}

