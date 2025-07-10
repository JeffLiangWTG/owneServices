using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonAddressInfoWrapperTest : WrapperHelperTest<NCTS5CommonAddressInfoWrapper>
	{
		public void TestGetNewNCTS5CommonAddressInfoWrapper()
		{
			CombineAssertions(() =>
			{
				OrgAddress orgAddress = null;
				AssertNull("OrgAddress null", GetWrapper(orgAddress));
				orgAddress = Factory.New<OrgAddress>();
				AssertNotNull("OrgAddress not null", GetWrapper(orgAddress));

				JobDocAddress jobDocAddress = null;
				AssertNull("JobDocAddress null", NCTS5CommonAddressInfoWrapper.New(jobDocAddress));
				jobDocAddress = Factory.New<JobDocAddress>();
				AssertNull("JobDocAddress not null but null Header", NCTS5CommonAddressInfoWrapper.New(jobDocAddress));
				var orgHeader1 = Factory.New<OrgHeader>();
				jobDocAddress.OrganisationPK = orgHeader1.PK;
				AssertNotNull("JobDocAddress not null and no null Header", NCTS5CommonAddressInfoWrapper.New(jobDocAddress));
			});
		}

		public void TestAddressFields()
		{
			CombineAssertions(() =>
			{
				wrapper = GetWrapperJobDocAddress(docAddress, isInPhase5TransitionPeriod: true);
				var address = wrapper;
				AssertNotNull("Expected filled Address when Id is PAS and category is NAT", address);
				AssertSame("Cached Address", wrapper, address);

				orgAddress.Address1 = "Address";
				orgAddress.Postcode = "PostCode12";
				orgAddress.City = "City";
				wrapper = GetWrapperJobDocAddress(docAddress, true, true);
				address = wrapper;
				AssertEquals("Expected Address when id declared PAS and not Override", "Address", address.StreetAndNumber);
				AssertEquals("Expected Post Code when id declared PAS and not Override and not Trimmed", "PostCode12", address.PostCode);
				AssertEquals("Expected City when id declared PAS and not Override", "City", address.City);

				putOverrideToTrue();
				wrapper = GetWrapperJobDocAddress(docAddress, isInPhase5TransitionPeriod: true);
				address = wrapper;
				AssertEquals("TransitionalPeriod: Expected Address when id declared PAS and is Override and trim into 35 chrs", "Address Override 012345678901234567", address.StreetAndNumber);
				AssertEquals("TransitionalPeriod: Expected Post Code when id declared PAS and is Override trim into 9 chrs", "Post Over", address.PostCode);
				AssertEquals("Expected City when id declared PAS and is Override", "City Over", address.City);

				wrapper = GetWrapperJobDocAddress(docAddress);
				address = wrapper;
				AssertEquals("FinalPeriod: Expected Address trim into 75 chrs", "Address Override 012345678901234567890123456789012", address.StreetAndNumber);
				AssertEquals("FinalPeriod: Expected Post Code trim into 17 chrs", "Post Over0", address.PostCode);
			});
		}

		public void TestCountry()
		{
			orgAddress.OA_RN_NKCountryCode = "ES";
			wrapper = GetWrapper(orgAddress);
			AssertEquals("Expected filled Country from dbo.OrgAddress", "ES", wrapper.Country);
		}

		protected override void SetUp()
		{
			base.SetUp();
			docAddress = Factory.New<JobDocAddress>();
			orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			wrapper = GetWrapper(orgAddress);
		}
		JobDocAddress docAddress;
		OrgAddress orgAddress;
		NCTS5CommonAddressInfoWrapper wrapper;

		void putOverrideToTrue()
		{
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "Name Override";
			docAddress.E2_Address1AndE2_Address2 = "Address Override 012345678901234567890123456789012";
			docAddress.E2_Postcode = "Post Over0";
			docAddress.E2_City = "City Over";
		}

		NCTS5CommonAddressInfoWrapper GetWrapper(OrgAddress orgAddress) => NCTS5CommonAddressInfoWrapper.New(orgAddress);

		NCTS5CommonAddressInfoWrapper GetWrapperJobDocAddress(JobDocAddress jobDocAddress, bool trimPostCode = false, bool isInPhase5TransitionPeriod = false) => NCTS5CommonAddressInfoWrapper.New(jobDocAddress, trimPostCode, isInPhase5TransitionPeriod);

		protected override NCTS5CommonAddressInfoWrapper GetProvider() => wrapper;
	}
}
