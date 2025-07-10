using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class MessageInventoryModuleButtonGrid : ZModuleButtonGrid
	{
		public MessageInventoryModuleButtonGrid()
		{
			ModuleID = ClientModuleRegistration.IncidentManagementGroup;
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
		}
	}
}
