using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CH.NCTS.Module;

public class NctsMovementController : EU.NCTS.Module.NctsMovementController
{
	protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new GUI.NctsPlugin((Freight.Integration.ICusInBondParent)businessEntity);
}
