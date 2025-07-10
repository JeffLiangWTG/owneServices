using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	sealed class OrganisationWrapperTest : TestCaseWithFactory
	{
		public void TestOrganisationWrapperFieldsAreTruncated()
		{
			var name = "THIS IS A VERY LONG NAME OF OVER SEVENTY CHARACTERS TO TEST THAT IT WILL BE TRUNCATED TO A LENGTH OF SEVENTY CHARACTERS";

			IOrganisation organisationWrapper = OrganisationWrapper.New(name, "1", AddressWrapper.New("", "", "", ""));

			var expectedName = "THIS IS A VERY LONG NAME OF OVER SEVENTY CHARACTERS TO TEST THAT IT WI";

			AssertEquals("Name should be truncated to 70 characters", expectedName, organisationWrapper.Name);
		}

		public void TestNameWithNewlines()
		{
			const string name = "THIS IS A VERY LONG NAME\nOF OVER SEVENTY CHARACTERS TO TEST THAT IT\r\nWILL BE TRUNCATED TO A LENGTH OF SEVENTY CHARACTERS\r\nAFTER THE CR AND LF CHARACTERS HAVE BEEN REMOVED";
			IOrganisation organisationWrapper = OrganisationWrapper.New(name, "1", AddressWrapper.New("", "", "", ""));
			var expectedName = "THIS IS A VERY LONG NAME OF OVER SEVENTY CHARACTERS TO TEST THAT IT WI";
			AssertEquals("Name should be truncated to 70 characters and not include the CR/LF characters", expectedName, organisationWrapper.Name);
		}

		public void TestIsForeignEori()
		{
			var address = AddressWrapper.New("", "", "", "");

			IOrganisation wrapper1 = OrganisationWrapper.New(string.Empty, "GB123", address);
			Assert(!wrapper1.IsForeignEori);

			IOrganisation wrapper2 = OrganisationWrapper.New(string.Empty, "XI123", address);
			Assert(!wrapper2.IsForeignEori);

			IOrganisation wrapper3 = OrganisationWrapper.New(string.Empty, "00200", address);
			Assert(!wrapper3.IsForeignEori);

			IOrganisation wrapper4 = OrganisationWrapper.New(string.Empty, "", address);
			Assert(!wrapper4.IsForeignEori);

			IOrganisation wrapper5 = OrganisationWrapper.New(string.Empty, "FR123", address);
			Assert(wrapper5.IsForeignEori);
		}

		public void TestIsPrivateIndividual()
		{
			var standardOrg = CreateOrgHeaderWithEori(Factory, "123", Core.Constants.CountryCodes.UnitedKingdom);
			var individualOrg = CreatePrivateIndividualOrg(Factory);
			var declaration = Factory.New<JobDeclaration>();

			declaration.ImporterDocumentaryAddress.OrganisationPK = standardOrg.PK;
			var standardWrapper = (IOrganisation)OrganisationWrapper.New(declaration.ImporterDocumentaryAddress);
			Assert(!standardWrapper.IsPrivateIndividual);

			declaration.ImporterDocumentaryAddress.OrganisationPK = individualOrg.PK;
			var individualWrapper = (IOrganisation)OrganisationWrapper.New(declaration.ImporterDocumentaryAddress);
			Assert(individualWrapper.IsPrivateIndividual);
		}

		public void TestNewWithJobDocAddress()
		{
			const string countryCodeGb = Core.Constants.CountryCodes.UnitedKingdom;
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var orgHeader = CreateOrgHeaderWithEori(Factory, "123", countryCodeGb);
			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;

			IOrganisation wrapper1 = OrganisationWrapper.New(declaration.SupplierDocumentaryAddress);
			CombineAssertions(() =>
			{
				AssertEquals("Name", "Company Name 1", wrapper1.Name);
				AssertEquals("Line", "Address1 1", wrapper1.Address.Line);
				AssertEquals("CityName", "City 1", wrapper1.Address.CityName);
				AssertEquals("Postcode", "Post 1", wrapper1.Address.PostcodeID);
				AssertEquals("ID", "GB123", wrapper1.ID);
			});

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = ZBool.True;
			SetupAddress(declaration.SupplierDocumentaryAddress, "2");
			declaration.SupplierDocumentaryAddress.E2_GovRegNum = "987";
			declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			IOrganisation wrapper2 = OrganisationWrapper.New(declaration.SupplierDocumentaryAddress);
			CombineAssertions(() =>
			{
				AssertEquals("Name", "Company Name 2", wrapper2.Name);
				AssertEquals("Line", "Address1 2", wrapper2.Address.Line);
				AssertEquals("CityName", "City 2", wrapper2.Address.CityName);
				AssertEquals("Postcode", "Post 2", wrapper2.Address.PostcodeID);
				AssertEquals("ID", "GB987", wrapper2.ID);
			});
		}

		public void TestEoriForNewWithJobDocAddressAndMultipleEori()
		{
			var declaration = Factory.New<JobDeclaration>();

			var orgHeader = CreateOrgHeaderWithEori(Factory, "0001", Core.Constants.CountryCodes.Netherlands);
			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;

			IOrganisation wrapper1 = OrganisationWrapper.New(declaration.SupplierDocumentaryAddress);
			AssertEquals("Should be NL", "NL0001", wrapper1.ID);

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI0002", Core.Constants.CountryCodes.UnitedKingdom);
			IOrganisation wrapper2 = OrganisationWrapper.New(declaration.SupplierDocumentaryAddress);
			AssertEquals("Should be XI", "XI0002", wrapper2.ID);

			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "0003", Core.Constants.CountryCodes.UnitedKingdom);
			IOrganisation wrapper3 = OrganisationWrapper.New(declaration.SupplierDocumentaryAddress);
			AssertEquals("Should be GB", "GB0003", wrapper3.ID);

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = ZBool.True;
			SetupAddress(declaration.SupplierDocumentaryAddress, "3");
			declaration.SupplierDocumentaryAddress.E2_GovRegNum = "987";
			declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			IOrganisation wrapper4 = OrganisationWrapper.New(declaration.SupplierDocumentaryAddress);
			AssertEquals("Should be GB GovRegNum", "GB987", wrapper4.ID);
		}

		public void TestNewWithAddressWithNullCountryValue()
		{
			var address = Factory.New<OrgAddressForTest>();
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_City = "London";
			AssertEquals("Address should be null", null, address.Country);
			IOrganisation organisation = OrganisationWrapper.New(address, string.Empty);
			AssertEquals("Country Code Should be empty string", string.Empty, organisation.Address.CountryCode);
		}

		internal static OrgHeader CreateOrgHeaderWithEori(BusinessObjectFactory factory, string eoriCode, string countryCode)
		{
			var result = factory.New<OrgHeader>();
			result.OH_Code = "ImporterA";
			result.OH_RL_NKClosestPort = "GBLHR";
			result.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode, countryCode);

			SetupAddress(result.MainAddress, "1");

			return result;
		}

		internal static OrgHeader CreatePrivateIndividualOrg(BusinessObjectFactory factory)
		{
			var result = factory.New<OrgHeader>();
			result.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			result.OH_RL_NKClosestPort = "GBLHR";
			result.OH_FullName = "John Smith";
			result.MainAddress.Address1 = "John's House";
			return result;
		}

		internal static void SetupAddress(OrgAddress address, ZString suffix)
		{
			address.CompanyName = "Company Name " + suffix;
			address.Address1 = "Address1 " + suffix;
			address.City = "City " + suffix;
			address.Postcode = "Post " + suffix;
		}

		internal static void SetupAddress(JobDocAddress address, ZString suffix)
		{
			address.E2_CompanyName = "Company Name " + suffix;
			address.E2_Address1 = "Address1 " + suffix;
			address.E2_City = "City " + suffix;
			address.E2_Postcode = "Post " + suffix;
		}

		sealed class OrgAddressForTest : OrgAddress
		{
			public OrgAddressForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override RefCountry Country => null;
		}
	}
}
