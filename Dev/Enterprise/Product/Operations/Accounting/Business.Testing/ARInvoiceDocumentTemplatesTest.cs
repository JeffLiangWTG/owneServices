using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngineTest.Testing
{
	public class ARInvoiceDocumentTemplatesTest : DocumentEngine.Testing.DocumentTemplateTestCase
	{
		public void TestARInvoice()
		{
			AssertDocumentCanBeRendered("Invoice");
		}

		protected override IDocumentSupportable GetNewParentBusinessObject()
		{
			return Factory.New<Accounting.Business.ARAP.Invoicing.ARInvoice>();
		}
	}
}