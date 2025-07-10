using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class TagLinkSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.TagLinkSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = TagLinkSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			TagLinkSchema.PK,
			TagLinkSchema.TGL_Description,
			TagLinkSchema.TGL_GS_NKRemovedBy,
			TagLinkSchema.TGL_Magnitude,
			TagLinkSchema.TGL_ParentId,
			TagLinkSchema.TGL_ParentTableCode,
			TagLinkSchema.TGL_RemovedTimeUtc,
			TagLinkSchema.TGL_Sequence,
			TagLinkSchema.TGL_SystemCreateTimeUtc,
			TagLinkSchema.TGL_SystemCreateUser,
			TagLinkSchema.TGL_SystemLastEditTimeUtc,
			TagLinkSchema.TGL_SystemLastEditUser,
			TagLinkSchema.TGL_TGM_Magnitude,
		};
	}
}
