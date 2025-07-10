using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class JobConversationParticipantSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.JobConversationParticipantSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = JobConversationParticipantSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			JobConversationParticipantSchema.PK,
			JobConversationParticipantSchema.JCP_EmailAddress,
			JobConversationParticipantSchema.JCP_IsSubscribed,
			JobConversationParticipantSchema.JCP_JCC_Conversation,
			JobConversationParticipantSchema.JCP_ParticipantID,
			JobConversationParticipantSchema.JCP_ParticipantTableCode,
			JobConversationParticipantSchema.JCP_Relation,
			JobConversationParticipantSchema.JCP_SystemCreateTimeUtc,
			JobConversationParticipantSchema.JCP_SystemCreateUser,
			JobConversationParticipantSchema.JCP_SystemLastEditTimeUtc,
			JobConversationParticipantSchema.JCP_SystemLastEditUser,
		};
	}
}
