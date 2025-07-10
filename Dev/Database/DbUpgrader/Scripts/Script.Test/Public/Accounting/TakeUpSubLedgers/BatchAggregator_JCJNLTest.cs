using System;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_JCJNLTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			var h1pk = TestHelper.InsertTransactionHeader("JC", "JNL");
			TestHelper.InsertTransactionLine("REV", h1pk, 150);
			var h2pk = TestHelper.InsertTransactionHeader("JC", "JNL");
			TestHelper.InsertTransactionLine("REV", h2pk, 250);
			var h3pk = TestHelper.InsertTransactionHeader("JC", "JNL");
			TestHelper.InsertTransactionLine("REV", h3pk, 350);

			var expected = new[]
			{
				(-750m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+750m, "", 200701, "SYD", "FIR", "EDI", "GL_CFX_ACCOUNT"),
				(-750m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+750m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JNL");
			TestHelper.InsertTransactionLine("REV", headerPK, 10);
			var headerPK2 = TestHelper.InsertTransactionHeader("JC", "JNL", useAnotherBranch: true);
			TestHelper.InsertTransactionLine("REV", headerPK2, 10, useAnotherBranch: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_CFX_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+10m, "", 200701, "BNE", "FIR", "EDI", "GL_CFX_ACCOUNT"),
				(-10m, "", 200701, "BNE", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JNL");
			TestHelper.InsertTransactionLine("REV", headerPK, 10);
			var headerPK2 = TestHelper.InsertTransactionHeader("JC", "JNL", useAnotherDepartment: true);
			TestHelper.InsertTransactionLine("REV", headerPK2, 10, useAnotherDepartment: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_CFX_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+10m, "", 200701, "SYD", "FEA", "EDI", "GL_CFX_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FEA", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JNL");
			TestHelper.InsertTransactionLine("REV", headerPK, 10);
			var headerPK2 = TestHelper.InsertTransactionHeader("JC", "JNL", useAnotherGLAccount: true);
			TestHelper.InsertTransactionLine("REV", headerPK2, 10, useAnotherGLAccount: true);

			var expected = new[]
			{
				(-20m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+20m, "", 200701, "SYD", "FIR", "EDI", "GL_CFX_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+20m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JNL");
			TestHelper.InsertTransactionLine("REV", headerPK, 10);
			var headerPK2 = TestHelper.InsertTransactionHeader("JC", "JNL", useNextPeriodPostDate: true);
			TestHelper.InsertTransactionLine("REV", headerPK2, 10, useNextPeriodPostDate: true, useNextPeriodReverseDate: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_CFX_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(-10m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+10m, "", 200702, "SYD", "FIR", "EDI", "GL_CFX_ACCOUNT"),
				(-10m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public void TestAggregationOfREversalsInDifferentPeriodOnSecondRun()
		{
			PrepareForAggregation();

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JNL");
			var linePK = TestHelper.InsertTransactionLine("REV", headerPK, 150, reverseDate: DateTime.MinValue);

			var expected = new[]
			{
				(-150m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
				(+150m, "", 200701, "SYD", "FIR", "EDI", "GL_CFX_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);

			expected = expected.Concat(new[]
			{
				(-150m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+150m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_SUSPENSE_CONTROL_ACCOUNT"),
			}).ToArray();

			TestHelper.UpdateTransactionLineReveseDate(linePK, TestHelper.DefaultPostDate.AddMonths(1));
			RunAndAssertAggregation(expected);
		}

		public void TestCFXAccountValidationPeriodValidation()
		{
			PrepareForAggregation(skipCFXAccountSetup: true);

			var headerPK = TestHelper.InsertTransactionHeader("JC", "JNL");
			TestHelper.InsertTransactionLine("REV", headerPK, 150);

			var errorMessage = RunAndAssertAggregationWithError(true);
			var expectedErrorMessage = "Please set up the following Control Accounts in the registry: Accounting > General Ledger Defaults > Control Account > CFX Account";
			AssertEquals(expectedErrorMessage, errorMessage);
		}
	}
}

