using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BatchTransactionCollection))]
	public class BatchTransactionCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexer()
		{
			BusinessObject obj1 = TestCollection.AddNew();
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = TestCollection.AddNew();
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		protected BatchTransactionCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new BatchTransactionCollection(Factory, null);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BatchTransactionCollection(Factory, new ZQuery());
		}
	}
}
