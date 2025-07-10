using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillImpendingArrivalReportLineInformationTest : TestCaseWithFactory
	{
		public void TestPortOfArrival()
		{
			AssertEquals("LastOverseasPortOfDeparture", "AUSYD", Info.PortOfArrival);
		}

		public void TestStevedoreID()
		{
			oceanBill.StevadoreID = "136";
			AssertEquals("StevadoreID", "136", Info.StevedoreID);
		}

		public void TestDischarge()
		{
			AssertEquals("EstimatedDateOfArrival", true, Info.DischargeIndicator);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();

			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
		}

		CusSCAOceanBill oceanBill;

		IImpendingArrivalReportLineInformation Info => new CusSCAOceanBillImpendingArrivalReportLineInformation(oceanBill);
	}
}
