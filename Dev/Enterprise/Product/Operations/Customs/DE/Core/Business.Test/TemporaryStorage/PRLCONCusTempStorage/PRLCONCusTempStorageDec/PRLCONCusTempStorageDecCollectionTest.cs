using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(PRLCONCusTempStorageDecCollection))]
	public class PRLCONCusTempStorageDecCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			var header = Factory.New<CusTempStorageJobHeader>();
			var dec1 = Factory.New<CUSPRLCusTempStorageDec>();
			dec1.STH_SJH = header.PK;
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger, dec1.STH_DeclarationType);

			var dec2 = Factory.New<PRLCONCusTempStorageDec>();
			dec2.STH_SJH = header.PK;
			AssertEquals(TemporaryStorageDeclarationTypeList.Codes.PresentationLedgerConsolidation, dec2.STH_DeclarationType);

			var collection = new PRLCONCusTempStorageDecCollection(header);
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(dec2.PK, collection[0].PK);
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			Assert("AllowNew is true", collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			Assert("AllowRemove is true", collection.AllowRemove);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<PRLCONCusTempStorageDec>();
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new PRLCONCusTempStorageDecCollection(Factory.New<CusTempStorageJobHeader>());
	}
}
