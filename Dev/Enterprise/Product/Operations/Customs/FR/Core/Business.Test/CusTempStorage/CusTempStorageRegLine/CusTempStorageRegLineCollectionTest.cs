using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegLineCollection))]
	class CusTempStorageRegLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineCollection>
	{
		public void TestOnLoadedIntoCollection()
		{
			var collection = new CusTempStorageRegLineCollection(Factory.New<CusTempStorageRegHeader>());
			collection.OnLoadedIntoCollection = l => l.SRL_GoodsDescription = "Set by OnLoadedIntoCollection";
			var line = collection.AddNew();
			AssertEquals("OnLoadedIntoCollection triggered", "Set by OnLoadedIntoCollection", line.SRL_GoodsDescription);
		}

		protected override CusTempStorageRegLineCollection GetCollectionToTest() => new CusTempStorageRegLineCollection(Factory.New<CusTempStorageRegHeader>());
	}
}
