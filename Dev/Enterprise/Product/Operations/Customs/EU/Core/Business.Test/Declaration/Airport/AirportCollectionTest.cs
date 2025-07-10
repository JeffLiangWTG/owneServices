using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(AirportCollection))]
	public class AirportCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AirportCollection(Factory, true);
		}

		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(AirportCollection);
		}

		public void TestAirportCollectionEU()
		{
			CombineAssertions(() =>
			{
				var airportEU = new AirportCollection(Factory, "FRCDG", isNonEuAirport: false);
				AssertContains("It is expected that the check will be with IN", "RL_RN_NKCountryCode IN", airportEU.CompleteFilter.FilterString);
				AssertContains("It is expected that the check will be with (AND RL_Code)", "AND RL_Code", airportEU.CompleteFilter.FilterString);
				airportEU = new AirportCollection(Factory, "FRCDG", isNonEuAirport: true);
				AssertContains("It is expected that the check will be with NOT IN", "RL_RN_NKCountryCode NOT IN", airportEU.CompleteFilter.FilterString);
				AssertContains("It is expected that the check will be with (AND RL_Code)", "AND RL_Code", airportEU.CompleteFilter.FilterString);

				airportEU = new AirportCollection(Factory, isNonEuAirport: false);
				AssertContains("It is expected that the check will be with IN", "RL_RN_NKCountryCode IN", airportEU.CompleteFilter.FilterString);
				AssertNotContains("It is expected that the check will be without (AND RL_Code)", "AND RL_Code", airportEU.CompleteFilter.FilterString);
				airportEU = new AirportCollection(Factory, isNonEuAirport: true);
				AssertContains("It is expected that the check will be with NOT IN", "RL_RN_NKCountryCode NOT IN", airportEU.CompleteFilter.FilterString);
				AssertNotContains("It is expected that the check will be without (AND RL_Code)", "AND RL_Code", airportEU.CompleteFilter.FilterString);
			});
		}
	}
}
