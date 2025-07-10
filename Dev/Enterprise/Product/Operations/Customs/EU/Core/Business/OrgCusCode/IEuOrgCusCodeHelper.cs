using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public interface IEuOrgCusCodeHelper
	{
		ZString GetEuIdentificationNumber(JobDocAddress docAddress, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		ZString GetEuIdentificationNumber(OrgAddress address, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		ZString GetEuIdentificationNumber(OrgHeader organisation, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		ZString GetEORI(OrgAddress address, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		ZString GetEORI(OrgHeader organisation, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		ZString GetUnprefixedEORI(OrgHeader organisation, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		ZString GetUnprefixedEORI(OrgAddress address, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		string PrivateEoriReg { get; }
		string UnregForEori { get; }
		bool ValidEORIorTCUIFormat(ZString eoriOrTCUI, BusinessObjectFactory factory);
		ZString GetRegoCodeOfThisOrg(OrgHeader organisation, string typeOfCodeEoriTurnEtc, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		ZString GetRegoCodeOfThisAddress(OrgAddress address, string typeOfCodeEoriTurnEtc, string countryOfIssuance = null, bool ignoreCountryOfIssuanceIfNotMatched = false);
		(ZString CountryCode, ZString RegistrationNumber) GetEuIdentificationNumberComponents(OrgHeader organisation);
		ZString GetEUVATCodeOfThisOrg(OrgHeader organisation);
		IEnumerable<ZString> GetEUVATCodesOfThisOrg(OrgHeader organisation);
		ZString GetBranchSuffixesForBox44(CusEntryHeader entryHeader);
		List<EoriSuffixAndType> GetBranchSuffixesListForBox44(CusEntryHeader entryHeader);
	}
}
