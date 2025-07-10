using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__AlternateGLAccountData))]
	internal class vw_CUS__AlternateGLAccountDataTest : BiCreateScriptTest
	{
		public void TestColumns()
		{
			var columns = new[]
			{
				"AlternateGLAccountDataKey", "AlternateGLAccountKey", "GLAccountKey", "CompanyKey", "ParentGLAccountNo", "ParentGLAccountName", "AlternateChartKey",
				"AlternateChartCode", "OriginalAccount", "DestinationAccountNo", "TierLevel", "ReportSection",
				"AccountNumberForTotals", "Description", "AccountTypeCode", "AccountType", "DebitCreditCode",
				"DebitCredit", "AlternateAccountKey", "PercentAccountKey", "ConsolidationAccountKey",
				"HeaderDependsOnTotalKey", "TotalLevel", "PrintSequence", "StatisticalUnits", "Attribute_ORG",
				"Attribute_OCG", "Attribute_LFO", "Attribute_LFE", "Attribute_TIC", "Attribute_SPR", "CashFlowType", "OrganizationKey"
			};

			ExecuteCusTableLoad("InitialLoadQuery");
			var result = SelectRows();
			foreach (string column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Length, result.Columns.Count);
		}

		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestDataForIniLoad();
			ExecuteCusTableLoad("InitialLoadQuery");
			var result = SelectRows();
			AssertEquals("Rowcount", 13, result.Rows.Count);
			AssertEquals(7, result.Select("AlternateChartCode = 'uyy1234567' and AlternateChartKey = 1").Length);
			AssertEquals(6, result.Select("AlternateChartCode = 'uye' and AlternateChartKey = 2").Length);
			AssertEquals(1, result.Select("AlternateChartKey = 1 and AlternateGLAccountKey = 8 and AccountNumberForTotals = '51-11-229' and ReportSection = 'LI' and Attribute_TIC = 'EMA'").Length);
			AssertEquals(1, result.Select("AlternateChartKey = 1 and AlternateGLAccountKey = 9 and AccountNumberForTotals = '61-12-222' and ReportSection = 'AS' and Attribute_TIC = 'EMA'").Length);
			AssertEquals(1, result.Select("AlternateChartKey = 2 and AlternateGLAccountKey = 3 and AccountNumberForTotals = '51,11.224' and ReportSection = 'AS' and Attribute_ORG = 'UUU'").Length);
			AssertEquals(1, result.Select("AlternateChartKey = 2 and AlternateGLAccountKey = 2 and AccountNumberForTotals = '61,11.000' and ReportSection = 'LI' and Attribute_ORG = 'UUU'").Length);
			AssertEquals(1, result.Select("AlternateChartKey = 2 and AlternateGLAccountKey = 6 and AccountNumberForTotals = '31,11.227' and ReportSection = 'AP' and Attribute_OCG = 'INT'").Length);
			AssertEquals(1, result.Select("AlternateChartKey = 1 and AlternateGLAccountKey = 12 and AccountNumberForTotals = '31-15-222' and ReportSection = 'AP' and Attribute_TIC = 'EMA' AND StatisticalUnits = 'UU'").Length);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM [{0}].Finance.BAS__AlternateChart where AlternateChartKey = 1
				INSERT INTO [{0}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES('Finance', 'BAS__AlternateChart', 1)
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 6, result.Rows.Count);
			AssertEquals("Rowcount", 6, result.Select("AlternateChartKey = 2").Length);
			AssertEquals("Rowcount", 0, result.Select("AlternateChartKey = 1").Length);
			var transformedRows = SelectTransformedRows();
			AssertEquals(7, transformedRows.Rows.Count);
			AssertEquals(7, transformedRows.Select("RefValue1 IN (1, 8, 9, 10, 11, 12, 13)").Length);
			TruncateTransformedRows();

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].Finance.BAS__AlternateChartFormat set Separator = '-' where AlternateChartFormatKey = 4
				INSERT INTO [{0}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue, RefValue1)
				VALUES('Finance', 'BAS__AlternateChartFormat', 4, 2)
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 6, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateChartKey = 2 and AlternateGLAccountKey = 3 and AccountNumberForTotals = '51-11.224' and ReportSection = 'AS' and Attribute_ORG = 'UUU'").Length);
			AssertEquals(1, result.Select("AlternateChartKey = 2 and AlternateGLAccountKey = 2 and AccountNumberForTotals = '61-11.000' and ReportSection = 'LI' and Attribute_ORG = 'UUU'").Length);
			transformedRows = SelectTransformedRows();
			AssertEquals(12, transformedRows.Rows.Count);
			AssertEquals(12, transformedRows.Select("KeyValue IN (2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)").Length);
			TruncateTransformedRows();

			TestHelper.InsertAlternateAccountAttribute(2, 2, "LFE", "IEU", 1, 1, organizationID);
			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].Finance.CUS__AlternateGLAccountAttributeInfo SET Attribute_ORG = '', Attribute_LFE = 'IEU', ParentGLAccountNo ='1.2.3.444' where AlternateChartKey = 2 and AlternateGLAccountKey = 2;
				UPDATE [{0}].Finance.CUS__AlternateGLAccountAttributeInfo SET Attribute_ORG = 'YYY' where AlternateChartKey = 2 and AlternateGLAccountKey = 3;
				INSERT INTO [{0}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				VALUES ('Finance', 'CUS__AlternateGLAccountAttributeInfo', 4),
					('Finance', 'CUS__AlternateGLAccountAttributeInfo', 3)
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 6, result.Rows.Count);
			AssertEquals(1, result.Select("AlternateChartKey = 2 and AlternateGLAccountKey = 2 and AccountNumberForTotals = '61-11.000' and ReportSection = 'LI' and Attribute_ORG = '' and Attribute_LFE = 'IEU' and ParentGLAccountNo ='1.2.3.444'").Length);
			AssertEquals(1, result.Select($"AlternateChartKey = 2 and AlternateGLAccountKey = 3 and Attribute_ORG = 'YYY'").Length);
			transformedRows = SelectTransformedRows();
			AssertEquals(4, transformedRows.Rows.Count);
			AssertEquals(4, transformedRows.Select("KeyValue IN (8, 9, 11, 14, 15)").Length);
			TruncateTransformedRows();

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].Finance.BAS__AlternateGLAccount SET AccountNum ='888444' WHERE AlternateGLAccountKey = 2
				INSERT INTO [{0}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue, RefValue1, RefValue2)
				VALUES
					('Finance', 'BAS__AlternateGLAccount', 2, 2, 2)
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 6, result.Rows.Count);
			AssertEquals(1, result.Select("DestinationAccountNo ='8-88.444'").Length);
			transformedRows = SelectTransformedRows();
			AssertEquals(2, transformedRows.Rows.Count);
			AssertEquals(1, transformedRows.Select("KeyValue IN (16)").Length);
		}

		public void TestAlternateGLAccountWithGlobalChart()
		{
			TestHelper.InsertALternateChart("uyr", isGlobal: 1);
			TestHelper.InsertALternateChartFormat("9", 1, "-", 1);
			TestHelper.InsertALternateChartFormat("99", 1, "-", 2);
			TestHelper.InsertGLAccount("1.2.3.444");
			TestHelper.InsertAlternateGLAccount("111", "BSH");
			TestHelper.InsertCUSAlternateAccountAttributeInfo(1, 1, 1, 1, "", "INT", attributeTIC: "EMA");
			ExecuteCusTableLoad("InitialLoadQuery");
			var result = SelectRows();
			AssertEquals("Rowcount", 1, result.Rows.Count);
			AssertEquals(1, result.Select("CompanyKey is null and Attribute_OCG = 'INT' and Attribute_TIC = 'EMA' and Attribute_ORG ='' and AlternateChartKey = 1").Length);
		}

		void PrepareTestDataForIniLoad()
		{
			TestHelper.InsertALternateChart("uyy1234567");
			TestHelper.InsertALternateChart("uye", isFixedLength: 0, balanceSheetStyle: "EAL");
			TestHelper.InsertALternateChartFormat("9", 1, "-", 1);
			TestHelper.InsertALternateChartFormat("99", 1, "-", 2);
			TestHelper.InsertALternateChartFormat("999", 1, "-", 3);
			TestHelper.InsertALternateChartFormat("9", 2, ",", 1);
			TestHelper.InsertALternateChartFormat("99", 2, ".", 2);
			TestHelper.InsertALternateChartFormat("999", 2, ",", 3);
			TestHelper.InsertAlternateGLAccount("111222", "BSH");
			TestHelper.InsertAlternateGLAccount("111000", "BSH", 2, 1, reportSection: "LI");
			TestHelper.InsertAlternateGLAccount("111224", "BSH", 2, 1, reportSection: "AS");
			TestHelper.InsertAlternateGLAccount("111225", "BSH", 2, 1, reportSection: "TS");
			TestHelper.InsertAlternateGLAccount("111226", "BSH", 2, 1, reportSection: "OV");
			TestHelper.InsertAlternateGLAccount("111227", "BSH", 2, 1, reportSection: "AP");
			TestHelper.InsertAlternateGLAccount("111228", "BSH", 2, 1, reportSection: "OE");
			TestHelper.InsertAlternateGLAccount("111229", "BSH", 1, 1, reportSection: "LI");
			TestHelper.InsertAlternateGLAccount("112222", "BSH", 1, 1, reportSection: "AS");
			TestHelper.InsertAlternateGLAccount("113222", "P&L", 1, 1, reportSection: "TS");
			TestHelper.InsertAlternateGLAccount("114222", "P&L", 1, 1, reportSection: "OV");
			TestHelper.InsertAlternateGLAccount("115222", "P&L", 1, 1, reportSection: "AP");
			TestHelper.InsertAlternateGLAccount("116222", "P&L", 1, 1, reportSection: "OE");

			TestHelper.InsertCUSAlternateAccountAttributeInfo(1, 1, 8, 1, "UUU", "INT", "", "", "EMA");
			TestHelper.InsertCUSAlternateAccountAttributeInfo(1, 1, 9, 1, "UUU", "INT", "", "", "EMA");
			TestHelper.InsertCUSAlternateAccountAttributeInfo(2, 1, 3, 1, "UUU", "INT", "", "", "EMA");
			TestHelper.InsertCUSAlternateAccountAttributeInfo(2, 1, 2, 1, "UUU", "INT", "", "", "EMA");
			TestHelper.InsertCUSAlternateAccountAttributeInfo(1, 1, 12, 1, "UUU", "INT", "", "", "EMA", units: "UU");
			TestHelper.InsertCUSAlternateAccountAttributeInfo(2, 1, 6, 1, "UUU", "INT", "", "", "EMA");
		}

		void ExecuteCusTableLoad(string sqlName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__AlternateGLAccountData'",
				sqlName, ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		readonly Guid organizationID;

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].CUS__AlternateGLAccountData");
		}

		DataTable SelectTransformedRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[biadmin].TransformedRow where Tablename ='CUS__AlternateGLAccountData'");
		}

		void TruncateTransformedRows()
		{
			DataUtils.GetDataTableFromQuery(TestConnection, $"TRUNCATE TABLE [{ScriptDbName}].[biadmin].TransformedRow");
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
