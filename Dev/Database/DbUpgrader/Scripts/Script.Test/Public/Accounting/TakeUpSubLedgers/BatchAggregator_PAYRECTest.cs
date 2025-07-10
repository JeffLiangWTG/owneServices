using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_PAYRECTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 130m);
			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 70m);
			TestHelper.InsertTransactionHeader("AR", "PAY", BankAccountPK, 150);
			TestHelper.InsertTransactionHeader("AP", "REC", BankAccountPK, 175);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 400);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 21);

			var expected = new[]
			{
				(-946m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(+350m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(+596m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 10m);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 11);
			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 10m, useAnotherBranch: true);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 11, useAnotherBranch: true);

			var expected = new[]
			{
				(-21m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(+11m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-21m, "", 200701, "BNE", "FIR", "EDI", BankGLAccount.Name),
				(+10m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(+11m, "", 200701, "BNE", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 10m);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 11);
			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 10m, useAnotherDepartment: true);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 11, useAnotherDepartment: true);

			var expected = new[]
			{
				(-21m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(+11m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-21m, "", 200701, "SYD", "FEA", "EDI", BankGLAccount.Name),
				(+10m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(+11m, "", 200701, "SYD", "FEA", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 10m);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 11);
			TestHelper.InsertTransactionHeader("AR", "REC", BankAccount2PK, 10m, useAnotherGLAccount: true);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccount2PK, 11, useAnotherGLAccount: true);

			var expected = new[]
			{
				(-21m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(+20m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(+22m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-21m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 10m);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 11);
			TestHelper.InsertTransactionHeader("AR", "REC", BankAccountPK, 10m, useNextPeriodPostDate: true);
			TestHelper.InsertTransactionHeader("AP", "PAY", BankAccountPK, 11, useNextPeriodPostDate: true);

			var expected = new[]
			{
				(-21m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(+10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(+11m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-21m, "", 200702, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(+10m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(+11m, "", 200702, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}
	}
}

