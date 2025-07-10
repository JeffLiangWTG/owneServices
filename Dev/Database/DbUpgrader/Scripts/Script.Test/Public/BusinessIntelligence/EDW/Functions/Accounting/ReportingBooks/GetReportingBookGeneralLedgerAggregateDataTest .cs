using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetReportingBookGeneralLedgerAggregateData))]
	internal class GetReportingBookGeneralLedgerAggregateDataTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			var resultsTable = Execute(CompanyPK1, ReportingBookPK1);
			var columns = GetColumns();
			var columnCount = 0;
			CombineAssertions(() =>
			{
				foreach (DataColumn column in resultsTable.Columns)
				{
					var columnExists = columns.Any(x => x == column.ToString());
					AssertEquals($"Expected {column} to exist in existing list of columns", columnExists, true);
					if (columnExists)
					{
						columnCount++;
					}
				}
			});
			AssertEquals(columns.Count, columnCount);

			PrepareData();
			resultsTable = Execute(CompanyPK1, ReportingBookPK1, branchList: "UUU", departmentList: "DDD,UUD");
			AssertEquals(4, resultsTable.Rows.Count);
			AssertEquals(1, resultsTable.Select("CompanyKey = 1 and BranchKey = 1 and DepartmentKey = 1 and GLAccountNum = '7651' and GLAccountName = 'GLAccount1' and Attribute_ORG = 'INTT1' and Attribute_LFO = 'LOC' and Attribute_LFE = 'WEU' and Attribute_TIC = 'STI' and Attribute_SPR = 'SPR' and Attribute_OCG = 'TPY' and AlternateGLAccount = '899989'").Length);
			AssertEquals(1, resultsTable.Select("GLAmountLocalCredit = 3 and GLAmountLocalDebit = 4 and GLAmountLocalBalance = 1 and TranslatedCredit = 3 and TranslatedDebit = 4 and TranslatedBalance =1 and TranslatedExchangeRate = 1 and CurrencyTranslationLevel = 'BAL' and TranslatedRateType = 'SEL' and TransactionCategory = 'EET' and ReportingBookCode = 'RRB' and ReportingBookKey = 1 and LocalCurrency = 'CNY' and TranslatedCurrency = 'CNY'").Length);
			AssertEquals(2, resultsTable.Select("LocalCurrency = 'CNY' and TranslatedCurrency = 'USD' and TranslatedCredit IS NULL and TranslatedDebit IS NULL and TranslatedBalance IS NULL and TranslatedExchangeRate = 2 and CurrencyTranslationLevel = 'BAL' and TranslatedRateType ='SEL'").Length);

			resultsTable = Execute(CompanyPK2, ReportingBookPK2, startPeriod: 202401, endPeriod: 202407);
			AssertEquals(1, resultsTable.Rows.Count);
			AssertEquals(1, resultsTable.Select("CompanyKey = 2 and ReportingBookKey = 2 and GLAmountLocalCredit = 2 and GLAmountLocalDebit = 3 and GLAmountLocalBalance = 1 and PostPeriod = 202405 and TransactionCategory = '' and AlternateChartKey = 2 and PeriodManagementKey = 1").Length);
			AssertEquals(1, resultsTable.Select("OriginalAttribute_ORG = 'INTT3' and OriginalAttribute_LFO = 'LOC' and OriginalAttribute_LFE = 'WEU' and OriginalAttribute_TIC = 'STI' and OriginalAttribute_SPR = 'SPR' and OriginalAttribute_OCG = 'TPY' and AccountTypeCode = 'BSH'").Length);

			resultsTable = Execute(CompanyPK1, ReportingBookPK1, includePLAppropriationAccount: 1);
			AssertEquals(5, resultsTable.Rows.Count);
			AssertEquals(1, resultsTable.Select("BranchKey IS NULL AND GLAccountKey = 6 and GLAccountNum = '7656' and GLAccountName = 'GLAccount6' and AlternateGLAccountKey IS NULL").Length);

			resultsTable = Execute(CompanyPK1, ReportingBookPK1, startPeriod: 0, endPeriod: 0, startDate: "2023-04-09", endDate: "2023-06-08");
			AssertEquals(2, resultsTable.Rows.Count);
			AssertEquals(1, resultsTable.Select("PostPeriod = 202306 and TranslatedCredit = 1 and TranslatedDebit = 3 and TranslatedBalance = 2 and CurrencyTranslationLevel = 'JNL' and GLAccountKey = 5 and DepartmentKey = 2").Length);
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>();

			columns.Add("BranchKey");
			columns.Add("CompanyKey");
			columns.Add("LocalCurrency");
			columns.Add("TranslatedCurrency");
			columns.Add("DepartmentKey");
			columns.Add("GLAccountKey");
			columns.Add("GLAmountLocalCredit");
			columns.Add("GLAmountLocalDebit");
			columns.Add("GLAmountLocalBalance");
			columns.Add("PostPeriod");
			columns.Add("TransactionCategory");
			columns.Add("ReportingBookCode");
			columns.Add("ReportingBookKey");
			columns.Add("CompanyOfPeriodKey");
			columns.Add("Attribute_TIC");
			columns.Add("Attribute_SPR");
			columns.Add("Attribute_ORG");
			columns.Add("Attribute_OCG");
			columns.Add("Attribute_LFE");
			columns.Add("Attribute_LFO");
			columns.Add("OriginalAttribute_TIC");
			columns.Add("OriginalAttribute_SPR");
			columns.Add("OriginalAttribute_ORG");
			columns.Add("OriginalAttribute_OCG");
			columns.Add("OriginalAttribute_LFE");
			columns.Add("OriginalAttribute_LFO");
			columns.Add("AlternateGLAccount");
			columns.Add("AlternateGLAccountKey");
			columns.Add("AccountTypeCode");
			columns.Add("GLAccountName");
			columns.Add("GLAccountNum");
			columns.Add("AlternateChartKey");
			columns.Add("PeriodManagementKey");
			columns.Add("TranslatedCredit");
			columns.Add("TranslatedDebit");
			columns.Add("TranslatedBalance");
			columns.Add("TranslatedExchangeRate");
			columns.Add("CurrencyTranslationLevel");
			columns.Add("TranslatedRateType");
			return columns;
		}

		#region IncludeChildPresentation

		public void TestIncludePresentationJournalsOfReportingBook()
		{
			PrepareData();
			var sql = string.Format(CultureInfo.InvariantCulture, $@"UPDATE {ScriptDbName}.[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = ''");
			TestConnection.ExecuteNonQuery(sql);

			var resultsTable = Execute(CompanyPK1, ReportingBookPK1);
			AssertEquals(1, resultsTable.Rows.Count);
			AssertEquals(0, resultsTable.Select("TransactionCategory = 'EET'").Length);
			AssertEquals(1, resultsTable.Select("TransactionCategory = ''").Length);

			sql = string.Format(CultureInfo.InvariantCulture, $@"UPDATE {ScriptDbName}.[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = 'OUT'");
			TestConnection.ExecuteNonQuery(sql);

			resultsTable = Execute(CompanyPK1, ReportingBookPK1);
			AssertEquals(1, resultsTable.Rows.Count);
			AssertEquals(0, resultsTable.Select("TransactionCategory = 'EET'").Length);
			AssertEquals(1, resultsTable.Select("TransactionCategory = ''").Length);

			sql = string.Format(CultureInfo.InvariantCulture, $@"
			DECLARE @registryRawValue nvarchar(MAX)
					Set @registryRawValue = '<?xml version=""1.0"" encoding=""utf-16""?><GLPresentationJournalCategoryCollection xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
						<GLPresentationJournalCategory>
							<CodeMaxLength>3</CodeMaxLength>
							<Code>EET</Code>
							<Description>Inter Company</Description>
							<Bool>Y</Bool>
							<Bool2>N</Bool2>
							<Bool3>Y</Bool3>
							<Bool4>Y</Bool4>
							<ParentCode>OUT</ParentCode>
						</GLPresentationJournalCategory>
					</GLPresentationJournalCategoryCollection>'

					INSERT INTO [{ScriptDbName}].Finance.BAS__AccountStmData
						(AccountStmDataID, AccountStmDataKey, BinaryValue, OwnerID, SDName)
					VALUES
						(NEWID(), 1, CONVERT(varbinary(MAX), CONVERT(XML, @registryRawValue)), null, 'GLJournalAdjustmentCategoriesList');
					UPDATE [{ScriptDbName}].Finance.BAS__AccReportingBook SET IncludeChildPresentation = 1
			");
			TestConnection.ExecuteNonQuery(sql);
			resultsTable = Execute(CompanyPK1, ReportingBookPK1);
			AssertEquals(4, resultsTable.Rows.Count);
			AssertEquals(3, resultsTable.Select("TransactionCategory = 'EET'").Length);
			AssertEquals(1, resultsTable.Select("TransactionCategory = ''").Length);

			sql = string.Format(CultureInfo.InvariantCulture, $@"UPDATE [{ScriptDbName}].Finance.BAS__AccReportingBook SET IncludeChildPresentation = 0");
			TestConnection.ExecuteNonQuery(sql);
			resultsTable = Execute(CompanyPK1, ReportingBookPK1);
			AssertEquals(1, resultsTable.Rows.Count);
			AssertEquals(0, resultsTable.Select("TransactionCategory = 'EET'").Length);
			AssertEquals(1, resultsTable.Select("TransactionCategory = ''").Length);
		}

		#endregion

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__Company];
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__Branch];
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__Department];
				DELETE FROM [{ScriptDbName}].[Finance].[BAS__GLAccount];
			");
			TestConnection.ExecuteNonQuery(sqlText);
			CompanyPK1 = TestHelper.InsertCompany("DYY", "DYY", countryCode: "YY");
			CompanyPK2 = TestHelper.InsertCompany("DXX", "DXX", countryCode: "XX");

			sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
					(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, IsReciprocal, SubUnitRatio, CurrencyTranslationLevel, LocalCurrency, TranslatedCurrency, TranslatedRateType, TranslatedCredit, TranslatedDebit, TranslatedBalance)
					VALUES
						(1, 1, 1, 1, 3, 4, 1, 202305, 'EET', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 1, 'BSH', 1, 1, 1, 100, 'BAL', 'CNY', 'CNY', 'SEL', 1, 3, 2),
						(2, 2, 1, 2, 2, 3, 1, 202405,    '', 'RRW', 2, 1, 'STI', 'SPR', 'INTT2', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT3', 'TPY', 'WEU', 'LOC', '899988', 1, 'BSH', 2, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 3, 2),
						(1, 3, 1, 3, 3, 4, 1, 202405, 'EET', 'RRR', 2, 1, 'STI', 'SPR', 'INTT3', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899987', 1, 'BSH', 2, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 3, 2),
						(1, 1, 1, 4, 3, 4, 1, 202307, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899986', 1, 'BSH', 1, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 3, 2),
						(1, 1, 2, 5, 3, 4, 1, 202306,    '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 1, 'BSH', 1, 1, 1, 100, 'JNL', 'CNY', 'USD', 'SEL', 1, 3, 2),
						(1, 1, 2, 1, 3, 4, 1, 202308, 'EET', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 1, 'BSH', 1, 1, 1, 100, 'BAL', 'CNY', 'USD', 'SEL', 1, 3, 2)

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AlternateGLAccountKey, DestinationAccountNo, AccountTypeCode, CompanyKey, AlternateChartKey, GLAccountKey, Attribute_LFE, Attribute_LFO, Attribute_TIC, Attribute_SPR, Attribute_OCG, Attribute_ORG, OriginalAccount, Description, StatisticalUnits, ParentGLAccountNo)
				VALUES
				(10, 10, '1109', 'BSH', 1, 2, 6, 'WEU', 'LOC', 'ETI', 'SPS', 'INT', 'OR1', '123451','description', 'KB', '1231')

				INSERT [{ScriptDbName}].Finance.CUS__ExchangeRatesforReportingbook
				(ExchangeRatesforReportingbookKey, CompanyKey, StartDate, EndDate, ExRateType, Currency, SellRate)
				VALUES
				(1, 1, '2022-01-01', '2025-11-01', 'SEL', 'USD', 2),
				(2, 2, '2022-01-01', '2025-11-01', 'BUY', 'USD', 5)
			"
			);
			TestConnection.ExecuteNonQuery(sqlText);

			TestHelper.CreateBASBranch(1, Guid.NewGuid(), 1, "UUU");
			TestHelper.CreateBASBranch(2, Guid.NewGuid(), 2, "UUY");
			TestHelper.CreateBASDepartment(1, Guid.NewGuid(), "DDD");
			TestHelper.CreateBASDepartment(2, Guid.NewGuid(), "UUD");

			TestHelper.InsertALternateChart("UUU", isGlobal: 1);
			ReportingBookPK1 = TestHelper.InsertReportingBook(1, 1, presentationJournals: "EET");
			ReportingBookPK2 = TestHelper.InsertReportingBook(2, 2, "YYY", currency: "USD", presentationJournals: "EEY");

			TestHelper.InsertGLAccount("7651", description: "GLAccount1");
			TestHelper.InsertGLAccount("7652", description: "GLAccount2");
			TestHelper.InsertGLAccount("7653", description: "GLAccount3");
			TestHelper.InsertGLAccount("7654", description: "GLAccount4");
			TestHelper.InsertGLAccount("7655", description: "GLAccount5");
			TestHelper.InsertGLAccount("7656", description: "GLAccount6");
			TestHelper.InsertBASAccount(6);
			TestHelper.InsertPeriodForInputYear(2023);
			TestHelper.InsertPeriodForInputYear(2024);
			TestHelper.InsertPeriodForInputYear(2024, 2);
		}

		DataTable Execute(Guid companyPK, Guid reportingBookPK, int startPeriod = 202301, int endPeriod = 202312, string startDate = "", string endDate = "", string branchList = "", string departmentList = "", int includePLAppropriationAccount = 0)
		{
			var sDate = startDate.IsNullOrEmpty() ? "NULL" : $"'{startDate}'";
			var eDate = endDate.IsNullOrEmpty() ? "NULL" : $"'{endDate}'";
			var sPeriod = startPeriod == 0 ? "NULL" : $"{startPeriod}";
			var ePeriod = endPeriod == 0 ? "NULL" : $"{endPeriod}";

			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].dbo.GetReportingBookGeneralLedgerAggregateData('{companyPK}', '{reportingBookPK}',{sPeriod}, {ePeriod}, {sDate}, {eDate},'{branchList}', '{departmentList}', {includePLAppropriationAccount})");
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;

		Guid CompanyPK1, CompanyPK2, ReportingBookPK1, ReportingBookPK2;
	}
}
