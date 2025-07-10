using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class TranslatableRegistryItemTest : TransactionedTestCase
	{
		public void TestGetMultilingualCaptions()
		{
			var translatableTest = new TranslatableRegistryItemForTest<DummyBusinessObjectCollection, DummyBusinessObjectCollection>(null);
			var factory = new BusinessObjectFactory();
			var value = new DummyBusinessObjectCollection(factory);
			var dummyObject = value.AddNew();
			dummyObject.HumanReadableNameForTest = "TestName";
			foreach (var item in translatableTest.GetMultilingualCaptions(value))
			{
				AssertEquals("TestName", item.ToString());
			}
		}
	}
}
