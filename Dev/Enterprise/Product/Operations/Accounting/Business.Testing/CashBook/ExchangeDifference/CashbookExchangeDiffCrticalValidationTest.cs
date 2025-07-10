using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	public class CashbookExchangeDiffCrticalValidationTest : TransactionHeaderCriticalValidationTest
	{
		public void TestCheckGeneralLedgerAccount_GainAccountSuccessful()
		 => TestCheckGeneralLedgerAccount_GainAccount(true);
		public void TestCheckGeneralLedgerAccount_GainAccountFail()
		 => TestCheckGeneralLedgerAccount_GainAccount(false);

		void TestCheckGeneralLedgerAccount_GainAccount(bool expectSucessful)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var bankAccount = testObjectCreator.AUDBankAccount;

			testObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, -1, bankAccount.PK);

			Factory.Save();

			var cashBook = testObjectCreator.CreateCashbookExchangeDifference(ZDateTime.Today, 0m, bankAccount, false);
			cashBook.AH_TransactionNum = "Test0000001";
			cashBook.AH_RX_NKTransactionCurrency = testObjectCreator.USD.Code;

			if (!expectSucessful)
			{
				var tempFactory = new BusinessObjectFactory();
				tempFactory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.ForeignCurrencyGLBalanceAdjustmentAccount))?.Delete();
				tempFactory.Save();
				AccountingConfigurationRegistry.Instance
					.CurrencyAdjustmentExchangeGainAccount.SetValue(
						Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.DefaultValue
					);
				var cvExp = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());
				AssertContains("Correct error message should be generated", CriticalValidationMessageTemplate.CashBookExchangeTransactionNeedGeneralLedgerAccountErrorMessage, cvExp.Message);
				ErrorReporter.Clear();
			}
			else
			{
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		public void TestCheckGeneralLedgerAccount_LossAccountSuccessful()
			=> TestCheckGeneralLedgerAccount_LossAccount(true);

		public void TestCheckGeneralLedgerAccount_LossAccountFail()
			=> TestCheckGeneralLedgerAccount_LossAccount(false);

		void TestCheckGeneralLedgerAccount_LossAccount(bool expectSucessful)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var bankAccount = testObjectCreator.AUDBankAccount;

			testObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 1, bankAccount.PK);
			Factory.Save();

			var cashBook = testObjectCreator.CreateCashbookExchangeDifference(ZDateTime.Today, 0m, bankAccount, false);
			cashBook.AH_TransactionNum = "Test0000001";
			cashBook.AH_RX_NKTransactionCurrency = testObjectCreator.USD.Code;

			if (!expectSucessful)
			{
				var tempFactory = new BusinessObjectFactory();
				tempFactory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.ForeignCurrencyGLBalanceAdjustmentAccount))?.Delete();
				tempFactory.Save();
				AccountingConfigurationRegistry.Instance
					.CurrencyAdjustmentExchangeLossAccount.SetValue(
						Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.DefaultValue
					);
				var cvExp = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());
				AssertContains("Correct error message should be generated", CriticalValidationMessageTemplate.CashBookExchangeTransactionNeedGeneralLedgerAccountErrorMessage, cvExp.Message);
				ErrorReporter.Clear();
			}
			else
			{
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}
	}
}
