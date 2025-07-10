using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(CleanUpWTAOnlyContacts))]
	public class CleanUpWTAOnlyContactsTest : DataTransformationTestCase
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
		Guid licenceDbNonWTAPk = Guid.NewGuid();
		Guid licenceDbWTAPk = Guid.NewGuid();
		Guid licenceDbWTAOrg2Pk = Guid.NewGuid();
		Guid contact1Pk;
		Guid contact2Pk;
		Guid contact3Pk;
		Guid contact4Pk;
		Guid contact5Pk;
		Guid contact6Pk;
		Guid orgPk;
		Guid org2Pk;

		protected override void AssertTransformationResults()
		{
			Assert("Contact2 should still be active because of Non-WTA links", ContactIsActiveAndExists(contact2Pk));
			Assert("Contact1 should be set to inactive", !ContactIsActiveAndExists(contact1Pk));
			Assert("Contact3 should be active because it only has a WTA account", ContactIsActiveAndExists(contact3Pk));
			Assert("Contact4 should be active because the matched email is for a WTA account", ContactIsActiveAndExists(contact4Pk));
			Assert("Contact5 should be inactive because it has a matched email with a non-WTA account under the same org", !ContactIsActiveAndExists(contact5Pk));
			Assert("Contact6 should be active because the matched email is in a different org", ContactIsActiveAndExists(contact6Pk));
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "LicenceDatabase", LicenceDatabaseTableCreationScript());
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "EdiCustomerUserAccount", EdiCustomerUserAccountTableCreationScript());
			var creator = new TransformationTestDataCreator();
			orgPk = creator.CreateOrg("AAA", "Test Org");
			org2Pk = creator.CreateOrg("BBB", "Test Org2");
			contact1Pk = creator.CreateContact("Contact 1", orgPk);
			contact2Pk = creator.CreateContact("Contact 2", orgPk);
			contact3Pk = creator.CreateContact("Contact 3", orgPk);
			contact4Pk = creator.CreateContact("Contact 4", orgPk);
			contact5Pk = creator.CreateContact("Contact 5", orgPk);
			contact6Pk = creator.CreateContact("Contact 5", org2Pk);
			TestConnection.ExecuteNonQuery(InsertDataQuery());
		}

		bool ContactIsActiveAndExists(Guid contactPk)
		{
			return TestConnection.Exists($"FROM [dbo].[OrgContact] WHERE OC_PK = '{contactPk}' AND OC_IsActive = 1");
		}

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
('{licenceDbWTAPk}', 'WTA', 'WTA', 101, '{orgPk}'),
('{licenceDbWTAOrg2Pk}', 'BBB', 'WTA', 102, '{org2Pk}');


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
('{account1Pk}', '{licenceDbNonWTAPk}', 'AAA', 'John Test', 'a@a.com', 0, '{contact1Pk}', 1),
('{account2Pk}', '{licenceDbWTAPk}', 'BBB', 'Jim Test', 'b@b.com', 1, '{contact1Pk}', 1),

('{account3Pk}', '{licenceDbNonWTAPk}', 'CCC', 'Jill Test', 'c@c.com', 1, '{contact2Pk}', 1),
('{account4Pk}', '{licenceDbWTAPk}', 'DDD', 'Bill Test', 'd@d.com', 1, '{contact2Pk}', 1),

('{account5Pk}', '{licenceDbWTAPk}', 'EEE', 'Lon Test', 'e@e.com', 1, '{contact3Pk}', 1),

('{account6Pk}', '{licenceDbWTAPk}', 'FFF', 'Test Name', 'same@same.com', 1, '{contact4Pk}', 1),
('{account7Pk}', '{licenceDbWTAPk}', 'GGG', 'Inactive Test', 'same@same.com', 0, NULL, 0),

('{account8Pk}', '{licenceDbWTAPk}', 'HHH', 'Test Testerson', 'match@match.com', 1, '{contact5Pk}', 1),
('{account9Pk}', '{licenceDbNonWTAPk}', 'III', 'NonWta Test', 'match@match.com', 0, NULL, 0),

('{account10Pk}', '{licenceDbWTAOrg2Pk}', 'JJJ', 'NewOrg Test', 'email@email.com', 1, '{contact6Pk}', 1),
('{account11Pk}', '{licenceDbNonWTAPk}', 'KLM', 'OldOrg NonWTA', 'email@email.com', 0, NULL, 0);
";
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new CleanUpWTAOnlyContacts();
		}
	}
}
