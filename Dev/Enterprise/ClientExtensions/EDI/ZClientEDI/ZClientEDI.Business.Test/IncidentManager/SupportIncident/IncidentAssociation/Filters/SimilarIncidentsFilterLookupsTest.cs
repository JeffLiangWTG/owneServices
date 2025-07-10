using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.Test
{
	public class SimilarIncidentsFilterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIncidentStatus()
		{
			var lookup = new SimilarIncidentsFilterLookups(new SimilarIncidentsFilter(Factory.New<SupportIncident>()));
			var incidentStatusList = lookup.IncidentStatus;

			AssertEquals(3, incidentStatusList.Count);
			Assert(incidentStatusList.ContainsCode(SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.All));
			Assert(incidentStatusList.ContainsCode(SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.Closed));
			Assert(incidentStatusList.ContainsCode(SimilarIncidentsFilterLookups.IncidentStatusCodeStrings.Open));
		}
	}
}
