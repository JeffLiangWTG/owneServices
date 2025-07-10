using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.AirCargo.Testing
{
	[TestedType(typeof(AUCustomsHouseAirCargoController))]
	sealed class AUCustomsHouseAirCargoControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			AssertEquals(Env.Security.ACAHouseDelete, new AUCustomsHouseAirCargoController().CheckPointForDeleteExposedForTest);
			AssertEquals(Env.Security.ACAHouseModify, new AUCustomsHouseAirCargoController().CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.ACAHouseNew, new AUCustomsHouseAirCargoController().CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.ACAHouseView, new AUCustomsHouseAirCargoController().CheckPointForViewExposedForTest);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.HouseAirCargo;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKDestination = "INBOM";
			shipment.JS_GoodsDescription = "GoodsDescription";
			shipment.JS_UniqueConsignRef = "BlahBlahBlah";
			shipment.JS_ReleaseType = shipment.Lookups.JS_ReleaseType_List[0].Code;
			var bizO = base.GetBusinessObjectWithoutValidationErrors() as CusHAWB;
			bizO.CS_JS = shipment.PK;
			bizO.CS_HAWB = "bill1";
			Factory.Save();
			return bizO;
		}

		protected override IEnumerable<ControllerID> NonCustomsPlugInsToExcludeFromTest => new ControllerID[] { ControllerIDs.Customs.AU.AirCargoDeclarationCusUnderbondController }.Union(base.NonCustomsPlugInsToExcludeFromTest);
	}
}
