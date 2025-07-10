using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystemWorkflowDeterminerCollection))]
	class BMSystemWorkflowDeterminerCollectionTest : ActiveBusinessObjectCollectionTestCase<BMSystemWorkflowDeterminerCollection>
	{
		public void TestCollection()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory);
			var system2 = BMSTestHelper.CreateSystem(Factory);

			var determiner1_1 = system1.RelatedWorkflowTypes.AddNew();
			var determiner1_2 = system1.RelatedWorkflowTypes.AddNew();
			var determiner2_1 = system2.RelatedWorkflowTypes.AddNew();
			var determiner2_2 = system2.RelatedWorkflowTypes.AddNew();

			determiner1_1.FSW_WorkflowType = "ORG";
			determiner1_2.FSW_WorkflowType = "INQ";
			determiner2_1.FSW_WorkflowType = "WKI";
			determiner2_2.FSW_WorkflowType = "WKP";

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSystem1 = newFactory.Load<BMSystem>(system1.PK);
			var loadedSystem2 = newFactory.Load<BMSystem>(system2.PK);

			AssertEquals(2, loadedSystem1.RelatedWorkflowTypes.Count);
			AssertEquals(2, loadedSystem2.RelatedWorkflowTypes.Count);

			AssertEquals(loadedSystem1, BMSystem.GetSystemForWorkflowType("ORG", newFactory));
			AssertEquals(loadedSystem1, BMSystem.GetSystemForWorkflowType("INQ", newFactory));
			AssertEquals(loadedSystem2, BMSystem.GetSystemForWorkflowType("WKI", newFactory));
			AssertEquals(loadedSystem2, BMSystem.GetSystemForWorkflowType("WKP", newFactory));
		}

		protected override BMSystemWorkflowDeterminerCollection GetCollectionToTest()
		{
			var system = BMSTestHelper.CreateSystem(Factory);

			return new BMSystemWorkflowDeterminerCollection(system);
		}
	}
}
