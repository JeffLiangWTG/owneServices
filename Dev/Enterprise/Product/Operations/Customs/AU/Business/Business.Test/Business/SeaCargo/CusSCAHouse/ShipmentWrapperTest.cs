using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ShipmentWrapper))]
	sealed class ShipmentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShipmentWrapper(shipment);
		}

		public void TestSynchroniseIfWeCan()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;
			//Factory.Save();
			var shipmentWrapper = new ShipmentWrapper(shipment);
			AssertEquals("Pre-condition: Should have no Port of Origin set on the house bill", "", shipmentWrapper.HouseBill.CA_RL_NK_PortOfOrigin);
			shipment.JS_RL_NKOrigin = "USLAX";
			shipmentWrapper.SynchroniseIfWeCan();
			AssertEquals("Origin port should have been synched", "USLAX", shipmentWrapper.HouseBill.CA_RL_NK_PortOfOrigin);
		}

		public void TestSynchroniseIfWeCan_DeletedShipment()
		{
			shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = TestHouseBillNumber;
			shipment.JS_RL_NKOrigin = TestPortOfLoading;
			shipment.JS_RL_NKDestination = TestPortOfDischarge;
			shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);

			var oceanBill = Factory.New<CusSCAOceanBill>();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;

			var shipmentWrapper = new ShipmentWrapper(shipment);

			shipment.Delete();
			AssertEquals(true, shipment.IsDeleted);
			AssertNoExceptionThrown("Should not throw any errors", () => shipmentWrapper.SynchroniseIfWeCan());
		}

		public void TestSeaCargoSynchroniserHasJustThisShipment()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment2.JS_HouseBill = TestHouseBillNumber;
			shipment2.JS_RL_NKOrigin = TestPortOfLoading;
			shipment2.JS_RL_NKDestination = TestPortOfDischarge;
			shipment2.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);

			var shipmentWrapper = new ShipmentWrapper(shipment2);
			var houseBill = shipmentWrapper.GetHouseBill;
			AssertSame(oceanBill, houseBill.OceanBill);
			AssertEquals("Wrapper has just the one synchroniser", 1, shipmentWrapper.SeaCargoSynchroniser.BusinessObjectSynchronisers.Count);

			AssertEquals("Should have synched HouseBillNumber on the house bill", TestHouseBillNumber, houseBill.CA_HouseBill);
			AssertEquals("Should have synched Port of Origin on the house bill", TestPortOfLoading, houseBill.CA_RL_NK_PortOfOrigin);
			AssertEquals("Should have synched Port of Destination on the house bill", TestPortOfDischarge, houseBill.CA_RL_NK_PortOfDestination);

			shipment2.JS_RL_NKOrigin = "USLAX";
			AssertEquals("Origin port should have synched", "USLAX", houseBill.CA_RL_NK_PortOfOrigin);
		}

		public void TestTopLevelObject()
		{
			var shipmentWrapper = new ShipmentWrapper(shipment);
			AssertEquals("TopLevelObject", shipment, ((ISeaCargoShipmentInfo)shipmentWrapper).TopLevelObject);
		}

		#region Implementation

		const string TestLloydsNumber = "1234567";
		const string TestVesselName = "SCOTTSFLOATINGBROTHEL";
		const string TestOceanBill = "OCEANTEST123";
		const string TestPortOfLoading = "HKHKG";
		const string TestPortOfDischarge = "AUSYD";
		const string TestPrincipalID = "C067764014";
		const string TestHouseBillNumber = "TESTHOUSE123";
		ForwardingShipment shipment;
		ForwardingConsol consol;

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestVessel(TestVesselName);
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = TestPortOfLoading;
			consol.JK_RL_NKDischargePort = TestPortOfDischarge;

			var transport = consol.Transports[0];
			consol.JK_MasterBillNum = TestOceanBill;
			transport.JW_Vessel = TestVesselName;
			transport.JW_VoyageFlight = "23";

			shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = TestHouseBillNumber;
			shipment.JS_RL_NKOrigin = TestPortOfLoading;
			shipment.JS_RL_NKDestination = TestPortOfDischarge;
			shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TESTIGNEE";
			consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
			consignee.MiscServ.OM_IMDefaultINCOTerm = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
			shipment.ConsigneePK = consignee.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();
			shipment.Consols.Load();
		}

		void CreateTestVessel(string vesselName)
		{
			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = vesselName;
			vessel.RV_LloydsNumber = TestLloydsNumber;
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "TESTSHIP";
			shippingLine.MainAddress.OA_Address1 = "TEST SHIP ADDRESS";
			shippingLine.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, TestPrincipalID);
			vessel.RV_OH = shippingLine.PK;
		}

		#endregion Implementation
	}
}
