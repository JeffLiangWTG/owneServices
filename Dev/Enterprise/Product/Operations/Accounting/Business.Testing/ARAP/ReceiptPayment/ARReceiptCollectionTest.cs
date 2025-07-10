using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(ARReceiptCollection))]
	public class ARReceiptCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			ARReceipt obj1 = Factory.New<ARReceipt>();
			TestCollection.Add(obj1);
			AssertEquals("Index 0", obj1, TestCollection[0]);

			ARReceipt obj2 = Factory.New<ARReceipt>();
			TestCollection.Add(obj2);
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		public void TestForeignTotalAmount()
		{
			ARReceipt testReceipt1 = TestCollection.AddNew();
			ARReceipt testReceipt2 = TestCollection.AddNew();
			AssertEquals("ForeignTotalAmount should be zero", 0M, TestCollection.ForeignTotalAmount);
			testReceipt1.AH_OSExTaxAmount = 100M;
			AssertEquals("ForeignTotalAmount should be changed", 100M, TestCollection.ForeignTotalAmount);
			testReceipt2.AH_OSExTaxAmount = 58.77M;
			AssertEquals("ForeignTotalAmount should be changed", 158.77M, TestCollection.ForeignTotalAmount);
		}

		public void TestLocalTotalAmount()
		{
			ARReceipt testReceipt1 = TestCollection.AddNew();
			ARReceipt testReceipt2 = TestCollection.AddNew();
			AssertEquals("ForeignTotalAmount should be zero", 0M, TestCollection.LocalTotalAmount);
			testReceipt1.AH_LocalExTaxAmount = 100M;
			AssertEquals("ForeignTotalAmount should be changed", 100M, TestCollection.LocalTotalAmount);
			testReceipt2.AH_LocalExTaxAmount = 58.77M;
			AssertEquals("ForeignTotalAmount should be changed", 158.77M, TestCollection.LocalTotalAmount);
		}

		public void TestSetDefaultsForNewChild() => AssertSetDefaultsForNewChild(ZArchitecture.Core.ReceiptTypes.Cheque, isReceiptTypeReadonly: false, isCashAccount: false);

		public void TestSetDefaultsForNewChildForCashReceipt() => AssertSetDefaultsForNewChild(ZArchitecture.Core.ReceiptTypes.Cash, isReceiptTypeReadonly: true, isCashAccount: true);

		void AssertSetDefaultsForNewChild(string receiptyType, bool isReceiptTypeReadonly, bool isCashAccount)
		{
			ARReceiptBatchPoster batchPoster = new ARReceiptBatchPoster(Factory);
			TestCollection = new ARReceiptCollection(Factory, batchPoster);
			TestCollection.SetReceiptHeaderDataIsReadOnly();
			RefCurrency newCurrency = Factory.NewWithValidTestData<RefCurrency>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			if (isCashAccount)
			{
				testBank.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			}
			TestCollection.DefaultIncludeInDepositBatch_ForTestOnly = ZBool.True;
			batchPoster.BankAccountPK = testBank.PK;
			batchPoster.SellExRate = 35M;
			ZDateTime invoiceDateForTest = ZDateTime.Now.AddDays(5).Date;
			batchPoster.InvoiceDate = invoiceDateForTest;
			ZDateTime postDateForTest = ZDateTime.Now.AddDays(-3).Date;
			batchPoster.PostDate = postDateForTest;
			batchPoster.ReceiptType = receiptyType;
			batchPoster.Description = "BLAH!";

			ARReceipt testReceipt = TestCollection.AddNew();
			Assert("IncludeInDepositBatch should be defaulted", testReceipt.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be read only", !testReceipt.IncludeInDepositBatchInfo.ReadOnly);
			AssertEquals("Bank Account should be defaulted", testBank.PK, testReceipt.AH_AB);
			AssertEquals("Description should be defaulted", "BLAH!", testReceipt.AH_Desc);
			AssertEquals("Exchange Rate should be defaulted", 35M, testReceipt.AH_ExchangeRate);
			AssertEquals("Currency should be defaulted", testBank.AB_RX_NKAccountCurrency, testReceipt.AH_RX_NKTransactionCurrency);
			AssertEquals("Invoice Date should be defaulted", invoiceDateForTest, testReceipt.AH_InvoiceDate);
			AssertEquals("Post Date should be defaulted", postDateForTest, testReceipt.AH_PostDate);
			AssertEquals("Receipt type should be defaulted", receiptyType, testReceipt.AH_ReceiptType);

			Assert(testReceipt.AH_InvoiceDateInfo.ReadOnly);
			Assert(testReceipt.AH_PostDateInfo.ReadOnly);
			AssertEquals(isReceiptTypeReadonly, testReceipt.AH_ReceiptTypeInfo.ReadOnly);
			Assert(testReceipt.AH_ABInfo.ReadOnly);
			Assert(testReceipt.AH_ExchangeRateInfo.ReadOnly);
			Assert(testReceipt.AH_RX_NKTransactionCurrencyInfo.ReadOnly);

			batchPoster.BankAccountPK = testBank.PK;
			testReceipt = TestCollection.AddNew();
			AssertEquals("Bank Account should be defaulted", testBank.PK, testReceipt.AH_AB);
			AssertEquals("Currency should be defaulted to Bank Account's currency", testBank.AB_RX_NKAccountCurrency, testReceipt.AH_RX_NKTransactionCurrency);
		}

		public void TestSetReceiptHeaderDataIsReadOnly()
		{
			ARReceipt testReceipt = TestCollection.AddNew();
			Assert(!testReceipt.AH_InvoiceDateInfo.ReadOnly);
			Assert(!testReceipt.AH_ReceiptTypeInfo.ReadOnly);
			Assert(!testReceipt.AH_ABInfo.ReadOnly);

			TestCollection.SetReceiptHeaderDataIsReadOnly();
			testReceipt = TestCollection.AddNew();
			Assert(testReceipt.AH_InvoiceDateInfo.ReadOnly);
			Assert(testReceipt.AH_PostDateInfo.ReadOnly);
			Assert(!testReceipt.AH_ReceiptTypeInfo.ReadOnly);
			Assert(testReceipt.AH_ABInfo.ReadOnly);
			Assert(testReceipt.AH_ExchangeRateInfo.ReadOnly);
			Assert(testReceipt.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
		}

		public void TestSetIncludeInDepositBatchForAllReceipts()
		{
			ARReceiptBatchPoster batchPoster = new ARReceiptBatchPoster(Factory);
			TestCollection = new ARReceiptCollection(Factory, batchPoster);
			ARReceipt testReceipt1 = TestCollection.AddNew();
			ARReceipt testReceipt2 = TestCollection.AddNew();
			ARReceipt testReceipt3 = TestCollection.AddNew();
			testReceipt3.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testReceipt1.IncludeInDepositBatch = ZBool.True;
			TestCollection.SetIncludeInDepositBatchForAllReceipts(ZBool.False);
			Assert("IncludeInDepositBatch should be updated for TestReceipt1", !testReceipt1.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be updated for TestReceipt2", !testReceipt2.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be updated for TestReceipt3", !testReceipt3.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be read only", testReceipt1.IncludeInDepositBatchInfo.ReadOnly);
			Assert("IncludeInDepositBatch should be read only", testReceipt2.IncludeInDepositBatchInfo.ReadOnly);
			Assert("IncludeInDepositBatch should be read only", testReceipt3.IncludeInDepositBatchInfo.ReadOnly);
			ARReceipt testReceipt4 = TestCollection.AddNew();
			Assert("IncludeInDepositBatch should be set correctly for new receipt", !testReceipt4.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be set read only for new receipt", testReceipt4.IncludeInDepositBatchInfo.ReadOnly);

			TestCollection.SetIncludeInDepositBatchForAllReceipts(ZBool.True);
			Assert("IncludeInDepositBatch should be updated for TestReceipt1", testReceipt1.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be updated for TestReceipt2", testReceipt2.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be updated for TestReceipt3", !testReceipt3.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be read only", !testReceipt1.IncludeInDepositBatchInfo.ReadOnly);
			Assert("IncludeInDepositBatch should not be read only", !testReceipt2.IncludeInDepositBatchInfo.ReadOnly);
			Assert("IncludeInDepositBatch should be read only", testReceipt3.IncludeInDepositBatchInfo.ReadOnly);
			testReceipt4 = TestCollection.AddNew();
			Assert("IncludeInDepositBatch should be set correctly for new receipt", testReceipt4.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be set to read only for new receipt", !testReceipt4.IncludeInDepositBatchInfo.ReadOnly);
		}

		public void TestOnAmountOnChildChanged()
		{
			TestCollection = new ARReceiptCollection(Factory, Factory.NewWithValidTestData<ARReceipt>());

			try
			{
				TestCollection.OnAmountOnChildChanged += new ARReceiptCollection.OnAmountOnChildChangedHandler(TestCollection_OnAmountOnChildChanged);
				ARReceipt testReceipt1 = TestCollection.AddNew();
				Assert("Refreshing should not be called yet", !AmountOnChildChangedFired);
				testReceipt1.AH_OSExTaxAmount = 150M;
				Assert("Changed Foreign Amount - refreshing should be called", AmountOnChildChangedFired);
				AmountOnChildChangedFired = ZBool.False;
				testReceipt1.AH_LocalExTaxAmount = 33M;
				Assert("Changed Local Amount - refreshing should be called", AmountOnChildChangedFired);
				AmountOnChildChangedFired = ZBool.False;
				testReceipt1.AH_ExchangeRate = 33M;
				Assert("Changed Exchange Rate - refreshing should be called", AmountOnChildChangedFired);
			}
			finally
			{
				TestCollection.OnAmountOnChildChanged -= new ARReceiptCollection.OnAmountOnChildChangedHandler(TestCollection_OnAmountOnChildChanged);
			}
		}

		public void TestOnReceiptTypeOnChildChanged()
		{
			TestCollection = new ARReceiptCollection(Factory, Factory.NewWithValidTestData<ARReceipt>());
			try
			{
				ARReceipt testReceipt1 = TestCollection.AddNew();
				TestCollection.OnReceiptTypeOnChildChanged += new ARReceiptCollection.OnReceiptTypeOnChildChangedHandler(TestCollection_OnReceiptTypeOnChildChanged);
				Assert("Refreshing should not be called yet", !ReceiptOnChildChangedFired);
				testReceipt1.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
				Assert("Changed AH_ReceiptType - refreshing should be called", ReceiptOnChildChangedFired);
			}
			finally
			{
				TestCollection.OnReceiptTypeOnChildChanged -= new ARReceiptCollection.OnReceiptTypeOnChildChangedHandler(TestCollection_OnReceiptTypeOnChildChanged);
			}
		}

		public void TestOnAdded()
		{
			TestCollection = new ARReceiptCollection(Factory, Factory.NewWithValidTestData<ARReceipt>());
			try
			{
				TestCollection.OnReceiptTypeOnChildChanged += new ARReceiptCollection.OnReceiptTypeOnChildChangedHandler(TestCollection_OnReceiptTypeOnChildChanged);
				Assert("Refreshing should not be called yet", !ReceiptOnChildChangedFired);
				ARReceipt testReceipt1 = TestCollection.AddNew();
				Assert("Added New Element - refreshing should be called", ReceiptOnChildChangedFired);
			}
			finally
			{
				TestCollection.OnReceiptTypeOnChildChanged -= new ARReceiptCollection.OnReceiptTypeOnChildChangedHandler(TestCollection_OnReceiptTypeOnChildChanged);
			}
		}

		#region Implementation

		void TestCollection_OnAmountOnChildChanged()
		{
			AmountOnChildChangedFired = ZBool.True;
		}
		ZBool AmountOnChildChangedFired;

		void TestCollection_OnReceiptTypeOnChildChanged()
		{
			ReceiptOnChildChangedFired = ZBool.True;
		}
		ZBool ReceiptOnChildChangedFired;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(ARReceipt));
		}

		protected ARReceiptCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new ARReceiptCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ARReceiptCollection(Factory);
		}

		#endregion
	}
}
