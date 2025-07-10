using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using WTG.Serialization.DataScience.Audit.ObjectModel;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Incidents
{
	public class IncidentRequestSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.IncidentRelated>
	{
		public override string Code => SubscriberCodes.IncidentRequestSubscriberCode;
		public override int DataSchemaVersion => 3; // Bump this if the schema changes
		public override ITableSchema Table { get; } = IncidentRequestSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			IncidentRequestSchema.PK,
			IncidentRequestSchema.INC_Area,
			IncidentRequestSchema.INC_ClientReference,
			IncidentRequestSchema.INC_Criticality,
			IncidentRequestSchema.INC_IncidentNumber,
			IncidentRequestSchema.INC_IsCustomerResolved,
			IncidentRequestSchema.INC_Language,
			IncidentRequestSchema.INC_OC_ApprovedBy,
			IncidentRequestSchema.INC_OC_ReportedBy,
			IncidentRequestSchema.INC_ProductLicence,
			IncidentRequestSchema.INC_ReferenceID,
			IncidentRequestSchema.INC_RN_NKCountry,
			IncidentRequestSchema.INC_ServiceType,
			IncidentRequestSchema.INC_Status,
			IncidentRequestSchema.INC_SubType,
			IncidentRequestSchema.INC_Summary,
			IncidentRequestSchema.INC_SystemCreateTimeUtc,
			IncidentRequestSchema.INC_SystemCreateUser,
			IncidentRequestSchema.INC_SystemLastEditUser,
			IncidentRequestSchema.INC_Type,
			IncidentRequestSchema.INC_SystemLastEditTimeUtc,
			IncidentRequestSchema.INC_Details,
			IncidentRequestSchema.INC_ExpiryCountdownStartTimeUtc,
		};

		public override IEnumerable<ColumnInfo> NonBizObjColumns => new ColumnInfo[]
		{
			new ColumnInfo(columnName: "INC_DetailsVersion", sqlType: "smallint", isNullable: false),
		};
	}
}
