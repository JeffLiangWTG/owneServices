using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class FullAddressProviderTest : DataProviderTestCase<FullAddressProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("Argument == null", FullAddressProvider.New(null));
				AssertNotNull("Valid argument", Provider);
			});
		}

		public void TestStreetAndNumber()
		{
			AssertEquals("10 Main Street", Provider.StreetAndNumber);
		}

		public void TestNumber()
		{
			AssertNull(Provider.Number);
		}

		public void TestPostcode()
		{
			AssertEquals("D12 10XX", Provider.Postcode);
		}

		public void TestCity()
		{
			AssertEquals("Dublin", Provider.City);
		}

		public void TestCountry()
		{
			AssertEquals("IE", Provider.Country);
		}

		public void TestSubDivision()
		{
			AssertNull(Provider.SubDivision);
		}

		public void TestStreetAdditionalLine()
		{
			AssertEquals("District 12", Provider.StreetAdditionalLine);
		}

		public void TestPoBox()
		{
			AssertNull(Provider.PoBox);
		}

		protected override FullAddressProvider GetProvider() => FullAddressProvider.New(orgAddress);

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Address1 = "10 Main Street";
			orgAddress.OA_Address2 = "District 12";
			orgAddress.OA_City = "Dublin";
			orgAddress.OA_PostCode = "D12 10XX";
			orgAddress.OA_RN_NKCountryCode = "IE";
		}

		OrgHeader orgHeader;
		OrgAddress orgAddress;
	}
}
