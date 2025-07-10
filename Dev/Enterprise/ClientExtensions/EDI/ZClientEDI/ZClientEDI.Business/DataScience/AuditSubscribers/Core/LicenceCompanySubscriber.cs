using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class LicenceCompanySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.LicenceCompanySubscriberCode;
		public override int DataSchemaVersion => 3; // Bump this if the schema changes
		public override ITableSchema Table { get; } = LicenceCompanySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			LicenceCompanySchema.PK,
			LicenceCompanySchema.LC_CompanyCode,
			LicenceCompanySchema.LC_CompanyCountry,
			LicenceCompanySchema.LC_IsReciprocal,
			LicenceCompanySchema.LC_IsGSTRegistered,
			LicenceCompanySchema.LC_IsGSTCashBasis,
			LicenceCompanySchema.LC_IsWHTRegistered,
			LicenceCompanySchema.LC_IsWHTCashBasis,
			LicenceCompanySchema.LC_OH,
			LicenceCompanySchema.LC_LE,
			LicenceCompanySchema.LC_CompanyNumber,
		};
	}
}
