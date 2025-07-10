using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaOutturnStandAloneReportHeaderTest : TestCaseWithFactory
	{
		public void TestEstablishmentID()
		{
			OutturnHeader.C6_OutturningPremiseID = "74291J";
			AssertEquals("74291J", ((ISeaOutturnReportHeaderInformation)ReportHeader).EstablishmentID);
		}

		public void TestLines()
		{
			OutturnHeader.Outturns.AddNew();
			AssertNotNull(((ISeaOutturnReportHeaderInformation)ReportHeader).Lines);
		}

		CusOutturnHeader outturnHeader;
		CusOutturnHeader OutturnHeader => outturnHeader ?? (outturnHeader = Factory.New<CusOutturnHeader>());

		SeaOutturnStandAloneReportHeader reportHeader;
		SeaOutturnStandAloneReportHeader ReportHeader => reportHeader ?? (reportHeader = new SeaOutturnStandAloneReportHeader(OutturnHeader));
	}
}
