using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ARAP.ReceiptPayment
{
	public class APPaymentBatchPosterFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchHintLoadPaymentApprovalCollection()
		{
			var chequeBook = TestObjectCreator.GetAutoPrintChequeBook(Factory, 1, 200, 1);
			var paymentApproval1 = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook.BankAccount, chequeBook);
			var paymentApproval2 = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook.BankAccount, chequeBook);

			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			paymentApproval1.AV_APB_PaymentBatch = paymentBatch.PK;
			paymentApproval2.AV_APB_PaymentBatch = paymentBatch.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var paymentBatchReload = newFactory.Load<APPaymentBatchPoster>(paymentBatch.PK);

			AssertNotNull(paymentBatchReload);
			AssertMaxDbHits("AccPaymentBatch:1", 1, newFactory);

			var paymentApproval1Reload = newFactory.Load<APPaymentApprovalWithoutAuthorisation>(paymentApproval1.PK);

			AssertMaxDbHits("AccPaymentBatch:1 / AccPaymentApproval:1. Loading a relevant payment approval should trigger the fetch hint", 2, newFactory);

			var list = paymentBatchReload.PaymentApprovalCollection;

			AssertMaxDbHits("No additional DB hit when load payment approvals", 2, newFactory);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
