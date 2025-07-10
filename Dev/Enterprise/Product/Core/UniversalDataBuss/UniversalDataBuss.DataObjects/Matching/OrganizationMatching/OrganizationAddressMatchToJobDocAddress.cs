using CargoWise.Application;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public interface IOrganizationAddressMatchToJobDocAddress : IOrganizationAddressMatch { }

	public static class OrganizationAddressMatchToJobDocAddress
	{
		public static IOrganizationAddressMatchToJobDocAddress New(MatchableOrganizationType organisationAddressType, DocAddressType jobDocAddressType, OrganizationAddressMatchPool organisationMatcher, OrganisationTypes unmatchedOrgNoteType, string unmatchedOrgNoteSubType)
		{
			return ObjectFactory.New<IOrganizationAddressMatchToJobDocAddress>(organisationAddressType, jobDocAddressType, organisationMatcher, unmatchedOrgNoteType, unmatchedOrgNoteSubType);
		}
	}
}
