using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationWithNoLinesCollection))]
	public class TransactionPendingAllocationWithNoLinesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = (InvoicingBase)Factory.NewWithValidTestData(typeof(APInvoice));
			return new TransactionPendingAllocationWithNoLinesCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<APInvoiceLine>();
		}

		public override void TestAdd()
		{
			// Cannot add to this Collection
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert(true);
		}

		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}
	}
}
