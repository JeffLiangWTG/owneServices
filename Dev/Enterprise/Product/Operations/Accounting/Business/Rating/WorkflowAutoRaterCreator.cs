using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class WorkflowAutoRaterCreator : IWorkflowAutoRaterCreator
	{
		public IProcessor CreateWorkflowAutoRater(IWorkflowProvider plugIn, bool autoRateRevenue, bool autoRateCosts, bool excludeConsolLevelCharges)
		{
			return new WorkflowAutoRater((IBusiness)plugIn, autoRateRevenue, autoRateCosts, excludeConsolLevelCharges);
		}
	}
}
