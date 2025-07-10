using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_EXXOVPDSCTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "EXX", null, 345);
			TestHelper.InsertTransactionHeader("AP", "EXX", null, 245);
			TestHelper.InsertTransactionHeader("AR", "EXX", null, 100);
			TestHelper.InsertTransactionHeader("AR", "EXX", null, 200);

			TestHelper.InsertTransactionHeader("AP", "OVP", null, 445);
			TestHelper.InsertTransactionHeader("AP", "OVP", null, 345);
			TestHelper.InsertTransactionHeader("AR", "OVP", null, 120);
			TestHelper.InsertTransactionHeader("AR", "OVP", null, 220);

			TestHelper.InsertTransactionHeader("AP", "DSC", null, 315);
			TestHelper.InsertTransactionHeader("AP", "DSC", null, 215);
			TestHelper.InsertTransactionHeader("AR", "DSC", null, 150);
			TestHelper.InsertTransactionHeader("AR", "DSC", null, 250);

			var expected = new[]
			{
				(-2950m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(1040m, "", 200701, "SYD", "FIR", "EDI", "GL_AR_CONTROL_ACCOUNT"),
				(1910m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "EXX", invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("AP", "OVP", invoiceAmount: 11);
			TestHelper.InsertTransactionHeader("AP", "DSC", invoiceAmount: 12);
			TestHelper.InsertTransactionHeader("AP", "EXX", invoiceAmount: 10, useAnotherBranch: true);
			TestHelper.InsertTransactionHeader("AP", "OVP", invoiceAmount: 11, useAnotherBranch: true);
			TestHelper.InsertTransactionHeader("AP", "DSC", invoiceAmount: 12, useAnotherBranch: true);

			var expected = new[]
			{
				(-33m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(33m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-33m, "", 200701, "BNE", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(33m, "", 200701, "BNE", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "EXX", invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("AP", "OVP", invoiceAmount: 11);
			TestHelper.InsertTransactionHeader("AP", "DSC", invoiceAmount: 12);
			TestHelper.InsertTransactionHeader("AP", "EXX", invoiceAmount: 10, useAnotherDepartment: true);
			TestHelper.InsertTransactionHeader("AP", "OVP", invoiceAmount: 11, useAnotherDepartment: true);
			TestHelper.InsertTransactionHeader("AP", "DSC", invoiceAmount: 12, useAnotherDepartment: true);

			var expected = new[]
			{
				(-33m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(33m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-33m, "", 200701, "SYD", "FEA", "EDI", TestHelper.HeaderGLAccount.Name),
				(33m, "", 200701, "SYD", "FEA", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "EXX", invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("AP", "OVP", invoiceAmount: 11);
			TestHelper.InsertTransactionHeader("AP", "DSC", invoiceAmount: 12);
			TestHelper.InsertTransactionHeader("AP", "EXX", invoiceAmount: 10, useAnotherGLAccount: true);
			TestHelper.InsertTransactionHeader("AP", "OVP", invoiceAmount: 11, useAnotherGLAccount: true);
			TestHelper.InsertTransactionHeader("AP", "DSC", invoiceAmount: 12, useAnotherGLAccount: true);

			var expected = new[]
			{
				(-33m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(66m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-33m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			TestHelper.InsertTransactionHeader("AP", "EXX", invoiceAmount: 10);
			TestHelper.InsertTransactionHeader("AP", "OVP", invoiceAmount: 11);
			TestHelper.InsertTransactionHeader("AP", "DSC", invoiceAmount: 12);
			TestHelper.InsertTransactionHeader("AP", "EXX", invoiceAmount: 10, useNextPeriodPostDate: true);
			TestHelper.InsertTransactionHeader("AP", "OVP", invoiceAmount: 11, useNextPeriodPostDate: true);
			TestHelper.InsertTransactionHeader("AP", "DSC", invoiceAmount: 12, useNextPeriodPostDate: true);

			var expected = new[]
			{
				(-33m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(33m, "", 200701, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
				(-33m, "", 200702, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(33m, "", 200702, "SYD", "FIR", "EDI", "GL_AP_CONTROL_ACCOUNT"),
			};

			RunAndAssertAggregation(expected);
		}
	}
}
