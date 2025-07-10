using Enterprise.DocumentEngine.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class CustomerServiceReports : ZReportModule
	{
		public CustomerServiceReports()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.CustomerServiceReports; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.CustomerServiceReports; }
		}
	}
}
