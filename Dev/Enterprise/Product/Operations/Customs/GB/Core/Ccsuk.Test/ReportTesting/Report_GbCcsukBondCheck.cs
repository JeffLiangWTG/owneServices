using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.ReportTesting;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Customs.GB.Ccsuk.ReportTesting.Testing
{
	class Report_GbCcsukBondCheck : ReportFunctionalTestCase
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var mawb = new ReportSchemaColumn(typeof(string), "MAWB");
				var hawb = new ReportSchemaColumn(typeof(string), "HAWB");
				var split = new ReportSchemaColumn(typeof(string), "Split");
				var npr = new ReportSchemaColumn(typeof(short), "NPR");
				var npx = new ReportSchemaColumn(typeof(short), "NPX");
				var piecesReleased = new ReportSchemaColumn(typeof(int), "PiecesReleased");
				var totalOutturnsReceived = new ReportSchemaColumn(typeof(int), "TotalOutturnsReceived");
				var totalOutturnsDelivered = new ReportSchemaColumn(typeof(int), "TotalOutturnsDelivered");
				var receiptDetails = new ReportSchemaColumn(typeof(string), "ReceiptDetails");
				var agent = new ReportSchemaColumn(typeof(string), "Agent");
				var airportAndShed = new ReportSchemaColumn(typeof(string), "AirportAndShed");
				var cac = new ReportSchemaColumn(typeof(string), "CAC");
				var arrival = new ReportSchemaColumn(typeof(DateTime), "Arrival");
				var releaseDetails = new ReportSchemaColumn(typeof(string), "ReleaseDetails");
				return new List<ReportSchemaColumn>() { mawb, hawb, split, npx, npr, totalOutturnsReceived, totalOutturnsDelivered, agent, airportAndShed, arrival, receiptDetails, cac, piecesReleased, releaseDetails };
			}
		}

		protected override SqlObjectType SqlObjectType
		{
			get { return Enterprise.ReportTesting.SqlObjectType.FunctionTable; }
		}

		protected override ZString ObjectName
		{
			get { return "Report_GbCcsukBondCheck"; }
		}

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals(5, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			var row2 = FormatRowsValues(results.Rows[1], results);
			var row3 = FormatRowsValues(results.Rows[2], results);
			var row4 = FormatRowsValues(results.Rows[3], results);
			var row5 = FormatRowsValues(results.Rows[4], results);

			AssertContains("[MAWB]='B2'; [HAWB]=''; [Split]='';", row1);
			AssertContains(FormattableString.Invariant($" [NPX]='100'; [NPR]='60'; [TotalOutturnsReceived]='60'; [TotalOutturnsDelivered]='0'; [ReceiptDetails]='60 PK stored in [{CusOutTurnTest.rowName1}], [Marks]"), row1);

			// This row is returned by the function but is excluded by the WHERE clause on the excel sheet
			AssertContains("[MAWB]='B3'; [HAWB]=''; [Split]='';", row2);
			AssertContains(@"[NPX]='1000'; [NPR]='1000'; [TotalOutturnsReceived]='1000'; [TotalOutturnsDelivered]='1000'; [ReceiptDetails]='400 PK delivered from [], []
600 PK delivered from [], []'; [CAC]=''; [PiecesReleased]='0'; [ReleaseDetails]=''", row2.Replace("dbo.", ""));

			AssertContains("[MAWB]='M1'; [HAWB]='000000H2'; [Split]='';", row3);
			AssertContains("[NPX]='10'; [NPR]='6'; [TotalOutturnsReceived]='6'; [TotalOutturnsDelivered]='0'; [ReceiptDetails]='6 PK stored in [], []'; [CAC]='CC'; [PiecesReleased]='3'; [ReleaseDetails]='", row3);

			AssertContains("[MAWB]='M1'; [HAWB]='000000H3'; [Split]='';", row4);
			AssertContains(FormattableString.Invariant($@"[NPX]='100'; [NPR]='100'; [TotalOutturnsReceived]='100'; [TotalOutturnsDelivered]='100'; [ReceiptDetails]='40 PK delivered from [], []
60 PK delivered from [{CusOutTurnTest.rowName2}], []'; [CAC]=''; [PiecesReleased]='0'; [ReleaseDetails]=''"), row4.Replace("dbo.", ""));

			AssertContains("[MAWB]='M1'; [HAWB]='000000H4'; [Split]='01';", row5);
			AssertContains(FormattableString.Invariant($@"[NPX]='8'; [NPR]='6'; [TotalOutturnsReceived]='6'; [TotalOutturnsDelivered]='6'; [ReceiptDetails]='6 PK delivered from [{CusOutTurnTest.rowName3}], []'; [CAC]='CC'; [PiecesReleased]='0'; [ReleaseDetails]='4 pc rel by E"), row5.Replace("dbo.", ""));
		}

		protected override void PrepareTestData()
		{
			IWhsLocation locationBac1;
			IWhsLocation locationBac2;
			IWhsLocation locationCax;
			CusOutTurnTest.CreateWarehouseAreasForTest(Factory, out locationBac1, out locationBac2, out locationCax);

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basicNotReceived_NotInReport = Factory.New<CusMAWB>();
			basicNotReceived_NotInReport.Profile = "CUKAIR98LHRBAC";
			var basicReceivedNotDelivered = Factory.New<CusMAWB>();
			basicReceivedNotDelivered.Profile = "CUKAIR98LHRBAC";
			var basicReceivedAndFullyDelivered = Factory.New<CusMAWB>();
			basicReceivedAndFullyDelivered.Profile = "CUKAIR98LHRBAC";
			basicNotReceived_NotInReport.CM_MAWB = "B1";
			basicNotReceived_NotInReport.CM_ArrivalDate = ZDateTime.BrettsBirthday;
			basicReceivedNotDelivered.CM_MAWB = "B2";
			basicReceivedNotDelivered.CM_ArrivalDate = ZDateTime.BrettsBirthday;
			basicReceivedAndFullyDelivered.CM_ArrivalDate = ZDateTime.BrettsBirthday;
			basicReceivedAndFullyDelivered.CM_MAWB = "B3";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "M1";
			mawb.CM_ArrivalDate = ZDateTime.BrettsBirthday;
			mawb.Profile = "CUKAIR98LHRBAC";
			var hawbNotReceived = mawb.ChildBills.AddNew();
			var hawbReceivedNotDelivered = mawb.ChildBills.AddNew();
			var hawbReceivedAndFullyDelivered_NotInReport = mawb.ChildBills.AddNew();
			var hawbInactive = mawb.ChildBills.AddNew();
			hawbNotReceived.CS_HAWB = "H1";
			hawbReceivedNotDelivered.CS_HAWB = "H2";
			hawbReceivedAndFullyDelivered_NotInReport.CS_HAWB = "H3";
			hawbInactive.CS_HAWB = "H4";

			basicNotReceived_NotInReport.NumberOfPiecesReceived = 0;
			basicReceivedNotDelivered.NumberOfPiecesExpected = 100;
			basicReceivedNotDelivered.NumberOfPiecesReceived = 100;
			basicReceivedAndFullyDelivered.NumberOfPiecesExpected = 1000;
			basicReceivedAndFullyDelivered.NumberOfPiecesReceived = 1000;
			hawbNotReceived.CS_PiecesLanded = 0;
			hawbReceivedNotDelivered.CS_PiecesManifested = 10;
			hawbReceivedNotDelivered.CS_PiecesLanded = 10;
			hawbReceivedAndFullyDelivered_NotInReport.CS_PiecesManifested = 100;
			hawbReceivedAndFullyDelivered_NotInReport.CS_PiecesLanded = 100;

			var hawbWithSplits = mawb.ChildBills.AddNew();
			hawbWithSplits.CS_HAWB = "H4";
			var s1 = hawbWithSplits.Splits.AddNew();
			s1.NumberOfPiecesExpected = 8;
			s1.NumberOfPiecesReceived = 7;
			s1.SplitReference = "01";
			s1.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);

			var ot1 = basicReceivedNotDelivered.OutTurns.AddNew();
			ot1.C5_PackagesOutturned = 60; // Hawb's 100 pieces, less these 60, gives us 40 in report
			ot1.C5_ReceiptOnlyIndicator = false;
			ot1.WarehouseLocationID = locationBac1.PK;
			ot1.C5_MarksAndNumbers = "Marks";
			var ot2 = basicReceivedAndFullyDelivered.OutTurns.AddNew();
			ot2.C5_PackagesOutturned = 600;
			ot2.C5_CargoReceiptDate = ZDateTime.Empty;
			ot2.C5_ReceiptOnlyIndicator = true;
			var ot3 = basicReceivedAndFullyDelivered.OutTurns.AddNew();
			ot3.C5_PackagesOutturned = 400;
			ot3.C5_CargoReceiptDate = ZDateTime.Empty;
			ot3.C5_ReceiptOnlyIndicator = true;

			hawbReceivedNotDelivered.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			hawbReceivedNotDelivered.ReleaseThisNumberOfPieces(3, NumberOfPiecesReleasedHelper.ShedEvent);
			var ot4 = hawbReceivedNotDelivered.OutTurns.AddNew();
			ot4.C5_PackagesOutturned = 6;
			ot4.C5_ReceiptOnlyIndicator = false;
			ot4.C5_CargoReceiptDate = ZDateTime.Empty;
			var ot5 = hawbReceivedAndFullyDelivered_NotInReport.OutTurns.AddNew();
			ot5.C5_PackagesOutturned = 60;
			ot5.C5_ReceiptOnlyIndicator = true;
			ot5.WarehouseLocationID = locationBac2.PK;
			ot5.C5_CargoReceiptDate = ZDateTime.Empty;
			var ot6 = hawbReceivedAndFullyDelivered_NotInReport.OutTurns.AddNew();
			ot6.C5_PackagesOutturned = 40;
			ot6.C5_ReceiptOnlyIndicator = true;
			ot6.C5_CargoReceiptDate = ZDateTime.Empty;

			var ot7 = hawbWithSplits.OutTurns.AddNew();
			ot7.SplitReferenceToWhichThisPertains = "01";
			ot7.WarehouseLocationID = locationCax.PK;
			ot7.C5_ReceiptOnlyIndicator = true;
			ot7.C5_PackagesOutturned = 6;
			Factory.Save(); // calculate NPR
			ot7.C5_CargoReceiptDate = ZDateTime.Empty;
			ot7.IsBeingReleasedNow = true;
			var ertsReleaseHelper = new NonPersistentErtsReleaseOrchestrator(s1);
			ertsReleaseHelper.ErtsReleaseHelper.NumberOfPieces = 4;
			ertsReleaseHelper.ReleaseAndPrintRRAOnFsnOrAwb(fsnMessage: null);

			// An orphaned AU hawb will have CS_IsMasterHouse N and CS_CM null. If we're not careful with our subqueries then this will screw us.  This orhpan will check that the bond check function successfully does:
			//			select * from dbo.cusMawb where CM_PK not in (select CS_CM from dbo.cusMawb where WHATEVER and CS_CM is not null)
			//				instead of the flawed
			//			select * from dbo.cusMawb where CM_PK not in (select CS_CM from dbo.cusMawb where WHATEVER).			
			var orphanedAussieHawb = Factory.New<Customs.Business.CusHAWB>();
			orphanedAussieHawb.CS_ApplicationCode = "CMR";
			orphanedAussieHawb.CS_HAWB = "AU Orphan";

			hawbInactive.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
			hawbInactive.CS_PiecesManifested = 10;
			var ot8 = hawbInactive.OutTurns.AddNew();
			ot8.C5_PackagesOutturned = 6;
			hawbInactive.ReleaseThisNumberOfPieces(3, NumberOfPiecesReleasedHelper.ShedEvent);
			hawbInactive.CS_IsActive = false;
		}
	}
}
