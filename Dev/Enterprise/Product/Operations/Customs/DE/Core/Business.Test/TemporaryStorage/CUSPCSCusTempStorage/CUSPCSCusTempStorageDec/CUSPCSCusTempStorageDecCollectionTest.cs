using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSCusTempStorageDecCollection))]
	class CUSPCSCusTempStorageDecCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var dec1 = Factory.New<CUSPRLCusTempStorageDec>();
			dec1.STH_SJH = header.PK;
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger, dec1.STH_DeclarationType);

			var dec2 = Factory.New<CUSPCSCusTempStorageDec>();
			dec2.STH_SJH = header.PK;
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo, dec2.STH_DeclarationType);

			var collection = new CUSPCSCusTempStorageDecCollection(header);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(dec2.PK, collection[0].PK);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CUSPCSCusTempStorageDec>();

		protected override BusinessObjectCollection GetCollectionToTest() => new CUSPCSCusTempStorageDecCollection(Factory.New<CusTempStorageJobHeader>());
	}
}
