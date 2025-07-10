using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(IncidentManagementGroupActivityCollection))]
	public class IncidentManagementGroupActivityCollectionTest : ActiveBusinessObjectCollectionTestCase<IncidentManagementGroupActivityCollection>
	{
		protected override IncidentManagementGroupActivityCollection GetCollectionToTest()
		{
			var incident = Factory.New<SupportIncident>();
			return new IncidentManagementGroupActivityCollection(incident, false, true);
		}
	}
}
