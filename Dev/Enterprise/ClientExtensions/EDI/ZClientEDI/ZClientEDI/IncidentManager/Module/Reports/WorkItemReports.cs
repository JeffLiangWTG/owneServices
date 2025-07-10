using Enterprise.DocumentEngine.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class WorkItemReports : ZReportModule
	{
		public WorkItemReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.WorkItemsReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.DevelopmentReports; }
		}
	}
}
