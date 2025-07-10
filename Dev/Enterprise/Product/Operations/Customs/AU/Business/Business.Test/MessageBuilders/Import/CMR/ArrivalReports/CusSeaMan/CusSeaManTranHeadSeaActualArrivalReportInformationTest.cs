using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManTranHeadSeaActualArrivalReportInformationTest : TestCaseWithFactory
	{
		public void TestActualArrivalDateTimeUTC()
		{
			Arrival.BA_ArrivalPortATA = new ZDateTime(2005, 5, 30, 10, 38, 0);
			Arrival.BA_RL_NKArrivalPort = "AUADL";
			AssertEquals("ActualArrivalDateTime", new ZDateTime(2005, 5, 30, 1, 8, 0), ReportInfo.ActualArrivalDateTimeUTC);
			Arrival.BA_RL_NKArrivalPort = ZString.Empty;
			AssertEquals("ActualArrivalDateTime", ZDateTime.Empty, ReportInfo.ActualArrivalDateTimeUTC);
		}

		public void TestLastOverseasPortOfDeparture()
		{
			Header.BT_RL_NKPortOfLastForeignPort = "NZAKL";
			AssertEquals("LastOverseasPortOfDeparture", "NZAKL", ReportInfo.LastOverseasPortOfDeparture);
		}

		public void TestPortOfArrival()
		{
			Arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("PortOfArrival", "AUSYD", ReportInfo.PortOfArrival);
		}

		public void TestDischargeCTOID()
		{
			CTOAddress.LocalControlledPremisesID = "12345";
			AssertEquals("DischargeCTOID", "12345", ReportInfo.DischargeCTOID);
		}

		public void TestLloydsNumber()
		{
			Header.BT_VesselName = "ADMIRALENGRACHT";
			AssertEquals("LloydsNumber", "8811924", ReportInfo.LloydsNumber);
		}

		public void TestVoyage()
		{
			Header.BT_VoyageNum = "134";
			AssertEquals("Voyage", "134", ReportInfo.Voyage);
		}

		public void TestBerthCode()
		{
			Arrival.BA_BerthCode = "150";
			AssertEquals("BerthCode", "150", ReportInfo.BerthCode);
		}

		public void TestStevadoreID()
		{
			CTOAddress.Header.PrimaryRegistrationNumber.Number = "54321";
			AssertEquals("StevadoreID", "54321", ReportInfo.StevedoreID);
		}

		CusSeaManArrivalPortSeaActualArrivalReportInformation reportInfo;
		CusSeaManArrivalPortSeaActualArrivalReportInformation ReportInfo
		{
			get
			{
				if (reportInfo == null)
				{
					reportInfo = new CusSeaManArrivalPortSeaActualArrivalReportInformation(Arrival);
				}
				return reportInfo;
			}
		}

		CusSeaManArrivalPort arrival;
		CusSeaManArrivalPort Arrival
		{
			get
			{
				if (arrival == null)
				{
					arrival = Header.Arrivals.AddNew();
				}
				return arrival;
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

		OrgAddress cTOAddress;
		OrgAddress CTOAddress
		{
			get
			{
				if (cTOAddress == null)
				{
					OrgHeader header = Factory.New<OrgHeader>();
					Arrival.BA_OA_CTOAddress = header.Addresses[0].PK;
					cTOAddress = header.Addresses[0];
				}
				return cTOAddress;
			}
		}
	}
}
