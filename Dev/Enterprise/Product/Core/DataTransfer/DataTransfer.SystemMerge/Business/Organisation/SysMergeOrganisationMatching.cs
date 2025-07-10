using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.SystemMerge.Business
{
	class SysMergeOrganisationMatching : IOrganisationMatching
	{
		#region IOrganisationMatching Members

		public IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			return null;
		}

		public IOrgHeaderForMatching FindOrCreateTempOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			return null;
		}

		public IOrgAddressSorter OrgAddressSorter { get; set; }

		public ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			return FindOrCreateTempOrganisationPK(value, sourceObject, orgType, new UnmatchOrgRecordCriteria());
		}

		public ZGuid FindOrCreateTempOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType, UnmatchOrgRecordCriteria unmatchOrgRecordCriteria)
		{
			return ZGuid.Empty;
		}

		public OrgHeader FindOrganisation(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			return null;
		}

		public ZGuid FindOrganisationPK(IValueObject value, BusinessObject sourceObject, OrganisationTypes orgType)
		{
			return ZGuid.Empty;
		}

		public OrgMatchingResult Match(IValueObject matchingCriteria, OrganisationTypes orgTypes, bool createTemporaryOrUnmatchOrgIfNoMatchFound)
		{
			return new OrgMatchingResult(null, false, false, null);
		}

		public OrgMatchingResult Match(IValueObject orgValue, OrganisationTypes orgTypes)
		{
			return new OrgMatchingResult(null, false, false, null);
		}

		#endregion
	}
}
