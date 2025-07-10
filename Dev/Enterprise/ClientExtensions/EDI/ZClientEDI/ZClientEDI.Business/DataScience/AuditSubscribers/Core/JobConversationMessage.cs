using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class JobConversationMessageSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.JobConversationMessageSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = JobConversationMessageSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			JobConversationMessageSchema.PK,
			JobConversationMessageSchema.JCM_Body,
			JobConversationMessageSchema.JCM_IsBroadcast,
			JobConversationMessageSchema.JCM_IsInternal,
			JobConversationMessageSchema.JCM_IsLocal,
			JobConversationMessageSchema.JCM_IsSystem,
			JobConversationMessageSchema.JCM_JCC_Conversation,
			JobConversationMessageSchema.JCM_JCP_Participant,
			JobConversationMessageSchema.JCM_Language,
			JobConversationMessageSchema.JCM_PostedTimeUtc,
			JobConversationMessageSchema.JCM_Score,
			JobConversationMessageSchema.JCM_SystemCreateTimeUtc,
			JobConversationMessageSchema.JCM_SystemCreateUser,
			JobConversationMessageSchema.JCM_SystemLastEditTimeUtc,
			JobConversationMessageSchema.JCM_SystemLastEditUser,
		};
	}
}
