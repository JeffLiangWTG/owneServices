using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public sealed class GlbEmploymentHistorySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbEmploymentHistoryCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbEmploymentHistorySchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbEmploymentHistorySchema.PK,
			GlbEmploymentHistorySchema.GEH_AutoEffectiveEndDate,
			GlbEmploymentHistorySchema.GEH_CompanyName,
			GlbEmploymentHistorySchema.GEH_DepartureComments,
			GlbEmploymentHistorySchema.GEH_DepartureReason,
			GlbEmploymentHistorySchema.GEH_EffectiveDate,
			GlbEmploymentHistorySchema.GEH_EmploymentType,
			GlbEmploymentHistorySchema.GEH_GCR_ChangeRequest,
			GlbEmploymentHistorySchema.GEH_GS_Staff,
			GlbEmploymentHistorySchema.GEH_HJ_JobRole,
			GlbEmploymentHistorySchema.GEH_IsApproved,
			GlbEmploymentHistorySchema.GEH_IsInternalPosition,
			GlbEmploymentHistorySchema.GEH_IsPromotion,
			GlbEmploymentHistorySchema.GEH_JobDescription,
			GlbEmploymentHistorySchema.GEH_JobFamily,
			GlbEmploymentHistorySchema.GEH_JobTitle,
			GlbEmploymentHistorySchema.GEH_SystemCreateTimeUtc,
			GlbEmploymentHistorySchema.GEH_SystemCreateUser,
			GlbEmploymentHistorySchema.GEH_SystemLastEditTimeUtc,
			GlbEmploymentHistorySchema.GEH_SystemLastEditUser,
			GlbEmploymentHistorySchema.GEH_WorksOutsideBranch,
		};
	}
}
