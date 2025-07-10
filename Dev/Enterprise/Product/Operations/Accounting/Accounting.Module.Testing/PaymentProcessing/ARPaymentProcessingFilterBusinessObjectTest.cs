using System;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARPaymentProcessingFilterBusinessObject))]
	public class ARPaymentProcessingFilterBusinessObjectTest : PaymentProcessingFilterBusinessObjectTest
	{
		public void TestLedgerCodeProperty()
		{
			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsReceivable, GetLedgerCode());
		}

		protected override Type GetPaymentApprovalType() => typeof(ARPaymentApprovalWithAuthorisation);

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ARPaymentProcessingFilterBusinessObject();
		}
	}
}
