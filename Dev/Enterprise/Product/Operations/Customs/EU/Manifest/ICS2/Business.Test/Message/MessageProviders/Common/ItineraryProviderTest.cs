using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ItineraryProviderTest : DataProviderTestCase<ItineraryProvider>
	{
		public void TestConstructor()
		{
			var route = Factory.NewWithValidTestData<RouteEntry>();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>(() => new ItineraryProvider(null));
				AssertNoExceptionThrown(() => new ItineraryProvider(route));
			});
		}

		public void TestOrder()
		{
			routeEntry.CY_Order = 1;
			AssertEquals("Order", (sbyte)1, Provider.Order);
		}

		public void TestCountry()
		{
			routeEntry.CY_Code = "AUSYD";
			AssertEquals("Country", "AU", Provider.Country);

			routeEntry.CY_Data = "SG";
			AssertEquals("Country", "SG", Provider.Country);
		}

		protected override void SetUp()
		{
			base.SetUp();
			routeEntry = Factory.NewWithValidTestData<RouteEntry>();
		}
		RouteEntry routeEntry;

		protected override ItineraryProvider GetProvider()
		{
			return new ItineraryProvider(routeEntry);
		}
	}
}
