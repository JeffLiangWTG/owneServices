using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EdecAddressDataProviderTest : TestCaseWithFactory
{
	public void TestConstructorNullArgument()
	{
		CombineAssertions(() =>
		{
			AssertNull("OrgAddress == null", EdecAddressDataProvider.New((OrgAddress)null));
			AssertNotNull("OrgAddress == null, returnFakeAddressIfNull=true", EdecAddressDataProvider.New((OrgAddress)null, true));
			AssertNotNull("OrgAddress != null", EdecAddressDataProvider.New(orgAddress));

			AssertNull("OrgHeader == null", EdecAddressDataProvider.New((OrgHeader)null));
			AssertNotNull("OrgHeader == null, returnFakeAddressIfNull=true", EdecAddressDataProvider.New((OrgHeader)null, true));
			AssertNotNull("OrgHeader != null", EdecAddressDataProvider.New(orgHeader));

			AssertNull("JobDocAddress == null", EdecAddressDataProvider.New((JobDocAddress)null));
			AssertNotNull("JobDocAddress == null, returnFakeAddressIfNull=true", EdecAddressDataProvider.New((JobDocAddress)null, true));
			AssertNotNull("JobDocAddress != null", EdecAddressDataProvider.New(docAddress));

			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("JobDocAddress is empty", EdecAddressDataProvider.New(docAddress));

			docAddress.E2_AddressOverride = true;
			AssertNotNull("JobDocAddress is overridden", EdecAddressDataProvider.New(docAddress));
		});
	}

	public void TestName()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			AssertEquals("org: default", "-", orgAddressProvider.Name);
			AssertEquals("doc: default", "-", docAddressProvider.Name);

			orgHeader.OH_FullName = "NAM45678901234567890123456789012345X";
			AssertEquals("org: CompanyName", "NAM45678901234567890123456789012345X", orgAddressProvider.Name);
			AssertEquals("doc: CompanyName", "NAM45678901234567890123456789012345X", docAddressProvider.Name);

			orgAddress.OA_CompanyNameOverride = "OVR45678901234567890123456789012345X";
			AssertEquals("org: CompanyNameOverride", "OVR45678901234567890123456789012345X", orgAddressProvider.Name);
			AssertEquals("doc: CompanyNameOverride", "OVR45678901234567890123456789012345X", docAddressProvider.Name);

			AssertEquals("Fake Address", "-", EdecAddressDataProvider.New((OrgAddress)null, true).Name);
		});
	}

	public void TestAddressSupplement1()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			AssertNull("org: empty", orgAddressProvider.AddressSupplement1);
			AssertNull("doc: empty", docAddressProvider.AddressSupplement1);

			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "ADD45678901234567890123456789012345X";
			AssertEquals("org: non-empty", "ADD45678901234567890123456789012345", orgAddressProvider.AddressSupplement1);
			AssertEquals("doc: non-empty", "ADD45678901234567890123456789012345", docAddressProvider.AddressSupplement1);

			AssertNull("Fake Address", EdecAddressDataProvider.New((OrgAddress)null, true).AddressSupplement1);
		});
	}

	public void TestAddressSupplement2()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			AssertNull("org: empty", orgAddressProvider.AddressSupplement2);
			AssertNull("doc: empty", docAddressProvider.AddressSupplement2);

			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "12345678901234567890123456789012345ADD456789012345";
			AssertEquals("org: non-empty", "ADD456789012345", orgAddressProvider.AddressSupplement2);
			AssertEquals("doc: non-empty", "ADD456789012345", docAddressProvider.AddressSupplement2);

			AssertNull("Fake Address", EdecAddressDataProvider.New((OrgAddress)null, true).AddressSupplement2);
		});
	}

	public void TestAddressSupplement3()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "ADD";
			AssertNull("org", orgAddressProvider.AddressSupplement3);
			AssertNull("doc", docAddressProvider.AddressSupplement3);

			AssertNull("Fake Address", EdecAddressDataProvider.New((OrgAddress)null, true).AddressSupplement3);
		});
	}

	public void TestStreet()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			AssertNull("org: empty", orgAddressProvider.Street);
			AssertNull("doc: empty", docAddressProvider.Street);

			orgAddress.OA_Address1 = "STR45678901234567890123456789012345X";
			AssertEquals("org: non-empty", "STR45678901234567890123456789012345X", orgAddressProvider.Street);
			AssertEquals("doc: non-empty", "STR45678901234567890123456789012345X", docAddressProvider.Street);

			AssertNull("Fake Address", EdecAddressDataProvider.New((OrgAddress)null, true).Street);
		});
	}

	public void TestPostalCode()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		var countryWithoutRule = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "VA");
		countryWithoutRule.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;

		var countryWithRule = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
		countryWithRule.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;

		CombineAssertions(() =>
		{
			orgAddress.OA_RL_NKRelatedPortCode = "VAVAT";
			AssertEquals("org: No postal code", ".", orgAddressProvider.PostalCode);
			AssertEquals("doc: No postal code", ".", docAddressProvider.PostalCode);

			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("org: No postal code, but validation rule", string.Empty, orgAddressProvider.PostalCode);
			AssertEquals("doc: No postal code, but validation rule", string.Empty, docAddressProvider.PostalCode);

			AssertEquals("Fake Address", ".", EdecAddressDataProvider.New((OrgAddress)null, true).PostalCode);
		});
	}

	public void TestCity()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			AssertEquals("org: default", "-", orgAddressProvider.City);
			AssertEquals("doc: default", "-", docAddressProvider.City);

			orgAddress.OA_City = "CIT45678901234567890123456789012345X";
			AssertEquals($"org: {nameof(orgAddressProvider.City)}", "CIT45678901234567890123456789012345X", orgAddressProvider.City);
			AssertEquals($"doc: {nameof(orgAddressProvider.City)}", "CIT45678901234567890123456789012345X", docAddressProvider.City);

			AssertEquals("Fake Address", "-", EdecAddressDataProvider.New((OrgAddress)null, true).City);
		});
	}

	public void TestCountry()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			orgAddress.OA_RL_NKRelatedPortCode = "CHBRN";
			AssertEquals("org: Country", "CH", orgAddressProvider.Country);
			AssertEquals("doc: Country", "CH", docAddressProvider.Country);

			AssertEquals("Fake Address", "xx", EdecAddressDataProvider.New((OrgAddress)null, true).Country);
		});
	}

	public void TestTraderIdentificationNumber()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			orgAddress.OA_RL_NKRelatedPortCode = "CHBRN";
			AssertEquals("org: Swiss address empty", ZString.Empty, orgAddressProvider.TraderIdentificationNumber);
			AssertEquals("doc: Swiss address empty", ZString.Empty, docAddressProvider.TraderIdentificationNumber);

			orgHeader.CustomsCodes.AddNew("UID", "E-123.456.789");
			AssertEquals("org: Swiss address E...", "CHE123456789", orgAddressProvider.TraderIdentificationNumber);
			AssertEquals("doc: Swiss address E...", "CHE123456789", docAddressProvider.TraderIdentificationNumber);

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("UID", "E-234.567.890", Core.Constants.CountryCodes.Switzerland);
			AssertEquals("org: Swiss address CHE...", "CHE234567890", orgAddressProvider.TraderIdentificationNumber);
			AssertEquals("doc: Swiss address CHE...", "CHE234567890", docAddressProvider.TraderIdentificationNumber);

			orgAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertNull("org: Non-Swiss address", orgAddressProvider.TraderIdentificationNumber);
			AssertNull("doc: Non-Swiss address", docAddressProvider.TraderIdentificationNumber);

			AssertNull("Fake Address", EdecAddressDataProvider.New((OrgAddress)null, true).TraderIdentificationNumber);
		});
	}

	public void TestReference()
	{
		var orgAddressProvider = EdecAddressDataProvider.New(orgAddress);
		var docAddressProvider = EdecAddressDataProvider.New(docAddress);

		CombineAssertions(() =>
		{
			AssertNull("org: empty", orgAddressProvider.Reference);
			AssertNull("doc: empty", docAddressProvider.Reference);

			orgAddress.OA_AdditionalAddressInformation = "OA345678901234567890123456789012345X";
			AssertEquals("org: empty", "OA345678901234567890123456789012345X", orgAddressProvider.Reference);
			AssertEquals("doc: empty", "OA345678901234567890123456789012345X", docAddressProvider.Reference);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_AdditionalAddressInformation = "E2345678901234567890123456789012345X";
			AssertEquals("doc: Overidden with value", "E2345678901234567890123456789012345X", docAddressProvider.Reference);

			AssertNull("Fake Address", EdecAddressDataProvider.New((OrgAddress)null, true).Reference);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgHeader = Factory.New<OrgHeader>();
		orgAddress = orgHeader.MainAddress;
		docAddress = Factory.New<JobDocAddress>();
		docAddress.E2_OA_Address = orgAddress.PK;
	}

	OrgHeader orgHeader;
	OrgAddress orgAddress;
	JobDocAddress docAddress;
}
