using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(AccMultilingualTrialBalanceAmountAndAlternateAmounts))]
	class AccMultilingualTrialBalanceAmountAndAlternateAmountsTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestAccMultilingualTrialBalanceAmountAndAlternateAmounts()
		{
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 01, 01));

			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.20");
			Helper.InsertGLAccount(2, "2010.00.30", "BSH");
			Helper.InsertGLAccount(3, "2010.00.10", "NTE");
			Helper.InsertGLAccount(4, "2010.00.40", "CLN", consolidationAccountKey: 3);
			Helper.InsertBASAccount(1);
			Helper.InsertBASAccount(2, "GL_BS_ACCOUNT_START");
			Helper.InsertGLAggregate(12, 202001, 1, "", 12);
			Helper.InsertGLAggregate(13, 202002, 1, "", 13);
			Helper.InsertGLAggregate(21, 202001, 2, "", 21);
			Helper.InsertGLAggregate(22, 202002, 2, "", 22);
			Helper.InsertAccountDescriptor(gLAccountKey: 2, alternativeDescriptorKey: 3);
			Helper.InsertAccountDescriptorPivot(gLAccountKey: 2);

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccMultilingualTrialBalanceAmountAndAlternateAmounts(202001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData GLAccountDescriptorKey", 1L, result.Rows[0]["GLAccountDescriptorKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData AlternativeDescriptorKey", 3L, result.Rows[0]["AlternativeDescriptorKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData CurrentAmount", 21m, result.Rows[0]["CurrentAmount"]);
			AssertEquals("GRP__GeneralLedgerAggregateData ClosingAmount", 21m, result.Rows[0]["ClosingAmount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
