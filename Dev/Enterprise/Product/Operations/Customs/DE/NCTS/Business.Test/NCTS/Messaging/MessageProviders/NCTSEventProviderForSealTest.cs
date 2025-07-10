using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSEventProviderForSealTest : Customs.Business.Testing.DataProviderTestCase<NCTSEventProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentException>(() => NCTSEventProvider.New((EnRouteSeal)null));
		}

		public void TestPlace()
		{
			AssertEquals("Entered", "place", dataProvider.Place);
		}

		public void TestCountry()
		{
			AssertEquals("Entered", Core.Constants.CountryCodes.Germany, dataProvider.Country);
		}

		public void TestIncident() => AssertNull(dataProvider.Incident);

		public void TestTranshipment() => AssertNull(dataProvider.Transhipment);

		public void TestSeals() => AssertNotNull(dataProvider.Seals);

		protected override void SetUp()
		{
			base.SetUp();
			seal = Factory.New<EnRouteSeal>();
			seal.BN_EventPlace = "place";
			seal.BN_EventCountryCode = Core.Constants.CountryCodes.Germany;
			dataProvider = NCTSEventProvider.New(seal);
		}
		EnRouteSeal seal;
		INCTSEvent dataProvider;

		protected override NCTSEventProvider GetProvider() => (NCTSEventProvider)dataProvider;
	}
}
