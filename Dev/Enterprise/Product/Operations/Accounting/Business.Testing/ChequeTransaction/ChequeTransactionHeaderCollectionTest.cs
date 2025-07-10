using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ChequeTransaction.Testing
{
	[TestedType(typeof(ChequeTransactionHeaderCollection))]
	sealed class ChequeTransactionHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateRelationshipFilter()
		{
			var currency = TestObjectCreator.GetCurrency("USD");
			var bankAccount = TestObjectCreator.CreateBankAccount("Test", "Test", currency, null);

			var paymentBatch1 = GetNewElementToAddToTheCollection() as AccPaymentBatch;
			paymentBatch1.APB_AB = bankAccount.PK;

			var paymentBatch2 = GetNewElementToAddToTheCollection() as AccPaymentBatch;
			paymentBatch2.APB_AB = bankAccount.PK;
			paymentBatch2.APB_PaymentType = ReceiptTypes.DirectDebit;
			paymentBatch2.APB_RX_NKBatchCurrency = "";

			var paymentBatch3 = GetNewElementToAddToTheCollection() as AccPaymentBatch;
			paymentBatch3.APB_AB = bankAccount.PK;
			paymentBatch3.APB_GC = TestObjectCreator.NonCurrentCompany.PK;

			Factory.Save();

			TestCollection.Load();

			AssertEquals(1, TestCollection.Count);
			Assert(TestCollection.Contains(paymentBatch1));

			var batch = TestCollection[0] as AccPaymentBatch;
			Assert("The record is Cheque Transaction Header record", batch.APB_PaymentType == ChequeTransactionTypes.ChequeEntryTransaction);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ChequeTransactionHeaderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<ChequeTransactionHeader>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new ChequeTransactionHeaderCollection(Factory);
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		ChequeTransactionHeaderCollection TestCollection;
		TestObjectCreator TestObjectCreator;

		#endregion
	}
}
