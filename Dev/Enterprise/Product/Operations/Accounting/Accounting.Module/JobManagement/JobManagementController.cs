using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for JobManagement.
	/// </summary>
	public class JobManagementController : JobManagementControllerBase
	{
		public JobManagementController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobManagement; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobManagement; }
		}
	}
}
