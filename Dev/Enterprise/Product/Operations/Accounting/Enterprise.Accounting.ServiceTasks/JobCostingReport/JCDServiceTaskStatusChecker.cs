using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;

namespace Enterprise.Accounting.ServiceTasks
{
	public class JCDServiceTaskStatusChecker : IJCDServiceTaskStatusChecker
	{
		public bool IsJCDServiceTaskComplete => AccountingConfigurationRegistry.Instance.JCDServiceTaskController.Value == JCDActionList.Codes.CompletedOldTransactionLinesHaveBeenProcessed
												&& JCDDependentObjectList.GetVersionManager().IsUpToDate;
	}
}
