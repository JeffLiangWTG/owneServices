using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__ConsolidatedAccountingCategory))]
	internal class vw_GRP__ConsolidatedAccountingCategoryTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestDataForIniLoad();
			ExecuteCusTableIniLoad();
			var result = SelectRows();
			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("Code = 'XX1' and Description = 'Undefined' and Class = 'TPY'").Length);

			var viewResult = SelectRows("vw_GRP__ConsolidatedAccountingCategory");
			AssertEquals("ViewRowcount", 1, viewResult.Rows.Count);
			AssertEquals(1, viewResult.Select("Code = 'XX1' and Description = 'Undefined' and Class = 'TPY'").Length);

			PrepareTestDataForIncLoad();
			ExecuteCusTableIncLoad();
			result = SelectRows();
			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("Code = 'XX1' and Description = 'Undefined' and Class = 'TPY'").Length);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			INSERT INTO {0}.[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES('Finance', 'BAS__AccountStmData', 1)
			", ScriptDbName);

			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableIncLoad();
			result = SelectRows();
			var transformedRows = SelectTransformedRows();
			AssertEquals("Rowcount", 2, result.Rows.Count);
			AssertEquals("Transformed Rowcount", 3, transformedRows.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("Code = 'XXX' and Description = 'Undefined' and Class = 'TPO'").Length);
				AssertEquals(1, result.Select("Code = 'NON' and Description = 'Non Cash' and Class = 'INT'").Length);
				AssertEquals(3, transformedRows.Select("RefValue1 in ('XXX','NON','XX1')").Length);
			});
		}

		void PrepareTestDataForIniLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				DELETE FROM [{0}].Finance.BAS__AccountStmData

				INSERT [{0}].Finance.BAS__AccountStmData
					([AccountStmDataID], [AccountStmDataKey], [BinaryValue],[SDName])
					VALUES
						(NEWID(), 1, convert(varbinary(max), N'<ArrayOfConsolidatedAccountingCategoryItem xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><ConsolidatedAccountingCategoryItem><Code>XX1</Code><Description>Undefined</Description><Group>TPY</Group></ConsolidatedAccountingCategoryItem></ArrayOfConsolidatedAccountingCategoryItem>'), 'ConsolidatedAccountingCategory')", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void PrepareTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				DELETE FROM [{0}].Finance.BAS__AccountStmData

				INSERT [{0}].Finance.BAS__AccountStmData
					([AccountStmDataID], [AccountStmDataKey], [BinaryValue],[SDName])
					VALUES
						(NEWID(), 1, convert(varbinary(max), N'<ArrayOfConsolidatedAccountingCategoryItem xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><ConsolidatedAccountingCategoryItem><Code>XXX</Code><Description>Undefined</Description><Group>TPO</Group></ConsolidatedAccountingCategoryItem><ConsolidatedAccountingCategoryItem><Code>NON</Code><Description>Non Cash</Description><Group>INT</Group></ConsolidatedAccountingCategoryItem></ArrayOfConsolidatedAccountingCategoryItem>'), 'ConsolidatedAccountingCategory')", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void ExecuteCusTableIniLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT InitialLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__ConsolidatedAccountingCategory'",
				ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		void ExecuteCusTableIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT IncrementalLoadQuery FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__ConsolidatedAccountingCategory'",
				ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		DataTable SelectRows(string tableName = "GRP__ConsolidatedAccountingCategory")
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].{tableName}");
		}

		DataTable SelectTransformedRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[biadmin].TransformedRow where Tablename ='GRP__ConsolidatedAccountingCategory'");
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
