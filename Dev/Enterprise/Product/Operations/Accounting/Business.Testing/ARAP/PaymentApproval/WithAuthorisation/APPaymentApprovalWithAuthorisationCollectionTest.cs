using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(APPaymentApprovalWithAuthorisationCollection))]
	public class APPaymentApprovalWithAuthorisationCollectionTest : PaymentApprovalBaseCollectionTest
	{
		public void TestRegisterAndUnRegisterEditableMatchingObject_PaymentIsInDatabase()
		{
			var batch = Factory.NewWithValidTestData<APPaymentBatchPoster>();

			var payment1 = Factory.NewWithValidTestData(ExpectedElementType) as PaymentApprovalBase;
			payment1.InitializeForPaymentBatch(() => false);
			payment1.AV_APB_PaymentBatch = batch.PK;

			var payment2 = Factory.NewWithValidTestData(ExpectedElementType) as PaymentApprovalBase;
			payment2.InitializeForPaymentBatch(() => false);
			payment2.AV_APB_PaymentBatch = batch.PK;

			IBusiness batchBizO = batch;

			AssertEquals("Percondition", 0, batchBizO.Children.Length);

			Factory.Save();

			AssertEquals("Percondition", 1, batchBizO.Children.Length);

			var stmLogs = batchBizO.Children[0];
			AssertEquals("Percondition", "StmALogDependentCollection", stmLogs.GetType().Name);
			AssertEquals(true, batch.IsInDatabase);
			AssertEquals(true, payment1.IsInDatabase);
			AssertEquals(true, payment2.IsInDatabase);

			batch.ClearPaymentApprovalCollection_ForTestOnly();
			batch.LoadPayments();

			AssertEquals(4, batchBizO.Children.Length);
			AssertContainsExactElementsInAnyOrder(new IBusiness[] { stmLogs, batch.PaymentApprovalCollection, payment1.MatchingBaseObject, payment2.MatchingBaseObject }, batchBizO.Children);

			IBusiness approvalCollection = batch.PaymentApprovalCollection;

			AssertEquals("Percondition", 2, approvalCollection.Count);
			AssertEquals(2, approvalCollection.Children.Length);
			AssertContainsExactElementsInAnyOrder(new IBusiness[] { payment1, payment2 }, approvalCollection.Children);

			var paymentMatchingBaseObject1 = batch.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Single(x => x.PK == payment1.PK).MatchingBaseObject;
			var paymentMatchingBaseObject2 = batch.PaymentApprovalCollection.OfType<PaymentApprovalBase>().Single(x => x.PK == payment2.PK).MatchingBaseObject;

			AssertCollectionContains("MatchingBaseObject already contain children: payment1", payment1, (paymentMatchingBaseObject1 as IBusiness).Children[0]);
			AssertCollectionContains("MatchingBaseObject already contain children: payment2", payment2, (paymentMatchingBaseObject2 as IBusiness).Children[0]);
			AssertEquals("MatchingBaseObject still contain children payment1", payment1, (paymentMatchingBaseObject1 as IBusiness).Children[0][0]);

			batch.PaymentApprovalCollection.Remove(payment1);

			AssertEquals(3, batchBizO.Children.Length);
			AssertContainsExactElementsInAnyOrder(new IBusiness[] { stmLogs, batch.PaymentApprovalCollection, payment2.MatchingBaseObject }, batchBizO.Children);
			AssertCollectionNotContains(payment1.MatchingBaseObject, batchBizO.Children);

			AssertEquals(1, approvalCollection.Count);
			AssertEquals(1, approvalCollection.Children.Length);
			AssertCollectionNotContains(payment1, approvalCollection.Children);
			AssertCollectionContains(payment2, approvalCollection.Children);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APPaymentApprovalWithAuthorisationCollection(Factory);
		}

		#endregion
	}
}
