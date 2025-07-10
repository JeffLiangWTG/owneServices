using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBImpendingArrivalReportInformationTest : TestCaseWithFactory
	{
		public void TestLastOverseasPortOfDeparture()
		{
			AssertEquals("LastOverseasPortOfDeparture", "NZAKL", Info.LastOverseasPortOfDeparture);
		}

		public void TestFlightNo()
		{
			AssertEquals("FlightNo", "QF123", Info.FlightNo);
		}

		public void TestPortOfFirstArrival()
		{
			mawb.CM_RL_NKFirstArrivalPort = "AUSYD";
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
			mawb = Factory.New<CusMAWB>();

			mawb.CM_RL_NKLoadPort = "NZAKL";
			mawb.CM_RL_NKDischargePort = "AUSYD";
			mawb.CM_ArrivalDate = new ZDateTime(2005, 1, 24, 12, 09, 55);
			mawb.CM_FlightNo = "QF123";
		}

		CusMAWB mawb;

		IAirImpendingArrivalReportInformation Info => new CusMAWBImpendingArrivalReportInformation(mawb);
	}
}
