using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_WIPACRTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionLine("WIP", lineAmount: 1);
			TestHelper.InsertTransactionLine("WIP", lineAmount: 10);
			TestHelper.InsertTransactionLine("WIP", lineAmount: 100);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 10000);

			var expected = new[]
			{
				(+10111m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-10111m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				//WIP
				(-111m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(+111m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				//ACR
				(-10000m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(+10000m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionLine("WIP", lineAmount: 10);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 11);
			TestHelper.InsertTransactionLine("WIP", lineAmount: 10, useAnotherBranch: true);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 11, useAnotherBranch: true);

			var expected = new[]
			{
				(+21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+21m, "", 200701, "BNE", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-21m, "", 200701, "BNE", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				//WIP
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(-10m, "", 200701, "BNE", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(+10m, "", 200701, "BNE", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				//ACR
				(-11m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(+11m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(-11m, "", 200701, "BNE", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(+11m, "", 200701, "BNE", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionLine("WIP", lineAmount: 10);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 11);
			TestHelper.InsertTransactionLine("WIP", lineAmount: 10, useAnotherDepartment: true);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 11, useAnotherDepartment: true);

			var expected = new[]
			{
				(+21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+21m, "", 200701, "SYD", "FEA", "EDI", TestHelper.LineGLAccount.Name),
				(-21m, "", 200701, "SYD", "FEA", "EDI", TestHelper.LineGLAccount.Name),
				//WIP
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FEA", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(+10m, "", 200701, "SYD", "FEA", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				//ACR
				(-11m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(+11m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(-11m, "", 200701, "SYD", "FEA", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(+11m, "", 200701, "SYD", "FEA", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionLine("WIP", lineAmount: 10);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 11);
			TestHelper.InsertTransactionLine("WIP", lineAmount: 10, useAnotherGLAccount: true);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 11, useAnotherGLAccount: true);

			var expected = new[]
			{
				(+21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount2.Name),
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount2.Name),
				//WIP
				(-20m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(+20m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				//ACR
				(-22m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(+22m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionLine("WIP", lineAmount: 10);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 11);
			TestHelper.InsertTransactionLine("WIP", lineAmount: 10, useNextPeriodPostDate: true, useNextPeriodReverseDate: true);
			TestHelper.InsertTransactionLine("ACR", lineAmount: 11, useNextPeriodPostDate: true, useNextPeriodReverseDate: true);

			var expected = new[]
			{
				(+21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+21m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-21m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				//WIP
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(-10m, "", 200702, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				(+10m, "", 200702, "SYD", "FIR", "EDI", "GL_ACCRUED_REVENUE_ACCOUNT"),
				//ACR
				(-11m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(+11m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(-11m, "", 200702, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(+11m, "", 200702, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public void TestAggregationOfREversalsInDifferentPeriodOnSecondRun()
		{
			PrepareForAggregation();

			var linePK = TestHelper.InsertTransactionLine("ACR", lineAmount: 150, reverseDate: DateTime.MinValue);

			var expected = new[]
			{
				(+150m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-150m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);

			expected = new[]
			{
				(+150m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-150m, "", 200701, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
				(-150m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+150m, "", 200702, "SYD", "FIR", "EDI", "GL_ACCRUED_COST_ACCOUNT"),
			};

			TestHelper.UpdateTransactionLineReveseDate(linePK, TestHelper.DefaultPostDate.AddMonths(1));
			RunAndAssertAggregation(expected);
		}
	}
}
