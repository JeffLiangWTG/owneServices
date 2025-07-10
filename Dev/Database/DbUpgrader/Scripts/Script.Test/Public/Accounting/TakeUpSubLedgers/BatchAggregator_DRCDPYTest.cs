using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_DRCDPYTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			var h1pk = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK);
			TestHelper.InsertTransactionLine("DPY", h1pk, 2000, reverseDate: DateTime.MinValue);
			var h2pk = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK);
			TestHelper.InsertTransactionLine("DPY", h2pk, 500, reverseDate: DateTime.MinValue);

			var h3pk = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK);
			TestHelper.InsertTransactionLine("DRC", h3pk, 100, reverseDate: DateTime.MinValue);
			var h4pk = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK);
			TestHelper.InsertTransactionLine("DRC", h4pk, 150, reverseDate: DateTime.MinValue);

			var expected = new[]
			{
				(-2750m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),		//Actual Posting
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),			// GST Posting
				(-20m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),			// GST Posting
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),			// Not Recoverable GST Posting
				(2790m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),				// Control Account Posting
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			var header1PK = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK);
			TestHelper.InsertTransactionLine("DPY", header1PK, 10, reverseDate: DateTime.MinValue);
			var header2PK = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK, useAnotherBranch: true);
			TestHelper.InsertTransactionLine("DPY", header2PK, 10, reverseDate: DateTime.MinValue, useAnotherBranch: true);
			var header3PK = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK);
			TestHelper.InsertTransactionLine("DRC", header3PK, 11, reverseDate: DateTime.MinValue);
			var header4PK = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK, useAnotherBranch: true);
			TestHelper.InsertTransactionLine("DRC", header4PK, 11, reverseDate: DateTime.MinValue, useAnotherBranch: true);

			var expected = new[]
			{
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-5m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(41m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(-21m, "", 200701, "BNE", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-5m, "", 200701, "BNE", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-10m, "", 200701, "BNE", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(-5m, "", 200701, "BNE", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(41m, "", 200701, "BNE", "FIR", "EDI", BankGLAccount.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			var header1PK = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK);
			TestHelper.InsertTransactionLine("DPY", header1PK, 10, reverseDate: DateTime.MinValue);
			var header2PK = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK, useAnotherDepartment: true);
			TestHelper.InsertTransactionLine("DPY", header2PK, 10, reverseDate: DateTime.MinValue, useAnotherDepartment: true);
			var header3PK = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK);
			TestHelper.InsertTransactionLine("DRC", header3PK, 11, reverseDate: DateTime.MinValue);
			var header4PK = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK, useAnotherDepartment: true);
			TestHelper.InsertTransactionLine("DRC", header4PK, 11, reverseDate: DateTime.MinValue, useAnotherDepartment: true);

			var expected = new[]
			{
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-5m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(41m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(-21m, "", 200701, "SYD", "FEA", "EDI", TestHelper.LineGLAccount.Name),
				(-5m, "", 200701, "SYD", "FEA", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FEA", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FEA", "EDI", TestHelper.LineGLAccount.Name),
				(41m, "", 200701, "SYD", "FEA", "EDI", BankGLAccount.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			var header1PK = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK);
			TestHelper.InsertTransactionLine("DPY", header1PK, 10, reverseDate: DateTime.MinValue);
			var header2PK = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccount2PK, useAnotherGLAccount: true);
			TestHelper.InsertTransactionLine("DPY", header2PK, 10, reverseDate: DateTime.MinValue, useAnotherGLAccount: true);
			var header3PK = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK);
			TestHelper.InsertTransactionLine("DRC", header3PK, 11, reverseDate: DateTime.MinValue);
			var header4PK = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccount2PK, useAnotherGLAccount: true);
			TestHelper.InsertTransactionLine("DRC", header4PK, 11, reverseDate: DateTime.MinValue, useAnotherGLAccount: true);

			var expected = new[]
			{
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-20m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(41m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount2.Name),
				(-5m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount2.Name),
				(41m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			var header1PK = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK);
			TestHelper.InsertTransactionLine("DPY", header1PK, 10, reverseDate: DateTime.MinValue);
			var header2PK = TestHelper.InsertTransactionHeader("CB", "DPY", BankAccountPK, useNextPeriodPostDate: true);
			TestHelper.InsertTransactionLine("DPY", header2PK, 10, reverseDate: DateTime.MinValue, useNextPeriodPostDate: true);
			var header3PK = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK);
			TestHelper.InsertTransactionLine("DRC", header3PK, 11, reverseDate: DateTime.MinValue);
			var header4PK = TestHelper.InsertTransactionHeader("CB", "DRC", BankAccountPK, useNextPeriodPostDate: true);
			TestHelper.InsertTransactionLine("DRC", header4PK, 11, reverseDate: DateTime.MinValue, useNextPeriodPostDate: true);

			var expected = new[]
			{
				(-21m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-5m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(-5m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(41m, "", 200701, "SYD", "FIR", "EDI", BankGLAccount.Name),
				(-21m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(-5m, "", 200702, "SYD", "FIR", "EDI", "GL_GST_INPUT_ACCOUNT"),
				(-10m, "", 200702, "SYD", "FIR", "EDI", "GL_GST_OUTPUT_ACCOUNT"),
				(-5m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(41m, "", 200702, "SYD", "FIR", "EDI", BankGLAccount.Name),
			};

			RunAndAssertAggregation(expected);
		}
	}
}
