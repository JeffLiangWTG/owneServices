using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class FindTriageFilterHelperLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			AssertEquals(triage.Lookups.Types, Lookup.Types);
		}
		
		public void TestProductList()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			AssertEquals(triage.Lookups.ProductList, Lookup.ProductList);
		}

		public void TestProductAreaList()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			AssertEquals(triage.Lookups.ProductAreaList, Lookup.ProductAreaList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Helper = new FindTriageFilterHelper(incident);
			Lookup = new FindTriageFilterHelperLookups(Helper);
		}

		FindTriageFilterHelper Helper;
		FindTriageFilterHelperLookups Lookup;
	}
}
