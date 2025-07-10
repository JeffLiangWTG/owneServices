using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class AccrualsForAPReconciliation<T>
	{
		public ZGuid JobParentId { get; set; }
		public string JobNumber { get; set; }
		public string JobParentTableCode { get; set; }
		public AccrualSourceTypes AccrualType { get; set; }
		public IEnumerable<T> Accruals { get; set; }
		public bool IsRelatedJob { get; set; }
		public IEnumerable<T> RelatedJobs { get; set; }
	}

	public class AccrualsForAPReconciliationWithConsolInfo<T> : AccrualsForAPReconciliation<T>
	{
		public ZGuid? ConsolPk { get; set; }
	}
}
