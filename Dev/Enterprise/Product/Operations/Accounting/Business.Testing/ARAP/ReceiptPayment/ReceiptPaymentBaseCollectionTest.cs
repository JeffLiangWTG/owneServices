using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(ReceiptPaymentBaseCollection))]
	public class ReceiptPaymentBaseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APPayment));
		}

		public void TestIndexer()
		{
			BusinessObject obj1 = Factory.New(typeof(APReceipt));
			TestCollection.Add(obj1);
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = Factory.New(typeof(ARReceipt));
			TestCollection.Add(obj2);
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		protected ReceiptPaymentBaseCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new ReceiptPaymentBaseCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ReceiptPaymentBaseCollection(Factory);
		}
	}
}
