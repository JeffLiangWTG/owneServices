using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(APPaymentBatchPosterCollection))]
	public class APPaymentBatchPosterCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			APPaymentBatchPoster batchPoster1 = (APPaymentBatchPoster)GetNewElementToAddToTheCollection();
			TestCollection.Add(batchPoster1);
			AssertEquals("Index 0", batchPoster1, TestCollection[0]);

			APPaymentBatchPoster batchPoster2 = (APPaymentBatchPoster)GetNewElementToAddToTheCollection();
			TestCollection.Add(batchPoster2);
			AssertEquals("Index 1", batchPoster2, TestCollection[1]);
		}

		public void TestReloadTransactionsInTheLocalFactory()
		{
			var testTransaction = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.USD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			Factory.Save();
			TransactionHeaders.Add(testTransaction);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestCollection = new APPaymentBatchPosterCollection(newFactory, TransactionPKs);
			AssertEquals("Reloaded Transactions factory should be NewFactory", TestCollection.ReloadedTransactionCollection_ForTestOnly.Factory, newFactory);
			Assert("ReloadedTransactions collection should contain the TestTransaction", TestCollection.ReloadedTransactionCollection_ForTestOnly.Contains(testTransaction));
		}

		public void TestExcludeFullyPaidTransactions()
		{
			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.USD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV002", TestObjectCreator.USD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV003", TestObjectCreator.USD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);

			PayInvoice((APInvoice)invoice2, -5M);
			PayInvoice((APInvoice)invoice3, -10M);
			Factory.Save();

			TransactionHeaders.Add(invoice1);
			TransactionHeaders.Add(invoice2);
			TransactionHeaders.Add(invoice3);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestCollection = new APPaymentBatchPosterCollection(newFactory, TransactionPKs);
			AssertEquals("Reloaded Transactions factory should be NewFactory", TestCollection.ReloadedTransactionCollection_ForTestOnly.Factory, newFactory);
			Assert("ReloadedTransactions collection should contain the unpaid transaction", TestCollection.ReloadedTransactionCollection_ForTestOnly.Contains(invoice1));
			Assert("ReloadedTransactions collection should contain the not fully paid transaction", TestCollection.ReloadedTransactionCollection_ForTestOnly.Contains(invoice2));
			Assert("ReloadedTransactions collection shouldn't contain the fully paid transaction", !TestCollection.ReloadedTransactionCollection_ForTestOnly.Contains(invoice3));
		}

		void PayInvoice(APInvoice invoice, decimal amount)
		{
			AccTransactionMatchLink linkForInvoice = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			linkForInvoice.AP_Amount = amount;
			linkForInvoice.AP_MatchDate = ZDateTime.Now;
			linkForInvoice.AP_AH = invoice.PK;
			invoice.AH_OutstandingAmount = invoice.AH_OutstandingAmount + invoice.AH_GSTAmount - amount;
			if (invoice.AH_OutstandingAmount == ZDecimal.Zero)
			{
				invoice.AH_FullyPaidDate = ZDateTime.Now;
			}

			AccTransactionHeader payment = Factory.NewWithValidTestData<AccTransactionHeader>();
			payment.AH_InvoiceAmount = -amount;

			AccTransactionMatchLink linkForPayment = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			linkForPayment.AP_AH = payment.PK;
			linkForPayment.AP_Amount = -amount;
			linkForPayment.AP_MatchDate = ZDateTime.Now;
		}

		public void TestSetDefaultsForNewChild()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(10);
			testOrg.CompanyData.OB_IsCreditor = true;
			testOrg.APSettlementGroupPK = testOrg.PK;

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg.PK;
			testAPInv.AH_LocalExTaxAmount = 94M;
			testAPInv.AH_OSExTaxAmount = 94M;
			TransactionHeaders.Add(testAPInv);

			TestCollection = new APPaymentBatchPosterCollection(Factory, TransactionPKs);
			APPaymentBatchPoster newPaymentBatchPoster = TestCollection.AddNew();
			AssertEquals("There should be 1 new PaymentApproval created in new APPaymentBatchPoster", 1, newPaymentBatchPoster.PaymentApprovalCollection.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APPaymentBatchPosterCollection(Factory, System.Array.Empty<ZGuid>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<APPaymentBatchPoster>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TransactionHeaders = new TransactionHeaderCollection(Factory);
			TestCollection = new APPaymentBatchPosterCollection(Factory, TransactionPKs);
			TestObjectCreator = new TestObjectCreator(Factory);
		}
		APPaymentBatchPosterCollection TestCollection;
		TransactionHeaderCollection TransactionHeaders;
		TestObjectCreator TestObjectCreator;

		IEnumerable<ZGuid> TransactionPKs
		{
			get
			{
				return TransactionHeaders.GetPKs();
			}
		}

		#endregion
	}
}
