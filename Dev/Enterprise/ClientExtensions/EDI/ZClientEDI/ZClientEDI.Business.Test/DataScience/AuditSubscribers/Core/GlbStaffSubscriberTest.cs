using System.Collections.Generic;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(GlbStaffSubscriber))]
	class GlbStaffSubscriberTest : DataScienceAuditSubscriberTestBase<GlbStaffSubscriber>
	{
		public override void TestCustomFilter()
		{
			var subscriber = new GlbStaffSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			//var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);

			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(4, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table GlbStaff required by GlbStaffSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(48, subscriber.ColumnInfos.Count);

					AssertEquals("GS_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("GS_City", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("nvarchar(128)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("GS_Code", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("GS_DepartureDate", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("GS_DueBack", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("GS_EmailAddress", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("nvarchar(254)", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("GS_EmploymentBasis", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("GS_EmploymentDate", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("GS_ExternalId", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("nvarchar(254)", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("GS_FriendlyName", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("GS_FullName", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("nvarchar(256)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("GS_GB_HomeBranch", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("GS_GE_HomeDepartment", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("GS_GivenName", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);

					AssertEquals("GS_IsActive", subscriber.ColumnInfos[14].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[14].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[14].IsNullable);

					AssertEquals("GS_IsActivityLogged", subscriber.ColumnInfos[15].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[15].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[15].IsNullable);

					AssertEquals("GS_IsController", subscriber.ColumnInfos[16].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[16].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[16].IsNullable);

					AssertEquals("GS_IsDeveloper", subscriber.ColumnInfos[17].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[17].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[17].IsNullable);

					AssertEquals("GS_IsDevice", subscriber.ColumnInfos[18].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[18].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[18].IsNullable);

					AssertEquals("GS_IsDriver", subscriber.ColumnInfos[19].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[19].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[19].IsNullable);

					AssertEquals("GS_IsInTrainingMode", subscriber.ColumnInfos[20].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[20].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[20].IsNullable);

					AssertEquals("GS_IsOperational", subscriber.ColumnInfos[21].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[21].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[21].IsNullable);

					AssertEquals("GS_IsResource", subscriber.ColumnInfos[22].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[22].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[22].IsNullable);

					AssertEquals("GS_IsRobot", subscriber.ColumnInfos[23].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[23].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[23].IsNullable);

					AssertEquals("GS_IsSalesRep", subscriber.ColumnInfos[24].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[24].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[24].IsNullable);

					AssertEquals("GS_IsSystemAccount", subscriber.ColumnInfos[25].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[25].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[25].IsNullable);

					AssertEquals("GS_IsValid", subscriber.ColumnInfos[26].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[26].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[26].IsNullable);

					AssertEquals("GS_LastActivityDate", subscriber.ColumnInfos[27].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[27].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[27].IsNullable);

					AssertEquals("GS_LastDayOfWork", subscriber.ColumnInfos[28].ColumnName);
					AssertEquals("date", subscriber.ColumnInfos[28].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[28].IsNullable);

					AssertEquals("GS_LoginName", subscriber.ColumnInfos[29].ColumnName);
					AssertEquals("nvarchar(104)", subscriber.ColumnInfos[29].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[29].IsNullable);

					AssertEquals("GS_NameSuffix", subscriber.ColumnInfos[30].ColumnName);
					AssertEquals("nvarchar(10)", subscriber.ColumnInfos[30].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[30].IsNullable);

					AssertEquals("GS_NameTitle", subscriber.ColumnInfos[31].ColumnName);
					AssertEquals("nvarchar(10)", subscriber.ColumnInfos[31].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[31].IsNullable);

					AssertEquals("GS_PER", subscriber.ColumnInfos[32].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[32].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[32].IsNullable);

					AssertEquals("GS_PreferredSurname", subscriber.ColumnInfos[33].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[33].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[33].IsNullable);

					AssertEquals("GS_PublishEmailAddress", subscriber.ColumnInfos[34].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[34].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[34].IsNullable);

					AssertEquals("GS_ResourceType", subscriber.ColumnInfos[35].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[35].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[35].IsNullable);

					AssertEquals("GS_RN_NKCountryCode", subscriber.ColumnInfos[36].ColumnName);
					AssertEquals("varchar(2)", subscriber.ColumnInfos[36].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[36].IsNullable);

					AssertEquals("GS_State", subscriber.ColumnInfos[37].ColumnName);
					AssertEquals("nvarchar(128)", subscriber.ColumnInfos[37].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[37].IsNullable);

					AssertEquals("GS_Surname", subscriber.ColumnInfos[38].ColumnName);
					AssertEquals("nvarchar(80)", subscriber.ColumnInfos[38].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[38].IsNullable);

					AssertEquals("GS_SystemCreateBranch", subscriber.ColumnInfos[39].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[39].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[39].IsNullable);

					AssertEquals("GS_SystemCreateDepartment", subscriber.ColumnInfos[40].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[40].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[40].IsNullable);

					AssertEquals("GS_SystemCreateTimeUtc", subscriber.ColumnInfos[41].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[41].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[41].IsNullable);

					AssertEquals("GS_SystemCreateUser", subscriber.ColumnInfos[42].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[42].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[42].IsNullable);

					AssertEquals("GS_SystemLastEditTimeUtc", subscriber.ColumnInfos[43].ColumnName);
					AssertEquals("datetime", subscriber.ColumnInfos[43].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[43].IsNullable);

					AssertEquals("GS_SystemLastEditUser", subscriber.ColumnInfos[44].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[44].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[44].IsNullable);

					AssertEquals("GS_Title", subscriber.ColumnInfos[45].ColumnName);
					AssertEquals("nvarchar(128)", subscriber.ColumnInfos[45].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[45].IsNullable);

					AssertEquals("GS_ValidationStatus", subscriber.ColumnInfos[46].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[46].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[46].IsNullable);

					AssertEquals("GS_WorkingLanguage", subscriber.ColumnInfos[47].ColumnName);
					AssertEquals("varchar(7)", subscriber.ColumnInfos[47].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[47].IsNullable);
				});
		}

		protected override IEnumerable<string> IgnoredColumnNames
		{
			get
			{
				yield return "GS_ActiveDirectoryObjectGuid";
				yield return "GS_ActiveDirectorySid";
				yield return "GS_Birthdate";
				yield return "GS_BrokerID";
				yield return "GS_BrokerPassword";
				yield return "GS_BrokerPasswordStatus";
				yield return "GS_BrokerWorkingPassword";
				yield return "GS_ChangePasswordAtNextLogin";
				yield return "GS_CommissionBasis";
				yield return "GS_EftWages";
				yield return "GS_EmergencyContactName";
				yield return "GS_EmergencyContactRelationship";
				yield return "GS_EmergencyHomePhone";
				yield return "GS_EmergencyWorkPhone";
				yield return "GS_EnterpriseCertificationID";
				yield return "GS_FaxNum";
				yield return "GS_GB_LastLogonBranch";
				yield return "GS_GC_PreferredPaymentCompany";
				yield return "GS_GE_LastLogonDepartment";
				yield return "GS_Gender";
				yield return "GS_HomePhone";
				yield return "GS_IsTwoFactorAuthenticationEnabled";
				yield return "GS_LastPasswordAttemptDateTime_Utc";
				yield return "GS_LastPasswordChangeDate";
				yield return "GS_MobilePhone";
				yield return "GS_NextOfKin";
				yield return "GS_NextOfKinHomePhone";
				yield return "GS_NextOfKinRelationship";
				yield return "GS_NextOfKinWorkPhone";
				yield return "GS_NextReviewDate";
				yield return "GS_OutOnTask";
				yield return "GS_Pager";
				yield return "GS_Passport";
				yield return "GS_PasswordNeverChanges";
				yield return "GS_PersonalEDIMailBox";
				yield return "GS_Postcode";
				yield return "GS_PublishFaxNum";
				yield return "GS_PublishHomePhone";
				yield return "GS_PublishMobilePhone";
				yield return "GS_PublishWorkExtension";
				yield return "GS_PublishWorkPhone";
				yield return "GS_SAMAccountFullName";
				yield return "GS_SecurityCardNumber";
				yield return "GS_SqlLoginPasswordHash";
				yield return "GS_UserAddress1";
				yield return "GS_UserAddress2";
				yield return "GS_WagesBankAccount";
				yield return "GS_WagesBankBsb";
				yield return "GS_WagesBankName";
				yield return "GS_WagesBankSwift";
				yield return "GS_WorkExtension";
				yield return "GS_WorkPhone";
				yield return "GS_CanLogin";
				yield return "GS_AddressMap";
				yield return "GS_DomainName";
				yield return "GS_RN_NKNationalityCode";
				yield return "GS_PasswordHashIterations";
				yield return "GS_ActivityTrackingStatus";
				yield return "GS_EmergencyContactEmail";
				yield return "GS_NextOfKinEmail";
				yield return "GS_ProbationEndDate";
				yield return "GS_ResidencyExpiry";
				yield return "GS_ResidencyStatus";
				yield return "GS_SavePersonalDataToActiveDirectory";
				yield return "GS_FullNameInMotherLanguage";
				yield return "GS_MiddleName";
				yield return "GS_GenderCustomTerm";
			}
		}
	}
}
