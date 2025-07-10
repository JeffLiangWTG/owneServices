using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class ReceiptPaymentBaseMatchingTest : Base.Transaction.Testing.IMatchingTestCase
	{
		protected override Base.Transaction.IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
		{
			ReceiptPaymentBase bizObj = GetNewReceiptPayment();
			bizObj.AH_GB = branchPK;
			bizObj.AH_OH = organisationPK;
			bizObj.AH_RX_NKTransactionCurrency = currencyCode;
			return bizObj;
		}

		protected abstract ReceiptPaymentBase GetNewReceiptPayment();
	}
}
