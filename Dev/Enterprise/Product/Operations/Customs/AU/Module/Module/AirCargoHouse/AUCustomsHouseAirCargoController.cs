using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	/// <summary>
	/// Module Controller for AUCustomsHouseAirCargo.
	/// </summary>
	public class AUCustomsHouseAirCargoController : ZController
	{
		public AUCustomsHouseAirCargoController()
		{
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.AU.HouseAirCargo;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.AU.HouseAirCargo;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusHAWB);

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|AUCustomsHouseAirCargo", "Air Cargo");

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => Factory.New(typeof(CusHAWB));

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var houseBill = businessEntity as CusHAWB;
			ZForm result = null;
			if (houseBill.Shipment == null)
			{
				result = new AirCargoHouseForm(houseBill);
			}
			return result;
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new AirCargoShipmentPlugIn(businessEntity as ForwardingShipment);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ACAHouseView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ACAHouseModify;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ACAHouseNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ACAHouseDelete;
	}
}
