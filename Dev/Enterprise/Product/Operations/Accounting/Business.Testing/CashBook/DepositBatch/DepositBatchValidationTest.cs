using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	class DepositBatchValidationTest : TestCaseWithFactory
	{
		public void TestBranchDepartmentCombinationValidation_DepositBatchValidation()
		{
			var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var depositBatch = Factory.NewWithValidTestData<DepositBatch>();
			var validation = new DepositBatchValidation(depositBatch);

			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(depositBatch.Branch, new GlbDepartment[] { bbbDepartment });
			validation.ValidateAll();
			Assert("Should skip validation", !validation.ShouldValidateBranchDepartmentCombination_ForTestOnly);
			Assert("No errors found", !depositBatch.AH_GEInfo.HasNotifications());
		}

		public void TestCheckAH_PostDateNotInFuture()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			var testObjectCreator = new TestObjectCreator(Factory);
			var testReceipt = testObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cheque, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 500M, testObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			testBatch.AH_PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError("AH_PostDate", testBatch.AH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			testBatch.AH_PostDate = ZDateTime.Now;
			AssertNoErrors("AH_PostDate", testBatch.AH_PostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			testObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			testBatch.AH_PostDate = ZDateTime.Now.AddDays(2);
			AssertHasError("AH_PostDate", testBatch.AH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			testBatch.AH_PostDate = ZDateTime.Now.AddDays(3);
			AssertNoErrors("AH_PostDate", testBatch.AH_PostDateInfo);
		}
	}
}
