using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateTenantForExistingEdiIdentityApplication))]
	public class UpdateTenantForExistingEdiIdentityApplicationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var getAllCountSQL = @"SELECT COUNT([IDA_PK]) FROM [dbo].[EdiIdentityApplication]";
			var count = TestConnection.ExecuteScalar<int>(getAllCountSQL);
			AssertEquals(7, count);

			var getCountSQL = @"SELECT COUNT([IDA_PK]) FROM [dbo].[EdiIdentityApplication]
WHERE IDA_IDA_ParentApplication IS NULL AND IDA_OH_ParentOrg IS NULL";
			count = TestConnection.ExecuteScalar<int>(getCountSQL);
			AssertEquals(5, count);

			var getIDTCountSQL = @"SELECT COUNT([IDA_PK]) FROM [dbo].[EdiIdentityApplication]
WHERE IDA_IDT = 'FD20FF85-FF07-485F-BAC7-1F27BBE9AEBB'";
			count = TestConnection.ExecuteScalar<int>(getIDTCountSQL);
			AssertEquals(3, count);
		}

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery(CreateTableSQL());

			Db.Connection.ExecuteNonQuery(@"DELETE FROM dbo.EdiIdentityTenant; DELETE FROM dbo.EdiIdentityApplication");

			var applicationPK1 = Guid.NewGuid();
			var sql = InsertIntoTableSQL();
			Db.Connection.ExecuteNonQuery(sql, (x) =>
			{
				x.AddParameter("@Application1", SqlDbType.UniqueIdentifier, applicationPK1);
			});
		}

		string CreateTableSQL()
		{
			return @"IF NOT EXISTS (
  SELECT * FROM sys.objects 
  WHERE object_id = OBJECT_ID(N'[dbo].[EdiIdentityTenant]') AND type in (N'U')
)
BEGIN
CREATE TABLE dbo.EdiIdentityTenant
(
IDT_PK uniqueidentifier NOT NULL,
IDT_TenantId varchar(36) NOT NULL,
IDT_Name varchar(64) NOT NULL,
IDT_AuthorityUrl varchar(128) NOT NULL DEFAULT '',
IDT_GraphClientId varchar(36) NOT NULL,
IDT_Onboarding bit NOT NULL DEFAULT 0,
IDT_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDT_SystemCreateTimeUtc smalldatetime NULL,
IDT_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDT_SystemLastEditTimeUtc datetime NULL
);

ALTER TABLE [EdiIdentityTenant]
ADD CONSTRAINT [PK_UX__IDT_PK] PRIMARY KEY CLUSTERED ([IDT_PK] ASC);

ALTER TABLE [EdiIdentityTenant]
ADD CONSTRAINT [NR_UX__IDT_TenantId] UNIQUE NONCLUSTERED ([IDT_TenantId]);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDT_Onboarding] ON [EdiIdentityTenant] ([IDT_Onboarding]) WHERE [IDT_Onboarding] = 1;
END

IF NOT EXISTS (
  SELECT * FROM sys.objects 
  WHERE object_id = OBJECT_ID(N'[dbo].[EdiIdentityApplication]') AND type in (N'U')
)
BEGIN
Create Table dbo.EdiIdentityApplication
(
IDA_PK uniqueidentifier NOT NULL,
IDA_LD uniqueidentifier NULL,
IDA_IDT uniqueidentifier NULL,
IDA_OH_ParentOrg uniqueidentifier NULL,
IDA_IDA_ParentApplication uniqueidentifier NULL,
IDA_ClientID varchar(36) NOT NULL DEFAULT '',
IDA_IsRollback bit NOT NULL Default 0,
IDA_ApplicationName varchar(256) NOT NULL DEFAULT '',
IDA_ApplicationModule nvarchar(15) NOT NULL DEFAULT '',
IDA_RedirectUrlStatus varchar(3) NOT NULL DEFAULT 'NON',
IDA_RedirectUrlLastSyncTimeUtc datetime NULL,
IDA_Product varchar(3) NOT NULL Default '',
IDA_ApplicationType varchar(3) NOT NULL Default '',
IDA_IsActive bit NOT NULL Default 1,
IDA_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemCreateTimeUtc smalldatetime NULL,
IDA_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IDA_SystemLastEditTimeUtc datetime NULL
);

ALTER TABLE [EdiIdentityApplication]
ADD CONSTRAINT [PK_UX__IDA_PK] PRIMARY KEY CLUSTERED ([IDA_PK] ASC)
;

ALTER TABLE [EdiIdentityApplication] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityApplication_IDA_IDT_FK2_EdiIdentityTenant_PK] FOREIGN KEY
		  ( [IDA_IDT] )
		  REFERENCES [EdiIdentityTenant]
		  ( [IDT_PK] );

ALTER TABLE [EdiIdentityApplication] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityApplication_IDA_IDA_ParentApplication_FK2_IDA_PK] FOREIGN KEY
		  ( [IDA_IDA_ParentApplication] )
		  REFERENCES [EdiIdentityApplication]
		  ( [IDA_PK] );

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDA_LD] ON [EdiIdentityApplication] ([IDA_LD]) WHERE IDA_LD IS NOT NULL;

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDA_ClientID] ON [EdiIdentityApplication] ([IDA_ClientID]) WHERE IDA_ClientID != '';

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IDA_ApplicationName] ON [EdiIdentityApplication] ([IDA_ApplicationName]) WHERE IDA_ApplicationName != '';

CREATE NONCLUSTERED INDEX [NR_RX__IDA_RedirectUrlStatus] ON [EdiIdentityApplication] ([IDA_RedirectUrlStatus]);

CREATE NONCLUSTERED INDEX [NR_RX__IDA_IsRollback_IDA_IsActive] ON [EdiIdentityApplication] ([IDA_IsRollback] ASC, [IDA_IsActive] ASC);

ALTER TABLE [EdiIdentityApplication] ADD CONSTRAINT [Constraint_IDA_RedirectUrlStatus] CHECK (IDA_RedirectUrlStatus in ('NON', 'SCH', 'NUD', 'ERR'));

END";
		}

		string InsertIntoTableSQL()
		{
			return @"
INSERT INTO dbo.EdiIdentityTenant 
(IDT_PK, IDT_TenantId, IDT_Name, IDT_GraphClientId)
VALUES
('FD20FF85-FF07-485F-BAC7-1F27BBE9AEBB', '1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1', 
 'TenantName1', 'GraphClientId1'),
('4ACADFAF-22EB-4086-B4F3-B642C6070784', 'A0EC7386-60CB-4A4B-88A8-D7410819A8E8', 
 'TenantName2', 'GraphClientId2');

INSERT INTO [dbo].[EdiIdentityApplication] 
(IDA_PK, IDA_OH_ParentOrg)
VALUES 
    (@Application1, 'C4884C29-BF1F-4C4B-83D7-ECB89B1D439E');

INSERT INTO [dbo].[EdiIdentityApplication] 
(IDA_PK, IDA_IDA_ParentApplication)
VALUES 
    (NEWID(), @Application1); 

INSERT INTO [dbo].[EdiIdentityApplication]
(IDA_PK, IDA_IDT)
VALUES
(NEWID(), NULL),

(NEWID(), NULL),

(NEWID(), NULL),

(NEWID(), '4ACADFAF-22EB-4086-B4F3-B642C6070784'),

(NEWID(), '4ACADFAF-22EB-4086-B4F3-B642C6070784')";
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateTenantForExistingEdiIdentityApplication();
		}
	}
}
