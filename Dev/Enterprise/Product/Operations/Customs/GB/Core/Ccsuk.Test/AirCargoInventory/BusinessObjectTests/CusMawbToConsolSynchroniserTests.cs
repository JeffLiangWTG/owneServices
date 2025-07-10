using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	public class CusMawbToConsolSynchroniserTests : TestCaseWithFactory
	{
		public void TestUpdateLoadAndDischarge()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			var transport1 = consol.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportType = "FL1";
			transport1.JW_RL_NKLoadPort = "ITSPE";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportType = "FL2";
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "GBLON";

			var bridge = new CusMawbToConsolSynchoniser(basic);
			bridge.UpdateLoadPort(transport2);
			bridge.UpdateDischargePort(transport2);

			AssertEquals("", basic.CM_RL_NKLoadPort);
			AssertEquals("", basic.CM_RL_NKDischargePort);

			bridge.UpdateLoadPort(transport1);
			bridge.UpdateDischargePort(transport1);

			AssertEquals("ITSPE", basic.CM_RL_NKLoadPort);
			AssertEquals("SIN", basic.CM_RL_NKDischargePort);
		}

		public void TestFieldsAreSynchedByDefault()
		{
			var bridge = new CusMawbToConsolSynchoniser(basic);

			var arrivalDate = ZDateTime.BrettsBirthday;
			var dischargePort = "LHR";
			var flightNumber = "BA123";
			var loadPort = "USATL";
			var mawbNumber = "12512345678";

			((IUpdateFromConsol)bridge).ArrivalDate = arrivalDate;
			((IUpdateFromConsol)bridge).DischargePort = dischargePort;
			((IUpdateFromConsol)bridge).FlightNumber = flightNumber;
			((IUpdateFromConsol)bridge).LoadPort = loadPort;
			((IUpdateFromConsol)bridge).MAWBNumber = mawbNumber;

			AssertEquals(arrivalDate, basic.CM_ArrivalDate);
			AssertEquals(dischargePort, basic.AirportOfDestination);
			AssertEquals(flightNumber, basic.CM_FlightNo);
			AssertEquals(loadPort, basic.AirportOfOrigin);
			AssertEquals(mawbNumber, basic.CM_MAWB);

			// CX means all fields can be set
			basic.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestCancelled, ZDateTime.Now);
			((IUpdateFromConsol)bridge).ArrivalDate = ZDateTime.BrettsBirthday;
			AssertEquals("CusMAWB.CM_ArrivalDate updated by changes to bridge, field is open", ZDateTime.BrettsBirthday, basic.CM_ArrivalDate);
			((IUpdateFromConsol)bridge).DischargePort = "POO";
			AssertEquals("CusMAWB.AirportOfDestination updated by changes to bridge, field is open", "POO", basic.AirportOfDestination);
			((IUpdateFromConsol)bridge).FlightNumber = "BA69";
			AssertEquals("CusMAWB.CM_FlightNo updated by changes to bridge, field is open", "BA69", basic.CM_FlightNo);
			((IUpdateFromConsol)bridge).LoadPort = "BUM";
			AssertEquals("CusMAWB.AirportOfOrigin updated by changes to bridge, field is open", "BUM", basic.AirportOfOrigin);
			((IUpdateFromConsol)bridge).MAWBNumber = "665544";
			AssertEquals("CusMAWB.CM_MAWB updated by changes to bridge, field is open", "665544", basic.CM_MAWB);

			// CT means all fields locked
			basic.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval, ZDateTime.Now);
			((IUpdateFromConsol)bridge).ArrivalDate = ZDateTime.Now;
			AssertEquals("CusMAWB.CM_ArrivalDate UNCHANGED by changes to bridge, field is locked", ZDateTime.BrettsBirthday, basic.CM_ArrivalDate);
			((IUpdateFromConsol)bridge).DischargePort = "new";
			AssertEquals("CusMAWB.AirportOfDestination UNCHANGED by changes to bridge, field is locked", "POO", basic.AirportOfDestination);
			((IUpdateFromConsol)bridge).FlightNumber = "new";
			AssertEquals("CusMAWB.CM_FlightNo UNCHANGED by changes to bridge, field is locked", "BA69", basic.CM_FlightNo);
			((IUpdateFromConsol)bridge).LoadPort = "new";
			AssertEquals("CusMAWB.AirportOfOrigin UNCHANGED by changes to bridge, field is locked", "BUM", basic.AirportOfOrigin);
			((IUpdateFromConsol)bridge).MAWBNumber = "new";
			AssertEquals("CusMAWB.CM_MAWB UNCHANGED by changes to bridge, field is locked", "665544", basic.CM_MAWB);
		}

		public void TestCommunityHandlingCodesSpecialHandlingCodes()
		{
			var bridge = new CusMawbToConsolSynchoniser(basic);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_RL_NKLoadPort = "USATL";
			consol.JK_MasterBillNum = "12512345678";
			bridge.SynchroniseFromConsol(consol);
			AssertEquals(0, basic.CommunityHandlingCodes.Count);
			var shc1 = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			var shc2 = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			shc1.EP_SpecialHandling = "ABC";
			shc2.EP_SpecialHandling = "DEF";
			bridge.SynchroniseFromConsol(consol);
			AssertEquals(2, basic.CommunityHandlingCodes.Count);
			var shc3 = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			shc3.EP_SpecialHandling = "XYZ";
			bridge.SynchroniseFromConsol(consol);
			AssertEquals("No duplicates", 3, basic.CommunityHandlingCodes.Count);
		}

		CusMAWB basic;
		protected override void SetUp()
		{
			base.SetUp();
			basic = Factory.New<CusMAWB>();
		}
	}
}
