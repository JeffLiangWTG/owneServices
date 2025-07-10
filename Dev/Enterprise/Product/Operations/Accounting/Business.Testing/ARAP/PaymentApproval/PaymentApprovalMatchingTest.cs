using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public abstract class PaymentApprovalMatchingTest : Base.Transaction.Testing.IMatchingTestCase
	{
		protected override Base.Transaction.IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
		{
			PaymentApprovalBase bizObj = GetNewPaymentApproval();
			bizObj.AV_GB = branchPK;
			bizObj.AV_OH = organisationPK;
			bizObj.AV_RX_NKPaymentCurrency = currencyCode;
			return bizObj;
		}

		protected abstract PaymentApprovalBase GetNewPaymentApproval();
	}
}
