using System;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public class PaymentApprovalAuthorizationNotificationEmailTest : AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get { return typeof(PaymentApprovalAuthorizationNotificationEmail); }
		}
	}
}
