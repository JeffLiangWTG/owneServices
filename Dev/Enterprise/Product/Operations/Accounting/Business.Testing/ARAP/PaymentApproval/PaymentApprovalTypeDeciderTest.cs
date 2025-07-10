using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public class PaymentApprovalTypeDeciderTest : TestCaseWithFactory
	{
		public void TestLoadingPaymentApprovals()
		{
			PaymentApprovalCollection approvalCollection = new PaymentApprovalCollection(Factory);
			approvalCollection.Load();

			int countBefore = approvalCollection.Count;

			APPaymentApprovalWithoutAuthorisation approval1 = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			ARPaymentApprovalWithoutAuthorisation approval2 = Factory.New<ARPaymentApprovalWithoutAuthorisation>();
			APPaymentApprovalWithAuthorisation approval3 = Factory.New<APPaymentApprovalWithAuthorisation>();
			ARPaymentApprovalWithAuthorisation approval4 = Factory.New<ARPaymentApprovalWithAuthorisation>();

			approvalCollection.Load();
			AssertEquals(4, approvalCollection.Count - countBefore);
		}
	}
}