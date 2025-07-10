using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class OriginWrapperTest : DataProviderTestCase<OriginWrapper>
	{
		public void TestCountryOfOrigin()
		{
			AssertEquals("CountryOfOrigin should equal line.JI_CountryOfOrigin.", "XX", Provider.CountryOfOrigin);
		}

		public void TestCountryOfPreferentialOrigin()
		{
			AssertEquals("CountryOfPreferentialOrigin should equal line.ZG_CountryOfSupply.", "YY", Provider.CountryOfPreferentialOrigin);
		}

		protected override OriginWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CountryOfOrigin = "XX";
			line.ZG_CountryOfSupply = "YY";
			return OriginWrapper.New(line);
		}
	}
}
