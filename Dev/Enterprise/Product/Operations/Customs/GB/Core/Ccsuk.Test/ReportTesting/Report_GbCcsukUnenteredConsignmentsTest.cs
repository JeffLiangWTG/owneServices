using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.GB.Ccsuk.ReportTesting.Testing
{
	class Report_GbCcsukUnenteredConsignmentsFunctionalTest : ReportFunctionalTestCase
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var mawb = new ReportSchemaColumn(typeof(string), "MAWB");
				var hawb = new ReportSchemaColumn(typeof(string), "HAWB");
				var split = new ReportSchemaColumn(typeof(string), "Split");
				var arrival = new ReportSchemaColumn(typeof(DateTime), "Arrival");
				var airportAndShed = new ReportSchemaColumn(typeof(string), "AirportAndShed");
				var agent = new ReportSchemaColumn(typeof(string), "Agent");
				var piecesExpected = new ReportSchemaColumn(typeof(short), "PiecesExpected");
				var piecesReceived = new ReportSchemaColumn(typeof(short), "PiecesReceived");
				var cac = new ReportSchemaColumn(typeof(string), "CAC");
				var cat = new ReportSchemaColumn(typeof(string), "CAT");
				return new List<ReportSchemaColumn>() { mawb, hawb, split, arrival, airportAndShed, agent, piecesExpected, piecesReceived, cac, cat };
			}
		}

		protected override SqlObjectType SqlObjectType
		{
			get { return Enterprise.ReportTesting.SqlObjectType.FunctionTable; }
		}

		protected override ZString ObjectName
		{
			get { return "Report_GbCcsukUnenteredConsignments"; }
		}

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals(6, results.Rows.Count);
			var rowBasic1 = FormatRowsValues(results.Rows[0], results);
			var rowBasic3 = FormatRowsValues(results.Rows[1], results);
			var rowBasic4S2 = FormatRowsValues(results.Rows[2], results);
			var rowComboHouse = FormatRowsValues(results.Rows[3], results);
			var rowHawb1 = FormatRowsValues(results.Rows[4], results);
			var rowHawb3S2 = FormatRowsValues(results.Rows[5], results);

			AssertContains("[MAWB]='11100000000'; [HAWB]=''; [Split]=''", rowBasic1);
			AssertContains("[CAC]=''", rowBasic1);

			AssertContains("[MAWB]='33300000000'; [HAWB]=''; [Split]=''", rowBasic3);

			AssertContains("[MAWB]='44400000000'; [HAWB]=''; [Split]='02'", rowBasic4S2);

			AssertContains("[MAWB]='MMM11111111'; [HAWB]='H1111111'; [Split]=''", rowHawb1);

			AssertContains("[MAWB]='MMM22222222'; [HAWB]='H3333333'; [Split]='02'", rowHawb3S2);

			CheckTheComboHouseRow(rowComboHouse);
		}

		void CheckTheComboHouseRow(string rowComboHouse)
		{
			AssertContains("[MAWB]='COMBO_MASTER'; [HAWB]='COMBO_HOUSE'; [Split]=''", rowComboHouse);
			AssertContains("[Agent]='HOUSE_PARTY'", rowComboHouse);
			AssertContains("[PiecesExpected]='39'", rowComboHouse);
			AssertContains("[PiecesReceived]='35'", rowComboHouse);

			AssertNotContains("COMBO_SPLIT", rowComboHouse);
		}

		protected override void PrepareTestData()
		{
			PrepareBasics();
			PrepareHouses();
			Prepare_a_MasterAWB_with_a_Split_and_a_HouseAWB();
		}

		void PrepareBasics()
		{
			var basic1 = Factory.New<CusMAWB>();
			basic1.CM_MAWB = "11100000000";
			basic1.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(1);

			var basic2CC = Factory.New<CusMAWB>();
			basic2CC.CM_MAWB = "22200000000";
			basic2CC.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(2);
			basic2CC.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);

			var basic3Include = Factory.New<CusMAWB>();
			basic3Include.CM_MAWB = "33300000000";
			basic3Include.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(3);

			var basicCAExclude = Factory.New<CusMAWB>();
			basicCAExclude.CM_MAWB = "33311111111";
			basicCAExclude.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(3);
			basicCAExclude.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday.AddDays(3));

			var basic4WithSplits = Factory.New<CusMAWB>();
			basic4WithSplits.CM_MAWB = "44400000000";
			basic4WithSplits.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(4);
			var split1 = basic4WithSplits.Splits.AddNew();
			var split2 = basic4WithSplits.Splits.AddNew();
			var split3 = basic4WithSplits.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			split3.SplitReference = "03";
			split1.SetCustomsActionCode("CB", ZDateTime.BrettsBirthday.AddDays(5));
			split3.SetCustomsActionCode(CustomsStatusCodes.Codes.CustomsQueriedDetained, ZDateTime.BrettsBirthday.AddDays(5));

			var basic5CSExclude = Factory.New<CusMAWB>();
			basic5CSExclude.CM_MAWB = "55500000000";
			basic5CSExclude.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(3);
			basic5CSExclude.SetCustomsActionCode("CS", ZDateTime.BrettsBirthday.AddDays(3));

			var basic6InactiveExclude = Factory.New<CusMAWB>();
			basic6InactiveExclude.CM_MAWB = "66611111111";
			basic6InactiveExclude.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(3);
			basic6InactiveExclude.SetCustomsActionCode("CA", ZDateTime.BrettsBirthday.AddDays(3));
			basic6InactiveExclude.CM_IsActive = false;

			var basic7CRExclude = Factory.New<CusMAWB>();
			basic7CRExclude.CM_MAWB = "77700000000";
			basic7CRExclude.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(3);
			basic7CRExclude.SetCustomsActionCode(CustomsStatusCodes.Codes.CustomsQueriedDetained, ZDateTime.BrettsBirthday.AddDays(3));

			var basic8EcStatus = Factory.New<CusMAWB>();
			basic8EcStatus.CM_MAWB = "88800000000";
			basic8EcStatus.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport; // C-status
		}

		void PrepareHouses()
		{
			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "MMM11111111";
			mawb1.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(1);
			var hawb1 = mawb1.ChildBills.AddNew();
			hawb1.CS_HAWB = "H1111111";

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "MMM22222222";
			mawb2.CM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(2);
			var hawb2 = mawb2.ChildBills.AddNew();
			hawb2.CS_HAWB = "H2222222";
			hawb2.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday.AddDays(2));

			var hawb3Splits = mawb2.ChildBills.AddNew();
			hawb3Splits.CS_HAWB = "H3333333";
			var split1 = hawb3Splits.Splits.AddNew();
			var split2 = hawb3Splits.Splits.AddNew();
			var split3 = hawb3Splits.Splits.AddNew();
			split1.SplitReference = "01";
			split2.SplitReference = "02";
			split3.SplitReference = "03";
			split1.SetCustomsActionCode("CB", ZDateTime.BrettsBirthday.AddDays(3));
			split3.SetCustomsActionCode(CustomsStatusCodes.Codes.CustomsQueriedDetained, ZDateTime.BrettsBirthday.AddDays(3));

			var hawb4Inactive = mawb2.ChildBills.AddNew();
			hawb4Inactive.CS_HAWB = "H4444444";
			hawb4Inactive.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday.AddDays(2));
			hawb4Inactive.CS_IsActive = false;

			var hawb5CrStatusExclude = mawb2.ChildBills.AddNew();
			hawb5CrStatusExclude.CS_HAWB = "H5555555";
			hawb5CrStatusExclude.SetCustomsActionCode(CustomsStatusCodes.Codes.CustomsQueriedDetained, ZDateTime.BrettsBirthday.AddDays(2));

			var hawbNoMawb = Factory.New<CusHAWB>();
			hawbNoMawb.CS_IsActive = true;
			hawbNoMawb.CS_HAWB = "NoMawb";
			hawbNoMawb.CS_IsMasterHouse = false;

			var hawb7EcStatus = mawb2.ChildBills.AddNew();
			hawb7EcStatus.CS_HAWB = "H7777777";
			hawb7EcStatus.ShipmentDescriptionCode = ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport; // C-status
		}

		void Prepare_a_MasterAWB_with_a_Split_and_a_HouseAWB()
		{
			var master = Factory.New<CusMAWB>();
			master.CM_MAWB = "COMBO_MASTER";
			master.CM_FlightNo = "MASTER_FLY";
			master.CM_ResponsiblePartyID = "MASTER_PARTY";
			master.NumberOfPiecesExpected = 19;
			master.NumberOfPiecesReceived = 15;

			var split = master.Splits.AddNew();
			split.CG_MessageReference = "COMBO_SPLIT";
			split.CG_FlightNo = "SPT";
			split.NumberOfPiecesExpected = 29;
			split.NumberOfPiecesReceived = 25;

			var house = master.ChildBills.AddNew();
			house.CS_HAWB = "COMBO_HOUSE";
			house.CS_ResponsiblePartyID = "HOUSE_PARTY";
			house.CS_PiecesManifested = 39;
			house.CS_PiecesLanded = 35;
		}
	}
}
