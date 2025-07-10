using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	public abstract class DiscountMatchingTest : Base.Transaction.Testing.IMatchingTestCase
	{
		protected override Base.Transaction.IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
		{
			Discount bizObj = GetNewDiscount();
			bizObj.AH_GB = branchPK;
			bizObj.AH_OH = organisationPK;
			bizObj.AH_RX_NKTransactionCurrency = currencyCode;
			return bizObj;
		}

		protected abstract Discount GetNewDiscount();
	}
}
