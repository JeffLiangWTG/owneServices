using System;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public class ARPaymentApprovalWithoutAuthorisationValidationTest : PaymentApprovalValidationTest
	{
		protected override Type GetValidationBizoType()
		{
			return typeof(ARPaymentApprovalWithoutAuthorisation);
		}
	}
}
