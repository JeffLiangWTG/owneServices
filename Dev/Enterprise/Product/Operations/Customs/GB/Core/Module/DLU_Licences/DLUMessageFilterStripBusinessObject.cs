using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Module
{
	public class DLUMessageFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddNumberFilter("Licence Number", EDIMessageSchema.EM_ApplicationReference)
				.MultilingualDescription = ResString.GetMultilingualString("A6867FEB-34FC-45AA-B740-547619DEF103", "License Number");
			result.AddFlagsFilter("Has response", ["Yes"], new GetFlagsQuery[] { GetHasLinkedMessageQuery })
				.MultilingualDescription = ResString.GetMultilingualString("49C41838-ADEB-4ABC-8A97-427C60C0E897", "Has response");
			result.AddTextFilter("Status", EDIMessageSchema.EM_Status)
				.MultilingualDescription = ResString.GetMultilingualString("D5AD9DB6-E5F0-4F26-AD18-5838274EA206", "Status");
			result.AddDateFilter("Date created", EDIMessageSchema.EM_SystemCreateTimeUtc, convertFromLocalToUTC: true)
				.MultilingualDescription = ResString.GetMultilingualString("6D2CB2D5-439B-415E-98F2-3ECE75FBA1AA", "Date created");
			result.AddTextFilter("Badge", EDIMessageSchema.EM_MessageOwner)
				.MultilingualDescription = ResString.GetMultilingualString("0622DABF-F385-4DF8-8355-FB2287822DBC", "Badge");
			result.AddGuidFilter("Organisation", ModuleIDs.Organisation, GetOrganisationQuery, new OrganisationsFindBoxCollection(Factory))
				.MultilingualDescription = ResString.GetMultilingualString("A4A9D255-867B-4607-A6DF-24EE1FEEC849", "Organization");
			return result;
		}

		ZQuery GetOrganisationQuery(ZGuid value)
		{
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			var addOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, value.ToString());
			addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, ChiefConstants.CusDecTypeDLU);
			addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			messageQuery.AddSubQuery(addOnQuery, JoinCondition.And);
			return messageQuery;
		}

		ZQuery GetHasLinkedMessageQuery(ZBool value)
		{
			var comparison = value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			return new ZQuery(EDIMessageSchema.EM_LinkTable, comparison, EDIMessageSchema.Constants.TableName);
		}
	}
}

