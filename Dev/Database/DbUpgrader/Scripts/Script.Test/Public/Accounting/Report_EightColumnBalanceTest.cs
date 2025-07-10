using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_EightColumnBalance))]
	class Report_EightColumnBalanceTest : DbCreateScriptTest
	{
		#region Test

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
			DbHelper.InsertGLAccount(accountNum: "1111.11.11", description: "Empty", accountType: "P&L", debitCredit: "CR", agColumn: "TS");

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

			result = Execute(202207, 0, CompanyPK);
			AssertEquals("Should be 1200.1200 amount in Closing Debit", 1, result.Select("AccountNumber = '1040.10.10' and ClosingDebit = 1200.1200").Length);
			AssertEquals("Should be 4 rows in report", 4, result.Rows.Count);

			result = Execute(202606, 0, CompanyPK);
			AssertEquals("Should be 254978.3434 amount in Closing Debit for Retained Earnings", 1, result.Select("AccountNumber = '4900.00.00' and ClosingDebit = 254978.3434").Length);
			AssertEquals("Should be 8 rows in report", 8, result.Rows.Count);

			result = Execute(202701, 0, CompanyPK);
			AssertEquals("Should be 0 rows in report", 0, result.Rows.Count);
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTestData();
		}

		void PrepareTestData()
		{
			var sqlString = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM [{0}].dbo.AccChargeCode;
				DELETE FROM [{0}].dbo.AccGLHeader;
				DELETE FROM [{0}].dbo.AccGeneralLedgerData;
			", ScriptDbName);

			TestConnection.ExecuteNonQuery(sqlString);

			Country = "CL";
			Currency = "CLP";
			CompanyPK = DbHelper.InsertCompany("8CB", "Eight Column Balance", Currency, Country, true, true);
			BranchPK = DbHelper.InsertBranch("ST8", CompanyPK, "Test");
			DepartmentPK = DbHelper.InsertDepartment("ACC", "Accounting");

			DbHelper.SetRegistryPLAppropriationAccount(DbHelper.InsertGLAccount(accountNum: "4900.00.00", description: "RETAINED EARNINGS FROM PREVIOUS YR", accountType: "BSH", debitCredit: "CR", agColumn: "AP"));

			var altAcc01 = DbHelper.InsertGLAccount(accountNum: "9510.00.02", description: "Asset alternate account", accountType: "ALT", debitCredit: "CR", agColumn: "LI");
			var altAcc02 = DbHelper.InsertGLAccount(accountNum: "6820.00.20", description: "Liability alternate account", accountType: "ALT", debitCredit: "DR", agColumn: "AS");

			var pylBadDebt = DbHelper.InsertGLAccount(accountNum: "3980.00.00", description: "BAD DEBT", accountType: "P&L", debitCredit: "DR", agColumn: "OV");
			var pylDocRevAct = DbHelper.InsertGLAccount(accountNum: "1040.10.10", description: "DOCUMENTATION REVENUE ACTUAL", accountType: "P&L", debitCredit: "CR", agColumn: "TS");
			var pylDocRevAccr = DbHelper.InsertGLAccount(accountNum: "1040.10.20", description: "DOCUMENTATION REVENUE ACCRUED", accountType: "P&L", debitCredit: "CR", agColumn: "TS");

			var bshWipC = DbHelper.InsertGLAccount(accountNum: "6240.00.00", description: "WIP CONTROL", accountType: "BSH", debitCredit: "DR", agColumn: "AS");
			var bshAssTest = DbHelper.InsertGLAccount(accountNum: "6210.00.01", description: "Asset test acc", accountType: "BSH", debitCredit: "DR", altAccount: altAcc01, agColumn: "AS");
			var bshOutTax = DbHelper.InsertGLAccount(accountNum: "8310.00.00", description: "OUTPUT TAX PAYABLE", accountType: "BSH", debitCredit: "DR", agColumn: "AS");
			var bshTradDeb = DbHelper.InsertGLAccount(accountNum: "6210.00.00", description: "TRADE DEBTORS CONTROL", accountType: "BSH", debitCredit: "DR", agColumn: "AS");
			var bshLiabTest = DbHelper.InsertGLAccount(accountNum: "9510.00.01", description: "Liability test account", accountType: "BSH", debitCredit: "CR", altAccount: altAcc02, agColumn: "LI");
			var bshRevSus = DbHelper.InsertGLAccount(accountNum: "6215.00.00", description: "REVENUE SUSPENSE CONTROL ACCOUNT", accountType: "BSH", debitCredit: "DR", agColumn: "AS");

			SetAccPeriod();

			var sqlBuilder = new StringBuilder();

			sqlBuilder.Append($"INSERT INTO {ScriptDbName}.dbo.AccGeneralLedgerData ");
			sqlBuilder.Append("(");
			sqlBuilder.Append("GLD_PK, GLD_GC_Company, GLD_PostDate, GLD_PostPeriod, GLD_GLAccountType, GLD_AG_GLAccount,");
			sqlBuilder.Append("GLD_OSDebitAmount, GLD_OSCreditAmount, GLD_LocalDebitAmount, GLD_LocalCreditAmount,");
			sqlBuilder.Append("GLD_GB_Branch, GLD_GE_Department, GLD_Currency, GLD_ExchangeRate, GLD_Type,");
			sqlBuilder.Append("GLD_SystemCreateTimeUtc, GLD_SystemCreateUser, GLD_SystemLastEditTimeUtc, GLD_SystemLastEditUser,");
			sqlBuilder.Append("GLD_JournalEntriesNumber, GLD_JournalEntriesNumberRuleCode)");
			sqlBuilder.Append(" VALUES ");

			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202207, "2022-01-19", pylDocRevAct, 1200.12m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202207, "2022-01-19", bshOutTax, 0.0m, 2280.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202210, "2022-01-31", bshTradDeb, 1228.54m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202210, "2022-01-31", bshRevSus, 22200.0m, 0.0m)},");

			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202302, "2022-08-01", pylDocRevAct, 0.0m, 3200.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202302, "2022-08-01", bshLiabTest, 0.0m, 4281.11m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202308, "2023-02-01", bshWipC, 14321.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202308, "2023-02-01", pylDocRevAccr, 12314.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202311, "2023-05-01", bshWipC, 31.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202311, "2023-05-01", pylDocRevAccr, 9871.1m, 0.0m)},");

			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202405, "2023-11-01", bshWipC, 11235.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202405, "2023-11-01", pylDocRevAccr, 1.1234m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202407, "2024-01-01", bshAssTest, 1680.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202407, "2024-01-01", bshLiabTest, 4550.0m, 0.0m)},");

			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202501, "2024-07-01", pylBadDebt, 0.0m, 15004.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202501, "2024-07-01", bshAssTest, 1680.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202504, "2024-10-01", bshLiabTest, 550.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202504, "2024-10-01", bshLiabTest, 45.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202507, "2025-01-01", pylDocRevAct, 249946.0m, 0.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202507, "2025-01-01", pylBadDebt, 0.0m, 150.0m)},");

			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202601, "2025-07-01", pylDocRevAct, 0.0m, 120.0m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202601, "2025-07-01", bshLiabTest, 0.0m, 201.31m)},");
			sqlBuilder.Append($"{InsertAccGeneralLedgerDataValue(202606, "2025-12-01", bshAssTest, 10.0m, 0.0m)}");

			TestConnection.ExecuteNonQuery(sqlBuilder.ToString());
		}

		#endregion

		#region Implementation

		string Country, Currency;

		Guid CompanyPK, BranchPK, DepartmentPK;

		string InsertAccGeneralLedgerDataValue(int postPeriod, string postDate, Guid glAccount, decimal debitAmount, decimal creditAmount)
		{
			return string.Format(CultureInfo.InvariantCulture,
				$"(NEWID(), '{CompanyPK}', '{postDate}', {postPeriod}, 'TLG', '{glAccount}', {debitAmount}, {creditAmount}, {debitAmount}, {creditAmount}, '{BranchPK}', '{DepartmentPK}', '{Currency}', 1, 'TGM', '{postDate}', '~BP', '{postDate}', '~BP', '', '')");
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

		void SetAccPeriod()
		{
			DbHelper.InsertAccPeriod(2022, 01, CompanyPK, new DateTime(2021, 7, 1));
			DbHelper.InsertAccPeriod(2022, 02, CompanyPK, new DateTime(2021, 8, 1));
			DbHelper.InsertAccPeriod(2022, 03, CompanyPK, new DateTime(2021, 9, 1));
			DbHelper.InsertAccPeriod(2022, 04, CompanyPK, new DateTime(2021, 10, 1));
			DbHelper.InsertAccPeriod(2022, 05, CompanyPK, new DateTime(2021, 11, 1));
			DbHelper.InsertAccPeriod(2022, 06, CompanyPK, new DateTime(2021, 12, 1));
			DbHelper.InsertAccPeriod(2022, 07, CompanyPK, new DateTime(2022, 1, 1));
			DbHelper.InsertAccPeriod(2022, 08, CompanyPK, new DateTime(2022, 2, 1));
			DbHelper.InsertAccPeriod(2022, 09, CompanyPK, new DateTime(2022, 3, 1));
			DbHelper.InsertAccPeriod(2022, 10, CompanyPK, new DateTime(2022, 4, 1));
			DbHelper.InsertAccPeriod(2022, 11, CompanyPK, new DateTime(2022, 5, 1));
			DbHelper.InsertAccPeriod(2022, 12, CompanyPK, new DateTime(2022, 6, 1));

			DbHelper.InsertAccPeriod(2023, 01, CompanyPK, new DateTime(2022, 7, 1));
			DbHelper.InsertAccPeriod(2023, 02, CompanyPK, new DateTime(2022, 8, 1));
			DbHelper.InsertAccPeriod(2023, 03, CompanyPK, new DateTime(2022, 9, 1));
			DbHelper.InsertAccPeriod(2023, 04, CompanyPK, new DateTime(2022, 10, 1));
			DbHelper.InsertAccPeriod(2023, 05, CompanyPK, new DateTime(2022, 11, 1));
			DbHelper.InsertAccPeriod(2023, 06, CompanyPK, new DateTime(2022, 12, 1));
			DbHelper.InsertAccPeriod(2023, 07, CompanyPK, new DateTime(2023, 1, 1));
			DbHelper.InsertAccPeriod(2023, 08, CompanyPK, new DateTime(2023, 2, 1));
			DbHelper.InsertAccPeriod(2023, 09, CompanyPK, new DateTime(2023, 3, 1));
			DbHelper.InsertAccPeriod(2023, 10, CompanyPK, new DateTime(2023, 4, 1));
			DbHelper.InsertAccPeriod(2023, 11, CompanyPK, new DateTime(2023, 5, 1));
			DbHelper.InsertAccPeriod(2023, 12, CompanyPK, new DateTime(2023, 6, 1));

			DbHelper.InsertAccPeriod(2024, 01, CompanyPK, new DateTime(2023, 7, 1));
			DbHelper.InsertAccPeriod(2024, 02, CompanyPK, new DateTime(2023, 8, 1));
			DbHelper.InsertAccPeriod(2024, 03, CompanyPK, new DateTime(2023, 9, 1));
			DbHelper.InsertAccPeriod(2024, 04, CompanyPK, new DateTime(2023, 10, 1));
			DbHelper.InsertAccPeriod(2024, 05, CompanyPK, new DateTime(2023, 11, 1));
			DbHelper.InsertAccPeriod(2024, 06, CompanyPK, new DateTime(2023, 12, 1));
			DbHelper.InsertAccPeriod(2024, 07, CompanyPK, new DateTime(2024, 1, 1));
			DbHelper.InsertAccPeriod(2024, 08, CompanyPK, new DateTime(2024, 2, 1));
			DbHelper.InsertAccPeriod(2024, 09, CompanyPK, new DateTime(2024, 3, 1));
			DbHelper.InsertAccPeriod(2024, 10, CompanyPK, new DateTime(2024, 4, 1));
			DbHelper.InsertAccPeriod(2024, 11, CompanyPK, new DateTime(2024, 5, 1));
			DbHelper.InsertAccPeriod(2024, 12, CompanyPK, new DateTime(2024, 6, 1));

			DbHelper.InsertAccPeriod(2025, 01, CompanyPK, new DateTime(2024, 7, 1));
			DbHelper.InsertAccPeriod(2025, 02, CompanyPK, new DateTime(2024, 8, 1));
			DbHelper.InsertAccPeriod(2025, 03, CompanyPK, new DateTime(2024, 9, 1));
			DbHelper.InsertAccPeriod(2025, 04, CompanyPK, new DateTime(2024, 10, 1));
			DbHelper.InsertAccPeriod(2025, 05, CompanyPK, new DateTime(2024, 11, 1));
			DbHelper.InsertAccPeriod(2025, 06, CompanyPK, new DateTime(2024, 12, 1));
			DbHelper.InsertAccPeriod(2025, 07, CompanyPK, new DateTime(2025, 1, 1));
			DbHelper.InsertAccPeriod(2025, 08, CompanyPK, new DateTime(2025, 2, 1));
			DbHelper.InsertAccPeriod(2025, 09, CompanyPK, new DateTime(2025, 3, 1));
			DbHelper.InsertAccPeriod(2025, 10, CompanyPK, new DateTime(2025, 4, 1));
			DbHelper.InsertAccPeriod(2025, 11, CompanyPK, new DateTime(2025, 5, 1));
			DbHelper.InsertAccPeriod(2025, 12, CompanyPK, new DateTime(2025, 6, 1));

			DbHelper.InsertAccPeriod(2026, 01, CompanyPK, new DateTime(2025, 7, 1));
			DbHelper.InsertAccPeriod(2026, 02, CompanyPK, new DateTime(2025, 8, 1));
			DbHelper.InsertAccPeriod(2026, 03, CompanyPK, new DateTime(2025, 9, 1));
			DbHelper.InsertAccPeriod(2026, 04, CompanyPK, new DateTime(2025, 10, 1));
			DbHelper.InsertAccPeriod(2026, 05, CompanyPK, new DateTime(2025, 11, 1));
			DbHelper.InsertAccPeriod(2026, 06, CompanyPK, new DateTime(2025, 12, 1));
			DbHelper.InsertAccPeriod(2026, 07, CompanyPK, new DateTime(2026, 1, 1));
			DbHelper.InsertAccPeriod(2026, 08, CompanyPK, new DateTime(2026, 2, 1));
			DbHelper.InsertAccPeriod(2026, 09, CompanyPK, new DateTime(2026, 3, 1));
			DbHelper.InsertAccPeriod(2026, 10, CompanyPK, new DateTime(2026, 4, 1));
			DbHelper.InsertAccPeriod(2026, 11, CompanyPK, new DateTime(2026, 5, 1));
			DbHelper.InsertAccPeriod(2026, 12, CompanyPK, new DateTime(2026, 6, 1));
		}

		#endregion
	}
}
