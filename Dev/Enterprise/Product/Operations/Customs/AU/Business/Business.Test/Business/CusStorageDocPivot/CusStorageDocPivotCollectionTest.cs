using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotCollection))]
	sealed class CusStorageDocPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return invoice.EDocPivotCollection;
		}

		public void TestSetCollectionRelationships()
		{
			var collection = (CusStorageDocPivotCollection)GetCollectionToTest();
			var item = collection.AddNew();
			AssertEquals("JZ", item.CSD_ParentTableCode);
			AssertNotNull(item.Parent);
		}

		public void TestMaster()
		{
			var collection = (CusStorageDocPivotCollection)GetCollectionToTest();
			Assert(collection.Master is ICusStorageDocPivotParent _);
		}
	}
}
