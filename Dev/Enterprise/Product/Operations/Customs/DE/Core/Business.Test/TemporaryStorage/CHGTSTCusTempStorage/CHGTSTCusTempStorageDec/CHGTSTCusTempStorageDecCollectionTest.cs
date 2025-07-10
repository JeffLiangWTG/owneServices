using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGTSTCusTempStorageDecCollection))]
	class CHGTSTCusTempStorageDecCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCHGTSTCusTempStorageDecCollectionAdditionalFilter()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			header.CHGTSTCusTempStorageDecs.AddNew();
			header.CHGTSTCusTempStorageDecs.AddNew();
			var cusprlDec = header.CUSPRLCusTempStorageDec;

			var chgtstsCollection = new CHGTSTCusTempStorageDecCollection(header);
			chgtstsCollection.Load();
			AssertEquals(2, chgtstsCollection.Count);
			var chgtstStorageDec = Factory.New<CHGTSTCusTempStorageDec>();
			chgtstStorageDec.STH_SJH = header.PK;
			chgtstsCollection.Reload(false);
			AssertEquals(3, chgtstsCollection.Count);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CHGTSTCusTempStorageDec>();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusTempStorageJobHeader storageHeader = Factory.New<CusTempStorageJobHeader>();
			return storageHeader.CHGTSTCusTempStorageDecs;
		}
	}
}
