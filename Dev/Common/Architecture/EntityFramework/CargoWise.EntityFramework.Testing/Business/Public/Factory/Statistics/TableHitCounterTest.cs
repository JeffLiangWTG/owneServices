using System.Collections.Generic;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TableHitCounterTest : TestCaseWithDummy
	{
		public void TestIsReportingHitsOnTable_FieldChanged_ShouldNotCauseTestDataLeak()
		{
			TableHitCounter hitCounter = new TableHitCounter();

			ICollection<string> allowedTablesCollection = new List<string>
			{
				Factory.GetRowFactory().GetTable("GlbStaff").TableName
			};
			TableHitCounter.ReportHitsOnAllTablesExceptSpecifiedTables(allowedTablesCollection);

			AssertEquals("Changing the shouldReportHits field to true should not result in thread leaks." +
				"Subsequent table hits from GUI thread should not occur during increaseAccessorCount",
				true, hitCounter.ShouldReportHits);
		}
	}
}
