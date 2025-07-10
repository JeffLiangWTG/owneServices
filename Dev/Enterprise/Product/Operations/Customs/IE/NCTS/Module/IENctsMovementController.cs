using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Module;
using Enterprise.Customs.IE.NCTS.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IE.NCTS.Module
{
	public class IENctsMovementController : NctsMovementController
	{
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new NctsPlugin((ICusInBondParent)businessEntity);
	}
}
