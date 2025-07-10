using System;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class CountryGuidsTest : TestCase
	{
		public void TestCountriesUnderUSCustomsJurisdiction()
		{
			AssertContainsExactElementsInAnyOrder(new[] { CountryGuids.Instance.UnitedStates, CountryGuids.Instance.PuertoRico }, CountryGuids.CountriesUnderUSCustomsJurisdiction);
		}

		public void TestAllUSCountriesForEntryFilerID()
		{
			AssertContainsExactElementsInAnyOrder(new[] { CountryGuids.Instance.UnitedStates, CountryGuids.Instance.PuertoRico, CountryGuids.Instance.VirginIslandsUS }, CountryGuids.AllUSCountriesForEntryFilerID);
		}

		public void TestCountriesUnderEUCustomsJurisdiction()
		{
			AssertContainsExactElementsInAnyOrder(euCountries, CountryGuids.CountriesUnderEUCustomsJurisdiction);
		}

		public void TestCountriesUnderEUCustomsJurisdictionExceptUK()
		{
			AssertSequencesEqual(CountryGuids.CountriesUnderEUCustomsJurisdiction.Except(new[] { CountryGuids.Instance.UnitedKingdom }), CountryGuids.CountriesUnderEUCustomsJurisdictionExceptUK);
		}

		public void TestCountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey()
		{
			var countries = euCountries
				.Add(CountryGuids.Instance.Switzerland)
				.Add(CountryGuids.Instance.Norway)
				.Add(CountryGuids.Instance.Turkey);
			AssertContainsExactElementsInAnyOrder(countries, CountryGuids.CountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey);
		}

		static readonly ImmutableHashSet<Guid> euCountries = ImmutableHashSet.Create
		(
			CountryGuids.Instance.Austria,
			CountryGuids.Instance.Belgium,
			CountryGuids.Instance.Bulgaria,
			CountryGuids.Instance.Croatia,
			CountryGuids.Instance.Cyprus,
			CountryGuids.Instance.CzechRepublic,
			CountryGuids.Instance.Denmark,
			CountryGuids.Instance.Germany,
			CountryGuids.Instance.Estonia,
			CountryGuids.Instance.Finland,
			CountryGuids.Instance.France,
			CountryGuids.Instance.Greece,
			CountryGuids.Instance.Hungary,
			CountryGuids.Instance.Ireland,
			CountryGuids.Instance.Italy,
			CountryGuids.Instance.Latvia,
			CountryGuids.Instance.Lithuania,
			CountryGuids.Instance.Luxembourg,
			CountryGuids.Instance.Malta,
			CountryGuids.Instance.Netherlands,
			CountryGuids.Instance.Poland,
			CountryGuids.Instance.Portugal,
			CountryGuids.Instance.Romania,
			CountryGuids.Instance.Spain,
			CountryGuids.Instance.Sweden,
			CountryGuids.Instance.Slovakia,
			CountryGuids.Instance.Slovenia,
			CountryGuids.Instance.UnitedKingdom
		);
	}
}
