using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class UrlDeciderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Null ExitReport", () => new UrlDecider(null));

				var header = Factory.New<CusExitHeader>();
				var exitReport = header.CusExitReports.AddNew();
				AssertExceptionThrown<ArgumentNullException>("Null Consignment", () => new UrlDecider(exitReport));
			});
		}

		public void TestGetUrlExitReport()
		{
			var expectedMRN = "AH3RRRRRRNNNNNNNN";
			var expectedUrl = "https://www1.agenciatributaria.gob.es/wlpl/ADEX-JDIT/AesDetalleDecLlegada?wMrn=" + expectedMRN;

			CombineAssertions(() =>
			{
				var header = Factory.New<CusExitHeader>();
				var exitReport = header.CusExitReports.AddNew();
				var consignment = exitReport.Header.CusExitConsignments.AddNew();
				consignment.CXC_MovementReference = expectedMRN;
				exitReport.CER_CXC_Consignment = consignment.PK;

				AssertNullOrEmpty("No url was returned when MessageStatus not ACC", new UrlDecider(exitReport).GetUrlExitReport());

				exitReport.CER_MessageStatus = "ACC";
				AssertNotNullOrEmpty("Prereq: If exitReport has ACC Message Status then it has MRN", exitReport.Consignment.CXC_MovementReference);

				AssertEquals("The correct url has been launched", expectedUrl, new UrlDecider(exitReport).GetUrlExitReport());
			});
		}
	}
}
