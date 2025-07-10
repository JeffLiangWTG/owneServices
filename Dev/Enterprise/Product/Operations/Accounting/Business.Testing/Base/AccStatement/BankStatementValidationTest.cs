using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	internal class BankStatementValidationTest : AccBankAccountValidationTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			BranchABankAccountAUD = Factory.NewWithValidTestData<BankStatement>();
		}

		public void TestValidateStatementDateFilter()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementDateFilter = ZDateTime.Now;
			AssertEquals("HasErrors", false, bankStatement.StatementDateFilterInfo.HasErrors());

			bankStatement.StatementDateFilter = ZDateTime.Empty;
			AssertEquals("HasErrors", true, bankStatement.StatementDateFilterInfo.HasErrors());

			bankStatement.StatementDateFilter = ZDateTime.Invalid;
			AssertEquals("HasErrors", true, bankStatement.StatementDateFilterInfo.HasErrors());
		}

		public void TestValidateDebitCreditFilter()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			AssertEquals("HasErrors", false, bankStatement.DebitCreditFilterInfo.HasErrors());

			bankStatement.DebitCreditFilter = "XX";
			AssertEquals("HasErrors", true, bankStatement.DebitCreditFilterInfo.HasErrors());

			bankStatement.DebitCreditFilter = bankStatement.AB_DebitCredit_List[0].Code;
			AssertEquals("HasErrors", false, bankStatement.DebitCreditFilterInfo.HasErrors());
		}

		public void TestValidateTypeFilter()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			AssertEquals("HasErrors", false, bankStatement.TypeFilterInfo.HasErrors());

			bankStatement.TypeFilter = "XXX";
			AssertEquals("HasErrors", true, bankStatement.TypeFilterInfo.HasErrors());

			bankStatement.TypeFilter = bankStatement.AB_Type_List[0].Code;
			AssertEquals("HasErrors", false, bankStatement.TypeFilterInfo.HasErrors());
		}

		public void TestRunPreSaveValidationCore()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			bankStatement.StatementDateFilter = ZDateTime.Now;
			bankStatement.RunPreSaveValidation();
			AssertEquals("HasErrors", false, bankStatement.StatementDateFilterInfo.HasErrors());
			AssertEquals("HasErrors", false, bankStatement.DebitCreditFilterInfo.HasErrors());
			AssertEquals("HasErrors", false, bankStatement.TypeFilterInfo.HasErrors());
		}
	}
}
