namespace Enterprise.BufferManagement.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using Enterprise.Registry.Business;
	using Enterprise.Registry.Business.Testing;
	using Enterprise.ZArchitecture.Core;
	using NUnit.Framework;

	[TestedType(typeof(WorkflowCategoryCollection))]
	class WorkflowCategoryCollectionTest : RegistryBusinessObjectCollectionTestCase<WorkflowCategoryCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		public void TestWorkflowCategoryCollectionClone()
		{
			IRegistryBusiness clone = Collection.Clone(Collection.CurrentFallbackLevel, Collection.Factory);
			AssertNotNull(clone);
			AssertEquals(typeof(WorkflowCategoryCollection), clone.GetType());
		}

		public void TestAddNewAndIndexer()
		{
			var taskType = Collection.AddNew();
			AssertNotNull(taskType);
			AssertEquals(taskType, Collection[0]);
		}

		public void TestICodeDescriptionPairListMembers()
		{
			var collection = new WorkflowCategoryCollection();
			var iterationReason1 = collection.AddNew();
			iterationReason1.Code = "001";
			iterationReason1.Description = (NoResString)"One";

			var iterationReason2 = collection.AddNew();
			iterationReason2.Code = "002";
			iterationReason2.Description = (NoResString)"Two";

			var pairList = (ICodeDescriptionPairList)collection;
			Assert(pairList.ContainsCode("001"));
			AssertEquals("One", pairList.GetDescriptionFromCode("001"));

			Assert(pairList.ContainsCode("002"));
			AssertEquals("Two", pairList.GetDescriptionFromCode("002"));

			Assert(!pairList.ContainsCode("003"));
			AssertEquals("", pairList.GetDescriptionFromCode("003"));
		}

		protected override WorkflowCategoryCollection GetCollectionToTest() => new WorkflowCategoryCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new WorkflowCategory();
	}
}

