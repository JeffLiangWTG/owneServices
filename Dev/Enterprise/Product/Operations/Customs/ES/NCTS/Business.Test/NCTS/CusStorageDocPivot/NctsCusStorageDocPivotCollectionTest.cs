using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCusStorageDocPivotCollection))]
	class NctsCusStorageDocPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			return header.EDocPivotCollection;
		}

		public void TestSetCollectionRelationships()
		{
			var collection = (NctsCusStorageDocPivotCollection)GetCollectionToTest();
			var item = collection.AddNew();
			AssertEquals("BH", item.CSD_ParentTableCode);
			AssertNotNull(item.Parent);
		}

		public void TestMaster()
		{
			var collection = (NctsCusStorageDocPivotCollection)GetCollectionToTest();
			AssertEquals(true, collection.Master is INctsCusStorageDocPivotParent _);
		}
	}
}
