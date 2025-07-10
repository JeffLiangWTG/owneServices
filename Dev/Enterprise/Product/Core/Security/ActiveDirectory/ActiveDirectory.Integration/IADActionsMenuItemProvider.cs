using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Security.ActiveDirectory
{
	public interface IADActionsMenuItemProvider
	{
		IMenuItem GetFormMenuItem(IADLinkedEntity parent);
		IMenuItem GetModuleMenuItem(IZFilterGridModule parent, BusinessObjectFactory factory);
	}
}
