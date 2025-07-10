using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_ARAPCTRTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "CTR", invoiceAmount: 222);
			TestHelper.InsertTransactionHeader("AR", "CTR", invoiceAmount: -222);

			TestHelper.InsertTransactionHeader("AP", "CTR", invoiceAmount: 111);
			TestHelper.InsertTransactionHeader("AR", "CTR", invoiceAmount: -111);

			var expected = new[]
			{
				(333m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-333m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "CTR", invoiceAmount: -10);
			TestHelper.InsertTransactionHeader("AR", "CTR", invoiceAmount: 10);

			TestHelper.InsertTransactionHeader("AP", "CTR", invoiceAmount: -10, useAnotherBranch: true);
			TestHelper.InsertTransactionHeader("AR", "CTR", invoiceAmount: 10, useAnotherBranch: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "BNE", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(10m, "", 200701, "BNE", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "CTR", invoiceAmount: -10);
			TestHelper.InsertTransactionHeader("AR", "CTR", invoiceAmount: 10);

			TestHelper.InsertTransactionHeader("AP", "CTR", invoiceAmount: -10, useAnotherDepartment: true);
			TestHelper.InsertTransactionHeader("AR", "CTR", invoiceAmount: 10, useAnotherDepartment: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FEA", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(10m, "", 200701, "SYD", "FEA", "EDI", "GL_AR_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			Assert("Not applicable as it uses only control account by ledger", true);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "CTR", invoiceAmount: -10);
			TestHelper.InsertTransactionHeader("AR", "CTR", invoiceAmount: 10);

			TestHelper.InsertTransactionHeader("AP", "CTR", invoiceAmount: -10, useNextPeriodPostDate: true);
			TestHelper.InsertTransactionHeader("AR", "CTR", invoiceAmount: 10, useNextPeriodPostDate: true);

			var expected = new[]
			{
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(10m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(-10m, "", 200702, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(10m, "", 200702, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}
	}
}

