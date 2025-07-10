using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class EndorsmentProviderTest : Customs.Business.Testing.DataProviderTestCase<EndorsmentProvider>
	{
		public void TestDate()
		{
			CombineAssertions(() =>
			{
				incident.BN_EndorsementDate = System.DateTime.FromOADate(1234);
				AssertEquals(System.DateTime.FromOADate(1234), Provider.Date);

				incident.BN_EndorsementDate = ZDateTime.Empty;
				AssertEquals("Date should be MinValue from Empty", DateTime.MinValue, Provider.Date);

				incident.BN_EndorsementDate = new ZDateTime(DateTime.MinValue);
				AssertEquals("Date should be MinValue from MinValue", DateTime.MinValue, Provider.Date);

				incident.BN_EndorsementDate = new ZDateTime(1994, 2, 1, 21, 48, 20, 789);
				AssertEquals("No Milliseconds", 0, Provider.Date.Millisecond);
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
			incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("GB", Provider.Country);
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
