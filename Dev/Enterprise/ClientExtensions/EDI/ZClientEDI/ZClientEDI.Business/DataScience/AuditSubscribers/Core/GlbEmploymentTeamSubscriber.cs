using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public sealed class GlbEmploymentTeamSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbEmploymentTeamCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbEmploymentTeamSchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbEmploymentTeamSchema.PK,
			GlbEmploymentTeamSchema.GET_AutoEffectiveEndDate,
			GlbEmploymentTeamSchema.GET_EffectiveDate,
			GlbEmploymentTeamSchema.GET_GCR_ChangeRequest,
			GlbEmploymentTeamSchema.GET_GST_NKTeamCode,
			GlbEmploymentTeamSchema.GET_GS_Staff,
			GlbEmploymentTeamSchema.GET_IsApproved,
			GlbEmploymentTeamSchema.GET_SystemCreateTimeUtc,
			GlbEmploymentTeamSchema.GET_SystemCreateUser,
			GlbEmploymentTeamSchema.GET_SystemLastEditTimeUtc,
			GlbEmploymentTeamSchema.GET_SystemLastEditUser,
		};
	}
}
