using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	public abstract class JournalMatchingTest : Base.Transaction.Testing.IMatchingTestCase
	{
		protected override Base.Transaction.IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
		{
			Journal bizObj = GetNewJournal();
			bizObj.AH_GB = branchPK;
			bizObj.AH_OH = organisationPK;
			bizObj.AH_RX_NKTransactionCurrency = currencyCode;
			return bizObj;
		}

		protected abstract Journal GetNewJournal();
	}
}
