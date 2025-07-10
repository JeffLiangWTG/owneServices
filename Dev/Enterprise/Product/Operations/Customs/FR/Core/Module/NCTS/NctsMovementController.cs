using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.GUI.NCTS;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.FR.Module
{
	public class NctsMovementController : EU.NCTS.Module.NctsMovementController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(NctsHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var nctsMovement = (NctsHeader)businessEntity;
			return nctsMovement.IsPhase5 ? base.GetForm(businessEntity) : new NctsMovementForm(nctsMovement);
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new NctsPlugin((ICusInBondParent)businessEntity);
	}
}
