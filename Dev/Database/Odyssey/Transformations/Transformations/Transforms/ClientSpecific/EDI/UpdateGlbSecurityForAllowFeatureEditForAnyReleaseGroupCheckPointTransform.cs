using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class UpdateGlbSecurityForAllowFeatureEditForAnyReleaseGroupCheckPointTransform : DataTransformation
	{
		public override string UserDescription => "Update AllowFeatureEditForAnyReleaseGroup checkpoint";

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "GlbSecurity", "dbo"))
			{
				var sql = @"
INSERT INTO dbo.GlbSecurity (
    GU_PK,
    GU_ItemGUID, 
    GU_GB, 
    GU_GE, 
    GU_GS, 
    GU_GG, 
    GU_GC, 
    GU_IsValid, 
    GU_SecurityItemIsAllowed, 
    GU_AutoVersion, 
    GU_SystemCreateTimeUtc, 
    GU_SystemCreateUser, 
    GU_SystemLastEditTimeUtc, 
    GU_SystemLastEditUser, 
    GU_SecurityRight
)
SELECT 
    NEWID(),
    src.GU_ItemGUID,
    src.GU_GB, 
    src.GU_GE, 
    src.GU_GS, 
    src.GU_GG, 
    src.GU_GC, 
    src.GU_IsValid, 
    0,
    src.GU_AutoVersion, 
    GETUTCDATE(), 
    '~BP',
    GETUTCDATE(), 
    '~BP', 
    'AllowFeatureEditForAnyReleaseGroup'
FROM dbo.GlbSecurity src
WHERE (src.GU_SecurityRight IN ('FeatureControl', 'FeatureControlEdit'))
  AND src.GU_SecurityItemIsAllowed = 1
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.GlbSecurity trg
      WHERE trg.GU_SecurityRight = 'AllowFeatureEditForAnyReleaseGroup'
        AND trg.GU_SecurityItemIsAllowed = 0
  );
";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
