using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.ES.NCTS.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ES.NCTS.Module
{
	public class NctsMovementController : EU.NCTS.Module.NctsMovementController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(NctsHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var nctsMovement = (NctsHeader)businessEntity;
			return nctsMovement.IsPhase5 ? base.GetForm(businessEntity) : new NctsMovementForm(nctsMovement);
		}

		protected override IZForm GetPhase5DepartureForm(EU.NCTS.Business.NctsHeader nctsHeader)
		{
			var esNctsheader = (NctsHeader)nctsHeader;
			var headerTNN = esNctsheader.MovementHeader?.ArrivalHeaderForTNN;
			return headerTNN != null ? new Phase5ArrivalMovementForm(headerTNN) : new Phase5DepartureMovementForm(esNctsheader);
		}

		protected override IZForm GetPhase5ArrivalMovementForm(EU.NCTS.Business.NctsHeader nctsHeader) => new Phase5ArrivalMovementForm((NctsHeader)nctsHeader);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new NctsPlugin((ICusInBondParent)businessEntity);
	}
}
