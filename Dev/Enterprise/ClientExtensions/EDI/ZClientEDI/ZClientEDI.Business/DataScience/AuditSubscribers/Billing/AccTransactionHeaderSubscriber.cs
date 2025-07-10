using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing
{
	public sealed class AccTransactionHeaderSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Billing>
	{
		public override string Code => SubscriberCodes.AccTransactionHeaderSubscriberCode;
		public override int DataSchemaVersion => 3; // Bump this if the schema changes
		public override ITableSchema Table => AccTransactionHeaderSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			AccTransactionHeaderSchema.PK,
			AccTransactionHeaderSchema.AH_InvoiceDate,
			AccTransactionHeaderSchema.AH_OH,
			AccTransactionHeaderSchema.AH_GC,
			AccTransactionHeaderSchema.AH_SystemCreateTimeUtc,
			AccTransactionHeaderSchema.AH_SystemCreateUser,
			AccTransactionHeaderSchema.AH_SystemCreateBranch,
			AccTransactionHeaderSchema.AH_SystemCreateDepartment,
			AccTransactionHeaderSchema.AH_SystemLastEditTimeUtc,
			AccTransactionHeaderSchema.AH_SystemLastEditUser,
			AccTransactionHeaderSchema.AH_OA_InvoiceAddressOverride,
			AccTransactionHeaderSchema.AH_OC_InvoiceContactOverride,
			AccTransactionHeaderSchema.AH_IsCancelled,
			AccTransactionHeaderSchema.AH_JobNumber,
		};
	}
}
