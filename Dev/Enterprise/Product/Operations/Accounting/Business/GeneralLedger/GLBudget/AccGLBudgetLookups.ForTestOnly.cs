#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public partial class AccGLBudgetLookups
	{
		public ZQuery BalanceSheetAndPLFilter_ForTestOnly => BalanceSheetAndPLFilter;
	}
}

#endif
