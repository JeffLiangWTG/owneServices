using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIAdministrationPanelControllerOverride : AdministrationPanelController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIAdministrationPanelForm((AdministrationPanelManager)businessEntity);
		}
	}
}
