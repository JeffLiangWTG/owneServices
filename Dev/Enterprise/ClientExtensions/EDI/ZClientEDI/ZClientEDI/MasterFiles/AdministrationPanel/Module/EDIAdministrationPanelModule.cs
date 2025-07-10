using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIAdministrationPanelModule : Enterprise.MasterFiles.Module.AdministrationPanelModule
	{
		protected override ZPopupController GetNewController()
		{
			return new EDIAdministrationPanelControllerOverride();
		}
	}
}
