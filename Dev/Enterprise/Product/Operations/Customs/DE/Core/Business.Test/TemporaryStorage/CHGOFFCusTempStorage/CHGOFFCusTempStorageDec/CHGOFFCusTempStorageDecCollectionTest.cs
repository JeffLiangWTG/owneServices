using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGOFFCusTempStorageDecCollection))]
	class CHGOFFCusTempStorageDecCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCHGOFFCusTempStorageDecCollectionAdditionalFilter()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			header.CHGTSTCusTempStorageDecs.AddNew();
			header.CHGOFFCusTempStorageDecs.AddNew();
			header.CHGOFFCusTempStorageDecs.AddNew();

			var chgoffCollection = new CHGOFFCusTempStorageDecCollection(header);
			chgoffCollection.Load();
			AssertEquals(2, chgoffCollection.Count);
			var chgoffStorageDec = Factory.New<CHGOFFCusTempStorageDec>();
			chgoffStorageDec.STH_SJH = header.PK;
			chgoffCollection.Reload(false);
			AssertEquals(3, chgoffCollection.Count);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CHGOFFCusTempStorageDec>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			return storageHeader.CHGOFFCusTempStorageDecs;
		}
	}
}
