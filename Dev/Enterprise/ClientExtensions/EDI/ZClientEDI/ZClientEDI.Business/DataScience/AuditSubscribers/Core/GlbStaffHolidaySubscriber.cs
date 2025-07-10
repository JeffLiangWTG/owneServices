using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class GlbStaffHolidaySubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbStaffHolidaySubscriberCode;
		public override int DataSchemaVersion => 2; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbStaffHolidaySchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbStaffHolidaySchema.PK,
			GlbStaffHolidaySchema.GA_ApprovalStatus,
			GlbStaffHolidaySchema.GA_AvailabilityPercentage,
			GlbStaffHolidaySchema.GA_DaysLeaveTaken,
			GlbStaffHolidaySchema.GA_EndTime,
			GlbStaffHolidaySchema.GA_IsValid,
			GlbStaffHolidaySchema.GA_IsWorkingAway,
			GlbStaffHolidaySchema.GA_OverrideLeaveTaken,
			GlbStaffHolidaySchema.GA_ParentTableCode,
			GlbStaffHolidaySchema.GA_WorkHolidayType,
			GlbStaffHolidaySchema.GA_GS,
			GlbStaffHolidaySchema.GA_ParentID,
			GlbStaffHolidaySchema.GA_RecordType,
			GlbStaffHolidaySchema.GA_StartTime,
			GlbStaffHolidaySchema.GA_SystemCreateTimeUtc,
			GlbStaffHolidaySchema.GA_SystemCreateUser,
			GlbStaffHolidaySchema.GA_SystemCreateBranch,
			GlbStaffHolidaySchema.GA_SystemCreateDepartment,
			GlbStaffHolidaySchema.GA_SystemLastEditTimeUtc,
			GlbStaffHolidaySchema.GA_SystemLastEditUser
		};
	}
}
