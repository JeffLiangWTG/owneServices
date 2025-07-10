using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	/// <summary>
	/// Summary description for SeaCargoDepotStandAloneController.
	/// </summary>
	public class AirCargoDepotStandAloneController : AUCustomsHouseAirCargoController
	{
		public override ControllerID ID => ControllerIDs.Customs.AU.AirCargoDepot;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusMAWB);

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.AU.AirCargoDepot;

		protected override Guid GetIDForBusinessEntity(IBusiness entity)
		{
			var mAWBProvider = (ICusMAWBProvider)entity;
			return mAWBProvider.MAWB.PK.ToGuid();
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => Factory.New<CusMAWB>();

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var mAWBProvider = (ICusMAWBProvider)businessEntity;
			return new AirCargoMasterForm(mAWBProvider.MAWB);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ACAMasterImportView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ACAMasterImportModify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ACAMasterImportNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ACAMasterImportDelete;
	}
}
