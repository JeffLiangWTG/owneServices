using CargoWise.EntityFramework;
using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TaskTypeRestrictionsCollection))]
	sealed class TaskTypeRestrictionsCollectionTest : RegistryBusinessObjectCollectionTestCase<TaskTypeRestrictionsCollection>
	{
		public void TestICodeDescriptionPairListMembers()
		{
			WorkflowDataRegistryTestHelper.SetupTaskTypesRegistry();

			var collection = new TaskTypeRestrictionsCollection();
			var taskTypes1 = collection.AddNew();
			taskTypes1.WorkflowType = "WKI";
			taskTypes1.TaskType = "INV";

			var taskTypes2 = collection.AddNew();
			taskTypes2.WorkflowType = "WKI";
			taskTypes2.TaskType = "COD";

			var pairList = (ICodeDescriptionPairList)collection;
			Assert(pairList.ContainsCode("INV"));
			AssertEquals("Investigation", pairList.GetDescriptionFromCode("INV"));

			Assert(pairList.ContainsCode("COD"));
			AssertEquals("Coding", pairList.GetDescriptionFromCode("COD"));

			Assert(!pairList.ContainsCode("003"));
			AssertEquals("", pairList.GetDescriptionFromCode("003"));
		}

		public void TestContainsActiveDifRestrictions()
		{
			var collection = new TaskTypeRestrictionsCollection();
			Assert("Collection should not contain DIF restrictions", !collection.ContainsActiveDifRestrictions);

			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "WKI";
			restriction.TaskType = "CBC";
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			restriction.Scope = ScopeList.Codes.Workflow;
			restriction.RestrictionType = RestrictionTypeList.Codes.DifferentResource;
			Assert("Collection should contain active DIF restrictions", collection.ContainsActiveDifRestrictions);

			restriction.Active = false;
			Assert("Collection should not contain active DIF restrictions", !collection.ContainsActiveDifRestrictions);
		}

		public override void TestGetCodeDescriptionPairList()
		{
			AssertEquals("GetCodeDescriptionPairList().Count", 0, Collection.GetCodeDescriptionPairList().Count);

			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();

			WorkflowDataRegistryTestHelper.SetupTaskTypesRegistry();
			element1.WorkflowType = "WKI";
			element1.TaskType = "INV";

			element2.WorkflowType = "WKI";
			element2.TaskType = "SHV";

			var list = Collection.GetCodeDescriptionPairList();
			AssertEquals("GetCodeDescriptionPairList().Count", 2, list.Count);
			AssertEquals("GetCodeDescriptionPairList()[0].Code", "WKI:INV:JOB", list[0].Code);
			AssertEquals("GetCodeDescriptionPairList()[0].Description", "Investigation", list[0].Description);

			AssertEquals("GetCodeDescriptionPairList()[1].Code", "WKI:SHV:JOB", list[1].Code);
			AssertEquals("GetCodeDescriptionPairList()[1].Description", "Shelf?", list[1].Description);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override TaskTypeRestrictionsCollection GetCollectionToTest()
		{
			return new TaskTypeRestrictionsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TaskTypeRestrictions();
		}
	}
}
