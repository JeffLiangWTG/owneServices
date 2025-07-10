using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotCollection))]
	class CusStorageDocPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return entryHeader.EDocPivotCollection;
		}

		public void TestSetCollectionRelationships()
		{
			var collection = (CusStorageDocPivotCollection)GetCollectionToTest();
			var item = collection.AddNew();
			AssertEquals("CH", item.CSD_ParentTableCode);
			AssertNotNull(item.Parent);
		}

		public void TestMaster()
		{
			var collection = (CusStorageDocPivotCollection)GetCollectionToTest();
			AssertEquals(true, collection.Master is ICusStorageDocPivotParent _);
		}
	}
}
