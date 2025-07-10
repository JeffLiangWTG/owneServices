using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(EDIDelinkInactiveSystemUserAccountsFromContactsTransform))]
	public class EDIDelinkInactiveSystemUserAccountsFromContactsTransformTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(2, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.EdiCustomerUserAccount"));

			AssertEquals(1, Db.Connection.ExecuteScalar<int>(@"SELECT COUNT(*) FROM dbo.EdiCustomerUserAccount WHERE EUA_OC_WebAccessContact IS NOT NULL and EUA_IsActive = 1;"));

			AssertEquals(1, Db.Connection.ExecuteScalar<int>(@"SELECT COUNT(*) FROM EdiCustomerUserAccount WHERE EUA_OC_WebAccessContact IS NULL and EUA_IsActive = 0"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new EDIDelinkInactiveSystemUserAccountsFromContactsTransform();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "LicenceEnterprise", @"
CREATE TABLE dbo.LicenceEnterprise
(
   [LE_PK] UNIQUEIDENTIFIER NOT NULL,
   [LE_EnterpriseCode] VARCHAR(3) NOT NULL DEFAULT '' ,
   [LE_OH] UNIQUEIDENTIFIER NOT NULL,
   [LE_IsInternal] BIT NOT NULL DEFAULT 0,
   [LE_EnterpriseID] VARCHAR(12) NOT NULL DEFAULT '' ,
);

ALTER TABLE [LicenceEnterprise]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE  [LicenceEnterprise]
ADD CONSTRAINT [PK_UC__LE_PK] PRIMARY KEY CLUSTERED  ([LE_PK] ASC)
WITH ( IGNORE_DUP_KEY = OFF)

;

CREATE NONCLUSTERED INDEX [FK_RX__LE_OH] ON [LicenceEnterprise] ([LE_OH] ASC)
WITH ( IGNORE_DUP_KEY = OFF)

;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LE_EnterpriseCode] ON [LicenceEnterprise] ([LE_EnterpriseCode] ASC)
WHERE LE_EnterpriseCode <> ''
WITH ( IGNORE_DUP_KEY = OFF)

;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__LE_EnterpriseID] ON [LicenceEnterprise] ([LE_EnterpriseID] ASC)
WITH ( IGNORE_DUP_KEY = OFF)

;

ALTER TABLE [LicenceEnterprise] WITH NOCHECK
	  ADD CONSTRAINT [LicenceEnterprise_LE_OH_FK2_OrgHeader_RRR_1210] FOREIGN KEY
		  ( [LE_OH] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

ALTER TABLE [LicenceEnterprise] WITH NOCHECK 
ADD CONSTRAINT [Constraint_LE_EnterpriseCode] CHECK (LE_EnterpriseCode = '' OR LEN(LE_EnterpriseCode) = 3)

");

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "LicenceDatabase", @"
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
   [LD_LE] UNIQUEIDENTIFIER NOT NULL,
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
   CONSTRAINT Constraint_LD_LD_ParentDatabase CHECK (LD_LD_ParentDatabase != LD_PK),
   CONSTRAINT Constraint_LD_LicenceType CHECK (LD_LicenceType != '')
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
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OC_ContractInstallerOrInternalTechContact_FK2_OrgContact_RRR_120N] FOREIGN KEY
		  ( [LD_OC_ContractInstallerOrInternalTechContact] )
		  REFERENCES [OrgContact]
		  ( [OC_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OA_SoftwareInstallAddressDetails_FK2_OrgAddress_RRR_120N] FOREIGN KEY
		  ( [LD_OA_SoftwareInstallAddressDetails] )
		  REFERENCES [OrgAddress]
		  ( [OA_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OC_LicenseeAdminContact_FK2_OrgContact_RRR_120N] FOREIGN KEY
		  ( [LD_OC_LicenseeAdminContact] )
		  REFERENCES [OrgContact]
		  ( [OC_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_LE_FK2_LicenceEnterprise_RRR_120N] FOREIGN KEY
		  ( [LD_LE] )
		  REFERENCES [LicenceEnterprise]
		  ( [LE_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OH_BillingParty_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [LD_OH_BillingParty] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK
	  ADD CONSTRAINT [LicenceDatabase_LD_OH_WebAccessOrg_FK2_OrgHeader_RRR_120N] FOREIGN KEY
		  ( [LD_OH_WebAccessOrg] )
		  REFERENCES [OrgHeader]
		  ( [OH_PK] )
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK 
ADD CONSTRAINT [LicenceDatabase_LD_LD_ParentDatabase_FK2_LicenceDatabase] FOREIGN KEY ([LD_LD_ParentDatabase]) REFERENCES [LicenceDatabase] ([LD_PK])
;

ALTER TABLE [LicenceDatabase] WITH NOCHECK 
ADD CONSTRAINT [Constraint_LD_Billable] CHECK (LD_Billable in ('X', 'Y', 'N', 'P'))
;
");

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "ClientStaff", @"
CREATE TABLE dbo.ClientStaff(
	[LS_PK] [uniqueidentifier] NOT NULL,
	[LS_LD] [uniqueidentifier] NOT NULL,
	[LS_Code] [char](3) NOT NULL DEFAULT(''),
	[LS_FullName] [nvarchar](256) NOT NULL DEFAULT(''),
	[LS_Email] [varchar](254) NOT NULL DEFAULT(''),
	[LS_IsActive] [bit] NOT NULL DEFAULT 1,
CONSTRAINT [PK_ClientStaff] PRIMARY KEY NONCLUSTERED ([LS_PK] ASC)
);

ALTER TABLE [ClientStaff]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [ClientStaff] WITH CHECK ADD CONSTRAINT [ClientStaff_LS_LD_FK_LicenceDatabase] FOREIGN KEY([LS_LD]) REFERENCES [LicenceDatabase] ([LD_PK]) ON DELETE CASCADE
;
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LS_LD_LS_FullName_LS_Code ON ClientStaff (LS_LD, LS_FullName, LS_Code) include (LS_PK)
;
CREATE UNIQUE NONCLUSTERED INDEX NR_UX__LS_LD_LS_Code ON ClientStaff (LS_LD, LS_Code) WHERE LS_Code != ''
;
CREATE CLUSTERED INDEX [NR_RC__LS_LD] ON [ClientStaff] ([LS_LD] ASC);
");

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiCustomerUserAccount", @"
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
	[EUA_SystemCreateTimeUtc]  SMALLDATETIME NULL,
	[EUA_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
	[EUA_SystemLastEditTimeUtc]  SMALLDATETIME NULL,
	[EUA_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''

CONSTRAINT [PK_EdiCustomerUserAccount] PRIMARY KEY NONCLUSTERED ([EUA_PK] ASC)
);

ALTER TABLE [EdiCustomerUserAccount]
	SET (LOCK_ESCALATION = DISABLE);

ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [EdiCustomerUserAccount_EUA_LD_FK_LicenceDatabase] FOREIGN KEY([EUA_LD]) REFERENCES [LicenceDatabase] ([LD_PK]) ON DELETE CASCADE;
ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [EdiCustomerUserAccount_EUA_OC_WebAccessContact_FK_OrgContact] FOREIGN KEY([EUA_OC_WebAccessContact]) REFERENCES [OrgContact] ([OC_PK]);
ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [EdiCustomerUserAccount_EUA_LS_FK_ClientStaff] FOREIGN KEY([EUA_LS]) REFERENCES [ClientStaff] ([LS_PK]);
ALTER TABLE [EdiCustomerUserAccount] WITH CHECK ADD CONSTRAINT [Constraint_EUA_UserID] CHECK (EUA_UserID != '');
CREATE UNIQUE CLUSTERED INDEX NR_UC__EUA_LD_EUA_UserID ON EdiCustomerUserAccount (EUA_LD, EUA_UserID);
CREATE INDEX FK_RX__EUA_OC_WebAccessContact ON EdiCustomerUserAccount (EUA_OC_WebAccessContact) WHERE EUA_OC_WebAccessContact IS NOT NULL;
");

			var oH_PK1 = Guid.NewGuid();
			var oc_PK1 = Guid.NewGuid();
			var oc_PK2 = Guid.NewGuid();
			var lE_PK1 = Guid.NewGuid();
			var ld_PK1 = Guid.NewGuid();

			var sql = $@"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsActive, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser, OH_SystemCreateTimeUtc, OH_SystemCreateUser)
VALUES
('{oH_PK1}', 'TESTORG1', 'Test Organisation1', 1, getutcdate(), 'E', getutcdate(), 'E');

INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser, OC_SystemCreateTimeUtc, OC_SystemCreateUser) VALUES
('{oc_PK1}', '{oH_PK1}', 'Contact 1', getutcdate(), 'E', getutcdate(), 'E'),
('{oc_PK2}', '{oH_PK1}', 'Contact 2', getutcdate(), 'E', getutcdate(), 'E')

INSERT INTO dbo.LicenceEnterprise (LE_PK, LE_OH, LE_EnterpriseCode, LE_EnterpriseID, LE_SystemCreateTimeUtc, LE_SystemCreateUser, LE_SystemLastEditTimeUtc, LE_SystemLastEditUser )
VALUES
('{lE_PK1}', '{oH_PK1}', 'MIQ', 'ID111', getutcdate(), 'E', getutcdate(), 'E')

INSERT INTO dbo.LicenceDatabase (LD_PK, LD_ServerCode, LD_LicenceType, LD_DatabaseNumber, LD_LE, LD_OH_WebAccessOrg)
VALUES
('{ld_PK1}', 'D01', 'PRD', 101, '{lE_PK1}', '{oH_PK1}')

INSERT INTO [dbo].[EdiCustomerUserAccount]
           ([EUA_PK]
           ,[EUA_LD]
           ,[EUA_UserID]
           ,[EUA_FullName]
           ,[EUA_Email]
           ,[EUA_IsActive]
           ,[EUA_OC_WebAccessContact]
           ,[EUA_LS]
           ,[EUA_ContactRelationshipStatus]
           ,[EUA_IsContactRelationshipActive]
           ,[EUA_SystemVerifiedDateUtc]
           ,[EUA_UserVerifiedDateUtc]
           ,[EUA_IsEmailVerificationRequired]
           ,[EUA_RN_NKCountry]
           ,[EUA_IsEmailOverridden]
           ,[EUA_PreviousEmail]
           ,[EUA_SystemCreateTimeUtc]
           ,[EUA_SystemCreateUser]
           ,[EUA_SystemLastEditTimeUtc]
           ,[EUA_SystemLastEditUser])
     VALUES
           (NEWID()
           ,'{ld_PK1}'    --EUA_LD
           ,'U01'       --EUA_UserID
           ,'Contact 1' --EUA_FullName
           ,'user1@cw1.com' --EUA_Email
           ,1 -- EUA_IsActive
           ,'{oc_PK1}' --EUA_OC_WebAccessContact
           ,NULL --EUA_LS
           ,'' --EUA_ContactRelationshipStatus
           ,1-- EUA_IsContactRelationshipActive
           ,GETUTCDATE() --EUA_SystemVerifiedDateUtc
           ,GETUTCDATE() --EUA_UserVerifiedDateUtc
           ,1 --EUA_IsEmailVerificationRequired
           ,'AU' --EUA_RN_NKCountry
           ,0    --EUA_IsEmailOverridden
           ,''    --EUA_PreviousEmail
           ,getutcdate()
           ,'E'
           ,getutcdate()
           ,'E')

		  ,(NEWID()
           ,'{ld_PK1}'    --EUA_LD
           ,'U02'       --EUA_UserID
           ,'Contact 2' --EUA_FullName
           ,'user2@cw1.com' --EUA_Email
           ,0 -- EUA_IsActive
           ,'{oc_PK2}' --EUA_OC_WebAccessContact
           ,NULL --EUA_LS
           ,'' --EUA_ContactRelationshipStatus
           ,1-- EUA_IsContactRelationshipActive
           ,GETUTCDATE() --EUA_SystemVerifiedDateUtc
           ,GETUTCDATE() --EUA_UserVerifiedDateUtc
           ,1 --EUA_IsEmailVerificationRequired
           ,'AU' --EUA_RN_NKCountry
           ,0    --EUA_IsEmailOverridden
           ,''    --EUA_PreviousEmail
           ,getutcdate()
           ,'E'
           ,getutcdate()
           ,'E')
";
			Db.Connection.ExecuteNonQuery(sql);

			AssertEquals(2, Db.Connection.ExecuteScalar<int>("SELECT COUNT(*)	FROM dbo.EdiCustomerUserAccount"));

			AssertEquals(2, Db.Connection.ExecuteScalar<int>(@"SELECT COUNT(*)	FROM dbo.EdiCustomerUserAccount WHERE EUA_OC_WebAccessContact IS NOT NULL;"));

			AssertEquals(1, Db.Connection.ExecuteScalar<int>(@"SELECT COUNT(*) FROM EdiCustomerUserAccount WHERE EUA_IsActive = 0"));
		}
	}
}
