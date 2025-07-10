using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExportHeaderDeliveryTermsProviderTest : Customs.Business.Testing.DataProviderTestCase<ExportHeaderDeliveryTermsProvider>
	{
		public void TestIncotermCode()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertEquals("FOB", Provider.IncotermCode);
		}

		public void TestUNLocode()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "AUSYD";
			CombineAssertions(() =>
			{
				AssertEquals("UNLocode", "AUSYD", Provider.UNLocode);
				AssertNull("Country", Provider.Country);
				AssertEquals("Location", ZString.Empty, Provider.Location);
			});
		}

		public void TestUNLocode_Unknown()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			AssertNull("UNLOCO", Provider.UNLocode);
		}

		public void TestUNLocode_IncotermXXX()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_IncoTermPlace = "AUSYD";
			AssertNull(Provider.UNLocode);
		}

		public void TestCountry()
		{
			AddUserDefinedRefUNLOCO();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			CombineAssertions(() =>
			{
				AssertNull("UNLocode null", Provider.UNLocode);
				AssertEquals("Country filled", "IE", Provider.Country);
			});
		}

		public void TestCountry_UNLOCO()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "AUSYD";
			AssertNull(Provider.Country);
		}

		public void TestCountry_IncotermXXX()
		{
			AddUserDefinedRefUNLOCO();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			AssertNull(Provider.Country);
		}

		public void TestLocation()
		{
			AddUserDefinedRefUNLOCO();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			CombineAssertions(() =>
			{
				AssertNull("UNLocode null", Provider.UNLocode);
				AssertEquals("Location filled", "ÄÄÄ", Provider.Location);
			});
		}

		public void TestLocation_UNLOCO()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_IncoTermPlace = "AUSYD";
			AssertEquals(ZString.Empty, Provider.Location);
		}

		public void TestLocation_IncotermXXX()
		{
			AddUserDefinedRefUNLOCO();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			invoiceHeader.JZ_IncoTermPlace = "IEAAA";
			AssertEquals(ZString.Empty, Provider.Location);
		}

		public void TestText_IncotermXXX()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
			AssertEquals("Location", Provider.Text);
		}

		public void TestText_IncotermNotXXX()
		{
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			AssertNull(Provider.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceHeader = Factory.New<JobComInvoiceHeader>();
		}
		JobComInvoiceHeader invoiceHeader;

		protected override ExportHeaderDeliveryTermsProvider GetProvider() => new ExportHeaderDeliveryTermsProvider(invoiceHeader);

		void AddUserDefinedRefUNLOCO()
		{
			var userDefinedRefUNLOCO = Factory.New<RefUNLOCO>();
			userDefinedRefUNLOCO.RL_Code = "IEAAA";
			userDefinedRefUNLOCO.RL_RN_NKCountryCode = "IE";
			userDefinedRefUNLOCO.RL_NameWithDiacriticals = "ÄÄÄ";
		}
	}
}
