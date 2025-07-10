using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonAddressWrapperTest : WrapperHelperTest<AESCommonAddressWrapper>
	{
		public void TestGetNewAESCommonAddressWrapper()
		{
			CombineAssertions(() =>
			{
				OrgAddress orgAddress = null;
				AssertNull("OrgAddress null", GetWrapper(orgAddress));
				orgAddress = Factory.New<OrgAddress>();
				AssertNotNull("OrgAddress not null", GetWrapper(orgAddress));

				AssertNotNull("Not null even if all arguments are empty", GetWrapper(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty));
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

				orgAddress.OA_Address1 = "importer address complete aaaaaaaaabbb";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled Address with all text when isProvisionalPeriod is false", "importer address complete aaaaaaaaabbb", wrapper.Address);
				wrapper = GetWrapper(orgAddress, true);
				AssertEquals("Expected filled Address with only 35 chars when isProvisionalPeriod is true", "importer address complete aaaaaaaaa", wrapper.Address);
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

				orgAddress.OA_PostCode = "0123456789";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled PostCode with all text when isProvisionalPeriod is false", "0123456789", wrapper.PostCode);
				wrapper = GetWrapper(orgAddress, true);
				AssertEquals("Expected filled PostCode with only 9 chars when isProvisionalPeriod is true", "012345678", wrapper.PostCode);
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
			orgAddress = Factory.New<OrgAddress>();
			wrapper = GetWrapper(orgAddress);
		}
		OrgAddress orgAddress;
		AESCommonAddressWrapper wrapper;

		AESCommonAddressWrapper GetWrapper(OrgAddress orgAddress, bool isProvisionalPeriod = false) => AESCommonAddressWrapper.New(orgAddress, isProvisionalPeriod);
		AESCommonAddressWrapper GetWrapper(ZString address, ZString city, ZString postCode, ZString country, bool isProvisionalPeriod = false) => AESCommonAddressWrapper.New(address, city, postCode, country, isProvisionalPeriod);

		protected override AESCommonAddressWrapper GetProvider() => wrapper;
	}
}
