using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class GenCustomAddOnValueSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GenCustomAddOnValueSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GenCustomAddOnValueSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GenCustomAddOnValueSchema.PK,
			GenCustomAddOnValueSchema.XV_Data,
			GenCustomAddOnValueSchema.XV_IsRuleEnabled,
			GenCustomAddOnValueSchema.XV_ParentID,
			GenCustomAddOnValueSchema.XV_ParentTableCode,
			GenCustomAddOnValueSchema.XV_Type,
			GenCustomAddOnValueSchema.XV_XR_Rule,
			GenCustomAddOnValueSchema.XV_SystemCreateTimeUtc,
			GenCustomAddOnValueSchema.XV_SystemCreateUser,
			GenCustomAddOnValueSchema.XV_SystemLastEditTimeUtc,
			GenCustomAddOnValueSchema.XV_SystemLastEditUser,
			GenCustomAddOnValueSchema.XV_Name,
		};
	}
}
