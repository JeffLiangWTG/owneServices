using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	class PartyAddressWrapperTest : WrapperHelperTest<PartyAddressWrapper>
	{
		public void TestGetNewPartyWrapper()
		{
			CombineAssertions(() =>
			{
				OrgAddress address = null;
				AssertNull("OrgAddress null", PartyAddressWrapper.New(address));
				address = Factory.New<OrgAddress>();
				AssertNull("OrgAddress with null OrgHeader null", PartyAddressWrapper.New(address));

				var org = Factory.New<OrgHeader>();
				address.OA_OH = org.PK;
				AssertNotNull("OrgAddress not null", PartyAddressWrapper.New(address));
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Address1 = "1234 Test Street";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled Address from dbo.OrgAddress", "1234 Test Street", wrapper.Address);

				wrapper = GetWrapper("Address", ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled Address from ZString", "Address", wrapper.Address);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_City = "Barcelona";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled City from dbo.OrgAddress", "Barcelona", wrapper.City);

				wrapper = GetWrapper(ZString.Empty, "City", ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled City from ZString", "City", wrapper.City);
			});
		}

		public void TestPostCode()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_PostCode = "98765";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled PostCode from dbo.OrgAddress", "98765", wrapper.PostCode);

				wrapper = GetWrapper(ZString.Empty, ZString.Empty, "PostCode", ZString.Empty);
				AssertEquals("Expected filled PostCode from ZString", "PostCode", wrapper.PostCode);
			});
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_RN_NKCountryCode = "ES";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled Country from dbo.OrgAddress", "ES", wrapper.Country);

				wrapper = GetWrapper(ZString.Empty, ZString.Empty, ZString.Empty, "Country");
				AssertEquals("Expected filled Country from ZString", "Country", wrapper.Country);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress = orgHeader.Addresses.AddNew();
			wrapper = PartyAddressWrapper.New(orgAddress);
		}
		OrgHeader orgHeader;
		OrgAddress orgAddress;
		PartyAddressWrapper wrapper;

		AESCommonAddressWrapper GetWrapper(OrgAddress orgAddress) => AESCommonAddressWrapper.New(orgAddress);
		AESCommonAddressWrapper GetWrapper(ZString address, ZString city, ZString postCode, ZString country) => AESCommonAddressWrapper.New(address, city, postCode, country);

		protected override PartyAddressWrapper GetProvider() => wrapper;
	}
}
