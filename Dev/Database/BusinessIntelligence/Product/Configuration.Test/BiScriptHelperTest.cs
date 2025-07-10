using System.Text;
using NUnit.Framework;

namespace CargoWise.Bi.Configuration.Test
{
	class BiScriptHelperTest : TestCase
	{
		public void TestGenerateAuditTableDefinition()
		{
			var dataSet = new DataSets.BiAutomationConfigDataSet();
			var cdcTableConfig = dataSet.CdcTableConfig.AddCdcTableConfigRow("TestSchema", "TestTable", "", "", "", true, false, "", "", "", false);
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn1", "int", 9, 8, 7, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn2", "decimal", 9, 8, 7, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn3", "varchar", 9, 8, 7, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn4", "xml", -1, 8, 7, true, false, "", "", true, "", "", true, false, "");
			var auditTableDefinition = BiScriptHelper.GenerateAuditTableDefinition(cdcTableConfig);
			AssertEquals(
@"CREATE TABLE [TestSchema].[TestTable]
(
	[__$start_lsn] binary(10) NOT NULL
	,[__$seqval] binary(10) NOT NULL
	,[__$operation] int NOT NULL
	,[__$update_mask] varbinary(128) NOT NULL
	,[__$lsn_period] smallint NOT NULL
	,[__$command_id] int NOT NULL DEFAULT 0
	,[TestColumn1] int NULL
	,[TestColumn2] decimal(8,7) NULL
	,[TestColumn3] varchar(9) NULL
	,[TestColumn4] nvarchar(max) NULL

 ) ON [PRIMARY];

CREATE CLUSTERED COLUMNSTORE INDEX [cci_TestSchema_TestTable] ON [TestSchema].[TestTable] WITH (DROP_EXISTING = OFF);

", auditTableDefinition);
		}

		public void TestGenerateAuditTableDefinitionDateTimeOffset()
		{
			var dataSet = new DataSets.BiAutomationConfigDataSet();
			var cdcTableConfig = dataSet.CdcTableConfig.AddCdcTableConfigRow("TestSchema", "TestTable", "", "", "", true, false, "", "", "", false);
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn1", "datetimeoffset", 10, 34, 7, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn2", "datetimeoffset", 8, 26, 0, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn3", "datetimeoffset", 8, 28, 1, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn4", "datetimeoffset", 8, 29, 2, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn5", "datetimeoffset", 9, 30, 3, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn6", "datetimeoffset", 9, 31, 4, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn7", "datetimeoffset", 10, 32, 5, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn8", "datetimeoffset", 10, 33, 6, true, true, "", "", true, "", "", true, false, "");
			var addColumnsDefinition = BiScriptHelper.GenerateAuditTableDefinition(cdcTableConfig);
			AssertEquals(
@"CREATE TABLE [TestSchema].[TestTable]
(
	[__$start_lsn] binary(10) NOT NULL
	,[__$seqval] binary(10) NOT NULL
	,[__$operation] int NOT NULL
	,[__$update_mask] varbinary(128) NOT NULL
	,[__$lsn_period] smallint NOT NULL
	,[__$command_id] int NOT NULL DEFAULT 0
	,[TestColumn1] datetimeoffset NULL
	,[TestColumn2] datetimeoffset(0) NULL
	,[TestColumn3] datetimeoffset(1) NULL
	,[TestColumn4] datetimeoffset(2) NULL
	,[TestColumn5] datetimeoffset(3) NULL
	,[TestColumn6] datetimeoffset(4) NULL
	,[TestColumn7] datetimeoffset(5) NULL
	,[TestColumn8] datetimeoffset(6) NULL

 ) ON [PRIMARY];

CREATE CLUSTERED COLUMNSTORE INDEX [cci_TestSchema_TestTable] ON [TestSchema].[TestTable] WITH (DROP_EXISTING = OFF);

", addColumnsDefinition);
		}

		public void TestGenerateAuditStartLsnIndexDefinition()
		{
			var auditStartLsnIndexDefinition = BiScriptHelper.GenerateAuditStartLsnIndexDefinition("TestSchema", "TestTable", "PkName");
			AssertEquals(
@"CREATE NONCLUSTERED INDEX IX_TestTable_StartLsn ON [TestSchema].[TestTable] (__$start_lsn,__$command_id,__$seqval,__$operation) INCLUDE (__$update_mask,PkName) WITH (DATA_COMPRESSION = PAGE);
", auditStartLsnIndexDefinition);
		}

		public void TestGenerateAuditStartLsnIndexDefinition_NoPrimaryKey()
		{
			var auditStartLsnIndexDefinition = BiScriptHelper.GenerateAuditStartLsnIndexDefinition("TestSchema", "TestTable", null);
			AssertEquals(
@"CREATE NONCLUSTERED INDEX IX_TestTable_StartLsn ON [TestSchema].[TestTable] (__$start_lsn,__$command_id,__$seqval,__$operation) INCLUDE (__$update_mask) WITH (DATA_COMPRESSION = PAGE);
", auditStartLsnIndexDefinition);
		}

		public void TestGenerateAuditTableAddColumnsDefinition()
		{
			var dataSet = new DataSets.BiAutomationConfigDataSet();
			var cdcTableConfig = dataSet.CdcTableConfig.AddCdcTableConfigRow("TestSchema", "TestTable", "", "", "", true, false, "", "", "", false);
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn1", "int", 9, 8, 7, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn2", "decimal", 9, 8, 7, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn3", "varchar", 9, 8, 7, true, true, "", "", true, "", "", true, false, "");
			dataSet.CdcColumnConfig.AddCdcColumnConfigRow(cdcTableConfig, "TestColumn4", "xml", -1, 8, 7, true, false, "", "", true, "", "", true, false, "");
			var addColumnsDefinition = BiScriptHelper.GenerateAuditTableAddColumnsDefinition(cdcTableConfig, new[] { "TestColumn1", "TestColumn2", "TestColumn3", "TestColumn4" });
			AssertEquals(
@"ALTER TABLE [TestSchema].[TestTable] ADD
	[TestColumn1] int NULL,
	[TestColumn2] decimal(8,7) NULL,
	[TestColumn3] varchar(9) NULL,
	[TestColumn4] nvarchar(max) NULL;", addColumnsDefinition);
		}

		public void TestContainsIllegalQueryElements_Null()
		{
			var tableMessage = new StringBuilder();
			var expression = string.Empty;
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);
			var expected = @"Expression cannot be empty.
";
			AssertEquals(expected, tableMessage.ToString());
		}

		public void TestContainsIllegalQueryElements_Hints()
		{
			var tableMessage = new StringBuilder();

			var expression = "employees e JOIN employer em ON e.Col1 = em.Col1 with (nolock)";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			expression = "employees e JOIN employer em ON e.Col1 = em.Col1 WITH (nolock)";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			expression = @"
				employees e
				JOIN employer em
				ON e.Col1 = em.Col1
				WITH (nolock)
			";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			var expected = @"Hints are not allowed. Remove WITH expression.
Hints are not allowed. Remove WITH expression.
Hints are not allowed. Remove WITH expression.
";
			AssertEquals(expected, tableMessage.ToString());
		}

		public void TestContainsIllegalQueryElements_Coalesce()
		{
			var tableMessage = new StringBuilder();
			var expression = "employees e JOIN employer em ON ISNULL(e.Col1, 0) = ISNULL(em.Col1, 0)";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			expression = "employees e JOIN employer em ON COALESCE(e.Col1, 0) = COALESCE(em.Col1, 0)";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			expression = @"
				employees e
				JOIN employer em
				ON ISNULL(e.Col1, 0) = ISNULL(em.Col1, 0)
			";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			var expected = @"Use CASE END for joins instead of ISNULL() or COALESCE.
Use CASE END for joins instead of ISNULL() or COALESCE.
Use CASE END for joins instead of ISNULL() or COALESCE.
";
			AssertEquals(expected, tableMessage.ToString());
		}

		public void TestContainsIllegalQueryElements_SubQuery()
		{
			var tableMessage = new StringBuilder();
			var expression = @"
				SELECT *
				FROM table
				WHERE department_id = (
					 SELECT department_id
					 FROM departments
					 WHERE department_name = 'Engineering'
				);
			";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			expression = @"
				SELECT *
				FROM table
				WHERE department_id = (SELECT department_id FROM departments WHERE department_name = 'Engineering');
			";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			expression = @"
[Workflow].[BAS__IncidentMain] as incident
LEFT JOIN (SELECT department_id FROM departments WHERE department_name = 'Engineering') as request
ON request.[IncidentRequestKey] = incident.[RequestKey]
";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			expression = @"
				table
				WHERE department_id = (
					SELECT department_id
					FROM departments
					WHERE department_name = 'Engineering'
				);
			";
			BiScriptHelper.ContainsIllegalQueryElements(tableMessage, expression);

			var expected = @"Subqueries are not supported.
Subqueries are not supported.
Subqueries are not supported.
Subqueries are not supported.
";
			AssertEquals(expected, tableMessage.ToString());
		}
	}
}
