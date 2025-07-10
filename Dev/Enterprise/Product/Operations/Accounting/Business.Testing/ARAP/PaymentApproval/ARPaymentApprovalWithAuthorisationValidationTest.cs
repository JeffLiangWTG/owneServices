using System;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public class ARPaymentApprovalWithAuthorisationValidationTest : PaymentApprovalValidationTest
	{
		protected override Type GetValidationBizoType()
		{
			return typeof(ARPaymentApprovalWithAuthorisation);
		}
	}
}
