using System.Collections.Generic;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class APReconciliationProcessingResult
	{
		public APReconciliationResultTypes Result { get; set; }
		public string FailureReasonCode { get; set; } = string.Empty;
		public string FailureReason { get; set; } = string.Empty;
		public ILogableError FailureReasonAsLogableError { get; set; }
		public IEnumerable<APReconciliationLine> ReconciliableAccruals { get; set; }
	}
}
