using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class OrgContactSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.OrgContactSubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = OrgContactSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			OrgContactSchema.PK,
			OrgContactSchema.OC_ContactName,
			OrgContactSchema.OC_Email,
			OrgContactSchema.OC_OH,
			OrgContactSchema.OC_OH_AddressOverride,
			OrgContactSchema.OC_SystemCreateBranch,
			OrgContactSchema.OC_SystemCreateDepartment,
			OrgContactSchema.OC_SystemCreateTimeUtc,
			OrgContactSchema.OC_SystemCreateUser,
			OrgContactSchema.OC_SystemLastEditTimeUtc,
			OrgContactSchema.OC_SystemLastEditUser,
		};
	}
}
