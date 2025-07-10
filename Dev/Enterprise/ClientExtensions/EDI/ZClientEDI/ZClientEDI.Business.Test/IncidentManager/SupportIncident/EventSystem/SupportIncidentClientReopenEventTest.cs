using System;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentClientReopenEventTest : SupportIncidentCargoWiseReopenEventTest
	{
		public void TestSetAssignee_NoTemplate()
		{
			GlbStaff pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM1";
			pm1.GS_EmailAddress = "pm1@test.com";
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "Module A", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			var incident = GetNewIncidentForTest() as SupportIncident;
			incident.IM_Module = "AAA";

			Factory.Save();

			var incidentEvent = GetNewEventForTest(incident);
			incidentEvent.Trigger();
			AssertEquals("PM1", incident.OverallAssignedToCode);
		}

		protected override IIncidentEvent GetNewEventForTest(IIncidentEventConsumer incident)
		{
			return new SupportIncidentClientReopenEvent((SupportIncident)incident);
		}
	}
}