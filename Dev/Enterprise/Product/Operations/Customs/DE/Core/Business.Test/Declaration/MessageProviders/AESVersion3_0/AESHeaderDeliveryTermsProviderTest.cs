using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class AESHeaderDeliveryTermsProviderTest : Customs.Business.Testing.DataProviderTestCase<AESHeaderDeliveryTermsProvider>
	{
		public void TestIncotermCode()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("FOB", Provider.IncotermCode);
		}

		public void TestUNLocode_Empty_IncoTermPlace()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "";
			invoiceHeader.ZG_AgreedPlaceCode = "A";
			AssertEquals("UNLocode", "A", Provider.UNLocode);
		}

		public void TestUNLocode_NotEmpty_IncoTermPlace()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			invoiceHeader.ZG_AgreedPlaceCode = "A";
			AssertNull(Provider.UNLocode);
		}

		public void TestUNLocode_IncotermXXX()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_IncoTermPlace = "";
			invoiceHeader.ZG_AgreedPlaceCode = "A";
			AssertNull(Provider.UNLocode);
		}

		public void TestCountry()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			invoiceHeader.ZG_AgreedPlaceCode = "A";
			AssertEquals("Country filled", "A", Provider.Country);
		}

		public void TestCountry_Empty_IncoTermPlace()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "";
			invoiceHeader.ZG_AgreedPlaceCode = "A";
			AssertNull(Provider.Country);
		}

		public void TestCountry_IncotermXXX()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			invoiceHeader.ZG_AgreedPlaceCode = "A";
			AssertNull(Provider.Country);
		}

		public void TestLocation()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			AssertEquals("Location filled", "IEAAA", Provider.Location);
		}

		public void TestLocation_IncotermXXX()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			AssertEquals(ZString.Empty, Provider.Location);
		}

		public void TestText()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			AssertNull(Provider.Text);
		}

		public void TestText_IncotermXXX()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			AssertEquals("IEAAA", Provider.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<JobComInvoiceHeader>();
		}

		protected override AESHeaderDeliveryTermsProvider GetProvider() => new AESHeaderDeliveryTermsProvider(invoiceHeader);

		JobComInvoiceHeader invoiceHeader;
	}
}
