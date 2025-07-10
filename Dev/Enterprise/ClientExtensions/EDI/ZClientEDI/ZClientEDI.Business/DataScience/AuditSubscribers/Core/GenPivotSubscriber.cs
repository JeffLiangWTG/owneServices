using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class GenPivotSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GenPivotSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GenPivotSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GenPivotSchema.PK,
			GenPivotSchema.XX_Relation1ID,
			GenPivotSchema.XX_Relation1TableCode,
			GenPivotSchema.XX_Relation2ID,
			GenPivotSchema.XX_Relation2TableCode,
			GenPivotSchema.XX_SystemCreateTimeUtc,
			GenPivotSchema.XX_SystemCreateUser,
			GenPivotSchema.XX_SystemLastEditTimeUtc,
			GenPivotSchema.XX_SystemLastEditUser,
			GenPivotSchema.XX_RelationType,
			GenPivotSchema.XX_Sequence,
		};
	}
}
