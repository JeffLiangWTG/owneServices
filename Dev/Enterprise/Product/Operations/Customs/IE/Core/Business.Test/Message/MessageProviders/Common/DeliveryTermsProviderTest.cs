using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	class DeliveryTermsProviderTest : Customs.Business.Testing.DataProviderTestCase<DeliveryTermsProvider>
	{
		public void TestNew()
		{
			AssertNotNull("Valid argument", Provider);
		}

		public void TestIncotermCode()
		{
			invoiceHeader.JZ_IncoTerm = "FOB";
			AssertEquals("IncotermCode", "FOB", Provider.IncotermCode);
		}

		public void TestUNLOCODE()
		{
			invoiceHeader.ZG_AgreedPlaceCode = "12345";
			invoiceHeader.JZ_IncoTerm = "xxx";

			var provider = GetProvider();
			AssertNull("UNLOCODE when IncoTerm is xxx", provider.UNLOCODE);

			invoiceHeader.JZ_IncoTerm = "FOB";

			CombineAssertions(() =>
			{
				provider = GetProvider();
				AssertEquals("UNLOCODE when IncoTerm is not xxx and ZG_AgreedPlaceCode.length is 5", "12345", provider.UNLOCODE);
				AssertNull("CountryCode when IncoTerm is not xxx and ZG_AgreedPlaceCode.length is 5", provider.CountryCode);
				AssertNull("Place when IncoTerm is not xxx and ZG_AgreedPlaceCode.length is 5", provider.Place);
			});
		}

		public void TestCountryCode()
		{
			invoiceHeader.ZG_AgreedPlaceCode = "IE";
			invoiceHeader.JZ_IncoTerm = "xxx";

			var provider = GetProvider();
			AssertNull("CountryCode when IncoTerm is xxx", provider.CountryCode);

			invoiceHeader.JZ_IncoTerm = "FOB";
			provider = GetProvider();
			CombineAssertions(() =>
			{
				AssertNull("UNLOCODE when IncoTerm is not xxx and ZG_AgreedPlaceCode.length is 2", provider.UNLOCODE);
				AssertEquals("CountryCode when IncoTerm is not xxx and ZG_AgreedPlaceCode.length is 2", "IE", provider.CountryCode);
			});
		}

		public void TestPlace()
		{
			invoiceHeader.ZG_AgreedPlaceCode = "IE";
			invoiceHeader.JZ_IncoTerm = "xxx";
			invoiceHeader.JZ_IncoTermPlace = "PLACE";
			var provider = GetProvider();
			AssertNull("Place when IncoTerm is xxx", provider.Place);

			invoiceHeader.JZ_IncoTerm = "FOB";
			provider = GetProvider();
			CombineAssertions(() =>
			{
				AssertNull("UNLOCODE when IncoTerm is not xxx and ZG_AgreedPlaceCode.length is 2", provider.UNLOCODE);
				AssertEquals("Place when IncoTerm is not xxx", "PLACE", provider.Place);
			});
		}

		public void TestText()
		{
			invoiceHeader.JZ_IncoTerm = "xxx";
			invoiceHeader.JZ_AdditionalTerms = "TEXT";
			var provider = GetProvider();
			AssertEquals("Place when IncoTerm is xxx", "TEXT", provider.Text);

			invoiceHeader.JZ_IncoTerm = "FOB";
			provider = GetProvider();
			AssertNull("Text when IncoTerm is xxx", provider.Text);
		}

		protected override DeliveryTermsProvider GetProvider() => DeliveryTermsProvider.New(invoiceHeader);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
		}

		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;
		protected JobComInvoiceHeader invoiceHeader;
	}
}
