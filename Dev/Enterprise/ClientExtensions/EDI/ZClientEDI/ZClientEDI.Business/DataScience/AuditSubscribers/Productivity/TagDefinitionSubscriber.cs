using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class TagDefinitionSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.TagDefinitionSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = TagDefinitionSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			TagDefinitionSchema.PK,
			TagDefinitionSchema.TGD_Code,
			TagDefinitionSchema.TGD_Description,
			TagDefinitionSchema.TGD_IsActive,
			TagDefinitionSchema.TGD_IsExclusive,
			TagDefinitionSchema.TGD_IsSystem,
			TagDefinitionSchema.TGD_Scope,
			TagDefinitionSchema.TGD_SystemCreateTimeUtc,
			TagDefinitionSchema.TGD_SystemCreateUser,
			TagDefinitionSchema.TGD_SystemLastEditTimeUtc,
			TagDefinitionSchema.TGD_SystemLastEditUser,
			TagDefinitionSchema.TGD_UsageScope,
		};
	}
}
