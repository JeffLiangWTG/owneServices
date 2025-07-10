using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSPartyIDAddressContactProviderTest : Customs.Business.Testing.DataProviderTestCase<INCTSPartyIDAddressContact>
	{
		public void TestPartyName_NullWithEori()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			AssertNull(Provider.PartyName);
		}

		public void TestPartyName_FromOrg()
		{
			AssertEquals("CompanyXYZ", Provider.PartyName);
		}

		public void TestPartyName_FromJobDocAddress()
		{
			var provider = GetProviderForOverridedAddress();
			AssertEquals("CompanyABC", provider.PartyName);
		}

		public void TestAddress_NullWithEori()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			AssertNull(Provider.Address);
		}

		public void TestAddress_FromOrg()
		{
			AssertEquals("Address part 1Address part 2", Provider.Address);
		}

		public void TestAddress_FromJobDocAddress()
		{
			var provider = GetProviderForOverridedAddress();
			AssertEquals("DocAddress part 1DocAddress part 2", provider.Address);
		}

		public void TestCity_NullWithEori()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			AssertNull(Provider.City);
		}

		public void TestCity_FromOrg()
		{
			AssertEquals("Entenhausen", Provider.City);
		}

		public void TestCity_FromJobDocAddress()
		{
			var provider = GetProviderForOverridedAddress();
			AssertEquals("Duckistan", provider.City);
		}

		public void TestPostcode_NullWithEori()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			AssertNull(Provider.Postcode);
		}

		public void TestPostcode_FromOrg()
		{
			AssertEquals("41516", Provider.Postcode);
		}

		public void TestPostcode_FromJobDocAddress()
		{
			var provider = GetProviderForOverridedAddress();
			AssertEquals("11111", provider.Postcode);
		}

		public void TestCountry_NullWithEori()
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CreateRegistrationNumber(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", "0001");
			AssertNull(Provider.Country);
		}

		public void TestCountry_FromOrg()
		{
			AssertEquals("DE", Provider.Country);
		}

		public void TestCountry_FromJobDocAddress()
		{
			var provider = GetProviderForOverridedAddress();
			AssertEquals("GB", provider.Country);
		}

		public void TestEquals()
		{
			var docAddress2 = (JobDocAddress)docAddress.Clone();
			docAddress2.E2_AddressOverride = true;
			var provider2Compare = NCTSPartyIDAddressContactProvider.NewOrNull(docAddress2, null);
			AssertEquals("same address content", true, Provider.Equals(provider2Compare));

			docAddress2.City = "changed";
			provider2Compare = NCTSPartyIDAddressContactProvider.NewOrNull(docAddress2, null);
			AssertEquals("different address content", false, Provider.Equals(provider2Compare));
		}

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "CompanyXYZ";
			orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "Address part 1";
			orgAddress.OA_Address2 = "Address part 2";
			orgAddress.OA_PostCode = "41516";
			orgAddress.OA_City = "Entenhausen";
			orgAddress.OA_RN_NKCountryCode = "DE";

			docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
		}

		protected override INCTSPartyIDAddressContact GetProvider() => NCTSPartyIDAddressContactProvider.NewOrNull(docAddress, null);

		void CreateRegistrationNumber(string identificationType, string identificationNumber, string ebs = null)
		{
			orgHeader.CustomsCodes.AddNew(identificationType, identificationNumber, Core.Constants.CountryCodes.Greece);
			if (ebs != null)
			{
				orgAddress.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
				var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebs, Core.Constants.CountryCodes.Germany);
				ebsCode.OK_OA_PremisesAddress = orgAddress.PK;
			}
		}

		INCTSPartyIDAddressContact GetProviderForOverridedAddress()
		{
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "CompanyABC";
			docAddress.E2_Address1 = "DocAddress part 1";
			docAddress.E2_Address2 = "DocAddress part 2";
			docAddress.E2_Postcode = "11111";
			docAddress.E2_City = "Duckistan";
			docAddress.E2_RN_NKCountryCode = "GB";
			return NCTSPartyIDAddressContactProvider.NewOrNull(docAddress, null);
		}

		JobDocAddress docAddress;
		OrgHeader orgHeader;
		OrgAddress orgAddress;
	}
}
