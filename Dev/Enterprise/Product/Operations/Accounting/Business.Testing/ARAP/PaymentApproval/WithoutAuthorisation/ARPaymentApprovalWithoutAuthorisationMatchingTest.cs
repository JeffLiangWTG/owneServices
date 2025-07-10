using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(ARPaymentApprovalWithoutAuthorisation))]
	public class ARPaymentApprovalWithoutAuthorisationMatchingTest : PaymentApprovalMatchingTest
	{
		protected override PaymentApprovalBase GetNewPaymentApproval()
		{
			return Factory.New<ARPaymentApprovalWithoutAuthorisation>();
		}
	}
}
