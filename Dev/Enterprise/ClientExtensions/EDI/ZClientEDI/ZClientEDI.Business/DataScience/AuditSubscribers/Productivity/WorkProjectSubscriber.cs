using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Productivity
{
	public class WorkProjectSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Productivity>
	{
		public override string Code => SubscriberCodes.WorkProjectSubscriberCode;
		public override int DataSchemaVersion => 4; // Bump this if the schema changes
		public override ITableSchema Table { get; } = WorkProjectSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			WorkProjectSchema.PK,
			WorkProjectSchema.WKP_ClosedDate,
			WorkProjectSchema.WKP_GS_NKProjectManager,
			WorkProjectSchema.WKP_Module,
			WorkProjectSchema.WKP_OA_ClientAddress,
			WorkProjectSchema.WKP_OC_Contact,
			WorkProjectSchema.WKP_OC_TechnicalContact,
			WorkProjectSchema.WKP_P8_Opportunity,
			WorkProjectSchema.WKP_Priority,
			WorkProjectSchema.WKP_ProjectNumber,
			WorkProjectSchema.WKP_Status,
			WorkProjectSchema.WKP_SubType,
			WorkProjectSchema.WKP_Summary,
			WorkProjectSchema.WKP_SystemCreateBranch,
			WorkProjectSchema.WKP_SystemCreateDepartment,
			WorkProjectSchema.WKP_SystemCreateTimeUtc,
			WorkProjectSchema.WKP_SystemCreateUser,
			WorkProjectSchema.WKP_SystemLastEditTimeUtc,
			WorkProjectSchema.WKP_SystemLastEditUser,
			WorkProjectSchema.WKP_Type,
		};
	}
}
