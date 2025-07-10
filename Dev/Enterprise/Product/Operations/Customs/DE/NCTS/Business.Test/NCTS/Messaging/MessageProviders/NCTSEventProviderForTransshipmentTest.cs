using System;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSEventProviderForTransshipmentTest : Customs.Business.Testing.DataProviderTestCase<NCTSEventProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentException>(() => NCTSEventProvider.New((EnRouteTransshipment)null));
		}

		public void TestPlace_Empty()
		{
			AssertEquals("Entered", "place", dataProvider.Place);
		}

		public void TestCountry()
		{
			AssertEquals("Entered", Core.Constants.CountryCodes.Germany, dataProvider.Country);
		}

		public void TestIncident() => AssertNull(dataProvider.Incident);

		public void TestTranshipment() => AssertNotNull(dataProvider.Transhipment);

		public void TestSeals() => AssertNull(dataProvider.Seals);

		protected override void SetUp()
		{
			base.SetUp();
			transshipment = Factory.New<EnRouteTransshipment>();
			transshipment.BN_EventPlace = "place";
			transshipment.BN_EventCountryCode = Core.Constants.CountryCodes.Germany;
			dataProvider = NCTSEventProvider.New(transshipment);
		}
		EnRouteTransshipment transshipment;
		INCTSEvent dataProvider;

		protected override NCTSEventProvider GetProvider() => (NCTSEventProvider)dataProvider;
	}
}
