using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonAddressWrapperTest : WrapperHelperTest<NCTS5CommonAddressWrapper>
	{
		public void TestGetNewAESCommonAddressWrapper()
		{
			CombineAssertions(() =>
			{
				OrgAddress orgAddress = null;
				AssertNull("OrgAddress null", GetWrapper(orgAddress));
				orgAddress = Factory.New<OrgAddress>();
				AssertNotNull("OrgAddress not null", GetWrapper(orgAddress));

				AssertNotNull("Not null even if all arguments are empty", GetWrapper(ZString.Empty, ZString.Empty, ZString.Empty));
			});
		}

		public void TestStreetAndNumber()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_Address1 = "1234567890123456789012345678 Test Street";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled StreetAndNumber from dbo.OrgAddress", "1234567890123456789012345678 Test Street", wrapper.StreetAndNumber);

				wrapper = GetWrapper("Address", ZString.Empty, ZString.Empty);
				AssertEquals("Expected filled StreetAndNumber from ZString", "Address", wrapper.StreetAndNumber);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_City = "Barcelona";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled City from dbo.OrgAddress", "Barcelona", wrapper.City);

				wrapper = GetWrapper(ZString.Empty, "City", ZString.Empty);
				AssertEquals("Expected filled City from ZString", "City", wrapper.City);
			});
		}

		public void TestPostCode()
		{
			CombineAssertions(() =>
			{
				orgAddress.OA_PostCode = "1234567890";
				wrapper = GetWrapper(orgAddress);
				AssertEquals("Expected filled PostCode from dbo.OrgAddress", "1234567890", wrapper.PostCode);

				wrapper = GetWrapper(ZString.Empty, ZString.Empty, "PostCode");
				AssertEquals("Expected filled PostCode from ZString", "PostCode", wrapper.PostCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgAddress = Factory.New<OrgAddress>();
			wrapper = GetWrapper(orgAddress);
		}
		OrgAddress orgAddress;
		NCTS5CommonAddressWrapper wrapper;

		NCTS5CommonAddressWrapper GetWrapper(OrgAddress orgAddress) => NCTS5CommonAddressWrapper.New(orgAddress, false);
		NCTS5CommonAddressWrapper GetWrapper(ZString address, ZString city, ZString postCode) => NCTS5CommonAddressWrapper.New(address, city, postCode, false);

		protected override NCTS5CommonAddressWrapper GetProvider() => wrapper;
	}
}
