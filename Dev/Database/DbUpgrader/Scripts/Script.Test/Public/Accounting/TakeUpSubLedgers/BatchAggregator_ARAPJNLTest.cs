using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_ARAPJNLTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 333);
			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 111);
			TestHelper.InsertTransactionHeader("AR", "JNL", invoiceAmount: 444);
			TestHelper.InsertTransactionHeader("AR", "JNL", invoiceAmount: 222);

			var expected = new[]
			{
				(-1110m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(444m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(666m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 10, useAnotherBranch: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(10m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "BNE", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(10m, "", 200701, "BNE", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 10, useAnotherDepartment: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(10m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FEA", "EDI", TestHelper.HeaderGLAccount.Name),
				(10m, "", 200701, "SYD", "FEA", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 10, useAnotherGLAccount: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(20m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("AP", "JNL", invoiceAmount: 10, useNextPeriodPostDate: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(10m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-10m, "", 200702, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(10m, "", 200702, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}
	}
}
