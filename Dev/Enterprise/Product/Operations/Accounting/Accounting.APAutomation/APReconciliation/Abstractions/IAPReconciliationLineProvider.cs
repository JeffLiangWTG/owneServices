using System.Collections.Generic;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IAPReconciliationLineProvider
	{
		IEnumerable<APReconciliationLine> GetLines(APReconciliationAccrualFilterTypes accrualFilterType);
	}
}
