using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class HolderOfTransitProcedureDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		AssertNull(HolderOfTransitProcedureDataProvider.New(null));

		var address = Factory.New<JobDocAddress>();
		AssertNull("JobDocAddress is empty", HolderOfTransitProcedureDataProvider.New(address));
		address.E2_AddressOverride = true;
		AssertNotNull("E2_AddressOverride = true", HolderOfTransitProcedureDataProvider.New(address));
		address.E2_OA_Address = Factory.New<OrgAddress>().PK;
		AssertNotNull("E2_AddressOverride = true", HolderOfTransitProcedureDataProvider.New(address));
	}

	public void TestName()
	{
		const string fullName = "abc";
		OrgAddress.Header.OH_FullName = fullName;
		OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		AssertNull(DataProvider.Name);

		OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		AssertEquals(fullName, HolderOfTransitProcedureDataProvider.New(DocAddress).Name);

		DocAddress.E2_AddressOverride = true;
		DocAddress.E2_CompanyName = "def";
		AssertEquals("def", HolderOfTransitProcedureDataProvider.New(DocAddress).Name);
	}

	public void TestIdentificationNumber_CH()
	{
		CombineAssertions(() =>
		{
			OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			AssertNull(HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123", Core.Constants.CountryCodes.Switzerland);
			AssertEquals("123", HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.UID, "456", Core.Constants.CountryCodes.Switzerland);
			AssertEquals("456", HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "789", Core.Constants.CountryCodes.Switzerland);
			AssertEquals("789", HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);
		});
	}

	public void TestIdentificationNumber_EU() => AssertTestIdentificationNumber(Core.Constants.CountryCodes.Germany);
	public void TestIdentificationNumber_GB() => AssertTestIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom);

	void AssertTestIdentificationNumber(ZString countryCode)
	{
		CombineAssertions(() =>
		{
			OrgAddress.OA_RN_NKCountryCode = countryCode;
			AssertNull(HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123", countryCode);
			AssertEquals("123", HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "456", countryCode);
			AssertEquals("456", HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);
		});
	}

	public void TestIdentificationNumber_NO()
	{
		CombineAssertions(() =>
		{
			OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
			AssertNull(HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OrganizationNumber, "XXX", Core.Constants.CountryCodes.Switzerland);
			AssertNull(HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OrganizationNumber, "123", Core.Constants.CountryCodes.Norway);
			AssertEquals("NO123", HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);
		});
	}

	public void TestIdentificationNumber_TR()
	{
		CombineAssertions(() =>
		{
			OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			AssertNull(HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "XXX", Core.Constants.CountryCodes.Switzerland);
			AssertNull(HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);

			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123", Core.Constants.CountryCodes.Turkey);
			AssertEquals("123", HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);
		});
	}

	public void TestIdentificationNumber_ShouldBeCached()
	{
		OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		var orgCusCode = OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");

		var initiallyAccessedValue = DataProvider.IdentificationNumber;
		orgCusCode.OK_CustomsRegNo = "456";

		AssertEquals(initiallyAccessedValue, DataProvider.IdentificationNumber);
	}

	public void TestIdentificationNumber_AddressOverridden()
	{
		CombineAssertions(() =>
		{
			OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123", Core.Constants.CountryCodes.Switzerland);

			DocAddress.E2_AddressOverride = true;
			DocAddress.E2_CompanyName = "abc";

			AssertNull(HolderOfTransitProcedureDataProvider.New(DocAddress).IdentificationNumber);
		});
	}

	public void TestTIRIdentificationNumber()
	{
		OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "XXX");
		AssertNull(HolderOfTransitProcedureDataProvider.New(DocAddress).TIRHolderIdentificationNumber);

		OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "123", Core.Constants.CountryCodes.Australia);
		AssertEquals("123", HolderOfTransitProcedureDataProvider.New(DocAddress).TIRHolderIdentificationNumber);

		OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "456", Core.Constants.CountryCodes.Switzerland);
		AssertEquals("456", HolderOfTransitProcedureDataProvider.New(DocAddress).TIRHolderIdentificationNumber);
	}

	public void TestTIRIdentificationNumber_ShouldBeCached()
	{
		var orgCusCode = OrgAddress.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "123");

		var initiallyAccessedValue = DataProvider.IdentificationNumber;
		orgCusCode.OK_CustomsRegNo = "456";

		AssertEquals(initiallyAccessedValue, DataProvider.IdentificationNumber);
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			AssertNull("CH", DataProvider.Address);

			OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			AssertType<AddressDataProvider>("type", DataProvider.Address);
			AssertSame("cached", DataProvider.Address, DataProvider.Address);
		});
	}

	public void TestAddress_CH()
	{
		OrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		AssertNull(DataProvider.Address);
	}

	public void TestContactPerson() => CombineAssertions(() =>
	{
		AssertType<StaffContactPersonDataProvider>("type", DataProvider.ContactPerson);
		AssertEquals("CurrentUser", GlbStaff.CurrentUser.GS_FullName, DataProvider.ContactPerson.Name);
		AssertSame("cached", DataProvider.ContactPerson, DataProvider.ContactPerson);

		DocAddress.E2_AddressOverride = true;
		DocAddress.E2_Contact = "John Doe";
		DocAddress.E2_Email = "john.doe@wisetechglobal.com";
		DocAddress.E2_Phone = "+61280012200";

		dataProvider = null;
		AssertEquals("Contact Name", DocAddress.E2_Contact, DataProvider.ContactPerson.Name);
		AssertEquals("Contact eMail", DocAddress.E2_Email, DataProvider.ContactPerson.EmailAddress);
		AssertEquals("Contact Phone", DocAddress.E2_Phone, DataProvider.ContactPerson.PhoneNumber);
	});

	JobDocAddress CreateDocAddress()
	{
		var address = Factory.New<JobDocAddress>();
		address.E2_OA_Address = OrgAddress.PK;
		return address;
	}

	JobDocAddress DocAddress => docAddress ?? (docAddress = CreateDocAddress());
	JobDocAddress docAddress;

	OrgAddress OrgAddress => orgAddress ?? (orgAddress = Factory.New<OrgHeader>().MainAddress);
	OrgAddress orgAddress;

	HolderOfTransitProcedureDataProvider DataProvider => dataProvider ?? (dataProvider = HolderOfTransitProcedureDataProvider.New(DocAddress));
	HolderOfTransitProcedureDataProvider dataProvider;
}
