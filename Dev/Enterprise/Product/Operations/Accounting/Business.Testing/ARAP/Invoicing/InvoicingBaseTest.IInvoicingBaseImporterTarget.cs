namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	partial class InvoicingBaseTest
	{
		public void TestAsInvoicingBaseImporterTarget()
		{
			AssertEquals(HeaderAsInvoicingBase, HeaderAsInvoicingBase.AsInvoicingBaseImporterTarget);
		}

		public void TestAsInvoicingBaseImporterTarget_Factory()
		{
			AssertEquals(HeaderAsInvoicingBase.Factory, HeaderAsInvoicingBase.AsInvoicingBaseImporterTarget.Factory);
		}

		public void TestAsInvoicingBaseImporterTarget_ConsolCosting()
		{
			AssertEquals(HeaderAsInvoicingBase.ConsolCosting, HeaderAsInvoicingBase.AsInvoicingBaseImporterTarget.ConsolCosting);
		}
	}
}
