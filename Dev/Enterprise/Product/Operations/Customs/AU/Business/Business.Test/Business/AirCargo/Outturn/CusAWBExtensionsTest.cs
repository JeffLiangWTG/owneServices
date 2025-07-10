using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusAWBExtensionsTest : TestCaseWithFactory
	{
		public void TestFindStandAloneAirCargo()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill1";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HouseBill2";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			var cusHAWB2 = CusHAWB.CreateNew(consolCusMAWB, shipment2);
			var cusHAWB3 = CusHAWB.CreateNew(consolCusMAWB, shipment3);

			consolCusMAWB.ChildBills.Add(cusHAWB1);
			consolCusMAWB.ChildBills.Add(cusHAWB2);
			consolCusMAWB.ChildBills.Add(cusHAWB3);

			var standAloneHouseBill1 = Factory.New<CusMAWB>();
			standAloneHouseBill1.CM_MAWB = "Master1";
			standAloneHouseBill1.CM_MasterHouseBill = "HouseBill1";

			var standAloneHouseBill2 = Factory.New<CusMAWB>();
			standAloneHouseBill2.CM_MAWB = "Master1";
			standAloneHouseBill2.CM_MasterHouseBill = "HouseBill1";

			var standAloneHouseBill3 = Factory.New<CusMAWB>();
			standAloneHouseBill3.CM_MAWB = "Master1";
			standAloneHouseBill3.CM_MasterHouseBill = "HouseBill2";

			AssertExceptionThrown(typeof(InvalidOperationException), "There are more than one stand alone air cargo for house bill 'HouseBill1'", () => consolCusMAWB.FindStandAloneAirCargo("HouseBill1"));
			AssertNull(consolCusMAWB.FindStandAloneAirCargo("HouseBill4"));
			AssertEquals("HouseBill2", consolCusMAWB.FindStandAloneAirCargo("HouseBill2").CM_MasterHouseBill);
		}

		public void TestGetUnderbonds()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			AssertNull(consolCusMAWB.GetUnderbonds());

			var underbond1 = consolCusMAWB.Underbonds.AddNew();
			var underbond2 = consolCusMAWB.Underbonds.AddNew();
			var underbond3 = consolCusMAWB.Underbonds.AddNew();

			AssertEquals(3, consolCusMAWB.GetUnderbonds().Length);
		}

		public void TestGetSentUnderbonds()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var underbond1 = consolCusMAWB.Underbonds.AddNew();
			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			var underbond2 = consolCusMAWB.Underbonds.AddNew();
			var underbond3 = consolCusMAWB.Underbonds.AddNew();
			underbond3.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;

			AssertEquals(2, consolCusMAWB.GetSentUnderbonds().Length);
		}

		public void TestGetMatchedToSelectedUnderbond()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			consolCusMAWB.ChildBills.Add(cusHAWB1);

			var standAloneHouseBill1 = Factory.New<CusMAWB>();
			standAloneHouseBill1.CM_MAWB = "Master1";
			standAloneHouseBill1.CM_MasterHouseBill = "HouseBill1";

			var consolUnderbond1 = consolCusMAWB.Underbonds.AddNew();
			consolUnderbond1.C4_FlightNo = "QF100";
			consolUnderbond1.C4_ArrivalDate = new ZDateTime(2010, 10, 23);
			consolUnderbond1.C4_OriginPremiseID = "OP1000";
			consolUnderbond1.C4_DischargePremiseID = "DP1000";

			var consolUnderbond2 = consolCusMAWB.Underbonds.AddNew();
			consolUnderbond2.C4_FlightNo = "QF101";
			consolUnderbond2.C4_ArrivalDate = new ZDateTime(2010, 10, 24);
			consolUnderbond2.C4_OriginPremiseID = "OP1001";
			consolUnderbond2.C4_DischargePremiseID = "DP1001";

			var consolUnderbond3 = consolCusMAWB.Underbonds.AddNew();
			consolUnderbond3.C4_FlightNo = "QF101";
			consolUnderbond3.C4_ArrivalDate = new ZDateTime(2010, 10, 24);
			consolUnderbond3.C4_OriginPremiseID = "OP1003";
			consolUnderbond3.C4_DischargePremiseID = "DP1001";

			AssertNull(standAloneHouseBill1.GetMatchedToSelectedUnderbond(consolUnderbond1));

			var shipmentUnderbond1 = standAloneHouseBill1.Underbonds.AddNew();
			shipmentUnderbond1.C4_FlightNo = "QF100";
			shipmentUnderbond1.C4_ArrivalDate = new ZDateTime(2010, 10, 23);
			shipmentUnderbond1.C4_OriginPremiseID = "OP1000";
			shipmentUnderbond1.C4_DischargePremiseID = "DP1000";
			shipmentUnderbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			var shipmentUnderbond11 = standAloneHouseBill1.Underbonds.AddNew();
			shipmentUnderbond11.C4_FlightNo = "QF100";
			shipmentUnderbond11.C4_ArrivalDate = new ZDateTime(2010, 10, 23);
			shipmentUnderbond11.C4_OriginPremiseID = "OP1000";
			shipmentUnderbond11.C4_DischargePremiseID = "DP1000";

			var shipmentUnderbond2 = standAloneHouseBill1.Underbonds.AddNew();
			shipmentUnderbond2.C4_FlightNo = "QF101";
			shipmentUnderbond2.C4_ArrivalDate = new ZDateTime(2010, 10, 24);
			shipmentUnderbond2.C4_OriginPremiseID = "OP1001";
			shipmentUnderbond2.C4_DischargePremiseID = "DP1001";

			var shipmentUnderbond3 = standAloneHouseBill1.Underbonds.AddNew();
			shipmentUnderbond3.C4_FlightNo = "QF101";
			shipmentUnderbond3.C4_ArrivalDate = new ZDateTime(2010, 10, 24);
			shipmentUnderbond3.C4_OriginPremiseID = "OP1002";
			shipmentUnderbond3.C4_DischargePremiseID = "DP1001";

			var shipmentUnderbond4 = standAloneHouseBill1.Underbonds.AddNew();
			shipmentUnderbond4.C4_FlightNo = "QF101";
			shipmentUnderbond4.C4_ArrivalDate = new ZDateTime(2010, 10, 25);
			shipmentUnderbond4.C4_OriginPremiseID = "OP1001";
			shipmentUnderbond4.C4_DischargePremiseID = "DP1002";

			AssertNull(standAloneHouseBill1.GetMatchedToSelectedUnderbond(consolUnderbond3));
			AssertNotNull(standAloneHouseBill1.GetMatchedToSelectedUnderbond(consolUnderbond2));
			AssertEquals(shipmentUnderbond2.PK, standAloneHouseBill1.GetMatchedToSelectedUnderbond(consolUnderbond2).PK);
			AssertNotNull(standAloneHouseBill1.GetMatchedToSelectedUnderbond(consolUnderbond1));
			AssertEquals(shipmentUnderbond11.PK, standAloneHouseBill1.GetMatchedToSelectedUnderbond(consolUnderbond1).PK);
		}

		public void TestIsStandAlone()
		{
			var standAloneHouseBill1 = Factory.New<CusMAWB>();
			standAloneHouseBill1.CM_MAWB = "Master1";
			standAloneHouseBill1.CM_MasterHouseBill = "HouseBill1";

			Assert(standAloneHouseBill1.IsStandAlone());

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			Assert(!consolCusMAWB.IsStandAlone());
		}
	}
}
