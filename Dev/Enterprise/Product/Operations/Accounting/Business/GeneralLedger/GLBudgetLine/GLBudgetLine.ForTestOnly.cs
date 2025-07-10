#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget
{
	public partial class GLBudgetLine
	{
		public ZInt GetLastYearPeriod_ForTestOnly(ZInt period)
		{
			return GetLastYearPeriod(period);
		}

		public ZDecimal LastYearActualWithoutDebitCredit_ForTestOnly => LastYearActualWithoutDebitCredit;

		public GLBudgetLine LastYearBudgetLine_ForTestOnly => LastYearBudgetLine;
	}
}

#endif
