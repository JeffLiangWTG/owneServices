using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(Report_EightColumnBalance))]
	class Report_EightColumnBalanceTest : BiCreateScriptTest
	{
		public void TestColumnsNamesAndQuantity()
		{
			var allowedNames = new List<string>
			{
				"SectionType",
				"DebitCredit",
				"AccountNumber",
				"AccountType",
				"AccountName",
				"ClosingDebit",
				"ClosingCredit",
				"CurrentDebit",
				"CurrentCredit",
				"Asset",
				"Liability",
				"Losses",
				"Profits"
			};

			var result = Execute(0, 0, null);

			AssertEquals("The number of columns should be the same as in the allowed names list", allowedNames.Count, result.Columns.Count);

			foreach (DataColumn column in result.Columns)
			{
				Assert($"Should contain column '{column.ColumnName}'", allowedNames.Contains(column.ColumnName));
			}
		}

		public void TestRetainedEarningsExist()
		{
			var result = Execute(202512, 0, CompanyPK);

			AssertEquals("There should be 1 PL Appropriation Account", 1, result.Select("AccountNumber = '4900.00.00' and AccountName = 'RETAINED EARNINGS FROM PREVIOUS YR'").Length);
		}

		public void TestRetainedEarningsNotExist()
		{
			var result = Execute(202212, 0, CompanyPK);

			AssertEquals("There should not be any PL Appropriation Account", 0, result.Select("AccountNumber = '4900.00.00' and AccountName = 'RETAINED EARNINGS FROM PREVIOUS YR'").Length);
		}

		public void TestAlternateAccountExist()
		{
			var result = Execute(202512, 0, CompanyPK);

			AssertEquals("There should be 1 ALT account", 1, result.Select("AccountType = 'ALT'").Length);
		}

		public void TestAlternateAccountNotExist()
		{
			var result = Execute(202212, 0, CompanyPK);

			AssertEquals("There should not be any ALT account", 0, result.Select("AccountType = 'ALT'").Length);
		}

		public void TestIncludeZeroBalance()
		{
			Helper.InsertGLAccount(accoutKey: 13, number: "1111.11.11", accountTypeCode: "P&L", debitCreditCode: "CR", sectionType: "TS");

			var result = Execute(202512, 1, CompanyPK);

			AssertEquals("Should be 13 rows in report", 13, result.Rows.Count);
			AssertEquals("Should be 0 amount", 1, result.Select("AccountNumber = '1111.11.11' and ClosingDebit = 0").Length);
			AssertEquals("Should be 0 amount", 1, result.Select("AccountNumber = '1111.11.11' and ClosingCredit = 0").Length);
			AssertEquals("Should be 0 amount", 1, result.Select("AccountNumber = '1111.11.11' and CurrentDebit = 0").Length);
			AssertEquals("Should be 0 amount", 1, result.Select("AccountNumber = '1111.11.11' and CurrentCredit = 0").Length);
		}

		public void TestNotIncludeZeroBalance()
		{
			var result = Execute(202512, 0, CompanyPK);

			AssertEquals("Should be 9 rows in report", 9, result.Rows.Count);
		}

		public void TestPeriodManagment()
		{
			var result = Execute(202206, 0, CompanyPK);
			AssertEquals("Should be 0 rows in report", 0, result.Rows.Count);

			result = Execute(202210, 0, CompanyPK);
			AssertEquals("Should be 1200.1200 amount in Closing Debit", 1, result.Select("AccountNumber = '1040.10.10' and ClosingDebit = 1200.1200").Length);
			AssertEquals("Should be 4 rows in report", 4, result.Rows.Count);

			result = Execute(202606, 0, CompanyPK);
			AssertEquals("Should be 254978.3434 amount in Closing Debit for Retained Earnings", 1, result.Select("AccountNumber = '4900.00.00' and ClosingDebit = 254978.3434").Length);
			AssertEquals("Should be 8 rows in report", 8, result.Rows.Count);

			result = Execute(202701, 0, CompanyPK);
			AssertEquals("Should be 0 rows in report", 0, result.Rows.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTestData();
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void PrepareTestData()
		{
			CompanyPK = Guid.NewGuid();

			var sqlBuilder = new StringBuilder();

			// Company / Branch / Department

			sqlBuilder.Append($"INSERT INTO {ScriptDbName}.[Organization].[BAS__Company] ");
			sqlBuilder.Append("([CompanyKey],[CompanyID],[LocalCurrency],[CompanyCode],[IsGSTCashBasis],[IsGSTRegistered])");
			sqlBuilder.Append(" VALUES ");
			sqlBuilder.Append($"(1, '{CompanyPK}', 'CL', 'CLP', 1, 1);");

			sqlBuilder.Append($"INSERT INTO {ScriptDbName}.[Organization].[BAS__Branch] ");
			sqlBuilder.Append("([BranchKey],[BranchID],[CompanyKey],[CompanyID],[BranchCode],[OrganizationKey])");
			sqlBuilder.Append(" VALUES ");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', 'ST8', 1);");

			sqlBuilder.Append($"INSERT INTO {ScriptDbName}.[Organization].[BAS__Department] ");
			sqlBuilder.Append("([DepartmentKey],[DepartmentID],[Code])");
			sqlBuilder.Append(" VALUES ");
			sqlBuilder.Append($"(1, newid(), 'ACC');");

			// GL Account and GL_PL_Appropriation

			sqlBuilder.Append($"INSERT INTO {ScriptDbName}.[Finance].[BAS__GLAccount] ");
			sqlBuilder.Append("([GLAccountKey],[GLAccountID],[AccountNo],[Description],[AccountTypeCode],[DebitCreditCode],[SectionType],[AlternateAccountKey])");
			sqlBuilder.Append(" VALUES ");
			sqlBuilder.Append($"(1, newid(), '9510.00.02', 'Asset alternate account', 'ALT', 'CR', 'LI', null),");
			sqlBuilder.Append($"(2, newid(), '6820.00.20', 'Liability alternate account', 'ALT', 'DR', 'AS', null),");
			sqlBuilder.Append($"(3, newid(), '3980.00.00', 'BAD DEBT', 'P&L', 'DR', 'OV', null),");
			sqlBuilder.Append($"(4, newid(), '1040.10.10', 'DOCUMENTATION REVENUE ACTUAL', 'P&L', 'CR', 'TS', null),");
			sqlBuilder.Append($"(5, newid(), '1040.10.20', 'DOCUMENTATION REVENUE ACCRUED', 'P&L', 'CR', 'TS', null),");
			sqlBuilder.Append($"(6, newid(), '6240.00.00', 'WIP CONTROL', 'BSH', 'DR', 'AS', null),");
			sqlBuilder.Append($"(7, newid(), '6210.00.01', 'Asset test acc', 'BSH', 'DR', 'AS', 1),");
			sqlBuilder.Append($"(8, newid(), '8310.00.00', 'OUTPUT TAX PAYABLE', 'BSH', 'DR', 'AS', null),");
			sqlBuilder.Append($"(9, newid(), '6210.00.00', 'TRADE DEBTORS CONTROL', 'BSH', 'DR', 'AS', null),");
			sqlBuilder.Append($"(10, newid(), '9510.00.01', 'Liability test account', 'BSH', 'CR', 'LI', 2),");
			sqlBuilder.Append($"(11, newid(), '6215.00.00', 'REVENUE SUSPENSE CONTROL ACCOUNT', 'BSH', 'DR', 'AS', null),");
			sqlBuilder.Append($"(12, newid(), '4900.00.00', 'RETAINED EARNINGS FROM PREVIOUS YR', 'BSH', 'CR', 'AP', null);");

			sqlBuilder.Append($"INSERT INTO {ScriptDbName}.[Customs].[BAS__Account] ");
			sqlBuilder.Append("([AccountKey],[AccountID],[GLAccountKey],[AccountType])");
			sqlBuilder.Append(" VALUES ");
			sqlBuilder.Append($"(1, newid(), 12, 'GL_PL_APPROPRIATION_ACCOUNT');");

			// Period Management

			sqlBuilder.Append($"INSERT INTO {ScriptDbName}.[Finance].[BAS__PeriodManagement] ");
			sqlBuilder.Append("([PeriodManagementKey],[PeriodManagementID],[CompanyKey],[CompanyID],[StartDate],[EndDate],[Period],[Year])");
			sqlBuilder.Append(" VALUES ");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2021-07-01', '2021-07-31', 202201, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2021-08-01', '2021-08-31', 202202, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2021-09-01', '2021-09-30', 202203, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2021-10-01', '2021-10-31', 202204, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2021-11-01', '2021-11-30', 202205, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2021-12-01', '2021-12-31', 202206, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-01-01', '2022-01-31', 202207, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-02-01', '2022-02-28', 202208, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-03-01', '2022-03-31', 202209, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-04-01', '2022-04-30', 202210, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-05-01', '2022-05-31', 202211, 2022),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-06-01', '2022-06-30', 202212, 2022),");

			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-07-01', '2022-07-31', 202301, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-08-01', '2022-08-31', 202302, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-09-01', '2022-09-30', 202303, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-10-01', '2022-10-31', 202304, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-11-01', '2022-11-30', 202305, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2022-12-01', '2022-12-31', 202306, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-01-01', '2023-01-31', 202307, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-02-01', '2023-02-28', 202308, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-03-01', '2023-03-31', 202309, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-04-01', '2023-04-30', 202310, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-05-01', '2023-05-31', 202311, 2023),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-06-01', '2023-06-30', 202312, 2023),");

			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-07-01', '2023-07-31', 202401, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-08-01', '2023-08-31', 202402, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-09-01', '2023-09-30', 202403, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-10-01', '2023-10-31', 202404, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-11-01', '2023-11-30', 202405, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2023-12-01', '2023-12-31', 202406, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-01-01', '2024-01-31', 202407, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-02-01', '2024-02-29', 202408, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-03-01', '2024-03-31', 202409, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-04-01', '2024-04-30', 202410, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-05-01', '2024-05-31', 202411, 2024),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-06-01', '2024-06-30', 202412, 2024),");

			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-07-01', '2024-07-31', 202501, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-08-01', '2024-08-31', 202502, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-09-01', '2024-09-30', 202503, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-10-01', '2024-10-31', 202504, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-11-01', '2024-11-30', 202505, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2024-12-01', '2024-12-31', 202506, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-01-01', '2025-01-31', 202507, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-02-01', '2025-02-28', 202508, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-03-01', '2025-03-31', 202509, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-04-01', '2025-04-30', 202510, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-05-01', '2025-05-31', 202511, 2025),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-06-01', '2025-06-30', 202512, 2025),");

			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-07-01', '2025-07-31', 202601, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-08-01', '2025-08-31', 202602, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-09-01', '2025-09-30', 202603, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-10-01', '2025-10-31', 202604, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-11-01', '2025-11-30', 202605, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2025-12-01', '2025-12-31', 202606, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2026-01-01', '2026-01-31', 202607, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2026-02-01', '2026-02-28', 202608, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2026-03-01', '2026-03-31', 202609, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2026-04-01', '2026-04-30', 202610, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2026-05-01', '2026-05-31', 202611, 2026),");
			sqlBuilder.Append($"(1, newid(), 1, '{CompanyPK}', '2026-06-01', '2026-06-30', 202612, 2026);");

			// GeneralLedgerAggregateData

			sqlBuilder.Append($"INSERT INTO {ScriptDbName}.[Finance].[GRP__GeneralLedgerAggregateData] ");
			sqlBuilder.Append("([CompanyKey],[BranchKey],[DepartmentKey],[PostPeriod],[GLAccountKey],[Currency],[GLAmountOSDebit],[GLAmountOSCredit],[GLAmountLocalDebit],[GLAmountLocalCredit]) ");
			sqlBuilder.Append(" VALUES ");

			sqlBuilder.Append($"(1, 1, 1, 202207, 4, 'CLP', 1200.12, 0.0, 1200.12, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202207, 8, 'CLP', 0.0, 2280.0, 0.0, 2280.0),");
			sqlBuilder.Append($"(1, 1, 1, 202210, 9, 'CLP', 1228.54, 0.0, 1228.54, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202210, 11, 'CLP', 22200.0, 0.0, 22200.0, 0.0),");

			sqlBuilder.Append($"(1, 1, 1, 202302, 4, 'CLP', 0.0, 3200.0, 0.0, 3200.0),");
			sqlBuilder.Append($"(1, 1, 1, 202302, 10, 'CLP', 0.0, 4281.11, 0.0, 4281.11),");
			sqlBuilder.Append($"(1, 1, 1, 202308, 6, 'CLP', 14321.0, 0.0, 14321.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202308, 5, 'CLP', 12314.0, 0.0, 12314.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202311, 6, 'CLP', 31.0, 0.0, 31.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202311, 5, 'CLP', 9871.1, 0.0, 9871.1, 0.0),");

			sqlBuilder.Append($"(1, 1, 1, 202405, 6, 'CLP', 11235.0, 0.0, 11235.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202405, 5, 'CLP', 1.1234, 0.0, 1.1234, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202407, 7, 'CLP', 1680.0, 0.0, 1680.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202407, 10, 'CLP', 4550.0, 0.0, 4550.0, 0.0),");

			sqlBuilder.Append($"(1, 1, 1, 202501, 3, 'CLP', 0.0, 15004.0, 0.0, 15004.0),");
			sqlBuilder.Append($"(1, 1, 1, 202501, 7, 'CLP', 1680.0, 0.0, 1680.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202504, 10, 'CLP', 550.0, 0.0, 550.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202504, 10, 'CLP', 45.0, 0.0, 45.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202507, 4, 'CLP', 249946.0, 0.0, 249946.0, 0.0),");
			sqlBuilder.Append($"(1, 1, 1, 202507, 3, 'CLP', 0.0, 150.0, 0.0, 150.0),");

			sqlBuilder.Append($"(1, 1, 1, 202601, 4, 'CLP', 0.0, 120.0, 0.0, 120.0),");
			sqlBuilder.Append($"(1, 1, 1, 202601, 10, 'CLP', 0.0, 201.31, 0.0, 201.31),");
			sqlBuilder.Append($"(1, 1, 1, 202606, 7, 'CLP', 10.0, 0.0, 10.0, 0.0);");

			TestConnection.ExecuteNonQuery(sqlBuilder.ToString());
		}

		DataTable Execute(int currentPeriod, int inclZeroBalance, Guid? companyKey = null)
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]");
			sqlBuilder.Append("@CurrentPeriod = ").Append(currentPeriod.ToString());
			sqlBuilder.Append(",@IncludeZeroBalance = ").Append(inclZeroBalance.ToString());
			sqlBuilder.Append(",@CompanyPK = ").Append(companyKey == null ? "NULL" : $"'{companyKey}'");

			var sqlText = string.Format(CultureInfo.InvariantCulture,
				sqlBuilder.ToString()
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		Guid CompanyPK;

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
