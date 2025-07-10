using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class GlbHolidaySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbHolidaySubscriberCode;
		public override int DataSchemaVersion => 1; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbHolidaySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbHolidaySchema.PK,
			GlbHolidaySchema.GH_IsValid,
			GlbHolidaySchema.GH_IsWorkingDay,
			GlbHolidaySchema.GH_RecurrDay,
			GlbHolidaySchema.GH_RecurrMonth,
			GlbHolidaySchema.GH_Date,
			GlbHolidaySchema.GH_HolidayName,
			GlbHolidaySchema.GH_IsActive,
			GlbHolidaySchema.GH_ParentID,
			GlbHolidaySchema.GH_ParentTableCode,
			GlbHolidaySchema.GH_Recurring,
			GlbHolidaySchema.GH_RecurrType,
			GlbHolidaySchema.GH_SystemCreateTimeUtc,
			GlbHolidaySchema.GH_SystemCreateUser,
			GlbHolidaySchema.GH_SystemLastEditTimeUtc,
			GlbHolidaySchema.GH_SystemLastEditUser
		};
	}
}
