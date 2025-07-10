using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBActualArrivalReportInformationTest : TestCaseWithFactory
	{
		public void TestEstimatedArrivalDate()
		{
			AssertEquals("EstimatedArrivalDate", new ZDateTime(2005, 1, 24, 12, 09, 55), Info.EstimatedArrivalDate);
		}

		public void TestFlightNo()
		{
			AssertEquals("FlightNo", "QF123", Info.FlightNo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mawb = Factory.New<CusMAWB>();

			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "AUSYD";
			mawb.CM_ArrivalDate = new ZDateTime(2005, 1, 24, 12, 09, 55);
			mawb.CM_FlightNo = "QF123";
		}

		CusMAWB mawb;

		IAirActualArrivalReportInformation Info => new CusMAWBActualArrivalReportInformation(mawb);
	}
}
