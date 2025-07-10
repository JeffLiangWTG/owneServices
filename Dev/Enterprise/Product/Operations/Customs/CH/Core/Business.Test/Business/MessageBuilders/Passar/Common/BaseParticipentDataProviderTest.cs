using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class BaseParticipientDataProviderTest : TestCaseWithFactory
{
	public void TestProvider()
	{
		const string aeoRegNo = "456";

		DocAddress.Organisation.OH_FullName = "OrgHeader Full Name";
		DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, aeoRegNo);
		var dataProvider = new BaseParticipentDataProviderForTesting(DocAddress, null, false);

		CombineAssertions(() =>
		{
			AssertEquals("Name", "OrgHeader Full Name", dataProvider.Name);
			AssertNull(dataProvider.IdentificationNumber);
			AssertEquals("AeoReferenceNumber", aeoRegNo, dataProvider.AeoReferenceNumber);
			AssertNotNull(dataProvider.Address);
			AssertSame("cached", dataProvider.Address, dataProvider.Address);
		});
	}

	public void TestContact() => CombineAssertions(() =>
	{
		var orgHeader = DocAddress.Organisation;
		orgHeader.MainAddress.OA_Email = "main@example.org";
		var contact1 = orgHeader.Contacts.AddNew();
		contact1.OC_Email = "unallocated@example.org";
		var contact2 = orgHeader.Contacts.AddNew();
		contact2.OC_Email = "haz@example.org";
		contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.HAZ;

		DocAddress.E2_AddressOverride = ZBool.False;
		var dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
		AssertEquals("Default contact", "main@example.org", dataProvider.ContactPerson.EmailAddress);

		DocAddress.E2_AddressOverride = ZBool.True;
		DocAddress.E2_Email = "override@example.org";
		dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
		AssertEquals("Overidden contact", "override@example.org", dataProvider.ContactPerson.EmailAddress);

		DocAddress.E2_AddressOverride = ZBool.False;
		dataProvider = new BaseParticipentDataProviderForTesting(DocAddress, OrgConstants.ContactAllocationType.HAZ);
		AssertEquals("Allocated contact", "haz@example.org", dataProvider.ContactPerson.EmailAddress);

		DocAddress.E2_AddressOverride = ZBool.True;
		DocAddress.E2_Email = "haz-override@example.org";
		dataProvider = new BaseParticipentDataProviderForTesting(DocAddress, OrgConstants.ContactAllocationType.HAZ);
		AssertEquals("Allocated contact overridden", "haz-override@example.org", dataProvider.ContactPerson.EmailAddress);

		AssertSame("cached", dataProvider.Address, dataProvider.Address);

		DocAddress.E2_AddressOverride = ZBool.False;
		dataProvider = new BaseParticipentDataProviderForTesting(DocAddress, OrgConstants.ContactAllocationType.CAPGA);
		AssertNull("Allocated contact missing", dataProvider.ContactPerson);
	});

	public void TestIdentificationNumber()
	{
		const string dunRegNoWithoutAddress = "DUN_WithoutAddress";
		const string dunRegNoWithAddress = "DUN_WithAddress";

		CombineAssertions(() =>
		{
			var dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertNull("Null when no CustomsRegNo is defined", dataProvider.IdentificationNumber);

			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, dunRegNoWithoutAddress);
			AssertEquals("DUN Without linked address", dunRegNoWithoutAddress, dataProvider.IdentificationNumber);

			var customsRegNoDun = DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, dunRegNoWithAddress);
			customsRegNoDun.OK_OA_PremisesAddress = DocAddress.E2_OA_Address;
			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertEquals("DUN with linked address", dunRegNoWithAddress, dataProvider.IdentificationNumber);

			customsRegNoDun.OK_OA_PremisesAddress = DocAddress.Organisation.Addresses.AddNew().PK;
			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertEquals("When Linked address != JobDocAddress -> DUN without linked address", dunRegNoWithoutAddress, dataProvider.IdentificationNumber);
		});
	}

	public void TestOmitAddressAndNameIfHasIdentificationNumber() => CombineAssertions(() =>
	{
		DocAddress.Address.CompanyName = "name";

		var dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
		AssertNull("With IdentificationNumber: IdentificationNumber", dataProvider.IdentificationNumber);
		AssertNotNullOrEmpty("With IdentificationNumber: Name", dataProvider.Name);
		AssertNotNull("With IdentificationNumber: Address", dataProvider.Address);

		DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "123");
		dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
		AssertNotNullOrEmpty("Without IdentificationNumber: IdentificationNumber", dataProvider.IdentificationNumber);
		AssertNull("Without IdentificationNumber: Name", dataProvider.Name);
		AssertNull("Without IdentificationNumber: Address", dataProvider.Address);
	});

	public void TestAeoReferenceNumber()
	{
		RefCusCodeTestHelper.CreateEUCountryList(Factory);
		CombineAssertions(() =>
		{
			var dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertNull("When no AEO defined", dataProvider.AeoReferenceNumber);

			DocAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO123");
			DocAddress.Address.OA_RN_NKCountryCode = RefCusCodeTestHelper.CountryNotInEU;
			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress, null, false);
			AssertEquals("With AEO defined and check disabled", "AEO123", dataProvider.AeoReferenceNumber);

			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertNull("When no AEO defined and check Enabled but country outside the list", dataProvider.AeoReferenceNumber);
			DocAddress.Address.OA_RN_NKCountryCode = RefCusCodeTestHelper.CountryInEUAndCL010CountryList;
			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertEquals("With AEO defined Enabled and country in EU", "AEO123", dataProvider.AeoReferenceNumber);
			DocAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertEquals("With AEO defined and country is GB", "AEO123", dataProvider.AeoReferenceNumber);
			DocAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;
			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertEquals("With AEO defined and country is NO", "AEO123", dataProvider.AeoReferenceNumber);
			DocAddress.Address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
			AssertEquals("With AEO defined and country is CN", "AEO123", dataProvider.AeoReferenceNumber);
		});
	}

	public void TestNullableName()
	{
		var dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
		AssertEquals("Name", null, dataProvider.Name);
	}

	public void TestOverrideName() => CombineAssertions(() =>
	{
		DocAddress.Organisation.OH_FullName = "OH_FullName";

		DocAddress.E2_AddressOverride = ZBool.True;
		DocAddress.E2_CompanyName = "E2_CompanyName";
		var dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
		AssertEquals($"E2_AddressOverride={DocAddress.E2_AddressOverride}", "E2_CompanyName", dataProvider.Name);

		DocAddress.E2_AddressOverride = ZBool.False;
		dataProvider = new BaseParticipentDataProviderForTesting(DocAddress);
		AssertEquals($"E2_AddressOverride={DocAddress.E2_AddressOverride}", "OH_FullName", dataProvider.Name);
	});

	JobDocAddress CreateDocAddress()
	{
		var orgAddress = Factory.New<OrgHeader>().MainAddress;
		var address = Factory.New<JobDocAddress>();
		address.E2_OA_Address = orgAddress.PK;
		return address;
	}

	JobDocAddress DocAddress => docAddress ?? (docAddress = CreateDocAddress());
	JobDocAddress docAddress;
}

sealed class BaseParticipentDataProviderForTesting : BaseParticipentDataProvider
{
	internal BaseParticipentDataProviderForTesting(JobDocAddress docAddress) : base(docAddress)
	{
	}
	internal BaseParticipentDataProviderForTesting(JobDocAddress docAddress, string contactType = null) : base(docAddress, contactType)
	{
	}
	internal BaseParticipentDataProviderForTesting(JobDocAddress docAddress, string contactType = null, bool notCheckCountryForAeoReferenceNumber = false) : base(docAddress, contactType, notCheckCountryForAeoReferenceNumber)
	{
	}
}
