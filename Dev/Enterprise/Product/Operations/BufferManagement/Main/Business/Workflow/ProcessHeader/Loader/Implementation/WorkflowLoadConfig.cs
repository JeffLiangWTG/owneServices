using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowLoadConfig
	{
		internal ZGuid ReleaseGroupPK { get; set; }

		internal int? MaxRowsToLoad { get; set; }

		internal bool LoadNonClosedWorkflowsOnly { get; set; }
	}
}
