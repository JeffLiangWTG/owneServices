using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(TG_GlbStaff_Insert))]
	class TG_GlbStaff_InsertTest : DbCreateScriptTest
	{
		const string refColumnsPerson =
			@"PER_AutoVersion,smallint
PER_BirthDate,date
PER_ChallengePhrase,varchar(20)
PER_ChallengePhraseType,varchar(3)
PER_City,nvarchar(50)
PER_DriversLicenseNumber,varchar(25)
PER_EmailAddress,nvarchar(254)
PER_EmailAddress2,nvarchar(254)
PER_FaxNumber,varchar(20)
PER_FriendlyName,nvarchar(80)
PER_FullName,nvarchar(256)
PER_IDPUserId,uniqueidentifier
PER_Gender,char(1)
PER_HomeAddress1,nvarchar(50)
PER_HomeAddress2,nvarchar(50)
PER_HomePhone,varchar(20)
PER_IsActive,bit
PER_IsValid,bit
PER_LegalName,nvarchar(256)
PER_LoginDisabledUntilUtc,smalldatetime
PER_MobilePhone,varchar(20)
PER_MobilePhone2,varchar(20)
PER_NameSuffix,nvarchar(10)
PER_Passport,varchar(20)
PER_PassportExpiryDate,date
PER_PassportPlaceOfIssue,nvarchar(50)
PER_PasswordHash,varbinary(20)
PER_PasswordHashIterations,int
PER_PasswordSalt,varbinary(16)
PER_PersonalInfo,nvarchar(max)
PER_Picture,varbinary(max)
PER_PK,uniqueidentifier
PER_Postcode,nvarchar(10)
PER_PreferredLanguage,varchar(7)
PER_RN_NKCountry,varchar(2)
PER_RN_NKNationalityCodeISO,varchar(2)
PER_State,nvarchar(25)
PER_SystemCreateTimeUtc,smalldatetime
PER_SystemCreateUser,varchar(3)
PER_SystemCreateBranch,varchar(3)
PER_SystemCreateDepartment,varchar(3)
PER_SystemLastEditTimeUtc,smalldatetime
PER_SystemLastEditUser,varchar(3)
PER_NameTitle,nvarchar(10)
PER_ValidationStatus,char(3)
PER_WebAccessEnabled,bit";

		const string refColumnsPersonWhiteList = @"PER_FriendlyNameAI,nvarchar(80)
PER_FullNameAI,nvarchar(256)
PER_LegalNameAI,nvarchar(256)";

		const string refColumnsStaff =
			@"GS_ActiveDirectoryObjectGuid,uniqueidentifier
GS_ActiveDirectorySid,varbinary(85)
GS_AddressMap,varchar(50)
GS_AutoVersion,smallint
GS_Birthdate,date
GS_BrokerID,varchar(20)
GS_BrokerPassword,varchar(128)
GS_BrokerPasswordStatus,varchar(3)
GS_BrokerWorkingPassword,varchar(128)
GS_CanLogin,bit
GS_ChangePasswordAtNextLogin,bit
GS_City,nvarchar(128)
GS_Code,varchar(3)
GS_CommissionBasis,varchar(3)
GS_DepartureDate,smalldatetime
GS_DomainName,nvarchar(255)
GS_DueBack,smalldatetime
GS_EftWages,bit
GS_EmailAddress,nvarchar(254)
GS_EmergencyContactName,nvarchar(50)
GS_EmergencyContactRelationship,varchar(3)
GS_EmergencyHomePhone,varchar(20)
GS_EmergencyWorkPhone,varchar(20)
GS_EmploymentBasis,varchar(3)
GS_EmploymentDate,smalldatetime
GS_EnterpriseCertificationID,varchar(20)
GS_FaxNum,varchar(20)
GS_FriendlyName,nvarchar(80)
GS_FullName,nvarchar(256)
GS_GivenName,nvarchar(80)
GS_MiddleName,nvarchar(80)
GS_Surname,nvarchar(80)
GS_PreferredSurname,nvarchar(80)
GS_FullNameInMotherLanguage,nvarchar(256)
GS_GB_HomeBranch,uniqueidentifier
GS_GB_LastLogonBranch,uniqueidentifier
GS_GC_PreferredPaymentCompany,uniqueidentifier
GS_GE_HomeDepartment,uniqueidentifier
GS_GE_LastLogonDepartment,uniqueidentifier
GS_Gender,varchar(1)
GS_GenderCustomTerm,nvarchar(50)
GS_GeoLocation,geography
GS_HomePhone,varchar(20)
GS_IsActive,bit
GS_IsActivityLogged,bit
GS_IsController,bit
GS_IsDeveloper,bit
GS_IsDevice,bit
GS_IsDriver,bit
GS_IsInTrainingMode,bit
GS_IsOperational,bit
GS_IsResource,bit
GS_IsSalesRep,bit
GS_IsSystemAccount,bit
GS_IsTwoFactorAuthenticationEnabled,bit
GS_IsValid,bit
GS_LastActivityDate,smalldatetime
GS_LastPasswordAttemptDateTime_Utc,smalldatetime
GS_LastPasswordChangeDate,smalldatetime
GS_LoginName,nvarchar(104)
GS_MobilePhone,varchar(20)
GS_NameSuffix,nvarchar(10)
GS_NameTitle,nvarchar(10)
GS_NextOfKin,nvarchar(50)
GS_NextOfKinHomePhone,varchar(20)
GS_NextOfKinRelationship,varchar(3)
GS_NextOfKinWorkPhone,varchar(20)
GS_NextReviewDate,smalldatetime
GS_OutOnTask,varchar(50)
GS_Pager,varchar(20)
GS_Passport,varchar(20)
GS_PasswordHash,varbinary(20)
GS_PasswordHashIterations,int
GS_PasswordNeverChanges,bit
GS_PasswordSalt,varbinary(16)
GS_PER,uniqueidentifier
GS_PersonalEDIMailBox,varchar(20)
GS_PK,uniqueidentifier
GS_Postcode,nvarchar(40)
GS_ProfilePhoto,varbinary(max)
GS_PublishEmailAddress,bit
GS_PublishFaxNum,bit
GS_PublishHomePhone,bit
GS_PublishMobilePhone,bit
GS_PublishWorkExtension,bit
GS_PublishWorkPhone,bit
GS_ResourceType,varchar(3)
GS_RN_NKCountryCode,varchar(2)
GS_RN_NKNationalityCode,varchar(2)
GS_SAMAccountFullName,nvarchar(128)
GS_SecurityCardNumber,varchar(20)
GS_SqlLoginPasswordHash,varbinary(256)
GS_State,nvarchar(128)
GS_SystemCreateTimeUtc,datetime
GS_SystemCreateUser,varchar(3)
GS_SystemCreateBranch,varchar(3)
GS_SystemCreateDepartment,varchar(3)
GS_SystemLastEditTimeUtc,datetime
GS_SystemLastEditUser,varchar(3)
GS_Title,nvarchar(128)
GS_UserAddress1,nvarchar(1024)
GS_UserAddress2,nvarchar(50)
GS_UserSignature,varbinary(max)
GS_ValidationStatus,char(3)
GS_WagesBankAccount,varchar(35)
GS_WagesBankBsb,varchar(15)
GS_WagesBankName,nvarchar(35)
GS_WagesBankSwift,varchar(35)
GS_WorkExtension,varchar(10)
GS_WorkingLanguage,varchar(7)
GS_WorkPhone,varchar(20)
GS_ActivityTrackingStatus,varchar(3)
GS_NextOfKinEmail,nvarchar(254)
GS_EmergencyContactEmail,nvarchar(254)
GS_ResidencyStatus,varchar(3)
GS_ResidencyExpiry,smalldatetime
GS_ProbationEndDate,date
GS_LastDayOfWork,date
GS_SavePersonalDataToActiveDirectory,bit
GS_IsRobot,bit
GS_ExternalId,nvarchar(254)";

		const string script = @"declare @ReportName nvarchar(255) = N'dbo.{0}'

SELECT
	--obj_name = OBJECT_NAME(col.object_id),
	col_definition = col.name
		+ ','
		+ ColumnType.Value
		
		--+ CASE col.is_nullable WHEN 0 THEN ' NOT NULL' ELSE '     NULL' END
	
FROM
	sys.columns    AS col
	JOIN sys.types AS typ ON typ.user_type_id = col.user_type_id
	CROSS APPLY
	(
		SELECT
			Value =
				CASE
					WHEN typ.name in ('binary', 'char', 'varbinary', 'varchar') THEN CONCAT(typ.name, QUOTENAME(_max_length, ')'))
					WHEN typ.name in ('decimal', 'float', 'numeric')            THEN CONCAT(typ.name, QUOTENAME(CONCAT(col.precision, ', ', col.scale), ')'))
					WHEN typ.name in ('nchar', 'ntext', 'nvarchar')             THEN CONCAT(typ.name, QUOTENAME(_n_max_length, ')'))
					ELSE typ.name
				END
		FROM
			(
				SELECT
					_max_length   = CASE col.max_length WHEN -1 THEN 'max' ELSE CONVERT(varchar(5), col.max_length) END,
					_n_max_length = CASE col.max_length WHEN -1 THEN 'max' ELSE CONVERT(varchar(5), col.max_length / 2) END
			) AS Data

	) AS ColumnType
WHERE 1=1
	AND col.object_id = OBJECT_ID(@ReportName)
ORDER BY
	col.name";

		public void TestTG_GlbStaff_InsertColumns()
		{
			AssertColumns(refColumnsStaff, "GlbStaff");
			AssertColumns(refColumnsPerson, "GlbPerson");
		}

		void AssertColumns(string refColumns, string table)
		{
			var refList = new List<string>(refColumns.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
			var whiteList = new List<string>(refColumnsPersonWhiteList.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
			using (var cmd = Db.Connection.Command(string.Format(script, table)))
			{
				using (var reader = cmd.ExecuteReader())
				{
					int count = 0;
					while (reader.Read())
					{
						var columnName = reader.GetString(0);
						if (!whiteList.Contains(columnName))
						{
							AssertCollectionContains($"Please verify TG_GlbStaff_Insert trigger validity after the column change. Column [{columnName}] in table [{table}]",
								columnName, refList);
							count++;
						}
					}

					AssertEquals("Please add/remove columns accordingly in the TG_GlbStaff_Insert trigger", refList.Count, count);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestTriggerTG_GlbStaff_InsertRunSuccessfully_WhenGlbStaffAddressLengthMoreThan50()
		{
			var glbStaffPK = Guid.NewGuid();
			var sqlScript = string.Format("insert into dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_UserAddress1, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) Values ('{0}', '{1}', 'TEST{1}', N'DSV UTI Air & Sea Agenciamento De Transportes Ltda Bala Bala', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')", glbStaffPK, glbStaffPK.ToString().Substring(0, 3));
			using (var cmd = Db.Connection.Command(sqlScript))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void TestTriggerTG_GlbStaff_InsertShouldCreatePrimaryRelationship()
		{
			var glbStaffPk = Guid.NewGuid();
			var sqlScript =
				string.Format(
					@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_UserAddress1, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES ('{0}', '{1}', 'TEST{1}', N'Agencia', GETUTCDATE(), 'E', GETUTCDATE(), 'E')",
					glbStaffPk, glbStaffPk.ToString().Substring(0, 3));
			using (var cmd = Db.Connection.Command(sqlScript))
			{
				cmd.ExecuteNonQuery();
			}

			var query = FormattableString.Invariant(
				$"SELECT COUNT(*) FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{glbStaffPk}' AND PPR_PrimaryTableCode = 'GS'");
			using (var command = TestConnection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(1, result);
			}
		}

		public void TestTriggerTG_GlbStaff_InsertShouldNotCreatePrimaryRelationshipIfPersonIsSpecified()
		{
			var personPk = Guid.NewGuid();
			var glbStaffPk = Guid.NewGuid();
			var sqlScript =
				string.Format(
					@"
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{2}', 'Alex A')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_UserAddress1, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES ('{0}', '{1}', 'TEST{1}', N'Agencia', '{2}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')",
					glbStaffPk, glbStaffPk.ToString().Substring(0, 3), personPk);
			using (var cmd = Db.Connection.Command(sqlScript))
			{
				cmd.ExecuteNonQuery();
			}

			var query = FormattableString.Invariant(
				$"SELECT COUNT(*) FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{glbStaffPk}' AND PPR_PrimaryTableCode = 'GS'");
			using (var command = TestConnection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(0, result);
			}
		}
	}
}

