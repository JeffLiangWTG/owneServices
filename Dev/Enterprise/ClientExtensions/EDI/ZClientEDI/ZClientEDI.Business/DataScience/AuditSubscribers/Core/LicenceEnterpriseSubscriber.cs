using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public sealed class LicenceEnterpriseSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.LicenceEnterpriseSubscriberCode;
		public override int DataSchemaVersion => 2;
		public override ITableSchema Table => LicenceEnterpriseSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			LicenceEnterpriseSchema.PK,
			LicenceEnterpriseSchema.LE_EnterpriseCode,
			LicenceEnterpriseSchema.LE_OH,
			LicenceEnterpriseSchema.LE_IsInternal,
			LicenceEnterpriseSchema.LE_EnterpriseID,
			LicenceEnterpriseSchema.LE_SystemCreateTimeUtc,
			LicenceEnterpriseSchema.LE_SystemCreateUser,
			LicenceEnterpriseSchema.LE_SystemLastEditTimeUtc,
			LicenceEnterpriseSchema.LE_SystemLastEditUser,
		};
	}
}
