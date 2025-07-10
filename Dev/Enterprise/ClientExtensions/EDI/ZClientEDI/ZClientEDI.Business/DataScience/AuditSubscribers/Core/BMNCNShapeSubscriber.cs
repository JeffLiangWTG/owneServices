using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public sealed class BMNCNShapeSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.BMNCNShape;

		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => BMNCNShapeSchema.Instance;

		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			BMNCNShapeSchema.PK,
			BMNCNShapeSchema.BNS_BNS_ParentShape,
			BMNCNShapeSchema.BNS_BNS_RootShape,
			BMNCNShapeSchema.BNS_BNT_Style,
			BMNCNShapeSchema.BNS_GS_NKApprovedBy,
			BMNCNShapeSchema.BNS_IsValid,
			BMNCNShapeSchema.BNS_JobType,
			BMNCNShapeSchema.BNS_Name,
			BMNCNShapeSchema.BNS_RelatedEntityID,
			BMNCNShapeSchema.BNS_RelatedEntityTableCode,
			BMNCNShapeSchema.BNS_ShapeType,
			BMNCNShapeSchema.BNS_Status,
			BMNCNShapeSchema.BNS_SystemCreateTimeUtc,
			BMNCNShapeSchema.BNS_SystemCreateUser,
			BMNCNShapeSchema.BNS_SystemLastEditTimeUtc,
			BMNCNShapeSchema.BNS_SystemLastEditUser,
		};
	}
}
