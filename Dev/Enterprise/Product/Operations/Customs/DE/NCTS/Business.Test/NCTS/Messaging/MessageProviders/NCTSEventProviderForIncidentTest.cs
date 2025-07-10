using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSEventProviderForIncidentTest : Customs.Business.Testing.DataProviderTestCase<NCTSEventProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentException>(() => NCTSEventProvider.New((EnRouteIncident)null));
		}

		public void TestPlace()
		{
			AssertEquals("Entered", "place", dataProvider.Place);
		}

		public void TestCountry()
		{
			AssertEquals("Entered", Core.Constants.CountryCodes.Germany, dataProvider.Country);
		}

		public void TestIncident() => AssertNotNull(dataProvider.Incident);

		public void TestTranshipment() => AssertNull(dataProvider.Transhipment);

		public void TestSeals() => AssertNull(dataProvider.Seals);

		protected override void SetUp()
		{
			base.SetUp();
			incident = Factory.New<EnRouteIncident>();
			incident.BN_EventPlace = "place";
			incident.BN_EventCountryCode = Core.Constants.CountryCodes.Germany;
			dataProvider = NCTSEventProvider.New(incident);
		}
		EnRouteIncident incident;
		INCTSEvent dataProvider;

		protected override NCTSEventProvider GetProvider() => (NCTSEventProvider)dataProvider;
	}
}
