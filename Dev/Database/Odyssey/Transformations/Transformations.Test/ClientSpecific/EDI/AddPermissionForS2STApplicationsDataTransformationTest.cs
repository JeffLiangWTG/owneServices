using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(AddPermissionForS2STApplicationsDataTransformation))]
	internal class AddPermissionForS2STApplicationsDataTransformationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(3, TestConnection.ExecuteScalar<int>("SELECT COUNT(DISTINCT IAP_IDA) FROM dbo.EdiIdentityApplicationPermission Left Join EdiIdentityApplication On IAP_IDA = IDA_PK Where IAP_Scope = '*' and IDA_IDT IS NOT NULL"));
			AssertEquals(0, TestConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.EdiIdentityApplicationPermission Left Join EdiIdentityApplication On IAP_IDA = IDA_PK Where IAP_Scope = '*' and IDA_PK = '{applicationPK4}'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new AddPermissionForS2STApplicationsDataTransformation();

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiIdentityApplicationPermission", @"
Create Table dbo.EdiIdentityApplicationPermission
(
IAP_PK uniqueidentifier NOT NULL,
IAP_IDA uniqueidentifier NOT NULL,
IAP_Scope varchar(64) NOT NULL DEFAULT '',
IAP_IsActive bit NOT NULL Default 1,
IAP_SystemCreateUser varchar(3) NOT NULL,
IAP_SystemCreateTimeUtc smalldatetime NULL,
IAP_SystemLastEditUser varchar(3) NOT NULL,
IAP_SystemLastEditTimeUtc datetime NULL
);");
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiIdentityApplication", @"
Create Table dbo.EdiIdentityApplication
(
IDA_PK uniqueidentifier NOT NULL,
IDA_LD uniqueidentifier NULL,
IDA_IDT uniqueidentifier NULL,
IDA_ClientID varchar(36) NOT NULL DEFAULT '',
IDA_IsRollback bit NOT NULL Default 0,
IDA_ApplicationName varchar(256) NOT NULL DEFAULT '',
IDA_RedirectUrlStatus varchar(3) NOT NULL DEFAULT 'NON',
IDA_RedirectUrlLastSyncTimeUtc datetime NULL,
IDA_IsActive bit NOT NULL Default 1,
IDA_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemCreateTimeUtc smalldatetime NULL,
IDA_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemLastEditTimeUtc datetime NULL
);");
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiIdentityTenant", @"
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
			var applicationPK1 = Guid.NewGuid();
			var applicationPK2 = Guid.NewGuid();
			var applicationPK3 = Guid.NewGuid();
			var tennatPK1 = Guid.NewGuid();

			var sql = $@"INSERT INTO dbo.EdiIdentityTenant(IDT_PK,IDT_TenantId,IDT_OidcClientId,IDT_Name,IDT_GraphClientId,IDT_SystemCreateUser,IDT_SystemCreateTimeUtc,IDT_SystemLastEditUser,IDT_SystemLastEditTimeUtc) VALUES ('{tennatPK1}', '{Guid.NewGuid()}','{Guid.NewGuid()}', 'Test','{Guid.NewGuid()}','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_IDT,IDA_ApplicationName,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK1}', '{tennatPK1}', 'Test1','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_IDT,IDA_ApplicationName,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK2}', '{tennatPK1}', 'Test2','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_IDT,IDA_ApplicationName,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK3}', '{tennatPK1}', 'Test3','E',SYSDATETIME(),'E',SYSDATETIME());
INSERT INTO dbo.EdiIdentityApplication(IDA_PK,IDA_ApplicationName,IDA_SystemCreateUser,IDA_SystemCreateTimeUtc,IDA_SystemLastEditUser,IDA_SystemLastEditTimeUtc) VALUES ('{applicationPK4}', 'Test4','E',SYSDATETIME(),'E',SYSDATETIME());";
			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid applicationPK4 = Guid.NewGuid();
	}
}
