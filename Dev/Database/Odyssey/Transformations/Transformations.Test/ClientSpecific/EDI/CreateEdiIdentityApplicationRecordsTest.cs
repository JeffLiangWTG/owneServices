using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(CreateEdiIdentityApplicationRecords))]
	internal class CreateEdiIdentityApplicationRecordsTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var getAllApplicationCountSQL = "SELECT COUNT([IDA_PK]) FROM [dbo].[EdiIdentityApplication]";
			var count = TestConnection.ExecuteScalar<int>(getAllApplicationCountSQL);
			AssertEquals(7, count);

			var expectedApplicationNameCounts = new List<(string ClientId, string ApplicationName, int ExpectedCount)>
			{
				("06b75561-7190-4f94-ad2a-48c50c3d20d4", "WTG, Domestic, MheIntegration", 1),
				("65753160-02cb-454c-9aa6-07577576892e", "WTG, International Logistics Service, EmissionCalculator", 1),
				("1ae29a5d-8c05-4121-a540-90c9e75d5242", "WTG, MDM, Denied Party Screen Service Server", 1),
				("692cd76d-1d76-4584-8fb8-aff67f96eab5", "WTG, MDM, Dps Portal", 1),
				("6fd2a92d-da25-4988-8ba0-c6ccbc6e147b", "WTG, Product Extensions, BorderWise", 1),
				("4992faa8-eb69-4341-9cc7-ac8c3a97c37a", "WTG, Product Extensions, MyAccount", 1),
				("a94d40e7-a323-476c-8649-ea2baeebabd4", "WTG, ReferenceData, Portal", 1)
			};

			foreach (var entry in expectedApplicationNameCounts)
			{
				var clientId = entry.ClientId;
				var applicationName = entry.ApplicationName;
				var expectedCount = entry.ExpectedCount;

				var sqlQuery = $"SELECT COUNT([IDA_PK]) FROM [dbo].[EdiIdentityApplication] WHERE IDA_ClientID = '{clientId}' AND IDA_ApplicationName='{applicationName}'";

				count = TestConnection.ExecuteScalar<int>(sqlQuery);
				AssertEquals(expectedCount, count);
			}

			var getRedirectUrlStatusIsSCHCountSQL = @"SELECT COUNT([IDA_PK]) FROM [dbo].[EdiIdentityApplication]
WHERE IDA_RedirectUrlStatus = 'SCH'";
			count = TestConnection.ExecuteScalar<int>(getRedirectUrlStatusIsSCHCountSQL);
			AssertEquals(4, count);

			var getAllRedirectUrlCountSQL = "SELECT COUNT([IAR_PK]) FROM [dbo].[EdiIdentityRedirectUrl]";
			count = TestConnection.ExecuteScalar<int>(getAllRedirectUrlCountSQL);
			AssertEquals(7, count);

			var expectedCounts = new Dictionary<string, int>
			{
				{ "06b75561-7190-4f94-ad2a-48c50c3d20d4", 0 },
				{ "65753160-02cb-454c-9aa6-07577576892e", 0 },
				{ "1ae29a5d-8c05-4121-a540-90c9e75d5242", 0 },
				{ "692cd76d-1d76-4584-8fb8-aff67f96eab5", 3 },
				{ "6fd2a92d-da25-4988-8ba0-c6ccbc6e147b", 2 },
				{ "4992faa8-eb69-4341-9cc7-ac8c3a97c37a", 1 },
				{ "a94d40e7-a323-476c-8649-ea2baeebabd4", 1 }
			};

			foreach (var entry in expectedCounts)
			{
				var clientId = entry.Key;
				var expectedCount = entry.Value;

				var sqlQuery = $"SELECT COUNT([IAR_PK]) FROM [dbo].[EdiIdentityRedirectUrl] RU JOIN [dbo].[EdiIdentityApplication] IA ON RU.IAR_IDA = IA.IDA_PK WHERE IA.IDA_ClientID = '{clientId}'";

				count = TestConnection.ExecuteScalar<int>(sqlQuery);
				AssertEquals(expectedCount, count);
			}

			var expectedRedirectUrlCounts = new List<(string ClientId, string RedirectUrl, int ExpectedCount)>
			{
				("692cd76d-1d76-4584-8fb8-aff67f96eab5", "https://compliancewise-test.wisegrid.net/Staging/dpp-oidc-signin", 1),
				("692cd76d-1d76-4584-8fb8-aff67f96eab5", "https://localhost:5011/dpp-oidc-signin", 1),
				("692cd76d-1d76-4584-8fb8-aff67f96eab5", "https://dpp-web-demo.wisegrid.net/dpp-oidc-signin", 1),
				("6fd2a92d-da25-4988-8ba0-c6ccbc6e147b", "https://webwatcher.wtg.zone/ump/api/v1/auth/oidc/callback", 1),
				("6fd2a92d-da25-4988-8ba0-c6ccbc6e147b", "https://ump.borderwise.com/api/v1/auth/oidc/callback", 1),
				("4992faa8-eb69-4341-9cc7-ac8c3a97c37a", "https://myaccount-portal.cargowise.com/myaccount/Login/OIDCLoginComplete.aspx", 1),
				("a94d40e7-a323-476c-8649-ea2baeebabd4", "https://refdbrepoupdate.wisecloud.zone/Portal/authenCallback", 1)
			};

			foreach (var entry in expectedRedirectUrlCounts)
			{
				var clientId = entry.ClientId;
				var redirectUrl = entry.RedirectUrl;
				var expectedCount = entry.ExpectedCount;

				var sqlQuery = $"SELECT COUNT([IAR_PK]) FROM [dbo].[EdiIdentityRedirectUrl] RU JOIN [dbo].[EdiIdentityApplication] IA ON RU.IAR_IDA = IA.IDA_PK WHERE IA.IDA_ClientID = '{clientId}' AND RU.IAR_RedirectUrl='{redirectUrl}'";

				count = TestConnection.ExecuteScalar<int>(sqlQuery);
				AssertEquals(expectedCount, count);
			}
		}

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery(CreateTableSQL());

			Db.Connection.ExecuteNonQuery(@"DELETE FROM dbo.EdiIdentityRedirectUrl; DELETE FROM dbo.EdiIdentityApplication; DELETE FROM dbo.EdiIdentityTenant;");

			Db.Connection.ExecuteNonQuery(InsertIntoTableSQL());
		}

		string CreateTableSQL()
		{
			return @"
IF NOT EXISTS (
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

END

IF NOT EXISTS (
  SELECT * FROM sys.objects 
  WHERE object_id = OBJECT_ID(N'[dbo].[EdiIdentityRedirectUrl]') AND type in (N'U')
)
BEGIN
Create Table dbo.EdiIdentityRedirectUrl
(
IAR_PK uniqueidentifier NOT NULL,
IAR_IDA uniqueidentifier NOT NULL,
IAR_ApplicationName varchar(256) NOT NULL,
IAR_RedirectType varchar(3) NOT NULL DEFAULT 'SPA',
IAR_RedirectUrl varchar(200) NOT NULL,
IAR_SystemCreateUser varchar(3) NOT NULL DEFAULT '',
IAR_SystemCreateTimeUtc smalldatetime NULL,
IAR_SystemLastEditUser varchar(3) NOT NULL DEFAULT '',
IAR_SystemLastEditTimeUtc datetime NULL
);

CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__IAR_IDA_IAR_RedirectUrl] ON [EdiIdentityRedirectUrl] ([IAR_IDA] ASC, [IAR_RedirectUrl] ASC)
WITH ( IGNORE_DUP_KEY = OFF);

ALTER TABLE [EdiIdentityRedirectUrl]
ADD CONSTRAINT [PK_UX__IAR_PK] PRIMARY KEY CLUSTERED ([IAR_PK] ASC)
;

ALTER TABLE [EdiIdentityRedirectUrl] WITH NOCHECK
	  ADD CONSTRAINT [EdiIdentityRedirectUrl_IAR_IDA_FK2_EdiIdentityApplication_PK] FOREIGN KEY
		  ( [IAR_IDA] )
		  REFERENCES [EdiIdentityApplication]
		  ( [IDA_PK] );

ALTER TABLE [EdiIdentityRedirectUrl] ADD CONSTRAINT [Constraint_IAR_RedirectType] CHECK (IAR_RedirectType  in ('WEB', 'SPA', 'ICL'));

END
";
		}

		string InsertIntoTableSQL()
		{
			return @"
INSERT INTO dbo.EdiIdentityTenant 
(IDT_PK, IDT_TenantId, IDT_Name, IDT_GraphClientId)
VALUES
('FD20FF85-FF07-485F-BAC7-1F27BBE9AEBB', '1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1', 
 'TenantName1', 'GraphClientId1');
";
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new CreateEdiIdentityApplicationRecords();
		}
	}
}
