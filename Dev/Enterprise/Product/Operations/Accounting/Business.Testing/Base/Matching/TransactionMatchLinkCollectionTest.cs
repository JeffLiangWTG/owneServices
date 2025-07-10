using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(TransactionMatchLinkCollection))]
	public class TransactionMatchLinkCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionAddNew()
		{
			AssertNotNull(TestCollection.AddNew());
			AssertEquals(1, TestCollection.Count);
		}

		public void TestIndexer()
		{
			BusinessObject obj1 = TestCollection.AddNew();
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = TestCollection.AddNew();
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		protected TransactionMatchLinkCollection TestCollection;
		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new TransactionMatchLinkCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TransactionMatchLinkCollection(Factory);
		}
	}
}
