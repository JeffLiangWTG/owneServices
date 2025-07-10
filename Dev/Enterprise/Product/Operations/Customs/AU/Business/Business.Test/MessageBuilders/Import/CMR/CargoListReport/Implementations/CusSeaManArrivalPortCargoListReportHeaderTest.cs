using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManArrivalPortCargoListReportHeaderTest : TestCaseWithFactory
	{
		public void TestCargoResponsiblePartyID()
		{
			const string ABN = "63004280871";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = ABN;
			AssertEquals("CargoResponsiblePartyID", ABN, ReportHeader.CargoResponsiblePartyID);
		}

		public void TestLloydsNumber()
		{
			ArrivalPort.Header.BT_VesselName = "ADMIRALENGRACHT";
			AssertEquals("LloydsNumber", "8811924", ReportHeader.LloydsNumber);
		}

		public void TestVoyageNumber()
		{
			ArrivalPort.Header.BT_VoyageNum = "111";
			AssertEquals("VoyageNumber", "111", ReportHeader.VoyageNumber);
		}

		public void TestDischargePort()
		{
			ArrivalPort.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("DischargePort", "AUSYD", ReportHeader.DischargePort);
		}

		public void TestLines()
		{
			AssertEquals("Length", 0, ReportHeader.Lines.Length);
			ArrivalPort.CargoLines.AddNew();
			ArrivalPort.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("Length", 1, ReportHeader.Lines.Length);
		}

		public void TestDatabaseLines()
		{
			ArrivalPort.Header.OceanBills.AddNew().Details.AddNew();
			ArrivalPort.BA_RL_NKArrivalPort = "AUSYD";
			ArrivalPort.CargoLines.AddNew();
			AssertEquals("Length", 0, ReportHeader.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("Length", 1, ReportHeader.DatabaseLines.Length);
		}

		CusSeaManArrivalPortCargoListReportHeader ReportHeader => new CusSeaManArrivalPortCargoListReportHeader(ArrivalPort);

		CusSeaManArrivalPort arrivalPort;
		CusSeaManArrivalPort ArrivalPort
		{
			get
			{
				if (arrivalPort == null)
				{
					CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
					arrivalPort = transportHeader.Arrivals.AddNew();
				}
				return arrivalPort;
			}
		}
	}
}
