using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccPaymentBatchCollection))]
	public class AccPaymentBatchCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateRelationshipFilter()
		{
			var currency = TestObjectCreator.GetCurrency("USD");
			var bankAccount = TestObjectCreator.CreateBankAccount("Test", "Test", currency, null);

			var paymentBatch1 = GetNewElementToAddToTheCollection() as AccPaymentBatch;
			paymentBatch1.APB_AB = bankAccount.PK;

			var paymentBatch2 = GetNewElementToAddToTheCollection() as AccPaymentBatch;
			paymentBatch2.APB_AB = bankAccount.PK;
			paymentBatch2.APB_PaymentType = ChequeTransactionTypes.ChequeEntryTransaction;
			paymentBatch2.APB_RX_NKBatchCurrency = "USD";

			var paymentBatch3 = GetNewElementToAddToTheCollection() as AccPaymentBatch;
			paymentBatch3.APB_AB = bankAccount.PK;
			paymentBatch3.APB_GC = TestObjectCreator.NonCurrentCompany.PK;

			Factory.Save();

			TestCollection.Load();

			AssertEquals(1, TestCollection.Count);
			Assert(TestCollection.Contains(paymentBatch1));

			var batch = TestCollection[0];
			Assert("The record is not Cheque Transaction Header record", batch.APB_PaymentType != ChequeTransactionTypes.ChequeEntryTransaction);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccPaymentBatchCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var paymentBatch = Factory.NewWithValidTestData<AccPaymentBatch>();
			return paymentBatch;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = GetCollectionToTest() as AccPaymentBatchCollection;
			TestObjectCreator = new TestObjectCreator(Factory);
		}
		AccPaymentBatchCollection TestCollection;
		TestObjectCreator TestObjectCreator;

		#endregion
	}
}
