using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetMissingExchangeRates))]
	class GetMissingExchangeRatesTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestRun()
		{
			using (var edwConnection = Db.NewAdminConnection(ScriptDbName))
			{
				var resultsTable = Execute(edwConnection, CompanyPK1, ReportingBookPK1);
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

				PrepareTestData(edwConnection);
				var result = Execute(edwConnection, CompanyPK1, ReportingBookPK1);
				AssertEquals(3, result.Rows.Count);
				AssertEquals(1, result.Select($"TranslatedCurrency = 'USD' and CurrencyDescription = 'currency desc' and RateDate = '2024-10-20' and RateType = 'C01'").Length);
				AssertEquals(1, result.Select($"TranslatedCurrency = 'USD' and CurrencyDescription = 'currency desc' and RateDate = '2024-10-20' and RateType = 'C02'").Length);
				AssertEquals(1, result.Select($"TranslatedCurrency = 'USD' and CurrencyDescription = 'currency desc' and RateDate = '2023-03-07' and RateType = 'SEL'").Length);

				result = Execute(edwConnection, CompanyPK2, ReportingBookPK2);
				AssertEquals(1, result.Rows.Count);
				AssertEquals(1, result.Select($"RateType = 'PER'").Length);

				result = Execute(edwConnection, CompanyPK1, ReportingBookPK1, "202307", endDate: "", departmentList: "YYY", branchList: "UUY");
				AssertEquals(0, result.Rows.Count);
			}
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>();

			columns.Add("TranslatedCurrency");
			columns.Add("CurrencyDescription");
			columns.Add("RateDate");
			columns.Add("RateType");
			return columns;
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareTestData(AdminConnection connection)
		{
			var testHelper = new AccountingFunctionTestingHelper(connection, ScriptDbName);
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM [{0}].Organization.BAS__Branch;
				DELETE FROM [{0}].Organization.BAS__Department;
			", ScriptDbName);
			connection.ExecuteNonQuery(sqlText);

			CompanyPK1 = testHelper.InsertCompany("CNY", "UUU");
			CompanyPK2 = testHelper.InsertCompany("USD", "DAU");

			sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTransactionData]
				(GeneralLedgerTransactionDataKey, CompanyKey, BranchKey, Currency, DepartmentKey, GLAccountKey, GLAccountName, GLAccountNum, GLAccountType, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, GLAmountOSCredit, GLAmountOSDebit, GLAmountOSBalance, GLType, TransactionCategory, GeneralLedgerDataKey, PostDate, CompanyID,
				TransactionDate, TransactionNum, TransactionType, LineJobKey, LineDescription, HeaderDescription, LedgerCode, JobInvoiceNumber, ExchangeRate, ComplianceSubType, ComplianceNumber, ChequeOrReference, ChargeCodeKey, OrganizationKey, TransactionLineKey, TransactionLineID, TransactionHeaderID)
				VALUES
				(1, 1, 1, 'CNY', 1, 1, 'GLAccount1', '10100009', 'ARC', 1, 0, -1, 3, 0, -3, 'PST', '', 1, '2023-03-07', '{1}', '2023-09-08', '121', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu01', 3.4, 'ert', '345', '456', 2, 3, 4, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'AE6D15CE-63EC-41BF-905C-A95894A08095'),
				(2, 1, 2, 'CNY', 1, 2, 'GLAccount2', '10100019', 'APC', 1, 0, -1, 3, 0, -3, 'PST', '', 2, '2023-06-07', '{1}', '2023-09-08', '122', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu02', 3.4, 'ert', '345', '456', 2, 3, 4, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'AE6D15CE-63EC-41BF-905C-A95894A08095'),
				(3, 1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 'ARS', 0, 1,  1, 3, 0, -3, 'PST', '', 3, '2023-08-07', '{1}', '2023-09-08', '123', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu03', 3.4, 'ert', '345', '456', 2, 3, 4, 'D774E57E-5FC7-465C-BC24-D6F795550AC6', '2EAD1186-B082-40F0-81A4-4C7D4D5BC18C'),
				(4, 1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 'APS', 1, 0, -1, 3, 0, -3, 'PST', '', 4, '2023-09-07', '{1}', '2023-09-08', '124', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu04', 3.4, 'ert', '345', '456', 2, 3, 4, 'D774E57E-5FC7-465C-BC24-D6F795550AC6', '2EAD1186-B082-40F0-81A4-4C7D4D5BC18C'),
				(5, 1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 'RGT', 0, 0, -1, 3, 0, -3, 'REC', '', 5, '2023-10-07', '{1}', '2023-09-08', '126', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu09', 3.4, 'ert', '345', '456', 2, 3, 4, 'AE6D15CE-63EC-41BF-905C-A95894A08095', 'C77EF93B-DC28-40FE-A22A-AC355A295215'),
				(6, 1, 1, 'CNY', 1, 1, 'GLAccount1', '10100029', 'PGT', 0, 1,  1, 3, 0, -3, 'REC', '', 6, '2023-11-07', '{1}', '2023-09-08', '125', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu10', 3.4, 'ert', '345', '456', 2, 3, 4, 'AE6D15CE-63EC-41BF-905C-A95894A08095', 'C77EF93B-DC28-40FE-A22A-AC355A295215'),
				(7, 2, 3, 'CNY', 1, 1, 'GLAccount1', '10100029', 'PGO', 1, 0, -1, 3, 0, -3, 'REC', '', 7, '2023-09-07', '{2}', '2023-09-08', '127', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu19', 3.4, 'ert', '345', '456', 2, 3, 4, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'AE6D15CE-63EC-41BF-905C-A95894A08095'),
				(8, 2, 3, 'CNY', 1, 1, 'GLAccount1', '10100029', 'PGI', 0, 1,  1, 3, 0, -3, 'REC', '', 8, '2023-08-07', '{2}', '2023-09-08', '128', 'REV', 1, 'lineDesc', 'HDesc', 'AP', 'Jiu29', 3.4, 'ert', '345', '456', 2, 3, 4, '7A5FF88B-76BE-41AE-A3EC-CEAC983125EF', 'AE6D15CE-63EC-41BF-905C-A95894A08095')

				INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
				(GeneralLedgerTranslatedDataKey, GeneralLedgerDataKey, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, Attribute_ORG, PostPeriodForReportingBook, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, OriginalAttribute_SPR, OriginalAttribute_TIC, OriginalAttribute_ORG, AlternateGLAccount, AlternateGLAccountDescription, AlternateGLAccountKey, AlternateChartKey, Units, OriginalAccount, ReportingBookKey, CompanyOfPeriodKey, LocalCurrency, TranslatedCurrency, AccountTypeCode, CurrencyTranslationLevel, TranslatedAmount, IsReciprocal, TranslatedRateType, SubUnitRatio, TranslatedExchangeRate)
				VALUES
				(1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 202304, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', '1231', 'Account1', 1, 1, 'KG', '165', 1, 1, 'CNY', 'USD', 'BSH', 'JNL', null, 1, 'SEL', 100, null),
				(2, 2, 'TPY', 'OEU', 'FOR', 'NAV', ''   , 'OR2', 202305, 'TPY', 'OEU', 'FOR', 'NAV', 'STI', 'OR2', '1232', 'Account2', 2, 1, 'KG', '265', 1, 1, 'CNY', 'USD', 'NTE', 'BAL',    2, 1, 'BUY', 100, null),
				(3, 3, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', 202306, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1233', 'Account3', 3, 1, 'KG', '365', 1, 1, 'CNY', 'USD', 'P&L', 'BAL', null, 1, 'C01', 100, null),
				(4, 4, 'INT', 'LOC', 'FOR', ''   , 'ETI', 'NAV', 100001, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1234', 'Account4', 4, 1, 'KG', '465', 1, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 0, 'C02', 100, null),
				(5, 5, 'INT', 'LOC', ''   , 'SPS', 'ETI', 'NAV', 202301, 'INT', 'LOC', 'FOR', 'SPS', 'ETI', 'NAV', '1235', 'Account5', 5, 1, 'KG', '565', 1, 1, 'CNY', 'USD', 'BSH', 'JNL',    2, 1, 'C03', 100,    1),
				(6, 6, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1236', 'Account6', 6, 1, 'KG', '665', 1, 1, 'CNY', 'CNY', 'BSH', 'JNL', null, 1, 'PER', 100,    1),
				(7, 7, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1237', 'Account7', 7, 1, 'KG', '265', 2, 1, 'CNY', 'USD', 'NTE', 'JNL',    2, 1, 'SEL', 100,    1),
				(8, 8, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', 202309, 'NAV', 'NAV', 'NAV', 'SPR', 'NAV', 'OR1', '1238', 'Account8', 8, 1, 'KG', '265', 2, 1, 'CNY', 'USD', 'BSH', 'BAL',    2, 1, 'PER', 100, null)

				INSERT [{0}].Finance.CUS__ExchangeRatesforReportingbook
				(ExchangeRatesforReportingbookKey, CompanyKey, StartDate, EndDate, ExRateType, Currency, SellRate)
				VALUES
				(1, 1, '2024-11-01', '2024-11-30', 'SEL', 'USD', 2),
				(2, 2, '2024-11-01', '2024-11-30', 'BUY', 'USD', 5)
				",
				ScriptDbName, CompanyPK1, CompanyPK2);
			connection.ExecuteNonQuery(sqlText);
			testHelper.CreateBASBranch(1, Guid.NewGuid(), 1, "UUU");
			testHelper.CreateBASBranch(2, Guid.NewGuid(), 1, "UUY");
			testHelper.CreateBASBranch(3, Guid.NewGuid(), 2, "UUO");
			testHelper.CreateBASDepartment(1, Guid.NewGuid(), "YYY");
			testHelper.CreateBASDepartment(2, Guid.NewGuid(), "YYU");
			testHelper.InsertCurrency("USD", 1000);
			testHelper.InsertCurrency("CNY", 100);

			testHelper.InsertPeriodForInputYear(2023, 1);
			testHelper.InsertPeriodForInputYear(2023, 2);
			ReportingBookPK1 = testHelper.InsertReportingBook(1, 1, "RUR");
			ReportingBookPK2 = testHelper.InsertReportingBook(2, 2, "RUY");
		}

		DataTable Execute(AdminConnection edwConnection, Guid companyPK, Guid reportingBookPK, string endPeriod = "null", string endDate = "2024-10-20", string branchList = "", string departmentList = "")
		{
			var dbCommand = edwConnection.Command($"USE {ScriptDbName} SELECT * FROM [{ScriptDbName}].[dbo].GetMissingExchangeRates('{companyPK}', '{reportingBookPK}', {endPeriod}, '{endDate}', '{branchList}', '{departmentList}')");
			return DataUtils.GetDataTableFromCommand(dbCommand);
		}

		Guid CompanyPK1, CompanyPK2, ReportingBookPK1, ReportingBookPK2;
	}
}
