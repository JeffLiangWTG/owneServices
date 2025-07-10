using CargoWise.EntityFramework;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingJournalTaxDetailForTest : AccountingJournalTaxDetail
	{
		public AccountingJournalTaxDetailForTest()
			: base(null, null)
		{
		}

		public AccountingJournalTaxDetailForTest(BusinessObjectFactory factory, IGLMovementDetails glMovementDetails)
			: base(factory, glMovementDetails)
		{
		}
	}
}
