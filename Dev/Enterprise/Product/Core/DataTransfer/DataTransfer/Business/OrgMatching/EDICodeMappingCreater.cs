using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business
{
	public class EDICodeMappingCreater
	{
		public void CreateEDICodeMapping(OrgHeader mappingOrg, ZString foreignOrgCode, IOrgHeaderForMatching orgMatch)
		{
			if (mappingOrg == null
				|| foreignOrgCode.IsEmpty
				|| GlbCompany.CurrentCompany == null
				|| orgMatch == null
				|| mappingOrg.PK == GlbCompany.CurrentCompany.GC_OH_OrgProxy)
			{
				return;
			}

			if (CodeMappingAlreadyDefined(mappingOrg, foreignOrgCode))
			{
				return;
			}

			var newOrgMapping = mappingOrg.Factory.New<OrgPatternMatchOverride>();
			using (newOrgMapping.GetValidationSuspender())
			using (newOrgMapping.SuspendSettingHasChanges())
			{
				newOrgMapping.OO_OH = mappingOrg.PK;
				newOrgMapping.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
				newOrgMapping.OO_ForeignCode = foreignOrgCode;
				newOrgMapping.OO_LocalGuid = orgMatch.PK;
			}
		}

		public bool CodeMappingAlreadyDefined(OrgHeader mappingOrg, ZString foreignOrgCode)
		{
			var searchFilter = new ZQuery(OrgPatternMatchOverrideSchema.OO_ForeignCode, foreignOrgCode.ConvertToWesternEuropeanCharacters());
			searchFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Organisation);
			searchFilter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, mappingOrg.PK);
			return mappingOrg.Factory.Exists(typeof(OrgPatternMatchOverride), searchFilter);
		}
	}
}
