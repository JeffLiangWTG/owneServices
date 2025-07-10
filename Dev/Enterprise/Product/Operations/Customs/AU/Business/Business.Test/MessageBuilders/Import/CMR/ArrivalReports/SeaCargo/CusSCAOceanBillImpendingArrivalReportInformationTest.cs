using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillImpendingArrivalReportInformationTest : TestCaseWithFactory
	{
		public void TestLastOverseasPortOfDeparture()
		{
			AssertEquals("LastOverseasPortOfDeparture", "NZAKL", Info.LastOverseasPortOfDeparture);
		}

		public void TestVoyage()
		{
			oceanBill.CB_Voyage = "789";
			AssertEquals("Voyage", "789", Info.Voyage);
		}

		public void TestLloyds()
		{
			oceanBill.CB_VesselName = "ADMIRALENGRACHT";
			AssertEquals("LloydsNumber", "8811924", Info.LloydsNumber);
		}

		public void TestSlotChartererIDs()
		{
			AssertEquals("SlotChartererIDs.Length", 0, Info.SlotChartererIDs.Length);
		}

		public void TestPortOfFirstArrival()
		{
			oceanBill.CB_RL_NKPortOfFirstArrival = "AUSYD";
			AssertEquals("PortOfFirstArrival", "AUSYD", Info.PortOfFirstArrival);
		}

		public void TestLines()
		{
			AssertEquals("Lines.Length", 1, Info.Lines.Length);
		}

		public void TestDatabaseLines()
		{
			AssertEquals("DatabaseLines.Length", 0, Info.DatabaseLines.Length);
			Factory.Save();
			AssertEquals("DatabaseLines.Length", 1, Info.DatabaseLines.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();

			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
		}

		CusSCAOceanBill oceanBill;

		ISeaImpendingArrivalReportInformation Info => new CusSCAOceanBillImpendingArrivalReportInformation(oceanBill);
	}
}
