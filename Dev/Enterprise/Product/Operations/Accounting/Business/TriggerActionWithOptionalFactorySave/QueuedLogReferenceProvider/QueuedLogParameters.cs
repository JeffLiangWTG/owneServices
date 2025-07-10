using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public class QueuedLogParameters
	{
		public virtual IWorkflowProvider WorkflowProvider { get; set; }
		public virtual ZGuid CompanyPK { get; set; }
	}
}
