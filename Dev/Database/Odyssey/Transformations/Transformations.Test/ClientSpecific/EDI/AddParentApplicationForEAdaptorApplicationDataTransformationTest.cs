using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(AddParentApplicationForEAdaptorApplicationDataTransformation))]
	public class AddParentApplicationForEAdaptorApplicationDataTransformationTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() =>
			new AddParentApplicationForEAdaptorApplicationDataTransformation();

		protected override void AssertTransformationResults()
		{
			AssertEquals(4, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.EdiIdentityApplication WHERE IDA_ApplicationModule = 'eAdaptor' and IDA_IDA_ParentApplication Is Not Null"));
			AssertEquals(1, TestConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.EdiIdentityApplication WHERE IDA_ApplicationModule = 'eAdaptor' and IDA_IDA_ParentApplication = '{parentApplicationPK1}' and IDA_PK = '{applicationPK1}'"));
			AssertEquals(1, TestConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.EdiIdentityApplication WHERE IDA_ApplicationModule = 'eAdaptor' and IDA_IDA_ParentApplication = '{parentApplicationPK1}' and IDA_PK = '{applicationPK2}'"));
			AssertEquals(1, TestConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.EdiIdentityApplication WHERE IDA_ApplicationModule = 'eAdaptor' and IDA_IDA_ParentApplication = '{parentApplicationPK2}' and IDA_PK = '{applicationPK3}'"));
			AssertEquals(1, TestConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.EdiIdentityApplication WHERE IDA_ApplicationModule = 'eAdaptor' and IDA_IDA_ParentApplication = '{parentApplicationPK3}' and IDA_PK = '{applicationPK4}'"));
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "LicenceEnterprise", @"
CREATE TABLE LicenceEnterprise
(
	[LE_PK] UNIQUEIDENTIFIER NOT NULL,
);");
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
   [LD_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [LD_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [LD_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [LD_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
   CONSTRAINT Constraint_LD_LD_ParentDatabase CHECK (LD_LD_ParentDatabase != LD_PK),
   CONSTRAINT Constraint_LD_LicenceType CHECK (LD_LicenceType != '')
);");
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiIdentityApplication", @"
Create Table dbo.EdiIdentityApplication
(
IDA_PK uniqueidentifier NOT NULL,
IDA_LD uniqueidentifier NULL,
IDA_IDA_ParentApplication uniqueidentifier NULL,
IDA_ClientID varchar(36) NOT NULL DEFAULT '',
IDA_IsRollback bit NOT NULL Default 0,
IDA_ApplicationName varchar(256) NOT NULL DEFAULT '',
IDA_ApplicationModule nvarchar(15) NOT NULL DEFAULT '',
IDA_RedirectUrlStatus varchar(3) NOT NULL DEFAULT 'NON',
IDA_RedirectUrlLastSyncTimeUtc datetime NULL,
IDA_IsActive bit NOT NULL Default 1,
IDA_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemCreateTimeUtc smalldatetime NULL,
IDA_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemLastEditTimeUtc datetime NULL
);");
			var orgHeaderPK1 = Guid.NewGuid();

			var licenceEnterprisePK1 = Guid.NewGuid();
			var licenceEnterprisePK2 = Guid.NewGuid();

			var licenceDatabasePK1 = Guid.NewGuid();
			var licenceDatabasePK2 = Guid.NewGuid();
			var licenceDatabasePK3 = Guid.NewGuid();

			var sql = $@"INSERT INTO [OrgHeader] ([OH_PK], [OH_Code], [OH_SystemCreateTimeUtc], [OH_SystemCreateUser], [OH_SystemLastEditTimeUtc], [OH_SystemLastEditUser]) VALUES ('{orgHeaderPK1}', 'TESTORG2', '2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');

INSERT INTO [LicenceEnterprise] ([LE_PK], [LE_OH],[LE_EnterpriseCode],[LE_EnterpriseID], [LE_SystemCreateTimeUtc], [LE_SystemCreateUser], [LE_SystemLastEditTimeUtc], [LE_SystemLastEditUser]) VALUES ('{licenceEnterprisePK1}', '{orgHeaderPK1}','QDT','E999990','2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');
INSERT INTO [LicenceEnterprise] ([LE_PK], [LE_OH],[LE_EnterpriseCode],[LE_EnterpriseID], [LE_SystemCreateTimeUtc], [LE_SystemCreateUser], [LE_SystemLastEditTimeUtc], [LE_SystemLastEditUser]) VALUES ('{licenceEnterprisePK2}', '{orgHeaderPK1}','TQD','E999991','2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');

INSERT INTO dbo.LicenceDatabase(LD_PK, LD_ServerCode, LD_LicenceType, LD_DatabaseNumber, LD_LE, LD_OH_WebAccessOrg,[LD_SystemCreateTimeUtc], [LD_SystemCreateUser], [LD_SystemLastEditTimeUtc], [LD_SystemLastEditUser])
VALUES
('{licenceDatabasePK1}', 'AAA', 'PRD', 101, '{licenceEnterprisePK1}', '{orgHeaderPK1}','2025-03-03 12:00:00', 'QD', '2025-03-03 12:00:00.000', 'QD'),
('{licenceDatabasePK2}', 'BBC', 'PRD', 102, '{licenceEnterprisePK2}', '{orgHeaderPK1}','2025-03-03 12:00:00', 'QD', '2025-03-03 12:00:00.000', 'QD'),
('{licenceDatabasePK3}', 'BBD', 'PRD', 103, '{licenceEnterprisePK2}', '{orgHeaderPK1}','2025-03-03 12:00:00', 'QD', '2025-03-03 12:00:00.000', 'QD');

INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_LD,IDA_ApplicationName,IDA_ApplicationModule,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{parentApplicationPK1}', '{licenceDatabasePK1}', 'Test1','','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_LD,IDA_ApplicationName,IDA_ApplicationModule,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{parentApplicationPK2}', '{licenceDatabasePK2}','Test2','','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_LD,IDA_ApplicationName,IDA_ApplicationModule,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{parentApplicationPK3}', '{licenceDatabasePK3}','Test3','','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_LD,IDA_ApplicationName,IDA_ApplicationModule,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK1}', null, 'QDT.AAA.eAdaptor.Test1','eAdaptor','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_LD,IDA_ApplicationName,IDA_ApplicationModule,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK2}', null, 'QDT.AAA.eAdaptor.Test2','eAdaptor','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_LD,IDA_ApplicationName,IDA_ApplicationModule,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK3}', null, 'TQD.BBC.eAdaptor.Test3','eAdaptor','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_LD,IDA_ApplicationName,IDA_ApplicationModule,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK4}', null, 'TQD.BBD.eAdaptor.Test4','eAdaptor','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_LD,IDA_ApplicationName,IDA_ApplicationModule,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK5}', null, 'S2STApplicationTest','Test','E',SYSDATETIME(),'E',SYSDATETIME());
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid parentApplicationPK1 = Guid.NewGuid();
		Guid parentApplicationPK2 = Guid.NewGuid();
		Guid parentApplicationPK3 = Guid.NewGuid();
		Guid applicationPK1 = Guid.NewGuid();
		Guid applicationPK2 = Guid.NewGuid();
		Guid applicationPK3 = Guid.NewGuid();
		Guid applicationPK4 = Guid.NewGuid();
		Guid applicationPK5 = Guid.NewGuid();
	}
}
