using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillArrivalReportInformationTest : TestCaseWithFactory
	{
		public void TestLastOverseasPortOfDeparture()
		{
			var info = new CusSCAOceanBillImpendingArrivalReportInformation(oceanBill);
			AssertEquals("LastOverseasPortOfDeparture", "NZAKL", info.LastOverseasPortOfDeparture);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();

			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
		}

		CusSCAOceanBill oceanBill;
	}
}
