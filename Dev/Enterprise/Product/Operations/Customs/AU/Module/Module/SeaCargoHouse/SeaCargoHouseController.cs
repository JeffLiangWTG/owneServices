using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoHouseController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.AU.SeaCargoHouseController;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.AU.HouseSeaCargo;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusSCAHouse);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.AUCustomsSCA;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.AUCustomsSCAImportModify;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.AUCustomsSCAImportModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.AUCustomsSCAImportModify;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			IZForm result = null;

			if (Enabled && businessEntity is CusSCAHouse houseBill && houseBill.Shipment == null)
			{
				result = new SeaCargoHouseForm(houseBill);
			}
			return result;
		}

		bool Enabled => CustomsDataRegistry.Instance.IsAUSeaCargoHouseEnabled;
	}
}
