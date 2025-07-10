using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class WebAddressFormatterTest : TestCaseWithFactory
	{
		public void TestFormattedAddressWithCompanyName()
		{
			OrgAddress testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.OA_Address1 = "Address1";
			testAddress.OA_Address2 = "Address2";
			testAddress.OA_City = "Test";
			testAddress.OA_PostCode = "60001";
			testAddress.OA_RL_NKRelatedPortCode = "USORD";

			WebAddressFormatter formatter = new WebAddressFormatter(testAddress);
			AddressFormatter helpFormatter = new AddressFormatter(testAddress.Factory, testAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(ArrayToTextConverter.ConvertToCommaSeparatedMultilineText(testAddress.EffectiveCompanyNameTruncated, helpFormatter.PostalAddressAsASingleLineWithoutCompanyName()), formatter.FormattedAddressWithCompanyName());

			helpFormatter = new AddressFormatter(testAddress.Factory, testAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Blah", helpFormatter.PostalAddressAsASingleLine()), formatter.FormattedAddressWithCompanyName("Blah"));

			JobDocAddress testDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			testDocAddress.E2_OA_Address = testAddress.PK;
			testDocAddress.E2_AddressOverride = false;
			formatter = new WebAddressFormatter(testDocAddress);
			helpFormatter = new AddressFormatter(testDocAddress.Factory, testDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(helpFormatter.PostalAddressAsASingleLine(), formatter.FormattedAddressWithCompanyName());

			helpFormatter = new AddressFormatter(testDocAddress.Factory, testDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Blah", helpFormatter.PostalAddressAsASingleLine()), formatter.FormattedAddressWithCompanyName("Blah"));

			testDocAddress.E2_AddressOverride = true;
			testDocAddress.E2_CompanyName = "";
			testDocAddress.E2_Address1 = "Test Address1";
			testDocAddress.E2_Address2 = "Test Address2";
			testDocAddress.E2_City = "Test City";
			testDocAddress.E2_Postcode = "60010";
			testDocAddress.E2_RN_NKCountryCode = "US";
			helpFormatter = new AddressFormatter(testDocAddress.Factory, testDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(helpFormatter.PostalAddressAsASingleLine(), formatter.FormattedAddressWithCompanyName());

			helpFormatter = new AddressFormatter(testDocAddress.Factory, testDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Blah", helpFormatter.PostalAddressAsASingleLine()), formatter.FormattedAddressWithCompanyName("Blah"));

			testDocAddress.E2_CompanyName = "Test Company";
			helpFormatter = new AddressFormatter(testDocAddress.Factory, testDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(helpFormatter.PostalAddressAsASingleLine(), formatter.FormattedAddressWithCompanyName());

			helpFormatter = new AddressFormatter(testDocAddress.Factory, testDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Blah", helpFormatter.PostalAddressAsASingleLine()), formatter.FormattedAddressWithCompanyName("Blah"));

			testDocAddress.E2_CompanyName = "Blah";
			helpFormatter = new AddressFormatter(testDocAddress.Factory, testDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(helpFormatter.PostalAddressAsASingleLine(), formatter.FormattedAddressWithCompanyName());

			helpFormatter = new AddressFormatter(testDocAddress.Factory, testDocAddress, GlbCompany.CurrentCompany, false);
			AssertEquals(ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Blah", helpFormatter.PostalAddressAsASingleLineWithoutCompanyName()), formatter.FormattedAddressWithCompanyName("Blah"));
		}
	}
}
