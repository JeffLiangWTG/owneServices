namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class EXDOCSJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsTest
	{
		public void TestEXDOCPermitAuthorityListIsCached()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			Assert("EXDOCPermitAuthorityList should be cached in same factory", ReferenceEquals(Factory.GetCachedValue<EXDOCPermitTypeCodes>(), invoiceLine.Lookups.EXDOCPermitAuthorityList));
		}
	}
}
