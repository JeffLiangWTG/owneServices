using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class GlbStaffSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override string Code => SubscriberCodes.GlbStaffSubscriberCode;
		public override int DataSchemaVersion => 4; // Bump this if the schema changes
		public override ITableSchema Table { get; } = GlbStaffSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			GlbStaffSchema.PK,
			GlbStaffSchema.GS_City,
			GlbStaffSchema.GS_Code,
			GlbStaffSchema.GS_DepartureDate,
			GlbStaffSchema.GS_DueBack,
			GlbStaffSchema.GS_EmailAddress,
			GlbStaffSchema.GS_EmploymentBasis,
			GlbStaffSchema.GS_EmploymentDate,
			GlbStaffSchema.GS_ExternalId,
			GlbStaffSchema.GS_FriendlyName,
			GlbStaffSchema.GS_FullName,
			GlbStaffSchema.GS_GB_HomeBranch,
			GlbStaffSchema.GS_GE_HomeDepartment,
			GlbStaffSchema.GS_GivenName,
			GlbStaffSchema.GS_IsActive,
			GlbStaffSchema.GS_IsActivityLogged,
			GlbStaffSchema.GS_IsController,
			GlbStaffSchema.GS_IsDeveloper,
			GlbStaffSchema.GS_IsDevice,
			GlbStaffSchema.GS_IsDriver,
			GlbStaffSchema.GS_IsInTrainingMode,
			GlbStaffSchema.GS_IsOperational,
			GlbStaffSchema.GS_IsResource,
			GlbStaffSchema.GS_IsRobot,
			GlbStaffSchema.GS_IsSalesRep,
			GlbStaffSchema.GS_IsSystemAccount,
			GlbStaffSchema.GS_IsValid,
			GlbStaffSchema.GS_LastActivityDate,
			GlbStaffSchema.GS_LastDayOfWork,
			GlbStaffSchema.GS_LoginName,
			GlbStaffSchema.GS_NameSuffix,
			GlbStaffSchema.GS_NameTitle,
			GlbStaffSchema.GS_PER,
			GlbStaffSchema.GS_PreferredSurname,
			GlbStaffSchema.GS_PublishEmailAddress,
			GlbStaffSchema.GS_ResourceType,
			GlbStaffSchema.GS_RN_NKCountryCode,
			GlbStaffSchema.GS_State,
			GlbStaffSchema.GS_Surname,
			GlbStaffSchema.GS_SystemCreateBranch,
			GlbStaffSchema.GS_SystemCreateDepartment,
			GlbStaffSchema.GS_SystemCreateTimeUtc,
			GlbStaffSchema.GS_SystemCreateUser,
			GlbStaffSchema.GS_SystemLastEditTimeUtc,
			GlbStaffSchema.GS_SystemLastEditUser,
			GlbStaffSchema.GS_Title,
			GlbStaffSchema.GS_ValidationStatus,
			GlbStaffSchema.GS_WorkingLanguage,
		};
	}
}
