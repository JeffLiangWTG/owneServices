using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Client.WLG.Testing
{
	[TestedType(typeof(WLGARInvoice))]
	public class WLGARInvoiceTest : ARInvoiceTest
	{
		public void TestTypeDecidingTheCorrectType()
		{
			WLGARInvoice invoice = Factory.NewWithValidTestData<WLGARInvoice>();
			Factory.Save();
			BusinessObject newInvoice = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);
			AssertEquals("If this fails, check the base class. It has to be subclassed from AR invoice.", typeof(WLGARInvoice), newInvoice.GetType());
		}

		public void TestDocumentSupporterPropertyCallsReturnSameInstance()
		{
			WLGARInvoice invoice = Factory.New<WLGARInvoice>();
			AssertEquals(invoice.DocumentSupporter, invoice.DocumentSupporter);
		}

		public new void TestGetDocBusinessObject()
		{
			AssertEquals("Type of doc wrapper", "DocWLGARInvoice", InvoicingBase.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.ARInvoice, null)[0].GetType().Name);
			AssertEquals("No data context", null, InvoicingBase.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.None, null));
		}

		[TestedType(typeof(WLGARInvoice))]
		public class WLGARInvoiceMatchingTest : InvoicingBaseMatchingTest
		{
			protected override InvoicingBase GetNewInvoice()
			{
				return Factory.New<WLGARInvoice>();
			}
		}
	}
}
