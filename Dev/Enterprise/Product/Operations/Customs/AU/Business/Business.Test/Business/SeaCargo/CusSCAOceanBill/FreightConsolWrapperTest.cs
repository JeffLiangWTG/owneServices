using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.SeaCargo.Test
{
	[TestedType(typeof(FreightConsolWrapper))]
	sealed class FreightConsolWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new FreightConsolWrapper(consol);
		}

		public void TestTopLevelObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolWrapper = new FreightConsolWrapper(consol);
			AssertEquals(consol, consolWrapper.TopLevelObject);
		}

		public void TestOceanBillTypeFromConsolWrapper()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2005, 10, 13);

			var consolWrapper = new FreightConsolWrapper(consol);
			Assert("ConsolWrapper OceanBill Type", consolWrapper.SeaCargoSynchroniser.OceanBill is CusSCAOceanBill);
		}

		public void TestSeaCargoSynchroniserHasEveryShipment()
		{
			CreateTestVessel(TestVesselName);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = TestMasterBillNumber;
			consol.JK_RL_NKLoadPort = TestPortOfLoading;
			consol.JK_RL_NKDischargePort = TestPortOfDischarge;

			var transport = consol.Transports[0];
			transport.JW_Vessel = TestVesselName;
			transport.JW_VoyageFlight = "23";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment1.JS_HouseBill = TestHouseBillNumber;
			shipment1.JS_RL_NKOrigin = TestPortOfLoading;
			shipment1.JS_RL_NKDestination = TestPortOfDischarge;
			shipment1.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TESTIGNEE";
			consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
			consignee.MiscServ.OM_IMDefaultINCOTerm = Enterprise.Core.Constants.IncoTerms.CostAndFreight;
			shipment1.ConsigneePK = consignee.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
			shipment1.ConsignorPK = consignor.PK;

			Factory.Save();
			shipment1.Consols.Load();

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment2.JS_HouseBill = Test2NDHouseBillNumber;
			shipment2.JS_RL_NKOrigin = TestPortOfLoading;
			shipment2.JS_RL_NKDestination = TestPortOfDischarge;
			shipment2.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);

			var consolWrapper = new FreightConsolWrapper(consol);
			var oceanBill = consolWrapper.SeaCargoSynchroniser.OceanBill;
			AssertEquals("Wrapper has all required synchronisers", 3, consolWrapper.SeaCargoSynchroniser.BusinessObjectSynchronisers.Count);

			AssertEquals("Should have synched MasterBillNumber on the ocean bill", TestMasterBillNumber, oceanBill.CB_OceanBill);
			AssertEquals("Should have synched Port of Loading on the ocean bill", TestPortOfLoading, oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Should have synched Port of Discharge on the ocean bill", TestPortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);

			var houseBill1 = oceanBill.HouseBills[0];
			AssertEquals("Should have synched HouseBillNumber on the house bill", TestHouseBillNumber, houseBill1.CA_HouseBill);
			AssertEquals("Should have synched Port of Origin on the house bill", TestPortOfLoading, houseBill1.CA_RL_NK_PortOfOrigin);
			AssertEquals("Should have synched Port of Destination on the house bill", TestPortOfDischarge, houseBill1.CA_RL_NK_PortOfDestination);

			var houseBill2 = oceanBill.HouseBills[1];
			AssertEquals("Should have synched HouseBillNumber on the house bill", Test2NDHouseBillNumber, houseBill2.CA_HouseBill);
			AssertEquals("Should have synched Port of Origin on the house bill", TestPortOfLoading, houseBill2.CA_RL_NK_PortOfOrigin);
			AssertEquals("Should have synched Port of Destination on the house bill", TestPortOfDischarge, houseBill2.CA_RL_NK_PortOfDestination);

			consol.JK_RL_NKLoadPort = "USLAX";
			shipment1.JS_RL_NKOrigin = "USLAX";
			shipment2.JS_RL_NKOrigin = "USLAX";
			AssertEquals("Origin port should have synched", "USLAX", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Origin port should have synched", "USLAX", houseBill1.CA_RL_NK_PortOfOrigin);
			AssertEquals("Origin port should have synched", "USLAX", houseBill2.CA_RL_NK_PortOfOrigin);
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

		const string TestLloydsNumber = "1234567";
		const string TestVesselName = "SCOTTSFLOATINGBROTHEL";
		const string TestPortOfLoading = "HKHKG";
		const string TestPortOfDischarge = "AUSYD";
		const string TestPrincipalID = "C067764014";
		const string TestMasterBillNumber = "TESTMASTER123";
		const string TestHouseBillNumber = "TESTHOUSE1";
		const string Test2NDHouseBillNumber = "TESTHOUSE2";
	}
}
