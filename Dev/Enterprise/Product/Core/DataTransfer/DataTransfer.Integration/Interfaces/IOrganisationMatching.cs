
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.Integration
{
	public interface IOrganisationMatching
	{
		OrgMatchingResult Match(IValueObject orgValue, OrganisationTypes orgTypes);
		OrgMatchingResult Match(IValueObject matchingCriteria, OrganisationTypes orgTypes, bool createTemporaryOrUnmatchOrgIfNoMatchFound);
		ZGuid FindOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType);
		OrgHeader FindOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType);
		ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType);

		/// <summary>
		/// OrgSubType value of UnmatchOrgRecordCriterial should be the Description of OrganisationsSubTypeList
		/// </summary>
		ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriterial);
		IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType);
		IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriterial);

		IOrgAddressSorter OrgAddressSorter { get; set; }
	}
}
