using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_TakeUpOtherTaxesTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			var debitGLAccountPK = DbHelper.InsertGLAccount("1001.10.10", "Debit Account");
			var creditGLAccountPK = DbHelper.InsertGLAccount("1001.10.20", "Credit Account");
			var transactionHeaderPK = DbHelper.InsertTransactionHeader("AR", "INV", "001001", 110, TestHelper.DefaultPostDate, TestHelper.Branch.PK, TestHelper.Department.PK);
			var taxTransactionPK = DbHelper.InsertTaxTransaction(transactionHeaderPK, TestDbHelper.DefaultCompanyPK, TestHelper.Branch.PK, TestHelper.Department.PK, TaxConfigurationPK, TaxIdPK);
			DbHelper.InsertTaxGLMovement(taxTransactionPK, debitGLAccountPK, creditGLAccountPK, 10, period: 200701, date: TestHelper.DefaultPostDate);

			var expected = new[]
			{
				(+10m, "", 200701, "SYD", "FIR", "EDI", "Debit Account"),
				(-10m, "", 200701, "SYD", "FIR", "EDI", "Credit Account")
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			PrepareForAggregation();

			InsertTaxTransaction();
			InsertTaxTransaction(useAnotherBranch: true);

			var expected = new[]
			{
				(+10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200701, "BNE", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "BNE", "FIR", "EDI", TestHelper.LineGLAccount.Name)
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			PrepareForAggregation();

			InsertTaxTransaction();
			InsertTaxTransaction(useAnotherDepartment: true);

			var expected = new[]
			{
				(+10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200701, "SYD", "FEA", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "SYD", "FEA", "EDI", TestHelper.LineGLAccount.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			PrepareForAggregation();

			InsertTaxTransaction();
			InsertTaxTransaction(useAnotherGLAccount: true);

			var expected = new[]
			{
				(+10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount2.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount2.Name),
			};

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			PrepareForAggregation();

			InsertTaxTransaction();
			InsertTaxTransaction(useNextPeriodPostDate: true);

			var expected = new[]
			{
				(+10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200701, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name),
				(+10m, "", 200702, "SYD", "FIR", "EDI", TestHelper.HeaderGLAccount.Name),
				(-10m, "", 200702, "SYD", "FIR", "EDI", TestHelper.LineGLAccount.Name)
			};

			RunAndAssertAggregation(expected);
		}

		void InsertTaxTransaction(
			bool useAnotherBranch = false,
			bool useAnotherDepartment = false,
			bool useAnotherGLAccount = false,
			bool useNextPeriodPostDate = false)
		{
			var branchPK = useAnotherBranch ? TestHelper.Branch2.PK : TestHelper.Branch.PK;
			var departmentPK = useAnotherDepartment ? TestHelper.Department2.PK : TestHelper.Department.PK;
			var glAccountPK1 = useAnotherGLAccount ? TestHelper.HeaderGLAccount2.PK : TestHelper.HeaderGLAccount.PK;
			var glAccountPK2 = useAnotherGLAccount ? TestHelper.LineGLAccount2.PK : TestHelper.LineGLAccount.PK;
			var postDate = useNextPeriodPostDate ? TestHelper.DefaultPostDate.AddMonths(1) : TestHelper.DefaultPostDate;
			var period = useNextPeriodPostDate ? 200702 : 200701;

			var transactionHeaderPK = DbHelper.InsertTransactionHeader("AP", "CRD", Guid.NewGuid().ToString(), 110, TestHelper.DefaultPostDate, TestHelper.Branch.PK, TestHelper.Department.PK);
			var taxTransactionPK = DbHelper.InsertTaxTransaction(transactionHeaderPK, TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, TaxConfigurationPK, TaxIdPK);
			DbHelper.InsertTaxGLMovement(taxTransactionPK, glAccountPK1, glAccountPK2, 10, period: period, date: postDate);
		}

		Guid TaxConfigurationPK => (Guid)(taxConfigurationPK ?? (taxConfigurationPK = DbHelper.InsertTaxConfiguration("ADT", TestDbHelper.DefaultCompanyPK)));
		Guid? taxConfigurationPK;

		Guid TaxIdPK => (Guid)(taxIdPK ?? (taxIdPK = DbHelper.InsertTaxRate("TAX1")));
		Guid? taxIdPK;
	}
}

