using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class FrOrgCusCodeHelper : EuOrgCusCodeHelper
	{
		protected override ZString GetEORICodeCore(OrgHeader organisation, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched, bool appendCountryCodePrefix)
		{
			var result = base.GetEORICodeCore(organisation, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, appendCountryCodePrefix);
			if (ShouldAppendSuffix(organisation, appendCountryCodePrefix, result))
			{
				var frEoriSuffix = GetFrEoriSuffixMatchingOrganisation(organisation);
				if (!frEoriSuffix.IsEmpty)
				{
					result += frEoriSuffix;
				}
			}
			return result;
		}

		protected override ZString GetEORICodeCore(OrgAddress address, string countryOfIssuance, bool ignoreCountryOfIssuanceIfNotMatched, bool appendCountryCodePrefix)
		{
			var result = base.GetEORICodeCore(address, countryOfIssuance, ignoreCountryOfIssuanceIfNotMatched, appendCountryCodePrefix);
			if (ShouldAppendSuffix(address.Header, appendCountryCodePrefix, result))
			{
				var frEoriSuffix = GetFrEoriSuffixMatchingAddress(address);
				if (frEoriSuffix.IsEmpty)
				{
					frEoriSuffix = GetFrEoriSuffixMatchingOrganisation(address.Header);
				}
				if (!frEoriSuffix.IsEmpty)
				{
					result += frEoriSuffix;
				}
			}
			return result;
		}

		bool ShouldAppendSuffix(OrgHeader organisation, bool appendCountryCodePrefix, ZString foundEori) => appendCountryCodePrefix && foundEori.StartsWith(Constants.CountryCodes.France) || !appendCountryCodePrefix && foundEori == base.GetOrgCusCode(organisation, null, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Constants.CountryCodes.France, false);

		protected override bool UseCurrentLoggedInCountryWhenNotSpecified => true;

		ZString GetFrEoriSuffixMatchingOrganisation(OrgHeader organisationHeader) => organisationHeader.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, Constants.CountryCodes.France, ZGuid.Empty);

		ZString GetFrEoriSuffixMatchingAddress(OrgAddress address) => address.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, Constants.CountryCodes.France, address.PK);
	}
}
