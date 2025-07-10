using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(APPaymentApprovalWithAuthorisation))]
	public class APPaymentApprovalWithAuthorisationMatchingTest : PaymentApprovalMatchingTest
	{
		protected override PaymentApprovalBase GetNewPaymentApproval()
		{
			return Factory.New<APPaymentApprovalWithAuthorisation>();
		}
	}
}
