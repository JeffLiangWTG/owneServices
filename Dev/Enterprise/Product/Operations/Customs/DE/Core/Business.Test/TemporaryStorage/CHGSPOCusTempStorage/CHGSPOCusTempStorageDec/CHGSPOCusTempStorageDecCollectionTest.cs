using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGSPOCusTempStorageDecCollection))]
	class CHGSPOCusTempStorageDecCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAddNew()
		{
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
			var dec2 = storageHeader.CHGSPOCusTempStorageDecs.AddNew();
			dec2.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
			AssertEquals(2, storageHeader.CHGSPOCusTempStorageDecs.Count);
			Assert(storageHeader.CHGSPOCusTempStorageDecs.Contains(storageDec));
			Assert(storageHeader.CHGSPOCusTempStorageDecs.Contains(dec2));
		}

		public void TestAllowAddNewAndRemoveCore()
		{
			AssertEquals(true, GetCollectionToTest().AllowNew);
			AssertEquals(true, GetCollectionToTest().AllowRemove);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var storageDec = Factory.New<CHGSPOCusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
			return storageDec;
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CHGSPOCusTempStorageDecCollection(Factory.New<CusTempStorageJobHeader>());

		protected override void SetUp()
		{
			base.SetUp();
			storageHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			storageDec = storageHeader.CHGSPOCusTempStorageDecs.AddNew();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
		}
		CusTempStorageJobHeader storageHeader;
		CHGSPOCusTempStorageDec storageDec;
	}
}
