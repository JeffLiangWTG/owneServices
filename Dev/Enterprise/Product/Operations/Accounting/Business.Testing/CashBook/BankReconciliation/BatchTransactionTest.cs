using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BatchTransaction))]
	public class BatchTransactionTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesBatchTransaction()
		{
			BatchTransaction testTransaction = Factory.New(typeof(BatchTransaction)) as BatchTransaction;

			var osList = new List<string>
				{
					nameof(testTransaction.Credit),
					nameof(testTransaction.Debit),
				};

			var exList = new List<string>
				{
					nameof(testTransaction.AH_ExchangeRate)
				};

			var tester = new DecimalPlacesAttributeTester(testTransaction, testTransaction.Company);
			tester.CheckNonLocalCurrency(osList, nameof(testTransaction.OSCurrencyDecimals), nameof(testTransaction.AH_RX_NKTransactionCurrency), testTransaction);
			tester.CheckExchangeRate(exList, nameof(testTransaction.ExchangeRateDecimalPlaces));
		}

		public void TestDebit()
		{
			BatchTransaction testTransaction = Factory.New(typeof(BatchTransaction)) as BatchTransaction;

			testTransaction.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Receipt;
			testTransaction.AH_OSTotal = -120.0m;

			AssertEquals(120.0m, testTransaction.Debit);
			AssertEquals(0m, testTransaction.Credit);

			testTransaction.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.DirectReceipt;
			testTransaction.AH_OSTotal = 120.0m;

			AssertEquals(120.0m, testTransaction.Debit);
			AssertEquals(0m, testTransaction.Credit);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestDeleteTransactionInDB()
		{
			BatchTransaction testTransaction = Factory.NewWithValidTestData<BatchTransaction>();
			Factory.Save();
			testTransaction.Delete();
		}

		public void TestDeleteTransactionNOTInDB()
		{
			BatchTransaction testTransaction = Factory.New<BatchTransaction>();
			testTransaction.Delete();
			AssertEquals("Must be deleted.", true, testTransaction.IsDeleted);
		}
	}
}
