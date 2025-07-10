using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public sealed class ClientCompanySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.ClientCompanySubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table => ClientCompanySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			ClientCompanySchema.PK,
			ClientCompanySchema.LCC_Code,
			ClientCompanySchema.LCC_Name,
			ClientCompanySchema.LCC_ClientPK,
			ClientCompanySchema.LCC_LD,
			ClientCompanySchema.LCC_OH,
			ClientCompanySchema.LCC_RN_NKCountryCode,
			ClientCompanySchema.LCC_CreateTimeUtc,
			ClientCompanySchema.LCC_DeactivateTimeUtc,
			ClientCompanySchema.LCC_City,
			ClientCompanySchema.LCC_PostCode,
			ClientCompanySchema.LCC_State,
			ClientCompanySchema.LCC_RX_NKLocalCurrency,
			ClientCompanySchema.LCC_WebAddress,
			ClientCompanySchema.LCC_IsReciprocal,
			ClientCompanySchema.LCC_CodeValidFromUtc,
		};
	}
}
