using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class CusExitReportFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var report = cusExitReport;
			var fetchStrategy = new CusExitReportFetchStrategy(report);

			var fetchHintsBeforeFetchForView = Factory.ActiveTableFetchHints;

			fetchStrategy.FetchForView(System.Array.Empty<TableColumn>());

			AssertEquals("Added 2 fetch hints", 2, Factory.ActiveTableFetchHints - fetchHintsBeforeFetchForView);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory) => cusExitReport.Header.CusExitReports;

		CusExitReport cusExitReport;
		protected override void SetUp()
		{
			base.SetUp();
			cusExitReport = CusExitReportTest.GetNewBusinessObject(Factory).report;
		}
	}
}
