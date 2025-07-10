using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using WTG.Serialization.DataScience.Audit.ObjectModel;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	[Immutable]
	public sealed class RefUNLOCOUtcOffsetCustomSchema : ITableSchema
	{
		readonly RefUNLOCOUtcOffsetSchema baseSchema;

		public string SqlSchemaName => ((ITableSchema)baseSchema).SqlSchemaName;

		public string TableName => ((ITableSchema)baseSchema).TableName.Replace("RefDatabase_", string.Empty);

		public SchemaPKColumn PK => ((ITableSchema)baseSchema).PK;

		public string PkIndexName => ((ITableSchema)baseSchema).PkIndexName;

		public SchemaColumnCollection All => ((ITableSchema)baseSchema).All;

		public SchemaColumn GetSchemaColumn(string columnName)
		{
			return ((ITableSchema)baseSchema).GetSchemaColumn(columnName);
		}

		public RefUNLOCOUtcOffsetCustomSchema(RefUNLOCOUtcOffsetSchema baseSchema)
		{
			this.baseSchema = baseSchema;
		}
	}

	public class RefUNLOCOUtcOffsetSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.RefUNLOCOUtcOffsetSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = new RefUNLOCOUtcOffsetCustomSchema(RefUNLOCOUtcOffsetSchema.Instance);
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			RefUNLOCOUtcOffsetSchema.PK,
			RefUNLOCOUtcOffsetSchema.RLO_EndTimeUtc,
			RefUNLOCOUtcOffsetSchema.RLO_OffsetMinutesFromUtc,
			RefUNLOCOUtcOffsetSchema.RLO_RL_NKCode,
			RefUNLOCOUtcOffsetSchema.RLO_StartTimeUtc
		};
		public override IEnumerable<ColumnInfo> NonBizObjColumns => new ColumnInfo[]
		{
			new ColumnInfo(columnName: "RLO_SystemCreateTimeUtc", sqlType: "smalldatetime", isNullable: true),
			new ColumnInfo(columnName: "RLO_SystemCreateUser", sqlType: "varchar(3)", isNullable: false),
			new ColumnInfo(columnName: "RLO_SystemLastEditTimeUtc", sqlType: "smalldatetime", isNullable: true),
			new ColumnInfo(columnName: "RLO_SystemLastEditUser", sqlType: "varchar(3)", isNullable: false),
		};
	}
}
