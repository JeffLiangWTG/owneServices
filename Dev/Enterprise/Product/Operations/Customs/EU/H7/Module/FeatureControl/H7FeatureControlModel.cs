using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.EU.H7.Module;

public record H7FeatureControlModel
{
	public List<string> AuthorizedCountries { get; set; }

	public List<H7AuthorizedCompanyGroup> AuthorizedCompanies { get; set; }
}

public record H7AuthorizedCompanyGroup
{
	public string CountryCode { get; set; }

	public List<string> CompanyCodes { get; set; }
}

static class H7AuthorizationDataExtensions
{
	internal static bool IsAuthorized(this H7FeatureControlModel data, string userCountryCode, string userCompanyCode)
	{
		if (data.AuthorizedCountries != null && data.AuthorizedCountries.Contains(userCountryCode))
		{
			return true;
		}

		var userCountryGroup = data.AuthorizedCompanies?.FirstOrDefault(g => g.CountryCode == userCountryCode);
		return userCountryGroup != null && userCountryGroup.CompanyCodes.Contains(userCompanyCode);
	}
}
