using System.Collections.Generic;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IEuropeanUnionCustomsMembersProvider
			{
				string[] GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers();
				string[] GetEuropeanUnionAndCtCountries();
				IEnumerable<string> GetCountriesInEuropeanCustomsUnionOrInheritsFromEU();
				bool IsInEuropeanCustomsUnion(string countryCode);
				bool IsMemberOfEU(string countryCode);
				bool IsCountryEuOrCtCountry(string countryCode);
				bool IsInEuropeanCustomsUnionOrInheritsFromEU(string countryCode);
			}
		}
	}
}
