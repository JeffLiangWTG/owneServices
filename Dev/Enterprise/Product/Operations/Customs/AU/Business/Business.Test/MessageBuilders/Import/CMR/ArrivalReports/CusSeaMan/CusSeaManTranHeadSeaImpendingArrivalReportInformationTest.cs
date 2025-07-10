using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManTranHeadSeaImpendingArrivalReportInformationTest : TestCaseWithFactory
	{
		public void TestLastOverseasPortOfDeparture()
		{
			Header.BT_RL_NKPortOfLastForeignPort = "NZAKL";
			AssertEquals("LastOverseasPortOfDeparture", "NZAKL", ReportInfo.LastOverseasPortOfDeparture);
		}

		public void TestLloydsNumber()
		{
			Header.BT_VesselName = "ADMIRALENGRACHT";
			AssertEquals("LloydsNumber", "8811924", ReportInfo.LloydsNumber);
		}

		public void TestVoyage()
		{
			Header.BT_VoyageNum = "123";
			AssertEquals("Voyage", "123", ReportInfo.Voyage);
		}

		public void TestDateTimeOfDeparture()
		{
			Header.BT_PortOfLastForeignPortATD = new ZDateTime(2005, 5, 30, 6, 51, 0);
			Header.BT_RL_NKPortOfLastForeignPort = ZString.Empty;
			AssertEquals("DateTimeOfDeparture", ZDateTime.Empty, ReportInfo.DateTimeOfDepartureUTC);
			Header.BT_RL_NKPortOfLastForeignPort = "USLAX";
			AssertEquals("DateTimeOfDeparture", new ZDateTime(2005, 5, 30, 13, 51, 0), ReportInfo.DateTimeOfDepartureUTC);

			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			port.RL_R3 = ZGuid.Empty;
			Header.BT_RL_NKPortOfLastForeignPort = port.RL_Code;
			Header.BT_PortOfLastForeignPortATD = new ZDateTime(2021, 8, 9, 9, 0, 0);

			AssertEquals("Should return the original date when there is no valid time zone on the last foreign port.", Header.BT_PortOfLastForeignPortATD, ReportInfo.DateTimeOfDepartureUTC);
		}

		public void TestPortOfFirstArrival()
		{
			var arrival = Header.Arrivals.AddNew();
			arrival.BA_RL_NKArrivalPort = "AUSYD";
			arrival.BA_IsFirstArrival = true;
			AssertEquals("PortOfFirstArrival", "AUSYD", ReportInfo.PortOfFirstArrival);
		}

		public void TestLines()
		{
			Header.Arrivals.AddNew();
			AssertEquals("Lines.Length", 1, ReportInfo.Lines.Length);
		}

		public void TestDatabaseLines()
		{
			Header.Arrivals.AddNew();
			AssertEquals("DatabaseLines.Length", 0, ReportInfo.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("DatabaseLines.Length", 1, ReportInfo.DatabaseLines.Length);
			Header.Arrivals.AddNew();
			AssertEquals("DatabaseLines.Length", 1, ReportInfo.DatabaseLines.Length);
			AssertEquals("DatabaseLines.Length", 2, ReportInfo.Lines.Length);
		}

		public void TestSlotChartererIDs()
		{
			AssertEquals("SlotChartererIDs.Length", 0, ReportInfo.SlotChartererIDs.Length);
			var slotPivot = Header.SlotCharterers.AddNew();
			OrgHeader slotCharterer = Factory.New<OrgHeader>();
			slotPivot.BS_OH_SlotCharterer = slotCharterer.PK;

			AssertEquals("SlotChartererIDs.Length", 0, ReportInfo.SlotChartererIDs.Length);
			slotCharterer.PrimaryRegistrationNumber.Number = "123";
			AssertEquals("SlotChartererIDs.Length", 1, ReportInfo.SlotChartererIDs.Length);
		}

		CusSeaManTranHeadSeaImpendingArrivalReportInformation reportInfo;
		CusSeaManTranHeadSeaImpendingArrivalReportInformation ReportInfo
		{
			get
			{
				if (reportInfo == null)
				{
					reportInfo = new CusSeaManTranHeadSeaImpendingArrivalReportInformation(Header);
				}
				return reportInfo;
			}
		}

		CusSeaManTranHead header;
		CusSeaManTranHead Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<CusSeaManTranHead>();
				}
				return header;
			}
		}
	}
}
