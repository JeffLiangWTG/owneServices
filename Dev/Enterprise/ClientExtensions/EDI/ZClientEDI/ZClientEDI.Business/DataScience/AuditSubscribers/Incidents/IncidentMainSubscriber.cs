using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentMainSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override string Code { get; } = SubscriberCodes.IncidentMainSubscriberCode;
		public override int DataSchemaVersion => 8; // Bump this if the schema changes
		public override ITableSchema Table { get; } = IncidentMainSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			IncidentMainSchema.PK,
			IncidentMainSchema.IM_BugFixDeployed,
			IncidentMainSchema.IM_Category,
			IncidentMainSchema.IM_ChargableWork,
			IncidentMainSchema.IM_ClientBugSeverity,
			IncidentMainSchema.IM_CloseTimeUtc,
			IncidentMainSchema.IM_ClosureResolution,
			IncidentMainSchema.IM_DefectDisposition,
			IncidentMainSchema.IM_DefectStatus,
			IncidentMainSchema.IM_DepositAmountRequired,
			IncidentMainSchema.IM_Description,
			IncidentMainSchema.IM_FeatureRequestCost,
			IncidentMainSchema.IM_FeatureRequestDisposition,
			IncidentMainSchema.IM_FeatureRequestIndustryValue,
			IncidentMainSchema.IM_FeatureRequestStatus,
			IncidentMainSchema.IM_FeatureRequestType,
			IncidentMainSchema.IM_GC,
			IncidentMainSchema.IM_GG_Team,
			IncidentMainSchema.IM_IMT_Triage,
			IncidentMainSchema.IM_INC_Request,
			IncidentMainSchema.IM_IncidentNumber,
			IncidentMainSchema.IM_IncidentType,
			IncidentMainSchema.IM_Language,
			IncidentMainSchema.IM_Module,
			IncidentMainSchema.IM_OA_BranchAddress,
			IncidentMainSchema.IM_OC_Contact,
			IncidentMainSchema.IM_OH_Client,
			IncidentMainSchema.IM_PatchTo,
			IncidentMainSchema.IM_Priority,
			IncidentMainSchema.IM_Product,
			IncidentMainSchema.IM_ProgramArea,
			IncidentMainSchema.IM_QuoteAmount,
			IncidentMainSchema.IM_ResolutionCode,
			IncidentMainSchema.IM_ResolveTimeUtc,
			IncidentMainSchema.IM_RN_NKCountry,
			IncidentMainSchema.IM_RX_NKQuoteCurrency,
			IncidentMainSchema.IM_ServiceType,
			IncidentMainSchema.IM_Source,
			IncidentMainSchema.IM_SourceModuleId,
			IncidentMainSchema.IM_Status,
			IncidentMainSchema.IM_SubCategory,
			IncidentMainSchema.IM_SystemCreateTimeUtc,
			IncidentMainSchema.IM_SystemCreateUser,
			IncidentMainSchema.IM_SystemCreateBranch,
			IncidentMainSchema.IM_SystemCreateDepartment,
			IncidentMainSchema.IM_SystemLastEditTimeUtc,
			IncidentMainSchema.IM_SystemLastEditUser,
			IncidentMainSchema.IM_UpgradeAssuranceAccepted,
			IncidentMainSchema.IM_WorkItemType,
		};
	}
}
