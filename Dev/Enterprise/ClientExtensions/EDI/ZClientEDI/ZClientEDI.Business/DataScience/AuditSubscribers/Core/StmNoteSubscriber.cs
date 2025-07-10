using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class StmNoteSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.StmNoteSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = StmNoteSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			StmNoteSchema.PK,
			StmNoteSchema.ST_Description,
			StmNoteSchema.ST_ForceRead,
			StmNoteSchema.ST_GC_RelatedCompany,
			StmNoteSchema.ST_IsCustomDescription,
			StmNoteSchema.ST_NoteContext,
			StmNoteSchema.ST_NoteData,
			StmNoteSchema.ST_NoteText,
			StmNoteSchema.ST_NoteType,
			StmNoteSchema.ST_ParentID,
			StmNoteSchema.ST_SystemCreateTimeUtc,
			StmNoteSchema.ST_SystemCreateUser,
			StmNoteSchema.ST_SystemLastEditTimeUtc,
			StmNoteSchema.ST_SystemLastEditUser,
			StmNoteSchema.ST_Table,
		};
	}
}
