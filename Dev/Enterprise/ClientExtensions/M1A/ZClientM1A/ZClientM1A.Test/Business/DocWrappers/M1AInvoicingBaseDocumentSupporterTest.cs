using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Client.M1A.Testing
{
	public class M1AInvoicingBaseDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestSupportedDataContexts()
		{
			AssertEquals("Core.Constants.DataContext.ARInvoice is Supported", true, documentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ARInvoice)));
			DocumentWrapper[] wrapperArray = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertEquals("Document Supporter BusinessObjects", 1, wrapperArray.Length);
		}

		public void TestMenuTemplateFilterValues()
		{
			ZString result = ZString.Empty;
			result = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, null);
			AssertEquals("Filter result for printing Standard Invoice", new ZString("N"), result);
			result = documentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, null);
			AssertEquals("Filter result for printing Client Specifc Invoice", new ZString("Y"), result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoice = Factory.New<ARInvoice>();
			AssertNotNull("Invoice should not be null", invoice);
			documentSupporter = InvoicingBaseDocumentSupporter.New(invoice);
			AssertNotNull("Document Supporter should not be null", documentSupporter);
		}

		InvoicingBaseDocumentSupporter documentSupporter;
		ARInvoice invoice;
	}
}
