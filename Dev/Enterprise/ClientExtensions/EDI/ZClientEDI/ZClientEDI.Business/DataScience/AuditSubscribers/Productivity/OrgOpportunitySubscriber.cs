using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class OrgOpportunitySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.OrgOpportunitySubscriberCode;
		public override int DataSchemaVersion => 2; // Bump this if the schema changes
		public override ITableSchema Table { get; } = OrgOpportunitySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			OrgOpportunitySchema.PK,
			OrgOpportunitySchema.P8_CloseCertainty,
			OrgOpportunitySchema.P8_ClosedDate,
			OrgOpportunitySchema.P8_DateForExchangeRate,
			OrgOpportunitySchema.P8_DiscountAmount,
			OrgOpportunitySchema.P8_EstimatedCloseDate,
			OrgOpportunitySchema.P8_EstimatedValue,
			OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson,
			OrgOpportunitySchema.P8_LostReason,
			OrgOpportunitySchema.P8_OpportunityDescription,
			OrgOpportunitySchema.P8_OpportunityNotes,
			OrgOpportunitySchema.P8_OpportunityType,
			OrgOpportunitySchema.P8_Outcome,
			OrgOpportunitySchema.P8_PackageType,
			OrgOpportunitySchema.P8_RecallDate,
			OrgOpportunitySchema.P8_RentalMultiplier,
			OrgOpportunitySchema.P8_RX_NKEstimatedValueCurrency,
			OrgOpportunitySchema.P8_Source,
			OrgOpportunitySchema.P8_SourceDetails,
			OrgOpportunitySchema.P8_Stage,
			OrgOpportunitySchema.P8_G0,
			OrgOpportunitySchema.P8_GC,
			OrgOpportunitySchema.P8_O1_Enquiry,
			OrgOpportunitySchema.P8_OA,
			OrgOpportunitySchema.P8_OA_AssignedOffice,
			OrgOpportunitySchema.P8_OC,
			OrgOpportunitySchema.P8_OC_AssignedOfficeContact,
			OrgOpportunitySchema.P8_OC_ReferringContact,
			OrgOpportunitySchema.P8_OH,
			OrgOpportunitySchema.P8_OH_ReferringOrganisation,
			OrgOpportunitySchema.P8_OpportunityID,
			OrgOpportunitySchema.P8_Status,
			OrgOpportunitySchema.P8_SystemCreateTimeUtc,
			OrgOpportunitySchema.P8_SystemCreateUser,
			OrgOpportunitySchema.P8_SystemCreateBranch,
			OrgOpportunitySchema.P8_SystemCreateDepartment,
			OrgOpportunitySchema.P8_SystemLastEditTimeUtc,
			OrgOpportunitySchema.P8_SystemLastEditUser
		};
	}
}
