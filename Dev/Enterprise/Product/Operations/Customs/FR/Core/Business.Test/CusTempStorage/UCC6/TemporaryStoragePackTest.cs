using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePack))]
	sealed class TemporaryStoragePackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetPackedItemType()
		{
			var pack = (TemporaryStoragePack)GetNewBusinessObject();
			AssertEquals("Should get TemporaryStoragePackedItem type.", typeof(TemporaryStoragePackedItem), pack.GetPackedItemType());
		}
	}
}
