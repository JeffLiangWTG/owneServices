using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt.Testing
{
	[TestedType(typeof(DirectReceiptLineCollection))]
	public class DirectReceiptLineCollection_InnerTest : DependentTransactionLineCollectionTest
	{
		public void TestIndexer()
		{
			BusinessObject obj1 = TestCollection.AddNew();
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = TestCollection.AddNew();
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		protected DirectReceiptLineCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new DirectReceiptLineCollection(Factory.New<DirectReceipt>(), Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<DirectReceipt>();
			AssertNotNull(
@"This is just to initialize 'Lines' collection before any lines are created to prevent loading them in it later as side effect of calling bizo properties.
Such 'accidental', from test position, 'Lines' collection load run some collection code that is interfere with test expectations.",
				parent.Lines);
			return new DirectReceiptLineCollection(parent, Factory);
		}
	}
}
