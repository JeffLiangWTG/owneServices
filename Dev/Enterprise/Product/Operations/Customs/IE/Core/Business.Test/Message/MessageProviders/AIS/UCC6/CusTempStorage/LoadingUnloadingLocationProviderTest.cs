using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class LoadingUnloadingLocationProviderTest : DataProviderTestCase<LoadingUnloadingLocationProvider>
	{
		public void TestConstructor()
		{
			AssertNull("Should return null with null parameter.", LoadingUnloadingLocationProvider.New(null));
		}

		public void TestLocation()
		{
			AssertNull("Location", Provider.Location);
		}

		public void TestUNLOCODE()
		{
			AssertEquals("UNLOCODE", "ABC", Provider.UNLOCODE);
		}

		public void TestCountry()
		{
			AssertEquals("Country", "IE", Provider.Country);
		}

		protected override LoadingUnloadingLocationProvider GetProvider()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ABC";
			unloco.RL_RN_NKCountryCode = "IE";
			return LoadingUnloadingLocationProvider.New(unloco);
		}
	}
}
