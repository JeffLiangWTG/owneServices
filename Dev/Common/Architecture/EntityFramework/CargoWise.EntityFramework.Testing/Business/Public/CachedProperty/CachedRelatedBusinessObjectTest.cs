using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CachedRelatedBusinessObjectTest : TestCaseWithDummy
	{
		public void TestCachedRelatedProperty()
		{
			var dummyObj1 = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var dummyForTest = Factory.New<DummyBusinessObjectForTest>();
			dummyForTest.Z0_Guid = ZGuid.Empty;

			AssertEquals("RelatedZ0_Guid should be null when Z0_Guid is ZGuid.Empty", null, dummyForTest.RelatedZ0_Guid);

			dummyForTest.Z0_Guid = dummyObj1.PK;
			AssertEquals("RelatedZ0_Guid should be dummyObj1 when Z0_Guid is dummyObj1.PK", dummyObj1.PK, dummyForTest.RelatedZ0_Guid.PK);

			dummyObj1.Delete();
			AssertEquals("RelatedZ0_Guid should be null when dummyObj1 was deleted, no matter what Z0_Guid was", null, dummyForTest.RelatedZ0_Guid);
		}
	}
}
