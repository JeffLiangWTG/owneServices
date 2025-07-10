using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillActualArrivalReportInformationTest : TestCaseWithFactory
	{
		public void TestBerthCode()
		{
			OceanBill.BerthCode = "123";
			AssertEquals("BerthCode", "123", ReportInfo.BerthCode);
		}

		public void TestDischargeCTOID()
		{
			OceanBill.DischargeCTOID = "345";
			AssertEquals("DischargeCTOID", "345", ReportInfo.DischargeCTOID);
		}

		public void TestVoyage()
		{
			OceanBill.CB_Voyage = "789";
			AssertEquals("Voyage", "789", ReportInfo.Voyage);
		}

		public void TestLloyds()
		{
			OceanBill.CB_VesselName = "ADMIRALENGRACHT";
			AssertEquals("LloydsNumber", "8811924", ReportInfo.LloydsNumber);
		}

		public void TestStevadoreID()
		{
			OceanBill.StevadoreID = "136";
			AssertEquals("StevadoreID", "136", ReportInfo.StevedoreID);
		}

		CusSCAOceanBill oceanBill;
		CusSCAOceanBill OceanBill
		{
			get
			{
				if (oceanBill == null)
				{
					oceanBill = Factory.New<CusSCAOceanBill>();
				}
				return oceanBill;
			}
		}

		CusSCAOceanBillActualArrivalReportInformation reportInfo;
		CusSCAOceanBillActualArrivalReportInformation ReportInfo
		{
			get
			{
				if (reportInfo == null)
				{
					reportInfo = new CusSCAOceanBillActualArrivalReportInformation(OceanBill);
				}
				return reportInfo;
			}
		}
	}
}
