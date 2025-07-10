using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class IBusinessObjectCollectionExtensionsTest : TestCaseWithFactory
	{
		public void TestToArray()
		{
			ActiveBusinessObjectCollection<DummyBusinessObject> collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			collection.AddNew();
			collection.AddNew();
			AssertEquals(2, collection.ToArray<DummyBusinessObject>().Length);
			collection = null;
			AssertEquals(0, collection.ToArray<DummyBusinessObject>().Length);
		}

		public void TestGetFieldValues()
		{
			ActiveBusinessObjectCollection<DummyBusinessObject> collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			DummyBusinessObject bizObj1 = collection.AddNew();
			bizObj1.Z0_NVarCharMax = "CODE1";
			bizObj1.Z0_Number = 1;
			DummyBusinessObject bizObj2 = collection.AddNew();
			bizObj2.Z0_NVarCharMax = "CODE2";
			bizObj2.Z0_Number = 3;
			DummyBusinessObject bizObj3 = collection.AddNew();
			bizObj3.Z0_NVarCharMax = "CODE3";
			bizObj3.Z0_Number = 2;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CODE1", "CODE2", "CODE3" }, collection.GetFieldValues(DummyBusinessObject.Schema.Z0_NVarCharMax));
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 3, 2 }, collection.GetFieldValues(DummyBizoSchema.Z0_Number));
		}
	}
}
