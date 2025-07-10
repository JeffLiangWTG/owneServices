using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	public abstract class ExchangeDifferenceMatchingTest : Base.Transaction.Testing.IMatchingTestCase
	{
		protected override Base.Transaction.IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
		{
			ExchangeDifference bizObj = GetNewExchangeDifference();
			bizObj.AH_GB = branchPK;
			bizObj.AH_OH = organisationPK;
			bizObj.AH_RX_NKTransactionCurrency = currencyCode;
			return bizObj;
		}

		protected abstract ExchangeDifference GetNewExchangeDifference();
	}
}
