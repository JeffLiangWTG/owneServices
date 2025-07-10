using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			JobDeclaration dec = JobDeclaration.New(Factory);
			CusEntryHeader header = dec.CustomsEntryHeaders.AddNew();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			dec.JE_ClusterKey = 1;

			JobDeclarationFetchStrategy strategy = new JobDeclarationFetchStrategy(dec);

			int count = Factory.ActiveTableFetchHints;
			TableColumn tc1 = new TableColumn("JobDeclaration", JobDeclaration.Schema.JE_EntryStatusDescription);
			TableColumn tc2 = new TableColumn("JobDeclaration", "Branch");
			TableColumn tc3 = new TableColumn("JobDeclaration", JobDeclaration.Schema.ConsolidatedCargoStatusDescription);
			strategy.FetchForView(new TableColumn[] { tc1, tc2 });
			AssertEquals("Added 1 fetch hints", count + 1, Factory.ActiveTableFetchHints);

			count = Factory.ActiveTableFetchHints;
			strategy.FetchForView(new TableColumn[] { tc3 });
			AssertEquals("Added 1 fetch hints", count + 1, Factory.ActiveTableFetchHints);
		}
	}
}
