namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			AssertSame(invoiceHeader, lookups.Invoice);
		}

		public void TestJZ_IncoTerm_List()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var invoice = declaration.Invoices.AddNew();
				AssertContains(Core.Constants.IncoTerms.Other, invoice.Lookups.JZ_IncoTerm_List.CodesAsString);
			}

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var invoice = declaration.Invoices.AddNew();
				AssertContains(Core.Constants.IncoTerms.Other, invoice.Lookups.JZ_IncoTerm_List.CodesAsString);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<JobComInvoiceHeader>();
			lookups = new JobComInvoiceHeaderLookups(invoiceHeader);
		}
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceHeaderLookups lookups;
	}
}
