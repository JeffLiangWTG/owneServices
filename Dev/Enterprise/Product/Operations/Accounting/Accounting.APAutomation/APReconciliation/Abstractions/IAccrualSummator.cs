using System;
using System.Collections.Generic;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IAccrualSummator
	{
		IEnumerable<(string CombinationKey, IEnumerable<APReconciliationLine> Accruals)> Sumup(IEnumerable<APReconciliationLine> reconciliationLines, decimal targetAmount, Func<APReconciliationLine, decimal> amountFieldSelector);
	}
}
