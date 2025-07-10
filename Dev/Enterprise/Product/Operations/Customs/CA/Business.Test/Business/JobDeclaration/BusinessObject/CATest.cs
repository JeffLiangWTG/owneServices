using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business
{
	sealed class CATest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			JobDeclaration dec = Factory.New<JobDeclaration>();
			CusEntryHeader header1 = dec.CustomsEntryHeaders.AddNew();
			JobDeclarationFetchStrategy strategy = new JobDeclarationFetchStrategy(dec);
			int count = Factory.ActiveTableFetchHints;
			TableColumn tc1 = new TableColumn("JobDeclaration", JobDeclaration.Schema.DeclarationNumber);
			TableColumn tc2 = new TableColumn("JobDeclaration", "Branch");
			TableColumn tc3 = new TableColumn("JobDeclaration", JobDeclaration.Schema.JE_MessageStatusDescription);
			strategy.FetchForView(new TableColumn[] { tc1, tc2 });
			AssertEquals("Added 3 fetch hint", count + 4, Factory.ActiveTableFetchHints);
			count = Factory.ActiveTableFetchHints;
			strategy.FetchForView(new TableColumn[] { tc3 });
			AssertEquals("Added 1 fetch hints", count + 1, Factory.ActiveTableFetchHints);
		}
	}
}
