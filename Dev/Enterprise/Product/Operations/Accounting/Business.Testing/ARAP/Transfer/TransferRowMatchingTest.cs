using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	public abstract class TransferRowMatchingTest : Base.Transaction.Testing.IMatchingTestCase
	{
		protected override Base.Transaction.IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
		{
			TransferRow bizObj = GetNewTransferRow();
			bizObj.AH_GB = branchPK;
			bizObj.AH_OH = organisationPK;
			bizObj.AH_RX_NKTransactionCurrency = currencyCode;
			return bizObj;
		}

		protected abstract TransferRow GetNewTransferRow();
	}
}
