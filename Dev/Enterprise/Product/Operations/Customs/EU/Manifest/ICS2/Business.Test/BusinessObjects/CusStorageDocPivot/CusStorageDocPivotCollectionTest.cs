using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(CusStorageDocPivotCollection))]
	sealed class CusStorageDocPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return manifestHeader.EDocPivotCollection;
		}

		public void TestSetCollectionRelationships()
		{
			var collection = (CusStorageDocPivotCollection)GetCollectionToTest();
			var item = collection.AddNew();
			AssertEquals("AMA", item.CSD_ParentTableCode);
			AssertNotNull(item.Parent);
		}

		public void TestMaster()
		{
			var collection = (CusStorageDocPivotCollection)GetCollectionToTest();
			AssertEquals(true, collection.Master is AsycudaManifestHeader);
		}
	}
}
