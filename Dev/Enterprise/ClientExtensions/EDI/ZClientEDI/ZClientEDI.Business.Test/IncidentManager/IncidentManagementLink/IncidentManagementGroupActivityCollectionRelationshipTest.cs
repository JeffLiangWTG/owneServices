using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentManagementGroupActivityCollectionRelationshipTest : TestCaseWithFactory
	{
		public void TestGetRelatedActivitiesQuery()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();

			var link = Factory.New<IncidentManagementLink>();

			link.INL_ING_Group = incidentGroup.PK;
			link.INL_IM_Incident = Incident.PK;
			Factory.Save();

			var query = IncidentManagementGroupActivityCollectionRelationship.GetRelatedActivitiesQuery(Incident, true, true);
			var queryResult = Factory.Load<IncidentManagementLink>(query);
			AssertEquals("Error : IncidentManagementGroup is loaded as child", 0, queryResult.Length);

			var query1 = IncidentManagementGroupActivityCollectionRelationship.GetRelatedActivitiesQuery(Incident, false, true);

			var query1Result = Factory.Load<IncidentManagementLink>(query1);

			AssertEquals(1, query1Result.Length);
		}

		SupportIncident Incident
		{
			get
			{
				if (incident == null)
				{
					incident = Factory.NewWithValidTestData<SupportIncident>();
				}
				return incident;
			}
		}
		SupportIncident incident;
	}
}
