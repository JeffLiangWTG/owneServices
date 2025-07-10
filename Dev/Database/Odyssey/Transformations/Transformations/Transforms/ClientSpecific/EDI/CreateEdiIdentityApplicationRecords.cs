using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class CreateEdiIdentityApplicationRecords : DataTransformation
	{
		public override string UserDescription => "Add applications on Azure that are missing in EdiIdentityApplication";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiIdentityApplication") && DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiIdentityRedirectUrl"))
			{
				var sql = @"
INSERT INTO dbo.EdiIdentityApplication (IDA_PK, IDA_IDT, IDA_ClientID, IDA_ApplicationName, IDA_SystemCreateUser, IDA_SystemCreateTimeUtc, IDA_SystemLastEditUser, IDA_SystemLastEditTimeUtc)
SELECT NEWID(), tenant.IDT_PK, ClientID, ApplicationName, '~BP', GETUTCDATE(), '~BP', GETUTCDATE()
FROM (VALUES 
    ('06b75561-7190-4f94-ad2a-48c50c3d20d4', 'WTG, Domestic, MheIntegration'),
    ('65753160-02cb-454c-9aa6-07577576892e', 'WTG, International Logistics Service, EmissionCalculator'),
    ('1ae29a5d-8c05-4121-a540-90c9e75d5242', 'WTG, MDM, Denied Party Screen Service Server'),
    ('692cd76d-1d76-4584-8fb8-aff67f96eab5', 'WTG, MDM, Dps Portal'),
    ('6fd2a92d-da25-4988-8ba0-c6ccbc6e147b', 'WTG, Product Extensions, BorderWise'),
    ('4992faa8-eb69-4341-9cc7-ac8c3a97c37a', 'WTG, Product Extensions, MyAccount'),
    ('a94d40e7-a323-476c-8649-ea2baeebabd4', 'WTG, ReferenceData, Portal')
) AS source (ClientID, ApplicationName)
LEFT JOIN 
    (SELECT IDT_PK FROM EdiIdentityTenant WHERE IDT_TenantId = '1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1') AS tenant ON 1=1 
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.EdiIdentityApplication 
    WHERE IDA_ClientID = source.ClientID
);

WITH RedirectUrls AS (
    SELECT *
    FROM (VALUES
        ('692cd76d-1d76-4584-8fb8-aff67f96eab5', 'SPA', 'https://compliancewise-test.wisegrid.net/Staging/dpp-oidc-signin'),
        ('692cd76d-1d76-4584-8fb8-aff67f96eab5', 'SPA', 'https://localhost:5011/dpp-oidc-signin'),
        ('692cd76d-1d76-4584-8fb8-aff67f96eab5', 'SPA', 'https://dpp-web-demo.wisegrid.net/dpp-oidc-signin'),
        ('6fd2a92d-da25-4988-8ba0-c6ccbc6e147b', 'ICL', 'https://webwatcher.wtg.zone/ump/api/v1/auth/oidc/callback'),
        ('6fd2a92d-da25-4988-8ba0-c6ccbc6e147b', 'ICL', 'https://ump.borderwise.com/api/v1/auth/oidc/callback'),
        ('4992faa8-eb69-4341-9cc7-ac8c3a97c37a', 'ICL', 'https://myaccount-portal.cargowise.com/myaccount/Login/OIDCLoginComplete.aspx'),
        ('a94d40e7-a323-476c-8649-ea2baeebabd4', 'SPA', 'https://refdbrepoupdate.wisecloud.zone/Portal/authenCallback')
    ) AS RU(ClientID, RedirectType, RedirectUrl)
)

INSERT INTO dbo.EdiIdentityRedirectUrl (IAR_PK, IAR_IDA, IAR_ApplicationName, IAR_RedirectType, IAR_RedirectUrl, IAR_SystemCreateUser, IAR_SystemCreateTimeUtc, IAR_SystemLastEditUser, IAR_SystemLastEditTimeUtc)
SELECT NEWID(), IA.IDA_PK, IA.IDA_ApplicationName, RU.RedirectType, RU.RedirectUrl, '~BP', GETUTCDATE(), '~BP', GETUTCDATE()
FROM dbo.EdiIdentityApplication IA
JOIN RedirectUrls RU ON IA.IDA_ClientID = RU.ClientID
WHERE NOT EXISTS (
    SELECT 1 
    FROM dbo.EdiIdentityRedirectUrl 
    WHERE IAR_IDA = IA.IDA_PK
      AND IAR_RedirectUrl = RU.RedirectUrl
);

UPDATE dbo.EdiIdentityApplication
SET 
    IDA_RedirectUrlStatus = 'SCH',
    IDA_SystemLastEditUser = '~BP',
    IDA_SystemLastEditTimeUtc = GETUTCDATE()
WHERE 
    IDA_ClientID IN (
        '692cd76d-1d76-4584-8fb8-aff67f96eab5',
        '6fd2a92d-da25-4988-8ba0-c6ccbc6e147b',
        '4992faa8-eb69-4341-9cc7-ac8c3a97c37a',
        'a94d40e7-a323-476c-8649-ea2baeebabd4'
    );
";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
