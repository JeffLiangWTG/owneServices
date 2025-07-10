using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_CBTRFEXXTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("CB", "TRF", BankAccountPK, invoiceAmount: 100);
			TestHelper.InsertTransactionHeader("CB", "TRF", BankAccount2PK, invoiceAmount: -100);

			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 150);
			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 250);

			var expected = new[]
			{
				(-100m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount2.Name),
				(500m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(-400m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 10, useAnotherBranch: true);

			var expected = new[]
			{
				(10m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(10m, "", 200701, "BNE", "FIR", "EDI", BankGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "BNE", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 10, useAnotherDepartment: true);

			var expected = new[]
			{
				(10m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(10m, "", 200701, "SYD", "FEA", "EDI", BankGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "SYD", "FEA", "EDI", TestHelper.HeaderGLAccount.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccount2PK, invoiceAmount: 10, useAnotherGLAccount: true);

			var expected = new[]
			{
				(10m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(10m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount2.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("CB", "EXX", BankAccountPK, invoiceAmount: 10, useNextPeriodPostDate: true);

			var expected = new[]
			{
				(10m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(10m, "", 200702, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200702, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
			};

			RunAndAssertAggregation(expected);
		}
	}
}

