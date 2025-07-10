using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateOidcClientIdForEdiIdentityTenantTableTransform))]
	internal class UpdateOidcClientIdForEdiIdentityTenantTableTransformTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateOidcClientIdForEdiIdentityTenantTableTransform();
		}

		protected override void PrepareTestData()
		{
			var sql = @"
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

INSERT INTO dbo.EdiIdentityTenant (IDT_PK, IDT_TenantId, IDT_Name, IDT_GraphClientId, IDT_Onboarding, IDT_SystemCreateUser, IDT_SystemCreateTimeUtc, IDT_SystemLastEditUser, IDT_SystemLastEditTimeUtc)
VALUES (newid(), '1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1', 'B2C 01', '9FA205B6-830D-423A-94C0-F0B285AA2EEA', 1, 'E', GETDATE(), 'E', GETDATE()),
(newid(), 'd44b3775-da13-49c3-a1d9-d5b64ec45cf0', 'MockB2C', '09690FF5-9259-436A-B2E0-83097716D767', 0, 'E', GETDATE(), 'E', GETDATE())
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("The IDT_OidcClientId is added and value is updated when IDT_TenantId matched.", 1, TestConnection.ExecuteScalar<int>("SELECT count(0) FROM dbo.EdiIdentityTenant where IDT_OidcClientId = '3492b154-a8ce-4ce3-a956-511276cbd9e6'"));
			AssertEquals("The IDT_OidcClientId is added and value is empty when IDT_TenantId is not mathced.", 1, TestConnection.ExecuteScalar<int>("SELECT count(0) FROM dbo.EdiIdentityTenant where IDT_OidcClientId = ''"));
		}
	}
}
