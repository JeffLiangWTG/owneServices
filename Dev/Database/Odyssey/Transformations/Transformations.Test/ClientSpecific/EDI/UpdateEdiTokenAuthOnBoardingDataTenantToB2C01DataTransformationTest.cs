using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateEdiTokenAuthOnBoardingDataTenantToB2C01DataTransformation))]
	class UpdateEdiTokenAuthOnBoardingDataTenantToB2C01DataTransformationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(3, TestConnection.ExecuteScalar<int>($"select count(1) from EdiTokenAuthOnBoardingData where TOD_IDT = '{tenantPK}'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateEdiTokenAuthOnBoardingDataTenantToB2C01DataTransformation();

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "LicenceEnterprise", @"
CREATE TABLE LicenceEnterprise
(
	[LE_PK] UNIQUEIDENTIFIER NOT NULL
);");

			DbObjectCreator.CreateTableIfNotExists(TestConnection, "IncidentMain", @"
CREATE TABLE IncidentMain
(
	[IM_PK] UNIQUEIDENTIFIER NOT NULL,
	[IM_IncidentNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
	[IM_Source] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_OH_Client] UNIQUEIDENTIFIER NULL,
	[IM_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[IM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_SystemLastEditTimeUtc] DATETIME NULL,
	[IM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
);");

			DbObjectCreator.CreateTableIfNotExists(TestConnection, "EdiIdentityTenant", @"
CREATE TABLE dbo.EdiIdentityTenant
(
IDT_PK uniqueidentifier NOT NULL,
IDT_TenantId varchar(36) NOT NULL,
IDT_OidcClientId varchar(36) NOT NULL,
IDT_Name varchar(64) NOT NULL,
IDT_AuthorityUrl varchar(128) NOT NULL DEFAULT '',
IDT_GraphClientId varchar(36) NOT NULL,
IDT_Onboarding bit NOT NULL DEFAULT 0,
IDT_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDT_SystemCreateTimeUtc smalldatetime NULL,
IDT_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDT_SystemLastEditTimeUtc datetime NULL
);");
			DbObjectCreator.CreateTableIfNotExists(TestConnection, "EdiTokenAuthOnBoardingData", @"
CREATE TABLE dbo.EdiTokenAuthOnBoardingData 
(
	[TOD_PK] uniqueidentifier NOT NULL,
	[TOD_SystemUniqueIdentifier] varchar(100) NOT NULL,
	[TOD_LE] uniqueidentifier NOT NULL,
	[TOD_ConfigurationIdentifier] varchar(20) NOT NULL,
	[TOD_OIDCServer] varchar(3) NOT NULL DEFAULT 'AZU',
	[TOD_ClaimMappingName] varchar(50) NOT NULL,
	[TOD_ClaimMappingIdentifier] varchar(50) NOT NULL,
	[TOD_Status] varchar(3) NOT NULL DEFAULT 'NEW',
	[TOD_IM] uniqueidentifier NOT NULL,
);");
			var orgHeaderPK1 = Guid.NewGuid();
			var orgHeaderPK2 = Guid.NewGuid();
			var orgHeaderPK3 = Guid.NewGuid();

			var licenceEnterprisePK1 = Guid.NewGuid();
			var licenceEnterprisePK2 = Guid.NewGuid();
			var licenceEnterprisePK3 = Guid.NewGuid();

			var incidentPK1 = Guid.NewGuid();
			var incidentPK2 = Guid.NewGuid();
			var incidentPK3 = Guid.NewGuid();

			var ediTokenAuthOnBoardingDataPK1 = Guid.NewGuid();
			var ediTokenAuthOnBoardingDataPK2 = Guid.NewGuid();
			var ediTokenAuthOnBoardingDataPK3 = Guid.NewGuid();

			var sql = $@"INSERT INTO [OrgHeader] ([OH_PK], [OH_Code], [OH_SystemCreateTimeUtc], [OH_SystemCreateUser], [OH_SystemLastEditTimeUtc], [OH_SystemLastEditUser]) VALUES ('{orgHeaderPK1}', 'TESTORG1', '2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');
INSERT INTO [OrgHeader] ([OH_PK], [OH_Code], [OH_SystemCreateTimeUtc], [OH_SystemCreateUser], [OH_SystemLastEditTimeUtc], [OH_SystemLastEditUser]) VALUES ('{orgHeaderPK2}', 'TESTORG2', '2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');
INSERT INTO [OrgHeader] ([OH_PK], [OH_Code], [OH_SystemCreateTimeUtc], [OH_SystemCreateUser], [OH_SystemLastEditTimeUtc], [OH_SystemLastEditUser]) VALUES ('{orgHeaderPK3}', 'TESTORG3', '2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');

INSERT INTO [LicenceEnterprise] ([LE_PK], [LE_OH],[LE_EnterpriseID], [LE_SystemCreateTimeUtc], [LE_SystemCreateUser], [LE_SystemLastEditTimeUtc], [LE_SystemLastEditUser]) VALUES ('{licenceEnterprisePK1}', '{orgHeaderPK1}', 'E999990','2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');
INSERT INTO [LicenceEnterprise] ([LE_PK], [LE_OH],[LE_EnterpriseID], [LE_SystemCreateTimeUtc], [LE_SystemCreateUser], [LE_SystemLastEditTimeUtc], [LE_SystemLastEditUser]) VALUES ('{licenceEnterprisePK2}', '{orgHeaderPK2}', 'E999991','2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');
INSERT INTO [LicenceEnterprise] ([LE_PK], [LE_OH],[LE_EnterpriseID], [LE_SystemCreateTimeUtc], [LE_SystemCreateUser], [LE_SystemLastEditTimeUtc], [LE_SystemLastEditUser]) VALUES ('{licenceEnterprisePK3}', '{orgHeaderPK3}', 'E999992','2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');

INSERT INTO [IncidentMain]([IM_PK], [IM_IncidentNumber], [IM_Source], [IM_SystemCreateTimeUtc], [IM_SystemCreateUser], [IM_SystemLastEditTimeUtc], [IM_SystemLastEditUser]) VALUES ('{incidentPK1}', 'CS00DSQ001', 'INC', '2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');
INSERT INTO [IncidentMain]([IM_PK], [IM_IncidentNumber], [IM_Source], [IM_SystemCreateTimeUtc], [IM_SystemCreateUser], [IM_SystemLastEditTimeUtc], [IM_SystemLastEditUser]) VALUES ('{incidentPK2}', 'CS00DSQ002', 'INC', '2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');
INSERT INTO [IncidentMain]([IM_PK], [IM_IncidentNumber], [IM_Source], [IM_SystemCreateTimeUtc], [IM_SystemCreateUser], [IM_SystemLastEditTimeUtc], [IM_SystemLastEditUser]) VALUES ('{incidentPK3}', 'CS00DSQ003', 'INC', '2023-06-06 12:00:00', 'QD', '2023-11-06 12:00:00.000', 'QD');

INSERT INTO dbo.EdiIdentityTenant(IDT_PK, IDT_TenantId, IDT_OidcClientId, IDT_Name, IDT_AuthorityUrl, IDT_GraphClientId, IDT_Onboarding, IDT_SystemCreateUser, IDT_SystemCreateTimeUtc, IDT_SystemLastEditUser, IDT_SystemLastEditTimeUtc) VALUES ('{tenantPK}', '1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1', '{tenantOidcClientId}', 'Test', 'https://www.example.com', '{Guid.NewGuid()}', 0, 'E', SYSDATETIME(),'E',SYSDATETIME());

INSERT INTO dbo.EdiTokenAuthOnBoardingData (TOD_PK, TOD_SystemUniqueIdentifier, TOD_ConfigurationIdentifier, TOD_ClaimMappingName, TOD_ClaimMappingIdentifier, TOD_IM, TOD_LE) VALUES ('{ediTokenAuthOnBoardingDataPK1}', '11', 'Azure', '123', 'GlbStaff.GS_LoginName', '{incidentPK1}', '{licenceEnterprisePK1}');
INSERT INTO dbo.EdiTokenAuthOnBoardingData (TOD_PK, TOD_SystemUniqueIdentifier, TOD_ConfigurationIdentifier, TOD_ClaimMappingName, TOD_ClaimMappingIdentifier, TOD_IM, TOD_LE) VALUES ('{ediTokenAuthOnBoardingDataPK2}', '11', 'Azure', '123', 'GlbStaff.GS_LoginName', '{incidentPK2}', '{licenceEnterprisePK2}');
INSERT INTO dbo.EdiTokenAuthOnBoardingData (TOD_PK, TOD_SystemUniqueIdentifier, TOD_ConfigurationIdentifier, TOD_ClaimMappingName, TOD_ClaimMappingIdentifier, TOD_IM, TOD_LE) VALUES ('{ediTokenAuthOnBoardingDataPK3}', '11', 'Azure', '123', 'GlbStaff.GS_LoginName', '{incidentPK3}', '{licenceEnterprisePK3}');";
			TestConnection.ExecuteNonQuery(sql);
		}

		readonly Guid tenantPK = Guid.NewGuid();
		readonly Guid tenantOidcClientId = Guid.NewGuid();
	}
}
