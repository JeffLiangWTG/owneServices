namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusAuthorizationUsageLookupsWithInvoiceLineParentTest : CusAuthorizationsUsageLookupsAbstractTest<CusAuthorizationUsageLookups>
	{
		protected override CusAuthorizationUsageLookups GetLookups() => jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew().CusAuthorizationUsages.AddNew().Lookups;
	}
}
