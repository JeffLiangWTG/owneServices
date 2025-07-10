using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class PopulateWebLayoutPublishersGroup : DataTransformation
	{
		public override string UserDescription => "Populate WebLayoutPublishers group from legacy security right";

		protected override void OfflinePreUpgradeTransform()
		{
			if (
				// We do not want to add these contact groups for EdiProd as it's a special environment
				DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage") 
				|| !DbObjectCreator.TableExists(Db.Connection, GlbGroupSchema.Constants.TableName)
				|| Db.Connection.Exists($@"
FROM
dbo.GlbGroup 
WHERE GG_Code = 'WEBLAYOUTPUB'
"))
			{
				return;
			}

			Db.Connection.ExecuteNonQuery(TransformQuery);
		}

		const string TransformQuery = @"
--create a table to keep legacy security records
DECLARE @LegacySecurity TABLE 
(
	LS_OX_SecurityItemName VARCHAR(35) NOT NULL
	,LS_OX_SU UNIQUEIDENTIFIER NULL
	,LS_IsGrantedByDefault BIT NOT NULL
	,LS_GG UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()
	,LS_GG_Code VARCHAR(15) NOT NULL
	,LS_GG_Desc NVARCHAR(64) NOT NULL
	,LS_GG_IsSystemDefined BIT NOT NULL
	,LS_GGR_RoleName VARCHAR(50) NOT NULL
	,LS_Type VARCHAR(3) NOT NULL CHECK (LS_Type IN ('WEB', 'DOC', 'RPT'))
	,UNIQUE NONCLUSTERED (LS_OX_SecurityItemName, LS_OX_SU)
	,UNIQUE NONCLUSTERED (LS_GG_Code)
);

--STATIC, from the SPEC
INSERT INTO @LegacySecurity 
(
	LS_OX_SecurityItemName
	,LS_GG_Code
	,LS_GG_Desc
	,LS_GGR_RoleName
	,LS_GG_IsSystemDefined
	,LS_IsGrantedByDefault
	,LS_Type
)
VALUES
('Web Publish Layouts', 'WEBLAYOUTPUB', 'Web Layout Publishers', 'weblayoutpublisher', 1, 0, 'WEB')
;

DECLARE @CurrentUTC  SMALLDATETIME = GETUTCDATE();

-- create groups from legacy security
INSERT INTO dbo.GlbGroup
(
	GG_PK
	,GG_Code
	,GG_Desc
	,GG_Type
	,GG_IsSystemDefined
	,GG_IsSecurityEnabled
	,GG_IsValid
	,GG_IsActive
	,GG_SystemCreateTimeUtc
	,GG_SystemCreateUser
	,GG_SystemLastEditTimeUtc
	,GG_SystemLastEditUser
)
SELECT GG_PK = LS_GG
	,GG_Code = LS_GG_Code
	,GG_Desc = LS_GG_Desc
	,GG_Type = 'ORG'
	,GG_IsSystemDefined = LS_GG_IsSystemDefined
	,GG_IsSecurityEnabled = 0
	,GG_IsValid = 1
	,GG_IsActive = 1
	,GG_SystemCreateTimeUtc = @CurrentUTC
	,GG_SystemCreateUser = 'E'
	,GG_SystemLastEditTimeUtc = @CurrentUTC
	,GG_SystemLastEditUser = 'E'
FROM @LegacySecurity

-- create group roles from legacy security for static web only
INSERT INTO dbo.GlbGroupRole 
(
	GGR_PK
	,GGR_RoleName
	,GGR_GG_Group
	,GGR_SystemCreateTimeUtc
	,GGR_SystemCreateUser
	,GGR_SystemLastEditTimeUtc
	,GGR_SystemLastEditUser
)
SELECT GGR_PK = NEWID()
	,GGR_RoleName = RoleName
	,GGR_GG_Group = GG_PK
	,GGR_SystemCreateTimeUtc = @CurrentUTC
	,GGR_SystemCreateUser = 'E'
	,GGR_SystemLastEditTimeUtc = @CurrentUTC
	,GGR_SystemLastEditUser = 'E'
FROM 
(
	SELECT GG_PK = LS_GG, RoleName = LS_GGR_RoleName FROM @LegacySecurity WHERE LS_Type = 'WEB'
) T

DROP TABLE IF EXISTS #LegacySecurityLookup

SELECT LLK_GG = LS_GG
		,LLK_OH = OC_OH
		,LLK_OC = OC_PK
INTO #LegacySecurityLookup 
FROM 
(
	SELECT LS_GG
		,OC_OH
		,OC_PK
		,Granted = COALESCE(OZ_Granted, OX_Granted, LS_IsGrantedByDefault)
	FROM dbo.OrgContact
	JOIN @LegacySecurity ON 1 = 1
	LEFT JOIN dbo.OrgSecurity ON
		(
			(
				( OX_SecurityItemName <> '' AND OX_SecurityItemName = LS_OX_SecurityItemName ) -- static / doc
				OR ( OX_SU IS NOT NULL AND OX_SU = LS_OX_SU ) -- report
			)
			AND OX_OH = OC_OH
		)
	LEFT JOIN dbo.OrgSecurityContacts ON OZ_OC = OC_PK AND OZ_OX = OX_PK
	WHERE OC_IsActive = 1 AND OC_WebAccessEnabled = 1
) T
WHERE Granted = 1;

INSERT INTO dbo.GlbGroupOrgLink
(
	GOK_PK
	,GOK_AutoVersion
	,GOK_GG_Group
	,GOK_OH_Org
	,GOK_SystemCreateTimeUtc
	,GOK_SystemCreateUser
	,GOK_SystemLastEditTimeUtc
	,GOK_SystemLastEditUser
)
SELECT 
	GOK_PK = NEWID()
	,GOK_AutoVersion = 0
	,GOK_GG_Group = LLK_GG
	,GOK_OH_Org = LLK_OH
	,GOK_SystemCreateTimeUtc = @CurrentUTC
	,GOK_SystemCreateUser = 'E'
	,GOK_SystemLastEditTimeUtc = @CurrentUTC
	,GOK_SystemLastEditUser = 'E'
FROM
(
	SELECT DISTINCT LLK_GG, LLK_OH
	FROM #LegacySecurityLookup
) LK

INSERT INTO dbo.GlbGroupOrgContactLink
(
	GCK_PK
	,GCK_AutoVersion
	,GCK_GG_Group
	,GCK_OC_Contact
	,GCK_SystemCreateTimeUtc
	,GCK_SystemCreateUser
	,GCK_SystemLastEditTimeUtc
	,GCK_SystemLastEditUser
)
SELECT 
	GCK_PK = NEWID()
	,GCK_AutoVersion = 0
	,GCK_GG_Group = LLK_GG
	,GCK_OC_Contact = LLK_OC
	,GCK_SystemCreateTimeUtc = @CurrentUTC
	,GCK_SystemCreateUser = 'E'
	,GCK_SystemLastEditTimeUtc = @CurrentUTC
	,GCK_SystemLastEditUser = 'E'
FROM #LegacySecurityLookup

DROP TABLE #LegacySecurityLookup;
";
	}
}
