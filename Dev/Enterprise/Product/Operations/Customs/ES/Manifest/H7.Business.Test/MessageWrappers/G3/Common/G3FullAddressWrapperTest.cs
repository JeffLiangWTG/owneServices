using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3FullAddressWrapperTest : DataProviderTestCase<G3FullAddressWrapper>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Expected non-null wrapper when declarant is not null", wrapper);

				wrapper = G3FullAddressWrapper.New(null);
				AssertNull("Expected null wrapper when declarant is null", wrapper);
			});
		}

		public void TestStreet()
		{
			AssertEquals("Expected filled Street", "address 1 address 2", wrapper.Street);
		}

		public void TestNumber()
		{
			AssertEquals("Expected filled Number", "address 1 address 2", wrapper.Number);
		}

		public void TestStreetAddLine()
		{
			AssertEquals("Expected empty StreetAddLine", ZString.Empty, wrapper.StreetAddLine);
		}

		public void TestCountry()
		{
			AssertEquals("Expected filled Country", "ES", wrapper.Country);
		}

		public void TestPostCode()
		{
			AssertEquals("Expected filled Post Code", "3053", wrapper.PostCode);
		}

		public void TestCity()
		{
			AssertEquals("Expected filled City", "city", wrapper.City);
		}

		public void TestEmptyFiels()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Street Add Line", string.Empty, wrapper.StreetAddLine);
				AssertEquals("Expected empty PO Box", string.Empty, wrapper.POBox);
				AssertEquals("Expected empty Sub Division", string.Empty, wrapper.SubDivision);
			});
		}

		public void TestPOBox()
		{
			AssertEquals("Expected empty POBox", ZString.Empty, wrapper.POBox);
		}

		public void TestSubDivision()
		{
			AssertEquals("Expected empty Subdivision", ZString.Empty, wrapper.SubDivision);
		}

		protected override void SetUp()
		{
			base.SetUp();

			address = Factory.New<OrgAddress>();
			address.OA_Address1 = "address 1";
			address.OA_Address2 = "address 2";
			address.OA_PostCode = "3053";
			address.City = "city";
			address.OA_RN_NKCountryCode = "ES";
			wrapper = G3FullAddressWrapper.New(address);
		}

		protected override G3FullAddressWrapper GetProvider()
		{
			return wrapper;
		}

		G3FullAddressWrapper wrapper;
		OrgAddress address;
	}
}
