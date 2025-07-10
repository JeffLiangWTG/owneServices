using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(AccMultilingualTrialBalance))]
	class AccMultilingualTrialBalanceTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestAccMultilingualTrialBalance()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.10", "NTE");
			Helper.InsertGLAccount(2, "2010.00.20", "P&L");
			Helper.InsertGLAccount(3, "2010.00.30", "NTE");
			Helper.InsertGLAccount(4, "2010.00.40", "P&L");

			Helper.InsertGLAggregate(12, 202001, 1, "", 12);
			Helper.InsertGLAggregate(13, 202001, 1, "", 13);
			Helper.InsertGLAggregate(21, 202001, 2, "", 21);
			Helper.InsertGLAggregate(22, 202001, 2, "", 22);
			Helper.InsertAccountDescriptor(gLAccountKey: 1, language: "", localAccountNumber: "2010.00.10", reportCategory: "NRE");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 2, gLAccountKey: 2, language: "", localAccountNumber: "2010.00.20", reportCategory: "");
			Helper.InsertAccountDescriptorPivot(gLAccountKey: 3);
			Helper.InsertAccountDescriptorPivot(gLAccountDescriptorKey: 2, gLAccountKey: 4);

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccMultilingualTrialBalance(202001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '')";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have 2 row", 2, result.Rows.Count);

			Helper.InsertPeriodManagement(202001, new DateTime(2023, 07, 01));
			Helper.InsertStmData(new DateTime(2022, 07, 01));
			result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().Equals("2010.00.30") select row).FirstOrDefault();
			AssertEquals("AccountType field", "NTE", rowSelected["AccountType"].ToString());
			AssertEquals("CurrentDebitForCalculateTotal field", 0M, rowSelected["CurrentDebitForCalculateTotal"]);
			AssertEquals("CurrentCreditForCalculateTotal field", 0M, rowSelected["CurrentCreditForCalculateTotal"]);
			AssertEquals("ClosingDebitForCalculateTotal field", 0M, rowSelected["ClosingDebitForCalculateTotal"]);
			AssertEquals("ClosingCreditForCalculateTotal field", 0M, rowSelected["ClosingCreditForCalculateTotal"]);
			AssertEquals("LocalAccountType field", "NRE", rowSelected["LocalAccountType"]);
			AssertEquals("LocalAccountNumber field", "2010.00.10", rowSelected["LocalAccountNumber"]);
			AssertEquals("LocalAccountDescription field", DBNull.Value, rowSelected["LocalAccountDescription"]);

			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().Equals("2010.00.40") select row).FirstOrDefault();
			AssertEquals("AccountType field", "P&L", rowSelected["AccountType"].ToString());
			AssertEquals("CurrentDebitForCalculateTotal field", DBNull.Value, rowSelected["CurrentDebitForCalculateTotal"]);
			AssertEquals("CurrentCreditForCalculateTotal field", DBNull.Value, rowSelected["CurrentCreditForCalculateTotal"]);
			AssertEquals("ClosingDebitForCalculateTotal field", DBNull.Value, rowSelected["ClosingDebitForCalculateTotal"]);
			AssertEquals("ClosingCreditForCalculateTotal field", DBNull.Value, rowSelected["ClosingCreditForCalculateTotal"]);
			AssertEquals("LocalAccountType field", "", rowSelected["LocalAccountType"]);
			AssertEquals("LocalAccountNumber field", "2010.00.20", rowSelected["LocalAccountNumber"]);
			AssertEquals("LocalAccountDescription field", DBNull.Value, rowSelected["LocalAccountDescription"]);
		}

		public void TestCanGetAllAggregateData()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.10", "NTE");
			Helper.InsertGLAccount(2, "2010.00.20", "P&L");
			Helper.InsertGLAccount(3, "2010.00.30", "NTE");
			Helper.InsertGLAccount(4, "2010.00.40", "P&L");

			Helper.InsertBASGLAggregate(12, 201001, 1);
			Helper.InsertGLAggregate(13, 202001, 1, "", 13);
			Helper.InsertGLAggregate(21, 202001, 2, "", 21);
			Helper.InsertGLAggregate(22, 202001, 2, "", 22);
			Helper.InsertAccountDescriptor(gLAccountKey: 1, language: "", localAccountNumber: "2010.00.10", reportCategory: "NRE");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 2, gLAccountKey: 2, language: "", localAccountNumber: "2010.00.20", reportCategory: "");
			Helper.InsertAccountDescriptorPivot(gLAccountKey: 3);
			Helper.InsertAccountDescriptorPivot(gLAccountDescriptorKey: 2, gLAccountKey: 4);

			Helper.InsertPeriodForInputYear(2019);
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2019, 01, 01));

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccMultilingualTrialBalance(202001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().Equals("2010.00.30") select row).FirstOrDefault();
			AssertEquals("AccountType field", "NTE", rowSelected["AccountType"].ToString());
			AssertEquals("CurrentDebitForCalculateTotal field", 0M, rowSelected["CurrentDebitForCalculateTotal"]);
			AssertEquals("CurrentCreditForCalculateTotal field", 0M, rowSelected["CurrentCreditForCalculateTotal"]);
			AssertEquals("ClosingDebitForCalculateTotal field", 0M, rowSelected["ClosingDebitForCalculateTotal"]);
			AssertEquals("ClosingCreditForCalculateTotal field", 0M, rowSelected["ClosingCreditForCalculateTotal"]);
			AssertEquals("LocalAccountType field", "NRE", rowSelected["LocalAccountType"]);
			AssertEquals("LocalAccountNumber field", "2010.00.10", rowSelected["LocalAccountNumber"]);
			AssertEquals("LocalAccountDescription field", DBNull.Value, rowSelected["LocalAccountDescription"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
