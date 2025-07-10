using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class OriginProviderTest : DataProviderTestCase<OriginProvider>
	{
		public void TestIOrigin()
		{
			Assert("Should implement IOrigin", Provider is IOrigin);
		}

		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Empty, Empty", OriginProvider.New(string.Empty, string.Empty));
				AssertNotNull("Has value, Empty", OriginProvider.New("IE", string.Empty));
				AssertNotNull("Empty, Has value", OriginProvider.New(string.Empty, "FR"));
			});
		}

		public void TestCountryOfOrigin()
		{
			AssertEquals("IE", Provider.CountryOfOrigin);
		}

		public void TestCountryOfPreferentialOrigin()
		{
			AssertEquals("FR", Provider.CountryOfPreferentialOrigin);
		}

		protected override OriginProvider GetProvider()
		{
			return OriginProvider.New("IE", "FR");
		}
	}
}
