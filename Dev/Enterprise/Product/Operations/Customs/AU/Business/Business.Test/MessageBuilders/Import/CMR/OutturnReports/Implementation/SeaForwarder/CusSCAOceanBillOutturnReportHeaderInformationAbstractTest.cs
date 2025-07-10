namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusSCAOceanBillOutturnReportHeaderInformationAbstractTest : CusUnderbondOutturnReportHeaderInformationAbstractTest
	{
		public void TestVoyageNumber()
		{
			OceanBill.CB_Voyage = "12345";
			AssertEquals("VoyageNumber", "12345", HeaderInfo.VoyageNumber);
		}

		public void TestVesselID()
		{
			OceanBill.CB_VesselName = "ADMIRALENGRACHT";
			AssertEquals("VoyageNumber", "8811924", HeaderInfo.VesselID);
		}

		CusSCAOceanBillOutturnReportHeaderInformation HeaderInfo => (CusSCAOceanBillOutturnReportHeaderInformation)GetHeaderInfo();

		CusSCAOceanBill oceanBill;
		protected CusSCAOceanBill OceanBill => oceanBill ?? (oceanBill = Factory.New<CusSCAOceanBill>());
	}
}
