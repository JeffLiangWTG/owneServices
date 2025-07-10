using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(APPaymentApprovalWithoutAuthorisation))]
	public class APPaymentApprovalWithoutAuthorisationMatchingTest : PaymentApprovalMatchingTest
	{
		protected override PaymentApprovalBase GetNewPaymentApproval()
		{
			return Factory.New<APPaymentApprovalWithoutAuthorisation>();
		}
	}
}
