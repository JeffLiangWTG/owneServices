using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Issues
{
	public sealed class HelpErrorLogSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Issues>
	{
		public override string Code => SubscriberCodes.HelpErrorLogSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => HelpErrorLogSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			HelpErrorLogSchema.PK,
			HelpErrorLogSchema.HE_ExceptionMessage,
			HelpErrorLogSchema.HE_ExceptionSource,
			HelpErrorLogSchema.HE_ExceptionType,
			HelpErrorLogSchema.HE_FailCount,
			HelpErrorLogSchema.HE_FirstEXEVersionDate,
			HelpErrorLogSchema.HE_FirstProcessed,
			HelpErrorLogSchema.HE_FirstReported,
			HelpErrorLogSchema.HE_FirstVersionNumber,
			HelpErrorLogSchema.HE_FixedCount,
			HelpErrorLogSchema.HE_FixedDate,
			HelpErrorLogSchema.HE_IsClientVisible,
			HelpErrorLogSchema.HE_IssueNumber,
			HelpErrorLogSchema.HE_LastEXEVersionDate,
			HelpErrorLogSchema.HE_LastReported,
			HelpErrorLogSchema.HE_LastVersionNumber,
			HelpErrorLogSchema.HE_LogType,
			HelpErrorLogSchema.HE_SystemCreateTimeUtc,
			HelpErrorLogSchema.HE_SystemCreateUser,
			HelpErrorLogSchema.HE_SystemLastEditTimeUtc,
			HelpErrorLogSchema.HE_SystemLastEditUser
		};
	}
}
