using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using CusTempStorageDecCollection = Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDecCollection<Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageDec, Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader>;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageDecCollectionView))]
	class CusTempStorageDecCollectionViewTest : BusinessObjectCollectionViewTestCase<CusTempStorageDecCollectionView>
	{
		protected override CusTempStorageDecCollectionView GetCollectionToTest()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDecCollection = new CusTempStorageDecCollection(storageHeader);
			return new CusTempStorageDecCollectionView(storageDecCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var storageDec = Factory.New<CUSPRLCusTempStorageDec>();
			storageDec.ReferenceNumber = "TEST1234";
			return storageDec;
		}

		public void TestCollectionFiltering()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			AssertEquals(0, storageHeader.CusTempStorageDecsWithValidData.Count);
			storageDec.ReferenceNumber = "TEST12345";
			AssertEquals(1, storageHeader.CusTempStorageDecsWithValidData.Count);
			Assert(storageHeader.CusTempStorageDecsWithValidData.Contains(storageDec));
		}
	}
}
