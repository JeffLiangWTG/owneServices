using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(FixWronglyDeactivatedContacts))]
	public class FixWronglyDeactivatedContactsTest : DataTransformationTestCase
	{
		Guid account1Pk = Guid.NewGuid();
		Guid account2Pk = Guid.NewGuid();
		Guid account3Pk = Guid.NewGuid();
		Guid account4Pk = Guid.NewGuid();
		Guid account5Pk = Guid.NewGuid();
		Guid account6Pk = Guid.NewGuid();
		Guid account7Pk = Guid.NewGuid();
		Guid account8Pk = Guid.NewGuid();
		Guid account9Pk = Guid.NewGuid();
		Guid account10Pk = Guid.NewGuid();
		Guid account11Pk = Guid.NewGuid();
		Guid account12Pk = Guid.NewGuid();
		Guid account13Pk = Guid.NewGuid();
		Guid account14Pk = Guid.NewGuid();
		Guid account15Pk = Guid.NewGuid();
		Guid account16Pk = Guid.NewGuid();
		Guid account17Pk = Guid.NewGuid();
		Guid account18Pk = Guid.NewGuid();
		Guid account19Pk = Guid.NewGuid();
		Guid account20Pk = Guid.NewGuid();
		Guid account21Pk = Guid.NewGuid();
		Guid account22Pk = Guid.NewGuid();
		Guid account23Pk = Guid.NewGuid();
		Guid account24Pk = Guid.NewGuid();
		Guid account25Pk = Guid.NewGuid();
		Guid account26Pk = Guid.NewGuid();
		Guid licenceDbNonWTAPk = Guid.NewGuid();
		Guid licenceDbWTAPk = Guid.NewGuid();
		Guid contact1Pk;
		Guid contact2Pk;
		Guid contact3Pk;
		Guid contact4Pk;
		Guid contactWithOlderlastEditTimePk;
		Guid contactWithInactiveRelationshipPk;
		Guid duplicateContactA;
		Guid duplicateContactB;
		Guid duplicateInactiveContactA;
		Guid duplicateInactiveContactB;
		Guid duplicateWebAccessDisabledContactA;
		Guid duplicateWebAccessDisabledContactB;
		Guid duplicateNoEmailContactA;
		Guid duplicateNoEmailContactB;
		Guid orgPk;

		protected override void AssertTransformationResults()
		{
			Assert("Contact1 should remain inactive because it only has a WTA user account", !ContactIsActiveAndExists(contact1Pk));
			Assert("Contact2 should remain inactive because it did not satisfy the previous faulty condition of having at least one active and at least one inactive non-WTA user account", !ContactIsActiveAndExists(contact2Pk));
			Assert("Contact3 should remain inactive because its non-WTA user account is inactive", !ContactIsActiveAndExists(contact3Pk));
			Assert("Contact4 should be reactivated because it has an active non-WTA user account", ContactIsActiveAndExists(contact4Pk));
			Assert("contactWithOlderlastEditTime should remain inactive because its last edit date is earlier than the errant transformation", !ContactIsActiveAndExists(contactWithOlderlastEditTimePk));
			Assert("contactWithInactiveRelationship should be reactivated regardless of its active user account having an inactive contact relationship", ContactIsActiveAndExists(contactWithInactiveRelationshipPk));
			Assert("duplicateContactB should not be reactivated because there's another contact in the org with matching email, OC_IsActive = 1 and OC_WebAccesEnabled = 1", !ContactIsActiveAndExists(duplicateContactB));
			Assert("duplicateInactiveContactB should be reactivated because the duplicate contact in the org with matching email is inactive", ContactIsActiveAndExists(duplicateInactiveContactB));
			Assert("duplicateWebAccessDisabledContactB should be reactivated because the duplicate contact in the org with matching email has no web access", ContactIsActiveAndExists(duplicateWebAccessDisabledContactB));
			Assert("duplicateNoEmailContactB should be reactivated because the duplicate contact in the org's email is empty", ContactIsActiveAndExists(duplicateNoEmailContactB));
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "LicenceDatabase", LicenceDatabaseTableCreationScript());
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "EdiCustomerUserAccount", EdiCustomerUserAccountTableCreationScript());
			var creator = new TransformationTestDataCreator();
			orgPk = creator.CreateOrg("AAA", "Test Org");
			contact1Pk = creator.CreateContact("Contact 1", orgPk);
			contact2Pk = creator.CreateContact("Contact 2", orgPk);
			contact3Pk = creator.CreateContact("Contact 3", orgPk);
			contact4Pk = creator.CreateContact("Contact 4", orgPk);
			contactWithOlderlastEditTimePk = creator.CreateContact("Contact with older last edit time", orgPk);
			contactWithInactiveRelationshipPk = creator.CreateContact("Contact with inactive relationship", orgPk);
			duplicateContactA = creator.CreateContact("Duplicate test 1 Contact A", orgPk);
			duplicateContactB = creator.CreateContact("Duplicate test 1 Contact B", orgPk);
			duplicateInactiveContactA = creator.CreateContact("Duplicate test 2 Contact A", orgPk);
			duplicateInactiveContactB = creator.CreateContact("Duplicate test 2 Contact B", orgPk);
			duplicateWebAccessDisabledContactA = creator.CreateContact("Duplicate test 3 Contact A", orgPk);
			duplicateWebAccessDisabledContactB = creator.CreateContact("Duplicate test 3 Contact B", orgPk);
			duplicateNoEmailContactA = creator.CreateContact("Duplicate test 4 Contact A", orgPk);
			duplicateNoEmailContactB = creator.CreateContact("Duplicate test 4 Contact B", orgPk);
			TestConnection.ExecuteNonQuery(InsertDataQuery());
		}

		bool ContactIsActiveAndExists(Guid contactPk)
		{
			return TestConnection.Exists($"FROM [dbo].[OrgContact] WHERE OC_PK = '{contactPk}' AND OC_IsActive = 1");
		}

		public override void TestNewIndex()
		{
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "LicenceDatabase", LicenceDatabaseTableCreationScript());
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "EdiCustomerUserAccount", EdiCustomerUserAccountTableCreationScript());
			base.TestNewIndex();
		}

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Fix edge case deactivated contacts from CleanUpWTAOnlyContacts_1] ON [dbo].[EdiCustomerUserAccount] ([EUA_IsActive], [EUA_OC_WebAccessContact]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		string LicenceDatabaseTableCreationScript()
		{
			return @"
CREATE TABLE dbo.LicenceDatabase
(
   [LD_PK] UNIQUEIDENTIFIER NOT NULL,
   [LD_IsActive] BIT NOT NULL DEFAULT 1 ,
   [LD_ServerCode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_LicenceType] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_LicenceExpiry] SMALLDATETIME NULL,
   [LD_PurchasedLicenceUnits] INT NOT NULL DEFAULT 0 ,
   [LD_LastHeartbeat] SMALLDATETIME NULL,
   [LD_ReleaseRing] VARCHAR(3) NOT NULL DEFAULT 'GPR' ,
   [LD_AvailableUpgradeMethod] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_DBServerSecurityMode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_PublicEmailAddressForUpdate] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_InternalPop3EmailAddress] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_InternalPop3UserName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_InternalPop3Port] INT NOT NULL DEFAULT 0 ,
   [LD_InternalSmtpEmailAddress] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_InternalSmtpPort] INT NOT NULL DEFAULT 0 ,
   [LD_HL_CurrentRunningVersion] UNIQUEIDENTIFIER NULL,
   [LD_HL_CurrentSentVersion] UNIQUEIDENTIFIER NULL,
   [LD_HostServerSID] UNIQUEIDENTIFIER NULL,
   [LD_HostServerName] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_HostDBName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_HostDBInstance] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_ReportedHostServerName] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_ReportedHostDBName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_ReportedHostDBInstance] VARCHAR(128) NOT NULL DEFAULT '' ,
   [LD_ScheduleStateUPG] VARCHAR(4096) DEFAULT '',
   [LD_ScheduleStateMUG] VARCHAR(4096) DEFAULT '',
   [LD_NextRunTimeUtcUPG] SMALLDATETIME NULL,
   [LD_NextRunTimeUtcMUG] SMALLDATETIME NULL,
   [LD_RetryTimeoutInMinutes] TINYINT NOT NULL DEFAULT 0 ,
   [LD_MaxDataInMegBeforeAck] DECIMAL(9,1) NOT NULL DEFAULT 0 ,
   [LD_OC_ContractInstallerOrInternalTechContact] UNIQUEIDENTIFIER NULL,
   [LD_OA_SoftwareInstallAddressDetails] UNIQUEIDENTIFIER NULL,
   [LD_OC_LicenseeAdminContact] UNIQUEIDENTIFIER NULL,
   [LD_NoOfActivePrintQueues] SMALLINT NOT NULL DEFAULT 0 ,
   [LD_LogFileOnDifferentPhysicalVolume] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LD_DatabaseFilePathDetail] VARCHAR(MAX) NOT NULL DEFAULT '' ,
   [LD_SQLEdition] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LD_SQLVersion] VARCHAR(10) NOT NULL DEFAULT '' ,
   [LD_SQLVerString] VARCHAR(MAX) NOT NULL DEFAULT '' ,
   [LD_OSName] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_OSVersion] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_SystemManufacturer] VARCHAR(64) NOT NULL DEFAULT '' ,
   [LD_BIOSDate] SMALLDATETIME NULL,
   [LD_TotalPhysicalMemoryMB] INT NOT NULL DEFAULT 0 ,
   [LD_AvailablePhysicalMemoryMB] INT NOT NULL DEFAULT 0 ,
   [LD_ProcessorType] VARCHAR(256) NOT NULL DEFAULT '' ,
   [LD_ProcessorReleaseDate] SMALLDATETIME NULL,
   [LD_ProcessorSpeedMHz] DECIMAL(9,3) NOT NULL DEFAULT 0 ,
   [LD_NoOfProcessorCores] INT NOT NULL DEFAULT 0 ,
   [LD_VirtualMachineDetected] CHAR(1) NOT NULL DEFAULT 'N' ,
   [LD_HostedLocation] CHAR(3) NOT NULL DEFAULT 'NCW' ,
   [LD_LegacyInterfaceSupport] CHAR(1) NOT NULL DEFAULT 'Y' ,
   [LD_OH_BillingParty] UNIQUEIDENTIFIER NULL,
   [LD_LE] UNIQUEIDENTIFIER NULL,
   [LD_DatabaseNumber] INT NOT NULL DEFAULT 0,
   [LD_Status] varchar(3) NOT NULL default '',
   [LD_HostDBCreateDate] datetime NULL,
   [LD_HostGroupId] UNIQUEIDENTIFIER NULL,
   [LD_Password] VARCHAR(200) NOT NULL DEFAULT '',
   [LD_Product] VARCHAR(3) NOT NULL DEFAULT '',
   [LD_HostConnectionServerName] varchar(255) NOT NULL DEFAULT '',
   [LD_CanReregisterToSameServer] bit not null default (0),
   [LD_CurrentVersionFirstReportUtc] smalldatetime null,
   [LD_CurrentVersionLastReportUtc] smalldatetime null,
   [LD_LD_ParentDatabase] uniqueidentifier NULL,
   [LD_IsBilledPerCompany] bit not null default(0),
   [LD_ManualLicenceExpiry] SMALLDATETIME NULL,
   [LD_GS_NKOwner] VARCHAR(3) NOT NULL DEFAULT '',
   [LD_Billable] CHAR(1) NOT NULL DEFAULT 'X',
   [LD_DatabaseConfig] VARCHAR(8000) NOT NULL DEFAULT '',
   [LD_AllowAutoLogin] BIT NOT NULL DEFAULT 1,
   [LD_StaffFirstReportUtc] SMALLDATETIME NULL,
   [LD_OH_WebAccessOrg] uniqueidentifier NULL,
   [LD_TenantID] VARCHAR(50) NOT NULL DEFAULT '',
   [LD_PreRegistrationExpiryDateUTC] SMALLDATETIME NULL,
   [LD_ETS_TrustedSystem] UNIQUEIDENTIFIER NULL,
   [LD_MasterOrgSuggestedUTC] SMALLDATETIME NULL,
   [LD_OutboundEAdaptorUrl] VARCHAR(2048) NULL,
   [LD_TokenAuthenticationEnabled] BIT NOT NULL DEFAULT 0,
   [LD_FeatureControlRuleLastSyncUtc] SMALLDATETIME NULL,
   [LD_FeatureControlRuleLastSyncContent] NVARCHAR(MAX) NULL,
   [LD_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [LD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [LD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [LD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
);

ALTER TABLE [LicenceDatabase]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [LicenceDatabase]
ADD CONSTRAINT [PK_UC__LD_PK] PRIMARY KEY CLUSTERED  ([LD_PK] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_HL_CurrentRunningVersion] ON [LicenceDatabase] ([LD_HL_CurrentRunningVersion] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_HL_CurrentSentVersion] ON [LicenceDatabase] ([LD_HL_CurrentSentVersion] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OC_ContractInstallerOrInternalTechContact] ON [LicenceDatabase] ([LD_OC_ContractInstallerOrInternalTechContact] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OA_SoftwareInstallAddressDetails] ON [LicenceDatabase] ([LD_OA_SoftwareInstallAddressDetails] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OC_LicenseeAdminContact] ON [LicenceDatabase] ([LD_OC_LicenseeAdminContact] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_LE] ON [LicenceDatabase] ([LD_LE] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OH_BillingParty] ON [LicenceDatabase] ([LD_OH_BillingParty] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_LD_ParentDatabase] ON [LicenceDatabase] ([LD_LD_ParentDatabase] ASC);

CREATE NONCLUSTERED INDEX [NR_RX__LD_GS_NKOwner] ON [LicenceDatabase] ([LD_GS_NKOwner] ASC)
WHERE LD_GS_NKOwner != ''
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LD_DatabaseNumber] ON [LicenceDatabase] ([LD_DatabaseNumber] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LD_ServerCode_LD_LE] ON [LicenceDatabase] ([LD_ServerCode] ASC, LD_LE)
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LD_Product_LD_TenantID] ON [LicenceDatabase] ([LD_Product] ASC, [LD_TenantID] ASC)
WHERE LD_Product <> '' AND LD_TenantID <> ''
WITH ( IGNORE_DUP_KEY = OFF)
;

CREATE NONCLUSTERED INDEX [FK_RX__LD_OH_WebAccessOrg] ON [LicenceDatabase] ([LD_OH_WebAccessOrg] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;";
		}

		string EdiCustomerUserAccountTableCreationScript()
		{
			return @"
CREATE TABLE dbo.EdiCustomerUserAccount(
	[EUA_PK] [uniqueidentifier] NOT NULL,
	[EUA_LD] [uniqueidentifier] NOT NULL,
	[EUA_UserID] [varchar](36) NOT NULL DEFAULT(''),
	[EUA_FullName] [nvarchar](256) NOT NULL DEFAULT(''),
	[EUA_Email] [varchar](254) NOT NULL DEFAULT(''),
	[EUA_PreviousEmail] [varchar](254) NOT NULL DEFAULT(''),
	[EUA_IsActive] [bit] NOT NULL DEFAULT 1,
	[EUA_OC_WebAccessContact] [uniqueidentifier] NULL,
	[EUA_LS] [uniqueidentifier] NULL,
	[EUA_IsContactRelationshipActive] [bit] NOT NULL DEFAULT 1,
	[EUA_ContactRelationshipStatus] [varchar](3) NOT NULL DEFAULT(''),
	[EUA_IsEmailVerificationRequired] [bit] NOT NULL DEFAULT 0,
	[EUA_SystemVerifiedDateUtc] [smalldatetime] NULL,
	[EUA_UserVerifiedDateUtc] [smalldatetime] NULL,
	[EUA_RN_NKCountry] [varchar](2) NOT NULL DEFAULT(''),
	[EUA_IsEmailOverridden] [bit] NOT NULL DEFAULT 0,
	[EUA_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[EUA_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
	[EUA_SystemLastEditTimeUtc] SMALLDATETIME NULL,
	[EUA_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',

CONSTRAINT [PK_EdiCustomerUserAccount] PRIMARY KEY NONCLUSTERED ([EUA_PK] ASC)
);

ALTER TABLE [EdiCustomerUserAccount]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [EdiCustomerUserAccount_EUA_LD_FK_LicenceDatabase] FOREIGN KEY([EUA_LD]) REFERENCES [LicenceDatabase] ([LD_PK]) ON DELETE CASCADE;
ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [EdiCustomerUserAccount_EUA_OC_WebAccessContact_FK_OrgContact] FOREIGN KEY([EUA_OC_WebAccessContact]) REFERENCES [OrgContact] ([OC_PK]);
ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [Constraint_EUA_UserID] CHECK (EUA_UserID != '');
CREATE UNIQUE CLUSTERED INDEX NR_UC__EUA_LD_EUA_UserID ON EdiCustomerUserAccount (EUA_LD, EUA_UserID);
CREATE INDEX FK_RX__EUA_OC_WebAccessContact ON EdiCustomerUserAccount (EUA_OC_WebAccessContact) WHERE EUA_OC_WebAccessContact IS NOT NULL;
";
		}

		string InsertDataQuery()
		{
			return @$"
INSERT INTO [dbo].[LicenceDatabase] (
	[LD_PK],
	[LD_ServerCode],
	[LD_Product],
	[LD_DatabaseNumber],
	[LD_OH_WebAccessOrg]
)
VALUES
('{licenceDbNonWTAPk}', 'BNE', 'CW1', 100, '{orgPk}'),
('{licenceDbWTAPk}', 'WTA', 'WTA', 101, '{orgPk}');

UPDATE [dbo].[OrgContact]
SET OC_IsActive = 1, OC_WebAccessEnabled = 1, OC_Email = 'duplicate1@email.com', OC_SystemLastEditTimeUtc = GETUTCDATE(), OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{duplicateContactA}');

UPDATE [dbo].[OrgContact]
SET OC_IsActive = 0, OC_WebAccessEnabled = 1, OC_Email = 'duplicate2@email.com', OC_SystemLastEditTimeUtc = '2024-01-01', OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{duplicateInactiveContactA}');

UPDATE [dbo].[OrgContact]
SET OC_IsActive = 1, OC_WebAccessEnabled = 0, OC_Email = 'duplicate3@email.com', OC_SystemLastEditTimeUtc = '2024-01-01', OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{duplicateWebAccessDisabledContactA}');

UPDATE [dbo].[OrgContact]
SET OC_IsActive = 1, OC_WebAccessEnabled = 1, OC_Email = '', OC_SystemLastEditTimeUtc = '2024-01-01', OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{duplicateNoEmailContactA}');

UPDATE [dbo].[OrgContact]
SET OC_IsActive = 0, OC_SystemLastEditTimeUtc = GETUTCDATE(), OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{contact1Pk}', '{contact2Pk}', '{contact3Pk}', '{contact4Pk}', '{contactWithInactiveRelationshipPk}', '{duplicateContactB}', '{duplicateInactiveContactB}', '{duplicateWebAccessDisabledContactB}', '{duplicateNoEmailContactB}');

UPDATE [dbo].[OrgContact]
SET OC_Email = 'duplicate1@email.com', OC_WebAccessEnabled = 1, OC_SystemLastEditTimeUtc = GETUTCDATE(), OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{duplicateContactB}');

UPDATE [dbo].[OrgContact]
SET OC_Email = 'duplicate2@email.com', OC_WebAccessEnabled = 1, OC_SystemLastEditTimeUtc = GETUTCDATE(), OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{duplicateInactiveContactB}');

UPDATE [dbo].[OrgContact]
SET OC_Email = 'duplicate3@email.com', OC_WebAccessEnabled = 1, OC_SystemLastEditTimeUtc = GETUTCDATE(), OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{duplicateWebAccessDisabledContactB}');

UPDATE [dbo].[OrgContact]
SET OC_IsActive = 0, OC_SystemLastEditTimeUtc = '2025-03-18', OC_SystemLastEditUser = '~BP'
WHERE OC_PK IN ('{contactWithOlderlastEditTimePk}');

INSERT INTO [dbo].[EdiCustomerUserAccount] (
	[EUA_PK],
	[EUA_LD],
	[EUA_UserID],
	[EUA_FullName],
	[EUA_Email],
	[EUA_IsActive],
	[EUA_OC_WebAccessContact],
	[EUA_IsContactRelationshipActive]
)
VALUES
('{account1Pk}', '{licenceDbWTAPk}', 'AAA', 'Jim Test', 'a@a.com', 1, '{contact1Pk}', 1),

('{account2Pk}', '{licenceDbNonWTAPk}', 'BBB', 'Jill Test', 'b@b.com', 1, '{contact2Pk}', 1),
('{account3Pk}', '{licenceDbWTAPk}', 'CCC', 'Bill Test', 'c@c.com', 1, '{contact2Pk}', 1),

('{account4Pk}', '{licenceDbWTAPk}', 'DDD', 'Lon Test', 'd@d.com', 1, '{contact3Pk}', 1),
('{account5Pk}', '{licenceDbNonWTAPk}', 'EEE', 'Test Test', 'e@e.com', 0, '{contact3Pk}', 1),

('{account6Pk}', '{licenceDbWTAPk}', 'FFF', 'Test Name', 'f@f.com', 1, '{contact4Pk}', 1),
('{account7Pk}', '{licenceDbNonWTAPk}', 'GGG', 'Inactive Test', 'g@g.com', 0, '{contact4Pk}', 1),
('{account8Pk}', '{licenceDbNonWTAPk}', 'HHH', 'Active Test', 'h@h.com', 1, '{contact4Pk}', 1),

('{account9Pk}', '{licenceDbWTAPk}', 'III', 'Old WTA Test', 'i@i.com', 1, '{contactWithOlderlastEditTimePk}', 1),
('{account10Pk}', '{licenceDbNonWTAPk}', 'JJJ', 'Old Active Test', 'j@j.com', 1, '{contactWithOlderlastEditTimePk}', 1),
('{account11Pk}', '{licenceDbNonWTAPk}', 'KKK', 'Old Inactive Test', 'k@k.com', 0, '{contactWithOlderlastEditTimePk}', 1),

('{account12Pk}', '{licenceDbWTAPk}', 'LLL', 'Ok Bill Test', 'l@l.com', 1, '{contactWithInactiveRelationshipPk}', 1),
('{account13Pk}', '{licenceDbNonWTAPk}', 'MMM', 'Active with bad relationship Test', 'm@m.com', 1, '{contactWithInactiveRelationshipPk}', 0),
('{account14Pk}', '{licenceDbNonWTAPk}', 'NNN', 'Inactive good relationship Test', 'n@n.com', 0, '{contactWithInactiveRelationshipPk}', 1),

('{account15Pk}', '{licenceDbWTAPk}', 'OOO', 'Duplicate Test 1', 'o@o.com', 1, '{duplicateContactB}', 1),
('{account16Pk}', '{licenceDbNonWTAPk}', 'PPP', 'Duplicate Test 1 Inactive', 'p@p.com', 1, '{duplicateContactB}', 0),
('{account17Pk}', '{licenceDbNonWTAPk}', 'QQQ', 'Duplicate Test 1 Active', 'q@q.com', 0, '{duplicateContactB}', 1),

('{account18Pk}', '{licenceDbWTAPk}', 'RRR', 'Duplicate Test 2', 'r@r.com', 1, '{duplicateInactiveContactB}', 1),
('{account19Pk}', '{licenceDbNonWTAPk}', 'SSS', 'Duplicate Test 2 Inactive', 's@s.com', 1, '{duplicateInactiveContactB}', 0),
('{account20Pk}', '{licenceDbNonWTAPk}', 'TTT', 'Duplicate Test 2 Active', 't@t.com', 0, '{duplicateInactiveContactB}', 1),

('{account21Pk}', '{licenceDbWTAPk}', 'UUU', 'Duplicate Test 3', 'u@u.com', 1, '{duplicateWebAccessDisabledContactB}', 1),
('{account22Pk}', '{licenceDbNonWTAPk}', 'VVV', 'Duplicate Test 3 Inactive', 'v@v.com', 1, '{duplicateWebAccessDisabledContactB}', 0),
('{account23Pk}', '{licenceDbNonWTAPk}', 'WWW', 'Duplicate Test 3 Active', 'w@w.com', 0, '{duplicateWebAccessDisabledContactB}', 1),

('{account24Pk}', '{licenceDbWTAPk}', 'XXX', 'Duplicate Test 4', 'x@x.com', 1, '{duplicateNoEmailContactB}', 1),
('{account25Pk}', '{licenceDbNonWTAPk}', 'YYY', 'Duplicate Test 4 Inactive', 'y@y.com', 1, '{duplicateNoEmailContactB}', 0),
('{account26Pk}', '{licenceDbNonWTAPk}', 'ZZZ', 'Duplicate Test 4 Active', 'z@z.com', 0, '{duplicateNoEmailContactB}', 1);
";
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new FixWronglyDeactivatedContacts();
		}
	}
}
