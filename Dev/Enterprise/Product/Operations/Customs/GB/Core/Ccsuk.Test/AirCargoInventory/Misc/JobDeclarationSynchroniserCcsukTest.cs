using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class JobDeclarationSynchroniserCcsukTest : EU.Business.Declaration.Testing.JobDeclarationSynchroniserTest
	{
		protected override EU.Business.Declaration.JobDeclaration GetDeclaration()
		{
			var dec =  Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return dec;
		}

		public void TestSynchroniseWithCcsuk_WithoutBasic()
		{
			ForwardingConsol consol;
			ForwardingShipment workerShipment;
			CreateConsolAndShipment(out consol, out workerShipment, Constants.AgentType.Direct);
			var dec = GetSynchronisedDec(workerShipment, MessageTypeList.Codes.Import);
			AssertDecWithoutAwb(dec);
		}

		void RunSynchroniseWithCcsuk_Basic(ForwardingConsol consol, ForwardingShipment workerShipment, out CusMAWB mawb, out EU.Business.Declaration.JobDeclaration dec)
		{
			mawb = Factory.New<CusMAWB>();
			mawb.CM_ArrivalDate = new ZDateTime(1986, 3, 12);
			SetPorts(mawb);
			mawb.DescriptionOfGoods = "CCSUK STUFF";
			mawb.NumberOfPiecesExpected = 2;
			mawb.AgentBadge = "CAR";
			mawb.CargoTerminalOperator = "CAX";
			mawb.CargoTerminalOperatorAirport = "LHR";
			mawb.Weight = 69m;
			mawb.CM_JK = consol.PK;
			dec = GetSynchronisedDec(workerShipment, MessageTypeList.Codes.Import);
		}

		public void TestSynchroniseWithCcsuk_Basic()
		{
			ForwardingConsol consol;
			ForwardingShipment workerShipment;
			CreateConsolAndShipment(out consol, out workerShipment, Constants.AgentType.Direct);
			CusMAWB mawb;
			EU.Business.Declaration.JobDeclaration dec;
			RunSynchroniseWithCcsuk_Basic(consol, workerShipment, out mawb, out dec);
			AssertDecWhenLinked(dec);
			AssertEquals(dec.PK, mawb.MasterLevelHouseHelper.CS_JE_CustomsFormalEntry);
		}

		public void TestSynchroniseWithCcsuk_BasicButCrapConsolAgentType()
		{
			// Basic AWB but user has registered the consol as AGT instead of DRT, meaning we look at the shipment's HAWB (and find none) rather than at the consol's MAWB. Expect to see base behavior, as if we saw no AWB. 
			ForwardingConsol consol;
			ForwardingShipment workerShipment;
			CreateConsolAndShipment(out consol, out workerShipment, Constants.AgentType.Agent);
			var dec = GetSynchronisedDec(workerShipment, MessageTypeList.Codes.Import);
			AssertDecWithoutAwb(dec);  // Note!
		}

		public void TestSynchroniseWithCcsuk_House()
		{
			ForwardingConsol consol;
			ForwardingShipment shipment;
			CreateConsolAndShipment(out consol, out shipment, Constants.AgentType.Agent);

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_ArrivalDate = new ZDateTime(1986, 3, 12);
			var hawb = mawb.ChildBills.AddNew();
			SetPorts(hawb);
			hawb.CS_GoodsDescription = "CCSUK STUFF";
			hawb.CS_PiecesManifested = 2;
			hawb.AgentBadge = "CAR";
			hawb.CargoTerminalOperator = "CAX";
			hawb.CargoTerminalOperatorAirport = "LHR";
			hawb.CS_Weight = 69m;

			hawb.CS_JS = shipment.PK;
			var dec = GetSynchronisedDec(shipment, MessageTypeList.Codes.Import);
			AssertDecWhenLinked(dec);
			AssertEquals(dec.PK, hawb.CS_JE_CustomsFormalEntry);
		}

		static void SetPorts(ICcsukCusAwb awb)
		{
			awb.AirportOfOrigin = "AUBNE";
			awb.AirportOfArrival = "GBMAN";
			awb.AirportOfDestination = "GBLHR";
		}

		public void TestSynchroniseWithCcsuk_WithoutHouse()
		{
			ForwardingConsol consol;
			ForwardingShipment shipment;
			CreateConsolAndShipment(out consol, out shipment, Constants.AgentType.Agent);
			var dec = GetSynchronisedDec(shipment, MessageTypeList.Codes.Import);
			AssertDecWithoutAwb(dec);
		}

		void CreateConsolAndShipment(out ForwardingConsol consol, out ForwardingShipment shipment, string agentType)
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.Transports[0].JW_ETA = new ZDateTime(1979, 8, 9);
			consol.JK_AgentType = agentType;
			shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
			shipment.JS_GoodsDescription = "SHIP STUFF";
			shipment.JS_OuterPacks = 1;
			shipment.JS_ActualChargeable = 1;
			shipment.JS_RL_NKDestination = "GBDTE";
			shipment.JS_RL_NKOrigin = "AUMEL";
		}

		static void AssertDecWithoutAwb(EU.Business.Declaration.JobDeclaration dec)
		{
			AssertEquals("No mawb - use base behavior", new ZDateTime(1979, 8, 9), dec.JE_DateOfArrival);
			AssertEquals("No mawb - use base behavior", "", dec.JE_CustomsProfile);
			AssertEquals("No mawb - GB-specific field is open", false, dec.JE_CustomsProfileInfo.ReadOnly);
			AssertEquals("No mawb - use base behavior, which is to take it from arrival port", "LHR", dec.JE_LocationOfGoods);
			AssertEquals("No mawb - use base behavior", "", dec.SubLocation);
			AssertEquals("No mawb - GB-specific field is open", false, dec.JE_LocationOfGoodsInfo.ReadOnly);
			CombineAssertions(delegate
			{
				AssertEquals("No AWB - use base behavior", "AUSYD", dec.JE_RL_NKPortOfLoading);
				AssertEquals("No AWB - use base behavior", "GBLHR", dec.JE_RL_NKPortOfArrival);
				AssertEquals("No AWB - use base behavior", "AUMEL", dec.JE_RL_NKOrigin);
				AssertEquals("No AWB - use base behavior", "GBDTE", dec.JE_RL_NKFinalDestination);
			});
		}

		static void AssertDecWhenLinked(EU.Business.Declaration.JobDeclaration dec)
		{
			AssertEquals("Has AWB - use this", new ZDateTime(1986, 3, 12), dec.JE_DateOfArrival);
			AssertEquals("Has AWB - use this, and don't let it get clobbered during the setting of the ports", "CAR", dec.JE_CustomsProfile);
			AssertEquals("Has AWB - GB-specific field is locked", true, dec.JE_CustomsProfileInfo.ReadOnly);
			AssertEquals("Has AWB - use this", "LHR", dec.JE_LocationOfGoods);
			AssertEquals("Has AWB - use this", "CAX", dec.SubLocation);
			AssertEquals("Has AWB- GB-specific field is locked", true, dec.JE_LocationOfGoodsInfo.ReadOnly);
			AssertEquals("Has AWB - use this", 2, dec.JE_TotalNoOfPacks);
			AssertEquals("Has AWB - use this", 69m, dec.JE_TotalWeight);
			CombineAssertions(delegate
			{
				AssertEquals("Has AWB - use its details", "AUBNE", dec.JE_RL_NKPortOfLoading);
				AssertEquals("Has AWB - use its details", "GBLHR", dec.JE_RL_NKPortOfArrival);
				AssertEquals("Has AWB - use its details", "AUMEL", dec.JE_RL_NKOrigin);
				AssertEquals("Has AWB - use its details", "GBDTE", dec.JE_RL_NKFinalDestination);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Port, "port");
			var codelist = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListType.Port, "LHR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist.PK, "Type", "CA3");
			Factory.Save();
		}
	}
}
