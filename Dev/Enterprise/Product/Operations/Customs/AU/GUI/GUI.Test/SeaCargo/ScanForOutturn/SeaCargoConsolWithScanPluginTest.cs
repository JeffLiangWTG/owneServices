using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoConsolWithScanPluginTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		public void TestOceanBillSynchronizerWhenloadThePlugin()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "TESTHB001";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "TESTOceanBill001";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			using (var plugin = new SeaCargoConsolWithScanPlugin(consol))
			{
				consol.Shipments.Add(shipment);
				AssertEquals("Create a new house bill for the shipment", 1, oceanBill.HouseBills.Count);
				var houseBill = oceanBill.HouseBills[0];
				AssertEquals(shipment.PK, houseBill.CA_JS);
			}

			var consol2 = Factory.New<ForwardingConsol>();
			var shipment2 = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "TESTHB001";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			using (var plugin2 = new SeaCargoConsolWithScanPlugin(consol2))
			{
				consol2.Shipments.Add(shipment2);
				AssertNull("No ocean bill created for the consol", consol2.AUCMRCusSCAOceanBill);
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			return new SeaCargoConsolWithScanPlugin(consol);
		}
	}
}
