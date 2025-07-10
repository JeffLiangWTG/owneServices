using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(EndorsmentProvider))]
	sealed class EndorsmentProviderTest : Customs.Business.Testing.DataProviderTestCase<EndorsmentProvider>
	{
		public void TestDate()
		{
			CombineAssertions(() =>
			{
				incident.BN_EndorsementDate = System.DateTime.FromOADate(1234);
				AssertEquals(System.DateTime.FromOADate(1234), Provider.Date);

				incident.BN_EndorsementDate = ZDateTime.Empty;
				AssertEquals("Date should be MinValue from Empty", System.DateTime.MinValue, Provider.Date);

				incident.BN_EndorsementDate = new ZDateTime(System.DateTime.MinValue);
				AssertEquals("Date should be MinValue from MinValue", System.DateTime.MinValue, Provider.Date);
			});
		}

		public void TestAuthority()
		{
			incident.BN_EndorsementAuthority = "ABC";
			AssertEquals("ABC", Provider.Authority);
		}

		public void TestPlace()
		{
			incident.BN_EndorsementPlace = "place";
			AssertEquals("place", Provider.Place);
		}

		public void TestCountry()
		{
			incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Belgium;
			AssertEquals("BE", Provider.Country);
		}

		protected override EndorsmentProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			incident = header.EnRouteIncidents.AddNew();
			provider = new EndorsmentProvider(incident);
		}

		EndorsmentProvider provider;
		EnRouteIncident incident;
	}
}
