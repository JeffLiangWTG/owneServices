using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;

namespace Enterprise.Client.GCG.DocWrappers.Testing
{
	public class GCGInvoicingBaseDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestMenuTemplateFilterValuesForPrintStandardInvoice()
		{
			ZString result;
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, TestDocumentWrapper.New(Invoice));
			AssertEquals("Filter result for printing Standard Invoice", new ZString("N"), result);
		}

		public void TestMenuTemplateFilterValuesForPrintClientSpecificInvoice()
		{
			ZString result;
			result = DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, TestDocumentWrapper.New(Invoice));
			AssertEquals("Filter result for printing Client Specifc Invoice", new ZString("Y"), result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Invoice = Factory.New<ARInvoice>();
			AssertNotNull("Invoice should not be null", Invoice);
			DocumentSupporter = InvoicingBaseDocumentSupporter.New(Invoice);
			AssertNotNull("Document Supporter should not be null", DocumentSupporter);
		}

		InvoicingBaseDocumentSupporter DocumentSupporter;
		ARInvoice Invoice;
	}
}
