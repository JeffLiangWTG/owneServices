using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class UNLOCOProviderTest : DataProviderTestCase<UNLOCOProvider>
	{
		public void TestUnlocode()
		{
			var provider = new UNLOCOProvider("UNLOCODE", "Country");
			AssertEquals("UNLOCODE", provider.Unlocode);
		}

		public void TestLocation()
		{
			AssertNull("Location", Provider.Location);
		}

		public void TestCountry()
		{
			var provider = new UNLOCOProvider("UNLOCODE", "Country");
			AssertEquals("Country", provider.Country);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			asycudaBill = header.Bills.AddNew();
		}
		AsycudaBill asycudaBill;

		protected sealed override UNLOCOProvider GetProvider()
		{
			return new UNLOCOProvider(asycudaBill.ABL_RL_NKFinalDestination, string.Empty);
		}
	}
}
