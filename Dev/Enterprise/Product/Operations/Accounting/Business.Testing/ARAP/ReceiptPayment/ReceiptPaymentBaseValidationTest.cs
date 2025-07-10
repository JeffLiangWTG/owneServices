using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class ReceiptPaymentBaseValidationTest : TransactionHeaderValidationTest
	{
		#region Data Refresh Bus Update Validation Tests

		#region AH_TransactionCategory

		public void TestAH_TransactionCategoryBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			var category1 = new ZString(InvoiceTypesList.Codes.DoNotPost);
			var category2 = new ZString(InvoiceTypesList.Codes.FinalInvoice);
			var category3 = new ZString(InvoiceTypesList.Codes.DisbursementInvoice);
			AssertPropertyBeingChangedByDataRefreshBus(AccTransactionHeaderSchema.Constants.AH_TransactionCategory, category1, category2, category3);
		}

		#endregion

		#endregion

		public void TestCheckAH_OH_IsValid()
		{
			var validAccount = TestReceiptPayment.AH_Ledger == LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			var invalidAccount = TestReceiptPayment.AH_Ledger != LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			TestReceiptPayment.AH_OH = invalidAccount;
			TestValidation.ValidateAH_OH();
			AssertHasError(TestReceiptPayment.AH_OHInfo, "Enter a valid Account.");
			TestReceiptPayment.AH_OH = validAccount;
			TestValidation.ValidateAH_OH();
			AssertNoErrors(TestReceiptPayment.AH_OHInfo);
		}

		public void TestValidateAH_ABWhenCurrencyChanges()
		{
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_IsDebtor = true;
			debtor.CompanyData.OB_AB_APDefaultBankAccount = TestObjectCreator.USDBankAccount.PK;
			debtor.CompanyData.OB_AB_ARPayToAccount = TestObjectCreator.USDBankAccount.PK;

			TestReceiptPayment.AH_OH = debtor.PK;
			AssertEquals("Payment / Receipt Currency should be same as the bank currency (USD)",
				TestObjectCreator.USDBankAccount.AB_RX_NKAccountCurrency,
				TestReceiptPayment.AH_RX_NKTransactionCurrency);

			AssertEquals("Should be no error on the bank account", ZString.Empty, TestReceiptPayment.AH_ABInfo.GetErrors().ToUniqueMessageListString());
			Assert("Should be no error on the bank account", !TestReceiptPayment.AH_ABInfo.HasErrors());
		}

		public void TestCheckAH_ABIfEmpty()
		{
			TestValidation.ValidateAH_AB();
			AssertHasError("Should have error", TestReceiptPayment.AH_ABInfo, "Please enter a Bank.");
		}

		public void TestCheckAH_ABIsValid()
		{
			TestReceiptPayment.AH_AB = ZGuid.Invalid;
			TestValidation.ValidateAH_AB();
			AssertHasError(TestReceiptPayment.AH_ABInfo, "Enter a valid Bank Account.");

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_GB = TestObjectCreator.NonCurrentBranch.PK;
			TestObjectCreator.AALSHI.CompanyData.OB_AB_ARPayToAccount = bankAccount.PK;
			TestObjectCreator.AALSHI.CompanyData.OB_AB_APDefaultBankAccount = bankAccount.PK;

			TestReceiptPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			AssertHasError(TestReceiptPayment.AH_ABInfo, "Enter a valid Bank Account.");

			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			TestReceiptPayment.AH_OH = ZGuid.Empty;
			TestReceiptPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			AssertNoErrors(TestReceiptPayment.AH_ABInfo);
		}

		public void TestCheckAH_ABProperCurrency()
		{
			AccBankAccount testBankForeign = TestObjectCreator.CreateBankAccount("TST", "TEST", TestObjectCreator.USD, null);
			TestReceiptPayment.AH_AB = testBankForeign.PK;
			TestReceiptPayment.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			TestValidation.ValidateAH_AB();
			AssertNoErrors(TestReceiptPayment.AH_ABInfo);

			AccBankAccount testBankLocal = TestObjectCreator.CreateBankAccount("TST", "TEST", TestObjectCreator.AUD, null);
			TestReceiptPayment.AH_AB = testBankLocal.PK;
			TestReceiptPayment.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			TestValidation.ValidateAH_AB();
			AssertNoErrors(TestReceiptPayment.AH_ABInfo);
		}

		#region TestCheckAH_ABProperCurrencyLocalBankForeign

		public void TestCheckAH_ABProperCurrencyLocalBankForeign()
		{
			var testBank = TestObjectCreator.CreateBankAccount("TST", "TEST", TestObjectCreator.AUD, null);

			TestReceiptPayment.AH_AB = testBank.PK;
			TestReceiptPayment.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			TestValidation.ValidateAH_AB();

			Assert(!TestReceiptPayment.AH_ABInfo.HasErrors());
		}

		#endregion

		public void TestCheckAH_ABCurrencyDoesNotMatch()
		{
			var testBank = TestObjectCreator.CreateBankAccount("TST", "TEST", TestObjectCreator.USD, null);

			TestReceiptPayment.AH_AB = testBank.PK;
			TestReceiptPayment.AH_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;

			TestValidation.ValidateAH_AB();

			AssertHasError(TestReceiptPayment.AH_ABInfo, string.Format("Bank account currency does not match the {0} currency.", TestReceiptPayment.HumanReadableName));

			testBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			TestValidation.ValidateAH_AB();
			AssertNoError(TestReceiptPayment.AH_ABInfo, string.Format("Bank account currency does not match the {0} currency.", TestReceiptPayment.HumanReadableName));
		}

		public void TestAH_ChequeOrReferenceValidation()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_ChequeNumDigits = 10;

			if (TestReceiptPayment is Payment)
			{
				TestReceiptPayment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
				TestReceiptPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				TestReceiptPayment.AH_AB = bank.PK;

				TestReceiptPayment.AH_ChequeOrReference = "ABC";
				AssertHasError(TestReceiptPayment.AH_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");

				TestReceiptPayment.AH_ChequeOrReference = "1".PadLeft(bank.AB_ChequeNumDigits, '0');
				AssertEquals("Has no error", false, TestReceiptPayment.AH_ChequeOrReferenceInfo.HasErrors());

				TestReceiptPayment.AH_ChequeOrReference = "1".PadLeft(bank.AB_ChequeNumDigits + 1, '0');
				AssertEquals("Has this error", true, TestReceiptPayment.AH_ChequeOrReferenceInfo.HasErrors());
			}
			else
			{
				TestReceiptPayment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Receipt;
				TestReceiptPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				TestReceiptPayment.AH_AB = bank.PK;

				TestReceiptPayment.AH_ChequeOrReference = "12345678901234567890";
				AssertEquals("Has no error", false, TestReceiptPayment.AH_ChequeOrReferenceInfo.HasErrors());
			}
		}

		public void TestCheckAH_PostDateNotInFuture()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			TestReceiptPayment.AH_PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError("AH_PostDate", TestReceiptPayment.AH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			TestReceiptPayment.AH_PostDate = ZDateTime.Now;
			AssertNoErrors("AH_PostDate", TestReceiptPayment.AH_PostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			TestReceiptPayment.AH_PostDate = ZDateTime.Now.AddDays(2);
			AssertHasError("AH_PostDate", TestReceiptPayment.AH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			TestReceiptPayment.AH_PostDate = ZDateTime.Now.AddDays(3);
			AssertNoErrors("AH_PostDate", TestReceiptPayment.AH_PostDateInfo);
		}

		public void TestAH_RX_NKTransactionCurrency()
		{
			var usdBankAcc = TestObjectCreator.CreateBankAccount("USD", "TEST USD ACC", TestObjectCreator.USD, null);
			var vndBankAcc = TestObjectCreator.CreateBankAccount("VND", "TEST VND ACC", TestObjectCreator.VND, null);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.VietNam))
			{
				TestReceiptPayment.AH_AB = usdBankAcc.PK;
				TestReceiptPayment.AH_ExchangeRate = 1.2568M;
				TestReceiptPayment.AH_OSExTaxAmount = 2560.25M;
				TestReceiptPayment.AH_OSTaxAmount = 256.02M;

				AssertEquals("OS Ex Tax Amount", 2560.25M, TestReceiptPayment.AH_OSExTaxAmount);
				AssertEquals("OS Tax Amount", 256.02M, TestReceiptPayment.AH_OSTaxAmount);
				AssertEquals("Local Ex Tax Amount", 2037M, TestReceiptPayment.AH_LocalExTaxAmount);
				AssertEquals("Local Tax Amount", 204M, TestReceiptPayment.AH_LocalTaxAmount);

				TestReceiptPayment.AH_AB = vndBankAcc.PK;
				AssertEquals("OS Ex Tax Amount", 2560M, TestReceiptPayment.AH_OSExTaxAmount);
				AssertEquals("OS Tax Amount", 256M, TestReceiptPayment.AH_OSTaxAmount);
				AssertEquals("Local Ex Tax Amount", 2560M, TestReceiptPayment.AH_LocalExTaxAmount);
				AssertEquals("Local Tax Amount", 256M, TestReceiptPayment.AH_LocalTaxAmount);
			}
		}

		protected abstract Type GetValidationParentType();

		protected ReceiptPaymentBase TestReceiptPayment
		{
			get { return TestReceiptPayment_cached ?? (TestReceiptPayment_cached = (ReceiptPaymentBase)Factory.NewWithValidTestData(GetValidationParentType())); }
		}
		ReceiptPaymentBase TestReceiptPayment_cached;

		protected ReceiptPaymentBaseValidation TestValidation
		{
			get { return (ReceiptPaymentBaseValidation)TestReceiptPayment.Validation; }
		}

		protected override Type HeaderType => GetValidationParentType();
	}
}
