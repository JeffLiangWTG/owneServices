using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.PaymentsRejectionReason;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(PaymentRejectionReasonHolder))]
	internal class PaymentRejectionReasonHolderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDescription()
		{
			var holder = new PaymentRejectionReasonHolder();
			holder.Code = Codes.InsufficientFunds;
			AssertEquals(Descriptions.InsufficientFunds, holder.Description);
			AssertEquals(Descriptions.InsufficientFunds, holder.Reason);

			holder.Reason = "Not enough liquidity in the system";
			AssertEquals("Not enough liquidity in the system", holder.Reason);

			holder.Code = Codes.InvoiceChargesDisputed;
			AssertEquals(Descriptions.InvoiceChargesDisputed, holder.Description);
			AssertEquals(Descriptions.InvoiceChargesDisputed, holder.Reason);
		}
	}
}
