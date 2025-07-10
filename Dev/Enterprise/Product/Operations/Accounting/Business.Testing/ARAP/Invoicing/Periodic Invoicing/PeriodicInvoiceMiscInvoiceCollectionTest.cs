using System;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceMiscInvoiceCollection))]
	public class PeriodicInvoiceMiscInvoiceCollectionTest : InvoicingBaseCollectionTest
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(PeriodicInvoiceMiscInvoiceCollection);
		}

		public void TestSelectedCount()
		{
			PeriodicInvoiceMiscInvoiceCollection testCollection = new PeriodicInvoiceMiscInvoiceCollection(Factory);
			ARInvoice invoice = Factory.New<ARInvoice>();
			ARInvoice invoice2 = Factory.New<ARInvoice>();
			testCollection.Add(invoice);
			testCollection.Add(invoice2);
			AssertEquals(2, testCollection.SelectedCount);
			testCollection[0].IncludeInThePeriodicInvoice = false;
			AssertEquals(1, testCollection.SelectedCount);
			testCollection[1].IncludeInThePeriodicInvoice = false;
			AssertEquals(0, testCollection.SelectedCount);
		}
	}
}
