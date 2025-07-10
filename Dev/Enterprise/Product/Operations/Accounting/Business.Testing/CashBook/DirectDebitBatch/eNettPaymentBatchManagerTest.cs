using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class eNettPaymentBatchManagerTest : TestCaseWithFactory
	{
		public void TestComPayBatchIsExcludedFromDirectDebitBatchCriticalValidation()
		{
			var batchDate = ZDateTime.Today;

			var payment1 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			var payment2 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 2", ReceiptTypes.eNettDirectDebit, 200m);
			AssertNoExceptionThrown("This step creates new DDR batch.", () => new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment1, batchDate));
			AssertNoExceptionThrown("This step adds payment2 to DDR batch created in previous step.", () => new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment2, batchDate));

			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 300m, payment1, payment2);
		}

		public void TestCreateNewBatchWhenNoBatchDateIsSupplied()
		{
			Payment payment1 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			Payment payment2 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 2", ReceiptTypes.eNettDirectDebit, 200m);
			Payment payment3 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 3", ReceiptTypes.eNettDirectDebit, 300m);

			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment1, ZDateTime.Empty);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment2, ZDateTime.Empty);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment3, ZDateTime.Empty);

			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 100m, payment1);
			CheckDDRBatch(payment2.AH_ReceiptBatchNo, 200m, payment2);
			CheckDDRBatch(payment3.AH_ReceiptBatchNo, 300m, payment3);

			AssertNotEquals("Payment 1 & 2 should be in difference batches", payment1.AH_ReceiptBatchNo, payment2.AH_ReceiptBatchNo);
			AssertNotEquals("Payment 1 & 3 should be in difference batches", payment1.AH_ReceiptBatchNo, payment3.AH_ReceiptBatchNo);
			AssertNotEquals("Payment 2 & 3 should be in difference batches", payment2.AH_ReceiptBatchNo, payment3.AH_ReceiptBatchNo);
		}

		public void TestCreateNewBatchWhenBatchDateIsDifferent()
		{
			Payment payment1 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			Payment payment2 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 2", ReceiptTypes.eNettDirectDebit, 200m);
			Payment payment3 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 3", ReceiptTypes.eNettDirectDebit, 300m);
			Payment payment4 = CreatePayment(TestObjectCreator.AUDBankAccount2, "PAYMENT 4", ReceiptTypes.eNettDirectDebit, 400m);
			Payment payment5 = CreatePayment(TestObjectCreator.AUDBankAccount2, "PAYMENT 5", ReceiptTypes.eNettDirectDebit, 500m);
			Payment payment6 = CreatePayment(TestObjectCreator.AUDBankAccount2, "PAYMENT 6", ReceiptTypes.eNettDirectDebit, 600m);

			ZDateTime dayOne = ZDateTime.Today.AddDays(1);
			ZDateTime dayTwo = dayOne.AddDays(1);

			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment1, dayOne);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment2, dayOne);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment4, dayTwo);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment5, dayTwo);

			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 300m, payment1, payment2);
			CheckDDRBatch(payment4.AH_ReceiptBatchNo, 900m, payment4, payment5);

			DirectDebitBatchHeader dDRHeaderOne = LoadDDRBatch(payment1.AH_ReceiptBatchNo);
			dDRHeaderOne.AH_DateClearedInCashbook = dayOne.Date;
			dDRHeaderOne.Factory.Save();

			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment3, dayOne);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment6, dayTwo);

			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 300m, payment1, payment2);
			CheckDDRBatch(payment3.AH_ReceiptBatchNo, 300m, payment3);
			CheckDDRBatch(payment4.AH_ReceiptBatchNo, 1500m, payment4, payment5, payment6);
		}

		public void TestCreateNewBatchWhenExistingBatchIsClearedInTheCashBook()
		{
			Payment payment1 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			Payment payment2 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 2", ReceiptTypes.eNettDirectDebit, 200m);
			Payment payment3 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 3", ReceiptTypes.eNettDirectDebit, 300m);
			Payment payment4 = CreatePayment(TestObjectCreator.AUDBankAccount2, "PAYMENT 4", ReceiptTypes.eNettDirectDebit, 400m);
			Payment payment5 = CreatePayment(TestObjectCreator.AUDBankAccount2, "PAYMENT 5", ReceiptTypes.eNettDirectDebit, 500m);
			Payment payment6 = CreatePayment(TestObjectCreator.AUDBankAccount2, "PAYMENT 6", ReceiptTypes.eNettDirectDebit, 600m);

			ZDateTime dayOne = ZDateTime.Today.AddDays(1);
			ZDateTime dayTwo = dayOne.AddDays(10);

			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment1, dayOne);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment2, dayOne);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment4, dayTwo);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment5, dayTwo);

			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 300m, payment1, payment2);
			CheckDDRBatch(payment4.AH_ReceiptBatchNo, 900m, payment4, payment5);

			DirectDebitBatchHeader dDRHeaderOne = LoadDDRBatch(payment1.AH_ReceiptBatchNo);
			dDRHeaderOne.AH_DateClearedInCashbook = dayOne.Date;
			Factory.Save();

			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment3, dayOne);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment6, dayTwo);

			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 300m, payment1, payment2);
			CheckDDRBatch(payment3.AH_ReceiptBatchNo, 300m, payment3);
			CheckDDRBatch(payment4.AH_ReceiptBatchNo, 1500m, payment4, payment5, payment6);
		}

		public void TestCreateNewBatchWhenBatchDateIsInThePast()
		{
			Payment payment1 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			Payment payment2 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 2", ReceiptTypes.eNettDirectDebit, 200m);
			Payment payment3 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 3", ReceiptTypes.eNettDirectDebit, 300m);
			Payment payment4 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 4", ReceiptTypes.eNettDirectDebit, 400m);
			Payment payment5 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 5", ReceiptTypes.eNettDirectDebit, 500m);
			Payment payment6 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 6", ReceiptTypes.eNettDirectDebit, 600m);

			ZDateTime today = ZDateTime.Today;
			ZDateTime dateInThePast = today.AddDays(-10);
			ZDateTime dateInTheFuture = today.AddDays(10);

			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment1, dateInThePast);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment2, dateInThePast);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment3, today);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment4, today);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment5, dateInTheFuture);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment6, dateInTheFuture);

			CheckDDRBatch("00001000", 100m, payment1);
			CheckDDRBatch("00001001", 200m, payment2);
			CheckDDRBatch("00001002", 700m, payment3, payment4);
			CheckDDRBatch("00001003", 1100m, payment5, payment6);

			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 100m, payment1);
			CheckDDRBatch(payment2.AH_ReceiptBatchNo, 200m, payment2);
			CheckDDRBatch(payment3.AH_ReceiptBatchNo, 700m, payment3, payment4);
			CheckDDRBatch(payment5.AH_ReceiptBatchNo, 1100m, payment5, payment6);
		}

		public void TestSeparateBatchForLocalAndForeignCurrencyPayments()
		{
			Payment payment1 = CreatePayment(TestObjectCreator.AUDBankAccount, Constants.CurrencyCodes.Australia, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			Payment payment2 = CreatePayment(TestObjectCreator.AUDBankAccount, Constants.CurrencyCodes.UnitedStates, "PAYMENT 2", ReceiptTypes.eNettDirectDebit, 200m);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment1, ZDateTime.Empty);
			new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment2, ZDateTime.Empty);

			TransactionHeader[] batchHeaders = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Two payment batches should be created", 2, batchHeaders.Length);
			Assert("One batch should have local currency receipt type", batchHeaders[0].AH_ReceiptType == ReceiptTypes.eNettDirectDebit || batchHeaders[1].AH_ReceiptType == ReceiptTypes.eNettDirectDebit);
			Assert("Other batch should have foreign currency receipt type", batchHeaders[0].AH_ReceiptType == ReceiptTypes.eNettDirectDebitForeignCurrency || batchHeaders[1].AH_ReceiptType == ReceiptTypes.eNettDirectDebitForeignCurrency);
		}

		/// <summary>
		/// This is to test the scenario of running the batch creation logic with different company & its country local currency context
		/// We are going to make sure the batch creation is solely based on the payment currency, not the company context
		/// </summary>
		public void TestPaymentsWithSameCurrencyUnderDifferentCountryCompnanyContext()
		{
			Payment payment1 = CreatePayment(TestObjectCreator.AUDBankAccount, Constants.CurrencyCodes.Australia, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			Payment payment2 = CreatePayment(TestObjectCreator.AUDBankAccount, Constants.CurrencyCodes.Australia, "PAYMENT 2", ReceiptTypes.eNettDirectDebit, 200m);

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RX_NKLocalCurrency = "USD";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RX_NKLocalCurrency = "GBP";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			var batchDate = ZDateTime.Today;

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.PK, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment1, batchDate);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.PK, branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				new eNettPaymentBatchManager(Factory).AddToExistingBatchOrCreateNewBatch(payment2, batchDate);
			}

			TransactionHeader[] batchHeaders = Factory.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("One payment batch should be created", 1, batchHeaders.Length);
			AssertEquals("Payment batch should have local currency receipt type", ReceiptTypes.eNettDirectDebit, batchHeaders[0].AH_ReceiptType);
			AssertEquals("Batch should have local currency", Constants.CurrencyCodes.Australia, batchHeaders[0].AH_RX_NKTransactionCurrency);
		}

		public void TestComPayBatchWithoutSaveFactory()
		{
			Payment payment1 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			Payment payment2 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 2", ReceiptTypes.eNettDirectDebit, 200m);
			Payment payment3 = CreatePayment(TestObjectCreator.AUDBankAccount, "PAYMENT 3", ReceiptTypes.eNettDirectDebit, 300m);

			var batchDate = ZDateTime.Today;
			var batchManager = new eNettPaymentBatchManager(Factory);

			batchManager.AddToExistingBatchOrCreateNewBatch(payment1, batchDate, false);
			batchManager.AddToExistingBatchOrCreateNewBatch(payment2, batchDate, false);
			Factory.Save();

			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 300m, payment1, payment2);

			batchManager.AddToExistingBatchOrCreateNewBatch(payment3, batchDate);
			CheckDDRBatch(payment1.AH_ReceiptBatchNo, 600m, payment3);
		}

		/// <summary>
		/// This is to test the scenario where the Batch Record is created under a different Compnany+Branch+Department user context
		/// We need to make sure the Batch Record and the payment record are still sharing the same Company+Branch+Department details.
		/// plus the AH_ReceiptType is EDF as the payment currency is foreign currency
		/// </summary>
		public void TestComPayBatchWithDifferentCompanyContextMatchingForeignCurrency()
		{
			var payment = AddPaymentIntoBatchUnderNonCurrentCompany(Constants.CurrencyCodes.UnitedKingdom, Constants.CurrencyCodes.UnitedKingdom);
			var dDRHeader = LoadDDRBatch(payment.AH_ReceiptBatchNo);

			AssertEquals("Payment AH_GC (" + payment.AH_GC + ")", payment.AH_GC, dDRHeader.AH_GC);
			AssertEquals("Payment AH_GB (" + payment.AH_GB + ")", payment.AH_GB, dDRHeader.AH_GB);
			AssertEquals("Payment AH_GE (" + payment.AH_GE + ")", payment.AH_GE, dDRHeader.AH_GE);
			AssertEquals("Payment receipt type", ReceiptTypes.eNettDirectDebitForeignCurrency, dDRHeader.AH_ReceiptType);
			AssertEquals("Payment currency is local as the foreign currency is not properly supported", Constants.CurrencyCodes.Australia, dDRHeader.AH_RX_NKTransactionCurrency);
		}

		/// <summary>
		/// This is to test the scenario where the Batch Record is created under a different Compnany+Branch+Department user context
		/// We need to make sure the Batch Record and the payment record are still sharing the same Company+Branch+Department details.
		/// plus the AH_ReceiptType is EDF as the payment currency is foreign currency
		/// </summary>
		public void TestComPayBatchWithDifferentCompanyContextForeignPaymentCurrency()
		{
			var payment = AddPaymentIntoBatchUnderNonCurrentCompany(Constants.CurrencyCodes.Australia, Constants.CurrencyCodes.UnitedKingdom);
			var dDRHeader = LoadDDRBatch(payment.AH_ReceiptBatchNo);

			AssertEquals("Payment AH_GC (" + payment.AH_GC + ")", payment.AH_GC, dDRHeader.AH_GC);
			AssertEquals("Payment AH_GB (" + payment.AH_GB + ")", payment.AH_GB, dDRHeader.AH_GB);
			AssertEquals("Payment AH_GE (" + payment.AH_GE + ")", payment.AH_GE, dDRHeader.AH_GE);
			AssertEquals("Payment receipt type", ReceiptTypes.eNettDirectDebitForeignCurrency, dDRHeader.AH_ReceiptType);
			AssertEquals("Payment currency is local as the foreign currency is not properly supported", Constants.CurrencyCodes.Australia, dDRHeader.AH_RX_NKTransactionCurrency);
		}

		/// <summary>
		/// This is to test the scenario where the Batch Record is created under a different Compnany+Branch+Department user context
		/// different from the context where the payment record was created. Both companies are shareing the same local currency.
		/// We need to make sure the Batch Record and the payment record are still sharing the same Company+Branch+Department details.
		/// plus the AH_ReceiptType is END as the payment currency is local
		/// </summary>
		public void TestComPayBatchWithDifferentCompanyContextMatchingLocalCurrency()
		{
			var payment = AddPaymentIntoBatchUnderNonCurrentCompany(Constants.CurrencyCodes.Australia, Constants.CurrencyCodes.Australia);
			var dDRHeader = LoadDDRBatch(payment.AH_ReceiptBatchNo);

			AssertEquals("Payment AH_GC (" + payment.AH_GC + ")", payment.AH_GC, dDRHeader.AH_GC);
			AssertEquals("Payment AH_GB (" + payment.AH_GB + ")", payment.AH_GB, dDRHeader.AH_GB);
			AssertEquals("Payment AH_GE (" + payment.AH_GE + ")", payment.AH_GE, dDRHeader.AH_GE);
			AssertEquals("Payment receipt type", ReceiptTypes.eNettDirectDebit, dDRHeader.AH_ReceiptType);
			AssertEquals("Payment currency", Constants.CurrencyCodes.Australia, dDRHeader.AH_RX_NKTransactionCurrency);
		}

		/// <summary>
		/// This is to test the scenario where the Batch Record is created under a different Compnany+Branch+Department user context
		/// different from the context where the payment record was created. These two companies are using different local currencies.
		/// We need to make sure the Batch Record and the payment record are still sharing the same Company+Branch+Department details,
		/// plus the AH_ReceiptType is END as the payment currency is local
		/// </summary>
		public void TestComPayBatchWithDifferentCompanyContextLocalPaymentCurrency()
		{
			var payment = AddPaymentIntoBatchUnderNonCurrentCompany(Constants.CurrencyCodes.UnitedKingdom, Constants.CurrencyCodes.Australia);

			var dDRHeader = LoadDDRBatch(payment.AH_ReceiptBatchNo);
			AssertEquals("Payment AH_GC (" + payment.AH_GC + ")", payment.AH_GC, dDRHeader.AH_GC);
			AssertEquals("Payment AH_GB (" + payment.AH_GB + ")", payment.AH_GB, dDRHeader.AH_GB);
			AssertEquals("Payment AH_GE (" + payment.AH_GE + ")", payment.AH_GE, dDRHeader.AH_GE);
			AssertEquals("Payment receipt type", ReceiptTypes.eNettDirectDebit, dDRHeader.AH_ReceiptType);
			AssertEquals("Payment currency", Constants.CurrencyCodes.Australia, dDRHeader.AH_RX_NKTransactionCurrency);
		}

		Payment AddPaymentIntoBatchUnderNonCurrentCompany(string companyLocalCurrency, string paymentCurrency)
		{
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			nonCurrentCompany.GC_RX_NKLocalCurrency = companyLocalCurrency;
			var nonCurrentBranch = Factory.NewWithValidTestData<GlbBranch>();
			nonCurrentBranch.GB_GC = nonCurrentCompany.PK;
			var nonCurrentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			Payment payment = CreatePayment(TestObjectCreator.AUDBankAccount, paymentCurrency, "PAYMENT 1", ReceiptTypes.eNettDirectDebit, 100m);
			AssertNotEquals(nonCurrentCompany.PK, payment.AH_GC);

			var batchDate = ZDateTime.Today;

			using (var context = EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.PK, nonCurrentBranch.PK.ToGuid(), nonCurrentDepartment.PK.ToGuid()))
			{
				var batchManager = new eNettPaymentBatchManager(Factory);
				batchManager.AddToExistingBatchOrCreateNewBatch(payment, batchDate, false);
				Factory.Save();
			}
			return payment;
		}

		void CheckDDRBatch(ZString batchNumber, ZDecimal expectedTotal, params Payment[] payments)
		{
			AssertNotEquals("Batch Number", ZString.Empty, batchNumber);
			DirectDebitBatchHeader dDRHeader = LoadDDRBatch(batchNumber);
			AssertNotNull("DDR Batch Header" + batchNumber, dDRHeader);
			AssertEquals(String.Format("DDR Batch Header {0} Amount", batchNumber), expectedTotal, dDRHeader.AH_OSTotalAmount);

			foreach (Payment payment in payments)
			{
				AssertEquals("Payment Batch Number (" + payment.AH_Desc + ")", batchNumber, payment.AH_ReceiptBatchNo);
				AssertEquals("Payment AH_GC (" + payment.AH_GC + ")", payment.AH_GC, dDRHeader.AH_GC);
				AssertEquals("Payment AH_GB (" + payment.AH_GB + ")", payment.AH_GB, dDRHeader.AH_GB);
				AssertEquals("Payment AH_GE (" + payment.AH_GE + ")", payment.AH_GE, dDRHeader.AH_GE);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);

			CreatePayment(TestObjectCreator.AUDBankAccount, "DDR PAYMENT", ReceiptTypes.DirectDebit, 100m);
		}

		APPayment CreatePayment(AccBankAccount bankAccount, ZString description, ZString paymentType, ZDecimal paymentAmount)
		{
			return CreatePayment(bankAccount, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, description, paymentType, paymentAmount);
		}

		APPayment CreatePayment(AccBankAccount bankAccount, ZString currencyCode, ZString description, ZString paymentType, ZDecimal paymentAmount)
		{
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = paymentType;
			payment.AH_Desc = description;
			payment.AH_AB = bankAccount.PK;
			payment.AH_RX_NKTransactionCurrency = currencyCode;
			payment.AH_ExchangeRate = 1m;
			payment.AH_OSExTaxAmount = paymentAmount;
			Factory.Save();

			AssertEquals("Precondition Payment DDR Batch", ZString.Empty, payment.AH_ReceiptBatchNo);
			return payment;
		}

		DirectDebitBatchHeader LoadDDRBatch(ZString batchNumber)
		{
			ZQuery findDDRBatchQuery = new ZQuery();
			findDDRBatchQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch);
			findDDRBatchQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, batchNumber);
			findDDRBatchQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			return Factory.LoadTop1<DirectDebitBatchHeader>(findDDRBatchQuery);
		}

		TestObjectCreator TestObjectCreator;
	}
}
