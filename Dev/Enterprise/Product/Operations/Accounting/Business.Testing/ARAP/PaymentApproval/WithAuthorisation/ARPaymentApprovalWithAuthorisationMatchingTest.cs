using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(ARPaymentApprovalWithAuthorisation))]
	public class ARPaymentApprovalWithAuthorisationMatchingTest : PaymentApprovalMatchingTest
	{
		protected override PaymentApprovalBase GetNewPaymentApproval()
		{
			return Factory.New<ARPaymentApprovalWithAuthorisation>();
		}
	}
}
