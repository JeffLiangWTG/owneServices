using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class RegistryBusinessObjectTemplateTest : TestCase
	{
		public void TestGetParentCollection()
		{
			DummyRegistryBusinessObjectCollection collection = new DummyRegistryBusinessObjectCollection();
			DummyRegistryBusinessObject dummy = collection.AddNew();
			AssertEquals("Parent Collection", collection, dummy.GetParentCollection(dummy, typeof(DummyRegistryBusinessObjectCollection)));
			collection = new DummyRegistryBusinessObjectCollectionChild();
			dummy = collection.AddNew();
			AssertEquals("Parent Collection", collection, dummy.GetParentCollection(dummy, typeof(DummyRegistryBusinessObjectCollection)));
		}

		public void TestClone()
		{
			RegistryBusinessObjectTemplateForTest dummy = new RegistryBusinessObjectTemplateForTest();
			RegistryBusinessObjectTemplateForTest clonedDummy = (RegistryBusinessObjectTemplateForTest)dummy.Clone(null, null);
			AssertEquals("Validation was suspended while cloning, should be false", false, clonedDummy.ValidateTestStringCalled);
			AssertEquals("Should be resumed", false, clonedDummy.IsValidationSuspended);

			dummy.SuspendValidationOnNewObjectForCloning = true;
			clonedDummy = (RegistryBusinessObjectTemplateForTest)dummy.Clone(null, null);
			AssertEquals("Validation was suspended while cloning, should be false", false, clonedDummy.ValidateTestStringCalled);
			AssertEquals("Should not be resumed. Validation was suspended to begin with", true, clonedDummy.IsValidationSuspended);
		}
	}
}
