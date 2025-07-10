#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Module
{
	public partial class AccountingFilterStripCreator
	{
		public ZDBOnlyQuery GetBaseJobSubQuery_ForTestOnly()
		{
			return GetBaseJobSubQuery();
		}

		public ZQuery GetNotInvoicedSubQuery_ForTestOnly()
		{
			return GetNotInvoicedQuery(true);
		}
	}
}

#endif
