using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class ControlAccountProviderTest : TestCaseWithFactory
	{
		public void TestControlAccountForAR()
		{
			ARInvoice testTranscation = Factory.NewWithValidTestData<ARInvoice>();
			ControlAccountProvider testControlAccountProvider = new ControlAccountProvider();
			testControlAccountProvider.SetTransaction(testTranscation);
			AssertEquals(AccountingConfigurationRegistry.Instance.ARControlAccount.Value, testControlAccountProvider.PK);
			AssertEquals(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value, testControlAccountProvider.GST);
		}

		public void TestControlAccountForAP()
		{
			APInvoice testTranscation = Factory.NewWithValidTestData<APInvoice>();
			ControlAccountProvider testControlAccountProvider = new ControlAccountProvider();
			testControlAccountProvider.SetTransaction(testTranscation);
			AssertEquals(AccountingConfigurationRegistry.Instance.APControlAccount.Value, testControlAccountProvider.PK);
			AssertEquals(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, testControlAccountProvider.GST);
		}

		public void TestControlAccountForDPY()
		{
			AccTransactionHeader testTranscation = Factory.NewWithValidTestData<AccTransactionHeader>();
			testTranscation.AH_Ledger = LedgerTypes.CashBook;
			testTranscation.AH_TransactionType = TransactionTypes.DirectPayment;
			ControlAccountProvider testControlAccountProvider = new ControlAccountProvider();
			testControlAccountProvider.SetTransaction(testTranscation);
			AssertEquals(ZGuid.Empty, testControlAccountProvider.PK);
			AssertEquals(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, testControlAccountProvider.GST);
		}

		public void TestControlAccountForDRC()
		{
			AccTransactionHeader testTranscation = Factory.NewWithValidTestData<AccTransactionHeader>();
			testTranscation.AH_Ledger = LedgerTypes.CashBook;
			testTranscation.AH_TransactionType = TransactionTypes.DirectReceipt;
			ControlAccountProvider testControlAccountProvider = new ControlAccountProvider();
			testControlAccountProvider.SetTransaction(testTranscation);
			AssertEquals(ZGuid.Empty, testControlAccountProvider.PK);
			AssertEquals(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value, testControlAccountProvider.GST);
		}

		[TestDate(2019, 12, 18)]
		public void TestControlAccountPKForEXXOfCB()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var currencyAdjustmentExchangeGainAccount = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount;
			var realizedExchangeDifferencesAccount = AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount;
			using (currencyAdjustmentExchangeGainAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.ExchangeGainLossControlAccount.PK.ToGuid()))
			using (realizedExchangeDifferencesAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.ExchangeGainLossAdjustmentAccount.PK.ToGuid()))
			{
				AssertNotEquals("Precondition", currencyAdjustmentExchangeGainAccount.Value, realizedExchangeDifferencesAccount.Value);

				testObjectCreator.CreateExchangeRate(testObjectCreator.USD, 1.5m);
				var cb = testObjectCreator.CreateCashbookExchangeDifference(ZDateTime.Today, 100m, testObjectCreator.USD.Code);

				ControlAccountProvider testControlAccountProvider = new ControlAccountProvider();

				AssertNotEquals("Precondition", realizedExchangeDifferencesAccount.Value, cb.AH_AG);
				AssertEquals("Precondition", currencyAdjustmentExchangeGainAccount.Value, cb.AH_AG);
				testControlAccountProvider.SetTransaction(cb);
				AssertNotEquals(realizedExchangeDifferencesAccount.Value, testControlAccountProvider.PK);
				AssertEquals(cb.AH_AG, testControlAccountProvider.PK);
			}
		}
	}
}