using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceBatchHeaderCollection))]
	public class Test : TransactionHeaderCollectionTest
	{
		public void TestRelationshipFilter()
		{
			ARInvoice aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			APInvoice aPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			ARPayment aRPayment = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			InvoiceBatchHeader invoiceBatchHeader = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			InvoiceBatchHeaderCollection testCollection = new InvoiceBatchHeaderCollection(Factory);
			testCollection.Load();

			AssertEquals(1, testCollection.Count);
			Assert(testCollection.Contains(invoiceBatchHeader.PK));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceBatchHeaderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<InvoiceBatchHeader>();
		}

		#endregion
	}
}
