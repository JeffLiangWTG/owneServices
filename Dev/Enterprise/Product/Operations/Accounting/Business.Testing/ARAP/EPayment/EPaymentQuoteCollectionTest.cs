using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.EPayment.Testing
{
	[TestedType(typeof(EPaymentQuoteCollection))]
	public class EPaymentQuoteCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			return new EPaymentQuoteCollection(paymentApproval);
		}

		public void TestEPaymentQuoteCollection_ForPaymentApproval()
		{
			var quote1 = Factory.NewWithValidTestData<EPaymentQuote>();
			var quote2 = Factory.NewWithValidTestData<EPaymentQuote>();
			Factory.NewWithValidTestData<EPaymentQuote>();

			var approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval.AV_OH = OrgHeader.DefaultOrg.PK;
			quote1.QU_AV = quote2.QU_AV = approval.PK;
			Factory.Save();

			var collection = new EPaymentQuoteCollection(approval);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { quote1, quote2 }, collection);
		}

		public void TestEPaymentQuoteCollection_ForPaymentBatchPoster()
		{
			var quote1 = Factory.NewWithValidTestData<EPaymentQuote>();
			var quote2 = Factory.NewWithValidTestData<EPaymentQuote>();
			var quote3 = Factory.NewWithValidTestData<EPaymentQuote>();
			var quote4 = Factory.NewWithValidTestData<EPaymentQuote>();
			Factory.NewWithValidTestData<EPaymentQuote>();

			var batchPoster = Factory.New<APPaymentBatchPoster>();
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			batchPoster.APB_AB = bankAccount.PK;

			var approval1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			batchPoster.PaymentApprovalCollection.Add(approval1);
			approval1.AV_OH = OrgHeader.DefaultOrg.PK;
			quote1.QU_AV = approval1.PK;
			quote2.QU_AV = approval1.PK;

			var approval2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval2.AV_AB = bankAccount.PK;
			batchPoster.PaymentApprovalCollection.Add(approval2);
			approval2.AV_OH = OrgHeader.DefaultOrg.PK;
			quote3.QU_AV = approval2.PK;
			quote4.QU_AV = approval2.PK;
			Factory.Save();

			var collection = new EPaymentQuoteCollection(batchPoster);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { quote1, quote2, quote3, quote4 }, collection);
		}
	}
}
