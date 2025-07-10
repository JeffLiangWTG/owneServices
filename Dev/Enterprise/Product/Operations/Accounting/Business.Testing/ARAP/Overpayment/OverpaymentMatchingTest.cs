using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	public abstract class OverpaymentMatchingTest : Base.Transaction.Testing.IMatchingTestCase
	{
		protected override Base.Transaction.IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
		{
			Overpayment bizObj = GetNewOverpayment();
			bizObj.AH_GB = branchPK;
			bizObj.AH_OH = organisationPK;
			bizObj.AH_RX_NKTransactionCurrency = currencyCode;
			return bizObj;
		}

		protected abstract Overpayment GetNewOverpayment();
	}
}
