using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(NewWorkItemDocManagerInfo))]
	public class NewWorkItemDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<NewWorkItem>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<NewWorkItem>();
		}

		public void TestRelatedObjects()
		{
			var workitem = Factory.NewWithValidTestData<NewWorkItem>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var project = Factory.NewWithValidTestData<EDIProject>();
			workitem.RelatedItems.Add(incident);
			workitem.RelatedItems.Add(project);

			AssertEquals(2, workitem.DocManagerInfo.RelatedObjects.Length);
			AssertEquals(incident.Request, workitem.DocManagerInfo.RelatedObjects[0]);
			AssertEquals(project, workitem.DocManagerInfo.RelatedObjects[1]);
		}
	}
}
