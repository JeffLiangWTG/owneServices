using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class ADActionsMenuItemProvider : IADActionsMenuItemProvider
	{
		public IMenuItem GetFormMenuItem(IADLinkedEntity parent)
		{
			if (parent.IsADIntegrationEnabled && (parent.IsADLinked || parent.IsActive))
			{
				return new ADActionsMenuItem(parent);
			}
			else
			{
				return null;
			}
		}

		public IMenuItem GetModuleMenuItem(IZFilterGridModule parent, BusinessObjectFactory factory)
		{
			return new ModuleADActionsMenuItem((ZFilterGridModule)parent, factory);
		}
	}
}
