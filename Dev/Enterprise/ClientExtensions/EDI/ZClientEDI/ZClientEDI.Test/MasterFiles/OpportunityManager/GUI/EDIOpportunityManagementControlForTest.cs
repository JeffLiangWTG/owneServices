using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	public class EDIOpportunityManagementControlForTest : EDIOpportunityManagementControl
	{
		public ZModuleButtonGrid OpportunitiesGridExposed()
		{
			return OpportunitiesGrid;
		}
	}
}
