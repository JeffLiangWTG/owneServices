using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__GeneralLedgerDataAttribute))]
	internal class vw_CUS__GeneralLedgerDataAttributeTest : BiCreateScriptTest
	{
		public void TestColumns()
		{
			var columns = new[]
			{
				"AlternateChartKey",
				"Attribute_LFE",
				"Attribute_LFO",
				"Attribute_OCG",
				"Attribute_ORG",
				"Attribute_SPR",
				"Attribute_TIC",
				"BranchKey",
				"CompanyKey",
				"CompanyOfPeriodKey",
				"DepartmentKey",
				"GeneralLedgerDataKey",
				"GLAccountKey",
				"LocalAmount",
				"LocalCurrency",
				"OrganizationKey",
				"OriginalAttribute_LFE",
				"OriginalAttribute_LFO",
				"OriginalAttribute_OCG",
				"OriginalAttribute_ORG",
				"OriginalAttribute_SPR",
				"OriginalAttribute_TIC",
				"PeriodManagementKey",
				"PostDate",
				"PostPeriodForReportingBook",
				"ReportingBookCode",
				"ReportingBookKey",
				"TransactionCategory",
				"TranslatedCurrency",
				"GeneralLedgerDataAttributeKey"
			};

			var result = SelectRows();
			foreach (string column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Length, result.Columns.Count);
		}

		public void TestViewData()
		{
			AssertResultData(SelectRows("vw_CUS__GeneralLedgerDataAttribute"));
		}

		[ExpectNoExceptions]
		public void TestInitialRun()
		{
			AssertResultData(SelectRows());
		}

		void AssertResultData(DataTable result)
		{
			AssertEquals(8, result.Rows.Count);
			AssertEquals(1, result.Select("GeneralLedgerDataKey = 2 and CompanyKey = 2 and OrganizationKey =2 and PeriodManagementKey = 32 AND ReportingBookCode = 'R12' AND PostPeriodForReportingBook = 202208 AND ReportingBookKey = 2 AND CompanyOfPeriodKey = 2").Length);
			AssertEquals(1, result.Select("Attribute_OCG = 'NAV' AND Attribute_LFO = 'FOR' AND Attribute_LFE = '' AND Attribute_ORG = '' AND Attribute_SPR = 'SPS' AND Attribute_TIC = 'ETI' AND OriginalAttribute_OCG = 'NAV' AND OriginalAttribute_LFO = 'FOR' AND OriginalAttribute_LFE = 'OEU' AND OriginalAttribute_ORG = '' AND OriginalAttribute_SPR = 'SPS' AND OriginalAttribute_TIC = 'ETI' AND GLAccountKey = 2 AND ReportingBookKey = 4").Length);
			AssertEquals(2, result.Select("GeneralLedgerDataKey = 1 and TransactionCategory = 'UYU' AND LocalCurrency = 'CNY' AND TranslatedCurrency = 'USD' AND LocalAmount = 10 AND BranchKey = 1 AND DepartmentKey = 1 AND Attribute_ORG = 'ORG1'").Length);
			AssertEquals(2, result.Select("PostPeriodForReportingBook = 100001 AND PeriodManagementKey is null AND PostDate = '2021-06-08' AND TransactionCategory = ''").Length);
		}

		public void TestIncrementLoadAccTransactionLine()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[biadmin].[TransformedRow];
				UPDATE [{ScriptDbName}].[Finance].[BAS__AccGLTransactionLine] set TaxRateKey = null where AccGLTransactionLineKey = 2
				UPDATE [{ScriptDbName}].[Finance].[CUS__TransactionLineDissectionAttribute] set Attribute_SPR = 'NAU' where TransactionLineDissectionAttributeKey = 1
				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow](SchemaName, TableName, KeyValue, PKValue, RefValue1)
				Values
				('Finance', 'CUS__TransactionLineDissectionAttribute', 1, newID(), 1),
				('Finance', 'BAS__AccGLTransactionLine', 2, newID(), 2)
			");
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();
			var transformedRows = SelectTransformedRows();
			AssertEquals(2, result.Select("GeneralLedgerDataKey = 1 and Attribute_SPR = 'NAU'").Length);
			AssertEquals(2, result.Select("GeneralLedgerDataKey = 2 and Attribute_TIC = 'NAV'").Length);
			AssertEquals(4, transformedRows.Select("RefValue1 in (1,2) ").Length);
		}

		public void TestIncrementLoadOrganization()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[biadmin].[TransformedRow];
				UPDATE [{ScriptDbName}].[Finance].[CUS__OrganizationCompanyData] set Attribute_OCG ='CCU' where OrganizationKey = 2
				UPDATE [{ScriptDbName}].[Finance].[CUS__OrganizationForAttributes] set CountryCode ='US', OrganizationCode = 'UTR' where OrganizationForAttributesKey = 3
				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, RefValue1, RefValue2)
				Values
					('Finance', 'CUS__OrganizationCompanyData', 1, 2, 1),
					('Finance', 'CUS__OrganizationForAttributes', 1, 3, 1)
				");
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();
			var transformedRows = SelectTransformedRows();
			AssertEquals(2, result.Select("OrganizationKey = 3 and Attribute_LFO ='LOC' AND Attribute_ORG ='UTR'").Length);
			AssertEquals(2, result.Select("OrganizationKey = 2 and Attribute_OCG ='CCU'").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 = 4 ").Length);
		}

		public void TestIncrementLoadReportingBook()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[biadmin].[TransformedRow];
				UPDATE [{ScriptDbName}].Finance.CUS__ReportingBookAndPeriodInfo set LocalCurrency ='CCC' where ReportingBookAndPeriodInfoKey =1
				DELETE FROM [{ScriptDbName}].Finance.CUS__ReportingBookAndPeriodInfo WHERE ReportingBookAndPeriodInfoKey = 2
				INSERT [{ScriptDbName}].[Finance].[CUS__ReportingBookAndPeriodInfo]
				([ReportingBookAndPeriodInfoKey], [AlternateChartKey], [CompanyKey], [CompanyOfPeriodKey], [LocalCurrency], [RX_NKCurrency], [ReportingBookKey], [ReportingBookCode], [StartDate], [CountryCode])
				VALUES
					(5, 2, 1, 2, 'AUD', 'USD', 5, 'R15', '2021-09-08', 'CN')

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow](SchemaName, TableName, KeyValue, RefValue2, RefValue3)
				Values
				('Finance', 'CUS__ReportingBookAndPeriodInfo', 1, 1, 1),
				('Finance', 'CUS__ReportingBookAndPeriodInfo', 2, 2, 2),
				('Finance', 'CUS__ReportingBookAndPeriodInfo', 5, 5, 1)
			");
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();
			var transformedRows = SelectTransformedRows();
			AssertEquals(2, result.Select("ReportingBookKey = 1 and LocalCurrency = 'CCC'").Length);
			AssertEquals(0, result.Select("ReportingBookKey = 2").Length);
			AssertEquals(2, result.Select("ReportingBookKey = 5 and LocalCurrency = 'AUD'").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 in (1,2) ").Length);
		}

		public void TestIncrementGeneralData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[Finance].[BAS__GeneralLedgerData] WHERE GLTransactionHeaderKey =3
				UPDATE [{ScriptDbName}].[Finance].[BAS__GeneralLedgerData] set GLTransactionHeaderKey =1 where GeneralLedgerDataKey =4
				INSERT [{ScriptDbName}].[Finance].[BAS__GeneralLedgerData]
					([AccGLTransactionLineKey], [CompanyKey], [GeneralLedgerDataID], [GeneralLedgerDataKey], [GLTransactionHeaderKey], [GLAccountKey], [PostDate], [LocalBalance], [BranchKey], [DepartmentKey], [TaxGLMovementKey])
					VALUES
						(2, 1, newid(), 5, 2, 1, '2022-09-08', 10, 1, 1, 1),
						(2, 1, newid(), 6, 2, 1, '2022-09-08', 10, 1, 1, 2)
				INSERT [{ScriptDbName}].[Finance].[CUS__TaxGLMovementInfo]
					([TaxGLMovementInfoKey], [TaxGLMovementKey], [TransactionTypeCode])
				VALUES (1, 1, 'INV'),
						(2, 2, 'CRD')

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow](SchemaName, TableName, KeyValue, RefValue1)
				VALUES	('Finance', 'BAS__GeneralLedgerData', 4, 'CCU'),
						('Finance', 'BAS__GeneralLedgerData', 5, 'CCU'),
						('Finance', 'BAS__GeneralLedgerData', 6, 'CCU'),
						('Finance', 'BAS__GeneralLedgerData', 3, 'CCU')
			");
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();
			var transformedRows = SelectTransformedRows();
			AssertEquals(2, result.Select("GeneralLedgerDataKey = 4 and Attribute_LFO ='NAV' AND Attribute_LFE ='' AND Attribute_ORG ='NAV' AND OriginalAttribute_LFE ='NAV'").Length);
			AssertEquals(2, result.Select("GeneralLedgerDataKey = 5 and Attribute_ORG ='TPY2' AND Attribute_SPR ='SPS'").Length);
			AssertEquals(2, result.Select("GeneralLedgerDataKey = 6 and Attribute_ORG ='TPY2' AND Attribute_SPR ='SPR'").Length);
			AssertEquals(4, transformedRows.Select("RefValue1 in(4, 3) ").Length);
		}

		public void TestIncrementAlternateGLAccountDissection()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountDissection] set Attribute_ORG = null, Attribute_OCG = '0', Attribute_TIC = '0' where AlternateGLAccountDissectionKey = 1

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow](SchemaName, TableName, KeyValue, RefValue1, RefValue2)
				Values
				('Finance', 'CUS__AlternateGLAccountDissection', 1, 1, 1)
			");
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();
			var transformedRows = SelectTransformedRows();
			AssertEquals(2, result.Select("GeneralLedgerDataKey = 1 and CompanyKey = 1 and OrganizationKey =1 and Attribute_LFO ='FOR' and Attribute_ORG = '' and Attribute_OCG = '' and Attribute_TIC = ''").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 = 1").Length);
		}

		public void TestIncrementPeriodMangement()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				UPDATE [{ScriptDbName}].[Finance].[BAS__PeriodManagement] set StartDate = '2020-01-01' where PeriodManagementKey = 25

				INSERT INTO [{ScriptDbName}].[biadmin].[TransformedRow](SchemaName, TableName, KeyValue, RefValue1, RefValue2)
				Values
				('Finance', 'BAS__PeriodManagement', 25, 1, 1)
			");
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteCusTableLoad("IncrementalLoadQuery");
			var result = SelectRows();
			var transformedRows = SelectTransformedRows();
			AssertEquals(2, result.Select("GeneralLedgerDataKey = 4 and CompanyKey = 2 and OrganizationKey =3 and PeriodManagementKey = 25 and PostPeriodForReportingBook = 202201").Length);
			AssertEquals(4, transformedRows.Select("KeyValue >= 7").Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareData();
			ExecuteCusTableLoad("InitialLoadQuery");
		}

		void PrepareData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[BAS__GeneralLedgerData]
					([AccGLTransactionLineKey], [CompanyKey], [GeneralLedgerDataID], [GeneralLedgerDataKey], [GLTransactionHeaderKey], [GLAccountKey], [PostDate], [LocalBalance], [BranchKey], [DepartmentKey], [TaxGLMovementKey])
					VALUES
						(1, 1, newid(), 1, 1, 1, '2022-09-08', 10, 1, 1, 1),
						(2, 2, NEWID(), 2, 2, 2, '2022-08-08', 12, 1, 1, null),
						(3, 1, newid(), 3, 3, 3, '2022-07-08', 14, 1, 1, null),
						(4, 2, NEWID(), 4, 4, 4, '2021-06-08', 16, 1, 1, null)

				INSERT [{ScriptDbName}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey], [GLTransactionHeaderID], [LedgerCode], [OrganizationHeaderKey], [TransactionCategory], [TransactionTypeCode], [TransactionBelongsToGroup], [IsCancelled])
					VALUES
						(1, newid(), 'GL', 1, 'UYU', 'INV', null, 0),
						(2, newid(), 'AP', 1, 'UY1', 'INV', null, 0),
						(3, newID(), 'AP', 2, 'UY2', 'DRC', newID(), 1),
						(4, newid(), 'AP', 3, 'UY3', 'CRD', null, 0),
						(5, newid(), 'AP', 4, 'UY4', 'DPY', null, 0)

				INSERT [{ScriptDbName}].[Finance].[BAS__AccGLTransactionLine]
					([AccGLTransactionLineKey], [AccGLTransactionLineID], [GLTransactionHeaderKey], [LineType], [OrganizationKey], [TaxRateKey], [TaxExtraRateNumerator])
					VALUES
						(1, newid(), 1, 'GJL', 1, 1, 2),
						(2, newid(), 2, 'WIP', 2, 1, 2),
						(3, newid(), 3, 'CST', null, 1, 0),
						(4, newid(), 4, 'CST', 3, null, 2)
				INSERT [{ScriptDbName}].[Finance].[CUS__ReportingBookAndPeriodInfo]
					([ReportingBookAndPeriodInfoKey], [AlternateChartKey], [CompanyKey], [CompanyOfPeriodKey], [LocalCurrency], [RX_NKCurrency], [ReportingBookKey], [ReportingBookCode], [StartDate], [CountryCode])
					VALUES
						(1, 1, 1, 2, 'CNY', 'USD', 1, 'R11', '2021-09-08', 'CN'),
						(2, 1, 2, 2, 'CNY', 'USD', 2, 'R12', '2022-09-08', 'US'),
						(3, 1, 1, 2, 'CNY', 'USD', 3, 'R13', '2022-09-08', 'CN'),
						(4, 1, 2, 2, 'CNY', 'USD', 4, 'R14', '2023-09-08', 'US')

				INSERT [{ScriptDbName}].[Finance].[CUS__OrganizationCompanyData]
					([OrganizationCompanyDataKey], [CompanyKey], [OrganizationKey], [Attribute_OCG])
					VALUES
						(1, 1, 1, 'INT'),
						(2, 1, 2, 'TPY'),
						(3, 1, 3, 'TPY'),
						(4, 1, 4, 'TPY')

				INSERT [{ScriptDbName}].[Finance].[CUS__OrganizationForAttributes]
					([OrganizationForAttributesKey], [OrganizationKey], [OrganizationCode], [Countrycode], [EconomicGrouping])
					VALUES
						(1, 1, 'INT1', 'CN', 'EUN'),
						(2, 2, 'TPY2', 'CN', ''),
						(3, 3, 'TPY3', 'CN', ''),
						(4, 4, 'TPY4', 'CN', 'EUN')

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountDissection]
					([AlternateGLAccountDissectionKey], [AlternateChartKey], [GLAccountKey], [Attribute_SPR], [Attribute_ORG], [Attribute_OCG], [Attribute_TIC], [Attribute_LFO], [Attribute_LFE])
					VALUES
						(1, 1, 1, '1', '1' ,'1','1', '1', '0'),
						(2, 1, 2, '1', null,'1','1', '1', '0'),
						(3, 1, 3, '1', '1','1' ,'1', '1', '0'),
						(4, 1, 4, '1', '1',null,'1', '1', '0')
				INSERT [{ScriptDbName}].Finance.CUS__TransactionLineDissectionAttribute
					([TransactionLineDissectionAttributeKey], [AccGLTransactionLineKey], [Attribute_SPR], [Attribute_ORG], [Attribute_OCG], [Attribute_TIC], [Attribute_LFO], [Attribute_LFE])
					VALUES
						(1, 1, 'SPS', 'ORG1' ,'INT','STI', 'FOR', 'LOC')
				"
			);

			TestConnection.ExecuteNonQuery(sqlText);

			TestHelper.InsertPeriodForInputYear(2022);
			TestHelper.InsertPeriodForInputYear(2021);
			TestHelper.InsertPeriodForInputYear(2022, 2);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void ExecuteCusTableLoad(string sqlName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__GeneralLedgerDataAttribute'",
				sqlName, ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		DataTable SelectRows(string tableName = "CUS__GeneralLedgerDataAttribute")
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].{tableName}");
		}

		DataTable SelectTransformedRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[biadmin].TransformedRow where Tablename ='CUS__GeneralLedgerDataAttribute'");
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
