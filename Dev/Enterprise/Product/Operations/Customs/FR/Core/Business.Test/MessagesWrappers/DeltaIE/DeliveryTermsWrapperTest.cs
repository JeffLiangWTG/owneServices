using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class DeliveryTermsWrapperTest : Customs.Business.Testing.DataProviderTestCase<DeliveryTermsWrapper>
	{
		protected override DeliveryTermsWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = "ABZZZ";
			invoice.JZ_IncoTermPlace = "Sydney";
			invoice.JZ_AdditionalTerms = "AAABBBCCC";
			invoice.ZG_IncotermCountry = "LV";

			return DeliveryTermsWrapper.New(invoice);
		}

		public void TestText()
		{
			AssertEquals("Text should be equal to JZ_AdditionalTerms.", "AAABBBCCC", Provider.Text);
		}

		public void TestUNLOCODE()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = "ABZZZ";
			var wrapper = DeliveryTermsWrapper.New(invoice);
			AssertEquals("UNLOCODE should be equal to ZG_AgreedPlaceCode.", "ABZZZ", wrapper.UNLOCODE);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTermPlace = "Sydney";
			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = ZString.Empty;
			declaration.ZG_AgreedPlaceCode = "CSEEE";

			var wrapper2 = DeliveryTermsWrapper.New(invoice);

			AssertEquals("UNLOCODE should be equal to ZG_AgreedPlaceCode of declaration.", "CSEEE", wrapper2.UNLOCODE);
		}

		public void TestLocation()
		{
			AssertEquals("Location should be equal to JZ_IncoTermPlace.", "Sydney", Provider.Location);
		}

		public void TestCountry()
		{
			AssertEquals("Country should be equal to ZG_IncotermCountry.", "LV", Provider.Country);
		}

		public void TestIncotermCode()
		{
			AssertEquals("IncotermCode should be equal to JZ_IncoTerm.", "FOB", Provider.IncotermCode);
		}
	}
}
