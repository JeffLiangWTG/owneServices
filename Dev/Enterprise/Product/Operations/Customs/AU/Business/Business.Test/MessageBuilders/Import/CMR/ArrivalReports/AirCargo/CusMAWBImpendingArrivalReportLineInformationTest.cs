using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBImpendingArrivalReportLineInformationTest : TestCaseWithFactory
	{
		public void TestPortOfArrival()
		{
			AssertEquals("LastOverseasPortOfDeparture", "AUSYD", Info.PortOfArrival);
		}

		public void TestDischarge()
		{
			AssertEquals("EstimatedDateOfArrival", true, Info.DischargeIndicator);
		}

		public void TestStevedoreID()
		{
			AssertEquals("EstimatedDateOfArrival", ZString.Empty, Info.StevedoreID);
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

		CusMAWBImpendingArrivalReportLineInformation Info => new CusMAWBImpendingArrivalReportLineInformation(mawb);
	}
}
