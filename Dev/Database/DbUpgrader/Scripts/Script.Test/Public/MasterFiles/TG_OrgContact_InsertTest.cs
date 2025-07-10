using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(TG_OrgContact_Insert))]
	class TG_OrgContact_InsertTest : DbCreateScriptTest
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

		const string refColumnsContact =
			@"OC_AttachmentType,varchar(4)
OC_AutoVersion,smallint
OC_Birthday,smalldatetime
OC_ContactName,nvarchar(256)
OC_ContactSource,varchar(20)
OC_DetailsVerified,smalldatetime
OC_Email,nvarchar(254)
OC_Fax,varchar(20)
OC_Gender,varchar(1)
OC_HomePhone,varchar(20)
OC_IsActive,bit
OC_IsValid,bit
OC_JobCategory,varchar(35)
OC_Language,varchar(7)
OC_Mobile,varchar(20)
OC_NotifyMode,char(3)
OC_OA_OrgAddress,uniqueidentifier
OC_OH,uniqueidentifier
OC_OH_AddressOverride,uniqueidentifier
OC_OtherPhone,varchar(20)
OC_Pager,varchar(20)
OC_PasswordHash,varbinary(20)
OC_PasswordHashIterations,int
OC_PasswordSalt,varbinary(16)
OC_PER,uniqueidentifier
OC_PersonalInfo,nvarchar(max)
OC_Phone,varchar(20)
OC_PhoneExtension,varchar(10)
OC_PK,uniqueidentifier
OC_ProfilePhoto,varbinary(max)
OC_RN_NKNationality,char(2)
OC_Salutation,nvarchar(50)
OC_Title,nvarchar(35)
OC_WebAccessEnabled,bit
OC_WebContractSignedDate,smalldatetime
OC_YearJoinedCompany,smalldatetime
OC_YearJoinedIndustry,smalldatetime
OC_SystemCreateTimeUtc,smalldatetime
OC_SystemLastEditTimeUtc,smalldatetime
OC_SystemCreateUser,varchar(3)
OC_SystemCreateBranch,varchar(3)
OC_SystemCreateDepartment,varchar(3)
OC_SystemLastEditUser,varchar(3)";

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

		public void TestTG_OrgContact_InsertColumns()
		{
			AssertColumns(refColumnsContact, "OrgContact");
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
							AssertCollectionContains($"Please verify TG_OrgContact_Insert trigger validity after the column change. Column [{columnName}]",
							columnName, refList);
							count++;
						}
					}

					AssertEquals("Please add/remove columns accordingly in the TG_OrgContact_Insert trigger", refList.Count, count);
				}
			}
		}

		public void TestTriggerTG_OrgContact_InsertShouldCreatePrimaryRelationship()
		{
			var orgHeader = Guid.NewGuid();
			var orgContactPk = Guid.NewGuid();
			var sqlScript =
				FormattableString.Invariant($@"
INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	('{orgHeader}', 'TESTORG1')

INSERT INTO dbo.OrgContact
	(OC_PK, OC_ContactName, OC_OH)
VALUES
	('{orgContactPk}', 'Con 1', '{orgHeader}')
");
			using (var cmd = Db.Connection.Command(sqlScript))
			{
				cmd.ExecuteNonQuery();
			}

			var query = FormattableString.Invariant(
				$"SELECT COUNT(*) FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{orgContactPk}' AND PPR_PrimaryTableCode = 'OC'");
			using (var command = TestConnection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(1, result);
			}
		}

		public void TestTriggerTG_OrgContact_InsertShouldNotCreatePrimaryRelationshipIfPersonIsSpecified()
		{
			var orgHeader = Guid.NewGuid();
			var person = Guid.NewGuid();
			var orgContactPk = Guid.NewGuid();
			var sqlScript =
				FormattableString.Invariant($@"
INSERT INTO dbo.OrgHeader
	(OH_PK, OH_Code)
VALUES
	('{orgHeader}', 'TESTORG1')

INSERT INTO dbo.GlbPerson
	(PER_PK, PER_FullName) 
VALUES
	('{person}', 'Alex A')

INSERT INTO dbo.OrgContact
	(OC_PK, OC_ContactName, OC_OH, OC_PER)
VALUES
	('{orgContactPk}', 'Con 1', '{orgHeader}', '{person}')
");
			using (var cmd = Db.Connection.Command(sqlScript))
			{
				cmd.ExecuteNonQuery();
			}

			var query = FormattableString.Invariant(
				$"SELECT COUNT(*) FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{orgContactPk}' AND PPR_PrimaryTableCode = 'OC'");
			using (var command = TestConnection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(0, result);
			}
		}
	}

	[UseSnapshotProtection]
	class TG_OrgContact_Insert_NonTranTest : TestCase
	{
		public void TestFailingContactInsertShouldNotCreateGhostRecords()
		{
			var orgHeader = Guid.NewGuid();
			var orgContactPk1 = Guid.NewGuid();
			var orgContactPk2 = Guid.NewGuid();
			var sqlScript =
				FormattableString.Invariant($@"
INSERT INTO dbo.OrgHeader
    (OH_PK, OH_Code)
VALUES
    ('{orgHeader}', 'TESTORG1')

 

INSERT INTO dbo.OrgContact
    (OC_PK, OC_ContactName, OC_OH, OC_Email, OC_Mobile)
VALUES
    ('{orgContactPk1}', 'Con 1', '{orgHeader}', 'email@test.com', '12345')
");
			using (var cmd = Db.Connection.Command(sqlScript))
			{
				cmd.ExecuteNonQuery();
			}

			sqlScript =
				FormattableString.Invariant($@"
INSERT INTO dbo.OrgContact
    (OC_PK, OC_ContactName, OC_OH, OC_Email, OC_Mobile)
VALUES
    ('{orgContactPk2}', 'Con 1', '{orgHeader}', 'email@test.com', '12345')
");

			try
			{
				Db.Connection.RunInTransaction(() =>
				{
					Db.Connection.ExecuteNonQuery(sqlScript);
				});
				Fail("Should have failed");
			}
			catch (Exception ex)
			{
				Assert("Should have failed", true);
				AssertNotNull(ex);
			}

			var query = FormattableString.Invariant($"SELECT COUNT(*) FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{orgContactPk1}' AND PPR_PrimaryTableCode = 'OC'");
			using (var command = Db.Connection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(1, result);
			}

			query = FormattableString.Invariant($"SELECT COUNT(*) FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{orgContactPk2}' AND PPR_PrimaryTableCode = 'OC'");
			using (var command = Db.Connection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(0, result);
			}

			query = FormattableString.Invariant($"SELECT COUNT(*) FROM dbo.GlbPerson WHERE PER_MobilePhone = '12345'");
			using (var command = Db.Connection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(1, result);
			}
		}

		public void TestFailingContactInsertShouldNotCreateGhostRecords_SameContact()
		{
			var orgHeader = Guid.NewGuid();
			var orgContactPk1 = Guid.NewGuid();
			var orgContactPk2 = Guid.NewGuid();
			var sqlScript =
				FormattableString.Invariant($@"
INSERT INTO dbo.OrgHeader
    (OH_PK, OH_Code)
VALUES
    ('{orgHeader}', 'TESTORG1')");

			using (var cmd = Db.Connection.Command(sqlScript))
			{
				cmd.ExecuteNonQuery();
			}

			sqlScript =
				FormattableString.Invariant($@"INSERT INTO dbo.OrgContact
    (OC_PK, OC_ContactName, OC_OH, OC_Email, OC_Mobile)
VALUES
    ('{orgContactPk1}', 'Con 1', '{orgHeader}', 'email@test.com', '12345')
");

			Db.Connection.RunInTransaction(() =>
			{
				Db.Connection.ExecuteNonQuery(sqlScript);
			});

			AssertExceptionThrown<SqlException>(() =>
			{
				Db.Connection.RunInTransaction(() =>
				{
					Db.Connection.ExecuteNonQuery(sqlScript);
				});
			});

			var query = FormattableString.Invariant($"SELECT COUNT(*) FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{orgContactPk1}' AND PPR_PrimaryTableCode = 'OC'");
			using (var command = Db.Connection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(1, result);
			}

			query = FormattableString.Invariant($"SELECT COUNT(*) FROM dbo.GlbPersonPrimaryRelationship WHERE PPR_PrimaryId = '{orgContactPk2}' AND PPR_PrimaryTableCode = 'OC'");
			using (var command = Db.Connection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(0, result);
			}

			query = FormattableString.Invariant($"SELECT COUNT(*) FROM dbo.GlbPerson WHERE PER_MobilePhone = '12345'");
			using (var command = Db.Connection.Command(query))
			{
				var result = (int)command.ExecuteScalar();
				AssertEquals(1, result);
			}
		}
	}
}

