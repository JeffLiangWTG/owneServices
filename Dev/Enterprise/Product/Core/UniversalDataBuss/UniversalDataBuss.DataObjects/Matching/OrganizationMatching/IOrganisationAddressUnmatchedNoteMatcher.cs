using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public interface IOrganisationAddressUnmatchedNoteMatcher
	{
		bool IsMatchToOrgInUnmatchedNote(OrganizationAddress organisationAddress, BusinessObject matchingBO, OrganisationTypes unmatchedOrgNoteType, string unmatchedOrgNoteSubType);
	}
}
