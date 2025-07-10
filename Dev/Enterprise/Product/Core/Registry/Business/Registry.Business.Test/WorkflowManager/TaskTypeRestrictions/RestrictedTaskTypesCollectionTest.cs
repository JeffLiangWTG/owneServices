using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RestrictedTaskTypesCollection))]
	sealed class RestrictedTaskTypesCollectionTest : RegistryBusinessObjectCollectionTestCase<RestrictedTaskTypesCollection>
	{
		public void TestTaskTypesCollectionClone()
		{
			var clone = Collection.Clone(Collection.CurrentFallbackLevel, Collection.Factory);
			AssertNotNull(clone);
			AssertEquals(typeof(RestrictedTaskTypesCollection), clone.GetType());
		}

		public void TestAddNewAndIndexer()
		{
			var taskType = Collection.AddNew();
			AssertNotNull(taskType);
			AssertEquals(taskType, Collection[0]);
		}

		public override void TestGetCodeDescriptionPairList()
		{
			AssertEquals("GetCodeDescriptionPairList().Count", 0, Collection.GetCodeDescriptionPairList().Count);

			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();

			WorkflowDataRegistryTestHelper.SetupTaskTypesRegistry();

			element1.WorkflowType = "WKI";
			element1.Code = "SHV";

			element2.WorkflowType = "WKI";
			element2.Code = "CRF";

			var list = Collection.GetCodeDescriptionPairList();
			AssertEquals("GetCodeDescriptionPairList().Count", 2, list.Count);
			AssertEquals("GetCodeDescriptionPairList()[0].Code", "SHV", list[0].Code);
			AssertEquals("GetCodeDescriptionPairList()[0].Description", "Shelf?", list[0].Description);

			AssertEquals("GetCodeDescriptionPairList()[1].Code", "CRF", list[1].Code);
			AssertEquals("GetCodeDescriptionPairList()[1].Description", "Code Review", list[1].Description);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RestrictedTaskTypesCollection GetCollectionToTest()
		{
			return new RestrictedTaskTypesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RestrictedTaskTypes();
		}
	}
}
