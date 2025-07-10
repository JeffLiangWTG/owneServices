using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Customs.DE.NCTS.GUI;
using Enterprise.Customs.EU.NCTS.Module;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.DE.NCTS.Module
{
	public class DENctsMovementController : NctsMovementController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(NctsHeader);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new NctsPlugin((ICusInBondParent)businessEntity);
	}
}
