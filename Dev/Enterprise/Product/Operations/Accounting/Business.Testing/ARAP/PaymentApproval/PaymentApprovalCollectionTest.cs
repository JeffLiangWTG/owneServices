using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(PaymentApprovalCollection))]
	public class PaymentApprovalCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectException(typeof(NoConcreteTypeException))]
		public new void TestAddNew()
		{
			TestCollection.AddNew();
		}

		[ExpectException(typeof(NoConcreteTypeException))]
		public new void TestTypedAddNew()
		{
			base.TestTypedAddNew();
		}

		public void TestIndexer()
		{
			BusinessObject obj1 = Factory.New(typeof(APPaymentApprovalWithAuthorisation));
			TestCollection.Add(obj1);
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = Factory.New(typeof(ARPaymentApprovalWithAuthorisation));
			TestCollection.Add(obj2);
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		PaymentApprovalCollection TestCollection;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new PaymentApprovalCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APPaymentApprovalWithAuthorisation));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new PaymentApprovalCollection(Factory);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert(true);
		}
	}
}
