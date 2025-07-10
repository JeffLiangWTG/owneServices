using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManArrivalPortImpendingArrivalReportLineInformationTest : TestCaseWithFactory
	{
		public void TestEstimatedDateTimeOfArrivalUTC()
		{
			Arrival.BA_ArrivalPortETA = new ZDateTime(2005, 5, 30, 10, 38, 0);
			Arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("ActualArrivalDateTime", new ZDateTime(2005, 5, 30, 0, 38, 0), ReportInfo.EstimatedDateTimeOfArrivalUTC);
			Arrival.BA_RL_NKArrivalPort = ZString.Empty;
			AssertEquals("ActualArrivalDateTime", ZDateTime.Empty, ReportInfo.EstimatedDateTimeOfArrivalUTC);
		}

		public void TestMessages()
		{
			AssertEquals("Messages", Arrival.Messages, ReportInfo.Messages);
		}

		public void TestPortOfArrival()
		{
			Arrival.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("PortOfArrival", "AUSYD", ReportInfo.PortOfArrival);
		}

		public void TestDischargeCTOEstablishmentID()
		{
			CTOAddress.LocalControlledPremisesID = "12345";
			AssertEquals("DischargeCTOID", "12345", ReportInfo.DischargeCTOEstablishmentID);
		}

		public void TestStevadoreID()
		{
			CTOAddress.Header.PrimaryRegistrationNumber.Number = "54321";
			AssertEquals("StevadoreID", "54321", ReportInfo.StevedoreID);
		}

		public void TestDischargeIndicator()
		{
			Arrival.BA_DischargeIndicator = false;
			AssertEquals("DischargeIndicator", false, ReportInfo.DischargeIndicator);

			Arrival.BA_DischargeIndicator = true;
			AssertEquals("DischargeIndicator", true, ReportInfo.DischargeIndicator);
		}

		CusSeaManArrivalPortImpendingArrivalReportLineInformation reportInfo;
		CusSeaManArrivalPortImpendingArrivalReportLineInformation ReportInfo
		{
			get
			{
				if (reportInfo == null)
				{
					reportInfo = new CusSeaManArrivalPortImpendingArrivalReportLineInformation(Arrival);
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
					var header = Factory.New<CusSeaManTranHead>();
					arrival = header.Arrivals.AddNew();
				}
				return arrival;
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
